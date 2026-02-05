import { Component, OnInit, inject } from '@angular/core';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { PaymentConditionDTO } from '../../models/payment-condition.model';
import { PaymentConditionsService } from '../../services/payment-conditions.service';
import {
  Feature,
  SoeModule,
} from '@shared/models/generated-interfaces/Enumerations';
import { PaymentConditionForm } from '../../models/payment-condition-form.model';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { UrlHelperService } from '@shared/services/url-params.service';

@Component({
  selector: 'soe-payment-conditions-edit',
  templateUrl: './payment-conditions-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class PaymentConditionsEditComponent
  extends EditBaseDirective<
    PaymentConditionDTO,
    PaymentConditionsService,
    PaymentConditionForm
  >
  implements OnInit
{
  service = inject(PaymentConditionsService);
  urlHelper = inject(UrlHelperService);

  get isEconomyModule() {
    return this.urlHelper.module === SoeModule.Economy;
  }

  ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(
      this.isEconomyModule
        ? Feature.Economy_Preferences_PayCondition_Edit
        : Feature.Billing_Preferences_PayCondition_Edit
    );
  }
}
