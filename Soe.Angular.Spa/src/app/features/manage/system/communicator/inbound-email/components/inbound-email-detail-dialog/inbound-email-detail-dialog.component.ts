import { Component, inject, OnInit } from '@angular/core';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { InboundEmailDialogData } from '../../models/inbound-email-dialog-data.model';
import { InboundEmailService } from '../../services/inbound-email.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { Perform } from '@shared/util/perform.class';
import {
  IIncomingEmailAttachmentDTO,
  IIncomingEmailDTO,
} from '@shared/models/generated-interfaces/IncomingEmailDTOs';
import { InboundEmailForm } from '../../models/inbound-email-form.model';
import { ValidationHandler } from '@shared/handlers';
import { tap } from 'rxjs';
import { DownloadUtility } from '@shared/util/download-util';
import { CrudActionTypeEnum } from '@shared/enums';
import { ResponseUtil } from '@shared/util/response-util';

@Component({
  selector: 'soe-inbound-email-detail-dialog',
  templateUrl: './inbound-email-detail-dialog.component.html',
  standalone: false,
})
export class InboundEmailDetailDialogComponent
  extends DialogComponent<InboundEmailDialogData>
  implements OnInit
{
  private readonly service = inject(InboundEmailService);
  private readonly progress = inject(ProgressService);
  private readonly performLoadEmail = new Perform<IIncomingEmailDTO | string>(
    this.progress
  );
  private readonly validationHandler = inject(ValidationHandler);
  protected form: InboundEmailForm = new InboundEmailForm({
    validationHandler: this.validationHandler,
    element: <IIncomingEmailDTO>{},
  });

  constructor() {
    super();
  }

  ngOnInit(): void {
    this.performLoadEmail.load(
      this.service.get(this.data.inboundEmailId).pipe(
        tap(email => {
          this.data.title = `${this.data.title} - ${email.subject}`;
          this.form.customPathValue(email);
        })
      )
    );
  }

  protected downloadAttachment(attachment: IIncomingEmailAttachmentDTO): void {
    this.performLoadEmail.crud(
      CrudActionTypeEnum.Work,
      this.service.getAttachement(attachment.id).pipe(
        tap(result => {
          const stringValue = ResponseUtil.getStringValue(result);
          if (stringValue) {
            DownloadUtility.downloadFile(
              attachment.fileName,
              attachment.contentType,
              stringValue
            );
          }
        })
      )
    );
  }
}
