import { Component, OnInit, inject, signal } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import {
  CompanySettingType,
  Feature,
} from '@shared/models/generated-interfaces/Enumerations';
import { ITimeScheduleTaskTypeGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { SettingsUtil } from '@shared/util/settings-util';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { take, tap } from 'rxjs/operators';
import { TimeScheduleTaskTypesService } from '../../services/time-schedule-task-types.service';

@Component({
  selector: 'soe-time-schedule-task-types-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class TimeScheduleTaskTypesGridComponent
  extends GridBaseDirective<
    ITimeScheduleTaskTypeGridDTO,
    TimeScheduleTaskTypesService
  >
  implements OnInit
{
  service = inject(TimeScheduleTaskTypesService);
  coreService = inject(CoreService);
  useAccountsHierarchy = signal(false);

  ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(
      Feature.Time_Schedule_StaffingNeeds_TaskTypes,
      'time.schedule.timescheduletasktype.types'
    );
  }

  override loadCompanySettings() {
    return this.coreService
      .getCompanySettings([CompanySettingType.UseAccountHierarchy])
      .pipe(
        tap(x => {
          this.useAccountsHierarchy.set(
            SettingsUtil.getBoolCompanySetting(
              x,
              CompanySettingType.UseAccountHierarchy
            )
          );
        })
      );
  }

  override onGridReadyToDefine(
    grid: GridComponent<ITimeScheduleTaskTypeGridDTO>
  ) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.name',
        'common.description',
        'common.user.attestrole.accounthierarchy',
        'core.edit',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText('name', terms['common.name'], {
          flex: 20,
          enableHiding: false,
        });
        this.grid.addColumnText('description', terms['common.description'], {
          flex: 40,
          enableHiding: true,
        });
        if (this.useAccountsHierarchy()) {
          this.grid.addColumnText(
            'accountName',
            terms['common.user.attestrole.accounthierarchy'],
            { flex: 40, enableHiding: true }
          );
        }
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
