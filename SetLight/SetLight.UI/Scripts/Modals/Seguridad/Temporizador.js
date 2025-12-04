

(function () {
    // ===== CONFIGURACIÓN =====

    var minutosInactividad = 60; 

    var idleTimeMs = minutosInactividad * 60 * 1000; // 5 * 60 * 1000 = 300000 ms

    var idleTimer;

    function resetTimer() {
        clearTimeout(idleTimer);
        idleTimer = setTimeout(cerrarSesionPorInactividad, idleTimeMs);
    }

    function cerrarSesionPorInactividad() {
        // Esta variable la definimos en el _Layout.cshtml
        if (window.autoLogOffUrl && typeof window.autoLogOffUrl === "string") {
            window.location.href = window.autoLogOffUrl;
        } else {
            // Por si acaso no está definida
            console.warn("autoLogOffUrl no está definida.");
        }
    }

    // Eventos que cuentan como "actividad" 
    var eventos = [
        "load",
        "mousemove",
        "mousedown",
        "click",
        "scroll",
        "keypress",
        "touchstart",
        "touchmove"
    ];

    eventos.forEach(function (e) {
        window.addEventListener(e, resetTimer, true);
    });

    // Iniciar el temporizador al cargar la página
    resetTimer();
})();
