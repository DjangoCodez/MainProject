import { Component, effect, input, OnInit } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { ISieExportConflictDTO } from '@shared/models/generated-interfaces/SieExportDTO';
import { GridComponent } from '@ui/grid/grid.component';
import { take } from 'rxjs';

@Component({
  selector: 'soe-sie-conflicts-grid',
  templateUrl: './sie-conflicts-grid.component.html',
  standalone: false,
})
export class SieConflictsGridComponent
  extends GridBaseDirective<ISieExportConflictDTO>
  implements OnInit
{
  conflictRows = input.required<Array<ISieExportConflictDTO>>();

  constructor() {
    super();

    effect(() => {
      const rows = this.conflictRows();
      this.rowData.next(rows);
    });
  }
  override ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(Feature.None, 'common.export.sie.conflicts', {
      skipInitialLoad: true,
    });
  }

  override onGridReadyToDefine(
    grid: GridComponent<ISieExportConflictDTO>
  ): void {
    super.onGridReadyToDefine(grid);
    this.translate
      .get([
        'economy.export.sie.conflict.label',
        'economy.export.sie.conflict.conflict',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText(
          'label',
          terms['economy.export.sie.conflict.label'],
          {
            enableHiding: false,
            suppressFilter: true,
            flex: 1,
            suppressSizeToFit: true,
            minWidth: 100,
          }
        );
        this.grid.addColumnText(
          'message',
          terms['economy.export.sie.conflict.conflict'],
          {
            enableHiding: false,
            suppressFilter: true,
            flex: 3,
            suppressSizeToFit: true,
            minWidth: 100,
          }
        );

        this.grid.context.suppressFiltering = true;
        this.grid.context.suppressGridMenu = true;

        super.finalizeInitGrid();
      });
  }
}
