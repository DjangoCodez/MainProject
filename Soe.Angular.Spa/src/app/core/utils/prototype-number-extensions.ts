export default class PrototypeNumberExtensions {}

declare global {
  interface Number {
    round: (places: number) => number;
  }
}

Number.prototype.round = function (places: number) {
  return Number(this.toFixed(places));
};
