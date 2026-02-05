import { Component, inject, OnInit, signal } from '@angular/core';
import { AccountDTO } from '@shared/models/account.model';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  CompanySettingType,
  Feature,
} from '@shared/models/generated-interfaces/Enumerations';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { SharedService } from '@shared/services/shared.service';
import { DateUtil } from '@shared/util/date-util';
import { Perform } from '@shared/util/perform.class';
import { SettingsUtil } from '@shared/util/settings-util';
import { Observable, of } from 'rxjs';
import { tap } from 'rxjs/operators';
import { TimeService } from '../../../services/time.service';
import { StaffingNeedsLocationGroupsService } from '../../services/staffing-needs-location-groups.service';
import { StaffingNeedsLocationGroupDTO } from '../../../models/staffing-needs.model';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';

@Component({
  selector: 'soe-staffing-needs-location-groups-edit',
  templateUrl: './staffing-needs-location-groups-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class StaffingNeedsLocationGroupsEditComponent
  extends EditBaseDirective<
    StaffingNeedsLocationGroupDTO,
    StaffingNeedsLocationGroupsService
  >
  implements OnInit
{
  service = inject(StaffingNeedsLocationGroupsService);
  private readonly coreService = inject(CoreService);
  private readonly sharedService = inject(SharedService);
  private readonly timeService = inject(TimeService);
  performAccounts = new Perform<AccountDTO[]>(this.progressService);
  performScheduleTasks = new Perform<SmallGenericType[]>(this.progressService);

  accounts: AccountDTO[] = [];
  useAccountsHierarchy = signal(false);

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(Feature.Time_Preferences_NeedsSettings_LocationGroups_Edit, {
      lookups: [this.loadTimeScheduleTasks()],
    });
  }

  override onFinished(): void {
    this.loadAccounts().subscribe();
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

  private loadTimeScheduleTasks() {
    return this.performScheduleTasks.load$(
      this.timeService.getTimeScheduleTasksDict(true)
    );
  }

  private loadAccounts(): Observable<any> {
    if (!this.useAccountsHierarchy()) return of(undefined);
    const today = DateUtil.getToday();

    return this.performAccounts.load$(
      this.sharedService.getAccountsFromHierarchyByUserSetting(
        today,
        today,
        true,
        false,
        false,
        true
      )
    );
  }
}
