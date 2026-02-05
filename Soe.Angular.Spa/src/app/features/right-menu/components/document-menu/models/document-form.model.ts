import {
  SoeCheckboxFormControl,
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { DocumentDTO } from '@shared/models/document.model';

interface IDocumentForm {
  validationHandler: ValidationHandler;
  element: DocumentDTO | undefined;
}
export class DocumentForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IDocumentForm) {
    super(validationHandler, {
      dataStorageId: new SoeTextFormControl(element?.dataStorageId || 0, {
        isIdField: true,
      }),
      fileName: new SoeTextFormControl(
        element?.fileName || '',
        { required: true, disabled: true, maxLength: 100, minLength: 1 },
        'common.filename'
      ),
      fileString: new SoeTextFormControl(''),
      extension: new SoeTextFormControl(
        element?.extension || '',
        { disabled: true },
        'core.document.extension'
      ),
      fileSize: new SoeNumberFormControl(element?.fileSize || 0),
      name: new SoeTextFormControl(
        element?.name || '',
        { isNameField: true, maxLength: 255, minLength: 1 },
        'common.name'
      ),
      description: new SoeTextFormControl(
        element?.description || '',
        { maxLength: 255 },
        'common.description'
      ),
      folder: new SoeTextFormControl(
        element?.folder || '',
        { maxLength: 255 },
        'core.document.folder'
      ),
      validFrom: new SoeDateFormControl(element?.validFrom || null),
      validTo: new SoeDateFormControl(element?.validTo || null),
      messageGroupIds: new SoeSelectFormControl(
        element?.messageGroupIds || undefined
      ),
      needsConfirmation: new SoeCheckboxFormControl(
        element?.needsConfirmation || false
      ),
      selectedFolder: new SoeSelectFormControl(undefined),
      selectedRecipientFilter: new SoeSelectFormControl(0),
    });
  }

  get isValid(): boolean {
    return (
      this.controls.fileName.getRawValue().length > 0 && this.dateRangeIsValid
    );
  }

  get dateRangeIsValid(): boolean {
    return (
      !this.controls.validFrom.value ||
      !this.controls.validTo.value ||
      this.controls.validFrom.value <= this.controls.validTo.value
    );
  }
}
