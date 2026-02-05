import { Component, effect, inject, OnInit, signal } from '@angular/core';
import { EconomyService } from '@features/economy/services/economy.service';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  CompanySettingType,
  Feature,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  IAccountDimDTO,
  ITimePeriodHeadGridDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { SettingsUtil } from '@shared/util/settings-util';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable } from 'rxjs';
import { take, tap } from 'rxjs/operators';
import { DistributionRuleService } from '../../services/distribution-rule.service';
import { PlanningPeriodsService } from '../../services/planning-periods.service';
import { SharedPlanningPeriodService } from '../../services/shared-planning-period-service';

@Component({
  selector: 'soe-planning-pteriods-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class PlanningPeriodsGridComponent
  extends GridBaseDirective<ITimePeriodHeadGridDTO, PlanningPeriodsService>
  implements OnInit
{
  service = inject(PlanningPeriodsService);

  useAccountHierarchy: boolean = false;
  useAveragingPeriod: boolean = false;
  defaultEmployeeAccountDimId: number = 0;
  accountDim: IAccountDimDTO | undefined;
  coreService = inject(CoreService);
  accountingService = inject(EconomyService);
  distributionService = inject(DistributionRuleService);
  sharedService = inject(SharedPlanningPeriodService);
  payrollProductDistributionRules: SmallGenericType[] = [];
  name = signal('');

  constructor() {
    super();

    effect(() => {
      const distributionRule = this.sharedService.data;
      if (distributionRule() && distributionRule() != 'init') {
        const rule = this.payrollProductDistributionRules.find(
          x => x.id === distributionRule().payrollProductDistributionRuleHeadId
        );
        if (rule === undefined)
          this.payrollProductDistributionRules.push(
            new SmallGenericType(
              distributionRule().payrollProductDistributionRuleHeadId,
              distributionRule().name
            )
          );
        else rule.name = distributionRule().name;

        if (this.grid) this.grid.refreshCells();
      }
    });
  }

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(
      Feature.Time_Preferences_TimeSettings_PlanningPeriod,
      'Time.Time.PlanningPeriod'
    );
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
        this.loadAccountDim();
      })
    );
  }

  override onGridReadyToDefine(grid: GridComponent<ITimePeriodHeadGridDTO>) {
    this.grid = grid;
  }

  private buildGrid() {
    super.onGridReadyToDefine(this.grid);
    this.translate
      .get([
        'common.name',
        'common.description',
        'core.edit',
        'time.time.planningperiod.child',
        'time.time.planningperiod.planningperiods.distribution.rule',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        if (this.useAccountHierarchy) {
          this.grid.addColumnText('accountName', this.name(), {
            flex: 25,
            enableHiding: true,
          });
        }
        this.grid.addColumnText('name', terms['common.name'], {
          flex: 25,
        });
        this.grid.addColumnText('description', terms['common.description'], {
          flex: 25,
          enableHiding: true,
        });
        if (this.useAveragingPeriod) {
          this.grid.addColumnText(
            'childName',
            terms['time.time.planningperiod.child'],
            {
              flex: 10,
              enableHiding: true,
            }
          );
        }
        this.grid.addColumnSelect(
          'payrollProductDistributionRuleHeadId',
          terms['time.time.planningperiod.planningperiods.distribution.rule'],
          this.payrollProductDistributionRules,
          null,
          {
            flex: 10,
            editable: false,
            enableHiding: true,
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

  private loadAccountDim() {
    return this.accountingService
      .getAccountDimByAccountDimId(this.defaultEmployeeAccountDimId, true)
      .subscribe((dimAcc: IAccountDimDTO) => {
        if (dimAcc) {
          this.accountDim = dimAcc;
          this.name.set(this.accountDim.name);
          this.loadPayrollProductDistributionRules().subscribe(() => {
            this.buildGrid();
          });
        }
      });
  }
  private loadPayrollProductDistributionRules() {
    return this.distributionService.getGrid().pipe(
      tap(x => {
        this.payrollProductDistributionRules = x.map(
          item =>
            new SmallGenericType(
              item.payrollProductDistributionRuleHeadId,
              item.name
            )
        );
      })
    );
  }
}
