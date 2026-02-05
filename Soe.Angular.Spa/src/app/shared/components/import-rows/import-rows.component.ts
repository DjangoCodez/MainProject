import { Component, inject, Input, OnInit, signal } from '@angular/core';
import { ImportIOModel } from '@features/economy/connect-importer/models/connect-importer.model';
import { ConnectImporterService } from '@features/economy/connect-importer/services/connect-importer.service';
import { ImportGridColumnDTO } from '@features/economy/import-connect/models/import-grid-columns-dto.model';
import { TranslateService } from '@ngx-translate/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { TermCollection } from '@shared/localization/term-types';
import {
  Feature,
  SettingMainType,
  SoeReportTemplateType,
  SoeReportType,
  TermGroup,
  TermGroup_IOImportHeadType,
  TermGroup_IOStatus,
} from '@shared/models/generated-interfaces/Enumerations';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { ReportService } from '@shared/services/report.service';
import { BrowserUtil } from '@shared/util/browser-util';
import { Perform } from '@shared/util/perform.class';
import { AggregationType } from '@ui/grid/interfaces';
import { GridComponent } from '@ui/grid/grid.component';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { CellValueChangedEvent } from 'ag-grid-community';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import {
  CustomerInvoiceIODTO,
  CustomerInvoiceRowIODTO,
  CustomerIODTO,
  ImportIODTO,
  ProjectIODTO,
  SupplierInvoiceHeadIODTO,
  SupplierIODTO,
  VoucherHeadIODTO,
} from './models/import-rows.model';
import { RequestReportService } from '@shared/services/request-report.service';

