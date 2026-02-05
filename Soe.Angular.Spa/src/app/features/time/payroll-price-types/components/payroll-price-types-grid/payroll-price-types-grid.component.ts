import { Component, inject, OnInit } from '@angular/core';
import { IPayrollPriceTypeGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { PayrollPriceTypesService } from '../../services/payroll-price-types.service';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { take } from 'rxjs';

@Component({
  selector: 'soe-payroll-price-types-grid',
  standalone: false,
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
})
export class PayrollPriceTypesGridComponent
  extends GridBaseDirective<IPayrollPriceTypeGridDTO, PayrollPriceTypesService>
  implements OnInit
{
  service = inject(PayrollPriceTypesService);

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(
      Feature.Time_Preferences_SalarySettings_PriceType,
      'Time.Payroll.PayrollPriceType.PayrollPriceTypes'
    );
  }

  override onGridReadyToDefine(grid: GridComponent<IPayrollPriceTypeGridDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.type',
        'common.code',
        'common.name',
        'common.description',
        'core.edit',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText('typeName', terms['common.type'], {
          flex: 15,
          enableHiding: true,
        });
        this.grid.addColumnText('code', terms['common.code'], {
          flex: 15,
        });
        this.grid.addColumnText('name', terms['common.name'], {
          flex: 35,
          enableHiding: true,
        });
        this.grid.addColumnText('description', terms['common.description'], {
          flex: 35,
          resizable: false,
          suppressSizeToFit: true,
          enableHiding: true,
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
