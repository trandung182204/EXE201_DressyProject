const API_BASE =
    location.hostname === "localhost" || location.hostname === "127.0.0.1"
        ? "http://localhost:5135/api"
        : "/api";

// =======================
// DASHBOARD COUNTERS
// =======================
async function fetchDashboardData() {
    try {
        const [bookingsRes, cartsRes, usersRes, providersRes, paymentsRes] = await Promise.all([
            fetch(`${API_BASE}/Bookings`),
            fetch(`${API_BASE}/Carts`),
            fetch(`${API_BASE}/Users`),
            fetch(`${API_BASE}/Providers`),
            fetch(`${API_BASE}/Payments`)
        ]);

        const bookingsJson = await bookingsRes.json();
        const carts = await cartsRes.json();
        const users = await usersRes.json();
        const providers = await providersRes.json();
        const paymentsJson = await paymentsRes.json();

        const bookingsList = bookingsJson && bookingsJson.data ? bookingsJson.data : [];
        const paymentsList = paymentsJson && paymentsJson.data ? paymentsJson.data : [];

        const counters = document.querySelectorAll(
            '.sherah-progress-card__title b.count-animate'
        );

        // helper to format VND
        function formatVND(n) {
            try { return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(n || 0); }
            catch { return (n || 0) + ' VND'; }
        }

        // Compute earnings: use Payments list to find paid invoices, then fetch booking details to subtract deposits
        const now = new Date();
        const currentYear = now.getFullYear();

        const paidPayments = (paymentsList || []).filter(p => {
            const s = String(p.status || p.Status || '').toLowerCase();
            return s === 'success' || s === 'paid';
        });

        // Unique booking IDs that have successful payments
        const bookingIdSet = {};
        paidPayments.forEach(p => {
            const id = p.bookingId || p.BookingId || p.booking_id || p.Booking_Id || p.bookingId || p.BookingId;
            if (id) bookingIdSet[String(id)] = true;
        });

        const paidBookingIds = Object.keys(bookingIdSet);
        console.debug('[Dashboard] payments:', paymentsList.length, 'paid bookings:', paidBookingIds.length);

        // fetch booking details for these booking ids (in batches)
        async function fetchDetailsForIds(ids, batchSize = 12) {
            const results = [];
            for (let i = 0; i < ids.length; i += batchSize) {
                const batch = ids.slice(i, i + batchSize);
                const ps = batch.map(id => fetch(`${API_BASE}/Bookings/${id}/detail`).then(r => r.json()).catch(() => null));
                const res = await Promise.all(ps);
                results.push(...res);
            }
            return results;
        }

        const details = await fetchDetailsForIds(paidBookingIds, 12);

        let earnings = 0;
        const monthlySales = new Array(12).fill(0);

        for (let i = 0; i < details.length; i++) {
            const det = details[i];
            if (!det || !det.success || !det.data) continue;

            const booking = det.data;
            const total = Number(booking.totalPrice || booking.TotalPrice || booking.Total || 0);

            const items = booking.items || booking.Items || [];
            const depositSum = Array.isArray(items) ? items.reduce((s, it) => s + (Number(it.depositAmount || it.DepositAmount || 0) || 0), 0) : 0;

            const net = Math.max(0, total - depositSum);
            earnings += net;

            const createdAt = new Date(booking.createdAt || booking.CreatedAt || booking.created_at || booking.CreatedAtUtc || booking.CreatedAt);
            if (!isNaN(createdAt)) monthlySales[createdAt.getMonth()] += net;
        }
        console.debug('[Dashboard] computed earnings:', earnings, 'monthlySales:', monthlySales);

        if (counters.length >= 4) {
            counters[0].textContent = formatVND(earnings);
            counters[1].textContent = bookingsList.length || 0; // total orders
            counters[2].textContent = (users?.data || users)?.length || 0;
            counters[3].textContent = (providers?.data || providers)?.length || 0;
        }

        // --- build orders per month (counts) for current year ---
        const ordersPerMonth = new Array(12).fill(0);
        (bookingsList || []).forEach(b => {
            const created = new Date(b.createdAt || b.CreatedAt || b.created_at || b.CreatedAtUtc || b.CreatedAt);
            if (!isNaN(created) && created.getFullYear() === currentYear) {
                ordersPerMonth[created.getMonth()] += 1;
            }
        });

        // --- Render / update Total Sales chart (orders count per month) ---
        try {
            const ctxOrders = clearChart('myChart_Total_Sales_Home');
            if (ctxOrders) {
                const gradientBg = ctxOrders.createLinearGradient(0, 0, 0, 190);
                gradientBg.addColorStop(0, 'rgba(97,118,254,0.43)');
                gradientBg.addColorStop(1, 'rgba(97,118,254,0)');

                new Chart(ctxOrders, {
                    type: 'line',
                    data: {
                        labels: ['Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec'],
                        datasets: [{
                            label: 'Orders',
                            data: ordersPerMonth,
                            backgroundColor: gradientBg,
                            borderColor: '#6176FE',
                            borderWidth: 4,
                            pointRadius: 2,
                            tension: 0.4,
                            fill: true
                        }]
                    },
                    options: {
                        responsive: true,
                        maintainAspectRatio: false,
                        plugins: { legend: { display: false } },
                        scales: { x: { grid: { color: '#c5c5c573' } }, y: { beginAtZero: true } }
                    }
                });
            }
        } catch (e) { console.warn('Render orders chart failed', e); }

        // --- Render / update Monthly Statistics chart (earnings per month) ---
        try {
            const ctxEarn = clearChart('myChart_one_monthly');
            if (ctxEarn) {
                new Chart(ctxEarn, {
                    type: 'bar',
                    data: {
                        labels: ['Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec'],
                        datasets: [{
                            label: 'Earnings',
                            data: monthlySales.map(v => Math.round(v)),
                            backgroundColor: ['#4caf50'],
                        }]
                    },
                    options: {
                        responsive: true,
                        maintainAspectRatio: false,
                        plugins: { legend: { display: false } },
                        scales: { y: { beginAtZero: true } }
                    }
                });
            }
        } catch (e) { console.warn('Render earnings chart failed', e); }

    } catch (error) {
        console.error("Dashboard counter error:", error);
    }
}

// =======================
// MONTHLY CHART
// =======================
async function fetchMonthlyStatistics() {
    // Monthly statistics are rendered from fetchDashboardData (earnings per month)
    // Keep this function as a noop to avoid overwriting that chart.
    return;
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

