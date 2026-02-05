import { ValidationErrors, ValidatorFn } from '@angular/forms';

export function overCostAllocationValidator(errorTerm: string): ValidatorFn {
  return (form): ValidationErrors | null => {
    const remainingAllocationAmount = form.get(
      'remainingAllocationAmount'
    )?.value;
    if (remainingAllocationAmount < 0) {
      const error: ValidationErrors = {};
      error[errorTerm] = true;
      return error;
    }
    return null;
  };
}
