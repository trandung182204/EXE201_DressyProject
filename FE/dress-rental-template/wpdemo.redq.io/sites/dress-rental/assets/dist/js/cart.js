/**
 * cart.js - Cart & Checkout Page Logic
 * =====================================
 * Uses original template CSS classes (.diva.short-dress, .shopping-bag, etc.)
 * Depends on: api.js (must be loaded first)
 */

// ============================================================
// 1. CART STATE
// ============================================================
const cartState = {
    items: [],
    discount: 0,
    voucherCode: "",
    voucherApplied: false,
};

let editingIndex = -1;

// ============================================================
// 2. INITIALIZATION
// ============================================================
document.addEventListener("DOMContentLoaded", () => {
    console.log("[CART] Initializing...");
    loadCartFromStorage();
    renderAll();
    setupEventListeners();
    autoFillDeliveryForm();
});

/**
 * Load cart items from localStorage
 */
function loadCartFromStorage() {
    // Try cartItems array first (multi-product cart)
    const rawItems = localStorage.getItem("cartItems");
    if (rawItems) {
        try {
            const items = JSON.parse(rawItems);
            if (Array.isArray(items) && items.length > 0) {
                cartState.items = items;
                console.log("[CART] Loaded", items.length, "items from cartItems");
                return;
            }
        } catch (e) {
            console.error("[CART] Failed to parse cartItems:", e);
        }
    }

    // Fallback: single currentBooking (backward compat)
    const raw = localStorage.getItem("currentBooking");
    if (raw) {
        try {
            const booking = JSON.parse(raw);
            cartState.items = [booking];
            // Migrate to cartItems format
            localStorage.setItem("cartItems", JSON.stringify([booking]));
            console.log("[CART] Migrated single booking to cartItems");
            return;
        } catch (e) {
            console.error("[CART] Failed to parse currentBooking:", e);
        }
    }

    console.warn("[CART] No cart data found in localStorage");
    cartState.items = [];
}

// ============================================================
// 3. RENDER ALL
// ============================================================
function renderAll() {
    renderCartItems();
    renderPriceSummary();
}

// ============================================================
// 4. CART ITEMS RENDERING (using original template .diva.short-dress)
// ============================================================
function renderCartItems() {
    const container = document.getElementById("cart-items-container");
    if (!container) return;

    if (cartState.items.length === 0) {
        container.innerHTML = `
      <div class="diva" style="text-align:center; padding: 40px;">
        <p style="font-size: 18px; color: #979797;">Giỏ hàng trống</p>
        <p style="color: #979797;">Bạn chưa chọn sản phẩm nào để thuê.</p>
        <a href="listing-page.html" style="color: #392606; text-decoration: underline;">← Khám phá sản phẩm</a>
      </div>`;
        return;
    }

    container.innerHTML = cartState.items.map((item, index) => {
        const imgUrl = buildImageUrl(item.thumbnailUrl);
        const startStr = formatDate(item.startDate);
        const endStr = formatDate(item.endDate);
        const itemTotal = calcItemTotal(item);

        return `
      <div class="diva short-dress" data-index="${index}">
        <img src="${imgUrl}" alt="${item.productName || ''}" style="width:130px; height:auto; object-fit:cover;" />
        <h5 class="font-30-for-reg-0">${item.productName || 'Sản phẩm'}</h5>
        <p class="color">Giá thuê : <span>${formatMoney(item.pricePerDay)} /ngày</span></p>
        ${item.depositAmount ? `<p>Giá cọc : <span>${formatMoney(item.depositAmount)}</span></p>` : ''}
        <p>Màu sắc : <span>${item.color || 'N/A'}</span></p>
        <p>Kích thước : <span>${item.size || 'N/A'}</span></p>
        <p>Số lượng : <span>${item.quantity || 1}</span></p>
        <p>Ngày nhận : <span>${startStr}</span></p>
        <p>Ngày trả : <span>${endStr}</span></p>
        <p>Thời gian thuê : <span>${item.days || 0} ngày</span></p>
        <p class="color" style="font-size: 20px; margin-top: 10px;">Thành tiền : <span style="font-weight:bold;">${formatMoney(itemTotal)}</span></p>
        <div style="display: flex; gap: 10px; margin-top: 15px;">
          <button onclick="openEditModal(${index})" style="white-space:nowrap; min-width:130px"><i class="ion-edit"></i> Chỉnh sửa</button>
          <button onclick="removeItem(${index})" style="white-space:nowrap;"><i class="fa fa-trash"></i> Xóa</button>
        </div>
      </div>`;
    }).join("");
}

