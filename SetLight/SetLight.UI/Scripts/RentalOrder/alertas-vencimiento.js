(function () {
    // Configuración rápida:
    // 48h = “pronta a vencer”
    const THRESHOLD_HOURS = 48;
    // Si querés que sea “en vivo”, actualiza cada minuto:
    const REFRESH_MS = 60 * 1000;

    function hoursBetween(now, end) {
        return (end.getTime() - now.getTime()) / (1000 * 60 * 60);
    }

    function renderBadge(text, type) {
        // Bootstrap badges
        const cls = (type === "danger")
            ? "badge bg-danger"
            : "badge bg-warning text-dark";

        return `<span class="${cls}">${text}</span>`;
    }

    function paintOne(card) {
        const status = parseInt(card.getAttribute("data-status"), 10);
        const endRaw = card.getAttribute("data-end");

        const slot = card.querySelector(".js-order-alert");
        if (!slot) return;

        // Solo alertas para Activas (1)
        if (status !== 1) {
            slot.innerHTML = "";
            return;
        }

        const end = new Date(endRaw);
        if (isNaN(end.getTime())) {
            slot.innerHTML = "";
            return;
        }

        const now = new Date();
        const h = hoursBetween(now, end);

        if (h < 0) {
            slot.innerHTML = renderBadge("⛔ Vencida", "danger");
            return;
        }

        if (h <= THRESHOLD_HOURS) {
            // Redondeos amigables
            const hrsLeft = Math.ceil(h);
            const daysLeft = Math.floor(hrsLeft / 24);
            const remHrs = hrsLeft % 24;

            let txt;
            if (daysLeft >= 1) txt = `⚠️ Pronta a vencer (${daysLeft}d ${remHrs}h)`;
            else txt = `⚠️ Pronta a vencer (${hrsLeft}h)`;

            slot.innerHTML = renderBadge(txt, "warn");
            return;
        }

        slot.innerHTML = "";
    }

    function run() {
        document.querySelectorAll(".js-order-card").forEach(paintOne);
    }

    document.addEventListener("DOMContentLoaded", function () {
        run();
        // Actualización “en vivo”
        setInterval(run, REFRESH_MS);
    });
})();
