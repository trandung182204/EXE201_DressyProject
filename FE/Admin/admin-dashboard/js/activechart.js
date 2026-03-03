
const API_BASE = "http://localhost:5135/api";

/* =========================
   TOOL: CLEAR CHART SAFE
========================= */
function clearChart(canvasId) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return null;

    const oldChart = Chart.getChart(canvas);
    if (oldChart) oldChart.destroy();

    return canvas.getContext("2d");
}

/* =========================
   ACTIVE CREATOR CHART
========================= */
async function renderActiveCreatorChart() {
    try {
        const res = await fetch(`${API_BASE}/Users`);
        const result = await res.json();

        const users = result?.data || [];

        const activeCreators = users.filter(
            u => u.role === "Creator" && u.isActive === true
        ).length;

        document.querySelector(
            '.sherah-order-card__text span'
        ).textContent = activeCreators;

        const ctx = clearChart("myChart_active_creators");
        if (!ctx) return;

        new Chart(ctx, {
            type: "doughnut",
            data: {
                labels: ["Active Creator", "Others"],
                datasets: [{
                    data: [activeCreators, users.length - activeCreators],
                    backgroundColor: ["#4caf50", "#e0e0e0"]
                }]
            },
            options: {
                cutout: "70%",
                plugins: { legend: { display: false } }
            }
        });

    } catch (e) {
        console.error("Active Creator chart error:", e);
    }
}

/* =========================
   RECENT ORDER CHART
========================= */
async function renderRecentOrderChart() {
    try {
        const res = await fetch(`${API_BASE}/Bookings`);
        const result = await res.json();
        const orders = result?.data || [];

        const today = new Date();
        const labels = [];
        const data = [];

        for (let i = 6; i >= 0; i--) {
            const d = new Date(today);
            d.setDate(today.getDate() - i);

            const dayLabel = d.toLocaleDateString("vi-VN");
            labels.push(dayLabel);

            const count = orders.filter(o => {
                const od = new Date(o.createdAt);
                return od.toDateString() === d.toDateString();
            }).length;

            data.push(count);
        }

        document.querySelectorAll(
            '.sherah-order-card__text span'
        )[1].textContent = orders.length;

        const ctx = clearChart("myChart_recent_orders");
        if (!ctx) return;

        new Chart(ctx, {
            type: "line",
            data: {
                labels,
                datasets: [{
                    data,
                    tension: 0.4,
                    fill: false,
                    borderWidth: 2
                }]
            },
            options: {
                responsive: true,
                plugins: { legend: { display: false } },
                scales: {
                    y: { beginAtZero: true }
                }
            }
        });

    } catch (e) {
        console.error("Recent Order chart error:", e);
    }
}

/* =========================
   INIT
========================= */
document.addEventListener("DOMContentLoaded", () => {
    renderActiveCreatorChart();
    renderRecentOrderChart();
});
