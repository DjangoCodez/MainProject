import { Component, inject, OnInit, signal } from '@angular/core';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import {
  CustomerCentralDTO,
  CustomerInvoiceGridDTO,
} from '../../models/customer-central.model';
import { CustomerCentralService } from '../../services/customer-central.service';
import { CustomerCentralForm } from '../../models/customer-central-form.model';
import {
  Feature,
  InvoiceRowInfoFlag,
  SoeCategoryType,
  SoeDataStorageRecordType,
  SoeEntityType,
  SoeOriginStatusClassification,
  SoeOriginStatusClassificationGroup,
  SoeOriginType,
  SoeStatusIcon,
  TermGroup,
} from '@shared/models/generated-interfaces/Enumerations';
import { BrowserUtil } from '@shared/util/browser-util';
import {
  CustomerSearchModelDTO,
  SelectCustomerSearchDialogData,
} from '@shared/components/select-customer-dialog/models/select-customer-dialog.model';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { TranslateService } from '@ngx-translate/core';
import { SelectCustomerDialogComponent } from '@shared/components/select-customer-dialog/components/select-customer-dialog/select-customer-dialog.component';
import { BehaviorSubject, tap } from 'rxjs';
import { ICategoryDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { CommonCustomerService } from '@billing/shared/services/common-customer.service';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { CustomerEditComponent } from '@shared/features/customer/components/customer-edit/customer-edit.component';
import { CustomerForm } from '@shared/features/customer/models/customer-form.model';
import { FilesHelper } from '@shared/components/files-helper/files-helper.component';

@Component({
  selector: 'soe-customer-central-edit',
  templateUrl: './customer-central-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class CustomerCentralEditComponent
  extends EditBaseDirective<
    CustomerCentralDTO,
    CustomerCentralService,
    CustomerCentralForm
  >
  implements OnInit
{
  service = inject(CustomerCentralService);
  coreService = inject(CoreService);
  dialogServiceV2 = inject(DialogService);
  translationService = inject(TranslateService);
  commonCustomerService = inject(CommonCustomerService);

  customerData: CustomerSearchModelDTO | undefined;
  customer!: CustomerCentralDTO;
  categories!: ICategoryDTO[];
  invoiceDeliveryTypes!: ISmallGenericType[];
  contractRows = new BehaviorSubject<CustomerInvoiceGridDTO[]>([]);
  contracts!: CustomerInvoiceGridDTO[];
  filteredContracts!: CustomerInvoiceGridDTO[];

  offers!: CustomerInvoiceGridDTO[];
  offerRows = new BehaviorSubject<CustomerInvoiceGridDTO[]>([]);
  filteredOffers!: CustomerInvoiceGridDTO[];

  orders!: CustomerInvoiceGridDTO[];
  orderRows = new BehaviorSubject<CustomerInvoiceGridDTO[]>([]);
  filteredOrders!: CustomerInvoiceGridDTO[];

  invoices!: CustomerInvoiceGridDTO[];
  invoiceRows = new BehaviorSubject<CustomerInvoiceGridDTO[]>([]);
  filteredInvoices!: CustomerInvoiceGridDTO[];

  //Flags
  contractsExpanderOpen = signal(false);
  onlyOpenContracts = signal<boolean>(true);
  offersExpanderOpen = signal(false);
  offersLoaded: boolean = false;
  ordersExpanderOpen = signal(false);
  ordersLoaded: boolean = false;
  invoiceExpanderOpen = signal(false);
  invoiceLoaded: boolean = false;

  // Permissions
  modifyPermission!: boolean;
  readOnlyPermission!: boolean;

  contractPermission = false;
  offerPermission = false;
  orderPermission = false;
  invoicePermission = false;
  offerForeignPermission = false;
  orderForeignPermission = false;
  invoiceForeignPermission = false;
  contractUserPermission = false;
  offerUserPermission = false;
  orderUserPermission = false;
  invoiceUserPermission = false;
  contractEditPermission = false;
  offerEditPermission = false;
  orderEditPermission = false;
  invoiceEditPermission = false;
  currencyPermission!: boolean;
  productSalesPricePermission!: boolean;
  planningPermission!: boolean;
  exportPermission!: boolean;
  documentsPermission = false;

  // Values
  contractsIncVat: number = 0;
  contractsExVat: number = 0;
  offersIncVat: number = 0;
  offersExVat: number = 0;
  offersCurrencyIncVat: number = 0;
  offersCurrencyExVat: number = 0;
  ordersIncVat: number = 0;
  ordersExVat: number = 0;
  ordersCurrencyIncVat: number = 0;
  ordersCurrencyExVat: number = 0;
  invoicesIncVat: number = 0;
  invoicesExVat: number = 0;
  invoicesCurrencyIncVat: number = 0;
  invoicesCurrencyExVat: number = 0;
  unpaid: number = 0;
  unpaidCurrency: number = 0;

  // Collections
  orderTypes: SmallGenericType[] = [];
  orderTypesDict: any[] = [];

  filesHelper!: FilesHelper;

  constructor() {
    super();

    this.filesHelper = new FilesHelper(
      true,
      SoeEntityType.CustomerCentral,
      SoeDataStorageRecordType.CustomerFileAttachment,
      Feature.Economy_Customer,
      this.performLoadData
    );
  }

  ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(Feature.Economy_Customer_Customers, {
      lookups: [
        this.loadCategories(),
        this.loadInvoiceDeliveryTypes(),
        this.loadOrderTypes(),
      ],
      additionalReadPermissions: [],
      additionalModifyPermissions: [
        Feature.Billing_Offer_Offers,
        Feature.Billing_Order_Orders,
        Feature.Billing_Invoice_Invoices,
        Feature.Billing_Contract_Contracts,
        Feature.Billing_Offer_OffersUser,
        Feature.Billing_Order_OrdersUser,
        Feature.Billing_Invoice_InvoicesUser,
        Feature.Billing_Contract_ContractsUser,
        Feature.Billing_Offer_Status_Foreign,
        Feature.Billing_Order_Status_Foreign,
        Feature.Billing_Invoice_Status_Foreign,
        Feature.Billing_Offer_Offers_Edit,
        Feature.Billing_Order_Orders_Edit,
        Feature.Billing_Invoice_Invoices_Edit,
        Feature.Billing_Contract_Contracts_Edit,
        Feature.Billing_Product_Products_ShowSalesPrice,
        Feature.Billing_Order_Planning,
        Feature.Economy_Customer_Invoice_Invoices_Edit_ExportSOP,
        Feature.Economy_Customer_Invoice_Invoices_Edit_ExportUniMicro,
        Feature.Economy_Customer_Invoice_Invoices_Edit_ExportDIRegnskap,
        Feature.Economy_Customer_Invoice_Invoices_Edit_ExportDnBNor,
        Feature.Billing_Offer_Offers_Edit_Images,
        Feature.Billing_Order_Orders_Edit_Images,
        Feature.Billing_Contract_Contracts_Edit_Images,
        Feature.Billing_Invoice_Invoices_Edit_Images,
        Feature.Billing_Customer_Customers_Edit_Documents,
      ],
    });

    this.onlyOpenContracts.set(true);
  }

  override createEditToolbar(): void {
    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton('searchCustomer', {
          iconName: signal('search'),
          caption: signal(
            'economy.customer.customercentral.seekcustomerbutton'
          ),
          tooltip: signal(
            'economy.customer.customercentral.seekcustomerbutton'
          ),
          onAction: () => this.openSelectCustomer(),
        }),
      ],
    });

    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton('newcustomerinvoice', {
          iconName: signal('plus'),
          caption: signal('common.customer.invoices.newcustomerinvoice'),
          tooltip: signal('common.customer.invoices.newcustomerinvoice'),
          onAction: () => this.openNewCustomerInvoice(),
        }),
      ],
    });

    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton('neworder', {
          iconName: signal('plus'),
          caption: signal('common.customer.invoices.neworder'),
          tooltip: signal('common.customer.invoices.neworder'),
          onAction: () => this.openNewOrder(),
        }),
      ],
    });
  }

  override onPermissionsLoaded() {
    super.onPermissionsLoaded();
    this.readOnlyPermission = this.flowHandler.hasReadAccess(
      Feature.Economy_Customer_Customers
    );
    this.modifyPermission = this.flowHandler.hasModifyAccess(
      Feature.Economy_Customer_Customers
    );

    this.contractPermission = this.flowHandler.hasModifyAccess(
      Feature.Billing_Contract_Contracts
    );
    this.offerPermission = this.flowHandler.hasModifyAccess(
      Feature.Billing_Offer_Offers
    );
    this.orderPermission = this.flowHandler.hasModifyAccess(
      Feature.Billing_Order_Orders
    );
    this.invoicePermission = this.flowHandler.hasModifyAccess(
      Feature.Billing_Invoice_Invoices
    );
    this.offerForeignPermission = this.flowHandler.hasModifyAccess(
      Feature.Billing_Offer_Status_Foreign
    );
    this.orderForeignPermission = this.flowHandler.hasModifyAccess(
      Feature.Billing_Order_Status_Foreign
    );
    this.currencyPermission = this.invoiceForeignPermission =
      this.flowHandler.hasModifyAccess(Feature.Billing_Invoice_Status_Foreign);
    this.contractUserPermission = this.flowHandler.hasModifyAccess(
      Feature.Billing_Contract_ContractsUser
    );
    this.offerUserPermission = this.flowHandler.hasModifyAccess(
      Feature.Billing_Offer_OffersUser
    );
    this.orderUserPermission = this.flowHandler.hasModifyAccess(
      Feature.Billing_Order_OrdersUser
    );
    this.invoiceUserPermission = this.flowHandler.hasModifyAccess(
      Feature.Billing_Invoice_InvoicesUser
    );
    this.contractEditPermission = this.flowHandler.hasModifyAccess(
      Feature.Billing_Contract_Contracts_Edit
    );
    this.offerEditPermission = this.flowHandler.hasModifyAccess(
      Feature.Billing_Offer_Offers_Edit
    );
    this.orderEditPermission = this.flowHandler.hasModifyAccess(
      Feature.Billing_Order_Orders_Edit
    );
    this.invoiceEditPermission = this.flowHandler.hasModifyAccess(
      Feature.Billing_Invoice_Invoices_Edit
    );
    this.productSalesPricePermission = this.flowHandler.hasModifyAccess(
      Feature.Billing_Product_Products_ShowSalesPrice
    );
    this.planningPermission = this.flowHandler.hasModifyAccess(
      Feature.Billing_Order_Planning
    );
    this.exportPermission =
      this.flowHandler.hasModifyAccess(
        Feature.Economy_Customer_Invoice_Invoices_Edit_ExportSOP
      ) ||
      this.flowHandler.hasModifyAccess(
        Feature.Economy_Customer_Invoice_Invoices_Edit_ExportUniMicro
      ) ||
      this.flowHandler.hasModifyAccess(
        Feature.Economy_Customer_Invoice_Invoices_Edit_ExportDIRegnskap
      ) ||
      this.flowHandler.hasModifyAccess(
        Feature.Economy_Customer_Invoice_Invoices_Edit_ExportDnBNor
      );
    this.documentsPermission =
      this.flowHandler.hasModifyAccess(
        Feature.Billing_Offer_Offers_Edit_Images
      ) ||
      this.flowHandler.hasModifyAccess(
        Feature.Billing_Order_Orders_Edit_Images
      ) ||
      this.flowHandler.hasModifyAccess(
        Feature.Billing_Contract_Contracts_Edit_Images
      ) ||
      this.flowHandler.hasModifyAccess(
        Feature.Billing_Invoice_Invoices_Edit_Images
      ) ||
      this.flowHandler.hasModifyAccess(
        Feature.Billing_Customer_Customers_Edit_Documents
      );

    const extendedPermissions: Feature[] = [];
    if (this.offerEditPermission) {
      extendedPermissions.push(Feature.Billing_Offer_Offers_Edit);
    }
    if (this.orderEditPermission) {
      extendedPermissions.push(Feature.Billing_Order_Orders_Edit);
    }
    if (this.contractEditPermission) {
      extendedPermissions.push(Feature.Billing_Contract_Contracts_Edit);
    }
    if (this.invoiceEditPermission) {
      extendedPermissions.push(Feature.Billing_Invoice_Invoices_Edit);
    }

    this.filesHelper.addExtendedPermission(extendedPermissions);
  }

  override onFinished(): void {
    this.openSelectCustomer();
  }

  openSelectCustomer() {
    const dialogData = new SelectCustomerSearchDialogData();
    dialogData.title = this.translate.instant(
      'economy.customer.customercentral.selectcustomer'
    );

    dialogData.size = 'lg';
    dialogData.originType = SoeOriginType.None;

    if (!this.customerData) {
      this.customerData = new CustomerSearchModelDTO();
    }

    this.dialogServiceV2
      .open(SelectCustomerDialogComponent, dialogData)
      .afterClosed()
      .subscribe(result => {
        if (result) {
          this.customerData = result;
          this.loadCustomer(result.actorCustomerId);
          this.filesHelper.recordId.set(result.actorCustomerId);
        }
      });
  }

  loadCategories() {
    return this.coreService
      .getCategoriesGrid(SoeCategoryType.Customer, false, false, false)
      .pipe(tap(x => (this.categories = x)));
  }

  loadInvoiceDeliveryTypes() {
    return this.coreService
      .getTermGroupContent(TermGroup.InvoiceDeliveryType, true, false)
      .pipe(tap(x => (this.invoiceDeliveryTypes = x)));
  }

  loadCustomer(actorCustomerId: number) {
    this.service
      .loadCompleteCustomer(
        actorCustomerId,
        false,
        false,
        true,
        false,
        true,
        true
      )
      .subscribe((x: CustomerCentralDTO) => {
        this.customer = x;
        this.patchCustomerCentralData();
      });
  }

  private patchCustomerCentralData() {
    if (this.customer) {
      const result = this.customer;
      if (this.customer.categoryIds) {
        this.customer.categoryIds.forEach((c, idx) => {
          this.customer.categoryString +=
            idx === 0
              ? ''
              : ', ' + this.categories.find(d => d.categoryId == c)?.name;
        });
      }

      const invoiceDeliveryType = this.invoiceDeliveryTypes.find(
        d => this.customer.invoiceDeliveryType == d.id
      );

      this.form?.patchValue({
        customer: (result.customerNr || '') + ' ' + (result.name || ''),
        orgNr: result ? result.orgNr : '',
        blockOrderString:
          result.blockOrder && result.blockNote
            ? this.translate.instant('core.yes') + ' ' + result.blockNote
            : result.blockOrder
              ? this.translate.instant('core.yes')
              : this.translate.instant('core.no'),
        note: result ? result.note : '',
        billingAddress: this.customerData?.billingAddress || '',
        deliveryAddress: this.customerData?.deliveryAddress || '',
        phoneNumber: this.customerData?.phoneNumber || '',
        categoryString: this.customer.categoryString,
        invoiceDeliveryTypeString: invoiceDeliveryType
          ? invoiceDeliveryType.name
          : '',
      });
      this.loadCustomerCentralCountersAndBalance();
    }
  }

  loadCustomerCentralCountersAndBalance() {
    const counterTypes: number[] = [];
    const isUserInvoice = !this.invoicePermission && this.invoiceUserPermission;
    const isUserOrder = !this.orderPermission && this.orderUserPermission;
    const isUserOffer = !this.offerPermission && this.offerUserPermission;
    const isUserContract =
      !this.contractPermission && this.contractUserPermission;

    counterTypes.push(
      isUserInvoice
        ? SoeOriginStatusClassification.CustomerInvoicesOpenUser
        : SoeOriginStatusClassification.CustomerInvoicesOpen
    );
    if (this.invoiceForeignPermission) {
      counterTypes.push(
        isUserInvoice
          ? SoeOriginStatusClassification.CustomerInvoicesOpenUserForeign
          : SoeOriginStatusClassification.CustomerInvoicesOpenForeign
      );
    }

    counterTypes.push(
      isUserOrder
        ? SoeOriginStatusClassification.OrdersOpenUser
        : SoeOriginStatusClassification.OrdersOpen
    );
    if (this.orderForeignPermission) {
      counterTypes.push(
        isUserOrder
          ? SoeOriginStatusClassification.OrdersOpenUserForeign
          : SoeOriginStatusClassification.OrdersOpenForeign
      );
    }

    counterTypes.push(
      isUserOffer
        ? SoeOriginStatusClassification.OffersOpenUser
        : SoeOriginStatusClassification.OffersOpen
    );
    if (this.offerForeignPermission) {
      counterTypes.push(
        isUserOffer
          ? SoeOriginStatusClassification.OffersOpenUserForeign
          : SoeOriginStatusClassification.OffersOpenForeign
      );
    }

    counterTypes.push(
      isUserContract
        ? SoeOriginStatusClassification.ContractsOpenUser
        : SoeOriginStatusClassification.ContractsOpen
    );

    this.commonCustomerService
      .getCustomerCentralCountersAndBalance(
        counterTypes,
        this.customer?.actorCustomerId,
        0,
        0
      )
      .subscribe(items => {
        for (const v of items) {
          switch (v.classification) {
            case SoeOriginStatusClassification.ContractsOpenUser:
            case SoeOriginStatusClassification.ContractsOpen:
              this.contractsIncVat = v.balanceTotal;
              this.contractsExVat = v.balanceExVat;
              break;
            case SoeOriginStatusClassification.OffersOpenUser:
            case SoeOriginStatusClassification.OffersOpen:
              this.offersIncVat = v.balanceTotal;
              this.offersExVat = v.balanceExVat;
              break;
            case SoeOriginStatusClassification.OffersOpenUserForeign:
            case SoeOriginStatusClassification.OffersOpenForeign:
              this.offersCurrencyIncVat = v.balanceTotal;
              this.offersCurrencyExVat = v.balanceExVat;
              break;
            case SoeOriginStatusClassification.OrdersOpen:
            case SoeOriginStatusClassification.OrdersOpenUser:
              this.ordersIncVat = v.balanceTotal;
              this.ordersExVat = v.balanceExVat;
              break;
            case SoeOriginStatusClassification.OrdersOpenForeign:
            case SoeOriginStatusClassification.OrdersOpenUserForeign:
              this.ordersCurrencyIncVat = v.balanceTotal;
              this.ordersCurrencyExVat = v.balanceExVat;
              break;
            case SoeOriginStatusClassification.CustomerInvoicesOpen:
            case SoeOriginStatusClassification.CustomerInvoicesOpenUser:
              this.invoicesIncVat = v.balanceTotal;
              this.invoicesExVat = v.balanceExVat;
              break;
            case SoeOriginStatusClassification.CustomerInvoicesOpenForeign:
            case SoeOriginStatusClassification.CustomerInvoicesOpenUserForeign:
              this.invoicesCurrencyIncVat = v.balanceTotal;
              this.invoicesCurrencyExVat = v.balanceExVat;
              break;
            case SoeOriginStatusClassification.CustomerPaymentsUnpayed:
              this.unpaid = v.balanceTotal;
              break;
            case SoeOriginStatusClassification.CustomerPaymentsUnpayedForeign:
              this.unpaidCurrency = v.balanceTotal;
              break;
          }
        }
      });
  }

  contractExpanderOpened() {
    if (!this.contractsExpanderOpen()) {
      this.contractsExpanderOpen.set(true);
      this.loadContracts();
    }
  }

  loadContracts() {
    this.performLoadData.load(
      this.commonCustomerService
        .getCustomerInvoicesForCustomerCentral(
          SoeOriginStatusClassification.ContractsAll,
          SoeOriginType.Contract,
          this.customer.actorCustomerId,
          false
        )
        .pipe(
          tap(x => {
            this.contracts = x;
            this.postProcessRows(this.contracts);
            this.contractRows.next(this.contracts);
            this.onShowOnlyOpenContracts(true);
          })
        )
    );
  }

  onShowOnlyOpenContracts(value: boolean) {
    if (this.contractsExpanderOpen()) {
      if (!value) this.filteredContracts = this.contracts;
      else
        this.filteredContracts = this.contracts.filter(c => !c.useClosedStyle);
      this.contractRows.next(this.filteredContracts);
    }
  }

  offerExpanderOpened() {
    if (!this.offersExpanderOpen()) {
      this.offersExpanderOpen.set(true);
    }
    if (!this.offersLoaded) {
      this.loadOffers();
      this.offersLoaded = true;
    }
  }

  loadOffers() {
    this.performLoadData.load(
      this.commonCustomerService
        .getCustomerInvoicesForCustomerCentral(
          SoeOriginStatusClassification.OffersAll,
          SoeOriginType.Offer,
          this.customer.actorCustomerId,
          false
        )
        .pipe(
          tap(x => {
            this.offers = x;
            this.postProcessRows(this.offers);
            this.offerRows.next(this.offers);
            this.onShowOnlyOpenOffers(true);
          })
        )
    );
  }

  onShowOnlyOpenOffers(value: boolean) {
    if (this.offersLoaded) {
      if (!value) this.filteredOffers = this.offers;
      else this.filteredOffers = this.offers.filter(o => !o.useClosedStyle);
      this.offerRows.next(this.filteredOffers);
    }
  }

  orderExpanderOpened() {
    if (!this.ordersExpanderOpen()) {
      this.ordersExpanderOpen.set(true);
    }
    if (!this.ordersLoaded) {
      this.loadOrders();
      this.ordersLoaded = true;
    }
  }

  loadOrders() {
    this.performLoadData.load(
      this.commonCustomerService
        .getCustomerInvoicesForCustomerCentral(
          SoeOriginStatusClassification.OrdersAll,
          SoeOriginType.Order,
          this.customer.actorCustomerId,
          false
        )
        .pipe(
          tap(x => {
            this.orders = x;
            this.postProcessRows(this.orders);
            this.orderRows.next(this.orders);
            this.onShowOnlyOpenOrders(true);
          })
        )
    );
  }

  onShowOnlyOpenOrders(value: boolean) {
    if (this.ordersLoaded) {
      if (!value) this.filteredOrders = this.orders;
      else this.filteredOrders = this.orders.filter(o => !o.useClosedStyle);

      this.orderRows.next(this.filteredOrders);
    }
  }

  invoiceExpanderOpened() {
    if (!this.invoiceExpanderOpen()) {
      this.invoiceExpanderOpen.set(true);
    }
    if (!this.invoiceLoaded) {
      this.loadInvoices();
      this.invoiceLoaded = true;
    }
  }

  loadInvoices() {
    this.performLoadData.load(
      this.commonCustomerService
        .getCustomerInvoicesForCustomerCentral(
          SoeOriginStatusClassification.CustomerInvoicesAll,
          SoeOriginType.CustomerInvoice,
          this.customer.actorCustomerId,
          false
        )
        .pipe(
          tap(x => {
            this.invoices = x;
            this.postProcessRows(this.invoices);
            this.invoiceRows.next(this.invoices);
            this.onShowOnlyOpenInvoices(true);
          })
        )
    );
  }

  onShowOnlyOpenInvoices(value: boolean) {
    if (this.invoiceLoaded) {
      if (!value) this.filteredInvoices = this.invoices;
      else this.filteredInvoices = this.invoices.filter(i => !i.useClosedStyle);

      this.invoiceRows.next(this.filteredInvoices);
    }
  }

  postProcessRows(items: CustomerInvoiceGridDTO[]) {
    items.forEach(invoice => {
      invoice.payDate = invoice.payDate || new Date();
      invoice.invoiceDate = invoice.invoiceDate || new Date();
      invoice.dueDate = invoice.dueDate || new Date();

      invoice.expandableDataIsLoaded = false;

      if (invoice.paidAmount === 0) invoice.payDate = undefined;

      if (invoice.exportStatus) {
        invoice.exportStatus = 1;
        invoice.exportStatusName =
          this.terms['common.customer.invoices.export'];
      }

      if (invoice.orderType) {
        const orderType = this.orderTypes.find(o => o.id === invoice.orderType);
        if (orderType) invoice.orderTypeName = orderType.name;
      } else {
        invoice.orderTypeName =
          this.terms['common.customer.invoices.notspecified'];
      }

      if (!invoice.attestStates || invoice.attestStates.length === 0) {
        invoice.useGradient = false;
        invoice.attestStateNames = this.terms['common.customer.invoice.norows'];
      } else if (invoice.attestStates.length === 1) {
        invoice.useGradient = false;
        invoice.attestStateColor = invoice.attestStates[0].color;
      } else {
        invoice.useGradient = true;
        invoice.attestStateColor = '';
      }

      if (invoice.fullyPaid) {
        invoice.paidInfo = this.terms['common.customer.invoices.invoicepaid'];
        invoice.paidStatusColor = '#98EF5D';
      } else {
        if (invoice.paidAmount > 0) {
          invoice.paidInfo =
            this.terms['common.customer.invoices.invoicepartlypaid'];
          invoice.paidStatusColor = '#EAF055';
        } else {
          invoice.paidInfo =
            this.terms['common.customer.invoices.invoiceunpaid'];
          invoice.paidStatusColor = '#ED8D6C';
        }
      }

      this.setInformationIconAndTooltip(invoice);

      if (invoice.useClosedStyle) {
        invoice.showCreatePayment = !invoice.useClosedStyle;
        invoice.payAmount = invoice.payAmountCurrency = 0;
      } else {
        invoice.showCreatePayment = true;
      }
    });
  }

  private loadOrderTypes() {
    return this.coreService
      .getTermGroupContent(TermGroup.OrderType, false, false)
      .pipe(
        tap(x => {
          this.orderTypes = x;
          x.forEach(row => {
            this.orderTypesDict.push({ value: row.name, label: row.name });
          });
        })
      );
  }

  private setInformationIconAndTooltip(item: CustomerInvoiceGridDTO) {
    const hasInfo: boolean =
      (item.infoIcon & Number(InvoiceRowInfoFlag.Info)) ==
      Number(InvoiceRowInfoFlag.Info);
    const hasError: boolean =
      (item.infoIcon & Number(InvoiceRowInfoFlag.Error)) ==
      Number(InvoiceRowInfoFlag.Error);
    const hasHousehold: boolean =
      (item.infoIcon & Number(InvoiceRowInfoFlag.HouseHold)) ==
      Number(InvoiceRowInfoFlag.HouseHold);

    // Get status icons
    // const flaggedEnum: IFlaggedEnum;

    //Printing - distribution
    if (item.billingInvoicePrinted) {
      item.billingIconValue = 'print';
      item.billingIconMessage = this.terms['common.customer.invoices.printed'];
    }
    if (item.statusIcon == SoeStatusIcon.ElectronicallyDistributed) {
      item.billingIconValue = 'paper-plane';
      item.billingIconMessage =
        this.terms['common.customer.invoices.einvoiced'];
    } else if (item.statusIcon == SoeStatusIcon.Email) {
      item.billingIconValue = 'envelope';
      item.billingIconMessage =
        this.terms['common.customer.invoices.emailsent'];
    } else if (item.statusIcon == SoeStatusIcon.EmailError) {
      item.billingIconValue = 'envelope';
      item.billingIconMessage =
        this.terms['common.customer.invoices.sendemailfailed'];
    }

    if (
      hasError ||
      hasInfo ||
      hasHousehold ||
      item.statusIcon != SoeStatusIcon.None
    ) {
      if (item.statusIcon == SoeStatusIcon.Imported) {
        item.statusIconValue = 'download';
      } else if (hasError) {
        item.statusIconValue = 'exclamation-triangle';
        item.statusIconMessage = this.translate.instant('core.showinfo');
      } else if (hasInfo && hasHousehold) {
        item.statusIconValue = 'home';
        item.statusIconMessage =
          this.terms['core.showinfo'] +
          ' - ' +
          this.terms['common.customer.invoices.hashousededuction'];
      } else if (hasInfo && !hasHousehold) {
        item.statusIconValue = 'info-circle';
        item.statusIconMessage = this.terms['core.showinfo'];
      } else if (!hasInfo && hasHousehold) {
        item.statusIconValue = 'home';
        item.statusIconMessage =
          this.terms['common.customer.invoices.hashousededuction'];
      } else if (item.statusIcon != SoeStatusIcon.None) {
        if (
          item.statusIcon != SoeStatusIcon.Email &&
          item.statusIcon != SoeStatusIcon.EmailError &&
          item.statusIcon != SoeStatusIcon.ElectronicallyDistributed
        ) {
          item.statusIconValue = 'paperclip';

          if (item.statusIcon == SoeStatusIcon.Imported) {
            item.statusIconMessage =
              item.statusIconMessage && item.statusIconMessage != ''
                ? '<br/>' + this.terms['common.imported']
                : this.terms['common.imported'];
          }
          if (item.statusIcon == SoeStatusIcon.Attachment) {
            item.statusIconMessage =
              item.statusIconMessage && item.statusIconMessage != ''
                ? '<br/>' + this.terms['common.hasaattachedfiles']
                : this.terms['common.hasaattachedfiles'];
          }
          if (item.statusIcon == SoeStatusIcon.Image) {
            item.statusIconMessage =
              item.statusIconMessage && item.statusIconMessage != ''
                ? '<br/>' + this.terms['common.hasattachedimages']
                : this.terms['common.hasattachedimages'];
          }
          if (item.statusIcon == SoeStatusIcon.Checklist) {
            item.statusIconMessage =
              item.statusIconMessage && item.statusIconMessage != ''
                ? '<br/>' + this.terms['common.customer.invoices.haschecklists']
                : this.terms['common.customer.invoices.haschecklists'];
          }
        }
      }
    }
  }

  openNewOrder() {
    if (!this.customer) return;
    const url = `/soe/billing/order/status/default.aspx?classificationgroup=${SoeOriginStatusClassificationGroup.HandleOrders}&invoiceId=${0}&invoiceNr=${''}&customerId=${this.customer.actorCustomerId}`;
    BrowserUtil.openInNewTab(window, url);
  }

  openNewCustomerInvoice() {
    if (!this.customer) return;
    const url = `/soe/billing/invoice/status/default.aspx?classificationgroup=${SoeOriginStatusClassificationGroup.HandleCustomerInvoices}&invoiceId=${0}&invoiceNr=${''}&customerId=${this.customer.actorCustomerId}`;
    BrowserUtil.openInNewTab(window, url);
  }

  openMap(addressLocation: string) {
    const parameters = addressLocation;
    BrowserUtil.openInNewTab(
      window,
      `https://www.google.com/maps/search/?api=1&query=${parameters}`
    );
  }

  openCustomer() {
    this.openEditInNewTab({
      id: this.customer.actorCustomerId || 0,
      additionalProps: {
        editComponent: CustomerEditComponent,
        FormClass: CustomerForm,
        editTabLabel: 'common.customer.customer.customer',
      },
    });
  }

  loadFileList(opened: boolean) {
    if (opened) this.filesHelper.loadFiles(true, true).subscribe();
  }
}
