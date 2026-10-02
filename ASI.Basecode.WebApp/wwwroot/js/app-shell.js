(() => {
    const toggle = document.getElementById("sidebar-toggle");
    const sidebar = document.getElementById("app-sidebar");
    const backdrop = document.getElementById("sidebar-backdrop");

    if (!toggle || !sidebar || !backdrop) return;

    const setOpen = (open) => {
        toggle.setAttribute("aria-expanded", String(open));
        toggle.setAttribute("aria-label", open ? "Close navigation" : "Open navigation");
        sidebar.classList.toggle("-translate-x-full", !open);
        sidebar.inert = !open && window.innerWidth < 1024;
        backdrop.classList.toggle("hidden", !open);
        document.body.classList.toggle("overflow-hidden", open);
    };

    toggle.addEventListener("click", () => setOpen(toggle.getAttribute("aria-expanded") !== "true"));
    backdrop.addEventListener("click", () => setOpen(false));
    document.addEventListener("keydown", (event) => {
        if (event.key === "Escape") setOpen(false);
    });
    let lastWidth = window.innerWidth;
    window.addEventListener("resize", () => {
        const width = window.innerWidth;
        if (width >= 1024 || (lastWidth >= 1024 && width < 1024)) {
            setOpen(false);
        }
        lastWidth = width;
    });

    setOpen(false);
})();
