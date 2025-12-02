// Scripts/Modals/EqCategory/EqCategoryModal.js

$(document).ready(function () {

    // Cuando se abre el modal, cargamos el formulario por GET
    $('#modalEqCategory').on('show.bs.modal', function () {
        $.get('/EqCategory/CrearEqCategory', function (data) {
            $('#modal-content-category').html(data);
        });
    });

    // Interceptar el submit de cualquier form dentro del modal
    $('#modalEqCategory').on('submit', 'form', function (e) {
        e.preventDefault();

        var $form = $(this);

        $.ajax({
            url: $form.attr('action'),          // /EqCategory/CrearEqCategory
            type: $form.attr('method') || 'POST',
            data: $form.serialize(),
            success: function (response) {
                // Si el controlador devuelve JSON { success: true }
                if (typeof response === 'object' && response.success) {
                    $('#modalEqCategory').modal('hide');
                    // Recargar página o, si querés, solo recargar la lista de categorías
                    location.reload();
                } else {
                    // Si devuelve HTML (partial con errores), lo pintamos de nuevo en el modal
                    $('#modal-content-category').html(response);
                }
            },
            error: function () {
                alert('Ocurrió un error al crear la categoría.');
            }
        });
    });
});
