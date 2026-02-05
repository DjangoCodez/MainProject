import { Component, inject, input, output } from '@angular/core';
import { ValidationHandler } from '@shared/handlers';
import { IIncomingEmailFilterDTO } from '@shared/models/generated-interfaces/IncomingEmailDTOs';
import { InboundEmailGridFilterForm } from '../../models/inbound-email-grid-filter-form.models';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { SoeFormGroup } from '@shared/extensions';

@Component({
  selector: 'soe-inbound-email-grid-filter',
  templateUrl: './inbound-email-grid-filter.component.html',
  standalone: false,
})
export class InboundEmailGridFilterComponent {
  deliveryStatusOptions = input<ISmallGenericType[]>([]);
  searchEmails = output<IIncomingEmailFilterDTO>();
  private readonly validationHandler = inject(ValidationHandler);
  protected form: InboundEmailGridFilterForm = new InboundEmailGridFilterForm({
    validationHandler: this.validationHandler,
    element: <IIncomingEmailFilterDTO>{},
  });

  protected triggerSearch(): void {
    this.searchEmails.emit(<IIncomingEmailFilterDTO>this.form.getRawValue());
  }

  protected openFormValidationErrors(): void {
    this.validationHandler.showFormValidationErrors(this.form as SoeFormGroup);
  }
}
