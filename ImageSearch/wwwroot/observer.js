export function registerObserver (component, targetId)
{
    let observer = new IntersectionObserver(e => {
        component.invokeMethodAsync('OnIntersection');
    });

    let element = document.getElementById(targetId);
    if (element == null) throw new Error("The observable target was not found");
    observer.observe(element);
}