import { Component, OnInit, inject } from '@angular/core';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProductGroupsService } from '../../services/product-groups.service';
import { ProductGroupDTO } from '../../models/product-groups.model';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { ProductGroupsForm } from '../../models/product-groups-form.model';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';

@Component({
  selector: 'soe-product-groups-edit',
  templateUrl: './product-groups-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class ProductGroupsEditComponent
  extends EditBaseDirective<
    ProductGroupDTO,
    ProductGroupsService,
    ProductGroupsForm
  >
  implements OnInit
{
  service = inject(ProductGroupsService);

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(
      Feature.Billing_Preferences_ProductSettings_ProductGroup_Edit
    );
  }
}
