import { Component, inject, OnInit } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { IUnionFeeGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { take } from 'rxjs/operators';
import { UnionFeesService } from '../../services/union-fees.service';

@Component({
  selector: 'soe-union-fees-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class UnionFeesGridComponent
  extends GridBaseDirective<IUnionFeeGridDTO, UnionFeesService>
  implements OnInit
{
  service = inject(UnionFeesService);

  ngOnInit() {
    super.ngOnInit();
    this.startFlow(Feature.Time_Payroll_UnionFee, 'Time.Payroll.UnionFees');
  }

  override onGridReadyToDefine(grid: GridComponent<IUnionFeeGridDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.name',
        'common.payrollproduct',
        'time.payroll.unionfee.percent',
        'time.payroll.unionfee.percentceiling',
        'time.payroll.unionfee.fixedamount',
        'core.edit',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText('name', terms['common.name'], { flex: 20 });
        this.grid.addColumnText(
          'payrollProductName',
          terms['common.payrollproduct'],
          { flex: 20, enableHiding: true }
        );
        this.grid.addColumnText(
          'payrollPriceTypeIdPercentName',
          terms['time.payroll.unionfee.percent'],
          { flex: 20, enableHiding: true }
        );
        this.grid.addColumnText(
          'payrollPriceTypeIdPercentCeilingName',
          terms['time.payroll.unionfee.percentceiling'],
          { flex: 20, enableHiding: true }
        );
        this.grid.addColumnText(
          'payrollPriceTypeIdFixedAmountName',
          terms['time.payroll.unionfee.fixedamount'],
          {
            flex: 20,
            resizable: false,
            suppressSizeToFit: true,
            enableHiding: true,
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
