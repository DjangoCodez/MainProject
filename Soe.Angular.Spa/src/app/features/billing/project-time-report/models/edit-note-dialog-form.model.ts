import { ValidationHandler } from '@shared/handlers';
import { SoeFormGroup, SoeTextFormControl } from '@shared/extensions';
import { NoteDialogDTO } from './project-time-report.model';

interface IEditNoteDialogForm {
  validationHandler: ValidationHandler;
  element: NoteDialogDTO | undefined;
}

export class EditNoteDialogForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IEditNoteDialogForm) {
    super(validationHandler, {
      externalNote: new SoeTextFormControl(element?.externalNote || []),
      internalNote: new SoeTextFormControl(element?.internalNote || []),
    });
  }

  get externalNote(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.externalNote;
  }

  get internalNote(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.internalNote;
  }
}
