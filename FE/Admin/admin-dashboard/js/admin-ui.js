// Admin UI helpers: toast and confirm using Bootstrap
function showToast(message, type = 'info', timeout = 3000) {
    try {
        let container = document.getElementById('toast-container');
        if (!container) {
            // fallback: create container at body end
            container = document.createElement('div');
            container.setAttribute('aria-live', 'polite');
            container.setAttribute('aria-atomic', 'true');
            container.className = 'position-fixed top-0 end-0 p-3';
            container.style.zIndex = 10850;
            container.id = 'toast-container';
            document.body.appendChild(container);
        }
        const id = 'toast-' + Date.now() + Math.floor(Math.random() * 1000);
        const colorClass = type === 'success' ? 'bg-success text-white' : (type === 'danger' ? 'bg-danger text-white' : 'bg-primary text-white');
        const toastHtml = `\n            <div id="${id}" class="toast align-items-center ${colorClass} border-0 mb-2" role="alert" aria-live="assertive" aria-atomic="true" data-bs-delay="${timeout}">\n                <div class="d-flex">\n                    <div class="toast-body">${message}</div>\n                    <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>\n                </div>\n            </div>`;
        const wrapper = document.createElement('div');
        wrapper.innerHTML = toastHtml;
        const el = wrapper.firstElementChild;
        container.appendChild(el);
        const bsToast = new bootstrap.Toast(el);
        bsToast.show();
        el.addEventListener('hidden.bs.toast', () => { try { el.remove(); } catch (e) { } });
    } catch (e) { console.warn('toast error', e); }
}

function showConfirm(message) {
    return new Promise((resolve) => {
        let modalEl = document.getElementById('confirmModal');
        if (!modalEl) {
            // create a simple confirm modal dynamically
            modalEl = document.createElement('div');
            modalEl.className = 'modal fade';
            modalEl.id = 'confirmModal';
            modalEl.tabIndex = -1;
            modalEl.innerHTML = `\n                <div class="modal-dialog modal-dialog-centered">\n                    <div class="modal-content">\n                        <div class="modal-header">\n                            <h5 class="modal-title">Xác nhận</h5>\n                            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>\n                        </div>\n                        <div class="modal-body" id="confirmModalBody">` + message + `</div>\n                        <div class="modal-footer">\n                            <button type="button" id="confirmCancelBtn" class="btn btn-secondary" data-bs-dismiss="modal">Hủy</button>\n                            <button type="button" id="confirmOkBtn" class="btn btn-primary">Đồng ý</button>\n                        </div>\n                    </div>\n                </div>`;
            document.body.appendChild(modalEl);
        }
        const body = modalEl.querySelector('#confirmModalBody');
        const okBtn = modalEl.querySelector('#confirmOkBtn');
        const cancelBtn = modalEl.querySelector('#confirmCancelBtn');
        if (body) body.innerText = message;
        const modal = new bootstrap.Modal(modalEl, { backdrop: 'static' });

        function cleanup() {
            okBtn && okBtn.removeEventListener('click', onOk);
            cancelBtn && cancelBtn.removeEventListener('click', onCancel);
        }
        function onOk() { cleanup(); modal.hide(); resolve(true); }
        function onCancel() { cleanup(); modal.hide(); resolve(false); }
        okBtn && okBtn.addEventListener('click', onOk);
        cancelBtn && cancelBtn.addEventListener('click', onCancel);
        modal.show();
    });
}
