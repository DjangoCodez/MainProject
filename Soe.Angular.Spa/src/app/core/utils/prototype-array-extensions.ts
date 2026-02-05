export default class PrototypeArrayExtensions {}

declare global {
  interface Array<T> {
    /**
     * Produces a new array where the item is added or removed
     */
    toggle(o: T): Array<T>;
  }
}

Array.prototype.toggle = function (o) {
  return this.some(x => x === o) ? this.filter(x => x !== o) : [...this, o];
};
