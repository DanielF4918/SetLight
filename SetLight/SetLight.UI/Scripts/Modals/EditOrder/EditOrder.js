
document.addEventListener("DOMContentLoaded", function () {
    const startDateInput = document.getElementById("StartDate");
    const endDateInput = document.getElementById("EndDate");
    const descuentoInput = document.getElementById("DescuentoManual");

    const diasAlquilerEl = document.getElementById("diasAlquiler");
    const subtotalEl = document.getElementById("subtotalAlquiler");
    const ivaEl = document.getElementById("ivaAlquiler");
    const totalEl = document.getElementById("totalAlquiler");
    const descuentoEl = document.getElementById("descuentoAplicado");

    function calcularDias() {
        const start = new Date(startDateInput.value);
        const end = new Date(endDateInput.value);
        const diffMs = end - start;
        const diffDays = Math.ceil(diffMs / (1000 * 60 * 60 * 24)) + 1;
        return isNaN(diffDays) || diffDays <= 0 ? 0 : diffDays;
    }

    function actualizarResumen() {
        const dias = calcularDias();
        diasAlquilerEl.textContent = dias;

        let subtotal = 0;
        document.querySelectorAll("#tablaResumenEquipos tbody tr").forEach(row => {
            const valor = parseFloat(row.children[3].textContent.replace(/[₡,]/g, ''));
            const cantidad = parseInt(row.children[4].textContent);
            if (!isNaN(valor) && !isNaN(cantidad)) {
                subtotal += valor * cantidad * dias;
            }
        });

        const iva = +(subtotal * 0.13).toFixed(2);
        const totalBruto = +(subtotal + iva).toFixed(2);
        const descuentoPct = parseFloat(descuentoInput.value) || 0;
        const montoDescuento = +(totalBruto * (descuentoPct / 100)).toFixed(2);
        const totalFinal = +(totalBruto - montoDescuento).toFixed(2);

        subtotalEl.textContent = subtotal.toLocaleString('es-CR', { minimumFractionDigits: 2 });
        ivaEl.textContent = iva.toLocaleString('es-CR', { minimumFractionDigits: 2 });
        descuentoEl.textContent = montoDescuento.toLocaleString('es-CR', { minimumFractionDigits: 2 });
        totalEl.textContent = totalFinal.toLocaleString('es-CR', { minimumFractionDigits: 2 });
    }

    if (startDateInput) startDateInput.addEventListener("change", actualizarResumen);
    if (endDateInput) endDateInput.addEventListener("change", actualizarResumen);
    if (descuentoInput) {
        descuentoInput.addEventListener("input", actualizarResumen);
        descuentoInput.addEventListener("change", actualizarResumen);
    }

    actualizarResumen();





});
