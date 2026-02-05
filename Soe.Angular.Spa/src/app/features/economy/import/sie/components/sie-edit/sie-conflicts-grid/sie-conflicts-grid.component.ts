import { Component, computed, effect, input, OnInit } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { ISieImportConflictDTO } from '@shared/models/generated-interfaces/SieImportDTO';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { take } from 'rxjs';

@Component({
  selector: 'soe-economy-import-sie-conflicts-grid',
  templateUrl: './sie-conflicts-grid.component.html',
  standalone: false,
  providers: [FlowHandlerService, ToolbarService],
})
export class SieConflictsGridComponent
  extends GridBaseDirective<ISieImportConflictDTO>
  implements OnInit
{
  conflictRows = input.required<Array<ISieImportConflictDTO>>();
  hasConflictRows = computed(() => this.conflictRows().length > 0);

  constructor() {
    super();

    effect(() => {
      this.rowData.next(this.conflictRows());
    });
  }
  override ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(Feature.Economy_Import_Sie, 'common.export.sie.conflicts', {
      skipInitialLoad: true,
    });
  }

  override onGridReadyToDefine(
    grid: GridComponent<ISieImportConflictDTO>
  ): void {
    super.onGridReadyToDefine(grid);
    this.translate
      .get([
        'economy.import.sie.filetag',
        'economy.import.sie.filerownr',
        'economy.import.sie.filefieldvalues',
        'economy.import.sie.fileconflict',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText('label', terms['economy.import.sie.filetag'], {
          enableHiding: false,
          suppressFilter: true,
          flex: 1,
          suppressSizeToFit: true,
          minWidth: 100,
        });
        this.grid.addColumnNumber(
          'rowNr',
          terms['economy.import.sie.filerownr'],
          {
            enableHiding: false,
            suppressFilter: true,
            flex: 3,
            suppressSizeToFit: true,
            minWidth: 100,
          }
        );

        this.grid.addColumnText(
          'value',
          terms['economy.import.sie.filefieldvalues'],
          {
            enableHiding: false,
            suppressFilter: true,
            flex: 3,
            suppressSizeToFit: true,
            minWidth: 100,
          }
        );

        this.grid.addColumnText(
          'conflict',
          terms['economy.import.sie.fileconflict'],
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