// ============================================================
// 5. PRICE SUMMARY
// ============================================================
function calcItemTotal(item) {
    const price = item.pricePerDay || 0;
    const days = item.days || 0;
    const qty = item.quantity || 1;
    return price * days * qty;
}

function calcSubtotal() {
    return cartState.items.reduce((sum, item) => sum + calcItemTotal(item), 0);
}

function renderPriceSummary() {
    const subtotal = calcSubtotal();
    const discount = cartState.discount || 0;
    const total = Math.max(0, subtotal - discount);

    const elSubtotal = document.getElementById("price-subtotal");
    const elDiscount = document.getElementById("price-discount");
    const elTotal = document.getElementById("price-total");

    if (elSubtotal) elSubtotal.textContent = formatMoney(subtotal);
    if (elDiscount) elDiscount.textContent = `-${formatMoney(discount)}`;
    if (elTotal) elTotal.textContent = formatMoney(total);
}

// ============================================================
// 6. EDIT MODAL
// ============================================================
function openEditModal(index) {
    const item = cartState.items[index];
    if (!item) return;
    editingIndex = index;

    const modal = document.getElementById("edit-modal");
    if (!modal) return;

    // Populate color
    const colorSelect = document.getElementById("edit-color");
    if (colorSelect) {
        const colors = item.colors || [];
        if (colors.length > 0) {
            colorSelect.innerHTML = colors.map(c =>
                `<option value="${c}" ${c === item.color ? 'selected' : ''}>${c}</option>`
            ).join("");
            colorSelect.disabled = false;
        } else {
            colorSelect.innerHTML = `<option value="${item.color || ''}">${item.color || 'N/A'}</option>`;
            colorSelect.disabled = true;
        }
    }

    // Populate size
    const sizeSelect = document.getElementById("edit-size");
    if (sizeSelect) {
        const sizes = item.sizes || [];
        if (sizes.length > 0) {
            sizeSelect.innerHTML = sizes.map(s =>
                `<option value="${s}" ${s === item.size ? 'selected' : ''}>${s}</option>`
            ).join("");
            sizeSelect.disabled = false;
        } else {
            sizeSelect.innerHTML = `<option value="${item.size || ''}">${item.size || 'N/A'}</option>`;
            sizeSelect.disabled = true;
        }
    }

    // Populate dates
    const startInput = document.getElementById("edit-start-date");
    const endInput = document.getElementById("edit-end-date");
    if (startInput && item.startDate) startInput.value = toInputDate(item.startDate);
    if (endInput && item.endDate) endInput.value = toInputDate(item.endDate);

    modal.style.display = "flex";
}

function closeEditModal() {
    const modal = document.getElementById("edit-modal");
    if (modal) modal.style.display = "none";
    editingIndex = -1;
}

function saveEdit() {
    if (editingIndex < 0 || !cartState.items[editingIndex]) return;
    const item = cartState.items[editingIndex];

    const newColor = document.getElementById("edit-color")?.value || item.color;
    const newSize = document.getElementById("edit-size")?.value || item.size;
    const newStart = document.getElementById("edit-start-date")?.value;
    const newEnd = document.getElementById("edit-end-date")?.value;

    if (!newStart || !newEnd) {
        showToast("Vui lòng chọn ngày nhận và ngày trả", "error");
        return;
    }

    const startD = new Date(newStart);
    const endD = new Date(newEnd);

    if (endD <= startD) {
        showToast("Ngày trả phải sau ngày nhận", "error");
        return;
    }

    const diffDays = Math.ceil((endD - startD) / (1000 * 60 * 60 * 24)) + 1;
    if (diffDays < 3) {
        showToast("Số ngày thuê phải từ 3 ngày trở lên", "error");
        return;
    }

    item.color = newColor;
    item.size = newSize;
    item.startDate = startD.toISOString();
    item.endDate = endD.toISOString();
    item.days = diffDays;
    item.totalPrice = calcItemTotal(item);

    saveCartToStorage();
    renderAll();
    closeEditModal();
    showToast("Đã cập nhật thông tin sản phẩm", "success");
}

