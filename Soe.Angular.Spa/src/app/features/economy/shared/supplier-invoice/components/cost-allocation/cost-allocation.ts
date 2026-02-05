import {
  Component,
  computed,
  inject,
  Injector,
  input,
  OnInit,
  signal,
  WritableSignal,
} from '@angular/core';
import { SupplierInvoiceForm } from '../../models/supplier-invoice-form.model';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { SupplierInvoiceService } from '../../services/supplier-invoice.service';
import { of, tap } from 'rxjs';
import { SupplierInvoiceCostAllocationDTO } from '../../models/supplier-invoice.model';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { TranslateService } from '@ngx-translate/core';

import { CostAllocationDialog } from './cost-allocation-dialog/cost-allocation-dialog';
import {
  CostAllocationDialogData,
  CostAllocationDialogDataResult,
  CostAllocationMode,
} from './models/cost-allocation-form.model';
import { CostAllocationLoaderService } from '../../services/cost-allocation-loader.service';
import { Perform } from '@shared/util/perform.class';
import { ProgressService } from '@shared/services/progress';
import { SupplierInvoiceSettingsService } from '../../services/supplier-invoice-settings.service';
import { SupplierInvoiceProjectOrderLoaderService } from '../../services/supplier-invoice-projectorder-loader.service';
import { IProjectTinyDTO } from '@shared/models/generated-interfaces/ProjectDTOs';
import { ICustomerInvoiceSmallGridDTO } from '@shared/models/generated-interfaces/CustomerInvoiceDTOs';

@Component({
  selector: 'soe-cost-allocation',
  templateUrl: './cost-allocation.html',
  styleUrls: ['./cost-allocation.scss'],
  standalone: false,
  providers: [FlowHandlerService],
})
export class CostAllocationComponent implements OnInit {
  form = input.required<SupplierInvoiceForm>();
  isCostAllocationRowsDataLoaded = input.required<boolean>();
  private readonly service = inject(SupplierInvoiceService);
  protected readonly loaderService = inject(CostAllocationLoaderService);
  protected readonly projectOrderLoaderService = inject(
    SupplierInvoiceProjectOrderLoaderService
  );
  private readonly progressService = inject(ProgressService);
  private readonly performLoadData = new Perform<any>(this.progressService);
  private readonly settingService = inject(SupplierInvoiceSettingsService);
  private readonly flowHandler = inject(FlowHandlerService);
  private readonly dialogService = inject(DialogService);
  private readonly translate = inject(TranslateService);

  supplierInvoiceId: WritableSignal<number> = signal(0);
  canShowConnectToProjectGrid: WritableSignal<boolean> = signal(false);
  canShowReBilledGrid: WritableSignal<boolean> = signal(false);

  chargedToProjectRow: WritableSignal<SupplierInvoiceCostAllocationDTO[]> =
    signal([]);
  reBilledRow: WritableSignal<SupplierInvoiceCostAllocationDTO[]> = signal([]);
  invoiceTotalAmount: WritableSignal<number> = signal(0);

  chargedToProjectTotalAmount = computed(() => {
    return this.chargedToProjectRow().reduce(
      (sum, r) =>
        r.chargeCostToProject ? sum + (r.projectAmountCurrency ?? 0) : sum,
      0
    );
  });

  reBilledTotalAmount = computed(() => {
    return this.reBilledRow().reduce(
      (sum, r) => sum + (r.orderAmountCurrency ?? 0),
      0
    );
  });

  leftToAllocationAmount = computed(() => {
    return (
      this.invoiceTotalAmount() -
      (this.chargedToProjectTotalAmount() + this.reBilledTotalAmount())
    );
  });

  totalAccolationAmount = computed(() => {
    return this.reBilledTotalAmount() + this.chargedToProjectTotalAmount();
  });

  protected readonly injector = Injector.create({
    providers: [{ provide: ToolbarService, useClass: ToolbarService }],
  });
  protected readonly _toolbarService = this.injector.get(ToolbarService);

  constructor() {
    this.flowHandler.execute({
      permission: Feature.Economy_Supplier_Suppliers_Edit,
      lookups: [
        this.loaderService.load(),
        this.projectOrderLoaderService.load(),
      ],
      onFinished: () =>
        this.loadSupplierInvoiceCostAllocationRows().subscribe(() => {
          this.updateProjectOrderInCostAllocationRows();
        }),
    });
  }
  ngOnInit(): void {
    this.createPageToolbar();
    this.setValues();
  }

