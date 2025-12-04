// Scripts/Modals/Maintenance/CreateMaintenance.js

document.addEventListener("DOMContentLoaded", function () {
    const btnAbrirModal = document.getElementById("btnAbrirModalEquipos");
    const modalEquiposEl = document.getElementById("modalEquipos");
    const filtroInput = document.getElementById("filtroEquiposMantenimiento");
    const tbodyEquipos = document.querySelector("#tablaEquiposMantenimiento tbody");

    const inputEquipmentId = document.getElementById("EquipmentId");
    const inputEquipmentName = document.getElementById("EquipoNombreVisible");
    const inputCantidadForm = document.getElementById("Cantidad"); // 👈 input del formulario principal

    // Abrir modal
    if (btnAbrirModal && modalEquiposEl) {
        btnAbrirModal.addEventListener("click", function () {
            const modal = new bootstrap.Modal(modalEquiposEl);
            modal.show();
        });
    }

    // Búsqueda / filtro en el modal
    if (filtroInput && tbodyEquipos) {
        filtroInput.addEventListener("input", function () {
            const texto = (filtroInput.value || "").toLowerCase();

            tbodyEquipos.querySelectorAll("tr").forEach(function (row) {
                const nombre = (row.querySelector(".col-nombre")?.textContent || "").toLowerCase();
                const marca = (row.querySelector(".col-marca")?.textContent || "").toLowerCase();
                const modelo = (row.querySelector(".col-modelo")?.textContent || "").toLowerCase();

                const coincide =
                    !texto ||
                    nombre.includes(texto) ||
                    marca.includes(texto) ||
                    modelo.includes(texto);

                row.style.display = coincide ? "" : "none";
            });
        });
    }

    // Seleccionar equipo + cantidad desde el modal
    if (tbodyEquipos && inputEquipmentId && inputEquipmentName && inputCantidadForm && modalEquiposEl) {
        tbodyEquipos.addEventListener("click", function (e) {
            const btn = e.target.closest(".seleccionar-equipo");
            if (!btn) return;

            const fila = btn.closest("tr");
            if (!fila) return;

            const id = btn.getAttribute("data-id");
            const nombre = btn.getAttribute("data-nombre");

            // Stock mostrado en la fila
            const stockTexto = (fila.querySelector(".col-stock")?.textContent || "0").trim();
            const stock = parseInt(stockTexto, 10) || 0;

            // Cantidad que el usuario escribió en el input de esa fila
            const inputCantFila = fila.querySelector(".input-cantidad");
            let cantidad = parseInt(inputCantFila?.value, 10) || 0;

            // Validaciones básicas
            if (!cantidad || cantidad < 1) {
                alert("La cantidad debe ser al menos 1.");
                return;
            }

            if (cantidad > stock) {
                alert("No puede enviar más equipos de los que hay en stock (" + stock + ").");
                return;
            }

            // Rellenar el formulario principal
            inputEquipmentId.value = id;
            inputEquipmentName.value = nombre;
            inputCantidadForm.value = cantidad; // 👈 aquí pasamos la cantidad al form

            // Cerrar el modal
            const modal = bootstrap.Modal.getInstance(modalEquiposEl);
            if (modal) {
                modal.hide();
            }
        });
    }
});
