import { Component, OnInit, inject } from '@angular/core';
import { GridComponent } from '@shared/ui-components/grid/grid.component';
import { take, tap } from 'rxjs/operators';
import {
  CompanySettingType,
  Feature,
} from '@shared/models/generated-interfaces/Enumerations';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { SettingsUtil } from '@shared/util/settings-util';
import { IStaffingNeedsRuleGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { StaffingNeedsRulesService } from '../../services/staffing-needs-rules.service';

@Component({
  selector: 'soe-staffing-needs-rules-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class StaffingNeedsRulesGridComponent
  extends GridBaseDirective<
    IStaffingNeedsRuleGridDTO,
    StaffingNeedsRulesService
  >
  implements OnInit
{
  service = inject(StaffingNeedsRulesService);
  coreService = inject(CoreService);
  useAccountsHierarchy = false;

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(
      Feature.Time_Preferences_NeedsSettings_Rules,
      'Time.Schedule.StaffingNeedsRules'
    );
  }

  override loadCompanySettings() {
    return this.coreService
      .getCompanySettings([CompanySettingType.UseAccountHierarchy])
      .pipe(
        tap(x => {
          this.useAccountsHierarchy = SettingsUtil.getBoolCompanySetting(
            x,
            CompanySettingType.UseAccountHierarchy
          );
        })
      );
  }

  override onGridReadyToDefine(grid: GridComponent<IStaffingNeedsRuleGridDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.name',
        'common.group',
        'common.user.attestrole.accounthierarchy',
        'time.schedule.staffingneedsrule.maxquantity',
        'core.edit',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText('name', terms['common.name'], {
          flex: 40,
        });
        this.grid.addColumnText('groupName', terms['common.group'], {
          flex: 40,
        });
        if (this.useAccountsHierarchy)
          this.grid.addColumnText(
            'accountName',
            terms['common.user.attestrole.accounthierarchy'],
            {
              flex: 40,
            }
          );
        this.grid.addColumnNumber(
          'maxQuantity',
          terms['time.schedule.staffingneedsrule.maxquantity'],
          {
            flex: 20,
          }
        );
        this.grid.addColumnIconEdit({
          tooltip: terms['common.edit'],
          onClick: row => {
            this.edit(row);
          },
        });
        super.finalizeInitGrid();
      });
  }
}
