import { AbstractControl, ValidationErrors } from '@angular/forms';

export class NumberRangeValidator {
  static requiredFrom(control: AbstractControl): ValidationErrors | null {
    const valueFrom = control.value ? control.value[0] : null;
    return valueFrom ? null : { requiredFrom: true };
  }

  static requiredTo(control: AbstractControl): ValidationErrors | null {
    const valueTo = control.value ? control.value[1] : null;
    return valueTo ? null : { requiredTo: true };
  }
}