@Component({
  selector: 'soe-import-rows',
  templateUrl: './import-rows.component.html',
  styleUrl: './import-rows.component.scss',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class ImportRowsComponent
  extends GridBaseDirective<ImportIODTO>
  implements OnInit
{
  @Input() importHeadType: number = 0;
  @Input() batchId: string = '';
  @Input() useAccountDistribution: boolean = false;
  @Input() useAccountDimensions: boolean = false;

  @Input() defaultDim2Account: number = 0;
  @Input() defaultDim3Account: number = 0;
  @Input() defaultDim4Account: number = 0;
  @Input() defaultDim5Account: number = 0;
  @Input() defaultDim6Account: number = 0;
  updateExistingInvoice: boolean = false;

  showPrintButton = signal(false);
  disableImportButton = signal(true);

  showImportButton = signal(false);
  coreService = inject(CoreService);
  reportService = inject(ReportService);
  connectImporterService = inject(ConnectImporterService);
  messageboxService = inject(MessageboxService);
  progressService = inject(ProgressService);
  translate = inject(TranslateService);
  private readonly requestReportService = inject(RequestReportService);

  performLoad = new Perform<any>(this.progressService);

  statusList: ISmallGenericType[] = [];
  gridColumns: ImportGridColumnDTO[] = [];
  rows = new BehaviorSubject<ImportIODTO[]>([]);
  printAllText = this.translate.instant('common.printall');
  printText = this.translate.instant('common.report.report.print');
  printAll = signal(true);
  alwaysPrintAll = signal(true);
  printButtonLable = signal(this.printAllText);

  protected isPrinting = false;

  ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(Feature.None, 'Common.Connect.ImportRows', {
      skipInitialLoad: true,
      lookups: this.loadStatus(),
    });

    if (
      this.importHeadType == TermGroup_IOImportHeadType.CustomerInvoice ||
      this.importHeadType == TermGroup_IOImportHeadType.Voucher
    )
      this.showPrintButton.set(true);
    this.setSubscribers();
  }

  setSubscribers() {
    this.rows.subscribe(rows => {
      this.showImportButton.set(false);
      if (rows.length > 0) {
        let canHide = true;
        rows.forEach(f => {
          if (f.status !== TermGroup_IOStatus.Processed) {
            canHide = false;
          }
        });
        if (!canHide) {
          this.showImportButton.set(true);
        }
      }
    });
  }

  reloadData() {
    this.loadImportIOResult();
  }
  setupGridColumns(grid: GridComponent<ImportIODTO>) {
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
    this.grid.api.updateGridOptions({
      onCellValueChanged: this.onCellValueChanged.bind(this),
    });
    this.grid.enableRowSelection((row: any) => {
      return row.data.status !== TermGroup_IOStatus.Processed;
    }, false);
    this.grid.addColumnModified('isModified', {});
    this.grid.addColumnText('statusName', 'Statusname', {
      width: 100,
      minWidth: 100,
      editable: false,
    });
    this.grid.addColumnText('errorMessage', 'Errormessage', {
      width: 100,
      minWidth: 100,
      editable: false,
    });
    const aggregations = {};
    for (let i = 0; i < this.gridColumns.length; i++) {
      const gridColumn = this.gridColumns[i];

      if (ignoredColumns.indexOf(gridColumn.columnName) > -1) continue;

      if (gridColumn.columnType == 'decimal') {
        this.grid.addColumnNumber(
          gridColumn.columnName,
          gridColumn.headerName,
          {
            decimals: 2,
            width: 50,
            minWidth: 50,
            editable: true,
            clearZero: true,
          }
        );
        this.addProp(aggregations, gridColumn.columnName, AggregationType.Sum);
      } else if (gridColumn.columnType == 'int32')
        this.grid.addColumnNumber(
          gridColumn.columnName,
          gridColumn.headerName,
          { width: 50, minWidth: 50, editable: true, clearZero: true }
        );
      else if (gridColumn.columnType == 'datetime')
        this.grid.addColumnDate(gridColumn.columnName, gridColumn.headerName, {
          width: 50,
          minWidth: 50,
          editable: true,
        });
      else if (gridColumn.columnType == 'boolean')
        this.grid.addColumnBool(gridColumn.columnName, gridColumn.headerName, {
          editable: true,
          width: 40,
          minWidth: 40,
        });
      else {
        this.grid.addColumnText(gridColumn.columnName, gridColumn.headerName, {
          width: 100,
          minWidth: 100,
          editable: gridColumn.columnName !== 'status',
        });
      }
    }

    this.grid.columns.forEach(col => {
      const cellcls: string = col.cellClass ? col.cellClass.toString() : '';
      col.cellClass = (grid: any) => {
        if (grid.data['status'] === TermGroup_IOStatus.Error)
          return cellcls + ' error-background-color';
        else return cellcls;
      };
    });

    this.grid.addAggregationsRow(aggregations);
    super.finalizeInitGrid();
    this.reloadData();
  }

  addProp<T extends object, K extends PropertyKey, V>(
    obj: T,
    key: K,
    value: V
  ): asserts obj is T & { [P in K]: V } {
    Object.assign(obj, { [key]: value });
  }

  //#region UI Events
  private onCellValueChanged(event: CellValueChangedEvent) {
    if (event.oldValue !== event.newValue) {
      const rowData = event.data as ImportIODTO;
      rowData.isModified = true;
      event.api.refreshCells();
    }
  }

  selectionChanged(data: ImportIODTO[]) {
    const isRowsSelected = data.length === 0;
    this.disableImportButton.set(isRowsSelected);
    this.printAll.set(isRowsSelected);
    this.printButtonLable.set(
      isRowsSelected || this.alwaysPrintAll()
        ? this.printAllText
        : this.printText
    );
  }
  //#endregion

  //#region UI Functions

  protected print(): void {
    let selectedRows: ImportIODTO[];

    if (this.printAll()) {
      selectedRows = this.grid.getAllRows();
    } else {
      selectedRows = this.grid.getSelectedRows();
    }

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

  public save() {
    const modifiedRows = this.rows.getValue().filter(r => r.isModified);

    if (this.importHeadType == TermGroup_IOImportHeadType.Customer) {
      //Customers
      const customerDTO: CustomerIODTO[] = [];

      for (let i = 0; i < modifiedRows.length; i++) {
        customerDTO.push(modifiedRows[i]);
      }

      this.connectImporterService.saveCustomerIODTO(customerDTO).pipe(
        tap((result: any) => {
          if (result.success) {
            this.reloadData();
          } else {
            this.messageboxService.error(
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
      const customerInvoiceDTO: CustomerInvoiceIODTO[] = [];

      for (let i = 0; i < modifiedRows.length; i++) {
        customerInvoiceDTO.push(modifiedRows[i]);
      }

      this.connectImporterService
        .saveCustomerInvoiceHeadIODTO(customerInvoiceDTO)
        .pipe(
          tap((result: any) => {
            if (result.success) {
              this.reloadData();
            } else {
              this.messageboxService.error(
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
      const customerInvoiceRowDTO: CustomerInvoiceRowIODTO[] = [];

      for (let i = 0; i < modifiedRows.length; i++) {
        customerInvoiceRowDTO.push(modifiedRows[i]);
      }

      this.connectImporterService
        .saveCustomerInvoiceRowIODTO(customerInvoiceRowDTO)
        .pipe(
          tap((result: any) => {
            if (result.success) {
              this.reloadData();
            } else {
              this.messageboxService.error(
                this.terms['core.error'],
                this.terms['common.connect.savenotsuccess']
              );
            }
          })
        );
    } else if (this.importHeadType == TermGroup_IOImportHeadType.Supplier) {
      //Suppliers
      const supplierDTO: SupplierIODTO[] = [];

      for (let i = 0; i < modifiedRows.length; i++) {
        supplierDTO.push(modifiedRows[i]);
      }

      this.connectImporterService.saveSupplierIODTO(supplierDTO).pipe(
        tap((result: any) => {
          if (result.success) {
            this.reloadData();
          } else {
            this.messageboxService.error(
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
      const supplierInvoiceDTO: SupplierInvoiceHeadIODTO[] = [];

      for (let i = 0; i < modifiedRows.length; i++) {
        supplierInvoiceDTO.push(modifiedRows[i]);
      }

      this.connectImporterService
        .saveSupplierInvoiceHeadIODTO(supplierInvoiceDTO)
        .pipe(
          tap((result: any) => {
            if (result.success) {
              this.reloadData();
            } else {
              this.messageboxService.error(
                this.terms['core.error'],
                this.terms['common.connect.savenotsuccess']
              );
            }
          })
        );
    } else if (this.importHeadType == TermGroup_IOImportHeadType.Voucher) {
      //Vouchers
      const voucherDTO: VoucherHeadIODTO[] = [];

      for (let i = 0; i < modifiedRows.length; i++) {
        voucherDTO.push(modifiedRows[i]);
      }

      this.connectImporterService.saveVoucherHeadIODTO(voucherDTO).pipe(
        tap((result: any) => {
          if (result.success) {
            this.reloadData();
          } else {
            this.messageboxService.error(
              this.terms['core.error'],
              this.terms['common.connect.savenotsuccess']
            );
          }
        })
      );
    } else if (this.importHeadType == TermGroup_IOImportHeadType.Project) {
      //Projects
      const projectDTO: ProjectIODTO[] = [];

      for (let i = 0; i < modifiedRows.length; i++) {
        projectDTO.push(modifiedRows[i]);
      }

      this.connectImporterService.saveProjectIODTO(projectDTO).pipe(
        tap((result: any) => {
          if (result.success) {
            this.reloadData();
          } else {
            this.messageboxService.error(
              this.terms['core.error'],
              this.terms['common.connect.savenotsuccess']
            );
          }
        })
      );
    }
  }

  public importSelectedRows() {
    const modifiedRows = this.rows.getValue().filter(r => r.isModified);

    if (modifiedRows.length > 0) {
      this.messageboxService.warning(
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

      this.connectImporterService
        .importIO(
          new ImportIOModel(
            this.importHeadType,
            ioIds,
            this.useAccountDistribution,
            this.useAccountDimensions,
            this.defaultDim2Account,
            this.defaultDim3Account,
            this.defaultDim4Account,
            this.defaultDim5Account,
            this.defaultDim6Account
          )
        )
        .pipe(
          tap((result: any) => {
            if (result.success) {
              this.messageboxService.success(
                this.terms['core.info'],
                this.terms['common.connect.importsuccess']
              );
              this.reloadData();
            } else {
              this.messageboxService.error(
                this.terms['core.error'],
                this.terms['common.connect.importnotsuccess'] +
                  '\n' +
                  result.errorMessage
              );
            }
          })
        )
        .subscribe();
    }
  }

  //#endregion

  //#region Overridings

  override onGridReadyToDefine(grid: GridComponent<ImportIODTO>) {
    super.onGridReadyToDefine(grid);
    this.connectImporterService
      .getImportGridColumns(this.importHeadType)
      .pipe(
        tap(data => {
          this.gridColumns = data;
          this.setupGridColumns(grid);
        })
      )
      .subscribe();
  }

  override loadTerms(): Observable<TermCollection> {
    return super.loadTerms([
      'core.aggrid.totals.filtered',
      'core.aggrid.totals.total',
      'core.aggrid.totals.selected',
      'core.error',
      'core.warning',
      'core.info',
      'common.connect.accountdimwarning',
      'common.obs',
      'common.connect.unsavedchangesmessage',
      'common.connect.importdata',
      'common.connect.importsuccess',
      'common.connect.importnotsuccess',
      'common.connect.savenotsuccess',
      'common.connect.standardreportmissing',
    ]);
  }

  //#endregion

  //#region Data Loding Functions

  loadStatus(): Observable<ISmallGenericType[]> {
    return this.coreService
      .getTermGroupContent(TermGroup.IOStatus, false, false)
      .pipe(
        tap(data => {
          this.statusList = data;
        })
      );
  }

  private loadImportIOResult() {
    // Load data
    this.connectImporterService
      .getImportIOResult(this.importHeadType, this.batchId)
      .pipe(
        tap(x => {
          for (let i = 0; i < x.length; i++) {
            const status = this.statusList.find(s => s.id == x[i].status);
            if (status) x[i].statusName = status.name;
          }
          this.rows.next(x);
        })
      )
      .subscribe(s => {
        let rowLength = this.rows.getValue().length;
        const rows = this.rows
          .getValue()
          .filter(r => r.status !== TermGroup_IOStatus.Processed);

        this.alwaysPrintAll.set(rowLength === 1 || rows.length === 0);
        if (rowLength < 10) rowLength = 10;
        if (rowLength > 30) rowLength = 30;
        this.grid.setNbrOfRowsToShow(rowLength);

        this.grid.refreshCells();
      });
  }

  //#endregion
}
