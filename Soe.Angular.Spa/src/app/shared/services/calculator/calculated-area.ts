import { Operand, Operator } from './calculation-utility';
type IndexMap = { [index: number]: CalculatedArea };

export class CalculatedMap {
  private map: IndexMap = {};
  merge(op: Operator, left: CalculatedArea, right: CalculatedArea) {
    //Left area will always swallow the right area.
    left.calculate(op, right);
    delete this.map[left.endIndex];
    delete this.map[right.startIndex];
    this.map[left.startIndex] = left;
    this.map[left.endIndex] = left;
  }

  isCalculated(index: number) {
    return !!this.map[index];
  }

  getArea(index: number) {
    return this.map[index];
  }

  getCalculatedValue() {
    if (this.map[0]) return this.map[0].value.get();
    return NaN;
  }
}

export class CalculatedArea {
  value: Operand;
  startIndex: number;
  endIndex: number;

  constructor(value: Operand, start: number, end: number) {
    this.value = value;
    this.startIndex = start;
    this.endIndex = end;
  }

  public calculate(operator: Operator, other: CalculatedArea) {
    this.endIndex = other.endIndex;
    this.value.calculate(operator, other.value);
  }
}
