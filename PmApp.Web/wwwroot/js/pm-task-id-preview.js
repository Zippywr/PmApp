
document.addEventListener("DOMContentLoaded", () => {
    const machine = document.getElementById("AssetId");
    const sequence = document.getElementById("TaskSequence");

    const preview = document.getElementById("idPmPreview");
    const status = document.getElementById(
        "idPmPreviewStatus"
    );

    if (!machine || !sequence || !preview || !status) {
        return;
    }

    const previewUrl =
        document.getElementById("pmIdPreviewUrl")?.value;

    if (!previewUrl) {
        console.error("URL preview ID-PM tidak ditemukan.");
        return;
    }

    let controller = null;
    let debounceTimer = null;

    async function updatePreview() {
        if (controller) controller.abort();

        preview.textContent = "------";

        if (!machine.value) {
            status.textContent =
                "Pilih mesin terlebih dahulu.";
            return;
        }

        const manualText = sequence.value.trim();

        if (manualText !== "") {
            const n = Number(manualText);

            if (!Number.isInteger(n) || n < 1 || n > 99) {
                status.textContent =
                    "Task Sequence harus antara 01–99.";
                return;
            }
        }

        const currentController = new AbortController();
        controller = currentController;

        status.textContent = "Menghitung ID-PM...";

        try {
            const url = new URL(
                previewUrl,
                window.location.origin
            );

            url.searchParams.set(
                "assetId",
                machine.value
            );

            if (manualText !== "") {
                url.searchParams.set(
                    "taskSequence",
                    manualText
                );
            }

            const response = await fetch(url, {
                signal: currentController.signal,
                headers: {
                    "Accept": "application/json"
                }
            });

            const result = await response.json();

            if (currentController.signal.aborted) return;

            if (!response.ok || !result.success) {
                preview.textContent = "------";
                status.textContent =
                    result.message ?? "ID-PM tidak tersedia.";
                return;
            }

            preview.textContent = result.idPm;
            status.textContent =
                `Mode ${result.mode} — Preview sementara`;

        } catch (error) {
            if (error.name === "AbortError") return;

            preview.textContent = "------";
            status.textContent =
                "Gagal mengambil preview ID-PM.";

            console.error("Preview ID-PM:", error);
        }
    }

    function schedulePreview() {
        clearTimeout(debounceTimer);

        if (controller) controller.abort();

        debounceTimer = setTimeout(updatePreview, 250);
    }

    machine.addEventListener("change", schedulePreview);
    sequence.addEventListener("input", schedulePreview);

    if (machine.value) schedulePreview();
});
