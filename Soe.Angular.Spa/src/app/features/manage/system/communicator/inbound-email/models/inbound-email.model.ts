import { IIncomingEmailGridDTO } from '@shared/models/generated-interfaces/IncomingEmailDTOs';

export class IncomingEmailGridDTO implements IIncomingEmailGridDTO {
  incomingEmailId: number;
  senderEmail: string;
  recipientEmails: string;
  date: Date;
  attachementNames: string;
  deliveryStatus?: number;
  deliveryStatusText: string;
  get devliveryStatusIcon(): string {
    let icon = '';
    if (
      this.deliveryStatus &&
      this.deliveryStatus >= 10 &&
      this.deliveryStatus <= 19
    )
      icon = 'shield-check';
    else if (
      this.deliveryStatus &&
      this.deliveryStatus >= 30 &&
      this.deliveryStatus <= 39
    )
      icon = 'triangle-exclamation';

    return icon;
  }
  constructor() {
    this.incomingEmailId = 0;
    this.senderEmail = '';
    this.recipientEmails = '';
    this.date = new Date();
    this.attachementNames = '';
    this.deliveryStatus = 0;
    this.deliveryStatusText = '';
  }
}
