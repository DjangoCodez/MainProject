export class PercentFilter {

    private static filter(value: any, decimals: number, supressZero: boolean = false, multiplyValueByHundred: boolean = false) {
        if (!decimals && decimals !== 0) {
            decimals = 2;
        }

        if (!value) {
            if (supressZero)
                return '';

            value = 0;
        }

        if (multiplyValueByHundred)
            value *= 100;
        return (<number>value).toLocaleString(undefined, {
            minimumFractionDigits: decimals, maximumFractionDigits: decimals
        }) + '%';
    }

    public static create() {
        return PercentFilter.filter;
    }
}
