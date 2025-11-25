// Scripts/Modals/Maintenance/CreateMaintenance.js

document.addEventListener("DOMContentLoaded", function () {
    const btnAbrirModal = document.getElementById("btnAbrirModalEquipos");
    const modalEquiposEl = document.getElementById("modalEquipos");
    const filtroInput = document.getElementById("filtroEquiposMantenimiento");
    const tbodyEquipos = document.querySelector("#tablaEquiposMantenimiento tbody");

    const inputEquipmentId = document.getElementById("EquipmentId");
    const inputEquipmentName = document.getElementById("EquipoNombreVisible");

    // Abrir modal
    if (btnAbrirModal && modalEquiposEl) {
        btnAbrirModal.addEventListener("click", function () {
            const modal = new bootstrap.Modal(modalEquiposEl);
            modal.show();
        });
    }

    // Búsqueda / filtro
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

    // Seleccionar equipo
    if (tbodyEquipos && inputEquipmentId && inputEquipmentName && modalEquiposEl) {
        tbodyEquipos.addEventListener("click", function (e) {
            const btn = e.target.closest(".seleccionar-equipo");
            if (!btn) return;

            const id = btn.getAttribute("data-id");
            const nombre = btn.getAttribute("data-nombre");

            inputEquipmentId.value = id;
            inputEquipmentName.value = nombre;

            const modal = bootstrap.Modal.getInstance(modalEquiposEl);
            if (modal) modal.hide();
        });
    }
});
