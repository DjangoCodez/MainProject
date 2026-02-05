import { ValidationHandler } from '@shared/handlers';
import { SoeDateFormControl, SoeFormGroup } from '@shared/extensions';
import { TransferToAccountDistributionEntryDTO } from './inventory-writeoffs.model';

interface IInventoryWriteoffsSearchForm {
  validationHandler: ValidationHandler;
  element: TransferToAccountDistributionEntryDTO | undefined;
}

export class InventoryWriteoffsSearchForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IInventoryWriteoffsSearchForm) {
    super(validationHandler, {
      periodDate: new SoeDateFormControl(element?.periodDate || new Date()),
    });
  }

  get periodDate() {
    return <SoeDateFormControl>this.controls.periodDate;
  }
}
