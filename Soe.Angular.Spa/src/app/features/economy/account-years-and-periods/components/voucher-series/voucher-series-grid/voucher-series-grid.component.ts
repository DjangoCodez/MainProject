import { Component, OnInit, inject } from '@angular/core';
import { VoucherSeriesTypeService } from '@features/economy/services/voucher-series-type.service';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { take } from 'rxjs';
import { VoucherSeriesTypeDTO } from '../../../../models/voucher-series-type.model';

@Component({
  selector: 'soe-voucher-series-grid',
  templateUrl:
    '../../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class VoucherSeriesGridComponent
  extends GridBaseDirective<VoucherSeriesTypeDTO, VoucherSeriesTypeService>
  implements OnInit
{
  service = inject(VoucherSeriesTypeService);

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(
      Feature.Billing_Purchase_Delivery_List,
      'economy.accounting.voucherseriestypes'
    );
  }

  override onGridReadyToDefine(grid: GridComponent<VoucherSeriesTypeDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'economy.accounting.voucherseriestype.voucherseriestypenr',
        'economy.accounting.voucherseriestype.name',
        'economy.accounting.voucherseriestype.startnr',
        'core.edit',
        'core.aggrid.totals.filtered',
        'core.aggrid.totals.total',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnNumber(
          'voucherSeriesTypeNr',
          terms['economy.accounting.voucherseriestype.voucherseriestypenr'],
          {
            flex: 30,
            enableHiding: false,
          }
        );
        this.grid.addColumnText(
          'name',
          terms['economy.accounting.voucherseriestype.name'],
          {
            flex: 40,
            enableHiding: false,
          }
        );
        this.grid.addColumnNumber(
          'startNr',
          terms['economy.accounting.voucherseriestype.startnr'],
          {
            flex: 30,
            enableHiding: false,
          }
        );

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
