import { Component, inject, OnInit } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { IDeliveryConditionGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { take } from 'rxjs/operators';
import { DeliveryConditionService } from '../../services/delivery-condition.service';

@Component({
  selector: 'soe-delivery-condition-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class DeliveryConditionGridComponent
  extends GridBaseDirective<IDeliveryConditionGridDTO, DeliveryConditionService>
  implements OnInit
{
  service = inject(DeliveryConditionService);

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(
      Feature.Billing_Preferences_DeliveryCondition,
      'Billing.Invoices.DeliveryConditions'
    );
  }

  override onGridReadyToDefine(grid: GridComponent<IDeliveryConditionGridDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get(['common.code', 'common.name', 'core.edit'])
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
