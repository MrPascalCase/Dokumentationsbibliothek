let observer;
let handler;

export function getWidth() {
    return window.innerWidth;
}

export function init(dotNetRef, elementToObserve) {
    // Create an IntersectionObserver for the element to observe. This lets us know when elementToObserve
    // comes into view. As elementToObserve is an end-of-page marker, we can use this information to
    // load more data to enable "infinite"-scrolling.
    observer = new IntersectionObserver(
        entries => {
            if (entries[0].isIntersecting) {
                dotNetRef.invokeMethodAsync('OnIntersection');
            }
        },
        {
            root: null,
            rootMargin: "200px",
            threshold: 0
        }
    );

    observer.observe(elementToObserve);

    // The "Results"-Component needs to adjust depending on screen width. Whenever the scree-width
    // changes we let the component know.
    handler = (() => {
        let timeout;
        return () => {
            clearTimeout(timeout);
            timeout = setTimeout(() => {
                dotNetRef.invokeMethodAsync("OnBrowserResize", window.innerWidth);
            }, 150);
        };
    })();

    window.addEventListener("resize", handler);
}

export function dispose() {
    if (observer) {
        observer.disconnect()
        observer = null;
    }

    if (handler) {
        window.removeEventListener("resize", handler);
        handler = null;
    }
}
