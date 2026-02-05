import { DialogData, DialogSize } from '@ui/dialog/models/dialog';
import { SoeFormGroup, SoeNumberFormControl } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';

export class HouseholdSequenceNumberDialogData implements DialogData {
  title: string;
  sequenceNumber: number;
  entityName: string;
  size?: DialogSize;

  constructor() {
    this.title = 'billing.invoices.householddeduction.setseqnrdialogheader';
    this.sequenceNumber = 0;
    this.entityName = '';
    this.size = 'md';
  }
}

interface IHouseholdSequenceNumberForm {
  validationHandler: ValidationHandler;
  sequenceNumber?: number;
}

export class HouseholdSequenceNumberForm extends SoeFormGroup {
  constructor({
    validationHandler,
    sequenceNumber,
  }: IHouseholdSequenceNumberForm) {
    super(validationHandler, {
      sequenceNumber: new SoeNumberFormControl(sequenceNumber || 0, {
        maxDecimals: 0,
      }),
    });
  }
}
