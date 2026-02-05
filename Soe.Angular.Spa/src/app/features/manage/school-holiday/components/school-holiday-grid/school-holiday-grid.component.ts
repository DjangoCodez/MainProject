import { Component, inject, OnInit } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { ISchoolHolidayGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { take } from 'rxjs/operators';
import { SchoolHolidayService } from '../../services/school-holiday.service';

@Component({
  selector: 'soe-school-holiday-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class SchoolHolidayGridComponent
  extends GridBaseDirective<ISchoolHolidayGridDTO, SchoolHolidayService>
  implements OnInit
{
  service = inject(SchoolHolidayService);

  ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(
      Feature.Manage_Preferences_Registry_SchoolHoliday,
      'Manage.Calendar.SchoolHoliday'
    );
  }

  override onGridReadyToDefine(grid: GridComponent<ISchoolHolidayGridDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get(['common.name', 'common.fromdate', 'common.todate', 'core.edit'])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText('name', terms['common.name'], {
          flex: 40,
          enableHiding: false,
        });
        this.grid.addColumnDate('dateFrom', terms['common.fromdate'], {
          flex: 30,
          enableHiding: true,
        });
        this.grid.addColumnDate('dateTo', terms['common.todate'], {
          flex: 30,
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
