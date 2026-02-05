import {
  Component,
  computed,
  effect,
  inject,
  input,
  OnInit,
} from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import {
  Feature,
  SoeModule,
} from '@shared/models/generated-interfaces/Enumerations';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { UrlHelperService } from '@shared/services/url-params.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { take } from 'rxjs';

@Component({
  selector: 'excel-import-conflicts-grid',
  templateUrl: './excel-import-conflicts-grid.component.html',
  standalone: false,
  providers: [FlowHandlerService, ToolbarService],
})
export class ExcelImportConflictsGridComponent
  extends GridBaseDirective<any>
  implements OnInit
{
  urlHelper = inject(UrlHelperService);
  conflictRows = input.required<Array<any>>();
  hasConflictRows = computed(() => this.conflictRows().length > 0);

  constructor() {
    super();

    effect(() => {
      this.rowData.next(this.conflictRows());
    });
  }
  override ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(this.getFeatureFromModule(), 'common.conflicts', {
      skipInitialLoad: true,
    });
  }

  override onGridReadyToDefine(grid: GridComponent<any>): void {
    super.onGridReadyToDefine(grid);
    this.translate
      .get(['common.rownr', 'common.column', 'core.info', 'common.id'])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnNumber('rowNr', terms['common.rownr'], {
          enableHiding: false,
          suppressFilter: true,
          flex: 1,
          suppressSizeToFit: true,
          minWidth: 100,
        });
        this.grid.addColumnText('field', terms['common.column'], {
          enableHiding: false,
          suppressFilter: true,
          flex: 3,
          suppressSizeToFit: true,
          minWidth: 100,
        });

        this.grid.addColumnText('message', terms['core.info'], {
          enableHiding: false,
          suppressFilter: true,
          flex: 3,
          suppressSizeToFit: true,
          minWidth: 100,
        });

        this.grid.addColumnText('identifier', terms['common.id'], {
          enableHiding: false,
          suppressFilter: true,
          flex: 3,
          suppressSizeToFit: true,
          minWidth: 100,
        });
        this.grid.context.suppressFiltering = true;
        this.grid.context.suppressGridMenu = true;

        super.finalizeInitGrid();
      });
  }

  getFeatureFromModule(): Feature {
    if (this.urlHelper.module === +SoeModule.Billing)
      return Feature.Billing_Import_ExcelImport;
    else if (this.urlHelper.module === +SoeModule.Economy)
      return Feature.Economy_Import_ExcelImport;
    else return Feature.Time_Import_ExcelImport;
  }
}
