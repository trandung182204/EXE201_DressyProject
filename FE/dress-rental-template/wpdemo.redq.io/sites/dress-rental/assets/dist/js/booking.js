/**
 * Booking Page - Product Detail & Date Validation
 * Fetches product details and validates rental duration
 * NO alert() - all errors displayed inline
 * Button 'Rent Now' is ALWAYS ENABLED - validation on click
 * Supports Select2 dynamic options (Correctly handles re-init)
 */

const API_BASE = "http://localhost:5135";

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

    // Update price
    const priceEl = document.querySelector(".rq-inner > h4");
    if (priceEl) {
        const price = product.minPricePerDay || product.pricePerDay || 0;
        priceEl.innerHTML = `<span id="productPrice">${money(price)}</span> /ngày`;
    }

    // Update main image
    const mainImg = document.querySelector(".flexslider .slides li:first-child img");
    if (mainImg && product.thumbnailUrl) {
        mainImg.src = safeImg(product.thumbnailUrl);
        mainImg.alt = product.name || "";
    }

    // Render colors dropdown
    renderColors(product.colors || []);

    // Render sizes dropdown
    renderSizes(product.sizes || []);
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
    rentalDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));

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
            const bookingInfo = {
                productId: currentProduct.id,
                productName: currentProduct.name,
                color: selectedColor,
                size: selectedSize,
                startDate: startDate.toISOString(),
                endDate: endDate.toISOString(),
                days: rentalDays,
                pricePerDay: currentProduct.minPricePerDay || currentProduct.pricePerDay || 0,
                totalPrice: (currentProduct.minPricePerDay || currentProduct.pricePerDay || 0) * rentalDays,
                thumbnailUrl: currentProduct.thumbnailUrl
            };

            localStorage.setItem("currentBooking", JSON.stringify(bookingInfo));
            window.location.href = `cart.html`;
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

document.addEventListener("DOMContentLoaded", init);
