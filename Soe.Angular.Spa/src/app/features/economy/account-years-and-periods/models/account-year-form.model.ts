import {
  SoeCheckboxFormControl,
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { AccountYearDTO } from './account-years-and-periods.model';
import { TermGroup_AccountStatus } from '@shared/models/generated-interfaces/Enumerations';
import { FormArray } from '@angular/forms';
import { AccountPeriodRowsForm } from './account-period-rows-form.model';
import { AccountYearsValidator } from './account-year-form.validators';

interface IAccountYearForm {
  validationHandler: ValidationHandler;
  element: AccountYearDTO | undefined;
}
export class AccountYearForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler;

  constructor({ validationHandler, element }: IAccountYearForm) {
    super(validationHandler, {
      accountYearId: new SoeNumberFormControl(element?.accountYearId, {
        isIdField: true,
      }),
      yearFromTo: new SoeTextFormControl(element?.yearFromTo ?? '', {
        isNameField: true,
      }),
      from: new SoeDateFormControl(
        element?.from,
        {
          required: true,
          lessThanDate: 'to',
        },
        'common.fromdate'
      ),
      to: new SoeDateFormControl(
        element?.to,
        {
          required: true,
          greaterThanDate: 'from',
        },
        'common.dateto'
      ),
      status: new SoeSelectFormControl(
        element?.status ?? TermGroup_AccountStatus.New,
        {}
      ),
      periods: new FormArray<AccountPeriodRowsForm>([]),
      keepNumberSeries: new SoeCheckboxFormControl(
        element?.keepNumberSeries || false,
        {}
      ),
    });
    this.thisValidationHandler = validationHandler;
    this.setupSubscriptions();
  }

  public addDateValidators(validator: AccountYearsValidator) {
    // The order in which the validators are added is important.
    // The last validator to return an error will be the one that is shown.
    this.addValidators([
      validator.ensureNoGapBehind.bind(validator),
      validator.ensureNoGapAhead.bind(validator),
      validator.ensureNoOverlap.bind(validator),
    ]);
  }

  private setupSubscriptions(): void {
    this.controls.status.valueChanges.subscribe(status => {
      if (status !== TermGroup_AccountStatus.New) {
        this.controls.from.disable();
        this.controls.to.disable();
      } else {
        this.controls.from.enable();
        this.controls.to.enable();
      }

      if (this.pristine) {
        if (this.accountYearStatus === TermGroup_AccountStatus.Locked) {
          this.controls.status.disable({
            emitEvent: false,
          });
        } else {
          this.controls.status.enable({
            emitEvent: false,
          });
        }
      }
    });
  }

  public unlockYear(): void {
    this.controls.status.patchValue(TermGroup_AccountStatus.Closed);
    this.controls.status.enable();
    this.controls.status.markAsDirty();
  }

  get isLocked(): boolean {
    return this.controls.status.value === TermGroup_AccountStatus.Locked;
  }

  get accountYearStatus(): number {
    return this.controls.status.value || TermGroup_AccountStatus.New;
  }

  get accountYearId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.accountYearId;
  }

  get statusValue(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.status;
  }

  get from(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.from;
  }

  get to(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.to;
  }

  get keepNumberSeries(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.keepNumberSeries;
  }

  get periods(): FormArray<AccountPeriodRowsForm> {
    return <FormArray>this.controls.periods;
  }
}
