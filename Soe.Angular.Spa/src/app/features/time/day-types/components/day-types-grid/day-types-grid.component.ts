import { Component, OnInit, inject } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { IDayTypeGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { take } from 'rxjs/operators';
import { DayTypesService } from '../../services/day-types.service';

@Component({
  selector: 'soe-day-types-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class DayTypesGridComponent
  extends GridBaseDirective<IDayTypeGridDTO, DayTypesService>
  implements OnInit
{
  service = inject(DayTypesService);

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(
      Feature.Time_Preferences_ScheduleSettings_DayTypes,
      'Time.Schedule.DayTypes'
    );
  }

  override onGridReadyToDefine(grid: GridComponent<IDayTypeGridDTO>) {
    super.onGridReadyToDefine(grid);

    //console.log('additionalGridProps', this.additionalGridProps());

    this.translate
      .get(['common.name', 'common.description', 'core.edit'])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText('name', terms['common.name'], {
          flex: 40,
        });
        this.grid.addColumnText('description', terms['common.description'], {
          flex: 60,
          enableHiding: true,
        });
        this.grid.addColumnIconEdit({
          tooltip: terms['core.edit'],
          onClick: row => {
            this.edit(row);
          },
        });
        super
          .finalizeInitGrid
          // {},
          // { field: 'name', filterModel: { type: 'contains', filter: 'test' } }
          ();
      });
  }

  // override getAdditionalPropsGridData(rows: IDayTypeGridDTO[]): any {
  //   return { testRows: rows.filter(r => r.name.startsWith('Test')) };
  // }
}
