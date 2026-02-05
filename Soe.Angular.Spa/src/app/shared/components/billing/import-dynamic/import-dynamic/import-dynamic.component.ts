import {
  AfterViewInit,
  Component,
  OnInit,
  ViewChild,
  ViewEncapsulation,
  inject,
  signal,
} from '@angular/core';
import { MatStepper } from '@angular/material/stepper';
import { AttachedFile } from '@ui/forms/file-upload/file-upload.component'
import { ColumnUtil } from '@ui/grid/util/column-util'
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { FileUploadDialogComponent } from '@shared/components/file-upload-dialog/file-upload-dialog.component';
import { CoreService } from '@shared/services/core.service'
import { FlowHandlerService } from '@shared/services/flow-handler.service'
import { MessagingService } from '@shared/services/messaging.service'
import { ProgressService } from '@shared/services/progress/progress.service';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { DateUtil } from '@shared/util/date-util'
import { Perform } from '@shared/util/perform.class';
import { ImportDynamicService } from './import-dynamic.service';
import {
  ImportDyanmicDialogData,
  ImportDyanmicDialogform,
  ImportDynamicDTO,
  ImportDynamicDialogDTO,
  ImportDynamicForm,
  ImportDynamicResultDTO,
  ImportDynamicResultForm,
  ImportOptionsDTO,
  ParseRowsResult,
} from './import-dynamic.model';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  SettingDataType,
  TermGroup,
} from '@shared/models/generated-interfaces/Enumerations';
import { ValidationHandler } from '@shared/handlers';
import {
  IImportDynamicFileUploadDTO,
  ISupplierProductImportRawDTO,
} from '@shared/models/generated-interfaces/ImportDynamicDTO';
import { IActionResult } from '@shared/models/generated-interfaces/ActionResult';
import { ColumnMapperColumns } from './column-mapper/column-mapper.component';
import { TranslateService } from '@ngx-translate/core';
import {
  fileUploadDialogData,
  FileUploadDialogData,
} from '@shared/components/file-upload-dialog/models/file-upload-dialog.model';
import { ColDef } from 'ag-grid-enterprise';
import { DialogService } from '@ui/dialog/services/dialog.service';

type DynamicType = { [id: string]: unknown };
enum Tab {
  FileUpload = 0,
  MatchFields = 1,
  Control = 2,
  Finish = 3,
}

