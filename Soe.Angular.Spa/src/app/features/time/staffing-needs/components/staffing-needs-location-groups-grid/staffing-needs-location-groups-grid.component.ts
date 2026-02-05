import { Component, OnInit, inject, signal } from '@angular/core';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { take, tap } from 'rxjs/operators';
import {
  CompanySettingType,
  Feature,
} from '@shared/models/generated-interfaces/Enumerations';
import { IStaffingNeedsLocationGroupGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { SettingsUtil } from '@shared/util/settings-util';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { StaffingNeedsLocationGroupsService } from '../../services/staffing-needs-location-groups.service';

@Component({
  selector: 'soe-staffing-needs-location-groups-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class StaffingNeedsLocationGroupsGridComponent
  extends GridBaseDirective<
    IStaffingNeedsLocationGroupGridDTO,
    StaffingNeedsLocationGroupsService
  >
  implements OnInit
{
  service = inject(StaffingNeedsLocationGroupsService);
  coreService = inject(CoreService);
  useAccountsHierarchy = signal(false);

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(
      Feature.Time_Preferences_NeedsSettings_LocationGroups,
      'Time.Schedule.StaffingNeedsLocationGroups'
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
    grid: GridComponent<IStaffingNeedsLocationGroupGridDTO>
  ) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.name',
        'common.description',
        'common.user.attestrole.accounthierarchy',
        'time.schedule.staffingneedslocationgroup.timescheduletask',
        'core.edit',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText('name', terms['common.name'], {
          flex: 25,
        });
        this.grid.addColumnText('description', terms['common.description'], {
          flex: 50,
        });
        if (this.useAccountsHierarchy()) {
          this.grid.addColumnText(
            'accountName',
            terms['common.user.attestrole.accounthierarchy'],
            {
              flex: 25,
            }
          );
        }
        this.grid.addColumnText(
          'timeScheduleTaskName',
          terms['time.schedule.staffingneedslocationgroup.timescheduletask'],
          {
            flex: 25,
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
