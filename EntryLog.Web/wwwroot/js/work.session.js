(function () {
    'use strict'

    $("#sessions-link").addClass("active");
})();

const video = document.getElementById('video');
const loader = document.getElementById("loader-overlay");
const snapshotCanvas = document.getElementById('snapshot');


const recognitionModal = document.getElementById("recognition-modal");
const recognitionBootstrapModal = new bootstrap.Modal(recognitionModal);

async function openRecognitionModal() {
    activeVideoSectionLoader();
    recognitionBootstrapModal.show();
    await initRecognitionStream();
}

async function initRecognitionStream() {
    loadModels().then(() => {
        startVideo().then(() => {
            startLocation().then(() => {
                console.log("OK");
                inactiveVideoSectionLoader();
            }).catch(() => {
                //error al activar la localizacion
            });
        }).catch(() => {
            //error al cargar el video
        });
    }).catch((err) => {
        //error de obtener modelos
    });
}


/**
 * Cargar los modelos a usar de face-api
 */
async function loadModels() {
    Promise.all([
        faceapi.nets.tinyFaceDetector.loadFromUri('/lib/face-api-js/models'),
        faceapi.nets.faceLandmark68Net.loadFromUri('/lib/face-api-js/models'),
        faceapi.nets.faceExpressionNet.loadFromUri('/lib/face-api-js/models'),
        faceapi.nets.faceRecognitionNet.loadFromUri('/lib/face-api-js/models')
    ]);
}


/**
 * Inicia la transmisión de video
 */
async function startVideo() {
    try {
        const stream = await navigator.mediaDevices.getUserMedia({ video: true });
        video.srcObject = stream;
        video.classList.remove('visually-hidden');
    } catch (err) {
        console.error("No se pudo acceder a la cámara", err);
        $.notify({
            icon: 'icon-bell',
            title: 'Notificación',
            message: 'No se pudo acceder a la cámara. Verifica permisos'
        }, {
            type: 'danger'
        });
    }
}

/**
 * Detiene la transmisión de video
 */
function stopVideo() {
    const stream = video.srcObject;
    if (stream) {
        stream.getTracks().forEach(track => track.stop());
    }
    video.srcObject = null;
}

var readySnapshot = false;
let countdownActive = false;
let countdownTimer = null;

/**
 * Inicia la cuenta regresiva para tomar la foto
 */
function startCountdown() {
    countdownActive = true;
    let count = 3;
    const countdownEl = document.getElementById('countdown-overlay');
    countdownEl.style.display = 'block';
    countdownEl.textContent = count;

    countdownTimer = setInterval(() => {
        console.log("CONTADOR: ", count);
        console.log("HAY MATCH: ", hasMatch);
        count--;
        if (count > 0) {
            countdownEl.textContent = count;
        } else {
            clearInterval(countdownTimer);
            countdownEl.style.display = 'none';
            takeSnapshot(); //// lanzamos la peticion post
            readySnapshot = true;
            //openCaptureModal();
            openWorkSession();
        }
    }, 1000);
}

/**
 * Toma la foto y la dibuja en el elemento canvas. Adicionalmente detiene la transmisión de video
 */
function takeSnapshot() {
    snapshotCanvas.width = video.videoWidth;
    snapshotCanvas.height = video.videoHeight;
    let ctxSnapshot = snapshotCanvas.getContext('2d');
    ctxSnapshot.drawImage(video, 0, 0);
    //const imgData = snapshotCanvas.toDataURL("image/png");
    stopVideo();
    video.classList.add('visually-hidden');
    //circleOverlay.classList.add('visually-hidden');
    $("#faceid-helper").addClass('visually-hidden');
    //console.log("Foto tomada:", imgData);
}

