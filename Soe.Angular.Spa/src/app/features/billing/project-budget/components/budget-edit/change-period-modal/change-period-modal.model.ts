import { DialogData, DialogSize } from '@ui/dialog/models/dialog';
import {
  SoeCheckboxFormControl,
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { SmallGenericType } from '@shared/models/generic-type.model';

export class ChangePeriodDialogData implements DialogData {
  title: string;
  fromDate?: Date;
  toDate?: Date;
  periodType: number;
  periodTypes: SmallGenericType[] = [];
  size?: DialogSize;

  constructor() {
    this.title = 'billing.invoices.householddeduction.setseqnrdialogheader';
    this.periodType = 0;
    this.size = 'sm';
  }
}

interface IChangePeriodForm {
  validationHandler: ValidationHandler;
  fromDate: Date;
  toDate: Date;
  periodType: number;
}

export class ChangePeriodForm extends SoeFormGroup {
  get fromDate(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.fromDate;
  }
  get fromDateValue(): Date {
    return <Date>this.fromDate.value;
  }

  get toDate(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.toDate;
  }
  get toDateValue(): Date {
    return <Date>this.toDate.value;
  }

  get periodType(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.periodType;
  }

  constructor({
    validationHandler,
    fromDate,
    toDate,
    periodType,
  }: IChangePeriodForm) {
    super(validationHandler, {
      fromDate: new SoeDateFormControl(fromDate || new Date()),
      toDate: new SoeDateFormControl(toDate || new Date()),
      periodType: new SoeSelectFormControl(periodType || 0),
    });
  }
}
