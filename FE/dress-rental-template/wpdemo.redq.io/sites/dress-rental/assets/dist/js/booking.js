/**
 * Booking Page - Product Detail & Date Validation
 * Fetches product details and validates rental duration
 * NO alert() - all errors displayed inline
 * Button 'Rent Now' is ALWAYS ENABLED - validation on click
 * Supports Select2 dynamic options (Correctly handles re-init)
 */

const isLocal =
    location.hostname === "localhost" ||
    location.hostname === "127.0.0.1";

// Production: API_BASE rỗng để gọi /api/... (nginx proxy đến backend port 5000)
// Local: sử dụng localhost:5135
const API_BASE = isLocal ? "http://localhost:5135" : "";

console.log("[BOOKING] Environment:", isLocal ? "LOCAL" : "PRODUCTION");
console.log("[BOOKING] API_BASE:", API_BASE);
console.log("[BOOKING] Hostname:", location.hostname);

// Current product state
let currentProduct = null;
let selectedColor = "";
let selectedSize = "";
let startDate = null;
let endDate = null;
let rentalDays = 0;

/**
 * Get product ID from URL query string
 */
function getProductId() {
    const params = new URLSearchParams(window.location.search);
    return params.get("id");
}

/**
 * Format money in Vietnamese dong
 */
function money(v) {
    if (v == null) return "0 ₫";
    return new Intl.NumberFormat("vi-VN").format(v) + " ₫";
}

/**
 * Safe image URL handling
 */
function safeImg(url) {
    if (!url) return "../assets/dist/img/product-details/1.jpg";
    if (url.startsWith("http")) return url;
    return API_BASE + url;
}

/**
 * Show error message on the page (for critical errors like missing product)
 */
function showError(message) {
    const container = document.querySelector(".rq-inner");
    if (container) {
        container.innerHTML = `
      <div style="padding: 40px; text-align: center; color: red;">
        <p>${message}</p>
        <a href="listing-page.html" style="color: #333;">← Quay lại danh sách sản phẩm</a>
      </div>
    `;
    }
}

/**
 * Render image gallery with thumbnails
 */
function renderImageGallery(product) {
    const slidesContainer = document.getElementById("pd-slides");
    if (!slidesContainer) return;

    const images = product.imageUrls && product.imageUrls.length > 0
        ? product.imageUrls
        : (product.thumbnailUrl ? [product.thumbnailUrl] : []);

    if (images.length === 0) {
        slidesContainer.innerHTML = `<li><img src="../assets/dist/img/product-details/1.jpg" alt="No image"></li>`;
        return;
    }

    // Build gallery HTML
    const galleryHTML = `
        <div id="main-image-wrapper" style="position:relative; text-align:center; background:#f9f9f9; min-height:350px; display:flex; align-items:center; justify-content:center;">
            <img id="main-product-img" src="${safeImg(images[0])}" alt="${product.name || ''}" style="max-width:100%; max-height:450px; object-fit:contain;">
            ${images.length > 1 ? `
                <button onclick="galleryPrev()" style="position:absolute;left:10px;top:50%;transform:translateY(-50%);background:rgba(0,0,0,0.4);color:#fff;border:none;width:36px;height:36px;border-radius:50%;cursor:pointer;font-size:18px;">
                    <i class="fa fa-chevron-left"></i>
                </button>
                <button onclick="galleryNext()" style="position:absolute;right:10px;top:50%;transform:translateY(-50%);background:rgba(0,0,0,0.4);color:#fff;border:none;width:36px;height:36px;border-radius:50%;cursor:pointer;font-size:18px;">
                    <i class="fa fa-chevron-right"></i>
                </button>
            ` : ''}
        </div>
        ${images.length > 1 ? `
            <div id="thumb-container" style="display:flex;gap:8px;margin-top:10px;overflow-x:auto;padding:5px 0;">
                ${images.map((img, i) => `
                    <img src="${safeImg(img)}" alt="thumb-${i}"
                         onclick="setMainImage(${i})"
                         class="gallery-thumb ${i === 0 ? 'active-thumb' : ''}"
                         style="width:70px;height:70px;object-fit:cover;border:2px solid ${i === 0 ? '#d0021b' : '#ddd'};border-radius:4px;cursor:pointer;flex-shrink:0;">
                `).join('')}
            </div>
        ` : ''}
    `;

    slidesContainer.innerHTML = galleryHTML;
    window._galleryImages = images;
    window._galleryIndex = 0;
}

function setMainImage(index) {
    const images = window._galleryImages || [];
    if (index < 0 || index >= images.length) return;
    window._galleryIndex = index;
    const mainImg = document.getElementById('main-product-img');
    if (mainImg) mainImg.src = safeImg(images[index]);
    // Update thumb borders
    document.querySelectorAll('.gallery-thumb').forEach((t, i) => {
        t.style.border = i === index ? '2px solid #d0021b' : '2px solid #ddd';
    });
}

function galleryPrev() {
    const images = window._galleryImages || [];
    if (images.length <= 1) return;
    let idx = (window._galleryIndex - 1 + images.length) % images.length;
    setMainImage(idx);
}

function galleryNext() {
    const images = window._galleryImages || [];
    if (images.length <= 1) return;
    let idx = (window._galleryIndex + 1) % images.length;
    setMainImage(idx);
}

/**
 * Find matching variant by color + size
 */
function findVariant(color, size) {
    if (!currentProduct || !currentProduct.variants) return null;
    return currentProduct.variants.find(v =>
        (v.colorName === color || !color || color === 'N/A') &&
        (v.sizeLabel === size || !size || size === 'N/A')
    );
}

/**
 * Update price, deposit, and stock based on selected variant
 */
function updateVariantInfo() {
    const variant = findVariant(selectedColor, selectedSize);
    const rentEl = document.getElementById('pd-rent-price');
    const depositEl = document.getElementById('pd-deposit');
    const qtyEl = document.getElementById('pd-quantity');
    const reserveBtn = document.querySelector('.reserve');

    if (variant) {
        const price = variant.pricePerDay || currentProduct.minPricePerDay || 0;
        const deposit = variant.depositAmount || 0;
        const qty = variant.quantity || 0;

        if (rentEl) rentEl.innerHTML = `Giá thuê: <span style="color:#d0021b;font-weight:bold;">${money(price)}</span> /ngày`;
        if (depositEl) depositEl.innerHTML = `Giá cọc: <span style="color:#333;font-weight:bold;">${money(deposit)}</span>`;
        if (qtyEl) {
            if (qty > 0) {
                qtyEl.innerHTML = `Còn lại: <span style="color:#4CAF50;font-weight:bold;">${qty} sản phẩm</span>`;
                qtyEl.style.color = '';
            } else {
                qtyEl.innerHTML = `<span style="color:#f44336;font-weight:bold;"><i class="fa fa-ban"></i> Hết hàng</span>`;
            }
        }

        // Disable reserve button if out of stock
        if (reserveBtn) {
            if (qty <= 0) {
                reserveBtn.style.opacity = '0.5';
                reserveBtn.style.pointerEvents = 'none';
                reserveBtn.textContent = 'Hết hàng';
            } else {
                reserveBtn.style.opacity = '1';
                reserveBtn.style.pointerEvents = 'auto';
                reserveBtn.textContent = 'Đặt thuê ngay';
            }
        }

        // Store selected variant info
        currentProduct._selectedVariant = variant;
    } else {
        const price = currentProduct.minPricePerDay || currentProduct.pricePerDay || 0;
        if (rentEl) rentEl.innerHTML = `Giá thuê: <span style="color:#d0021b;font-weight:bold;">${money(price)}</span> /ngày`;
        if (depositEl) depositEl.innerHTML = `Giá cọc: --`;
        if (qtyEl) qtyEl.innerHTML = `Số lượng: <span style="color:#999;">Chọn màu & size</span>`;
    }
}

