function setupSeleccionarEquiposEvent() {
    const btnAgregar = document.getElementById("btnAgregarEquipos");
    if (!btnAgregar) return;

    btnAgregar.onclick = function () {
        const tablaResumen = document
            .getElementById("tablaResumenEquipos")
            .querySelector("tbody");
        const inputsContainer = document.getElementById("inputsEquiposContainer");

        // Limpiamos tabla y hidden inputs antes de reconstruir
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

                // Fila con data-valor y data-cantidad para el cálculo
                tablaResumen.innerHTML += `
                    <tr data-valor="${val}" data-cantidad="${qty}">
                        <td>${name}</td>
                        <td>${brand}</td>
                        <td>${model}</td>
                        <td>$${val.toLocaleString('es-CR')}</td>
                        <td>${qty}</td>
                        <td>$${subItem.toLocaleString('es-CR')}</td>
                    </tr>
                `;

                // Hidden inputs para el POST
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

        // Recalcular el resumen en base a la nueva tabla
        actualizarResumen();

        if (!hayError) {
            const modal = bootstrap.Modal.getInstance(
                document.getElementById("modalSeleccionarEquipos")
            );
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

    // Usar las filas de la tabla de equipos seleccionados
    const filas = document.querySelectorAll("#tablaResumenEquipos tbody tr");
    filas.forEach(row => {
        const val = parseFloat(row.dataset.valor);
        const qty = parseInt(row.dataset.cantidad);

        if (!isNaN(val) && !isNaN(qty) && qty > 0 && dias > 0) {
            subtotal += qty * val * dias;
        }
    });

    const iva = +(subtotal * 0.13).toFixed(2);
    const totalBruto = +(subtotal + iva).toFixed(2);

    // Descuento manual en porcentaje
    const descuentoPct = parseFloat(
        descuentoInput ? (descuentoInput.value || "0").replace(",", ".") : "0"
    ) || 0;

    const montoDescuento = +(totalBruto * (descuentoPct / 100)).toFixed(2);
    const totalFinal = +(totalBruto - montoDescuento).toFixed(2);

    if (subtotalEl)
        subtotalEl.textContent = subtotal.toLocaleString("es-CR", { minimumFractionDigits: 2 });
    if (ivaEl)
        ivaEl.textContent = iva.toLocaleString("es-CR", { minimumFractionDigits: 2 });
    if (descuentoEl)
        descuentoEl.textContent = montoDescuento.toLocaleString("es-CR", { minimumFractionDigits: 2 });
    if (totalEl)
        totalEl.textContent = totalFinal.toLocaleString("es-CR", { minimumFractionDigits: 2 });
}

function aplicarFiltroEquipos() {
    const input = document.getElementById("filtroEquipos");
    if (!input) return;

    const texto = (input.value || "").toLowerCase();

    const filas = document.querySelectorAll("#tablaEquiposSeleccionModal tbody tr");
    filas.forEach(row => {
        const nombreCell = row.querySelector(".col-nombre");
        const marcaCell = row.querySelector(".col-marca");
        const modeloCell = row.querySelector(".col-modelo");

        const nombre = nombreCell ? nombreCell.textContent.toLowerCase() : "";
        const marca = marcaCell ? marcaCell.textContent.toLowerCase() : "";
        const modelo = modeloCell ? modeloCell.textContent.toLowerCase() : "";

        const coincide =
            texto === "" ||
            nombre.indexOf(texto) !== -1 ||
            marca.indexOf(texto) !== -1 ||
            modelo.indexOf(texto) !== -1;

        row.style.display = coincide ? "" : "none";
    });
}

document.addEventListener("DOMContentLoaded", function () {
    // Configuramos el botón del modal
    setupSeleccionarEquiposEvent();

    // Validación y actualización visual de stock al cambiar cantidades en el modal
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

    // 👉 Enganchamos el filtro UNA sola vez
    const filtroEquiposInput = document.getElementById("filtroEquipos");
    if (filtroEquiposInput) {
        filtroEquiposInput.addEventListener("input", aplicarFiltroEquipos);
    }

    // Eventos para recalcular cuando cambian fechas o descuento
    const startDateInput = document.getElementById("StartDate");
    const endDateInput = document.getElementById("EndDate");
    const descuentoInput = document.getElementById("DescuentoManual");

    if (startDateInput) startDateInput.addEventListener("change", actualizarResumen);
    if (endDateInput) endDateInput.addEventListener("change", actualizarResumen);
    if (descuentoInput) {
        descuentoInput.addEventListener("input", actualizarResumen);
        descuentoInput.addEventListener("change", actualizarResumen);
    }

    // Recalcular una vez al cargar, usando los equipos que vienen del servidor
    actualizarResumen();
});
