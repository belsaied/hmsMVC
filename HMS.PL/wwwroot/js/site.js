// Password Toggle
(function () {
    'use strict';

    function initPasswordToggles() {
        document.querySelectorAll('.hms-password-toggle').forEach(function (toggle) {
            toggle.addEventListener('click', function (e) {
                var input = this.parentElement.querySelector('input');
                if (!input) return;

                if (input.type === 'password') {
                    input.type = 'text';
                    this.classList.replace('fa-eye', 'fa-eye-slash');
                    this.setAttribute('aria-label', 'Hide password');
                    this.setAttribute('aria-pressed', 'true');
                } else {
                    input.type = 'password';
                    this.classList.replace('fa-eye-slash', 'fa-eye');
                    this.setAttribute('aria-label', 'Show password');
                    this.setAttribute('aria-pressed', 'false');
                }
            });
        });
    }

    // Form loading states
    function initFormLoadingStates() {
        document.querySelectorAll('.hms-auth-form').forEach(function (form) {
            form.addEventListener('submit', function () {
                var btn = this.querySelector('.hms-auth-btn');
                if (!btn) return;
                btn.disabled = true;
                var originalHtml = btn.innerHTML;
                btn.dataset.originalHtml = originalHtml;
                btn.innerHTML = '<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span><span>' +
                    (btn.classList.contains('btn-loading-text') ? btn.dataset.loadingText : 'Please wait...') +
                    '</span>';
            });
        });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function () {
            initPasswordToggles();
            initFormLoadingStates();
        });
    } else {
        initPasswordToggles();
        initFormLoadingStates();
    }
})();
