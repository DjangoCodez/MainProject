import {
  SoeFormGroup,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { CurrencyDTO, CurrencyRateDTO } from './currencies.model';
import { TermGroup_CurrencyIntervalType } from '@shared/models/generated-interfaces/Enumerations';
import { arrayToFormArray, clearAndSetFormArray } from '@shared/util/form-util';
import { FormArray, FormControl } from '@angular/forms';

interface ICurrenciesForm {
  validationHandler: ValidationHandler;
  element: CurrencyDTO | undefined;
}

export class CurrenciesForm extends SoeFormGroup {
  constructor({ validationHandler, element }: ICurrenciesForm) {
    super(validationHandler, {
      currencyId: new SoeTextFormControl(element?.currencyId || 0, {
        isIdField: true,
      }),
      sysCurrencyId: new SoeSelectFormControl(element?.sysCurrencyId || 0, {
        required: true,
      }),
      description: new SoeTextFormControl(element?.description || '', {
        isNameField: true,
      }),
      intervalType: new SoeSelectFormControl(
        element?.intervalType || TermGroup_CurrencyIntervalType.Manually,
        {
          required: true,
        }
      ),
      currencyRates: arrayToFormArray(element?.currencyRates || []),
    });
    this.currencyIdSubscriber();
  }

  get hasSysCurrency(): boolean {
    return !!this.sysCurrencyId.value;
  }

  get sysCurrencyId(): FormControl<number> {
    return <FormControl<number>>this.controls.sysCurrencyId;
  }

  get description(): FormControl<string> {
    return <FormControl<string>>this.controls.description;
  }

  get currencyRates(): FormArray<FormControl<number>> {
    return <FormArray<FormControl<number>>>this.controls.currencyRates;
  }

  customReset(element: CurrencyDTO) {
    this.reset(element);
    this.patchRates(element.currencyRates, false);
  }

  patchRates(rates: CurrencyRateDTO[], isModified: boolean) {
    clearAndSetFormArray(rates, this.currencyRates, isModified);
  }

  currencyIdSubscriber() {
    this.controls.currencyId.valueChanges.subscribe(value => {
      if (value) {
        this.controls.sysCurrencyId.disable();
      } else {
        this.controls.sysCurrencyId.enable();
      }
    });
  }

  isManual() {
    return (
      this.controls.intervalType.value ===
      TermGroup_CurrencyIntervalType.Manually
    );
  }

  ratesArray() {
    return this.controls.currencyRates;
  }

  getRates() {
    return this.controls.currencyRates.value as CurrencyRateDTO[];
  }
}
