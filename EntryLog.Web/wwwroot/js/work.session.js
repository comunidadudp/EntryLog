(function () {
    'use strict'

    $("#sessions-link").addClass("active");
})();

const OPEN_ACTION = 'open';
const CLOSE_ACTION = 'close';

const video = document.getElementById('video');
const loader = document.getElementById("loader-overlay");
const snapshotCanvas = document.getElementById('snapshot');

const recognitionModal = document.getElementById("recognition-modal");
const recognitionBootstrapModal = new bootstrap.Modal(recognitionModal);

var workSessionAction = OPEN_ACTION;

async function openRecognitionModal(action) {
    changeModalDesign(action);
    activeVideoSectionLoader();
    recognitionBootstrapModal.show();
    await initRecognitionStream();
}

function changeModalDesign(action) {

    recognitionModal.setAttribute('action', action);
    workSessionAction = action;

    if (action === OPEN_ACTION) {
        $("#camera-modal-title").text('Abrir sesión');
    } else {
        $("#camera-modal-title").text('Cerrar sesión');
    }
}

async function initRecognitionStream() {
    loadModels().then(() => {
        startVideo().then(() => {
            startLocation().then(() => {
                console.log("OK");
            }).catch(() => {
                //error al activar la localizacion
                $.notify({
                    icon: 'icon-bell',
                    title: 'Notificación',
                    message: 'Error al activar la localizacion. Verifica permisos'
                }, {
                    type: 'danger'
                });
            });
        }).catch(() => {
            //error al cargar el video
            $.notify({
                icon: 'icon-bell',
                title: 'Notificación',
                message: 'Error al activar la localizacion. Verifica permisos'
            }, {
                type: 'danger'
            });
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
    stopVideo();
    video.classList.add('visually-hidden');
    $("#faceid-helper").addClass('visually-hidden');
}

async function openWorkSession() {

    $("#open-session-button-container").html(`
        <button onclick="startCountdown()" id="save-session-button" class="btn btn-primary" type="button" disabled>
            <span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>
            Validando...
        </button>`);

    if (!canvasHaveContent(snapshotCanvas)) {
        cleanupVideoSession();
        recognitionBootstrapModal.hide();
        $.notify({
            icon: 'icon-bell',
            title: 'Notificación',
            message: 'Debes capturar tu rostro'
        }, {
            type: 'warning'
        });
        return;
    }

    if (!hasMatch) {
        cleanupVideoSession();
        let ctxSnapshot = snapshotCanvas.getContext('2d');
        ctxSnapshot.clearRect(0, 0, snapshotCanvas.width, snapshotCanvas.height);
        snapshotCanvas.style.display = 'none'
        recognitionBootstrapModal.hide();

        $("#save-session-button").attr("disabled", true);
        $("#save-session-button").text("Preparando...");

        $.notify({
            icon: 'icon-bell',
            title: 'Notificación',
            message: 'Su rostro no coincide con el FaceID'
        }, {
            type: 'warning'
        });
        return;
    }

    const location = await getCurrentLocation({ highAccuracy: true, timeout: 10000, maximumAge: 0 });
    console.log("LOCATION POST", location);
    console.log(`Ubicación obtenida: Lat=${location.lat}, Lng=${location.lng}, Precisión=${location.accuracy}m`);

    const faceDetection = await faceapi
        .detectSingleFace(snapshotCanvas, new faceapi.TinyFaceDetectorOptions())
        .withFaceLandmarks()
        .withFaceDescriptor();

    console.log("DETECTION POST", !faceDetection, faceDetection?.descriptor);

    if (faceDetection == null || faceDetection == undefined) {
        cleanupVideoSession();
        recognitionBootstrapModal.hide();
        $.notify({
            icon: 'icon-bell',
            title: 'Notificación',
            message: 'No se detectó ningún rostro en la captura'
        }, { type: 'warning' });
        return;
    }

    const descriptorArray = Array.from(faceDetection.descriptor);

    const formData = new FormData();
    formData.append("latitude", location.lat);
    formData.append("longitude", location.lng);

    snapshotCanvas.toBlob((blob) => {

        formData.append("image", blob, "capture.png");
        formData.append("descriptor", JSON.stringify(descriptorArray));

        let path = workSessionAction === OPEN_ACTION
            ? 'abrir'
            : 'cerrar';

        $.ajax({
            url: `/empleado/sesiones/${path}`,
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
                    $("#open-session-button-container").html(`
                    <button onclick="startCountdown()" id="save-session-button" type="button" class="btn btn-primary" disabled>
                        Preparando...
                    </button>`);
                }, 1000);
            },
            success: (result) => {
                if (result.success) {
                    $("#add-work-session-btn").remove();
                    cleanupVideoSession();
                    clearSnapshotCanvas();

                    if (result.data.status === STATUS_IN_PROGRESS) {
                        drawSessionContent(result.data);
                    } else {
                        updateSessionContent(result.data);
                    }

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
                console.log("Error: ", err);
                $.notify({
                    icon: 'icon-bell',
                    title: 'Notificación',
                    message: 'Ha ocurrido un error inesperado'
                }, {
                    type: 'error'
                });
                clearSnapshotCanvas();
            }
        });

    }, 'image/png');
}

const STATUS_COMPLETED = "Completed";
const STATUS_IN_PROGRESS = "InProgress";
const STATUS_COMPLETED_NAME = "Completada";
const STATUS_IN_PROGRESS_NAME = "En Progreso";


function updateSessionContent(data) {
    let sessionId = data.id;

    let statusIcon = "";
    let statusName = "";

    switch (data.status) {
        case STATUS_COMPLETED:
            statusIcon = "fa-check-circle text-success";
            statusName = STATUS_COMPLETED_NAME;
            drawOpenSessionButton();
            break;

        case STATUS_IN_PROGRESS:
            statusIcon = "fa-spinner text-primary";
            statusName = STATUS_IN_PROGRESS_NAME;
            break;
        default:
            break;
    }

    $(`#status-fields-${sessionId}`).html(`<i class="fas fa-1x ${statusIcon}"></i>${statusName}`);
    $(`#close-session-${sessionId}`).remove();
    $(`#ckeckout-date-${sessionId}`).text(data.checkOut.date);
    $(`#total-worked-${sessionId}`).text(data.totalWorked);
}

function drawOpenSessionButton() {
    $("#open-work-session-content").html(`
        <div id="add-work-session-btn">
            <button onclick="openRecognitionModal('open')" class="btn btn-primary btn-round">Agregar sesión</button>
        </div>`);
}


/**
 * Dibuja la tarjeta que contiene la información de la sesión
 * @param {object} data Información de Face ID
 */
function drawSessionContent(data) {
    console.log("data", data);
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
        ? `<button   id="close-session-${data.id}" onclick="openRecognitionModal('close')" class="btn btn-label-success btn-round btn-sm">
               <span class="btn-label">
                <i class="fa fa-arrow-circle-right"></i>
               </span>
               Salir
           </button>`
        : ``;

    let checkOutDate = data.checkOut != null ? data.checkOut.date : 'No presenta';
    let totalWorked = data.totalWorked != null ? data.totalWorked : 'N/A';

    sessionCard.innerHTML = `
    <div class="card-header">
                <div class="d-flex align-items-left align-items-md-center flex-column flex-md-row">
                    <div id="status-fields-${data.id}">
                        <i class="fas fa-1x ${statusIcon}"></i>
                        ${statusName}
                    </div>
                    <div class="ms-md-auto py-2 py-md-0">
                        <button onclick="showSessionDetail('@item.Id')" class="btn btn-label-info btn-round btn-sm">
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
                        <p class="card-text">${data.checkIn.date}</p>
                    </div>
                    <div class="col-3">
                        <h6 class="fw-bolder">Salida:</h6>
                        <p id="ckeckout-date-${data.id}" class="card-text">${checkOutDate}</p>
                    </div>
                    <div class="col-3">
                        <h6 class="fw-bolder">Duración:</h6>
                        <p id="total-worked-${data.id}" class="card-text">${totalWorked}</p>
                    </div>
                </div>
            </div>`;

    const workSessionsDiv = document.getElementById("work-sessions");
    workSessionsDiv.insertBefore(sessionCard, workSessionsDiv.firstChild);
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

/**
 * Inicializa la geolocalización mediante el navegador
 */
async function startLocation() {
    try {
        const location = await getCurrentLocation({ highAccuracy: true, timeout: 10000, maximumAge: 0 });
        console.log(`Ubicación obtenida: Lat=${location.lat}, Lng=${location.lng}, Precisión=${location.accuracy}m`);
    } catch (err) {
        console.error("Error al obtener la ubicación:", err.message);
        $.notify({
            icon: 'icon-bell',
            title: 'Notificación',
            message: 'Error al activar la localizacion. Verifica permisos'
        }, {
            type: 'danger'
        });
    }
}

/**
 * Obtiene la imagen de referencia de FaceId
 * @returns
 */
async function getReferenceImage() {

    const tokenResp = await fetch('/empleado/faceid/session', { credentials: 'include' });
    if (!tokenResp.ok) throw new Error("Error obteniendo token");
    const { token } = await tokenResp.json();

    const imgResp = await fetch('/empleado/faceid/reference', {
        headers: {
            "Authorization": `Bearer ${token}`,  // Se envía el JWT
            "Accept": "application/json"
        }
    });

    if (!imgResp.ok) throw new Error("Error obteniendo imagen de referencia");
    const { imageBase64 } = await imgResp.json();

    return await faceapi.fetchImage(imageBase64);
}

/**
 * Obtiene el descriptor facial de una imagen
 * @param {any} img
 * @returns
 */
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

let overlayCanvas = null;
let intervalId = null;

/**
 * Limpia el elemento video y detiene la transmisión
 */
function cleanupVideoSession() {
    if (overlayCanvas && overlayCanvas.parentNode) {
        overlayCanvas.parentNode.removeChild(overlayCanvas);
        overlayCanvas = null;
    }
    if (intervalId) {
        clearInterval(intervalId);
        intervalId = null;
    }
    stopVideo();
    video.classList.add('visually-hidden');
    $("#faceid-helper").removeClass('visually-hidden');
}

/**
 * Limpiar el elemento canvas del snapshot
 */
function clearSnapshotCanvas() {
    snapshotCanvas.getContext('2d').clearRect(0, 0, snapshotCanvas.width, snapshotCanvas.height);
    snapshotCanvas.style.display = 'none';
}


function showSessionDetail(id) {

    console.log("ID", id);

    $.ajax({
        url: `/empleado/sesion/detail?id=${id}`,
        type: 'GET',
        async: false,
        beforeSend: () => {

        },
        complete: () => {

        },
        success: (result) => {
            console.log(result.data);

            let data = result.data;
            let statusName;

            switch (data.status) {
                case STATUS_COMPLETED:
                    statusName = STATUS_COMPLETED_NAME;
                    break;

                case STATUS_IN_PROGRESS:
                    statusName = STATUS_IN_PROGRESS_NAME;
                    break;
                default:
                    break;
            }


            // Propiedades principales
            document.getElementById('session-id').textContent = data.id;
            document.getElementById('employee-id').textContent = data.employeeId;
            document.getElementById('session-status').textContent = statusName;
            document.getElementById('total-worked').textContent = data.totalWorked || 'N/A';

            // CheckIn
            document.getElementById('checkin-method').textContent = data.checkIn.method;
            document.getElementById('checkin-device').textContent = data.checkIn.deviceName || 'N/A';
            document.getElementById('checkin-date').textContent = data.checkIn.date;
            document.getElementById('checkin-notes').textContent = data.checkIn.notes || 'N/A';
            document.getElementById('checkin-photo').src = data.checkIn.photoUrl;
            document.getElementById('checkin-lat').textContent = data.checkIn.location.latitude;
            document.getElementById('checkin-lng').textContent = data.checkIn.location.longtitude;
            document.getElementById('checkin-ip').textContent = data.checkIn.location.ipAddress;

            // CheckOut (puede ser null)
            if (data.checkOut) {
                document.getElementById('checkout-method').textContent = data.checkOut.method;
                document.getElementById('checkout-device').textContent = data.checkOut.deviceName || 'N/A';
                document.getElementById('checkout-date').textContent = data.checkOut.date;
                document.getElementById('checkout-notes').textContent = data.checkOut.notes || 'N/A';
                document.getElementById('checkout-photo').src = data.checkOut.photoUrl;
                document.getElementById('checkout-lat').textContent = data.checkOut.location.latitude;
                document.getElementById('checkout-lng').textContent = data.checkOut.location.longtitude;
                document.getElementById('checkout-ip').textContent = data.checkOut.location.ipAddress;
            } else {
                document.getElementById('checkout-method').textContent = 'No presenta';
                document.getElementById('checkout-device').textContent = 'No presenta';
                document.getElementById('checkout-date').textContent = 'No presenta';
                document.getElementById('checkout-notes').textContent = 'No presenta';
                document.getElementById('checkout-photo').src = '';
                document.getElementById('checkout-lat').textContent = 'No presenta';
                document.getElementById('checkout-lng').textContent = 'No presenta';
                document.getElementById('checkout-ip').textContent = 'No presenta';
            }

            // Mostrar modal
            const detailModal = new bootstrap.Modal(document.getElementById('detail-modal'));
            detailModal.show();
        },
        error: (err) => {
            $.notify({
                icon: 'icon-bell',
                title: 'Error',
                message: 'Ha ocurrido un error inesperado'
            }, {
                type: 'danger'
            });
        }
    })
}

let hasMatch = false;

(() => {
    let overlayCanvas = null;
    let intervalId = null;

    video.addEventListener('play', async () => {

        // Elimina cualquier canvas anterior para evitar duplicados
        if (overlayCanvas && overlayCanvas.parentNode) {
            overlayCanvas.parentNode.removeChild(overlayCanvas);
            overlayCanvas = null;
        }

        overlayCanvas = faceapi.createCanvasFromMedia(video);
        video.parentNode.append(overlayCanvas);
        const displaySize = { width: video.width, height: video.height };
        faceapi.matchDimensions(overlayCanvas, displaySize);

        overlayCanvas.style.position = 'absolute';
        overlayCanvas.style.top = '0';
        overlayCanvas.style.left = '0';

        console.log("Obteniendo imagen de referencia...");

        const referenceImage = await getReferenceImage();
        console.log("Imagen de referencia obtenida");
        console.log("Obteniendo descriptor de imagen...");

        const referenceDescriptor = await getDescriptorFromImage(referenceImage);
        console.log("Descriptor obtenido");

        inactiveVideoSectionLoader();
        $("#save-session-button").removeAttr("disabled");
        $("#save-session-button").text("Abrir");

        // Limpia cualquier intervalo anterior
        if (intervalId) {
            clearInterval(intervalId);
        }

        intervalId = setInterval(async () => {
            const detections = await faceapi
                .detectAllFaces(video, new faceapi.TinyFaceDetectorOptions())
                .withFaceLandmarks()
                .withFaceExpressions()
                .withFaceDescriptors();

            const resized = faceapi.resizeResults(detections, displaySize);
            overlayCanvas.getContext('2d').clearRect(0, 0, overlayCanvas.width, overlayCanvas.height + 20);

            for (const detection of resized) {
                const box = detection.detection.box;
                const drawBox = new faceapi.draw.DrawBox(box, { label: '' });
                drawBox.draw(overlayCanvas);

                // Dibujar landmarks
                //faceapi.draw.drawFaceLandmarks(overlayCanvas, detection);

                // Comparar rostro con referencia
                const similarity = faceapi.euclideanDistance(detection.descriptor, referenceDescriptor);
                console.log(`Similitud: ${similarity}`);
                hasMatch = similarity < 0.5;
                const match = hasMatch ? '✅ Coincide' : '❌ No coincide';
                new faceapi.draw.DrawTextField([`${match}`], { x: box.left, y: box.top - 20 }).draw(overlayCanvas);
            }

        }, 500);
    });

    // Elimina el canvas cuando el video se detiene
    function removeOverlayCanvas() {
        if (overlayCanvas && overlayCanvas.parentNode) {
            overlayCanvas.parentNode.removeChild(overlayCanvas);
            overlayCanvas = null;
        }
        if (intervalId) {
            clearInterval(intervalId);
            intervalId = null;
        }
    }

    video.addEventListener('pause', removeOverlayCanvas);
    video.addEventListener('ended', removeOverlayCanvas);

})();

// Limpieza al cerrar el modal para evitar desalineación del video
recognitionModal.addEventListener('hidden.bs.modal', function () {
    cleanupVideoSession();
});