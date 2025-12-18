function setupSeleccionarEquiposEvent() {
    const btnAgregar = document.getElementById("btnAgregarEquipos");
    if (!btnAgregar) return;

    btnAgregar.onclick = function () {
        const tablaResumen = document
            .getElementById("tablaResumenEquipos")
            ?.querySelector("tbody");

        const inputsContainer = document.getElementById("inputsEquiposContainer");

        if (!tablaResumen || !inputsContainer) return;

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
            const qty = parseInt(input.value, 10) || 0;
            const stock = parseInt(input.dataset.stock, 10) || 0;
            const seleccionado = parseInt(input.dataset.selected || "0", 10) || 0;
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

                // OJO: dataset.value ya fue "pisado" por el precio pactado si existía (ver aplicarSnapshotEnModal)
                const val = parseFloat(input.dataset.value) || 0;
                const subItem = qty * val * dias;

                // Fila con data-valor y data-cantidad para el cálculo
                tablaResumen.innerHTML += `
                    <tr data-valor="${val}" data-cantidad="${qty}">
                        <td>${name}</td>
                        <td>${brand}</td>
                        <td>${model}</td>
                        <td>$${val.toLocaleString('es-CR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}</td>
                        <td>${qty}</td>
                        <td>$${subItem.toLocaleString('es-CR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}</td>
                    </tr>
                `;

                // Hidden inputs para el POST
                const container = document.createElement("div");
                container.innerHTML = `
                    <input type="hidden" name="EquiposSeleccionados[${index}].EquipmentId" value="${id}" />
                    <input type="hidden" name="EquiposSeleccionados[${index}].EquipmentName" value="${name}" />
                    <input type="hidden" name="EquiposSeleccionados[${index}].Brand" value="${brand}" />
                    <input type="hidden" name="EquiposSeleccionados[${index}].Model" value="${model}" />

                    <!-- Compatibilidad actual -->
                    <input type="hidden" name="EquiposSeleccionados[${index}].RentalValue" value="${val}" />

                    <!-- ✅ precio pactado (snapshot) -->
                    <input type="hidden" name="EquiposSeleccionados[${index}].UnitRentalPrice" value="${val}" />

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

// =======================
// ✅ Helpers Misión 2: entrega
// =======================
function toNumber(v) {
    const n = parseFloat((v ?? "").toString().replace(",", "."));
    return isNaN(n) ? 0 : n;
}
function formatearCRC(n) {
    return (n || 0).toLocaleString("es-CR", { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}
function obtenerTransporte() {
    const isDelivery = !!(document.getElementById("IsDelivery")?.checked);
    if (!isDelivery) return 0;

    const tc = toNumber(document.getElementById("TransportCost")?.value);
    return tc < 0 ? 0 : tc;
}

function actualizarResumen() {
    const diasAlquilerEl = document.getElementById("diasAlquiler");
    const subtotalEl = document.getElementById("subtotalAlquiler");
    const ivaEl = document.getElementById("ivaAlquiler");
    const totalEl = document.getElementById("totalAlquiler");
    const descuentoEl = document.getElementById("descuentoAplicado");
    const descuentoInput = document.getElementById("DescuentoManual");

    // ✅ Misión 2
    const transporteEl = document.getElementById("transporteAlquiler");

    const dias = calcularDias();
    if (diasAlquilerEl) diasAlquilerEl.textContent = dias;

    let subtotal = 0;

    // Usar las filas de la tabla de equipos seleccionados
    const filas = document.querySelectorAll("#tablaResumenEquipos tbody tr");
    filas.forEach(row => {
        const val = parseFloat(row.dataset.valor) || 0;
        const qty = parseInt(row.dataset.cantidad, 10) || 0;

        if (qty > 0 && dias > 0) {
            subtotal += qty * val * dias;
        }
    });

    // ✅ Transporte (solo si IsDelivery)
    const transporte = obtenerTransporte();
    if (transporteEl) transporteEl.textContent = formatearCRC(transporte);

    // ✅ Regla consistente con Create:
    // Base = subtotal + transporte
    // IVA = 13% de base
    const base = +(subtotal + transporte).toFixed(2);
    const iva = +(base * 0.13).toFixed(2);
    const totalBruto = +(base + iva).toFixed(2);

    // Descuento manual en porcentaje
    const descuentoPct = toNumber(descuentoInput ? (descuentoInput.value || "0") : "0") || 0;

    const montoDescuento = +(totalBruto * (descuentoPct / 100)).toFixed(2);
    const totalFinal = +(totalBruto - montoDescuento).toFixed(2);

    if (subtotalEl) subtotalEl.textContent = formatearCRC(subtotal);
    if (ivaEl) ivaEl.textContent = formatearCRC(iva);
    if (descuentoEl) descuentoEl.textContent = formatearCRC(montoDescuento);
    if (totalEl) totalEl.textContent = formatearCRC(totalFinal);
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

/**
 * ✅ Lee los hidden inputs que vienen del servidor (Edit) para saber:
 * - qué equipos ya estaban seleccionados
 * - con qué cantidad
 * - y con qué precio pactado (UnitRentalPrice o RentalValue)
 */
function leerSeleccionadosDesdeHidden() {
    const map = {}; // { equipmentId: { qty, price } }

    const container = document.getElementById("inputsEquiposContainer");
    if (!container) return map;

    const idInputs = container.querySelectorAll('input[name$=".EquipmentId"]');

    idInputs.forEach(idInput => {
        const nameBase = idInput.name.replace(".EquipmentId", ""); // EquiposSeleccionados[0]
        const id = parseInt(idInput.value, 10);

        const qtyInput = container.querySelector(`input[name="${nameBase}.Quantity"]`);
        const priceInput =
            container.querySelector(`input[name="${nameBase}.UnitRentalPrice"]`) ||
            container.querySelector(`input[name="${nameBase}.RentalValue"]`);

        const qty = qtyInput ? (parseInt(qtyInput.value, 10) || 0) : 0;
        const price = priceInput ? (parseFloat(priceInput.value) || 0) : 0;

        if (!isNaN(id) && id > 0) {
            map[id] = { qty: qty, price: price };
        }
    });

    return map;
}

/**
 * ✅ Aplica el "precio pactado" al modal:
 * PISA input.dataset.value con el precio snapshot, y rehidrata cantidad.
 */
function aplicarSnapshotEnModal() {
    const selectedMap = leerSeleccionadosDesdeHidden();

    document.querySelectorAll(".cantidad-equipo").forEach(input => {
        const id = parseInt(input.dataset.id, 10);
        if (selectedMap[id]) {
            input.dataset.value = selectedMap[id].price;  // 🔒 precio pactado
            input.value = selectedMap[id].qty;            // 🔁 cantidad pactada

            const stock = parseInt(input.dataset.stock, 10) || 0;
            const seleccionado = parseInt(input.dataset.selected || "0", 10) || 0;
            const stockCell = document.getElementById("stock-display-" + id);
            if (stockCell) {
                const nuevoStock = stock + seleccionado - (parseInt(input.value, 10) || 0);
                stockCell.innerHTML = `<strong>${nuevoStock}</strong>`;
            }
        }
    });
}

document.addEventListener("DOMContentLoaded", function () {
    // Configuramos el botón del modal
    setupSeleccionarEquiposEvent();

    // ✅ Aplicar snapshot al cargar la página (antes de que el user abra el modal)
    aplicarSnapshotEnModal();

    // ✅ Aplicar snapshot cada vez que se abre el modal
    const modalEl = document.getElementById("modalSeleccionarEquipos");
    if (modalEl) {
        modalEl.addEventListener("shown.bs.modal", function () {
            aplicarSnapshotEnModal();
        });
    }

    // Validación y actualización visual de stock al cambiar cantidades en el modal
    document.addEventListener("input", function (e) {
        if (e.target && e.target.classList.contains("cantidad-equipo")) {
            const input = e.target;
            const qty = parseInt(input.value, 10) || 0;
            const stock = parseInt(input.dataset.stock, 10) || 0;
            const seleccionado = parseInt(input.dataset.selected || "0", 10) || 0;
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

    // Enganchamos el filtro UNA sola vez
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

    // ✅ Misión 2: listeners entrega
    const isDeliveryInput = document.getElementById("IsDelivery");
    const transportCostInput = document.getElementById("TransportCost");

    if (isDeliveryInput) {
        isDeliveryInput.addEventListener("change", function () {
            // Si quita entrega, forzamos transporte a 0 (y el controller también limpia)
            if (!isDeliveryInput.checked && transportCostInput) {
                transportCostInput.value = "0";
            }
            actualizarResumen();
        });
    }

    if (transportCostInput) {
        transportCostInput.addEventListener("input", function () {
            const v = toNumber(transportCostInput.value);
            if (v < 0) transportCostInput.value = "0";
            actualizarResumen();
        });
        transportCostInput.addEventListener("change", actualizarResumen);
    }

    // Recalcular una vez al cargar
    actualizarResumen();
});
