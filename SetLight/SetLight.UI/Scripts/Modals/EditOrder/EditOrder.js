document.addEventListener("DOMContentLoaded", function () {
    const btnAgregar = document.getElementById("btnAgregarEquipos");
    const tablaResumen = document.getElementById("tablaResumenEquipos").querySelector("tbody");
    const form = document.querySelector("form");

    // Cargar los equipos ya seleccionados al iniciar
    const equiposSeleccionados = [];

    document.querySelectorAll("input[type=hidden][name^='EquiposSeleccionados']").forEach(input => {
        const match = input.name.match(/EquiposSeleccionados\[(\d+)]\.(\w+)/);
        if (match) {
            const index = match[1];
            const prop = match[2];

            if (!equiposSeleccionados[index]) equiposSeleccionados[index] = {};
            equiposSeleccionados[index][prop] = input.value;
        }
    });

    // Cargar cantidades previas al abrir el modal
    const modal = document.getElementById('modalSeleccionarEquipos');
    modal.addEventListener('show.bs.modal', () => {
        document.querySelectorAll(".cantidad-equipo").forEach(input => {
            const id = input.dataset.id;
            const equipo = equiposSeleccionados.find(e => e.EquipmentId === id);
            if (equipo) {
                input.value = equipo.Quantity;
            } else {
                input.value = 0;
            }
        });
    });

    btnAgregar.addEventListener("click", function () {
        tablaResumen.innerHTML = "";
        document.querySelectorAll("input[type=hidden][name^='EquiposSeleccionados']").forEach(el => el.remove());

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

                // Agregar fila
                tablaResumen.innerHTML += `
                    <tr>
                        <td>${name}</td>
                        <td>${brand}</td>
                        <td>${model}</td>
                        <td>₡${value.toLocaleString('es-CR')}</td>
                        <td>${cantidad}</td>
                    </tr>
                `;

                // Agregar inputs ocultos
                const container = document.createElement("div");
                container.innerHTML = `
                    <input type="hidden" name="EquiposSeleccionados[${index}].EquipmentId" value="${id}" />
                    <input type="hidden" name="EquiposSeleccionados[${index}].EquipmentName" value="${name}" />
                    <input type="hidden" name="EquiposSeleccionados[${index}].Brand" value="${brand}" />
                    <input type="hidden" name="EquiposSeleccionados[${index}].Model" value="${model}" />
                    <input type="hidden" name="EquiposSeleccionados[${index}].RentalValue" value="${value}" />
                    <input type="hidden" name="EquiposSeleccionados[${index}].Quantity" value="${cantidad}" />
                `;
                form.appendChild(container);
                index++;
            }
        });

        if (!hayError) {
            const bsModal = bootstrap.Modal.getInstance(modal);
            bsModal.hide();
        }
    });
});
