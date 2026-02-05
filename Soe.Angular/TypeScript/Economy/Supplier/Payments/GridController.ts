import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { ToolBarUtility, ToolBarButton } from "../../../Util/ToolBarUtility";
import { IconLibrary, SoeGridOptionsEvent, SOEMessageBoxImage, SOEMessageBoxButtons, SupplierPaymentGridButtonFunctions, SOEMessageBoxSize, SOEMessageBoxButton } from "../../../Util/Enumerations";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { SupplierPaymentGridDTO } from "../../../Common/Models/InvoiceDTO";
import { ISupplierService } from "../../../Shared/Economy/Supplier/SupplierService";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { TabMessage } from "../../../Core/Controllers/TabsControllerBase1";
import { FlaggedEnum } from "../../../Util/EnumerationsUtility";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { NumberUtility } from "../../../Util/NumberUtility";
import { HtmlUtility } from "../../../Util/HtmlUtility";
import { CoreUtility } from "../../../Util/CoreUtility";
import { SoeOriginStatusChange, SoeOriginStatusClassification, Feature, TermGroup_BillingType, TermGroup_SysPaymentType, SoeStatusIcon, SettingMainType, UserSettingType, CompanySettingType, TermGroup_AttestEntity, SoeModule, TermGroup, SoeOriginType, SoeOriginStatus, SoePaymentStatus, TermGroup_SysPaymentMethod, SoeReportTemplateType, ActionResultSave, TermGroup_SignatoryContractPermissionType } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { EditController as InvoiceEditController } from "../../../Shared/Economy/Supplier/Invoices/EditController";
import { EditController } from "../../../Shared/Economy/Supplier/Payments/EditController";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { ShowAttestCommentsAndAnswersDialogController } from "../../../Common/Dialogs/ShowAttestCommentsAndAnswersDialog/ShowAttestCommentsAndAnswersDialogController";
import { SignatoryContractAuthenticationController } from "../../../Common/Dialogs/SignatoryContractAuthentication/SignatoryContractAuthenticationController";
import { IRequestReportService } from "../../../Shared/Reports/RequestReportService";
import { BalanceListPrintDTO } from "../../../Common/Models/RequestReports/BalanceListPrintDTO";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    // Lookups 
    allItemsSelectionDict: any[];
    invoiceBillingTypes: any[];
    paymentMethods: any[];
    originStatus: any[];
    attestStates: any[];
    suppliers: ISmallGenericType[];
    items: SupplierPaymentGridDTO[];
    filteredItems: any[];
    actors: number[];
    actorsPaymentInformation: { [id: number]: any[] };
    actorsPaymentInformationSmall: { [id: number]: any[] };
    sysPaymentTypes: any = [];

    //Terms
    terms: { [index: string]: string };

    //Flags
    dataLoaded: boolean = false;

    //Account year
    currentAccountYearId: number;
    currentAccountYearFromDate: any;
    currentAccountYearToDate: any;

    // Variables
    splitButtonLabel: string;
    lookups: number;
    hasCurrencyPermission: boolean;
    hasSuggestionPermission: boolean;
    hasSuggestionToCancelPermission: boolean;
    hasOriginToPaymentPermission: boolean;
    hasPaidToVoucherPermission: boolean;
    hasPaidEditDateAndAmountPermission: boolean;
    hasPaidToCancelPermission: boolean;
    hasAttestPermission: boolean;
    private hasSendPaymentPermission: boolean;
    private hasSendPaymentNotificationPermission: boolean;
    protected setupComplete: boolean;
    protected hasOpenPermission: boolean;
    protected hasClosedPermission: boolean;
    selectedPayDate: Date = null;
    filteredTotal: number = 0;
    selectedTotal: number = 0;
    selectedItemsCount: number = 0;
    filteredTotalExVat: number = 0;
    selectedTotalIncVat: number = 0;
    filteredTotalIncVat: number = 0;
    selectedTotalExVat: number = 0;
    filteredPaid: number = 0;
    selectedPaid: number = 0;
    filteredToPay: number = 0;
    selectedToPay: number = 0;
    date = new Date();
    allowedPaymentMinDate!: Date;
    dateToday: Date = new Date(this.date.getFullYear(), this.date.getMonth(), this.date.getDate(), 0, 0, 0);

    //Flags
    setupDone = false;
    showVatFree: boolean = true;
    showVatFreeCheckbox: boolean = true;
    hideAutogiro: boolean = false;
    hideAutogiroVisibility: boolean = false;
    showSplitButton: boolean;
    showPaymentInformation: boolean = false;
    showSelected: boolean = false;
    showPayDate: boolean;
    showPaymentMethod: boolean;
    showPrintSuggestion: boolean = false;
    showPrintVoucher: boolean = false;
    showToPayTotals: boolean = false;
    showPaidTotals: boolean = false;
    showTotals: boolean = false;

    //Validation
    ignoreDateValidation = false;

    //Settings
    supplierPaymentTransferToVoucher: boolean;
    supplierUseTimeDiscount: boolean;
    supplierUnderPayAccount: number;
    supplierDefaultPaymentMethod: number;
    supplierSetDefaultPayDateAsDueDate: boolean;
    supplierBalanceListReportId: number;
    supplierChecklistPaymentReportId: number;
    supplierPaymentSuggestionReportId: number;
    supplierPaymentVoucherReportId: number;
    coreBaseCurrency = 0;
    supplierUsePaymentSuggestion: boolean;
    usesSignatoryContractForSendingPayments: boolean = false;

    //Compsetting
    userIdNeededWithTotalAmount = 0;
    totalAmountWhenUserReguired = 0;

    //StatusChange
    originStatusChange: SoeOriginStatusChange;

    // Properties
    private _allItemsSelection: any;
    get allItemsSelection() {
        return this._allItemsSelection;
    }
    set allItemsSelection(item: any) {
        this._allItemsSelection = item;
        if (this.setupComplete == true)
            this.updateItemsSelection();
    }

    private _selectedPaymentMethod: any;
    get selectedPaymentMethod() {
        return this._selectedPaymentMethod;
    }
    set selectedPaymentMethod(item: any) {
        this._selectedPaymentMethod = item;
        if (this._selectedPaymentMethod != null) {
            if (this.showPaymentInformation && this.actors && this.actors.length > 0) {
                this.loadPaymentInformationFromActorsForType();
            }
        }
    }

    get showSelectionFilter() {
        return this.classification !== SoeOriginStatusClassification.SupplierPaymentSuggestions && this.classification !== SoeOriginStatusClassification.SupplierPaymentSuggestionsForeign;
    }

    // Functions
    buttonFunctions: any = [];

    // Grid header and footer
    gridFooterComponentUrl: any;

    //modal
    private modalInstance: any;
    private paymentTitle: string;

    private classification: SoeOriginStatusClassification;
    private usesSuggestion: boolean;

    private isSupplierBalanceListPrinting: boolean = false;
    private isSupplierPaymentSuggestionReportPrinting: boolean = false;

    //@ngInject
    constructor(
        private $window,
        $uibModal,
        private $timeout: ng.ITimeoutService,
        private $scope: ng.IScope,
        private coreService: ICoreService,
        private supplierService: ISupplierService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private urlHelperService: IUrlHelperService,
        gridHandlerFactory: IGridHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        private $q: ng.IQService,
        private readonly requestReportService: IRequestReportService) {
        super(gridHandlerFactory, "Economy.Supplier.Invoices" + "_" + Feature.Economy_Supplier_Invoice_Status, progressHandlerFactory, messagingHandlerFactory);
        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onAllPermissionsLoaded(x))            
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory)) 
            .onLoadSettings(() => this.doLoadSettings())
            .onBeforeSetUpGrid(() => this.doLookups())
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData());

        this.modalInstance = $uibModal;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.loadGridData());
        this.toolbar.addInclude(this.urlHelperService.getViewUrl("gridHeader.html"));

        // Print
        if (this.classification != SoeOriginStatusClassification.SupplierPaymentsVoucher) {
            var group = ToolBarUtility.createGroup(new ToolBarButton("economy.supplier.invoice.printbalance", "economy.supplier.invoice.printbalance", IconLibrary.FontAwesome, "fa-print", () => {
                this.printSelectedInvoices();
            }, () => {
                return this.gridAg.options.getSelectedCount() === 0
                    || this.isSupplierBalanceListPrinting;
            }));

            this.toolbar.addButtonGroup(group);
        }

        // Print payment suggestion
        if (this.classification === SoeOriginStatusClassification.SupplierPaymentSuggestions) {
            var group = ToolBarUtility.createGroup(new ToolBarButton("economy.supplier.payment.printsuggestion", "economy.supplier.payment.printsuggestion", IconLibrary.FontAwesome, "fa-print", () => {
                this.printSuggestion();
            },
            () => {
                return this.gridAg.options.getSelectedCount() === 0
                    || this.isSupplierPaymentSuggestionReportPrinting;
            }));

            this.toolbar.addButtonGroup(group);
        }
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;
        this.guid = parameters.guid;
        this.classification = this.parameters.classification;
        this.usesSuggestion = this.parameters.usePaymentSuggestion;


        this.onTabActivetedAndModified(() => {
            if (this.setupDone)
                this.loadGridData()
        });

        this.gridFooterComponentUrl = this.urlHelperService.getViewUrl("gridFooter.html");
        this.onTabActivated(() => this.localOnTabActivated());
        this.setupComplete = false;
    }
    
    private localOnTabActivated() {
        if (!this.setupComplete) {
            this.flowHandler.start(this.getPermissions());
            this.setupComplete = true;
        }

        if (this.classification === SoeOriginStatusClassification.SupplierPaymentSuggestions) {
            if (this.setupDone)
                this.loadGridData()
        }
    }

    private doLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.setupGuiForClassification(),
            this.loadAttestStates(),
            this.loadSelectionTypes(),
            this.loadInvoiceBillingTypes(),
            this.loadSysPaymentTypes(),
            this.loadOriginStatus(),
            this.loadSuppliers(),
            this.loadPaymentMethods(),
            this.loadCurrentAccountYear()
        ]).then(() => {
            this.setupDone = true;
        });
    }

    private doLoadSettings(): ng.IPromise<any> {
        return this.$q.all([
            this.checkIfUsesSignatoryContract(),
            this.loadUserSettings(),
            this.loadCompanySettings()
        ]).then(() => {
            // Setup Send payement notification button
            if (this.hasSendPaymentNotificationPermission && ((this.classification === SoeOriginStatusClassification.SupplierPaymentsUnpayed && !this.supplierUsePaymentSuggestion) || this.classification === SoeOriginStatusClassification.SupplierPaymentSuggestions)) {
                const group = ToolBarUtility.createGroup(new ToolBarButton("economy.supplier.payment.sendpaymentnotification", "economy.supplier.payment.sendpaymentnotification.tooltip", IconLibrary.FontAwesome, "fa-envelope", () => {
                    this.showSendPaymentNotification();
                }, () => {
                    return !(this.selectedPaymentMethod && this.selectedPaymentMethod.paymentMethodId > 0);
                }));
                this.toolbar.addButtonGroup(group);
            }   
        });
    }
    public validatePaymentDate(data: any) {
        this.$timeout(() => {
            if (this.selectedPayDate < this.allowedPaymentMinDate.addDays(-1)) {
                this.selectedPayDate = null;
                this.notificationService.showDialog(this.translationService.translateInstant("core.warning"), this.translationService.translateInstant("economy.supplier.payment.invalid.paymentdate"), SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
            }
        });            
    }

    private showSendPaymentNotification() {
        const model = this.notificationService
            .showDialog(
                this.terms["economy.supplier.payment.sendpaymentnotification.popup.title"],
                this.terms["economy.supplier.payment.sendpaymentnotification.popup.text"].replace('{0}', this.selectedPaymentMethod?.name ?? ''),
                SOEMessageBoxImage.Question,
                SOEMessageBoxButtons.YesNo,
                SOEMessageBoxSize.Small);

        model.result.then((response) => {
            if (response === true) {
                this.sendPaymentNotification();
            }
        });
    }

    private sendPaymentNotification() {
        this.progress.startWorkProgress((completion) => {
            const pageUrl = this.classification === SoeOriginStatusClassification.SupplierPaymentSuggestions ? this.getPageUrl(): "";
            this.supplierService
                .sendPaymentNotification(this.selectedPaymentMethod.paymentMethodId, pageUrl, this.classification)
                .then((result) => {
                    if (result.success) {
                        completion.completed(result, false, this.terms["economy.supplier.payment.sendpaymentnotification.success"]);
                    } else {
                        if (result.errorNumner !== ActionResultSave.Unknown)
                            completion.failed(result.errorMessage);
                        else
                            completion.failed(this.terms["economy.supplier.payment.sendpaymentnotification.fail"]);
                    }
                }, error => {
                    completion.failed(error.message);
                });
        });
    }

    private getPageUrl() {
        let url = this.$window.location.origin + this.$window.location.pathname;
        if (this.$window.location.search) {
            url += this.$window.location.search + "&proposal=true";
        }
        else {
            url += "?proposal=true";
        }
        return url;
    }

    protected setupButtons() {
        // Functions
        const keys: string[] = [
            "core.transfertovoucher",
            "economy.supplier.payment.createsuggestion",
            "economy.supplier.payment.match",
            "economy.supplier.payment.createpaymentfile",
            "economy.supplier.payment.deletesuggestion",
            "economy.supplier.payment.changedatevoucher",
            "economy.supplier.payment.changepaydate",
            "economy.supplier.payment.savechanges",
            "economy.supplier.payment.deletepaymentfile",
            "economy.supplier.payment.savechangestransfervoucher",
            "economy.supplier.payment.sendpaymentfile"
        ];

        this.translationService.translateMany(keys).then((terms) => {

            switch (this.classification) {
                case SoeOriginStatusClassification.SupplierPaymentsUnpayed:
                    if (this.usesSuggestion === true) {
                        if (this.hasSuggestionPermission === true) {
                            this.splitButtonLabel = terms["economy.supplier.payment.createsuggestion"];
                            this.buttonFunctions.push({ id: SupplierPaymentGridButtonFunctions.CreateSuggestion, name: terms["economy.supplier.payment.createsuggestion"], icon: 'fal fa-receipt' });
                        }
                    }
                    else {
                        if (this.hasOriginToPaymentPermission) {
                            this.splitButtonLabel = terms["economy.supplier.payment.createpaymentfile"];
                            this.buttonFunctions.push({ id: SupplierPaymentGridButtonFunctions.CreatePaymentFile, name: terms["economy.supplier.payment.createpaymentfile"], icon: 'fal fa-download' });

                            if (this.hasSendPaymentPermission) {
                                this.buttonFunctions.push({ id: SupplierPaymentGridButtonFunctions.SendPaymentFile, name: terms["economy.supplier.payment.sendpaymentfile"], icon: 'fal fa-cloud-upload' });
                            }
                        }
                    }

                    this.buttonFunctions.push({ id: SupplierPaymentGridButtonFunctions.Match, name: terms["economy.supplier.payment.match"], icon: 'fal fa-exchange' });
                    break;
                case SoeOriginStatusClassification.SupplierPaymentSuggestions:
                    if (this.hasOriginToPaymentPermission) {
                        this.buttonFunctions.push({ id: SupplierPaymentGridButtonFunctions.CreatePaymentFile, name: terms["economy.supplier.payment.createpaymentfile"], icon: 'fal fa-download' });
                        if (this.hasSendPaymentPermission) {
                            this.buttonFunctions.push({ id: SupplierPaymentGridButtonFunctions.SendPaymentFile, name: terms["economy.supplier.payment.sendpaymentfile"], icon: 'fal fa-cloud-upload' });
                        }
                    }
                    if (this.hasSuggestionToCancelPermission)
                        this.buttonFunctions.push({ id: SupplierPaymentGridButtonFunctions.DeleteSuggestion, name: terms["economy.supplier.payment.deletesuggestion"], icon: 'fal fa-undo-alt' });
                    break;
                case SoeOriginStatusClassification.SupplierPaymentsPayed:
                    if (this.hasPaidToVoucherPermission)
                        this.buttonFunctions.push({ id: this.hasPaidEditDateAndAmountPermission ? SupplierPaymentGridButtonFunctions.SaveChangesTransferToVoucher : SupplierPaymentGridButtonFunctions.TransferToVoucher, name: this.hasPaidEditDateAndAmountPermission ? terms["economy.supplier.payment.savechangestransfervoucher"] : terms["core.transfertovoucher"] });

                    if (this.hasPaidToCancelPermission)
                        this.buttonFunctions.push({ id: SupplierPaymentGridButtonFunctions.DeletePaymentFile, name: terms["economy.supplier.payment.deletepaymentfile"] });
                    break;
            }
        });
    }

    protected setupGuiForClassification() {
        switch (this.classification) {
            case SoeOriginStatusClassification.SupplierPaymentsUnpayed:
                this.showPaymentMethod = !this.usesSuggestion;
                this.showPayDate = true;
                this.showPaymentInformation = true;
                this.hideAutogiroVisibility = true;
                this.showSplitButton = true;
                this.showTotals = true;
                this.showToPayTotals = true;
                this.allowedPaymentMinDate = new Date();
                break;
            case SoeOriginStatusClassification.SupplierPaymentSuggestions:
                this.showPaymentMethod = true;
                this.showPayDate = true;
                this.showPaymentInformation = true;
                this.hideAutogiroVisibility = false;
                this.showSplitButton = true;
                this.showPrintSuggestion = true;
                this.showTotals = true;
                this.showToPayTotals = true;
                this.allowedPaymentMinDate = new Date();
                break;
            case SoeOriginStatusClassification.SupplierPaymentsPayed:
                this.showPaymentMethod = false;
                this.showPayDate = this.hasPaidEditDateAndAmountPermission;
                this.hideAutogiroVisibility = false;
                this.showSplitButton = true;
                this.showPaidTotals = true;
                this.showVatFreeCheckbox = false;
                break;
            case SoeOriginStatusClassification.SupplierPaymentsVoucher:
                this.showPaymentMethod = false;
                this.showPayDate = false;
                this.hideAutogiroVisibility = false;
                this.showSplitButton = false;
                this.showPaidTotals = true;
                this.showVatFreeCheckbox = false;
                break;
        }
    }

    public setupGrid() {

        this.gridAg.options.setName("Economy.Supplier.Invoices" + "_" + Feature.Economy_Supplier_Invoice_Status + "_" + this.parameters.classification);
        this.setupButtons();

        // Columns
        const keys: string[] = [
            "common.type",
            "economy.supplier.payment.payment",
            "economy.supplier.invoice.seqnr",
            "economy.supplier.invoice.invoicenr",
            "economy.supplier.invoice.invoicetype",
            "economy.supplier.invoice.invoice",
            "common.tracerows.status",
            "economy.supplier.supplier.suppliernr.grid",
            "economy.supplier.supplier.suppliername.grid",
            "economy.supplier.invoice.amountexvat",
            "economy.supplier.invoice.amountincvat",
            "economy.supplier.invoice.remainingamount",
            "economy.supplier.invoice.foreignamount",
            "economy.supplier.invoice.foreignremainingamount",
            "economy.supplier.invoice.currencycode",
            "economy.supplier.invoice.invoicedate",
            "economy.supplier.invoice.duedate",
            "economy.supplier.invoice.attest",
            "economy.supplier.invoice.attestname",
            "economy.supplier.invoice.openpdf",
            "economy.supplier.payment.changepaydate",
            "core.edit",
            "core.warning",
            "core.verifyquestion",
            "core.continue",
            "economy.supplier.payment.wrongyear",
            "economy.supplier.payment.transfertovoucher",
            "economy.supplier.payment.unsecuremark",
            "economy.supplier.payment.validcreatesuggestion",
            "economy.supplier.payment.invalidcreatesuggestion",
            "economy.supplier.payment.validinvoice",
            "economy.supplier.payment.validinvoices",
            "economy.supplier.payment.validpayment",
            "economy.supplier.payment.validpayments",
            "economy.supplier.payment.validsuggestion",
            "economy.supplier.payment.validpaymentfile",
            "economy.supplier.payment.paymentdate",
            "economy.supplier.payment.validmatch",
            "economy.supplier.payment.invalidmatch",
            "economy.supplier.payment.validcreatepayment",
            "economy.supplier.payment.samepaymentdate",
            "economy.supplier.payment.duedateaspaydate",
            "economy.supplier.payment.suggestiontransferall",
            "economy.supplier.payment.hasbeenchecked",
            "economy.supplier.payment.invalidcreatepayment",
            "economy.supplier.payment.invalidcancel",
            "economy.supplier.payment.validcancel",
            "economy.supplier.payment.validvoucher",
            "economy.supplier.payment.invalidvoucher",
            "economy.supplier.payment.validsavechangesvoucher",
            "economy.supplier.payment.validchangedate",
            "economy.supplier.payment.validsavechanges",
            "economy.supplier.payment.invalidsavechanges",
            "economy.supplier.payment.supplierwrongpaymentmethod",
            "economy.supplier.payment.invoicewrongpaymentmethod",
            "economy.supplier.payment.missingattest",
            "economy.supplier.payment.defaultVoucherListMissing",
            "economy.supplier.payment.askPrintVoucher",
            "economy.supplier.payment.paymentnr",
            "economy.supplier.payment.paymentamount",
            "economy.supplier.payment.paymentamountforeign",
            "economy.supplier.payment.topaymentamount",
            "economy.supplier.payment.topaymentamountforeign",
            "economy.supplier.payment.validsavechangestransfertovoucher",
            "economy.supplier.payment.registerpayment",
            "economy.supplier.payment.showinvoice",
            "economy.supplier.payment.editpayment",
            "economy.supplier.payment.showpayment",
            "economy.supplier.payment.missingatteststatus",
            "economy.supplier.payment.underattest",
            "economy.supplier.payment.attested",
            "economy.supplier.payment.hiddenattest",
            "economy.supplier.payment.newpayment",
            "common.reportsettingmissing",
            "economy.supplier.payment.voucherscreated",
            "economy.supplier.payment.invoicetransfertosuggestionvalid",
            "economy.supplier.payment.invoicestransfertosuggestionvalid",
            "economy.supplier.payment.invoicetransfertosuggestioninvalid",
            "economy.supplier.payment.invoicestransfertosuggestioninvalid",
            "economy.supplier.payment.transferedtosuggestion",
            "economy.supplier.payment.transferedtosuggestionfailed",
            "economy.supplier.payment.invoicesmatched",
            "economy.supplier.payment.invoicesmatchedfailed",
            "economy.supplier.payment.invoicecreatepaymentinvalid",
            "economy.supplier.payment.invoicescreatepaymentinvalid",
            "economy.supplier.payment.paymentcreated",
            "economy.supplier.payment.paymentcreatedfailed",
            "economy.supplier.payment.paymentcancelvalid",
            "economy.supplier.payment.suggestioncancelvalid",
            "economy.supplier.payment.paymentcancelinvalid",
            "economy.supplier.payment.suggestioncancelinvalid",
            "economy.supplier.payment.singlecancelled",
            "economy.supplier.payment.multicancel",
            "economy.supplier.payment.cancelfailed",
            "common.paymentorigintovouchervalid",
            "common.paymentsorigintovouchervalid",
            "common.paymentorigintovoucherinvalid",
            "common.paymentsorigintovoucherinvalid",
            "common.paymenttransferedtovoucher",
            "common.paymentstransferedtovoucher",
            "common.paymenttransferedtovoucherfailed",
            "common.paymentstransferedtovoucherfailed",
            "economy.supplier.payment.paymentchangedatetovouchervalid",
            "economy.supplier.payment.paymentschangedatetovouchervalid",
            "economy.supplier.payment.paymentdatetotvoucher",
            "economy.supplier.payment.paymentsdatetovocher",
            "economy.supplier.payment.paymentdatetotvoucherfailed",
            "economy.supplier.payment.paymentsdatetotvoucher",
            "economy.supplier.payment.paymentchangedatevalid",
            "economy,supplier.payment.paymentschangedatevalid",
            "common.savedsingle",
            "common.savedmulti",
            "common.savefailed",
            "economy.supplier.payment.paymentsavevalid",
            "economy.supplier.payment.paymentssavevalid",
            "economy.supplier.payment.paymentsaveinvalid",
            "economy.supplier.payment.paymentssaveinvalid",
            "economy.supplier.payment.paymentsavetovoucher",
            "economy.supplier.payment.paymentssavetovoucher",
            "economy.supplier.payment.paymentsavedtovoucher",
            "economy.supplier.payment.paymentssavedtovoucher",
            "economy.supplier.payment.paymentsavedtovoucherfailed",
            "economy.supplier.payment.paymentssavedtovoucherfailed",
            "common.imported",
            "common.hasaattachedfiles",
            "common.hasattachedimages",
            "economy.supploer.invoice.invoiceimported",
            "economy.supplier.invoice.partlypaid",
            "economy.supplier.invoice.paidlate",
            "economy.supplier.invoice.matches.totalamount",
            "economy.supplier.payment.paymentamount",
            "economy.supplier.invoice.hastimediscount",
            "economy.supplier.invoice.amounttopay",
            "economy.supplier.invoice.timediscount",
            "economy.supplier.payment.notsameactor",
            "economy.supplier.payment.notmatchedamount",
            "economy.supplier.payment.payamountinvalidmessage",
            "economy.supplier.payment.paidamountchengenotallowed",
            "economy.supplier.payment.needsetdate",
            "economy.import.payment.invoicetotalamount",
            "economy.supplier.payment.topayamountforeign",
            "common.customer.payment.paymentseqnr",
            "economy.import.payment.invoiceseries",
            "economy.import.payment.invoicenr",
            "economy.supplier.payment.suggestionnr",
            "economy.supplier.payment.underpayaccountmissing",
            "economy.supplier.payment.blockchangeingrid",
            "economy.supplier.payment.timediscountloss",
            "economy.supplier.payment.validcreatepayments",
            "economy.supplier.payment.only2invoicesforpaymentmatch",
            "economy.supplier.payment.only1currencyforpaymentmatch",
            "economy.supplier.payment.hiddenautogiromessage",
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total",
            "core.aggrid.totals.selected",
            "economy.supplier.invoice.voucherdate",
            "core.attestflowregistered",
            "common.reason",
            "economy.supplier.payment.needsavechanges",
            "economy.supplier.invoice.description",
            "economy.supplier.invoice.credithandlemessage", 
            "economy.supplier.invoice.creditnegativeamountmessage",
            "economy.supplier.invoice.credittoomanymessage",
            "economy.supplier.invoice.creditsinglemessage",
            "economy.supplier.invoice.suppliershort",
            "economy.import.payment.invoiceseries",
            "economy.supplier.invoice.paydateshort",
            "economy.supplier.invoice.credittoomanywarning",
            "economy.supplier.invoice.timediscountdate",
            "economy.supplier.payment.sendpaymentnotification.popup.title",
            "economy.supplier.payment.sendpaymentnotification.popup.text",
            "economy.supplier.payment.sendpaymentnotification.success",
            "economy.supplier.payment.sendpaymentnotification.fail",
            "economy.supplier.invoice.ocr",
            "economy.import.payment.paymentstatus",
            "core.error"
        ];

        //this.setupTypeAhead();

        this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;

            switch (this.classification) {
                case SoeOriginStatusClassification.SupplierPaymentsUnpayed:
                    this.gridAg.addColumnIsModified();
                    this.gridAg.addColumnNumber("invoiceSeqNr", terms["economy.import.payment.invoiceseries"], null, { alignLeft: true, formatAsText: true });
                    this.gridAg.addColumnText("invoiceNr", terms["economy.import.payment.invoicenr"], null);
                    this.gridAg.addColumnText("ocr", this.terms["economy.supplier.invoice.ocr"], null, true);
                    this.gridAg.addColumnSelect("billingTypeName", terms["economy.supplier.invoice.invoicetype"], null, { selectOptions: this.invoiceBillingTypes, displayField: "billingTypeName" });
                    this.gridAg.addColumnSelect("statusName", terms["common.tracerows.status"], null, { selectOptions: this.originStatus, displayField: "statusName" });
                    this.gridAg.addColumnText("supplierNr", terms["economy.supplier.supplier.suppliernr.grid"], null, true);
                    this.gridAg.addColumnText("supplierName", terms["economy.supplier.supplier.suppliername.grid"], null);
                    this.gridAg.addColumnText("description", terms["economy.supplier.invoice.description"], null, true, { hide: true });
                    this.gridAg.addColumnNumber("totalAmountExVat", terms["economy.supplier.invoice.amountexvat"], null, { enableHiding:true,decimals:2 });
                    this.gridAg.addColumnNumber("totalAmount", terms["economy.supplier.invoice.amountincvat"], null, { enableHiding: false, decimals: 2 });
                    this.gridAg.addColumnNumber("payAmount", terms["economy.import.payment.invoicetotalamount"], null, { enableHiding: false, decimals: 2, editable: true }); //.cellClass = (params) => { return (params.data.payAmountModified ? " modifiedCell" : ""); };
                    if (this.hasCurrencyPermission) {
                        this.gridAg.addColumnNumber("totalAmountCurrency", terms["economy.supplier.invoice.foreignamount"], null, { enableHiding: true, decimals: 2 });
                        this.gridAg.addColumnNumber("payAmountCurrency", terms["economy.supplier.payment.topayamountforeign"], null, { enableHiding: true, decimals: 2, editable:true });
                        this.gridAg.addColumnText("currencyCode", terms["economy.supplier.invoice.currencycode"], null, true);
                    }
                    this.gridAg.addColumnDate("invoiceDate", terms["economy.supplier.invoice.invoicedate"], null);
                    this.gridAg.addColumnDate("dueDate", terms["economy.supplier.invoice.duedate"], null);
                    this.gridAg.addColumnDate("payDate", terms["economy.supplier.payment.changepaydate"], null, true, null, { editable: true, minDate: this.allowedPaymentMinDate }); //.cellClass = (params) => { return (params.data.payDateModified ? " modifiedCell" : ""); };
                    this.gridAg.addColumnDate("voucherDate", this.terms["economy.supplier.invoice.voucherdate"], null, true, null, { hide: true });
                    this.gridAg.addColumnDate("timeDiscountDate", this.terms["economy.supplier.invoice.timediscountdate"], null, true, null, { hide: true });
                    this.gridAg.addColumnSelect("paymentInformationRowId", this.terms["economy.supplier.payment.paymentnr"], null,
                        {
                            editable: true,
                            enableHiding: true,
                            displayField: "paymentNrString",
                            dropdownIdLabel: "paymentInformationRowId",
                            dropdownValueLabel: "paymentNr",
                            selectOptions: [],
                            //onChanged: ({ data }) => this.paymentInfoChanged(data),
                            dynamicSelectOptions: {
                                idField: "paymentInformationRowId",
                                displayField: "paymentNr",
                                options: "validPaymentInformations"
                            },
                            populateFilterFromGrid: true,
                        }); //.cellClass = (params) => { return (params.data.paymentInformationRowIdModified ? " modifiedCell" : ""); };

                    this.gridAg.addColumnIcon(null, terms["economy.supplier.payment.registerpayment"], null, { icon: "fal fa-plus iconEdit", onClick: this.createPayment.bind(this) });
                    
                    this.gridAg.addColumnIcon("attestStateIcon", "...", null, { suppressSorting: false, enableHiding: true, toolTipField: "attestStateMessage", hide: true, showTooltipFieldInFilter: true });

                    this.gridAg.addColumnIcon(null, "", null, { icon: "fal fa-comment-dots", showIcon: this.showAttestCommentIcon.bind(this), onClick: this.showAttestCommentDialog.bind(this) });

                    this.gridAg.addColumnIcon("infoIconValue", null, null, { onClick: this.showInformationMessage.bind(this), toolTipField: "infoIconTooltip" });
                    this.gridAg.addColumnIcon(null, null, null, { icon: "fal fa-unlock-alt warningColor", showIcon: (row) => row.blockPayment, onClick: this.showBlockReason.bind(this), toolTipField: "blockReason" });
                    this.gridAg.addColumnEdit(terms["economy.supplier.payment.showinvoice"], this.edit.bind(this));
                    break;
                case SoeOriginStatusClassification.SupplierPaymentSuggestions:
                    this.gridAg.addColumnIsModified();
                    this.gridAg.addColumnNumber("invoiceSeqNr", terms["economy.import.payment.invoiceseries"], null, { alignLeft: true, formatAsText: true });
                    this.gridAg.addColumnText("invoiceNr", terms["economy.import.payment.invoicenr"], null);
                    this.gridAg.addColumnText("ocr", this.terms["economy.supplier.invoice.ocr"], null, true);
                    //this.gridAg.addColumnSelect("paymentInformationRowId", terms["economy.supplier.payment.paymentnr"], null, null, true, true, "paymentNrString", "paymentInformationRowId", "paymentNr", "paymentInfoChanged", "ctrl", "validPaymentInformations");
                    //this.gridAg.addColumnSelect("paymentInformationRowId", this.terms["economy.supplier.payment.paymentnr"], null, { enableHiding: true, displayField: "paymentNrString", dropdownIdLabel: "paymentInformationRowId", dropdownValueLabel: "paymentNr", selectOptions: null });
                    this.gridAg.addColumnText("sequenceNumber", terms["economy.supplier.payment.suggestionnr"], null);
                    this.gridAg.addColumnSelect("billingTypeName", terms["economy.supplier.invoice.invoicetype"], null, { selectOptions: this.invoiceBillingTypes, displayField: "billingTypeName" });
                    this.gridAg.addColumnSelect("statusName", terms["common.tracerows.status"], null, { selectOptions: this.originStatus, displayField: "statusName" });
                    this.gridAg.addColumnText("supplierNr", terms["economy.supplier.supplier.suppliernr.grid"], null, true);
                    this.gridAg.addColumnText("supplierName", terms["economy.supplier.supplier.suppliername.grid"], null);
                    this.gridAg.addColumnText("description", terms["economy.supplier.invoice.description"], null, true, { hide: true });
                    this.gridAg.addColumnNumber("totalAmountExVat", terms["economy.supplier.invoice.amountexvat"], null, { enableHiding: true, decimals: 2 });
                    this.gridAg.addColumnNumber("totalAmount", terms["economy.supplier.invoice.amountincvat"], null, { enableHiding: false, decimals: 2 });
                    this.gridAg.addColumnNumber("paymentAmount", terms["economy.supplier.payment.topaymentamount"], null, { enableHiding: false, decimals: 2 });
                    if (this.hasCurrencyPermission) {
                        this.gridAg.addColumnNumber("totalAmountCurrency", terms["economy.supplier.invoice.foreignamount"], null, { enableHiding: true, decimals: 2 });
                        this.gridAg.addColumnNumber("paymentAmountCurrency", terms["economy.supplier.payment.topaymentamountforeign"], null, { enableHiding: true, decimals: 2 });
                        this.gridAg.addColumnText("currencyCode", terms["economy.supplier.invoice.currencycode"], null, true);
                    }
                    this.gridAg.addColumnDate("invoiceDate", terms["economy.supplier.invoice.invoicedate"], null);
                    this.gridAg.addColumnDate("dueDate", terms["economy.supplier.invoice.duedate"], null);
                    this.gridAg.addColumnDate("payDate", terms["economy.supplier.payment.paymentdate"], null);
                    this.gridAg.addColumnSelect("paymentInformationRowId", this.terms["economy.supplier.payment.paymentnr"], null,
                        {
                            editable: false,
                            enableHiding: true,
                            displayField: "paymentNrString",
                            dropdownIdLabel: "paymentInformationRowId",
                            dropdownValueLabel: "paymentNr",
                            selectOptions: [],
                            //onChanged: ({ data }) => this.paymentInfoChanged(data),
                            dynamicSelectOptions: {
                                idField: "paymentInformationRowId",
                                displayField: "paymentNr",
                                options: "validPaymentInformations"
                            },
                            populateFilterFromGrid: true,
                        }); //.cellClass = (params) => { return (params.data.paymentInformationRowIdModified ? " modifiedCell" : ""); };

                    //super.addColumnEdit(terms["core.edit"]);
                    this.gridAg.addColumnEdit(terms["economy.supplier.payment.editpayment"], this.edit.bind(this));
                    this.gridAg.addColumnIcon("attestStateIcon", "...", null, { suppressSorting: false, enableHiding: true, toolTipField: "attestStateMessage", hide: true, showTooltipFieldInFilter: true });

                    break;
                case SoeOriginStatusClassification.SupplierPaymentsPayed:
                    this.gridAg.addColumnIsModified();
                    this.gridAg.addColumnNumber("invoiceSeqNr", terms["economy.import.payment.invoiceseries"], null, { alignLeft: true, formatAsText: true, clearZero: true });
                    this.gridAg.addColumnText("invoiceNr", terms["economy.import.payment.invoicenr"], null);
                    this.gridAg.addColumnText("ocr", this.terms["economy.supplier.invoice.ocr"], null, true);
                    this.gridAg.addColumnNumber("paymentSeqNr", terms["common.customer.payment.paymentseqnr"], null);
                    this.gridAg.addColumnText("paymentNrString", terms["economy.supplier.payment.paymentnr"], null);
                    this.gridAg.addColumnSelect("billingTypeName", terms["economy.supplier.invoice.invoicetype"], null, { selectOptions: this.invoiceBillingTypes, displayField: "billingTypeName" });
                    this.gridAg.addColumnSelect("statusName", terms["common.tracerows.status"], null, { selectOptions: this.originStatus, displayField: "statusName" });
                    this.gridAg.addColumnText("supplierNr", terms["economy.supplier.supplier.suppliernr.grid"], null,true);
                    this.gridAg.addColumnText("supplierName", terms["economy.supplier.supplier.suppliername.grid"], null);
                    this.gridAg.addColumnText("description", terms["economy.supplier.invoice.description"], null, true, { hide: true });
                    this.gridAg.addColumnText("paymentStatus", terms["economy.import.payment.paymentstatus"], null, true, { hide: true });
                    this.gridAg.addColumnNumber("paymentAmount", terms["economy.supplier.payment.paymentamount"], null, { enableHiding: true, decimals: 2, editable: this.hasPaidEditDateAndAmountPermission });
                    if (this.hasCurrencyPermission) {
                        this.gridAg.addColumnNumber("paymentAmountCurrency", terms["economy.supplier.payment.paymentamountforeign"], null, { enableHiding: true, decimals: 2, editable: this.hasPaidEditDateAndAmountPermission });
                        this.gridAg.addColumnText("currencyCode", terms["economy.supplier.invoice.currencycode"], null, true);
                    }
                    this.gridAg.addColumnDate("invoiceDate", terms["economy.supplier.invoice.invoicedate"], null);
                    this.gridAg.addColumnDate("dueDate", terms["economy.supplier.invoice.duedate"], null);
                    this.gridAg.addColumnDate("payDate", terms["economy.supplier.payment.paymentdate"], null, false, null, { editable: this.hasPaidEditDateAndAmountPermission});

                    this.gridAg.addColumnEdit(terms["economy.supplier.payment.editpayment"], this.edit.bind(this));
                    this.gridAg.addColumnIcon("attestStateIcon", "...", null, { suppressSorting: false, enableHiding: true, toolTipField: "attestStateMessage", hide: true, showTooltipFieldInFilter: true });

                    break;
                case SoeOriginStatusClassification.SupplierPaymentsVoucher:
                    this.gridAg.addColumnNumber("invoiceSeqNr", terms["economy.import.payment.invoiceseries"], null, { alignLeft: true, formatAsText: true, clearZero: true });
                    this.gridAg.addColumnText("invoiceNr", terms["economy.import.payment.invoicenr"], null);
                    this.gridAg.addColumnText("ocr", this.terms["economy.supplier.invoice.ocr"], null, true);
                    this.gridAg.addColumnNumber("paymentSeqNr", terms["common.customer.payment.paymentseqnr"], null);
                    this.gridAg.addColumnText("paymentNrString", terms["economy.supplier.payment.paymentnr"], null);
                    this.gridAg.addColumnSelect("billingTypeName", terms["economy.supplier.invoice.invoicetype"], null, { selectOptions: this.invoiceBillingTypes, displayField: "billingTypeName" });
                    this.gridAg.addColumnSelect("statusName", terms["common.tracerows.status"], null, { selectOptions: this.originStatus, displayField: "statusName" });
                    this.gridAg.addColumnText("supplierNr", terms["economy.supplier.supplier.suppliernr.grid"], null, true);
                    this.gridAg.addColumnText("supplierName", terms["economy.supplier.supplier.suppliername.grid"], null);
                    this.gridAg.addColumnText("description", terms["economy.supplier.invoice.description"], null, true, { hide: true });
                    this.gridAg.addColumnNumber("paymentAmount", terms["economy.supplier.payment.paymentamount"], null, { enableHiding: false, decimals: 2 });
                    if (this.hasCurrencyPermission) {
                        this.gridAg.addColumnNumber("paymentAmountCurrency", terms["economy.supplier.payment.paymentamountforeign"], null, { enableHiding: true, decimals: 2 });
                        this.gridAg.addColumnText("currencyCode", terms["economy.supplier.invoice.currencycode"], null, true);
                    }
                    this.gridAg.addColumnDate("invoiceDate", terms["economy.supplier.invoice.invoicedate"], null);
                    this.gridAg.addColumnDate("dueDate", terms["economy.supplier.invoice.duedate"], null);
                    this.gridAg.addColumnDate("payDate", terms["economy.supplier.payment.paymentdate"], null);
                    this.gridAg.addColumnIcon(null, terms["economy.supplier.payment.showpayment"], null, { icon: "fal fa-file-search iconEdit", onClick: this.edit.bind(this) });
                    this.gridAg.addColumnIcon("attestStateIcon", "...", null, { suppressSorting: false, enableHiding: true, toolTipField: "attestStateMessage", hide: true, showTooltipFieldInFilter: true });

                    break;
            }

            this.gridAg.options.getColumnDefs().forEach(f => {
                var cellCls: string = f.cellClass ? f.cellClass.toString() : "";
                f.cellClass = (grid: any) => {
                    // Append closedRow to cellClass
                    var newCls: string = cellCls;
                    if (grid.data.useClosedStyle)
                        newCls += " closedRow";
                    else if (grid.data.blockPayment)
                        newCls += " warningRow";

                    // Append modifiedCell to cellClass on editable columns
                    if (f.field === 'payAmount') {
                        newCls += (grid.data.payAmountModified ? " modifiedCell" : "");
                    } else if (f.field === 'paidAmount') {
                        newCls += (grid.data.paidAmountModified ? " modifiedCell" : "");
                    } else if (f.field === 'payDate') {
                        newCls += (grid.data.payDateModified ? " modifiedCell" : "");
                    } else if (f.field === 'paymentInformationRowId') {
                        newCls += (grid.data.paymentInformationRowIdModified ? " modifiedCell" : "");
                    }
                    else if (f.field === "dueDate") {
                        if (grid.data['isOverdue'] && this.classification === SoeOriginStatusClassification.SupplierPaymentsUnpayed)
                            newCls += " errorRow";
                    }

                    return newCls;
                };
            });

            // Subscribe to grid events
            const events: GridEvent[] = [];
            events.push(new GridEvent(SoeGridOptionsEvent.AfterCellEdit, (entity, colDef, newValue, oldValue) => { this.afterCellEdit(entity, colDef, newValue, oldValue); }));
            events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChanged, (row: uiGrid.IGridRow) => { this.summarizeSelected(); }));
            events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, () => { this.summarizeSelected(); }));
            events.push(new GridEvent(SoeGridOptionsEvent.RowsVisibleChanged, (rows: SupplierPaymentGridDTO[]) => { this.summarizeFiltered(rows); }));
            this.gridAg.options.subscribe(events);

            this.gridAg.finalizeInitGrid("economy.supplier.payment.payment", true);
        });
    }

    private afterCellEdit(row: SupplierPaymentGridDTO, colDef: uiGrid.IColumnDef, newValue, oldValue) {
        if (newValue === oldValue)
            return;

        if ((this.classification === SoeOriginStatusClassification.SupplierPaymentsPayed) &&
            (row.currencyRate) && (row.currencyRate !== 1)) {
            this.notificationService.showDialog(this.terms["core.warning"], this.terms["economy.supplier.payment.blockchangeingrid"], SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK);
            row[colDef.field] = oldValue;
            row.isModified = false;
            return false;
        }
        
        switch (colDef.field) {
            case 'payAmount':
            case 'paidAmount':
            case 'paymentAmount':
                if (colDef.field === 'payAmount' && !row['payAmountOriginal'])
                    row['payAmountOriginal'] = oldValue;
                else if (colDef.field === 'paidAmount' && !row['paidAmountOriginal'])
                    row['paidAmountOriginal'] = oldValue;
                else if (colDef.field === 'paymentAmount' && !row['paymentAmountOriginal'])
                    row['paymentAmountOriginal'] = oldValue;

                if (this.classification === SoeOriginStatusClassification.SupplierPaymentsPayed && row.billingTypeId === TermGroup_BillingType.Debit) {
                    this.notificationService.showDialog(this.terms["core.warning"], this.terms["economy.supplier.payment.paidamountchengenotallowed"], SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK);
                    row.paymentAmount = oldValue;
                    row.isModified = false;
                } else {
                    var num: number = NumberUtility.parseDecimal(newValue);
                    if ((row.billingTypeId === TermGroup_BillingType.Debit ? num > (row.totalAmount - row.paidAmount) : num < (row.totalAmount + row.paidAmount)) || num === 0) {
                        this.notificationService.showDialog(this.terms["core.warning"], this.terms["economy.supplier.payment.payamountinvalidmessage"], SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK);
                        row.payAmount = oldValue;
                        row.isModified = false;
                    } else if (!num) {
                        if (colDef.field === 'payAmount')
                            row.payAmount = oldValue;
                        else if (colDef.field === 'paidAmount')
                            row.paidAmount = oldValue;
                        else if (colDef.field === 'paymentAmount')
                            row.paidAmount = oldValue;
                        row.isModified = false;
                    } else {
                        //Set amounts
                        if (colDef.field === 'payAmount')
                            row.payAmount = num;
                        else if (colDef.field === 'paidAmount')
                            row.paidAmount = num;
                        else if (colDef.field === 'paymentAmount')
                            row.paymentAmount = num;

                        //Set currency amount
                        row.payAmountCurrency = row.currencyRate != 0 ? (row.payAmount / row.currencyRate).round(2) : row.payAmount;
                        row.paidAmountCurrency = row.currencyRate != 0 ? (row.paidAmount / row.currencyRate).round(2) : row.paidAmount;
                        row.paymentAmountCurrency = row.currencyRate != 0 ? (row.paymentAmount / row.currencyRate).round(2) : row.payAmount;
                        if (colDef.field === 'payAmount')
                            row['payAmountModified'] = row['payAmountOriginal'] && row['payAmountOriginal'] == newValue ? false : true;
                        else if (colDef.field === 'paidAmount')
                            row['paidAmountModified'] = row['paidAmountOriginal'] && row['paidAmountOriginal'] == newValue ? false : true;
                        else if (colDef.field === 'paymentAmount')
                            row['paymentAmountModified'] = row['paymentAmountOriginal'] && row['paymentAmountOriginal'] == newValue ? false : true;
                        this.gridAg.options.refreshRows(row);
                    }
                }
                this.summarizeSelected();
                break;
            case 'payAmountCurrency':
            case 'paidAmountCurrency':
            case 'paymentAmountCurrency':
                if (colDef.field === 'payAmountCurrency' && !row['payAmountCurrencyOriginal'])
                    row['payAmountCurrencyOriginal'] = oldValue;
                else if (colDef.field === 'paidAmountCurrency' && !row['paidAmountCurrencyOriginal'])
                    row['paidAmountCurrencyOriginal'] = oldValue;
                else if (colDef.field === 'paymentAmountCurrency' && !row['paymentAmountCurrencyOriginal'])
                    row['paymentAmountCurrencyOriginal'] = oldValue;

                if (this.classification === SoeOriginStatusClassification.SupplierPaymentsPayed && row.billingTypeId === TermGroup_BillingType.Debit) {
                    this.notificationService.showDialog(this.terms["core.warning"], this.terms["economy.supplier.payment.paidamountchengenotallowed"], SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK);
                    row.paidAmountCurrency = oldValue;
                    row.isModified = false;
                } else {
                    var num: number = NumberUtility.parseDecimal(newValue);
                    if ((row.billingTypeId === TermGroup_BillingType.Debit ? num > (row.totalAmountCurrency - row.paidAmountCurrency) : num < (row.totalAmountCurrency + row.paidAmountCurrency)) || num === 0) {
                        this.notificationService.showDialog(this.terms["core.warning"], this.terms["economy.supplier.payment.payamountinvalidmessage"], SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK);
                        row.payAmountCurrency = oldValue;
                        row.isModified = false;
                    } else if (!num) {
                        if (colDef.field === 'payAmountCurrency')
                            row.payAmountCurrency = oldValue;
                        else if (colDef.field === 'paidAmountCurrency')
                            row.paidAmountCurrency = oldValue;
                        else if (colDef.field === 'paymentAmountCurrency')
                            row.paymentAmountCurrency = oldValue;
                        row.isModified = false;
                    } else {
                        //Set amounts
                        if (colDef.field === 'payAmountCurrency')
                            row.payAmountCurrency = num;
                        else if (colDef.field === 'paidAmountCurrency')
                            row.paidAmountCurrency = num;
                        else if (colDef.field === 'paymentAmountCurrency')
                            row.paymentAmountCurrency = num;

                        //Set base currency amount
                        row.payAmount = row.currencyRate != 0 ? (row.payAmountCurrency * row.currencyRate).round(2) : row.payAmountCurrency;
                        row.paidAmount = row.currencyRate != 0 ? (row.paidAmountCurrency * row.currencyRate).round(2) : row.paidAmountCurrency;
                        row.paymentAmount = row.currencyRate != 0 ? (row.paymentAmountCurrency * row.currencyRate).round(2) : row.paymentAmountCurrency;
                        if (colDef.field === 'payAmountCurrency')
                            row['payAmountCurrencyModified'] = row['payAmountCurrencyOriginal'] && row['payAmountCurrencyOriginal'] == newValue ? false : true;
                        else if (colDef.field === 'paidAmountCurrency')
                            row['paidAmountCurrencyModified'] = row['paidAmountCurrencyOriginal'] && row['paidAmountCurrencyOriginal'] == newValue ? false : true;
                        else if (colDef.field === 'paymentAmountCurrency')
                            row['paymentAmountCurrencyModified'] = row['paymentAmountCurrencyOriginal'] && row['paymentAmountCurrencyOriginal'] == newValue ? false : true;
                        this.gridAg.options.refreshRows(row);
                    }
                }
                this.summarizeSelected();
                break;
            case 'payDate':
                if (!row['payDateOriginal'])
                    row['payDateOriginal'] = oldValue;
                row['payDateModified'] = row['payDateOriginal'] && CalendarUtility.convertToDate(row['payDateOriginal']).isSameDayAs(CalendarUtility.convertToDate(newValue)) ? false : true;
                
                this.gridAg.options.refreshRows(row);
                break;
            case 'paymentInformationRowId':
                if (!row['paymentInformationRowIdOriginal'])
                    row['paymentInformationRowIdOriginal'] = oldValue;
                this.paymentInfoChanged(row);
                row['paymentInformationRowIdModified'] = row['paymentInformationRowIdOriginal'] && row['paymentInformationRowIdOriginal'] == newValue ? false : true;
                
                //this.gridAg.options.refreshRows(row);
                break;
        }
    }

    private paymentInfoChanged(item: any) {
        if (item) {
            const paymentInfoRow = _.find(this.actorsPaymentInformation[item.supplierId], { paymentInformationRowId: item.paymentInformationRowId });
            if (paymentInfoRow) {
                item.sysPaymentTypeId = paymentInfoRow.sysPaymentTypeId;
                item.paymentNr = paymentInfoRow.paymentNr;
                item.paymentNrString = paymentInfoRow.paymentNrDisplay ? TermGroup_SysPaymentType[item.sysPaymentTypeId] + " " + paymentInfoRow.paymentNrDisplay : paymentInfoRow.paymentNr;
                item.hasNoValidPaymentInfo = false;
            }
        }
    }

    public createPayment(row: any) {
        const message = new TabMessage(
            this.terms["economy.supplier.payment.newpayment"],
            row.paymentRowId,
            EditController,
            { invoiceId: row.supplierInvoiceId },
            this.urlHelperService.getGlobalUrl("Shared/Economy/Supplier/Payments/Views/edit.html"));
        this.messagingHandler.publishEvent(Constants.EVENT_OPEN_TAB, message);
    }


    public edit(row) {

        if (this.classification === SoeOriginStatusClassification.SupplierPaymentsUnpayed) {
            this.showInvoice(row);
        } else if (this.classification === 
            SoeOriginStatusClassification.SupplierPaymentSuggestions
            || this.classification === SoeOriginStatusClassification.SupplierPaymentsPayed
            || this.classification === SoeOriginStatusClassification.SupplierPaymentsVoucher) {
            this.openPayment(row);
        } 
    }


    private openPayment(row) {
        const message = new TabMessage(
            this.terms["economy.supplier.payment.payment"] + " " + row.paymentSeqNr,
            row.paymentRowId,
            EditController,
            { paymentId: row.paymentRowId },
            this.urlHelperService.getGlobalUrl("Shared/Economy/Supplier/Payments/Views/edit.html"));            
        this.messagingHandler.publishEvent(Constants.EVENT_OPEN_TAB, message);
    }

    private showInvoice(row: any) {
        const message = new TabMessage(
            `${this.terms["economy.supplier.invoice.invoice"]} ${row.invoiceNr}`,
            row.supplierInvoiceId,
            InvoiceEditController,
            { id: row.supplierInvoiceId },
            this.urlHelperService.getGlobalUrl("Shared/Economy/Supplier/Invoices/Views/edit.html")
        );
        this.messagingHandler.publishEvent(Constants.EVENT_OPEN_TAB, message);
    }

    public filtersuppliers(filter) {
        return this.suppliers.filter(acc => {
            return acc.name.contains(filter);
        });
    }

    public filterAutogiroInvoices() {
        this.filteredItems = [];
        let hiddenAutoGiroPayments = false;
        
        if (this.hideAutogiro) {            
            for (let i = 0; i < this.items.length; i++) {
                let invoice = this.items[i];
                if (invoice.sysPaymentTypeId != TermGroup_SysPaymentType.Autogiro) {
                    this.filteredItems.push(invoice);
                }
                else if (this.classification === SoeOriginStatusClassification.SupplierPaymentSuggestions) {
                    hiddenAutoGiroPayments = true;
                }
            };            

            if (hiddenAutoGiroPayments) {
                this.notificationService.showDialog(this.terms["core.information"], this.terms["economy.supplier.payment.hiddenautogiromessage"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
            }

        }
        else {
            this.filteredItems = this.items;
        }

        this.setData(this.filteredItems);
    }

    public loadGridData() {
        this.actors = [];
        this.dataLoaded = false;

        this.progress.startLoadingProgress([() => {
            return this.supplierService.getPayments(this.classification, this.allItemsSelection).then((x) => {
                this.items = x;
                this.items = this.items.map(ca => {
                    var obj = new SupplierPaymentGridDTO();
                    angular.extend(obj, ca);
                    return obj;
                });

                _.forEach(this.items, (y: SupplierPaymentGridDTO) => {//Fix dates
                    y.payDate = new Date(<any>y.payDate).date();
                    y.invoiceDate = new Date(<any>y.invoiceDate).date();
                    y.dueDate = new Date(<any>y.dueDate).date();
                    y.voucherDate = new Date(<any>y.voucherDate).date();
                    

                    if (y.timeDiscountDate) {
                        y.timeDiscountDate = new Date(<any>y.timeDiscountDate).date();
                    }

                    if (this.showPaymentInformation) {
                        if (_.includes(this.actors, y.supplierId) === false)
                            this.actors.push(y.supplierId);
                    }
                    else {
                        y.paymentNrString = TermGroup_SysPaymentType[y.sysPaymentTypeId] + " " + y.paymentNr;
                    }

                    if (this.classification === SoeOriginStatusClassification.SupplierPaymentsUnpayed) {
                        //Time discount on amounts
                        if (this.supplierUseTimeDiscount === true) {
                            if (y.timeDiscountPercent != null && y.timeDiscountPercent != 0 && y.timeDiscountDate != null && y.timeDiscountDate >= this.dateToday) {
                                y.payAmount = ((100 - y.timeDiscountPercent) / 100 * y.payAmount).round(2);
                                y.payAmountCurrency = ((100 - y.timeDiscountPercent) / 100 * y.payAmountCurrency).round(2);
                            }
                        }

                        //Set dates
                        var today: Date = this.dateToday;
                        if (this.supplierSetDefaultPayDateAsDueDate === false) {
                            //PayDate
                            if (y.paidAmount === 0) {
                                //Don show PayDate if not paíd
                                y.payDate = null;
                            }
                            else {
                                //You cannot have paydate in history (Bank rejects payment's and if payment's go through with currdate > bookkeeping does not match with account history)
                                if (y.dueDate < today)
                                    y.payDate = today;
                                else
                                    y.payDate = y.dueDate;
                            }
                        }
                        else if (this.supplierUseTimeDiscount === true) {
                            if (y.timeDiscountDate != null) {
                                y.payDate = y.timeDiscountDate >= today ? y.timeDiscountDate : (y.dueDate >= today ? y.dueDate : today);
                            }
                            else {
                                //You cannot have paydate in history (Bank rejects payment's and if payment's go through with currdate > bookkeeping does not match with account history)
                                if (y.dueDate < today)
                                    y.payDate = today;
                                else
                                    y.payDate = y.dueDate;
                            }
                        }
                        else {
                            //You cannot have paydate in history (Bank rejects payment's and if payment's go through with currdate > bookkeeping does not match with account history)
                            if (y.dueDate < today)
                                y.payDate = today;
                            else
                                y.payDate = y.dueDate;
                        }

                        if (y.dueDate < today)
                            y['isOverdue'] = true;

                        y.expandableDataIsLoaded = false;
                    }

                    //Attest
                    var checkValue = (y.attestStateId == null || y.attestStateId === 0);

                    if (checkValue === true) {
                        y.attestStateIcon = "fas fa-circle errorColor"
                        y.attestStateMessage = this.terms["economy.supplier.payment.missingatteststatus"];
                    }
                    else {
                        var attestState = _.find(this.attestStates, { attestStateId: y.attestStateId });

                        if (attestState != null) {
                            if (attestState.hidden === true) {
                                y.attestStateIcon = "fas fa-circle errorColor"
                                y.attestStateMessage = this.terms["economy.supplier.payment.hiddenattest"];

                            }
                            else {
                                if (attestState.closed === true) {
                                    y.attestStateIcon = "fas fa-circle okColor";
                                    y.attestStateMessage = this.terms["economy.supplier.payment.attested"];
                                }
                                else {
                                    y.attestStateIcon = "fas fa-circle warningColor";
                                    y.attestStateMessage = this.terms["economy.supplier.payment.underattest"];
                                }
                            }
                        }
                        else {
                            y.attestStateIcon = "fas fa-circle errorColor"
                            y.attestStateMessage = this.terms["economy.supplier.payment.missingatteststatus"];
                        }
                    }

                    this.setInformationIconAndTooltip(y);
                });

                this.filteredItems = this.items;

                this.summarize(this.items);
                this.summarizeSelected();

                if (this.showPaymentInformation) {
                    this.loadPaymentInformationFromActorsForType(true);
                }
                else {
                    this.setData(this.items);
                    this.dataLoaded = true;
                }
            });
        }]);
    }

    public setInformationIconAndTooltip(item: SupplierPaymentGridDTO) {
        var hasInfo: boolean = false;
        var hasError: boolean = false;
        var flaggedEnum: FlaggedEnum.IFlaggedEnum = FlaggedEnum.create(SoeStatusIcon, SoeStatusIcon.ElectronicallyDistributed);
        var statusIcons: FlaggedEnum.IFlaggedEnum = new flaggedEnum(item.statusIcon);
        if (item.paidAmount != 0 && item.fullyPaid === false) {
            hasInfo = true;
        }
        if (this.supplierUseTimeDiscount === true && item.fullyPaid === false &&
            item.timeDiscountDate != null && item.timeDiscountDate >= this.dateToday &&
            item.timeDiscountPercent != null && item.timeDiscountPercent != 0) {
            hasInfo = true;
        }

        if (statusIcons.contains(SoeStatusIcon.Imported)) {
            item.infoIconValue = "fal fa-download";
        }
        else if (hasInfo) {
            item.infoIconValue = "fal fa-info-circle infoColor";
            item.infoIconTooltip = this.terms["core.showinfo"];
        }
        else if (item.statusIcon != SoeStatusIcon.None) {
            if (!statusIcons.contains(SoeStatusIcon.Email) && !statusIcons.contains(SoeStatusIcon.ElectronicallyDistributed)) {
                item.infoIconValue = "fal fa-paperclip";

                if (statusIcons.contains(SoeStatusIcon.Imported))
                    item.infoIconTooltip = item.infoIconTooltip && item.infoIconTooltip != "" ? "<br/>" + this.terms["common.imported"] : this.terms["common.imported"];
                if (statusIcons.contains(SoeStatusIcon.Attachment))
                    item.infoIconTooltip = item.infoIconTooltip && item.infoIconTooltip != "" ? "<br/>" + this.terms["common.hasaattachedfiles"] : this.terms["common.hasaattachedfiles"];
                if (statusIcons.contains(SoeStatusIcon.Image))
                    item.infoIconTooltip = item.infoIconTooltip && item.infoIconTooltip != "" ? "<br/>" + this.terms["common.hasattachedimages"] : this.terms["common.hasattachedimages"];
            }
        }
    }

    public updateItemsSelection() {
        this.coreService.saveIntSetting(SettingMainType.User, UserSettingType.SupplierPaymentsAllItemsSelection, this.allItemsSelection).then((x) => {
            this.loadGridData();
        });
    }

    protected showScannedState(row) {
        if (row.typeName === "Scanning")
            return true;
        else
            return false;
    }

    private showAttestCommentIcon(row: any) {
        return row.hasAttestComment;
    }

    private showAttestCommentDialog(row: any) {
        this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Common/Dialogs/ShowAttestCommentsAndAnswersDialog/ShowAttestCommentsAndAnswers.html"),
            controller: ShowAttestCommentsAndAnswersDialogController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'lg',
            resolve: {
                translationService: () => { return this.translationService },
                coreService: () => { return this.coreService },
                supplierService: () => { return this.supplierService },
                invoiceId: () => { return row.supplierInvoiceId },
                registeredTerm: () => { return this.terms["core.attestflowregistered"] }
            }
        });
    }

    private showInformationMessage(row: SupplierPaymentGridDTO) {
        var message: string = "";
        if (!row.fullyPaid) {
            var isTotalAmountPaid: boolean = false;
            var partlyPaid: boolean = false;
            var partlyPaidForeign: boolean = false;
            if (row.paidAmount >= 0) {
                isTotalAmountPaid = row.paidAmount != 0 && row.paidAmount >= row.totalAmount;
                partlyPaid = row.paidAmount != 0 && row.paidAmount < row.totalAmount;
                partlyPaidForeign = row.paidAmountCurrency != 0 && row.paidAmountCurrency < row.totalAmountCurrency;
            }
            else {
                isTotalAmountPaid = row.paidAmount != 0 && row.paidAmount <= row.totalAmount;
                partlyPaid = row.paidAmount != 0 && row.paidAmount > row.totalAmount;
                partlyPaidForeign = row.paidAmountCurrency != 0 && row.paidAmountCurrency > row.totalAmountCurrency;
            }

            if (isTotalAmountPaid) {
                message = message + this.terms["economy.supplier.invoice.paidlate"] + "<br/>";
                message = message + this.terms["economy.supplier.invoice.matches.totalamount"] + ": " + (row.sysCurrencyId != this.coreBaseCurrency ? row.totalAmountCurrency.toString() : row.totalAmount.toString()) + "<br/>";
                message = message + this.terms["economy.supplier.payment.paymentamount"] + ": " + (row.sysCurrencyId != this.coreBaseCurrency ? row.paidAmountCurrency.toString() : row.paidAmount.toString()) + "<br/>";
                message = message + this.terms["economy.supplier.invoice.amounttopay"] + ": " + (row.sysCurrencyId != this.coreBaseCurrency ? row.payAmountCurrency.toString() : row.payAmount.toString()) + "<br/>";
            }
            else if ((partlyPaid || partlyPaidForeign) && (this.supplierUseTimeDiscount == false || row.timeDiscountDate == null)) {
                message = message + this.terms["economy.supplier.invoice.partlypaid"] + "<br/>";
                message = message + this.terms["economy.supplier.invoice.matches.totalamount"] + ": " + (row.sysCurrencyId != this.coreBaseCurrency ? row.totalAmountCurrency.toString() : row.totalAmount.toString()) + "<br/>";
                message = message + this.terms["economy.supplier.payment.paymentamount"] + ": " + (row.sysCurrencyId != this.coreBaseCurrency ? row.paidAmountCurrency.toString() : row.paidAmount.toString()) + "<br/>";
                message = message + this.terms["economy.supplier.invoice.amounttopay"] + ": " + (row.sysCurrencyId != this.coreBaseCurrency ? row.payAmountCurrency.toString() : row.payAmount.toString()) + "<br/>";
            }
            else if (partlyPaid && this.supplierUseTimeDiscount == true && row.timeDiscountDate != null) {
                message = message + this.terms["economy.supplier.invoice.partlypaid"] + "<br/>";
                if (row.timeDiscountDate >= row.payDate)
                    message = message + this.terms["economy.supplier.invoice.hastimediscount"] + "<br/>";
                message = message + this.terms["economy.supplier.invoice.matches.totalamount"] + ": " + (row.sysCurrencyId != this.coreBaseCurrency ? row.totalAmountCurrency.toString() : row.totalAmount.toString()) + "<br/>";
                message = message + this.terms["economy.supplier.payment.paymentamount"] + ": " + (row.sysCurrencyId != this.coreBaseCurrency ? row.paidAmountCurrency.toString() : row.paidAmount.toString()) + "<br/>";
                if (row.timeDiscountDate >= row.payDate)
                    message = message + this.terms["economy.supplier.invoice.timediscount"] + ": " + (row.sysCurrencyId != this.coreBaseCurrency ? (row.totalAmountCurrency - row.paidAmountCurrency - row.payAmountCurrency).round(2).toString() : (row.totalAmount - row.paidAmount - row.payAmount).round(2).toString()) + "<br/>";
                message = message + this.terms["economy.supplier.invoice.amounttopay"] + ": " + (row.sysCurrencyId != this.coreBaseCurrency ? row.payAmountCurrency.round(2).toString() : row.payAmount.round(2).toString()) + "<br/>";
            }
            else if (!partlyPaid && this.supplierUseTimeDiscount == true && row.timeDiscountDate != null) {
                message = message + this.terms["economy.supplier.invoice.hastimediscount"] + "<br/>";
                message = message + this.terms["economy.supplier.invoice.matches.totalamount"] + ": " + (row.sysCurrencyId != this.coreBaseCurrency ? row.totalAmountCurrency.toString() : row.totalAmount.toString()) + "<br/>";
                message = message + this.terms["economy.supplier.invoice.timediscount"] + ": " + (row.sysCurrencyId != this.coreBaseCurrency ? (row.totalAmountCurrency - row.payAmountCurrency).round(2).toString() : (row.totalAmount - row.payAmount).round(2).toString()) + "<br/>";
                message = message + this.terms["economy.supplier.invoice.amounttopay"] + ": " + (row.sysCurrencyId != this.coreBaseCurrency ? row.payAmountCurrency.round(2).toString() : row.payAmount.round(2).toString()) + "<br/>";
            }

            if (row.multipleDebtRows) {
                if (message != "")
                    message = message + "---<br/>";

                message = message + this.terms["economy.supplier.invoice.multipledebtrows"] + "<br/>";
                message = message + this.terms["economy.supplier.invoice.manualadjustmentneeded"] + "<br/>";
            }

        }
        else {
            message = row.infoIconMessage;
        }

        if (message != "")
            this.notificationService.showDialog(this.terms["core.information"], message, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
    }

    private showBlockReason(row: SupplierPaymentGridDTO) {
        this.notificationService.showDialog(this.terms["common.reason"], row.blockReason, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
    }

    private getPermissions(): any[] {
        var features: any[] = [];
        features.push({ feature: Feature.Economy_Supplier_Invoice_Status, loadReadPermissions: true, loadModifyPermissions: true });

        features.push({ feature: Feature.Economy_Supplier_Invoice_Invoices_All, loadModifyPermissions: true });
        features.push({ feature: Feature.Economy_Supplier_Invoice_Invoices, loadModifyPermissions: true });
        features.push({ feature: Feature.Economy_Supplier_Invoice_Status_Foreign, loadModifyPermissions: true });
        features.push({ feature: Feature.Economy_Supplier_Invoice_Status_DraftToOrigin, loadModifyPermissions: true });
        features.push({ feature: Feature.Economy_Supplier_Invoice_Status_OriginToVoucher, loadModifyPermissions: true });
        features.push({ feature: Feature.Economy_Supplier_Invoice_Status_OriginToPayment, loadModifyPermissions: true });
        features.push({ feature: Feature.Economy_Supplier_Invoice_Status_OriginToPaymentSuggestion, loadModifyPermissions: true });
        features.push({ feature: Feature.Economy_Supplier_Invoice_Status_PaymentSuggestionToCancel, loadModifyPermissions: true });

        features.push({ feature: Feature.Economy_Supplier_Invoice_Status_PayedEditDateAndAmount, loadModifyPermissions: true });
        features.push({ feature: Feature.Economy_Supplier_Invoice_Status_PayedToVoucher, loadModifyPermissions: true });
        features.push({ feature: Feature.Economy_Supplier_Invoice_Status_PayedToCancel, loadModifyPermissions: true });
        features.push({ feature: Feature.Economy_Supplier_Invoice_AttestFlow, loadModifyPermissions: true });
        features.push({ feature: Feature.Economy_Supplier_Invoice_Status_SendPayment, loadModifyPermissions: true });

        features.push({ feature: Feature.Economy_Supplier_Payment_Send_Notification, loadModifyPermissions: true });
        
        return features;
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {

        this.readPermission = response[Feature.Economy_Supplier_Invoice_Status].readPermission;
        this.modifyPermission = response[Feature.Economy_Supplier_Invoice_Status].modifyPermission;

        if (this.modifyPermission)
            this.messagingHandler.publishActivateAddTab();

        this.hasCurrencyPermission = response[Feature.Economy_Supplier_Invoice_Status_Foreign].modifyPermission;
        this.hasSuggestionPermission = response[Feature.Economy_Supplier_Invoice_Status_OriginToPaymentSuggestion].modifyPermission;
        this.hasSuggestionToCancelPermission = response[Feature.Economy_Supplier_Invoice_Status_PaymentSuggestionToCancel].modifyPermission;
        this.hasOriginToPaymentPermission = response[Feature.Economy_Supplier_Invoice_Status_OriginToPayment].modifyPermission;
        this.hasPaidToVoucherPermission = response[Feature.Economy_Supplier_Invoice_Status_PayedToVoucher].modifyPermission;
        this.hasPaidEditDateAndAmountPermission = response[Feature.Economy_Supplier_Invoice_Status_PayedEditDateAndAmount].modifyPermission;
        this.hasPaidToCancelPermission = response[Feature.Economy_Supplier_Invoice_Status_PayedToCancel].modifyPermission;
        this.hasAttestPermission = response[Feature.Economy_Supplier_Invoice_AttestFlow].modifyPermission;
        this.hasOpenPermission = response[Feature.Economy_Supplier_Invoice_AttestFlow].modifyPermission;
        this.hasSendPaymentPermission = response[Feature.Economy_Supplier_Invoice_Status_SendPayment].modifyPermission;
        this.hasSendPaymentNotificationPermission = response[Feature.Economy_Supplier_Payment_Send_Notification].modifyPermission;

        if ( response[Feature.Economy_Supplier_Invoice_Invoices_All].modifyPermission ) {
            this.hasOpenPermission = true;
            this.hasClosedPermission = true;
        }
        else {
            this.hasOpenPermission = response[Feature.Economy_Supplier_Invoice_Invoices].modifyPermission ||
                    response[Feature.Economy_Supplier_Invoice_Status_DraftToOrigin].modifyPermission ||
                    response[Feature.Economy_Supplier_Invoice_Status_OriginToVoucher].modifyPermission;

            this.hasClosedPermission = response[Feature.Economy_Supplier_Invoice_Invoices].modifyPermission ||
                response[Feature.Economy_Supplier_Invoice_Status_OriginToVoucher].modifyPermission;
        }
    }

    private checkIfUsesSignatoryContract(): ng.IPromise<any> {
        return this.coreService.signatoryContractUsesPermission(TermGroup_SignatoryContractPermissionType.AccountsPayable_SendPaymentToBank).then(val => {
            this.usesSignatoryContractForSendingPayments = val;
        })

    }

    private loadUserSettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];
        settingTypes.push(UserSettingType.SupplierPaymentsAllItemsSelection);

        return this.coreService.getUserSettings(settingTypes).then(x => {
            this._allItemsSelection = SettingsUtility.getIntUserSetting(x, UserSettingType.SupplierPaymentsAllItemsSelection, 1, false);
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {

        var settingTypeIds: number[] = [];
        settingTypeIds.push(CompanySettingType.SupplierPaymentManualTransferToVoucher);
        settingTypeIds.push(CompanySettingType.SupplierUseTimeDiscount);
        settingTypeIds.push(CompanySettingType.AccountSupplierUnderpay);
        settingTypeIds.push(CompanySettingType.SupplierPaymentDefaultPaymentMethod);
        settingTypeIds.push(CompanySettingType.SupplierDefaultBalanceList);
        settingTypeIds.push(CompanySettingType.SupplierDefaultChecklistPayments);
        settingTypeIds.push(CompanySettingType.SupplierDefaultPaymentSuggestionList);
        settingTypeIds.push(CompanySettingType.SupplierPaymentAskPrintVoucherOnTransfer);
        settingTypeIds.push(CompanySettingType.AccountingDefaultVoucherList);
        settingTypeIds.push(CompanySettingType.CoreBaseCurrency);
        settingTypeIds.push(CompanySettingType.SupplierHideAutogiroInvoicesFromUnpaid);
        settingTypeIds.push(CompanySettingType.SupplierUsePaymentSuggestions);

        /*
        int defaultSupplierBalanceListReportId = SettingManager.GetIntSetting((int)SettingMainType.Company, (int)CompanySettingType.SupplierDefaultBalanceList, 0, actorCompanyId);
        int defaultSupplierPaymentSuggestionReportId = SettingManager.GetIntSetting((int)SettingMainType.Company, (int)CompanySettingType.SupplierDefaultPaymentSuggestionList, 0, actorCompanyId);*/
        return this.coreService.getCompanySettings(settingTypeIds).then(x => {
            if (x[CompanySettingType.SupplierPaymentManualTransferToVoucher] != null)
                this.supplierPaymentTransferToVoucher = x[CompanySettingType.SupplierPaymentManualTransferToVoucher];
            if (x[CompanySettingType.SupplierUseTimeDiscount] != null)
                this.supplierUseTimeDiscount = x[CompanySettingType.SupplierUseTimeDiscount];
            if (x[CompanySettingType.AccountSupplierUnderpay] != null)
                this.supplierUnderPayAccount = x[CompanySettingType.AccountSupplierUnderpay];
            if (x[CompanySettingType.SupplierPaymentDefaultPaymentMethod] != null)
                this.supplierDefaultPaymentMethod = x[CompanySettingType.SupplierPaymentDefaultPaymentMethod];
            if (x[CompanySettingType.SupplierSetPaymentDefaultPayDateAsDueDate] != null)
                this.supplierSetDefaultPayDateAsDueDate = x[CompanySettingType.SupplierSetPaymentDefaultPayDateAsDueDate];
            if (x[CompanySettingType.SupplierDefaultBalanceList] != null)
                this.supplierBalanceListReportId = x[CompanySettingType.SupplierDefaultBalanceList];
            if (x[CompanySettingType.SupplierDefaultChecklistPayments] !== null)
                this.supplierChecklistPaymentReportId = x[CompanySettingType.SupplierDefaultChecklistPayments];
            if (x[CompanySettingType.SupplierDefaultPaymentSuggestionList] != null)
                this.supplierPaymentSuggestionReportId = x[CompanySettingType.SupplierDefaultPaymentSuggestionList];
            if (x[CompanySettingType.SupplierPaymentAskPrintVoucherOnTransfer] != null)
                this.showPrintVoucher = x[CompanySettingType.SupplierPaymentAskPrintVoucherOnTransfer];
            if (x[CompanySettingType.AccountingDefaultVoucherList] != null)
                this.supplierPaymentVoucherReportId = x[CompanySettingType.AccountingDefaultVoucherList];
            if (x[CompanySettingType.CoreBaseCurrency] != null)
                this.coreBaseCurrency = x[CompanySettingType.CoreBaseCurrency];
            if (x[CompanySettingType.SupplierHideAutogiroInvoicesFromUnpaid] != null) {
                this.hideAutogiro = x[CompanySettingType.SupplierHideAutogiroInvoicesFromUnpaid];
            }
            if (x[CompanySettingType.SupplierUsePaymentSuggestions] != null)
                this.supplierUsePaymentSuggestion = x[CompanySettingType.SupplierUsePaymentSuggestions];
        });
    }

    private loadAttestStates(): ng.IPromise<any> {
        return this.supplierService.getAttestStates(TermGroup_AttestEntity.SupplierInvoice, SoeModule.Economy, false).then((x) => {
            this.attestStates = x;
        });
    }

    private loadSelectionTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.ChangeStatusGridAllItemsSelection, false, true, true).then((x) => {
            this.allItemsSelectionDict = x;
        });
    }

    private loadInvoiceBillingTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.InvoiceBillingType, false, false).then((x) => {
            this.invoiceBillingTypes = [];
            _.forEach(x, (row) => {
                this.invoiceBillingTypes.push({ value: row.name, label: row.name });
            });
        });
    }

    private loadSysPaymentTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.SysPaymentType, false, false).then((x) => {
            this.sysPaymentTypes = x;
        });
    }

    private loadOriginStatus(): ng.IPromise<any> {
        var originType = (this.classification === SoeOriginStatusClassification.SupplierPaymentsPayed || this.classification === SoeOriginStatusClassification.SupplierPaymentsVoucher) ? SoeOriginType.SupplierPayment : SoeOriginType.SupplierInvoice;
        return this.supplierService.getInvoiceAndPaymentStatus(originType, false).then((x) => {
            this.originStatus = [];
            _.forEach(x, (row) => {
                this.originStatus.push({ value: row.name, label: row.name });
            });
        });
    }

    private loadSuppliers(): ng.IPromise<any> {
        return this.supplierService.getSuppliersDict(true, true, true).then((x: ISmallGenericType[]) => {
            this.suppliers = x;
        });
    }

    private loadPaymentMethods(): ng.IPromise<any> {
        return this.supplierService.getPaymentMethods(SoeOriginType.SupplierPayment, true, false, false, true).then((x) => {
            this.paymentMethods = x;
            if (this.supplierDefaultPaymentMethod != null) {
                this.selectedPaymentMethod = _.find(this.paymentMethods, { paymentMethodId: this.supplierDefaultPaymentMethod });
            }
            else {
                if (this.paymentMethods.length > 0)
                    this.selectedPaymentMethod = _.first(this.paymentMethods);
            }
        });
    }

    private loadCurrentAccountYear() {
        return this.coreService.getCurrentAccountYear().then((x) => {
            if (x) {
                this.currentAccountYearId = x.accountYearId;
                this.currentAccountYearFromDate = new Date(x.from);
                this.currentAccountYearToDate = new Date(x.to);
            }
        });
    }

    private loadPaymentInformationFromActors() {
        if (this.actors.length > 0) {
            return this.supplierService.getPaymentInformationFromActor(this.actors).then((x) => {
                this.actorsPaymentInformation = x;
                _.forEach(this.items, (row) => {
                    this.setRowPaymentMethod(row, true);
                });
            });
        }
    }

    private loadPaymentInformationFromActorsForType(setDataLoaded = false) {
        if (this.selectedPaymentMethod) {
            return this.supplierService.getPaymentInformationFromActorForPaymentMethod(this.selectedPaymentMethod.paymentMethodId, this.actors).then((x) => {
                this.actorsPaymentInformation = x;
                for (let i = 0; i < this.items.length; i++) {
                    const row = this.items[i];
                    this.setRowPaymentMethod(row, true);
                };

                if (setDataLoaded) {
                    this.filterAutogiroInvoices();
                    this.dataLoaded = true;
                }

            });
        }
    }

    private executeButtonFunction(option: any, ignoreMultipleCreditsValidation = false) {
        var notAttested: number = 0;
        var wrongPaymentMethodCount: number = 0;
        var timeDiscountLoss: number = 0;
        var validatedItems: SupplierPaymentGridDTO[] = [];
        var validMessage: string = "";
        var invalidMessage: string = "";
        var errorMessage: string = "";
        var successMessage: string = "";

        let selectedItems = this.gridAg.options.getSelectedRows();

        switch (option.id) {
            case SupplierPaymentGridButtonFunctions.CreateSuggestion:
                if (this.ignoreDateValidation || this.ValidateDates(true, option)) {
                    _.forEach(selectedItems, (row: SupplierPaymentGridDTO) => {
                        if ((row.status === SoeOriginStatus.Origin || row.status === SoeOriginStatus.Voucher) && row.fullyPaid === false && row.blockPayment === false && row.supplierBlockPayment === false) {
                            if (row.payDate === null)
                                row.payDate = this.selectedPayDate;

                            if (this.hasAttestPermission && (row.attestStateId == null || row.attestStateId === 0))
                                notAttested++;

                            validatedItems.push(row);
                        }
                    });

                    validMessage += validatedItems.length > 1 ? this.terms["economy.supplier.payment.invoicestransfertosuggestionvalid"] : this.terms["economy.supplier.payment.invoicetransfertosuggestionvalid"];
                    if (this.selectedPayDate != null)
                        validMessage += " (" + this.terms["economy.supplier.payment.paymentdate"] + " " + CalendarUtility.toFormattedDate(this.selectedPayDate) + ")";
                    invalidMessage += (selectedItems.length - validatedItems.length) > 1 ? this.terms["economy.supplier.payment.invoicestransfertosuggestioninvalid"] : this.terms["economy.supplier.payment.invoicetransfertosuggestioninvalid"];
                    successMessage += this.terms["economy.supplier.payment.transferedtosuggestion"];
                    errorMessage += this.terms["economy.supplier.payment.transferedtosuggestionfailed"];

                    this.originStatusChange = SoeOriginStatusChange.SupplierInvoice_OriginToPaymentSuggestion;
                }
                else {
                    return;
                }
                break;
            case SupplierPaymentGridButtonFunctions.Match:
                /*if (selectedItems.length != 2) {
                    this.notificationService.showDialog(this.terms["core.error"], this.terms["economy.supplier.payment.only2invoicesforpaymentmatch"], SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
                    return;
                }
                else if (selectedItems.length == 2) {*/
                    // Check actor 
                    var firstActorId: number = null;
                    var firstCurrencyCode: string;
                    for (var i = 0; i < selectedItems.length; i++) {
                        var row = selectedItems[i];
                        if (row.status === SoeOriginStatus.Origin || row.status === SoeOriginStatus.Voucher) {
                            if (firstActorId === null) {
                                firstActorId = row.supplierId;
                                firstCurrencyCode = row.currencyCode;
                            }
                            else {
                                if (firstActorId != row.supplierId) {
                                    this.notificationService.showDialog(this.terms["core.error"], this.terms["economy.supplier.payment.notsameactor"], SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
                                    return;
                                }
                                if (firstCurrencyCode != row.currencyCode) {
                                    this.notificationService.showDialog(this.terms["core.error"], this.terms["economy.supplier.payment.only1currencyforpaymentmatch"], SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
                                    return;
                                }
                            }

                            validatedItems.push(row);
                        }
                    };

                    // Check amounts
                    /*if (validatedItems.length > 2) {
                        var totalAmount: number = 0;
                        var paidAmount: number = 0;
                        var payAmount: number = 0;
                        _.forEach(selectedItems, (row: SupplierPaymentGridDTO) => {
                            totalAmount += row.totalAmountCurrency;
                            paidAmount += row.paidAmount;
                            payAmount += row.payAmount;
                        });

                        if (totalAmount != 0 || paidAmount != 0 || payAmount != 0) {
                            this.notificationService.showDialog(this.terms["core.warning"], this.terms["economy.supplier.payment.notmatchedamount"], SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK);
                            return;
                        }
                    }
                    else {
                        if ((selectedItems[0].billingTypeId === TermGroup_BillingType.Credit && selectedItems[1].billingTypeId === TermGroup_BillingType.Credit) ||
                            (selectedItems[0].billingTypeId === TermGroup_BillingType.Debit && selectedItems[1].billingTypeId === TermGroup_BillingType.Debit)) {
                            this.notificationService.showDialog(this.terms["core.warning"], this.terms["economy.supplier.payment.notmatchedamount"], SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK);
                            return;
                        }
                    }
*/
                    validMessage += this.terms["economy.supplier.payment.validmatch"];
                    invalidMessage += this.terms["economy.supplier.payment.invalidmatch"];
                    successMessage += this.terms["economy.supplier.payment.invoicesmatched"];
                    errorMessage += this.terms["economy.supplier.payment.invoicesmatchedfailed"];

                    this.originStatusChange = SoeOriginStatusChange.OriginToMatched;
                //}

                break;
            case SupplierPaymentGridButtonFunctions.CreatePaymentFile:
            case SupplierPaymentGridButtonFunctions.SendPaymentFile:
                if (this.ignoreDateValidation || this.ValidateDates(false, option)) {
                    if (this.usesSuggestion) {
                        selectedItems = this.items;
                        _.forEach(selectedItems, (row: SupplierPaymentGridDTO) => {
                            if ((row.status === SoePaymentStatus.Pending || row.status === SoePaymentStatus.Verified || row.status === SoePaymentStatus.Verified) && row.hasVoucher === false) {
                                if (row.hasNoValidPaymentInfo) {
                                    wrongPaymentMethodCount++;
                                }
                                else {
                                    if (this.selectedPayDate != null)
                                        row.payDate = this.selectedPayDate;

                                    validatedItems.push(row);
                                }

                                if (this.hasAttestPermission && (row.attestStateId == null || row.attestStateId === 0))
                                    notAttested++;

                                if (row.timeDiscountDate != null && row.timeDiscountDate >= this.dateToday && this.selectedPayDate != null && this.selectedPayDate > row.timeDiscountDate)
                                    timeDiscountLoss++;

                                if (!this.IsPayDatesEntered(selectedItems) && this.selectedPaymentMethod.sysPaymentMethodId != TermGroup_SysPaymentMethod.SEPA) {
                                    this.notificationService.showDialog(this.terms["core.warning"], this.terms["economy.supplier.payment.needsetdate"], SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK);
                                    return;
                                }
                            }
                        });
                        validMessage += validatedItems.length > 1 
                            ? this.terms["economy.supplier.payment.validcreatepayments"] 
                            : this.terms["economy.supplier.payment.validcreatepayment"];

                        if (this.selectedPayDate != null) {
                            validMessage += "<br\>(" + this.terms["economy.supplier.payment.samepaymentdate"] + " " + CalendarUtility.toFormattedDate(this.selectedPayDate) + ")";
                        }
                        else
                            if (!this.selectedPayDate && this.selectedPaymentMethod.sysPaymentMethodId == TermGroup_SysPaymentMethod.SEPA) {
                                validMessage += "<br\>(" + this.terms["economy.supplier.payment.duedateaspaydate"] + ")";
                            }

                        var selectCount = this.gridAg.options.getSelectedRows().length;
                        if (selectCount != this.items.length) {
                            validMessage += "<br\>";
                            validMessage += this.terms["economy.supplier.payment.suggestiontransferall"] + " " + selectCount.toString() + " " + this.terms["economy.supplier.payment.hasbeenchecked"];
                        }

                        invalidMessage += (selectedItems.length - validatedItems.length) > 1 ? this.terms["economy.supplier.payment.invoicescreatepaymentinvalid"] : this.terms["economy.supplier.payment.invoicecreatepaymentinvalid"];
                        successMessage += this.terms["economy.supplier.payment.paymentcreated"];
                        errorMessage += this.terms["economy.supplier.payment.paymentcreatedfailed"];

                        // Validate ISO
                        if (this.selectedPaymentMethod.sysPaymentMethodId == TermGroup_SysPaymentMethod.ISO20022) {
                            var isInvalid = false;

                            var groups = _.groupBy(validatedItems, 'supplierId');
                            _.forEach(groups, (group) => {
                                const credits = group.filter(item => item.billingTypeId === TermGroup_BillingType.Credit);
                                const debits = group.filter(item => item.billingTypeId !== TermGroup_BillingType.Credit);

                                if (!debits.length) {
                                    isInvalid = true;

                                    let seqNrs = undefined;
                                    _.forEach(credits, (credit) => {
                                        if (!seqNrs)
                                            seqNrs = this.terms["economy.supplier.invoice.suppliershort"] + ": " + group[0].supplierNr + " - " + group[0].supplierName + " " + this.terms["economy.import.payment.invoiceseries"] + ": " + credit.invoiceSeqNr + " " + this.terms["economy.supplier.invoice.paydateshort"] + ": " + credit.payDate.toFormattedDate();
                                        else
                                            seqNrs = seqNrs + "\n" + this.terms["economy.supplier.invoice.suppliershort"] + ": " + group[0].supplierNr + " - " + group[0].supplierName + " " + this.terms["economy.import.payment.invoiceseries"] + ": " + credit.invoiceSeqNr + " " + this.terms["economy.supplier.invoice.paydateshort"] + ": " + credit.payDate.toFormattedDate();
                                    });

                                    let message = this.terms["economy.supplier.invoice.creditnegativeamountmessage"] + "\n\n" + seqNrs + "\n\n" + this.terms["economy.supplier.invoice.credithandlemessage"];
                                    this.notificationService.showDialog(this.terms["core.warning"], message, SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK, SOEMessageBoxSize.Large);
                                    return false;
                                }

                                if (!this.selectedPayDate) {
                                    let seqNrs = undefined;
                                    let hasSingleCredits = false;
                                    _.forEach(credits, (credit) => {
                                        if (!_.find(debits, (debit) => debit.payDate && debit.payDate.toDateString() === credit.payDate.toDateString())) {
                                            if (!seqNrs)
                                                seqNrs = this.terms["economy.supplier.invoice.suppliershort"] + ": " + group[0].supplierNr + " - " + group[0].supplierName + " " + this.terms["economy.import.payment.invoiceseries"] + ": " + credit.invoiceSeqNr + " " + this.terms["economy.supplier.invoice.paydateshort"] + ": " + credit.payDate.toFormattedDate();
                                            else
                                                seqNrs = seqNrs + "\n" + this.terms["economy.supplier.invoice.suppliershort"] + ": " + group[0].supplierNr + " - " + group[0].supplierName + " " + this.terms["economy.import.payment.invoiceseries"] + ": " + credit.invoiceSeqNr + " " + this.terms["economy.supplier.invoice.paydateshort"] + ": " + credit.payDate.toFormattedDate();
                                            hasSingleCredits = true;
                                        }
                                    });

                                    if (hasSingleCredits) {
                                        isInvalid = true;
                                        let message = this.terms["economy.supplier.invoice.creditsinglemessage"] + "\n\n" + seqNrs + "\n\n" + this.terms["economy.supplier.invoice.credithandlemessage"];
                                        this.notificationService.showDialog(this.terms["core.warning"], message, SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK, SOEMessageBoxSize.Large);
                                        return false;
                                    }
                                }

                                if (credits.length > 5 && !ignoreMultipleCreditsValidation) {
                                    isInvalid = true;

                                    let seqNrs = undefined;
                                    _.forEach(credits, (credit) => {
                                        if (!seqNrs)
                                            seqNrs = this.terms["economy.supplier.invoice.suppliershort"] + ": " + group[0].supplierNr + " - " + group[0].supplierName + " " + this.terms["economy.import.payment.invoiceseries"] + ": " + credit.invoiceSeqNr + " " + this.terms["economy.supplier.invoice.paydateshort"] + ": " + credit.payDate.toFormattedDate();
                                        else
                                            seqNrs = seqNrs + "\n" + this.terms["economy.supplier.invoice.suppliershort"] + ": " + group[0].supplierNr + " - " + group[0].supplierName + " " + this.terms["economy.import.payment.invoiceseries"] + ": " + credit.invoiceSeqNr + " " + this.terms["economy.supplier.invoice.paydateshort"] + ": " + credit.payDate.toFormattedDate();
                                    });

                                    let message = this.terms["economy.supplier.invoice.credittoomanymessage"] + "\n\n" + seqNrs + "\n\n" + this.terms["economy.supplier.invoice.credittoomanywarning"];
                                    const modal = this.notificationService.showDialog(this.terms["core.warning"], message, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.YesNo, SOEMessageBoxSize.Large);
                                    modal.result.then(val => {
                                        if (val != null && val === true)
                                            this.executeButtonFunction(option, true);
                                    });
                                    return false;
                                }

                                const total = group.reduce((total, item) => total + item.paymentAmountCurrency, 0);
                                if (total <= 0) {
                                    isInvalid = true;

                                    let seqNrs = undefined;
                                    _.forEach(credits, (credit) => {
                                        if (!seqNrs)
                                            seqNrs = this.terms["economy.supplier.invoice.suppliershort"] + ": " + group[0].supplierNr + " - " + group[0].supplierName + " " + this.terms["economy.import.payment.invoiceseries"] + ": " + credit.invoiceSeqNr + " " + this.terms["economy.supplier.invoice.paydateshort"] + ": " + credit.payDate.toFormattedDate();
                                        else
                                            seqNrs = seqNrs + "\n" + this.terms["economy.supplier.invoice.suppliershort"] + ": " + group[0].supplierNr + " - " + group[0].supplierName + " " + this.terms["economy.import.payment.invoiceseries"] + ": " + credit.invoiceSeqNr + " " + this.terms["economy.supplier.invoice.paydateshort"] + ": " + credit.payDate.toFormattedDate();
                                    });

                                    let message = this.terms["economy.supplier.invoice.creditnegativeamountmessage"] + "\n\n" + seqNrs + "\n\n" + this.terms["economy.supplier.invoice.credithandlemessage"];
                                    this.notificationService.showDialog(this.terms["core.warning"], message, SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK, SOEMessageBoxSize.Large);
                                    return false;
                                }

                            });

                            if (isInvalid)
                                return;

                            // Check if debits in selection has matching credits
                            /*var message = undefined;
                            const selectedIds = _.map(validatedItems, (i) => i.supplierInvoiceId);
                            var groups = _.groupBy(validatedItems, 'supplierId');
                            _.forEach(groups, (group) => {
                                var credits = _.filter(group, (item) => item.billingTypeId === TermGroup_BillingType.Credit);
                                var debits = _.filter(group, (item) => item.billingTypeId === TermGroup_BillingType.Debit)

                                var creditAmount = credits.length === 0 ? 0 : (_.sum(_.map(credits, (item) => item.paymentAmountCurrency)) * -1);
                                var debitAmount = debits.length === 0 ? 0 : _.sum(_.map(debits, (item) => item.paymentAmountCurrency));
                                var supplierTotalAmount = debitAmount - creditAmount;

                                var unusedCredits = _.filter(this.items, (i) => !_.includes(selectedIds, i.supplierInvoiceId) && i.billingTypeId === TermGroup_BillingType.Credit && i.supplierId === group[0].supplierId);
                                message += "";
                                if (unusedCredits.length > 0) {
                                    _.forEach(unusedCredits, (inv) => {
                                        if ((inv.paymentAmount * -1) <= supplierTotalAmount)
                                            message += "";
                                    });
                                }
                            });

                            if (message) {

                                return;
                            }*/
                        }

                        this.originStatusChange = SoeOriginStatusChange.SupplierPayment_PaymentSuggestionToPayed;
                    }
                    else {
                        _.forEach(selectedItems, (row: SupplierPaymentGridDTO) => {
                            if ((row.status === SoeOriginStatus.Origin || row.status === SoeOriginStatus.Voucher) && row.fullyPaid === false && row.blockPayment === false && row.supplierBlockPayment === false) {
                                if (row.hasNoValidPaymentInfo) {
                                    wrongPaymentMethodCount++;
                                }
                                else {
                                    if (this.selectedPayDate != null)
                                        row.payDate = this.selectedPayDate;

                                    validatedItems.push(row);
                                }

                                if (this.hasAttestPermission && (row.attestStateId == null || row.attestStateId == 0))
                                    notAttested++;

                                if (row.timeDiscountDate != null && row.timeDiscountDate >= this.dateToday && this.selectedPayDate != null && this.selectedPayDate > row.timeDiscountDate)
                                    timeDiscountLoss++;

                                if (!this.IsPayDatesEntered(selectedItems) && this.selectedPaymentMethod.sysPaymentMethodId != TermGroup_SysPaymentMethod.SEPA) {
                                    this.notificationService.showDialog(this.terms["core.warning"], this.terms["economy.supplier.payment.needsetdate"], SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK);
                                    return;
                                }
                            }
                        });

                        validMessage += validatedItems.length > 1 ? this.terms["economy.supplier.payment.validcreatepayments"] : this.terms["economy.supplier.payment.validcreatepayment"];

                        if (this.selectedPayDate != null) {
                            validMessage += "<br\>(" + this.terms["economy.supplier.payment.samepaymentdate"] + " " + CalendarUtility.toFormattedDate(this.selectedPayDate) + ")";
                        }
                        else
                            if (!this.selectedPayDate && this.selectedPaymentMethod.sysPaymentMethodId == TermGroup_SysPaymentMethod.SEPA) {
                                validMessage += "<br\>(" + this.terms["economy.supplier.payment.duedateaspaydate"] + ")";
                            }

                        invalidMessage += (selectedItems.length - validatedItems.length) > 1 ? this.terms["economy.supplier.payment.invoicescreatepaymentinvalid"] : this.terms["economy.supplier.payment.invoicecreatepaymentinvalid"];
                        successMessage += this.terms["economy.supplier.payment.paymentcreated"];
                        errorMessage += this.terms["economy.supplier.payment.paymentcreatedfailed"];

                        // Validate ISO
                        if (this.selectedPaymentMethod.sysPaymentMethodId == TermGroup_SysPaymentMethod.ISO20022) {
                            var isInvalid = false;
                            var groups = _.groupBy(validatedItems, 'supplierId');
                            _.forEach(groups, (group) => {
                                var credits = _.filter(group, (item) => item.billingTypeId === TermGroup_BillingType.Credit);
                                var debits = _.filter(group, (item) => item.billingTypeId === TermGroup_BillingType.Debit)

                                if (!debits || debits.length === 0) {
                                    isInvalid = true;

                                    let seqNrs = undefined;
                                    _.forEach(credits, (credit) => {
                                        if (!seqNrs)
                                            seqNrs = this.terms["economy.supplier.invoice.suppliershort"] + ": " + group[0].supplierNr + " - " + group[0].supplierName + " " + this.terms["economy.import.payment.invoiceseries"] + ": " + credit.invoiceSeqNr + " " + this.terms["economy.supplier.invoice.paydateshort"] + ": " + credit.payDate.toFormattedDate();
                                        else
                                            seqNrs = seqNrs + "\n" + this.terms["economy.supplier.invoice.suppliershort"] + ": " + group[0].supplierNr + " - " + group[0].supplierName + " " + this.terms["economy.import.payment.invoiceseries"] + ": " + credit.invoiceSeqNr + " " + this.terms["economy.supplier.invoice.paydateshort"] + ": " + credit.payDate.toFormattedDate();
                                    });

                                    let message = this.terms["economy.supplier.invoice.creditnegativeamountmessage"] + "\n\n" + seqNrs + "\n\n" + this.terms["economy.supplier.invoice.credithandlemessage"];
                                    this.notificationService.showDialog(this.terms["core.warning"], message, SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK, SOEMessageBoxSize.Large);
                                    return false;
                                }

                                if (!this.selectedPayDate) {
                                    let seqNrs = undefined;
                                    let hasSingleCredits = false;
                                    _.forEach(credits, (credit) => {
                                        if (!_.find(debits, (debit) => debit.payDate && debit.payDate.toDateString() === credit.payDate.toDateString())) {
                                            if (!seqNrs)
                                                seqNrs = this.terms["economy.supplier.invoice.suppliershort"] + ": " + group[0].supplierNr + " - " + group[0].supplierName + " " + this.terms["economy.import.payment.invoiceseries"] + ": " + credit.invoiceSeqNr + " " + this.terms["economy.supplier.invoice.paydateshort"] + ": " + credit.payDate.toFormattedDate();
                                            else
                                                seqNrs = seqNrs + "\n" + this.terms["economy.supplier.invoice.suppliershort"] + ": " + group[0].supplierNr + " - " + group[0].supplierName + " " + this.terms["economy.import.payment.invoiceseries"] + ": " + credit.invoiceSeqNr + " " + this.terms["economy.supplier.invoice.paydateshort"] + ": " + credit.payDate.toFormattedDate();
                                            hasSingleCredits = true;
                                        }
                                    });

                                    if (hasSingleCredits) {
                                        isInvalid = true;
                                        let message = this.terms["economy.supplier.invoice.creditsinglemessage"] + "\n\n" + seqNrs + "\n\n" + this.terms["economy.supplier.invoice.credithandlemessage"];
                                        this.notificationService.showDialog(this.terms["core.warning"], message, SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK, SOEMessageBoxSize.Large);
                                        return false;
                                    }
                                }
                                
                                if (credits.length > 5 && !ignoreMultipleCreditsValidation) {
                                    isInvalid = true;

                                    let seqNrs = undefined;
                                    _.forEach(credits, (credit) => {
                                        if (!seqNrs)
                                            seqNrs = this.terms["economy.supplier.invoice.suppliershort"] + ": " + group[0].supplierNr + " - " + group[0].supplierName + " " + this.terms["economy.import.payment.invoiceseries"] + ": " + credit.invoiceSeqNr + " " + this.terms["economy.supplier.invoice.paydateshort"] + ": " + credit.payDate.toFormattedDate();
                                        else
                                            seqNrs = seqNrs + "\n" + this.terms["economy.supplier.invoice.suppliershort"] + ": " + group[0].supplierNr + " - " + group[0].supplierName + " " + this.terms["economy.import.payment.invoiceseries"] + ": " + credit.invoiceSeqNr + " " + this.terms["economy.supplier.invoice.paydateshort"] + ": " + credit.payDate.toFormattedDate();
                                    });

                                    let message = this.terms["economy.supplier.invoice.credittoomanymessage"] + "\n\n" + seqNrs + "\n\n" + this.terms["economy.supplier.invoice.credittoomanywarning"];
                                    const modal = this.notificationService.showDialog(this.terms["core.warning"], message, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.YesNo, SOEMessageBoxSize.Large);
                                    modal.result.then(val => {
                                        if (val != null && val === true)
                                            this.executeButtonFunction(option, true);
                                    });
                                    return false;
                                }

                                var creditAmount = (_.sum(_.map(credits, (item) => item.totalAmountCurrency - item.paidAmountCurrency)) * -1);
                                var debitAmount = _.sum(_.map(debits, (item) => item.totalAmountCurrency - item.paidAmountCurrency));

                                if ((debitAmount - creditAmount) <= 0) {
                                    isInvalid = true;

                                    let seqNrs = undefined;
                                    _.forEach(credits, (credit) => {
                                        if (!seqNrs)
                                            seqNrs = this.terms["economy.supplier.invoice.suppliershort"] + ": " + group[0].supplierNr + " - " + group[0].supplierName + " " + this.terms["economy.import.payment.invoiceseries"] + ": " + credit.invoiceSeqNr + " " + this.terms["economy.supplier.invoice.paydateshort"] + ": " + credit.payDate.toFormattedDate();
                                        else
                                            seqNrs = seqNrs + "\n" + this.terms["economy.supplier.invoice.suppliershort"] + ": " + group[0].supplierNr + " - " + group[0].supplierName + " " + this.terms["economy.import.payment.invoiceseries"] + ": " + credit.invoiceSeqNr + " " + this.terms["economy.supplier.invoice.paydateshort"] + ": " + credit.payDate.toFormattedDate();
                                    });

                                    let message = this.terms["economy.supplier.invoice.creditnegativeamountmessage"] + "\n\n" + seqNrs + "\n\n" + this.terms["economy.supplier.invoice.credithandlemessage"];
                                    this.notificationService.showDialog(this.terms["core.warning"], message, SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK, SOEMessageBoxSize.Large);
                                    return false;
                                }

                            });

                            if (isInvalid)
                                return;
                        }

                        this.originStatusChange = SoeOriginStatusChange.SupplierInvoice_OriginToPayment;
                    }
                }
                else {
                    return;
                }
                break;
            case SupplierPaymentGridButtonFunctions.DeletePaymentFile:
                _.forEach(selectedItems, (row: SupplierPaymentGridDTO) => {
                    if ((row.status === SoePaymentStatus.Pending || row.status === SoePaymentStatus.Verified || row.status === SoePaymentStatus.Error) && row.hasVoucher === false) {
                        validatedItems.push(row);
                    }
                });

                validMessage += this.terms["economy.supplier.payment.paymentcancelvalid"];
                invalidMessage += this.terms["economy.supplier.payment.paymentcancelinvalid"];
                successMessage += selectedItems.length > 1 ? this.terms["economy.supplier.payment.multicancel"] : this.terms["economy.supplier.payment.singlecancelled"];
                errorMessage += this.terms["economy.supplier.payment.cancelfailed"];

                this.originStatusChange = SoeOriginStatusChange.SupplierPayment_PayedToCancel;
                break;
            case SupplierPaymentGridButtonFunctions.DeleteSuggestion:
                _.forEach(selectedItems, (row: SupplierPaymentGridDTO) => {
                    if ((row.status === SoePaymentStatus.Pending || row.status === SoePaymentStatus.Verified || row.status === SoePaymentStatus.Error) && row.hasVoucher === false) {
                        validatedItems.push(row);
                    }
                });

                validMessage += this.terms["economy.supplier.payment.suggestioncancelvalid"];
                invalidMessage += this.terms["economy.supplier.payment.suggestioncancelinvalid"];
                successMessage += selectedItems.length > 1 ? this.terms["economy.supplier.payment.multicancel"] : this.terms["economy.supplier.payment.singlecancelled"];
                errorMessage += this.terms["economy.supplier.payment.cancelfailed"];

                this.originStatusChange = SoeOriginStatusChange.SupplierPayment_PayedToCancel;
                break;
            case SupplierPaymentGridButtonFunctions.TransferToVoucher:
                if (_.filter(selectedItems, { isModified: true }).length > 0) {
                    this.notificationService.showDialog(this.terms["core.warning"], this.terms["economy.supplier.payment.needsavechanges"], SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK);
                    return;
                }

                _.forEach(selectedItems, (row: SupplierPaymentGridDTO) => {
                    if ((row.status === SoePaymentStatus.Pending || row.status === SoePaymentStatus.Verified || row.status === SoePaymentStatus.Error) && row.hasVoucher === false) {
                        validatedItems.push(row);
                    }
                });

                validMessage += validatedItems.length > 1 ? this.terms["common.paymentsorigintovouchervalid"] : this.terms["common.paymentorigintovouchervalid"];
                invalidMessage += (selectedItems.length - validatedItems.length) > 1 ? this.terms["common.paymentsorigintovoucherinvalid"] : this.terms["common.paymentorigintovoucherinvalid"];
                successMessage += validatedItems.length > 1 ? this.terms["common.paymentstransferedtovoucher"] : this.terms["common.paymenttransferedtovoucher"];
                errorMessage += validatedItems.length > 1 ? this.terms["common.paymentstransferedtovoucherfailed"] : this.terms["common.paymenttransferedtovoucherfailed"];

                this.originStatusChange = SoeOriginStatusChange.SupplierPayment_PayedToVoucher;
                break;
            case SupplierPaymentGridButtonFunctions.ChangeDateVoucher:
                _.forEach(selectedItems, (row: SupplierPaymentGridDTO) => {
                    if ((row.status === SoePaymentStatus.Pending || row.status === SoePaymentStatus.Verified || row.status === SoePaymentStatus.Error) && row.hasVoucher === false) {
                        validatedItems.push(row);
                    }
                });

                validMessage += validatedItems.length > 1 ? this.terms["economy.supplier.payment.paymentschangedatetovouchervalid"] : this.terms["economy.supplier.payment.paymentchangedatetovouchervalid"];
                invalidMessage += (selectedItems.length - validatedItems.length) > 1 ? this.terms["common.paymentsorigintovoucherinvalid"] : this.terms["common.paymentorigintovoucherinvalid"];
                successMessage += validatedItems.length > 1 ? this.terms["economy.supplier.payment.paymentsdatetovocher"] : this.terms["economy.supplier.payment.paymentdatetotvoucher"];
                errorMessage += validatedItems.length > 1 ? this.terms["economy.supplier.payment.paymentsdatetotvoucher"] : this.terms["economy.supplier.payment.paymentdatetotvoucherfailed"];

                this.originStatusChange = SoeOriginStatusChange.SupplierPayment_ChangePayDateToVoucher;
                break;
            case SupplierPaymentGridButtonFunctions.ChangePayDate:
                _.forEach(selectedItems, (row: SupplierPaymentGridDTO) => {
                    if ((row.status === SoePaymentStatus.Pending || row.status === SoePaymentStatus.Verified || row.status === SoePaymentStatus.Error) && row.hasVoucher === false) {
                        validatedItems.push(row);
                    }
                });

                validMessage += validatedItems.length > 1 ? this.terms["economy.supplier.payment.paymentschangedatetovouchervalid"] : this.terms["economy.supplier.payment.paymentchangedatevalid"];
                successMessage += validatedItems.length > 1 ? this.terms["common.savedmulti"] : this.terms["common.savedsingle"];
                errorMessage += this.terms["common.savefailed"];

                this.originStatusChange = SoeOriginStatusChange.SupplierPayment_ChangePayDate;

                break;
            case SupplierPaymentGridButtonFunctions.SaveChanges:

                _.forEach(selectedItems, (row: SupplierPaymentGridDTO) => {
                    if ((row.status === SoePaymentStatus.Pending || row.status === SoePaymentStatus.Verified || row.status === SoePaymentStatus.Error) && row.hasVoucher === false && (row.isModified == true || this.selectedPayDate != null)) {
                        if (this.selectedPayDate && !row['payDateModified']) {
                            row.payDate = this.selectedPayDate;
                            row.isModified = true;
                        }
                        validatedItems.push(row);
                    }
                });

                validMessage += validatedItems.length > 1 ? this.terms["economy.supplier.payment.paymentssavevalid"] : this.terms["economy.supplier.payment.paymentsavevalid"];
                invalidMessage += (selectedItems.length - validatedItems.length) > 1 ? this.terms["economy.supplier.payment.paymentssaveinvalid"] : this.terms["economy.supplier.payment.paymentsaveinvalid"];
                successMessage += validatedItems.length > 1 ? this.terms["common.savedmulti"] : this.terms["common.savedsingle"];
                errorMessage += this.terms["common.savefailed"];

                this.originStatusChange = SoeOriginStatusChange.SupplierPayment_PayedEditDateAndAmount;
                break;
            case SupplierPaymentGridButtonFunctions.SaveChangesTransferToVoucher:
                _.forEach(selectedItems, (row: SupplierPaymentGridDTO) => {
                    if ((row.status === SoePaymentStatus.Pending || row.status === SoePaymentStatus.Verified || row.status === SoePaymentStatus.Error) && row.hasVoucher === false) {
                        if (this.selectedPayDate && !row['payDateModified']) {
                            row.payDate = this.selectedPayDate;
                            row.isModified = true;
                        }

                        validatedItems.push(row);
                    }
                });

                validMessage += validatedItems.length > 1 ? this.terms["economy.supplier.payment.paymentssavetovoucher"] : this.terms["economy.supplier.payment.paymentsavetovoucher"];
                invalidMessage += (selectedItems.length - validatedItems.length) > 1 ? this.terms["common.paymentsorigintovoucherinvalid"] : this.terms["common.paymentorigintovoucherinvalid"];
                successMessage += validatedItems.length > 1 ? this.terms["economy.supplier.payment.paymentssavedtovoucher"] : this.terms["economy.supplier.payment.paymentsavedtovoucher"];
                errorMessage += validatedItems.length > 1 ? this.terms["economy.supplier.payment.paymentssavedtovoucherfailed"] : this.terms["economy.supplier.payment.paymentsavedtovoucherfailed"];

                this.originStatusChange = SoeOriginStatusChange.SupplierPayment_PayedEditDateAndAmountToVoucher;
                break;
        }

        var noOfValid: number = validatedItems.length;
        var noOfInvalid = selectedItems.length - validatedItems.length;

        //Validate payment methods
        if (wrongPaymentMethodCount > 0) {
            //if (wrongPaymentMethodCount === 1)
            invalidMessage += this.terms["economy.supplier.payment.supplierwrongpaymentmethod"];
        }

        //Validate attest
        if (notAttested > 0) {
            
            if (noOfInvalid == 0 && wrongPaymentMethodCount == 0)
                invalidMessage = notAttested.toString() + " ";
            else
                invalidMessage += "<br\>" + notAttested.toString() + " ";

            if (notAttested == 1)
                invalidMessage += this.terms["economy.supplier.payment.validinvoice"] + " " + this.terms["economy.supplier.payment.missingattest"];
            else
                invalidMessage += this.terms["economy.supplier.payment.validinvoices"] + " " + this.terms["economy.supplier.payment.missingattest"];

        }

        //Validate time discount
        if (timeDiscountLoss > 0) {
            invalidMessage += "<br\>" + this.terms["economy.supplier.payment.timediscountloss"];
        }

        // Items to transfer
        var itemsToTransfer: SupplierPaymentGridDTO[] = validatedItems;

        var title: string = "";
        var text: string = "";
        var doTransfer: boolean = false;
        var yesButtonText: string = "";
        var noButtonText: string = "";
        var cancelButtonText: string = "";
        var image: SOEMessageBoxImage = SOEMessageBoxImage.None;
        var buttons: SOEMessageBoxButtons = SOEMessageBoxButtons.None;
        if ((selectedItems.length === validatedItems.length || selectedItems.length < validatedItems.length) && notAttested === 0) {
            title = this.terms["core.verifyquestion"];

            text = "";
            text += noOfValid.toString() + " " + validMessage + "<br\>";
            text += this.terms["core.continue"];

            image = SOEMessageBoxImage.Question;
            buttons = SOEMessageBoxButtons.OKCancel;

            doTransfer = true;
        }
        else if (selectedItems.length > validatedItems.length || notAttested != 0) {
            if (noOfValid === 0) {
                title = this.terms["core.warning"];

                text = "";
                text += selectedItems.length.toString() + " " + invalidMessage + "<br\>";

                image = SOEMessageBoxImage.Warning;
                buttons = SOEMessageBoxButtons.OK;

                doTransfer = false;
            }
            else {
                title = this.terms["core.verifyquestion"];

                text = "";
                text += invalidMessage + "<br\>";
                text += noOfValid.toString() + " " + validMessage + "<br\>";
                text += this.terms["core.continue"];

                image = SOEMessageBoxImage.Question;
                buttons = SOEMessageBoxButtons.OKCancel;

                doTransfer = true;
            }
        }
        console.log("transfer")

        const modal = this.notificationService.showDialog(title, text, image, buttons);
        modal.result.then(val => {
            const sendPaymentFile = option.id === SupplierPaymentGridButtonFunctions.SendPaymentFile;
            if (val != null && val === true && doTransfer === true) {
                this.onTransfer(itemsToTransfer, sendPaymentFile);
            }
        });

        this.ignoreDateValidation = false;
        this.selectedItemsCount = 0;
    }

    private onTransfer(items, sendPaymentFile: boolean) {
        if (this.usesSignatoryContractForSendingPayments && sendPaymentFile) {
            const modal = this.modalInstance.open({
                templateUrl: this.urlHelperService.getGlobalUrl("Common/Dialogs/SignatoryContractAuthentication/SignatoryContractAuthentication.html"),
                controller: SignatoryContractAuthenticationController,
                controllerAs: 'ctrl',
                backdrop: 'static',
                size: 'sm',
                resolve: {
                    coreService: () => this.coreService,
                    progressService: () => this.progress,
                    permissionType: () => TermGroup_SignatoryContractPermissionType.AccountsPayable_SendPaymentToBank,
                }
            });
            modal.result.then((result) => {
                if (result.success === true) {
                    this.transfer(items, sendPaymentFile);
                }
            })
        } else {
            this.transfer(items, sendPaymentFile) 
        }
    }

    private transfer(itemsToTransfer: SupplierPaymentGridDTO[], sendPaymentFile: boolean) {

        this.progress.startSaveProgress((completion) => {

            this.supplierService.transferSupplierPayments(itemsToTransfer, soeConfig.accountYearId, this.originStatusChange, this.selectedPaymentMethod.paymentMethodId, sendPaymentFile,  this.selectedPayDate).then((result) => {
                if (result.success) {
                    if (result.integerValue2 && result.stringValue) {
                        //PaymentExportId to be loaded
                        if (!sendPaymentFile) {
                            this.doDownload(result.stringValue, result.integerValue2);
                        }
                        //print report from payments     
                        if (this.supplierChecklistPaymentReportId && this.supplierChecklistPaymentReportId > 0) {
                            var ids = [];
                            _.forEach(itemsToTransfer, (row) => {
                                ids.push(row.supplierInvoiceId);
                            });
                            var prIds = [];
                            _.forEach(result.intDict, (kvp) => {
                                prIds.push(kvp);
                            });
                            this.printExportedPayments(ids, prIds);
                        }
                    }

                    if (result.idDict) {
                        //Vouchers
                        if (this.showPrintVoucher) {
                            // Get keys
                            var voucherIds: number[] = []
                            _.forEach(Object.keys(result.idDict), (key) => {
                                voucherIds.push(Number(key));
                            });

                            // Get values
                            var first: boolean = true;
                            var voucherNrs: string = "";
                            _.forEach(result.idDict, (pair) => {
                                if (!first)
                                    voucherNrs = voucherNrs + ", ";
                                else
                                    first = false;
                                voucherNrs = voucherNrs + pair;
                            });

                            var message = this.terms["economy.supplier.payment.voucherscreated"] + "<br/>" + voucherNrs + "<br/>" + this.terms["economy.supplier.payment.askPrintVoucher"];
                            var modal = this.notificationService.showDialog(this.terms["core.verifyquestion"], message, SOEMessageBoxImage.Question, SOEMessageBoxButtons.OKCancel);
                            modal.result.then(val => {
                                if (val != null && val === true) {
                                    this.printVouchers(voucherIds);
                                }
                            });
                        }
                    }

                    completion.completed(null, null, true);
                }
                else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, null)
            .then(data => {
                this.loadGridData();
            }, error => {
                console.log("transfer error", error);
            });
    }

    private ValidateDates(toSuggestion: boolean, buttonOption: any) {
        var message = "";
        var showMessageBox = false;

        if (!this.IsPayDatesWithinCurrentAccountYear()) {
            message += this.terms["economy.supplier.payment.wrongyear"];
            message += "<br\>";
            showMessageBox = true;
        }
        if (this.supplierPaymentTransferToVoucher === true && !toSuggestion) {
            message += this.terms["economy.supplier.payment.transfertovoucher"];
            message += "<br\>";
            showMessageBox = true;
        }
        if (this.supplierUseTimeDiscount === true && this.supplierUnderPayAccount == null) {
            this.notificationService.showDialog(this.terms["core.warning"], this.terms["economy.supplier.payment.underpayaccountmissing"], SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK);
            return;
        }
        var valid = false;
        if (showMessageBox == true) {
            var modal = this.notificationService.showDialog(this.terms["core.warning"], message, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
            modal.result.then(val => {
                this.ignoreDateValidation = val;
                this.executeButtonFunction(buttonOption);
            });
        }

        return !showMessageBox;
    }

    private summarize(x) {
        this.filteredTotal = 0;
        this.filteredTotalIncVat = 0;
        this.filteredTotalExVat = 0;
        this.filteredPaid = 0;
        this.filteredToPay = 0;
        _.forEach(x, (y: any) => {
            this.filteredTotalIncVat += NumberUtility.parseDecimal(y.totalAmount);
            this.filteredTotalExVat += NumberUtility.parseDecimal(y.totalAmountExVat);
            this.filteredToPay += y.payAmount ? NumberUtility.parseDecimal(y.payAmount) : 0;
            this.filteredPaid += y.paymentAmount ? NumberUtility.parseDecimal(y.paymentAmount) : 0;
        });
        if (this.showVatFree)
            this.filteredTotal = this.filteredTotalIncVat;
        else
            this.filteredTotal = this.filteredTotalExVat;
    }

    private summarizeFiltered(rows: SupplierPaymentGridDTO[]) {
        this.filteredTotal = 0;
        this.filteredTotalIncVat = 0;
        this.filteredTotalExVat = 0;
        this.filteredPaid = 0;
        this.filteredToPay = 0;
        _.forEach(rows, (y: SupplierPaymentGridDTO) => {
            this.filteredTotalIncVat += y.totalAmount;
            this.filteredTotalExVat += y.totalAmountExVat;
            this.filteredToPay += this.classification === SoeOriginStatusClassification.SupplierPaymentSuggestions ? (y.paymentAmount ? y.paymentAmount : 0) : (y.payAmount ? y.payAmount : 0);
            this.filteredPaid += y.paymentAmount ? y.paymentAmount : 0;
        });
        if (this.showVatFree)
            this.filteredTotal = this.filteredTotalIncVat;
        else
            this.filteredTotal = this.filteredTotalExVat;
    }

    private summarizeSelected() {

        this.selectedTotal = 0;
        this.selectedTotalIncVat = 0;
        this.selectedTotalExVat = 0;
        this.selectedItemsCount = 0;
        this.selectedPaid = 0;
        this.selectedToPay = 0;

        let rows: SupplierPaymentGridDTO[] = this.gridAg.options.getSelectedRows();
        
        for (var i=0; i < rows.length; i++) {
            let y = rows[i];
            this.selectedTotalIncVat += y.totalAmount;
            this.selectedTotalExVat += y.totalAmountExVat;
            this.selectedItemsCount += 1;
            this.selectedToPay += this.classification === SoeOriginStatusClassification.SupplierPaymentSuggestions ? (y.paymentAmount ? y.paymentAmount : 0) : (y.payAmount ? y.payAmount : 0);
            this.selectedPaid += y.paymentAmount ? y.paymentAmount : 0;
        }

        if (this.showVatFree)
            this.selectedTotal = this.selectedTotalIncVat;
        else
            this.selectedTotal = this.selectedTotalExVat;

        this.$scope.$applyAsync();
    }

    private showVatFreeChanged() {
        this.$timeout(() => {
        if (this.showVatFree) {
            this.filteredTotal = this.filteredTotalIncVat;
            this.selectedTotal = this.selectedTotalIncVat;
        } else {
            this.filteredTotal = this.filteredTotalExVat;
            this.selectedTotal = this.selectedTotalExVat;
            }
        });
    }

    private hideAutogiroChanged() {
        this.$timeout(() => {
            this.filterAutogiroInvoices();
        });
    }

    private IsPayDatesEntered(selectedItems: any[]): boolean {
        let result = true;
        if (this.selectedPayDate != null) {
            result = true;
        }
        else {
            _.forEach(selectedItems, (row) => {
                if (row.payDate === null) {
                    result = false;
                }
            });
        }

        return result;
    }

    private IsPayDatesWithinCurrentAccountYear() {
        if (this.selectedPayDate != null) {
            return this.selectedPayDate >= this.currentAccountYearFromDate && this.selectedPayDate <= this.currentAccountYearToDate;
        }
        else {
            _.forEach(this.gridAg.options.getSelectedRows(), (row) => {
                if (row.payDate != null && (row.payDate < this.currentAccountYearFromDate || row.payDate > this.currentAccountYearToDate)) {
                    return false;
                }
            });
        }

        return true;
    }

    private setRowPaymentMethod(item: any, selectBestMatch?: boolean) {
        if (this.actorsPaymentInformation === null || !this.showPaymentInformation) {
            return;
        }

        const validPaymentInformations = _.filter(this.actorsPaymentInformation[item.supplierId], { billingType: item.billingTypeId });

        //Set row infos
        item.validPaymentInformations = [];
        _.forEach(validPaymentInformations, (info) => {
            item.validPaymentInformations.push(info);
        });

        if (validPaymentInformations === null || validPaymentInformations.length === 0) {
            item.paymentInformationRowId = null;
            item.paymentNr = "";
            item.paymentNrString = "";
            item.hasNoValidPaymentInfo = true;
        } else {
            if (this.classification === SoeOriginStatusClassification.SupplierPaymentSuggestions)
                item.isSelectDisabled = false;

            var paymentInfoRow;
            if (item.paymentNr != null && item.paymentNr != "") {
                paymentInfoRow = _.find(validPaymentInformations, (p) => p.paymentNr === item.paymentNr && p.sysPaymentTypeId === item.sysPaymentTypeId);
                if(!paymentInfoRow)
                    paymentInfoRow = _.find(validPaymentInformations, { paymentNr: item.paymentNr });
            }

            if (!paymentInfoRow) {
                paymentInfoRow = _.find(validPaymentInformations, { default: true });
            }
            
            if (paymentInfoRow) {
                item.paymentInformationRowId = paymentInfoRow.paymentInformationRowId;
                item.sysPaymentTypeId = paymentInfoRow.sysPaymentTypeId;
                item.paymentNr = paymentInfoRow.paymentNr;

                const paymentType = _.find(this.sysPaymentTypes, (t) => t.id === item.sysPaymentTypeId);
                item.paymentNrString = paymentInfoRow.paymentNrDisplay ? paymentType.name + " " + paymentInfoRow.paymentNrDisplay : paymentType.name + " " + paymentInfoRow.paymentNr;

                item.hasNoValidPaymentInfo = false;

            }
            else {
                item.paymentInformationRowId = null;
                item.paymentNr = "";
                item.paymentNrString = "";
                item.hasNoValidPaymentInfo = true;
            }

        }
    }

    private printSelectedInvoices() {
        if (this.supplierBalanceListReportId) {
            const ids = [];
            _.forEach(this.gridAg.options.getSelectedRows(), (row) => {
                ids.push(row.supplierInvoiceId);
            });

            const reportItem = new BalanceListPrintDTO(ids);
            reportItem.companySettingType = CompanySettingType.SupplierDefaultBalanceList;

            this.isSupplierBalanceListPrinting = true;
            this.requestReportService.printSupplierBalanceList(reportItem)
            .then(() => {
                this.isSupplierBalanceListPrinting = false;
            });

        }
        else {
            this.notificationService.showDialog(this.terms["core.warning"], this.terms["common.reportsettingmissing"], SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK);
        }
    }

    private printSuggestion() {
        if (this.supplierPaymentSuggestionReportId) {
            const ids: number[] = [];
            _.forEach(this.items, (row) => {
                ids.push(row.supplierInvoiceId);
            });

            const reportItem = new BalanceListPrintDTO(ids);
            reportItem.companySettingType = CompanySettingType.SupplierDefaultPaymentSuggestionList;

            this.isSupplierPaymentSuggestionReportPrinting = true;
            this.requestReportService.printSupplierBalanceList(reportItem)
            .then(() => {
                this.isSupplierPaymentSuggestionReportPrinting = false;
            });

        }
        else {
            this.notificationService.showDialog(this.terms["core.warning"], this.terms["common.reportsettingmissing"], SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK);
        }
    }

    private printExportedPayments(ids: number[], prIds: number[]) {
        if (this.supplierChecklistPaymentReportId) {
            const reportItem = new BalanceListPrintDTO(ids);
            reportItem.companySettingType = CompanySettingType.SupplierDefaultChecklistPayments;
            reportItem.paymentRowIds = prIds;
            this.requestReportService.printSupplierBalanceList(reportItem);
        }
        else {
            this.notificationService.showDialog(this.terms["core.warning"], this.terms["common.reportsettingmissing"], SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK);
        }
    }

    private printVouchers(ids: number[]) {
        if (this.supplierPaymentVoucherReportId) {

            this.requestReportService.printVoucherList(ids);

        }
        else {
            //Show messagebox
            this.notificationService.showDialog(this.terms["core.warning"], this.terms["economy.supplier.payment.defaultVoucherListMissing"], SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK);
        }
    }

    private doDownload(guid: string, exportId: number) {
        guid = guid.substring(guid.lastIndexOf("_") + 1);

        let url = window.location.protocol + "//" + window.location.host;
        url = url + "/soe/economy/export/payments/default.aspx" + "?c=" + CoreUtility.actorCompanyId + "&r=" + CoreUtility.roleId + "&type=" + this.selectedPaymentMethod.paymentMethodId + "&exportfile=" + guid + "&paymentExportId=" + exportId;        

        HtmlUtility.downloadUrl(url, "exportfile");
    }
    
}