  private setValues() {
    this.invoiceTotalAmount.set(this.form()?.totalAmount?.value ?? 0);
    this.supplierInvoiceId.set(this.form()?.invoiceId?.value ?? 0);
  }
  private createPageToolbar(): void {
    this._toolbarService.createItemGroup({
      items: [
        this._toolbarService.createToolbarButton('rebilled', {
          caption: signal('economy.supplier.invoice.rebilled'),
          tooltip: signal('economy.supplier.invoice.rebilled'),
          iconName: signal('plus'),
          onAction: () => {
            this.openRebilledDialog();
          },
        }),
      ],
    });
    this._toolbarService.createItemGroup({
      items: [
        this._toolbarService.createToolbarButton('chargedToProject', {
          caption: signal('economy.supplier.invoice.linktoproject'),
          tooltip: signal('economy.supplier.invoice.linktoproject'),
          iconName: signal('plus'),
          onAction: () => {
            //charged to project
            this.openChargedToProjectDialog();
          },
        }),
      ],
    });
  }

  onFinish() {
    this.loadSupplierInvoiceCostAllocationRows();
  }

  //#region Helper Methods

  public updateGridRowsFromCostAllocationRows() {
    if (this.isCostAllocationRowsDataLoaded()) {
      this.canShowConnectToProjectGrid.set(true);
      const rows: SupplierInvoiceCostAllocationDTO[] =
        this.form()?.supplierInvoiceCostAllocationRows?.value || [];
      if (rows.length == 0) return;
      this.chargedToProjectRow.set(rows.filter(r => r.isConnectToProjectRow));
      this.reBilledRow.set(rows.filter(r => !r.isConnectToProjectRow));
    }
  }

  updateProjectOrderInCostAllocationRows() {
    const orderId: number = this.form()?.orderCustomerInvoiceId?.value ?? 0;
    const projectId: number = this.form()?.projectId?.value ?? 0;
    const order = this.projectOrderLoaderService.getOrder(orderId);
    const project = this.projectOrderLoaderService.getProject(projectId);
    this.addChargedToProjectIfRowsEmpty(order, project);
    this.updateChargedToProjectRows(order, project);
    this.updateReBilledRows(order, project);
    this.setFormDirty();
    this.patchSupplierInvoiceCostAllocationRows();
  }

  updateReBilledRows(
    order: ICustomerInvoiceSmallGridDTO | undefined,
    project: IProjectTinyDTO | undefined
  ) {
    this.reBilledRow.update(rows =>
      rows.map((r: SupplierInvoiceCostAllocationDTO) => {
        let row = {
          ...r,
        } as SupplierInvoiceCostAllocationDTO;
        if (order) {
          row.orderId = order.invoiceId;
          row.customerInvoiceNumberName =
            order.customerInvoiceNumberNameWithoutDescription;
          row.orderNr = order.invoiceNr;
        }
        if (project) {
          row.projectId = project.projectId;
          row.projectName = project.name;
          row.projectNr = project.number;
          row.projectNrName = `${project.number} ${project.name}`;
        }
        return row;
      })
    );
  }

  updateChargedToProjectRows(
    order: ICustomerInvoiceSmallGridDTO | undefined,
    project: IProjectTinyDTO | undefined
  ) {
    this.chargedToProjectRow.update(rows =>
      rows.map((r: SupplierInvoiceCostAllocationDTO) => {
        let row = {
          ...r,
        } as SupplierInvoiceCostAllocationDTO;
        if (order) {
          row.orderId = order.invoiceId;
          row.customerInvoiceNumberName =
            order.customerInvoiceNumberNameWithoutDescription;
          row.orderNr = order.invoiceNr;
        }
        if (project) {
          row.projectId = project.projectId;
          row.projectName = project.name;
          row.projectNr = project.number;
          row.projectNrName = `${project.number} ${project.name}`;
        }
        return row;
      })
    );
  }

  addChargedToProjectIfRowsEmpty(
    order: ICustomerInvoiceSmallGridDTO | undefined,
    project: IProjectTinyDTO | undefined
  ) {
    if (
      this.form()?.supplierInvoiceCostAllocationRows?.value.filter(
        f => f.isConnectToProjectRow
      ).length == 0
    ) {
      const row = new SupplierInvoiceCostAllocationDTO();
      row.isConnectToProjectRow = true;
      row.isTransferToOrderRow = false;
      row.includeSupplierInvoiceImage = true;
      row.timeCodeTransactionId = -(
        (this.form()?.supplierInvoiceCostAllocationRows?.value.length ?? 0) + 1
      );
      this.projectOrderLoaderService.setOrderDetails(row, order);
      this.projectOrderLoaderService.setProjectDetails(row, project);
      row.projectAmountCurrency = this.form().totalAmount?.value ?? 0.0;
      this.loaderService.setTimeCodeDetails(row);
      this.chargedToProjectRow.set([row]);
    }
  }

