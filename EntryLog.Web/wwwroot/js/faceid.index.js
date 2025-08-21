const video = document.getElementById('video');
const snapshotCanvas = document.getElementById('snapshot');
const ctxSnapshot = snapshotCanvas.getContext('2d');
const configButton = document.getElementById("faceid-config-btn");
const circleOverlay = document.getElementById('circle-overlay');
const loader = document.getElementById("loader-overlay");


/**
 * Inicia la transmisión de video
 */
async function startVideo() {
    try {
        const stream = await navigator.mediaDevices.getUserMedia({ video: true });
        video.srcObject = stream;
        circleOverlay.style.display = 'block';
        //video.classList.remove('visually-hidden');
        $("#faceid-helper").removeClass('visually-hidden');
    } catch (err) {
        console.error("No se pudo acceder a la cámara", err);
        $.notify({
            icon: 'icon-bell',
            title: 'Notificación',
            message: 'No se pudo acceder a la cámara. Verifica permisos'
        }, {
            type: 'danger'
        });
        inactiveVideoSectionLoader();
        $("#faceid-config-btn").removeClass('pointer-events');
        $("#faceid-config-btn").removeClass('disabled');
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

/**
 * Carga los modelos de face-api.js
 */
async function loadModels() {
    await faceapi.nets.tinyFaceDetector.loadFromUri('/lib/face-api-js/models');
    await faceapi.nets.faceLandmark68Net.loadFromUri('/lib/face-api-js/models');
    await faceapi.nets.faceRecognitionNet.loadFromUri('/lib/face-api-js/models');
}

/**
 * Activa el loader de video
 */
function activeVideoSectionLoader() {
    loader.style.display = 'flex';
    loader.style.visibility = 'visible';
}

/**
 * Inactiva el loader de video
 */
function inactiveVideoSectionLoader() {
    loader.style.display = 'none';
    loader.style.visibility = 'hidden';
}


const detectorData = document.getElementById("detector-data");

/**
 * Verifica que el rostro esté completamente dentro del círculo
 * @param {any} box
 * @param {any} circleX
 * @param {any} circleY
 * @param {any} radius
 * @returns
 */
function isFaceFullyInsideCircle(box, circleX, circleY, radius) {
    console.log("isFaceFullyInsideCircle", box, circleX, circleY, radius);

    // Coordenadas de las esquinas de la caja
    const points = [
        { x: box.x, y: box.y }, // arriba izquierda
        { x: box.x + box.width, y: box.y }, // arriba derecha
        { x: box.x, y: box.y + box.height }, // abajo izquierda
        { x: box.x + box.width, y: box.y + box.height } // abajo derecha
    ];

    // Verificar que TODOS los puntos estén dentro del círculo
    return points.every(p => {
        const dx = p.x - circleX;
        const dy = p.y - circleY;
        let sqrt = Math.sqrt(dx * dx + dy * dy);
        let isValid = sqrt <= (radius * 2) - 20;
        //detectorData.innerHTML = `
        //    <p>dx: ${dx}</p> <p>dy: ${dy}</p> <p>sqrt: ${sqrt}</p> 
        //    <p>isValid: ${isValid}</p> <p>radius: ${radius}</p>
        //    <p>${sqrt}<=${(radius * 2) - 10}`;
        console.log("DECISION", sqrt);
        return isValid;
    });
}

/**
 * Toma la instantánea y la dibuja en el elemento canvas
 */
function takeSnapshot() {
    snapshotCanvas.width = video.videoWidth;
    snapshotCanvas.height = video.videoHeight;
    ctxSnapshot.drawImage(video, 0, 0);
    //const imgData = snapshotCanvas.toDataURL("image/png");
    stopVideo();
    video.classList.add('visually-hidden');
    circleOverlay.classList.add('visually-hidden');
    $("#faceid-helper").addClass('visually-hidden');
    //console.log("Foto tomada:", imgData);
}


/**
 * Reinicia la configuración y transmite video de nuevo
 */
async function snapshotAgain() {
    activeVideoSectionLoader();
    captureBootstrapModal.hide();

    await startVideo().then(() => {
        console.log("Video started again");
        readySnapshot = false;
        $("#circle-overlay").removeClass("visually-hidden");
        video.addEventListener('playing', detectFace);
    }).finally(() => {
        $("#loader-overlay").css('display', 'none');
    });
}



const captureModal = document.getElementById('camera-modal');
const captureBootstrapModal = new bootstrap.Modal(captureModal);

/**
* Muestra el modal que contiene el canvas
*/
function openCaptureModal() {
    captureBootstrapModal.show();
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
        count--;
        if (count > 0) {
            countdownEl.textContent = count;
        } else {
            clearInterval(countdownTimer);
            countdownEl.style.display = 'none';
            takeSnapshot();
            readySnapshot = true;
            openCaptureModal();
        }
    }, 1000);
}


