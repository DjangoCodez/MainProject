import { AbstractControl, ValidatorFn } from '@angular/forms';

export class DateValidators {
  static dateLessThan(otherDateField: string): ValidatorFn {
    return this.dateComparison(otherDateField, false);
  }
  static dateGreaterThan(otherDateField: string): ValidatorFn {
    return this.dateComparison(otherDateField, true);
  }
  static dateComparison(otherDateField: string, gt: boolean): ValidatorFn {
    return (c: AbstractControl): { [key: string]: boolean } | null => {
      const other = c.parent?.get(otherDateField);
      const date1 = gt ? c.value : other?.value;
      const date2 = gt ? other?.value : c.value;

      if (date1 && date2 && date1 > date2) {
        return {
          dateValid: false,
        };
      }
      return null;
    };
  }
}