/**
 * Render product details to the page
 */
function renderProduct(product) {
    currentProduct = product;

    // Update breadcrumb
    const breadcrumb = document.querySelector(".breadcrumb li:last-child");
    if (breadcrumb) {
        breadcrumb.textContent = product.name || "Chi tiết sản phẩm";
    }

    // Update product name
    const nameEl = document.querySelector(".rq-inner > a.font-30-for-reg-0");
    if (nameEl) {
        nameEl.textContent = product.name || "";
        nameEl.id = "productName";
    }

    // Update brand/category
    const brandEl = document.querySelector(".rq-inner > h3.font-18-for-reg-0");
    if (brandEl) {
        brandEl.innerHTML = `Danh mục: <span id="productCategory">${product.categoryName || ""}</span>`;
    }

    // Update price & deposit
    const price = product.minPricePerDay || product.pricePerDay || 0;
    const rentEl = document.getElementById('pd-rent-price');
    if (rentEl) rentEl.innerHTML = `Giá thuê: <span style="color:#d0021b;font-weight:bold;">${money(price)}</span> /ngày`;

    // Deposit - show from first variant if available
    const depositEl = document.getElementById('pd-deposit');
    if (depositEl && product.variants && product.variants.length > 0) {
        const dep = product.variants[0].depositAmount || 0;
        depositEl.innerHTML = `Giá cọc: <span style="color:#333;font-weight:bold;">${money(dep)}</span>`;
    }

    // Render image gallery
    renderImageGallery(product);

    // Render colors dropdown
    renderColors(product.colors || []);

    // Render sizes dropdown (initially disabled — user must pick color first)
    renderSizes(product.sizes || []);
    const $sizeInit = $("#sizeSelect");
    if ($sizeInit.length && product.colors && product.colors.length > 0) {
        $sizeInit.prop("disabled", true);
        // Re-init Select2 disabled state
        if ($sizeInit.hasClass("select2-hidden-accessible")) {
            try { $sizeInit.select2("destroy"); } catch (e) { }
        }
        $sizeInit.select2({
            width: '100%',
            minimumResultsForSearch: Infinity,
            placeholder: "Vui lòng chọn màu trước",
            allowClear: false
        });
    }

    // Description
    const descEl = document.getElementById('pd-desc');
    if (descEl && product.description) {
        descEl.textContent = product.description;
    }
}

/**
 * Render colors dropdown (Handles Select2)
 */
function renderColors(colors) {
    const colorSelect = $("#colorSelect"); // Use jQuery
    const colorError = document.getElementById("colorError");

    if (!colorSelect.length) return;

    // Destroy existing Select2 instance if it exists to prevent conflicts
    if (colorSelect.hasClass("select2-hidden-accessible")) {
        try {
            colorSelect.select2("destroy");
        } catch (e) {
            console.warn("Could not destroy select2:", e);
        }
    }

    if (colors && colors.length > 0) {
        // Generate new options - First option empty for Select2 placeholder
        const optionsHtml = `<option value=""></option>` +
            colors.map(c => `<option value="${c}">${c}</option>`).join("");

        // Update HTML
        colorSelect.html(optionsHtml);
        colorSelect.prop("disabled", false);

        // Initialize Select2 with width 100% and hidden search
        colorSelect.select2({
            width: '100%',
            minimumResultsForSearch: Infinity,
            placeholder: "Chọn màu sắc",
            allowClear: false
        });

        // Bind change event
        colorSelect.off("change").on("change", function () {
            selectedColor = $(this).val();
            console.log("Color selected:", selectedColor);
            if (selectedColor) {
                if (colorError) colorError.style.display = "none";
                updateColorUI(selectedColor);
            }

            // Cross-filter: disable sizes that don't exist for this color
            if (currentProduct && currentProduct.variants) {
                if (selectedColor) {
                    const validSizes = [...new Set(
                        currentProduct.variants
                            .filter(v => v.colorName === selectedColor)
                            .map(v => v.sizeLabel)
                            .filter(Boolean)
                    )];
                    // Enable size dropdown and set valid/invalid states
                    $("#sizeSelect").prop("disabled", false);
                    disableInvalidSizes(validSizes);
                    // Re-init Select2 with enabled state
                    const $ss = $("#sizeSelect");
                    if ($ss.hasClass("select2-hidden-accessible")) {
                        try { $ss.select2("destroy"); } catch (e) { }
                    }
                    $ss.select2({
                        width: '100%',
                        minimumResultsForSearch: Infinity,
                        placeholder: "Chọn kích thước",
                        allowClear: false
                    });
                    // Rebind size change (since Select2 was re-inited)
                    $ss.off("change").on("change", sizeChangeHandler);
                    // Reset size if currently selected size is now invalid
                    if (selectedSize && !validSizes.includes(selectedSize)) {
                        selectedSize = "";
                        window._isFilteringSizes = true;
                        $ss.val("").trigger("change.select2");
                        window._isFilteringSizes = false;
                    }
                } else {
                    // Color cleared → disable size dropdown and reset
                    selectedSize = "";
                    const $ss = $("#sizeSelect");
                    $ss.val("");
                    $ss.prop("disabled", true);
                    enableAllSizes();
                    if ($ss.hasClass("select2-hidden-accessible")) {
                        try { $ss.select2("destroy"); } catch (e) { }
                    }
                    $ss.select2({
                        width: '100%',
                        minimumResultsForSearch: Infinity,
                        placeholder: "Vui lòng chọn màu trước",
                        allowClear: false
                    });
                }
            }
            updateVariantInfo();
        });

    } else {
        colorSelect.html(`<option value=""></option>`);
        colorSelect.prop("disabled", true);
        colorSelect.select2({
            width: '100%',
            minimumResultsForSearch: Infinity,
            placeholder: "Không có màu sắc"
        });
        selectedColor = "N/A";
    }
}

/**
 * Render sizes dropdown (Handles Select2)
 */
