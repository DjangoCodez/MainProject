import { Component, inject, OnInit } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { IPayrollLevelGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { take } from 'rxjs/operators';
import { PayrollLevelsService } from '../../services/payroll-levels.service';

@Component({
  selector: 'soe-payroll-levels-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class PayrollLevelsGridComponent
  extends GridBaseDirective<IPayrollLevelGridDTO, PayrollLevelsService>
  implements OnInit
{
  service = inject(PayrollLevelsService);

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(
      Feature.Time_Employee_PayrollLevels,
      'Time.Employee.PayrollLevels'
    );
  }

  override onGridReadyToDefine(grid: GridComponent<IPayrollLevelGridDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.active',
        'common.name',
        'common.description',
        'core.edit',
        'common.code',
        'common.externalcode',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnActive('state', terms['common.active'], {
          idField: 'payrollLevelId',
        });
        this.grid.addColumnText('name', terms['common.name'], {
          flex: 25,
        });
        this.grid.addColumnText('description', terms['common.description'], {
          flex: 25,
          enableHiding: true,
        });
        this.grid.addColumnText('code', terms['common.code'], {
          flex: 25,
          enableHiding: true,
        });
        this.grid.addColumnText('externalCode', terms['common.externalcode'], {
          flex: 25,
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
