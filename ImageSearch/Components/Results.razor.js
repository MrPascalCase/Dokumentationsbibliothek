let Observer;
let Handler;
let DotNet;

export function getWidth() {
    return window.innerWidth;
}

export function init(dotNet) {
    DotNet = dotNet;

    // The "Results"-Component needs to adjust depending on screen width. Whenever the scree-width
    // changes we let the component know.
    Handler = (() => {
        let timeout;
        return () => {
            clearTimeout(timeout);
            timeout = setTimeout(() => {
                dotNet.invokeMethodAsync("OnBrowserResize", window.innerWidth);
            }, 150);
        };
    })();

    window.addEventListener("resize", Handler);
}

export function observe(endMarker, payload) {
    if (endMarker === null) {
        // The endmarker is null when this script is run before the first render (which does happen). 
        return;
    }

    Observer?.disconnect();

    // Create an IntersectionObserver for the element to observe. This lets us know when elementToObserve
    // comes into view. As elementToObserve is an end-of-page marker, we can use this information to
    // load more data to enable "infinite"-scrolling.
    Observer = new IntersectionObserver(
        entries => {
            if (entries[0].isIntersecting) {
                DotNet.invokeMethodAsync('OnIntersection', payload);
            }
        },
        {
            root: null,
            rootMargin: "400px",
            threshold: 0,
        }
    );

    Observer.observe(endMarker);
}

export function dispose() {
    if (Observer) {
        Observer.disconnect()
        Observer = null;
    }

    if (Handler) {
        window.removeEventListener("resize", Handler);
        Handler = null;
    }
}
