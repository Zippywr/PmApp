
document.addEventListener("DOMContentLoaded", () => {
    const plant = document.getElementById("PlantId");
    const product = document.getElementById("ProductId");
    const line = document.getElementById("LineId");
    const machine = document.getElementById("AssetId");

    if (!plant || !product || !line || !machine) return;

    let activeRequest = null;

    function reset(select, text) {
        select.replaceChildren(new Option(text, ""));
        select.value = "";
    }

    async function loadOptions(url, parameter, value, target) {
        if (activeRequest) activeRequest.abort();

        const controller = new AbortController();
        activeRequest = controller;

        target.disabled = true;
        reset(target, "Memuat data...");

        try {
            const endpoint = new URL(url, window.location.origin);
            endpoint.searchParams.set(parameter, value);

            const response = await fetch(endpoint, {
                signal: controller.signal,
                headers: { "Accept": "application/json" }
            });

            if (!response.ok) {
                throw new Error("Gagal memuat data.");
            }

            const data = await response.json();

            if (controller.signal.aborted) return;

            reset(target, "-- Pilih --");

            data.forEach(item => {
                target.add(new Option(item.text, item.id));
            });

            if (data.length === 0) {
                reset(target, "Tidak ada data");
            }

            target.disabled = false;
        } catch (error) {
            if (error.name === "AbortError") return;

            console.error(error);
            reset(target, "Gagal memuat data");
            target.disabled = true;
        }
    }

    plant.addEventListener("change", async () => {
        if (activeRequest) activeRequest.abort();

        reset(product, "-- Pilih Product --");
        reset(line, "-- Pilih Line --");
        reset(machine, "-- Pilih Machine --");

        product.disabled = true;
        line.disabled = true;
        machine.disabled = true;

        if (!plant.value) return;

        await loadOptions(
            plant.dataset.productsUrl,
            "plantId",
            plant.value,
            product
        );
    });

    product.addEventListener("change", async () => {
        if (activeRequest) activeRequest.abort();

        reset(line, "-- Pilih Line --");
        reset(machine, "-- Pilih Machine --");

        line.disabled = true;
        machine.disabled = true;

        if (!product.value) return;

        await loadOptions(
            product.dataset.linesUrl,
            "productId",
            product.value,
            line
        );
    });

    line.addEventListener("change", async () => {
        if (activeRequest) activeRequest.abort();

        reset(machine, "-- Pilih Machine --");
        machine.disabled = true;

        if (!line.value) return;

        await loadOptions(
            line.dataset.machinesUrl,
            "lineId",
            line.value,
            machine
        );
    });

    // Kondisi awal Create
    product.disabled = !plant.value;
    line.disabled = !product.value;
    machine.disabled = !line.value;
});
