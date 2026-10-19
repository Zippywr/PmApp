$(function () {
    const $unit = $("#UnitName");
    const $sub = $("#SubUnitName");
    if (!$unit.length || !$sub.length || !$.fn.select2) return;
    const opts = {
        tags: true, width: "100%", minimumResultsForSearch: 0,
        placeholder: "-- Pilih atau ketik baru --", allowClear: true
    };
    $unit.select2(opts);
    $sub.select2(opts);
    let active = null;
    $unit.on("change", async function () {
        if (active) active.abort();
        const name = ($unit.val() || "").trim();
        $sub.empty().append(new Option("-- Pilih atau ketik Sub Unit --", ""))
            .val("").trigger("change.select2");
        if (!name) return;
        const ctrl = new AbortController(); active = ctrl;
        try {
            const url = new URL($unit.data("subunits-url"), location.origin);
            url.searchParams.set("unitName", name);
            const response = await fetch(url, { signal: ctrl.signal });
            if (!response.ok) throw new Error("HTTP " + response.status);
            const names = await response.json();
            if (ctrl.signal.aborted) return;
            names.forEach(n => $sub.append(new Option(n, n)));
            $sub.trigger("change.select2");
        } catch (e) {
            if (e.name !== "AbortError") console.error("Sub Unit:", e);
        }
    });
});