/**
 * Detecta el rostro en la transmisión de la cámara
 */
async function detectFace() {
    const detection = await faceapi
        .detectSingleFace(video, new faceapi.TinyFaceDetectorOptions());

    const circleRect = circleOverlay.getBoundingClientRect();
    const videoRect = video.getBoundingClientRect();
    const circleRadius = circleRect.width / 2;
    const circleX = (circleRect.left - videoRect.left) + circleRadius;
    const circleY = (circleRect.top - videoRect.top) + circleRadius;

    if (detection) {
        if (isFaceFullyInsideCircle(detection.box, circleX, circleY, circleRadius)) {
            circleOverlay.style.borderColor = "limegreen";

            if (!readySnapshot && !countdownActive) {
                startCountdown(); // solo iniciamos la cuenta una vez
            }
        } else {
            circleOverlay.style.borderColor = "red";

            // Si el rostro se sale del círculo, cancelamos la cuenta
            if (countdownActive) {
                clearInterval(countdownTimer);
                document.getElementById('countdown-overlay').style.display = 'none';
                countdownActive = false;
            }
        }
    } else {
        circleOverlay.style.borderColor = "red";
        if (countdownActive) {
            clearInterval(countdownTimer);
            document.getElementById('countdown-overlay').style.display = 'none';
            countdownActive = false;
        }
    }

    if (!readySnapshot)
        requestAnimationFrame(detectFace);
}


/**
* Inicializa toda la funcionalidad de Face ID
*/
async function initFaceId() {
    $("#faceid-config-btn").addClass('pointer-events');
    $("#faceid-config-btn").addClass('disabled');
    activeVideoSectionLoader();

    $("#no-configured-alert").remove();

    await loadModels()
        .then(() => console.log("Models loaded"))
        .catch((err) => console.log("Error. ", err));
    await startVideo().then(() => {
        console.log("Video started");
        readySnapshot = false;
        $("#circle-overlay").removeClass("visually-hidden");
        video.addEventListener('playing', detectFace);
        //inactiveVideoSectionLoader();
    }).finally(() => {
        $("#loader-overlay").css('display', 'none');
    });
}



const saveFaceIdButton = document.getElementById("save-faceid-button");

//Eventos
(() => {
    'use strict'

    $("#faceid-link").addClass("active");

    saveFaceIdButton.addEventListener('click', async function (event) {

        $("#save-faceid-button-container").html(`
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

        snapshotCanvas.toBlob((blob) => {
            formData.append("image", blob, "capture.png");
            formData.append("descriptor", JSON.stringify(descriptorArray));

            $.ajax({
                url: '/empleado/faceid',
                method: 'POST',
                async: true,
                cache: false,
                contentType: false,
                processData: false,
                data: formData,
                beforeSend: () => {
                    $("#save-faceid-button-container").html(`
                    <button class="btn btn-primary" type="button" disabled>
                        <span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>
                        Creando...
                    </button>`);
                },
                complete: () => {
                    $("#faceid-config-btn").remove();
                    captureBootstrapModal.hide();
                    setTimeout(() => {
                        $("#save-faceid-button-container")
                            .html(`<button id="save-faceid-button" type="button" class="btn btn-primary">Aprobar</button>`);
                    }, 1000);
                },
                success: (result) => {

                    if (result.success) {
                        drawFaceIDContent(result.data);
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
    });

    video.addEventListener("loadeddata", (event) => {
        inactiveVideoSectionLoader();
    });

})();



/**
 * Dibuja la tarjeta que contiene la información del Face ID
 * @param {object} data Información de Face ID
 */
function drawFaceIDContent(data) {
    let faceidDiv = document.createElement('div');
    faceidDiv.classList.add('col-md-4');

    faceidDiv.innerHTML = `
    <div class="card card-post card-round">
        <div class="card-body">
            <div class="d-flex">
                <div class="avatar avatar-${data.active ? 'online' : 'offline'}">
                    <img src="${data.base64Image}" alt="..." class="avatar-img rounded-circle">
                </div>
                <div class="info-post ms-2">
                    <p class="username">Fecha registro</p>
                    <p class="date text-muted">${data.registerDate}</p>
                </div>
            </div>
            <div class="separator-solid"></div>
            <button class="btn btn-primary btn-rounded btn-sm" data-bs-toggle="collapse" data-bs-target="#faceid-image"
                    aria-expanded="true" aria-controls="faceid-image">
                Ver foto
            </button>
        </div>
        <div id="faceid-image" class="collapse">
            <img class="card-img-bottom" src="${data.base64Image}" alt="Card image cap">
        </div>
    </div>`;

    const faceidSectionDiv = document.getElementById("faceid-info-section");
    faceidSectionDiv.appendChild(faceidDiv);
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



