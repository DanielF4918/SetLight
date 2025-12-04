function mostrarEquiposModal(orderId) {
    const modalEl = document.getElementById('modalEquipos-' + orderId);
    const modal = bootstrap.Modal.getOrCreateInstance(modalEl);
    modal.show();
}
