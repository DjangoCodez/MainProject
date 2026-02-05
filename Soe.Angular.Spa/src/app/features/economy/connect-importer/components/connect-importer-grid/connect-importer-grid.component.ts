import { Component, inject, OnInit } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { TermCollection } from '@shared/localization/term-types';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { map, Observable, of } from 'rxjs';
import {
  ConnectImporterGridFilterDTO,
  ImportBatchDTO,
} from '../../models/connect-importer.model';
import { ConnectImporterService } from '../../services/connect-importer.service';

@Component({
  selector: 'soe-connect-importer-grid',
  templateUrl: './connect-importer-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class ConnectImporterGridComponent
  extends GridBaseDirective<ImportBatchDTO, ConnectImporterService>
  implements OnInit {
  service = inject(ConnectImporterService);
  private filterIsReady = false;

  filter!: ConnectImporterGridFilterDTO;
  dynamicStatusOptions: SmallGenericType[] = [];

  ngOnInit(): void {
    super.ngOnInit();

    this.service.gridFilter$.subscribe(filter => {
      this.filter = filter;
    });

    this.startFlow(
      Feature.Economy_Import_XEConnect,
      'Economy.Import.Batches',
      {}
    );
  }

  override onFinished(): void {
    if (this.filterIsReady) {
      this.refreshGrid();
    }
  }

  //#region UI events

  doFilter(filter: ConnectImporterGridFilterDTO) {
    this.filterIsReady = true;
    this.service.setFilterSubject(filter);
    if (this.grid && this.grid.gridIsReady) {
      this.refreshGrid();
    }
  }

  gridRowEdit(row: ImportBatchDTO) {
    this.edit({
      ...row!,
      importHeadType: this.service.filter?.iOImportHeadType,
    });
  }
  //#endregion

  //#region Overridings

  override loadTerms(): Observable<TermCollection> {
    return super.loadTerms([
      'common.name',
      'common.connect.import',
      'common.connect.imports',
      'common.status',
      'common.connect.importtype',
      'common.connect.importheadtype',
      'core.edit',
      'common.created',
      'economy.import.batches.batch',
    ]);
  }

  override onGridReadyToDefine(grid: GridComponent<ImportBatchDTO>) {
    super.onGridReadyToDefine(grid);

    this.grid.addColumnText(
      'typeName',
      this.terms['common.connect.importtype'],
      {
        flex: 1,
      }
    );
    this.grid.addColumnText('sourceName', this.terms['common.connect.import'], {
      flex: 1,
    });
    this.grid.addColumnText(
      'importHeadTypeName',
      this.terms['common.connect.importheadtype'],
      {
        flex: 1,
      }
    );
    this.grid.addColumnSelect(
      'statusNameId',
      this.terms['common.status'],
      [],
      undefined,
      {
        flex: 1,
        editable: false,
        dropDownIdLabel: 'id',
        dropDownValueLabel: 'name',
        dynamicSelectOptions: (row: any) =>
          this.dynamicStatusOptions,
      }
    );
    this.grid.addColumnText(
      'batchId',
      this.terms['economy.import.batches.batch'],
      {
        flex: 2,
      }
    );
    this.grid.addColumnDateTime('created', this.terms['common.created'], {
      flex: 1,
    });

    this.grid.addColumnIconEdit({
      pinned: 'right',
      tooltip: this.terms['core.edit'],
      onClick: row => {
        this.edit({
          ...row,
          importHeadType: this.service.filter?.iOImportHeadType,
        });
      },
    });

    super.finalizeInitGrid();
  }

  override loadData(
    id?: number | undefined,
    additionalProps?: {
      importHeadType: number;
      allItemsSelection: number;
    }
  ): Observable<ImportBatchDTO[]> {
    if (
      this.filter &&
      this.filter.iOImportHeadType &&
      this.filter.dateSelectionId
    ) {
      return this.service
        .getGrid(undefined, {
          importHeadType: this.filter.iOImportHeadType,
          allItemsSelection: this.filter.dateSelectionId,
        })
        .pipe(
          map(data => {
            this.dynamicStatusOptions = [];
            data.forEach(row => {
              row = row as ImportBatchDTO;
              let statuses: string = '';
              row.statusName.sort().forEach(status => {
                if (statuses.length > 0) statuses += ', ';
                statuses += status;
              });
              let stasutDist = this.dynamicStatusOptions.find(f => f.name === statuses);
              row.statusNameStr = statuses;
              if (stasutDist) {
                row.statusNameId = stasutDist.id;
              } else {
                row.statusNameId = this.dynamicStatusOptions.length + 1;
                stasutDist = new SmallGenericType(row.statusNameId, statuses);
                this.dynamicStatusOptions.push(stasutDist);
              }
            });
            return data;
          })
        );
    } else {
      return of([]);
    }
  }

  //#endregion
}
