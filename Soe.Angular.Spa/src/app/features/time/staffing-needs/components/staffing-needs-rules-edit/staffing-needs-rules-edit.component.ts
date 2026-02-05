import { Component, OnInit, inject, signal } from '@angular/core';
import { tap } from 'rxjs/operators';
import { StaffingNeedsRulesService } from '../../services/staffing-needs-rules.service';
import {
  CompanySettingType,
  Feature,
  TermGroup,
} from '@shared/models/generated-interfaces/Enumerations';
import { StaffingNeedsService } from '../../services/staffing-needs.service';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { SharedService } from '@shared/services/shared.service';
import { DateUtil } from '@shared/util/date-util';
import { Perform } from '@shared/util/perform.class';
import { SettingsUtil } from '@shared/util/settings-util';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { StaffingNeedsRuleDTO } from '../../../models/staffing-needs.model';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { StaffingNeedsRulesForm } from '../../models/staffing-needs-rules-form.model';
import { Observable, of } from 'rxjs';
import { CrudActionTypeEnum } from '@shared/enums';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';

@Component({
  selector: 'soe-staffing-needs-rules-edit',
  templateUrl: './staffing-needs-rules-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class StaffingNeedsRulesEditComponent
  extends EditBaseDirective<
    StaffingNeedsRuleDTO,
    StaffingNeedsRulesService,
    StaffingNeedsRulesForm
  >
  implements OnInit
{
  //@ViewChild(StaffingNeedsRulesEditGridComponent)
  readonly service = inject(StaffingNeedsRulesService);
  private readonly coreService = inject(CoreService);
  private readonly sharedService = inject(SharedService);
  private readonly staffingNeedsService = inject(StaffingNeedsService);

  performLoad = new Perform<any>(this.progressService);

  useAccountsHierarchy = signal(false);
  units: SmallGenericType[] = [];
  accounts: SmallGenericType[] = [];
  locationGroups: SmallGenericType[] = [];

  ngOnInit() {
    super.ngOnInit();
    this.startFlow(Feature.Time_Preferences_NeedsSettings_Rules_Edit, {
      lookups: [this.loadUnits()],
    });
  }

  override onFinished(): void {
    this.loadAccounts().subscribe();
    this.loadLocationGroups().subscribe();
  }

  override loadCompanySettings(): Observable<void> {
    return this.performLoad.load$(
      this.coreService
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
        )
    );
  }

  private loadLocationGroups() {
    this.locationGroups = [];
    return this.staffingNeedsService
      .getStaffingNeedsLocationGroupsDict(false, this.useAccountsHierarchy())
      .pipe(
        tap(x => {
          this.locationGroups = x;
          if (this.form?.value.staffingNeedsRuleId > 0)
            this.locationGroups.splice(0, 0, new SmallGenericType(0, ''));
        })
      );
  }

  loadAccounts(): Observable<void> {
    if (!this.useAccountsHierarchy()) return of(undefined);

    const today = DateUtil.getToday();
    this.accounts = [];
    return this.performLoad.load$(
      this.sharedService
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
            if (this.form?.value.staffingNeedsRuleId > 0) {
              this.accounts.unshift(new SmallGenericType(0, ''));
            }
          })
        )
    );
  }

  private loadUnits() {
    return this.coreService
      .getTermGroupContent(TermGroup.StaffingNeedsRuleUnit, false, true)
      .pipe(
        tap(x => {
          this.units = x;
          if (this.form?.value.staffingNeedsRuleId > 0)
            this.units.unshift(new SmallGenericType(0, ''));
        })
      );
  }

  loadData(): Observable<void> {
    return this.performLoadData.load$(
      this.service.get(this.form?.getIdControl()?.value).pipe(
        tap((value: StaffingNeedsRuleDTO) => {
          this.form?.customPatch(value);
        })
      )
    );
  }

  override performSave() {
    if (!this.form || this.form.invalid || !this.service) return;
    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.service.save(this.form?.getAllValues()).pipe(
        tap(res => {
          this.updateFormValueAndEmitChange(res);
          if (res.success) this.triggerCloseDialog(res);
        })
      )
    );
  }
}
