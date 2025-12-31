window.windowSize = {
    getWidth: function () {
        return window.innerWidth;
    },
    registerResizeCallback: function (dotNetRef) {
        const handler = () => {
            dotNetRef.invokeMethodAsync("OnBrowserResize", window.innerWidth);
        };

        window.addEventListener("resize", handler);

        // store handler for cleanup
        window._blazorResizeHandler = handler;
    },
    unregisterResizeCallback: function () {
        if (window._blazorResizeHandler) {
            window.removeEventListener("resize", window._blazorResizeHandler);
            delete window._blazorResizeHandler;
        }
    }
};