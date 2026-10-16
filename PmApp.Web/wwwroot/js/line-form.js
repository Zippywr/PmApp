
document.addEventListener("DOMContentLoaded", () => {
    const plantSelect = document.getElementById("PlantId");
    const productSelect = document.getElementById("ProductId");

    if (!plantSelect || !productSelect) return;

    const productsUrl = plantSelect.dataset.productsUrl;

    let requestController = null;

    const resetProduct = (message = "-- Pilih Product --") => {
        productSelect.replaceChildren(new Option(message, ""));
        productSelect.value = "";
    };

    plantSelect.addEventListener("change", async () => {
        // Batalkan request sebelumnya jika Plant cepat diganti.
        if (requestController) {
            requestController.abort();
        }

        resetProduct();

        const plantId = plantSelect.value;

        if (!plantId) {
            productSelect.disabled = true;
            return;
        }

        requestController = new AbortController();

        productSelect.disabled = true;
        resetProduct("Memuat Product...");

        try {
            const url = new URL(productsUrl, window.location.origin);
            url.searchParams.set("plantId", plantId);

            const response = await fetch(url, {
                signal: requestController.signal,
                headers: {
                    "Accept": "application/json"
                }
            });

            if (!response.ok) {
                throw new Error("Gagal mengambil data Product.");
            }

            const products = await response.json();

            // Abaikan jika Plant berubah saat request berlangsung.
            if (plantSelect.value !== plantId) return;

            resetProduct();

            products.forEach(product => {
                const option = new Option(
                    `${product.code} - ${product.name}`,
                    product.id
                );

                productSelect.add(option);
            });

            if (products.length === 0) {
                resetProduct("Tidak ada Product pada Plant ini");
            }

        } catch (error) {
            if (error.name === "AbortError") return;

            console.error(error);

            if (plantSelect.value === plantId) {
                resetProduct("Gagal memuat Product");
            }
        } finally {
            if (plantSelect.value === plantId) {
                productSelect.disabled = false;
            }
        }
    });

    // Saat Create: belum ada Plant terpilih.
    // Saat Edit: ProductList sudah diisi oleh Controller.
    productSelect.disabled = !plantSelect.value;
});
