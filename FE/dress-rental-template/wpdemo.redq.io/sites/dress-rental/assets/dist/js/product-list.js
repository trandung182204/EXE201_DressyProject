/**
 * Product List - API Integration for Listing Page
 * Connects to ASP.NET Core backend with pagination, sorting, and filtering
 */

const isLocal =
  location.hostname === "localhost" ||
  location.hostname === "127.0.0.1";

// Production: API_BASE rỗng để gọi /api/... (nginx proxy đến backend port 5000)
// Local: sử dụng localhost:5135
const API_BASE = isLocal
  ? "http://localhost:5135"
  : "";

console.log("[PRODUCT-LIST] Environment:", isLocal ? "LOCAL" : "PRODUCTION");
console.log("[PRODUCT-LIST] API_BASE:", API_BASE);
console.log("[PRODUCT-LIST] Hostname:", location.hostname);

const ENDPOINT = `${API_BASE}/api/ProductsCustomer/listing`;
const CATEGORIES_ENDPOINT = `${API_BASE}/api/Categories`;

// State management
const state = {
  status: "AVAILABLE",
  page: 1,
  pageSize: 9,
  sortBy: "createdAt",
  sortDir: "desc",
  minPrice: null,
  maxPrice: null,
  categoryIds: [],
  sizes: [],
  colors: []
};

// Size mapping: checkbox id -> size value
const SIZE_MAP = {
  "size-one": "XXS",
  "size-two": "XS",
  "size-three": "S",
  "size-four": "M",
  "size-five": "L",
  "size-six": "XL"
};

// Color mapping: hex code -> color name (for API)
const COLOR_MAP = {
  "#D0021B": "Red",
  "#F5A623": "Orange",
  "#F8E71C": "Yellow",
  "#F43030": "Coral",
  "#D8D8D8": "Gray",
  "#25479B": "Navy",
  "#000000": "Black",
  "#7ED321": "Green",
  "#8B572A": "Brown",
  "#5F9FE8": "Blue",
  "#846E6E": "Taupe",
  "#F5AEAE": "Pink",
  "#50E3C2": "Teal",
  "#A3CEFF": "LightBlue",
  "#EAD7C3": "Beige"
};

/**
 * Build query string from state
 */
function buildQueryString() {
  const p = new URLSearchParams();
  p.set("status", state.status);
  p.set("page", state.page);
  p.set("pageSize", state.pageSize);
  p.set("sortBy", state.sortBy);
  p.set("sortDir", state.sortDir);

  if (state.minPrice != null) p.set("minPrice", state.minPrice);
  if (state.maxPrice != null) p.set("maxPrice", state.maxPrice);

  state.categoryIds.forEach(id => p.append("categoryIds", id));
  state.sizes.forEach(s => p.append("sizes", s));
  state.colors.forEach(c => p.append("colors", c));

  return p.toString();
}

/**
 * Format money in Vietnamese dong
 */
function money(v) {
  if (v == null) return "";
  return new Intl.NumberFormat("vi-VN").format(v) + " ₫/ngày";
}

/**
 * Safe image URL handling
 */
function safeImg(url) {
  if (!url) return "../assets/dist/img/products-list/grid1.jpg";
  if (url.startsWith("http")) return url;
  return API_BASE + url;
}

/**
 * Render product grid
 */
function renderGrid(items) {
  const grid = document.getElementById("productGrid");
  if (!grid) return;

  if (!items || items.length === 0) {
    grid.innerHTML = `<div class="col-sm-12"><p style="text-align:center; padding: 40px 0;">Không có sản phẩm phù hợp bộ lọc.</p></div>`;
    return;
  }

  grid.innerHTML = items.map(p => `
    <div class="col-sm-4 col-def">
      <div class="product-list">
        <figure>
          <a href="${safeImg(p.thumbnailUrl)}" data-lightbox="roadtrip">
            <img class="img-responsive" src="${safeImg(p.thumbnailUrl)}" alt="${p.name || ""}" />
          </a>
          <figcaption>
            <a href="#" class="left"><i class="fa fa-gift" aria-hidden="true"></i></a>
            <a href="#" class="right"><i class="fa fa-heart-o" aria-hidden="true"></i></a>
            <div class="quick-view">
              <a href="booking.html?id=${p.id}">Xem chi tiết</a>
            </div>
          </figcaption>
        </figure>
        <div class="description">
          <a href="booking.html?id=${p.id}" class="font-18-for-reg-0">${p.name || ""}</a>
          <p>${p.categoryName || ""}</p>
          <h5>${money(p.minPricePerDay)}</h5>
        </div>
      </div>
    </div>
  `).join("");
}

/**
 * Render pagination controls
 */
