// Scripts/cliente-detalles.js

// Comprobación de carga del archivo
console.log("cliente-detalles.js cargado");

// Aseguramos que el DOM está listo
$(function () {
    // Delegamos el click en cualquier enlace con la clase link-detalle-cliente
    $(document).on('click', '.link-detalle-cliente', function (e) {
        e.preventDefault();

        var clientId = $(this).data('client-id');
        console.log("Click en link-detalle-cliente, clientId =", clientId);

        // Solo para la prueba: mostramos info básica en el modal, sin AJAX
        $('#clienteModal .modal-body').html(
            '<p><strong>ID del cliente:</strong> ' + clientId + '</p>' +
            '<p>Si ves este modal, el wiring está funcionando correctamente.</p>'
        );

        // Mostrar el modal (Bootstrap 5)
        var modalEl = document.getElementById('clienteModal');

        if (window.bootstrap && bootstrap.Modal) {
            var modal = bootstrap.Modal.getOrCreateInstance(modalEl);
            modal.show();
        } else if (window.jQuery) {
            // Compatibilidad con Bootstrap 4
            $('#clienteModal').modal('show');
        } else {
            alert("Bootstrap JS no está disponible.");
        }
    });
});
