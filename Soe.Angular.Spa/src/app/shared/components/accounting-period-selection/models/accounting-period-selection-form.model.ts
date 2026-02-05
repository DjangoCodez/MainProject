import { ValidationHandler } from '@shared/handlers';
import {
  SoeDateFormControl,
  SoeDateRangeFormControl,
  SoeFormControl,
  SoeFormGroup,
  SoeRadioFormControl,
  SoeSelectFormControl,
} from '@shared/extensions';
import {
  IAccountingPeriodSelection,
  AccountingPeriodSelectionType,
} from './accounting-period-selection.model';
import { IAccountYearDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { DateUtil } from '@shared/util/date-util';
import { ValidationErrors, ValidatorFn, Validators } from '@angular/forms';

interface IAccountingPeriodSelectionForm {
  validationHandler: ValidationHandler;
  element?: IAccountingPeriodSelection;
}

export class AccountingPeriodSelectionForm extends SoeFormGroup {
  currentAccountingYear?: number;
  fromAccountYearDto?: IAccountYearDTO;
  toAccountYearDto?: IAccountYearDTO;
  accountingYears: IAccountYearDTO[] = [];
  constructor({ validationHandler, element }: IAccountingPeriodSelectionForm) {
    super(validationHandler, {
      selectionType: new SoeRadioFormControl(
        element?.selectionType ||
          AccountingPeriodSelectionType.ByFinancialYear.valueOf()
      ),
      accountingYearFrom: new SoeSelectFormControl(
        element?.accountingYearFrom || undefined,
        { required: true, zeroNotAllowed: true },
        'economy.accounting.accountyear'
      ),
      accountingYearTo: new SoeSelectFormControl(
        element?.accountingYearTo || undefined
      ),
      monthFrom: new SoeSelectFormControl(element?.monthFrom || undefined),
      monthTo: new SoeSelectFormControl(element?.monthTo || undefined),
      dateFrom: new SoeDateFormControl(element?.dateFrom || undefined),
      dateTo: new SoeDateFormControl(element?.dateTo || undefined),
      dateRange: new SoeDateRangeFormControl([undefined, undefined]),
    });

    this.selectionType.valueChanges.subscribe(() => {
      this.resetAccountingYearFields();
      this.toggleValidators();
    });

    this.dateRange.valueChanges.subscribe(
      (dRange: [Date | undefined, Date | undefined] | undefined) => {
        if (dRange instanceof Array && dRange.length > 1) {
          if (DateUtil.isValidDate(<Date>dRange[0]))
            this.dateFrom.patchValue(<Date>dRange[0]);

          if (DateUtil.isValidDate(<Date>dRange[1]))
            this.dateTo.patchValue(<Date>dRange[1]);

          if (
            DateUtil.isValidDate(<Date>dRange[0]) &&
            DateUtil.isValidDate(<Date>dRange[1])
          )
            this.setAccountingYearsDates(<Date>dRange[0], <Date>dRange[1]);
        } else {
          this.resetAccountingYearFields();
        }
      }
    );
  }

  get selectionType(): SoeRadioFormControl {
    return <SoeRadioFormControl>this.controls.selectionType;
  }
  get accountingYearFrom(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.accountingYearFrom;
  }
  get accountingYearTo(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.accountingYearTo;
  }
  get monthFrom(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.monthFrom;
  }
  get monthTo(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.monthTo;
  }
  get dateFrom(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.dateFrom;
  }
  get dateTo(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.dateTo;
  }
  get dateRange(): SoeDateRangeFormControl {
    return <SoeDateRangeFormControl>this.controls.dateRange;
  }

  public isValidSelection(isAccountingYearToHidden: boolean = false): boolean {
    return !isAccountingYearToHidden
      ? this.accountingYearFrom.value &&
          this.accountingYearTo.value &&
          this.monthFrom.value &&
          this.monthTo.value
      : this.accountingYearFrom.value &&
          this.monthFrom.value &&
          this.monthTo.value;
  }

  public setCurrentAccountYear(accountingYearId: number): void {
    this.currentAccountingYear = accountingYearId;

    this.patchValue({
      accountingYearFrom: this.currentAccountingYear,
      accountingYearTo: this.currentAccountingYear,
    });
  }

  public populateDates(): void {
    if (this.monthFrom.value && !isNaN(this.monthFrom.value)) {
      this.dateFrom.setValue(this.getFromDate(Number(this.monthFrom.value)), {
        emitEvent: false,
        emitModelToViewChange: false,
        emitViewToModelChange: false,
      });
    }

    if (this.monthTo.value && !isNaN(this.monthTo.value)) {
      this.dateTo.setValue(this.getToDate(Number(this.monthTo.value)), {
        emitEvent: false,
        emitModelToViewChange: false,
        emitViewToModelChange: false,
      });
    }
  }

  private toggleValidators(): void {
    this.accountingYearFrom.clearValidators();
    this.accountingYearFrom.clearAsyncValidators();
    this.dateRange.clearValidators();
    if (
      +this.selectionType.value ===
      +AccountingPeriodSelectionType.ByFinancialYear
    ) {
      this.accountingYearFrom.addValidators([Validators.required]);
      this.accountingYearFrom.addAsyncValidators([
        SoeFormControl.validateZeroNotAllowed(),
      ]);
    } else if (
      +this.selectionType.value === +AccountingPeriodSelectionType.ByDate
    ) {
      this.dateRange.addValidators([Validators.required]);
    }
    this.accountingYearFrom.updateValueAndValidity();
    this.dateRange.updateValueAndValidity();
  }

  private resetAccountingYearFields(): void {
    if (this.selectionType.value === AccountingPeriodSelectionType.ByDate) {
      this.accountingYearFrom.setValue(0);
      this.accountingYearTo.setValue(0);
      this.monthFrom.setValue(0);
      this.monthTo.setValue(0);
    } else {
      this.accountingYearFrom.setValue(this.currentAccountingYear);
      this.accountingYearTo.setValue(this.currentAccountingYear);
    }
  }

  private getFromDate(intervalId: number): Date | undefined {
    const interval = (this.fromAccountYearDto?.periods ?? []).find(
      x => x.accountPeriodId === intervalId
    );
    if (interval) {
      return DateUtil.parseDateOrJson(interval.from);
    } else return undefined;
  }

  private getToDate(intervalId: number): Date | undefined {
    const interval = (this.toAccountYearDto?.periods ?? []).find(
      x => x.accountPeriodId === intervalId
    );
    if (interval) {
      return DateUtil.parseDateOrJson(interval.to);
    } else return undefined;
  }

  private getAccountingYearsDates(
    dateFrom: Date,
    dateTo: Date
  ): IAccountYearDTO[] {
    if (
      dateFrom &&
      DateUtil.isValidDate(<Date>dateFrom) &&
      dateTo &&
      DateUtil.isValidDate(<Date>dateTo)
    ) {
      return this.accountingYears.filter(
        y =>
          (dateFrom >= y.from && dateFrom <= y.to) ||
          (dateTo >= y.from && dateTo <= y.to)
      );
    }

    return [];
  }

  private setAccountingYearsDates(dateFrom: Date, dateTo: Date): void {
    const years = this.getAccountingYearsDates(dateFrom, dateTo);
    if (years.length === 1) {
      this.accountingYearFrom.setValue(years[0].accountYearId, {
        emitEvent: false,
        emitModelToViewChange: false,
        emitViewToModelChange: false,
      });
      this.accountingYearTo.setValue(years[0].accountYearId, {
        emitEvent: false,
        emitModelToViewChange: false,
        emitViewToModelChange: false,
      });
    }
  }
}

export function validateDateRange(): ValidatorFn {
  return (form): ValidationErrors | null => {
    const dateFrom = form.get('accountingPeriod')?.get('dateFrom')?.value;
    const dateTo = form.get('accountingPeriod')?.get('dateTo')?.value;
    const selectionType = form
      .get('accountingPeriod')
      ?.get('selectionType')?.value;
    const accountingYears = (
      form.get('accountingPeriod') as AccountingPeriodSelectionForm
    ).accountingYears;

    if (
      selectionType === AccountingPeriodSelectionType.ByDate.valueOf() &&
      dateFrom &&
      DateUtil.isValidDate(<Date>dateFrom) &&
      dateTo &&
      DateUtil.isValidDate(<Date>dateTo)
    ) {
      const filteredYears = accountingYears.filter(
        y =>
          (dateFrom >= y.from && dateFrom <= y.to) ||
          (dateTo >= y.from && dateTo <= y.to)
      );

      if (filteredYears.length > 1) {
        const error: ValidationErrors = {
          custom: { translationKey: 'common.period.selection.daterange.error' },
        };
        return error;
      }

      if (filteredYears.length === 0) {
        const error: ValidationErrors = {
          custom: {
            translationKey:
              'common.period.selection.daterange.error.noaccountyear',
          },
        };
        return error;
      }
    }

    return null;
  };
}