  patchSupplierInvoiceCostAllocationRows() {
    //patch rows to form control
    const allRows: SupplierInvoiceCostAllocationDTO[] = [
      ...this.chargedToProjectRow(),
      ...this.reBilledRow(),
    ];
    this.form()?.patchSupplierInvoiceCostAllocationRows(allRows);
  }

  openChargedToProjectDialog() {
    this.canShowConnectToProjectGrid.set(true);
    const row = new SupplierInvoiceCostAllocationDTO();
    row.isConnectToProjectRow = true;
    row.isTransferToOrderRow = false;
    row.includeSupplierInvoiceImage = true;
    row.projectId = this.form()?.projectId?.value ?? 0;
    row.orderId = this.form()?.orderCustomerInvoiceId?.value ?? 0;
    row.timeCodeTransactionId = -(
      (this.form()?.supplierInvoiceCostAllocationRows?.value.length ?? 0) + 1
    );
    if (row.projectId) {
      this.projectOrderLoaderService.setProjectDetails(
        row,
        this.projectOrderLoaderService.getProject(row.projectId)
      );
    }
    if (row.orderId) {
      this.projectOrderLoaderService.setOrderDetails(
        row,
        this.projectOrderLoaderService.getOrder(row.orderId)
      );
    }
    row.timeCodeId = this.settingService.projectDefaultTimeCodeId ?? 0;
    this.loaderService.setTimeCodeDetails(row);
    const chargedToProjectComponent = this.dialogService.open(
      CostAllocationDialog,
      {
        title: this.translate.instant(
          'economy.supplier.invoice.chargedtoproject'
        ),
        size: 'md',
        rowItem: row,
        timeCodes: this.loaderService.timeCodes(),
        employees: this.loaderService.employees(),
        projects: this.projectOrderLoaderService.projectTinyDtos(),
        customerInvoices: this.projectOrderLoaderService.customerInvoices(),
        isNew: true,
        projectAmountCurrency: 0.0,
        rowAmountCurrency: 0.0,
        orderAmountCurrency: 0.0,
        invoiceTotalAmount: this.invoiceTotalAmount(),
        totalAccolationAmount: this.totalAccolationAmount(),
        costAllocationMode: CostAllocationMode.ChargedToProject,
        supplierInvoiceId: this.supplierInvoiceId(),
      } as CostAllocationDialogData
    );

    chargedToProjectComponent
      .afterClosed()
      .subscribe((result: CostAllocationDialogDataResult) => {
        if (result.result) {
          //update row logic
          let chargedToProjectArray: SupplierInvoiceCostAllocationDTO[] = [];
          if (result.rowItem) {
            result.rowItem.projectNrName = `${result.rowItem.projectNr} ${result.rowItem.projectName}`;
            result.rowItem.employeeNrName = `${result.rowItem.employeeNr} ${result.rowItem.employeeName}`;

            if (result.isNew) {
              chargedToProjectArray = [
                ...this.chargedToProjectRow(),
                result.rowItem,
              ];
            }
            this.chargedToProjectRow.set(chargedToProjectArray);
            this.onOrderProjectChanged(result.rowItem);
          }
        }
      });
  }

