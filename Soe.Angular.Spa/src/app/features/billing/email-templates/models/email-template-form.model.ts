import { ValidationHandler } from '@shared/handlers';
import { EmailTemplateDTO } from './email-template.model';
import {
  SoeCheckboxFormControl,
  SoeFormGroup,
  SoeTextFormControl,
} from '@shared/extensions';

interface IEmailTemplateForm {
  validationHandler: ValidationHandler;
  element: EmailTemplateDTO | undefined;
}

export class EmailTemplateForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IEmailTemplateForm) {
    super(validationHandler, {
      emailTemplateId: new SoeTextFormControl(element?.emailTemplateId || 0, {
        isIdField: true,
      }),
      actorCompanyId: new SoeTextFormControl(element?.actorCompanyId || 0),
      type: new SoeTextFormControl(
        element?.type || 0,
        {
          required: true,
        },
        'common.type'
      ),
      name: new SoeTextFormControl(
        element?.name || '',
        {
          isNameField: true,
          required: true,
          maxLength: 100,
        },
        'common.name'
      ),
      subject: new SoeTextFormControl(
        element?.subject || '',
        {
          required: true,
        },
        'billing.invoices.emailtemplate.subject'
      ),
      body: new SoeTextFormControl(
        element?.body || '',
        {
          required: true,
        },
        'billing.invoices.emailtemplate.body'
      ),
      typename: new SoeTextFormControl(element?.typename || ''),
      bodyIsHTML: new SoeCheckboxFormControl(
        element?.bodyIsHTML || false,
        undefined,
        'billing.invoices.emailtemplate.htmlformat'
      ),
    });
  }

  get emailTemplateId(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.emailTemplateId;
  }

  get type(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.type;
  }

  get name(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.name;
  }

  get subject(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.subject;
  }

  get body(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.body;
  }

  get typename(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.typename;
  }

  get bodyIsHTML(): SoeCheckboxFormControl {
    return <SoeCheckboxFormControl>this.controls.bodyIsHTML;
  }
}
