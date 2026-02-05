import { Component, inject, OnInit } from '@angular/core';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  CompanySettingType,
  Feature,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  IAccountDimSmallDTO,
  IAccountDTO,
  ITimePeriodHeadDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { SharedService } from '@shared/services/shared.service';
import { DateUtil } from '@shared/util/date-util';
import { SettingsUtil } from '@shared/util/settings-util';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable, of, tap } from 'rxjs';
import { PlanningPeriodsForm } from '../../models/planning-periods-form.model';
import { DistributionRuleService } from '../../services/distribution-rule.service';
import { PlanningPeriodsService } from '../../services/planning-periods.service';

@Component({
  selector: 'soe-planning-periods-edit',
  templateUrl: './planning-periods-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class PlanningPeriodsEditComponent
  extends EditBaseDirective<
    ITimePeriodHeadDTO,
    PlanningPeriodsService,
    PlanningPeriodsForm
  >
  implements OnInit
{
  // Services
  readonly service = inject(PlanningPeriodsService);
  private readonly coreService = inject(CoreService);
  private readonly distributionService = inject(DistributionRuleService);
  readonly periodToolbarService = inject(ToolbarService);
  private readonly sharedService = inject(SharedService);

  // Company settings
  useAccountHierarchy: any;
  useAveragingPeriod: boolean = false;
  defaultEmployeeAccountDimId: number = 0;

  // Lookups
  private accountIds: number[] = [];
  public accountDim: IAccountDimSmallDTO | undefined;
  private timePeriodHeads: SmallGenericType[] = [];
  public filteredTimePeriodHeads: SmallGenericType[] = [];
  public payrollProductDistributionRules: SmallGenericType[] = [];

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(Feature.Time_Preferences_TimeSettings_PlanningPeriod, {
      lookups: [
        this.loadTimePeriodHeads(),
        this.loadPayrollProductDistributionRules(),
      ],
    });
  }

  override loadCompanySettings(): Observable<void> {
    const settingTypes: CompanySettingType[] = [
      CompanySettingType.UseAccountHierarchy,
      CompanySettingType.DefaultEmployeeAccountDimEmployee,
      CompanySettingType.TimeCalculatePlanningPeriodScheduledTimeUseAveragingPeriod,
    ];
    return this.coreService.getCompanySettings(settingTypes).pipe(
      tap((settings: any) => {
        this.useAccountHierarchy = SettingsUtil.getBoolCompanySetting(
          settings,
          CompanySettingType.UseAccountHierarchy,
          false
        );
        this.useAveragingPeriod = SettingsUtil.getBoolCompanySetting(
          settings,
          CompanySettingType.TimeCalculatePlanningPeriodScheduledTimeUseAveragingPeriod,
          false
        );
        this.defaultEmployeeAccountDimId = SettingsUtil.getIntCompanySetting(
          settings,
          CompanySettingType.DefaultEmployeeAccountDimEmployee,
          0
        );
        this.loadAccountsByUserFromHierarchy();
      })
    );
  }

  private loadAccountsByUserFromHierarchy(): Observable<any> {
    if (this.useAccountHierarchy) {
      this.sharedService
        .getAccountIdsFromHierarchyByUser(
          DateUtil.getToday(),
          DateUtil.getToday()
        )
        .subscribe(x => {
          this.accountIds = x;
          this.loadAccountDim();
        });
    }
    return of();
  }

  private loadAccountDim() {
    this.service
      .getDim(this.defaultEmployeeAccountDimId, true, false)
      .subscribe((x: IAccountDimSmallDTO) => {
        this.accountDim = x;
        this.accountDim.accounts = this.accountDim.accounts.filter(a =>
          this.accountIds.includes(a.accountId)
        );
        this.accountDim.accounts.unshift({
          accountId: 0,
          name: '',
        } as IAccountDTO);
      });
  }

  private loadTimePeriodHeads(): Observable<void> {
    return this.performLoadData.load$(
      this.service.getGrid().pipe(
        tap(x => {
          this.timePeriodHeads = x.map(
            item => new SmallGenericType(item.timePeriodHeadId, item.name)
          );
          this.setFilteredTimePeriodHeads();
        })
      )
    );
  }

  private loadPayrollProductDistributionRules(): Observable<void> {
    return this.performLoadData.load$(
      this.distributionService.getGrid().pipe(
        tap(x => {
          const rules = x.map(
            item =>
              new SmallGenericType(
                item.payrollProductDistributionRuleHeadId,
                item.name
              )
          );
          this.payrollProductDistributionRules = [{ id: 0, name: '' }].concat(
            rules
          );
        })
      )
    );
  }

  override loadData(): Observable<void> {
    return this.performLoadData.load$(
      this.service.get(this.form?.getIdControl()?.value).pipe(
        tap((value: ITimePeriodHeadDTO) => {
          this.form?.customPatchValue(value);
        })
      )
    );
  }

  override newRecord(): Observable<void> {
    if (this.form?.isCopy) {
      this.form?.customPatchValue(this.form.value, true);
    }
    return of(undefined);
  }

  // HELPER METHODS
  private setFilteredTimePeriodHeads() {
    this.filteredTimePeriodHeads = [{ id: 0, name: '' }].concat(
      this.timePeriodHeads.filter(
        t => t.id !== this.form?.value.timePeriodHeadId
      )
    );
  }
}
