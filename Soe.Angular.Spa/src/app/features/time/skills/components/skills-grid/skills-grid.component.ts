import { Component, inject, OnInit } from '@angular/core';
import { GridComponent } from '@shared/ui-components/grid/grid.component';
import { take } from 'rxjs/operators';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ISkillGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { SkillsService } from '../../services/skills.service';

@Component({
  selector: 'soe-skills-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class SkillsGridComponent
  extends GridBaseDirective<ISkillGridDTO, SkillsService>
  implements OnInit
{
  service = inject(SkillsService);

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(
      Feature.Time_Preferences_ScheduleSettings_Skill,
      'Time.Schedule.Skills'
    );
  }

  override onGridReadyToDefine(grid: GridComponent<ISkillGridDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get(['common.type', 'common.name', 'common.description', 'core.edit'])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText('skillTypeName', terms['common.type'], {
          flex: 20,
        });
        this.grid.addColumnText('name', terms['common.name'], {
          flex: 30,
        });
        this.grid.addColumnText('description', terms['common.description'], {
          flex: 50,
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
