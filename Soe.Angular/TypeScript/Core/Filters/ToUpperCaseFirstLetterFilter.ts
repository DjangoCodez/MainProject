export class ToUpperCaseFirstLetterFilter {

    private static filter(str: string) {
        return str.toUpperCaseFirstLetter();
    }

    public static create() {
        return ToUpperCaseFirstLetterFilter.filter;
    }
}
