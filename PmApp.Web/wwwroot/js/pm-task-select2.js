
$(function () {
    if (typeof $.fn.select2 !== "function") {
        console.error("Library Select2 belum dimuat.");
        return;
    }

    $("#methodSelect, #standardSelect").select2({
        tags: true,
        width: "100%",
        placeholder: "-- Pilih atau ketik baru --",
        allowClear: true,
        selectOnClose: false,

        createTag: function (params) {
            const term = $.trim(params.term);

            if (!term) return null;

            return {
                id: term,
                text: term,
                newTag: true
            };
        },

        language: {
            noResults: function () {
                return "Tidak ditemukan. Ketik untuk menambah.";
            },
            searching: function () {
                return "Mencari...";
            }
        }
    });
});
