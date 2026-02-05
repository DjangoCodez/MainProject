import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';
import {
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { IIncomingEmailFilterDTO } from '@shared/models/generated-interfaces/IncomingEmailDTOs';

interface IInboundEmailGridFilterForm {
  validationHandler: ValidationHandler;
  element?: IIncomingEmailFilterDTO;
}

export class InboundEmailGridFilterForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IInboundEmailGridFilterForm) {
    super(validationHandler, {
      senderEmail: new SoeTextFormControl(
        element?.senderEmail || '',
        {},
        'Sender Email'
      ),
      recipientEmails: new SoeTextFormControl(
        element?.recipientEmails || '',
        {},
        'Recipient Emails'
      ),
      deliveryStatus: new SoeSelectFormControl(element?.deliveryStatus || []),
      fromDate: new SoeDateFormControl(element?.fromDate || undefined),
      toDate: new SoeDateFormControl(element?.toDate || undefined),
      noOfRecords: new SoeNumberFormControl(
        element?.noOfRecords ?? 100,
        {},
        'No. of Records'
      ),
    });
    this.recipientEmails.setValidators([
      InboundEmailValidators.validateMultipleEmails(),
    ]);
  }

  get fromDate(): SoeDateFormControl {
    return this.controls.fromDate as SoeDateFormControl;
  }

  get toDate(): SoeDateFormControl {
    return this.controls.toDate as SoeDateFormControl;
  }
  get senderEmail(): SoeTextFormControl {
    return this.controls.senderEmail as SoeTextFormControl;
  }
  get recipientEmails(): SoeTextFormControl {
    return this.controls.recipientEmails as SoeTextFormControl;
  }
  get deliveryStatus(): SoeSelectFormControl {
    return this.controls.deliveryStatus as SoeSelectFormControl;
  }
  get noOfRecords(): SoeNumberFormControl {
    return this.controls.noOfRecords as SoeNumberFormControl;
  }
}

class InboundEmailValidators {
  public static validateMultipleEmails(separator: string = ';'): ValidatorFn {
    // Escape special regex characters in separator for regex use
    const escapedSeparator = separator.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');

    const pattern =
      '^(([^<>()[\\]\\\\.,;:\\s@"]+(\\.[^<>()[\\]\\\\.,;:\\s@"]+)*|"[^"]+")' +
      '@[a-zA-Z0-9-]+(\\.[a-zA-Z0-9-]+)*\\.[a-zA-Z]{2,})' +
      '(\\s*' +
      escapedSeparator +
      '\\s*' +
      '([^<>()[\\]\\\\.,;:\\s@"]+(\\.[^<>()[\\]\\\\.,;:\\s@"]+)*|"[^"]+")' +
      '@[a-zA-Z0-9-]+(\\.[a-zA-Z0-9-]+)*\\.[a-zA-Z]{2,})*' +
      '(\\s*' +
      escapedSeparator +
      '\\s*)?$';

    const emailListRegex = new RegExp(pattern);

    return (control: AbstractControl): ValidationErrors | null => {
      const value = control.value;
      if (!value || typeof value !== 'string') return null;

      // Auto-trim and sanitize
      const sanitized = value
        .split(separator)
        .map(e => e.trim())
        .filter(e => e.length > 0)
        .join(separator);

      // Validate
      return emailListRegex.test(sanitized)
        ? null
        : {
            [`Recipient Emails: Please enter valid email addresses separated by "${separator}"`]:
              true,
          };
    };
  }
}
