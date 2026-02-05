import { AbstractControl, ValidationErrors } from '@angular/forms';

export class DateRangeValidator {
  static requiredFrom(control: AbstractControl): ValidationErrors | null {
    const dateFrom = control.value ? control.value[0] : null;
    return dateFrom ? null : { requiredFrom: true };
  }

  static requiredTo(control: AbstractControl): ValidationErrors | null {
    const dateTo = control.value ? control.value[1] : null;
    return dateTo ? null : { requiredTo: true };
  }
}
