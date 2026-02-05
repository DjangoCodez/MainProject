import { maxBy } from 'lodash';
import { SoeConfigUtil } from './soeconfig-util';

export class NumberUtil {
  // PARSE
  public static tryParseInt(value: string, defaultValue: number) {
    let retValue = defaultValue;
    if (value !== null) {
      if (value.length > 0) {
        if (!isNaN(<any>value)) {
          retValue = parseInt(value);
        }
      }
    }
    return retValue;
  }

  static parseNumberByCurrentUserLanguage(str: string): number {
    const locale = SoeConfigUtil.language;
    const nf = new Intl.NumberFormat(locale);
    const parts = nf.formatToParts(-12345.6);
    const minus = parts.find(p => p.type === 'minusSign')?.value ?? '-';
    const group = parts.find(p => p.type === 'group')?.value ?? ',';
    const decimal = parts.find(p => p.type === 'decimal')?.value ?? '.';

    let s = str.trim();

    // Escapes a literal string for use inside a RegExp
    let escRe = (lit: string)  => {
      return lit.replace(/[-\/\\^$*+?.()|[\]{}]/g, '\\$&');
    }

    // Normalize minus variants to ASCII hyphen-minus (what Number() accepts)
    s = s
      .replace(/\u2212/g, '-')                // true minus ? hyphen-minus
      .replace(new RegExp(escRe(minus), 'g'), '-');

    // Remove common spacing used as group separators (incl. NBSP/NNBSP/thin space)
    s = s.replace(/\s/g, '');

    // Remove locale group separator if it�s not a space-like char already handled
    if (group && !/\s/.test(group)) {
      s = s.replace(new RegExp(escRe(group), 'g'), '');
    }

    // Replace locale decimal with dot
    if (decimal !== '.') {
      s = s.replace(new RegExp(escRe(decimal), 'g'), '.');
    }

    const n = Number(s);
    return Number.isNaN(n) ? NaN : n;
  }

  /**
   * Does necessary replacements to prepare a number string for calculation. Calculator expects:
   * - no spaces
   * - dots as decimals 
   * @param value
   * @returns
   */

  static prepareCalculationExpression(value: string): string {
    if (!value) return value;

    const language = SoeConfigUtil.language;
    let cleaned = value.replace(/\s/g, '');

    if (
      language.startsWith('fi-') ||
      language.startsWith('sv-') ||
      language.startsWith('da-') ||
      language.startsWith('nb-') ||
      language.startsWith('no-')
    ) {
      cleaned = cleaned.replaceAll(',', '.');
    } else if (language.startsWith('en-') || language.startsWith('en-US')) {
      cleaned = cleaned.replace(/,(?=\d{3}(?!\d))/g, '');
    } else {
      console.warn(`NumberUtil: Unsupported language ${language}, defaulting to sv-SE parsing.`);
      cleaned = cleaned.replaceAll(',', '.');
    }

    return cleaned;
  }

  // BUG: This will fail if english number format is provided (e.g. "1,234.56").
  static parseDecimal(value: string): number {
    if (value) {
      // Remove whitespaces
      value = value.toString().removeWhitespaces();
      // Replace , with .
      value = value.toString().replaceCommaWithDot();
      // Replace dash with minus
      value = value.replaceDashWithMinus();

      return Number(value) || 0;
    }

    return 0;
  }

  // FORMAT
  static formatDecimal(
    nbr: number,
    fractionDigits?: number,
    maxFractionDigits?: number
  ): string {
    let options;
    if (fractionDigits !== undefined && fractionDigits !== null) {
      options = {
        minimumFractionDigits: fractionDigits,
        maximumFractionDigits:
          maxFractionDigits && maxFractionDigits > fractionDigits!
            ? maxFractionDigits
            : fractionDigits,
      };
    }

    return nbr
      .toLocaleString(SoeConfigUtil.language, options)
      .replaceDashWithMinus();
  }

  // FIXME: This method does not properly handle properties that do not exist.
  static max(list: any[], property: string): number {
    let maxCountObj: any = {};

    // If the property does not exist for any object then maxCountObject will be undefined...
    if (list && list.length > 0) {
      maxCountObj = maxBy(list, function (x) {
        return x[property];
      });
    }

    // ...and then this will throw an error, not return 0 as intended.
    if (maxCountObj[property]) return parseInt(maxCountObj[property]);
    else return 0;
  }

  static padZeroLen2(val: string | number) {
    if (typeof val === 'number') val = '' + val;

    if (val.length < 2) {
      val = '0' + val;
    }
    return val;
  }

  static median(values: number[]): number {
    if (values.length === 0) return 0;

    values.sort((a, b) => a - b);

    const half = Math.floor(values.length / 2);

    if (values.length % 2) return values[half];

    return (values[half - 1] + values[half]) / 2.0;
  }

  static round(value: number, decimals: number): number {
    const power = Math.pow(10, decimals);
    return Math.round((value + Number.EPSILON) * power) / power;
  }

  static compareArrays(arr1: number[], arr2: number[]): boolean {
    arr1.sort((a, b) => a - b);
    arr2.sort((a, b) => a - b);
    return arr1 + '' === arr2 + '';
  }
}
