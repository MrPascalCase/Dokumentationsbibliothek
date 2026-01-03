let keyHandler;

export function init(dotNetRef, callbackMethodName) {
    dispose();
    keyHandler = function (e) {
        if (e.key === "Escape") {
            dotNetRef.invokeMethodAsync(callbackMethodName)
                .catch(() => { /* ignore if disposed */ });
            
            dispose();
        }
    };

    document.body.classList.add("overlay-open");
    document.addEventListener("keydown", keyHandler, true);
}

export function dispose() {
    document.body.classList.remove("overlay-open");
    if (keyHandler) {
        document.removeEventListener("keydown", keyHandler, true);
        keyHandler = null;
    }
}