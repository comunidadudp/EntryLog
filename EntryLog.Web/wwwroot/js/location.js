
/**
 * Obtiene la ubicación del usuario usando la API de geolocalización del navegador.
 * Retorna un objeto con latitud y longitud o lanza un error con el motivo.
 * 
 * @param {Object} options - Configuración de la geolocalización
 * @param {boolean} options.highAccuracy - Mayor precisión (consume más batería)
 * @param {number} options.timeout - Tiempo máximo en ms para obtener la ubicación
 * @param {number} options.maximumAge - Máxima antigüedad permitida en ms de la ubicación cacheada
 */
function getCurrentLocation({
    highAccuracy = true,
    timeout = 10000,
    maximumAge = 0
} = {}) {
    return new Promise((resolve, reject) => {
        if (!("geolocation" in navigator)) {
            return reject(new Error("La API de geolocalización no está soportada en este navegador"));
        }

        navigator.geolocation.getCurrentPosition(
            position => {
                resolve({
                    lat: position.coords.latitude,
                    lng: position.coords.longitude,
                    accuracy: position.coords.accuracy
                });
            },
            error => {
                let message;
                switch (error.code) {
                    case error.PERMISSION_DENIED:
                        message = "El usuario denegó el permiso de geolocalización.";
                        break;
                    case error.POSITION_UNAVAILABLE:
                        message = "La información de ubicación no está disponible.";
                        break;
                    case error.TIMEOUT:
                        message = "La solicitud de ubicación expiró.";
                        break;
                    default:
                        message = "Error desconocido al obtener la ubicación.";
                        break;
                }
                reject(new Error(message));
            },
            {
                enableHighAccuracy: highAccuracy,
                timeout,
                maximumAge
            }
        );
    });
}