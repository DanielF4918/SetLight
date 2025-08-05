document.addEventListener("DOMContentLoaded", function () {

    const tablaResumen = document.getElementById("tablaResumenEquipos").querySelector("tbody");
    const inputsContainer = document.getElementById("inputsEquiposContainer");

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
        document.querySelectorAll(".cantidad-equipo").forEach(input => {
            const qty = parseInt(input.value);
            const val = parseFloat(input.dataset.value);
            if (!isNaN(qty) && qty > 0 && dias > 0) {
                subtotal += qty * val * dias;
            }
        });

        const iva = +(subtotal * 0.13).toFixed(2);
        const totalBruto = +(subtotal + iva).toFixed(2);


        const descuentoMonto = parseFloat(descuentoInput.value) || 0;
        const totalFinal = +(totalBruto - descuentoMonto).toFixed(2);


        subtotalEl.textContent = subtotal.toLocaleString('es-CR', { minimumFractionDigits: 2 });
        ivaEl.textContent = iva.toLocaleString('es-CR', { minimumFractionDigits: 2 });
        descuentoEl.textContent = descuentoMonto.toLocaleString('es-CR', { minimumFractionDigits: 2 });
        totalEl.textContent = totalFinal.toLocaleString('es-CR', { minimumFractionDigits: 2 });

        descuentoInput.value = descuentoMonto;
    }


    if (startDateInput) startDateInput.addEventListener("change", actualizarResumen);
    if (endDateInput) endDateInput.addEventListener("change", actualizarResumen);
    if (descuentoInput) {
        descuentoInput.addEventListener("input", actualizarResumen);
        descuentoInput.addEventListener("change", actualizarResumen);
    }


    document.addEventListener("click", function (e) {
        if (e.target && e.target.id === "btnAgregarEquipos") {
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
                    const val = parseFloat(input.dataset.value);
                    const subItem = qty * val * dias;

                    tablaResumen.innerHTML += `
                        <tr>
                            <td>${name}</td>
                            <td>${brand}</td>
                            <td>${model}</td>
                            <td>₡${val.toLocaleString('es-CR')}</td>
                            <td>${qty}</td>
                            <td>₡${subItem.toLocaleString('es-CR')}</td>
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
                }
            });

            actualizarResumen();

            if (!hayError) {
                const modal = bootstrap.Modal.getInstance(document.getElementById('modalSeleccionarEquipos'));
                if (modal) modal.hide();
            }
        }
    });


    actualizarResumen();
});
