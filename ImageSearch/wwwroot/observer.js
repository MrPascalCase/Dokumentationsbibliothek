export function registerObserver(component, element) {
    // console.log("register the observer...");
    // console.log(component);
    // console.log(element);
    
    let observer = new IntersectionObserver(e => {
        console.log("intersection detected.");
        component.invokeMethodAsync('OnIntersection');
    });

    observer.observe(element);
}