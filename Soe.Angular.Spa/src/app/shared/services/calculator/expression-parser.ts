import {
  Acronyms,
  CharacterUtility,
  Operator,
  Operators,
} from './calculation-utility';

export class ExpressionParser {
  private static cleanStatement(statement: string) {
    //For string cleaning.
    return statement.trim().toLowerCase().replaceAll(' ', '');
  }

  static cleanOperators(statement: string): string[] {
    //Making sure the statement matches our expectations of what it should look like.
    const chars = this.cleanStatement(statement).split('');

    let i = 1;
    while (i < chars.length) {
      let prevChar = chars[i - 1];
      let char = chars[i];

      if (prevChar === CharacterUtility.CHAR_HYPHEN) {
        prevChar = Operators.SUBTRACTION;
        chars[i - 1] = prevChar;
      }

      if (char === CharacterUtility.CHAR_HYPHEN) {
        char = Operators.SUBTRACTION;
        chars[i] = char;
      }

      if (char === Operators.ADDITION && prevChar === Operators.ADDITION) {
        //Rounded multiplication, a special operation
        chars.splice(i - 1, 2, Operators.ROUNDED_ADDITION);
      } else if (
        char === Operators.SUBTRACTION &&
        prevChar === Operators.SUBTRACTION
      ) {
        //Rounded multiplication, a special operation
        chars.splice(i - 1, 2, Operators.ROUNDED_SUBTRACTION);
      } else if (
        char === Operators.DIVISION &&
        prevChar === Operators.DIVISION
      ) {
        //Rounded multiplication, a special operation
        chars.splice(i - 1, 2, Operators.ROUNDED_DIVISION);
      } else if (
        char === Operators.MULTIPLICATION &&
        prevChar === Operators.MULTIPLICATION
      ) {
        //Rounded multiplication, a special operation
        chars.splice(i - 1, 2, Operators.ROUNDED_MULTIPLICATION);
      } else if (
        char === CharacterUtility.CHAR_START_PARENTHESIS &&
        !CharacterUtility.isOperator(prevChar) &&
        prevChar !== CharacterUtility.CHAR_START_PARENTHESIS
      ) {
        //Making sure we handle multiplications as default
        chars.splice(i, 0, Operators.MULTIPLICATION);
      } else if (
        prevChar === CharacterUtility.CHAR_CLOSE_PARENTHESIS &&
        !CharacterUtility.isOperator(char) &&
        char !== CharacterUtility.CHAR_CLOSE_PARENTHESIS
      ) {
        chars.splice(i, 0, Operators.MULTIPLICATION);
      } else if (char === Acronyms.ScientificExponential) {
        chars.splice(i, 1);
        CharacterUtility.SCIENTIFIC_EXPONENTIAL.forEach(v =>
          chars.splice(i++, 0, v)
        );
        chars.splice(i++, 0, '(');
        const j = i;
        while (
          chars[i] &&
          !CharacterUtility.isOperator(chars[i]) &&
          !CharacterUtility.isParenthesis(chars[i])
        ) {
          i++;
        }
        //Insert 1 if just
        if (i === j) chars.splice(i++, 0, '1');
        chars.splice(i++, 0, ')');
        i--;
      }

      i++;
    }
    return chars;
  }

  static getSortedOperators(statement: string[]): OperatorEntry[] {
    let depth = 0;
    const operators = [];

    for (let i = 0; i < statement.length; i++) {
      const char = statement[i];
      const depthFactor = depth * 10;

      if (char === CharacterUtility.CHAR_START_PARENTHESIS) {
        depth++;
      } else if (char === CharacterUtility.CHAR_CLOSE_PARENTHESIS) {
        depth--;
      } else if (CharacterUtility.isOperator(char)) {
        operators.push(
          new OperatorEntry(char, i, depthFactor + Operators.getPriority(char))
        );
      }
    }

    return operators.sort(OperatorEntry.compare);
  }
}

export class OperatorEntry {
  operation: Operator;
  index: number;
  priority: number;
  constructor(operator: Operator, index: number, priority: number) {
    this.operation = operator;
    this.index = index;
    this.priority = priority;
  }

  public static compare(self: OperatorEntry, other: OperatorEntry): number {
    //If the same priority, we go left to right.
    if (self.priority > other.priority) return -1;
    if (self.priority < other.priority) return 1;
    if (self.index < other.index) return -1;
    if (self.index > other.index) return 1;
    return 0;
  }
}
