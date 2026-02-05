import { Component, OnInit, inject } from '@angular/core';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { DeliveryConditionService } from '../../services/delivery-condition.service';
import { DeliveryConditionDTO } from '../../models/delivery-condition.model';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';

@Component({
  selector: 'soe-delivery-condition-edit',
  templateUrl: './delivery-condition-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class DeliveryConditionEditComponent
  extends EditBaseDirective<DeliveryConditionDTO, DeliveryConditionService>
  implements OnInit
{
  service = inject(DeliveryConditionService);

  ngOnInit() {
    super.ngOnInit();
    this.startFlow(Feature.Billing_Preferences_DeliveryCondition_Edit);
  }
}
