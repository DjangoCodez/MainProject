
export class NumberUtility {

    public static tryParseInt(value: string, defaultValue: number) {
        var retValue = defaultValue;
        if (value !== null) {
            if (value.length > 0) {
                if (!isNaN(<any>value)) {
                    retValue = parseInt(value);
                }
            }
        }
        return retValue;
    }

    public static parseDecimal(nbr: string): number {
        if (nbr) {
            // Remove whitespaces
            nbr = nbr.toString().replace(/\s+/g, "");
            // Replace , with .
            nbr = nbr.toString().replace(",", ".");
            //replace dash with minus.
            nbr = nbr.replace("−", "-");

            return Number(nbr) || 0;
        }

        return 0;
    }

    public static parseNumericDecimal(nbr: number): number {
        return this.parseDecimal(nbr.toString());
    }

    public static max(list: any[], property: string): number {
        var maxCountObj: any = {};

        if (list && list.length > 0) {
            maxCountObj = _.maxBy(list, function (x) {
                return x[property];
            });
        }

        if (maxCountObj[property])
            return parseInt(maxCountObj[property]);
        else
            return 0;
    }

    public static median(values: number[]): number {
        if (values.length === 0)
            return 0;

        values.sort(function (a, b) {
            return a - b;
        });

        let half = Math.floor(values.length / 2);

        if (values.length % 2)
            return values[half];

        return (values[half - 1] + values[half]) / 2.0;
    }

    public static printDecimal(nbr: number, fractionDigits?: number, maxFractionDigits?: number) {
        let options;
        if (!_.isNil(fractionDigits)) {
            options = { minimumFractionDigits: fractionDigits, maximumFractionDigits: maxFractionDigits && maxFractionDigits > fractionDigits ? maxFractionDigits : fractionDigits };
        }

        return nbr
            .toLocaleString("sv-SE", options)
            .replace("−", "-"); //replace dash with minus.
    }

    public static intersect(arr1: number[], arr2: number[]): number[] {
        if (!arr1 || !arr2)
            return [];

        let m = new Map();
        arr1.forEach(a => m.set(a, (m.get(a) || 0) + 1));
        return arr2.filter(a => m.get(a) && m.set(a, m.get(a) - 1));
    }

    public static compareArrays(a1: number[], a2: number[], sort: boolean = true): boolean {
        if (sort) {
            a1.sort((a, b) => a - b);
            a2.sort((a, b) => a - b);
        }

        return a1.length == a2.length && a1.every((element, index) => element === a2[index]);
    }
}
