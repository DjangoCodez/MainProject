import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/validationsummaryhandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { ICommonCustomerService } from "../../../Common/Customer/CommonCustomerService";
import { ISmallGenericType, IPriceListTypeDTO, IShiftTypeGridDTO, IOrderShiftDTO, IImagesDTO, ICustomerProductPriceSmallDTO, IActionResult, ICustomerInvoicePrintDTO } from "../../../Scripts/TypeLite.Net4";
import { OrderDTO, CustomerInvoiceAccountRowDTO, ProductRowDTO } from "../../../Common/Models/InvoiceDTO";
import { CustomerDTO } from "../../../Common/Models/CustomerDTO";
import { ProjectTimeBlockDTO, ProjectDTO } from "../../../Common/Models/ProjectDTO";
import { ChecklistHeadRecordCompactDTO, ChecklistExtendedRowDTO } from "../../../Common/Models/checklistdto";
import { OrderEditTransferFunctions, TimeProjectContainer, ProductRowsContainers, IconLibrary, SOEMessageBoxImage, SOEMessageBoxButtons, OrderEditProjectFunctions, OrderEditSaveFunctions, OrderInvoiceEditPrintFunctions, SOEMessageBoxButton, SOEMessageBoxSize, HouseholdDeductionGridButtonFunctions } from "../../../Util/Enumerations";
import { ShiftTypeGridDTO } from "../../../Common/Models/ShiftTypeDTO";
import { InvoiceEditHandler } from "../Helpers/InvoiceEditHandler";
import { IReportService } from "../../../Core/Services/ReportService";
import { IOrderService } from "./OrderService";
import { IProductService } from "../Products/ProductService";
import { ToolBarUtility, ToolBarButton } from "../../../Util/ToolBarUtility";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { CoreUtility } from "../../../Util/CoreUtility";
import { EditController as BillingProjectsEditController } from "../Projects/EditController";
import { EditController as BillingInvoicesEditController } from "../Invoices/EditController";
import { EditController as CustomerEditController } from "../../../Common/Customer/Customers/EditController";
import { SelectProjectController } from "../../../Common/Dialogs/SelectProject/SelectProjectController";
import { HtmlUtility } from "../../../Util/HtmlUtility";
import { SelectEmailController } from "../../../Common/Dialogs/SelectEmail/SelectEmailController";
import { SelectReportController } from "../../../Common/Dialogs/SelectReport/SelectReportController";
import { TabMessage } from "../../../Core/Controllers/TabsControllerBase1";
import { EditDeliveryAddressController } from "../Dialogs/EditDeliveryAddress/EditDeliveryAddressController";
import { FileUploadDTO } from "../../../Common/Models/fileuploaddto";
import { TextBlockDialogController } from "../../../Common/Dialogs/textblock/textblockdialogcontroller";
import { AccountingRowDTO } from "../../../Common/Models/AccountingRowDTO";
import { SelectCustomerController } from "../../../Common/Dialogs/SelectCustomer/SelectCustomerController";
import { StringUtility, Guid } from "../../../Util/StringUtility";
import { AccordionSettingsController } from "../../../Common/Dialogs/AccordionSettings/AccordionSettingsController";
import { SoeEntityImageType, SoeEntityType, Feature, SoeProjectRecordType, TermGroup_ProjectType, TermGroup_OrderType, TermGroup_BillingType, OrderContractType, SoeOriginStatus, CompanySettingType, UserSettingType, TermGroup, TermGroup_Languages, TermGroup_TimeScheduleTemplateBlockType, TermGroup_CurrencyType, SoeInvoiceRowType, TermGroup_InvoiceVatType, OrderInvoiceRegistrationType, ActionResultSave, SoeReportTemplateType, SoeEntityState, SoeOriginStatusChange, TextBlockType, SimpleTextEditorDialogMode, CustomerAccountType, AccountingRowType, TermGroup_ChecklistRowType, SoeOriginType, TermGroup_ContractGroupPeriod, EmailTemplateType, TermGroup_ProjectStatus, TermGroup_OrderEdiTransferMode, TermGroup_EInvoiceFormat, SoeInvoiceDeliveryType } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { IShortCutService } from "../../../Core/Services/ShortCutService";
import { InvoiceCurrencyHelper } from "../Helpers/InvoiceCurrencyHelper";
import { FilesHelper } from "../../../Common/Files/FilesHelper";
import { ContractGroupExtendedGridDTO } from "../../../Common/Models/ContractDTO";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IScopeWatcherService } from "../../../Core/Services/ScopeWatcherService";
import { OrderIsReadyHelper } from "./OrderIsReadyHelper";
import { SelectCustomerInvoiceController } from "../../../Common/Dialogs/SelectCustomerInvoice/SelectCustomerInvoiceController";
import { OneTimeCustomerController } from "../Dialogs/OneTimeCustomer/OneTimeCustomerController";
import { OriginUserHelper } from "../Helpers/OriginUserHelper";
import { ShowCustomerInvoiceInfoController } from "../Dialogs/ShowCustomerInvoiceInfo/ShowCustomerInvoiceInfoController";
import { IPurchaseService } from "../Purchase/Purchase/PurchaseService";
import { IRequestReportService } from "../../Reports/RequestReportService";

export class EditController extends EditControllerBase2 implements ICompositionEditController {
    // Modal
    private modal: any;
    private modalTitle: string;
    private isModal = false;

    // Helpers
    //private amountHelper: AmountHelper;
    private invoiceEditHandler: InvoiceEditHandler;
    private currencyHelper: InvoiceCurrencyHelper;
    private invoiceFilesHelper: FilesHelper;
    private orderIsReadyHelper: OrderIsReadyHelper;
    private originUserHelper: OriginUserHelper;

    // Config
    private currentAccountYearId = 0;
    private currentAccountYearIsOpen = false;

    private isTemplateRegistration = false;

    // Permissions
    private productRowsPermission = false;
    private timeProjectPermission = false;
    private filesPermission = false;
    private checklistsPermission = false;
    private accountingRowsPermission = false;
    private orderPlanningPermission = false;
    private tracingPermission = false;
    private expensesPermission = false;
    private orderSupplierInvoicesPermission = false;
    private supplierInvoicesEditPermission = false;
    private mainOrderPermission = false;
    private purchasePermission = false;
    private directInvoicingPermission = false;
    private priceOptimizationPermission = false;

    private templatesPermission = false;
    private showSalesPricePermission = true;
    private showPurchasePricePermission = true;
    private editCustomerPermission = false;
    private useCurrency = false;
    private reportPermission = false;
    private showProjectsWithoutCustomer = false;
    private useDiffWarning = false;
    private createEInvoicePermission = false;

    private unlockPermission = false;
    private closePermission = false;
    private transferOrderToContract = false;
    private categoriesPermission = false;
    private changeNextInvoiceDatePermission = false;
    private modifyOtherEmployeesPermission = false;
    private modifyProjectPermission = false;
    private modifyCustomerPermission = false;

    // Company settings
    private showPayingCustomer = false;
    private showTransactionCurrency = false;
    private showEnterpriseCurrency = false;
    private showLedgerCurrency = false;

    private defaultInvoiceText: string;
    private defaultOurReference: string;
    private defaultWholesellerId = 0;
    private defaultPriceListTypeId = 0;
    private includeWorkDescriptionOnInvoice = false;
    private defaultDeliveryTypeId = 0;
    private defaultDeliveryConditionId = 0;
    private useDeliveryAddressAsInvoiceAddress = false;
    private defaultPaymentConditionHouseholdDeductionId = 0;
    private useCashSales = false;

    private discountDays: number = null;
    private discountPercent = 0;
    private paymentConditionDays = 0;
    private paymentServiceOnlyToContract = false;

    private fixedPriceProductId = 0;
    private freightAmountProductId = 0;
    private invoiceFeeProductId = 0;
    private hideVatRate = false;
    private useFreightAmount = false;
    private useInvoiceFee = false;
    private disableInvoiceFee = false;

    private autoGenerateProject = false;
    private suggestOrderNrAsProjectNr = false;
    private useCustomerNameAsProjectName = false;
    private projectIncludeTimeProjectReport = false;
    private projectIncludeOnlyInvoicedTimeInTimeProjectReport = false;
    private triangulationSales = false;
    private defaultOneTimeCustomerId = 0;
    private offerValidNoOfDays = 0;

    private mandatoryChecklist = false;

    private emailTemplateId = 0;
    private offerEmailTemplateId: number;
    private orderEmailTemplateId: number;
    private contractEmailTemplateId: number;
    private eInvoiceFormat = 0;
    private reminderReportId = 0;
    private interestReportId = 0;
    private voucherListReportId = 0;
    private timeProjectReportId = 0;
    private billingContractReportId = 0;
    private customerReportTemplateId = 0;

    private transferToVoucher = false;
    private askPrintVoucherOnTransfer = false;
    private usePartialInvoicingOnOrderRow = false;
    private askCreateInvoiceWhenOrderReady = false;
    private createInvoiceWhenOrderReady = false;
    private hasDeductionAmountMismatch: any;
    private askOpenInvoiceWhenCreateInvoiceFromOrder = false;

    private showZeroRowWarning = false;

    private autoSaveInterval = 0;

    // User settings
    private defaultOurReferenceUserId = 0;
    private checkConflictsOnSave = false;
    private useOneTimeCustomer = false;
    private defaultOrderType = 0;
    private expanderSettings: any;

    // Company accounts
    private defaultCreditAccountId = 0;
    private defaultDebitAccountId = 0;
    private defaultVatAccountId = 0;
    private reverseVatPurchaseId = 0;
    private reverseVatSalesId = 0;
    private contractorVatAccountDebitId = 0;
    private contractorVatAccountCreditId = 0;
    private defaultCustomerDiscountAccount = 0;
    private defaultCustomerDiscountOffsetAccount = 0;
    private defaultVatRate = 0;

    // Customer accounts
    private customerVatAccountId = 0;

    private vatRate: number = Constants.DEFAULT_VAT_RATE;

    // Lookups
    private terms: any;
    private customers: any[];
    private orderTypes: ISmallGenericType[];
    private orderTemplates: ISmallGenericType[];
    private fixedPriceOrderTypes: ISmallGenericType[];
    //private vatTypes: ISmallGenericType[];
    private ourReferences: ISmallGenericType[];

    private priceListTypes: IPriceListTypeDTO[];

    private invoicePaymentServices: ISmallGenericType[];
    private voucherSeries: any[];
    private shiftTypes: IShiftTypeGridDTO[];
    private invoiceDeliveryTypes: ISmallGenericType[];
    private contractGroups: any[];
    private includeTimeInReportItems: any[] = [];

    // Data
    private invoice: OrderDTO;
    private originalInvoice: OrderDTO;
    private accountRows: CustomerInvoiceAccountRowDTO[];
    private customer: CustomerDTO; //Also used for paying customer
    private deliveryCustomer: CustomerDTO;
    private customerReferences: ISmallGenericType[];
    private customerEmails: ISmallGenericType[];
    private plannedShifts: IOrderShiftDTO[];
    private projectTimeBlockRows: ProjectTimeBlockDTO[] = [];
    private checklistHeads: ChecklistHeadRecordCompactDTO[] = [];
    private signatures: IImagesDTO[];
    private invoiceIds: number[];
    private currentBalance: number;
    private ordersForProject: any[];

    // Flags
    private loadingInvoice = false;
    private invoiceIsLoaded = false;
    private hasModifiedProductRows = false;
    private hasHouseholdTaxDeduction = false;
    private isStopped = false;
    private isLocked = false;
    private loadingTimeProjectRows = false;
    private createCopy = false;
    private showNavigationButtons = true;
    private loadChecklistsRecords = true;
    private checklistsLoaded = false;
    private showUnlockButton = false;
    private showCloseButton = false;
    private paymentServiceReadOnly = false;
    private validateRowAttestStateChange = false;
    private askChangingProject = true;
    private autoSaveActive = false;
    private executing = false;
    private transfering = false;
    private invoiceFeeUpdated = false;
    private freightAmountUpdated = false;
    private loadingCustomer = false;
    private loadingDeliveryCustomer = false;
    private copyProductRows = false;
    private vatTypeUpdated = false;
    private updateInternalIdCounter = false;
    private ignoreReloadInvoiceFee = false;
    private ignoreReloadFreightAmount = false;
    private lockTemplateFlag = false;
    private templateOpenFromAgreement = false;
    private createOrderTemplateFromAgreement = false;
    private transferingToInvoice = false;
    private includeAttachments = false;
    private contractHeadIsLocked = false;
    private validateProductRowsForTransfer = false;
    private resetDocumentsGridData = false;
    private copyChecklistRecords = false;
    private doNotLoadCustomer = false;
    private updateTab = false;
    private checkRecalculatePrices = false;

    // Transfer
    private transferType: OrderEditTransferFunctions;
    private transferAllRowsAndCloseOrder = false;
    private transferFixedPriceToInvoiceLeavingOthers = false;
    private transferringContractProducts = false;
    private copyTransferredContractRows = false;
    private hasLiftRowsNotTransferable = false;
    private keepOrderOpen = false;
    private createdInvoices: any[];
    private performDirectInvoicing = false;
    private performCreateServiceOrder = false;

    // Properties
    public invoiceId: number;
    private customerId: number;
    private projectId: number;
    private feature: Feature;
    private invoiceAccountYearId = 0;
    private accountPeriodId = 0;

    private fixedPrice = false;
    private fixedPriceKeepPrices = false;
    private showRemainingAmountExVat = true;
    private recordType = SoeProjectRecordType.Order;
    private projectType = TermGroup_ProjectType.TimeProject;
    private projectContainer = TimeProjectContainer.Order;
    private employeeId = 0;
    private timeProjectFrom: Date;
    private timeProjectTo: Date;
    private sendXEMail = false;
    private isProjectCentral = false;
    private originalCustomerId = 0;
    private prevContractGroupId = 0;
    private contractGroupPeriodId: string;
    private contractGroupPeriod: string;
    private contractGroupInterval: number;
    private templateText: string;

    // Autosave
    private timerToken: any = 0;
    private tickCounter = 0;
    private autoSaveText: string;
    private timerPaused = false;

    get isOrderTypeUnspecified(): boolean {
        return this.invoice && this.invoice.orderType === TermGroup_OrderType.Unspecified;
    }

    // Service orders are handled as project orders
    get isOrderTypeProject(): boolean {
        return this.invoice && (this.invoice.orderType === TermGroup_OrderType.Project || this.invoice.orderType === TermGroup_OrderType.Contract || this.invoice.orderType === TermGroup_OrderType.Service || this.invoice.orderType === TermGroup_OrderType.ATA);
    }

    get isOrderTypeSales(): boolean {
        return this.invoice && this.invoice.orderType === TermGroup_OrderType.Sales;
    }

    get showNote(): boolean {
        return this.invoice ? this.isOrder && this.invoice.note && this.invoice.note.length > 0 : false;
    }

    get showChecklist(): boolean {
        return this.invoice ? this.checklistsPermission && !this.isOrderTypeSales && !this.isOrderTypeInternal && this.isOrder : false;
    }

    get isOrderTypeInternal(): boolean {
        return this.invoice && this.invoice.orderType === TermGroup_OrderType.Internal;
    }

    get isCredit(): boolean {
        return this.invoice && this.invoice.billingType === TermGroup_BillingType.Credit;
    }
    set isCredit(value: boolean) { /* Not actually a setter, just to make binding work */ }

    get projectCentralLink(): any {
        return "/soe/billing/project/central/?project=" + this.invoice.projectId;
    }
    set projectCentralLink(value: any) { /* Not actually a setter, just to make binding work */ }

    private resetReference = false;
    private _selectedCustomer;
    get selectedCustomer(): ISmallGenericType {
        return this._selectedCustomer;
    }
    set selectedCustomer(item: ISmallGenericType) {

        if (this._selectedCustomer && item && item.id === this._selectedCustomer.id) {
            return;
        }

        if (this.doNotLoadCustomer) {
            this._selectedCustomer = item;
        }
        else {
            if (item && this.invoice.actorId !== item.id) {
                this.invoice.customerName = undefined;
                this.invoice.customerEmail = undefined;
                this.invoice.customerPhoneNr = undefined;
            }

            if (item && item.id > 0) {
                this.resetReference = (this.invoice && this.invoice.actorId !== item.id);
                this._selectedCustomer = item;
                this.setAsDirty();
                this.initLoadCustomer();
            }
            else {
                this._selectedCustomer = undefined;
                //null=empty choosen, undefined=item not yet selected
                if (item === null) {
                    if (this.invoice)
                        this.invoice.actorId = undefined;
                    if (this.customer)
                        this.customer = undefined;
                }
            }
        }
    }

    private _selectedDeliveryCustomer;
    get selectedDeliveryCustomer(): ISmallGenericType {
        return this._selectedDeliveryCustomer;
    }
    set selectedDeliveryCustomer(item: ISmallGenericType) {
        if (this.doNotLoadCustomer) {
            this._selectedDeliveryCustomer = item;
        }
        else {
            if (item && item.id > 0) {
                this._selectedDeliveryCustomer = item;
                if (this._selectedDeliveryCustomer && this._selectedDeliveryCustomer.id) {
                    if (this.invoice)
                        this.invoice.deliveryCustomerId = this._selectedDeliveryCustomer.id;
                    this.loadDeliveryCustomer();
                }
            }
            else {
                this._selectedDeliveryCustomer = undefined;
                if (this.invoice)
                    this.invoice.deliveryCustomerId = undefined;
                if (this.deliveryCustomer)
                    this.deliveryCustomer = undefined;
            }
        }
    }

    get customerProducts(): ICustomerProductPriceSmallDTO[] {
        return this.customer ? this.customer.customerProducts : [];
    }

    private _selectedFixedPriceOrder: ISmallGenericType;
    get selectedFixedPriceOrder(): ISmallGenericType {
        return this._selectedFixedPriceOrder;
    }
    set selectedFixedPriceOrder(item: ISmallGenericType) {
        //this._selectedFixedPriceOrder = item;
        //this.invoice.fixedPriceOrder = item && item.id === OrderContractType.Fixed;
        if (!item) return

        if (this._selectedFixedPriceOrder && this._selectedFixedPriceOrder.id === item.id) return;

        if (item.id === OrderContractType.Fixed) {
            this.fixedPrice = true;
            this.invoice.fixedPriceOrder = true;
            this.openProductRowExpander(true);
            this._selectedFixedPriceOrder = item;
        }
        else if (item.id === OrderContractType.Continuous) {
            if (this.hasFixedPriceRows()) {
                this.hasFixedPriceRowsDialog();
            }
            else {
                this.invoice.fixedPriceOrder = false;
                this._selectedFixedPriceOrder = item;
            }
        }
    }

    private selectedOrderTemplate: number;

    private _selecedInvoiceDate: Date;
    get selectedInvoiceDate() {
        return this._selecedInvoiceDate;
    }
    set selectedInvoiceDate(date: Date) {
        this._selecedInvoiceDate = date ? new Date(<any>date.toString()) : null;
        if (this.invoice) {
            this.invoice.invoiceDate = this._selecedInvoiceDate;
            if (this.isContract) {
                if (!this.loadingInvoice)
                    this.getNextContractPeriod();
                this.invoice.currencyDate = this.currencyHelper.currencyDate = this._selecedInvoiceDate;
            }
            else {
                this.selectedVoucherDate = this._selecedInvoiceDate;
                this.currencyHelper.currencyDate = this._selecedInvoiceDate;
                this.setDueDate();
            }
        }
    }

    private _selecedNextInvoiceDate: Date;
    get selectedNextInvoiceDate() {
        return this._selecedNextInvoiceDate;
    }
    set selectedNextInvoiceDate(date: Date) {
        this._selecedNextInvoiceDate = date ? new Date(<any>date.toString()) : null;
        if (this.invoice) {
            this.invoice.nextContractPeriodDate = this._selecedNextInvoiceDate;
            const group = _.find(this.contractGroups, (c) => c.contractGroupId === this.invoice.contractGroupId);
            if (group)
                this.setNextContractPeriod(group.periodId, this._selecedNextInvoiceDate);
        }
    }

    private _selectedVoucherDate: Date;
    get selectedVoucherDate() {
        return this._selectedVoucherDate;
    }
    set selectedVoucherDate(date: Date) {
        this._selectedVoucherDate = date ? new Date(<any>date.toString()) : null;
        if (this.invoice) {
            this.invoice.voucherDate = this.selectedVoucherDate;
            this.invoice.currencyDate = this.selectedVoucherDate;
        }
    }

    get selectedOrderType() {
        if (!this.invoice)
            return TermGroup_OrderType.Unspecified;

        return this.invoice.orderType;
    }
    set selectedOrderType(type: TermGroup_OrderType) {
        if (!this.invoice)
            return;

        if (type !== this.selectedOrderType && type === TermGroup_OrderType.Sales && this.checklistHeads) {
            if (this.checklistHeads.filter(head => head.state !== SoeEntityState.Deleted).length > 0) {
                this.invoice.orderType = this.selectedOrderType;

                var keys: string[] = [
                    "core.error",
                    "billing.order.lockedtypechecklist"
                ];
                this.translationService.translateMany(keys).then((terms) => {
                    this.notificationService.showDialog(terms["core.error"], terms["billing.order.lockedtypechecklist"], SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK);
                });
            } else {
                this.invoice.orderType = type;
            }
        } else {
            this.invoice.orderType = type;
        }
    }

    private _selectedPriceListType: IPriceListTypeDTO;
    get selectedPriceListType() {
        return this._selectedPriceListType;
    }
    set selectedPriceListType(type: IPriceListTypeDTO) {
        this.setPriceListType(type ? type.priceListTypeId : 0);
    }

    private previousShiftTypeId: number;
    private _selectedShiftType;
    get selectedShiftType(): ShiftTypeGridDTO {
        return this._selectedShiftType;
    }
    set selectedShiftType(item: ShiftTypeGridDTO) {
        this._selectedShiftType = item;
        if (this.invoice) {
            this.invoice.shiftTypeId = item ? item.shiftTypeId : undefined;
            if (!this.loadingInvoice) {
                this.setValuesFromShiftType(item);
                this.setAsDirty();
            }
        }
    }

    private _selectedOrderInvoiceTemplate: ISmallGenericType;
    get selectedOrderInvoiceTemplate(): ISmallGenericType {
        return this._selectedOrderInvoiceTemplate;
    }

    set selectedOrderInvoiceTemplate(template: ISmallGenericType) {
        this.$timeout(() => {
            this._selectedOrderInvoiceTemplate = template;
            if (this._selectedOrderInvoiceTemplate)
                this.invoice.orderInvoiceTemplateId = this._selectedOrderInvoiceTemplate.id;
            else
                this.invoice.orderInvoiceTemplateId = undefined;

            if (!this.loadingInvoice)
                this.setAsDirty();
        });
    }

    // Only used for template
    private _includeTimeInReport: number;
    private set includeTimeInReport(id: number) {
        this.invoice.printTimeReport = (id !== 0);
        this.invoice.includeOnlyInvoicedTime = (id === 2);
        this._includeTimeInReport = id;
    }
    private get includeTimeInReport() {
        return this._includeTimeInReport;
    }

    private _orderExpanderInitiallyOpened: boolean;
    set orderExpanderInitiallyOpened(value: boolean) {
        this._orderExpanderInitiallyOpened = value;
    }
    get orderExpanderInitiallyOpened(): boolean {
        return this._orderExpanderInitiallyOpened !== undefined ? this._orderExpanderInitiallyOpened : this.isNew;
    }

    private orderExpanderIsOpen = false;
    private orderOrderExpanderIsOpen = true;
    private orderConditionExpanderIsOpen = true;
    private accountingRowsExpanderIsOpen = false;
    private timeProjectRowsExpanderIsOpen = false;
    private checklistExpanderIsOpen = false;
    private planningExpanderIsOpen = false;
    private productRowsExpanderIsOpen = false;
    private traceRowsExpanderIsOpen = false;
    private expensesExpanderIsOpen = false;
    private supplierInvoiceExpanderIsOpen = false;
    private documentExpanderIsOpen = false;
    private noteExpanderIsOpen = false;

    // Flags for hiding
    private headRendered = false;
    private productRowsRendered = false;
    private productRowsRenderFinalized = false;
    private timeProjectRendered = false;
    private filesRendered = false;
    private accountingRowsRendered = false;
    private planningRendered = false;
    private traceRowsRendered = false;
    private expensesRendered = false;
    private projectNotSaved = false;
    private supplierInvoicesRowsRendered = false;

    get isOrder(): boolean {
        return this.feature === Feature.Billing_Order_Status;
    }

    get isOffer(): boolean {
        return this.feature === Feature.Billing_Offer_Status;
    }

    get isContract(): boolean {
        return this.feature === Feature.Billing_Contract_Status;
    }

    private orderExpanderLabel: string;

    get productRowsExpanderLabel(): string {

        if (!this.terms || !this.invoice)
            return '';

        let label: string = '';

        // If no rows are hidden (transferred), just display one number
        // Otherwise show both active and visible
        if (this.productRowsRendered) {
            if (this.nbrOfVisibleRows === this.nbrOfActiveRows)
                label = "({0})".format(this.nbrOfActiveRows.toString());
            else
                label = "({0}/{1})".format(this.nbrOfVisibleRows.toString(), this.nbrOfActiveRows.toString());
        }

        if (this.showSalesPricePermission && this.showPurchasePricePermission) {
            if (this.isContract || this.isOffer) {
                label += " {0}: {1} | {2}: {3} | {4}: {5}".format(
                    this.terms["billing.productrows.sumamount"],
                    this.amountFilter(this.invoice.sumAmountCurrency),
                    this.terms["billing.productrows.vatamount"],
                    this.amountFilter(this.invoice.vatAmountCurrency),
                    this.terms["billing.productrows.totalamount"],
                    this.amountFilter(this.invoice.totalAmountCurrency));
            }
            else {
                label += " {0}: {1} | {2}: {3} | {4}: {5} | {6}: {7}".format(
                    this.terms["billing.productrows.sumamount"],
                    this.amountFilter(this.invoice.sumAmountCurrency),
                    this.terms["billing.productrows.vatamount"],
                    this.amountFilter(this.invoice.vatAmountCurrency),
                    this.terms["billing.productrows.totalamount"],
                    this.amountFilter(this.invoice.totalAmountCurrency),
                    this.terms["billing.productrows.remainingamount"],
                    this.amountFilter(this.showRemainingAmountExVat ? this.invoice.remainingAmountExVat : this.invoice.remainingAmount));
            }
        }

        return label;
    }

    private nbrOfVisibleRows: number = 0;
    private nbrOfActiveRows: number = 0;

    //Split button label
    transferButtonSelectedOption: {};

    // Filters
    private amountFilter: any;

    // Functions
    private projectFunctions: any = [];
    private saveFunctions: any = [];
    private transferFunctions: any = [];
    private printFunctions: any = [];

    private edit: ng.IFormController;
    private modalInstance: any;
    private openedFromOrderPlanning = false;

    // Guid to map tabs
    public parentGuid: Guid;

    // Debug
    get isDebug(): boolean {
        return CoreUtility.isDebugMode;
    }

