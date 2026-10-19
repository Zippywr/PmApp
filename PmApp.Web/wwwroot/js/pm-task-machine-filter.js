
$(function () {

    // ============================================
    // DROPDOWN PLANT > PRODUCT > LINE > MACHINE
    // Mendukung Select2 dan dropdown biasa
    // ============================================

    const $plant = $("#PlantId");
    const $product = $("#ProductId");
    const $line = $("#LineId");
    const $machine = $("#AssetId");

    if (!$plant.length ||
        !$product.length ||
        !$line.length ||
        !$machine.length) {
        return;
    }

    let request = null;
    let requestVersion = 0;

    // ============================================
    // REFRESH TAMPILAN SELECT2
    // ============================================
    function refresh($select) {
        $select.trigger("change.select2");
    }

    // ============================================
    // RESET DROPDOWN
    // ============================================
    function reset($select, placeholder) {
        $select.empty();

        $select.append(
            new Option(placeholder, "", true, true)
        );

        $select.prop("disabled", true);
        refresh($select);
    }

    // ============================================
    // BATALKAN REQUEST SEBELUMNYA
    // ============================================
    function cancelRequest() {
        requestVersion++;

        if (request) {
            request.abort();
            request = null;
        }
    }

    // ============================================
    // LOAD DATA DARI CONTROLLER
    // ============================================
    async function loadOptions(
        url,
        parameter,
        value,
        $target,
        placeholder
    ) {
        cancelRequest();

        const version = requestVersion;
        const controller = new AbortController();

        request = controller;

        reset($target, "Memuat data...");

        try {
            const endpoint = new URL(
                url,
                window.location.origin
            );

            endpoint.searchParams.set(parameter, value);

            const response = await fetch(endpoint, {
                signal: controller.signal,
                headers: {
                    "Accept": "application/json"
                }
            });

            if (!response.ok) {
                throw new Error(
                    "HTTP " + response.status
                );
            }

            const data = await response.json();

            // Abaikan respons yang sudah tidak relevan
            if (version !== requestVersion) return;

            $target.empty();

            $target.append(
                new Option(placeholder, "", true, true)
            );

            data.forEach(item => {
                $target.append(
                    new Option(item.text, item.id)
                );
            });

            if (data.length === 0) {
                reset($target, "Tidak ada data");
                return;
            }

            $target.prop("disabled", false);

            refresh($target);

        } catch (error) {

            if (error.name === "AbortError") {
                return;
            }

            if (version !== requestVersion) return;

            console.error(
                "Gagal memuat dropdown:",
                error
            );

            reset($target, "Gagal memuat data");

        } finally {
            if (version === requestVersion) {
                request = null;
            }
        }
    }

    // ============================================
    // PLANT BERUBAH
    // ============================================
    $plant.on("change", async function () {

        cancelRequest();

        reset($product, "-- Pilih Product --");
        reset($line, "-- Pilih Line --");
        reset($machine, "-- Pilih Machine --");

        const plantId = $plant.val();

        if (!plantId) return;

        await loadOptions(
            $plant.data("products-url"),
            "plantId",
            plantId,
            $product,
            "-- Pilih Product --"
        );
    });

    // ============================================
    // PRODUCT BERUBAH
    // ============================================
    $product.on("change", async function () {

        cancelRequest();

        reset($line, "-- Pilih Line --");
        reset($machine, "-- Pilih Machine --");

        const productId = $product.val();

        if (!productId) return;

        await loadOptions(
            $product.data("lines-url"),
            "productId",
            productId,
            $line,
            "-- Pilih Line --"
        );
    });

    // ============================================
    // LINE BERUBAH
    // ============================================
    $line.on("change", async function () {

        cancelRequest();

        reset($machine, "-- Pilih Machine --");

        const lineId = $line.val();

        if (!lineId) return;

        await loadOptions(
            $line.data("machines-url"),
            "lineId",
            lineId,
            $machine,
            "-- Pilih Machine --"
        );
    });

    // ============================================
    // MACHINE BERUBAH
    // Agar preview ID-PM ikut diperbarui
    // ============================================
    $machine.on("select2:select select2:clear", function () {

        this.dispatchEvent(
            new Event("change", { bubbles: false })
        );
    });

    // ============================================
    // KONDISI AWAL
    // ============================================
    $product.prop(
        "disabled",
        !$plant.val()
    );

    $line.prop(
        "disabled",
        !$product.val()
    );

    $machine.prop(
        "disabled",
        !$line.val()
    );

    refresh($product);
    refresh($line);
    refresh($machine);

});
