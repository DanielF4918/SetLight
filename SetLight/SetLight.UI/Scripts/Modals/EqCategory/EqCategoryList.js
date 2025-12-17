// Scripts/Modals/EqCategory/EqCategoryList.js

document.addEventListener("DOMContentLoaded", function () {

    const btnAdminCat = document.getElementById("btnAdministrarCategorias");
    const modalCategoriasEl = document.getElementById("modalCategorias");
    const modalEditarCatEl = document.getElementById("modalEditarCategoria");
    const modalEqCategoryEl = document.getElementById("modalEqCategory"); // modal crear

    if (!btnAdminCat || !modalCategoriasEl || !modalEditarCatEl) return;

    const modalCategorias = new bootstrap.Modal(modalCategoriasEl);
    const modalEditarCategoria = new bootstrap.Modal(modalEditarCatEl);
    const modalEqCategory = modalEqCategoryEl ? new bootstrap.Modal(modalEqCategoryEl) : null;

    const contenedorLista = document.getElementById("contenedorListaCategorias");
    const contenedorEditar = document.getElementById("contenedorEditarCategoria");
    const btnNuevaCatDesdeLista = document.getElementById("btnAbrirCrearCategoriaDesdeLista");

    // =========================
    // Cargar lista (con paginación)
    // =========================
    function cargarListaCategorias(page = 1) {
        fetch("/EqCategory/ListarCategoriasPartial?page=" + page)
            .then(r => r.text())
            .then(html => {
                contenedorLista.innerHTML = html;
            })
            .catch(() => {
                contenedorLista.innerHTML = "<p class='text-danger'>Error al cargar categorías</p>";
            });
    }

    // =========================
    // Abrir modal de categorías
    // =========================
    btnAdminCat.addEventListener("click", function () {
        cargarListaCategorias(1);
        modalCategorias.show();
    });

    // =========================
    // Delegación: editar + paginación
    // =========================
    contenedorLista.addEventListener("click", function (e) {

        // 🟢 Editar categoría
        const btnEditar = e.target.closest(".btn-editar-cat");
        if (btnEditar) {
            const id = btnEditar.dataset.id;

            fetch("/EqCategory/EditarCategoriaPartial?id=" + encodeURIComponent(id))
                .then(r => r.text())
                .then(html => {
                    contenedorEditar.innerHTML = html;
                    modalEditarCategoria.show();
                });

            return; // ⛔ corta aquí
        }

        // 🔵 Paginación
        const link = e.target.closest(".pagination a");
        if (!link) return;

        e.preventDefault();

        const url = new URL(link.href);
        const page = url.searchParams.get("page") || 1;

        cargarListaCategorias(page);
    });

    // =========================
    // Submit edición (AJAX)
    // =========================
    modalEditarCatEl.addEventListener("submit", function (e) {
        const form = e.target;
        if (form.id !== "formEditarCategoria") return;

        e.preventDefault();

        const formData = new FormData(form);

        fetch(form.action, {
            method: "POST",
            body: formData
        })
            .then(r => r.json())
            .then(data => {
                if (data.success) {
                    modalEditarCategoria.hide();
                    cargarListaCategorias(1); // refresca lista
                } else {
                    alert(data.mensaje || "Ocurrió un error al actualizar la categoría.");
                }
            })
            .catch(() => {
                alert("Error al comunicarse con el servidor.");
            });
    });

    // =========================
    // Abrir modal crear categoría
    // =========================
    if (btnNuevaCatDesdeLista && modalEqCategory) {
        btnNuevaCatDesdeLista.addEventListener("click", function () {
            modalCategorias.hide();
            modalEqCategory.show();
        });
    }

});
