import { ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import {
  SoeCheckboxFormControl,
  SoeFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { TermGroup_FinnishTaxReturnExportTaxPeriodLength } from '@shared/models/generated-interfaces/Enumerations';
import { DateUtil } from '@shared/util/date-util';
import { FinnishTaxExportDTO } from './finnish-tax-export.model';

interface IFinnishTaxExportForm {
  validationHandler: ValidationHandler;
  element?: FinnishTaxExportDTO;
}

export class FinnishTaxExportForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IFinnishTaxExportForm) {
    super(validationHandler, {
      lengthOfTaxPeriod: new SoeSelectFormControl(
        element?.lengthOfTaxPeriod || 1,
        {
          required: true,
        },
        'economy.export.finnishtax.periodtaxreturn.lengthoftaxperiod'
      ),
      taxPeriod: new SoeNumberFormControl(
        element?.taxPeriod || undefined,
        {
          required: true,
        },
        'economy.export.finnishtax.periodtaxreturn.taxperiod'
      ),
      taxPeriodYear: new SoeNumberFormControl(
        element?.taxPeriodYear || DateUtil.getToday().getFullYear(),
        {
          required: true,
          zeroNotAllowed: true,
          decimals: 0,
        },
        'economy.export.finnishtax.periodtaxreturn.taxperiodyear'
      ),
      noActivity: new SoeCheckboxFormControl(element?.noActivity || false),
      correction: new SoeCheckboxFormControl(element?.correction || false),
      cause: new SoeSelectFormControl(
        element?.cause || 0,
        {},
        'economy.export.finnishtax.periodtaxreturn.cause'
      ),
    });
    this.lengthOfTaxPeriod.valueChanges.subscribe(l =>
      this.togglePeriodValidation(l)
    );
    this.correction.valueChanges.subscribe(c => this.toggleCauseValidation(c));
    this.valueChanges.subscribe(() => this.resetForm());
  }

  get lengthOfTaxPeriod(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.lengthOfTaxPeriod;
  }
  get taxPeriod(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.taxPeriod;
  }
  get taxPeriodYear(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.taxPeriodYear;
  }
  get noActivity(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.noActivity;
  }
  get correction(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.correction;
  }
  get cause(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.cause;
  }

  private resetForm(): void {
    this.markAsUntouched();
    this.markAsPristine();
  }

  private toggleCauseValidation(correction: boolean): void {
    if (correction) {
      this.cause.addValidators(Validators.required);
      this.cause.addAsyncValidators(SoeFormControl.validateZeroNotAllowed());
    } else {
      this.cause.clearValidators();
      this.cause.clearAsyncValidators();
    }

    this.cause.updateValueAndValidity();
  }

  private togglePeriodValidation(lengthOfPeriod: number): void {
    switch (lengthOfPeriod) {
      case TermGroup_FinnishTaxReturnExportTaxPeriodLength.Month.valueOf():
      case TermGroup_FinnishTaxReturnExportTaxPeriodLength.Quarter.valueOf():
        this.taxPeriod.addValidators(Validators.required);
        break;
      default:
        this.taxPeriod.clearValidators();
        break;
    }
    this.taxPeriod.updateValueAndValidity();
  }

  setCustomValidators(
    monthError: string,
    quarterError: string,
    yearLengthError: string,
    yearToopastError: string
  ): void {
    this.addValidators(validateTaxPeriod(monthError, quarterError));
    this.taxPeriodYear.addValidators(
      validateTaxYear(yearLengthError, yearToopastError)
    );
  }
}

export function validateTaxPeriod(
  monthErrorStr: string,
  quarterErrorStr: string
): ValidatorFn {
  return (form): ValidationErrors | null => {
    if (form) {
      const taxExport = form.value as FinnishTaxExportDTO;

      switch (taxExport.lengthOfTaxPeriod) {
        case TermGroup_FinnishTaxReturnExportTaxPeriodLength.Month.valueOf():
          if (taxExport.taxPeriod === 0 || taxExport.taxPeriod > 12) {
            const error: ValidationErrors = {
              [monthErrorStr]: true,
            };
            return error;
          }
          break;
        case TermGroup_FinnishTaxReturnExportTaxPeriodLength.Quarter.valueOf():
          if (taxExport.taxPeriod === 0 || taxExport.taxPeriod > 4) {
            const error: ValidationErrors = {
              [quarterErrorStr]: true,
            };
            return error;
          }
          break;
      }
    }
    return null;
  };
}

export function validateTaxYear(
  lengthError: string,
  tooPastError: string
): ValidatorFn {
  return (control): ValidationErrors | null => {
    const year = control.value;
    if (String(year).length !== 4) {
      const error: ValidationErrors = {
        custom: { value: lengthError },
      };
      return error;
    }

    if (year < DateUtil.getToday().addYears(-4).getFullYear()) {
      const error: ValidationErrors = {
        custom: { value: tooPastError },
      };
      return error;
    }
    return null;
  };
}
