import {
  Component,
  computed,
  EventEmitter,
  inject,
  Input,
  OnInit,
  Output,
  signal,
} from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { FileRecordDTO } from '@shared/models/file.model';
import {
  Feature,
  SoeEntityType,
  SoeOriginStatusClassificationGroup,
} from '@shared/models/generated-interfaces/Enumerations';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import {
  AttachedFile,
  IFileUploader,
} from '@ui/forms/file-upload/file-upload.component';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { GridComponent } from '@ui/grid/grid.component';
import { IMessageboxComponentResponse } from '@ui/dialog/models/messagebox';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { BehaviorSubject, map, Observable, take, tap } from 'rxjs';
import { FileUploadDialogComponent } from '../file-upload-dialog/file-upload-dialog.component';
import { FilesHelper } from '../files-helper/files-helper.component';

import { BrowserUtil } from '@shared/util/browser-util';
import {
  FilePreviewDialogData,
  FileViewerComponent,
} from '@ui/file-viewer/file-viewer.component';
import {
  defaultFileUploadDialogData,
  fileReplaceDialogData,
  FileUploadDialogData,
} from '../file-upload-dialog/models/file-upload-dialog.model';
import { ResponseUtil } from '@shared/util/response-util';
import { Perform } from '@shared/util/perform.class';
import { SelectEmailDialogComponent } from '../select-email-dialog/components/select-email-dialog/select-email-dialog.component';
import { SelectEmailDialogCloseData } from '../select-email-dialog/models/select-email-dialog.model';
import { IEmailDocumentsRequestDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { ToasterService } from '@ui/toaster/services/toaster.service';

@Component({
  selector: 'soe-file-display',
  templateUrl: './file-display.component.html',
  styleUrls: ['./file-display.component.scss'],
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class FileDisplayComponent
  extends GridBaseDirective<FileRecordDTO>
  implements OnInit
{
  @Input() useGrid = true;
  @Input() isReadOnly = false;
  @Input() files: FileRecordDTO[] = [];
  @Input() uploadPermission: Feature | undefined;
  @Input() gridHeight = 400;
  @Input() hideUploadButton = false;
  @Input() hideReloadButton = false;
  @Input() helper: FilesHelper | undefined;
  @Input() allowReplace = false;
  @Input() pageName: string | undefined;
  @Input() showSendEmailButton = false;
  @Input() emailGetter?: () => Observable<string>;

  @Output() onReload = new EventEmitter<void>();
  @Output() fileRecordChanged = new EventEmitter<FileRecordDTO>();

  grid!: GridComponent<FileRecordDTO>;
  gridData = new BehaviorSubject<any[]>([]);

  coreService = inject(CoreService);
  dialogService = inject(DialogService);
  messageboxService = inject(MessageboxService);
  toasterService = inject(ToasterService);
  performAction = new Perform<any>(this.progressService);

  private readonly hasRowsSelected = signal<boolean>(false);

  ngOnInit() {
    super.ngOnInit();
    this.setupHelper();
    this.gridData = new BehaviorSubject(this.files);
    this.startFlow(Feature.None, 'FileDisplayGrid', {
      skipInitialLoad: true,
    });

    if (this.helper) {
      this.helper.fileRecords.subscribe(files => {
        this.gridData.next(files);
      });
    }
  }

  private setupHelper() {
    if (this.helper) {
      if (this.uploadPermission === undefined)
        this.uploadPermission = this.helper.getUploadPermission();
    }
  }

  override createGridToolbar(): void {
    super.createGridToolbar({
      hideReload: this.hideReloadButton,
    });
    if (
      !this.hideUploadButton &&
      this.uploadPermission !== undefined &&
      !this.helper?.isReadOnly
    ) {
      this.coreService
        .hasModifyPermissions([this.uploadPermission])
        .subscribe(res => {
          if (res[<Feature>this.uploadPermission]) {
            this.toolbarService.createItemGroup({
              items: [
                this.toolbarService.createToolbarButton('uploadbutton', {
                  iconName: signal('upload'),
                  caption: signal('core.fileupload.upload'),
                  tooltip: signal('core.fileupload.upload'),
                  onAction: () => {
                    this.openUploadFileDialog();
                  },
                }),
              ],
            });
          }
          this.toolbarService.createItemGroup({
            items: [
              this.toolbarService.createToolbarButton('download', {
                iconName: signal('download'),
                tooltip: signal('common.download'),
                caption: signal('common.download'),
                disabled: computed(() => !this.hasRowsSelected()),
                onAction: () => this.downloadSelectedFiles(),
              }),
            ],
          });
        });
      if (this.showSendEmailButton) {
        this.toolbarService.createItemGroup({
          items: [
            this.toolbarService.createToolbarButton('sendEmail', {
              iconName: signal('envelope'),
              caption: signal('common.sendemail'),
              tooltip: signal('common.sendemail'),
              disabled: computed(() => !this.hasRowsSelected()),
              onAction: () => {
                this.openSendEmailDialog();
              },
            }),
          ],
        });
      }
    }
  }

  override refreshGrid() {
    this.helper?.loadFiles(true, true).subscribe();
    this.onReload.emit();
  }

  override onGridReadyToDefine(grid: GridComponent<FileRecordDTO>): void {
    super.onGridReadyToDefine(grid);

    if (this.helper?.useExtended) {
      this.addExtendedGridColumns();
    } else {
      this.addGeneralGridColumns();
    }
  }

  protected gridRowSelectionChanged(selectedRows: FileRecordDTO[]): void {
    this.hasRowsSelected.set(selectedRows.length > 0);
  }

  private addGeneralGridColumns() {
    this.translate
      .get([
        'core.filename',
        'common.description',
        'common.fileextension',
        'common.created',
        'common.download',
        'common.filereplace',
        'core.open',
        'common.editroles',
        'core.delete',
        'common.createdby',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.enableRowSelection(undefined, false);
        this.grid.addColumnText('fileName', terms['core.filename'], {
          flex: 2,
        });
        this.grid.addColumnText('description', terms['common.description'], {
          flex: 2,
          editable: true,
        });
        this.grid.addColumnText('extension', terms['common.fileextension'], {
          flex: 1,
        });
        this.grid.addColumnDateTime('created', terms['common.created'], {
          sort: 'desc',
          flex: 1,
        });
        this.grid.addColumnText('createdBy', terms['common.createdby'], {
          flex: 1,
        });

        if (this.allowReplace) {
          this.grid.addColumnDateTime('modified', terms['common.modified'], {
            flex: 1,
          });
          this.grid.addColumnText('modifiedBy', terms['common.modifiedby'], {
            flex: 1,
          });
        }

        this.grid.addColumnIcon('', '', {
          tooltip: terms['common.download'],
          enableHiding: true,
          pinned: 'right',
          iconName: 'download',
          onClick: row => {
            this.helper?.download(row);
          },
        });

        if (this.allowReplace) {
          this.grid.addColumnIcon('', '', {
            tooltip: terms['common.filereplace'],
            enableHiding: true,
            pinned: 'right',
            iconName: 'arrow-right-arrow-left',
            onClick: row => {
              this.openReplaceFileDialog(row);
            },
          });
        }

        this.grid.addColumnIcon('', '', {
          tooltip: terms['core.open'],
          enableHiding: true,
          pinned: 'right',
          iconName: 'eye',
          showIcon: row => {
            return FilePreviewDialogData.canBePreviewed(row.extension);
          },
          onClick: row => {
            this.openPreviewDialog(row);
          },
        });

        if (!this.isReadOnly && !this.helper?.isReadOnly) {
          this.grid.addColumnIconDelete({
            tooltip: terms['core.delete'],
            onClick: (row: FileRecordDTO) => {
              this.openConfirmDeleteDialog(row);
            },
          });

          this.grid.agGrid.api.updateGridOptions({
            onCellValueChanged: event => {
              if (event.colDef.field == 'description') {
                this.helper?.update(event.data, 'description');
              }
              this.fileRecordChanged.emit(event.data);
            },
          });
        }

        this.grid.setNbrOfRowsToShow(10, 10);
        super.finalizeInitGrid();
      });
  }

  private addExtendedGridColumns() {
    this.translate
      .get([
        'common.type',
        'core.source',
        'core.filename',
        'common.description',
        'common.fileextension',
        'common.created',
        'common.createdby',
        'common.download',
        'core.open',
        'common.editroles',
        'core.delete',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.enableRowSelection(undefined, false);
        this.grid.addColumnText('entityTypeName', terms['common.type'], {
          flex: 2,
          enableGrouping: true,
          grouped: true,
        });
        this.grid.addColumnText('identifierName', terms['core.source'], {
          flex: 2,
          enableGrouping: true,
          grouped: true,
          buttonConfiguration: {
            iconPrefix: 'fal',
            iconClass: 'iconEdit',
            iconName: 'pen',
            onClick: row => this.openOrigin(row),
            show: row => Boolean(row && this.hasPermission(row.entity)),
          },
        });
        this.grid.addColumnText('fileName', terms['core.filename'], {
          flex: 2,
        });
        this.grid.addColumnText('description', terms['common.description'], {
          flex: 2,
          editable: true,
        });
        this.grid.addColumnText('extension', terms['common.fileextension'], {
          flex: 1,
        });
        this.grid.addColumnDateTime('created', terms['common.created'], {
          sort: 'desc',
          flex: 1,
        });
        this.grid.addColumnText('createdBy', terms['common.createdby'], {
          flex: 1,
        });
        this.grid.addColumnIcon('', '', {
          tooltip: terms['common.download'],
          enableHiding: true,
          pinned: 'right',
          iconName: 'download',
          onClick: row => {
            this.helper?.download(row);
          },
        });

        this.grid.addColumnIcon('', '', {
          tooltip: terms['core.open'],
          enableHiding: true,
          pinned: 'right',
          iconName: 'eye',
          showIcon: row => {
            return Boolean(
              row && FilePreviewDialogData.canBePreviewed(row.extension)
            );
          },
          onClick: row => {
            this.openPreviewDialog(row);
          },
        });

        if (!this.isReadOnly && !this.helper?.isReadOnly) {
          this.grid.addColumnIconDelete({
            tooltip: terms['core.delete'],
            onClick: (row: FileRecordDTO) => {
              this.openConfirmDeleteDialog(row);
            },
          });

          this.grid.agGrid.api.updateGridOptions({
            onCellValueChanged: event => {
              if (event.colDef.field == 'description') {
                this.helper?.update(event.data, 'description');
              }
              this.fileRecordChanged.emit(event.data);
            },
          });
        }

        this.grid.useGrouping();

        super.finalizeInitGrid();
      });
  }

  private hasPermission(originType: SoeEntityType) {
    switch (originType) {
      case SoeEntityType.Offer:
        return this.helper?.hasExtendedPermission(
          Feature.Billing_Offer_Offers_Edit
        );
      case SoeEntityType.Order:
        return this.helper?.hasExtendedPermission(
          Feature.Billing_Order_Orders_Edit
        );
      case SoeEntityType.Contract:
        return this.helper?.hasExtendedPermission(
          Feature.Billing_Contract_Contracts_Edit
        );
      case SoeEntityType.CustomerInvoice:
        return this.helper?.hasExtendedPermission(
          Feature.Billing_Invoice_Invoices_Edit
        );
      default:
        return false;
    }
  }

  // EVENTS

  openConfirmDeleteDialog(fileRecord: FileRecordDTO) {
    const mb = this.messageboxService.warning(
      'core.delete',
      'core.deletewarning'
    );
    mb.afterClosed().subscribe((response: IMessageboxComponentResponse) => {
      if (response?.result) this.helper?.delete(fileRecord);
    });
  }

  openReplaceFileDialog(row: FileRecordDTO) {
    const fileUploader = this.helper
      ? new FileUploader(this.helper, row.fileId)
      : undefined;
    this.dialogService.open<FileUploadDialogComponent, FileUploadDialogData>(
      FileUploadDialogComponent,
      fileReplaceDialogData(fileUploader, row)
    );
  }

  openUploadFileDialog() {
    const fileUploader = this.helper
      ? new FileUploader(this.helper)
      : undefined;
    this.dialogService.open<FileUploadDialogComponent, FileUploadDialogData>(
      FileUploadDialogComponent,
      defaultFileUploadDialogData(fileUploader)
    );
  }

  openPreviewDialog(file: FileRecordDTO) {
    if (FilePreviewDialogData.canBePreviewed(file.extension)) {
      this.helper?.downloadFile(file).subscribe(data => {
        const dialogData = new FilePreviewDialogData(
          file.fileName,
          data.data,
          file.extension
        );
        this.dialogService.open(FileViewerComponent, dialogData);
      });
    }
  }

  openOrigin(row: FileRecordDTO) {
    if (row.identifierId) {
      switch (row.entity) {
        case SoeEntityType.Offer:
          const offerUrl = `/soe/billing/offer/status/default.aspx?classificationgroup=${SoeOriginStatusClassificationGroup.HandleOrders}&invoiceId=${row.identifierId}&invoiceNr=${row.identifierNumber}`;
          BrowserUtil.openInNewTab(window, offerUrl);
          break;
        case SoeEntityType.Order:
          const orderUrl = `/soe/billing/order/status/default.aspx?classificationgroup=${SoeOriginStatusClassificationGroup.HandleOrders}&invoiceId=${row.identifierId}&invoiceNr=${row.identifierNumber}`;
          BrowserUtil.openInNewTab(window, orderUrl);
          break;
        case SoeEntityType.Contract:
          const contractUrl = `/soe/billing/contract/status/default.aspx?classificationgroup=${SoeOriginStatusClassificationGroup.HandleOrders}&invoiceId=${row.identifierId}&invoiceNr=${row.identifierNumber}`;
          BrowserUtil.openInNewTab(window, contractUrl);
          break;
        case SoeEntityType.CustomerInvoice:
          const invoiceUrl = `/soe/billing/invoice/status/default.aspx?classificationgroup=${SoeOriginStatusClassificationGroup.HandleOrders}&invoiceId=${row.identifierId}&invoiceNr=${row.identifierNumber}`;
          BrowserUtil.openInNewTab(window, invoiceUrl);
          break;
      }
    }
  }

  downloadSelectedFiles(): void {
    const selectedRows = this.grid.getSelectedRows();
    if (selectedRows.length === 0) return;

    const ids = selectedRows.map(r => r.fileRecordId);
    const pageName = this.pageName || '';

    this.helper?.downloadFilesAsZip(ids, pageName);
  }

  openSendEmailDialog(): void {
    const selectedFiles = this.grid.getSelectedRows() as FileRecordDTO[];
    if (selectedFiles.length === 0) return;

    if (this.emailGetter) {
      this.emailGetter()
        .pipe(take(1))
        .subscribe(email => {
          this.openEmailDialogWithEmail(email, selectedFiles);
        });
    }
  }

  private openEmailDialogWithEmail(
    email: string,
    selectedFiles: FileRecordDTO[]
  ) {
    this.dialogService
      .open(SelectEmailDialogComponent, {
        title: 'common.sendemail',
        size: 'md',
        defaultEmail: email,
        hideTemplatePart: true,
        showAddRecipient: true,
        showAttachments: true,
        hideTemplate: true,
        attachments: selectedFiles,
        isSendEmailDocuments: true,
        grid: false,
      })
      .afterClosed()
      .subscribe((result: SelectEmailDialogCloseData) => {
        if (!result) return;
        let recipients: string[] = [];
        if (result.emailAddresses && result.attachments.length > 0) {
          recipients = result.emailAddresses?.split(/[;,]/);
        }
        const model: IEmailDocumentsRequestDTO = {
          fileRecordIds: result.attachments
            .filter(f => f.isSelected)
            .map(f => f.fileRecordId),
          recipientUserIds: [],
          singleRecipient: email || '',
          emailAddresses: recipients,
          subject: this.pageName
            ? `Documents from ${this.pageName}`
            : 'Documents',
          body: 'Please find the attached documents.',
        };
        if (model.fileRecordIds.length === 0) {
          this.toasterService.warning(
            this.translate.instant('common.selectattachments'),
            ''
          );
          return;
        }
        this.helper
          ?.sendFilesAsEmail(model)
          .pipe(
            tap((res: any) => {
              if (res.success) {
                this.toasterService.success(
                  this.translate.instant('common.sent'),
                  ''
                );
              }
            })
          )
          .subscribe();
      });
  }
}

class FileUploader implements IFileUploader {
  /**
   * In charge of the actual file upload.
   * Will dynamically update the grid with the new file if successful.
   */
  constructor(
    private fileHelper: FilesHelper,
    private dataStorageId: number = 0
  ) {}
  uploadFile(file: AttachedFile) {
    const req =
      this.dataStorageId > 0
        ? this.fileHelper.replaceFile(file, this.dataStorageId)
        : this.fileHelper.saveFile(file);

    return req.pipe(
      map(res => ({
        success: res.success,
        message: ResponseUtil.getErrorMessage(res),
      }))
    );
  }
}
