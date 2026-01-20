// assets/product.js
document.addEventListener("DOMContentLoaded", async () => {
  const row = document.getElementById("be-products-row");
  const tpl = document.getElementById("be-product-template");

  if (!row || !tpl) {
    console.error("Missing #be-products-row or #be-product-template");
    return;
  }

  // ✅ Backend API port (đổi đúng port BE của bạn)
  // Ví dụ bạn đang chạy Swagger ở http://localhost:5135 thì để 5135
  const API_BASE = "http://localhost:5135";
  const API_URL = `${API_BASE}/api/ProductsCustomer?status=AVAILABLE`;

  const formatVNDPerDay = (x) => {
    if (x == null || x === "") return "";
    const n = Number(x);
    if (Number.isNaN(n)) return "";
    return n.toLocaleString("vi-VN") + "₫/ngày";
  };

  const setText = (el, text) => {
    if (el) el.textContent = text ?? "";
  };

  const setLink = (el, href, title) => {
    if (!el) return;
    el.setAttribute("href", href || "#");
    if (title != null) el.setAttribute("title", title);
  };

  const setImage = (imgEl, url, alt) => {
    if (!imgEl) return;

    const fallback =
      "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsQAAA7EAZUrDhsAAAANSURBVBhXYzh8+PB/AAffA0nNPuCLAAAAAElFTkSuQmCC";

    const u = (url && String(url).trim()) ? url : fallback;

    // Theme dùng lazyload -> set cả data-src và src cho chắc ăn
    imgEl.setAttribute("data-src", u);
    imgEl.setAttribute("src", u);
    imgEl.setAttribute("alt", alt ?? "");
  };

  // Nếu bạn CHƯA có trang detail, cứ để "#"
  // Nếu bạn có detail theo id: return `../product-detail.html?id=${id}`;
  const buildDetailHref = (p) => "#";

  try {
    const res = await fetch(API_URL, {
      headers: { accept: "application/json" }
    });

    if (!res.ok) throw new Error(`API error: ${res.status}`);

    const items = await res.json();

    // Xóa hết nội dung hiện có (đảm bảo không còn dữ liệu mẫu)
    row.innerHTML = "";

    if (!Array.isArray(items) || items.length === 0) {
      row.innerHTML = `<div class="col-12">Không có sản phẩm.</div>`;
      return;
    }

    items.forEach((p) => {
      const card = tpl.content.firstElementChild.cloneNode(true);

      const href = buildDetailHref(p);
      const name = p?.name ?? "";
      const categoryName = (p?.categoryName && p.categoryName !== "???") ? p.categoryName : "";
      const thumb = p?.thumbnailUrl ?? "";

      // Link (cả ảnh và tên đều có class be-link)
      card.querySelectorAll(".be-link").forEach((a) => setLink(a, href, name));

      // Name / vendor / price
      setText(card.querySelector(".be-name"), name);
      setText(card.querySelector(".be-vendor"), categoryName);
      setText(card.querySelector(".be-price"), formatVNDPerDay(p?.minPricePerDay));

      // Images
      setImage(card.querySelector(".be-img1"), thumb, name);
      setImage(card.querySelector(".be-img2"), thumb, name);

      // variantId (nếu sau này bạn trả được variantId từ API)
      // const vInput = card.querySelector('input[name="variantId"]');
      // if (vInput) vInput.value = p?.variantId ?? "";

      row.appendChild(card);
    });
  } catch (err) {
    console.error(err);
    row.innerHTML = `<div class="col-12">Không tải được sản phẩm (kiểm tra API URL/CORS).</div>`;
  }
});
