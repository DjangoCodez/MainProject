export class AbsFilter {

    private static filter(value: any) {
        if (!value)
            value = 0;

        return Math.abs(<number>value);
    }

    public static create() {
        return AbsFilter.filter;
    }
}
