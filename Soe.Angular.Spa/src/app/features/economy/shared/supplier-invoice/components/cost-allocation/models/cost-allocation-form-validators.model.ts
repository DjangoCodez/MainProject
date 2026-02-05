import { Injectable } from '@angular/core';
import { AbstractControl, ValidationErrors } from '@angular/forms';

@Injectable({
  providedIn: 'root',
})
export class CostAllocationValidators {
  static overCostAllocation(control: AbstractControl): ValidationErrors | null {
    const remainingAllocationAmount = control.get(
      'remainingAllocationAmount'
    )?.value;

    if (remainingAllocationAmount < 0) {
      return {
        custom: {
          translationKey:
            'economy.supplierInvoice.costAllocation.overcostallocation.message',
        },
      };
    }

    return null;
  }
}
