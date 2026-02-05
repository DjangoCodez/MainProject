/* Number prototype extensions */

export class Dummy {
    // Exported to make file a module to make it easier to include in bundle...
}

declare global {
    interface Number {
        round: (places: number) => number;
        roundToNearest: (places: number) => number;
    }
}

Number.prototype.round = function (places: number) {
    return Number(this.toFixed(places));
}

Number.prototype.roundToNearest = function (decimals: number) {
    var p = Math.pow(10, decimals);
    var n = (this * p) * (1 + Number.EPSILON);
    return Math.round(n) / p;
}