async function inject(selector, url) {
  const el = document.querySelector(selector);
  if (!el) return;
  const res = await fetch(url, { cache: "no-store" });
  el.innerHTML = await res.text();
}

document.addEventListener("DOMContentLoaded", async () => {
  await inject("#app-header", "./header.html");
  await inject("#app-footer", "./footer.html");

  // GỌI SAU KHI HEADER ĐÃ VÀO DOM
  if (typeof renderAuthHeader === "function") renderAuthHeader();
});
