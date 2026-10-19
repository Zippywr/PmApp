// ============================================
// PM SYSTEM - UI HELPERS (SweetAlert2 Simple)
// ============================================

(function () {
    'use strict';

    // ============================================
    // NOTIFIKASI (Modal SweetAlert2 di tengah)
    // ============================================
    window.showNotify = function (type, message, title) {
        var titles = {
            success: 'Berhasil!',
            danger: 'Gagal!',
            warning: 'Perhatian!',
            info: 'Info'
        };

        var iconMap = {
            success: 'success',
            danger: 'error',
            warning: 'warning',
            info: 'info'
        };

        var isSuccess = type === 'success';

        Swal.fire({
            icon: iconMap[type] || 'info',
            title: title || titles[type] || 'Info',
            text: message,
            timer: isSuccess ? 1800 : undefined,
            timerProgressBar: isSuccess,
            showConfirmButton: !isSuccess,
            confirmButtonText: 'OK',
            confirmButtonColor: '#0d6efd',
            customClass: {
                popup: 'swal-pm-popup',
                title: 'swal-pm-title'
            }
        });
    };

    // Alias — biar kompatibel dengan kode lama
    window.showToast = function (type, message, title) {
        showNotify(type, message, title);
    };

    // ============================================
    // LOADING
    // ============================================
    window.showLoading = function (message) {
        Swal.fire({
            title: message || 'Memproses...',
            allowOutsideClick: false,
            allowEscapeKey: false,
            didOpen: function () {
                Swal.showLoading();
            }
        });
    };

    window.hideLoading = function () {
        Swal.close();
    };

    window.addEventListener('pageshow', function (event) {
        if (event.persisted && Swal.isVisible()) Swal.close();
        document.querySelectorAll('form.form-confirm').forEach(function (form) {
            delete form.dataset.confirmed;
        });
    });

    // ============================================
    // CONFIRM ACTION
    // ============================================
    window.confirmAction = function (options) {
        var defaults = {
            title: 'Apakah Anda yakin?',
            message: 'Data akan diubah secara permanen.',
            details: null,
            type: 'warning',
            confirmText: 'Ya, Lanjutkan!',
            cancelText: 'Batal',
            onConfirm: null,
            onCancel: null
        };

        var opts = Object.assign({}, defaults, options);

        var iconMap = {
            primary: 'question',
            success: 'success',
            danger: 'warning',
            warning: 'warning',
            question: 'question',
            info: 'info',
            error: 'error'
        };

        var icon = opts.icon || iconMap[opts.type] || 'warning';

        var detailHtml = '';
        if (opts.details && opts.details.length > 0) {
            detailHtml = '<div style="text-align:left; background:#f8f9fa; border-radius:8px; padding:15px; margin-top:15px; font-size:0.9rem;">';
            opts.details.forEach(function (d) {
                detailHtml += '<div style="padding:4px 0;">' +
                    '<strong style="color:#495057;">' + d.label + ':</strong> ' +
                    '<span style="color:#6c757d;"> ' + d.value + '</span>' +
                    '</div>';
            });
            detailHtml += '</div>';
        }

        var confirmColor = '#3085d6';
        if (opts.type === 'danger' || opts.type === 'error') confirmColor = '#d33';
        if (opts.type === 'success') confirmColor = '#198754';

        Swal.fire({
            title: opts.title,
            html: detailHtml ? '<p style="color:#6c757d; margin-bottom:0;">' + opts.message + '</p>' + detailHtml : undefined,
            text: detailHtml ? undefined : opts.message,
            icon: icon,
            showCancelButton: true,
            confirmButtonColor: confirmColor,
            cancelButtonColor: '#d33',
            confirmButtonText: opts.confirmText,
            cancelButtonText: opts.cancelText,
            reverseButtons: true,
            customClass: {
                popup: 'swal-pm-popup',
                title: 'swal-pm-title'
            }
        }).then(function (result) {
            if (result.isConfirmed) {
                if (opts.onConfirm) opts.onConfirm();
            } else if (result.dismiss === Swal.DismissReason.cancel) {
                if (opts.onCancel) opts.onCancel();
            }
        });
    };

    // ============================================
    // AUTO-ATTACH HANDLERS
    // ============================================
    document.addEventListener('DOMContentLoaded', function () {

        // 1. Form dengan class "form-confirm" → konfirmasi sebelum submit
        document.querySelectorAll('form.form-confirm').forEach(function (form) {
            form.addEventListener('submit', function (e) {
                if (form.dataset.confirmed === 'true') {
                    showLoading(form.dataset.loadingText || 'Menyimpan data...');
                    return;
                }

                // Hindari SweetAlert saat form belum lolos constraint validation.
                if (!form.checkValidity()) {
                    form.reportValidity();
                    e.preventDefault();
                    return;
                }

                e.preventDefault();

                var title = form.dataset.confirmTitle || 'Konfirmasi Simpan';
                var message = form.dataset.confirmMessage || 'Yakin simpan data ini?';
                var btnText = form.dataset.confirmButton || 'Ya, Simpan';

                confirmAction({
                    title: title,
                    message: message,
                    type: 'info',
                    confirmText: btnText,
                    cancelText: 'Batal',
                    onConfirm: function () {
                        form.dataset.confirmed = 'true';
                        showLoading(form.dataset.loadingText || 'Menyimpan data...');
                        // requestSubmit memicu validasi + submit event seperti klik asli.
                        // Flag confirmed mencegah dialog konfirmasi kedua.
                        form.requestSubmit();
                    }
                });
            });
        });

        // 2. Tombol dengan class "btn-delete" → konfirmasi hapus
        document.querySelectorAll('.btn-delete').forEach(function (btn) {
            btn.addEventListener('click', function (e) {
                e.preventDefault();

                var form = btn.closest('form');
                if (!form) {
                    console.error('btn-delete harus di dalam <form>');
                    return;
                }

                var name = btn.dataset.name || 'data ini';
                var detail = btn.dataset.detail;

                var details = [];
                if (detail) {
                    details.push({ label: 'Info', value: detail });
                }

                confirmAction({
                    title: 'Apakah Anda yakin?',
                    message: 'Anda akan menghapus ' + name + '.',
                    type: 'warning',
                    confirmText: 'Ya, Hapus!',
                    cancelText: 'Batal',
                    details: details.length > 0 ? details : null,
                    onConfirm: function () {
                        showLoading('Menghapus data...');
                        form.submit();
                    }
                });
            });
        });

        // 3. Tombol dengan class "btn-post" → konfirmasi sebelum POST
        document.querySelectorAll('.btn-post').forEach(function (btn) {
            btn.addEventListener('click', function (e) {
                e.preventDefault();

                var form = btn.closest('form');
                if (!form) return;

                var title = btn.dataset.confirmTitle || 'Konfirmasi';
                var message = btn.dataset.confirmMessage || 'Yakin lanjutkan?';
                var type = btn.dataset.confirmType || 'info';
                var btnText = btn.dataset.confirmButton || 'Ya, Lanjutkan';
                var loadingText = btn.dataset.loadingText || 'Memproses...';

                confirmAction({
                    title: title,
                    message: message,
                    type: type,
                    confirmText: btnText,
                    onConfirm: function () {
                        showLoading(loadingText);
                        form.submit();
                    }
                });
            });
        });
    });

})();