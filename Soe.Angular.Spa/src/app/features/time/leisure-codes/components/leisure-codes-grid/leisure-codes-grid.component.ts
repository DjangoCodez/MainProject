import { Component, inject, OnInit } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { IEmployeeGroupTimeLeisureCodeGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { take } from 'rxjs/operators';
import { LeisureCodesService } from '../../services/leisure-codes.service';

@Component({
  selector: 'soe-leisure-codes-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class LeisureCodesGridComponent
  extends GridBaseDirective<
    IEmployeeGroupTimeLeisureCodeGridDTO,
    LeisureCodesService
  >
  implements OnInit
{
  service = inject(LeisureCodesService);

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(
      Feature.Time_Preferences_ScheduleSettings_LeisureCode,
      'Time.Schedule.LeisureCodes'
    );
  }

  override onGridReadyToDefine(
    grid: GridComponent<IEmployeeGroupTimeLeisureCodeGridDTO>
  ) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.name',
        'time.employee.employeegroup.employeegroup',
        'time.schedule.leisurecode.leisurecodetype',
        'time.schedule.leisurecode.datefrom',
        'core.edit',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText(
          'timeLeisureCodeName',
          terms['time.schedule.leisurecode.leisurecodetype'],
          {
            flex: 50,
          }
        );
        this.grid.addColumnText(
          'employeeGroupName',
          terms['time.employee.employeegroup.employeegroup'],
          {
            flex: 20,
            enableHiding: true,
          }
        );
        this.grid.addColumnDate(
          'dateFrom',
          terms['time.schedule.leisurecode.datefrom'],
          {
            flex: 20,
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
