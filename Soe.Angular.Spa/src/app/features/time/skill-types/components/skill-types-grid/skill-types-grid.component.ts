import { Component, OnInit, inject } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { CrudActionTypeEnum } from '@shared/enums';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { ISkillTypeGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { Perform } from '@shared/util/perform.class';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { take, tap } from 'rxjs/operators';
import { SkillTypesService } from '../../services/skill-types.service';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Component({
  selector: 'soe-skill-types-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class SkillTypesGridComponent
  extends GridBaseDirective<ISkillTypeGridDTO, SkillTypesService>
  implements OnInit
{
  service = inject(SkillTypesService);
  progressService = inject(ProgressService);
  performAction = new Perform<any>(this.progressService);

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(
      Feature.Time_Preferences_ScheduleSettings_SkillType,
      'Time.Schedule.SkillTypes'
    );
  }

  override createGridToolbar(): void {
    return super.createGridToolbar({
      useDefaltSaveOption: true,
    });
  }

  override onGridReadyToDefine(grid: GridComponent<ISkillTypeGridDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get(['common.active', 'common.name', 'common.description', 'core.edit'])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnActive('state', terms['common.active'], {
          idField: 'skillTypeId',
          editable: true,
        });
        this.grid.addColumnText('name', terms['common.name'], { flex: 25 });
        this.grid.addColumnText('description', terms['common.description'], {
          flex: 75,
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

  override saveStatus(): void {
    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.service
        .updateSkillTypesState(this.grid.selectedItemsService.toDict())
        .pipe(
          tap((response: BackendResponse) => {
            if (response.success) this.refreshGrid();
          })
        )
    );
  }
}
