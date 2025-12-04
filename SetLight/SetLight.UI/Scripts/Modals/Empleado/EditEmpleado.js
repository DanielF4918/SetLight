// Scripts/Empleado/EditEmpleado.js

document.addEventListener("DOMContentLoaded", function () {
    const btnGuardar = document.getElementById("btnGuardarEmpleado");
    const modalEl = document.getElementById("modalConfirmarPassword");
    const inputModalPassword = document.getElementById("txtAdminPasswordModal");
    const hiddenAdminPassword = document.getElementById("AdminPassword");
    const btnConfirmar = document.getElementById("btnConfirmarPassword");
    const errorModal = document.getElementById("errorModalPassword");
    const form = document.getElementById("formEditarEmpleado");

    if (!btnGuardar || !modalEl || !form) {
        return; // por si esta vista no tiene algo de esto
    }

    const modal = new bootstrap.Modal(modalEl);

    // 🔐 Si el servidor devolvió un error de contraseña, abrimos el modal y lo mostramos
    if (typeof adminPasswordServerError !== "undefined" && adminPasswordServerError) {
        if (errorModal) {
            errorModal.textContent = adminPasswordServerError;
            errorModal.classList.remove("d-none");
        }
        modal.show();
    }

    // 1) Click en "Guardar" → abrir modal
    btnGuardar.addEventListener("click", function () {
        // Limpiar mensaje solo si no viene de servidor
        if (!(typeof adminPasswordServerError !== "undefined" && adminPasswordServerError)) {
            if (errorModal) {
                errorModal.textContent = "";
                errorModal.classList.add("d-none");
            }
        }

        inputModalPassword.value = "";
        modal.show();
    });

    // 2) Click en "Confirmar y guardar" en el modal
    btnConfirmar.addEventListener("click", function () {
        const pwd = (inputModalPassword.value || "").trim();

        if (!pwd) {
            if (errorModal) {
                errorModal.textContent = "Debe ingresar su contraseña.";
                errorModal.classList.remove("d-none");
            }
            return;
        }

        // Pasar la contraseña al hidden ligado al modelo
        hiddenAdminPassword.value = pwd;

        // Enviar el formulario (el servidor dirá si la contraseña es correcta)
        modal.hide();
        form.submit();
    });
});
