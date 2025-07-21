document.addEventListener("DOMContentLoaded", function () {
    const btnAgregar = document.getElementById("btnAgregarEquipos");
    const tablaResumen = document.getElementById("tablaResumenEquipos").querySelector("tbody");
    const inputsContainer = document.getElementById("inputsEquiposContainer");

    // 🔍 Buscar equipos en tiempo real
    const inputBusqueda = document.getElementById("buscarEquipo");
    if (inputBusqueda) {
        inputBusqueda.addEventListener("input", function () {
            const filtro = this.value.toLowerCase();
            document.querySelectorAll("#tablaEquipos tbody tr").forEach(row => {
                const texto = row.textContent.toLowerCase();
                row.style.display = texto.includes(filtro) ? "" : "none";
            });
        });
    }

    // ➕ Agregar equipos seleccionados
    if (btnAgregar) {
        btnAgregar.addEventListener("click", function () {
            tablaResumen.innerHTML = "";
            inputsContainer.innerHTML = "";

            const cantidades = document.querySelectorAll(".cantidad-equipo");
            let index = 0;
            let hayError = false;

            cantidades.forEach(input => {
                const cantidad = parseInt(input.value);
                const stock = parseInt(input.dataset.stock);

                if (cantidad > 0) {
                    if (cantidad > stock) {
                        alert(`No puede seleccionar más de ${stock} unidades del equipo "${input.dataset.name}"`);
                        hayError = true;
                        return;
                    }

                    const id = input.dataset.id;
                    const name = input.dataset.name;
                    const brand = input.dataset.brand;
                    const model = input.dataset.model;
                    const value = parseFloat(input.dataset.value);

                    // Mostrar en tabla resumen
                    tablaResumen.innerHTML += `
                        <tr>
                            <td>${name}</td>
                            <td>${brand}</td>
                            <td>${model}</td>
                            <td>₡${value.toLocaleString('es-CR')}</td>
                            <td>${cantidad}</td>
                        </tr>
                    `;

                    // Agregar campos ocultos
                    const container = document.createElement("div");
                    container.innerHTML = `
                        <input type="hidden" name="EquiposSeleccionados[${index}].EquipmentId" value="${id}" />
                        <input type="hidden" name="EquiposSeleccionados[${index}].EquipmentName" value="${name}" />
                        <input type="hidden" name="EquiposSeleccionados[${index}].Brand" value="${brand}" />
                        <input type="hidden" name="EquiposSeleccionados[${index}].Model" value="${model}" />
                        <input type="hidden" name="EquiposSeleccionados[${index}].RentalValue" value="${value}" />
                        <input type="hidden" name="EquiposSeleccionados[${index}].Quantity" value="${cantidad}" />
                    `;
                    inputsContainer.appendChild(container);
                    index++;
                }
            });

            if (!hayError) {
                const modal = bootstrap.Modal.getInstance(document.getElementById('modalSeleccionarEquipos'));
                modal.hide();
            }
        });
    }
});
