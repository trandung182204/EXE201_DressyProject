// ../assets/dist/js/product-detail.js

const isLocal = location.hostname === "localhost" || location.hostname === "127.0.0.1";
const API_BASE = isLocal ? "http://localhost:5135" : "";

function formatVND(n) {
  try {
    return new Intl.NumberFormat("vi-VN", { style: "currency", currency: "VND" }).format(n ?? 0);
  } catch {
    return (n ?? 0) + " VND";
  }
}

function buildAssetUrl(pathOrUrl) {
  if (!pathOrUrl) return "";
  if (pathOrUrl.startsWith("http://") || pathOrUrl.startsWith("https://")) return pathOrUrl;
  return API_BASE + pathOrUrl; // "/api/media-files/112"
}

function getQueryParam(name) {
  const u = new URL(location.href);
  return u.searchParams.get(name);
}

function uniq(arr) {
  return Array.from(new Set(arr.filter(Boolean)));
}

// Chọn variant min theo price/deposit để hiển thị (nếu chưa chọn color/size)
function calcMin(values) {
  const nums = values.map(v => Number(v)).filter(v => !Number.isNaN(v) && v > 0);
  return nums.length ? Math.min(...nums) : 0;
}

async function fetchProductDetail(productId) {
  // ✅ endpoint bạn đã có sẵn: GET /api/Products/{id}
  // Nếu API này chỉ trả Products entity (không có images/variants), bạn cần thêm endpoint detail.
  const url = API_BASE + `/api/Products/${encodeURIComponent(productId)}`;
  const res = await fetch(url, { headers: { "Accept": "application/json" } });
  if (!res.ok) throw new Error(await res.text());
  const json = await res.json();
  return json.data ?? json; // tùy bạn bọc {success,data}
}

function renderBasic(product) {
  const name = product.name ?? product.Name ?? "";
  const desc = product.description ?? product.Description ?? "";
  const brand = product.providerBrandName ?? product.ProviderBrandName ?? product.brandName ?? product.BrandName ?? "Xúng Xính Store";

  const images = product.imageUrls ?? product.ImageUrls ?? [];
  const variants = product.variants ?? product.Variants ?? [];

  // ===== breadcrumb + title + desc =====
  const bc = document.getElementById("bc-product-name");
  if (bc) bc.textContent = name || "Chi tiết sản phẩm";

  const elName = document.getElementById("pd-name");
  if (elName) elName.textContent = name;

  const elBrand = document.getElementById("pd-brand");
  if (elBrand) elBrand.textContent = brand;

  const elDesc = document.getElementById("pd-desc");
  if (elDesc) elDesc.textContent = desc;

  // ===== giá (min) =====
  const minRent = calcMin(variants.map(v => v.pricePerDay ?? v.PricePerDay));
  const minDeposit = calcMin(variants.map(v => v.depositAmount ?? v.DepositAmount));

  const elRent = document.getElementById("pd-rent-price");
  if (elRent) elRent.textContent = `Giá thuê: ${formatVND(minRent)}`;

  const elDep = document.getElementById("pd-deposit");
  if (elDep) elDep.textContent = `Giá cọc: ${formatVND(minDeposit)}`;

  // ===== slider ảnh =====
  const slidesEl = document.getElementById("pd-slides");
  if (slidesEl) {
    const imgs = (images && images.length) ? images : [];
    slidesEl.innerHTML = imgs.map(u => {
      const full = buildAssetUrl(u);
      // flexslider cần data-thumb
      return `
        <li data-thumb="${full}">
          <img src="${full}" class="img-responsive" alt="${name}">
        </li>
      `;
    }).join("");

    // Re-init flexslider sau khi render
    if (window.jQuery && jQuery.fn.flexslider) {
      const $slider = jQuery(".product_details_slider");

      // nếu đã init rồi thì destroy (tùy phiên bản)
      try { $slider.flexslider(0); } catch {}

      // init lại
      $slider.flexslider({
        animation: "slide",
        controlNav: "thumbnails",
        slideshow: false
      });
    }
  }

  // ===== fill select color/size từ variants =====
  const colorSelect = document.getElementById("colorSelect");
  const sizeSelect = document.getElementById("sizeSelect");

  if (colorSelect) {
    const colors = uniq(variants.map(v => v.colorName ?? v.ColorName));
    colorSelect.innerHTML = `<option value="">Chọn màu sắc</option>` + colors.map(c => `<option value="${c}">${c}</option>`).join("");
  }
  if (sizeSelect) {
    const sizes = uniq(variants.map(v => v.sizeLabel ?? v.SizeLabel));
    sizeSelect.innerHTML = `<option value="">Chọn kích thước</option>` + sizes.map(s => `<option value="${s}">${s}</option>`).join("");
  }
}

async function bootProductDetail() {
  const productId = getQueryParam("id");
  if (!productId) {
    console.error("Missing product id in URL: booking.html?id=123");
    return;
  }

  try {
    const product = await fetchProductDetail(productId);
    renderBasic(product);
  } catch (e) {
    console.error("Load product detail failed:", e);
  }
}

document.addEventListener("DOMContentLoaded", bootProductDetail);