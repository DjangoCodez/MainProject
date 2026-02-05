import { Component, OnInit, inject, signal } from '@angular/core';
import { Observable } from 'rxjs';
import { take, tap } from 'rxjs/operators';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ToolbarService } from '@shared/ui-components/toolbar/services/toolbar.service';
import { GridComponent } from '@shared/ui-components/grid/grid.component';
import { IScheduleCycleGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { ScheduleCyclesService } from '../../services/schedule-cycles.service';
import {
  Feature,
  CompanySettingType,
} from '@shared/models/generated-interfaces/Enumerations';
import { CoreService } from '@shared/services/core.service';
import {
  SettingsUtil,
  UserCompanySettingCollection,
} from '@shared/util/settings-util';
import { TermCollection } from '@shared/localization/term-types';

@Component({
  selector: 'soe-schedule-cycles-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  standalone: false,
  providers: [FlowHandlerService, ToolbarService],
})
export class ScheduleCyclesGridComponent
  extends GridBaseDirective<IScheduleCycleGridDTO, ScheduleCyclesService>
  implements OnInit
{
  service = inject(ScheduleCyclesService);
  private readonly coreService = inject(CoreService);

  // Company settings
  useAccountsHierarchy = signal(false);

  ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(
      Feature.Time_Schedule_StaffingNeeds_ScheduleCycle,
      'Time.Schedule.ScheduleCycle'
    );
  }

  override onGridReadyToDefine(grid: GridComponent<IScheduleCycleGridDTO>) {
    super.onGridReadyToDefine(grid);
    this.translate
      .get([
        'common.name',
        'common.description',
        'common.user.attestrole.accounthierarchy',
        'time.schedule.schedulecycle.nbrofweeks',
        'core.edit',
      ])
      .pipe(take(1))
      .subscribe((terms: TermCollection) => {
        this.grid.addColumnText('name', terms['common.name'], {
          flex: 20,
        });

        this.grid.addColumnText('description', terms['common.description'], {
          flex: 40,
        });

        if (this.useAccountsHierarchy()) {
          this.grid.addColumnText(
            'accountName',
            terms['common.user.attestrole.accounthierarchy'],
            {
              flex: 20,
              enableHiding: true,
            }
          );
        }

        this.grid.addColumnNumber(
          'nbrOfWeeks',
          terms['time.schedule.schedulecycle.nbrofweeks'],
          {
            flex: 15,
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
          this.useAccountsHierarchy.set(SettingsUtil.getBoolCompanySetting(
            settings,
            CompanySettingType.UseAccountHierarchy
          ));
        })
      )
    );
  }
}