function renderSizes(sizes) {
    const sizeSelect = $("#sizeSelect"); // Use jQuery
    const sizeError = document.getElementById("sizeError");

    if (!sizeSelect.length) return;

    // Destroy existing Select2 instance
    if (sizeSelect.hasClass("select2-hidden-accessible")) {
        try {
            sizeSelect.select2("destroy");
        } catch (e) {
            console.warn("Could not destroy select2:", e);
        }
    }

    if (sizes && sizes.length > 0) {
        // Generate new options - First option empty for Select2 placeholder
        const optionsHtml = `<option value=""></option>` +
            sizes.map(s => `<option value="${s}">${s}</option>`).join("");

        sizeSelect.html(optionsHtml);
        sizeSelect.prop("disabled", false);

        // Initialize Select2
        sizeSelect.select2({
            width: '100%',
            minimumResultsForSearch: Infinity,
            placeholder: "Chọn kích thước",
            allowClear: false
        });

        // Bind change event
        sizeSelect.off("change").on("change", sizeChangeHandler);

    } else {
        sizeSelect.html(`<option value=""></option>`);
        sizeSelect.prop("disabled", true);
        sizeSelect.select2({
            width: '100%',
            minimumResultsForSearch: Infinity,
            placeholder: "Không có kích thước"
        });
        selectedSize = "N/A";
    }
}

/**
 * Size change handler (extracted so it can be rebound after Select2 re-init)
 */
function sizeChangeHandler() {
    if (window._isFilteringSizes) return;
    selectedSize = $(this).val();
    console.log("Size selected:", selectedSize);
    const sizeError = document.getElementById("sizeError");
    if (selectedSize) {
        if (sizeError) sizeError.style.display = "none";
        updateSizeUI(selectedSize);
    }
    // No color disabling — colors are always selectable to prevent deadlock
    updateVariantInfo();
}

/**
 * Disable size options that are not in the validSizes array
 */
function disableInvalidSizes(validSizes) {
    const sizeSelect = document.getElementById("sizeSelect");
    if (!sizeSelect) return;
    Array.from(sizeSelect.options).forEach(opt => {
        if (opt.value === "") return; // skip placeholder
        opt.disabled = !validSizes.includes(opt.value);
    });
}

/**
 * Enable all size options
 */
function enableAllSizes() {
    const sizeSelect = document.getElementById("sizeSelect");
    if (!sizeSelect) return;
    Array.from(sizeSelect.options).forEach(opt => {
        opt.disabled = false;
    });
}


/**
 * Setup date validation UI
 */
function setupDateValidation() {
    // Try to find the container. User might have deleted .row.ex-padd
    let container = document.querySelector(".row.ex-padd");

    if (!container) {
        // Fallback: Create a container after the Date Picker
        const picker = document.querySelector(".rq-picker");
        if (picker && picker.parentNode) {
            let msgWrapper = document.getElementById("date-validation-msg");
            if (!msgWrapper) {
                msgWrapper = document.createElement("div");
                msgWrapper.id = "date-validation-msg";
                // Style to look like a row/column
                msgWrapper.className = "row";
                msgWrapper.innerHTML = `<div class="col-sm-12" style="margin-top: 15px;"></div>`;
                picker.parentNode.insertBefore(msgWrapper, picker.nextSibling);
            }
            container = msgWrapper.querySelector(".col-sm-12");
        }
    } else {
        // Use the existing .col-sm-12 if .row.ex-padd exists
        // Be careful not to wipe it if it has other stuff, but here we replace content
        // However, user deleted the content inside .row.ex-padd before.
        // Let's ensure we are inside a col
        if (!container.querySelector(".col-sm-12")) {
            container.innerHTML = `<div class="col-sm-12"></div>`;
        }
        container = container.querySelector(".col-sm-12");
    }

    if (container) {
        container.innerHTML = `
        <div id="rentalDaysError" class="rq-error-message" style="display: none; padding: 12px; margin-bottom: 15px; border-radius: 4px; background: #f8d7da; color: #721c24; border: 1px solid #f5c6cb; font-size: 14px;">
          <i class="fa fa-exclamation-circle"></i> 
          <span>Số ngày thuê phải từ 3 ngày trở lên</span>
        </div>
        
        <div id="rentalDaysInfo" style="display: none; padding: 12px; margin-bottom: 15px; border-radius: 4px; background: #d4edda; color: #155724; border: 1px solid #c3e6cb; font-size: 14px;">
          <i class="fa fa-check-circle"></i> 
          Số ngày thuê: <strong id="rentalDaysCount">0</strong> ngày
        </div>
    `;
    }

    initDatePickerObserver();
    setupReserveButton();
}

/**
 * Initialize observer for React date picker hidden inputs
 */
function initDatePickerObserver() {
    const houstonComponent = document.getElementById("houston-component");
    if (!houstonComponent) return;

    const observer = new MutationObserver((mutations) => {
        checkDateInputs();
    });

    observer.observe(houstonComponent, {
        childList: true,
        subtree: true,
        attributes: true,
        attributeFilter: ["value"]
    });

    setInterval(checkDateInputs, 500);
}

/**
 * Check the hidden date inputs and validate
 */
function checkDateInputs() {
    const startInput = document.querySelector('input[name="start_date"]');
    const endInput = document.querySelector('input[name="end_date"]');

    if (!startInput || !endInput) return;

    const startVal = startInput.value;
    const endVal = endInput.value;

    // If start_date is set but end_date is empty, auto-set end = start (1-day rental)
    if (startVal && startVal !== "" && (!endVal || endVal === "")) {
        const start = parseDate(startVal);
        if (start) {
            endInput.value = startVal; // sync the hidden input
            if (!startDate || !endDate ||
                start.getTime() !== startDate.getTime() ||
                start.getTime() !== endDate.getTime()) {
                validateRentalDays(start, new Date(start.getTime()));
            }
        }
        return;
    }

    if (startVal && endVal && startVal !== "" && endVal !== "") {
        const start = parseDate(startVal);
        const end = parseDate(endVal);

        if (start && end) {
            if (!startDate || !endDate ||
                start.getTime() !== startDate.getTime() ||
                end.getTime() !== endDate.getTime()) {
                validateRentalDays(start, end);
            }
        }
    }
}

/**
 * Parse date
 */
function parseDate(dateStr) {
    if (!dateStr) return null;

    if (/^\d{4}-\d{2}-\d{2}/.test(dateStr)) {
        const parts = dateStr.split("-");
        const date = new Date(parts[0], parts[1] - 1, parts[2]);
        date.setHours(0, 0, 0, 0);
        return date;
    }

    if (/^\d{2}\/\d{2}\/\d{4}/.test(dateStr)) {
        const parts = dateStr.split("/");
        const date = new Date(parts[2], parts[1] - 1, parts[0]);
        date.setHours(0, 0, 0, 0);
        return date;
    }

    const date = new Date(dateStr);
    if (!isNaN(date.getTime())) {
        date.setHours(0, 0, 0, 0);
        return date;
    }

    return null;
}

/**
 * Validate rental days (Realtime)
 * Allow >= 1 day
 */
function validateRentalDays(start, end) {
    start.setHours(0, 0, 0, 0);
    end.setHours(0, 0, 0, 0);

    startDate = start;
    endDate = end;

    const diffTime = Math.abs(end.getTime() - start.getTime());
    rentalDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24)) + 1;

    console.log(`Rental days: ${rentalDays}`);

    const errorDiv = document.getElementById("rentalDaysError");
    const infoDiv = document.getElementById("rentalDaysInfo");
    const daysCount = document.getElementById("rentalDaysCount");

    // Always valid as long as >= 1 day
    if (errorDiv) errorDiv.style.display = "none";
    if (infoDiv) {
        infoDiv.style.display = "block";
        if (daysCount) daysCount.textContent = rentalDays;
    }
}

