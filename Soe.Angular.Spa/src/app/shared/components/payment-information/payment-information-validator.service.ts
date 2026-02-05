import { inject, Injectable } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { PaymentInformationRowForm } from './payment-information-form.model';
import { catchError, map, of } from 'rxjs';
import { AbstractControl, AsyncValidatorFn } from '@angular/forms';
import { TermGroup_SysPaymentType } from '@shared/models/generated-interfaces/Enumerations';
import { PaymentInformationService } from './services/payment-information.service';

@Injectable({
  providedIn: 'root',
})
export class PaymentInformationValidatorService {
  readonly translate = inject(TranslateService);
  readonly paymentInformationService = inject(PaymentInformationService);

  constructor() {}

  ibanValidator(): AsyncValidatorFn {
    return (control: AbstractControl) => {
      const parent = control && (control.parent as PaymentInformationRowForm);

      const iban = this.getParsedIBAN(
        control.value,
        parent.isForeign,
        parent?.get('sysPaymentTypeId')?.value
      );

      if (!iban) {
        return of(null);
      }

      return this.paymentInformationService.isIbanValid(iban).pipe(
        map(isValid => {
          const result = isValid
            ? null
            : {
                custom: {
                  value: this.translate.instant(
                    'economy.supplier.supplier.ibannotvalid'
                  ),
                },
              };
          return result;
        }),
        catchError(() => of(null))
      );
    };
  }

  getParsedIBAN(
    str: string,
    isForeign: boolean,
    sysPaymentTypeId: number
  ): string | null {
    if (sysPaymentTypeId === TermGroup_SysPaymentType.BIC) {
      const parts = str.split('/');
      return parts.length === 2 ? parts[1] : isForeign ? str : null;
    }
    return sysPaymentTypeId === TermGroup_SysPaymentType.SEPA ? str : null;
  }
}
