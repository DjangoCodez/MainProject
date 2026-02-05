export enum Acronyms {
  Thousand = 'k',
  Million = 'm',
  Percentage = '%',
  ScientificExponential = 'e',
  History = 'h',
  Factorial = '!',
}

export type Operator =
  | typeof Operators.ADDITION
  | typeof Operators.SUBTRACTION
  | typeof Operators.MULTIPLICATION
  | typeof Operators.DIVISION
  | typeof Operators.EXPONENTIATION
  | typeof Operators.ROUNDED_ADDITION
  | typeof Operators.ROUNDED_SUBTRACTION
  | typeof Operators.ROUNDED_MULTIPLICATION
  | typeof Operators.ROUNDED_DIVISION;

export class Operators {
  public static readonly ADDITION = '+' as const;
  public static readonly SUBTRACTION = '−' as const;
  public static readonly MULTIPLICATION = '*' as const;
  public static readonly DIVISION = '/' as const;
  public static readonly EXPONENTIATION = '^' as const;
  public static readonly ROUNDED_ADDITION = ':' as const;
  public static readonly ROUNDED_SUBTRACTION = ';' as const;
  public static readonly ROUNDED_MULTIPLICATION = '<' as const;
  public static readonly ROUNDED_DIVISION = '>' as const;
  public static readonly OPERATOR_SET = new Set<string>([
    Operators.ADDITION,
    Operators.SUBTRACTION,
    Operators.MULTIPLICATION,
    Operators.DIVISION,
    Operators.EXPONENTIATION,
    Operators.ROUNDED_ADDITION,
    Operators.ROUNDED_SUBTRACTION,
    Operators.ROUNDED_MULTIPLICATION,
    Operators.ROUNDED_DIVISION,
  ]);

  public static performOperation(
    operator: Operator,
    left: number,
    right: number
  ): number {
    switch (operator) {
      case Operators.SUBTRACTION:
        return left - right;
      case Operators.ROUNDED_SUBTRACTION:
        return Math.round(left - right);
      case Operators.ADDITION:
        return left + right;
      case Operators.ROUNDED_ADDITION:
        return Math.round(left + right);
      case Operators.MULTIPLICATION:
        return left * right;
      case Operators.ROUNDED_MULTIPLICATION:
        return Math.round(left * right);
      case Operators.DIVISION:
        return left / right;
      case Operators.ROUNDED_DIVISION:
        return Math.round(left / right);
      case Operators.EXPONENTIATION:
        return left ** right;
    }
    throw new Error('Invalid operator');
  }

  static getPriority(operator: Operator) {
    /**
     * Depth corresponds to parenthesis level, e.g.
     * in "1 + (5 + (1 + 1))" the second last "+" has a
     * higher priority than then previous two.
     *
     * We perform operations based on common rules such as
     * Exp. > Multi. / Divis. > Add. / Sub.
     *
     * Higher priority is more prioritized.
     */
    switch (operator) {
      case Operators.EXPONENTIATION:
        return 3;
      case Operators.MULTIPLICATION:
      case Operators.DIVISION:
      case Operators.ROUNDED_MULTIPLICATION:
      case Operators.ROUNDED_DIVISION:
        return 2;
      case Operators.SUBTRACTION:
      case Operators.ADDITION:
      case Operators.ROUNDED_SUBTRACTION:
      case Operators.ROUNDED_ADDITION:
        return 1;
      default:
        throw new Error('Unknown operator!');
    }
  }
}

export class CharacterUtility {
  public static readonly CHAR_HYPHEN = '-';
  public static readonly CHAR_MINUS = '−';
  public static readonly CHAR_PUNCTUATION = '.';
  public static readonly CHAR_START_PARENTHESIS = '(';
  public static readonly CHAR_CLOSE_PARENTHESIS = ')';
  public static readonly SCIENTIFIC_EXPONENTIAL = ['*', '1', '0', '^'];

  public static isOperator(value: string): value is Operator {
    return Operators.OPERATOR_SET.has(value);
  }

