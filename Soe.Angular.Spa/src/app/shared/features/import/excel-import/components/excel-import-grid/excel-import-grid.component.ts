import {
  Component,
  computed,
  inject,
  OnInit,
  signal,
  viewChild,
} from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import {
  Feature,
  SoeModule,
} from '@shared/models/generated-interfaces/Enumerations';
import { IExcelImportTemplateDTO } from '@shared/models/generated-interfaces/Excel';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { DownloadUtility } from '@shared/util/download-util';
import {
  AttachedFile,
  FileUploadComponent,
} from '@ui/forms/file-upload/file-upload.component';
import { GridComponent } from '@ui/grid/grid.component';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { take, tap } from 'rxjs';
import { ExcelImportService } from '../../services/excel-import.service';
import { Perform } from '@shared/util/perform.class';
import { ExcelImportDTO } from '../../model/excel-import.model';
import { ValidationHandler } from '@shared/handlers';
import { ExcelImportForm } from '../../model/excel-import-grid-form.model';
import { RowDoubleClickedEvent } from 'ag-grid-community';
import { UrlHelperService } from '@shared/services/url-params.service';

@Component({
  templateUrl: 'excel-import-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class ExcelImportGridComponent
  extends GridBaseDirective<IExcelImportTemplateDTO, ExcelImportService>
  implements OnInit
{
  service = inject(ExcelImportService);
  urlHelper = inject(UrlHelperService);

  get disableTextfield() {
    return true;
  }

  validationHandler = inject(ValidationHandler);
  coreService = inject(CoreService);
  messageboxService = inject(MessageboxService);
  progressService = inject(ProgressService);
  performAction = new Perform<any>(this.progressService);

  _conflicts: any[] = [];
  get conflicts() {
    return this._conflicts;
  }

  gridHeight = computed(() => {
    console.log(
      'gridHeight',
      this.grid ? this.grid.totalRowsCount() * 35 : this.availableScreenHeight()
    );
    return this.grid
      ? this.grid.totalRowsCount() * 35
      : this.availableScreenHeight();
  });
  private availableScreenHeight = signal(0);
  private toolbarHeight = 235;

  form: ExcelImportForm = new ExcelImportForm({
    validationHandler: this.validationHandler,
    element: [{}],
  });

  fileUploadRef = viewChild<FileUploadComponent>(FileUploadComponent);

  ngOnInit() {
    super.ngOnInit();
    this.availableScreenHeight.set(window.innerHeight - this.toolbarHeight);

    this.startFlow(this.getFeatureFromModule(), 'common.excelimport');
  }

  override onGridReadyToDefine(
    grid: GridComponent<IExcelImportTemplateDTO>
  ): void {
    super.onGridReadyToDefine(grid);

    this.translate
      .get(['common.filetemplate', 'common.download'])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.onRowDoubleClicked = this.doubleClickFileTemplate.bind(this);

        this.grid.addColumnText('description', terms['common.filetemplate'], {
          flex: 33,
          enableHiding: false,
        });
        this.grid.addColumnIcon('', '', {
          tooltip: terms['common.download'],
          enableHiding: true,
          pinned: 'right',
          iconName: 'download',
          onClick: row => {
            DownloadUtility.openInNewTab(window, row.href);
          },
        });

        super.finalizeInitGrid();
      });
  }

  override onAfterLoadData(rows: any) {
    this.grid.updateGridHeightBasedOnNbrOfRows();
  }

  doubleClickFileTemplate(
    event: RowDoubleClickedEvent<IExcelImportTemplateDTO, any>
  ) {
    const row = { ...(event.data as IExcelImportTemplateDTO) };
    DownloadUtility.openInNewTab(window, row.href);
  }

  afterFilesAttached(files: AttachedFile[]) {
    const file = files && files.length > 0 ? files[0] : null;
    if (file) {
      this.form.patchValue({ file: file });
    }
  }

  importFile() {
    const bytes = Array.from(
      new Uint8Array(this.form.file.value.binaryContent)
    );
    const importModel = new ExcelImportDTO(
      this.form.value.file.name,
      false,
      bytes
    );
    this.performAction.load(
      this.service.importFile(importModel).pipe(
        tap(result => {
          if (result.errorMessage && result.errorMessage.length > 0) {
            this.messageboxService.question(
              this.translate.instant('core.info'),
              result.errorMessage,
              {
                buttons: 'ok',
              }
            );
          }

          if (result.value) {
            const confl = result.value.$values as any[];
            if (confl.length > 0) this._conflicts = confl;
          }

          // Reset
          this.form.patchValue({ file: undefined });
          this.fileUploadRef()?.clearAllFiles();
        })
      )
    );
  }

  getFeatureFromModule(): Feature {
    if (this.urlHelper.module === SoeModule.Billing)
      return Feature.Billing_Import_ExcelImport;
    else if (this.urlHelper.module === SoeModule.Economy)
      return Feature.Economy_Import_ExcelImport;
    else return Feature.Time_Import_ExcelImport;
  }
}