/**
 * Validate all fields when user clicks Submit
 */
function validateAllFieldsOnSubmit() {
    let firstErrorEl = null;

    // 1. Validate Date
    const dateErrorDiv = document.getElementById("rentalDaysError");
    if (!startDate || !endDate) {
        // If user deleted the container, we might not find dateErrorDiv.
        // We should try to find it or create alert fallback if critical? 
        // But setupDateValidation should handled creation.
        if (dateErrorDiv) {
            dateErrorDiv.innerHTML = `<i class="fa fa-exclamation-circle"></i> Vui lòng chọn ngày thuê`;
            dateErrorDiv.style.display = "block";
            firstErrorEl = dateErrorDiv;
        }
    }

    // 2. Validate Color
    if (currentProduct && currentProduct.colors && currentProduct.colors.length > 0) {
        if (!selectedColor || selectedColor === "N/A" || selectedColor === "") {
            const colorError = document.getElementById("colorError");
            if (colorError) {
                colorError.style.display = "block";
                if (!firstErrorEl) firstErrorEl = colorError;
            }
        }
    }

    // 3. Validate Size
    if (currentProduct && currentProduct.sizes && currentProduct.sizes.length > 0) {
        if (!selectedSize || selectedSize === "N/A" || selectedSize === "") {
            const sizeError = document.getElementById("sizeError");
            if (sizeError) {
                sizeError.style.display = "block";
                if (!firstErrorEl) firstErrorEl = sizeError;
            }
        }
    }

    if (firstErrorEl) {
        firstErrorEl.scrollIntoView({ behavior: "smooth", block: "center" });
        return false;
    }

    return true;
}

/**
 * Setup reserve button
 */
function setupReserveButton() {
    const reserveBtn = document.querySelector(".reserve");
    if (!reserveBtn) return;

    reserveBtn.id = "reserveBtn";
    reserveBtn.classList.remove("disabled");
    reserveBtn.style.pointerEvents = "auto";
    reserveBtn.style.opacity = "1";
    reserveBtn.style.cursor = "pointer";

    reserveBtn.addEventListener("click", (e) => {
        e.preventDefault();

        const isValid = validateAllFieldsOnSubmit();

        if (isValid) {
            // Get variant info
            const variant = findVariant(selectedColor, selectedSize);
            const pricePerDay = variant?.pricePerDay || currentProduct.minPricePerDay || currentProduct.pricePerDay || 0;
            const depositAmount = variant?.depositAmount || 0;
            const qtyInput = document.getElementById('quantityInput');
            const quantity = qtyInput ? parseInt(qtyInput.value) || 1 : 1;

            // Check stock
            if (variant && (variant.quantity || 0) < quantity) {
                const errDiv = document.getElementById('quantityError');
                if (errDiv) {
                    errDiv.innerHTML = `<i class="fa fa-exclamation-circle"></i> Chỉ còn ${variant.quantity || 0} sản phẩm`;
                    errDiv.style.display = 'block';
                }
                return;
            }

            const bookingInfo = {
                productId: currentProduct.id,
                variantId: variant?.id || null,
                providerId: currentProduct.providerId || currentProduct.ProviderId || null,
                productName: currentProduct.name,
                color: selectedColor,
                size: selectedSize,
                startDate: startDate.toISOString(),
                endDate: endDate.toISOString(),
                days: rentalDays,
                quantity: quantity,
                pricePerDay: pricePerDay,
                depositAmount: depositAmount,
                totalPrice: pricePerDay * rentalDays * quantity,
                thumbnailUrl: currentProduct.thumbnailUrl || (currentProduct.imageUrls && currentProduct.imageUrls[0]) || '',
                // Include arrays for cart edit modal dropdown options
                colors: currentProduct.colors || [],
                sizes: currentProduct.sizes || [],
            };

            // Check stock including what's already in cart
            function readCartForBooking() {
                const uid = localStorage.getItem('userId');
                const keys = uid ? [`cartItems:user:${uid}`, 'cartItems:anon', 'cartItems'] : ['cartItems:anon', 'cartItems'];
                for (const k of keys) {
                    const raw = localStorage.getItem(k);
                    if (!raw) continue;
                    try { const arr = JSON.parse(raw); if (Array.isArray(arr)) return arr; } catch (e) { }
                }
                return [];
            }

            const existingCart = readCartForBooking();
            const existingItem = existingCart.find(ci =>
                ci.productId === bookingInfo.productId &&
                ci.color === bookingInfo.color &&
                ci.size === bookingInfo.size
            );
            const alreadyInCart = existingItem ? (existingItem.quantity || 1) : 0;
            const maxStock = variant ? (variant.quantity || 0) : 999;
            const totalQty = alreadyInCart + quantity;

            if (totalQty > maxStock) {
                const errDiv = document.getElementById('quantityError');
                if (errDiv) {
                    errDiv.innerHTML = `<i class="fa fa-exclamation-circle"></i> Vượt quá số lượng tồn kho! Kho còn ${maxStock}, trong giỏ đã có ${alreadyInCart}.`;
                    errDiv.style.display = 'block';
                }
                return;
            }

            localStorage.setItem("currentBooking", JSON.stringify(bookingInfo));

            // Merge into cartItems: if same product+color+size exists, add quantity
            if (existingItem) {
                existingItem.quantity = (existingItem.quantity || 1) + quantity;
                existingItem.totalPrice = existingItem.pricePerDay * existingItem.days * existingItem.quantity;
            } else {
                existingCart.push(bookingInfo);
            }
            // Save to user-scoped key when possible
            const uid = localStorage.getItem('userId');
            const key = uid ? `cartItems:user:${uid}` : 'cartItems:anon';
            localStorage.setItem(key, JSON.stringify(existingCart));
            if (!uid) localStorage.setItem('cartItems', JSON.stringify(existingCart));

            window.location.href = `cart.html`;
        }
    });
}

/**
 * Show a toast notification on the booking page
 */
function showBookingToast(msg, type, ms) {
    let toast = document.getElementById('booking-toast');
    if (!toast) {
        toast = document.createElement('div');
        toast.id = 'booking-toast';
        toast.style.cssText = 'position:fixed;top:20px;right:20px;z-index:99999;padding:14px 24px;border-radius:8px;font-size:14px;color:#fff;box-shadow:0 4px 12px rgba(0,0,0,0.2);transition:opacity 0.3s;';
        document.body.appendChild(toast);
    }
    toast.style.background = type === 'success' ? '#4CAF50' : '#f44336';
    toast.textContent = msg;
    toast.style.opacity = '1';
    toast.style.display = 'block';
    setTimeout(() => { toast.style.opacity = '0'; setTimeout(() => { toast.style.display = 'none'; }, 300); }, ms || 3000);
}

/**
 * Setup "Thêm vào giỏ" button — adds to cart WITHOUT redirect
 */
