import { Component, inject, OnInit } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { ITimeLeisureCodeGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { take } from 'rxjs/operators';
import { LeisureCodeTypesService } from '../../services/leisure-code-types.service';

@Component({
  selector: 'soe-leisure-code-types-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class LeisureCodeTypesGridComponent
  extends GridBaseDirective<ITimeLeisureCodeGridDTO, LeisureCodeTypesService>
  implements OnInit
{
  service = inject(LeisureCodeTypesService);

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(
      Feature.Time_Preferences_ScheduleSettings_LeisureCodeType,
      'Time.Schedule.LeisureCodeTypes'
    );
  }

  override onGridReadyToDefine(grid: GridComponent<ITimeLeisureCodeGridDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get(['common.name', 'common.code', 'common.type', 'core.edit'])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText('name', terms['common.name'], {
          flex: 50,
        });
        this.grid.addColumnText('code', terms['common.code'], {
          flex: 20,
          enableHiding: true,
        });
        this.grid.addColumnText('typeName', terms['common.type'], {
          flex: 20,
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
