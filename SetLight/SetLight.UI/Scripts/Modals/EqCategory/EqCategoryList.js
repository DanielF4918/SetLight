// Scripts/Modals/EqCategory/EqCategoryList.js

document.addEventListener("DOMContentLoaded", function () {

    const btnAdminCat = document.getElementById("btnAdministrarCategorias");
    const modalCategoriasEl = document.getElementById("modalCategorias");
    const modalEditarCatEl = document.getElementById("modalEditarCategoria");
    const modalEqCategoryEl = document.getElementById("modalEqCategory"); // 👈 tu modal de crear

    if (!btnAdminCat || !modalCategoriasEl || !modalEditarCatEl) return;

    const modalCategorias = new bootstrap.Modal(modalCategoriasEl);
    const modalEditarCategoria = new bootstrap.Modal(modalEditarCatEl);
    const modalEqCategory = modalEqCategoryEl ? new bootstrap.Modal(modalEqCategoryEl) : null;

    const contenedorLista = document.getElementById("contenedorListaCategorias");
    const contenedorEditar = document.getElementById("contenedorEditarCategoria");

    const btnNuevaCatDesdeLista = document.getElementById("btnAbrirCrearCategoriaDesdeLista");

    // Cargar lista de categorías dentro del modal
    function cargarListaCategorias() {
        fetch("/EqCategory/ListarCategoriasPartial")
            .then(r => r.text())
            .then(html => {
                contenedorLista.innerHTML = html;
            });
    }

    // Abrir modal de categorías
    btnAdminCat.addEventListener("click", function () {
        cargarListaCategorias();
        modalCategorias.show();
    });

    // Delegación: botón Editar en la tabla
    contenedorLista.addEventListener("click", function (e) {
        const btn = e.target.closest(".btn-editar-cat");
        if (!btn) return;

        const id = btn.dataset.id;

        fetch("/EqCategory/EditarCategoriaPartial?id=" + encodeURIComponent(id))
            .then(r => r.text())
            .then(html => {
                contenedorEditar.innerHTML = html;
                modalEditarCategoria.show();
            });
    });

    // Submit del form de edición (AJAX)
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
                    cargarListaCategorias(); // refrescar tabla
                } else {
                    alert(data.mensaje || "Ocurrió un error al actualizar la categoría.");
                }
            })
            .catch(() => {
                alert("Error al comunicarse con el servidor.");
            });
    });

    // 👉 Abrir modal de crear categoría desde la lista
    if (btnNuevaCatDesdeLista && modalEqCategory) {
        btnNuevaCatDesdeLista.addEventListener("click", function () {
            modalCategorias.hide();
            // Esto dispara el show.bs.modal que tenés en EqCategoryModal.js
            modalEqCategory.show();
        });
    }
});
