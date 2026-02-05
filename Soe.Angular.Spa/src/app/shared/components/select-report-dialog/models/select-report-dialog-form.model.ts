import {
  SoeCheckboxFormControl,
  SoeFormGroup,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';

import { FormArray } from '@angular/forms';
import { SelectReportDialogFormDTO } from './select-report-dialog.model';

interface ISelectReportDialogForm {
  validationHandler: ValidationHandler;
  element: SelectReportDialogFormDTO;
}

export class SelectReportDialogForm extends SoeFormGroup {
  constructor({ validationHandler, element }: ISelectReportDialogForm) {
    super(validationHandler, {
      languageId: new SoeSelectFormControl(element.languageId || '', {}),
      isReportCopy: new SoeCheckboxFormControl(
        element.isReportCopy || false,
        {}
      ),
      isReminder: new SoeCheckboxFormControl(element.isReminder || '', {}),
      savePrintout: new SoeCheckboxFormControl(
        element.savePrintout || false,
        {}
      ),
    });
  }

  get languageId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.languageId;
  }
  get isReportCopy(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.isReportCopy;
  }
  get isReminder(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.isReminder;
  }
  get savePrintout(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.savePrintout;
  }
}
