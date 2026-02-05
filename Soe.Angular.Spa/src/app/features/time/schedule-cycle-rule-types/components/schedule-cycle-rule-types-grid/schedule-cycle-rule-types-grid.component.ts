import { Component, OnInit, inject, signal } from '@angular/core';
import { Observable } from 'rxjs';
import { take, tap } from 'rxjs/operators';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ToolbarService } from '@shared/ui-components/toolbar/services/toolbar.service';
import { GridComponent } from '@shared/ui-components/grid/grid.component';
import { IScheduleCycleRuleTypeGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { ScheduleCycleRuleTypesService } from '../../services/schedule-cycle-rule-types.service';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { CompanySettingType } from '@shared/models/generated-interfaces/Enumerations';
import { CoreService } from '@shared/services/core.service';
import {
  SettingsUtil,
  UserCompanySettingCollection,
} from '@shared/util/settings-util';
import { TermCollection } from '@shared/localization/term-types';
@Component({
  selector: 'soe-schedule-cycle-rule-types-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class ScheduleCycleRuleTypesGridComponent
  extends GridBaseDirective<
    IScheduleCycleRuleTypeGridDTO,
    ScheduleCycleRuleTypesService
  >
  implements OnInit
{
  service = inject(ScheduleCycleRuleTypesService);
  coreService = inject(CoreService);

  useAccountsHierarchy = signal(false);

  ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(
      Feature.Time_Schedule_StaffingNeeds_ScheduleCycleRuleType,
      'Time.Schedule.ScheduleCycleRuleType'
    );
  }

  override onGridReadyToDefine(
    grid: GridComponent<IScheduleCycleRuleTypeGridDTO>
  ) {
    super.onGridReadyToDefine(grid);
    this.translate
      .get([
        'common.name',
        'common.user.attestrole.accounthierarchy',
        'time.schedule.schedulecycleruletype.weekday',
        'time.schedule.schedulecycleruletype.starttime',
        'time.schedule.schedulecycleruletype.stoptime',
        'core.edit',
      ])
      .pipe(take(1))
      .subscribe((terms: TermCollection) => {
        this.grid.addColumnText('name', terms['common.name'], {
          flex: 15,
        });

        if (this.useAccountsHierarchy()) {
          this.grid.addColumnText(
            'accountName',
            terms['common.user.attestrole.accounthierarchy'],
            {
              flex: 15,
            }
          );
        }

        this.grid.addColumnText(
          'dayOfWeeksGridString',
          terms['time.schedule.schedulecycleruletype.weekday'],
          {
            flex: 40,
          }
        );

        this.grid.addColumnTime(
          'startTime',
          terms['time.schedule.schedulecycleruletype.starttime'],
          {
            flex: 15,
            dateFormat: 'HH:mm',
          }
        );

        this.grid.addColumnTime(
          'stopTime',
          terms['time.schedule.schedulecycleruletype.stoptime'],
          {
            flex: 15,
            dateFormat: 'HH:mm',
          }
        );

        this.grid.addColumnIconEdit({
          tooltip: terms['core.edit'],
          onClick: row => this.edit(row),
        });
        super.finalizeInitGrid();
      });
  }

  override loadCompanySettings(): Observable<void> {
    const settingTypes: number[] = [CompanySettingType.UseAccountHierarchy];
    return this.performLoadData.load$(
      this.coreService.getCompanySettings(settingTypes).pipe(
        tap((settings: UserCompanySettingCollection) => {
          this.useAccountsHierarchy.set(
            SettingsUtil.getBoolCompanySetting(
              settings,
              CompanySettingType.UseAccountHierarchy
            )
          );
        })
      )
    );
  }
}
