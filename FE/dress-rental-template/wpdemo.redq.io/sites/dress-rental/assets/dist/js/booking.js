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

    // Render sizes dropdown
    renderSizes(product.sizes || []);

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
                updateVariantInfo();
            }
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
        sizeSelect.off("change").on("change", function () {
            selectedSize = $(this).val();
            console.log("Size selected:", selectedSize);
            if (selectedSize) {
                if (sizeError) sizeError.style.display = "none";
                updateSizeUI(selectedSize);
                updateVariantInfo();
            }
        });

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
 * Allow >= 3 days
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

    // Logic: >= 3 days (Updated per request)
    if (rentalDays < 3) {
        if (errorDiv) {
            errorDiv.innerHTML = `<i class="fa fa-exclamation-circle"></i> Số ngày thuê phải từ 3 ngày trở lên (hiện tại: ${rentalDays} ngày)`;
            errorDiv.style.display = "block";
        }
        if (infoDiv) infoDiv.style.display = "none";
    } else {
        if (errorDiv) errorDiv.style.display = "none";
        if (infoDiv) {
            infoDiv.style.display = "block";
            if (daysCount) daysCount.textContent = rentalDays;
        }
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
    } else if (rentalDays < 3) { // Check < 3
        if (dateErrorDiv) {
            dateErrorDiv.innerHTML = `<i class="fa fa-exclamation-circle"></i> Số ngày thuê phải từ 3 ngày trở lên`;
            dateErrorDiv.style.display = "block";
            if (!firstErrorEl) firstErrorEl = dateErrorDiv;
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
            const existingCart = JSON.parse(localStorage.getItem("cartItems") || "[]");
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
            localStorage.setItem("cartItems", JSON.stringify(existingCart));

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
        const existingCart = JSON.parse(localStorage.getItem("cartItems") || "[]");
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
        localStorage.setItem("cartItems", JSON.stringify(existingCart));

        showBookingToast(`✓ Đã thêm "${currentProduct.name}" vào giỏ hàng!`, 'success', 3000);

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
    setupDateValidation();
    setupReserveButton();
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
