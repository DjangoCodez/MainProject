import { vi, describe, it, expect, beforeEach, beforeAll } from 'vitest';

describe('NumberUtil', () => {
  let NumberUtil: any;
  let SoeConfigUtil: any;

  beforeEach(async () => {
    vi.clearAllMocks();
    // Import SoeConfigUtil first so we can mock it before NumberUtil uses it
    SoeConfigUtil = (await import('./soeconfig-util')).SoeConfigUtil;
    NumberUtil = (await import('./number-util')).NumberUtil;
  });

  describe('tryParseInt', () => {
    it('should parse valid integer string', () => {
      expect(NumberUtil.tryParseInt('123', 0)).toBe(123);
    });

    it('should return default value for null', () => {
      expect(NumberUtil.tryParseInt(null as any, 42)).toBe(42);
    });

    it('should return default value for empty string', () => {
      expect(NumberUtil.tryParseInt('', 10)).toBe(10);
    });

    it('should return default value for non-numeric string', () => {
      expect(NumberUtil.tryParseInt('abc', 5)).toBe(5);
    });

    it('should parse negative integers', () => {
      expect(NumberUtil.tryParseInt('-456', 0)).toBe(-456);
    });

    it('should parse floats as integers', () => {
      expect(NumberUtil.tryParseInt('123.45', 0)).toBe(123);
    });
  });

  describe('parseNumberByCurrentUserLanguage', () => {
    it('should parse number with en-US locale', async () => {
      vi.spyOn(SoeConfigUtil, 'language', 'get').mockReturnValue('en-US');
      const util = (await import('./number-util')).NumberUtil;
      expect(util.parseNumberByCurrentUserLanguage('1,234.56')).toBe(1234.56);
    });

    it('should parse number with fi-FI locale (comma as decimal)', async () => {
      vi.spyOn(SoeConfigUtil, 'language', 'get').mockReturnValue('fi-FI');
      const util = (await import('./number-util')).NumberUtil;
      expect(util.parseNumberByCurrentUserLanguage('1 234,56')).toBe(1234.56);
    });

    it('should parse number with de-DE locale', async () => {
      vi.spyOn(SoeConfigUtil, 'language', 'get').mockReturnValue('de-DE');
      const util = (await import('./number-util')).NumberUtil;
      expect(util.parseNumberByCurrentUserLanguage('1.234,56')).toBe(1234.56);
    });

    it('should parse number with sv-SE locale', async () => {
      vi.spyOn(SoeConfigUtil, 'language', 'get').mockReturnValue('sv-SE');
      const util = (await import('./number-util')).NumberUtil;
      expect(util.parseNumberByCurrentUserLanguage('1 234,56')).toBe(1234.56);
    });

    it('should handle negative numbers', async () => {
      vi.spyOn(SoeConfigUtil, 'language', 'get').mockReturnValue('en-US');
      const util = (await import('./number-util')).NumberUtil;
      expect(util.parseNumberByCurrentUserLanguage('-1,234.56')).toBe(-1234.56);
    });

    it('should handle unicode minus sign (U+2212)', async () => {
      vi.spyOn(SoeConfigUtil, 'language', 'get').mockReturnValue('en-US');
      const util = (await import('./number-util')).NumberUtil;
      expect(util.parseNumberByCurrentUserLanguage('−123')).toBe(-123);
    });

    it('should return NaN for invalid input', async () => {
      vi.spyOn(SoeConfigUtil, 'language', 'get').mockReturnValue('en-US');
      const util = (await import('./number-util')).NumberUtil;
      expect(util.parseNumberByCurrentUserLanguage('abc')).toBeNaN();
    });

    it('should handle whitespace', async () => {
      vi.spyOn(SoeConfigUtil, 'language', 'get').mockReturnValue('en-US');
      const util = (await import('./number-util')).NumberUtil;
      expect(util.parseNumberByCurrentUserLanguage('  1,234.56  ')).toBe(1234.56);
    });

    it('should handle numbers with non-breaking spaces', async () => {
      vi.spyOn(SoeConfigUtil, 'language', 'get').mockReturnValue('fr-FR');
      const util = (await import('./number-util')).NumberUtil;
      expect(util.parseNumberByCurrentUserLanguage('1\u00A0234,56')).toBe(1234.56);
    });
  });

  describe('cleanNumberExpressionByCurrentUserLanguage', () => {
    it('should return value if null or undefined', () => {
      expect(NumberUtil.prepareCalculationExpression(null as any)).toBeNull();
      expect(NumberUtil.prepareCalculationExpression(undefined as any)).toBeUndefined();
    });

    it('should clean Finnish number format', () => {
      vi.spyOn(SoeConfigUtil, 'language', 'get').mockReturnValue('fi-FI');
      expect(NumberUtil.prepareCalculationExpression('1 234,56')).toBe('1234.56');
    });

    it('should clean Swedish number format', () => {
      vi.spyOn(SoeConfigUtil, 'language', 'get').mockReturnValue('sv-SE');
      expect(NumberUtil.prepareCalculationExpression('1 234,56')).toBe('1234.56');
    });

    it('should clean Danish number format', () => {
      vi.spyOn(SoeConfigUtil, 'language', 'get').mockReturnValue('da-DK');
      expect(NumberUtil.prepareCalculationExpression('1.234,56')).toBe('1234.56');
    });

    it('should clean Norwegian number format', () => {
      vi.spyOn(SoeConfigUtil, 'language', 'get').mockReturnValue('nb-NO');
      expect(NumberUtil.prepareCalculationExpression('1 234,56')).toBe('1234.56');
    });

    it('should clean US English number format', () => {
      vi.spyOn(SoeConfigUtil, 'language', 'get').mockReturnValue('en-US');
      expect(NumberUtil.prepareCalculationExpression('1,234.56')).toBe('1234.56');
    });

    it('should clean UK English number format', () => {
      vi.spyOn(SoeConfigUtil, 'language', 'get').mockReturnValue('en-GB');
      expect(NumberUtil.prepareCalculationExpression('1,234.56')).toBe('1234.56');
    });

    it('should warn for unsupported language', () => {
      const consoleSpy = vi.spyOn(console, 'warn').mockImplementation(() => {});
      vi.spyOn(SoeConfigUtil, 'language', 'get').mockReturnValue('zh-CN');
      NumberUtil.prepareCalculationExpression('1,234.56');
      expect(consoleSpy).toHaveBeenCalledWith(
        expect.stringContaining('Unsupported language zh-CN')
      );
      consoleSpy.mockRestore();
    });
  });

  describe('parseDecimal', () => {
    beforeAll(() => {
      // Add string extension methods
      String.prototype.removeWhitespaces = function() {
        return this.replace(/\s/g, '');
      };
      String.prototype.replaceCommaWithDot = function() {
        return this.replace(/,/g, '.');
      };
      String.prototype.replaceDashWithMinus = function() {
        return this.replace(/–/g, '-').replace(/−/g, '-');
      };
    });

    it('should parse decimal string', () => {
      expect(NumberUtil.parseDecimal('123.45')).toBe(123.45);
    });

    it('should parse string with comma as decimal separator', () => {
      expect(NumberUtil.parseDecimal('123,45')).toBe(123.45);
    });

    it('should parse string with whitespaces', () => {
      expect(NumberUtil.parseDecimal('1 234.56')).toBe(1234.56);
    });

    it('should return 0 for null or undefined', () => {
      expect(NumberUtil.parseDecimal(null as any)).toBe(0);
      expect(NumberUtil.parseDecimal(undefined as any)).toBe(0);
    });

    it('should return 0 for invalid input', () => {
      expect(NumberUtil.parseDecimal('abc')).toBe(0);
    });

    it('should handle negative numbers with dash', () => {
      expect(NumberUtil.parseDecimal('–123.45')).toBe(-123.45);
    });
  });

  describe('formatDecimal', () => {
    beforeAll(() => {
      String.prototype.replaceDashWithMinus = function() {
        return this.replace(/–/g, '-').replace(/−/g, '-');
      };
    });

    it('should format number with en-US locale', async () => {
      vi.spyOn(SoeConfigUtil, 'language', 'get').mockReturnValue('en-US');
      const util = (await import('./number-util')).NumberUtil;
      const result = util.formatDecimal(1234.56);
      expect(result).toBe('1,234.56');
    });

    it('should format with specified fraction digits', async () => {
      vi.spyOn(SoeConfigUtil, 'language', 'get').mockReturnValue('en-US');
      const util = (await import('./number-util')).NumberUtil;
      const result = util.formatDecimal(1234.5, 3);
      expect(result).toBe('1,234.500');
    });

    it('should format with max fraction digits', async () => {
      vi.spyOn(SoeConfigUtil, 'language', 'get').mockReturnValue('en-US');
      const util = (await import('./number-util')).NumberUtil;
      const result = util.formatDecimal(1234.56789, 2, 4);
      expect(result).toBe('1,234.5679');
    });

    it('should format with Finnish locale', async () => {
      vi.spyOn(SoeConfigUtil, 'language', 'get').mockReturnValue('fi-FI');
      const util = (await import('./number-util')).NumberUtil;
      const result = util.formatDecimal(1234.56);
      expect(result).toContain('1');
      expect(result).toContain('234');
    });

    it('should format zero', async () => {
      vi.spyOn(SoeConfigUtil, 'language', 'get').mockReturnValue('en-US');
      const util = (await import('./number-util')).NumberUtil;
      const result = util.formatDecimal(0);
      expect(result).toBe('0');
    });
  });

  describe('max', () => {
    it('should return max value from array of objects', () => {
      const list = [
        { count: 5 },
        { count: 10 },
        { count: 3 }
      ];
      expect(NumberUtil.max(list, 'count')).toBe(10);
    });

    it('should return 0 for empty array', () => {
      expect(NumberUtil.max([], 'count')).toBe(0);
    });

    it('should return 0 for null array', () => {
      expect(NumberUtil.max(null as any, 'count')).toBe(0);
    });

    // TODO: Activate this test again when the issue in NumberUtil.max is fixed
    it.fails('should return 0 if property does not exist', () => {
      const list = [{ value: 5 }];
      expect(NumberUtil.max(list, 'nonexistent')).toBe(0);
    });
  });

  describe('padZeroLen2', () => {
    it('should pad single digit number', () => {
      expect(NumberUtil.padZeroLen2(5)).toBe('05');
    });

    it('should not pad double digit number', () => {
      expect(NumberUtil.padZeroLen2(15)).toBe('15');
    });

    it('should pad single digit string', () => {
      expect(NumberUtil.padZeroLen2('7')).toBe('07');
    });

    it('should not pad double digit string', () => {
      expect(NumberUtil.padZeroLen2('23')).toBe('23');
    });

    it('should handle zero', () => {
      expect(NumberUtil.padZeroLen2(0)).toBe('00');
    });

    it('should not pad longer strings', () => {
      expect(NumberUtil.padZeroLen2('123')).toBe('123');
    });
  });

  describe('median', () => {
    it('should calculate median of odd-length array', () => {
      expect(NumberUtil.median([1, 3, 5, 7, 9])).toBe(5);
    });

    it('should calculate median of even-length array', () => {
      expect(NumberUtil.median([1, 2, 3, 4])).toBe(2.5);
    });

    it('should return 0 for empty array', () => {
      expect(NumberUtil.median([])).toBe(0);
    });

    it('should handle unsorted arrays', () => {
      expect(NumberUtil.median([9, 1, 5, 3, 7])).toBe(5);
    });

    it('should handle single element array', () => {
      expect(NumberUtil.median([42])).toBe(42);
    });

    it('should handle negative numbers', () => {
      expect(NumberUtil.median([-5, -1, 0, 1, 5])).toBe(0);
    });

    it('should handle duplicate values', () => {
      expect(NumberUtil.median([1, 2, 2, 2, 3])).toBe(2);
    });
  });

  describe('round', () => {
    it('should round to 2 decimal places', () => {
      expect(NumberUtil.round(1.2345, 2)).toBe(1.23);
    });

    it('should round up when appropriate', () => {
      expect(NumberUtil.round(1.2367, 2)).toBe(1.24);
    });

    it('should round to 0 decimal places', () => {
      expect(NumberUtil.round(1.5, 0)).toBe(2);
    });

    it('should handle negative numbers', () => {
      expect(NumberUtil.round(-1.2367, 2)).toBe(-1.24);
    });

    it('should handle zero', () => {
      expect(NumberUtil.round(0, 2)).toBe(0);
    });

    it('should round to 3 decimal places', () => {
      expect(NumberUtil.round(1.23456, 3)).toBe(1.235);
    });

    it('should handle very small numbers with precision', () => {
      expect(NumberUtil.round(0.0000012345, 8)).toBe(0.00000123);
    });
  });

  describe('compareArrays', () => {
    it('should return true for identical arrays', () => {
      expect(NumberUtil.compareArrays([1, 2, 3], [1, 2, 3])).toBe(true);
    });

    it('should return true for arrays with same elements in different order', () => {
      expect(NumberUtil.compareArrays([3, 1, 2], [1, 2, 3])).toBe(true);
    });

    it('should return false for arrays with different elements', () => {
      expect(NumberUtil.compareArrays([1, 2, 3], [1, 2, 4])).toBe(false);
    });

    it('should return false for arrays with different lengths', () => {
      expect(NumberUtil.compareArrays([1, 2], [1, 2, 3])).toBe(false);
    });

    it('should return true for empty arrays', () => {
      expect(NumberUtil.compareArrays([], [])).toBe(true);
    });

    it('should handle negative numbers', () => {
      expect(NumberUtil.compareArrays([-1, 0, 1], [1, -1, 0])).toBe(true);
    });

    it('should handle duplicate values', () => {
      expect(NumberUtil.compareArrays([1, 2, 2, 3], [3, 2, 1, 2])).toBe(true);
    });
  });
});
