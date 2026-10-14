// SPEC-007 FR-002 + FR-001 (eye-toggle): JS externo (CSP script-src 'self', sin inline).
document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('form[data-loading="spinner"]').forEach((form) => {
        form.addEventListener('submit', () => {
            const button = form.querySelector('button[type="submit"]');
            if (!button || button.disabled) {
                return;
            }
            button.disabled = true;
            const text = button.querySelector('.btn-text');
            const spinner = button.querySelector('.spinner-border');
            if (text) {
                text.classList.add('d-none');
            }
            if (spinner) {
                spinner.classList.remove('d-none');
            }
        });
    });

    document.querySelectorAll('button[data-password-toggle]').forEach((toggle) => {
        toggle.addEventListener('click', () => {
            const group = toggle.closest('.input-group');
            const input = group ? group.querySelector('input') : null;
            const icon = toggle.querySelector('i.bi');
            if (!input) {
                return;
            }
            const show = input.type === 'password';
            input.type = show ? 'text' : 'password';
            if (icon) {
                icon.classList.toggle('bi-eye', !show);
                icon.classList.toggle('bi-eye-slash', show);
            }
        });
    });

    document.querySelectorAll('input[data-caps-warn]').forEach((input) => {
        const warn = document.querySelector(input.getAttribute('data-caps-warn'));
        if (!warn) {
            return;
        }
        input.addEventListener('keyup', (event) => {
            const on = event.getModifierState ? event.getModifierState('CapsLock') : false;
            warn.classList.toggle('d-none', !on);
        });
        input.addEventListener('blur', () => {
            warn.classList.add('d-none');
        });
    });
});