    //@ngInject
    constructor(
        private $uibModal,
        private $filter: ng.IFilterService,
        private $timeout: ng.ITimeoutService,
        private coreService: ICoreService,
        private reportService: IReportService,
        private commonCustomerService: ICommonCustomerService,
        private orderService: IOrderService,
        private productService: IProductService,
        private purchaseService: IPurchaseService,
        private messagingService: IMessagingService,
        private notificationService: INotificationService,
        private translationService: ITranslationService,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private $window: ng.IWindowService,
        private urlHelperService: IUrlHelperService,
        private dirtyHandlerFactory: IDirtyHandlerFactory,
        private scopeWatcherService: IScopeWatcherService,
        shortCutService: IShortCutService,
        progressHandlerFactory: IProgressHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        private readonly requestReportService: IRequestReportService) {

        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.orderIsReadyHelper = new OrderIsReadyHelper(this, this.$uibModal, this.orderService, this.urlHelperService, this.notificationService, this.translationService);
        this.originUserHelper = new OriginUserHelper(this, coreService, urlHelperService, translationService, $q, $uibModal);

        shortCutService.bindSave($scope, () => { this.save(false); });
        shortCutService.bindSaveAndClose($scope, () => { this.save(true); });
        shortCutService.bindEnterAsTab($scope);

        this.feature = soeConfig.feature;
        this.modalInstance = $uibModal;
        this.amountFilter = $filter("amount");

        // Config parameters
        this.currentAccountYearId = soeConfig.accountYearId;
        this.currentAccountYearIsOpen = soeConfig.accountYearIsOpen;
        this.isTemplateRegistration = soeConfig.isTemplateRegistration;

        this.setupListeners();

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.onDoLookups())
            .onLoadData(() => this.  load(false, false, true, this.lockTemplateFlag))
            .onAfterFirstLoad(() => this.onAfterFirstLoad())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));

        this.setTabCallbacks(() => this.onTabActivated(), () => this.onTabDeActivated())
    }

    public $onInit() {
        //Set currency
        var rowContainer = ProductRowsContainers.Order;
        if (this.isOffer)
            rowContainer = ProductRowsContainers.Offer;
        else if (this.isContract)
            rowContainer = ProductRowsContainers.Contract

        this.currencyHelper = new InvoiceCurrencyHelper(rowContainer, this.coreService, this.$q, this.$timeout, () => this.currencyChanged(), () => this.currencyIdChanged());
    }

    public onInit(parameters: any) {
        this.guid = parameters.guid;
        this.invoiceId = parameters.id || 0;
        this.customerId = parameters.customerId ?? Number(soeConfig.customerId ?? 0);
        this.projectId = parameters.projectId ?? Number(soeConfig.projectId ?? 0);
        this.isProjectCentral = parameters.isProjectCentral || false;
        this.isNew = (this.invoiceId == 0);
        let isTemplate = parameters.isTemplate || false;
        this.lockTemplateFlag = isTemplate;
        this.templateOpenFromAgreement = parameters.templateOpenFromAgreement || false;
        this.createOrderTemplateFromAgreement = parameters.createOrderTemplateFromAgreement || false;
        this.templateText = parameters.templateText || "";
        this.parentGuid = parameters.parentGuid || "";
        this.updateTab = parameters.updateTab || false;

        if (parameters.feature)
            this.feature = parameters.feature;

        if (!this.feature) { this.feature = Feature.Billing_Order_Status; }

        if (parameters.ids && parameters.ids.length > 0)
            this.invoiceIds = parameters.ids;
        else
            this.showNavigationButtons = false;

        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.translationService.translate("common.manuallyadded").then(term => {
            this.invoiceFilesHelper = new FilesHelper(this.coreService, this.$q, this.dirtyHandler, true, SoeEntityType.Order, SoeEntityImageType.OrderInvoice, () => this.invoiceId, term);
        });

        this.invoiceEditHandler = new InvoiceEditHandler(this, this.coreService, this.commonCustomerService, this.urlHelperService, this.notificationService, this.translationService, this.reportService, this.$uibModal, this.progress, this.messagingService, this.guid);

        this.startFlow();
    }

    private onDoLookups(): ng.IPromise<any> {
        if (this.isOrder) {
            this.invoiceFilesHelper.entity = SoeEntityType.Order;
            return this.$q.all([
                this.loadTerms(),
                this.loadCompanySettings(),
                this.loadUserSettings(),
                this.loadCompanyAccounts(),
                this.loadCurrentAccountYear(),
                this.invoiceEditHandler.loadCurrencies(),
                this.loadCustomers(),
                this.invoiceEditHandler.loadEmployeeId((employeeId) => this.employeeId = employeeId)]).then(() => {
                    this.includeTimeInReportItems.push({ id: 0, name: this.terms["billing.project.timesheet.includetimeinreport.none"] });
                    this.includeTimeInReportItems.push({ id: 1, name: this.terms["billing.project.timesheet.includetimeinreport.all"] });
                    this.includeTimeInReportItems.push({ id: 2, name: this.terms["billing.project.timesheet.includetimeinreport.invoiced"] });

                    return this.$q.all([
                        this.loadOrderTypes(),
                        this.loadTemplates(),
                        this.loadFixedPriceOrderTypes(),
                        this.invoiceEditHandler.loadVatTypes(),
                        this.loadOurReferences(),
                        this.invoiceEditHandler.loadWholesellers(),
                        this.loadPriceListTypes(),
                        this.invoiceEditHandler.loadDeliveryTypes(),
                        this.invoiceEditHandler.loadDeliveryConditions(),
                        this.invoiceEditHandler.loadPaymentConditions(),
                        this.loadInvoicePaymentServices(),
                        this.invoiceEditHandler.loadDefaultVoucherSeriesId(this.currentAccountYearId),
                        this.invoiceEditHandler.loadEdiTransferModes(),
                        this.loadVoucherSeries(this.currentAccountYearId)]);
                });
        }
        else if (this.isOffer) {
            this.invoiceFilesHelper.entity = SoeEntityType.Offer;
            return this.$q.all([
                this.loadTerms(),
                this.loadCompanySettings(),
                this.loadUserSettings(),
                this.loadCompanyAccounts(),
                this.loadCurrentAccountYear(),
                this.invoiceEditHandler.loadCurrencies(),
                this.loadCustomers(),
                this.invoiceEditHandler.loadEmployeeId((employeeId) => this.employeeId = employeeId)]).then(() => {
                    return this.$q.all([
                        this.loadTemplates(this.isTemplateRegistration),
                        this.loadFixedPriceOrderTypes(),
                        this.invoiceEditHandler.loadVatTypes(),
                        this.loadOurReferences(),
                        this.invoiceEditHandler.loadWholesellers(),
                        this.loadPriceListTypes(),
                        this.invoiceEditHandler.loadDeliveryTypes(),
                        this.invoiceEditHandler.loadDeliveryConditions(),
                        this.invoiceEditHandler.loadPaymentConditions(),
                        this.loadInvoicePaymentServices(),
                        this.invoiceEditHandler.loadDefaultVoucherSeriesId(this.currentAccountYearId),
                        this.loadVoucherSeries(this.currentAccountYearId)]);
                });
        }
        else if (this.isContract) {
            this.invoiceFilesHelper.entity = SoeEntityType.Contract;
            return this.$q.all([
                this.loadTerms(),
                this.loadCompanySettings(),
                this.loadUserSettings(),
                this.loadCompanyAccounts(),
                this.loadCurrentAccountYear(),
                this.invoiceEditHandler.loadCurrencies(),
                this.loadCustomers()]).then(() => {
                    return this.$q.all([
                        this.loadTemplates(this.isTemplateRegistration),
                        this.loadInvoiceDeliveryTypes(),
                        this.loadFixedPriceOrderTypes(),
                        this.invoiceEditHandler.loadVatTypes(),
                        this.loadOurReferences(),
                        this.invoiceEditHandler.loadWholesellers(),
                        this.loadPriceListTypes(),
                        this.invoiceEditHandler.loadDeliveryTypes(),
                        this.invoiceEditHandler.loadDeliveryConditions(),
                        this.invoiceEditHandler.loadPaymentConditions(),
                        this.loadInvoicePaymentServices(),
                        this.invoiceEditHandler.loadDefaultVoucherSeriesId(this.currentAccountYearId),
                        this.loadVoucherSeries(this.currentAccountYearId),
                        this.loadContractGroups()]);
                });
        }
    }

    private onAfterFirstLoad() {
        if (this.isOrder) {
            this.loadTimeProjectLastDate(false);
        }

        this.setupWatchers();
        this.handleExpanderSettings();
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {

        if (this.isOffer) {
            this.modifyPermission = response[Feature.Billing_Offer_Offers_Edit].modifyPermission;
            this.productRowsPermission = response[Feature.Billing_Offer_Offers_Edit_ProductRows].modifyPermission;
            this.accountingRowsPermission = response[Feature.Billing_Offer_Offers_Edit_AccountingRows].modifyPermission;
            this.tracingPermission = response[Feature.Billing_Offer_Offers_Edit_Tracing].modifyPermission;
            this.filesPermission = response[Feature.Billing_Offer_Offers_Edit_Images].modifyPermission;
            this.unlockPermission = response[Feature.Billing_Offer_Offers_Edit_Unlock].modifyPermission;
            this.closePermission = response[Feature.Manage_System].modifyPermission || response[Feature.Billing_Offer_Offers_Edit_Close].modifyPermission;
            this.priceOptimizationPermission = response[Feature.Billing_Price_Optimization].modifyPermission;
        }
        else if (this.isOrder) {
            // Expanders
            this.modifyPermission = response[Feature.Billing_Order_Orders_Edit].modifyPermission;
            this.productRowsPermission = response[Feature.Billing_Order_Orders_Edit_ProductRows].modifyPermission;
            this.timeProjectPermission = response[Feature.Time_Project_Invoice_Edit].modifyPermission;
            this.filesPermission = response[Feature.Billing_Order_Orders_Edit_Images].modifyPermission;
            this.checklistsPermission = response[Feature.Billing_Order_Checklists].modifyPermission;
            this.accountingRowsPermission = response[Feature.Billing_Order_Orders_Edit_AccountingRows].modifyPermission;
            this.orderPlanningPermission = response[Feature.Billing_Order_Planning].modifyPermission || response[Feature.Billing_Order_PlanningUser].modifyPermission;
            if (this.orderPlanningPermission)
                this.loadShiftTypes();
            this.tracingPermission = response[Feature.Billing_Order_Orders_Edit_Tracing].modifyPermission;
            this.expensesPermission = response[Feature.Billing_Order_Orders_Edit_Expenses].modifyPermission;
            this.orderSupplierInvoicesPermission = response[Feature.Billing_Order_SupplierInvoices].modifyPermission;
            this.supplierInvoicesEditPermission = response[Feature.Economy_Supplier_Invoice_Invoices_Edit].modifyPermission;
            this.mainOrderPermission = response[Feature.Billing_Order_Orders_Edit_MainOrder].modifyPermission;
            this.showProjectsWithoutCustomer = response[Feature.Time_Project_Invoice_ShowProjectsWithoutCustomer].modifyPermission;
            this.useDiffWarning = response[Feature.Billing_Order_UseDiffWarning].modifyPermission;
            this.modifyOtherEmployeesPermission = response[Feature.Billing_Project_TimeSheetUser_OtherEmployees].modifyPermission;
            this.createEInvoicePermission = response[Feature.Billing_Invoice_Invoices_Edit_EInvoice].modifyPermission ||
                response[Feature.Billing_Invoice_Invoices_Edit_EInvoice_CreateSvefaktura].modifyPermission ||
                response[Feature.Billing_Invoice_Invoices_Edit_EInvoice_CreateFinvoice].modifyPermission;

            this.purchasePermission = response[Feature.Billing_Purchase].modifyPermission;
            this.directInvoicingPermission = response[Feature.Billing_Order_Orders_Edit_DirectInvoicing].modifyPermission;
            this.priceOptimizationPermission = response[Feature.Billing_Price_Optimization].modifyPermission;

            // Tools
            this.unlockPermission = response[Feature.Billing_Order_Orders_Edit_Unlock].modifyPermission;
            this.closePermission = response[Feature.Manage_System].modifyPermission || response[Feature.Billing_Order_Orders_Edit_Close].modifyPermission;
            this.transferOrderToContract = response[Feature.Billing_Order_Status_OrderToContract].modifyPermission;
            this.orderIsReadyHelper.onlyChangeRowStateIfOwner = response[Feature.Billing_Order_Only_ChangeRowState_IfOwner].modifyPermission;
            this.modifyProjectPermission = response[Feature.Billing_Order_Orders_Edit_Project].modifyPermission;
            this.modifyCustomerPermission = response[Feature.Billing_Order_Orders_Edit_Customer].modifyPermission;
        }
        else if (this.isContract) {
            this.modifyPermission = response[Feature.Billing_Contract_Contracts_Edit].modifyPermission;
            this.productRowsPermission = response[Feature.Billing_Contract_Contracts_Edit_ProductRows].modifyPermission;
            this.filesPermission = response[Feature.Billing_Contract_Contracts_Edit_Images].modifyPermission;
            this.accountingRowsPermission = response[Feature.Billing_Contract_Contracts_Edit_AccountingRows].modifyPermission;
            this.orderPlanningPermission = response[Feature.Billing_Order_Planning].modifyPermission || response[Feature.Billing_Order_PlanningUser].modifyPermission;
            if (this.orderPlanningPermission)
                this.loadShiftTypes();
            this.tracingPermission = response[Feature.Billing_Contract_Contracts_Edit_Tracing].modifyPermission;
            this.categoriesPermission = response[Feature.Common_Categories_Contract].modifyPermission;
            this.changeNextInvoiceDatePermission = response[Feature.Billing_Contract_Contracts_Edit_ChangeInvoiceDate].modifyPermission;
            this.createEInvoicePermission = response[Feature.Billing_Invoice_Invoices_Edit_EInvoice].modifyPermission ||
                response[Feature.Billing_Invoice_Invoices_Edit_EInvoice_CreateSvefaktura].modifyPermission ||
                response[Feature.Billing_Invoice_Invoices_Edit_EInvoice_CreateFinvoice].modifyPermission;
        }

        this.templatesPermission = response[Feature.Billing_Preferences_InvoiceSettings_Templates].modifyPermission;
        this.showSalesPricePermission = response[Feature.Billing_Product_Products_ShowSalesPrice].modifyPermission;
        this.showPurchasePricePermission = response[Feature.Billing_Product_Products_ShowPurchasePrice].modifyPermission;
        this.editCustomerPermission = response[Feature.Billing_Customer_Customers_Edit].modifyPermission;
        this.useCurrency = response[Feature.Economy_Preferences_Currency].modifyPermission;
        this.reportPermission = response[Feature.Billing_Distribution_Reports_Selection].modifyPermission && response[Feature.Billing_Distribution_Reports_Selection_Download].modifyPermission;
    }

    private startFlow() {
        const features: Feature[] = [];

        if (this.isOffer) {
            features.push(Feature.Billing_Offer_Offers_Edit);                                 // Edit permissions
            features.push(Feature.Billing_Offer_Offers_Edit_ProductRows);                     // Product rows
            features.push(Feature.Billing_Offer_Offers_Edit_AccountingRows);                  // Accounting rows
            features.push(Feature.Billing_Offer_Offers_Edit_Tracing);                         // Tracing
            features.push(Feature.Billing_Offer_Offers_Edit_Images);                          // Images and files
            features.push(Feature.Billing_Offer_Offers_Edit_Unlock);
            features.push(Feature.Billing_Offer_Offers_Edit_Close);
            features.push(Feature.Manage_System);                                             // Close support
            features.push(Feature.Billing_Price_Optimization);                                // Transfert to Price Optimization
        }
        else if (this.isOrder) {
            // Expanders
            features.push(Feature.Billing_Order_Orders_Edit);
            features.push(Feature.Billing_Order_Orders_Edit_ProductRows);                     // Product rows
            features.push(Feature.Time_Project_Invoice_Edit);                                 // Time project
            features.push(Feature.Billing_Order_Orders_Edit_Images);                          // Images and files
            features.push(Feature.Billing_Order_Checklists);                                  // Checklists
            features.push(Feature.Billing_Order_Orders_Edit_AccountingRows);                  // Accounting rows
            features.push(Feature.Billing_Order_Planning);                                    // Order planning
            features.push(Feature.Billing_Order_PlanningUser);                                // Order planning
            features.push(Feature.Billing_Order_Orders_Edit_Tracing);                         // Tracing
            features.push(Feature.Billing_Order_Orders_Edit_Expenses);                        // Expenses
            features.push(Feature.Billing_Order_SupplierInvoices);                            // SupplierInvoices
            features.push(Feature.Economy_Supplier_Invoice_Invoices_Edit)                     // Edit supplier invoices
            features.push(Feature.Billing_Order_Orders_Edit_MainOrder)                        // Main order
            features.push(Feature.Billing_Project_TimeSheetUser_OtherEmployees)               // Main order
            features.push(Feature.Billing_Order_Orders_Edit_DirectInvoicing)                  // Bypassing invoice row ready for invoice
            features.push(Feature.Billing_Price_Optimization);                                // Transfert to Price Optimization

            features.push(Feature.Time_Project_Invoice_ShowProjectsWithoutCustomer);          // Show projects with no customer
            features.push(Feature.Billing_Order_UseDiffWarning);
            features.push(Feature.Billing_Invoice_Invoices_Edit_EInvoice);
            features.push(Feature.Billing_Invoice_Invoices_Edit_EInvoice_CreateSvefaktura);
            features.push(Feature.Billing_Invoice_Invoices_Edit_EInvoice_CreateFinvoice);
            features.push(Feature.Billing_Order_Orders_Edit_Project);
            features.push(Feature.Billing_Order_Orders_Edit_Customer);

            // Tools
            features.push(Feature.Billing_Order_Orders_Edit_Unlock);                          // Unlock
            features.push(Feature.Billing_Order_Orders_Edit_Close);                           // Close
            features.push(Feature.Manage_System);                                             // Close support
            features.push(Feature.Billing_Order_Status_OrderToContract);                      // Transfer order to contract
            features.push(Feature.Billing_Order_Only_ChangeRowState_IfOwner);
            features.push(Feature.Billing_Purchase);
        }
        else if (this.isContract) {
            features.push(Feature.Billing_Contract_Contracts_Edit);                           // Edit
            features.push(Feature.Billing_Contract_Contracts_Edit_ProductRows);               // Product rows expander
            features.push(Feature.Billing_Contract_Contracts_Edit_AccountingRows);            // Accounting rows expander
            features.push(Feature.Billing_Contract_Contracts_Edit_Images);                    // Images expander
            features.push(Feature.Billing_Contract_Contracts_Edit_Tracing);                   // Tracing expander
            features.push(Feature.Billing_Contract_Contracts_Edit_ChangeInvoiceDate);
            features.push(Feature.Common_Categories_Contract);                                // Categories expander
            features.push(Feature.Billing_Contract_Contracts_Edit_ProductRows_Copy);          // Copy productrows
            features.push(Feature.Billing_Contract_Contracts_Edit_ProductRows_Merge);         // Merge product rows
            features.push(Feature.Billing_Order_Planning);                                    // Order planning
            features.push(Feature.Billing_Order_PlanningUser);                                // Order planning
            features.push(Feature.Billing_Invoice_Invoices_Edit_EInvoice);
            features.push(Feature.Billing_Invoice_Invoices_Edit_EInvoice_CreateSvefaktura);
            features.push(Feature.Billing_Invoice_Invoices_Edit_EInvoice_CreateFinvoice);
        }

        features.push(Feature.Billing_Preferences_InvoiceSettings_Templates);             // Show templates
        features.push(Feature.Billing_Product_Products_ShowSalesPrice);                   // Show sales price
        features.push(Feature.Billing_Product_Products_ShowPurchasePrice);                   // Show purchase price
        features.push(Feature.Billing_Customer_Customers_Edit);                           // Edit customer
        features.push(Feature.Economy_Preferences_Currency);                              // Use currency
        features.push(Feature.Billing_Distribution_Reports_Selection);                    // Invoice report
        features.push(Feature.Billing_Distribution_Reports_Selection_Download);           // Invoice report

        var permissionRequests = _.map(features, (f) => { return { feature: f, loadModifyPermissions: true } });
        this.flowHandler.start(permissionRequests);
    }

    private setupListeners() {
        this.$scope.$on(Constants.EVENT_ON_INIT_MODAL, (e, parameters) => {
            if (parameters && parameters.sourceGuid === this.guid) {
                return;
            }
            parameters.guid = Guid.newGuid();
            if (parameters.label)
                this.modalTitle = parameters.label;
            this.isModal = true;
            this.onInit(parameters);
            this.modal = parameters.modal;
            if (parameters.orderPlanning)
                this.openedFromOrderPlanning = true;
        });

        //this.$scope.$on(Constants.EVENT_ORDER_PRODUCT_ROWS_COPIED, (e) => {
        //    console.log(Constants.EVENT_ORDER_PRODUCT_ROWS_COPIED);
        //});

        this.messagingService.subscribe(Constants.EVENT_ORDER_PRODUCT_ROWS_COPIED, (parameters) => {
            if (this.invoice.invoiceId == parameters.invoiceId && this.guid === parameters.guid) {
                this.$timeout(() => {
                    this.load();
                });
            }
        }, this.$scope);

        // Events - Quarantine
        this.messagingService.subscribe(Constants.EVENT_REGENERATE_ACCOUNTING_ROWS, (invoiceId) => {
            // Make sure event does not come from any other orders product rows
            if (invoiceId === this.invoiceId)
                this.generateAccountingRows(true);
        }, this.$scope);

        this.messagingService.subscribe(Constants.EVENT_HOUSEHOLD_TAX_DEDUCTION_ADDED, (invoiceId) => {
            // Make sure event does not come from any other orders product rows
            if (invoiceId === this.invoiceId) {
                // Set payment condition from company setting if household tax deduction row is added
                if (this.defaultPaymentConditionHouseholdDeductionId) {
                    this.setPaymentCondition(this.defaultPaymentConditionHouseholdDeductionId);
                }
            }
        }, this.$scope);
        this.messagingService.subscribe(Constants.EVENT_FIXED_PRICE_ADDED, (x) => {
            // Make sure event does not come from any other orders product rows
            if (x.guid === this.guid) {
                // Set order to fixed price.
                this.selectedFixedPriceOrder = this.fixedPriceOrderTypes.find(t => t.id === x.orderType);
            }
        }, this.$scope);
        this.messagingService.subscribe(Constants.EVENT_MANUALLY_DELETED_TIME_PROJECT_ROW, (invoiceId) => {
            // Make sure event does not come from any other orders product rows
            if (invoiceId === this.invoiceId)
                this.invoice.hasManuallyDeletedTimeProjectRows = true;
        }, this.$scope);
        this.messagingService.subscribe(Constants.EVENT_SEARCH_TIME_PROJECT_ROWS, (x) => {
            // Make sure event does not come from any other orders product rows
            if (x.guid === this.guid)
                this.loadTimeProjectRows(x.GetIntervall);
        }, this.$scope);
        this.messagingService.subscribe(Constants.EVENT_RELOAD_INVOICE, (x) => {
            // Make sure event does not come from any other orders time project rows
            if (x.guid === this.guid) {
                this.load();
                this.loadTimeProjectRows();
                this.generateAccountingRows(true);
                this.$scope.$broadcast('productRowsUpdated', null);
            }
        }, this.$scope);
        this.messagingService.subscribe(Constants.EVENT_SAVE_ORDER, (x) => {
            // Make sure event does not come from any other orders time project rowsconso
            if (x.guid === this.guid && this.dirtyHandler.isDirty) {
                if (x.open || x.delete) {
                    this.save(false, null, null, () => { this.$scope.$broadcast('editTimeRow', x); });
                }
                else {
                    this.save(false);
                }
            }
        }, this.$scope)
        this.messagingService.subscribe(Constants.EVENT_VALIDATE_TRANSFER_TO_INVOICE_RESULT, (x) => {
            if (x.guid === this.guid && x.invoiceId === this.invoiceId) {
                this.transferAllRowsAndCloseOrder = x.notTransferable;
                this.transferFixedPriceToInvoiceLeavingOthers = x.fixedPriceLeavingOthers;
                this.transferringContractProducts = x.transferringContractProducts;
                this.hasLiftRowsNotTransferable = x.hasLiftRowsNotTransferable;
                this.createInvoiceWhenOrderReady = x.canCreateInvoice;
                this.hasDeductionAmountMismatch = x.hasDeductionAmountMismatch;

                if (x.performDirectInvoicing) {
                    this.save(false);
                }
                else {
                    this.initTransfer().then(isValid => {
                        if (isValid)
                            this.transfer();
                    });
                }
            }
        }, this.$scope);
        this.messagingService.subscribe(Constants.EVENT_PRODUCTROW_GRID_READY, (parentGuid) => {
            if (parentGuid === this.guid) {
                const fixedPrice = (this.invoice.fixedPriceOrder && this.invoiceId == 0);
                this.$scope.$broadcast('updateCustomer', { customer: this.customer, getFreight: false, getInvoiceFee: !this.invoiceFeeUpdated });
                this.$scope.$broadcast('updateWholesellers', { wholesellers: this.invoiceEditHandler.wholesellers });
                this.$scope.$broadcast('addNewRow', { fixedPrice: fixedPrice });
                if (this.invoiceFeeUpdated) {
                    this.$scope.$broadcast('updateInvoiceFee', { invoiceFeeCurrency: this.invoice.invoiceFeeCurrency });
                    this.invoiceFeeUpdated = false;
                }
                if (this.freightAmountUpdated) {
                    this.$scope.$broadcast('updateFreighAmount', { freightAmountCurrency: this.invoice.freightAmountCurrency });
                    this.freightAmountUpdated = false;
                }
                if (this.vatTypeUpdated) {
                    this.$scope.$broadcast('vatTypeChanged');
                    this.vatTypeUpdated = false;
                    this.executing = false;
                }
                if (this.validateProductRowsForTransfer) {
                    // Do some validations in product rows
                    // Result will be returned in a suvbscription that will in turn call initTransfer()
                    this.transferAllRowsAndCloseOrder = false;
                    this.$scope.$broadcast('validateTransferToInvoice', { guid: this.guid, directInvoicing: this.performDirectInvoicing });
                    this.validateProductRowsForTransfer = false;
                }
            }
            if (this.copyProductRows) {
                this.$scope.$broadcast('copyRows', { guid: this.guid, checkRecalculate: this.checkRecalculatePrices });
                this.copyProductRows = false;

                this.setLocked();
                this.setAsDirty(true);
                this.messagingService.publish(Constants.EVENT_EDIT_NEW, { guid: this.guid });
            }

            if (this.updateInternalIdCounter && this.invoice && this.invoice.customerInvoiceRows) {
                this.$scope.$broadcast('updateInternalIdCounter', { numberOfRows: this.invoice.customerInvoiceRows.length });
            }
            this.productRowsRenderFinalized = true;
        }, this.$scope);
        this.messagingService.subscribe(Constants.EVENT_RELOAD_ORDER_IMAGES, (x) => {
            if (x.guid === this.guid && this.invoiceFilesHelper.filesRendered) {
                this.invoiceFilesHelper.loadFiles(true, this.invoice && this.invoice.projectId ? this.invoice.projectId : 0);
            }
        }, this.$scope);
        this.messagingService.subscribe(Constants.EVENT_UPDATE_TRANSFER_ALL, (x) => {
            if (x.guid === this.guid && this.invoiceFilesHelper.filesRendered) {
                this.invoiceFilesHelper.changeTransferBatch(x.value);
                this.resetDocumentsGridData = true;
            }
        }, this.$scope);
        this.messagingService.subscribe(Constants.EVENT_UPDATE_DISTRIBUTE_ALL, (x) => {
            if (x.guid === this.guid && this.invoiceFilesHelper.filesRendered) {
                this.invoiceFilesHelper.changeDistributeBatch(x.value);
                this.resetDocumentsGridData = true;
            }
        }, this.$scope);
        this.messagingService.subscribe(Constants.EVENT_PAUSE_AUTOSAVE, (x) => {
            if (x.guid === this.guid) {
                this.pauseAutoSaveTimer();
            }
        }, this.$scope);
        this.messagingService.subscribe(Constants.EVENT_AGREEMENT_RELOAD_ORDERTEMPLATES, (x) => {
            if (x.guid === this.guid)
                this.loadTemplates(false, true, x.resetId);
        }, this.$scope);
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => this.copy());

        const statusGroup = ToolBarUtility.createGroup();
        let householdLabel = "billing.order.hashousehold";
        let textLabel = "common.customer.invoices.editordertext";
        if (this.isContract) {
            householdLabel = "billing.contract.hashousehold";
            textLabel = "common.customer.contract.editcontracttext";
        }
        else if (this.isOffer) {
            householdLabel = "billing.offer.hashousehold";
            textLabel = "billing.offer.editoffertext";
        }
        statusGroup.buttons.push(new ToolBarButton("", householdLabel, IconLibrary.FontAwesome, "fa-home textColor", () => {
            this.notificationService.showDialogEx(this.terms["core.info"], this.terms["billing.order.hashousehold"], SOEMessageBoxImage.Information);
        }, null, () => {
            return !this.hasHouseholdTaxDeduction;
        }));
        statusGroup.buttons.push(new ToolBarButton("", "billing.order.isfixedprice", IconLibrary.FontAwesome, "fa-money-bill-alt textColor", () => {
            this.notificationService.showDialogEx(this.terms["core.info"], this.terms["billing.order.isfixedprice"], SOEMessageBoxImage.Information);
        }, null, () => {
            return !this.invoice.fixedPriceOrder;
        }));

        statusGroup.buttons.push(new ToolBarButton("", "common.accordionsettings", IconLibrary.FontAwesome, "fa-cog", () => {
            this.updateAccordionSettings();
        }, null, () => {
            return false;
        }));

        statusGroup.buttons.push(new ToolBarButton("", textLabel, IconLibrary.FontAwesome, "fa-file-alt textColor", () => {
            this.editOrderText();
        }, () => {
            return this.isLocked;
        }, () => {
            return !this.invoice;
        }));

        if (this.isOrder) {
            statusGroup.buttons.push(new ToolBarButton("", "billing.order.showordersummary", IconLibrary.FontAwesome, "fa-info-circle textColor", () => {
                this.viewOrderInformation();
            }, null, () => {
                return this.isNew;
            }));

            statusGroup.buttons.push(new ToolBarButton("", "common.customer.invoices.openorder", IconLibrary.FontAwesome, "fa-unlock-alt textColor", () => {
                this.unlockOrder();
            }, null, () => {
                return !this.showUnlockButton;
            }));

            statusGroup.buttons.push(new ToolBarButton("", "common.customer.invoices.closeorder", IconLibrary.FontAwesome, "fa-lock-alt textColor", () => {
                const modal = this.notificationService.showDialog(this.terms["core.warning"], this.terms["common.customer.invoices.closeorderwarning"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                modal.result.then(val => {
                    this.closeOrder();
                });
            }, null, () => {
                return !this.showCloseButton;
            }));

            this.orderIsReadyHelper.addToolBarButtons(this.toolbar);
        }
        else if (this.isOffer) {
            statusGroup.buttons.push(new ToolBarButton("", "common.customer.invoices.openoffer", IconLibrary.FontAwesome, "fa-unlock-alt textColor", () => {
                this.unlockOffer();
            }, null, () => {
                return !this.showUnlockButton;
            }));

            statusGroup.buttons.push(new ToolBarButton("", "common.customer.invoices.closeoffer", IconLibrary.FontAwesome, "fa-lock-alt textColor", () => {
                this.closeOffer();
            }, null, () => {
                return !this.showCloseButton;
            }));
        }
        else if (this.isContract) {
            statusGroup.buttons.push(new ToolBarButton("", "billing.contract.unlockcontractinfo", IconLibrary.FontAwesome, "fa-unlock-alt textColor", () => {
                this.contractHeadIsLocked = !this.contractHeadIsLocked;
            }, null, () => {
                return !this.contractHeadIsLocked || this.isLocked || (this.invoice && (this.invoice.originStatus === SoeOriginStatus.ContractClosed || this.invoice.originStatus === SoeOriginStatus.Cancel));
            }));
        }

        statusGroup.buttons.push(new ToolBarButton("", this.isOffer ? "billing.offer.reloadoffer" : "common.customer.invoices.reloadorder", IconLibrary.FontAwesome, "fa-sync", () => {
            this.load(false, false, true);
            this.reloadTracingData();
        }, null, () => {
            return false;
        }));

        this.toolbar.addButtonGroup(statusGroup);

        // Navigation
        this.toolbar.setupNavigationGroup(() => { return this.isNew }, null, (newInvoiceId) => {
            this.supplierInvoicesRowsRendered = false;
            this.supplierInvoiceExpanderIsOpen = false;
            this.planningExpanderIsOpen = false;
            this.plannedShifts = undefined;
            this.invoiceId = newInvoiceId;
            this.load(true);
        }, this.invoiceIds, this.invoiceId);

        // Functions
        const keys: string[] = [
            "billing.order.project.create",
            "billing.order.project.link",
            "billing.order.transfer.preliminary",
            "billing.order.transfer.definitive",
            "core.save",
            "core.saveandclose",
            "common.report.report.print",
            "common.email",
            "common.report.report.reports",
            "billing.offer.transfertoorder",
            "billing.priceoptimization.transfertopriceoptimization",
            "billing.order.transfertoinvoice",
            "billing.order.transfertoinvoicecash",
            "billing.contract.createserviceorder",
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.projectFunctions = [];
            this.saveFunctions = [];
            this.transferFunctions = [];
            this.printFunctions = [];

            this.saveFunctions.push({ id: OrderEditSaveFunctions.Save, name: terms["core.save"] + " (Ctrl+S)", icon: 'fal fa-fw fa-save' });
            this.saveFunctions.push({ id: OrderEditSaveFunctions.SaveAndClose, name: terms["core.saveandclose"] + " (Ctrl+Enter)", icon: 'fal fa-fw fa-save' });

            this.printFunctions.push({ id: OrderInvoiceEditPrintFunctions.Print, name: terms["common.report.report.print"], icon: 'fal fa-fw fa-print' });
            this.printFunctions.push({ id: OrderInvoiceEditPrintFunctions.eMail, name: terms["common.email"], icon: 'fal fa-fw fa-envelope' });
            this.printFunctions.push({ id: OrderInvoiceEditPrintFunctions.ReportDialog, name: terms["common.report.report.reports"], icon: 'fal fa-fw fa-print' });

            if (this.isOrder) {
                this.projectFunctions.push({ id: OrderEditProjectFunctions.Create, name: terms["billing.order.project.create"], icon: 'fal fa-fw fa-plus' });
                this.projectFunctions.push({ id: OrderEditProjectFunctions.Link, name: terms["billing.order.project.link"], icon: 'fal fa-fw fa-link' });
                if (this.directInvoicingPermission) {
                    this.transferFunctions.push({ id: OrderEditTransferFunctions.DirectTransferToInvoice, name: this.useCashSales ? terms["billing.order.transfertoinvoicecash"] : terms["billing.order.transfertoinvoice"], disabled: () => { return !this.productRowsRenderFinalized || this['edit'].$invalid } });
                }
                else {
                    this.transferFunctions.push({ id: OrderEditTransferFunctions.TransferToPreliminaryInvoice, name: terms["billing.order.transfer.preliminary"] });
                    this.transferFunctions.push({ id: OrderEditTransferFunctions.TransferToDefinitiveInvoice, name: terms["billing.order.transfer.definitive"], icon: "" });
                }
                if (this.priceOptimizationPermission) {
                    this.transferFunctions.push({ id: OrderEditTransferFunctions.TransferToPriceOptimization, name: terms["billing.priceoptimization.transfertopriceoptimization"] });
                }
            }
            else if (this.isOffer) {
                this.transferFunctions.push({ id: OrderEditTransferFunctions.TransferToOrder, name: terms["billing.offer.transfertoorder"] });
                this.transferFunctions.push({ id: OrderEditTransferFunctions.TransferToPreliminaryInvoice, name: terms["billing.order.transfer.preliminary"] });
                this.transferFunctions.push({ id: OrderEditTransferFunctions.TransferToDefinitiveInvoice, name: terms["billing.order.transfer.definitive"], icon: "" });
                if (this.priceOptimizationPermission) {
                    this.transferFunctions.push({ id: OrderEditTransferFunctions.TransferToPriceOptimization, name: terms["billing.priceoptimization.transfertopriceoptimization"] });
                }
            }
            else if (this.isContract) {
                this.transferFunctions.push({ id: OrderEditTransferFunctions.TransferToServiceOrderFromAgreement, name: terms["billing.contract.createserviceorder"] });
                this.transferFunctions.push({ id: OrderEditTransferFunctions.TransferToOrder, name: terms["billing.offer.transfertoorder"] });
                this.transferFunctions.push({ id: OrderEditTransferFunctions.TransferToPreliminaryInvoice, name: terms["billing.order.transfer.preliminary"] });
                this.transferFunctions.push({ id: OrderEditTransferFunctions.TransferToDefinitiveInvoice, name: terms["billing.order.transfer.definitive"], icon: "" });
            }

            //Set default
            this.$timeout(() => {
                this.transferButtonSelectedOption = this.transferFunctions[0];
            });
        });
    }

    private reloadTracingData() {
        if (this.traceRowsRendered) {
            this.$scope.$broadcast('reloadTracingData', { guid: this.guid });
        }
    }

    private setupWatchers() {
        this.watchUnRegisterCallbacks.push(
            // Convert currency amounts
            this.$scope.$watch(() => this.invoice.totalAmountCurrency, () => {
                this.convertAmount('totalAmount', this.invoice.totalAmountCurrency);
            }),
            this.$scope.$watch(() => this.invoice.vatAmountCurrency, () => {
                this.convertAmount('vatAmount', this.invoice.vatAmountCurrency);
            }),
            this.$scope.$watch(() => this.invoice.invoiceFeeCurrency, (newValue, oldValue) => {
                if (!this.productRowsRendered && newValue !== oldValue) {
                    this.invoiceFeeUpdated = true;
                    this.setProductRowsRendered();
                }
            }),
            this.$scope.$watch(() => this.invoice.freightAmountCurrency, (newValue, oldValue) => {
                if (!this.productRowsRendered && newValue !== oldValue) {
                    this.freightAmountUpdated = true;
                    this.setProductRowsRendered();
                }
            }),
            this.$scope.$watch(() => this.invoice.addAttachementsToEInvoice, () => {
                this.invoiceFilesHelper.addAttachementsToEInvoice = this.invoice.addAttachementsToEInvoice;
            }),
            this.$scope.$watch(() => this.invoice.addSupplierInvoicesToEInvoice, () => {
                this.invoiceFilesHelper.addSupplierInvoicesToEInvoice = this.invoice.addSupplierInvoicesToEInvoice;
            }));
    }

    private handleExpanderSettings() {
        if (this.isOffer) {
            if (this.expanderSettings) {
                const settings = this.expanderSettings.split(";");

                if (_.includes(settings, 'ProductRowsExpander'))
                    this.openProductRowExpander();

                this.orderExpanderIsOpen = this.isNew || _.includes(settings, 'OfferExpander');
                this.orderOrderExpanderIsOpen = _.includes(settings, 'OfferOfferExpander');
                this.orderConditionExpanderIsOpen = _.includes(settings, 'OfferConditionExpander');
                this.documentExpanderIsOpen = _.includes(settings, 'DocumentExpander');
                this.accountingRowsExpanderIsOpen = _.includes(settings, 'AccountingRowExpander');
                this.traceRowsExpanderIsOpen = _.includes(settings, 'TracingExpander');
            }
        }
        else if (this.isOrder) {
            if (this.expanderSettings) {
                const settings = this.expanderSettings.split(";");

                if (_.includes(settings, 'ProductRowsExpander') && !this.openedFromOrderPlanning)
                    this.openProductRowExpander();

                this.orderExpanderIsOpen = this.isNew || _.includes(settings, 'OrderExpander');
                this.orderOrderExpanderIsOpen = !this.openedFromOrderPlanning && _.includes(settings, 'OrderOrderExpander');
                this.orderConditionExpanderIsOpen = !this.openedFromOrderPlanning && _.includes(settings, 'OrderConditionExpander');
                this.documentExpanderIsOpen = !this.openedFromOrderPlanning && _.includes(settings, 'DocumentExpander');
                this.planningExpanderIsOpen = this.openedFromOrderPlanning || _.includes(settings, 'PlanningExpander');
                this.timeProjectRowsExpanderIsOpen = !this.openedFromOrderPlanning && _.includes(settings, 'TimeRowExpander');
                this.checklistExpanderIsOpen = !this.openedFromOrderPlanning && _.includes(settings, 'ChecklistExpander');
                this.accountingRowsExpanderIsOpen = !this.openedFromOrderPlanning && _.includes(settings, 'AccountingRowExpander');
                this.traceRowsExpanderIsOpen = !this.openedFromOrderPlanning && _.includes(settings, 'TracingExpander');
                this.expensesExpanderIsOpen = !this.openedFromOrderPlanning && _.includes(settings, 'ExpensesExpander');
                this.supplierInvoiceExpanderIsOpen = !this.openedFromOrderPlanning && _.includes(settings, 'SupplierInvoicesExpander');
            } else {
                if (this.openedFromOrderPlanning) {
                    this.productRowsExpanderIsOpen = false;
                    this.planningExpanderIsOpen = true;
                }
            }
        }
        else if (this.isContract) {
            if (this.expanderSettings) {
                const settings = this.expanderSettings.split(";");

                if (_.includes(settings, 'ProductRowsExpander') && !this.openedFromOrderPlanning)
                    this.openProductRowExpander();

                this.noteExpanderIsOpen = this.isNew || _.includes(settings, 'NoteExpander');
                this.orderExpanderIsOpen = this.isNew || _.includes(settings, 'ContractExpander');
                this.orderOrderExpanderIsOpen = !this.openedFromOrderPlanning && _.includes(settings, 'ContractContractExpander');
                this.orderConditionExpanderIsOpen = !this.openedFromOrderPlanning && _.includes(settings, 'ContractConditionExpander');
                this.documentExpanderIsOpen = !this.openedFromOrderPlanning && _.includes(settings, 'DocumentExpander');
                this.planningExpanderIsOpen = this.openedFromOrderPlanning || _.includes(settings, 'PlanningExpander');
                this.accountingRowsExpanderIsOpen = !this.openedFromOrderPlanning && _.includes(settings, 'AccountingRowExpander');
                this.traceRowsExpanderIsOpen = !this.openedFromOrderPlanning && _.includes(settings, 'TracingExpander');
            }
        }
    }

    private onTabActivated() {
        if (this.invoice && this.watchUnRegisterCallbacks.length == 0) {
            this.setupWatchers()
        }

        this.scopeWatcherService.resumeWatchers(this.$scope);
    }

    private onTabDeActivated() {
        this.flowHandler.starting().finally(() => {
            if (this.isTabActivated) {
                return;
            }

            this.traceRowsRendered = false;
            this.traceRowsExpanderIsOpen = false;

            this.scopeWatcherService.suspendWatchers(this.$scope);
        });
    }

    // LOOKUPS

    private load(updateTab = false, loadingTemplate = false, loadRows = true, newAsTemplate = false): ng.IPromise<any> {
        const deferral = this.$q.defer();
        this.invoiceIsLoaded = false;
        if (this.invoiceId > 0) {

            this.loadingInvoice = true;

            let currentInvoiceRows: any;
            if (!loadRows && this.invoice) {
                currentInvoiceRows = this.invoice.customerInvoiceRows;
            }
            this.orderService.getOrder(this.invoiceId, true, loadRows).then((x) => {

                // Keep old invoicelabel
                const tempInvoiceLabel = this.invoice ? this.invoice.invoiceLabel : null;

                this.invoice = new OrderDTO();
                angular.extend(this.invoice, x);

                //Fix invoiceLabel
                if (loadingTemplate) {
                    this.invoice.invoiceLabel = this.invoice.invoiceLabel ? this.invoice.invoiceLabel : tempInvoiceLabel;
                }

                //Fix vat type
                if (this.invoice.vatType === TermGroup_InvoiceVatType.EU || this.invoice.vatType === TermGroup_InvoiceVatType.NonEU)
                    this.invoiceEditHandler.addMissingVatType(this.invoice.vatType);

                // Fix dates
                this.invoice.orderDate = CalendarUtility.convertToDate(this.invoice.orderDate);
                this.invoice.invoiceDate = CalendarUtility.convertToDate(this.invoice.invoiceDate);
                this.invoice.deliveryDate = CalendarUtility.convertToDate(this.invoice.deliveryDate);
                this.invoice.dueDate = CalendarUtility.convertToDate(this.invoice.dueDate);
                this.invoice.voucherDate = CalendarUtility.convertToDate(this.invoice.voucherDate);
                this.invoice.plannedStartDate = CalendarUtility.convertToDate(this.invoice.plannedStartDate);
                this.invoice.plannedStopDate = CalendarUtility.convertToDate(this.invoice.plannedStopDate);
                this.invoice.nextContractPeriodDate = CalendarUtility.convertToDate(this.invoice.nextContractPeriodDate);
                this.invoice.currencyDate = CalendarUtility.convertToDate(this.invoice.currencyDate);

                this.isNew = loadingTemplate;
                let tempRowId: number = this.invoice.customerInvoiceRows.length;
                if (loadingTemplate) {
                    this.invoice.isTemplate = false;
                    this.invoice.invoiceId = this.invoiceId = 0;
                    this.invoice.invoiceNr = "";
                    this.invoice.originStatus = this.isContract ? SoeOriginStatus.Origin : SoeOriginStatus.None;
                    if (this.customer) {
                        this.invoice.actorId = this.customer.actorCustomerId;
                    }

                    // Order date check
                    if (!this.invoice.invoiceDate)
                        this.selectedInvoiceDate = CalendarUtility.getDateToday();

                    tempRowId = 1;
                    var temp: any[] = [];
                    _.forEach(this.invoice.customerInvoiceRows, (o) => {
                        if (o.type !== SoeInvoiceRowType.AccountingRow) {
                            o.customerInvoiceRowId = 0;
                            o.tempRowId = tempRowId;
                            tempRowId = tempRowId + 1;
                            temp.push(o);
                        }
                    });
                    this.invoice.customerInvoiceRows = temp;
                }
                else if (this.invoice.isTemplate) {
                    if (this.invoice.printTimeReport && this.invoice.includeOnlyInvoicedTime)
                        this.includeTimeInReport = 2;
                    else if (this.invoice.printTimeReport)
                        this.includeTimeInReport = 1;
                    else
                        this.includeTimeInReport = 0;
                }

                // Update row counter
                if (this.productRowsRendered) {
                    this.$scope.$broadcast('updateInternalIdCounter', { numberOfRows: tempRowId });
                }
                else
                    this.updateInternalIdCounter = true;

                // Change customer name
                if (!StringUtility.isEmpty(this.invoice.customerName) && this.invoice.actorId && this.invoice.actorId > 0) {
                    const customer = _.find(this.customers, c => c.id === this.invoice.actorId);
                    if (customer)
                        customer.name = customer.number + " " + this.invoice.customerName;
                }

                this.selectedCustomer = _.find(this.customers, c => c.id === this.invoice.actorId);

                //Customer might be inactive, trigger customer load anyways.
                if (!this.selectedCustomer) {
                    this.selectedCustomer = {
                        id: this.invoice.actorId,
                        name: "",
                    }
                }

                this.loadingCustomer = true;
                if (this.invoice.deliveryCustomerId) {
                    this.loadingDeliveryCustomer = true;

                    //prevent set method kicking off
                    let selectedDeliveryCustomer = _.find(this.customers, c => c.id === this.invoice.deliveryCustomerId);
                    if (selectedDeliveryCustomer) this.selectedDeliveryCustomer = selectedDeliveryCustomer;

                    //Customer might be inactive, trigger customer load anyways.
                    if (!this.selectedDeliveryCustomer) {
                        this.selectedDeliveryCustomer = {
                            id: this.invoice.deliveryCustomerId,
                            name: "",
                        }
                    }
                }

                this._selectedFixedPriceOrder = _.find(this.fixedPriceOrderTypes, f => f.id === (this.invoice.fixedPriceOrder ? OrderContractType.Fixed : OrderContractType.Continuous));
                if (this._selectedFixedPriceOrder && this._selectedFixedPriceOrder.id === OrderContractType.Fixed)
                    this.fixedPrice = true;
                if (this.invoice.shiftTypeId) {
                    this.previousShiftTypeId = this.invoice.shiftTypeId;
                    this.selectedShiftType = _.find(this.shiftTypes, s => s.shiftTypeId === this.invoice.shiftTypeId);
                } else {
                    this.selectedShiftType = undefined;
                }
                this.selectedPriceListType = _.find(this.priceListTypes, { priceListTypeId: this.invoice.priceListTypeId });

                if (this.invoice.orderInvoiceTemplateId)
                    this.selectedOrderInvoiceTemplate = _.find(this.orderTemplates, s => s.id === this.invoice.orderInvoiceTemplateId);

                this._selecedInvoiceDate = this.invoice.invoiceDate;
                this._selectedVoucherDate = this.invoice.voucherDate;
                this.currencyHelper.currencyDate = this.invoice.currencyDate;

                this.currencyHelper.fromInvoice(this.invoice);

                this.invoiceFilesHelper.addAttachementsToEInvoice = this.invoice.addAttachementsToEInvoice;

                this.invoiceFilesHelper.addSupplierInvoicesToEInvoice = this.invoice.addSupplierInvoicesToEInvoice;

                this.originalCustomerId = this.invoice.actorId;

                if (loadRows) {
                    this.invoice.customerInvoiceRows = _.sortBy(this.invoice.customerInvoiceRows, 'rowNr').map(r => {
                        var obj = new ProductRowDTO();
                        angular.extend(obj, r);
                        return obj;
                    });

                    if (this.purchasePermission && this.invoice.customerInvoiceRows && this.invoice.customerInvoiceRows.length > 0) {
                        this.purchaseService.getPurchaseRowsForOrder(this.invoiceId).then((rows: any[]) => {
                            rows.forEach((purchaseRow) => {
                                const orderRow = this.invoice.customerInvoiceRows.find((r) => purchaseRow.customerInvoiceRowId === r.customerInvoiceRowId);
                                if (orderRow) {
                                    orderRow.purchaseStatus = purchaseRow.statusName;
                                    orderRow.purchaseNr = purchaseRow.purchaseNr;
                                    orderRow.purchaseId = purchaseRow.purchaseId;
                                }
                            });

                            this.$scope.$broadcast('refreshRows');
                        });
                    }

                    this.hasHouseholdTaxDeduction = _.some(this.invoice.customerInvoiceRows, r => r.isHouseholdRow);
                }
                else {
                    this.invoice.customerInvoiceRows = currentInvoiceRows;
                }

                this.invoiceFilesHelper.reset();
                if (this.invoiceEditHandler.containsAttachments(this.invoice.statusIcon))
                    this.invoiceFilesHelper.nbrOfFiles = '*';


                // Save original to be able to compare when saving
                //this.originalInvoice = Util.CoreUtility.cloneDTO(this.invoice);
                this.originalInvoice = new OrderDTO();
                angular.extend(this.originalInvoice, CoreUtility.cloneDTO(this.invoice));

                //Origin users
                this.orderIsReadyHelper.invoicedLoaded(this.invoice.originUsers);

                this.originUserHelper.setOriginUsers(this.invoice.originUsers);


                this.hasModifiedProductRows = false;
                if (loadingTemplate) {
                    this.setAsDirty(true);
                }
                else {
                    this.setAsDirty(false);
                }
                this.setLocked();

                if (x.customerBlockNote) {
                    this.invoiceEditHandler.showBlockNote(x.customerBlockNote);
                }

                if ((updateTab) || (this.isProjectCentral) || (this.templateOpenFromAgreement && !this.createOrderTemplateFromAgreement))
                    this.updateTabCaption();

                if (this.createdInvoices != null && ((this.askOpenInvoiceWhenCreateInvoiceFromOrder && this.transferingToInvoice) || this.performCreateServiceOrder)) {
                    this.showAskOpenInvoiceDialog();
                    this.transferingToInvoice = false;
                }

                if (this.invoice.contractGroupId)
                    this.contractGroupChanged(this.invoice.contractGroupId);

                if (!this.invoice.actorId && !this.invoice.deliveryCustomerId)
                    this.invoiceLoaded();

                if (this.isContract) {
                    //Set contract head locked
                    this.contractHeadIsLocked = true;
                    this.prevContractGroupId = this.invoice.contractGroupId;
                    this.selectedNextInvoiceDate = this.invoice.nextContractPeriodDate;
                }

                // If user has opened the accounting rows expander, reload them
                if (this.accountingRowsExpanderIsOpen)
                    this.loadAccountRows();

                // If user has opened the time project rows expander, reload them
                if (this.timeProjectRowsExpanderIsOpen)
                    this.loadTimeProjectRows();

                if (this.documentExpanderIsOpen) {
                    this.invoiceFilesHelper.loadFiles(true, this.invoice && this.invoice.projectId ? this.invoice.projectId : 0);
                    this.resetDocumentsGridData = true;
                }

                if (this.createOrderTemplateFromAgreement)
                    this.copy();

                deferral.resolve();
            });

            //Close expanders in order to reload whats necessary when opened
            //this.timeProjectRowsExpanderIsOpen = false;

            //Reload checklists - not needed since reload is done when recordid changes
            /*if (this.checklistsLoaded || loadingTemplate) {
                this.checklistsLoaded = false;
                this.$scope.$broadcast('reloadChecklists', { copy: loadingTemplate, id: this.invoiceId });
            }*/

            if (updateTab)
                this.orderExpanderInitiallyOpened = true;
        }
        else {
            this.new(newAsTemplate);
            deferral.resolve();
            this.invoiceLoaded();
        }

        return deferral.promise;
    }

    private invoiceLoaded() {
        this.$timeout(() => {
            //TL: Added timeout to put behind trailing events which should be blocked when the order is not loaded.
            this.loadingInvoice = false;
            this.invoiceIsLoaded = true;
            this.setOrderExpanderLabel();
        }, 0);
    }

    private loadAccountRows() {
        //Set rendered
        this.accountingRowsRendered = true;

        if (!this.accountRows || this.accountRows.length === 0) {
            this.orderService.getAccountRows(this.invoiceId).then(rows => {
                this.accountRows = _.sortBy(rows, 'rowNr').map(dto => {
                    var obj = new CustomerInvoiceAccountRowDTO();
                    angular.extend(obj, dto);
                    return obj;
                });
            });
        }
    }

    private setOrderExpanderLabel() {
        if (!this.terms || !this.invoice)
            return '';

        if (this.isOrder) {
            var type = _.find(this.orderTypes, { id: this.invoice.orderType });
            var typeName: string = type ? type.name : this.terms["billing.order.notspecified"];
            var invoiceNr: string = this.invoice.invoiceNr ? this.invoice.invoiceNr : ' ';
            var customerName: string = this.customer ? this.customer.name : this.terms["billing.order.notspecified"];
            var statusName: string = this.invoice.originStatusName ? this.invoice.originStatusName : ' ';
            var projectNr: string = this.invoice.projectNr ? this.invoice.projectNr : this.terms["billing.order.noproject"];

            var label: string = "{0} {1} | {2}: {3} | {4}: {5}".format(
                typeName,
                invoiceNr,
                this.terms["common.customer.customer.customer"],
                customerName.toEllipsisString(50),
                this.terms["billing.order.status"],
                statusName);

            if (this.isOrderTypeUnspecified || this.isOrderTypeProject)
                label += " | {0}: {1} ".format(this.terms["billing.project.projectnr"], projectNr);
        }
        else if (this.isOffer) {
            var invoiceNr: string = this.invoice.invoiceNr ? this.invoice.invoiceNr : ' ';
            var customerName: string = this.customer ? this.customer.name : this.terms["billing.order.notspecified"];
            var statusName: string = this.invoice.originStatusName ? this.invoice.originStatusName : ' ';

            var label: string = "{0} | {1}: {2} | {3}: {4}".format(
                invoiceNr,
                this.terms["common.customer.customer.customer"],
                customerName.toEllipsisString(50),
                this.terms["billing.order.status"],
                statusName);
        }
        else if (this.isContract) {
            var invoiceNr: string = this.invoice.invoiceNr ? this.invoice.invoiceNr : ' ';
            var customerName: string = this.customer ? this.customer.name : this.terms["billing.order.notspecified"];
            var statusName: string = this.invoice.originStatusName ? this.invoice.originStatusName : ' ';

            var label: string = "{0} | {1}: {2} | {3}: {4}".format(
                invoiceNr,
                this.terms["common.customer.customer.customer"],
                customerName.toEllipsisString(50),
                this.terms["billing.order.status"],
                statusName);
        }

        this.orderExpanderLabel = label;
    }

    private loadTimeProjectLastDate(loadRows: boolean) {
        if (this.invoice.projectId && this.invoice.invoiceId) {
            this.orderService.getProjectTimeBlocksLastDate(this.invoice.projectId, this.invoice.invoiceId, this.recordType, this.employeeId, false).then(date => {
                date = CalendarUtility.convertToDate(date);
                this.timeProjectFrom = date.beginningOfWeek();
                this.timeProjectTo = date.endOfWeek();
                if (loadRows)
                    this.loadTimeProjectRows();
            });
        }
    }

    private loadTimeProjectRowsTimeout: any;
    private loadTimeProjectRows(getIntervall: boolean = true) {

        //Set rendered
        this.timeProjectRendered = true;

        var tempProjectFrom: Date;
        var tempProjectTo: Date;

        if (getIntervall) {
            if (!this.timeProjectFrom && !this.timeProjectTo) {
                this.initTimeProjectDates();
            }
            tempProjectFrom = this.timeProjectFrom;
            tempProjectTo = this.timeProjectTo;
        }

        if (!this.invoice || !this.invoice.projectId || !this.invoice.invoiceId)
            return;

        if (this.loadTimeProjectRowsTimeout)
            this.$timeout.cancel(this.loadTimeProjectRowsTimeout);

        this.loadTimeProjectRowsTimeout = this.$timeout(() => {
            this.loadingTimeProjectRows = true;
            this.orderService.getProjectTimeBlocks(this.invoice.projectId, this.invoice.invoiceId, this.recordType, this.employeeId, !this.modifyOtherEmployeesPermission, this.invoice.vatType, tempProjectFrom, tempProjectTo).then(rows => {
                this.projectTimeBlockRows = rows.map(dto => {
                    var obj = new ProjectTimeBlockDTO();
                    angular.extend(obj, dto);
                    obj.date = CalendarUtility.convertToDate(obj.date);
                    if (obj.startTime)
                        obj.startTime = CalendarUtility.convertToDate(obj.startTime);
                    if (obj.stopTime)
                        obj.stopTime = CalendarUtility.convertToDate(obj.stopTime);
                    return obj;
                });

                if (!getIntervall) {
                    this.setMinMaxDates(this.projectTimeBlockRows);
                }

                this.loadingTimeProjectRows = false;
            });
        }, 500);
    }

    private loadExpensesRows() {
        //Set rendered
        this.expensesRendered = true;
    }

    private setMinMaxDates(projectTimeBlockRows: ProjectTimeBlockDTO[]) {
        var dates = projectTimeBlockRows.map(d => d.date);
        if (dates.length > 0) {
            this.timeProjectTo = new Date(Math.max.apply(null, dates));
            this.timeProjectFrom = new Date(Math.min.apply(null, dates));
        }
    }

    private loadTerms(): ng.IPromise<any> {

        const keys: string[] = [
            "core.info",
            "core.verifyquestion",
            "core.notallowed",
            "core.open",
            "core.no",
            "core.openandprint",
            "common.customer.customer.customer",
            "common.customerinvoice",
            "billing.order.status",
            "billing.project.projectnr",
            "billing.order.notspecified",
            "billing.order.noproject",
            "billing.order.hashousehold",
            "billing.order.isfixedprice",
            "billing.productrows.sumamount",
            "billing.productrows.vatamount",
            "billing.productrows.totalamount",
            "billing.productrows.remainingamount",
            "billing.invoices.checklist.hasmandatorysingle",
            "billing.invoices.checklists.hasmandatorymany",
            "common.customer.invoices.closeorderwarning",
            "common.customer.invoices.orderunlockstatusfailed",
            "common.customer.invoices.orderunlockfailed",
            "common.customer.invoices.orderclosefailed",
            "common.customer.invoices.editordertext",
            "common.customer.invoices.yourreference",
            "common.customer.invoices.reloadorder",
            "billing.invoices.checklist.notready",
            "billing.invoices.checklist.attestnotvalid",
            "billing.order.transfered",
            "billing.offer.transfered",
            "billing.contract.transfered",
            "billing.order.project.keepproject.question",
            "billing.order.project.changecustomerinproject.question",
            "billing.order.project.changecustomerinprojectnotallowed.message",
            "billing.order.autosavemessage",
            "billing.order.autosavemessageminutes",
            "billing.order.transfer.openinoice",
            "billing.order.transfer.openinvoicemessage",
            "billing.order.creditlimit",
            "billing.order.creditlimit.message1",
            "billing.order.creditlimit.message2",
            "billing.offer.hashousehold",
            "billing.project.timesheet.includetimeinreport.none",
            "billing.project.timesheet.includetimeinreport.all",
            "billing.project.timesheet.includetimeinreport.invoiced",
            "billing.order.connectmainorder",
            "common.customer.invoices.copyattachmentsheader",
            "common.customer.invoices.copyattachmentstext",
            "billing.order.hashousehold",
            "billing.order.delete",
            "billing.offer.delete",
            "billing.contract.delete",
            "billing.order.showordersummary",
            "billing.order.ordersummary",
            "core.missingmandatoryfield",
            "billing.order.transfertoinvoice",
            "billing.order.transfertoinvoicecash",
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];

        // Order fields
        settingTypes.push(CompanySettingType.CustomerInvoiceUseDeliveryCustomer);
        settingTypes.push(CompanySettingType.CustomerShowTransactionCurrency);
        settingTypes.push(CompanySettingType.CustomerShowEnterpriseCurrency);
        settingTypes.push(CompanySettingType.CustomerShowLedgerCurrency);
        settingTypes.push(CompanySettingType.CustomerInvoiceDefaultVatType);
        settingTypes.push(CompanySettingType.BillingInvoiceText);
        settingTypes.push(CompanySettingType.CustomerInvoiceOurReference);
        settingTypes.push(CompanySettingType.BillingDefaultWholeseller);
        settingTypes.push(CompanySettingType.BillingDefaultPriceListType);
        settingTypes.push(CompanySettingType.BillingIncludeWorkDescriptionOnInvoice);
        settingTypes.push(CompanySettingType.BillingDefaultDeliveryType);
        settingTypes.push(CompanySettingType.BillingDefaultDeliveryCondition);
        settingTypes.push(CompanySettingType.UseDeliveryAddressAsInvoiceAddress);
        settingTypes.push(CompanySettingType.CustomerPaymentDefaultPaymentCondition);
        settingTypes.push(CompanySettingType.CustomerPaymentDefaultPaymentConditionHouseholdDeduction);
        settingTypes.push(CompanySettingType.CustomerPaymentServiceOnlyToContract);
        settingTypes.push(CompanySettingType.BillingUseFreightAmount);
        settingTypes.push(CompanySettingType.BillingUseInvoiceFee);
        settingTypes.push(CompanySettingType.ProductFreight);
        settingTypes.push(CompanySettingType.ProductInvoiceFee);
        settingTypes.push(CompanySettingType.CustomerInvoiceTriangulationSales);
        settingTypes.push(CompanySettingType.BillingDefaultOneTimeCustomer);
        settingTypes.push(CompanySettingType.BillingOfferValidNoOfDays);

        // Product rows
        settingTypes.push(CompanySettingType.ProductFlatPrice);
        settingTypes.push(CompanySettingType.BillingHideVatRate);

        // Project
        settingTypes.push(CompanySettingType.ProjectAutoGenerateOnNewInvoice);
        settingTypes.push(CompanySettingType.ProjectSuggestOrderNumberAsProjectNumber);
        settingTypes.push(CompanySettingType.ProjectUseCustomerNameAsProjectName);
        settingTypes.push(CompanySettingType.ProjectIncludeTimeProjectReport);
        settingTypes.push(CompanySettingType.ProjectIncludeOnlyInvoicedTimeInTimeProjectReport);

        // Checklist
        settingTypes.push(CompanySettingType.BillingMandatoryChecklist);

        // Printing
        settingTypes.push(CompanySettingType.BillingDefaultEmailTemplate);
        settingTypes.push(CompanySettingType.BillingOfferDefaultEmailTemplate);
        settingTypes.push(CompanySettingType.BillingOrderDefaultEmailTemplate);
        settingTypes.push(CompanySettingType.BillingContractDefaultEmailTemplate);
        settingTypes.push(CompanySettingType.BillingEInvoiceFormat);
        settingTypes.push(CompanySettingType.CustomerDefaultReminderTemplate);
        settingTypes.push(CompanySettingType.CustomerDefaultInterestTemplate);
        settingTypes.push(CompanySettingType.AccountingDefaultVoucherList);
        settingTypes.push(CompanySettingType.BillingDefaultTimeProjectReportTemplate);
        settingTypes.push(CompanySettingType.BillingDefaultContractTemplate);

        // Transfer
        settingTypes.push(CompanySettingType.CustomerInvoiceTransferToVoucher);
        settingTypes.push(CompanySettingType.CustomerInvoiceAskPrintVoucherOnTransfer);
        settingTypes.push(CompanySettingType.BillingUsePartialInvoicingOnOrderRow);
        settingTypes.push(CompanySettingType.BillingAskCreateInvoiceWhenOrderReady);
        settingTypes.push(CompanySettingType.BillingAskOpenInvoiceWhenCreateInvoiceFromOrder);

        // Cash sales
        settingTypes.push(CompanySettingType.BillingUseCashSales);

        // Validation
        settingTypes.push(CompanySettingType.BillingShowZeroRowWarning);

        // Save
        settingTypes.push(CompanySettingType.BillingOrderAutoSaveInterval);
        settingTypes.push(CompanySettingType.BillingOfferAutoSaveInterval);
        settingTypes.push(CompanySettingType.BillingContractAutoSaveInterval);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            // Order fields
            this.showPayingCustomer = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.CustomerInvoiceUseDeliveryCustomer);
            this.showTransactionCurrency = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.CustomerShowTransactionCurrency);
            this.showEnterpriseCurrency = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.CustomerShowEnterpriseCurrency);
            this.showLedgerCurrency = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.CustomerShowLedgerCurrency);
            this.invoiceEditHandler.defaultVatType = SettingsUtility.getIntCompanySetting(x, CompanySettingType.CustomerInvoiceDefaultVatType, this.invoiceEditHandler.defaultVatType);
            this.defaultInvoiceText = SettingsUtility.getStringCompanySetting(x, CompanySettingType.BillingInvoiceText);
            this.defaultOurReference = SettingsUtility.getStringCompanySetting(x, CompanySettingType.CustomerInvoiceOurReference);
            this.defaultWholesellerId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingDefaultWholeseller);
            this.defaultPriceListTypeId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingDefaultPriceListType);
            if (this.defaultPriceListTypeId === 0)
                this.showMissingDefaultPriceListTypeWarning();
            this.includeWorkDescriptionOnInvoice = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingIncludeWorkDescriptionOnInvoice);
            this.defaultDeliveryTypeId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingDefaultDeliveryType);
            this.defaultDeliveryConditionId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingDefaultDeliveryCondition);
            this.useDeliveryAddressAsInvoiceAddress = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseDeliveryAddressAsInvoiceAddress);
            this.invoiceEditHandler.defaultPaymentConditionId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.CustomerPaymentDefaultPaymentCondition);
            this.defaultPaymentConditionHouseholdDeductionId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.CustomerPaymentDefaultPaymentConditionHouseholdDeduction);
            this.paymentServiceOnlyToContract = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.CustomerPaymentServiceOnlyToContract);
            this.useFreightAmount = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingUseFreightAmount);
            this.useInvoiceFee = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingUseInvoiceFee);
            this.freightAmountProductId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.ProductFreight);
            this.invoiceFeeProductId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.ProductInvoiceFee);
            this.triangulationSales = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.CustomerInvoiceTriangulationSales, true);
            this.defaultOneTimeCustomerId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingDefaultOneTimeCustomer);
            this.offerValidNoOfDays = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingOfferValidNoOfDays);

            // Product rows
            this.fixedPriceProductId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.ProductFlatPrice);
            this.hideVatRate = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingHideVatRate);

            // Project
            this.autoGenerateProject = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.ProjectAutoGenerateOnNewInvoice);
            this.suggestOrderNrAsProjectNr = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.ProjectSuggestOrderNumberAsProjectNumber);
            this.useCustomerNameAsProjectName = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.ProjectUseCustomerNameAsProjectName, true);
            this.projectIncludeTimeProjectReport = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.ProjectIncludeTimeProjectReport);
            this.projectIncludeOnlyInvoicedTimeInTimeProjectReport = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.ProjectIncludeOnlyInvoicedTimeInTimeProjectReport, true);

            // Checklist
            this.mandatoryChecklist = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingMandatoryChecklist);

            // Printing
            this.emailTemplateId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingDefaultEmailTemplate);
            this.offerEmailTemplateId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingOfferDefaultEmailTemplate);
            this.orderEmailTemplateId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingOrderDefaultEmailTemplate);
            this.contractEmailTemplateId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingContractDefaultEmailTemplate);
            this.eInvoiceFormat = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingEInvoiceFormat);
            this.reminderReportId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.CustomerDefaultReminderTemplate);
            this.interestReportId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.CustomerDefaultInterestTemplate);
            this.voucherListReportId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountingDefaultVoucherList);
            this.timeProjectReportId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingDefaultTimeProjectReportTemplate);
            this.billingContractReportId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingDefaultContractTemplate);

            // Transfer
            this.transferToVoucher = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.CustomerInvoiceTransferToVoucher);
            this.askPrintVoucherOnTransfer = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.CustomerInvoiceAskPrintVoucherOnTransfer);
            this.usePartialInvoicingOnOrderRow = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingUsePartialInvoicingOnOrderRow);
            this.askCreateInvoiceWhenOrderReady = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingAskCreateInvoiceWhenOrderReady);
            this.askOpenInvoiceWhenCreateInvoiceFromOrder = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingAskOpenInvoiceWhenCreateInvoiceFromOrder);

            //Cash sales
            this.$timeout(() => {
                this.useCashSales = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingUseCashSales);
            });

            // Validation
            this.showZeroRowWarning = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingShowZeroRowWarning);

            // Save
            if (this.isOffer) {
                this.autoSaveInterval = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingOfferAutoSaveInterval);
            }
            else if (this.isOrder) {
                this.autoSaveInterval = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingOrderAutoSaveInterval);
            }
            else {
                this.autoSaveInterval = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingContractAutoSaveInterval);
            }
            this.autoSaveInterval = 60 * this.autoSaveInterval; // Setting is stored in minutes, convert to seconds
        });
    }

    private loadUserSettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];

        settingTypes.push(UserSettingType.BillingInvoiceOurReference);
        settingTypes.push(UserSettingType.BillingCheckConflictsOnSave);
        settingTypes.push(UserSettingType.BillingUseOneTimeCustomerAsDefault);
        settingTypes.push(UserSettingType.BillingDefaultOrderType);

        if (this.isOffer)
            settingTypes.push(UserSettingType.BillingOfferDefaultExpanders);
        else if (this.isOrder)
            settingTypes.push(UserSettingType.BillingOrderDefaultExpanders);
        else if (this.isContract)
            settingTypes.push(UserSettingType.BillingContractDefaultExpanders);

        return this.coreService.getUserSettings(settingTypes).then(x => {

            this.defaultOurReferenceUserId = SettingsUtility.getIntUserSetting(x, UserSettingType.BillingInvoiceOurReference);
            this.checkConflictsOnSave = SettingsUtility.getBoolUserSetting(x, UserSettingType.BillingCheckConflictsOnSave);
            this.useOneTimeCustomer = SettingsUtility.getBoolUserSetting(x, UserSettingType.BillingUseOneTimeCustomerAsDefault);
            this.defaultOrderType = SettingsUtility.getIntUserSetting(x, UserSettingType.BillingDefaultOrderType);

            if (this.isOffer) {
                this.expanderSettings = x[UserSettingType.BillingOfferDefaultExpanders];
            }
            else if (this.isOrder) {
                this.expanderSettings = x[UserSettingType.BillingOrderDefaultExpanders];
            }
            else if (this.isContract) {
                this.expanderSettings = x[UserSettingType.BillingContractDefaultExpanders];
            }
        });
    }

    private loadCompanyAccounts(): ng.IPromise<any> {
        var settingTypes: number[] = [];

        settingTypes.push(CompanySettingType.AccountCustomerSalesVat);
        settingTypes.push(CompanySettingType.AccountCustomerClaim);
        settingTypes.push(CompanySettingType.AccountCommonVatPayable1);
        settingTypes.push(CompanySettingType.AccountCommonReverseVatPurchase);
        settingTypes.push(CompanySettingType.AccountCommonReverseVatSales);
        settingTypes.push(CompanySettingType.AccountCommonVatPayable1Reversed);
        settingTypes.push(CompanySettingType.AccountCommonVatReceivableReversed);
        //settingTypes.push(CompanySettingType.AccountCustomerDiscount);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.defaultCreditAccountId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCustomerSalesVat);
            this.defaultDebitAccountId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCustomerClaim);
            this.defaultVatAccountId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCommonVatPayable1);
            this.reverseVatPurchaseId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCommonReverseVatPurchase);
            this.reverseVatSalesId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCommonReverseVatSales);
            this.contractorVatAccountCreditId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCommonVatPayable1Reversed);
            this.contractorVatAccountDebitId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCommonVatReceivableReversed);
            this.defaultCustomerDiscountAccount = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCustomerDiscount);
            this.defaultCustomerDiscountOffsetAccount = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCustomerDiscountOffset);

            // Load default VAT rate for the company
            this.loadVatRate(this.defaultVatAccountId);
        });
    }

    private loadCustomers(): ng.IPromise<any> {
        this.customers = [];
        return this.commonCustomerService.getCustomersSmall(true).then((x) => {
            this.customers.push({ id: 0, name: " " });
            _.forEach(x, (customer) => {
                this.customers.push({ id: customer.actorCustomerId, name: customer.customerNr + " " + customer.customerName, number: customer.customerNr });
            });
        });
    }

    private loadOrderTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.OrderType, false, false).then(x => {
            this.orderTypes = x;
        });
    }

    private loadTemplates(useCache = true, resetSelectedTemplate = false, resetId = 0): ng.IPromise<any> {
        if (this.templatesPermission) {
            return this.orderService.getTemplates(this.isOffer ? SoeOriginType.Offer : SoeOriginType.Order, useCache).then(x => {
                this.orderTemplates = x;

                if (resetSelectedTemplate) {
                    this.$timeout(() => {
                        if (resetId > 0 && this.invoice.orderInvoiceTemplateId != resetId) {
                            this.invoice.orderInvoiceTemplateId = resetId;
                            this._selectedOrderInvoiceTemplate = _.find(this.orderTemplates, s => s.id === this.invoice.orderInvoiceTemplateId);
                            this.setAsDirty(true);
                        }
                        else {
                            this._selectedOrderInvoiceTemplate = _.find(this.orderTemplates, s => s.id === this.invoice.orderInvoiceTemplateId);
                        }
                    });
                }
            });
        }
    }

    private loadFixedPriceOrderTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.OrderContractType, false, false).then(x => {
            this.fixedPriceOrderTypes = x;
        });
    }

    private loadOurReferences(): ng.IPromise<any> {
        return this.coreService.getUsersDict(true, false, true, false).then(x => {
            this.ourReferences = x;
        });
    }

    private loadPriceListTypes(): ng.IPromise<any> {
        return this.commonCustomerService.getPriceLists().then(x => {
            this.translationService.translate("common.customer.invoices.projectpricelist").then(term => {
                this.priceListTypes = x;
                this.priceListTypes.forEach(r => {
                    if (r.isProjectPriceList) {
                        r.name = `${r.name} (${term})`;
                    }
                })
                if (this.isNew)
                    this.selectedPriceListType = _.find(this.priceListTypes, { priceListTypeId: 0 });
            })
        });
    }

    private loadInvoicePaymentServices(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.InvoicePaymentService, true, false).then(x => {
            this.invoicePaymentServices = x;
        });
    }

    private loadVoucherSeries(accountYearId: number): ng.IPromise<any> {
        return this.commonCustomerService.getVoucherSeriesByYear(accountYearId, false).then((x) => {
            this.voucherSeries = x;
        });
    }

    private loadContractGroups(): ng.IPromise<any> {
        return this.orderService.getContractGroups().then(x => {
            this.contractGroups = x;
            this.contractGroups.splice(0, 0, {
                contractGroupId: 0, name: " "
            });
        });
    }

    private loadCurrentAccountYear(): ng.IPromise<any> {
        return this.coreService.getCurrentAccountYear().then((x) => {
            if (x != null)
                this.currentAccountYearId = x.accountYearId;
            else {
                const keys: string[] = [
                    "core.warning",
                    "billing.order.accountyearmissingmessage"
                ];
                return this.translationService.translateMany(keys).then((terms) => {
                    this.notificationService.showDialog(terms["core.warning"], terms["billing.order.accountyearmissingmessage"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
                });
            }
        });
    }

    private loadAccountYear(date: Date) {
        var prevAccountYearId = this.invoiceAccountYearId;

        this.commonCustomerService.getAccountYearId(date).then((id: number) => {
            this.invoiceAccountYearId = id;
            if (this.invoiceAccountYearId !== this.currentAccountYearId || this.invoiceAccountYearId !== prevAccountYearId) {
                //If account year has changed, load voucher series for new year
                this.loadVoucherSeries(this.invoiceAccountYearId);
                this.loadAccountPeriod(this.invoiceAccountYearId);
            } else {
                this.loadAccountPeriod(this.currentAccountYearId);
            }
        });
    }

    private loadAccountPeriod(accountYearId: number) {
        if (!this.invoice || !this.invoice.voucherDate)
            return;

        this.commonCustomerService.getAccountPeriodId(accountYearId, this.invoice.voucherDate).then((id: number) => {
            this.accountPeriodId = id;
        });
    }

    private loadVatRate(accountId: number) {
        if (accountId === 0) {
            this.setDefaultVatRate();
            return;
        }

        this.commonCustomerService.getAccountSysVatRate(accountId).then(x => {
            this.defaultVatRate = x;
            this.setDefaultVatRate();
        });
    }

    private setVatRate() {
        this.setDefaultVatRate();
    }

    private setDefaultVatRate() {
        if (this.defaultVatRate === 0)
            this.defaultVatRate = CoreUtility.sysCountryId == TermGroup_Languages.Finnish ? Constants.DEFAULT_VAT_RATE_FIN : Constants.DEFAULT_VAT_RATE;

        this.vatRate = this.defaultVatRate;
    }

    private loadInvoiceDeliveryTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.InvoiceDeliveryType, true, false).then(x => {
            this.invoiceDeliveryTypes = x;
            if (!this.createEInvoicePermission) {
                this.invoiceDeliveryTypes = this.invoiceDeliveryTypes.filter(type => type.id !== 3 && type.id !== 4);
            }
            if (this.eInvoiceFormat !== TermGroup_EInvoiceFormat.Intrum) {
                this.invoiceDeliveryTypes = this.invoiceDeliveryTypes.filter(x => x.id !== SoeInvoiceDeliveryType.EDI);
            }
        });
    }

    private loadShiftTypes() {
        this.coreService.getShiftTypesForUsersCategories(0, true, [TermGroup_TimeScheduleTemplateBlockType.Order]).then(x => {
            this.shiftTypes = x;

            // Add empty
            var shiftType = new ShiftTypeGridDTO();
            shiftType.shiftTypeId = 0;
            shiftType.name = ' ';
            shiftType.defaultLength = 0;
            this.shiftTypes.splice(0, 0, shiftType);
        });
    }

    private loadOrderShifts() {
        //Set rendered
        this.planningRendered = true;

        if (!this.plannedShifts || this.plannedShifts.length === 0) {
            this.orderService.getOrderShifts(this.invoiceId).then(x => {
                this.plannedShifts = x;
            });
        }
    }

    private loadCustomer(customerId: number, keepPriceList: boolean = false) {
        if (customerId) {
            this.commonCustomerService.getCustomer(customerId, false, true, false, false, false, false).then(x => {
                this.customer = x;

                if (!this.selectedCustomer.name) {
                    this.selectedCustomer.name = `${this.customer.customerNr} ${this.customer.name}`
                    if (!this.customers.find(c => c.id === this.selectedCustomer.id)) {
                        this.customers.push(this.selectedCustomer);
                    }
                    this._selectedCustomer = { ...this.selectedCustomer }
                }

                this.loadingCustomer = false;
                this.customerChanged(keepPriceList);
            });
        } else {
            this.customer = null;
            this.customerChanged();
        }
    }

    private checkCustomerCreditLimit(customerId: number, creditLimit: number) {
        if (!creditLimit || creditLimit === 0)
            return;

        this.commonCustomerService.checkCustomerCreditLimit(customerId, creditLimit).then(limit => {
            if (limit) {
                if (this.customer.creditLimit && this.customer.creditLimit < limit) {
                    var message;

                    if (this.customer != null && this.customer.creditLimit) {
                        this.currentBalance = this.customer.creditLimit - limit;
                        var filter: Function = this.$filter("amount");
                        message = this.terms["billing.order.creditlimit.message1"].format(filter(this.customer.creditLimit), filter(limit - this.customer.creditLimit));
                    }
                    else {
                        var filter: Function = this.$filter("amount");
                        message = this.terms["billing.order.creditlimit.message2"] + " " + filter(limit.toString());
                    }

                    var modal = this.notificationService.showDialog(this.terms["billing.order.creditlimit"], message, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
                    modal.result.then(val => {
                        this.showValidationError();
                    });
                }
                else {
                    this.currentBalance = this.customer.creditLimit - limit;
                }
            }
        });
    }

    private loadDeliveryCustomer() {
        if (this.selectedDeliveryCustomer && this.selectedDeliveryCustomer.id !== 0) {
            this.commonCustomerService.getCustomer(this.selectedDeliveryCustomer.id, true, true, true, false, false, false).then(x => {
                this.deliveryCustomer = x;
                this.loadingDeliveryCustomer = false;
                if (this.deliveryCustomer) {

                    //handle inactive customer
                    if (!this.selectedDeliveryCustomer.name) {
                        this.selectedDeliveryCustomer.name = `${this.deliveryCustomer.customerNr} ${this.deliveryCustomer.name}`
                        if (!this.customers.find(c => c.id === this.selectedDeliveryCustomer.id)) {
                            this.customers.push(this.selectedDeliveryCustomer);
                        }
                        this._selectedDeliveryCustomer = { ...this.selectedDeliveryCustomer }
                    }

                    if (!this.invoice.invoiceId || this.invoice.invoiceId === 0) {
                        if (this.deliveryCustomer.payingCustomerId && this.deliveryCustomer.payingCustomerId > 0) {
                            if (!this.selectedCustomer)
                                this.selectedCustomer = _.find(this.customers, c => c.id === this.deliveryCustomer.payingCustomerId);
                            else
                                this.customerChanged();
                        }
                        else if (!this.selectedCustomer) {
                            this.selectedCustomer = _.find(this.customers, c => c.id === this.deliveryCustomer.actorCustomerId);
                            this.customer = this.deliveryCustomer;
                            this.customerChanged();
                        }
                    }
                    else {
                        this.customerChanged();
                    }
                }
            });
        }
        else {
            this.selectedDeliveryCustomer = null;
            this.customerChanged();
        }
    }

    private loadCustomerReferences(customerId: number) {
        this.commonCustomerService.getCustomerReferences(customerId, true).then(x => {
            this.customerReferences = x;

            // Add customer invoice reference to list
            if (this.customer.invoiceReference) {
                this.customerReferences.splice(1, 0, {
                    id: 1, name: this.customer.invoiceReference
                });
            }

            if (!_.find(this.customerReferences, { 'name': this.invoice.referenceYour }) && this.resetReference)
                this.invoice.referenceYour = '';

            if ((this.isNew || (StringUtility.isEmpty(this.invoice.referenceYour) && this.resetReference)) && this.customerReferences.length > 1 && this.customer.invoiceReference)
                this.invoice.referenceYour = this.customerReferences[1].name;

            this.resetReference = false;
        });
    }

    private loadCustomerEmails(customerId: number) {
        this.commonCustomerService.getCustomerEmails(customerId, true, true).then(x => {
            this.customerEmails = x;

            if (!this.isNew && this.invoice.customerEmail) {
                this.customerEmails[0].name = this.invoice.customerEmail;
                this.invoice.contactEComId = 0;
            }
            else if (this.isNew || !_.find(this.customerEmails, { 'id': this.invoice.contactEComId })) {
                if (this.customer.orderContactEComId) {
                    this.invoice.contactEComId = this.customer.orderContactEComId;
                }
                else if (this.customer.contactEComId) {
                    this.invoice.contactEComId = this.customer.contactEComId;
                }
                else if (this.customerEmails.length > 1) {
                    this.invoice.contactEComId = this.customerEmails[1].id;
                }
            }
        });
    }

    private getFreightAmount() {
        if (this.loadingInvoice || !this.invoice || this.productRowsRendered || !this.useFreightAmount)
            return;

        if (this.ignoreReloadFreightAmount) {
            this.ignoreReloadFreightAmount = false;
            return;
        }

        this.productService.getProductPriceDecimal(this.invoice.priceListTypeId, this.freightAmountProductId).then(x => {
            if (this.invoice.billingType === TermGroup_BillingType.Credit)
                x = 0;

            if (x && this.invoice.freightAmount !== x) {
                this.invoice.freightAmount = x;
                this.currencyHelper.amountOrCurrencyChanged(this.invoice);

                if (!this.productRowsRenderFinalized)
                    this.freightAmountUpdated = true;
            }
        });
    }

    private getInvoiceFee() {
        if (this.loadingInvoice || !this.invoice || !this.useInvoiceFee || this.disableInvoiceFee)
            return;

        if (this.ignoreReloadInvoiceFee) {
            this.ignoreReloadInvoiceFee = false;
            return;
        }

        this.productService.getProductPriceDecimal(this.invoice.priceListTypeId, this.invoiceFeeProductId).then(x => {
            if (this.invoice.billingType === TermGroup_BillingType.Credit)
                x = 0;

            if (x && this.invoice.invoiceFee !== x) {
                this.invoice.invoiceFee = x;
                this.currencyHelper.amountOrCurrencyChanged(this.invoice);

                if (!this.productRowsRenderFinalized)
                    this.invoiceFeeUpdated = true;
            }
        });
    }

    // EVENTS

    private billingTypeChanging(oldValue) {
        // Only show warning if amount is entered and user has manually modified any row
        if (this.invoice.totalAmountCurrency !== 0 && this.hasModifiedRows()) {
            const keys: string[] = [
                "core.warning",
                "common.customer.invoices.billingtypechangewarning"
            ];

            this.translationService.translateMany(keys).then((terms) => {
                var modal = this.notificationService.showDialog(terms["core.warning"], terms["common.customer.invoices.billingtypechangewarning"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                modal.result.then(val => {
                    this.changeBillingType();
                }, (reason) => {
                    // User cancelled, revoke to previous billing type
                    this.invoice.billingType = oldValue;
                });
            });
        } else {
            this.changeBillingType();
        }
    }

    private changeBillingType() {
        // Switch sign on total amount
        this.currencyHelper.isCreditChanged(this.isCredit);
        this.$timeout(() => {
            this.generateAccountingRows(true);
            this.getFreightAmount();
            this.getInvoiceFee();
        });
    }

    private vatTypeChanging(oldValue) {
        this.$timeout(() => {
            // Show warning if household deduction row is present
            if (this.hasHouseholdTaxDeduction && (this.invoice.vatType === TermGroup_InvoiceVatType.Contractor || this.invoice.vatType === TermGroup_InvoiceVatType.NoVat)) {
                const keys: string[] = [
                    "core.warning",
                    "common.customer.invoices.householdvattypewarning"
                ];

                this.translationService.translateMany(keys).then((terms) => {
                    this.notificationService.showDialog(terms["core.warning"], terms["common.customer.invoices.householdvattypewarning"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
                    this.invoice.vatType = oldValue;
                });
                return;
            }
            // Only show warning if amount is entered and user has manually modified any row
            else if (this.invoice.totalAmountCurrency !== 0 && !this.isNew) {
                const keys: string[] = [
                    "core.warning",
                    "common.customer.invoices.vattypechangewarning"
                ];

                this.translationService.translateMany(keys).then((terms) => {
                    var modal = this.notificationService.showDialog(terms["core.warning"], terms["common.customer.invoices.vattypechangewarning"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                    modal.result.then(val => {
                        if (this.productRowsRendered) {
                            this.$scope.$broadcast('vatTypeChanged');
                        }
                        else {
                            this.vatTypeUpdated = true;
                            this.productRowsRendered = true;
                            this.executing = true;
                        }
                        //this.changeVatType();
                    }, (reason) => {
                        // User cancelled, revoke to previous vat type
                        this.invoice.vatType = oldValue;
                    });
                });
            } else {
                if (this.productRowsRendered) {
                    this.$scope.$broadcast('vatTypeChanged');
                }
                else {
                    this.vatTypeUpdated = true;
                    this.productRowsRendered = true;
                    this.executing = true;
                }
                //this.changeVatType();
            }
        });
    }

    private changeVatType() {
        this.$timeout(() => {
            _.forEach(_.filter(this.invoice.customerInvoiceRows, r => r.type === SoeInvoiceRowType.ProductRow || r.type === SoeInvoiceRowType.BaseProductRow || r.isInvoiceFeeRow), (row) => {
                row.isModified = true;
            });
        });
    }

    private restoreOriginalCustomer() {
        var softChange = (this.customer && (this.customer.actorCustomerId === this.originalCustomerId));

        if (softChange)
            this._selectedCustomer = _.find(this.customers, c => c.id === this.originalCustomerId);
        else
            this.selectedCustomer = _.find(this.customers, c => c.id === this.originalCustomerId);
    }

    private initLoadCustomer() {
        if (!this.loadingInvoice && this.askChangingProject && this.invoice.projectId && this.invoice.projectId > 0 && (this.invoice.orderType.valueOf() == TermGroup_OrderType.Unspecified || this.isOrderTypeProject)) {
            this.orderService.getProject(this.invoice.projectId).then((project: ProjectDTO) => {
                let keepPriceList = (project.priceListTypeId === this.invoice.priceListTypeId);
                if (project.status !== TermGroup_ProjectStatus.Hidden && project.customerId && project.customerId != this.selectedCustomer.id) {
                    const modal = this.notificationService.showDialogEx(this.terms["core.verifyquestion"], this.terms["billing.order.project.keepproject.question"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.YesNoCancel);
                    modal.result.then(
                        (val) => {
                            if (val == true) {
                                // keep project, change customer to the project
                                this.changeCustomerInProject(keepPriceList);
                            }
                            else {
                                // don't keep project, remove it from order
                                this.invoice.projectId = 0;
                                this.invoice.projectNr = "";
                                this.loadCustomer(this.selectedCustomer ? this.selectedCustomer.id : null);
                            }
                        },
                        (cancel) => {
                            // cancel and return the original customer to the order
                            this.restoreOriginalCustomer();
                            //this.selectedCustomer = _.find(this.customers, c => c.id === this.originalCustomerId);
                            //this.loadCustomer(this.originalCustomerId, keepPriceList);
                            this.askChangingProject = true;
                        })
                }
                else {
                    this.loadCustomer(this.selectedCustomer ? this.selectedCustomer.id : null, keepPriceList);
                }
            })
        }
        else {
            this.loadCustomer(this.selectedCustomer ? this.selectedCustomer.id : null);
        }
    }

    private currencyChanged() {
        if (this.invoice && !this.loadingInvoice) {
            this.currencyHelper.toInvoice(this.invoice);

            this.currencyHelper.amountOrCurrencyChanged(this.invoice);
            this.convertAmount('totalAmount', this.invoice.totalAmountCurrency);
            this.convertAmount('vatAmount', this.invoice.vatAmountCurrency);

            this.$scope.$broadcast('recalculateTotals', { guid: this.guid });
        }
    }

    private currencyIdChanged() {
        if (this.invoice && !this.loadingInvoice) {
            // Triangulation sales
            if (this.triangulationSales) {
                this.$timeout(() => {
                    var settingCustomer = this.deliveryCustomer && this.showPayingCustomer ? this.deliveryCustomer : this.customer;
                    this.invoice.triangulationSales = !this.currencyHelper.isBaseCurrency && settingCustomer ? settingCustomer.triangulationSales : false;
                });
            }
        }
    }

    private customerChanged(keepPriceList: boolean = false) {
        if (this.loadingCustomer || this.loadingDeliveryCustomer)
            return;

        // Set customer dependant values
        var settingCustomer = this.deliveryCustomer && this.showPayingCustomer ? this.deliveryCustomer : this.customer;
        var deliveryCustomer = this.customer;

        this.$scope.$broadcast('updateCustomer', { customer: settingCustomer, getFreight: true });

        //Disable invoice fee
        this.disableInvoiceFee = settingCustomer && settingCustomer.disableInvoiceFee;

        if (!this.loadingInvoice) {
            this.invoice.actorId = this.customer.actorCustomerId;

            if (settingCustomer && settingCustomer.currencyId)
                this.invoice.currencyId = settingCustomer.currencyId;

            this.currencyHelper.currencyId = this.invoice.currencyId;

            // Wholeseller
            this.invoice.sysWholeSellerId = settingCustomer && settingCustomer.sysWholeSellerId ? settingCustomer.sysWholeSellerId : this.defaultWholesellerId;

            // Price list
            if (!keepPriceList) {
                this.setPriceListType(settingCustomer && settingCustomer.priceListTypeId ? settingCustomer.priceListTypeId : this.defaultPriceListTypeId);
            }

            // VAT
            var oldVatType = this.invoice.vatType;
            this.invoice.vatType = settingCustomer && settingCustomer.vatType !== TermGroup_InvoiceVatType.None ? settingCustomer.vatType : this.invoiceEditHandler.defaultVatType;
            this.setVatRate();

            // Delivery type
            this.invoice.deliveryTypeId = deliveryCustomer && deliveryCustomer.deliveryTypeId ? deliveryCustomer.deliveryTypeId : this.defaultDeliveryTypeId;

            // Delivery condition
            this.invoice.deliveryConditionId = deliveryCustomer && deliveryCustomer.deliveryConditionId ? deliveryCustomer.deliveryConditionId : this.defaultDeliveryConditionId;

            // Attachments
            this.invoiceFilesHelper.addAttachementsToEInvoice = this.invoice.addAttachementsToEInvoice = this.customer && this.customer.addAttachementsToEInvoice ? this.customer.addAttachementsToEInvoice : false;
            this.invoiceFilesHelper.addSupplierInvoicesToEInvoice = this.invoice.addSupplierInvoicesToEInvoice = this.customer && this.customer.addSupplierInvoicesToEInvoice ? this.customer.addSupplierInvoicesToEInvoice : false;


            // GLN number
            this.invoice.contactGLNId = settingCustomer.contactGLNId;
            this.invoice.invoiceLabel = settingCustomer.invoiceLabel ? settingCustomer.invoiceLabel : this.invoice.invoiceLabel;

            // Contract number
            this.invoice.contractNr = settingCustomer.contractNr;

            // Payment
            this.setPaymentCondition(settingCustomer && settingCustomer.paymentConditionId ? settingCustomer.paymentConditionId : 0);
            this.setDueDate();

            // Payment service
            this.invoice.invoicePaymentService = settingCustomer && settingCustomer.invoicePaymentService ? settingCustomer.invoicePaymentService : 0;
            this.paymentServiceReadOnly = settingCustomer && (settingCustomer.invoicePaymentService && settingCustomer.invoicePaymentService > 0);

            // Invoice fee
            if (this.disableInvoiceFee)
                this.invoice.invoiceFee = this.invoice.invoiceFeeCurrency = 0;

            // Invoice delivery type
            if (this.isContract)
                this.invoice.invoiceDeliveryType = settingCustomer && settingCustomer.invoiceDeliveryType ? settingCustomer.invoiceDeliveryType : 0;

            // Accounting rows
            this.generateAccountingRows(true);

            // Note
            if (settingCustomer && settingCustomer.showNote && settingCustomer.note) {
                this.invoiceEditHandler.showCustomerNote(settingCustomer);
            }

            if (settingCustomer) {
                this.invoiceEditHandler.showCustomerBlockNote(settingCustomer, OrderInvoiceRegistrationType.Order);
            }

            this.getFreightAmount();
            this.getInvoiceFee();

            this.askChangingProject = true;
            this.setAsDirty(true);
            this.setLocked();
        }

        if (settingCustomer) {
            this.loadCustomerReferences(settingCustomer.actorCustomerId);
            this.loadCustomerEmails(this.customer.actorCustomerId);
            this.loadAddresses(deliveryCustomer, settingCustomer, !this.loadingInvoice || this.isNew); // Setting and delivery customer are switched
            this.invoiceEditHandler.loadCustomerGLNs(settingCustomer);

            if (settingCustomer.creditLimit)
                this.checkCustomerCreditLimit(settingCustomer.actorCustomerId, settingCustomer.creditLimit);
        }
        else {
            this.customerReferences = [];
            this.customerEmails = [];
            this.invoiceEditHandler.deliveryAddresses = [];
            this.invoiceEditHandler.invoiceAddresses = [];
        }

        if (this.invoice.vatType !== oldVatType && !this.loadingInvoice)
            this.vatTypeChanging(oldVatType);

        if (!this.loadingCustomer && !this.loadingDeliveryCustomer)
            this.invoiceLoaded();
    }

    private loadAddresses(settingCustomer: CustomerDTO, deliveryCustomer: CustomerDTO, setFirstAsDefault: boolean) {
        this.$q.all(
            [this.invoiceEditHandler.loadDeliveryAddresses(deliveryCustomer.actorCustomerId),
            this.invoiceEditHandler.loadInvoiceAddresses(settingCustomer.actorCustomerId)]
        ).then(() => {
            //delivery
            if (setFirstAsDefault && this.invoiceEditHandler.deliveryAddresses.length > 1)
                this.invoice.deliveryAddressId = this.invoiceEditHandler.deliveryAddresses[1].contactAddressId;
            else if (this.invoice.invoiceHeadText != null && this.invoice.invoiceHeadText != "")
                this.invoiceEditHandler.deliveryAddresses[0].address = this.invoice.invoiceHeadText;

            //invoice
            if (setFirstAsDefault && this.invoiceEditHandler.invoiceAddresses.length > 1)
                this.invoice.billingAddressId = this.invoiceEditHandler.invoiceAddresses[1].contactAddressId;
            else if (this.isNew && this.useDeliveryAddressAsInvoiceAddress && this.invoice.deliveryAddressId && !this.invoice.billingAddressId) {
                this.invoice.billingAdressText = this.invoiceEditHandler.formatDeliveryAddress(_.filter(this.invoiceEditHandler.deliveryAddresses, i => i.contactAddressId == this.invoice.deliveryAddressId)[0]?.contactAddressRows, settingCustomer.isFinvoiceCustomer);
                this.invoiceEditHandler.invoiceAddresses[0].address = this.invoice.billingAdressText;
                this.invoice.billingAddressId = 0;
            }
            else if (this.invoice.billingAdressText != null && this.invoice.billingAdressText != "")
                this.invoiceEditHandler.invoiceAddresses[0].address = this.invoice.billingAdressText;

        });
    }

    private amountChanged(id: string) {
        this.$timeout(() => {
            if (id === 'total') {
                var totalAmount = this.invoice.totalAmountCurrency;
                if (totalAmount < 0) {
                    // If a negative total amount is entered, change billing type to credit.
                    if (!this.isCredit)
                        this.invoice.billingType = TermGroup_BillingType.Credit;
                }
                else if (totalAmount > 0 && this.isCredit) {
                    // If a positive total amount is entered for a credit invoice, negate the amount
                    totalAmount = -totalAmount;
                    this.invoice.totalAmountCurrency = totalAmount;
                }

                this.calculateVatAmount();
            }

            //Quarantine
            this.generateAccountingRows(id === 'total');
        });
    }

    private convertAmount(field: string, amount: number) {
        if (this.loadingInvoice)
            return;

        // Call amount currency converter in accounting rows directive
        var item = {
            field: field,
            amount: amount ? amount : 0,
            sourceCurrencyType: TermGroup_CurrencyType.TransactionCurrency
        };
        this.$scope.$broadcast('amountChanged', item);
    }

    private amountConverted(item) {
        if (item.parentRecordId === this.invoice.invoiceId) {
            // Result from amount currency converter in accounting rows directive
            this.invoice[item.field] = item.baseCurrencyAmount;
            this.invoice[item.field + 'Currency'] = item.transactionCurrencyAmount;
            this.invoice[item.field + 'EnterpriceCurrency'] = item.enterpriseCurrencyAmount;
            this.invoice[item.field + 'LedgerCurrency'] = item.ledgerCurrencyAmount;
        }
    }

    private setValuesFromShiftType(shiftType: ShiftTypeGridDTO) {
        // Set default length from shift type if not already specified
        if (shiftType && shiftType.defaultLength > 0 && (!this.invoice.estimatedTime || this.invoice.estimatedTime === 0)) {
            this.invoice.estimatedTime = shiftType.defaultLength;
            this.invoice.remainingTime = shiftType.defaultLength;
        }
    }

    private contractGroupChanged(contractGroupId) {
        if (this.prevContractGroupId === 0 || this.loadingInvoice) {
            this.changeContractGroup(contractGroupId);
        }
        else {
            var keys: string[] = [
                "core.verifyquestion",
                "billing.contract.changecontractgroupmessage"
            ];

            this.translationService.translateMany(keys).then(terms => {
                var modal = this.notificationService.showDialogEx(terms["core.verifyquestion"], terms["billing.contract.changecontractgroupmessage"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                modal.result.then(val => {
                    this.changeContractGroup(contractGroupId);
                }, (reason) => {
                    // User cancelled
                    this.invoice.contractGroupId = this.prevContractGroupId;
                });
            });
        }
    }

    private changeContractGroup(contractGroupId) {
        this.prevContractGroupId = contractGroupId;
        var contractGroup = _.find(this.contractGroups, c => c.contractGroupId === contractGroupId);
        if (contractGroup) {
            this.contractGroupPeriodId = contractGroup.periodId;
            this.contractGroupPeriod = contractGroup.periodText;
            this.contractGroupInterval = contractGroup.interval;

            // Only set new values if already loaded
            if (!this.loadingInvoice)
                this.getNextContractPeriod(false, contractGroup);
        }
    }

    private getNextContractPeriod(keepPeriod: boolean = false, group: ContractGroupExtendedGridDTO = null) {
        if (this.invoice === null)
            return;

        if (!group)
            group = _.find(this.contractGroups, (c) => c.contractGroupId === this.invoice.contractGroupId);

        if (group != null) {
            if (this.invoiceId > 0 && !keepPeriod) {
                var currentYear = this.invoice.nextContractPeriodYear || 0;
                var currentValue = this.invoice.nextContractPeriodValue || 0;

                var values = CalendarUtility.calculateNextPeriod(group.periodId, group.interval, currentYear, currentValue);
                this.invoice.nextContractPeriodYear = values.currentYear;
                this.invoice.nextContractPeriodValue = values.currentValue;
                this.getNextContractDate(group, false);
            }
            else {
                this.getNextContractDate(group, true);
            }
        }
    }

    private setNextContractPeriod(periodId: number, date: Date) {
        if (periodId && date) {
            var values = CalendarUtility.calculateCurrentPeriod(periodId, date);
            this.invoice.nextContractPeriodYear = values.currentYear;
            this.invoice.nextContractPeriodValue = values.currentValue;
        }
    }

    private getNextContractDate(group: ContractGroupExtendedGridDTO, setPeriod: boolean) {
        if (group != null) {
            var currentYear = this.invoice.nextContractPeriodYear || 0;
            var currentValue = this.invoice.nextContractPeriodValue || 0;
            var date: Date = CalendarUtility.convertContractPeriodToDate(group.periodId, this.invoice.invoiceDate ? this.invoice.invoiceDate : CalendarUtility.getDateToday(), currentYear, currentValue, group.dayInMonth);
            this.invoice.nextContractPeriodDate = this._selecedNextInvoiceDate = date;
            if (setPeriod)
                this.setNextContractPeriod(group.periodId, this.invoice.nextContractPeriodDate);
        }
    }

    private editOrderTemplate() {
        this.messagingService.publish(Constants.EVENT_OPEN_TAB, new TabMessage("", this.selectedOrderInvoiceTemplate.id, EditController, { id: this.selectedOrderInvoiceTemplate.id, isTemplate: true, templateOpenFromAgreement: true, feature: Feature.Billing_Order_Status, parentGuid: this.guid }, this.urlHelperService.getGlobalUrl('Shared/Billing/Orders/Views/edit.html')));
    }

    private addOrderTemplate() {
        this.translationService.translate("billing.contract.templatecreatedfrom").then(term => {
            this.messagingService.publish(Constants.EVENT_OPEN_TAB, new TabMessage("", 0, EditController, { id: this.invoice.invoiceId, isTemplate: true, templateOpenFromAgreement: true, createOrderTemplateFromAgreement: true, templateText: term + " " + this.invoice.invoiceNr, feature: Feature.Billing_Order_Status, parentGuid: this.guid }, this.urlHelperService.getGlobalUrl('Shared/Billing/Orders/Views/edit.html')));
        });
    }

    // ACTIONS

    public closeModal() {
        if (this.isModal) {
            if (this.invoiceId) {
                this.modal.close({ invoiceId: this.invoiceId });
            } else {
                this.modal.dismiss();
            }
        }
    }

    private new(isTemplate = false) {
        this.isNew = true;
        this.orderExpanderIsOpen = true;

        this.invoiceId = 0;
        this.invoice = new OrderDTO();
        this.invoice.originStatus = this.isContract ? SoeOriginStatus.Origin : SoeOriginStatus.None;

        this.selectedInvoiceDate = CalendarUtility.getDateToday();
        if (this.isContract)
            this.invoice.orderDate = CalendarUtility.getDateToday();
        if (this.fixedPriceOrderTypes.length > 0)
            this.selectedFixedPriceOrder = _.find(this.fixedPriceOrderTypes, { id: OrderContractType.Continuous });
        this.invoice.invoiceText = this.defaultInvoiceText;
        this.invoice.vatType = this.invoiceEditHandler.defaultVatType;
        this.invoice.sysWholeSellerId = this.defaultWholesellerId;
        this.invoice.includeOnInvoice = this.includeWorkDescriptionOnInvoice;
        this.invoice.deliveryTypeId = this.defaultDeliveryTypeId;
        this.invoice.deliveryConditionId = this.defaultDeliveryConditionId;
        this.invoice.paymentConditionId = this.invoiceEditHandler.defaultPaymentConditionId;
        this.invoice.voucherSeriesId = this.invoiceEditHandler.defaultVoucherSeriesId;
        this.invoice.currencyId = this.invoiceEditHandler.currencies[0].currencyId;    // Base currency is first in collection
        this.invoice.currencyDate = this.currencyHelper.currencyDate = CalendarUtility.getDateToday();
        this.invoice.currencyRate = this.currencyHelper.currencyRate;
        this.invoice.freightAmount = this.invoice.freightAmountCurrency = 0;
        this.invoice.invoiceFee = this.invoice.invoiceFeeCurrency = 0;
        this.invoice.billingType = TermGroup_BillingType.Debit;
        this.invoice.ediTransferMode = TermGroup_OrderEdiTransferMode.None;
        this.invoice.customerInvoiceRows = [];
        if (this.isOrder) {
            this.invoice.orderType = this.defaultOrderType > 0 ? this.defaultOrderType : TermGroup_OrderType.Project;
            this.invoice.printTimeReport = this.projectIncludeTimeProjectReport;
            this.invoice.includeOnlyInvoicedTime = this.projectIncludeOnlyInvoicedTimeInTimeProjectReport;
        }
        this.invoice.isTemplate = isTemplate;
        // Get origin status text for new
        this.translationService.translate("core.new").then(term => {
            this.invoice.originStatusName = term;
        });

        this.setOurReference();
        this.setPriceListType(0);   // Will set default

        if (this.customerId)
            this.selectedCustomer = _.find(this.customers, c => c.id === this.customerId)
        else
            this.selectedCustomer = null;

        if (this.projectId && this.isOrder)
            this.connectProject();

        // OriginUsers
        this.originUserHelper.setDefaultUser();
        this.invoiceFilesHelper.reset();
        this.orderIsReadyHelper.invoiceNew(this.originUserHelper.originUsers);

        if (this.useOneTimeCustomer && this.defaultOneTimeCustomerId > 0)
            this.selectedCustomer = _.find(this.customers, c => c.id === this.defaultOneTimeCustomerId);
    }

    private openProductRowExpander(fromFixedPrice = false) {

        const sendAddNew = fromFixedPrice && (this.productRowsRendered || (!this.productRowsExpanderIsOpen && this.productRowsRendered));
        this.setProductRowsRendered();
        this.productRowsExpanderIsOpen = true;

        if (sendAddNew) {
            this.$scope.$broadcast('addNewRow', { fixedPrice: this.invoice.fixedPriceOrder });
        }

        /*
        //Below is done in Grid read callback
        if (fromFixedPrice) {
           this.$scope.$broadcast('addNewRow', { fixedPrice: this.invoice.fixedPriceOrder });
        }
        */
        //don't count invoice fee as product row - removed due to moved check in to product rows directive
        //var hasNoInvoiceRows: boolean = this.invoice.customerInvoiceRows.length === 0 || (this.invoice.customerInvoiceRows.length === 1 && this.invoice.customerInvoiceRows[0].isInvoiceFeeRow == true)

        /*
        if (this.invoice && this.invoice.customerInvoiceRows) {
            if (fixedPrice) {
                // Add fixed price row
                this.$scope.$broadcast('addNewRow', { fixedPrice: true});
            }
            else {
                // Add empty product row
                this.$scope.$broadcast('addNewRow', {});
            }
        }
        */
    }

    private setProductRowsRendered() {
        this.productRowsRendered = true;
    }

    private executeProjectFunction(option) {
        switch (option.id) {
            case OrderEditProjectFunctions.Create:
                this.openProject(true);
                break;
            case OrderEditProjectFunctions.Link:
                this.openSelectProject();
                break;
            case OrderEditProjectFunctions.Change:
                this.openSelectProject();
                break;
            case OrderEditProjectFunctions.Remove:
                this.removeProject();
                break;
            case OrderEditProjectFunctions.OpenProjectCentral:
                this.openProjectCentral();
                break;
            case OrderEditProjectFunctions.Open:
                this.openProject(false);
                break;
        }
    }

    private openProject(newProject: boolean) {
        const modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Billing/Projects/Views/edit.html"),
            controller: BillingProjectsEditController,
            controllerAs: 'ctrl',
            bindToController: true,
            backdrop: 'static',
            size: 'xl',
            windowClass: 'fullsize-modal',
            scope: this.$scope
        });

        modal.rendered.then(() => {
            this.$scope.$broadcast(Constants.EVENT_ON_INIT_MODAL, { modal: modal, sourceGuid: this.guid, id: this.invoice.projectId ? this.invoice.projectId : 0 });
        });

        modal.result.then(result => {
            if (newProject) {
                this.setProjectValues(result.id, result.number);
                this.$scope.$broadcast(Constants.EVENT_RELOAD_ACCOUNT_DIMS, {});
            }
        });
    }

    private connectMainOrder() {
        const modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getCommonViewUrl("Dialogs/SelectCustomerInvoice", "selectcustomerinvoice.html"),
            controller: SelectCustomerInvoiceController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'lg',
            resolve: {
                title: () => { return this.terms["billing.order.connectmainorder"] },
                isNew: () => { return this.isNew },
                ignoreChildren: () => { return true },
                originType: () => { return SoeOriginType.Order },
                customerId: () => { return this.invoice.actorId },
                projectId: () => { return this.invoice.projectId },
                invoiceId: () => { return this.invoice.invoiceId },
                currentMainInvoiceId: () => { return this.invoice.mainInvoiceId },
                selectedProjectName: () => { return null },
                userId: () => { return null },
                includePreliminary: () => { return null },
                includeVoucher: () => { return null },
                fullyPaid: () => { return null },
                useExternalInvoiceNr: () => { return null },
                importRow: () => { return null },
            }
        });

        modal.result.then(result => {
            if (result) {
                if (result.remove) {
                    this.invoice.mainInvoiceId = undefined;
                    this.invoice.mainInvoiceNr = undefined;
                    this.invoice.mainInvoice = undefined;
                    this.setAsDirty();
                }
                else if (result.invoice) {
                    if (result.copy) {
                        // For a later time?
                    }
                    else if (this.invoice.mainInvoiceId !== result.invoice.customerInvoiceId) {
                        this.invoice.mainInvoiceId = result.invoice.customerInvoiceId;
                        this.invoice.mainInvoice = result.invoice.number + " - " + result.invoice.customerNr + " " + result.invoice.customerName;

                        if (!this.selectedCustomer)
                            this.selectedCustomer = _.find(this.customers, c => c.id === result.invoice.customerId);

                        if (result.invoice.projectId && result.invoice.projectId != this.invoice.projectId) {
                            if (this.invoice.projectId) {
                                this.changeProject(result.invoice.projectId, true).then((x) => {
                                    this.setProjectValues(result.invoice.projectId, result.invoice.projectNr, false);
                                });
                            }
                            else {
                                this.projectNotSaved = true;
                                this.setProjectValues(result.invoice.projectId, result.invoice.projectNr, false);
                            }
                        }

                        this.setAsDirty();

                    }
                }
            }
        });
    }

    private openMainOrder() {
        this.messagingService.publish(Constants.EVENT_OPEN_ORDER, { row: { customerInvoiceId: this.invoice.mainInvoiceId, invoiceNr: this.invoice.mainInvoiceNr }, ids: [] });
    }

    private orderContainsDefaultDims(): boolean {
        return (this.invoice.defaultDim1AccountId || this.invoice.defaultDim2AccountId ||
            this.invoice.defaultDim3AccountId || this.invoice.defaultDim4AccountId ||
            this.invoice.defaultDim5AccountId || this.invoice.defaultDim6AccountId) ? true : false;
    }

    private overWriteDimsDialog(): ng.IPromise<boolean> {
        const deferral = this.$q.defer<boolean>();
        if (this.orderContainsDefaultDims()) {
            const keys: string[] = [
                "billing.order.project.changeproject",
                "billing.order.overwritedefaultdims"
            ];

            this.translationService.translateMany(keys).then(terms => {
                const modal = this.notificationService.showDialogEx(terms["billing.order.project.changeproject"], terms["billing.order.overwritedefaultdims"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.YesNo, { initialFocusButton: SOEMessageBoxButton.Yes });
                modal.result.then(val => {
                    deferral.resolve(val);
                }, cancel => {
                    deferral.resolve(false);
                });
            });
        }
        else {
            deferral.resolve(true);
        }
        return deferral.promise;
    }

    private openSelectProject() {
        const modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getCommonViewUrl("Dialogs/SelectProject", "selectproject.html"),
            controller: SelectProjectController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'lg',
            resolve: {
                projects: () => { return null },
                customerId: () => { return this.invoice.actorId },
                projectsWithoutCustomer: () => { return this.showProjectsWithoutCustomer },
                showFindHidden: () => { return null },
                loadHidden: () => { return false },
                useDelete: () => { return false },
                currentProjectNr: () => { return null },
                currentProjectId: () => { return null },
                excludedProjectId: () => { return null },
                showAllProjects: () => { return false },
            }
        });

        modal.result.then(project => {
            var projectId: number = (project ? project.projectId : 0);

            this.overWriteDimsDialog().then(overwrite => {
                if (this.invoice.projectId) {

                    if (this.invoice.mainInvoiceId && this.invoice.projectId !== projectId) {
                        this.invoice.mainInvoiceId = undefined;
                        this.invoice.mainInvoiceNr = undefined;
                        this.invoice.mainInvoice = undefined;
                    }

                    this.changeProject(projectId, overwrite).then((x) => {
                        this.setProjectValues(projectId, project ? project.number : '', false);
                    });
                }
                else {
                    this.projectNotSaved = true;
                    this.setProjectValues(projectId, project ? project.number : '', false);
                    this.setAsDirty();
                }
            })

        });
    }

    private connectProject() {
        this.orderService.getProjectGridDTO(this.projectId).then(result => {
            if (result)
                this.setProjectValues(result.projectId, result.number, true, true);
        });
    }

    private changeProject(projectId: number, overwriteDefaultDimes): ng.IPromise<boolean> {
        var deferral = this.$q.defer<boolean>();
        if (this.invoice.invoiceId) {
            this.progress.startSaveProgress((completion) => {
                this.orderService.changeProjectOnInvoice(projectId, this.invoice.invoiceId, SoeProjectRecordType.Order, overwriteDefaultDimes).then(result => {
                    if (result.success) {
                        this.load();
                        completion.completed("", this.invoice);
                    }
                    else {
                        completion.failed(result.errorMessage);
                    }
                });
            }, this.guid).then(() => deferral.resolve(true), () => deferral.resolve(false));
        }
        else {
            deferral.resolve(true);
        }
        return deferral.promise;
    }

    private removeProject() {
        if (!this.invoice.projectId)
            return;

        if (this.isDisabled()) {
            const keys: string[] = [
                "core.verifyquestion",
                "billing.order.project.removeproject.question"
            ];

            this.translationService.translateMany(keys).then(terms => {
                const modal = this.notificationService.showDialogEx(terms["core.verifyquestion"], terms["billing.order.project.removeproject.question"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                modal.result.then(val => {
                    this.invoice.projectId = null;
                    if (this.invoice.mainInvoiceId) {
                        this.invoice.mainInvoiceId = undefined;
                        this.invoice.mainInvoiceNr = undefined;
                        this.invoice.mainInvoice = undefined;
                    }
                    this.setAsDirty();
                    this.save(false);
                });
            });
        }
    }

    private openProjectCentral() {
        if (!this.invoice.projectId)
            return;

        HtmlUtility.openInSameTab(this.$window, "/soe/billing/project/central/?project=" + this.invoice.projectId);
    }

    private changeCustomerInProject(keepPriceList: boolean) {
        const modal = this.notificationService.showDialogEx(this.terms["core.verifyquestion"], this.terms["billing.order.project.changecustomerinproject.question"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
        modal.result.then(
            (val) => {
                this.orderService.updateCustomerOnProject(this.invoice.invoiceId, this.invoice.projectId, this.selectedCustomer.id).then((result) => {
                    if (result.errorNumber > 0) {
                        var message: string = "";
                        if (result.errorNumber == ActionResultSave.OneToOneRelationshipRequiredToUpdateProjectCustomer)
                            message = this.terms["billing.order.project.changecustomerinprojectnotallowed.message"];

                        this.notificationService.showErrorDialog(this.terms["core.notallowed"], message, "");
                        this.askChangingProject = false;
                        //this.selectedCustomer = _.find(this.customers, c => c.id === this.originalCustomerId);
                        //this.loadCustomer(this.originalCustomerId, keepPriceList);
                        this.restoreOriginalCustomer();
                    }
                    else {
                        //Success - load customer
                        this.loadCustomer(this.selectedCustomer ? this.selectedCustomer.id : null, keepPriceList);
                    }
                })
            },
            (cancel) => {
                //this.selectedCustomer = _.find(this.customers, c => c.id === this.originalCustomerId);
                //this.loadCustomer(this.originalCustomerId, keepPriceList);
                this.restoreOriginalCustomer();
                this.askChangingProject = true;
            })
    }

    private setProjectValues(projectId: number, projectNr: string, setupTimeProjectRows: boolean = true, initWithProject: boolean = false) {
        this.orderService.getProject(projectId).then((project: ProjectDTO) => {
            if ((this.isNew) && (project.orderTemplateId)) {
                if (initWithProject) {
                    this.loadOrderTemplateFromProject(project);
                }
                else {
                    this.loadOrderTemplateFromProjectDialog(project);
                }
            }

            if (project.priceListTypeId) {
                this.setPriceListType(project.priceListTypeId);
                this.setAsDirty();
            }

            if (this.isNew) {
                this.invoice.defaultDim1AccountId = project.defaultDim1AccountId;
                this.invoice.defaultDim2AccountId = project.defaultDim2AccountId;
                this.invoice.defaultDim3AccountId = project.defaultDim3AccountId;
                this.invoice.defaultDim4AccountId = project.defaultDim4AccountId;
                this.invoice.defaultDim5AccountId = project.defaultDim5AccountId;
                this.invoice.defaultDim6AccountId = project.defaultDim6AccountId;
            }
        });

        this.invoice.projectNr = projectNr;
        this.invoice.projectId = projectId;

        if (setupTimeProjectRows) {
            this.initTimeProjectDates();
            this.loadTimeProjectRows();
        }
    }

    private initTimeProjectDates() {
        var date = CalendarUtility.getDateToday();
        this.timeProjectFrom = date.beginningOfWeek();
        this.timeProjectTo = date.endOfWeek();
    }

    private executeSaveFunction(option) {
        switch (option.id) {
            case OrderEditSaveFunctions.Save:
                this.save(false);
                break;
            case OrderEditSaveFunctions.SaveAndClose:
                this.save(true);
                break;
        }
    }

    private executeTransferFunction(option) {
        this.transferType = OrderEditTransferFunctions.None;
        switch (option.id) {
            case OrderEditTransferFunctions.TransferToOrder:
                this.transferType = OrderEditTransferFunctions.TransferToOrder;
                break;
            case OrderEditTransferFunctions.TransferToPriceOptimization:
                this.transferType = OrderEditTransferFunctions.TransferToPriceOptimization;
                break;
            case OrderEditTransferFunctions.TransferToPreliminaryInvoice:
                this.transferType = OrderEditTransferFunctions.TransferToPreliminaryInvoice;
                break;
            case OrderEditTransferFunctions.TransferToDefinitiveInvoice:
                this.transferType = OrderEditTransferFunctions.TransferToDefinitiveInvoice;
                break;
            case OrderEditTransferFunctions.DirectTransferToInvoice:
                this.transferType = OrderEditTransferFunctions.DirectTransferToInvoice;
                const keys: string[] = [
                    "billing.order.transfertoinvoice",
                    "billing.order.transfertoinvoicequestion",
                ];

                this.translationService.translateMany(keys).then(terms => {
                    var modal = this.notificationService.showDialog(terms["billing.order.transfertoinvoice"], terms["billing.order.transfertoinvoicequestion"], SOEMessageBoxImage.Question, SOEMessageBoxButtons.OKCancel);
                    modal.result.then(val => {
                        this.validateTransfer();
                    });
                });
                return;
            case OrderEditTransferFunctions.TransferToServiceOrderFromAgreement:
                this.transferType = OrderEditTransferFunctions.TransferToServiceOrderFromAgreement;
                break;
        }

        this.validateTransfer();
    }

    private executePrintFunction(option) {
        if (this.invoice) {
            switch (option.id) {
                case OrderInvoiceEditPrintFunctions.Print:
                    let reportId = 0;
                    if (this.customer) {
                        if (this.isOffer && this.customer.offerTemplate)
                            reportId = this.customer.offerTemplate;
                        else if (this.isOrder && this.customer.orderTemplate)
                            reportId = this.customer.orderTemplate;
                        else if (this.isContract && this.customer.agreementTemplate)
                            reportId = this.customer.agreementTemplate;
                    }
                    this.executePrintRequest(reportId, 0);
                    break;
                case OrderInvoiceEditPrintFunctions.eMail:
                    this.showEmailDialog();
                    break;
                case OrderInvoiceEditPrintFunctions.ReportDialog:
                    this.printFromDialog();
                    break;
            }
        }
    }

    private executePrintRequest(reportId: number, languageId: number, invoiceId?: number) {
        let registrationType = OrderInvoiceRegistrationType.Unknown;
        if (invoiceId && invoiceId > 0) {
            registrationType = OrderInvoiceRegistrationType.Invoice;
        }
        else {
            if (this.isOffer)
                registrationType = OrderInvoiceRegistrationType.Offer;
            else if (this.isOrder)
                registrationType = OrderInvoiceRegistrationType.Order;
            else if (this.isContract)
                registrationType = OrderInvoiceRegistrationType.Contract;
        }

        this.progress.startWorkProgress((completion) => {
            let model: ICustomerInvoicePrintDTO = {
                reportId: reportId,
                ids: [invoiceId && invoiceId > 0 ? invoiceId : this.invoice.invoiceId],
                queue: false,
                sysReportTemplateTypeId: 0,
                attachmentIds: [],
                checklistIds: [],
                includeOnlyInvoiced: this.invoice.includeOnlyInvoicedTime,
                orderInvoiceRegistrationType: registrationType,
                printTimeReport: this.invoice.printTimeReport,
                invoiceCopy: false, 
                asReminder: false,
                reportLanguageId: languageId,
                mergePdfs: false,
            };

            return this.requestReportService.printCustomerInvoice(model).then(() => {
                completion.completed(null, true);
            });
        });
    }

    private printOrder(reportId: number, languageId: number, recipients: any[] = null, emailTemplate: number = 0) {
        var registrationType: OrderInvoiceRegistrationType = OrderInvoiceRegistrationType.Unknown;
        if (this.isOffer)
            registrationType = OrderInvoiceRegistrationType.Offer;
        else if (this.isOrder)
            registrationType = OrderInvoiceRegistrationType.Order;
        else if (this.isContract)
            registrationType = OrderInvoiceRegistrationType.Contract;
        this.reportService.getOrderPrintUrlSingle(this.invoice.invoiceId, recipients, reportId, languageId, this.invoice.invoiceNr, this.customer.actorCustomerId, this.invoice.printTimeReport, this.invoice.includeOnlyInvoicedTime, registrationType, false, emailTemplate, false)
            .then((url) => {
                HtmlUtility.openInSameTab(this.$window, url);
            });
    }

    private printCustomerInvoice(invoiceId: number, invoiceNr: string) {
        this.reportService.getOrderPrintUrlSingle(invoiceId, null, 0, 0, invoiceNr, this.customer.actorCustomerId, this.invoice.printTimeReport, this.invoice.includeOnlyInvoicedTime, OrderInvoiceRegistrationType.Invoice, false, 0, false)
            .then((url) => {
                HtmlUtility.openInSameTab(this.$window, url);
            });
    }

    private showEmailDialog(reportId: number = 0) {
        const keys: string[] = [
            "billing.invoices.invoice",
            "common.customer.invoices.reminder",
        ];

        return this.translationService.translateMany(keys).then((types) => {
            // Set template
            let emailTemplateId = this.emailTemplateId;
            if (this.isOffer && this.offerEmailTemplateId)
                emailTemplateId = this.offerEmailTemplateId;
            else if (this.isOrder && this.orderEmailTemplateId)
                emailTemplateId = this.orderEmailTemplateId;
            else if (this.isContract && this.contractEmailTemplateId)
                emailTemplateId = this.contractEmailTemplateId;

            const modal = this.modalInstance.open({
                templateUrl: this.urlHelperService.getGlobalUrl("Common/Dialogs/SelectEmail/SelectEmail.html"),
                controller: SelectEmailController,
                controllerAs: 'ctrl',
                backdrop: 'static',
                size: 'lg',
                resolve: {
                    translationService: () => { return this.translationService },
                    coreService: () => { return this.coreService },
                    defaultEmail: () => { return this.invoice.contactEComId },
                    defaultEmailTemplateId: () => { return emailTemplateId },
                    recipients: () => { return this.customerEmails },
                    attachments: () => { return this.invoiceFilesHelper.loadFiles(false, this.invoice && this.invoice.projectId ? this.invoice.projectId : 0).then(() => { return _.filter(this.invoiceFilesHelper.files, (file) => { return !file['isDeleted'] }) }) },
                    attachmentsSelected: () => { return this.invoice.addAttachementsToEInvoice },
                    checklists: () => { return this.checklistHeads },
                    types: () => { return types },
                    grid: () => { return false },
                    type: () => { return EmailTemplateType.Invoice },
                    showReportSelection: () => { return false },
                    reports: () => { return [] },
                    defaultReportTemplateId: () => { return null },
                    langId: () => { return this.customer && this.customer.sysLanguageId ? this.customer.sysLanguageId : null }
                }
            });

            modal.result.then((result: any) => {
                let singleRecipient = "";
                const recs: number[] = [];
                const attachmentIds: number[] = [];
                const checklistIds: number[] = [];

                _.forEach(result.recipients, rec => {
                    if (rec.id > 0)
                        recs.push(rec.id);
                    else
                        singleRecipient = rec.name;
                });

                _.forEach(result.attachments, att => {
                    attachmentIds.push(att.imageId ? att.imageId : att.id);
                });

                _.forEach(result.checklists, chk => {
                    checklistIds.push(chk.checklistHeadRecordId);
                });

                const params = {
                    invoiceId: this.invoice.invoiceId, invoiceNr: this.invoice.invoiceNr,
                    actorCustomerId: this.customer.actorCustomerId,
                    printTimeReport: this.invoice.printTimeReport,
                    includeOnlyInvoicedTime: this.invoice.includeOnlyInvoicedTime,
                    addAttachmentsToEinvoice: this.invoice.addAttachementsToEInvoice,
                    attachmentIds: attachmentIds,
                    checklistIds: checklistIds,
                    singleRecipient: singleRecipient,
                }

                this.invoiceEditHandler.sendReport(params, this.isOrder ? OrderInvoiceRegistrationType.Order : OrderInvoiceRegistrationType.Offer, true, reportId, 0, false, recs, result.emailTemplateId, result.mergePdfs);
            });
        });
    }

    private printFromDialog() {

        const reportTypes: number[] = [];
        if (this.isOrder)
            reportTypes.push(SoeReportTemplateType.BillingOrder);
        else if (this.isOffer)
            reportTypes.push(SoeReportTemplateType.BillingOffer);
        else if (this.isContract)
            reportTypes.push(SoeReportTemplateType.BillingContract);

        const modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Common/Dialogs/SelectReport/SelectReport.html"),
            controller: SelectReportController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'lg',
            resolve: {
                module: () => { return null },
                reportTypes: () => { return reportTypes },
                showCopy: () => { return false },
                showEmail: () => { return true },
                copyValue: () => { return false },
                reports: () => { return null },
                defaultReportId: () => { return null },
                langId: () => { return this.customer && this.customer.sysLanguageId ? this.customer.sysLanguageId : null },
                showReminder: () => { return false },
                showLangSelection: () => { return true },
                showSavePrintout: () => { return false },
                savePrintout: () => { return false }
            }
        });

        modal.result.then((result: any) => {
            if ((result) && (result.reportId)) {
                if (result.email)
                    this.showEmailDialog(result.reportId);
                else
                    this.executePrintRequest(result.reportId, result.languageId);
            }
        });
    }

    private selectUsers() {
        this.originUserHelper.selectUsersDialog(true, true, true).then((result) => {
            if (result) {
                this.sendXEMail = result.sendMessage;
                this.setAsDirty(true);
            }
        });
    }


    private loadOrderTemplate(orderTemplateId: number): ng.IPromise<any> {
        this.selectedOrderTemplate = orderTemplateId;
        this.invoiceId = orderTemplateId;
        this.createCopy = true;
        this.copyChecklistRecords = true;
        return this.load(false, true);
    }

    private changeOrderTemplate() {
        this.$timeout(() => {
            const keys: string[] = [
                "core.warning",
                "billing.invoices.templates.load",
                "billing.invoices.templates.clear"
            ];

            this.translationService.translateMany(keys).then((terms) => {
                const modal = this.notificationService.showDialogDefButton(terms["core.warning"], this.selectedOrderTemplate ? terms["billing.invoices.templates.load"] : terms["billing.invoices.templates.clear"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel, SOEMessageBoxButton.OK);
                modal.result.then(val => {
                    //Load template
                    if (this.selectedOrderTemplate) {
                        this.loadOrderTemplate(this.selectedOrderTemplate);
                    }
                    else {
                        //Clear project?
                        this.new();
                    }
                }, (reason) => {
                    // User cancelled
                    this.selectedOrderTemplate = 0;
                });
            });
        });
    }

    private loadOrderTemplateFromProject(project: any) {
        this.loadOrderTemplate(project.orderTemplateId).then(() => {
            //should be overwritten with project values instead of template values...
            this.invoice.projectId = project.projectId;
            this.invoice.projectNr = project.number;
            if (project.priceListTypeId)
                this.setPriceListType(project.priceListTypeId);
            this.setAsDirty();
        })

    }

    private loadOrderTemplateFromProjectDialog(project: any) {

        var orderTemplateId = project.orderTemplateId;
        var orderTemplate = _.find(this.orderTemplates, x => x.id == orderTemplateId);
        var orderTemplateName = orderTemplate ? orderTemplate.name : "unknown name";

        const keys: string[] = [
            "core.warning",
            "billing.order.projectordertemplateoverwrite"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            var message = terms["billing.order.projectordertemplateoverwrite"].format(orderTemplateName, project.number);
            var modal = this.notificationService.showDialogDefButton(terms["core.warning"], message, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel, SOEMessageBoxButton.OK);
            modal.result.then(val => {
                this.loadOrderTemplateFromProject(project);
            }, (reason) => {
                // User cancelled
                this.selectedOrderTemplate = 0;
            });
        });
    }

    private showAskOpenInvoiceDialog() {
        var invoiceNbrs: string = "";
        var ids: number[] = [];

        var keys = Object.keys(this.createdInvoices); //Should contain only one
        var id = +keys[0];
        if (keys.length > 0) {
            if (this.performDirectInvoicing) {
                this.messagingService.publish(Constants.EVENT_OPEN_TAB, new TabMessage(this.terms["common.customerinvoice"] + " " + (this.createdInvoices[keys[0]] ? this.createdInvoices[keys[0]] : ""), id, BillingInvoicesEditController, { id: id, fromOrder: true }, this.urlHelperService.getGlobalUrl('Shared/Billing/Invoices/Views/edit.html')));
                this.performDirectInvoicing = false;
                this.createdInvoices = [];
            }
            else if (this.performCreateServiceOrder) {
                const termkeys: string[] = [
                    "common.customer.invoices.openorder",
                    "billing.contract.openorderquestion",
                    "common.order"
                ];

                this.translationService.translateMany(termkeys).then(terms => {
                    var dialog = this.notificationService.showDialog(terms["common.customer.invoices.openorder"], terms["billing.contract.openorderquestion"], SOEMessageBoxImage.Question, SOEMessageBoxButtons.YesNo, SOEMessageBoxSize.Medium)
                    dialog.result.then(val => {
                        if (val === true) {
                            this.messagingService.publish(Constants.EVENT_OPEN_TAB, new TabMessage(terms["common.order"] + " " + (this.createdInvoices[keys[0]] ? this.createdInvoices[keys[0]] : ""), id, EditController, { id: id, updateTab: true, feature: Feature.Billing_Order_Status }, this.urlHelperService.getGlobalUrl('Shared/Billing/Orders/Views/edit.html')));
                        }

                        this.performCreateServiceOrder = false;
                        this.createdInvoices = [];
                    });
                });
            }
            else {
                var dialog = this.notificationService.showDialog(this.terms["billing.order.transfer.openinoice"], this.terms["billing.order.transfer.openinvoicemessage"], SOEMessageBoxImage.Question, SOEMessageBoxButtons.YesNoCancel, SOEMessageBoxSize.Medium, false, false, null, false, null, this.terms["core.openandprint"], this.terms["core.open"], this.terms["core.no"], SOEMessageBoxButton.Cancel, true);
                dialog.result.then(val => {
                    if (val === true) {
                        this.messagingService.publish(Constants.EVENT_OPEN_TAB, new TabMessage(this.terms["common.customerinvoice"] + " " + (this.createdInvoices[keys[0]] ? this.createdInvoices[keys[0]] : ""), id, BillingInvoicesEditController, { id: id, fromOrder: true }, this.urlHelperService.getGlobalUrl('Shared/Billing/Invoices/Views/edit.html')));
                        this.executePrintRequest(0, 0, id);
                    }
                    else if (val === false) {
                        this.messagingService.publish(Constants.EVENT_OPEN_TAB, new TabMessage(this.terms["common.customerinvoice"] + " " + (this.createdInvoices[keys[0]] ? this.createdInvoices[keys[0]] : ""), id, BillingInvoicesEditController, { id: id, fromOrder: true }, this.urlHelperService.getGlobalUrl('Shared/Billing/Invoices/Views/edit.html')));
                    }

                    // Clear collection
                    this.createdInvoices = [];
                });
            }
        }
        return;
    }

    private showYourReferenceInfo() {
        var reference = this.customerReferences.filter((x) => x.name == this.invoice.referenceYour);
        if (reference && reference.length > 0) {
            this.invoiceEditHandler.showContactInfo(reference[0].id);
        }
    }

    private emailChanging() {
        this.$timeout(() => {
            if (this.invoice.contactEComId && this.invoice.contactEComId > 0)
                this.invoice.customerEmail = undefined;
            else {
                const email = _.find(this.customerEmails, e => e.id === this.invoice.contactEComId);
                if (email && !StringUtility.isEmpty(email.name))
                    this.invoice.customerEmail = email.name;
            }
        });
    }

    private editEmailAddress() {
        const modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Shared/Billing/Dialogs/OneTimeCustomer/OneTimeCustomer.html"),
            controller: OneTimeCustomerController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'sm',
            resolve: {
                translationService: () => { return this.translationService },
                coreService: () => { return this.coreService },
                name: () => { return "" },
                deliveryAddress: () => { return "" },
                phone: () => { return "" },
                email: () => { return this.invoice.customerEmail },
                isFinvoiceCustomer: () => { return this.customer.isFinvoiceCustomer },
                isLocked: () => { return this.isLocked },
                isEmailMode: () => { return true }
            }
        });

        modal.result.then((result: any) => {
            if (result) {
                this.invoice.customerEmail = result.email;
                this.customerEmails[0].name = result.email;
                this.invoice.contactEComId = 0;
                this.setAsDirty();
            }
        });
    }

    private editDeliveryAddress() {
        var tmpInvoiceHeadText: string = this.invoice.invoiceHeadText;

        if (this.invoice.deliveryAddressId && this.invoice.deliveryAddressId != 0) {
            const deliveryAdress = _.find(this.invoiceEditHandler.deliveryAddresses, i => i.contactAddressId == this.invoice.deliveryAddressId);
            if (deliveryAdress)
                tmpInvoiceHeadText = this.invoiceEditHandler.formatDeliveryAddress(deliveryAdress.contactAddressRows, this.customer.isFinvoiceCustomer);
        }

        const modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Shared/Billing/Dialogs/EditDeliveryAddress/EditDeliveryAddress.html"),
            controller: EditDeliveryAddressController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'sm',
            resolve: {
                translationService: () => { return this.translationService },
                coreService: () => { return this.coreService },
                deliveryAddress: () => { return tmpInvoiceHeadText },
                isFinvoiceCustomer: () => { return this.customer.isFinvoiceCustomer },
                isLocked: () => { return this.isLocked }
            }
        });

        modal.result.then((result: any) => {
            if ((result) && (result.deliveryAddress != null)) {
                if (result.deliveryAddress !== tmpInvoiceHeadText) {
                    this.invoice.invoiceHeadText = result.deliveryAddress;
                    this.invoiceEditHandler.deliveryAddresses[0].address = result.deliveryAddress;
                    this.invoice.deliveryAddressId = 0;
                    if (!this.invoice.billingAddressId && this.useDeliveryAddressAsInvoiceAddress) {
                        this.invoiceEditHandler.invoiceAddresses[0].address = result.deliveryAddress;
                        this.invoice.billingAdressText = result.deliveryAddress;
                        this.invoice.billingAddressId = 0;
                    }
                    this.setAsDirty();
                }
            }
        });
    }

    // Called from product rows when changing attest states
    private changeAttestStates(canCreateInvoice: boolean, directInvoicing: boolean) {
        this.createInvoiceWhenOrderReady = (canCreateInvoice && this.askCreateInvoiceWhenOrderReady);
        this.save(false);
    }

    private save(closeAfterSave: boolean, discardConcurrencyCheck = false, ignoreChecklists = false, afterCompletedCallback: () => void = undefined, overrideDirtyCheck = false, autoSave = false) {
        if (this.productRowsRendered) {
            this.$scope.$broadcast('stopEditing', {
                functionComplete: (source: string) => {
                    if (source === "productrows") {
                        this.savePhase2(closeAfterSave, discardConcurrencyCheck, ignoreChecklists, afterCompletedCallback, overrideDirtyCheck, autoSave)
                    }
                }
            });
        }
        else {
            this.savePhase2(closeAfterSave, discardConcurrencyCheck, ignoreChecklists, afterCompletedCallback, overrideDirtyCheck, autoSave);
        }
    }

    private savePhase2(closeAfterSave: boolean, discardConcurrencyCheck = false, ignoreChecklists = false, afterCompletedCallback: () => void = undefined, overrideDirtyCheck = false, autoSave = false) {
        if (this['edit'].$invalid) {
            console.log("Save called with invalid form");
            return;
        }

        if (!this.dirtyHandler.isDirty && !overrideDirtyCheck) {
            if (closeAfterSave) {
                this.closeMe(true)
            }
            return
        }

        //Validate template name
        if (this.invoice.isTemplate) {
            const existing = _.find(this.orderTemplates, (t) => t.name === this.invoice.originDescription && t.id !== this.invoiceId);
            if (existing) {
                this.translationService.translate("common.templateexists").then((text) => {
                    this.notificationService.showDialogEx(this.terms["core.warning"], text.format(this.invoice.originDescription), SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
                });
                return;
            }
        }

        // Contact ecom double check
        if (this.invoice.contactEComId && !_.find(this.customerEmails, { 'id': this.invoice.contactEComId }))
            this.invoice.contactEComId = undefined;

        //lock so autosave dosent starts....
        this.executing = true;

        let saving: ng.IPromise<any>;
        if (this.isOrder) {
            saving = this.saveOrder(closeAfterSave, discardConcurrencyCheck, ignoreChecklists, afterCompletedCallback, autoSave);
        }
        else if (this.isOffer) {
            saving = this.saveOffer(closeAfterSave, discardConcurrencyCheck, ignoreChecklists, afterCompletedCallback);
        }
        else if (this.isContract) {
            saving = this.saveContract(closeAfterSave, discardConcurrencyCheck, ignoreChecklists, afterCompletedCallback);
        }

        if (saving) {
            saving.then(() => {
                this.executing = false;
                this.dirtyHandler.clean();
            });
        }
        else {
            this.executing = false;
        }
    }

    private saveOrder(closeAfterSave: boolean, discardConcurrencyCheck = false, ignoreChecklists = false, afterCompletedCallback: () => void = undefined, autoSave: boolean = false): ng.IPromise<any> {
        // Check list
        const checklistHeadsToSave: ChecklistHeadRecordCompactDTO[] = [];
        const checklistRowsToSave: ChecklistExtendedRowDTO[] = [];

        let mandatoryRowCount = 0;

        if (this.checklistHeads && !(this.isNew && this.invoice.orderType === TermGroup_OrderType.Sales)) {
            this.checklistHeads.forEach((head) => {
                if (head.state === SoeEntityState.Deleted) {
                    head.checklistRowRecords = null;
                    if (head.checklistHeadRecordId && head.checklistHeadRecordId > 0)
                        checklistHeadsToSave.push(head);
                }
                else { //if (!head.checklistHeadRecordId || head.checklistHeadRecordId === 0 ) {
                    checklistHeadsToSave.push(head);
                }

                if (head.checklistRowRecords) {
                    head.checklistRowRecords.forEach((r) => {
                        if (r.mandatory === true && this.checklistRowHasAnswer(r) === false)
                            mandatoryRowCount = mandatoryRowCount + 1;
                        if (r.isModified) {
                            if (r.type === TermGroup_ChecklistRowType.MultipleChoice) {
                                const data = r.selectOption.find(x => x.id === r.intData)
                                if (data)
                                    r.strData = data.name;
                            }
                            checklistRowsToSave.push(r);
                        }
                    });
                }
            });
        }

        if (!ignoreChecklists) {
            if (mandatoryRowCount > 0) {
                const message = mandatoryRowCount > 1 ? this.terms["billing.invoices.checklists.hasmandatorymany"].format(mandatoryRowCount.toString()) : this.terms["billing.invoices.checklist.hasmandatorysingle"].format(mandatoryRowCount.toString());
                const modal = this.notificationService.showDialog(this.terms["core.verifyquestion"], message, SOEMessageBoxImage.Question, SOEMessageBoxButtons.OKCancel);
                modal.result.then(val => {
                    if (val != null && val === true) {

                        this.savePhase2(closeAfterSave, discardConcurrencyCheck, true, afterCompletedCallback, false, autoSave);
                    }
                });
                return null;
            }
        }

        return this.progress.startSaveProgress((completion) => {
            // Handle project
            if ((!this.invoice.projectId || this.invoice.projectId === 0) && this.autoGenerateProject)
                this.invoice.projectNr = "USEORDERNBR";

            if (this.invoice.deliveryAddressId > 0) {
                this.invoice.invoiceHeadText = null;
                this.invoiceEditHandler.deliveryAddresses[0].address = "";
            }

            if (this.invoice.contactEComId > 0) {
                this.invoice.customerEmail = null;
                this.customerEmails[0].name = "";
            }

            if (!this.selectedShiftType && this.previousShiftTypeId > 0)
                this.invoice.shiftTypeId = this.previousShiftTypeId;

            let modifiedFields = null;
            if (this.isNew) {
                modifiedFields = CoreUtility.toDTO(this.invoice, OrderDTO.getPropertiesToSkipOnSave(), true);
            }
            else {
                modifiedFields = CoreUtility.diffDTO(this.originalInvoice, this.invoice, OrderDTO.getPropertiesToSkipOnSave(), true);
                modifiedFields['id'] = this.invoice.invoiceId ? this.invoice.invoiceId : 0;
            }

            //Make sure originstatus is present
            if (!modifiedFields['originstatus'])
                modifiedFields['originstatus'] = this.invoice.originStatus;

            //Always send main
            if (!modifiedFields['maininvoiceid'])
                modifiedFields['maininvoiceid'] = this.invoice.mainInvoiceId;


            if (this.originalInvoice)
                modifiedFields['modified'] = this.originalInvoice.modified; // this.originalInvoice.modified ? CalendarUtility.convertToDate(this.originalInvoice.modified).toFormattedDateTime() : null;

            //modifiedFields['istemplate'] = this.saveAsTemplate;
            modifiedFields['checkconflictsonsave'] = this.checkConflictsOnSave;

            // Check if renumbering needs to be done
            var firstRowToRemove = undefined;
            var lastRow = undefined;
            _.forEach(_.orderBy(_.filter(this.invoice.customerInvoiceRows, r => r.state === SoeEntityState.Active), 'rowNr'), (r) => {
                if (!firstRowToRemove && r.type === SoeInvoiceRowType.ProductRow && (!r.productId || r.productId === 0))
                    firstRowToRemove = r.rowNr;
                lastRow = r.rowNr;
            });

            if (firstRowToRemove && firstRowToRemove !== lastRow) {
                var i: number = 1;
                _.forEach(_.orderBy(_.filter(this.invoice.customerInvoiceRows, r => r.state === SoeEntityState.Active && (r.type === SoeInvoiceRowType.ProductRow && r.productId && r.productId > 0) || r.type === SoeInvoiceRowType.TextRow || r.type === SoeInvoiceRowType.PageBreakRow || r.type === SoeInvoiceRowType.SubTotalRow), 'rowNr'), r => {
                    r.rowNr = i++;
                    r.isModified = true;
                });
            }

            // New product rows
            var newRows = _.filter(this.invoice.customerInvoiceRows, r => !r.customerInvoiceRowId && (r.type != SoeInvoiceRowType.ProductRow || (r.productId && r.productId > 0)));

            // Modified product rows (only modified fields)
            var modifiedRows: any[] = [];
            _.forEach(_.filter(this.invoice.customerInvoiceRows, r => r.customerInvoiceRowId && r.isModified), row => {
                var origRow: ProductRowDTO = new ProductRowDTO();
                angular.extend(origRow, _.find(this.originalInvoice.customerInvoiceRows, r => r.customerInvoiceRowId == row.customerInvoiceRowId));
                if (origRow) {
                    var rowDiffs = CoreUtility.diffDTO(origRow, row, ProductRowDTO.getPropertiesToSkipOnSave(), true);
                    if (row.quantity !== origRow.quantity)
                        rowDiffs["quantity"] = row.quantity;
                    if (row.purchasePriceCurrency !== origRow.purchasePriceCurrency)
                        rowDiffs["purchasepricecurrency"] = row.purchasePriceCurrency;
                    rowDiffs["customerinvoicerowid"] = origRow.customerInvoiceRowId;
                    rowDiffs["type"] = origRow.type;
                    rowDiffs["rownr"] = row.rowNr;
                    rowDiffs["state"] = row.state;
                    modifiedRows.push(rowDiffs);
                } else {
                    newRows.push(row);
                }
            });

            // Extra seat belt, suspenders and parachute
            newRows = this.resetTempRowIds(newRows);

            const filesDto = this.invoiceFilesHelper.getAsDTOs(true);

            //Signatures

            _.forEach(_.filter(this.signatures, s => s['isDeleted'] === true), (s) => {
                var file = new FileUploadDTO();
                file.imageId = s.imageId;
                file.isDeleted = true;
                filesDto.push(file);
            });

            //OriginUser
            const users = this.originUserHelper.getOriginUserDTOs();
            this.orderService.saveOrder(modifiedFields, newRows, modifiedRows, checklistHeadsToSave, checklistRowsToSave, users, filesDto, discardConcurrencyCheck, this.sendXEMail, autoSave).then(result => {
                if (result.success && result.booleanValue) {
                    if (!this.invoiceId || this.invoiceId === 0)
                        this.invoiceId = result.integerValue;

                    completion.completed(this.getSaveEvent(), this.invoice);

                    if (this.templateOpenFromAgreement) {
                        this.messagingService.publish(Constants.EVENT_AGREEMENT_RELOAD_ORDERTEMPLATES, { guid: this.parentGuid, resetId: this.invoiceId });

                        if (closeAfterSave) {
                            this.dirtyHandler.clean();
                            this.closeMe(true);
                        }
                        else {
                            if (this.autoSaveInterval > 0 && !this.isLocked)
                                this.startAutoSaveTimer();

                            this.$scope.$applyAsync(() => this.load());
                        }
                    }
                    else {
                        if (this.autoSaveInterval > 0 && !this.isLocked)
                            this.startAutoSaveTimer();

                        this.$scope.$applyAsync(() => this.load());
                    }
                }
                else if (result.success) {
                    const seqNr = result.value ? result.value : 0;
                    if (this.invoice) {
                        if (result.integerValue && (!this.invoice.invoiceId || this.invoice.invoiceId === 0)) {
                            this.invoice.invoiceId = result.integerValue;
                            if (!this.invoice.isTemplate) {
                                this.invoice.seqNr = seqNr;
                                this.invoice.invoiceNr = seqNr;
                            }
                        }

                        if (this.isNew) {
                            this.invoice.created = result.modified;
                            this.invoice.createdBy = result.modifiedBy;
                        }
                        else {
                            this.invoice.modified = result.modified;
                            this.invoice.modifiedBy = result.modifiedBy;
                        }

                        if (this.originalInvoice) {
                            this.originalInvoice.modified = result.modified;
                            this.originalInvoice.modifiedBy = result.modifiedBy;
                        }

                        if (result.value2) {
                            this.invoice.projectId = result.value2.projectId;
                            this.invoice.projectNr = result.value2.number;
                        }
                    }

                    if (!this.invoiceId || this.invoiceId === 0)
                        this.invoiceId = result.integerValue;

                    if (result.intDict) {
                        var temprows = [];
                        _.forEach(_.filter(this.invoice.customerInvoiceRows, r => (r.type != SoeInvoiceRowType.ProductRow || (r.productId && r.productId > 0))), (row) => {
                            if (!row.customerInvoiceRowId || row.customerInvoiceRowId === 0 || row.isModified) {
                                row.isModified = false;
                                if (result.intDict[row.tempRowId]) {
                                    row.customerInvoiceRowId = result.intDict[row.tempRowId];
                                    row.isModified = false;
                                    row.modified = result.modified;
                                    row.modifiedBy = result.modifiedBy;
                                }
                                if (row.parentRowId && result.intDict[row.parentRowId]) {
                                    row.parentRowId = result.intDict[row.parentRowId];
                                }
                                row.splitAccountingRows = undefined;
                            }
                            temprows.push(row);
                        });
                        this.invoice.customerInvoiceRows = temprows;
                    }
                    else {
                        // Remove split accounting
                        _.forEach(_.filter(this.invoice.customerInvoiceRows, r => !r.customerInvoiceRowId || r.customerInvoiceRowId === 0 || r.isModified), (row) => {
                            row.isModified = false;
                            row.splitAccountingRows = undefined;
                        });
                    }

                    // Remove empty rows
                    this.invoice.customerInvoiceRows = _.filter(this.invoice.customerInvoiceRows, r => r.customerInvoiceRowId && r.customerInvoiceRowId > 0);

                    if (this.invoice.isTemplate) {
                        //reload templates ddl
                        this.loadTemplates(false);
                        this.updateTabCaption();
                    }

                    completion.completed(this.getSaveEvent(), this.invoice);

                    if (this.isModal)
                        this.closeModal();
                    else if (closeAfterSave) {
                        this.dirtyHandler.clean();

                        if (this.templateOpenFromAgreement)
                            this.messagingService.publish(Constants.EVENT_AGREEMENT_RELOAD_ORDERTEMPLATES, { guid: this.parentGuid, resetId: this.invoiceId });

                        this.closeMe(true);
                    } else {
                        if (this.isNew) {
                            this.isNew = false;
                            this.updateTabCaption();
                        }

                        this.setOrderExpanderLabel();

                        if (this.timeProjectRowsExpanderIsOpen)
                            this.loadTimeProjectRows();

                        //Close expanders in order to reload whats necessary when opened
                        //this.timeProjectRowsExpanderIsOpen = false;

                        //Reload checklists
                        if (this.checklistsLoaded) {
                            this.checklistsLoaded = false;
                            this.$scope.$broadcast('reloadChecklists', null);
                        }

                        if (this.invoiceFilesHelper.filesLoaded) {
                            this.documentExpanderIsOpen = false;
                            this.invoiceFilesHelper.loadFiles(true, this.invoice && this.invoice.projectId ? this.invoice.projectId : 0).then(() => {
                                this.resetDocumentsGridData = true;
                            });
                        }

                        if (this.accountingRowsExpanderIsOpen && this.hasModifiedProductRows) {
                            this.loadAccountRows();
                        }
                        else {
                            this.accountingRowsExpanderIsOpen = false;
                            this.accountRows = undefined;
                        }

                        //Set original
                        this.originalInvoice = new OrderDTO();
                        angular.extend(this.originalInvoice, CoreUtility.cloneDTO(this.invoice));

                        this.hasModifiedProductRows = false;

                        if (this.autoSaveInterval > 0 && !this.isLocked)
                            this.startAutoSaveTimer();

                        this.$scope.$applyAsync(() => this.dirtyHandler.clean());

                        if (this.templateOpenFromAgreement)
                            this.messagingService.publish(Constants.EVENT_AGREEMENT_RELOAD_ORDERTEMPLATES, { guid: this.parentGuid, resetId: this.invoiceId });
                    }

                    this.projectNotSaved = false;

                    if (this.createInvoiceWhenOrderReady) {
                        if (this.performDirectInvoicing) {
                            this.transfer();
                            this.createInvoiceWhenOrderReady = false;
                        }
                        else {
                            const keys: string[] = [
                                "billing.order.createinvoice",
                                "billing.order.askcreateinvoicetext",
                                "billing.order.preliminary",
                                "billing.order.definitive",
                                "core.no"
                            ];

                            this.translationService.translateMany(keys).then(terms => {
                                var dialog = this.notificationService.showDialog(terms["billing.order.createinvoice"], terms["billing.order.askcreateinvoicetext"], SOEMessageBoxImage.Question, SOEMessageBoxButtons.YesNoCancel, SOEMessageBoxSize.Medium, false, false, null, false, null, terms["billing.order.preliminary"], terms["billing.order.definitive"], terms["core.no"], SOEMessageBoxButton.Cancel, true);
                                dialog.result.then(val => {
                                    if (val === true) {
                                        this.transferType = OrderEditTransferFunctions.TransferToPreliminaryInvoice;
                                        this.validateTransfer();
                                    }
                                    else if (val === false) {
                                        this.transferType = OrderEditTransferFunctions.TransferToDefinitiveInvoice;
                                        this.validateTransfer();
                                    }
                                    this.createInvoiceWhenOrderReady = false;
                                });
                            });
                        }
                    }

                    if (afterCompletedCallback) {
                        this.$scope.$applyAsync(() => afterCompletedCallback())
                    }
                }
                else {
                    if (result.errorNumber === ActionResultSave.EntityIsModifiedByOtherUser) {
                        this.$timeout(() => {
                            completion.failed("");
                            const modal = this.notificationService.showDialog(this.terms["core.warning"], result.errorMessage, SOEMessageBoxImage.Question, SOEMessageBoxButtons.YesNo);
                            modal.result.then(val => {
                                if (val === true) {
                                    this.save(closeAfterSave, true, ignoreChecklists, afterCompletedCallback);
                                }
                            });
                        });
                    }
                    else {
                        completion.failed(result.errorMessage);
                        this.performDirectInvoicing = false;
                    }
                }
            });
        }, this.guid);
    }

    private saveOffer(closeAfterSave: boolean, discardConcurrencyCheck = false, ignoreChecklists = false, afterCompletedCallback: () => void = undefined): ng.IPromise<any> {
        return this.progress.startSaveProgress((completion) => {

            if (this.invoice.deliveryAddressId > 0) {
                this.invoice.invoiceHeadText = null;
            }

            if (this.invoice.contactEComId > 0) {
                this.invoice.customerEmail = null;
                this.customerEmails[0].name = "";
            }

            let modifiedFields = null;
            if (this.isNew) {
                modifiedFields = CoreUtility.toDTO(this.invoice, OrderDTO.getPropertiesToSkipOnSave(), true)
            }
            else {
                modifiedFields = CoreUtility.diffDTO(this.originalInvoice, this.invoice, OrderDTO.getPropertiesToSkipOnSave(), true);
                modifiedFields['id'] = this.invoice.invoiceId ? this.invoice.invoiceId : 0;
            }

            //Make sure originstatus is present
            if (!modifiedFields['originstatus'])
                modifiedFields['originstatus'] = this.invoice.originStatus;

            if (this.originalInvoice)
                modifiedFields['modified'] = this.originalInvoice.modified;

            //modifiedFields['istemplate'] = this.saveAsTemplate;
            modifiedFields['checkconflictsonsave'] = this.checkConflictsOnSave;

            // Check if renumbering needs to be done
            var firstRowToRemove = undefined;
            var lastRow = undefined;
            _.forEach(_.orderBy(_.filter(this.invoice.customerInvoiceRows, r => r.state === SoeEntityState.Active), 'rowNr'), (r) => {
                if (!firstRowToRemove && r.type === SoeInvoiceRowType.ProductRow && (!r.productId || r.productId === 0))
                    firstRowToRemove = r.rowNr;
                lastRow = r.rowNr;
            });

            if (firstRowToRemove && firstRowToRemove !== lastRow) {
                var i: number = 1;
                _.forEach(_.orderBy(_.filter(this.invoice.customerInvoiceRows, r => r.state === SoeEntityState.Active && (r.type === SoeInvoiceRowType.ProductRow && r.productId && r.productId > 0) || r.type === SoeInvoiceRowType.TextRow || r.type === SoeInvoiceRowType.PageBreakRow || r.type === SoeInvoiceRowType.SubTotalRow), 'rowNr'), r => {
                    r.rowNr = i++;
                    r.isModified = true;
                });
            }

            // New product rows
            let newRows = _.filter(this.invoice.customerInvoiceRows, r => !r.customerInvoiceRowId && (r.type != SoeInvoiceRowType.ProductRow || (r.productId && r.productId > 0)));

            // Modified product rows (only modified fields)
            const modifiedRows: any[] = [];
            _.forEach(_.filter(this.invoice.customerInvoiceRows, r => r.customerInvoiceRowId && r.isModified), row => {
                var origRow: ProductRowDTO = new ProductRowDTO();
                angular.extend(origRow, _.find(this.originalInvoice.customerInvoiceRows, r => r.customerInvoiceRowId == row.customerInvoiceRowId));
                if (origRow) {
                    var rowDiffs = CoreUtility.diffDTO(origRow, row, ProductRowDTO.getPropertiesToSkipOnSave(), true);
                    rowDiffs["customerinvoicerowid"] = origRow.customerInvoiceRowId;
                    if (row.quantity !== origRow.quantity)
                        rowDiffs["quantity"] = row.quantity;
                    if (row.purchasePriceCurrency !== origRow.purchasePriceCurrency)
                        rowDiffs["purchasepricecurrency"] = row.purchasePriceCurrency;
                    rowDiffs["type"] = origRow.type;
                    rowDiffs["rownr"] = row.rowNr;
                    rowDiffs["state"] = row.state;
                    modifiedRows.push(rowDiffs);
                } else {
                    newRows.push(row);
                }
            });

            // Extra seat belt, suspenders and parachute
            newRows = this.resetTempRowIds(newRows);

            const filesDto = this.invoiceFilesHelper.getAsDTOs(true);
            //Signatures

            //OriginUser
            const users = this.originUserHelper.getOriginUserDTOs();

            this.orderService.saveOffer(modifiedFields, newRows, modifiedRows, users, filesDto, discardConcurrencyCheck).then(result => {

                if (result.success && result.booleanValue) {
                    if (!this.invoiceId || this.invoiceId === 0)
                        this.invoiceId = result.integerValue;

                    completion.completed(this.getSaveEvent(), this.invoice);

                    if (this.autoSaveInterval > 0 && !this.isLocked)
                        this.startAutoSaveTimer();

                    this.$scope.$applyAsync(() => this.load());
                }
                else if (result.success) {
                    const seqNr = result.value ? result.value : 0;
                    if (this.invoice) {
                        if (result.integerValue && (!this.invoice.invoiceId || this.invoice.invoiceId === 0)) {
                            this.invoice.invoiceId = result.integerValue;
                            if (!this.invoice.isTemplate) {
                                this.invoice.seqNr = seqNr;
                                this.invoice.invoiceNr = seqNr;
                            }
                        }

                        if (this.isNew) {
                            this.invoice.created = result.modified;
                            this.invoice.createdBy = result.modifiedBy;
                        }
                        else {
                            this.invoice.modified = result.modified;
                            this.invoice.modifiedBy = result.modifiedBy;
                        }

                        if (this.originalInvoice) {
                            this.originalInvoice.modified = result.modified;
                            this.originalInvoice.modifiedBy = result.modifiedBy;
                        }

                        if (result.value2) {
                            this.invoice.projectId = result.value2.projectId;
                            this.invoice.projectNr = result.value2.number;
                        }
                    }

                    if (!this.invoiceId || this.invoiceId === 0)
                        this.invoiceId = result.integerValue;

                    if (result.intDict) {
                        let temprows = [];

                        let rows = this.invoice.customerInvoiceRows.filter(r => (r.type != SoeInvoiceRowType.ProductRow || (r.productId && r.productId > 0)));

                        for (let row of rows) {
                            if (!row.customerInvoiceRowId || row.customerInvoiceRowId === 0 || row.isModified) {
                                row.isModified = false;
                                if (result.intDict[row.tempRowId]) {
                                    row.customerInvoiceRowId = result.intDict[row.tempRowId];
                                    row.isModified = false;
                                    row.modified = result.modified;
                                    row.modifiedBy = result.modifiedBy;
                                }
                                if (row.parentRowId && result.intDict[row.parentRowId]) {
                                    row.parentRowId = result.intDict[row.parentRowId];
                                }
                                row.splitAccountingRows = undefined;
                            }
                            temprows.push(row);
                        }
                    }
                    else {
                        // Remove split accounting
                        _.forEach(_.filter(this.invoice.customerInvoiceRows, r => !r.customerInvoiceRowId || r.customerInvoiceRowId === 0 || r.isModified), (row) => {
                            row.isModified = false;
                            row.splitAccountingRows = undefined;
                        });
                    }

                    // Remove empty rows
                    this.invoice.customerInvoiceRows = _.filter(this.invoice.customerInvoiceRows, r => r.customerInvoiceRowId && r.customerInvoiceRowId > 0);

                    if (this.invoice.isTemplate) {
                        //reload templates ddl
                        this.loadTemplates(false);
                        this.updateTabCaption();
                    }

                    completion.completed(this.getSaveEvent(), this.invoice);

                    if (this.isModal)
                        this.closeModal();
                    else if (closeAfterSave) {
                        this.closeMe(true);
                    } else {
                        if (this.isNew) {
                            this.isNew = false;
                            this.updateTabCaption();
                        }

                        if (filesDto && (filesDto.length > 0) && this.invoiceFilesHelper.filesLoaded) {
                            this.documentExpanderIsOpen = false;
                            this.invoiceFilesHelper.loadFiles(true, this.invoice && this.invoice.projectId ? this.invoice.projectId : 0);
                        }

                        if (this.accountingRowsExpanderIsOpen && this.hasModifiedProductRows) {
                            this.loadAccountRows();
                        }
                        else {
                            this.accountingRowsExpanderIsOpen = false;
                            this.accountRows = undefined;
                        }

                        //Set original
                        this.originalInvoice = new OrderDTO();
                        angular.extend(this.originalInvoice, CoreUtility.cloneDTO(this.invoice));

                        this.hasModifiedProductRows = false;

                        if (this.autoSaveInterval > 0 && !this.isLocked)
                            this.startAutoSaveTimer();
                    }

                    this.projectNotSaved = false;

                }
                else {
                    if (result.errorNumber === ActionResultSave.EntityIsModifiedByOtherUser) {
                        this.$timeout(() => {
                            completion.failed("");

                            var modal = this.notificationService.showDialog(this.terms["core.warning"], result.errorMessage, SOEMessageBoxImage.Question, SOEMessageBoxButtons.YesNo);
                            modal.result.then(val => {
                                if (val === true) {
                                    this.save(closeAfterSave, true, ignoreChecklists);
                                }
                            });
                        });
                    }
                    else {
                        completion.failed(result.errorMessage);
                    }
                }
            });
        }, this.guid);
    }

    private saveContract(closeAfterSave: boolean, discardConcurrencyCheck = false, ignoreChecklists = false, afterCompletedCallback: () => void = undefined): ng.IPromise<any> {
        return this.progress.startSaveProgress((completion) => {
            if (this.invoice.deliveryAddressId > 0) {
                this.invoice.invoiceHeadText = null;
                this.invoiceEditHandler.deliveryAddresses[0].address = "";
            }

            if (this.invoice.contactEComId > 0) {
                this.invoice.customerEmail = null;
                this.customerEmails[0].name = "";
            }

            var modifiedFields = null;
            if (this.isNew) {
                modifiedFields = CoreUtility.toDTO(this.invoice, OrderDTO.getPropertiesToSkipOnSave(), true)
            }
            else {
                modifiedFields = CoreUtility.diffDTO(this.originalInvoice, this.invoice, OrderDTO.getPropertiesToSkipOnSave(), true);
                modifiedFields['id'] = this.invoice.invoiceId ? this.invoice.invoiceId : 0;
            }

            //Make sure originstatus is present
            if (!modifiedFields['originstatus'])
                modifiedFields['originstatus'] = this.invoice.originStatus;

            if (this.originalInvoice)
                modifiedFields['modified'] = this.originalInvoice.modified;

            modifiedFields['checkconflictsonsave'] = this.checkConflictsOnSave;

            // Check if renumbering needs to be done
            var firstRowToRemove = undefined;
            var lastRow = undefined;

            _.forEach(_.orderBy(_.filter(this.invoice.customerInvoiceRows, r => r.state === SoeEntityState.Active), 'rowNr'), (r) => {
                if (!firstRowToRemove && r.type === SoeInvoiceRowType.ProductRow && (!r.productId || r.productId === 0))
                    firstRowToRemove = r.rowNr;
                lastRow = r.rowNr;
            });

            if (firstRowToRemove && firstRowToRemove !== lastRow) {
                var i: number = 1;
                _.forEach(_.orderBy(_.filter(this.invoice.customerInvoiceRows, r => r.state === SoeEntityState.Active && (r.type === SoeInvoiceRowType.ProductRow && r.productId && r.productId > 0) || r.type === SoeInvoiceRowType.TextRow || r.type === SoeInvoiceRowType.PageBreakRow || r.type === SoeInvoiceRowType.SubTotalRow), 'rowNr'), r => {
                    r.rowNr = i++;
                    r.isModified = true;
                });
            }

            // New product rows
            var newRows = _.filter(this.invoice.customerInvoiceRows, r => !r.customerInvoiceRowId && (r.type != SoeInvoiceRowType.ProductRow || (r.productId && r.productId > 0)));

            // Modified product rows (only modified fields)
            var modifiedRows: any[] = [];
            _.forEach(_.filter(this.invoice.customerInvoiceRows, r => r.customerInvoiceRowId && r.isModified), row => {
                var origRow: ProductRowDTO = new ProductRowDTO();
                angular.extend(origRow, _.find(this.originalInvoice.customerInvoiceRows, r => r.customerInvoiceRowId == row.customerInvoiceRowId));
                if (origRow) {
                    var rowDiffs = CoreUtility.diffDTO(origRow, row, ProductRowDTO.getPropertiesToSkipOnSave(), true);
                    rowDiffs["customerinvoicerowid"] = origRow.customerInvoiceRowId;
                    if (row.quantity !== origRow.quantity)
                        rowDiffs["quantity"] = row.quantity;
                    if (row.purchasePriceCurrency !== origRow.purchasePriceCurrency)
                        rowDiffs["purchasepricecurrency"] = row.purchasePriceCurrency;
                    rowDiffs["type"] = origRow.type;
                    rowDiffs["rownr"] = row.rowNr;
                    rowDiffs["state"] = row.state;
                    modifiedRows.push(rowDiffs);
                } else {
                    newRows.push(row);
                }
            });

            // Extra seat belt, suspenders and parachute
            newRows = this.resetTempRowIds(newRows);

            var filesDto = this.invoiceFilesHelper.getAsDTOs(true);

            //OriginUser
            const users = this.originUserHelper.getOriginUserDTOs();

            this.orderService.saveContract(modifiedFields, newRows, modifiedRows, users, filesDto, discardConcurrencyCheck).then(result => {

                if (result.success && result.booleanValue) {
                    if (!this.invoiceId || this.invoiceId === 0)
                        this.invoiceId = result.integerValue;

                    completion.completed(this.getSaveEvent(), this.invoice);

                    if (this.autoSaveInterval > 0 && !this.isLocked)
                        this.startAutoSaveTimer();

                    this.$scope.$applyAsync(() => this.load());
                }
                else if (result.success && result.booleanValue2) {
                    if (!this.invoiceId || this.invoiceId === 0)
                        this.invoiceId = result.integerValue;

                    completion.completed(this.getSaveEvent(), this.invoice);

                    if (closeAfterSave) {
                        this.dirtyHandler.clean();
                        this.closeMe(true);
                    } else {
                        this.$scope.$applyAsync(() => this.load(true));
                    }

                }
                else if (result.success) {

                    var seqNr = result.value ? result.value : 0;
                    if (this.invoice) {
                        if (result.integerValue && (!this.invoice.invoiceId || this.invoice.invoiceId === 0)) {
                            this.invoice.invoiceId = result.integerValue;
                            if (!this.invoice.isTemplate) {
                                this.invoice.seqNr = seqNr;
                                this.invoice.invoiceNr = seqNr;
                            }
                        }

                        if (this.isNew) {
                            this.invoice.created = result.modified;
                            this.invoice.createdBy = result.modifiedBy;
                        }
                        else {
                            this.invoice.modified = result.modified;
                            this.invoice.modifiedBy = result.modifiedBy;
                        }

                        if (this.originalInvoice) {
                            this.originalInvoice.modified = result.modified;
                            this.originalInvoice.modifiedBy = result.modifiedBy;
                        }

                        if (result.value2) {
                            this.invoice.projectId = result.value2.projectId;
                            this.invoice.projectNr = result.value2.number;
                        }
                    }

                    if (!this.invoiceId || this.invoiceId === 0)
                        this.invoiceId = result.integerValue;

                    if (result.intDict) {
                        var temprows = [];
                        _.forEach(_.filter(this.invoice.customerInvoiceRows, r => (r.type != SoeInvoiceRowType.ProductRow || (r.productId && r.productId > 0))), (row) => {
                            if (!row.customerInvoiceRowId || row.customerInvoiceRowId === 0 || row.isModified) {
                                row.isModified = false;
                                if (result.intDict[row.tempRowId]) {
                                    row.customerInvoiceRowId = result.intDict[row.tempRowId];
                                    row.isModified = false;
                                    row.modified = result.modified;
                                    row.modifiedBy = result.modifiedBy;
                                }
                                if (row.parentRowId && result.intDict[row.parentRowId]) {
                                    row.parentRowId = result.intDict[row.parentRowId];
                                }
                                row.splitAccountingRows = undefined;
                            }
                            temprows.push(row);
                        });
                        this.invoice.customerInvoiceRows = temprows;
                    }
                    else {
                        // Remove split accounting
                        _.forEach(_.filter(this.invoice.customerInvoiceRows, r => !r.customerInvoiceRowId || r.customerInvoiceRowId === 0 || r.isModified), (row) => {
                            row.isModified = false;
                            row.splitAccountingRows = undefined;
                        });
                    }

                    // Remove empty rows
                    this.invoice.customerInvoiceRows = _.filter(this.invoice.customerInvoiceRows, r => r.customerInvoiceRowId && r.customerInvoiceRowId > 0);

                    this.contractHeadIsLocked = true;

                    completion.completed(this.getSaveEvent(), this.invoice);

                    if (this.isModal)
                        this.closeModal();
                    else if (closeAfterSave) {
                        this.closeMe(true);
                    } else {
                        if (this.isNew) {
                            this.isNew = false;
                            this.updateTabCaption();
                        }

                        if (this.timeProjectRowsExpanderIsOpen)
                            this.loadTimeProjectRows();

                        if (filesDto && (filesDto.length > 0) && this.invoiceFilesHelper.filesLoaded) {
                            this.documentExpanderIsOpen = false;
                            this.invoiceFilesHelper.loadFiles(true, this.invoice && this.invoice.projectId ? this.invoice.projectId : 0);
                        }

                        if (this.accountingRowsExpanderIsOpen && this.hasModifiedProductRows) {
                            this.loadAccountRows();
                        }
                        else {
                            this.accountingRowsExpanderIsOpen = false;
                            this.accountRows = undefined;
                        }

                        //Set original
                        this.originalInvoice = new OrderDTO();
                        angular.extend(this.originalInvoice, CoreUtility.cloneDTO(this.invoice));

                        this.hasModifiedProductRows = false;

                        if (this.autoSaveInterval > 0 && !this.isLocked)
                            this.startAutoSaveTimer();
                    }

                    this.projectNotSaved = false;

                    if (afterCompletedCallback) {
                        this.$scope.$applyAsync(() => afterCompletedCallback())
                    }
                }
                else {
                    if (result.errorNumber === ActionResultSave.EntityIsModifiedByOtherUser) {
                        this.$timeout(() => {
                            completion.failed("");
                            var modal = this.notificationService.showDialog(this.terms["core.warning"], result.errorMessage, SOEMessageBoxImage.Question, SOEMessageBoxButtons.YesNo);
                            modal.result.then(val => {
                                if (val === true) {
                                    this.save(closeAfterSave, true, ignoreChecklists, afterCompletedCallback);
                                }
                            });
                        });
                    }
                    else {
                        completion.failed(result.errorMessage);
                    }
                }
            });
        }, this.guid);
    }

    private resetTempRowIds(rows: ProductRowDTO[]): ProductRowDTO[] {
        var i: number = 1;
        _.forEach(rows, (r) => {
            var childRows = _.filter(rows, (cr) => cr.parentRowId === r.tempRowId);
            if (childRows && childRows.length > 0) {
                _.forEach(childRows, (c) => { c.parentRowId = i });
            }
            r.tempRowId = i;
            i = i + 1;
        });
        return rows;
    }

    private validateRowAttestStateChanged(): boolean {
        var missingMandatoryAnswer: boolean = false;
        _.forEach(_.filter(this.checklistHeads, h => h.state != SoeEntityState.Deleted), (head) => {
            _.forEach(head.checklistRowRecords, (r) => {
                if (r.mandatory === true && this.checklistRowHasAnswer(r) === false) {
                    missingMandatoryAnswer = true;
                    return false;
                }
            });
        });

        if (missingMandatoryAnswer) {
            this.notificationService.showDialog(this.terms["billing.invoices.checklist.notready"], this.terms["billing.invoices.checklist.attestnotvalid"], SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
            return false;
        }
        else {
            return true;
        }
    }

    private validateTransfer() {
        if (this.customer && this.customer.blockInvoice) {
            // Not able to transfer order to invoice if customer is blocked
            this.invoiceEditHandler.showCustomerBlockNote(this.customer, OrderInvoiceRegistrationType.Invoice);
            return;
        }

        // If transfer to definitive invoice, ask a verification question
        if (this.transferType === OrderEditTransferFunctions.TransferToDefinitiveInvoice) {
            const keys: string[] = [
                "billing.order.transfer.definitive.question",
                "billing.order.transfer.definitive.voucherquestion"
            ];

            this.translationService.translateMany(keys).then(terms => {
                const msg = this.transferToVoucher ? terms["billing.order.transfer.definitive.voucherquestion"] : terms["billing.order.transfer.definitive.question"];
                this.notificationService.showDialogEx(this.terms["billing.order.transfer.definitive"], msg, SOEMessageBoxImage.Question, SOEMessageBoxButtons.OKCancel).result.then(val => { this.validateTransferInProductRows(); });
            });
            return;
        }

        if (this.dirtyHandler.isDirty) {
            this.save(false, false, false, () => { this.validateTransferInProductRows(); });
        }
        else {
            this.validateTransferInProductRows();
        }
    }

    private validateTransferInProductRows() {
        this.performDirectInvoicing = (this.transferType === OrderEditTransferFunctions.DirectTransferToInvoice);
        if (this.productRowsRendered) {
            // Do some validations in product rows
            // Result will be returned in a suvbscription that will in turn call initTransfer()
            this.transferAllRowsAndCloseOrder = false;
            this.$scope.$broadcast('validateTransferToInvoice', { guid: this.guid, directInvoicing: this.performDirectInvoicing });
        }
        else {
            this.validateProductRowsForTransfer = true;
            this.setProductRowsRendered();
        }
    }

    private initTransfer(): ng.IPromise<boolean> {
        const deferral = this.$q.defer<boolean>();
        this.validateHouseholdAmountMismatch().then(val => {
            if (val === false)
                deferral.resolve(false);

            this.validateTransferFixedPrice().then(val => {
                if (val === false)
                    deferral.resolve(false);

                this.validateTransferContractProducts().then(val => {
                    if (val === false)
                        deferral.resolve(false);
                    else
                        deferral.resolve(true);
                });
            });
        });

        return deferral.promise;
    }

    private validateTransferFixedPrice(): ng.IPromise<boolean> {
        // Check transfer fixed price

        const deferral = this.$q.defer<boolean>();

        if (this.transferFixedPriceToInvoiceLeavingOthers) {
            const keys: string[] = [
                "billing.offer.transfer.fixedpricequestionmessage",
                "billing.order.transfer.fixedpricequestiontitle",
                "billing.order.transfer.fixedpricequestionmessage"
            ];

            this.translationService.translateMany(keys).then(terms => {
                var modal = this.notificationService.showDialog(terms["billing.order.transfer.fixedpricequestiontitle"], this.isOffer ? terms["billing.offer.transfer.fixedpricequestionmessage"] : terms["billing.order.transfer.fixedpricequestionmessage"], SOEMessageBoxImage.Question, SOEMessageBoxButtons.YesNo);
                modal.result.then(val => {
                    if (val)
                        this.keepOrderOpen = true;
                    else
                        this.keepOrderOpen = false;
                    deferral.resolve(true);
                });
            });
        }
        else
            deferral.resolve(true);

        return deferral.promise;
    }

    private validateTransferContractProducts(): ng.IPromise<boolean> {
        // Check if transferring contract products

        const deferral = this.$q.defer<boolean>();

        if (this.transferringContractProducts) {
            const keys: string[] = [
                "billing.order.transfer.contractproductquestiontitle",
                "billing.order.transfer.contractproductquestionmessage"
            ];

            this.translationService.translateMany(keys).then(terms => {
                var modal = this.notificationService.showDialog(terms["billing.order.transfer.contractproductquestiontitle"], terms["billing.order.transfer.contractproductquestionmessage"], SOEMessageBoxImage.Question, SOEMessageBoxButtons.YesNo);
                modal.result.then(val => {
                    // Yes
                    this.copyTransferredContractRows = true;
                    deferral.resolve(true);
                }, (reason) => {
                    // No
                    this.copyTransferredContractRows = false;
                    deferral.resolve(true);
                });
            });
        }
        else
            deferral.resolve(true);

        return deferral.promise;
    }

    private validateHouseholdAmountMismatch(): ng.IPromise<boolean> {
        const deferral = this.$q.defer<boolean>();
        if (this.hasDeductionAmountMismatch && this.hasDeductionAmountMismatch.value) {
            const keys: string[] = [
                "core.warning",
                "billing.productrows.dialogs.deductionvaluewarning"
            ];

            this.translationService.translateMany(keys).then(terms => {
                var modal = this.notificationService.showDialog(terms["core.warning"], terms["billing.productrows.dialogs.deductionvaluewarning"].format(this.hasDeductionAmountMismatch.existingAmount, this.hasDeductionAmountMismatch.calculatedAmount), SOEMessageBoxImage.Question, SOEMessageBoxButtons.YesNo);
                modal.result.then(val => {
                    if (val)
                        deferral.resolve(true);
                    else
                        deferral.resolve(false);
                }, (reason) => {
                    deferral.resolve(false);
                });
            });
        }
        else
            deferral.resolve(true);
        return deferral.promise;
    }

    private transfer() {
        if (this.transfering)
            return;

        var statusChange: number = SoeOriginStatusChange.None;
        var setToOrigin: boolean = true;
        var transferMessage: string = "";
        if (this.isOffer) {
            transferMessage = this.terms["billing.offer.transfered"];
            if (this.transferType === OrderEditTransferFunctions.TransferToOrder) {
                statusChange = SoeOriginStatusChange.Billing_OfferToOrder;
            }
            else if (this.transferType === OrderEditTransferFunctions.TransferToPriceOptimization) {
                statusChange = SoeOriginStatusChange.Billing_OfferToPriceOptimization;
            }
            else {
                statusChange = SoeOriginStatusChange.Billing_OfferToInvoice;
                setToOrigin = (this.transferType === OrderEditTransferFunctions.TransferToDefinitiveInvoice);
                this.transferingToInvoice = true;
            }
        }
        else if (this.isOrder) {
            transferMessage = this.terms["billing.order.transfered"];
            if (this.transferType === OrderEditTransferFunctions.TransferToPriceOptimization) {
                statusChange = SoeOriginStatusChange.Billing_OrderToPriceOptimization;
            }
            else {
                statusChange = SoeOriginStatusChange.Billing_OrderToInvoice;
                setToOrigin = (this.transferType === OrderEditTransferFunctions.TransferToDefinitiveInvoice);
                this.transferingToInvoice = true;
            }
        }
        else if (this.isContract) {
            transferMessage = this.terms["billing.contract.transfered"];
            if (this.transferType === OrderEditTransferFunctions.TransferToOrder) {
                statusChange = SoeOriginStatusChange.Billing_ContractToOrder; 
            }
            else if (this.transferType === OrderEditTransferFunctions.TransferToServiceOrderFromAgreement) {
                statusChange = SoeOriginStatusChange.Billing_ContractToServiceOrder;
                this.performCreateServiceOrder = true;
            }
            else {
                statusChange = SoeOriginStatusChange.Billing_ContractToInvoice;
                setToOrigin = (this.transferType === OrderEditTransferFunctions.TransferToDefinitiveInvoice);
                this.transferingToInvoice = true;
            }
        }

        this.progress.startWorkProgress((completion) => {
            this.transfering = true;
            var items: any = [];
            var item: any = {};
            item.customerInvoiceId = this.invoiceId;
            item.actorCustomerId = this.invoice.actorId;
            if (statusChange === SoeOriginStatusChange.Billing_ContractToOrder || statusChange === SoeOriginStatusChange.Billing_ContractToInvoice)
                item.invoiceDate = this.invoice.nextContractPeriodDate;
            items.push(item);

            this.commonCustomerService.transferCustomerInvoices(items, statusChange, this.currentAccountYearId, 0, false, 0, this.usePartialInvoicingOnOrderRow, setToOrigin, null, null, null, null, null, null, null, null, this.keepOrderOpen).then((result) => {
                this.transfering = false;
                if (result.success) {
                    this.createdInvoices = result.strDict;

                    if ((statusChange === SoeOriginStatusChange.Billing_OrderToInvoice) && (result.strDict != null)) {
                        this.messagingHandler.publishReloadGrid(this.guid);
                    }

                    completion.completed(
                        null,
                        ((this.createdInvoices != null && this.askOpenInvoiceWhenCreateInvoiceFromOrder && this.transferingToInvoice) || (result.infoMessage && result.infoMessage != "") || this.performCreateServiceOrder),
                        transferMessage);

                    if (result.infoMessage && result.infoMessage != "") {
                        this.notificationService.showDialog(this.terms["core.warning"], this.terms["common.customer.invoices.errorinaccounting"] + "<br/>" + result.infoMessage, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
                    }

                    this.load();
                    this.reloadTracingData();
                }
                else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                this.transfering = false;
                completion.failed(error.message);
            });
        });
    }

    public editOrderText() {
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Common/Dialogs/TextBlock/TextBlockDialog.html"),
            controller: TextBlockDialogController,
            controllerAs: "ctrl",
            size: 'lg',
            resolve: {
                text: () => { return this.invoice.invoiceText },
                editPermission: () => { return this.isLocked === false },
                entity: () => { return SoeEntityType.Order },
                type: () => { return TextBlockType.TextBlockEntity },
                headline: () => { return this.terms["common.customer.invoices.editordertext"] },
                mode: () => { return SimpleTextEditorDialogMode.EditInvoiceDescription },
                container: () => { return ProductRowsContainers.Order },
                langId: () => { return TermGroup_Languages.Swedish },
                maxTextLength: () => { return 995 },
                textboxTitle: () => { return undefined },
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result) {
                this.invoice.invoiceText = result.text;
                this.setAsDirty();
            }
        });
    }

    private editWorkingDescription() {
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Common/Dialogs/TextBlock/TextBlockDialog.html"),
            controller: TextBlockDialogController,
            controllerAs: "ctrl",
            size: 'lg',
            backdrop: 'static',
            resolve: {
                text: () => { return this.invoice.workingDescription },
                editPermission: () => { return this.isLocked === false },
                entity: () => { return SoeEntityType.CustomerInvoice },
                type: () => { return TextBlockType.WorkingDescription },
                headline: () => { return this.terms["billing.order.workingdescription"] },
                mode: () => { return SimpleTextEditorDialogMode.EditWorkingDescription },
                container: () => { return ProductRowsContainers.Order },
                langId: () => { return TermGroup_Languages.Swedish },
                maxTextLength: () => { return null },
                textboxTitle: () => { return undefined },
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result) {
                this.invoice.workingDescription = result.text;
                this.setAsDirty();
            }
        });
    }

    public viewOrderInformation() {
        this.$uibModal.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Shared/Billing/Dialogs/ShowCustomerInvoiceInfo/ShowCustomerInvoiceInfo.html"),
            controller: ShowCustomerInvoiceInfoController,
            controllerAs: "ctrl",
            size: 'lg',
            backdrop: 'static',
            resolve: {
                customerInvoiceId: () => { return this.invoice.invoiceId },
                projectId: () => { return this.invoice.projectId ? this.invoice.projectId : 0 },
                title: () => { return this.terms["billing.order.ordersummary"] + " " + this.invoice.invoiceNr }
            }
        });
    }

    public unlockOffer() {
        this.progress.startSaveProgress((completion) => {
            this.orderService.unlockOrder(this.invoice.invoiceId).then((result: IActionResult) => {
                if (result.success) {
                    completion.completed(this.getSaveEvent(), true);
                    this.load();

                }
                else {
                    completion.failed(result.errorMessage);
                }
            });
        }, this.guid);
    }

    public closeOffer() {
        const modal = this.notificationService.showDialog(this.terms["core.warning"], this.translationService.translateInstant("common.customer.invoices.closeofferwarning"), SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
        modal.result.then(val => {
            this.progress.startSaveProgress((completion) => {
                this.orderService.closeOffer(this.invoice.invoiceId).then((result: IActionResult) => {
                    if (result.success) {
                        completion.completed(this.getSaveEvent(), true);
                        this.load();
                    }
                    else {
                        completion.failed(result.errorMessage);
                    }
                });
            }, this.guid);
        });
    }

    public unlockOrder() {
        this.progress.startSaveProgress((completion) => {
            this.orderService.unlockOrder(this.invoice.invoiceId).then(result => {

                if (result.success) {
                    completion.completed(this.getSaveEvent(), true);
                    this.load();

                }
                else {
                    const message = result.errorNumber === ActionResultSave.InvalidStateTransition ? this.terms["common.customer.invoices.orderunlockstatusfailed"] : this.terms["common.customer.invoices.orderunlockfailed"];
                    completion.failed(message);
                }
            });
        }, this.guid);
    }

    public closeOrder() {

        this.progress.startSaveProgress((completion) => {
            this.orderService.closeOrder(this.invoice.invoiceId).then(result => {
                if (result.success) {
                    completion.completed(this.getSaveEvent(), true);
                    this.load();
                }
                else {
                    completion.failed(this.terms["common.customer.invoices.orderclosefailed"]);
                }
            });
        }, this.guid);
    }

    protected delete() {
        if (this.invoice.projectId) {
            this.translationService.translate("billing.order.removeprojectquestion").then((text) => {
                const modal = this.notificationService.showDialogEx(this.terms["core.warning"], text, SOEMessageBoxImage.Question, SOEMessageBoxButtons.YesNo);
                modal.result.then(val => {
                    this.performDelete(false, val)
                });
            });
        }
        else {
            this.performDelete(false, false);
        }
    }

    protected performDelete(copy: boolean, deleteProject: boolean, message: string = null) {
        let deleteMessage = this.terms["billing.order.delete"];
        if (this.isOffer)
            deleteMessage = this.terms["billing.offer.delete"];
        else if (this.isContract)
            deleteMessage = this.terms["billing.contract.delete"];

        this.progress.startDeleteProgress((completion) => {
            this.orderService.deleteOrder(this.invoice.invoiceId, deleteProject).then((result: IActionResult) => {
                if (result.success) {
                    completion.completed(this.invoice, false, message);

                    if (copy)
                        this.copy();
                    else
                        this.new();

                    this.setOrderExpanderLabel();
                    this.updateTabCaption();
                }
                else {
                    if (result.errorMessage) {
                        completion.failed(result.errorMessage);
                    }
                    else {
                        this.translationService.translate("billing.order.delete.notsuccess").then((term) => {
                            completion.failed(term);
                        })
                    }
                }
            }, error => {
                completion.failed(error.message);
            });
        }, null, deleteMessage);
    }

    protected transferToFinished() {
        this.translationService.translate("common.customer.contract.asktransfertofinished").then((text) => {
            const modal = this.notificationService.showDialogEx(this.terms["core.warning"], text, SOEMessageBoxImage.Question, SOEMessageBoxButtons.YesNo);
            modal.result.then(val => {
                if (val) {
                    this.invoice.originStatus = SoeOriginStatus.ContractClosed;
                    this.save(false, false, false, undefined, true);
                }
            });
        });
    }

    private updateTabCaption() {
        if (this.isOrder) {
            const keys: string[] = [
                "common.customer.invoices.neworder",
                "common.order"
            ];

            this.translationService.translateMany(keys).then((terms) => {
                this.messagingService.publish(Constants.EVENT_SET_TAB_LABEL, {
                    guid: this.guid,
                    label: this.invoice && this.invoice.isTemplate ? this.invoice.originDescription : (this.isNew ? terms["common.customer.invoices.neworder"] : terms["common.order"] + " " + this.invoice.seqNr),
                    id: this.invoiceId,
                });
            });
        }
        else if (this.isOffer) {
            var keys: string[] = [
                "common.customer.invoices.newoffers",
                "common.offer"
            ];

            this.translationService.translateMany(keys).then((terms) => {
                this.messagingService.publish(Constants.EVENT_SET_TAB_LABEL, {
                    guid: this.guid,
                    label: this.invoice && this.invoice.isTemplate ? this.invoice.originDescription : (this.isNew ? terms["common.customer.invoices.newoffers"] : terms["common.offer"] + " " + this.invoice.seqNr),
                    id: this.invoiceId,
                });
            });
        }
        else if (this.isContract) {
            var keys: string[] = [
                "common.customer.invoices.newcontract",
                "common.contract"
            ];

            this.translationService.translateMany(keys).then((terms) => {
                this.messagingService.publish(Constants.EVENT_SET_TAB_LABEL, {
                    guid: this.guid,
                    label: this.invoice && this.invoice.isTemplate ? this.invoice.originDescription : (this.isNew ? terms["common.customer.invoices.newcontract"] : terms["common.contract"] + " " + this.invoice.seqNr),
                    id: this.invoiceId,
                });
            });
        }
    }

    protected copy(removeProject: boolean = true, ignoreEnable: boolean = false) {
        if (this.createOrderTemplateFromAgreement) {
            if (!this.documentExpanderIsOpen) {
                this.$scope.$watch(() => this.invoiceFilesHelper.filesLoaded, (newValue) => {
                    if (newValue) {
                        _.forEach(this.invoiceFilesHelper.files, (f) => {
                            f.isModified = true;
                        });
                    }
                });
                this.invoiceFilesHelper.loadFiles(false);
            }
            else {
                _.forEach(this.invoiceFilesHelper.files, (f) => {
                    f.isModified = true;
                });
            }

            this.performCopy(removeProject, ignoreEnable);
            this.invoice.originDescription = this.templateText;
            this.invoice.orderType = TermGroup_OrderType.Service;
            this.createOrderTemplateFromAgreement = false;
            this.$scope.$broadcast('recalculateTotals', { guid: this.guid });
            this.updateTabCaption();
            this.setAsDirty(true);
        }
        else if (this.invoiceEditHandler && this.invoiceEditHandler.containsAttachments(this.invoice.statusIcon)) {
            var dialog = this.notificationService.showDialog(this.terms["common.customer.invoices.copyattachmentsheader"], this.terms["common.customer.invoices.copyattachmentstext"], SOEMessageBoxImage.Question, SOEMessageBoxButtons.YesNo);
            dialog.result.then(val => {
                if (!this.documentExpanderIsOpen) {
                    var filesWatch = this.$scope.$watch(() => this.invoiceFilesHelper.filesLoaded, (newValue) => {
                        if (newValue) {
                            if (!val)
                                this.invoiceFilesHelper.files = [];
                            this.performCopy(removeProject, ignoreEnable, true);
                            filesWatch();
                        }
                    });
                    this.documentExpanderIsOpen = true;
                }
                else {
                    if (!val)
                        this.invoiceFilesHelper.files = [];
                    this.performCopy(removeProject, ignoreEnable, true);
                }
            });
        }
        else {
            this.performCopy(removeProject, ignoreEnable, true);
        }
    }

    protected performCopy(removeProject: boolean = true, ignoreEnable: boolean = false, recalculate: boolean = false) {
        this.isNew = true;
        this.contractHeadIsLocked = false;
        this.ignoreReloadInvoiceFee = true;
        this.ignoreReloadFreightAmount = true;
        this.checkRecalculatePrices = recalculate && (this.isOffer || this.isOrder);

        this.invoiceId = 0;
        this.invoice.invoiceId = 0;
        this.invoice.invoiceNr = undefined;
        this.invoice.seqNr = undefined;
        this.invoice.shiftTypeId = undefined;
        this.invoice.invoiceDate = CalendarUtility.getDateToday();
        this.invoice.modified = undefined;
        this.invoice.modifiedBy = '';
        if (this.isContract) {
            this.invoice.originStatus = SoeOriginStatus.Origin;
            // Get origin status text for new
            this.translationService.translate("core.new").then(term => {
                this.invoice.originStatusName = term;
            });
        }
        else {
            this.invoice.originStatus = SoeOriginStatus.None;
        }

        if (this.createOrderTemplateFromAgreement) {
            this.invoice.customerInvoiceRows = [];
            this.accountRows = [];

            //Checklists
            _.forEach(this.checklistHeads, (h) => {
                h.checklistHeadRecordId = undefined;
                _.forEach(h.checklistRowRecords, (r) => {
                    r.headRecordId = undefined;
                    r.rowRecordId = undefined;
                    r.boolData = undefined;
                    r.comment = "";
                    r.created = undefined
                    r.createdBy = undefined;
                    r.date = undefined;
                    r.dateString = undefined;
                    r.dateData = undefined;
                    r.decimalData = undefined;
                    r.description = undefined;
                    r.intData = undefined;
                    r.modified = undefined;
                    r.modifiedBy = undefined;
                    r.strData = undefined;
                    r.value = undefined;
                });
            });

            this.invoice.isTemplate = true;
        }
        else if (!this.productRowsRendered) {
            this.copyProductRows = true;
            this.openProductRowExpander();


            //Accounting rows
            this.accountRows = [];

            //Checklists
            _.forEach(this.checklistHeads, (h) => {
                h.checklistHeadRecordId = undefined;
                _.forEach(h.checklistRowRecords, (r) => {
                    r.headRecordId = undefined;
                    r.rowRecordId = undefined;
                    r.boolData = undefined;
                    r.comment = "";
                    r.created = undefined
                    r.createdBy = undefined;
                    r.date = undefined;
                    r.dateString = undefined;
                    r.dateData = undefined;
                    r.decimalData = undefined;
                    r.description = undefined;
                    r.intData = undefined;
                    r.modified = undefined;
                    r.modifiedBy = undefined;
                    r.strData = undefined;
                    r.value = undefined;
                });
            });
        }
        else {
            //Product rows
            this.$scope.$broadcast('copyRows', { guid: this.guid, checkRecalculate: this.checkRecalculatePrices });

            //Accounting rows
            this.accountRows = [];

            //Checklists
            _.forEach(this.checklistHeads, (h) => {
                h.checklistHeadRecordId = undefined;
                _.forEach(h.checklistRowRecords, (r) => {
                    r.headRecordId = undefined;
                    r.rowRecordId = undefined;
                    r.boolData = undefined;
                    r.comment = "";
                    r.created = undefined
                    r.createdBy = undefined;
                    r.date = undefined;
                    r.dateString = undefined;
                    r.dateData = undefined;
                    r.decimalData = undefined;
                    r.description = undefined;
                    r.intData = undefined;
                    r.modified = undefined;
                    r.modifiedBy = undefined;
                    r.strData = undefined;
                    r.value = undefined;
                });
            });

            _.forEach(this.invoiceFilesHelper.files, (f) => {
                f.isModified = true;
            });

            this.setLocked();

            this.setAsDirty(true);
            this.messagingService.publish(Constants.EVENT_EDIT_NEW, { guid: this.guid });
        }

        if (this.isOrder && this.invoice.projectId && this.invoice.projectId > 0 && this.invoice.projectIsActive) {
            var keys: string[] = [
                "billing.order.keepproject",
                "billing.order.keepprojecttext"
            ];

            this.translationService.translateMany(keys).then(terms => {
                var modal = this.notificationService.showDialog(terms["billing.order.keepproject"], terms["billing.order.keepprojecttext"].format(this.invoice.projectNr), SOEMessageBoxImage.Question, SOEMessageBoxButtons.YesNo);
                modal.result.then(val => {
                    if (!val) {
                        this.invoice.projectId = undefined;
                        this.invoice.projectNr = undefined;
                        this.setOrderExpanderLabel();
                    }
                });
            });
        }
        else {
            this.invoice.projectId = undefined;
            this.invoice.projectNr = undefined
        }

        this.setOrderExpanderLabel();
    }

    public reloadCustomerInvoice(invoiceId: any) {
        this.invoiceId = invoiceId;
        this.onDoLookups();
    }

    // HELP-METHODS

    private hasModifiedRows(): boolean {
        return _.filter(this.invoice.customerInvoiceRows, r => r.isModified).length > 0;
    }

    private hasFixedPriceRows(): boolean {
        return this.invoice.customerInvoiceRows && this.invoice.customerInvoiceRows.find(r => r.isFixedPriceProduct === true && r.state === SoeEntityState.Active) !== undefined;
    }

    private getAccountId(type: CustomerAccountType, dimNr: number): number {
        // First try to get account from customer
        var accountId = this.getCustomerAccountId(type, dimNr);
        if (accountId === 0 && dimNr === 1) {
            // No account found on customer, use base account
            switch (type) {
                case CustomerAccountType.Credit:
                    if (this.invoice.vatType === TermGroup_InvoiceVatType.Contractor)
                        accountId = this.reverseVatSalesId;
                    else
                        accountId = this.defaultCreditAccountId;
                    break;
                case CustomerAccountType.Debit:
                    accountId = this.defaultDebitAccountId;
                    break;
                case CustomerAccountType.VAT:
                    accountId = this.defaultVatAccountId;
                    break;
            }
        }

        return accountId;
    }

    private getCustomerAccountId(type: CustomerAccountType, dimNr: number): number {
        var accountId = 0;

        if (type === CustomerAccountType.VAT && dimNr === 1 && this.customerVatAccountId !== 0)
            return this.customerVatAccountId;

        if (this.customer && this.customer.accountingSettings) {
            var setting = _.find(this.customer.accountingSettings, { type: type });
            if (setting) {
                switch (dimNr) {
                    case 1:
                        accountId = setting.account1Id ? setting.account1Id : 0;
                        break;
                    case 2:
                        accountId = setting.account2Id ? setting.account2Id : 0;
                        break;
                    case 3:
                        accountId = setting.account3Id ? setting.account3Id : 0;
                        break;
                    case 4:
                        accountId = setting.account4Id ? setting.account4Id : 0;
                        break;
                    case 5:
                        accountId = setting.account5Id ? setting.account5Id : 0;
                        break;
                    case 6:
                        accountId = setting.account6Id ? setting.account6Id : 0;
                        break;
                }
            }
        }

        return accountId;
    }


    private setAsDirty(dirty: boolean = true) {
        this.dirtyHandler.isDirty = dirty;
    }

    private setOurReference() {
        this.invoice.referenceOur = null;

        // User setting
        if (this.defaultOurReferenceUserId !== 0) {
            var ref = _.find(this.ourReferences, { id: this.defaultOurReferenceUserId });
            if (ref)
                this.invoice.referenceOur = ref.name;

        }

        // Company setting
        if (!this.invoice.referenceOur && this.defaultOurReference) {
            var ref = _.find(this.ourReferences, { name: this.defaultOurReference });
            if (!ref) {
                ref = {
                    id: -1,
                    name: this.defaultOurReference
                };
                this.ourReferences.push(ref);
            }
            this.invoice.referenceOur = ref.name;
        }

        // Current user
        if (!this.invoice.referenceOur) {
            var ref = _.find(this.ourReferences, { id: CoreUtility.userId });
            if (ref)
                this.invoice.referenceOur = ref.name;
        }
    }

    private setPriceListType(typeId: number) {
        if (this.selectedPriceListType && this.selectedPriceListType.priceListTypeId === typeId)
            return;

        if (typeId === 0)
            typeId = this.defaultPriceListTypeId;

        var priceListType: IPriceListTypeDTO = _.find(this.priceListTypes, { priceListTypeId: typeId });

        if (priceListType) {
            this._selectedPriceListType = priceListType;
            if (this.invoice)
                this.invoice.priceListTypeId = priceListType.priceListTypeId;
            this.getFreightAmount();
            this.getInvoiceFee();
            this.currencyHelper.priceListTypeInclusiveVatChanged(priceListType.inclusiveVat);
        } else if (this.defaultPriceListTypeId !== 0) {
            this.setPriceListType(this.defaultPriceListTypeId);
        }
    }

    private setPaymentCondition(paymentConditionId: number) {
        if (paymentConditionId === 0)
            paymentConditionId = this.invoiceEditHandler.defaultPaymentConditionId;

        // Get condition
        var condition = _.find(this.invoiceEditHandler.paymentConditions, { paymentConditionId: paymentConditionId });

        this.paymentConditionDays = condition ? condition.days : this.invoiceEditHandler.defaultPaymentConditionDays;
        this.discountDays = condition ? condition.discountDays : 0;
        this.discountPercent = condition && condition.discountPercent ? condition.discountPercent : 0;
        if (this.invoice) {
            this.invoice.paymentConditionId = paymentConditionId;
        }
    }

    private setDueDate() {
        if (this.isOffer && this.offerValidNoOfDays > 0 && !this.loadingInvoice)
            this.invoice.dueDate = this.invoice.invoiceDate ? this.invoice.invoiceDate.addDays(this.offerValidNoOfDays) : null;
        else if (this.invoice && !this.loadingInvoice && !this.isContract)
            this.invoice.dueDate = this.invoice.invoiceDate ? this.invoice.invoiceDate.addDays(this.paymentConditionDays) : null;
    }

    //Quarantine
    private generateAccountingRows(calculateVat: boolean) {
        this.hasModifiedProductRows = true;
        this.accountRows = [];

        // Clear rows
        //this.invoice.accountingRows = [];

        // Debit row
        /*this.createAccountingRow(CustomerAccountType.Debit, 0, this.invoice.totalAmountCurrency, true, false, false);

        // VAT row
        if (calculateVat)
            this.calculateVatAmount();

        // Remember VAT amount (to be used on contractor VAT rows)
        var vatAmount = this.invoice.vatAmountCurrency;

        switch (this.invoice.vatType) {
            case (TermGroup_InvoiceVatType.Contractor):
            case (TermGroup_InvoiceVatType.NoVat):
                // 'Contractor' invoices does not have any regular VAT
                // 'No VAT' invoices does not have any VAT at all
                this.invoice.vatAmountCurrency = 0;
                break;
            default:
                if (this.vatRate > 0) {
                    this.createAccountingRow(CustomerAccountType.VAT, 0, this.invoice.vatAmountCurrency, false, true, false);
                    break;
                }
        }

        // Credit row
        this.createAccountingRow(CustomerAccountType.Credit, 0, this.invoice.totalAmountCurrency - this.invoice.vatAmountCurrency, false, false, false);

        // Contractor VAT rows
        if (this.invoice.vatType === TermGroup_InvoiceVatType.Contractor) {
            this.createAccountingRow(CustomerAccountType.Unknown, this.contractorVatAccountCreditId, vatAmount, false, false, true);
            this.createAccountingRow(CustomerAccountType.Unknown, this.contractorVatAccountDebitId, vatAmount, true, false, true);
        }

        this.$timeout(() => {
            this.$scope.$broadcast('setRowItemAccountsOnAllRows');
        });*/
    }

    private createAccountingRow(type: CustomerAccountType, accountId: number, amount: number, isDebitRow: boolean, isVatRow: boolean, isContractorVatRow: boolean): AccountingRowDTO {
        // Credit invoice, negate isDebitRow
        if (this.isCredit)
            isDebitRow = !isDebitRow;

        amount = Math.abs(amount);

        var row = new AccountingRowDTO();
        row.type = AccountingRowType.AccountingRow;
        row.invoiceAccountRowId = 0;
        row.tempRowId = 0;
        //row.rowNr = AccountingRowDTO.getNextRowNr(this.invoice.accountingRows);
        row.debitAmountCurrency = isDebitRow ? amount : 0;
        row.creditAmountCurrency = isDebitRow ? 0 : amount;
        row.quantity = null;
        row.date = new Date().date();
        row.isCreditRow = !isDebitRow;
        row.isDebitRow = isDebitRow;
        row.isVatRow = isVatRow;
        row.isContractorVatRow = isContractorVatRow;
        row.isInterimRow = false;
        row.state = SoeEntityState.Active;
        row.invoiceId = this.invoice.invoiceId;
        row.isModified = false;

        // Set accounts
        if (type !== CustomerAccountType.Unknown) {
            row.dim1Id = this.getAccountId(type, 1);
            row.dim2Id = this.getAccountId(type, 2);
            row.dim3Id = this.getAccountId(type, 3);
            row.dim4Id = this.getAccountId(type, 4);
            row.dim5Id = this.getAccountId(type, 5);
            row.dim6Id = this.getAccountId(type, 6);
        }

        if (accountId !== 0)
            row.dim1Id = accountId;

        //this.invoice.accountingRows.push(row);

        return row;
    }

    private calculateVatAmount(forceContractor: boolean = false) {
        // Calculate VAT amount based on vat percent
        var vatAmount: number = 0;
        var vatRateValue: number = this.vatRate / 100;

        if (this.invoice.vatType === TermGroup_InvoiceVatType.Contractor || forceContractor)
            vatAmount = this.invoice.totalAmountCurrency * vatRateValue;
        else
            vatAmount = this.invoice.totalAmountCurrency * (1 - (1 / (vatRateValue + 1)));

        this.invoice.vatAmountCurrency = vatAmount.round(2);
    }

    private shouldCopyCustomerDataOnChange() {
        return this.invoice.originStatus !== SoeOriginStatus.OrderFullyInvoice && this.invoice.originStatus !== SoeOriginStatus.OrderClosed;
    }

    public openCustomer(openDeliveryCustomer: boolean) {
        if (this.customer && this.customer.isOneTimeCustomer) {
            let tmpInvoiceHeadText = this.invoice.invoiceHeadText;

            if (this.invoice.deliveryAddressId && this.invoice.deliveryAddressId !== 0) {
                tmpInvoiceHeadText = this.invoiceEditHandler.formatDeliveryAddress(_.filter(this.invoiceEditHandler.deliveryAddresses, i => i.contactAddressId === this.invoice.deliveryAddressId)[0].contactAddressRows, this.customer.isFinvoiceCustomer);
            }

            const modal = this.modalInstance.open({
                templateUrl: this.urlHelperService.getGlobalUrl("Shared/Billing/Dialogs/OneTimeCustomer/OneTimeCustomer.html"),
                controller: OneTimeCustomerController,
                controllerAs: 'ctrl',
                backdrop: 'static',
                size: 'sm',
                resolve: {
                    translationService: () => { return this.translationService },
                    coreService: () => { return this.coreService },
                    name: () => { return this.invoice.customerName },
                    deliveryAddress: () => { return tmpInvoiceHeadText ? tmpInvoiceHeadText : "" },
                    phone: () => { return this.invoice.customerPhoneNr },
                    email: () => { return this.invoice.customerEmail },
                    isFinvoiceCustomer: () => { return this.customer.isFinvoiceCustomer },
                    isLocked: () => { return this.isLocked },
                    isEmailMode: () => { return false }
                }
            });

            modal.result.then((result: any) => {
                if (!this.shouldCopyCustomerDataOnChange()) {
                    return;
                }
                if (result) {
                    this.invoice.customerName = result.name;
                    this.invoice.customerPhoneNr = result.phone;

                    if (!StringUtility.isEmpty(result.email)) {
                        this.invoice.customerEmail = result.email;
                        this.customerEmails[0].name = result.email;
                        this.invoice.contactEComId = 0;
                    }

                    if (result.address !== tmpInvoiceHeadText) {
                        this.invoice.invoiceHeadText = result.address;
                        this.invoiceEditHandler.deliveryAddresses[0].address = result.address;
                        this.invoice.deliveryAddressId = 0;
                    }

                    // Change customer name
                    if (this.invoice.actorId && this.invoice.actorId > 0) {
                        const customer = _.find(this.customers, c => c.id === this.invoice.actorId);
                        if (customer) {
                            this.doNotLoadCustomer = true;
                            this.selectedCustomer = undefined;
                            if (this.showPayingCustomer)
                                this.selectedDeliveryCustomer = undefined;
                            this.$timeout(() => {
                                customer.name = customer.number + " " + this.invoice.customerName;
                                this.selectedCustomer = customer;

                                if (this.showPayingCustomer)
                                    this.selectedDeliveryCustomer = customer;

                                this.doNotLoadCustomer = false;
                            });
                        }
                    }

                    this.setAsDirty();
                }
            });
        }
        else {
            const modal = this.modalInstance.open({
                templateUrl: this.urlHelperService.getGlobalUrl("Common/Customer/Customers/Views/edit.html"),
                controller: CustomerEditController,
                controllerAs: 'ctrl',
                bindToController: true,
                backdrop: 'static',
                size: 'xl',
                windowClass: 'fullsize-modal',
                scope: this.$scope

            });
            modal.rendered.then(() => {
                this.$scope.$broadcast(Constants.EVENT_ON_INIT_MODAL, {
                    modal: modal, sourceGuid: this.guid, id: openDeliveryCustomer ? (this.deliveryCustomer ? this.deliveryCustomer.actorCustomerId : 0) : (this.customer ? this.customer.actorCustomerId : 0)
                });
            });


            modal.result.then(result => {
                if (!this.shouldCopyCustomerDataOnChange()) {
                    return;
                }
                if (openDeliveryCustomer) {
                    const customer = this.selectedDeliveryCustomer = _.find(this.customers, c => c.id === result.customerId);
                    if (!customer) {
                        const x = { id: result.customerId, name: result.customerName };
                        this.customers.push(x);
                        this.selectedDeliveryCustomer = x;
                    }

                    if (result.saved)
                        this.loadDeliveryCustomer();
                }
                else {
                    const newCustomer = _.find(this.customers, c => c.id === result.customerId);
                    if (!this.selectedCustomer || newCustomer.id !== this.selectedCustomer.id) {
                        this.selectedCustomer = newCustomer;
                        const customer = this.selectedCustomer;
                        if (!customer) {
                            const x = { id: result.customerId, name: result.customerName };
                            this.customers.push(x);
                            this.selectedCustomer = x;
                        }
                    }

                    if (result.saved)
                        this.loadCustomer(result.customerId, true);
                }
            });
        }
    }

    public searchCustomer(isDeliveryCustomer: boolean) {
        var modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getCommonViewUrl("Dialogs/SelectCustomer", "selectcustomer.html"),
            controller: SelectCustomerController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'lg',
            resolve: {
                translationService: () => { return this.translationService },
                coreService: () => { return this.coreService },
                commonCustomerService: () => { return this.commonCustomerService }
            }
        });

        modal.result.then(item => {
            if (item) {
                if (isDeliveryCustomer) {
                    this.selectedDeliveryCustomer = _.find(this.customers, c => c.id === item.actorCustomerId);
                }
                else {
                    this.selectedCustomer = _.find(this.customers, c => c.id === item.actorCustomerId);
                }
            }
        }, function () {
        });

        return modal;
    }


    private hasFixedPriceRowsDialog() {
        var keys: string[] = [
            "core.error",
            "billing.order.lockedtofixedprice"
        ];
        this.translationService.translateMany(keys).then((terms) => {
            var modal = this.notificationService.showDialog(terms["core.error"], terms["billing.order.lockedtofixedprice"], SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK);
            modal.result.then(val => {
            }, (reason) => {
            });
        });
    }

    private showMissingDefaultPriceListTypeWarning() {
        var keys: string[] = [
            "billing.order.missingdefaultpricelisttype.title",
            "billing.order.missingdefaultpricelisttype.message"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.notificationService.showDialog(terms["billing.order.missingdefaultpricelisttype.title"], terms["billing.order.missingdefaultpricelisttype.message"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
        });
    }

    // VALIDATION
    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            var errors = this['edit'].$error;

            // Mandatory fields
            if (this.invoice && !this.invoice.vatType) {
                mandatoryFieldKeys.push("common.customer.invoices.vattype");
            }

            var mandatoryDims = _.filter(Object.keys(errors), (e) => e.startsWith("DIM_"));
            if (mandatoryDims.length > 0) {
                _.forEach(mandatoryDims, (dim) => {
                    var strings = dim.split('_');
                    validationErrorStrings.push(this.terms["core.missingmandatoryfield"] + " " + strings[1]);
                });
            }

            if (errors['yearOpen'])
                validationErrorKeys.push("economy.accounting.voucher.accountyearclosed");

            if (errors['locked'])
                validationErrorKeys.push("common.customer.invoice.orderlocked");

            if (errors['templateDescription'])
                validationErrorKeys.push("common.customer.invoices.validationdesc");

            if (errors['customer'])
                validationErrorKeys.push("common.customer.invoices.validationcustomer");

            if (errors['priceList'])
                validationErrorKeys.push("common.customer.invoices.validationpricelist");

            if (errors['standardVoucherSeries'])
                validationErrorKeys.push("common.customer.invoices.validationstandvoucherseries");

            if (errors['defaultVoucherSeries'])
                validationErrorKeys.push("common.customer.invoices.validationvoucherserie");

            if (errors['customerBlocked'])
                validationErrorKeys.push("common.customer.invoices.validationcustomerblocked");

            if (errors['freightBase'])
                validationErrorKeys.push("common.customer.invoices.validationfreightsetting");

            if (errors['invoiceFeeBase'])
                validationErrorKeys.push("common.customer.invoices.validationfeesetting");

            if (errors['centRoundingBase'])
                validationErrorKeys.push("common.customer.invoices.validationcentsetting");

            if (errors['missingProduct']) {
                _.forEach(_.filter(this.invoice.customerInvoiceRows, (r) => (!r.productId || r.productId === 0) && (r.sumAmountCurrency && r.sumAmountCurrency != 0)), (r) => {
                    validationErrorKeys.push("common.customer.invoices.validationprodmissing");
                });
            }

            if (errors['credit'])
                validationErrorKeys.push("common.customer.invoices.validationcredit");

            if (errors['nonCredit'])
                validationErrorKeys.push("common.customer.invoices.validationdebetnew");

            if (errors['contractGroup'])
                validationErrorKeys.push("billing.contract.invalidcontractgroup");

            if (errors['contractInterval'])
                validationErrorKeys.push("billing.contract.invalidinterval");
        });
    }

    public isDisabled() {
        return !this.dirtyHandler.isDirty || this.edit.$invalid || this.executing; //|| (this.saveAsTemplate && this.invoice && (!this.invoice.originDescription || this.invoice.originDescription.length < 1));
    }

    public isValidToChangeAttestState(): boolean {
        if (this.mandatoryChecklist) {
            if (this.checklistHeads.length === 0) {
                if (this.edit.$invalid) {
                    this.showValidationError();
                    return false;
                }
                else
                    return true;
            }
            else {
                if (!this.validateRowAttestStateChanged()) {
                    return false;
                }
                else {
                    if (this.edit.$invalid) {
                        this.showValidationError();
                        return false;
                    }
                    else
                        return true;
                }
            }
        }
        else {
            if (this.edit.$invalid) {
                this.showValidationError();
                return false;
            }
            else
                return true;
        }
    }

    public orderClosed(): boolean {
        if (!this.invoice) {
            return true;
        }

        return (this.invoice.originStatus == SoeOriginStatus.OrderFullyInvoice || this.invoice.originStatus == SoeOriginStatus.OrderClosed || this.invoice.originStatus == SoeOriginStatus.ContractClosed || this.invoice.originStatus == SoeOriginStatus.Cancel)
    }

    public offerClosed(): boolean {
        if (!this.invoice) {
            return true;
        }

        return (this.invoice.originStatus == SoeOriginStatus.OfferFullyInvoice || this.invoice.originStatus == SoeOriginStatus.OfferFullyOrder || this.invoice.originStatus == SoeOriginStatus.OfferClosed)
    }

    public setLocked() {
        if (!this.invoice)
            return;

        let locked: boolean = this.isOrder ? this.orderClosed() : this.offerClosed();

        if (this.customer?.blockOrder) {
            locked = true;
        }


        if (this.isOrder) {
            this.showUnlockButton = this.orderClosed() && this.modifyPermission && (this.unlockPermission || CoreUtility.isSupportAdmin);
            this.showCloseButton = (this.invoice.originStatus == SoeOriginStatus.OrderPartlyInvoice || this.invoice.originStatus == SoeOriginStatus.Origin) && this.modifyPermission && (this.closePermission || CoreUtility.isSupportAdmin);
        }
        else if (this.isOffer) {
            this.showUnlockButton = this.offerClosed() && this.modifyPermission && (this.unlockPermission || CoreUtility.isSupportAdmin);
            this.showCloseButton = (!this.offerClosed()) && this.modifyPermission && (this.closePermission || CoreUtility.isSupportAdmin);
        }

        if (this.invoice.isTemplate) {
            this.showUnlockButton = this.showCloseButton = false;
        }

        this.isLocked = locked;
    }

    private startAutoSaveTimer() {
        if (this.autoSaveActive)
            this.stopAutoSaveTimer();

        this.autoSaveActive = true;
        var setMin = 0;
        this.timerToken = setInterval(() => {
            this.tickCounter++;
            var currentMin = (Math.floor((this.autoSaveInterval - this.tickCounter) / 60) + 1);
            if (setMin !== currentMin) {
                setMin = currentMin;
                this.$timeout(() => {
                    this.autoSaveText = this.terms["billing.order.autosavemessageminutes"].format(setMin);
                });
            }

            if (this.tickCounter === this.autoSaveInterval) {
                if (this.isDisabled()) {
                    this.startAutoSaveTimer();
                }
                else {
                    this.stopAutoSaveTimer();
                    this.save(false, true, false, undefined, false, true);
                }
            }
        }, 1000);
    }

    private pauseAutoSaveTimer() {
        if (this.timerPaused) {
            if (!this.autoSaveInterval || this.autoSaveInterval === 0)
                return;

            this.timerPaused = false;
            this.startAutoSaveTimer();
        }
        else {
            this.timerPaused = true;
            clearInterval(this.timerToken);
        }
    }

    private stopAutoSaveTimer() {
        clearInterval(this.timerToken);
        this.timerToken = undefined;
        this.autoSaveActive = false;
        this.tickCounter = 0;
    }

    private checklistRowHasAnswer(row: ChecklistExtendedRowDTO) {
        let hasAnswer: boolean = false;

        switch (row.type) {
            case TermGroup_ChecklistRowType.String:
                var comment = row.comment ? row.comment.trim() : "";
                hasAnswer = !StringUtility.isEmpty(comment);
                break;
            case TermGroup_ChecklistRowType.YesNo:
                hasAnswer = row.boolData !== null && row.boolData !== undefined;
                break;
            case TermGroup_ChecklistRowType.Checkbox:
                hasAnswer = row.boolData && row.boolData === true; //Must be checked
                break;
            case TermGroup_ChecklistRowType.MultipleChoice:
                hasAnswer = row.intData && row.intData > 0;
                break;
            case TermGroup_ChecklistRowType.Image:
                hasAnswer = row.boolData && row.boolData === true;
                break;
        }

        return hasAnswer;
    }

    private updateAccordionSettings() {

        const keys: string[] = [
            "common.offer",
            "billing.order.productrows",
            "core.document",
            "common.checklists",
            "common.customer.invoices.accountingrows",
            "billing.order.planning",
            "common.tracing",
            "billing.order.order",
            "billing.order.conditions",
            "billing.order.orderdetail",
            "common.contract",
            "common.customer.invoices.offerdetails",
            "common.customer.invoices.contractdetails",
            "billing.order.times",
            "billing.order.expenses",
            "billing.project.central.supplierinvoices",
            "common.note",
        ];
        let accordionList: any[] = [];

        this.translationService.translateMany(keys).then((terms) => {
            let settingType: any = null;
            if (this.isOffer) {
                settingType = UserSettingType.BillingOfferDefaultExpanders;
                let offerText = terms["common.offer"];
                accordionList.push({ name: "OfferExpander", description: offerText });
                accordionList.push({ name: "OfferOfferExpander", description: offerText + " >> " + terms["common.customer.invoices.offerdetails"] });
                accordionList.push({ name: "OfferConditionExpander", description: offerText + " >> " + terms["billing.order.conditions"] });
            }
            else if (this.isOrder) {
                settingType = UserSettingType.BillingOrderDefaultExpanders;
                let orderText = terms["billing.order.order"];
                accordionList.push({ name: "OrderExpander", description: orderText });
                accordionList.push({ name: "OrderOrderExpander", description: orderText + " >> " + terms["billing.order.orderdetail"] });
                accordionList.push({ name: "OrderConditionExpander", description: orderText + " >> " + terms["billing.order.conditions"] });
            }
            else if (this.isContract) {
                settingType = UserSettingType.BillingContractDefaultExpanders;
                let contractText = terms["common.contract"];
                accordionList.push({ name: "ContractExpander", description: contractText });
                accordionList.push({ name: "ContractContractExpander", description: contractText + " >> " + terms["common.customer.invoices.contractdetails"] });
                accordionList.push({ name: "ContractConditionExpander", description: contractText + " >> " + terms["billing.order.conditions"] });
            }

            if (this.productRowsPermission)
                accordionList.push({ name: "ProductRowsExpander", description: terms["billing.order.productrows"] });
            if (this.isContract)
                accordionList.push({ name: "NoteExpander", description: terms["common.note"]});
            if (this.timeProjectPermission)
                accordionList.push({ name: "TimeRowExpander", description: terms["billing.order.times"] });
            if (this.expensesPermission)
                accordionList.push({ name: "ExpensesExpander", description: terms["billing.order.expenses"] });
            if (this.filesPermission)
                accordionList.push({ name: "DocumentExpander", description: terms["core.document"] });
            if (this.checklistsPermission)
                accordionList.push({ name: "ChecklistExpander", description: terms["common.checklists"] });
            if (this.accountingRowsPermission)
                accordionList.push({ name: "AccountingRowExpander", description: terms["common.customer.invoices.accountingrows"] });
            if (this.orderPlanningPermission)
                accordionList.push({ name: "PlanningExpander", description: terms["billing.order.planning"] });
            if (this.orderSupplierInvoicesPermission)
                accordionList.push({ name: "SupplierInvoicesExpander", description: terms["billing.project.central.supplierinvoices"] });
            if (this.tracingPermission)
                accordionList.push({ name: "TracingExpander", description: terms["common.tracing"] });

            const modal = this.modalInstance.open({
                templateUrl: this.urlHelperService.getGlobalUrl("Common/Dialogs/AccordionSettings/Views/accordionsettings.html"),
                controller: AccordionSettingsController,
                controllerAs: 'ctrl',
                backdrop: 'static',
                size: 'md',
                resolve: {
                    coreService: () => { return this.coreService },
                    userSettingType: () => { return settingType },
                    accordionList: () => { return accordionList },
                    userSliderSettingType: () => { return null }
                }
            });

            modal.result.then(ids => {
                this.loadUserSettings();
            }, function () {
                //Cancelled
            });
        });
    }
}
