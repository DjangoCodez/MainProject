
export class ElementHelper {
    public static appendConcatClasses(el: HTMLElement, concatClasses: string) {
        if (el && concatClasses) {
            _.forEach(concatClasses.split(' '), c => el.classList.add(c));
        }
    }
}
