import { DialogData } from '@ui/dialog/models/dialog';
import { CurrencyRateDTO } from '@features/economy/currencies/models/currencies.model';
import {
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { TermGroup_CurrencySource } from '@shared/models/generated-interfaces/Enumerations';
import {
  AbstractControl,
  AsyncValidatorFn,
  ValidationErrors,
} from '@angular/forms';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { DateUtil } from '@shared/util/date-util';

export class CurrencyRatesEditDialogData implements DialogData {
  title: string;
  rate: CurrencyRateDTO | undefined;
  rows: CurrencyRateDTO[];
  baseCurrencyCode: string;
  otherCurrencyCode: string;
  sources: SmallGenericType[];

  constructor(
    rate: CurrencyRateDTO | undefined,
    rows: CurrencyRateDTO[],
    sources: SmallGenericType[],
    baseCurrencyCode: string,
    otherCurrencyCode: string
  ) {
    this.rate = rate;
    this.title = 'economy.accounting.currency.rate';
    this.rows = rows;
    this.sources = sources;
    this.baseCurrencyCode = baseCurrencyCode;
    this.otherCurrencyCode = otherCurrencyCode;
  }
}

interface ICurrencyRatesForm {
  validationHandler: ValidationHandler;
  element: CurrencyRateDTO | undefined;
  rows: CurrencyRateDTO[];
}

export class CurrencyRatesForm extends SoeFormGroup {
  constructor({ validationHandler, element, rows }: ICurrencyRatesForm) {
    super(validationHandler, {
      currencyRateId: new SoeTextFormControl(element?.currencyRateId || 0, {
        isIdField: true,
      }),
      currencyId: new SoeTextFormControl(element?.currencyId || 0),
      date: new SoeDateFormControl(element?.date || new Date(), {}),
      source: new SoeTextFormControl(
        element?.source || TermGroup_CurrencySource.Manually
      ),
      rateToBase: new SoeNumberFormControl(element?.rateToBase || 0, {
        maxDecimals: 4,
        minValue: 0.0001,
      }),
      rateFromBase: new SoeNumberFormControl(element?.rateFromBase || 0, {
        maxDecimals: 4,
        minValue: 0.0001,
      }),
    });
    this.setupSubscribers();
    this.setupDateValidator(rows);
    this.setDisabledFields();
  }

  get sourceType(): TermGroup_CurrencySource {
    return this.controls.source.value;
  }

  setDisabledFields() {
    this.controls.source.disable();
    if (this.controls.currencyRateId.value) {
      this.controls.date.disable();
    }
    if (this.controls.source.value !== TermGroup_CurrencySource.Manually) {
      this.controls.date.disable();
      this.controls.rateToBase.disable();
    }
  }

  setupSubscribers() {
    this.controls.currencyRateId.valueChanges.subscribe(value => {
      if (value) {
        this.controls.date.disable();
      } else {
        this.controls.date.enable();
      }
    });
  }

  setupDateValidator(rows: CurrencyRateDTO[]) {
    const dateValidator = new RowDateDuplicateValidator(rows);
    this.controls.date.setAsyncValidators(dateValidator.validateUniqueDate());
  }
}

class RowDateDuplicateValidator {
  constructor(private rows: CurrencyRateDTO[]) {}

  dateExists(date: Date): boolean {
    return this.rows.some(row => {
      const other = DateUtil.isValidDateOrString(row.date)
        ? new Date(row.date)
        : undefined;

      if (!other) return false;

      return other.toDateString() === date.toDateString();
    });
  }

  validateUniqueDate(): AsyncValidatorFn {
    return (control: AbstractControl): Promise<ValidationErrors | null> => {
      return new Promise<ValidationErrors | null>(resolve => {
        this.dateExists(control.value)
          ? resolve({
              notAllowed: {
                value: DateUtil.localeDateFormat(control.value),
              },
            })
          : resolve(null);
      });
    };
  }
}