async function openWorkSession() {

    $("#open-session-button-container").html(`
        <button class="btn btn-primary" type="button" disabled>
            <span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>
            Validando...
        </button>`);

    if (!canvasHaveContent(snapshotCanvas)) {
        $.notify({
            // options
            icon: 'icon-bell',
            title: 'Notificación',
            message: 'Debes capturar tu rostro'
        }, {
            // settings
            type: 'warning'
        });
        return;
    }

    const location = await getCurrentLocation({ highAccuracy: true, timeout: 10000, maximumAge: 0 });
    console.log("LOCATION POST", location);
    console.log(`Ubicación obtenida: Lat=${location.lat}, Lng=${location.lng}, Precisión=${location.accuracy}m`);

    // 👇 1. Detectar el rostro en el canvas y obtener el descriptor
    const faceDetection = await faceapi
        .detectSingleFace(snapshotCanvas, new faceapi.TinyFaceDetectorOptions())
        .withFaceLandmarks()
        .withFaceDescriptor();

    console.log("DETECTION POST", !faceDetection, faceDetection.descriptor);

    if (faceDetection == null || faceDetection == undefined) {
        $.notify({
            icon: 'icon-bell',
            title: 'Notificación',
            message: 'No se detectó ningún rostro en la captura'
        }, { type: 'warning' });
        return;
    }

    // 👇 2. Convertir Float32Array en array normal para que sea serializable
    const descriptorArray = Array.from(faceDetection.descriptor);

    const formData = new FormData();
    formData.append("latitude", location.lat);
    formData.append("longitude", location.lng);

    snapshotCanvas.toBlob((blob) => {

        formData.append("image", blob, "capture.png");
        formData.append("descriptor", JSON.stringify(descriptorArray));

        $.ajax({
            url: '/empleado/sesiones/abrir',
            method: 'POST',
            async: true,
            cache: false,
            contentType: false,
            processData: false,
            data: formData,
            beforeSend: () => {
                $("#open-session-button-container").html(`
                    <button class="btn btn-primary" type="button" disabled>
                        <span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>
                        Creando...
                    </button>`);
            },
            complete: () => {
                $("#no-sessions-alert").remove();
                recognitionBootstrapModal.hide();
                setTimeout(() => {
                    $("#open-session-button-container")
                        .html(`<button id="save-faceid-button" type="button" class="btn btn-primary">Aprobar</button>`);
                }, 1000);
            },
            success: (result) => {

                if (result.success) {
                    console.log(result.data)
                    drawSessionContent(result.data);
                }

                $.notify({
                    icon: 'icon-bell',
                    title: 'Notificación',
                    message: result.message
                }, {
                    type: result.success ? 'success' : 'warning'
                });
            },
            error: (err) => {
                $.notify({
                    icon: 'icon-bell',
                    title: 'Notificación',
                    message: 'Ha ocurrido un error inesperado'
                }, {
                    type: 'error'
                });
            }
        });

    }, 'image/png');
}

const STATUS_COMPLETED = "Completed";
const STATUS_IN_PROGRESS = "InProgress";
const STATUS_COMPLETED_NAME = "Completada";
const STATUS_IN_PROGRESS_NAME = "En Progreso";

/**
 * Dibuja la tarjeta que contiene la información de la sesión
 * @param {object} data Información de Face ID
 */
function drawSessionContent(data) {
    let sessionCard = document.createElement('div');
    sessionCard.classList.add('card');

    let statusIcon = "";
    let statusName = "";

    switch (data.status) {
        case STATUS_COMPLETED:
            statusIcon = "fa-check-circle text-success";
            statusName = STATUS_COMPLETED_NAME;
            break;

        case STATUS_IN_PROGRESS:
            statusIcon = "fa-spinner text-primary";
            statusName = STATUS_IN_PROGRESS_NAME;
            break;
        default:
            break;
    }

    let closeActionButton = data.status == STATUS_IN_PROGRESS
        ? `<button class="btn btn-label-success btn-round btn-sm">
               <span class="btn-label">
                <i class="fa fa-arrow-circle-right"></i>
               </span>
               Salir
           </button>`
        : ``;

    let checkInDate = formatDate(new Date(data.checkIn.date));
    let checkOutDate = data.checkOut != null ? formatDate(new Date(data.checkOut.date)) : 'No presenta';
    let totalWorked = data.totalWorked != null ? formatDate(data.totalWorked) : 'N/A';

    sessionCard.innerHTML = `
    <div class="card-header">
                <div class="d-flex align-items-left align-items-md-center flex-column flex-md-row">
                    <div>
                        <i class="fas fa-1x ${statusIcon}"></i>
                        ${statusName}
                    </div>
                    <div class="ms-md-auto py-2 py-md-0">
                        <button class="btn btn-label-info btn-round btn-sm">
                            <span class="btn-label">
                                <i class="fa fa-eye"></i>
                            </span>
                            Detalle
                        </button>
                        ${closeActionButton}
                    </div>
                </div>
            </div>
            <div class="card-body">
                <div class="row">
                    <div class="col-3">
                        <h6 class="fw-bolder">Entrada:</h6>
                        <p class="card-text">${checkInDate}</p>
                    </div>
                    <div class="col-3">
                        <h6 class="fw-bolder">Salida:</h6>
                        <p class="card-text">${checkOutDate}</p>
                    </div>
                    <div class="col-3">
                        <h6 class="fw-bolder">Duración:</h6>
                        <p class="card-text">${totalWorked}</p>
                    </div>
                </div>
            </div>`;

    const workSessionsDiv = document.getElementById("work-sessions");
    workSessionsDiv.appendChild(sessionCard, workSessionsDiv.lastChild);
}

function formatDate(date) {
    const day = String(date.getDate()).padStart(2, "0");
    const month = String(date.getMonth() + 1).padStart(2, "0"); // meses 0-11
    const year = date.getFullYear();

    let hours = date.getHours();
    const minutes = String(date.getMinutes()).padStart(2, "0");
    const ampm = hours >= 12 ? "PM" : "AM";

    hours = hours % 12;       // convierte a formato 12h
    hours = hours ? hours : 12; // 0 => 12
    const strHours = String(hours).padStart(2, "0");

    return `${day}/${month}/${year} ${strHours}:${minutes} ${ampm}`;
}