function setupAddToCartButton() {
    const btn = document.getElementById('addToCartBtn');
    if (!btn) return;

    btn.addEventListener('click', (e) => {
        e.preventDefault();

        // CHECK LOGIN FIRST
        const token = localStorage.getItem("token");
        if (!token) {
            showBookingToast("Vui lòng đăng nhập hoặc đăng ký để thêm sản phẩm vào giỏ hàng.", "error", 3000);
            setTimeout(() => {
                window.location.href = "login.html";
            }, 1500);
            return;
        }

        const isValid = validateAllFieldsOnSubmit();
        if (!isValid) return;

        const variant = findVariant(selectedColor, selectedSize);
        const pricePerDay = variant?.pricePerDay || currentProduct.minPricePerDay || currentProduct.pricePerDay || 0;
        const depositAmount = variant?.depositAmount || 0;
        const qtyInput = document.getElementById('quantityInput');
        const quantity = qtyInput ? parseInt(qtyInput.value) || 1 : 1;

        // Check stock
        if (variant && (variant.quantity || 0) < quantity) {
            const errDiv = document.getElementById('quantityError');
            if (errDiv) {
                errDiv.innerHTML = `<i class="fa fa-exclamation-circle"></i> Chỉ còn ${variant.quantity || 0} sản phẩm`;
                errDiv.style.display = 'block';
            }
            return;
        }

        const bookingInfo = {
            productId: currentProduct.id,
            variantId: variant?.id || null,
            providerId: currentProduct.providerId || currentProduct.ProviderId || null,
            productName: currentProduct.name,
            color: selectedColor,
            size: selectedSize,
            startDate: startDate.toISOString(),
            endDate: endDate.toISOString(),
            days: rentalDays,
            quantity: quantity,
            pricePerDay: pricePerDay,
            depositAmount: depositAmount,
            totalPrice: pricePerDay * rentalDays * quantity,
            thumbnailUrl: currentProduct.thumbnailUrl || (currentProduct.imageUrls && currentProduct.imageUrls[0]) || '',
            colors: currentProduct.colors || [],
            sizes: currentProduct.sizes || [],
        };

        // Check stock including cart
        const existingCart = (function () {
            const uid = localStorage.getItem('userId');
            const keys = uid ? [`cartItems:user:${uid}`, 'cartItems:anon', 'cartItems'] : ['cartItems:anon', 'cartItems'];
            for (const k of keys) {
                const raw = localStorage.getItem(k);
                if (!raw) continue;
                try { const arr = JSON.parse(raw); if (Array.isArray(arr)) return arr; } catch (e) { }
            }
            return [];
        })();
        const existingItem = existingCart.find(ci =>
            ci.productId === bookingInfo.productId &&
            ci.color === bookingInfo.color &&
            ci.size === bookingInfo.size
        );
        const alreadyInCart = existingItem ? (existingItem.quantity || 1) : 0;
        const maxStock = variant ? (variant.quantity || 0) : 999;

        if (alreadyInCart + quantity > maxStock) {
            showBookingToast(`Vượt quá tồn kho! Kho còn ${maxStock}, trong giỏ đã có ${alreadyInCart}.`, 'error', 4000);
            return;
        }

        // Merge or push
        if (existingItem) {
            existingItem.quantity = (existingItem.quantity || 1) + quantity;
            existingItem.totalPrice = existingItem.pricePerDay * existingItem.days * existingItem.quantity;
        } else {
            existingCart.push(bookingInfo);
        }
        const uid2 = localStorage.getItem('userId');
        const key2 = uid2 ? `cartItems:user:${uid2}` : 'cartItems:anon';
        localStorage.setItem(key2, JSON.stringify(existingCart));
        if (!uid2) localStorage.setItem('cartItems', JSON.stringify(existingCart));

        showBookingToast(`✓ Đã thêm "${currentProduct.name}" vào giỏ hàng!`, 'success', 3000);

        // Sync to server if logged in
        if (token && bookingInfo.variantId) {
            var startDateOnly = bookingInfo.startDate ? bookingInfo.startDate.split('T')[0] : null;
            var endDateOnly = bookingInfo.endDate ? bookingInfo.endDate.split('T')[0] : null;
            fetch(API_BASE + '/api/Carts/me/items', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': 'Bearer ' + token
                },
                body: JSON.stringify({
                    productVariantId: bookingInfo.variantId,
                    quantity: quantity,
                    startDate: startDateOnly,
                    endDate: endDateOnly
                })
            }).then(function (res) {
                if (res.ok) console.log('[CART] Synced to server');
                else console.warn('[CART] Server sync failed:', res.status);
            }).catch(function (err) {
                console.warn('[CART] Server sync error:', err);
            });
        }

        // Refresh header cart dropdown in real-time
        if (typeof window.renderHeaderCart === 'function') {
            window.renderHeaderCart();
        }
    });
}

/**
 * Fetch product details from API
 */
async function loadProduct(id) {
    try {
        let res = await fetch(`${API_BASE}/api/ProductsCustomer/${id}`);

        if (res.ok) {
            const data = await res.json();
            const product = data.data || data;
            if (product && product.id) {
                // Normalize data from Detail API (ProductDetailDto has variants list, not flat colors/sizes)
                if (product.variants && Array.isArray(product.variants)) {
                    // Extract unique colors
                    const colors = [...new Set(product.variants.map(v => v.colorName).filter(c => c))];
                    product.colors = colors;

                    // Extract unique sizes
                    const sizes = [...new Set(product.variants.map(v => v.sizeLabel).filter(s => s))];
                    product.sizes = sizes;
                }

                // Normalize image (Detail API has imageUrls list)
                if (!product.thumbnailUrl && product.imageUrls && product.imageUrls.length > 0) {
                    product.thumbnailUrl = product.imageUrls[0];
                }

                renderProduct(product);
                return;
            }
        }

        res = await fetch(`${API_BASE}/api/ProductsCustomer/listing?status=AVAILABLE&pageSize=100`);
        if (res.ok) {
            const data = await res.json();
            const items = data.items || [];
            const product = items.find(p => String(p.id) === String(id));

            if (product) {
                renderProduct(product);
                return;
            }
        }

        showError("Không tìm thấy sản phẩm");

    } catch (error) {
        console.error("Load product failed:", error);
        showError("Không tải được thông tin sản phẩm. Vui lòng thử lại sau.");
    }
}

/**
 * Initialize booking page
 */
function init() {
    const productId = getProductId();
    if (!productId) {
        showError("Thiếu id sản phẩm");
        return;
    }
    setupDateValidation(); // This already calls setupReserveButton() internally
    setupAddToCartButton();
    loadProduct(productId);
}

/**
 * Force update Select2 Color UI
 */
function updateColorUI(text) {
    // Find the Select2 Rendered element (next sibling of the select box)
    const rendered = $("#colorSelect").next(".select2-container").find(".select2-selection__rendered");
    if (rendered.length) {
        rendered.text(text);
        rendered.css({
            "color": "#d0021b",
            "font-weight": "bold",
            "opacity": "1" // Ensure it's not faded
        });
        rendered.removeClass("select2-selection__placeholder"); // Remove placeholder gray style
    }
}

/**
 * Force update Select2 Size UI
 */
