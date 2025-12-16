$(document).ready(function () {


    $('#clienteModal').on('show.bs.modal', function () {
        $.get('/Client/BuscarClientesModal', function (data) {

            $('#contenedorClientes').html(data);


            paginarClientes(9);
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

        $('#clientesBody tr').each(function () {
            var nombre = $(this).find('td:eq(0)').text().toLowerCase();
            var correo = $(this).find('td:eq(2)').text().toLowerCase();
            var telefono = $(this).find('td:eq(3)').text().toLowerCase();

            if (nombre.includes(filtro) || correo.includes(filtro) || telefono.includes(filtro)) {
                $(this).show();
            } else {
                $(this).hide();
            }
        });


        if (filtro === "") {
            $('#clientesBody tr').show();
            paginarClientes(9); 
        } else {

            paginarClientes(9);
        }

    });

});


function paginarClientes(rowsPerPage) {

    const table = document.querySelector("#clientesBody");
    if (!table) return;

    const allRows = Array.from(table.querySelectorAll("tr"));
    const rows = allRows.filter(r => r.style.display !== "none"); 
    const totalRows = rows.length;
    const totalPages = Math.ceil(totalRows / rowsPerPage);

    if (totalPages === 0) return;

    const pagination = document.querySelector("#clientesPagination");

    function mostrarPagina(page) {
        const start = (page - 1) * rowsPerPage;
        const end = start + rowsPerPage;

        rows.forEach((row, i) => {
            row.style.display = (i >= start && i < end) ? "" : "none";
        });

        dibujarPaginacion(page);
    }

    function boton(text, disabled, onClick, active = false) {
        const li = document.createElement("li");
        li.className = "page-item " + (disabled ? "disabled" : "") + (active ? "active" : "");

        const a = document.createElement("a");
        a.className = "page-link";
        a.href = "#";
        a.textContent = text;

        if (!disabled) {
            a.addEventListener("click", function (e) {
                e.preventDefault();
                onClick();
            });
        }

        li.appendChild(a);
        return li;
    }

    function dibujarPaginacion(currentPage) {
        pagination.innerHTML = "";


        pagination.appendChild(
            boton("««", currentPage === 1, () => mostrarPagina(1))
        );


        pagination.appendChild(
            boton("«", currentPage === 1, () => mostrarPagina(currentPage - 1))
        );

        for (let i = 1; i <= totalPages; i++) {
            pagination.appendChild(
                boton(i, false, () => mostrarPagina(i), i === currentPage)
            );
        }

        pagination.appendChild(
            boton("»", currentPage === totalPages, () => mostrarPagina(currentPage + 1))
        );


        pagination.appendChild(
            boton("»»", currentPage === totalPages, () => mostrarPagina(totalPages))
        );
    }


    mostrarPagina(1);
}