function removeItem(index) {
    cartState.items.splice(index, 1);
    saveCartToStorage();
    renderAll();
    showToast("Đã xóa sản phẩm khỏi giỏ hàng", "info");
}

// ============================================================
// 7. VOUCHER
// ============================================================
async function applyVoucher() {
    const input = document.getElementById("voucher-input");
    const code = input?.value?.trim();

    if (!code) {
        showToast("Vui lòng nhập mã giảm giá", "error");
        return;
    }

    try {
        const data = await apiFetch("POST", "/api/Vouchers/validate", {
            code: code,
            orderTotal: calcSubtotal(),
        });

        const discountAmount = data.discountAmount || data.discount || 0;
        cartState.discount = discountAmount;
        cartState.voucherCode = code;
        cartState.voucherApplied = true;

        renderPriceSummary();
        showToast(`Áp dụng mã "${code}" thành công! Giảm ${formatMoney(discountAmount)}`, "success");
    } catch (err) {
        console.error("[CART] Voucher error:", err);
        cartState.discount = 0;
        cartState.voucherApplied = false;
        renderPriceSummary();
        showToast(err.message || "Mã giảm giá không hợp lệ", "error");
    }
}

// ============================================================
// 8. AUTO-FILL DELIVERY FORM
// ============================================================
async function autoFillDeliveryForm() {
    if (!isLoggedIn()) {
        console.log("[CART] User not logged in, skipping auto-fill");
        return;
    }

    try {
        const data = await apiFetch("GET", "/api/auth/me", null, false);
        const profile = data.data || data;

        const fullName = profile.fullName || profile.userName || "";
        const phone = profile.phone || profile.phoneNumber || "";
        const address = profile.address || "";

        const elName = document.getElementById("delivery-name");
        const elPhone = document.getElementById("delivery-phone");
        const elAddress = document.getElementById("delivery-address");

        if (elName && fullName) elName.value = fullName;
        if (elPhone && phone) elPhone.value = phone;
        if (elAddress && address) elAddress.value = address;

        console.log("[CART] Auto-filled delivery form from profile");
    } catch (err) {
        console.warn("[CART] Could not auto-fill delivery form:", err.message);
    }
}

// ============================================================
// 9. VALIDATION
// ============================================================
function validateCheckout() {
    const errors = [];

    if (cartState.items.length === 0) {
        errors.push("Giỏ hàng trống");
    }

    for (const item of cartState.items) {
        if (!item.startDate || !item.endDate) {
            errors.push(`Sản phẩm "${item.productName}" chưa chọn ngày nhận/trả`);
        }
        if (item.days < 3) {
            errors.push(`Sản phẩm "${item.productName}" phải thuê tối thiểu 3 ngày`);
        }
    }

    const name = document.getElementById("delivery-name")?.value?.trim();
    const phone = document.getElementById("delivery-phone")?.value?.trim();
    const address = document.getElementById("delivery-address")?.value?.trim();

    if (!name) errors.push("Vui lòng nhập họ tên");
    if (!phone) errors.push("Vui lòng nhập số điện thoại");
    if (!address) errors.push("Vui lòng nhập địa chỉ giao hàng");

    if (phone && !/^(0|\+84)\d{8,10}$/.test(phone.replace(/\s/g, ""))) {
        errors.push("Số điện thoại không hợp lệ");
    }

    if (errors.length > 0) {
        errors.forEach(msg => showToast(msg, "error", 4000));
        return false;
    }
    return true;
}

