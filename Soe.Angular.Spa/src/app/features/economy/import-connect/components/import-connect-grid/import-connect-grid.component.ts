import { Component, inject, OnInit, signal } from '@angular/core';
import { FileUploadDialogComponent } from '@shared/components/file-upload-dialog/file-upload-dialog.component';
import { defaultFileUploadDialogData } from '@shared/components/file-upload-dialog/models/file-upload-dialog.model';
import { ImportSelectionModelComponent } from '@shared/components/import-selection-model/import-selection-model.component';
import { ImportSelectionModel } from '@shared/components/import-selection-model/import-selection-model.model';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { TermCollection } from '@shared/localization/term-types';
import { FilesLookupDTO } from '@shared/models/file.model';
import {
  Feature,
  SoeModule,
} from '@shared/models/generated-interfaces/Enumerations';
import { IImportSelectionGridRowDTO } from '@shared/models/generated-interfaces/ImportSelectionGridRowDTO';
import { IImportDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { Perform } from '@shared/util/perform.class';
import { AttachedFile } from '@ui/forms/file-upload/file-upload.component';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { GridComponent } from '@ui/grid/grid.component';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { map, mergeMap, Observable, of, take, tap } from 'rxjs';
import { FileUploader } from '../../models/file-uploader';
import { ImportDTO, SimpleFile } from '../../models/import-connect.model';
import { ImportConnectService } from '../../services/import-connect.service';
import { ImportConnectEditComponent } from '../import-connect-edit/import-connect-edit.component';

type ConnectTabData = {
  key: string;
  fileType: string;
  import: ImportDTO;
  files: SimpleFile[];
};

@Component({
  selector: 'soe-import-connect-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class ImportConnectGridComponent
  extends GridBaseDirective<IImportDTO, ImportConnectService>
  implements OnInit
{
  service = inject(ImportConnectService);
  dialogService = inject(DialogService);
  coreService = inject(CoreService);
  messageboxService = inject(MessageboxService);
  progressService = inject(ProgressService);

  /* eslint-disable @typescript-eslint/no-explicit-any */
  performAction = new Perform<any>(this.progressService);

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(Feature.Economy_Import_XEConnect, 'Common.Connect');
  }

  override createGridToolbar(): void {
    super.createGridToolbar();

    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton('uploadfiles', {
          iconName: signal('upload'),
          caption: signal('common.connect.uploadfiles'),
          tooltip: signal('common.connect.uploadfiles'),
          onAction: () => this.startUploading(),
        }),
      ],
    });
  }

  override onGridReadyToDefine(grid: GridComponent<IImportDTO>) {
    super.onGridReadyToDefine(grid);
    this.translate
      .get([
        'common.name',
        'common.connect.import',
        'common.connect.imports',
        'common.standard',
        'common.connect.importtype',
        'core.edit',
      ])
      .pipe(
        take(1),
        tap((terms: TermCollection) => {
          this.grid.enableRowSelection();
          this.grid.addColumnText('name', terms['common.name'], {
            flex: 5,
          });
          this.grid.addColumnText('headName', terms['common.connect.import'], {
            flex: 5,
          });
          this.grid.addColumnText(
            'typeText',
            terms['common.connect.importtype'],
            {
              flex: 3,
            }
          );
          this.grid.addColumnBool('isStandard', terms['common.standard'], {
            flex: 2,
            enableHiding: true,
            alignCenter: true,
          });
          this.grid.addColumnIconEdit({
            onClick: this.edit.bind(this),
          });
          this.grid.finalizeInitGrid();
        })
      )
      .subscribe();
  }

  override loadData(
    id?: number | undefined,
    additionalProps?: { module: SoeModule }
  ): Observable<IImportDTO[]> {
    return super.loadData(id, { module: SoeModule.Economy });
  }

  startUploading() {
    this.uploadFiles()
      .pipe(
        mergeMap(uploadedFiles => this.uploadSelection(uploadedFiles)),
        map((x: IImportSelectionGridRowDTO[]) => {
          const filesToImportObject = x?.filter(d => d.doImport);
          if (!filesToImportObject) return;

          const filesToImport: number[] = filesToImportObject.map(
            i => i.dataStorageId
          );
          if (filesToImport.length == 0) return;

          return filesToImportObject;
        }),
        tap(filesToImportObject => {
          if (!filesToImportObject) return;
          const tabControllerData = this.groupFiles(filesToImportObject);
          if (!tabControllerData) return;
          this.openEdit(tabControllerData);
        })
      )
      .subscribe();
  }

  uploadFiles() {
    const fileUploader = new FileUploader(this.coreService);
    const dialogData = defaultFileUploadDialogData(fileUploader, true);
    dialogData.disableClose = true;

    return this.dialogService
      .open(FileUploadDialogComponent, dialogData)
      .afterClosed()
      .pipe(
        map((files: AttachedFile[]) => {
          if (!files || files.length === 0) return;
          return fileUploader.fileLookup;
        })
      );
  }

  uploadSelection(
    uploadedFiles: FilesLookupDTO | undefined
  ): Observable<IImportSelectionGridRowDTO[]> {
    if (!uploadedFiles) return of([]);

    const dialogOpts = <Partial<ImportSelectionModel>>{
      size: 'lg',
      uploadedFiles: uploadedFiles,
      disableClose: true,
    };

    return this.dialogService
      .open(ImportSelectionModelComponent, dialogOpts)
      .afterClosed();
  }

  private groupFiles(filesToImportObject: IImportSelectionGridRowDTO[]) {
    /**
     * We want to group the files by importId, split up by fileType.
     * E.g. one file for importId 1 with fileType 'A' and one file for importId 1 with fileType 'B'
     * should be in separate tabs.
     * One file for importId 1 with fileType 'A' and one file for importId 2 with fileType 'A'
     * should be in separate tabs.
     * One file for importId 1 with fileType 'A' and one file for importId 1 with fileType 'A'
     * should be in the same tab.
     */
    if (!filesToImportObject) return;

    const keyBuilder = (importId: number, fileType: string) =>
      `${importId}-${fileType}`;

    const tabData: ConnectTabData[] = [];

    for (const file of filesToImportObject) {
      const { importId, fileType } = file;
      if (!importId) continue;

      const k = keyBuilder(importId, fileType);
      const record = tabData.find(d => d.key === k);
      const simpleFile = SimpleFile.fromImportSelectionGridRowDTO(file);

      if (record) {
        record.files.push(simpleFile);
      } else {
        tabData.push({
          key: k,
          fileType,
          import: new ImportDTO(file),
          files: [simpleFile],
        });
      }
    }

    return tabData;
  }

  openEdit(tabControllerData: ConnectTabData[]) {
    const gridIndex = this.gridIndex();
    const gridRows = this.grid.getAllRows() || [];
    const rows = this.grid.getFilteredRows();

    tabControllerData.forEach(data => {
      setTimeout(() => {
        if (!data.import) return;
        const title = data.fileType
          ? `${data.import.name} - ${data.fileType}`
          : data.import.name;
        this.rowEdited.set({
          gridIndex,
          rows,
          row: data.import,
          filteredRows: gridRows,
          additionalProps: {
            editComponent: ImportConnectEditComponent,
            editLabel: title,
            label: title,
            gridData: {
              files: data.files,
            },
          },
        });
      }, 0);
    });
  }
}

type ImportsArrayValue = { fileType: string; name: string };
