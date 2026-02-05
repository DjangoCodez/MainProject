import {
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { DateUtil } from '@shared/util/date-util';
import {
  IIncomingEmailDTO,
  IIncomingEmailAddressDTO,
  IIncomingEmailAttachmentDTO,
  IIncomingEmailLogDTO,
} from '@shared/models/generated-interfaces/IncomingEmailDTOs';

interface IInboundEmailForm {
  validationHandler: ValidationHandler;
  element?: IIncomingEmailDTO;
}

export class InboundEmailForm extends SoeFormGroup {
  inboundEmailGridHeaders = [
    'Id',
    'Type',
    'Email',
    'Delivery Status',
    'Retries',
    'Last updated',
    'Dispatcher Id',
  ];
  inboundEmailAddresses: IIncomingEmailAddressDTO[] = [];
  attachments: IIncomingEmailAttachmentDTO[] = [];
  logs: IIncomingEmailLogDTO[] = [];
  constructor({ validationHandler, element }: IInboundEmailForm) {
    super(validationHandler, {
      incomingEmailId: new SoeNumberFormControl(element?.incomingEmailId ?? 0),
      received: new SoeDateFormControl(
        element?.received ? new Date(element.received) : 0
      ),
      receivedText: new SoeTextFormControl(''),
      subject: new SoeTextFormControl(element?.subject ?? ''),
      spamScore: new SoeNumberFormControl(element?.spamScore ?? 0),
      uniqueIdentifier: new SoeTextFormControl(element?.uniqueIdentifier ?? ''),
      from: new SoeTextFormControl(element?.from ?? ''),
      text: new SoeTextFormControl(element?.text ?? ''),
      html: new SoeTextFormControl(element?.html ?? ''),
    });
  }

  customPathValue(email: IIncomingEmailDTO): void {
    this.patchValue(email);
    this.controls.receivedText.setValue(
      DateUtil.format(new Date(email.received), 'yyyy-MM-dd HH:mm:ss')
    );
    this.inboundEmailAddresses = email.inboundEmails;
    this.attachments = email.attachments;
    this.logs = email.logs;
    this.disable();
  }
}