// ============================================================
// 10. SUBMIT ORDER → POST /api/Bookings
// ============================================================
async function submitOrder() {
    if (!validateCheckout()) return;

    if (!isLoggedIn()) {
        showToast("Vui lòng đăng nhập trước khi đặt hàng", "error");
        return;
    }

    const customerId = localStorage.getItem("userId");
    const subtotal = calcSubtotal();
    const discount = cartState.discount || 0;
    const totalPrice = Math.max(0, subtotal - discount);

    // Build BookingItems array matching BE.Models.BookingItems
    const bookingItems = cartState.items.map(item => {
        // Convert ISO date string to "yyyy-MM-dd" for DateOnly
        const startDateOnly = item.startDate ? item.startDate.split("T")[0] : null;
        const endDateOnly = item.endDate ? item.endDate.split("T")[0] : null;

        return {
            productId: item.productId,
            startDate: startDateOnly,
            endDate: endDateOnly,
            price: calcItemTotal(item),
        };
    });

    // Build Bookings object matching BE.Models.Bookings
    const payload = {
        customerId: customerId ? Number(customerId) : null,
        totalPrice: totalPrice,
        status: "PENDING",
        createdAt: new Date().toISOString(),
        recipientName: document.getElementById("delivery-name")?.value?.trim() || "",
        recipientPhone: document.getElementById("delivery-phone")?.value?.trim() || "",
        recipientAddress: document.getElementById("delivery-address")?.value?.trim() || "",
        bookingItems: bookingItems,
    };

    console.log("[CART] Submitting booking:", payload);

    try {
        const data = await apiFetch("POST", "/api/Bookings", payload);
        console.log("[CART] Booking response:", data);

        // Get booking ID from response
        const bookingId = data?.data?.id || data?.id || "";

        localStorage.removeItem("cartItems");
        localStorage.removeItem("currentBooking");
        cartState.items = [];
        cartState.discount = 0;
        cartState.voucherCode = "";
        cartState.voucherApplied = false;

        showToast("Đặt hàng thành công!", "success", 3000);

        setTimeout(() => {
            window.location.href = "order-success.html" + (bookingId ? "?id=" + bookingId : "");
        }, 1500);
    } catch (err) {
        console.error("[CART] Booking submission failed:", err);
        showToast(err.message || "Đặt hàng thất bại. Vui lòng thử lại.", "error");
    }
}

// ============================================================
// 11. EVENT LISTENERS
// ============================================================
function setupEventListeners() {
    // Voucher
    const voucherBtn = document.getElementById("btn-apply-voucher");
    if (voucherBtn) voucherBtn.addEventListener("click", applyVoucher);

    const voucherInput = document.getElementById("voucher-input");
    if (voucherInput) {
        voucherInput.addEventListener("keypress", (e) => {
            if (e.key === "Enter") { e.preventDefault(); applyVoucher(); }
        });
    }

    // Order buttons (left column)
    const orderBtn = document.getElementById("btn-place-order");
    if (orderBtn) orderBtn.addEventListener("click", () => submitOrder());

    // Sidebar order button
    const sidebarBtn = document.getElementById("btn-sidebar-order");
    if (sidebarBtn) sidebarBtn.addEventListener("click", () => submitOrder());

    // Update cart button
    const updateBtn = document.getElementById("btn-update-cart");
    if (updateBtn) {
        updateBtn.addEventListener("click", (e) => {
            e.preventDefault();
            renderAll();
            showToast("Giỏ hàng đã được cập nhật", "success");
        });
    }

    // Edit modal
    const closeBtn = document.getElementById("edit-modal-close");
    if (closeBtn) closeBtn.addEventListener("click", closeEditModal);

    const saveBtn = document.getElementById("edit-modal-save");
    if (saveBtn) saveBtn.addEventListener("click", saveEdit);

    // Click outside modal
    const modal = document.getElementById("edit-modal");
    if (modal) {
        modal.addEventListener("click", (e) => {
            if (e.target === modal) closeEditModal();
        });
    }
}

// ============================================================
// 12. UTILITY HELPERS
// ============================================================
function formatDate(dateStr) {
    if (!dateStr) return "Chưa chọn";
    try {
        const d = new Date(dateStr);
        if (isNaN(d.getTime())) return "Chưa chọn";
        const dd = String(d.getDate()).padStart(2, "0");
        const mm = String(d.getMonth() + 1).padStart(2, "0");
        const yyyy = d.getFullYear();
        return `${dd}/${mm}/${yyyy}`;
    } catch { return "Chưa chọn"; }
}

function toInputDate(dateStr) {
    if (!dateStr) return "";
    try {
        const d = new Date(dateStr);
        if (isNaN(d.getTime())) return "";
        return d.toISOString().split("T")[0];
    } catch { return ""; }
}

function saveCartToStorage() {
    if (cartState.items.length > 0) {
        localStorage.setItem("cartItems", JSON.stringify(cartState.items));
    } else {
        localStorage.removeItem("cartItems");
        localStorage.removeItem("currentBooking");
    }
}