@Component({
  selector: 'soe-import-dynamic',
  templateUrl: './import-dynamic.component.html',
  styleUrls: ['./import-dynamic.component.scss'],
  encapsulation: ViewEncapsulation.None,
  providers: [FlowHandlerService],
  standalone: false,
})
export class ImportDynamicComponent
  extends DialogComponent<ImportDyanmicDialogData>
  implements OnInit, AfterViewInit
{
  @ViewChild('stepper')
  private stepper!: MatStepper;
  importDynamicGridColumns: ColDef[] = [];
  previewGridColumns: ColDef[] = [];
  resultGridColumns: ColDef[] = [];

  importDynamicGridRows = new BehaviorSubject<any[]>([]);
  previewGridRows = new BehaviorSubject<any[]>([]);
  resultGridRows = new BehaviorSubject<any[]>([]);

  private readonly handler = inject(FlowHandlerService);
  private readonly service = inject(ImportDynamicService);
  private readonly coreService = inject(CoreService);
  private readonly progressService = inject(ProgressService);
  private readonly validationHandler = inject(ValidationHandler);
  private readonly translate = inject(TranslateService);
  private readonly dialogService = inject(DialogService);

  totalSteps = 0;
  protected mainForm = new ImportDyanmicDialogform({
    validationHandler: this.validationHandler,
    element: new ImportDynamicDialogDTO(),
  });
  protected importDynamicForm = new ImportDynamicForm({
    validationHandler: this.validationHandler,
    element: new ImportDynamicDTO(),
  });
  protected resultForm = new ImportDynamicResultForm({
    validationHandler: this.validationHandler,
    element: new ImportDynamicResultDTO(),
  });
  protected performLoad = new Perform<unknown>(this.progressService);

  fileTypes: SmallGenericType[] = [];
  fileContentRows: Array<string[]> = [];
  isNextDisabled = signal(true);
  showImportDynamicGrid = signal(false);
  showPreviewGrid = signal(false);
  showResultGrid = signal(false);
  showFileUploadError = signal(false);
  showFileContentGrid = signal(false);
  allRequiredSet = false;
  parsedRows: unknown[] = [];

  draggableColumns: ColumnMapperColumns[] = [];

  constructor(private messageService: MessagingService) {
    super();
    this.handler.execute({
      lookups: [this.loadFileTypes()],
    });
  }

  ngOnInit(): void {
    this.resetFieldsIndex();
    this.importDynamicForm.reset(this.data.importDTO);
  }

  private resetFieldsIndex(): void {
    this.data.importDTO.fields.forEach(f => {
      f.index = -1;
    });
  }

  private loadFileTypes(): Observable<unknown> {
    return this.performLoad.load$(
      this.coreService
        .getTermGroupContent(
          TermGroup.ImportDynamicFileTypes,
          false,
          true,
          true
        )
        .pipe(
          tap(x => {
            this.fileTypes = <SmallGenericType[]>x;
          })
        )
    );
  }

  ngAfterViewInit(): void {
    this.totalSteps = this.stepper?._steps.length;
  }

  protected goBack() {
    this.stepper?.previous();
    this.setTab();
  }

  protected goForward() {
    if (this.stepper.selectedIndex === this.totalSteps - 1) {
      this.stepper._steps
        .get(this.totalSteps - 1)!
        ._completedOverride.set(true);
    }
    this.stepper?.next();
    this.setTab();
  }

  private setTab(): void {
    switch (this.stepper.selectedIndex) {
      case Tab.FileUpload:
        this.isNextDisabled.set(this.fileContentRows.length === 0);
        break;
      case Tab.MatchFields:
        this.isNextDisabled.set(!this.allRequiredSet);
        break;
      case Tab.Control:
        this.isNextDisabled.set(this.parsedRows.length === 0);
        this.parseFileData();
        break;
      case Tab.Finish:
        this.importData();
        break;
    }
  }

  close(isOK: boolean): void {
    this.dialogRef.close(isOK);
  }

  //#region Tab 1 Methods/Events

  protected addFile($event: unknown): void {
    this.dialogService
      .open<FileUploadDialogComponent, FileUploadDialogData>(
        FileUploadDialogComponent,
        fileUploadDialogData({
          multipleFiles: false,
          asBinary: false,
          acceptedFileExtensions: ['.txt', '.csv'],
        })
      )
      .afterClosed()
      .subscribe((result: unknown) => {
        if (result !== undefined && result !== false) {
          const uploadFile = <AttachedFile>result;

          this.mainForm.patchFileUploadTabValues(uploadFile?.name ?? '');
          this.service
            .getFileContent(this.mainForm.fileUploadTab.fileType.value, <
              IImportDynamicFileUploadDTO
            >{
              fileName: uploadFile?.name ?? '',
              fileContent: uploadFile?.content,
            })
            .subscribe({
              next: (fileContentResult: IActionResult) => {
                if (fileContentResult.success) {
                  this.fileContentRows = this.fixDatesToLocale(
                    fileContentResult.value?.$values || []
                  );
                  if (this.fileContentRows.length > 0) {
                    this.showImportDynamicGrid.set(true);
                    this.setFileContentData();
                    this.showFileUploadError.set(false);
                    this.showFileContentGrid.set(true);
                  } else {
                    this.showFileUploadError.set(true);
                    this.showFileContentGrid.set(false);
                  }
                  this.resetFieldsIndex();
                }
              },
              error: (): void => {
                this.showFileUploadError.set(true);
                this.showFileContentGrid.set(false);
              },
            });

          this.isNextDisabled.set(
            uploadFile?.name !== undefined &&
              uploadFile?.name !== null &&
              uploadFile?.name !== ''
          );
        }
      });
  }

  private setFileContentData(): void {
    const data = this.fileContentRows.slice(0, 5);
    if (data.length > 0) {
      const rows: DynamicType[] = [];
      data.forEach(r => {
        const row: DynamicType = {};
        r.forEach((c, i) => {
          row[String(i)] = c;
        });
        rows.push(row);
      });
      this.setFileContentGridColumns(this.fileContentRows[0].length - 1);
      this.setDragableColumns(this.fileContentRows[0]);
      this.importDynamicGridRows.next(rows);
      this.isNextDisabled.set(false);
    } else {
      this.isNextDisabled.set(false);
    }
  }

  private setFileContentGridColumns(colCount: number): void {
    for (let index = 0; index < colCount; index++) {
      this.importDynamicGridColumns.push(
        ColumnUtil.createColumnText(String(index + 1), '', {
          enableHiding: false,
          suppressFilter: true,
          flex: 1,
          suppressSizeToFit: true,
          minWidth: 100,
        })
      );
    }
  }

  private setDragableColumns(firstRow: Array<string>): void {
    this.draggableColumns = [];
    this.draggableColumns = firstRow.map((value: string, index: number) => {
      return <ColumnMapperColumns>{
        columnHeader: value,
        columnIndex: index,
      };
    });
  }

  private fixDatesToLocale(rows: Array<string[]>): Array<string[]> {
    const newRows = new Array<string[]>();
    let newRow = new Array<string>();

    rows.forEach((r: string[]) => {
      newRow = [];
      r.forEach((c: string) => {
        if (!isNaN(Date.parse(c)) && isNaN(Number(c))) {
          newRow.push(`${DateUtil.toSwedishFormattedDate(new Date(c))}`);
        } else newRow.push(c);
      });
      newRows.push(newRow);
    });

    return newRows;
  }

  //#endregion

  //#region Tab 2 Methods/Events

  protected allRequiredFieldsSet(value: boolean): void {
    this.allRequiredSet = value;
    this.isNextDisabled.set(!this.allRequiredSet);
  }

  //#endregion

  //#region Tab 3 Methods/Events

  private fixDates(rows: Array<string[]>): Array<string[]> {
    const newRows = new Array<string[]>();
    let newRow = new Array<string>();
    rows.forEach((r: string[]) => {
      newRow = [];
      r.forEach((c: string) => {
        if (!isNaN(Date.parse(c)) && DateUtil.isValidDate(new Date(c))) {
          newRow.push(`${DateUtil.parseDate(c)}`);
        } else newRow.push(c);
      });
      newRows.push(newRow);
    });

    return newRows;
  }

  private parseFileData(): void {
    this.showPreviewGrid.set(true);
    this.setupPreviewGridColumns();
    this.performLoad.load(
      this.service
        .parseRows({
          options: this.importDynamicForm.options.value as ImportOptionsDTO,
          fields: this.data.importDTO.fields,
          data: this.fileContentRows,
        })
        .pipe(
          tap((result: unknown[]): void => {
            this.parsedRows = result;
            (this.parsedRows as any[] as ParseRowsResult[]).forEach(r => {
              if (
                new Date(
                  String(r.supplierProductPriceDateStop)
                ).getFullYear() === 9999
              ) {
                r.supplierProductPriceDateStop = DateUtil.getDateLastInYear(
                  new Date(String(r.supplierProductPriceDateStop))
                );
              }
            });
            this.previewGridRows.next(this.parsedRows);
            this.isNextDisabled.set(false);
          })
        )
    );
  }

  private setupPreviewGridColumns(): void {
    this.previewGridColumns = [];
    this.data.importDTO.fields.forEach(f => {
      switch (f.dataType) {
        case SettingDataType.String:
          this.previewGridColumns.push(
            ColumnUtil.createColumnText(f.field, f.label)
          );
          break;
        case SettingDataType.Integer:
        case SettingDataType.Decimal:
          this.previewGridColumns.push(
            ColumnUtil.createColumnNumber(f.field, f.label)
          );
          break;
        case SettingDataType.Date:
          this.previewGridColumns.push(
            ColumnUtil.createColumnDate(f.field, f.label)
          );
          break;
      }
    });
  }

  //#endregion

  //#region Tab 4 Methods/Events

  private importData(): void {
    this.data.importDTO.options = this.importDynamicForm.options.value;
    this.performLoad
      .load$(
        this.data.callback(
          this.parsedRows as ISupplierProductImportRawDTO[],
          this.data.importDTO.options
        )
      )
      .subscribe(result => {
        const importDynamicResult = result as ImportDynamicResultDTO;
        this.resultForm.reset(result);
        this.resultForm.newCount.disable();
        this.resultForm.updateCount.disable();

        if (importDynamicResult.logs && importDynamicResult.logs.length > 0) {
          this.showResultGrid.set(true);
          this.setupResultGridColumns();
          this.resultGridRows.next(importDynamicResult.logs);
        }
      });
  }

  private setupResultGridColumns(): void {
    this.resultGridColumns = [];
    this.resultGridColumns.push(
      ColumnUtil.createColumnNumber(
        'rowNr',
        this.translate.instant('common.row')
      )
    );
    this.resultGridColumns.push(
      ColumnUtil.createColumnText(
        'message',
        this.translate.instant('common.message')
      )
    );
  }

  //#endregion
}
