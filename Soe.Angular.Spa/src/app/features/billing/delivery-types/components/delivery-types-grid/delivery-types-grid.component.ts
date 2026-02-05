import { Component, inject, OnInit } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { IDeliveryTypeGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { take } from 'rxjs/operators';
import { DeliveryTypesService } from '../../services/delivery-types.service';
@Component({
  selector: 'soe-delivery-types-grid',
  templateUrl: './delivery-types-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class DeliveryTypesGridComponent
  extends GridBaseDirective<IDeliveryTypeGridDTO, DeliveryTypesService>
  implements OnInit
{
  service = inject(DeliveryTypesService);

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(
      Feature.Billing_Preferences_DeliveryType,
      'Billing.Invoices.DeliveryType'
    );
  }

  override onGridReadyToDefine(grid: GridComponent<IDeliveryTypeGridDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get(['common.name', 'common.code', 'core.edit'])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText('code', terms['common.code'], {
          flex: 25,
          enableHiding: false,
        });
        this.grid.addColumnText('name', terms['common.name'], {
          flex: 75,
          enableHiding: false,
        });
        this.grid.addColumnIconEdit({
          tooltip: terms['core.edit'],
          onClick: row => {
            this.edit(row);
          },
        });
        super.finalizeInitGrid();
      });
  }
}
