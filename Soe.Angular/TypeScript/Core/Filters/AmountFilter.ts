export class AmountFilter {

    private static filter(value: any, decimals: number, supressZero: boolean = false) {
        if (!decimals && decimals !== 0) {
            decimals = 2;
        }

        if (!value) {
            if (supressZero)
                return '';

            value = 0;
        }

        return (<number>value).toLocaleString(undefined, {
            minimumFractionDigits: decimals, maximumFractionDigits: decimals
        });
    }

    public static create() {
        return AmountFilter.filter;
    }
}
