import { Component, Input, OnInit, inject } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import {
  Feature,
  OrderInvoiceRegistrationType,
  SoeEntityState,
  SoeOriginType,
  SoeReportTemplateType,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  IAccountDistributionTraceViewDTO,
  IContractTraceViewDTO,
  IInvoiceTraceViewDTO,
  IOfferTraceViewDTO,
  IPaymentTraceViewDTO,
  IProjectTraceViewDTO,
  IPurchaseTraceViewDTO,
  IVoucherTraceViewDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { BrowserUtil } from '@shared/util/browser-util';
import { Perform } from '@shared/util/perform.class';
import { PurchaseEditComponent } from '@src/app/features/billing/purchase/components/purchase-edit/purchase-edit.component';
import { PurchaseForm } from '@src/app/features/billing/purchase/models/purchase-form.model';
import { GridComponent } from '@ui/grid/grid.component';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { take, tap } from 'rxjs';
import {
  InvoiceTraceViewDTO,
  OrderTraceViewDTO,
  TraceRowPageName,
} from '../models/trace-rows.model';

@Component({
  selector: 'soe-trace-rows',
  templateUrl: './trace-rows.component.html',
  styleUrls: ['./trace-rows.component.scss'],
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class TraceRowsComponent
  extends GridBaseDirective<any>
  implements OnInit
{
  @Input() traceId!: number; // Common id eg. invoiceId, voucherheadId
  @Input() pageName!: TraceRowPageName;

  editSupplierInvoicePermission = false;
  editCustomerInvoicePermission = false;
  editBillingInvoicePermission = false;
  editCustomerPaymentPermission = false;
  editSupplierPaymentPermission = false;
  editInventoryPermission = false;
  editVoucherPermission = false;
  editProjectPermission = false;
  editOrderPermission = false;
  editAccountDistributionHeadPermission = false;
  editOfferPermission = false;
  editContractsPermission = false;
  editPurchasePermission = false;
  editPurchaseDeliveryPermission = false;

  terms: any;
  text = '';

  messageboxService = inject(MessageboxService);
  coreService = inject(CoreService);
  progressService = inject(ProgressService);

  performVoucherTraceViewsLoad = new Perform<IVoucherTraceViewDTO[]>(
    this.progressService
  );
  performInvoiceTraceViewsLoad = new Perform<InvoiceTraceViewDTO[]>(
    this.progressService
  );
  performOrderTraceViewsLoad = new Perform<OrderTraceViewDTO[]>(
    this.progressService
  );
  performPaymentTraceViewsLoad = new Perform<IPaymentTraceViewDTO[]>(
    this.progressService
  );
  performProjectTraceViewsLoad = new Perform<IProjectTraceViewDTO[]>(
    this.progressService
  );
  performAccountDistributionTraceViewsLoad = new Perform<
    IAccountDistributionTraceViewDTO[]
  >(this.progressService);
  performOfferTraceViewsLoad = new Perform<IOfferTraceViewDTO[]>(
    this.progressService
  );
  performContractTraceViewsLoad = new Perform<IContractTraceViewDTO[]>(
    this.progressService
  );
  performPurchaseTraceViewsLoad = new Perform<IPurchaseTraceViewDTO[]>(
    this.progressService
  );

  ngOnInit() {
    super.ngOnInit();
    this.startFlow(Feature.None, 'TraceGrid', {
      additionalReadPermissions: [
        Feature.Economy_Supplier_Invoice_Invoices_Edit,
        Feature.Billing_Invoice_Invoices_Edit,
        Feature.Economy_Customer_Invoice_Invoices_Edit,
        Feature.Economy_Customer_Payment_Payments_Edit,
        Feature.Economy_Supplier_Payment_Payments_Edit,
        Feature.Economy_Accounting_Vouchers_Edit,
        Feature.Billing_Project_Edit,
        Feature.Billing_Order_Orders_Edit,
        Feature.Economy_Preferences_VoucherSettings_AccountDistributionPeriod,
        Feature.Billing_Offer_Offers_Edit,
        Feature.Billing_Contract_Contracts_Edit,
        Feature.Billing_Purchase_Purchase_Edit,
        Feature.Billing_Purchase_Delivery_Edit,
        Feature.Economy_Inventory_Inventories_Edit,
      ],
      skipInitialLoad: true,
    });
  }

  override onPermissionsLoaded() {
    super.onPermissionsLoaded();
    this.editSupplierInvoicePermission = this.flowHandler.hasReadAccess(
      Feature.Economy_Supplier_Invoice_Invoices_Edit
    );
    this.editCustomerInvoicePermission = this.flowHandler.hasReadAccess(
      Feature.Economy_Customer_Invoice_Invoices_Edit
    );
    this.editBillingInvoicePermission = this.flowHandler.hasReadAccess(
      Feature.Billing_Invoice_Invoices_Edit
    );
    this.editCustomerPaymentPermission = this.flowHandler.hasReadAccess(
      Feature.Economy_Customer_Payment_Payments_Edit
    );
    this.editSupplierPaymentPermission = this.flowHandler.hasReadAccess(
      Feature.Economy_Supplier_Payment_Payments_Edit
    );
    this.editInventoryPermission = this.flowHandler.hasReadAccess(
      Feature.Economy_Inventory_Inventories_Edit
    );
    this.editVoucherPermission = this.flowHandler.hasReadAccess(
      Feature.Economy_Accounting_Vouchers_Edit
    );
    this.editProjectPermission = this.flowHandler.hasReadAccess(
      Feature.Billing_Project_Edit
    );
    this.editOrderPermission = this.flowHandler.hasReadAccess(
      Feature.Billing_Order_Orders_Edit
    );
    this.editAccountDistributionHeadPermission = this.flowHandler.hasReadAccess(
      Feature.Economy_Preferences_VoucherSettings_AccountDistributionPeriod
    );
    this.editOfferPermission = this.flowHandler.hasReadAccess(
      Feature.Billing_Offer_Offers_Edit
    );
    this.editContractsPermission = this.flowHandler.hasReadAccess(
      Feature.Billing_Contract_Contracts_Edit
    );
    this.editPurchasePermission = this.flowHandler.hasReadAccess(
      Feature.Billing_Purchase_Purchase_Edit
    );
    this.editPurchaseDeliveryPermission = this.flowHandler.hasReadAccess(
      Feature.Billing_Purchase_Delivery_Edit
    );
  }

  override onGridReadyToDefine(grid: GridComponent<any>) {
    super.onGridReadyToDefine(grid);

    const keys: string[] = [
      'core.deleted',
      'core.warning',
      'common.type',
      'common.tracerows.status',
      'common.tracerows.invoicetype',
      'common.description',
      'common.number',
      'common.date',
      'common.tracerows.invoiceshow',
      'common.tracerows.invoiceshort',
      'common.tracerows.paymentshow',
      'common.tracerows.paymentshort',
      'common.tracerows.inventoryshow',
      'common.tracerows.inventoryshort',
      'economy.supplier.invoice.invoice',
      'economy.accounting.voucher.voucher',
      'economy.supplier.payment.payment',
      'common.showpdf',
      'common.pdferror',
      'economy.accounting.voucher.voucher',
      'economy.accounting.accountdistribution.accountdistribution',
    ];

    this.translate
      .get(keys)
      .pipe(take(1))
      .subscribe(terms => {
        this.terms = terms;
        this.text = '* = ' + terms['core.deleted'];
        this.grid.context.suppressGridMenu = true;
        this.grid.addColumnText('originTypeName', terms['common.type'], {
          enableHiding: true,
          flex: 1,
        });
        this.grid.addColumnText(
          'originStatusName',
          terms['common.tracerows.status'],
          { enableHiding: true, flex: 1 }
        );
        this.grid.addColumnText('description', terms['common.description'], {
          enableHiding: true,
          flex: 1,
        });
        this.grid.addColumnText('number', terms['common.number'], {
          enableHiding: true,
          flex: 1,
        });
        this.grid.addColumnDate('date', terms['common.date'], {
          enableHiding: true,
          flex: 1,
        });
        this.grid.addColumnIcon('', '', {
          iconName: 'pencil',
          iconClass: 'pencil',
          tooltip: terms['common.tracerows.openEdit'],
          onClick: row => {
            this.openEdit(row);
          },
          showIcon: row => this.showEdit(row),
          suppressFilter: true,
        });
        this.grid.addColumnIcon('', '', {
          iconName: 'file-pdf',
          iconClass: 'file-pdf',
          tooltip: terms['common.tracerows.openEdit'],
          onClick: row => {
            this.showPdf(row);
          },
          showIcon: row => row.showPdfIcon,
          suppressFilter: true,
        });
        this.grid.onRowDoubleClicked = this.openEditDoubleClick.bind(this);
        super.finalizeInitGrid();

        this.loadGridData();
      });
  }

  setRowAsDeleted(row: IInvoiceTraceViewDTO) {
    if (row.originTypeName.indexOf('*') === -1) {
      row.originTypeName += ' *';
    }
  }

  showPdf(row: any) {
    if (row.ediHasPdf) {
      const ediPdfReportUrl =
        '/ajax/downloadReport.aspx?templatetype=' +
        SoeReportTemplateType.SymbrioEdiSupplierInvoice +
        '&edientryid=' +
        row.ediEntryId;
      window.open(ediPdfReportUrl, '_blank');
    } else {
      this.coreService.generateReportForEdi([row.ediEntryId]).pipe(
        tap(result => {
          if (result.success) {
            const ediPdfReportUrl: string =
              '/ajax/downloadReport.aspx?templatetype=' +
              SoeReportTemplateType.SymbrioEdiSupplierInvoice +
              '&edientryid=' +
              row.ediEntryId;
            window.open(ediPdfReportUrl, '_blank');
          } else {
            this.messageboxService.error('core.delete', 'core.deletewarning');
          }
        })
      );
    }
  }

  showInventory(row: any) {
    if (row.state === SoeEntityState.Deleted) {
      this.setRowAsDeleted(row);
    } else if (row.state === SoeEntityState.Active && row.isInventory) {
      if (this.editInventoryPermission) return true;
    }
    return false;
  }

  openEdit(row: any) {
    if (row.isVoucher || row.isStockVoucher) {
      //this.messagingService.publish(Constants.EVENT_OPEN_TAB, new TabMessage(this.terms["economy.accounting.voucher.voucher"] + " " + row.number, row.voucherHeadId, VouchersEditController, { id: row.voucherHeadId }, this.urlHelperService.getGlobalUrl('Economy/Accounting/Vouchers/Views/edit.html')));
    } else if (row.isSupplierInvoice) {
      //this.messagingService.publish(Constants.EVENT_OPEN_TAB, new TabMessage(row.originTypeName + " " + row.number, row.supplierInvoiceId, SupplierInvoicesEditController, { id: row.supplierInvoiceId }, this.urlHelperService.getGlobalUrl('Shared/Economy/Supplier/Invoices/Views/edit.html')));
    } else if (
      row.isInvoice &&
      row.originType === SoeOriginType.SupplierInvoice
    ) {
      //this.messagingService.publish(Constants.EVENT_OPEN_TAB, new TabMessage(row.originTypeName + " " + row.number, row.mappedInvoiceId ? row.mappedInvoiceId : row.invoiceId, SupplierInvoicesEditController, { id: row.mappedInvoiceId ? row.mappedInvoiceId : row.invoiceId }, this.urlHelperService.getGlobalUrl('Shared/Economy/Supplier/Invoices/Views/edit.html')));
    } else if (
      row.isCustomerInvoice ||
      (row.isInvoice && row.originType === SoeOriginType.CustomerInvoice)
    ) {
      if (row.registrationType === OrderInvoiceRegistrationType.Ledger) {
        //this.messagingService.publish(Constants.EVENT_OPEN_TAB, new TabMessage(row.originTypeName + " " + row.number, row.isCustomerInvoice ? row.customerInvoicId : row.invoiceId, CustomerInvoicesEditController, { id: row.invoiceId }, this.urlHelperService.getGlobalUrl('Common/Customer/Invoices/Views/edit.html')));
      } else {
        const invoiceId =
          row.mappedInvoiceId ?? row.invoiceId ?? row.customerInvoiceId;
        // var message = new TabMessage(
        //     row.originTypeName + " " + row.number,
        //     invoiceId,
        //     BillingInvoicesEditController,
        //     { id: invoiceId },
        //     this.urlHelperService.getGlobalUrl('Shared/Billing/Invoices/Views/edit.html')
        // );
        // this.messagingService.publish(Constants.EVENT_OPEN_TAB, message);
      }
    } else if (
      row.isPayment &&
      row.originType === SoeOriginType.SupplierPayment
    ) {
      //this.messagingService.publish(Constants.EVENT_OPEN_TAB, new TabMessage(row.originTypeName + " " + row.number, row.paymentRowId, SupplierPaymentsEditController, { paymentId: row.paymentRowId }, this.urlHelperService.getGlobalUrl('Shared/Economy/Supplier/Payments/Views/edit.html')));
    } else if (
      row.isPayment &&
      row.originType === SoeOriginType.CustomerPayment
    ) {
      //this.messagingService.publish(Constants.EVENT_OPEN_TAB, new TabMessage(row.originTypeName + " " + row.number, row.paymentRowId, CustomerPaymentsEditController, { paymentId: row.paymentRowId }, this.urlHelperService.getGlobalUrl('Common/Customer/Payments/Views/edit.html')));
    } else if (row.isProject) {
      //this.messagingService.publish(Constants.EVENT_OPEN_TAB, new TabMessage(row.originTypeName + " " + row.number, row.projectId, BillingProjectsEditController, { id: row.projectId }, this.urlHelperService.getGlobalUrl('Billing/Projects/Views/edit.html')));
    } else if (row.isOrder) {
      BrowserUtil.openInNewWindow(
        window,
        `/soe/billing/order/status/?invoiceId=${row.orderId}&invoiceNr=${row.number}`
      );
      //this.messagingService.publish(Constants.EVENT_OPEN_TAB, new TabMessage(row.originTypeName + " " + row.number, row.orderId, BillingOrdersEditController, { id: row.orderId, originType: SoeOriginType.Order, feature: Feature.Billing_Order_Status }, this.urlHelperService.getGlobalUrl('Shared/Billing/Orders/Views/edit.html')));
    } else if (row.isContract) {
      //this.messagingService.publish(Constants.EVENT_OPEN_TAB, new TabMessage(row.originTypeName + " " + row.number, row.contractId, BillingOrdersEditController, { id: row.contractId, originType: SoeOriginType.Contract, feature: Feature.Billing_Contract_Status }, this.urlHelperService.getGlobalUrl('Shared/Billing/Orders/Views/editContract.html')));
    } else if (row.isOffer) {
      //this.messagingService.publish(Constants.EVENT_OPEN_TAB, new TabMessage(row.originTypeName + " " + row.number, row.offerId, BillingOrdersEditController, { id: row.offerId, originType: SoeOriginType.Offer, feature: Feature.Billing_Offer_Status }, this.urlHelperService.getGlobalUrl('Shared/Billing/Orders/Views/editOffer.html')));
    } else if (row.isAccountDistribution) {
      //this.messagingService.publish(Constants.EVENT_OPEN_TAB, new TabMessage(row.originTypeName + " " + row.accountDistributionName, row.accountDistributionHeadId, AccountDistributionEditController, { id: row.accountDistributionHeadId, accountDistributionType: 'Period' }, this.urlHelperService.getGlobalUrl('Shared/Economy/Accounting/AccountDistribution/Views/edit.html')));
    } else if (row.isPurchase) {
      this.edit(
        {
          ...row,
          purchaseNr: row.purchaseNr,
          purchaseId: row.purchaseId,
        },
        {
          filteredRows: [],
          editComponent: PurchaseEditComponent,
          editTabLabel: 'billing.purchase.list.purchase',
          FormClass: PurchaseForm,
        }
      );
      //this.messagingService.publish(Constants.EVENT_OPEN_TAB, new TabMessage(row.originTypeName + " " + row.number, row.purchaseId, BillingPurchaseEditController, { id: row.purchaseId }, this.urlHelperService.getGlobalUrl('Billing/Purchase/Purchase/Views/edit.html')));
    } else if (row.isDelivery) {
      //this.messagingService.publish(Constants.EVENT_OPEN_TAB, new TabMessage(row.originTypeName + " " + row.number, row.purchaseDeliveryId, BillingDeliveryEditController, { id: row.purchaseDeliveryId }, this.urlHelperService.getGlobalUrl('Billing/Purchase/Delivery/Views/edit.html')));
    } else if (row.isInventory) {
      //this.messagingService.publish(Constants.EVENT_OPEN_TAB, new TabMessage(row.originTypeName + " " + row.number, row.purchaseDeliveryId, InventoryEditController, { id: row.inventoryId }, this.urlHelperService.getGlobalUrl('Shared/Economy/Inventory/Inventories/Views/edit.html')));
    }
  }

  showEdit(row: any) {
    if (row.state === SoeEntityState.Deleted) {
      this.setRowAsDeleted(row);
    } else if (
      row.state === SoeEntityState.Active &&
      (row.isCustomerInvoice ||
        (row.isInvoice && row.originType === SoeOriginType.CustomerInvoice))
    ) {
      if (
        this.editCustomerInvoicePermission ||
        this.editBillingInvoicePermission
      ) {
        return true;
      }
    } else if (
      row.state === SoeEntityState.Active &&
      (row.isSupplierInvoice ||
        (row.isInvoice && row.originType === SoeOriginType.SupplierInvoice))
    ) {
      if (this.editSupplierInvoicePermission) return true;
    } else if (row.state === SoeEntityState.Active && row.isPayment) {
      if (
        (row.originType === SoeOriginType.CustomerPayment &&
          this.editCustomerPaymentPermission) ||
        (row.originType === SoeOriginType.SupplierPayment &&
          this.editSupplierPaymentPermission)
      ) {
        return true;
      }
    } else if (row.state === SoeEntityState.Active && row.isInventory) {
      if (this.editInventoryPermission) return true;
    } else if (
      row.state === SoeEntityState.Active &&
      (row.isVoucher || row.isStockVoucher)
    ) {
      if (this.editVoucherPermission) return true;
    } else if (row.state === SoeEntityState.Active && row.isProject) {
      if (this.editProjectPermission) return true;
    } else if (row.state === SoeEntityState.Active && row.isOrder) {
      if (this.editOrderPermission) return true;
    } else if (
      row.state === SoeEntityState.Active &&
      row.isAccountDistribution
    ) {
      if (this.editAccountDistributionHeadPermission) return true;
    } else if (row.state === SoeEntityState.Active && row.isOffer) {
      if (this.editOfferPermission) return true;
    } else if (row.state === SoeEntityState.Active && row.isContract) {
      if (this.editContractsPermission) return true;
    } else if (row.state === SoeEntityState.Active && row.isPurchase) {
      if (this.editPurchasePermission) return true;
    } else if (row.state === SoeEntityState.Active && row.isDelivery) {
      if (this.editPurchaseDeliveryPermission) return true;
    }

    return false;
  }

  openEditDoubleClick(row: any) {
    if (this.showEdit(row)) this.openEdit(row);
  }

  public loadGridData() {
    if (!this.traceId) return;

    //Depending where we are, different data should be loaded
    if (this.pageName === TraceRowPageName.Voucher) {
      this.performVoucherTraceViewsLoad.load(
        this.coreService.getVoucherTaceViews(this.traceId).pipe(
          tap(data => {
            data.forEach(y => {
              //y.date = CalendarUtility.convertToDate(y.date);
              if (y.isAccountDistribution) {
                y.originTypeName =
                  this.terms[
                    'economy.accounting.accountdistribution.accountdistribution'
                  ];
              }
            });
            this.grid.setData(data);
          })
        )
      );
    } else if (
      this.pageName === TraceRowPageName.SupplierInvoice ||
      this.pageName === TraceRowPageName.CustomerInvoice
    )
      this.performInvoiceTraceViewsLoad.load(
        this.coreService.getInvoiceTraceViews(this.traceId).pipe(
          tap(data => {
            data.forEach(y => {
              if (y.isInventory) y.originStatusName = y.inventoryStatusName;
              else if (y.isPayment) y.originStatusName = y.paymentStatusName;
              else if (y.isAccountDistribution) {
                y.originTypeName =
                  this.terms[
                    'economy.accounting.accountdistribution.accountdistribution'
                  ];
              }

              if (y.ediEntryId) y.showPdfIcon = true;
              else y.showPdfIcon = false;
            });
            this.grid.setData(data);
          })
        )
      );
    else if (this.pageName === TraceRowPageName.Order) {
      this.performOrderTraceViewsLoad.load(
        this.coreService.getOrderTraceViews(this.traceId).pipe(
          tap(data => {
            data.forEach(y => {
              // TODO: Fix any cast... Missing in export interface declaration
              if ((<any>y).isInventory)
                y.originStatusName = (<any>y).inventoryStatusName;
              else if ((<any>y).isPayment)
                y.originStatusName = (<any>y).paymentStatusName;

              if (y.ediEntryId) y.showPdfIcon = true;
              else y.showPdfIcon = false;
            });
            this.grid.setData(data);
          })
        )
      );
    } else if (this.pageName === TraceRowPageName.Payment) {
      this.performPaymentTraceViewsLoad.load(
        this.coreService.getPaymentTraceViews(this.traceId).pipe(
          tap(data => {
            this.grid.setData(data);
          })
        )
      );
    } else if (this.pageName === TraceRowPageName.Project) {
      this.performProjectTraceViewsLoad.load(
        this.coreService.getProjectTraceViews(this.traceId).pipe(
          tap(data => {
            this.grid.setData(data);
          })
        )
      );
    } else if (this.pageName === TraceRowPageName.AccountDistribution) {
      this.performAccountDistributionTraceViewsLoad.load(
        this.coreService.getAccountDistributionTraceViews(this.traceId).pipe(
          tap(data => {
            data.forEach(y => {
              if (y.isVoucher)
                y.originTypeName =
                  this.terms['economy.accounting.voucher.voucher'];
            });
            this.grid.setData(data);
          })
        )
      );
    } else if (this.pageName === TraceRowPageName.Offer) {
      this.performOfferTraceViewsLoad.load$(
        this.coreService.getOfferTraceViews(this.traceId).pipe(
          tap(data => {
            data.forEach(y => {
              // TODO: Fix any cast... Missing in export interface declaration
              if ((<any>y).isInventory)
                y.originStatusName = (<any>y).inventoryStatusName;
              else if ((<any>y).isPayment)
                y.originStatusName = (<any>y).paymentStatusName;
            });
            this.grid.setData(data);
          })
        )
      );
    } else if (this.pageName === TraceRowPageName.Contract) {
      this.performContractTraceViewsLoad.load(
        this.coreService.getContractTraceViews(this.traceId).pipe(
          tap(data => {
            data.forEach(y => {
              // TODO: Fix any cast... Missing in export interface declaration
              if ((<any>y).isInventory)
                y.originStatusName = (<any>y).inventoryStatusName;
              else if ((<any>y).isPayment)
                y.originStatusName = (<any>y).paymentStatusName;
            });
            this.grid.setData(data);
          })
        )
      );
    } else if (this.pageName === TraceRowPageName.PriceOptimization) {
      this.performPurchaseTraceViewsLoad.load(
        this.coreService.getPriceOptimizationTraceViews(this.traceId).pipe(
          tap((data: any) => {
            this.grid.setData(data);
          })
        )
      );
    } else if (this.pageName === TraceRowPageName.Purchase) {
      this.performPurchaseTraceViewsLoad.load(
        this.coreService.getPurchaseTraceViews(this.traceId).pipe(
          tap(data => {
            this.grid.setData(data);
          })
        )
      );
    }
  }
}
