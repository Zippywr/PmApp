
document.addEventListener("DOMContentLoaded", () => {
    const plant = document.getElementById("PlantId");
    const product = document.getElementById("ProductId");
    const line = document.getElementById("LineId");

    if (!plant || !product || !line) return;

    const productsUrl = plant.dataset.productsUrl;
    const linesUrl = product.dataset.linesUrl;

    let productRequest = null;
    let lineRequest = null;

    function resetSelect(select, placeholder) {
        select.replaceChildren(new Option(placeholder, ""));
        select.value = "";
    }

    function setProductEnabled(enabled) {
        product.disabled = !enabled;
    }

    function setLineEnabled(enabled) {
        line.disabled = !enabled;
    }

    // ========================================
    // SAAT PLANT BERUBAH
    // ========================================
    plant.addEventListener("change", async () => {
        if (productRequest) productRequest.abort();
        if (lineRequest) lineRequest.abort();

        const plantId = plant.value;

        resetSelect(product, "-- Pilih Product --");
        resetSelect(line, "-- Pilih Line --");

        setProductEnabled(false);
        setLineEnabled(false);

        if (!plantId) return;

        const controller = new AbortController();
        productRequest = controller;

        resetSelect(product, "Memuat Product...");

        try {
            const url = new URL(
                productsUrl,
                window.location.origin
            );

            url.searchParams.set("plantId", plantId);

            const response = await fetch(url, {
                signal: controller.signal,
                headers: {
                    Accept: "application/json"
                }
            });

            if (!response.ok) {
                throw new Error("Gagal memuat Product");
            }

            const data = await response.json();

            if (controller.signal.aborted ||
                plant.value !== plantId) {
                return;
            }

            resetSelect(product, "-- Pilih Product --");

            data.forEach(item => {
                product.add(new Option(
                    `${item.code} - ${item.name}`,
                    item.id
                ));
            });

            if (data.length === 0) {
                resetSelect(
                    product,
                    "Tidak ada Product"
                );
            }

            setProductEnabled(true);

        } catch (error) {
            if (error.name === "AbortError") return;

            console.error(error);

            if (plant.value === plantId) {
                resetSelect(product, "Gagal memuat Product");
                setProductEnabled(false);
            }
        }
    });

    // ========================================
    // SAAT PRODUCT BERUBAH
    // ========================================
    product.addEventListener("change", async () => {
        if (lineRequest) lineRequest.abort();

        const productId = product.value;
        const selectedPlantId = plant.value;

        resetSelect(line, "-- Pilih Line --");
        setLineEnabled(false);

        if (!productId || !selectedPlantId) return;

        const controller = new AbortController();
        lineRequest = controller;

        resetSelect(line, "Memuat Line...");

        try {
            const url = new URL(
                linesUrl,
                window.location.origin
            );

            url.searchParams.set("productId", productId);

            const response = await fetch(url, {
                signal: controller.signal,
                headers: {
                    Accept: "application/json"
                }
            });

            if (!response.ok) {
                throw new Error("Gagal memuat Line");
            }

            const data = await response.json();

            if (controller.signal.aborted ||
                product.value !== productId ||
                plant.value !== selectedPlantId) {
                return;
            }

            resetSelect(line, "-- Pilih Line --");

            data.forEach(item => {
                line.add(new Option(
                    `${item.code} - ${item.name}`,
                    item.id
                ));
            });

            if (data.length === 0) {
                resetSelect(line, "Tidak ada Line");
            }

            setLineEnabled(true);

        } catch (error) {
            if (error.name === "AbortError") return;

            console.error(error);

            if (product.value === productId &&
                plant.value === selectedPlantId) {
                resetSelect(line, "Gagal memuat Line");
                setLineEnabled(false);
            }
        }
    });

    // ========================================
    // KONDISI AWAL CREATE / EDIT
    // ========================================

    // Pada Edit, pilihan lama sudah diberikan
    // oleh AssetController dan tidak perlu di-fetch ulang.
    setProductEnabled(Boolean(plant.value));
    setLineEnabled(Boolean(plant.value && product.value));
});