function updateSizeUI(text) {
    const rendered = $("#sizeSelect").next(".select2-container").find(".select2-selection__rendered");
    if (rendered.length) {
        rendered.text(text);
        rendered.css({
            "color": "#333",
            "font-weight": "bold",
            "opacity": "1"
        });
        rendered.removeClass("select2-selection__placeholder");
    }
}

/**
 * Quantity input helpers
 */
function changeQty(delta) {
    const input = document.getElementById('quantityInput');
    if (!input) return;
    let val = parseInt(input.value) || 1;
    val = Math.max(1, val + delta);
    input.value = val;
    validateQty();
}

function validateQty() {
    const input = document.getElementById('quantityInput');
    const errDiv = document.getElementById('quantityError');
    if (!input) return;
    let val = parseInt(input.value) || 1;
    if (val < 1) { val = 1; input.value = 1; }

    const variant = findVariant(selectedColor, selectedSize);
    if (variant && variant.quantity != null && val > variant.quantity) {
        if (errDiv) {
            errDiv.innerHTML = `<i class="fa fa-exclamation-circle"></i> Chỉ còn ${variant.quantity} sản phẩm`;
            errDiv.style.display = 'block';
        }
    } else {
        if (errDiv) errDiv.style.display = 'none';
    }
}

document.addEventListener("DOMContentLoaded", init);
// ==========================================
// PRODUCT REVIEWS LOGIC
// ==========================================

var currentReviewsPage = 1;
var reviewsPageSize = 5;
var eligibleBookingItemId = null; // Set by eligibility API

/**
 * Initialize Review System
 */
document.addEventListener("DOMContentLoaded", function () {
    setTimeout(initReviews, 300);
});

function initReviews() {
    setupReviewStars();
    setupImagePreview();
    setupReviewFormSubmit();
    loadReviews(1);
    checkReviewEligibility();

    var loadMoreBtn = document.getElementById('btn-load-more-reviews');
    if (loadMoreBtn) {
        loadMoreBtn.addEventListener('click', function () {
            currentReviewsPage++;
            loadReviews(currentReviewsPage, true);
        });
    }
}

/**
 * Check if user is eligible to review this product
 * Always show the button; show toast if not eligible
 */
function checkReviewEligibility() {
    var token = localStorage.getItem('token');
    var reviewBtn = document.getElementById('btn-open-review-modal');
    if (!reviewBtn) return;

    // Always show the button
    reviewBtn.style.display = '';

    // If not logged in: show button but prompt login on click
    if (!token) {
        eligibleBookingItemId = null;
        reviewBtn.removeAttribute('data-toggle');
        reviewBtn.removeAttribute('data-target');
        reviewBtn.addEventListener('click', function (e) {
            e.preventDefault();
            showBookingToast('Vui lòng đăng nhập để viết đánh giá.', 'error', 3000);
            setTimeout(function () { window.location.href = 'login.html'; }, 1500);
        });
        return;
    }

    var productId = getProductId();
    if (!productId) return;

    fetch(API_BASE + '/api/ProductReviews/eligibility?productId=' + productId, {
        headers: { 'Authorization': 'Bearer ' + token }
    })
        .then(function (res) {
            if (!res.ok) throw new Error("Eligibility check failed");
            return res.json();
        })
        .then(function (data) {
            if (data.canReview && data.bookingItemId) {
                eligibleBookingItemId = data.bookingItemId;
                reviewBtn.disabled = false;
                console.log('[REVIEW] Eligible, bookingItemId:', eligibleBookingItemId);
            } else {
                eligibleBookingItemId = null;
                reviewBtn.removeAttribute('data-toggle');
                reviewBtn.removeAttribute('data-target');
                reviewBtn.addEventListener('click', function (e) {
                    e.preventDefault();
                    showBookingToast(data.message || 'Hãy đặt hàng để viết đánh giá', 'error', 4000);
                });
                console.log('[REVIEW] Not eligible:', data.message);
            }
        })
        .catch(function (err) {
            console.error('[REVIEW] Eligibility check error:', err);
        });
}

/**
 * Setup interactive star rating (click + hover)
 */
function setupReviewStars() {
    var container = document.getElementById('review-stars-input');
    if (!container) return;

    var ratingInput = document.getElementById('review-rating');
    var errorMsg = document.getElementById('ratingError');

    container.style.position = 'relative';
    container.style.zIndex = '10';
    container.style.cursor = 'pointer';

    container.addEventListener('click', function (e) {
        var star = e.target.closest('i[data-value]');
        if (!star) return;
        e.preventDefault();
        e.stopPropagation();
        var val = parseInt(star.getAttribute('data-value'));
        if (ratingInput) ratingInput.value = val;
        highlightStars(val);
        if (errorMsg) errorMsg.style.display = 'none';
    });

    container.addEventListener('mouseover', function (e) {
        var star = e.target.closest('i[data-value]');
        if (!star) return;
        highlightStars(parseInt(star.getAttribute('data-value')));
    });

    container.addEventListener('mouseleave', function () {
        var currentVal = ratingInput ? parseInt(ratingInput.value) : 0;
        highlightStars(currentVal);
    });
}

function highlightStars(val) {
    var stars = document.querySelectorAll('#review-stars-input i[data-value]');
    stars.forEach(function (s) {
        var starVal = parseInt(s.getAttribute('data-value'));
        if (starVal <= val) {
            s.className = 'fa fa-star';
            s.style.color = '#f39c12';
        } else {
            s.className = 'fa fa-star-o';
            s.style.color = '#f39c12';
        }
    });
}

/**
 * Setup image file input preview with consistent sizing
 */
function setupImagePreview() {
    var imageInput = document.getElementById('review-images');
    var previewContainer = document.getElementById('review-image-preview');
    if (!imageInput || !previewContainer) return;

    imageInput.addEventListener('change', function () {
        previewContainer.innerHTML = '';
        if (!this.files || this.files.length === 0) return;
        Array.from(this.files).forEach(function (file) {
            if (!file.type.startsWith('image/')) return;
            var reader = new FileReader();
            reader.onload = function (e) {
                var wrapper = document.createElement('div');
                wrapper.style.cssText = 'width:80px; height:80px; border-radius:6px; overflow:hidden; border:2px solid #ddd; flex-shrink:0;';
                var img = document.createElement('img');
                img.src = e.target.result;
                img.style.cssText = 'width:100%; height:100%; object-fit:cover; display:block;';
                img.alt = 'Preview';
                wrapper.appendChild(img);
                previewContainer.appendChild(wrapper);
            };
            reader.readAsDataURL(file);
        });
    });

    if (typeof $ !== 'undefined') {
        $('#rq_modal1').on('show.bs.modal', function (e) {
            var token = localStorage.getItem('token');
            if (!token) {
                e.preventDefault();
                showBookingToast("Vui lòng đăng nhập để viết đánh giá.", "error", 3000);
                setTimeout(function () { window.location.href = "login.html"; }, 1500);
                return;
            }
            if (!eligibleBookingItemId) {
                e.preventDefault();
                showBookingToast("Bạn chỉ có thể đánh giá sản phẩm sau khi đã mua.", "error", 4000);
                return;
            }
        });

        $('#rq_modal1').on('hidden.bs.modal', function () {
            var form = document.getElementById('review-form');
            if (form) form.reset();
            var ratingInput = document.getElementById('review-rating');
            if (ratingInput) ratingInput.value = '0';
            highlightStars(0);
            if (previewContainer) previewContainer.innerHTML = '';
            var errorMsg = document.getElementById('ratingError');
            if (errorMsg) errorMsg.style.display = 'none';
        });
    }
}

