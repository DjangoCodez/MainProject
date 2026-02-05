import { SoeFormGroup, SoeTextFormControl } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';

export class FileUploadDialogDTO {
  fileName?: string;
  fileContent?: string;
}

interface IFileUploadDialogForm {
  validationHandler: ValidationHandler;
  element: FileUploadDialogDTO;
}

export class FileUploadDialogForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IFileUploadDialogForm) {
    super(validationHandler, {
      fileName: new SoeTextFormControl(element?.fileName || undefined),
      fileContent: new SoeTextFormControl(element?.fileContent || undefined),
    });
  }

  get fileName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.fileName;
  }

  get fileContent(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.fileContent;
  }
}
