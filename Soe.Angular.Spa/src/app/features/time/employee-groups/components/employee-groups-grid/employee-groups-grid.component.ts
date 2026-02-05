import { Component, inject, OnInit } from '@angular/core';
import { IEmployeeGroupGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { EmployeeGroupsService } from '../../services/employee-groups.service';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { GridComponent } from '@ui/grid/grid.component';
import { Observable, take, tap } from 'rxjs';
import { SmallGenericType } from '@shared/models/generic-type.model';

@Component({
  selector: 'soe-employee-groups-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class EmployeeGroupsGridComponent
  extends GridBaseDirective<IEmployeeGroupGridDTO, EmployeeGroupsService>
  implements OnInit
{
  service = inject(EmployeeGroupsService);
  timeReportTypes: SmallGenericType[] = [];

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(
      Feature.Time_Employee_Groups,
      'time.employee.employeegroups',
      {
        lookups: [this.loadTimeReportTypes()],
      }
    );
  }

  override onGridReadyToDefine(grid: GridComponent<IEmployeeGroupGridDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.name',
        'time.employee.employeegroup.daytypes',
        'time.employee.employeegroup.timedeviationcauses',
        'time.employee.employeegroup.timereporttype',
        'core.edit',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText('name', terms['common.name'], { flex: 10 });
        this.grid.addColumnText(
          'dayTypesNames',
          terms['time.employee.employeegroup.daytypes'],
          {
            flex: 10,
          }
        );
        this.grid.addColumnText(
          'timeDeviationCausesNames',
          terms['time.employee.employeegroup.timedeviationcauses'],
          {
            flex: 10,
          }
        );
        this.grid.addColumnSelect(
          'timeReportType',
          terms['time.employee.employeegroup.timereporttype'],
          this.service.performTimeReportTypes.data || [],
          null,
          {
            dropDownIdLabel: 'id',
            dropDownValueLabel: 'name',
            flex: 10,
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

  private loadTimeReportTypes(): Observable<SmallGenericType[]> {
    return this.service.getTimeReportTypes();
  }
}
