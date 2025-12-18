document.addEventListener("DOMContentLoaded", function () {

    const tablaResumen = document.getElementById("tablaResumenEquipos")?.querySelector("tbody");
    const inputsContainer = document.getElementById("inputsEquiposContainer");

    const startDateInput = document.getElementById("StartDate");
    const endDateInput = document.getElementById("EndDate");
    const descuentoInput = document.getElementById("DescuentoManual");

    const diasAlquilerEl = document.getElementById("diasAlquiler");
    const subtotalEl = document.getElementById("subtotalAlquiler");
    const ivaEl = document.getElementById("ivaAlquiler");
    const totalEl = document.getElementById("totalAlquiler");
    const descuentoEl = document.getElementById("descuentoAplicado");

    // =======================
    // ✅ Misión 2: Entrega
    // =======================
    const isDeliveryInput = document.getElementById("IsDelivery");
    const transportCostInput = document.getElementById("TransportCost");
    const transporteEl = document.getElementById("transporteAlquiler"); // span del resumen

    function toNumber(v) {
        const n = parseFloat((v ?? "").toString().replace(",", "."));
        return isNaN(n) ? 0 : n;
    }

    function formatearCRC(n) {
        return (n || 0).toLocaleString('es-CR', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    }

    function calcularDias() {
        const start = new Date(startDateInput?.value);
        const end = new Date(endDateInput?.value);
        const diffMs = end - start;
        const diffDays = Math.ceil(diffMs / (1000 * 60 * 60 * 24)) + 1; // inclusivo
        return isNaN(diffDays) || diffDays <= 0 ? 0 : diffDays;
    }

    function obtenerTransporte() {
        const isDelivery = !!(isDeliveryInput && isDeliveryInput.checked);

        if (!isDelivery) return 0;

        let tc = toNumber(transportCostInput ? transportCostInput.value : 0);
        if (tc < 0) tc = 0; // blindaje extra
        return tc;
    }

    function actualizarResumen() {
        const dias = calcularDias();
        if (diasAlquilerEl) diasAlquilerEl.textContent = dias;

        let subtotal = 0;

        document.querySelectorAll(".cantidad-equipo").forEach(input => {
            const qty = parseInt(input.value, 10) || 0;
            const val = parseFloat(input.dataset.value) || 0;

            if (qty > 0 && dias > 0) {
                subtotal += qty * val * dias;
            }
        });

        // ✅ Transporte (solo si IsDelivery = true)
        const transporte = obtenerTransporte();
        if (transporteEl) transporteEl.textContent = formatearCRC(transporte);

        // =======================
        // ✅ Regla de cálculo (simple y consistente):
        // Base gravable = subtotal + transporte
        // IVA = 13% de base
        // Descuento = % aplicado sobre totalBruto (como ya lo tenías)
        // =======================
        const base = +(subtotal + transporte).toFixed(2);

        const iva = +(base * 0.13).toFixed(2);
        const totalBruto = +(base + iva).toFixed(2);

        const descuentoPct = toNumber(descuentoInput?.value) || 0;
        const montoDescuento = +(totalBruto * (descuentoPct / 100)).toFixed(2);
        const totalFinal = +(totalBruto - montoDescuento).toFixed(2);

        if (subtotalEl) subtotalEl.textContent = formatearCRC(subtotal);
        if (ivaEl) ivaEl.textContent = formatearCRC(iva);
        if (descuentoEl) descuentoEl.textContent = formatearCRC(montoDescuento);
        if (totalEl) totalEl.textContent = formatearCRC(totalFinal);
    }

    // =======================
    // Listeners: fechas y descuento
    // =======================
    if (startDateInput) startDateInput.addEventListener("change", actualizarResumen);
    if (endDateInput) endDateInput.addEventListener("change", actualizarResumen);

    if (descuentoInput) {
        descuentoInput.addEventListener("input", actualizarResumen);
        descuentoInput.addEventListener("change", actualizarResumen);
    }

    // =======================
    // ✅ Misión 2: listeners entrega
    // =======================
    if (isDeliveryInput) {
        isDeliveryInput.addEventListener("change", function () {
            // Si quita entrega: forzar transporte a 0 (y el controller lo limpia igual)
            if (!isDeliveryInput.checked) {
                if (transportCostInput) transportCostInput.value = "0";
            }
            actualizarResumen();
        });
    }

    if (transportCostInput) {
        transportCostInput.addEventListener("input", function () {
            // si escriben negativo, lo normalizamos visualmente
            const v = toNumber(transportCostInput.value);
            if (v < 0) transportCostInput.value = "0";
            actualizarResumen();
        });
        transportCostInput.addEventListener("change", actualizarResumen);
    }

    // =======================
    // Botón: Agregar equipos
    // =======================
    document.addEventListener("click", function (e) {
        if (e.target && e.target.id === "btnAgregarEquipos") {

            if (!tablaResumen || !inputsContainer) return;

            tablaResumen.innerHTML = "";
            inputsContainer.innerHTML = "";

            const cantidades = document.querySelectorAll(".cantidad-equipo");
            let index = 0;
            let hayError = false;

            const dias = calcularDias();
            if (dias === 0) {
                alert("Debe seleccionar un rango de fechas válido.");
                return;
            }

            cantidades.forEach(input => {
                const qty = parseInt(input.value, 10) || 0;
                const stock = parseInt(input.dataset.stock, 10) || 0;

                if (qty > 0) {
                    if (qty > stock) {
                        alert(`No puede seleccionar más de ${stock} unidades de "${input.dataset.name}".`);
                        hayError = true;
                        return;
                    }

                    const id = input.dataset.id;
                    const name = input.dataset.name;
                    const brand = input.dataset.brand;
                    const model = input.dataset.model;

                    const val = parseFloat(input.dataset.value) || 0; // precio mostrado actualmente
                    const subItem = qty * val * dias;

                    tablaResumen.innerHTML += `
                        <tr>
                            <td>${name}</td>
                            <td>${brand}</td>
                            <td>${model}</td>
                            <td>$${val.toLocaleString('es-CR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}</td>
                            <td>${qty}</td>
                            <td>$${subItem.toLocaleString('es-CR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}</td>
                        </tr>
                    `;

                    const container = document.createElement("div");
                    container.innerHTML = `
                        <input type="hidden" name="EquiposSeleccionados[${index}].EquipmentId" value="${id}" />
                        <input type="hidden" name="EquiposSeleccionados[${index}].EquipmentName" value="${name}" />
                        <input type="hidden" name="EquiposSeleccionados[${index}].Brand" value="${brand}" />
                        <input type="hidden" name="EquiposSeleccionados[${index}].Model" value="${model}" />

                        <!-- Compatibilidad con lo actual -->
                        <input type="hidden" name="EquiposSeleccionados[${index}].RentalValue" value="${val}" />

                        <!-- ✅ NUEVO: nombre semántico para precio pactado -->
                        <input type="hidden" name="EquiposSeleccionados[${index}].UnitRentalPrice" value="${val}" />

                        <input type="hidden" name="EquiposSeleccionados[${index}].Quantity" value="${qty}" />
                    `;
                    inputsContainer.appendChild(container);
                    index++;
                }
            });

            actualizarResumen();

            if (!hayError) {
                const modalEl = document.getElementById('modalSeleccionarEquipos');
                const modal = modalEl ? bootstrap.Modal.getInstance(modalEl) : null;
                if (modal) modal.hide();
            }
        }
    });

    // Inicial
    actualizarResumen();
});
