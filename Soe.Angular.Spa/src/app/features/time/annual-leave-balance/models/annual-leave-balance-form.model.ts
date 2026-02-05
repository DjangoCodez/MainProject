import {
  SoeCheckboxFormControl,
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { IAnnualLeaveTransactionEditDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

interface IAnnualLeaveTransactionForm {
  validationHandler: ValidationHandler;
  element: IAnnualLeaveTransactionEditDTO | undefined;
}
export class AnnualLeaveTransactionForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IAnnualLeaveTransactionForm) {
    super(validationHandler, {
      annualLeaveTransactionId: new SoeTextFormControl(
        element?.annualLeaveTransactionId || 0,
        {
          isIdField: true,
        }
      ),
      employeeId: new SoeSelectFormControl(element?.employeeId || 0, {
        required: true,
        zeroNotAllowed: true,
      }),
      type: new SoeSelectFormControl(element?.type || 0, { required: true }),
      dateEarned: new SoeDateFormControl(element?.dateEarned || new Date(), {
        required: true,
      }),
      dateSpent: new SoeDateFormControl(element?.dateSpent || new Date(), {
        required: true,
      }),
      minutesEarned: new SoeNumberFormControl(element?.minutesEarned || null, {
        required: true,
      }),
      minutesSpent: new SoeNumberFormControl(element?.minutesSpent || null, {
        required: true,
      }),
      accumulatedMinutes: new SoeNumberFormControl(
        element?.accumulatedMinutes || null,
        {
          required: true,
        }
      ),
      manuallySpent: new SoeCheckboxFormControl(
        element?.manuallySpent || false
      ),
      manuallyEarned: new SoeCheckboxFormControl(
        element?.manuallyEarned || false
      ),
      transactionName: new SoeTextFormControl('', { isNameField: true }),
    });
  }

  customPatchValue(data: Partial<IAnnualLeaveTransactionEditDTO>) {
    this.reset(data);
    let string = '';
    string +=
      (data.employeeNrAndName ? data.employeeNrAndName.toString() : '') + ', ';
    string +=
      (data.dateEarned ? data.dateEarned.toLocaleDateString() : '') + '/';
    string += data.dateSpent ? data.dateSpent.toLocaleDateString() : '';
    this.transactionName.setValue(string);
  }

  get annualLeaveTransactionId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.annualLeaveTransactionId;
  }
  get annualLeaveGroupId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.annualLeaveGroupId;
  }
  get employeeId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.employeeId;
  }
  get type(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.type;
  }
  get dateEarned(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.dateEarned;
  }
  get dateSpent(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.dateSpent;
  }
  get minutesEarned(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.minutesEarned;
  }
  get minutesSpent(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.minutesSpent;
  }
  get accumulatedMinutes(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.accumulatedMinutes;
  }
  get manuallySpent(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.manuallySpent;
  }
  get manuallyEarned(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.manuallyEarned;
  }
  get transactionName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.transactionName;
  }
}