function renderPagination(page, totalPages) {
  const ul = document.getElementById("pagination");
  if (!ul) return;

  const mk = (label, p, disabled, active) => `
    <li class="${disabled ? "disabled" : ""}">
      <a href="#" class="${active ? "active" : ""}" data-page="${p}">${label}</a>
    </li>
  `;

  let html = "";

  // Previous button
  html += mk(`<i class="fa fa-angle-left" aria-hidden="true"></i>`, page - 1, page <= 1, false);

  // Page numbers with ellipsis
  const start = Math.max(1, page - 2);
  const end = Math.min(totalPages, page + 2);

  if (start > 1) html += mk("1", 1, false, page === 1);
  if (start > 2) html += mk("...", page, true, false);

  for (let i = start; i <= end; i++) {
    html += mk(String(i), i, false, i === page);
  }

  if (end < totalPages - 1) html += mk("...", page, true, false);
  if (end < totalPages) html += mk(String(totalPages), totalPages, false, page === totalPages);

  // Next button
  html += mk(`<i class="fa fa-angle-right" aria-hidden="true"></i>`, page + 1, page >= totalPages, false);

  ul.innerHTML = html;

  // Pagination click handlers
  ul.querySelectorAll("a[data-page]").forEach(a => {
    a.addEventListener("click", (e) => {
      e.preventDefault();
      const targetPage = Number(a.dataset.page);
      if (!targetPage || targetPage < 1 || targetPage > totalPages || targetPage === state.page) return;
      state.page = targetPage;
      load();
    });
  });
}

/**
 * Update page info text
 */
function updatePageInfo(page, pageSize, totalItems) {
  const infoEl = document.querySelector(".pages-on");
  if (infoEl) {
    const from = totalItems > 0 ? (page - 1) * pageSize + 1 : 0;
    const to = Math.min(page * pageSize, totalItems);
    infoEl.textContent = `${from}-${to} of ${totalItems}`;
  }
}

/**
 * Load products from API
 */
async function load() {
  try {
    const qs = buildQueryString();
    const res = await fetch(`${ENDPOINT}?${qs}`);

    if (!res.ok) {
      console.error("API error", res.status);
      const grid = document.getElementById("productGrid");
      if (grid) {
        grid.innerHTML = `<div class="col-sm-12"><p style="text-align:center; padding: 40px 0; color: red;">Không tải được sản phẩm. Vui lòng thử lại sau.</p></div>`;
      }
      return;
    }

    const data = await res.json();

    // Update total count
    const totalEl = document.getElementById("totalCount");
    if (totalEl) totalEl.textContent = `(${data.totalItems || 0})`;

    // Update page info
    updatePageInfo(data.page || 1, data.pageSize || 9, data.totalItems || 0);

    // Render grid and pagination
    renderGrid(data.items || []);
    renderPagination(data.page || 1, data.totalPages || 1);

  } catch (error) {
    console.error("Load products failed:", error);
    const grid = document.getElementById("productGrid");
    if (grid) {
      grid.innerHTML = `<div class="col-sm-12"><p style="text-align:center; padding: 40px 0; color: red;">Không tải được sản phẩm. Vui lòng thử lại sau.</p></div>`;
    }
  }
}

/**
 * Initialize sort dropdown - using value-based approach
 */
function initSort() {
  const select = document.getElementById("sortSelect");
  if (!select) return;

  select.addEventListener("change", () => {
    const value = select.value;

    switch (value) {
      case "name-asc":
        state.sortBy = "name";
        state.sortDir = "asc";
        break;
      case "price-asc":
        state.sortBy = "price";
        state.sortDir = "asc";
        break;
      case "price-desc":
        state.sortBy = "price";
        state.sortDir = "desc";
        break;
      case "newest":
      default:
        state.sortBy = "createdAt";
        state.sortDir = "desc";
        break;
    }

    state.page = 1;
    load();
  });
}

/**
 * Initialize size filter checkboxes
 */
function initSizeFilter() {
  Object.keys(SIZE_MAP).forEach(checkboxId => {
    const cb = document.getElementById(checkboxId);
    if (!cb) return;

    cb.addEventListener("change", () => {
      const size = SIZE_MAP[checkboxId];
      if (cb.checked) {
        if (!state.sizes.includes(size)) state.sizes.push(size);
      } else {
        state.sizes = state.sizes.filter(s => s !== size);
      }
      state.page = 1;
      load();
    });
  });
}

/**
 * Initialize color filter
 */
