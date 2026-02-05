import { CalculatorService } from './calculator.service';

describe('Calculator tests', () => {
  const calc = new CalculatorService();
  it('should add', () => {
    expect(calc.calculate('5 + 5')).toBe(10);
  });
  it('should subtract', () => {
    expect(calc.calculate('5 - 4')).toBe(1);
    expect(calc.calculate('-5')).toBe(-5);
  });
  it('should multiply', () => {
    expect(calc.calculate('2 * 3')).toBe(6);
  });
  it('shandle handle rounded multiplication', () => {
    expect(calc.calculate('1.2 ** 2')).toBe(2);
    expect(calc.calculate('1.5**1')).toBe(2);
    expect(calc.calculate('10 ** 2')).toBe(20);
    expect(calc.calculate('1.2 * 2 ** 1')).toBe(2);
    expect(calc.calculate('1 ** 1.2 * 2')).toBe(2);
    expect(calc.calculate('3.3**2')).toBe(7);
    expect(calc.calculate('1.2 ^ (1.2 ** 2)')).toBe(1.44);
  });
  it('should divide', () => {
    expect(calc.calculate('6/ 3')).toBe(2);
    expect(calc.calculate('6/ 0')).toBe(NaN);
    expect(calc.calculate('6/2/2')).toBe(1.5);
    expect(calc.calculate('2/10')).toBe(0.2);
    expect(calc.calculate('1.2 ^ (1.2 // 2')).toBe(1.2);
  });
  it('should handle rounded division', () => {
    expect(calc.calculate('6// 3')).toBe(2);
    expect(calc.calculate('6// 0')).toBe(NaN);
    expect(calc.calculate('6/2//2')).toBe(2);
    expect(calc.calculate('2//10')).toBe(0);
  });
  it('should handle percentages', () => {
    expect(calc.calculate('50%*100')).toBe(50);
    expect(calc.calculate('2*200%')).toBe(4);
    expect(calc.calculate('1 + 100%')).toBe(2);
    expect(calc.calculate('2-100%')).toBe(0);
    expect(calc.calculate('100%+100%')).toBe(2);
    expect(calc.calculate('25%')).toBe(0.25);
  });
  it('should handle priority rules', () => {
    expect(calc.calculate('50+10^2')).toBe(150);
    expect(calc.calculate('(50+50)*10^2-20')).toBe(9980);
    expect(calc.calculate('(200-10^2)/2')).toBe(50);
  });
  it('should handle acronyms', () => {
    expect(calc.calculate('40k')).toBe(40_000);
    expect(calc.calculate('1000k + 1m')).toBe(2_000_000);
    expect(calc.calculate('1k/1k')).toBe(1);
    expect(calc.calculate('1k*150%')).toBe(1_500);
    expect(calc.calculate('5e6')).toBe(5_000_000);
    expect(calc.calculate('3e2')).toBe(300);
    expect(calc.calculate('2e-1')).toBe(0.2);
    expect(calc.calculate('2e-1+(20)')).toBe(20.2);
    expect(calc.calculate('2e-1(1)')).toBe(0.2);
    expect(calc.calculate('2e-1(20)')).toBe(4);
    expect(calc.calculate('(20)2e-1')).toBe(4);
    expect(calc.calculate('e')).toBeNaN();
  });
  it('should remember last 9 entries', () => {
    const calcMem = new CalculatorService();
    expect(calcMem.calculate('h1')).toBeNaN();
    calcMem.calculate('5+5');
    expect(calcMem.calculate('h1')).toBe(10);
    expect(calcMem.calculate('5+5^2')).toBe(30);
    expect(calcMem.calculate('h1 + h2')).toBe(40);
    expect(calcMem.calculate('h1 ^ 2 - h4')).toBe(1590);
  });
});
