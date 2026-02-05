import { Component, inject, OnInit, signal } from '@angular/core';
import { DateUtil } from '@shared/util/date-util';
import { SettingsUtil } from '@shared/util/settings-util';
import { TimeScheduleTaskTypesService } from '../../services/time-schedule-task-types.service';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { SharedService } from '@shared/services/shared.service';
import {
  CompanySettingType,
  Feature,
} from '@shared/models/generated-interfaces/Enumerations';
import { tap } from 'rxjs/operators';
import { AccountDTO } from '@shared/models/account.model';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { Observable, of } from 'rxjs';
import { ITimeScheduleTaskTypeDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';

@Component({
  selector: 'soe-time-schedule-task-types-edit',
  templateUrl: './time-schedule-task-types-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class TimeScheduleTaskTypesEditComponent
  extends EditBaseDirective<
    ITimeScheduleTaskTypeDTO,
    TimeScheduleTaskTypesService
  >
  implements OnInit
{
  service = inject(TimeScheduleTaskTypesService);
  coreService = inject(CoreService);
  sharedService = inject(SharedService);

  useAccountsHierarchy = signal(false);
  accounts: SmallGenericType[] = [];

  ngOnInit() {
    super.ngOnInit();
    this.startFlow(Feature.Time_Schedule_StaffingNeeds_TaskTypes, {});
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
          if (this.useAccountsHierarchy()) {
            this.loadAccounts().subscribe(); // Instead of in lookup for timing reasons
          }
        })
      );
  }

  private loadAccounts(): Observable<AccountDTO[] | undefined> {
    if (!this.useAccountsHierarchy()) return of(undefined);

    const today = DateUtil.getToday();
    this.accounts = [];
    return this.sharedService
      .getAccountsFromHierarchyByUserSetting(
        today,
        today,
        true,
        false,
        false,
        true
      )
      .pipe(
        tap(accounts => {
          this.accounts = accounts.map(a => ({
            id: a.accountId,
            name: a.name,
          }));
          this.accounts.unshift(new SmallGenericType(0, ''));
        })
      );
  }
}