  openRebilledDialog() {
    this.canShowReBilledGrid.set(true);
    const row = new SupplierInvoiceCostAllocationDTO();
    row.isConnectToProjectRow = false;
    row.isTransferToOrderRow = true;
    row.includeSupplierInvoiceImage = true;
    row.projectId = this.form()?.projectId?.value ?? 0;
    row.orderId = this.form()?.orderCustomerInvoiceId?.value ?? 0;
    row.timeCodeTransactionId = -(
      (this.form()?.supplierInvoiceCostAllocationRows?.value.length ?? 0) + 1
    );
    if (row.projectId) {
      this.projectOrderLoaderService.setProjectDetails(
        row,
        this.projectOrderLoaderService.getProject(row.projectId)
      );
    }
    if (row.orderId) {
      this.projectOrderLoaderService.setOrderDetails(
        row,
        this.projectOrderLoaderService.getOrder(row.orderId)
      );
    }

    const reBilledComponent = this.dialogService.open(CostAllocationDialog, {
      title: this.translate.instant('economy.supplier.invoice.rebilled'),
      size: 'md',
      rowItem: row,
      timeCodes: this.loaderService.timeCodes(),
      employees: this.loaderService.employees(),
      projects: this.projectOrderLoaderService.projectTinyDtos(),
      customerInvoices: this.projectOrderLoaderService.customerInvoices(),
      isNew: true,
      rowAmountCurrency: 0.0,
      orderAmountCurrency: 0.0,
      invoiceTotalAmount: this.invoiceTotalAmount(),
      totalAccolationAmount: this.totalAccolationAmount(),
      products: this.loaderService.products(),
      costAllocationMode: CostAllocationMode.ReBilled,
      projectAmountCurrency: 0,
      supplierInvoiceId: this.supplierInvoiceId(),
    } as CostAllocationDialogData);

    reBilledComponent
      .afterClosed()
      .subscribe((result: CostAllocationDialogDataResult) => {
        if (result.result) {
          //update row logic
          let reBilledArray: SupplierInvoiceCostAllocationDTO[] = [];
          if (result.rowItem) {
            result.rowItem.projectNrName = `${result.rowItem.projectNr} ${result.rowItem.projectName}`;
            result.rowItem.employeeNrName = `${result.rowItem.employeeNr} ${result.rowItem.employeeName}`;

            if (result.isNew) {
              reBilledArray = [...this.reBilledRow(), result.rowItem];
            } else {
              reBilledArray = this.reBilledRow().map(r => {
                if (
                  r.timeCodeTransactionId ===
                  result.rowItem?.timeCodeTransactionId
                ) {
                  r = result.rowItem;
                  r.projectNrName = `${result.rowItem.projectNr} ${result.rowItem.projectName}`;
                  r.employeeNrName = `${result.rowItem.employeeNr} ${result.rowItem.employeeName}`;
                }
                return r;
              });
            }
            this.reBilledRow.set(reBilledArray);
            this.onOrderProjectChanged(result.rowItem);
          }
        }
      });
  }

  private setFormDirty() {
    this.form()?.markAsDirty();
    this.form()?.markAsTouched();
    this.form()?.updateValueAndValidity();
  }

  private setGridData(rows: SupplierInvoiceCostAllocationDTO[]) {
    const chargedToProjectRows: SupplierInvoiceCostAllocationDTO[] =
      rows.filter(r => r.isConnectToProjectRow) || [];
    this.chargedToProjectRow.set(chargedToProjectRows);

    const reBilledRows: SupplierInvoiceCostAllocationDTO[] =
      rows.filter(r => !r.isConnectToProjectRow) || [];
    this.reBilledRow.set(reBilledRows);
    this.canShowConnectToProjectGrid.set(true);
  }

  //#endregion

  //#region UI Events

  onReBilledChanged(rows: SupplierInvoiceCostAllocationDTO[]) {
    this.reBilledRow.set(rows);
    this.form()?.setFormDirty();
    this.patchSupplierInvoiceCostAllocationRows();
  }

  onChargedToProjectChanged(rows: SupplierInvoiceCostAllocationDTO[]) {
    this.chargedToProjectRow.set(rows);
    this.form()?.setFormDirty();
    this.patchSupplierInvoiceCostAllocationRows();
  }

  onOrderProjectChanged(row: SupplierInvoiceCostAllocationDTO) {
    const orderId: number = row?.orderId;
    const projectId: number = row?.projectId;
    const order = this.projectOrderLoaderService.getOrder(orderId);
    const project = this.projectOrderLoaderService.getProject(projectId);
    this.updateChargedToProjectRows(order, project);
    this.updateReBilledRows(order, project);
    this.patchSupplierInvoiceCostAllocationRows();
    this.form()?.patchValueOrderProject(order, project);
    this.form()?.setFormDirty();
  }

  //#endregion

  //#region Data loading methods

  loadSupplierInvoiceCostAllocationRows() {
    if (this.isCostAllocationRowsDataLoaded()) {
      this.setGridData(
        this.form()?.supplierInvoiceCostAllocationRows?.value || []
      );
      return of();
    }
    if (!this.form()?.invoiceId?.value) return of();
    return this.performLoadData.load$(
      this.service
        .getSupplierInvoiceCostAllocationRows(this.form()?.invoiceId.value)
        .pipe(
          tap(rows => {
            this.setGridData(rows);
            this.patchSupplierInvoiceCostAllocationRows();
          })
        )
    );
  }

  //#endregion
}
