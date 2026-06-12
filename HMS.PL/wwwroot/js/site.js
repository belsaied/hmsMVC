// Password Toggle
(function () {
    'use strict';

    function initPasswordToggles() {
        document.querySelectorAll('.hms-password-toggle').forEach(function (toggle) {
            toggle.addEventListener('click', function () {
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

    // Form loading states — hooked into jQuery Validation's submitHandler
    // so the button only enters loading state when the form is genuinely valid.
    function initFormLoadingStates() {
        document.querySelectorAll('.hms-auth-form').forEach(function (form) {
            // Wait for jQuery + jquery-validation to be available
            if (typeof $ === 'undefined' || !$.fn || !$.fn.validate) return;

            var $form = $(form);
            var validator = $form.data('validator');

            if (!validator) {
                $form.on('submit', function () {
                    if ($form.valid && !$form.valid()) return;
                    setLoading($form[0]);
                });
                return;
            }

            var originalSubmitHandler = validator.settings.submitHandler;
            validator.settings.submitHandler = function (formEl, event) {
                setLoading(formEl);
                if (originalSubmitHandler) {
                    return originalSubmitHandler.call(this, formEl, event);
                }
                formEl.submit();
            };
        });
    }

    function setLoading(formEl) {
        var btn = formEl.querySelector('.hms-auth-btn');
        if (!btn || btn.disabled) return;
        btn.disabled = true;
        btn.dataset.originalHtml = btn.innerHTML;
        btn.innerHTML =
            '<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>' +
            '<span>Please wait...</span>';
    }

    function init() {
        initPasswordToggles();

        if (typeof $ !== 'undefined' && $.fn && $.fn.validate) {
            initFormLoadingStates();
        } else {
            // jQuery or jquery-validation not yet loaded — wait for it.
            var waited = 0;
            var interval = setInterval(function () {
                waited += 50;
                if ((typeof $ !== 'undefined' && $.fn && $.fn.validate) || waited > 3000) {
                    clearInterval(interval);
                    initFormLoadingStates();
                }
            }, 50);
        }
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();