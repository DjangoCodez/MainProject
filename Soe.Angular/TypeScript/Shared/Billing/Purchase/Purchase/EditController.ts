import { IControllerFlowHandlerFactory } from "../../../../Core/Handlers/controllerflowhandlerfactory";
import { IDirtyHandlerFactory } from "../../../../Core/Handlers/DirtyHandlerFactory";
import { IMessagingHandlerFactory } from "../../../../Core/Handlers/messaginghandlerfactory";
import { IProgressHandlerFactory } from "../../../../Core/Handlers/progresshandlerfactory";
import { IValidationSummaryHandlerFactory } from "../../../../Core/Handlers/validationsummaryhandlerfactory";
import { ICompositionEditController } from "../../../../Core/ICompositionEditController";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { EditControllerBase2 } from "../../../../Core/Controllers/EditControllerBase2";
import { CompanySettingType, EmailTemplateType, Feature, PurchaseRowType, SoeEntityState, SoeOriginStatus, SoeReportTemplateType, TermGroup, TermGroup_InvoiceVatType, TermGroup_SysContactAddressRowType, TermGroup_SysContactAddressType } from "../../../../Util/CommonEnumerations";
import { IPermissionRetrievalResponse } from "../../../../Core/Handlers/ControllerFlowHandler";
import { PurchaseDTO, PurchaseRowDTO } from "../../../../Common/Models/PurchaseDTO";
import { IToolbarFactory } from "../../../../Core/Handlers/ToolbarFactory";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IconLibrary, PurchaseEditPrintFunctions, PurchaseEditSaveFunctions, SOEMessageBoxButton, SOEMessageBoxButtons, SOEMessageBoxImage } from "../../../../Util/Enumerations";
import { ISupplierService } from "../../../Economy/Supplier/SupplierService";
import { SupplierHelper } from "../Helpers/SupplierHelper";
import { ISmallGenericType, IActionResult } from "../../../../Scripts/TypeLite.Net4";
import { CoreUtility } from "../../../../Util/CoreUtility";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { ICommonCustomerService } from "../../../../Common/Customer/CommonCustomerService";
import { ToolBarButton, ToolBarUtility } from "../../../../Util/ToolBarUtility";
import { ContactAddressDTO } from "../../../../Common/Models/ContactAddressDTOs";
import { SettingsUtility } from "../../../../Util/SettingsUtility";
import { SupplierDTO } from "../../../../Common/Models/supplierdto";
import { OriginUserHelper } from "../../Helpers/OriginUserHelper";
import { CalendarUtility } from "../../../../Util/CalendarUtility";
import { ProjectHelper } from "../../Helpers/ProjectHelper";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { SelectCustomerInvoiceHelper } from "../../Helpers/SelectCustomerInvoiceHelper";
import { PurchaseAmountHelper } from "../Helpers/PurchaseAmountHelper";
import { IPurchaseService } from "./PurchaseService";
import { SelectReportController } from "../../../../Common/Dialogs/SelectReport/SelectReportController";
import { SelectEmailController } from "../../../../Common/Dialogs/SelectEmail/SelectEmailController";
import { IReportService } from "../../../../Core/Services/ReportService";
import { HtmlUtility } from "../../../../Util/HtmlUtility";
import { SetPurchaseDateController } from "../../Dialogs/SetPurchaseDate/SetPurchaseDateController";
import { PurchaseDeliveryRowDTO } from "../../../../Common/Models/PurchaseDeliveryDTO";
import { EditDeliveryAddressController } from "../../Dialogs/EditDeliveryAddress/EditDeliveryAddressController";
import { DeliveryAddressesPurchaseController } from "../../Dialogs/DeliveryAddressesPurchase/DeliveryAddressesPurchase";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { Constants } from "../../../../Util/Constants";
import { IShortCutService } from "../../../../Core/Services/ShortCutService";
import { StringUtility } from "../../../../Util/StringUtility";
import { IProgressHandler } from "../../../../Core/Handlers/ProgressHandler";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    private purchaseId: number;
    private purchase: PurchaseDTO;
    private purchaseRows: PurchaseRowDTO[] = [];
    private purchaseDelivaryRows: PurchaseDeliveryRowDTO[] = [];

    private originalPurchase: PurchaseDTO;
    private originalPurchaseRows: PurchaseRowDTO[] = [];

    private terms: any;

    private headExpanderLabel: string;

    //Permissions
    private useCurrency: boolean = false;
    private tracingPermission: boolean = false;
    private deliveryPermission: boolean = false;
    private orderRowsPermission: boolean = false;

    //settings
    private defaultVatType: TermGroup_InvoiceVatType = TermGroup_InvoiceVatType.Merchandise;
    private defaultDeliveryTypeId: number;
    private defaultDeliveryConditionId: number;
    private defaultReportId: number;
    private useCentRounding: boolean;
    private defaultEmailTemplatePurchase: number;

    private showNavigationButtons = true;

    private saveFunctions: any = [];
    private printFunctions: any = [];

    private statusTypes: ISmallGenericType[];
    // private allowedStatusTypes: SmallGenericType[] = [];
    private ourReferences: ISmallGenericType[];
    private deliveryTypes: ISmallGenericType[] = [];
    private deliveryConditions: ISmallGenericType[] = [];
    private deliveryAddresses: ContactAddressDTO[];

    // Flags
    private traceRowsRendered: boolean = false;
    private deliveryRowsRendered: boolean = false;
    private customerInvoiceRowsRendered: boolean = false;
    private isLocked: boolean = false;
    private purchaseRowsExpanderIsOpen = false;

    private purchaseIds: number[];

    private oldStatus;
    private currentStatusOption: any;
    private statusFunctions: any = [];
    private _allowedStatusFunctions: any = [];
    get allowedStatusFunctions() {
        if (this.oldStatus === this.purchase?.originStatus) {
            return this._allowedStatusFunctions;
        } else {
            this.oldStatus = this.purchase?.originStatus;
        }

        const current = this.purchase.originStatus;
        const currentFunc = this.statusFunctions.find(s => s.id === current);
        const idx = this.statusFunctions.findIndex(t => t.id === current);
        this._allowedStatusFunctions = [];

        this._allowedStatusFunctions.push(currentFunc);
        for (let i = 0; i < this.statusFunctions.length; i++) {
            if (i === idx - 1 || i === idx + 1)
                this._allowedStatusFunctions.push(this.statusFunctions[i]);
        }
        return this._allowedStatusFunctions;
    }

    private _selectedPurchaseDate: Date;
    get selectedPurchaseDate() {
        return this._selectedPurchaseDate;
    }
    set selectedPurchaseDate(date: Date) {
        this._selectedPurchaseDate = date ? new Date(date.toString() as any) : null;

        if (this.purchase) {
            this.purchase.purchaseDate = this.amountHelper.currencyDate = this.selectedPurchaseDate;
        }
    }

    get selectedWantedDeliveryDate() {
        return this.purchase ? this.purchase.wantedDeliveryDate : undefined;
    }
    set selectedWantedDeliveryDate(date: Date) {
        if (this.purchase && date && this.purchase.wantedDeliveryDate !== date) {
            this.purchase.wantedDeliveryDate = date;

            if (this.purchaseRowsRendered) {
                this.$scope.$applyAsync(() => {
                    _.forEach(this.purchaseRows, (r) => {
                        r.wantedDeliveryDate = date;
                        r.isModified = true;
                    });
                    this.$scope.$broadcast('refreshRows');
                });
            }
            else {
                this.openPurchaseRowExpander(null, true);
            }

            this.dirtyHandler.setDirty();
        }
    }

    get disablePrint() {
        return !this.purchase || !this.purchase.purchaseId || this.dirtyHandler.isDirty || this.purchase.originStatus === SoeOriginStatus.Origin;
    }

    //Gui
    private purchaseRowsRendered = false;
    get purchaseRowsExpanderLabel(): string {

        if (!this.purchaseRowsRendered) return "";
        const nbrOfRows = this.purchaseRows ? this.purchaseRows.length : undefined;
        return "({0}) | {1}: {2}".format(
            nbrOfRows.toString(),
            this.terms["billing.productrows.totalamount"],
            this.amountFilter(this.purchase.totalAmountExVatCurrency),
        )
    }

    //helpers
    private amountHelper: PurchaseAmountHelper;
    private supplierHelper: SupplierHelper;
    private originUserHelper: OriginUserHelper;
    private projectHelper: ProjectHelper;
    private orderSelectHelper: SelectCustomerInvoiceHelper;

    // Functions
    private modalInstance: any;

    // Filters
    private amountFilter: any;

    private ordId: number = 0;

    //@ngInject
    constructor(
        shortCutService: IShortCutService,
        private $uibModal,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private $window: ng.IWindowService,
        private $timeout: ng.ITimeoutService,
        $filter: ng.IFilterService,
        private purchaseService: IPurchaseService,
        private supplierService: ISupplierService,
        private commonCustomerService: ICommonCustomerService,
        private coreService: ICoreService,
        private reportService: IReportService,
        private urlHelperService: IUrlHelperService,
        private notificationService: INotificationService,
        private translationService: ITranslationService,
        private messagingService: IMessagingService,
        progressHandlerFactory: IProgressHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory,) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.modalInstance = $uibModal;
        this.amountFilter = $filter("amount");

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onLoadData(() => this.loadData())
            .onDoLookUp(() => this.onDoLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));

        this.amountHelper = new PurchaseAmountHelper(coreService, $timeout, $q, () => this.currencyChanged());
        this.supplierHelper = new SupplierHelper(this, coreService, translationService, this.supplierService, urlHelperService, $q, $scope, $uibModal, (supplier: SupplierDTO) => { this.initSupplierChanged(supplier); });
        this.originUserHelper = new OriginUserHelper(this, coreService, urlHelperService, translationService, $q, $uibModal);
        this.projectHelper = new ProjectHelper(this, null, urlHelperService, translationService, notificationService, $q, $window, $uibModal,
            (projectId, projectNumber) => { this.projectChanged(projectId, projectNumber) })
        this.orderSelectHelper = new SelectCustomerInvoiceHelper(this, null, urlHelperService, translationService, notificationService, $q, $uibModal,
            (result) => this.customerInvoiceChanged(result),
            () => { return { projectName: this.purchase.projectNr, projectId: this.purchase.projectId } }
        )

        shortCutService.bindSaveAndClose($scope, () => { this.save(true); });
        shortCutService.bindSave($scope, () => { this.save(false); });

        this.projectHelper.showAllProjects = true;

    }

    public onInit(parameters: any) {
        this.purchaseId = parameters.id;
        this.guid = parameters.guid;

        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);
        if (parameters.ids && parameters.ids.length > 0) {
            this.purchaseIds = parameters.ids;
        }
        else {
            this.showNavigationButtons = false;
        }

        this.flowHandler.start([
            { feature: Feature.Billing_Purchase_Purchase_Edit, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Economy_Supplier_Suppliers_Edit, loadReadPermissions: false, loadModifyPermissions: true },
            { feature: Feature.Economy_Preferences_Currency, loadReadPermissions: false, loadModifyPermissions: true },
            { feature: Feature.Billing_Purchase_Purchase_Edit_TraceRows, loadReadPermissions: false, loadModifyPermissions: true },
            { feature: Feature.Billing_Purchase_Delivery_List, loadReadPermissions: false, loadModifyPermissions: true },
            { feature: Feature.Billing_Order_Orders_Edit_ProductRows, loadReadPermissions: false, loadModifyPermissions: true },
        ]);

        // Events - Quarantine
        this.messagingService.subscribe(Constants.EVENT_RELOAD_ROWS, (guid) => {
            if (this.guid === guid && this.purchaseRowsRendered) 
                this.loadPurchaseRows(null);
        }, this.$scope);
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {

        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => this.copy(), () => this.isNew);

        const statusGroup = ToolBarUtility.createGroup();

        statusGroup.buttons.push(new ToolBarButton("", "common.customer.invoices.reloadorder", IconLibrary.FontAwesome, "fa-sync", () => {
            this.loadData();
            if (this.purchaseRowsRendered)
                this.loadPurchaseRows(null);
            if (this.deliveryRowsRendered) 
                this.loadDeliveryRows();
        }, null, () => {
            return false;
        }));
        this.toolbar.addButtonGroup(statusGroup);

        //Navigation
        this.toolbar.setupNavigationGroup(() => { return this.isNew }, null, (newPurchaseId) => {
            this.purchaseId = newPurchaseId;
            this.loadData(true);
            if (this.purchaseRowsRendered) {
                this.loadPurchaseRows(null);
            }
            if (this.deliveryRowsRendered) {
                this.loadDeliveryRows();
            }
        }, this.purchaseIds, this.purchaseId);

        this.setupFunctions();

    }

    private setupFunctions() {
        // Functions
        const keys: string[] = [
            "core.save",
            "core.saveandclose",
            "common.report.report.print",
            "common.email",
            "common.report.report.reports",
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.saveFunctions = [
                ({ id: PurchaseEditSaveFunctions.Save, name: terms["core.save"] + " (Ctrl+S)", icon: 'fal fa-fw fa-save' }),
                ({ id: PurchaseEditSaveFunctions.SaveAndClose, name: terms["core.saveandclose"] + " (Ctrl+Enter)", icon: 'fal fa-fw fa-save' }),
            ];

            this.printFunctions = [
                ({ id: PurchaseEditPrintFunctions.Print, name: terms["common.report.report.print"], icon: 'fal fa-fw fa-print' }),
                ({ id: PurchaseEditPrintFunctions.eMail, name: terms["common.email"], icon: 'fal fa-fw fa-envelope' }),
                ({ id: PurchaseEditPrintFunctions.ReportDialog, name: terms["common.report.report.reports"], icon: 'fal fa-fw fa-print' }),
            ];
        });
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Billing_Purchase_Purchase_Edit].readPermission;
        this.modifyPermission = response[Feature.Billing_Purchase_Purchase_Edit].modifyPermission;
        this.useCurrency = response[Feature.Economy_Preferences_Currency].modifyPermission;
        this.tracingPermission = response[Feature.Billing_Purchase_Purchase_Edit_TraceRows].modifyPermission;
        this.deliveryPermission = response[Feature.Billing_Purchase_Delivery_List].modifyPermission;
        this.orderRowsPermission = response[Feature.Billing_Order_Orders_Edit_ProductRows].modifyPermission;
    }


    private onDoLookups(): ng.IPromise<any> {
        const promise = this.$q.all([
            this.loadTerms(),
            this.supplierHelper.loadSuppliers(true),
            this.loadOurReferences(),
            this.loadDeliveryTypes(),
            this.loadDeliveryConditions(),
            this.supplierHelper.loadPaymentConditions(),
            this.amountHelper.loadCurrencies(),
            this.loadCompanySettings(),
            this.loadStatusTypes(),
            this.loadDeliveryAddresses(CoreUtility.actorCompanyId),
        ]);
        return this.progress.startLoadingProgress([() => promise]);
    }

    private loadCompanySettings(): ng.IPromise<any> {
        const settingTypes: number[] = [CompanySettingType.CustomerInvoiceDefaultVatType,
            CompanySettingType.BillingDefaultDeliveryType,
            CompanySettingType.BillingDefaultDeliveryCondition,
            CompanySettingType.BillingDefaultPurchaseOrderReportTemplate,
            CompanySettingType.BillingUseCentRounding,
            CompanySettingType.BillingDefaultEmailTemplatePurchase
        ];

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.defaultVatType = SettingsUtility.getIntCompanySetting(x, CompanySettingType.CustomerInvoiceDefaultVatType, this.defaultVatType);
            this.defaultDeliveryTypeId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingDefaultDeliveryType);
            this.defaultDeliveryConditionId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingDefaultDeliveryCondition);
            this.defaultReportId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingDefaultPurchaseOrderReportTemplate);
            this.useCentRounding = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingUseCentRounding);
            this.defaultEmailTemplatePurchase = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingDefaultEmailTemplatePurchase);
        });
    }

    private loadOurReferences(): ng.IPromise<any> {
        return this.coreService.getUsersDict(true, false, true, false).then(x => {
            this.ourReferences = x;
        });
    }

    private loadDeliveryTypes(): ng.IPromise<any> {
        return this.commonCustomerService.getDeliveryTypesDict(true).then(x => {
            this.deliveryTypes = x;
        });
    }

    private loadDeliveryConditions(): ng.IPromise<any> {
        return this.commonCustomerService.getDeliveryConditionsDict(true).then(x => {
            this.deliveryConditions = x;
        });
    }

    private loadStatusTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.OriginStatus, false, false, true).then(x => {
            this.statusTypes = x.filter(t =>
                t.id === SoeOriginStatus.PurchaseAccepted ||
                t.id === SoeOriginStatus.PurchaseSent ||
                t.id === SoeOriginStatus.PurchaseDone ||
                t.id === SoeOriginStatus.Origin ||
                t.id === SoeOriginStatus.None
            );
            this.setupStatusFunctions(this.statusTypes);
        })
    }
    public loadDeliveryAddresses(customerId: number): ng.IPromise<any> {

        return this.commonCustomerService.getContactAddresses(customerId, TermGroup_SysContactAddressType.Delivery, true, true, false).then((x: ContactAddressDTO[]) => {
            this.deliveryAddresses = x;
        });
    }

    public formatDeliveryAddress(addressRows: any[], isFinInvoiceCustomer: boolean): string {

        let strAddress: string = "";
        let tmpName: string = "";
        let tmpStreetAddress: string = "";
        let tmpPostalCode: string = "";
        let tmpPostalAddress: string = "";
        let tmpCountry: string = "";

        _.forEach(addressRows, (row) => {
            switch (row.sysContactAddressRowTypeId) {
                case TermGroup_SysContactAddressRowType.Name:
                    tmpName += row.text;
                    break;
                case TermGroup_SysContactAddressRowType.StreetAddress:
                    tmpStreetAddress += row.text;
                    break;
                case TermGroup_SysContactAddressRowType.Address:
                    tmpStreetAddress += row.text;
                    break;
                case TermGroup_SysContactAddressRowType.PostalCode:
                    tmpPostalCode += row.text;
                    break;
                case TermGroup_SysContactAddressRowType.PostalAddress:
                    tmpPostalAddress += row.text;
                    break;
                case TermGroup_SysContactAddressRowType.Country:
                    tmpCountry += row.text;
                    break;
            }
        });

        strAddress = tmpName;

        if (strAddress == "" || strAddress == " ")
            strAddress = tmpStreetAddress;
        else
            strAddress += '\r' + tmpStreetAddress;

        strAddress += '\r' + tmpPostalCode;

        if (isFinInvoiceCustomer) //4 lines needed for finvoice
            strAddress += '\r' + tmpPostalAddress;
        else
            strAddress += ' ' + tmpPostalAddress;

        if (tmpCountry !== "") {
            strAddress += '\r' + tmpCountry;
        }

        return strAddress;
    }

    private editDeliveryAddress() {
        let tmpDeliveryAddress: string = this.purchase.deliveryAddress;

        if (this.purchase.deliveryAddressId && this.purchase.deliveryAddressId != 0) {
            tmpDeliveryAddress = this.formatDeliveryAddress(_.filter(this.deliveryAddresses, i => i.contactAddressId == this.purchase.deliveryAddressId)[0].contactAddressRows, false);//this.customer.isFinvoiceCustomer);
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
                deliveryAddress: () => { return tmpDeliveryAddress },
                isFinvoiceCustomer: () => { return false; },
                //isFinvoiceCustomer: () => { return this.customer.isFinvoiceCustomer },
                isLocked: () => { return this.isLocked }
            }
        });

        modal.result.then((result: any) => {
            if ((result) && (result.deliveryAddress != null)) {
                if (result.deliveryAddress !== tmpDeliveryAddress) {
                    this.deliveryAddresses[0].address = result.deliveryAddress;
                    this.purchase.deliveryAddressId = 0;
                    this.purchase.deliveryAddress = result.deliveryAddress;
                    this.setAsDirty();
                }
            }
        });
    }

    private getDeliveryAddresses() {
        const modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Shared/Billing/Dialogs/DeliveryAddressesPurchase/DeliveryAddressesPurchase.html"),
            controller: DeliveryAddressesPurchaseController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'sm',
            resolve: {
                customerOrderId: () => this.purchase.orderId,
            }
        });

        modal.result.then((result: string) => {
            if (result) {
                this.deliveryAddresses[0].address = result;
                this.purchase.deliveryAddressId = 0;
                this.purchase.deliveryAddress = result;
                this.setAsDirty();
            }
        });
    }

    private editEmail() {
        const tmpSupplierEmail: string = this.purchase.supplierEmail;

        const modal = this.notificationService.showDialogEx(this.terms["common.contactaddresses.ecommenu.email"], "", SOEMessageBoxImage.None, SOEMessageBoxButtons.OKCancel, { showTextBox: true, textBoxLabel: this.terms["common.contactaddresses.ecommenu.email"], textBoxValue: this.purchase.supplierEmail });
        modal.result.then(val => {
            modal.result.then(result => {
                if (result.result && result.textBoxValue) {
                    if (result.textBoxValue !== tmpSupplierEmail) {
                        this.supplierHelper.supplierEmails[0].name = result.textBoxValue;
                        this.purchase.contactEComId = 0;
                        this.purchase.supplierEmail = result.textBoxValue;
                        this.setAsDirty();
                    }
                }
            });
        });
    }

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "billing.productrows.vatamount",
            "billing.productrows.totalamount",
            "billing.productrows.functions.newpurchase",
            "billing.purchase.list.purchase",
            "billing.purchase.supplier",
            "billing.order.status",
            "billing.project.project",
            "billing.order.noproject",
            "common.contactaddresses.ecommenu.email",
            "common.customer.invoices.ready",
            "common.sent",
            "billing.purchase.late",
            "billing.purchase.partlydeliverd",
            "billing.purchase.finaldelivered",
            "billing.purchase.confirmed",
            "billing.purchase.origin"
        ]

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }
    private loadData(updateTab = false): ng.IPromise<any> {
        const deferral = this.$q.defer();

        if (this.purchaseId) {
            return this.purchaseService.getPurchaseOrder(this.purchaseId).then((data: PurchaseDTO) => {
                this.isNew = false;
                this.purchase = data;
                //Fix dates
                if (this.purchase.purchaseDate)
                    this.purchase.purchaseDate = new Date(<any>this.purchase.purchaseDate).date();
                if (this.purchase.wantedDeliveryDate)
                    this.purchase.wantedDeliveryDate = new Date(<any>this.purchase.wantedDeliveryDate).date();
                if (this.purchase.confirmedDeliveryDate)
                    this.purchase.confirmedDeliveryDate = new Date(<any>this.purchase.confirmedDeliveryDate).date();

                this.originalPurchase = new PurchaseDTO();
                angular.extend(this.originalPurchase, CoreUtility.cloneDTO(this.purchase));

                this.originUserHelper.setOriginUsers(this.purchase.originUsers);
                this.supplierHelper.loadSupplier(this.purchase.supplierId, true).then(() => {
                    //manual email
                    if (!this.purchase.contactEComId) {
                        this.purchase.contactEComId = 0;
                        if (this.purchase.supplierEmail) {
                            this.supplierHelper.supplierEmails[0].name = this.purchase.supplierEmail;
                        }
                    }
                });
                this.projectHelper.projectId = this.purchase.projectId;

                this.amountHelper.fromPurchase(this.purchase);
                this.setStatusName(false);

                if (updateTab) {
                    this.updateTabCaption();
                }

                //manual delivery Address
                if (this.purchase.deliveryAddressId == 0) {
                    if (this.purchase.deliveryAddress != null && this.purchase.deliveryAddress != "") {
                        this.deliveryAddresses[0].address = this.purchase.deliveryAddress;
                    }
                }                               

                this.setHeadExpanderLabel();
            });
        }
        else {
            this.new();
            deferral.resolve();
        }

        return deferral.promise;
    }

    private loadPurchaseRows(cb, applyWantedPurchaseDate = null, openConfirmedDialog = null, loadFromCopy = false) {
        if (this.purchaseId) {
            this.purchaseService.getPurchaseRows(this.purchaseId).then((data: PurchaseRowDTO[]) => {
                this.originalPurchaseRows = [];

                data.forEach((r, i) => {
                    if (applyWantedPurchaseDate) {
                        r.wantedDeliveryDate = this.selectedWantedDeliveryDate;
                        r.isModified = true;
                    }

                    const originalRow = new PurchaseRowDTO();
                    angular.extend(originalRow, CoreUtility.cloneDTO(r));

                    this.originalPurchaseRows.push(originalRow);
                    if (loadFromCopy) {
                        r.purchaseRowId = 0;
                        r.tempRowId = i + 1;
                    }

                    this.setStatusValues(r);
                });

                this.purchaseRows = data;
                
                if (cb) cb();

                if (openConfirmedDialog)
                    this.openSetPurchaseDateModal(this.purchase.originStatus, this.purchase.originStatus, true);
            });
        }
        else {
            this.purchaseRows = [];
            if (cb) cb();
        }
    }

    private setStatusValues(row: PurchaseRowDTO) {
        if (row.status < 73 && ((row.accDeliveryDate && row.accDeliveryDate < CalendarUtility.getDateToday()) || (this.purchase.confirmedDeliveryDate && this.purchase.confirmedDeliveryDate < CalendarUtility.getDateToday()))) {
            row.statusName = this.terms["billing.purchase.late"];
            row.statusIcon = "fas fa-circle errorColor";
        }
        else {
            switch (row.status) {
                case 70:
                    row.statusName = this.terms["common.customer.invoices.ready"];
                    row.statusIcon = "fas fa-circle mediumGrayColor";
                    break;
                case 71:
                    row.statusName = this.terms["common.sent"]; 
                    row.statusIcon = "fas fa-circle mediumGrayColor";
                    break;
                case 72:
                    row.statusName = this.terms["billing.purchase.confirmed"]; 
                    row.statusIcon = "fas fa-circle warningColor";
                    break;
                case 73:
                    row.statusName = this.terms["billing.purchase.partlydeliverd"]; 
                    row.statusIcon = "fas fa-circle infoColor";
                    break;
                case 74:
                    row.statusName = this.terms["billing.purchase.finaldelivered"]; 
                    row.statusIcon = "fas fa-circle okColor";
                    break;
                default:
                    row.statusName = this.terms["billing.purchase.origin"]; 
                    row.statusIcon = "fas fa-circle mediumGrayColor";
                    break;
            }
        }
    }

    private openPurchaseRowExpander(cb = null, applyWantedPurchaseDate = null, openConfirmedDialog = null) {
        if (!this.purchaseRowsRendered) {
            this.purchaseRowsRendered = true;
            this.$timeout(() => {
                this.updatePurchaseRowsWithBaseData();
            }, 100);
            this.loadPurchaseRows(cb, applyWantedPurchaseDate, openConfirmedDialog);
        }
    }

    private openDeliveryRowsExpander() {
        if (!this.deliveryRowsRendered) {
            this.deliveryRowsRendered = true;
            this.loadDeliveryRows();
        }
    }

    private loadDeliveryRows() {
        if (this.purchaseId) {
            this.purchaseDelivaryRows = [];
            this.purchaseService.getPurchaseDeliveryRowsByPurchaseId(this.purchaseId).then((data: PurchaseDeliveryRowDTO[]) => {
                if (data) {
                    this.purchaseDelivaryRows = data;
                }
            });
        }
    }

    private updatePurchaseRowsWithBaseData() {
        this.$scope.$broadcast('updateFromPurchase', { supplierId: this.purchase.supplierId, purchaseDate: this.purchase.purchaseDate });
    }

    public isDisabled() {
        return !this.dirtyHandler.isDirty || this["edit"].$invalid;
    }

    private executeSaveFunction(option) {
        switch (option.id) {
            case PurchaseEditSaveFunctions.Save:
                this.save(false);
                break;
            case PurchaseEditSaveFunctions.SaveAndClose:
                this.save(true);
                break;
        }
    }

    private executePrintFunction(option) {
        switch (option.id) {
            case PurchaseEditPrintFunctions.Print:
                this.printPurchase(this.defaultReportId, null);
                break;
            case PurchaseEditPrintFunctions.eMail:
                this.showEmailDialog();
                break;
            case PurchaseEditPrintFunctions.ReportDialog:
                this.printFromDialog();
                break;
        }
    }

    private printPurchase(reportId: number, languageId: number, recipients: any[] = null, emailTemplate: number = 0) {
        this.reportService.getPurchaseOrderPrintUrl([this.purchaseId], recipients, reportId, languageId)
            .then((url) => {
                HtmlUtility.openInSameTab(this.$window, url);
            });
    }

    private printFromDialog() {
        const modal = this.$uibModal.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Common/Dialogs/SelectReport/SelectReport.html"),
            controller: SelectReportController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'lg',
            resolve: {
                module: () => null,
                reportTypes: () => [SoeReportTemplateType.PurchaseOrder],
                showCopy: () => false,
                showEmail: () => true,
                copyValue: () => false,
                reports: () => null,
                defaultReportId: () => null,
                langId: () => this.supplierHelper.sysLanguageId,
                showReminder: () => false,
                showLangSelection: () => true,
                showSavePrintout: () => false,
                savePrintout: () => false
            }
        });

        modal.result.then((result: any) => {
            if (result?.reportId) {
                if (result.email)
                    this.showEmailDialog(result.reportId);
                else
                    this.printPurchase(result.reportId, result.languageId);
            }
        });
    }

    private showEmailDialog(reportId: number = 0) {
        const keys: string[] = [
            "billing.purchase.list.purchase",
        ];

        return this.translationService.translateMany(keys).then((types) => {
            const modal = this.$uibModal.open({
                templateUrl: this.urlHelperService.getGlobalUrl("Common/Dialogs/SelectEmail/SelectEmail.html"),
                controller: SelectEmailController,
                controllerAs: 'ctrl',
                backdrop: 'static',
                size: 'lg',
                resolve: {
                    translationService: () => this.translationService,
                    coreService: () => this.coreService,
                    defaultEmail: () => this.purchase.contactEComId,
                    defaultEmailTemplateId: () => this.defaultEmailTemplatePurchase,
                    recipients: () => this.supplierHelper.supplierEmails,
                    attachments: () => null,
                    attachmentsSelected: () => null,
                    checklists: () => null,
                    types: () => types,
                    grid: () => false,
                    type: () => EmailTemplateType.PurchaseOrder,
                    showReportSelection: () => false,
                    reports: () => [],
                    defaultReportTemplateId: null,
                    langId: () => null
                }
            });

            modal.result.then(result => {
                const keys: string[] = [
                    "common.sent",
                    "common.sending"
                ];

                this.translationService.translateMany(keys).then((terms) => {
                    this.progress.startWorkProgress((completion) => {
                        const recipients: number[] = [];
                        let singleRecipient = "";
                        _.forEach(result.recipients, rec => {
                            if (rec.id > 0)
                                recipients.push(rec.id);
                            else
                                singleRecipient = rec.name;
                        });
                        
                        this.purchaseService.sendPurchaseAsEmail(this.purchaseId, reportId, result.emailTemplateId, this.supplierHelper.sysLanguageId, recipients, singleRecipient)
                            .then(res => {
                                if (res.success) {
                                    this.purchase.originStatus = SoeOriginStatus.PurchaseSent;
                                    this.setStatusName(true);
                                    completion.completed(null, false, terms["common.sent"]);
                                }
                            })
                    });

                });
            })
        })
    }

    private saveChangedStatus(id: SoeOriginStatus): ng.IPromise<any> {
        return this.progress.startSaveProgress((completion) => {
            this.purchaseService.savePurchaseStatus(id, this.purchase.purchaseId).then((result) => {
                if (result.success) {
                    completion.completed(Constants.EVENT_EDIT_SAVED, this.purchase);
                }
                else {
                    completion.failed(result.errorMessage);
                }
            });
        }, this.guid).then(data => {
            this.dirtyHandler.clean();
        });
    }

    private save(closeAfterSave: boolean): ng.IPromise<any> {
        return this.progress.startSaveProgress((completion) => {
            let modifiedFields = null;
            if (this.isNew) {
                modifiedFields = CoreUtility.toDTO(this.purchase, PurchaseDTO.getPropertiesToSkipOnSave(), true);
                modifiedFields.copyDeliveryAddress = false;
            }
            else {
                modifiedFields = CoreUtility.diffDTO(this.originalPurchase, this.purchase, PurchaseDTO.getPropertiesToSkipOnSave(), true);
                modifiedFields['purchaseid'] = this.purchase.purchaseId ? this.purchase.purchaseId : 0;
            }

            if (this.purchase.deliveryAddressId > 0) {
                this.purchase.deliveryAddress = "";
                modifiedFields['deliveryaddress'] = null;
            }

            if (this.purchase.contactEComId > 0) {
                this.purchase.supplierEmail = "";
                modifiedFields['supplieremail'] = null;
            } else {
                this.purchase.contactEComId = 0;
                modifiedFields['contactecomid'] = null;
            }

            const users = this.originUserHelper.getOriginUserDTOs();
            const newRows = this.purchaseRows.filter(r => !r.purchaseRowId && (r.type === PurchaseRowType.TextRow || (r.productId || r.supplierProductId )));

            // Modified product rows (only modified fields)
            let modifiedRows: any[] = [];
            _.forEach(_.filter(this.purchaseRows, r => r.purchaseRowId && r.isModified), row => {
                let origRow: PurchaseRowDTO = new PurchaseRowDTO();
                angular.extend(origRow, _.find(this.originalPurchaseRows, r => r.purchaseRowId && r.purchaseRowId === row.purchaseRowId));
                if (origRow) {
                    const rowDiffs = CoreUtility.diffDTO(origRow, row, PurchaseRowDTO.getPropertiesToSkipOnSave(), true);
                    if (row.quantity !== origRow.quantity)
                        rowDiffs["quantity"] = row.quantity;
                    if (row.purchasePriceCurrency !== origRow.purchasePriceCurrency)
                        rowDiffs["purchasepricecurrency"] = row.purchasePriceCurrency;
                    rowDiffs["purchaserowid"] = origRow.purchaseRowId;
                    rowDiffs["rownr"] = row.rowNr;
                    rowDiffs["state"] = row.state;
                    rowDiffs["accDeliveryDate"] = row.accDeliveryDate;
                    modifiedRows.push(rowDiffs);
                } else {
                    newRows.push(row);
                }
            });

            this.purchaseService.savePurchaseOrder(modifiedFields, users, newRows, modifiedRows).then((result) => {
                if (result.success && !this.purchaseId) {
                    this.purchaseId = this.purchase.purchaseId = result.integerValue;
                    this.purchase.purchaseNr = result.stringValue;
                }

                if (result.intDict) {
                    _.forEach(_.filter(this.purchaseRows, r => !r.purchaseRowId || r.purchaseRowId === 0 || r.isModified), (row) => {
                        row.isModified = false;
                        if (result.intDict[row.tempRowId]) {
                            row.purchaseRowId = result.intDict[row.tempRowId];
                            row.isModified = false;
                            row.modified = result.modified;
                            row.modifiedBy = result.modifiedBy;
                        }
                    });
                }

                if (result.success) {
                    this.isNew = false;
                    this.updateTabCaption();
                    this.setStatusName(true);
                    this.originalPurchase = new PurchaseDTO();
                    angular.extend(this.originalPurchase, CoreUtility.cloneDTO(this.purchase));
                    this.originalPurchaseRows = [];

                    this.purchaseRows = _.filter(this.purchaseRows, p => p.purchaseRowId && p.purchaseRowId > 0);

                    this.purchaseRows.forEach(r => {
                        const originalRow = new PurchaseRowDTO();
                        angular.extend(originalRow, CoreUtility.cloneDTO(r));
                        this.originalPurchaseRows.push(originalRow);
                    });

                    this.$scope.$broadcast('refreshRows');

                    completion.completed(this.getSaveEvent(), this.purchase);
                }
                else {
                    completion.failed(result.errorMessage);
                }
            });

        }, this.guid).then(data => {
            this.dirtyHandler.clean();
            if (closeAfterSave) {
                this.closeMe(true);
            }
        });
    }

    private delete() {

        this.translationService.translate("billing.purchase.delete").then((term: string) => {
            this.progress.startDeleteProgress((completion) => {
                this.purchaseService.deletePurchase(this.purchase.purchaseId).then((result: IActionResult) => {
                    if (result.success) {
                        completion.completed(this.purchase, false, null);
                        this.new();
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
            }, null, term);
        });
    }

    private new() {
        this.setAsDirty();
        this.isNew = true;
        this.purchase = new PurchaseDTO();
        this.purchase.originUsers = [];
        this.purchase.vatType = this.defaultVatType;
        this.purchase.originStatus = SoeOriginStatus.Origin;
        this.setStatusName(true);
        this.purchase.deliveryTypeId = this.defaultDeliveryTypeId;
        this.purchase.deliveryConditionId = this.defaultDeliveryConditionId;

        this.purchase.currencyId = this.amountHelper.currencies[0].currencyId;    // Base currency is first in collection
        this.purchase.currencyDate = this.amountHelper.currencyDate = CalendarUtility.getDateToday();
        this.purchase.currencyRate = this.amountHelper.currencyRate;

        this.purchase.purchaseDate = CalendarUtility.getDateToday();
        this.purchase.vatAmountCurrency = 0;
        this.purchase.totalAmountCurrency = 0;

        // Get origin status text for new
        this.translationService.translate("core.new").then(term => {
            this.purchase.statusName = term;
        });

        this.originUserHelper.setDefaultUser();
        this.supplierHelper.loadSupplier(undefined, false);

        this.setHeadExpanderLabel();
    }

    protected copy() {
        this.setAsDirty();

        this.purchase.purchaseId = 0;
        this.purchaseId = 0;
        this.purchase.purchaseNr = "";
        this.purchase.confirmedDeliveryDate = undefined;
        this.purchase.created = undefined;
        this.purchase.createdBy = undefined;
        this.purchase.modified = undefined;
        this.purchase.modifiedBy = undefined;

        this.purchase.originStatus = SoeOriginStatus.Origin;
        this.setStatusName(true);

        if (this.purchaseRowsRendered) {
            if (this.purchaseRows && this.purchaseRows.length > 0) {
                this.purchaseRows.forEach((r, i) => {
                    r.purchaseRowId = 0;
                    r.tempRowId = i + 1;
                });
            }
        } else {
            this.loadPurchaseRows(null, null, null, true);
        }
    }

    private setAsDirty(dirty = true) {
        this.dirtyHandler.isDirty = dirty;
    }

    private projectChanged(projectId: number, projectNumber) {
        this.purchase.projectId = projectId;
        this.purchase.projectNr = projectNumber;

        this.setAsDirty();
    }

    private customerInvoiceChanged(result) {
        if (result && (result.deliveryAddressId > 0 || !StringUtility.isEmpty(result.invoiceHeadText))) {
            const keys: string[] = [
                "core.verifyquestion",
                "billing.purchase.takeorderaddress"
            ];

            this.translationService.translateMany(keys).then((terms) => {
                var modal = this.notificationService.showDialog(terms["core.verifyquestion"], terms["billing.purchase.takeorderaddress"], SOEMessageBoxImage.Question, SOEMessageBoxButtons.YesNo, null, null, null, null, null, null, null, null, null, SOEMessageBoxButton.No);
                modal.result.then(val => {
                    if (val && result.deliveryAddressId > 0) {
                        this.commonCustomerService.getContactAddresses(result.customerId, TermGroup_SysContactAddressType.Delivery, false, true, false).then((addresses: ContactAddressDTO[]) => {
                            const deliveryAddress = _.find(addresses, (a) => a.contactAddressId === result.deliveryAddressId);
                            if (deliveryAddress) {
                                this.purchase.deliveryAddress = this.deliveryAddresses[0].address = this.formatDeliveryAddress(deliveryAddress.contactAddressRows, false);
                                this.purchase.deliveryAddressId = 0;
                            }
                        });
                    }
                    else if (!StringUtility.isEmpty(result.invoiceHeadText)) {
                        this.purchase.deliveryAddress = this.deliveryAddresses[0].address = result.invoiceHeadText;
                        this.purchase.deliveryAddressId = 0;
                    }
                });
            });
        }

        this.purchase.orderId = result ? result.customerInvoiceId : 0;
        this.purchase.orderNr = result ? result.number : "";
        this.purchase.projectId = result ? result.projectId : 0;
        this.purchase.projectNr = result ? result.projectNr : "";

        this.setAsDirty();
    }

    private currencyChanged() {
        if (this.purchase) {
            if (!this.purchaseRowsRendered) {
                this.openPurchaseRowExpander(() => this.currencyChanged())
                return;
            }

            this.amountHelper.toPurchase(this.purchase);
            // Recalculate rows
            this.$scope.$broadcast('recalculateRows');
            //To update GUI
            this.$scope.$applyAsync();
        }
    }

    private initSupplierChanged(supplier: SupplierDTO) {
        if (this.purchase.supplierId && this.purchase.supplierId > 0 && supplier) {
            if (!this.purchaseRowsRendered) {
                this.$timeout(() => {
                    this.purchaseRowsExpanderIsOpen = true;
                    this.openPurchaseRowExpander();
                });
            }
            if (this.purchase.supplierId !== supplier.actorSupplierId) {
                const keys: string[] = [
                    "core.warning",
                    "billing.purchase.changesupplier"
                ];

                this.translationService.translateMany(keys).then((terms) => {
                    const modal = this.notificationService.showDialog(terms["core.warning"], terms["billing.purchase.changesupplier"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                    modal.result.then(val => {
                        this.supplierChanged(supplier);
                    });
                });
            }
            else {
                this.supplierChanged(supplier);
            }
        }
        else {
            this.supplierChanged(supplier);
        }
    }

    private supplierChanged(supplier: SupplierDTO) {
        if (supplier) {
            if (this.purchase.supplierId !== supplier.actorSupplierId) {
                this.purchase.supplierId = supplier.actorSupplierId;
                this.setAsDirty();
                this.updatePurchaseRowsWithBaseData();
            }

            this.purchase.paymentConditionId = supplier.paymentConditionId;
            this.purchase.deliveryConditionId = supplier.deliveryConditionId;
            this.purchase.deliveryTypeId = supplier.deliveryTypeId;
            this.purchase.contactEComId = supplier.contactEcomId;
            this.purchase.supplierCustomerNr = supplier.ourCustomerNr;

            if (!this.purchase.contactEComId && this.supplierHelper.supplierEmails.length > 1) {
                this.purchase.contactEComId = this.supplierHelper.supplierEmails[1].id;
            }

            if (supplier.vatType) {
                this.purchase.vatType = supplier.vatType;
            }

            if (supplier.currencyId) {
                this.amountHelper.currencyId = this.purchase.currencyId = supplier.currencyId;
            }
        }
    }

    private openSetPurchaseDateModal(id: SoeOriginStatus, oldId: SoeOriginStatus, setConfirmed: boolean = false) {
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Shared/Billing/Dialogs/SetPurchaseDate/SetPurchaseDate.html"),
            controller: SetPurchaseDateController,
            controllerAs: "ctrl",
            size: 'lg',
            resolve: {
                newStatus: () => id,
                purchaseRows: () => _.filter(this.purchaseRows, (r) => r.state === SoeEntityState.Active),
                confirmedDate: () => setConfirmed ? this.purchase.confirmedDeliveryDate : undefined,
                useConfirmed: () => setConfirmed,
                purchaseDate: () => this.purchase.purchaseDate,
            }
        }
        const modal = this.$uibModal.open(options);
        modal.result.then((result: any) => {
            const { success } = result;
            if (success) {
                if ((setConfirmed || id === SoeOriginStatus.PurchaseAccepted) && result.date) {
                    this.purchase.confirmedDeliveryDate = result.date;
                    this.dirtyHandler.setDirty();
                }

                this.purchase.originStatus = id;
            } else {
                this.purchase.originStatus = oldId;
            }
            this.setStatusName(true);
            this.purchaseRows = [...this.purchaseRows];
        }, (result: any) => {
            console.log(result)
        });
    }

    private setPurchaseDateCb(newId, oldId) {
        return () => {
            this.openSetPurchaseDateModal(newId, oldId);
        }
    }

    private changeConfirmedDate() {
        if (!this.purchaseRowsRendered)
            this.openPurchaseRowExpander(null, false, true);
        else
            this.openSetPurchaseDateModal(this.purchase.originStatus, this.purchase.originStatus, true);
    }

    private statusChanged(id: SoeOriginStatus) {
        if (id !== this.purchase.originStatus) {
            //this.dirtyHandler.setDirty();
            this.saveChangedStatus(id);
        }

        if (id === SoeOriginStatus.PurchaseAccepted) {
            const cb = this.setPurchaseDateCb(id, this.purchase.originStatus);
            if (this.purchaseRowsRendered) {
                cb();
            } else {
                this.openPurchaseRowExpander(cb)
            }
        } else {
            this.purchase.originStatus = id;
            this.setStatusName(true);
        }
    }

    private setStatusName(statusChanged: boolean) {
        if (statusChanged) {
            const item = this.statusTypes?.find(s => s.id === this.purchase.originStatus)
                || this.statusTypes.find(s => s.id === SoeOriginStatus.Origin);
            this.purchase.statusName = item?.name || "";
        }
        this.currentStatusOption = this.statusFunctions.find(s => s.id === this.purchase.originStatus);
    }



    private updateTabCaption() {
        const termKey = this.isNew ? "billing.purchase.list.new_purchase" : "billing.purchase.list.purchase";
        this.translationService.translate(termKey).then((term) => {
            if (this.isNew)
                this.messagingHandler.publishSetTabLabel(this.guid, term);
            else
                this.messagingHandler.publishSetTabLabel(this.guid, term + " " + this.purchase.purchaseNr, this.purchaseId);
        });
    }

    private setupStatusFunctions(statusTypes: ISmallGenericType[]) {
        this.statusFunctions.push({ id: SoeOriginStatus.Origin, name: statusTypes.find(s => s.id === SoeOriginStatus.Origin).name, icon: "fal fa-file-alt" });
        this.statusFunctions.push({ id: SoeOriginStatus.PurchaseDone, name: statusTypes.find(s => s.id === SoeOriginStatus.PurchaseDone).name, icon: "fal fa-file-check" });
        this.statusFunctions.push({ id: SoeOriginStatus.PurchaseSent, name: statusTypes.find(s => s.id === SoeOriginStatus.PurchaseSent).name, icon: "fal fa-fw fa-envelope" });
        this.statusFunctions.push({ id: SoeOriginStatus.PurchaseAccepted, name: statusTypes.find(s => s.id === SoeOriginStatus.PurchaseAccepted).name, icon: "fal fa-comment-check" });
    }

    private executeStatusFunction(option) {
        this.statusChanged(option.id);
    }

    private shouldUpdateSupplierOnChange() {
        return this.purchase.originStatus === SoeOriginStatus.Origin;
    }

    private isReadonly(lastEditableStatus: string) {
        const status: SoeOriginStatus = this.purchase?.originStatus || SoeOriginStatus.Origin;
        let allowEdit: SoeOriginStatus[];

        if (lastEditableStatus === "origin")
            allowEdit = [SoeOriginStatus.Origin];
        else if (lastEditableStatus === "done")
            allowEdit = [SoeOriginStatus.Origin, SoeOriginStatus.PurchaseDone];
        else if (lastEditableStatus === "sent")
            allowEdit = [
                SoeOriginStatus.Origin,
                SoeOriginStatus.PurchaseDone,
                SoeOriginStatus.PurchaseSent
            ];
        else if (lastEditableStatus === "accepted")
            allowEdit = [
                SoeOriginStatus.Origin,
                SoeOriginStatus.PurchaseDone,
                SoeOriginStatus.PurchaseSent,
                SoeOriginStatus.PurchaseAccepted
            ];
        else if (lastEditableStatus === "partlyDelivered")
            allowEdit = [
                SoeOriginStatus.Origin,
                SoeOriginStatus.PurchaseDone,
                SoeOriginStatus.PurchaseSent,
                SoeOriginStatus.PurchaseAccepted,
                SoeOriginStatus.PurchasePartlyDelivered
            ];
        else if (lastEditableStatus === "deliveryCompleted")
            allowEdit = [
                SoeOriginStatus.Origin,
                SoeOriginStatus.PurchaseDone,
                SoeOriginStatus.PurchaseSent,
                SoeOriginStatus.PurchaseAccepted,
                SoeOriginStatus.PurchasePartlyDelivered,
                SoeOriginStatus.PurchaseDeliveryCompleted
            ];
        else if (lastEditableStatus === "partlyDeliveredNonOrigin")
            allowEdit = [
                SoeOriginStatus.PurchaseDone,
                SoeOriginStatus.PurchaseSent,
                SoeOriginStatus.PurchaseAccepted,
                SoeOriginStatus.PurchasePartlyDelivered
            ];
        return !_.includes(allowEdit, status);
    }

    // VALIDATION
    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            const errors = this['edit'].$error;

            if (errors['supplier'])
                validationErrorKeys.push("common.customer.invoices.validationcustomer");
        });
    }

    public setRowDefaultValues = (data: PurchaseRowDTO) => {
        data.orderId = this.purchase.orderId;
        data.orderNr = this.purchase.orderNr;
    }

    public getProject = () => {
        return {
            projectId: this.purchase.projectId,
            name: this.purchase.projectNr,
        }
    }

    public setHeadExpanderLabel() {
        this.$timeout(() => {
            if (this.isNew) {
                this.headExpanderLabel = this.terms["billing.productrows.functions.newpurchase"];
            }
            else {
                const purchaseNr: string = this.purchase.purchaseNr;
                const supplier: string = this.supplierHelper.selectedSupplier ? this.supplierHelper.selectedSupplier.name : ' ';
                const statusName: string = this.purchase.statusName ? this.purchase.statusName : ' ';
                const projectNr: string = this.purchase.projectNr ? this.purchase.projectNr : this.terms["billing.order.noproject"];

                const label: string = "{0} {1} | {2}: {3} | {4}: {5} | {6}: {7}".format(
                    this.terms["billing.purchase.list.purchase"],
                    purchaseNr,
                    this.terms["billing.purchase.supplier"],
                    supplier,
                    this.terms["billing.order.status"],
                    statusName,
                    this.terms["billing.project.project"],
                    projectNr
                );

                this.headExpanderLabel = label;
            }
        });
    }

    //Dialogs...
    private selectUsers() {
        this.originUserHelper.selectUsersDialog(true, false, false).then((result) => {
            if (result) {
                this.setAsDirty(true);
            }
        });
    }

}