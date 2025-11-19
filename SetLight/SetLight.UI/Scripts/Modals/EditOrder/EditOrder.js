function setupSeleccionarEquiposEvent() {
    const btnAgregar = document.getElementById("btnAgregarEquipos");
    if (!btnAgregar) return;
    btnAgregar.onclick = function () {
        const tablaResumen = document.getElementById("tablaResumenEquipos").querySelector("tbody");
        const inputsContainer = document.getElementById("inputsEquiposContainer");

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
            const qty = parseInt(input.value);
            const stock = parseInt(input.dataset.stock);
            const seleccionado = parseInt(input.dataset.selected || "0");
            const max = stock + seleccionado;

            if (qty > 0) {
                if (qty > max) {
                    alert(`No puede seleccionar más de ${max} unidades de "${input.dataset.name}".`);
                    hayError = true;
                    return;
                }

                const id = input.dataset.id;
                const name = input.dataset.name;
                const brand = input.dataset.brand;
                const model = input.dataset.model;
                const val = parseFloat(input.dataset.value);
                const subItem = qty * val * dias;


                tablaResumen.innerHTML += `
                    <tr>
                        <td>${name}</td>
                        <td>${brand}</td>
                        <td>${model}</td>
                        <td>$${val.toLocaleString('es-CR')}</td>
                        <td>${qty}</td>
                        <td>$${subItem.toLocaleString('es-CR')}</td>
                    </tr>
                `;

                const container = document.createElement("div");
                container.innerHTML = `
                    <input type="hidden" name="EquiposSeleccionados[${index}].EquipmentId" value="${id}" />
                    <input type="hidden" name="EquiposSeleccionados[${index}].EquipmentName" value="${name}" />
                    <input type="hidden" name="EquiposSeleccionados[${index}].Brand" value="${brand}" />
                    <input type="hidden" name="EquiposSeleccionados[${index}].Model" value="${model}" />
                    <input type="hidden" name="EquiposSeleccionados[${index}].RentalValue" value="${val}" />
                    <input type="hidden" name="EquiposSeleccionados[${index}].Quantity" value="${qty}" />
                `;
                inputsContainer.appendChild(container);
                index++;

                // Actualiza visualmente el stock disponible en tiempo real
                const stockCell = document.getElementById("stock-display-" + id);
                if (stockCell) {
                    const nuevoStock = stock + seleccionado - qty;
                    stockCell.innerHTML = `<strong>${nuevoStock}</strong>`;
                }
            }
        });

        actualizarResumen();

        if (!hayError) {
            const modal = bootstrap.Modal.getInstance(document.getElementById('modalSeleccionarEquipos'));
            if (modal) modal.hide();
        }
    };
}

function calcularDias() {
    const startDateInput = document.getElementById("StartDate");
    const endDateInput = document.getElementById("EndDate");
    if (!startDateInput || !endDateInput) return 0;
    const start = new Date(startDateInput.value);
    const end = new Date(endDateInput.value);
    const diffMs = end - start;
    const diffDays = Math.ceil(diffMs / (1000 * 60 * 60 * 24)) + 1;
    return isNaN(diffDays) || diffDays <= 0 ? 0 : diffDays;
}


function actualizarResumen() {
    const diasAlquilerEl = document.getElementById("diasAlquiler");
    const subtotalEl = document.getElementById("subtotalAlquiler");
    const ivaEl = document.getElementById("ivaAlquiler");
    const totalEl = document.getElementById("totalAlquiler");
    const descuentoEl = document.getElementById("descuentoAplicado");
    const descuentoInput = document.getElementById("DescuentoManual");

    const dias = calcularDias();
    if (diasAlquilerEl) diasAlquilerEl.textContent = dias;

    let subtotal = 0;
    document.querySelectorAll(".cantidad-equipo").forEach(input => {
        const qty = parseInt(input.value);
        const val = parseFloat(input.dataset.value);
        if (!isNaN(qty) && qty > 0 && dias > 0) {
            subtotal += qty * val * dias;
        }
    });

    const iva = +(subtotal * 0.13).toFixed(2);
    const totalBruto = +(subtotal + iva).toFixed(2);
    const descuentoPct = parseFloat(descuentoInput ? descuentoInput.value : "0") || 0;
    const montoDescuento = +(totalBruto * (descuentoPct / 100)).toFixed(2);
    const totalFinal = +(totalBruto - montoDescuento).toFixed(2);

    if (subtotalEl) subtotalEl.textContent = subtotal.toLocaleString('es-CR', { minimumFractionDigits: 2 });
    if (ivaEl) ivaEl.textContent = iva.toLocaleString('es-CR', { minimumFractionDigits: 2 });
    if (descuentoEl) descuentoEl.textContent = montoDescuento.toLocaleString('es-CR', { minimumFractionDigits: 2 });
    if (totalEl) totalEl.textContent = totalFinal.toLocaleString('es-CR', { minimumFractionDigits: 2 });
}


document.addEventListener("DOMContentLoaded", function () {
    setupSeleccionarEquiposEvent();


    document.addEventListener("input", function (e) {
        if (e.target && e.target.classList.contains("cantidad-equipo")) {
            const input = e.target;
            const qty = parseInt(input.value) || 0;
            const stock = parseInt(input.dataset.stock);
            const seleccionado = parseInt(input.dataset.selected || "0");
            const id = input.dataset.id;
            const max = stock + seleccionado;

            const stockCell = document.getElementById("stock-display-" + id);
            if (stockCell) {
                const nuevoStock = stock + seleccionado - qty;
                stockCell.innerHTML = `<strong>${nuevoStock}</strong>`;
            }

            if (qty > max) input.value = max;
            if (qty < 0) input.value = 0;
        }
    });


    const startDateInput = document.getElementById("StartDate");
    const endDateInput = document.getElementById("EndDate");
    const descuentoInput = document.getElementById("DescuentoManual");
    if (startDateInput) startDateInput.addEventListener("change", actualizarResumen);
    if (endDateInput) endDateInput.addEventListener("change", actualizarResumen);
    if (descuentoInput) {
        descuentoInput.addEventListener("input", actualizarResumen);
        descuentoInput.addEventListener("change", actualizarResumen);
    }
    actualizarResumen();
});
