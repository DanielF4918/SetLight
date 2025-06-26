$(document).ready(function () {

    $('#clienteModal').on('show.bs.modal', function () {
        $.get('/Client/BuscarClientesModal', function (data) {
            $('#contenedorClientes').html(data);
        });
    });

    $(document).on('click', '.seleccionar-cliente', function () {
        var id = $(this).data('id');
        var nombre = $(this).data('nombre');

        $('#clienteId').val(id);
        $('#clienteNombre').val(nombre);
        $('#clienteModal').modal('hide');
    });

    $(document).on('input', '#buscadorClientes', function () {
        var filtro = $(this).val().toLowerCase();

        $('#tablaClientes tbody tr').each(function () {
            var nombre = $(this).find('td:eq(0)').text().toLowerCase();
            var correo = $(this).find('td:eq(1)').text().toLowerCase();
            var telefono = $(this).find('td:eq(2)').text().toLowerCase();

            if (nombre.includes(filtro) || correo.includes(filtro) || telefono.includes(filtro)) {
                $(this).show();
            } else {
                $(this).hide();
            }
        });
    });

});
