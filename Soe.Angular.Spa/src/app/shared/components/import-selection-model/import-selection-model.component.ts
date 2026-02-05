import { Component, inject } from '@angular/core';
import { MatDialogRef } from '@angular/material/dialog';
import { ImportDTO } from '@features/economy/import-connect/models/import-connect.model';
import { ImportConnectService } from '@features/economy/import-connect/services/import-connect.service';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { TermCollection } from '@shared/localization/term-types';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { SoeModule } from '@shared/models/generated-interfaces/Enumerations';
import { IImportSelectionGridRowDTO } from '@shared/models/generated-interfaces/ImportSelectionGridRowDTO';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { Perform } from '@shared/util/perform.class';
import { ButtonComponent } from '@ui/button/button/button.component';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { GridComponent } from '@ui/grid/grid.component';
import { GridResizeType } from '@ui/grid/enums/resize-type.enum';
import { CellClassParams } from 'ag-grid-community';
import { BehaviorSubject, forkJoin, of, take, tap } from 'rxjs';
import { ImportSelectionModel } from './import-selection-model.model';

@Component({
  selector: 'soe-import-selection-model',
  imports: [TranslateModule, ButtonComponent, DialogComponent, GridComponent],
  providers: [FlowHandlerService, ImportConnectService, ProgressService],
  templateUrl: './import-selection-model.component.html',
  styleUrl: './import-selection-model.component.scss',
})
export class ImportSelectionModelComponent extends DialogComponent<ImportSelectionModel> {
  translate = inject(TranslateService);
  flowHandler = inject(FlowHandlerService);
  connectService = inject(ImportConnectService);
  rows = new BehaviorSubject<IImportSelectionGridRowDTO[]>([]);
  rowList: IImportSelectionGridRowDTO[] = [];
  grid!: GridComponent<IImportSelectionGridRowDTO>;
  dialogRef = inject(MatDialogRef);
  progressService = inject(ProgressService);
  performAction = new Perform<any>(this.progressService);

  terms!: TermCollection;
  importLookupSimple!: SmallGenericType[];
  importLookup!: ImportDTO[];

  constructor() {
    super();
    this.flowHandler.execute({
      lookups: [this.lookups()],
      setupGrid: this.setupGrid.bind(this),
    });
  }

  setupGrid(grid: GridComponent<IImportSelectionGridRowDTO>) {
    this.grid = grid;
    this._constructGridColumns();
    this.loadData();
  }

  lookups() {
    return this.performAction.load$(
      forkJoin([this.loadTerms(), this.loadImport()])
    );
  }

  loadImport() {
    return this.connectService
      .getGrid(undefined, { module: SoeModule.Economy })
      .pipe(
        take(1),
        tap(data => {
          this.importLookupSimple = data.map(
            d => new SmallGenericType(d.importId, d.name)
          );
          this.importLookup = data.map(row => {
            return new ImportDTO({
              actorCompanyId: row.actorCompanyId,
              created: row.created,
              createdBy: row.createdBy,
              guid: row.guid,
              headName: row.headName,
              importDefinitionId: row.importDefinitionId,
              importHeadType: row.importHeadType,
              importId: row.importId,
              isStandard: row.isStandard,
              module: row.module,
              name: row.name,
              specialFunctionality: row.specialFunctionality,
              state: row.state,
              type: row.type,
              typeText: row.typeText,
              updateExistingInvoice: row.updateExistingInvoice,
              useAccountDimensions: row.useAccountDimensions,
              useAccountDistribution: row.useAccountDistribution,
            });
          });
        })
      );
  }

  loadTerms() {
    const keys: string[] = [
      'common.connect.import',
      'common.filename',
      'common.message',
      'common.connect.modalTitle',
      'core.import',
    ];
    return this.translate.get(keys).pipe(
      take(1),
      tap(terms => {
        this.terms = terms;
        this.data.title = terms['common.connect.modalTitle'];
      })
    );
  }

  private _constructGridColumns() {
    const errorCellRules = {
      'error-background-color': (params: CellClassParams) => {
        return !!params?.data?.message;
      },
    };

    this.grid.addColumnText('fileName', this.terms['common.filename'], {
      flex: 30,
      cellClassRules: errorCellRules,
    });
    this.grid.addColumnAutocomplete(
      'importId',
      this.terms['common.connect.import'],
      {
        flex: 25,
        cellClassRules: errorCellRules,
        source: () => this.importLookupSimple,
        optionIdField: 'id',
        optionNameField: 'name',
        editable: true,
        updater: row => this.handleImportChange(row),
      }
    );
    this.grid.addColumnText('message', this.terms['common.message'], {
      flex: 20,
      cellClassRules: errorCellRules,
      buttonConfiguration: {
        iconPrefix: 'fal',
        iconName: 'triangle-exclamation',
        show: row => !!row.message,
        onClick: () => null,
      },
    });
    this.grid.addColumnBool('doImport', this.terms['core.import'], {
      flex: 6,
      editable: true,
      cellClassRules: errorCellRules,
    });
    this.grid.finalizeInitGrid();
  }

  loadData() {
    if (!this.data.uploadedFiles) return;
    this.connectService
      .getImportSelectionGrid(this.data.uploadedFiles)
      .pipe(
        take(1),
        tap(rows => {
          rows = rows.map((d, i) => ({
            id: i + 1,
            ...d,
            doImport: d.message == '',
            disableImport: d.import == undefined,
          }));
          this.rows.next(rows);
          this.rowList = rows;
          this.grid.resizeColumns(GridResizeType.AutoAllAndHeaders);
        })
      )
      .subscribe();
  }

  ok() {
    this.dialogRef.close(this.grid.getAllRows());
  }

  private handleImportChange(row: IImportSelectionGridRowDTO) {
    const member = this.importLookupSimple.find(i => i.id == row.importId);
    if (!member) return;

    const changeObj = this.rowList?.find(
      i => i.dataStorageId == row.dataStorageId
    );
    if (!changeObj) return;

    changeObj.importId = member.id;
    changeObj.importName = member.name;
    const selectedImport = this.importLookup.find(
      i => i.importId == row.importId
    );
    if (selectedImport) changeObj.import = selectedImport;

    if (changeObj.disableImport) {
      changeObj.disableImport = false;
    }
    this.rows.next(this.rowList);
  }
}