/**
 * Handle Review Form Submission — BookingItemId is NOT sent, server resolves it
 */
function setupReviewFormSubmit() {
    var form = document.getElementById('review-form');
    if (!form) return;

    form.addEventListener('submit', function (e) {
        e.preventDefault();

        var rating = parseInt(document.getElementById('review-rating').value);
        if (!rating || rating === 0) {
            document.getElementById('ratingError').style.display = 'block';
            return;
        }

        if (!eligibleBookingItemId) {
            showBookingToast('Bạn không đủ điều kiện đánh giá sản phẩm này.', 'error', 4000);
            return;
        }

        var comment = document.getElementById('review-comment').value;
        var imagesBox = document.getElementById('review-images');
        var productId = getProductId();

        var formData = new FormData();
        formData.append('ProductId', productId);
        formData.append('Rating', rating);
        if (comment) formData.append('Comment', comment);

        if (imagesBox && imagesBox.files && imagesBox.files.length > 0) {
            Array.from(imagesBox.files).forEach(function (file) {
                formData.append('Images', file);
            });
        }

        var btn = document.getElementById('submit-review-btn');
        var originalText = btn.innerHTML;
        btn.innerHTML = '<i class="fa fa-spinner fa-spin"></i> Đang gửi...';
        btn.disabled = true;

        var token = localStorage.getItem('token');

        fetch(API_BASE + '/api/ProductReviews', {
            method: 'POST',
            headers: { 'Authorization': 'Bearer ' + token },
            body: formData
        })
            .then(function (res) {
                if (res.ok) {
                    return res.json().then(function () {
                        showBookingToast("Cảm ơn bạn đã đánh giá sản phẩm!", "success");
                        if (typeof $ !== 'undefined') $('#rq_modal1').modal('hide');
                        eligibleBookingItemId = null;
                        var reviewBtn = document.getElementById('btn-open-review-modal');
                        if (reviewBtn) {
                            reviewBtn.disabled = true;
                            reviewBtn.style.opacity = '0.5';
                            reviewBtn.style.cursor = 'not-allowed';
                            reviewBtn.removeAttribute('data-toggle');
                            reviewBtn.removeAttribute('data-target');
                            reviewBtn.title = 'Bạn đã đánh giá sản phẩm này rồi.';
                        }
                        currentReviewsPage = 1;
                        loadReviews(1);
                    });
                } else {
                    return res.json().then(function (err) {
                        showBookingToast(err.message || "Lỗi khi gửi đánh giá.", "error", 4000);
                    });
                }
            })
            .catch(function (error) {
                console.error("Submit review error:", error);
                showBookingToast("Không thể kết nối đến máy chủ.", "error", 4000);
            })
            .finally(function () {
                btn.innerHTML = originalText;
                btn.disabled = false;
            });
    });
}

/**
 * Fetch and Render Reviews from API
 */
function loadReviews(page, append) {
    page = page || 1;
    append = append || false;

    var productId = getProductId();
    if (!productId) return;

    var container = document.getElementById('reviews-list-container');
    var loadMoreDiv = document.getElementById('reviews-pagination');

    if (!append && container) {
        container.innerHTML = '<div style="text-align:center; padding:40px; color:#666;"><i class="fa fa-spinner fa-spin fa-2x"></i><p style="margin-top:10px;">Đang tải đánh giá...</p></div>';
    }

    fetch(API_BASE + '/api/ProductReviews?productId=' + productId + '&page=' + page + '&pageSize=' + reviewsPageSize)
        .then(function (res) {
            if (!res.ok) throw new Error("Failed to fetch reviews");
            return res.json();
        })
        .then(function (data) {
            if (!append) {
                updateReviewHeaderStats(data.totalItems, data.items);
            }

            if (!data.items || data.totalItems === 0) {
                container.innerHTML = '<div style="text-align:center; padding:30px; color:#888; border:1px dashed #ddd; border-radius:8px; margin-top:20px;">Chưa có đánh giá nào cho sản phẩm này. Hãy là người đầu tiên đánh giá!</div>';
                if (loadMoreDiv) loadMoreDiv.style.display = 'none';
                return;
            }

            var html = '';
            data.items.forEach(function (review) {
                html += generateReviewHtml(review);
            });

            if (append) {
                container.innerHTML += html;
            } else {
                container.innerHTML = html;
            }

            if (loadMoreDiv) {
                if (data.page * data.pageSize < data.totalItems) {
                    loadMoreDiv.style.display = 'block';
                } else {
                    loadMoreDiv.style.display = 'none';
                }
            }

            // Load customer photos on first page
            if (!append) {
                loadCustomerReviewPhotos();
            }
        })
        .catch(function (error) {
            console.error("Load reviews error:", error);
            if (!append && container) {
                container.innerHTML = '<div style="text-align:center; padding:20px; color:red;">Không thể tải đánh giá. Vui lòng tải lại trang.</div>';
            }
        });
}

/**
 * Load all review images and display in "Ảnh từ khách hàng" section
 * with uniform size and prev/next navigation arrows
 */
var customerPhotoIndex = 0;
var customerPhotoPerPage = 6;
var customerPhotoList = [];

function loadCustomerReviewPhotos() {
    var productId = getProductId();
    if (!productId) return;

    var container = document.getElementById('customer-review-photos');
    if (!container) return;

    fetch(API_BASE + '/api/ProductReviews?productId=' + productId + '&page=1&pageSize=100')
        .then(function (res) {
            if (!res.ok) throw new Error("Failed to fetch reviews for photos");
            return res.json();
        })
        .then(function (data) {
            if (!data.items || data.items.length === 0) return;

            customerPhotoList = [];
            data.items.forEach(function (review) {
                if (review.imageUrls && review.imageUrls.length > 0) {
                    review.imageUrls.forEach(function (url) {
                        var fullUrl = url.startsWith('http') ? url : (API_BASE + url);
                        customerPhotoList.push(fullUrl);
                    });
                }
            });

            if (customerPhotoList.length === 0) return;

            customerPhotoIndex = 0;
            renderCustomerPhotos(container);
        })
        .catch(function (err) {
            console.error('[REVIEW] Load customer photos error:', err);
        });
}

