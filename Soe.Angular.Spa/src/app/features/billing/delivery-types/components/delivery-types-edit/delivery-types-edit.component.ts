import { Component, OnInit, inject } from '@angular/core';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { DeliveryTypesService } from '../../services/delivery-types.service';
import { DeliveryTypeDTO } from '../../models/delivery-types.model';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';

@Component({
  selector: 'soe-delivery-types-edit',
  templateUrl: './delivery-types-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class DeliveryTypesEditComponent
  extends EditBaseDirective<DeliveryTypeDTO, DeliveryTypesService>
  implements OnInit
{
  service = inject(DeliveryTypesService);

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(Feature.Billing_Preferences_DeliveryType_Edit);
  }
}
