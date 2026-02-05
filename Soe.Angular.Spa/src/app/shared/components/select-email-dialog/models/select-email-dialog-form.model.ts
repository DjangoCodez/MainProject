import {
  SoeCheckboxFormControl,
  SoeDateFormControl,
  SoeFormGroup,
  SoeRadioFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import {
  EmailTemplateDTO,
  SelectEmailAttachmentsDTO,
  SelectEmailCheckListsDTO,
  SelectEmailDialogFormDTO,
  SelectEmailRecipientsDTO,
} from './select-email-dialog.model';
import { FormArray } from '@angular/forms';

interface ISelectEmailDialogForm {
  validationHandler: ValidationHandler;
  element: SelectEmailDialogFormDTO;
}
interface IEmailTemplatesForm {
  validationHandler: ValidationHandler;
  element: EmailTemplateDTO;
}
export class EmailTemplatesForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IEmailTemplatesForm) {
    super(validationHandler, {
      emailTemplateId: new SoeRadioFormControl<number>(
        element?.emailTemplateId || 0,
        {
          isIdField: true,
        }
      ),
      name: new SoeTextFormControl(element?.name || ''),
      subject: new SoeTextFormControl(element?.subject || ''),
      typename: new SoeTextFormControl(element?.typename || ''),
    });
  }

  get emailTemplateId(): SoeRadioFormControl {
    return <SoeRadioFormControl>this.controls.emailTemplateId;
  }
  get name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.name;
  }
  get subject(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.subject;
  }
  get typename(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.typename;
  }
}

interface IEmailRecipientsForm {
  validationHandler: ValidationHandler;
  element: SelectEmailRecipientsDTO;
}

export class EmailRecipientsForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IEmailRecipientsForm) {
    super(validationHandler, {
      id: new SoeTextFormControl(element?.id || 0, {
        isIdField: true,
      }),
      name: new SoeTextFormControl(element?.name || ''),
      isSelected: new SoeCheckboxFormControl(element?.isSelected || false),
    });
  }

  get id(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.id;
  }
  get name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.name;
  }
  get isSelected(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.isSelected;
  }
}

interface IEmailAattachmentsForm {
  validationHandler: ValidationHandler;
  element: SelectEmailAttachmentsDTO;
}

export class EmailAattachmentsForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IEmailAattachmentsForm) {
    super(validationHandler, {
      invoiceAttachmentId: new SoeTextFormControl(
        element?.invoiceAttachmentId || 0
      ),
      description: new SoeTextFormControl(element?.description || ''),
      fileName: new SoeTextFormControl(element?.fileName || ''),
      imageId: new SoeTextFormControl(element?.imageId || ''),
      isSelected: new SoeCheckboxFormControl(element?.isSelected || false),
      fileRecordId: new SoeTextFormControl(element?.fileRecordId || 0),
    });
  }

  get invoiceAttachmentId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.invoiceAttachmentId;
  }
  get imageId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.imageId;
  }
  get description(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.description;
  }
  get fileName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.fileName;
  }
  get isSelected(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.isSelected;
  }
  get fileRecordId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.fileRecordId;
  }
}

interface IEmailCheckListsForm {
  validationHandler: ValidationHandler;
  element: SelectEmailCheckListsDTO;
}

export class EmailCheckListsForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IEmailCheckListsForm) {
    super(validationHandler, {
      tempHeadId: new SoeTextFormControl(element?.tempHeadId || ''),
      checklistHeadRecordId: new SoeTextFormControl(
        element?.checklistHeadRecordId || 0
      ),
      checklistHeadId: new SoeTextFormControl(element?.checklistHeadId || 0),
      checklistHeadName: new SoeTextFormControl(
        element?.checklistHeadName || 0
      ),
      created: new SoeDateFormControl(element?.created || ''),
      recordId: new SoeTextFormControl(element?.recordId || 0),
      isSelected: new SoeCheckboxFormControl(element?.isSelected || false),
    });
  }

  get tempHeadId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.tempHeadId;
  }
  get checklistHeadRecordId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.checklistHeadRecordId;
  }
  get checklistHeadId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.checklistHeadId;
  }
  get checklistHeadName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.checklistHeadName;
  }
  get created(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.created;
  }
  get recordId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.recordId;
  }
  get isSelected(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.isSelected;
  }
}

