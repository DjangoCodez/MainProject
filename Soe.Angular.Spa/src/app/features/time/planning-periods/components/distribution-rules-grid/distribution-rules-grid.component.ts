import { Component, OnInit, inject } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { IPayrollProductDistributionRuleHeadGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { take } from 'rxjs';
import { DistributionRuleService } from '../../services/distribution-rule.service';

@Component({
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class DistributionRulesGridComponent
  extends GridBaseDirective<
    IPayrollProductDistributionRuleHeadGridDTO,
    DistributionRuleService
  >
  implements OnInit
{
  service = inject(DistributionRuleService);

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(
      Feature.Time_Preferences_TimeSettings_PlanningPeriod,
      'Time.Time.PlanningPeriod.DistributionRules'
    );
  }

  override onGridReadyToDefine(
    grid: GridComponent<IPayrollProductDistributionRuleHeadGridDTO>
  ) {
    this.grid = grid;

    super.onGridReadyToDefine(this.grid);
    this.translate
      .get(['common.name', 'common.description', 'core.edit'])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText('name', terms['common.name'], {
          flex: 40,
        });
        this.grid.addColumnText('description', terms['common.description'], {
          flex: 40,
          enableHiding: true,
        });
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