function formatTimeSpan(ms) {
    let totalSeconds = Math.floor(ms / 1000);
    const hours = String(Math.floor(totalSeconds / 3600)).padStart(2, "0");
    totalSeconds %= 3600;
    const minutes = String(Math.floor(totalSeconds / 60)).padStart(2, "0");
    const seconds = String(totalSeconds % 60).padStart(2, "0");

    return `${hours}:${minutes}:${seconds}`;
}

/**
 * Activa el loader de la transmisión de video
 */
function activeVideoSectionLoader() {
    loader.style.display = 'flex';
    loader.style.visibility = 'visible';
}

/**
 * Inactiva el loader de la transmisión de video
 */
function inactiveVideoSectionLoader() {
    loader.style.display = 'none';
    loader.style.visibility = 'hidden';
}


async function startLocation() {
    try {
        const location = await getCurrentLocation({ highAccuracy: true, timeout: 10000, maximumAge: 0 });
        console.log(`Ubicación obtenida: Lat=${location.lat}, Lng=${location.lng}, Precisión=${location.accuracy}m`);
    } catch (err) {
        console.error("Error al obtener la ubicación:", err.message);
    }
}

async function getReferenceImage() {
    //Obtener el token efímero
    const tokenResp = await fetch('/empleado/faceid/session', { credentials: 'include' });
    if (!tokenResp.ok) throw new Error("Error obteniendo token");
    const { token } = await tokenResp.json();

    // Usar token para imagen base64
    const imgResp = await fetch('/empleado/faceid/reference', {
        headers: {
            "Authorization": `Bearer ${token}`,  // Se envía el JWT
            "Accept": "application/json"
        }
    });

    if (!imgResp.ok) throw new Error("Error obteniendo imagen de referencia");
    const { imageBase64 } = await imgResp.json();

    // 3️⃣ Cargar imagen en objeto Image sin guardarla en disco
    return await faceapi.fetchImage(imageBase64);
}

// ✅ Obtener descriptor facial de una imagen
async function getDescriptorFromImage(img) {
    const detection = await faceapi
        .detectSingleFace(img, new faceapi.TinyFaceDetectorOptions())
        .withFaceLandmarks()
        .withFaceDescriptor();

    return detection?.descriptor;
}

/**
 * Valida si el elemento canvas contiene una imagen
 * @param {HTMLElement} canvas
 * @returns
 */
function canvasHaveContent(canvas) {
    const ctx = canvas.getContext('2d');
    const imageData = ctx.getImageData(0, 0, canvas.width, canvas.height);
    const data = imageData.data;
    for (let i = 0; i < data.length; i += 4) {
        if (data[i + 0] !== 0 || data[i + 1] !== 0 || data[i + 2] !== 0 || data[i + 3] !== 0) {
            return true; // Se encontró al menos un píxel no transparente
        }
    }
    return false; // Todos los píxeles son transparentes
}

let hasMatch = false;

(() => {
    video.addEventListener('play', async () => {
        const canvas = faceapi.createCanvasFromMedia(video);
        video.parentNode.append(canvas);
        const displaySize = { width: video.width, height: video.height };
        faceapi.matchDimensions(canvas, displaySize);

        canvas.style.position = 'absolute';
        canvas.style.top = '0';
        canvas.style.left = '0';

        console.log("display Size" + displaySize.width);

        // ✅ Imagen de referencia para comparación
        //const referenceImage = await faceapi.fetchImage('https://images.unsplash.com/photo-1500648767791-00dcc994a43e?fm=jpg&q=60&w=3000&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1yZWxhdGVkfDE1fHx8ZW58MHx8fHx8');
        const referenceImage = await getReferenceImage();
        console.log("referenceImage");
        console.log(referenceImage);
        const referenceDescriptor = await getDescriptorFromImage(referenceImage);

        setInterval(async () => {
            const detections = await faceapi
                .detectAllFaces(video, new faceapi.TinyFaceDetectorOptions())
                .withFaceLandmarks()
                .withFaceExpressions()
                .withFaceDescriptors();

            const resized = faceapi.resizeResults(detections, displaySize);
            canvas.getContext('2d').clearRect(0, 0, canvas.width, canvas.height + 20);

            for (const detection of resized) {
                const box = detection.detection.box;
                const drawBox = new faceapi.draw.DrawBox(box, { label: '' });
                drawBox.draw(canvas);

                // ✅ Dibujar landmarks
                //faceapi.draw.drawFaceLandmarks(canvas, detection);

                // ✅ Comparar rostro con referencia
                const similarity = faceapi.euclideanDistance(detection.descriptor, referenceDescriptor);
                console.log(`Similitud: ${similarity}`);
                hasMatch = similarity < 0.5;
                const match = hasMatch ? '✅ Coincide' : '❌ No coincide';
                new faceapi.draw.DrawTextField([`${match}`], { x: box.left, y: box.top - 20 }).draw(canvas);
            }

        }, 500);
    });
})();