import { TranslateService } from '@ngx-translate/core';
import {
  SoeCheckboxFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';

export class SupplierCentralGridForm extends SoeFormGroup {
  constructor(
    transation: TranslateService,
    messageBoxService: MessageboxService
  ) {
    const validationHandler = new ValidationHandler(
      transation,
      messageBoxService
    );
    super(validationHandler, {
      allItemsSelection: new SoeNumberFormControl(1),
      loadOpen: new SoeCheckboxFormControl(true),
      loadClosed: new SoeCheckboxFormControl(false),
    });
  }

  get allItemsSelection(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.allItemsSelection;
  }

  get loadOpen(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.loadOpen;
  }

  get loadClosed(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.loadClosed;
  }
}