export class SelectEmailDialogForm extends SoeFormGroup {
  recipientsValidationHandler: ValidationHandler;
  attachmentsValidationHandler: ValidationHandler;
  checkListsValidationHandler: ValidationHandler;
  constructor({ validationHandler, element }: ISelectEmailDialogForm) {
    super(validationHandler, {
      selectedLanguageId: new SoeSelectFormControl(
        element.selectedLanguageId || '',
        {}
      ),
      selectedReportId: new SoeSelectFormControl(
        element.selectedReportId || 0,
        {}
      ),
      selectedTemplateId: new SoeTextFormControl(
        element.selectedTemplateId || '',
        {}
      ),
      // emailTemplates: new FormArray<EmailTemplatesForm>([]),
      recipients: new FormArray<EmailRecipientsForm>([]),
      attachments: new FormArray<EmailAattachmentsForm>([]),
      checkLists: new FormArray<EmailCheckListsForm>([]),
      mergePdfs: new SoeCheckboxFormControl(element.mergePdfs || false, {}),
      emailAddresses: new SoeTextFormControl(element.emailAddresses || '', {}),
    });
    this.recipientsValidationHandler = validationHandler;
    this.attachmentsValidationHandler = validationHandler;
    this.checkListsValidationHandler = validationHandler;
  }

  get selectedLanguageId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.selectedLanguageId;
  }
  get selectedReportId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.selectedReportId;
  }
  get selectedTemplateId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.selectedTemplateId;
  }
  get mergePdfs(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.mergePdfs;
  }
  get recipients(): FormArray<EmailRecipientsForm> {
    return <FormArray>this.controls.recipients;
  }

  get selectedRecipients(): FormArray<EmailRecipientsForm> {
    const allRecipients = this.controls.recipients as FormArray;
    const selectedRecipientsArray = allRecipients.controls.filter(
      control => control.value.isSelected
    ) as EmailRecipientsForm[];
    return new FormArray(selectedRecipientsArray);
  }

  get attachments(): FormArray<EmailAattachmentsForm> {
    return <FormArray>this.controls.attachments;
  }
  get checkLists(): FormArray<EmailCheckListsForm> {
    return <FormArray>this.controls.checkLists;
  }

  get emailAddresses(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.emailAddresses;
  }

  customRecipientsPatchValue(recipients: SelectEmailRecipientsDTO[]) {
    (this.controls.recipients as FormArray).clear();
    if (recipients) {
      for (const recipient of recipients) {
        if (
          recipient.id !== null &&
          recipient.id !== undefined &&
          typeof recipient.id === 'number'
        ) {
          const row = new EmailRecipientsForm({
            validationHandler: this.recipientsValidationHandler,
            element: recipient,
          });
          (this.controls.recipients as FormArray).push(row, {
            emitEvent: false,
          });
        }
      }
    }
  }

  customAttachmentsPatchValue(attachments: SelectEmailAttachmentsDTO[]) {
    (this.controls.attachments as FormArray).clear();
    if (attachments) {
      for (const attachment of attachments) {
        const row = new EmailAattachmentsForm({
          validationHandler: this.attachmentsValidationHandler,
          element: attachment,
        });
        (this.controls.attachments as FormArray).push(row, {
          emitEvent: false,
        });
      }
    }
  }
  customCheckListsPatchValue(checkLists: SelectEmailCheckListsDTO[]) {
    (this.controls.checkLists as FormArray).clear();
    if (checkLists) {
      for (const checkList of checkLists) {
        const row = new EmailCheckListsForm({
          validationHandler: this.checkListsValidationHandler,
          element: checkList,
        });
        (this.controls.checkLists as FormArray).push(row, {
          emitEvent: false,
        });
      }
    }
  }
}
