import { Component, inject, OnInit } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { ITimeCodeBreakGroupGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { take } from 'rxjs/operators';
import { TimeCodeBreakGroupService } from '../../services/time-code-break-group.service';

@Component({
  selector: 'soe-time-code-break-group-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class TimeCodeBreakGroupGridComponent
  extends GridBaseDirective<
    ITimeCodeBreakGroupGridDTO,
    TimeCodeBreakGroupService
  >
  implements OnInit
{
  service = inject(TimeCodeBreakGroupService);

  ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(
      Feature.Time_Preferences_TimeSettings_TimeCodeBreakGroup,
      'Time.Time.TimeCodeBreakGroups'
    );
  }

  override onGridReadyToDefine(
    grid: GridComponent<ITimeCodeBreakGroupGridDTO>
  ) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get(['common.name', 'common.description', 'core.edit'])
      .pipe(take(1))
      .subscribe(terms => {
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
}
