import { Component, inject, OnInit, signal } from '@angular/core';
import { AccountDTO } from '@shared/models/account.model';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  CompanySettingType,
  Feature,
} from '@shared/models/generated-interfaces/Enumerations';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { Perform } from '@shared/util/perform.class';
import { SettingsUtil } from '@shared/util/settings-util';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { TimeService } from '../../../services/time.service';
import { StaffingNeedsLocationsService } from '../../services/staffing-needs-locations.service';
import { StaffingNeedsService } from '../../services/staffing-needs.service';
import { StaffingNeedsLocationDTO } from '../../../models/staffing-needs.model';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';

@Component({
  selector: 'soe-staffing-needs-locations-edit',
  templateUrl: './staffing-needs-locations-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class StaffingNeedsLocationsEditComponent
  extends EditBaseDirective<
    StaffingNeedsLocationDTO,
    StaffingNeedsLocationsService
  >
  implements OnInit
{
  service = inject(StaffingNeedsLocationsService);
  private readonly staffingNeedsService = inject(StaffingNeedsService);
  private readonly coreService = inject(CoreService);
  private readonly timeService = inject(TimeService);

  performAccounts = new Perform<AccountDTO[]>(this.progressService);
  performLocationGroups = new Perform<SmallGenericType[]>(this.progressService);

  locationGroups: SmallGenericType[] = [];
  accounts: AccountDTO[] = [];
  useAccountsHierarchy = signal(false);

  get timeScheduleTasks(): SmallGenericType[] {
    return this.timeService.timeScheduleTasks;
  }

  ngOnInit() {
    super.ngOnInit();
    this.startFlow(Feature.Time_Preferences_NeedsSettings_Locations_Edit);
  }

  override onFinished(): void {
    this.loadLocationGroups().subscribe();
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

  private loadLocationGroups(): Observable<any> {
    return this.performLocationGroups.load$(
      this.staffingNeedsService
        .getStaffingNeedsLocationGroupsDict(false, this.useAccountsHierarchy())
        .pipe(
          tap(x => {
            this.locationGroups = x;
            if (this.form?.value[this.idFieldName] > 0)
              this.locationGroups.splice(0, 0, new SmallGenericType(0, ''));
          })
        )
    );
  }
}
