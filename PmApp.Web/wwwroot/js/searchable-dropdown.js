
document.addEventListener("DOMContentLoaded", function () {
    if (!window.jQuery || !jQuery.fn.select2) {
        console.error("Select2 belum tersedia.");
        return;
    }

    jQuery("select.searchable-select").each(function () {
        const $select = jQuery(this);

        // Hindari inisialisasi berulang
        if ($select.hasClass("select2-hidden-accessible")) {
            return;
        }

        $select.select2({
            width: "100%",
            placeholder: $select.data("placeholder")
                || "-- Cari atau Pilih --",
            allowClear: true,
            minimumResultsForSearch: 0,
            language: {
                noResults: function () {
                    return "Data tidak ditemukan";
                },
                searching: function () {
                    return "Mencari...";
                }
            }
        });
    });
});
