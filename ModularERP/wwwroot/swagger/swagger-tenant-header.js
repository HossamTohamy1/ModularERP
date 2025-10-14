(function () {
    // انتظر تحميل Swagger UI
    const waitForSwagger = setInterval(() => {
        const authorizeBtn = document.querySelector('.btn.authorize');
        if (authorizeBtn) {
            clearInterval(waitForSwagger);
            initTenantIdPersistence();
        }
    }, 100);

    function initTenantIdPersistence() {
        // استرجع آخر TenantId محفوظ
        const savedTenantId = localStorage.getItem('swagger_tenantId');

        // راقب زر Authorize
        const observer = new MutationObserver(() => {
            const tenantIdInput = document.querySelector('input[placeholder="TenantId"]');
            if (tenantIdInput && savedTenantId && !tenantIdInput.value) {
                tenantIdInput.value = savedTenantId;
            }
        });

        observer.observe(document.body, {
            childList: true,
            subtree: true
        });

        // احفظ القيمة عند الضغط على Authorize
        document.addEventListener('click', (e) => {
            if (e.target.closest('.btn.modal-btn.auth.authorize')) {
                setTimeout(() => {
                    const tenantIdInput = document.querySelector('input[placeholder="TenantId"]');
                    if (tenantIdInput && tenantIdInput.value) {
                        localStorage.setItem('swagger_tenantId', tenantIdInput.value);
                        console.log('✅ TenantId saved:', tenantIdInput.value);
                    }
                }, 500);
            }
        });
    }
})();