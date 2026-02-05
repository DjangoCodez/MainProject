import { Component, OnInit, inject } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { IAnnualLeaveGroupGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { take } from 'rxjs/operators';
import { AnnualLeaveGroupsService } from '../../services/annual-leave-groups.service';

@Component({
  selector: 'soe-annual-leave-groups-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class AnnualLeaveGroupsGridComponent
  extends GridBaseDirective<IAnnualLeaveGroupGridDTO, AnnualLeaveGroupsService>
  implements OnInit
{
  service = inject(AnnualLeaveGroupsService);

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(
      Feature.Time_Employee_AnnualLeaveGroups,
      'Time.Employee.AnnualLeaveGroups'
    );
  }

  override onGridReadyToDefine(grid: GridComponent<IAnnualLeaveGroupGridDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.name',
        'common.description',
        'core.edit',
        'common.type',
        'time.time.timedeviationcause.timedeviationcause',
        'time.employee.annualleavegroups.qualifyingmonths',
        'time.employee.annualleavegroups.gapdays',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText('name', terms['common.name'], {
          flex: 20,
        });
        this.grid.addColumnText('description', terms['common.description'], {
          flex: 30,
          enableHiding: true,
        });
        this.grid.addColumnText('typeName', terms['common.type'], {
          flex: 20,
          enableHiding: true,
        });
        this.grid.addColumnText(
          'timeDeviationCauseName',
          terms['time.time.timedeviationcause.timedeviationcause'],
          {
            flex: 10,
            enableHiding: true,
          }
        );
        this.grid.addColumnText(
          'qualifyingMonths',
          terms['time.employee.annualleavegroups.qualifyingmonths'],
          {
            flex: 10,
            enableHiding: true,
          }
        );
        this.grid.addColumnText(
          'gapDays',
          terms['time.employee.annualleavegroups.gapdays'],
          {
            flex: 10,
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