  public static isPlusMinus(
    value: string
  ): value is typeof Operators.SUBTRACTION | typeof Operators.ADDITION {
    return value === Operators.SUBTRACTION || value === Operators.ADDITION;
  }

  public static isParenthesis(value: string) {
    return (
      value === CharacterUtility.CHAR_START_PARENTHESIS ||
      value == CharacterUtility.CHAR_CLOSE_PARENTHESIS
    );
  }

  public static isNumerical(value: string) {
    return (
      !CharacterUtility.isParenthesis(value) &&
      !CharacterUtility.isOperator(value)
    );
  }
}

export class OperandFactory {
  public static create(value: string): Operand {
    const number = Number(value);
    if (!isNaN(number)) return new Operand(number, OperandType.Ordinary);
    return OperandFactory.acronymFactory(value);
  }
  public static acronymFactory(operand: string): Operand {
    if (operand.endsWith(Acronyms.Percentage)) {
      const value = this.parseExSuffix(operand) / 100;
      return this.Percentage(value);
    }
    if (operand.endsWith(Acronyms.Thousand)) {
      const value = this.parseExSuffix(operand) * 1_000;
      return this.Ordinary(value);
    }
    if (operand.endsWith(Acronyms.Million)) {
      const value = this.parseExSuffix(operand) * 1_000_000;
      return this.Ordinary(value);
    }
    if (operand.endsWith(Acronyms.Factorial)) {
      const value = this.parseExSuffix(operand);
      if (isNaN(value)) return this.NaN();
      let sum = 1;
      for (let i = 1; i <= value; i++) {
        sum *= i;
      }
      return this.Ordinary(sum);
    }
    if (operand.startsWith(Acronyms.History)) {
      return new Operand(
        this.parseExPrefix(operand),
        OperandType.MemoryHistory
      );
    }

    return this.NaN();
  }

  public static parseExSuffix(value: string) {
    return this.parseNumber(value.substring(0, value.length - 1));
  }
  public static parseExPrefix(value: string) {
    return this.parseNumber(value.substring(1, value.length));
  }
  public static NaN() {
    return new Operand(NaN, OperandType.Ordinary);
  }

  public static Ordinary(value: number) {
    return new Operand(value, OperandType.Ordinary);
  }

  public static Percentage(value: number) {
    return new Operand(value, OperandType.Percentage);
  }

  public static parseNumber(value: string) {
    return Number(value);
  }
}

enum OperandType {
  Ordinary,
  Percentage,
  MemoryHistory,
}

export class Operand {
  private type: OperandType;
  private value: number;

  constructor(value: number, type: OperandType) {
    this.value = value;
    this.type = type;
  }

  replaceSubAddForPercentage(op: Operator, other: Operand) {
    const [percentage, value] = this.isPercentage()
      ? [this.value, other.value]
      : [other.value, this.value];
    const percentageAdjusted =
      op === Operators.ADDITION ? 1 + percentage : 1 - percentage;
    return Operators.performOperation(
      Operators.MULTIPLICATION,
      percentageAdjusted,
      value
    );
  }

  calculate(op: Operator, other: Operand) {
    /**
     * Here we perform the calculation in place.
     */

    if (this.isNaN() || other.isNaN()) {
      this.value = NaN;
    } else if (this.type === other.type) {
      this.value = Operators.performOperation(op, this.value, other.value);
    } else if (op === Operators.ADDITION || op === Operators.SUBTRACTION) {
      this.value = this.replaceSubAddForPercentage(op, other);
      this.type = OperandType.Ordinary;
    } else {
      this.value = Operators.performOperation(op, this.value, other.value);
      this.type = OperandType.Ordinary;
    }
  }

  get() {
    return this.value;
  }

  set(value: number) {
    this.value = value;
  }

  setValueFromMem(value: number) {
    this.value = value;
    this.type = OperandType.Ordinary;
  }

  isOrdinary() {
    return this.type === OperandType.Ordinary && !this.isNaN();
  }

  isMemory() {
    return this.type === OperandType.MemoryHistory;
  }

  isPercentage() {
    return this.type === OperandType.Percentage;
  }

  isNaN() {
    return isNaN(this.value);
  }
}
