// Scripts/Modals/Maintenance/CreateMaintenance.js

document.addEventListener("DOMContentLoaded", function () {
    const btnAbrirModal = document.getElementById("btnAbrirModalEquipos");
    const modalEquiposEl = document.getElementById("modalEquipos");
    const filtroInput = document.getElementById("filtroEquiposMantenimiento");
    const tbodyEquipos = document.querySelector("#tablaEquiposMantenimiento tbody");

    const inputEquipmentId = document.getElementById("EquipmentId");
    const inputEquipmentName = document.getElementById("EquipoNombreVisible");
    const inputCantidad = document.getElementById("Cantidad");

    if (!modalEquiposEl) return;

    // Instancia única del modal de Bootstrap 5
    const modalEquipos = new bootstrap.Modal(modalEquiposEl);

    // Abrir modal al dar clic en "Buscar equipo"
    if (btnAbrirModal) {
        btnAbrirModal.addEventListener("click", function () {
            modalEquipos.show();
        });
    }

    // Click en "Seleccionar" dentro de la tabla
    if (tbodyEquipos) {
        tbodyEquipos.addEventListener("click", function (e) {
            const boton = e.target.closest(".seleccionar-equipo");
            if (!boton) return;

            const fila = boton.closest("tr");
            const id = boton.getAttribute("data-id");
            const nombre = boton.getAttribute("data-nombre");
            const inputCantFila = fila.querySelector(".input-cantidad");

            let cantidad = 1;
            if (inputCantFila) {
                const val = parseInt(inputCantFila.value, 10);
                cantidad = isNaN(val) || val <= 0 ? 1 : val;
            }

            // Setear valores en el formulario principal
            if (inputEquipmentId) inputEquipmentId.value = id;
            if (inputEquipmentName) inputEquipmentName.value = nombre;
            if (inputCantidad) inputCantidad.value = cantidad;

            // Cerrar el modal correctamente
            modalEquipos.hide();
        });
    }

    // Filtro simple por texto en la tabla
    if (filtroInput && tbodyEquipos) {
        filtroInput.addEventListener("input", function () {
            const filtro = filtroInput.value.toLowerCase();

            tbodyEquipos.querySelectorAll("tr.fila-equipo").forEach(function (fila) {
                const nombre = fila.querySelector(".col-nombre")?.textContent.toLowerCase() || "";
                const marca = fila.querySelector(".col-marca")?.textContent.toLowerCase() || "";
                const modelo = fila.querySelector(".col-modelo")?.textContent.toLowerCase() || "";

                if (nombre.includes(filtro) || marca.includes(filtro) || modelo.includes(filtro)) {
                    fila.style.display = "";
                } else {
                    fila.style.display = "none";
                }
            });
        });
    }
});
