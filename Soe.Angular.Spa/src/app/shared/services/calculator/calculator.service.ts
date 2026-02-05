import { Injectable } from '@angular/core';
import { CalculatedArea, CalculatedMap } from './calculated-area';
import { CharacterUtility, OperandFactory } from './calculation-utility';
import { ExpressionParser, OperatorEntry } from './expression-parser';

type TraversalResult = [CalculatedArea | undefined, string, number];

class Memory<T> {
  private readonly storage: T[];
  private readonly size: number;

  constructor(size: number) {
    this.storage = [];
    this.size = size;
  }

  public add(val: T) {
    const length = this.storage.unshift(val);
    if (length > this.size) this.storage.pop();
  }

  public get(position: number) {
    const index = position - 1;
    return this.storage[index];
  }
}

@Injectable({
  providedIn: 'root',
})
export class CalculatorService {
  map!: CalculatedMap;
  history: Memory<number> = new Memory(9);

  public calculate(statement: string): number {
    const val = this.doCalculate(statement);
    if (!isNaN(val) && Math.abs(val) !== Infinity) {
      this.history.add(val);
      return val;
    }
    return NaN;
  }

  private doCalculate(statement: string) {
    const parsedStatement = ExpressionParser.cleanOperators(statement);

    //To prevent unnecessary extensive operations we first see if the statement is calculable.
    const operand = this.createOperand(parsedStatement.join(''));
    if (!operand.isNaN()) return operand.get();

    const operators = ExpressionParser.getSortedOperators(parsedStatement);
    return this.performCalculation(operators, parsedStatement);
  }

  private createOperand(value: string) {
    const operand = OperandFactory.create(value);
    if (operand.isMemory()) {
      //Swap index for value
      operand.setValueFromMem(this.history.get(operand.get()));
    }
    return operand;
  }

  private performCalculation(operators: OperatorEntry[], statement: string[]) {
    /**
     *  We perform the operations based on operator priority and then LTR.
     *  Each evaluated part of the statement occupies that part of the map (i.e. the statement),
     */

    this.map = new CalculatedMap();
    for (const operatorEntry of operators) {
      const center = operatorEntry.index;
      const operator = operatorEntry.operation;
      const [leftEv, leftText, leftBound] = this.traverseLeft(
        operatorEntry.index,
        statement
      );
      const [rightEv, rightText, rightBound] = this.traverseRight(
        operatorEntry.index,
        statement
      );

      const leftArea =
        leftEv ||
        new CalculatedArea(this.createOperand(leftText), leftBound, center);
      const rightArea =
        rightEv ||
        new CalculatedArea(this.createOperand(rightText), center, rightBound);

      this.map.merge(operator, leftArea, rightArea);
    }
    return this.map.getCalculatedValue();
  }

  private traverseLeft(
    operatorIndex: number,
    statement: string[]
  ): TraversalResult {
    let i = operatorIndex - 1;
    if (this.map.isCalculated(i)) {
      return [this.map.getArea(i), '', i];
    } else {
      while (statement[i] && !CharacterUtility.isOperator(statement[i])) {
        i--;
      }
      i++;
      const leftValue = statement
        .slice(i, operatorIndex)
        .join('')
        .replaceAll('(', '')
        .replaceAll(')', '');
      return [undefined, leftValue, i];
    }
  }

  private traverseRight(
    operatorIndex: number,
    statement: string[]
  ): TraversalResult {
    //Traverse right to get the value before the next operator.
    let i = operatorIndex + 1;
    if (this.map.isCalculated(i)) {
      //Check if right area has already been calculated.
      return [this.map.getArea(i), '', i];
    } else {
      while (statement[i] && !CharacterUtility.isOperator(statement[i])) {
        i++;
      }
      //i++;
      const rightValue = statement
        .slice(operatorIndex + 1, i)
        .join('')
        .replaceAll('(', '')
        .replaceAll(')', '');
      if (!CharacterUtility.isParenthesis(statement[i])) i--;
      return [undefined, rightValue, i];
    }
  }
}
