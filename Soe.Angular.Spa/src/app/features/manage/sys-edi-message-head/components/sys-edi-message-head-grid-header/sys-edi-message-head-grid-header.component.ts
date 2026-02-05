import { Component, EventEmitter, inject, Output } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { SoeCheckboxFormControl, SoeFormGroup } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';

@Component({
  selector: 'soe-sys-edi-message-head-grid-header',
  templateUrl: './sys-edi-message-head-grid-header.component.html',
  standalone: false,
})
export class SysEdiMessageHeadGridHeaderComponent {
  @Output() openMessagesChanged = new EventEmitter<boolean>();
  @Output() closedMessagesChanged = new EventEmitter<boolean>();
  @Output() rawMessagesChanged = new EventEmitter<boolean>();

  validationHandler = inject(ValidationHandler);
  translate = inject(TranslateService);

  form = new SoeFormGroup(this.validationHandler, {
    openMessages: new SoeCheckboxFormControl(true),
    closedMessages: new SoeCheckboxFormControl(false),
    rawMessages: new SoeCheckboxFormControl(false),
  });
}
