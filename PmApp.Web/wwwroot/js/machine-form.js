
$(function () {
    // ================================================
    // MACHINE FORM
    // Plant -> Product -> Line
    // Mendukung Select2 dan dropdown biasa
    // ================================================

    const $plant = $("#PlantId");
    const $product = $("#ProductId");
    const $line = $("#LineId");

    if (!$plant.length || !$product.length || !$line.length) {
        return;
    }

    let productRequest = null;
    let lineRequest = null;

    // --------------------------------
    // Refresh tampilan Select2
    // --------------------------------
    function refresh($select) {
        $select.trigger("change.select2");
    }

    // --------------------------------
    // Reset dropdown
    // --------------------------------
    function reset($select, placeholder) {
        $select.empty();
        $select.append(new Option(placeholder, "", true, true));
        $select.prop("disabled", true);
        refresh($select);
    }

    // --------------------------------
    // Mengaktifkan dropdown
    // --------------------------------
    function enable($select) {
        $select.prop("disabled", false);
        refresh($select);
    }

    // --------------------------------
    // Ambil data JSON
    // Endpoint menggunakan { id, text }
    // --------------------------------
    async function getData(url, parameter, value, signal) {
        if (!url) {
            throw new Error("URL endpoint tidak ditemukan.");
        }

        const endpoint = new URL(url, window.location.origin);
        endpoint.searchParams.set(parameter, value);

        const response = await fetch(endpoint, {
            signal,
            headers: {
                Accept: "application/json"
            }
        });

        if (!response.ok) {
            throw new Error(`HTTP ${response.status}`);
        }

        const data = await response.json();

        if (!Array.isArray(data)) {
            throw new Error("Respons endpoint bukan array JSON.");
        }

        return data;
    }

    // ================================================
    // PLANT BERUBAH
    // ================================================
    $plant.on("change", async function () {
        if (productRequest) productRequest.abort();
        if (lineRequest) lineRequest.abort();

        const plantId = $plant.val();

        reset($product, "-- Pilih Product --");
        reset($line, "-- Pilih Line --");

        if (!plantId) return;

        const controller = new AbortController();
        productRequest = controller;

        reset($product, "Memuat Product...");

        try {
            const data = await getData(
                $plant.attr("data-products-url"),
                "plantId",
                plantId,
                controller.signal
            );

            if (controller.signal.aborted ||
                $plant.val() !== plantId) {
                return;
            }

            reset($product, "-- Pilih Product --");

            data.forEach(item => {
                $product.append(
                    new Option(item.text, item.id)
                );
            });

            if (data.length === 0) {
                reset($product, "Tidak ada Product");
                return;
            }

            enable($product);

        } catch (error) {
            if (error.name === "AbortError") return;

            console.error("Gagal memuat Product:", error);

            if ($plant.val() === plantId) {
                reset($product, "Gagal memuat Product");
            }

        } finally {
            if (productRequest === controller) {
                productRequest = null;
            }
        }
    });

    // ================================================
    // PRODUCT BERUBAH
    // ================================================
    $product.on("change", async function () {
        if (lineRequest) lineRequest.abort();

        const productId = $product.val();
        const plantId = $plant.val();

        reset($line, "-- Pilih Line --");

        if (!productId || !plantId) return;

        const controller = new AbortController();
        lineRequest = controller;

        reset($line, "Memuat Line...");

        try {
            const data = await getData(
                $product.attr("data-lines-url"),
                "productId",
                productId,
                controller.signal
            );

            if (controller.signal.aborted ||
                $product.val() !== productId ||
                $plant.val() !== plantId) {
                return;
            }

            reset($line, "-- Pilih Line --");

            data.forEach(item => {
                $line.append(
                    new Option(item.text, item.id)
                );
            });

            if (data.length === 0) {
                reset($line, "Tidak ada Line");
                return;
            }

            enable($line);

        } catch (error) {
            if (error.name === "AbortError") return;

            console.error("Gagal memuat Line:", error);

            if ($product.val() === productId &&
                $plant.val() === plantId) {
                reset($line, "Gagal memuat Line");
            }

        } finally {
            if (lineRequest === controller) {
                lineRequest = null;
            }
        }
    });

    // ================================================
    // KONDISI AWAL CREATE / EDIT
    // ================================================

    // Create:
    // Product dan Line kosong sampai Plant dipilih.
    //
    // Edit:
    // Pilihan lama dipertahankan.

    $product.prop("disabled", !$plant.val());
    $line.prop(
        "disabled",
        !($plant.val() && $product.val())
    );

    refresh($plant);
    refresh($product);
    refresh($line);
});