function initColorFilter() {
  const colorChooser = document.querySelector(".color-chooser");
  if (!colorChooser) return;

  colorChooser.querySelectorAll("span").forEach(span => {
    // Try data-color attribute first
    let colorName = span.dataset.color;

    // Fallback: try to get from background style
    if (!colorName) {
      const bgColor = span.style.background || span.style.backgroundColor;
      if (bgColor) {
        // Match hex color
        const hexMatch = bgColor.match(/#[0-9A-Fa-f]{6}/);
        if (hexMatch) {
          colorName = COLOR_MAP[hexMatch[0].toUpperCase()];
        }
      }
    }

    if (!colorName) return; // Skip if no color can be determined

    span.style.cursor = "pointer";
    span.title = colorName;

    span.addEventListener("click", () => {
      span.classList.toggle("selected");

      if (span.classList.contains("selected")) {
        if (!state.colors.includes(colorName)) state.colors.push(colorName);
        span.style.boxShadow = "0 0 0 3px #333";
      } else {
        state.colors = state.colors.filter(c => c !== colorName);
        span.style.boxShadow = "";
      }

      state.page = 1;
      load();
    });
  });
}

/**
 * Load categories from API and render dynamically
 */
async function loadCategories() {
  const container = document.getElementById("categoryList");
  if (!container) return;

  try {
    const res = await fetch(CATEGORIES_ENDPOINT);
    if (!res.ok) {
      console.error("Failed to load categories", res.status);
      container.innerHTML = `<p style="padding: 10px; color: #999;">Không tải được danh mục</p>`;
      return;
    }

    const response = await res.json();
    // API returns { success, data, message }
    const categories = response.data || response || [];

    if (!categories.length) {
      container.innerHTML = `<p style="padding: 10px; color: #999;">Không có danh mục</p>`;
      return;
    }

    // Render category checkboxes
    container.innerHTML = categories.map(cat => `
      <div>
        <span class="rq-checkbox">
          <input type="checkbox" id="cat-${cat.id}" data-cat-id="${cat.id}">
          <label for="cat-${cat.id}">${cat.name || cat.categoryName || 'Danh mục ' + cat.id}</label>
        </span>
      </div>
    `).join("");

    // Attach event listeners to new checkboxes
    categories.forEach(cat => {
      const cb = document.getElementById(`cat-${cat.id}`);
      if (!cb) return;

      cb.addEventListener("change", () => {
        const catId = String(cat.id);
        if (cb.checked) {
          if (!state.categoryIds.includes(catId)) state.categoryIds.push(catId);
        } else {
          state.categoryIds = state.categoryIds.filter(id => id !== catId);
        }
        state.page = 1;
        load();
      });
    });

  } catch (error) {
    console.error("Load categories failed:", error);
    container.innerHTML = `<p style="padding: 10px; color: #999;">Không tải được danh mục</p>`;
  }
}

/**
 * Initialize price range filter
 */
function initPriceFilter() {
  const rangeInput = document.getElementById("range_id");
  if (!rangeInput) return;

  // Try to hook into ionRangeSlider if available
  const checkSlider = setInterval(() => {
    if (typeof $ !== "undefined") {
      const sliderData = $(rangeInput).data("ionRangeSlider");
      if (sliderData) {
        clearInterval(checkSlider);

        // Override the onFinish callback
        const originalOnFinish = sliderData.options.onFinish || (() => { });
        sliderData.update({
          onFinish: (data) => {
            originalOnFinish(data);
            state.minPrice = data.from;
            state.maxPrice = data.to;
            state.page = 1;
            load();
          }
        });
      }
    }
  }, 500);

  // Fallback: clear interval after 5 seconds if slider not found
  setTimeout(() => clearInterval(checkSlider), 5000);

  // Also watch for manual input changes
  rangeInput.addEventListener("change", () => {
    const val = rangeInput.value;
    if (val && val.includes(";")) {
      const [min, max] = val.split(";").map(Number);
      if (!isNaN(min) && !isNaN(max)) {
        state.minPrice = min;
        state.maxPrice = max;
        state.page = 1;
        load();
      }
    }
  });
}

/**
 * Initialize clear all filters button
 */
function initClearAll() {
  const clearBtn = document.querySelector(".filter-header a");
  if (!clearBtn) return;

  clearBtn.addEventListener("click", (e) => {
    e.preventDefault();

    // Reset state
    state.page = 1;
    state.minPrice = null;
    state.maxPrice = null;
    state.categoryIds = [];
    state.sizes = [];
    state.colors = [];
    state.sortBy = "createdAt";
    state.sortDir = "desc";

    // Uncheck all checkboxes
    document.querySelectorAll(".filters input[type='checkbox']").forEach(cb => {
      cb.checked = false;
    });

    // Clear color selection
    document.querySelectorAll(".color-chooser span").forEach(span => {
      span.classList.remove("selected");
      span.style.boxShadow = "";
    });

    // Reset sort dropdown
    const select = document.getElementById("sortSelect");
    if (select) select.selectedIndex = 0;

    // Reset price slider if available
    const rangeInput = document.getElementById("range_id");
    if (rangeInput && typeof $ !== "undefined") {
      const sliderData = $(rangeInput).data("ionRangeSlider");
      if (sliderData) sliderData.reset();
    }

    load();
  });
}

/**
 * Initialize all event handlers
 */
async function init() {
  // Load categories from API first
  await loadCategories();

  // Initialize filters and sort
  initSort();
  initSizeFilter();
  initColorFilter();
  initPriceFilter();
  initClearAll();

  // Load products
  load();
}

// Start when DOM is ready
document.addEventListener("DOMContentLoaded", init);