function renderCustomerPhotos(container) {
    container.innerHTML = '';

    // Wrapper with overflow hidden
    var outerWrap = document.createElement('div');
    outerWrap.style.cssText = 'position:relative; width:100%;';

    // Left arrow
    var btnLeft = document.createElement('button');
    btnLeft.innerHTML = '<i class="fa fa-chevron-left"></i>';
    btnLeft.style.cssText = 'position:absolute; left:-15px; top:50%; transform:translateY(-50%); z-index:2; width:32px; height:32px; border-radius:50%; border:1px solid #ddd; background:#fff; cursor:pointer; display:flex; align-items:center; justify-content:center; box-shadow:0 1px 4px rgba(0,0,0,0.15); font-size:14px; color:#333;';
    btnLeft.onclick = function () {
        customerPhotoIndex = Math.max(0, customerPhotoIndex - customerPhotoPerPage);
        renderCustomerPhotos(container);
    };

    // Right arrow
    var btnRight = document.createElement('button');
    btnRight.innerHTML = '<i class="fa fa-chevron-right"></i>';
    btnRight.style.cssText = 'position:absolute; right:-15px; top:50%; transform:translateY(-50%); z-index:2; width:32px; height:32px; border-radius:50%; border:1px solid #ddd; background:#fff; cursor:pointer; display:flex; align-items:center; justify-content:center; box-shadow:0 1px 4px rgba(0,0,0,0.15); font-size:14px; color:#333;';
    btnRight.onclick = function () {
        customerPhotoIndex = Math.min(customerPhotoList.length - customerPhotoPerPage, customerPhotoIndex + customerPhotoPerPage);
        if (customerPhotoIndex < 0) customerPhotoIndex = 0;
        renderCustomerPhotos(container);
    };

    // Hide arrows when not needed
    if (customerPhotoIndex <= 0) btnLeft.style.display = 'none';
    if (customerPhotoIndex + customerPhotoPerPage >= customerPhotoList.length) btnRight.style.display = 'none';

    // Images row
    var imagesRow = document.createElement('div');
    imagesRow.style.cssText = 'display:flex; gap:10px; overflow:hidden; padding:0 5px;';

    var visibleImages = customerPhotoList.slice(customerPhotoIndex, customerPhotoIndex + customerPhotoPerPage);
    visibleImages.forEach(function (imgUrl) {
        var wrapper = document.createElement('div');
        wrapper.style.cssText = 'width:100px; height:100px; border-radius:8px; overflow:hidden; border:1px solid #eee; flex-shrink:0; cursor:pointer; transition:transform 0.2s;';
        wrapper.onmouseenter = function () { this.style.transform = 'scale(1.05)'; };
        wrapper.onmouseleave = function () { this.style.transform = 'scale(1)'; };
        wrapper.onclick = function () { window.open(imgUrl, '_blank'); };

        var img = document.createElement('img');
        img.src = imgUrl;
        img.alt = 'Ảnh khách hàng';
        img.style.cssText = 'width:100%; height:100%; object-fit:cover; display:block;';

        wrapper.appendChild(img);
        imagesRow.appendChild(wrapper);
    });

    outerWrap.appendChild(btnLeft);
    outerWrap.appendChild(imagesRow);
    outerWrap.appendChild(btnRight);
    container.appendChild(outerWrap);

    // Counter
    var counter = document.createElement('div');
    counter.style.cssText = 'text-align:center; margin-top:8px; font-size:13px; color:#999;';
    var from = customerPhotoIndex + 1;
    var to = Math.min(customerPhotoIndex + customerPhotoPerPage, customerPhotoList.length);
    counter.innerText = from + '-' + to + ' / ' + customerPhotoList.length + ' ảnh';
    container.appendChild(counter);
}

function updateReviewHeaderStats(totalItems, items) {
    var countEl = document.getElementById('total-reviews-count');
    if (countEl) countEl.innerText = '(' + totalItems + ' đánh giá)';

    var avg = 0;
    if (items && items.length > 0) {
        var sum = 0;
        items.forEach(function (r) { sum += r.rating; });
        avg = Math.round((sum / items.length) * 10) / 10;
    }

    var summaryCount = document.querySelector('.review-top span');
    var summaryAvg = document.querySelector('.review-top p');
    var summaryStars = document.querySelectorAll('.review-top > a > i');
    var overallStars = document.querySelectorAll('#overall-star-rating > a > i');

    if (summaryCount) summaryCount.innerText = totalItems;
    if (summaryAvg) summaryAvg.innerText = avg + ' trên 5 sao';

    var fullStarsCount = Math.floor(avg);
    var hasHalfStar = (avg - fullStarsCount) >= 0.5;

    [summaryStars, overallStars].forEach(function (starsNodeList) {
        if (starsNodeList && starsNodeList.length === 5) {
            starsNodeList.forEach(function (star, index) {
                if (index < fullStarsCount) {
                    star.className = 'fa fa-star';
                } else if (index === fullStarsCount && hasHalfStar) {
                    star.className = 'fa fa-star-half-o';
                } else {
                    star.className = 'fa fa-star-o';
                }
            });
        }
    });
}

function generateReviewHtml(review) {
    var dateObj = new Date(review.createdAt);
    var dateStr = dateObj.toLocaleDateString('vi-VN', { year: 'numeric', month: 'long', day: 'numeric' });

    var starsHtml = '';
    for (var i = 1; i <= 5; i++) {
        if (i <= review.rating) {
            starsHtml += '<i class="fa fa-star" style="color:#f39c12; font-size:16px; margin-right:2px;"></i>';
        } else {
            starsHtml += '<i class="fa fa-star-o" style="color:#f39c12; font-size:16px; margin-right:2px;"></i>';
        }
    }

    var imagesHtml = '';
    if (review.imageUrls && review.imageUrls.length > 0) {
        imagesHtml = '<div style="display:flex; gap:8px; margin-top:12px; flex-wrap:wrap;">';
        review.imageUrls.forEach(function (url) {
            var fullUrl = url.startsWith('http') ? url : (API_BASE + url);
            imagesHtml += '<div style="width:80px; height:80px; border-radius:6px; overflow:hidden; border:1px solid #eee; flex-shrink:0; cursor:pointer;" onclick="window.open(\'' + fullUrl + '\', \'_blank\')">'
                + '<img src="' + fullUrl + '" alt="Review" style="width:100%; height:100%; object-fit:cover; display:block;">'
                + '</div>';
        });
        imagesHtml += '</div>';
    }

    var firstLetter = review.customerName ? review.customerName.charAt(0).toUpperCase() : 'K';

    return '<div style="background:#fff; padding:25px 30px; margin-bottom:20px; border-radius:8px; box-shadow:0 1px 3px rgba(0,0,0,0.08);">'
        + '  <div style="display:flex; align-items:center; margin-bottom:12px;">'
        + '    <div style="width:42px; height:42px; border-radius:50%; background:#d0021b; color:#fff; display:flex; align-items:center; justify-content:center; font-size:18px; font-weight:bold; margin-right:12px; flex-shrink:0;">' + firstLetter + '</div>'
        + '    <div>'
        + '      <strong style="font-size:16px;">' + (review.customerName || 'Khách hàng') + '</strong>'
        + '      <div style="margin-top:2px;">' + starsHtml + ' <span style="color:#999; font-size:13px; margin-left:10px;">' + dateStr + '</span></div>'
        + '    </div>'
        + '  </div>'
        + '  <p style="color:#555; line-height:1.7; margin:0;">' + (review.comment || '<i style="color:#aaa;">(Không có nội dung đánh giá)</i>') + '</p>'
        + imagesHtml
        + '</div>';
}
