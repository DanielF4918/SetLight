$(document).ready(function () {
    $('#modalEqCategory').on('show.bs.modal', function () {
        $.get('/EqCategory/CrearEqCategory', function (data) {
            $('#modal-content-category').html(data);
        });
    });
});

function onCategoryCreated(response) {
    if (response.success) {
        $('#modalEqCategory').modal('hide');
        location.reload();
    } else {
        $('#modal-content-category').html(response);
    }
}
