import { Component, inject, Input, OnInit } from '@angular/core';
import { AccountDimsForm } from '@shared/components/account-dims/account-dims-form.model';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { CrudActionTypeEnum } from '@shared/enums';
import { TermCollection } from '@shared/localization/term-types';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { ICustomerIODTO } from '@shared/models/generated-interfaces/CustomerDTO';
import {
  ICustomerInvoiceIODTO,
  ICustomerInvoiceRowIODTO,
} from '@shared/models/generated-interfaces/CustomerInvoiceIODTOs';
import {
  Feature,
  TermGroup,
  TermGroup_IOImportHeadType,
  TermGroup_IOStatus,
} from '@shared/models/generated-interfaces/Enumerations';
import { IProjectIODTO } from '@shared/models/generated-interfaces/ProjectDTOs';
import { ISupplierIODTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { ISupplierInvoiceHeadIODTO } from '@shared/models/generated-interfaces/SupplierInvoiceIODTOs';
import { IVoucherHeadIODTO } from '@shared/models/generated-interfaces/VoucherHeadDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { ReportService } from '@shared/services/report.service';
import { Perform } from '@shared/util/perform.class';
import { GridResizeType } from '@ui/grid/enums/resize-type.enum';
import { IMessageboxComponentResponse } from '@ui/dialog/models/messagebox';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { CellClassParams, CellValueChangedEvent } from 'ag-grid-community';
import { Observable, take, tap } from 'rxjs';
import { ImportGridColumnDTO } from '../../models/import-grid-columns-dto.model';
import { ImportConnectService } from '../../services/import-connect.service';
import { RequestReportService } from '@shared/services/request-report.service';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';
import { ResponseUtil } from '@shared/util/response-util';

@Component({
  selector: 'soe-import-rows',
  templateUrl: './import-rows.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
/* eslint-disable @typescript-eslint/no-explicit-any */
export class ImportRowsComponent
  extends GridBaseDirective<any>
  implements OnInit
{
  // Setup
  @Input() batchId!: string;
  @Input() importHeadType!: number;
  @Input() useAccountDistribution!: boolean;
  @Input() useAccountDimensions!: boolean;
  @Input() defaultDimAccounts!: AccountDimsForm;
  @Input() updateExistingInvoice!: boolean;

  // Flags
  showPrintButton = false;

  // Data
  importRows: any[] = [];
  statusList: SmallGenericType[] = [];

  // Collections
  gridColumns!: ImportGridColumnDTO[];

  coreService = inject(CoreService);
  connectService = inject(ImportConnectService);
  notificationService = inject(MessageboxService);
  reportService = inject(ReportService);
  progressService = inject(ProgressService);
  private readonly requestReportService = inject(RequestReportService);

  performAction = new Perform<any[]>(this.progressService);

  protected loadingIOResult = false;
  protected importingRows = false;
  protected processingRowsSelected = false;
  protected isPrinting = false;

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(Feature.None, 'Common.Connect.ImportRows', {
      skipInitialLoad: true,
      skipDefaultToolbar: true,
      lookups: [this.loadColumns(), this.loadStatus()],
    });

    if (
      this.importHeadType == TermGroup_IOImportHeadType.CustomerInvoice ||
      this.importHeadType == TermGroup_IOImportHeadType.Voucher
    )
      this.showPrintButton = true;
  }

  override onFinished(): void {
    this.grid.cellValueChanged.subscribe(row => {
      this.setRowAsModified(row);
    });

    this.loadImportIOResult();
  }

  loadImportIOResult() {
    this.loadingIOResult = true;
    return this.connectService
      .getImportIOResult(this.importHeadType, this.batchId ?? '')
      .pipe(
        take(1),
        tap((x: any) => {
          x.forEach((row: any) => {
            const status = this.statusList.find(
              status => status.id == row.status
            );
            row.statusName = status?.name;

            if (row.status == TermGroup_IOStatus.Error) {
              row.statusColor = 'red';
            } else if (row.status == TermGroup_IOStatus.Processed) {
              row.statusColor = 'green';
            } else if (row.status == TermGroup_IOStatus.UnderProcessing) {
              row.statusColor = 'yellow';
            } else {
              row.statusColor = 'black';
            }
          });

          this.importRows = x;
          this.setImportRows();

          this.grid.resizeColumns(GridResizeType.AutoAllAndHeaders);
          this.grid.refreshCells();
          this.loadingIOResult = false;
        })
      )
      .subscribe();
  }

  private setImportRows() {
    this.rowData.next(this.importRows);
  }

  private loadStatus(): Observable<SmallGenericType[]> {
    return this.coreService
      .getTermGroupContent(TermGroup.IOStatus, false, false)
      .pipe(
        take(1),
        tap(x => {
          this.statusList = x;
        })
      );
  }

  private loadColumns(): Observable<ImportGridColumnDTO[]> {
    return this.connectService.getImportGridColumns(this.importHeadType).pipe(
      take(1),
      tap(x => {
        this.gridColumns = x;
      })
    );
  }

  override selectionChanged(rows: any[]): void {
    super.selectionChanged(rows);
    this.processingRowsSelected = rows.some(
      (row: any) => row.status == TermGroup_IOStatus.UnderProcessing
    );
  }

  override loadTerms(): Observable<TermCollection> {
    const translationsKeys = [
      'common.connect.savenotsuccess',
      'core.error',
      'core.failed',
      'core.aggrid.totals.filtered',
      'core.aggrid.totals.total',
      'core.aggrid.totals.selected',
      'common.connect.standardreportmissing',
      'common.obs',
      'common.connect.unsavedchangesmessage',
      'common.connect.importdata',
      'common.connect.importsuccess',
      'common.connect.importnotsuccess',
      'core.info',
      'common.connect.importcompleted.with.errors',
      'common.connect.importcompleted.success.partialy',
    ];
    return super.loadTerms(translationsKeys);
  }

  override onGridReadyToDefine(grid: any): void {
    super.onGridReadyToDefine(grid);

    const ignoredColumns: string[] = [
      'status',
      'statusName',
      'errorMessage',
      'actorCustomerId',
      'customerIoId',
      'customerInvoiceHeadIoId',
      'invoiceId',
      'supplierIoId',
      'supplierInvoiceHeadIoId',
      'supplierId',
      'voucherHeadIoId',
      'actorCompanyId',
      'import',
      'importId',
      'type',
      'source',
      'batchId',
      'state',
      'created',
      'createdBy',
      'modified',
      'modifiedBy',
      'entityState',
      'entityKey',
    ];

    const errorCellRules = {
      'error-background-color': (params: CellClassParams) => {
        return params?.data?.status == TermGroup_IOStatus.Error;
      },
    };

    this.grid.enableRowSelection((row: any) => {
      return row.data.status !== TermGroup_IOStatus.Processed;
    }, false);

    this.grid.addColumnModified('isModified', {
      cellClassRules: errorCellRules,
    });
    this.grid.addColumnText('statusName', 'Statusname', {
      cellClassRules: errorCellRules,
      shapeConfiguration: {
        shape: 'circle',
        colorField: 'statusColor',
        width: 16,
      },
    });
    this.grid.addColumnText('errorMessage', 'Errormessage', {
      cellClassRules: errorCellRules,
    });

    const aggregations: any = {};
    for (const element of this.gridColumns) {
      const gridColumn = element;

      if (ignoredColumns.indexOf(gridColumn.columnName) > -1) continue;

      if (gridColumn.columnType == 'decimal') {
        this.grid.addColumnNumber(
          gridColumn.columnName,
          gridColumn.headerName,
          {
            cellClassRules: errorCellRules,
            editable: this.isEditable(gridColumn.columnName),
            decimals: 2,
          }
        );
        aggregations[gridColumn.columnName] = 'sum';
      } else if (gridColumn.columnType == 'int32')
        this.grid.addColumnNumber(
          gridColumn.columnName,
          gridColumn.headerName,
          {
            cellClassRules: errorCellRules,
            editable: this.isEditable(gridColumn.columnName),
          }
        );
      else if (gridColumn.columnType == 'datetime')
        this.grid.addColumnDate(gridColumn.columnName, gridColumn.headerName, {
          cellClassRules: errorCellRules,
          editable: this.isEditable(gridColumn.columnName),
        });
      else if (gridColumn.columnType == 'boolean')
        this.grid.addColumnBool(gridColumn.columnName, gridColumn.headerName, {
          cellClassRules: errorCellRules,
          editable: this.isEditable(gridColumn.columnName),
        });
      else
        this.grid.addColumnText(gridColumn.columnName, gridColumn.headerName, {
          cellClassRules: errorCellRules,
          editable: this.isEditable(gridColumn.columnName),
        });
    }

    let rows = this.importRows ? this.importRows.length : 0;
    if (rows < 10) rows = 10;
    if (rows > 30) rows = 30;

    this.grid.setNbrOfRowsToShow(rows);
    this.grid.finalizeInitGrid();
  }

  private isEditable(columnName: string): boolean {
    let isEditable: boolean = true;
    const readOnlyColumns: string[] = ['status', 'statusName', 'errorMessage'];

    if (readOnlyColumns.indexOf(columnName) > -1) isEditable = false;

    return isEditable;
  }

  setRowAsModified(row: CellValueChangedEvent) {
    if (row.data) {
      row.data.isModified = true;
      this.grid.api.refreshCells();
    }
  }

  save() {
    if (this.saveByImportHeadType() == null) return;
    const saveAction =
      this.saveByImportHeadType() as Observable<BackendResponse>;
    this.performAction.crud(CrudActionTypeEnum.Save, saveAction);
  }

  saveByImportHeadType(): Observable<BackendResponse> | null {
    const modifiedRows = this.grid.getAllRows().filter(x => x.isModified);
    if (this.importHeadType == TermGroup_IOImportHeadType.Customer) {
      //Customers
      const customerDTO: ICustomerIODTO[] = [];

      for (let i = 0; i < modifiedRows.length; i++) {
        const row = modifiedRows[i];
        customerDTO.push(row);
      }

      return this.connectService.saveCustomerIODTO(customerDTO).pipe(
        take(1),
        tap((result: BackendResponse) => {
          if (result.success) {
            this.loadImportIOResult();
          } else {
            this.notificationService.error(
              this.terms['core.error'],
              this.terms['common.connect.savenotsuccess']
            );
          }
        })
      );
    } else if (
      this.importHeadType == TermGroup_IOImportHeadType.CustomerInvoice
    ) {
      //Customer invoice heads
      const customerInvoiceDTO: ICustomerInvoiceIODTO[] = [];

      for (let i = 0; i < modifiedRows.length; i++) {
        const row = modifiedRows[i];
        customerInvoiceDTO.push(row);
      }

      return this.connectService
        .saveCustomerInvoiceHeadIODTO(customerInvoiceDTO)
        .pipe(
          take(1),
          tap((result: BackendResponse) => {
            if (result.success) {
              this.loadImportIOResult();
            } else {
              this.notificationService.error(
                this.terms['core.error'],
                this.terms['common.connect.savenotsuccess']
              );
            }
          })
        );
    } else if (
      this.importHeadType == TermGroup_IOImportHeadType.CustomerInvoiceRow
    ) {
      //Customer invoice rows
      const customerInvoiceRowDTO: ICustomerInvoiceRowIODTO[] = [];

      for (let i = 0; i < modifiedRows.length; i++) {
        const row = modifiedRows[i];
        customerInvoiceRowDTO.push(row);
      }

      return this.connectService
        .saveCustomerInvoiceRowIODTO(customerInvoiceRowDTO)
        .pipe(
          take(1),
          tap((result: BackendResponse) => {
            if (result.success) {
              this.loadImportIOResult();
            } else {
              this.notificationService.error(
                this.terms['core.error'],
                this.terms['common.connect.savenotsuccess']
              );
            }
          })
        );
    } else if (this.importHeadType == TermGroup_IOImportHeadType.Supplier) {
      //Suppliers
      const supplierDTO: ISupplierIODTO[] = [];

      for (let i = 0; i < modifiedRows.length; i++) {
        const row = modifiedRows[i];
        supplierDTO.push(row);
      }

      return this.connectService.saveSupplierIODTO(supplierDTO).pipe(
        take(1),
        tap((result: BackendResponse) => {
          if (result.success) {
            this.loadImportIOResult();
          } else {
            this.notificationService.error(
              this.terms['core.error'],
              this.terms['common.connect.savenotsuccess']
            );
          }
        })
      );
    } else if (
      this.importHeadType == TermGroup_IOImportHeadType.SupplierInvoice ||
      this.importHeadType == TermGroup_IOImportHeadType.SupplierInvoiceAnsjo
    ) {
      //Supplier invoice heads
      const supplierInvoiceDTO: ISupplierInvoiceHeadIODTO[] = [];

      for (let i = 0; i < modifiedRows.length; i++) {
        const row = modifiedRows[i];
        supplierInvoiceDTO.push(row);
      }

      return this.connectService
        .saveSupplierInvoiceHeadIODTO(supplierInvoiceDTO)
        .pipe(
          take(1),
          tap((result: BackendResponse) => {
            if (result.success) {
              this.loadImportIOResult();
            } else {
              this.notificationService.error(
                this.terms['core.error'],
                this.terms['common.connect.savenotsuccess']
              );
            }
          })
        );
    } else if (this.importHeadType == TermGroup_IOImportHeadType.Voucher) {
      //Vouchers
      const voucherDTO: IVoucherHeadIODTO[] = [];

      for (let i = 0; i < modifiedRows.length; i++) {
        const row = modifiedRows[i];
        voucherDTO.push(row);
      }

      return this.connectService.saveVoucherHeadIODTO(voucherDTO).pipe(
        take(1),
        tap((result: BackendResponse) => {
          if (result.success) {
            this.loadImportIOResult();
          } else {
            this.notificationService.error(
              this.terms['core.error'],
              this.terms['common.connect.savenotsuccess']
            );
          }
        })
      );
    } else if (this.importHeadType == TermGroup_IOImportHeadType.Project) {
      //Projects
      const projectDTO: IProjectIODTO[] = [];

      for (let i = 0; i < modifiedRows.length; i++) {
        const row = modifiedRows[i];
        projectDTO.push(row);
      }

      return this.connectService.saveProjectIODTO(projectDTO).pipe(
        take(1),
        tap((result: BackendResponse) => {
          if (result.success) {
            this.loadImportIOResult();
          } else {
            this.notificationService.error(
              this.terms['core.error'],
              this.terms['common.connect.savenotsuccess']
            );
          }
        })
      );
    } else {
      return null;
    }
  }

  initImportSelectedRows() {
    if (this.useAccountDimensions) {
      const modal = this.notificationService.warning(
        this.translate.instant('core.warning'),
        this.translate.instant('common.connect.accountdimwarning'),
        {
          size: 'md',
        }
      );
      modal
        .afterClosed()
        .pipe(
          take(1),
          tap((res: IMessageboxComponentResponse) => {
            if (res.result) this.importSelectedRows();
          })
        )
        .subscribe();
    } else {
      this.importSelectedRows();
    }
  }

  private importSelectedRows() {
    const modifiedRows = this.grid.getAllRows().filter(x => x.isModified);

    if (modifiedRows.length > 0) {
      this.notificationService.warning(
        this.terms['common.obs'],
        this.terms['common.connect.unsavedchangesmessage']
      );
    } else {
      const selectedRows = this.grid.getSelectedRows();

      const ioIds: any[] = [];
      for (let i = 0; i < selectedRows.length; i++) {
        const row = selectedRows[i];

        if (this.importHeadType == TermGroup_IOImportHeadType.Customer)
          ioIds.push(row.customerIOId);
        else if (
          this.importHeadType == TermGroup_IOImportHeadType.CustomerInvoice
        )
          ioIds.push(row.customerInvoiceHeadIOId);
        else if (
          this.importHeadType == TermGroup_IOImportHeadType.CustomerInvoiceRow
        )
          ioIds.push(row.customerInvoiceRowIOId);
        else if (this.importHeadType == TermGroup_IOImportHeadType.Supplier)
          ioIds.push(row.supplierIOId);
        else if (
          this.importHeadType == TermGroup_IOImportHeadType.SupplierInvoice ||
          this.importHeadType == TermGroup_IOImportHeadType.SupplierInvoiceAnsjo
        )
          ioIds.push(row.supplierInvoiceHeadIOId);
        else if (this.importHeadType == TermGroup_IOImportHeadType.Voucher)
          ioIds.push(row.voucherHeadIOId);
        else if (this.importHeadType == TermGroup_IOImportHeadType.Project)
          ioIds.push(row.projectIOId);
      }

      this.importingRows = true;
      this.performLoadData
        .load$(
          this.connectService.importIO(
            this.importHeadType,
            ioIds,
            this.useAccountDistribution,
            this.useAccountDimensions,
            this.defaultDimAccounts.account2.getRawValue(),
            this.defaultDimAccounts.account3.getRawValue(),
            this.defaultDimAccounts.account4.getRawValue(),
            this.defaultDimAccounts.account5.getRawValue(),
            this.defaultDimAccounts.account6.getRawValue()
          )
        )
        .subscribe((result: BackendResponse) => {
          if (result.success) {
            this.notificationService.information(
              this.terms['core.info'],
              this.terms['common.connect.importsuccess']
            );
          } else {
            this.setErrorNotification(ResponseUtil.getStringValue(result));
          }
          this.loadImportIOResult();
        })
        .add(() => {
          this.importingRows = false;
        });
    }
  }

  private setErrorNotification(stringCounters: string) {
    let errorMessage = '';
    if (stringCounters) {
      const [successNumber, errorNumber] = stringCounters
        .split(',')
        .map(Number);
      if (successNumber === 0) {
        errorMessage += this.terms['common.connect.importnotsuccess'];
      } else {
        if (successNumber > 0 && errorNumber > 0) {
          errorMessage +=
            this.terms['common.connect.importcompleted.with.errors'] + '\n';
          errorMessage +=
            successNumber +
            ' ' +
            this.terms['common.connect.importcompleted.success.partialy'];

          errorMessage += '\n' + errorNumber + ' ' + this.terms['core.failed'];
        }
      }

      this.notificationService.error(this.terms['core.error'], errorMessage);
    }
  }

  protected print(): void {
    const selectedRows = this.grid.getSelectedRows();

    if (this.importHeadType == TermGroup_IOImportHeadType.CustomerInvoice) {
      const ioIds: number[] = selectedRows.map(
        row => row.customerInvoiceHeadIOId
      );
      this.isPrinting = true;
      this.performLoadData.load(
        this.requestReportService.printIOCustomerInvoice(ioIds).pipe(
          tap(() => {
            this.isPrinting = false;
          })
        )
      );
    } else if (this.importHeadType == TermGroup_IOImportHeadType.Voucher) {
      const ioIds: number[] = selectedRows.map(row => row.voucherHeadIOId);
      this.isPrinting = true;
      this.performLoadData.load(
        this.requestReportService.printIOVoucher(ioIds).pipe(
          tap(() => {
            this.isPrinting = false;
          })
        )
      );
    }
  }
}

/* eslint-enable @typescript-eslint/no-explicit-any */
