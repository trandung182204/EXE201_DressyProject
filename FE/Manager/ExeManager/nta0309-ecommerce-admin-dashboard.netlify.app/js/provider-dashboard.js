(function () {
  const isLocal = location.hostname === "localhost" || location.hostname === "127.0.0.1";
  const API_BASE = isLocal ? "http://localhost:5135" : "";

  function getToken() {
    return localStorage.getItem("token") || "";
  }

  async function apiGet(path) {
    const res = await fetch(API_BASE + path, {
      headers: { "Authorization": "Bearer " + getToken() }
    });
    if (res.status === 401) throw new Error("UNAUTHORIZED");
    return res.json();
  }

  function setText(id, text) {
    const el = document.getElementById(id);
    if (el) el.textContent = text;
  }

  function fmtMoney(v) {
    return Number(v || 0).toLocaleString("vi-VN") + " ₫";
  }

  async function loadSummary() {
    const json = await apiGet("/api/provider/dashboard/summary");
    const d = json.data || {};

    setText("kpi-total-revenue", fmtMoney(d.totalRevenue));
    setText("kpi-total-orders", Number(d.totalOrders || 0).toLocaleString("vi-VN"));
    setText("kpi-orders-today", Number(d.ordersToday || 0).toLocaleString("vi-VN"));
    setText("kpi-revenue-today", fmtMoney(d.revenueToday));
  }

  async function loadRevenueByMonth(totalSalesChart) {
    const year = new Date().getFullYear();
    const json = await apiGet("/api/provider/dashboard/revenue-by-month?year=" + year);
    const rows = json.data || [];

    const labels = ["Jan","Feb","Mar","Apr","May","Jun","Jul","Aug","Sep","Oct","Nov","Dec"];
    const values = Array(12).fill(0);

    rows.forEach(r => {
      const m = Number(r.month);
      if (m >= 1 && m <= 12) values[m - 1] = Number(r.revenue || 0);
    });

    if (totalSalesChart) {
      totalSalesChart.data.labels = labels;
      totalSalesChart.data.datasets[0].data = values;
      totalSalesChart.update();
    }
  }

  // gọi từ index sau khi chart đã tạo
  window.initProviderDashboard = async function (charts) {
    await loadSummary();
    await loadRevenueByMonth(charts?.totalSales);
  };
})();