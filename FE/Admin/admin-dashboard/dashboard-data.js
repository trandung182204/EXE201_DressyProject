const API_BASE =
    location.hostname === "localhost" || location.hostname === "127.0.0.1"
        ? "http://localhost:5135/api"
        : "/api";

// =======================
// DASHBOARD COUNTERS
// =======================
async function fetchDashboardData() {
    try {
        const [bookingsRes, cartsRes, usersRes, providersRes] = await Promise.all([
            fetch(`${API_BASE}/Bookings`),
            fetch(`${API_BASE}/Carts`),
            fetch(`${API_BASE}/Users`),
            fetch(`${API_BASE}/Providers`)
        ]);

        const bookings = await bookingsRes.json();
        const carts = await cartsRes.json();
        const users = await usersRes.json();
        const providers = await providersRes.json();

        const counters = document.querySelectorAll(
            '.sherah-progress-card__title b.count-animate'
        );

        if (counters.length >= 4) {
            counters[0].textContent = bookings?.data?.length || 0;
            counters[1].textContent = carts?.data?.length || 0;
            counters[2].textContent = users?.data?.length || 0;
            counters[3].textContent = providers?.data?.length || 0;
        }

    } catch (error) {
        console.error("Dashboard counter error:", error);
    }
}

// =======================
// MONTHLY CHART
// =======================
async function fetchMonthlyStatistics() {
    try {
        const res = await fetch(`${API_BASE}/Statistics/monthly`);
        const result = await res.json();

        if (!result.success) return;

        const { profit, refunds, expenses } = result.data;

        const canvas = document.getElementById('myChart_one_monthly');
        if (!canvas) {
            console.warn("Canvas myChart_one_monthly not found");
            return;
        }

        // 🔥 CLEAR CHART CŨ (FIX LỖI Canvas is already in use)
        const oldChart = Chart.getChart(canvas);
        if (oldChart) {
            oldChart.destroy();
        }

        const ctx = canvas.getContext('2d');

        new Chart(ctx, {
            type: 'bar',
            data: {
                labels: ['Profit', 'Refunds', 'Expenses'],
                datasets: [{
                    label: 'Monthly Statistics',
                    data: [
                        Number(profit) || 0,
                        Number(refunds) || 0,
                        Number(expenses) || 0
                    ]
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false }
                },
                scales: {
                    y: {
                        beginAtZero: true
                    }
                }
            }
        });

    } catch (error) {
        console.error("Monthly chart error:", error);
    }
}

// =======================
// INIT AFTER DOM READY
// =======================
document.addEventListener('DOMContentLoaded', () => {
    fetchDashboardData();
    fetchMonthlyStatistics();
});

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

