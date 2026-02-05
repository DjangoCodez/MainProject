import { EditControllerBase2 } from "../../../../Core/Controllers/EditControllerBase2";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { ICompositionEditController } from "../../../../Core/ICompositionEditController";
import { SmallGenericType } from "../../../../Common/Models/smallgenerictype";
import { ISmallGenericType } from "../../../../Scripts/TypeLite.Net4";
import { InventoryService } from "../InventoryService";
import { AccountingService } from "../../Accounting/AccountingService";
import { ICommonCustomerService } from "../../../../Common/Customer/CommonCustomerService";
import { ISupplierService } from "../../Supplier/SupplierService";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { IProgressHandlerFactory } from "../../../../Core/Handlers/ProgressHandlerFactory";
import { IControllerFlowHandlerFactory } from "../../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { IMessagingHandlerFactory } from "../../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../../Core/Handlers/DirtyHandlerFactory";
import { Guid } from "../../../../Util/StringUtility";
import { IPermissionRetrievalResponse } from "../../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../../Core/Handlers/ToolbarFactory";
import { InventoryAdjustFunctions, SOEMessageBoxImage, SOEMessageBoxButtons, IconLibrary } from "../../../../Util/Enumerations";
import { SettingsUtility } from "../../../../Util/SettingsUtility";
import { ToolBarButtonGroup, ToolBarButton } from "../../../../Util/ToolBarUtility";
import { TabMessage } from "../../../../Core/Controllers/TabsControllerBase1";
import { EditController as CustomerInvoicesEditController } from "../../../../Common/Customer/Invoices/EditController";
import { EditController as VouchersEditController } from "../../../../Shared/Economy/Accounting/Vouchers/EditController";
import { InventoryAdjustmentController } from "./Dialogs/InventoryAdjustment/InventoryAdjustmentController";
import { TermGroup_InventoryStatus, SoeEntityType, SoeEntityImageType, Feature, CompanySettingType, InventoryAccountType, TermGroup, SoeOriginStatusClassification, SoeOriginType, TermGroup_ChangeStatusGridAllItemsSelection, TermGroup_InventoryWriteOffMethodPeriodType } from "../../../../Util/CommonEnumerations";
import { Constants } from "../../../../Util/Constants";
import { EditController as SupplierInvoicesEditController } from "../../../../Shared/Economy/Supplier/Invoices/EditController";
import { FilesHelper } from "../../../../Common/Files/FilesHelper";
import { ISoeGridOptionsAg, SoeGridOptionsAg } from "../../../../Util/SoeGridOptionsAg";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Modal
    modal: any;
    private isModal: boolean = false;

    //helpers...
    private filesHelper: FilesHelper;
    private documentExpanderIsOpen: boolean = false;

    // Data
    inventory: any;
    inventoryId: number
    inventoryWriteOffTemplate: any;
    accountingRows: any[] = [];
    inventoryWriteOffTemplateId: number = 0;
    purchaseAmount: number = 0;
    purchaseDate: Date;
    inventoryIds: number[];

    // Lookups     
    periodTypes: any[];
    voucherSeries: any[];
    categoryRecords: any = [];
    inventoryWriteOffTemplates: any[];
    inventoryWriteOffMethods: any[];
    inventories: any[];
    inventoryTraceViews: any[];
    private supplierInvoices: SmallGenericType[] = [];
    private customerInvoices: SmallGenericType[] = [];

    // Settings
    inventoryBaseAccounts: SmallGenericType[];
    inventoryAccountSettingTypes: SmallGenericType[];

    // Properties
    private isWriteOffsStarted = false;
    private showNavigationButtons = true;
    private isTraceViewOpened = false;

    private _draft = false;
    get draft(): boolean {
        return this._draft;
    }
    set draft(draft: boolean) {
        if (this._draft !== draft) {
            this._draft = draft;

            this.fixInventoryList();
        }
    }

    get isDraft(): boolean {
        return this.isNew || this.inventory.status === TermGroup_InventoryStatus.Draft;
    }

    get isDraftOrActive(): boolean {
        return this.isNew || this.inventory.status === TermGroup_InventoryStatus.Draft ||
            this.inventory.status === TermGroup_InventoryStatus.Active;
    }

    get isDraftOrActiveOrWrittenOff(): boolean {
        return this.isNew || this.inventory.status === TermGroup_InventoryStatus.Draft ||
            this.inventory.status === TermGroup_InventoryStatus.Active ||
            this.inventory.status === TermGroup_InventoryStatus.WrittenOff;
    }

    private _selectedWriteOffTemplateId;
    get selectedWriteOffTemplateId(): number {
        return this._selectedWriteOffTemplateId;
    }
    set selectedWriteOffTemplateId(id: number) {
        this._selectedWriteOffTemplateId = id;
        this.loadWriteOffTemplate(id);
    }

    private _selectedSupplierInvoice;
    get selectedSupplierInvoice(): ISmallGenericType {
        return this._selectedSupplierInvoice;
    }
    set selectedSupplierInvoice(item: ISmallGenericType) {
        this._selectedSupplierInvoice = item;
        this.inventory.supplierInvoiceId = this.selectedSupplierInvoice ? this.selectedSupplierInvoice.id : 0;
    }

    private _selectedCustomerInvoice;
    get selectedCustomerInvoice(): ISmallGenericType {
        return this._selectedCustomerInvoice;
    }
    set selectedCustomerInvoice(item: ISmallGenericType) {
        this._selectedCustomerInvoice = item;
        this.inventory.customerInvoiceId = this.selectedCustomerInvoice ? this.selectedCustomerInvoice.id : 0;
    }

    private _selectedPurchaseDate: any;
    get selectedPurchaseDate() {
        return this._selectedPurchaseDate;
    }
    set selectedPurchaseDate(date: any) {
        this._selectedPurchaseDate = date;

        if (this.inventory) {
            this.inventory.purchaseDate = this.selectedPurchaseDate;
            if ((this.selectedPurchaseDate) && (this.inventory.writeOffDate == null || this.inventory.writeOffDate == "" || this.isNew))
                this.inventory.writeOffDate = this.selectedPurchaseDate.beginningOfMonth();
        }
    }

    //Terms
    terms: { [index: string]: string; };

    // Flags
    private isWrittenOff: boolean = false;

    //InventoryTraceViews
    private inventoryTraceViewGridOptions: ISoeGridOptionsAg;

    info: string;
    private modalInstance: any;

    // Functions
    private adjustFunctions: any = [];
    private disposeFunctions: any = [];


    //@ngInject  
    constructor(
        $uibModal,
        private $q: ng.IQService,
        private $timeout: ng.ITimeoutService,
        private inventoryService: InventoryService,
        private accountingService: AccountingService,
        private commonCustomerService: ICommonCustomerService,
        private supplierService: ISupplierService,
        private coreService: ICoreService,
        private translationService: ITranslationService,
        private messagingService: IMessagingService,
        private urlHelperService: IUrlHelperService,
        private notificationService: INotificationService,
        progressHandlerFactory: IProgressHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory,
        private $scope: ng.IScope) {

        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onLoadData(() => this.onLoadData()) //this.doLookups())
            .onDoLookUp(() => this.onDoLookups()) //this.doLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));

        this.modalInstance = $uibModal;

        $scope.$on(Constants.EVENT_ON_INIT_MODAL, (e, parameters) => {
            parameters.guid = Guid.newGuid();
            this.isModal = true;
            this.modal = parameters.modal;
            this.onInit(parameters);
            //this.focusService.focusByName(parameters.id ? "ctrl_inventory_name" : "ctrl_inventory_number");                
        });
    }

    public onInit(parameters: any) {
        this.inventoryId = parameters.id;
        this.guid = parameters.guid;
        if (this.isModal) {
            this.inventoryWriteOffTemplateId = parameters.writeOffTemplateId;
            this.purchaseAmount = parameters.amount;
            this.purchaseDate = parameters.purchaseDate;
        }
        if (parameters.ids && parameters.ids.length > 0) {
            this.inventoryIds = parameters.ids;
        } else {
            this.showNavigationButtons = false;
        }


        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);
        this.filesHelper = new FilesHelper(this.coreService, this.$q, this.dirtyHandler, true, SoeEntityType.Inventory, SoeEntityImageType.Inventory, () => this.inventoryId);

        this.initInventoryTraceViewGrid();
        this.setupInventoryTraceViewGrid();
        this.flowHandler.start([{ feature: Feature.Economy_Inventory_Inventories_Edit, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Economy_Inventory_Inventories_Edit].readPermission;
        this.modifyPermission = response[Feature.Economy_Inventory_Inventories_Edit].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => this.copy(), () => this.isNew);

        let buttonGroup = new ToolBarButtonGroup();
        buttonGroup.buttons.push(new ToolBarButton("", null, IconLibrary.FontAwesome, "fa-sync", () => {
            this.onLoadData();
        }, null, () => {
            return false;
        }))
        this.toolbar.addButtonGroup(buttonGroup);


        if (this.showNavigationButtons) {
            this.toolbar.setupNavigationGroup(null, () => { return this.isNew }, (inventoryId) => {
                this.inventoryId = inventoryId;
                this.onLoadData();
            }, this.inventoryIds, this.inventoryId);
        }

        // Functions
        const keys: string[] = [
            "economy.inventory.inventories.overwriteoff",
            "economy.inventory.inventories.underwriteoff",
            "economy.inventory.inventories.writedown",
            "economy.inventory.inventories.writeup",
            "economy.inventory.inventories.sold",
            "economy.inventory.inventories.discarded",
            "economy.inventory.inventories.setaswrittenoff"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.adjustFunctions.push({ id: InventoryAdjustFunctions.OverWriteOff, name: terms["economy.inventory.inventories.overwriteoff"] });
            this.adjustFunctions.push({ id: InventoryAdjustFunctions.UnderWriteOff, name: terms["economy.inventory.inventories.underwriteoff"] });
            this.adjustFunctions.push({ id: InventoryAdjustFunctions.WriteDown, name: terms["economy.inventory.inventories.writedown"] });
            this.adjustFunctions.push({ id: InventoryAdjustFunctions.WriteUp, name: terms["economy.inventory.inventories.writeup"] });

            this.disposeFunctions.push({ id: InventoryAdjustFunctions.Sold, name: terms["economy.inventory.inventories.sold"], hidden: () => { return !this.isDraftOrActive; } });
            this.disposeFunctions.push({ id: InventoryAdjustFunctions.Discarded, name: terms["economy.inventory.inventories.discarded"] });
            this.disposeFunctions.push({ id: InventoryAdjustFunctions.WrittenOff, name: terms["economy.inventory.inventories.setaswrittenoff"], hidden: () => { return !this.isDraftOrActive || !this.isWrittenOff; } });
        });

    }

    // SETUP

    private onDoLookups() {
        return this.$q.all([this.loadCompanySettings(), this.loadSettingTypes(), this.loadTerms(), this.loadPeriodTypes(), this.loadVoucherSeriesTypes(), this.loadWriteOffTemplates(),
        this.loadWriteOffMethods(), this.loadInventoriesDict()]);
    }

    private setupWatchers() {

        this.$scope.$watch(() => this.inventoryTraceViews, () => {
            this.inventoryTraceViewGridOptions.setData(this.inventoryTraceViews);
        });

    }

    // LOOKUPS

    private onLoadData(): ng.IPromise<any> {
        if (this.inventoryId > 0) {
            return this.inventoryService.getInventory(this.inventoryId).then((x) => {
                this.setTabLabel();
                this.isNew = false;
                this.inventory = x;
                this.inventory.accWriteOffAmount = (this.inventory.writeOffAmount - this.inventory.writeOffRemainingAmount).round(2);

                this.isWrittenOff = this.inventory.writeOffRemainingAmount <= 0;

                // Fix dates
                if (this.inventory.purchaseDate)
                    this.inventory.purchaseDate = new Date(<any>this.inventory.purchaseDate);
                if (this.inventory.writeOffDate)
                    this.inventory.writeOffDate = new Date(<any>this.inventory.writeOffDate);

                this.setPeriodValuePercent();

                // Mark image gallery with an asterix if any images or attachments are on the inventory
                // TODO
                this.filesHelper.nbrOfFiles = '*';
                this.draft = (this.inventory.status === TermGroup_InventoryStatus.Draft);

                /* Should be:
                    this.inventory.accWriteOffAmount <= this.inventory.writeOffSum.
                    Otherwise, if user enters value into previously deprecated (and saves), the delete button disapears.
                */
                this.isWriteOffsStarted = this.inventory.accWriteOffAmount != 0;

                this.selectedPurchaseDate = this.inventory.purchaseDate;
                this.selectedSupplierInvoice = { id: this.inventory.supplierInvoiceId, name: this.inventory.supplierInvoiceInfo };
                this.selectedCustomerInvoice = { id: this.inventory.customerInvoiceId, name: this.inventory.customerInvoiceInfo };
                this.dirtyHandler.clean()
            });
        }
        else {
            this.new();
        }
    }

    private fixInventoryList() {
        //Make sure you cant select your self!
        if (this.draft && this.inventoryId && this.inventories) {
            this.inventories = this.inventories.filter(x => x.id !== this.inventoryId);
        }
    }

    private loadCompanySettings(): ng.IPromise<any> {
        const settingTypes: number[] = [];

        //base accounts
        settingTypes.push(CompanySettingType.AccountInventoryInventories);
        settingTypes.push(CompanySettingType.AccountInventoryAccWriteOff);
        settingTypes.push(CompanySettingType.AccountInventoryWriteOff);
        settingTypes.push(CompanySettingType.AccountInventoryAccOverWriteOff);
        settingTypes.push(CompanySettingType.AccountInventoryOverWriteOff);
        settingTypes.push(CompanySettingType.AccountInventoryAccWriteDown);
        settingTypes.push(CompanySettingType.AccountInventoryWriteDown);
        settingTypes.push(CompanySettingType.AccountInventoryAccWriteUp);
        settingTypes.push(CompanySettingType.AccountInventoryWriteUp);
        settingTypes.push(CompanySettingType.AccountInventorySalesProfit);
        settingTypes.push(CompanySettingType.AccountInventorySalesLoss);
        settingTypes.push(CompanySettingType.AccountInventorySales);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            // Base accounts
            this.inventoryBaseAccounts = [];
            this.inventoryBaseAccounts.push(new SmallGenericType(InventoryAccountType.Inventory, SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountInventoryInventories).toString()));
            this.inventoryBaseAccounts.push(new SmallGenericType(InventoryAccountType.AccWriteOff, SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountInventoryAccWriteOff).toString()));
            this.inventoryBaseAccounts.push(new SmallGenericType(InventoryAccountType.WriteOff, SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountInventoryWriteOff).toString()));
            this.inventoryBaseAccounts.push(new SmallGenericType(InventoryAccountType.AccOverWriteOff, SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountInventoryAccOverWriteOff).toString()));
            this.inventoryBaseAccounts.push(new SmallGenericType(InventoryAccountType.OverWriteOff, SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountInventoryOverWriteOff).toString()));
            this.inventoryBaseAccounts.push(new SmallGenericType(InventoryAccountType.AccWriteDown, SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountInventoryAccWriteDown).toString()));
            this.inventoryBaseAccounts.push(new SmallGenericType(InventoryAccountType.WriteDown, SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountInventoryWriteDown).toString()));
            this.inventoryBaseAccounts.push(new SmallGenericType(InventoryAccountType.AccWriteUp, SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountInventoryAccWriteUp).toString()));
            this.inventoryBaseAccounts.push(new SmallGenericType(InventoryAccountType.WriteUp, SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountInventoryWriteUp).toString()));
            this.inventoryBaseAccounts.push(new SmallGenericType(InventoryAccountType.SalesProfit, SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountInventorySalesProfit).toString()));
            this.inventoryBaseAccounts.push(new SmallGenericType(InventoryAccountType.SalesLoss, SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountInventorySalesLoss).toString()));
            this.inventoryBaseAccounts.push(new SmallGenericType(InventoryAccountType.Sales, SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountInventorySales).toString()));
        });
    }

    private loadSettingTypes(): ng.IPromise<any> {
        const keys: string[] = [
            "economy.inventory.inventoryaccountsettingtype.inventory",
            "economy.inventory.inventoryaccountsettingtype.accwriteoff",
            "economy.inventory.inventoryaccountsettingtype.writeoff",
            "economy.inventory.inventoryaccountsettingtype.accoverwriteoff",
            "economy.inventory.inventoryaccountsettingtype.overwriteoff",
            "economy.inventory.inventoryaccountsettingtype.accwritedown",
            "economy.inventory.inventoryaccountsettingtype.writedown",
            "economy.inventory.inventoryaccountsettingtype.accwriteup",
            "economy.inventory.inventoryaccountsettingtype.writeup"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.inventoryAccountSettingTypes = [];
            this.inventoryAccountSettingTypes.push(new SmallGenericType(InventoryAccountType.Inventory, terms["economy.inventory.inventoryaccountsettingtype.inventory"]));
            this.inventoryAccountSettingTypes.push(new SmallGenericType(InventoryAccountType.AccWriteOff, terms["economy.inventory.inventoryaccountsettingtype.accwriteoff"]));
            this.inventoryAccountSettingTypes.push(new SmallGenericType(InventoryAccountType.WriteOff, terms["economy.inventory.inventoryaccountsettingtype.writeoff"]));
            this.inventoryAccountSettingTypes.push(new SmallGenericType(InventoryAccountType.AccOverWriteOff, terms["economy.inventory.inventoryaccountsettingtype.accoverwriteoff"]));
            this.inventoryAccountSettingTypes.push(new SmallGenericType(InventoryAccountType.OverWriteOff, terms["economy.inventory.inventoryaccountsettingtype.overwriteoff"]));
            this.inventoryAccountSettingTypes.push(new SmallGenericType(InventoryAccountType.AccWriteDown, terms["economy.inventory.inventoryaccountsettingtype.accwritedown"]));
            this.inventoryAccountSettingTypes.push(new SmallGenericType(InventoryAccountType.WriteDown, terms["economy.inventory.inventoryaccountsettingtype.writedown"]));
            this.inventoryAccountSettingTypes.push(new SmallGenericType(InventoryAccountType.AccWriteUp, terms["economy.inventory.inventoryaccountsettingtype.accwriteup"]));
            this.inventoryAccountSettingTypes.push(new SmallGenericType(InventoryAccountType.WriteUp, terms["economy.inventory.inventoryaccountsettingtype.writeup"]));
        });
    }

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "economy.inventory.inventories.years",
            "economy.accounting.voucher.voucher",
            "economy.inventory.inventories.supplierinvoice",
            "economy.inventory.inventories.customerinvoice",
            "common.name",
            "core.edit",
            "core.yes",
            "core.no"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private loadPeriodTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.InventoryWriteOffMethodPeriodType, false, false).then((x) => {
            this.periodTypes = x;
        });
    }

    private loadVoucherSeriesTypes(): ng.IPromise<any> {
        return this.accountingService.getVoucherSeriesTypes().then((x) => {
            this.voucherSeries = x;
        });
    }

    private loadWriteOffTemplates(): ng.IPromise<any> {
        return this.inventoryService.getInventoryWriteOffTemplatesDict(true).then((x) => {
            this.inventoryWriteOffTemplates = x;
        });
    }

    private loadWriteOffMethods(): ng.IPromise<any> {
        return this.inventoryService.getInventoryWriteOffMethods().then((x) => {
            this.inventoryWriteOffMethods = x;
        });
    }

    private loadInventoriesDict(): ng.IPromise<any> {
        return this.inventoryService.getInventoriesDict().then((x) => {
            this.inventories = x;
        });
    }

    private ConvertToInventoryAccountingSettings() {
        this.inventory.accountingSettings = [];

        _.forEach(this.inventoryAccountSettingTypes, (y) => {
            switch (y.id) {
                case InventoryAccountType.Inventory:
                    var accounts = this.inventoryWriteOffTemplate.inventoryAccounts;
                    break;
                case InventoryAccountType.AccWriteOff:
                    var accounts = this.inventoryWriteOffTemplate.accWriteOffAccounts;
                    break;
                case InventoryAccountType.WriteOff:
                    var accounts = this.inventoryWriteOffTemplate.writeOffAccounts;
                    break;
                case InventoryAccountType.AccOverWriteOff:
                    var accounts = this.inventoryWriteOffTemplate.accOverWriteOffAccounts;
                    break;
                case InventoryAccountType.OverWriteOff:
                    var accounts = this.inventoryWriteOffTemplate.overWriteOffAccounts;
                    break;
                case InventoryAccountType.AccWriteDown:
                    var accounts = this.inventoryWriteOffTemplate.accWriteDownAccounts;
                    break;
                case InventoryAccountType.WriteDown:
                    var accounts = this.inventoryWriteOffTemplate.writeDownAccounts;
                    break;
                case InventoryAccountType.AccWriteUp:
                    var accounts = this.inventoryWriteOffTemplate.accWriteUpAccounts;
                    break;
                case InventoryAccountType.WriteUp:
                    var accounts = this.inventoryWriteOffTemplate.writeUpAccounts;
                    break;
            }

            var row = {
                "type": y.id,
                "typeName": y.name,
                "account1Id": accounts != null && accounts[1] != null ? accounts[1].accountId : 0,
                "account1Name": accounts != null && accounts[1] != null ? accounts[1].name : null,
                "account1Nr": accounts != null && accounts[1] != null ? accounts[1].number : null,
                "accountDim1Nr": 1,
                "account2Id": accounts != null && accounts[2] != null ? accounts[2].accountId : 0,
                "account2Name": accounts != null && accounts[2] != null ? accounts[2].name : null,
                "account2Nr": accounts != null && accounts[2] != null ? accounts[2].number : null,
                "accountDim2Nr": 2,
                "account3Id": accounts != null && accounts[3] != null ? accounts[3].accountId : 0,
                "account3Name": accounts != null && accounts[3] != null ? accounts[3].name : null,
                "account3Nr": accounts != null && accounts[3] != null ? accounts[3].number : null,
                "accountDim3Nr": 3,
                "account4Id": accounts != null && accounts[4] != null ? accounts[4].accountId : 0,
                "account4Name": accounts != null && accounts[4] != null ? accounts[4].name : null,
                "account4Nr": accounts != null && accounts[4] != null ? accounts[4].number : null,
                "accountDim4Nr": 4,
                "account5Id": accounts != null && accounts[5] != null ? accounts[5].accountId : 0,
                "account5Name": accounts != null && accounts[5] != null ? accounts[5].name : null,
                "account5Nr": accounts != null && accounts[5] != null ? accounts[5].number : null,
                "accountDim5Nr": 5,
                "account6Id": accounts != null && accounts[6] != null ? accounts[6].accountId : 0,
                "account6Name": accounts != null && accounts[6] != null ? accounts[6].name : null,
                "account6Nr": accounts != null && accounts[6] != null ? accounts[6].number : null,
                "accountDim6Nr": 6,
                "baseAccount": null
            };
            this.inventory.accountingSettings.push(row);
        });
    }

    private setTabLabel() {
        let tabLabel: string;
        const keys: string[] = [
            "economy.inventory.inventories.inventory",
            "economy.inventory.inventories.new"
        ]

        this.translationService.translateMany(keys).then((terms) => {
            if (this.inventoryId < 1) {
                tabLabel = terms["economy.inventory.inventories.new"];
            }
            else {
                tabLabel = terms["economy.inventory.inventories.inventory"] + " " + this.inventory.name;
            }
            this.messagingService.publish(Constants.EVENT_SET_TAB_LABEL, {
                guid: this.guid,
                label: tabLabel
            });
        });
    }

    //EVENTS


    private amountChanged(id: string) {
        this.$timeout(() => {
            var purchaseAmount: number = this.inventory.purchaseAmount;
            var endAmount: number = this.inventory.endAmount;
            var writeOffSum: number = this.inventory.writeOffSum;
            var writeOffAmount: number = 0;
            var writeOffRemainingAmount: number = 0;
            var accWriteOffAmount: number = 0;

            writeOffAmount = purchaseAmount - endAmount;
            writeOffRemainingAmount = writeOffAmount - writeOffSum;
            accWriteOffAmount = writeOffAmount - writeOffRemainingAmount;

            this.inventory.writeOffAmount = writeOffAmount;
            this.inventory.writeOffRemainingAmount = writeOffRemainingAmount;
            this.inventory.accWriteOffAmount = accWriteOffAmount;
        });
    }

    private writeOffMethodIdChanged(id: number) {
        this.inventory.periodType = _.filter(this.inventoryWriteOffMethods, i => i.inventoryWriteOffMethodId == id)[0].periodType;
        this.inventory.periodValue = _.filter(this.inventoryWriteOffMethods, i => i.inventoryWriteOffMethodId == id)[0].periodValue;
        this.setPeriodValuePercent()
    }

    private LoadInvoices() {
        this.commonCustomerService.getCustomerInvoices(SoeOriginStatusClassification.CustomerInvoicesAll, SoeOriginType.CustomerInvoice, true, true, false, true, TermGroup_ChangeStatusGridAllItemsSelection.Twelve_Months, false).then((x) => {
            _.forEach(x, (y) => {
                if (y.seqNr) {
                    this.customerInvoices.push({
                        id: y.customerInvoiceId,
                        name: y.seqNr + ' - ' + y.invoiceNr + ' - ' + y.actorCustomerName
                    })
                }
            })
        });
        this.supplierService.getInvoicesForGrid(TermGroup_ChangeStatusGridAllItemsSelection.Twelve_Months, true, true).then((x) => {
            _.forEach(x, (y) => {
                if (y.seqNr) {
                    this.supplierInvoices.push({
                        id: y.supplierInvoiceId,
                        name: y.seqNr + ' - ' + y.invoiceNr + ' - ' + y.supplierName
                    })
                }
            });
        });
    }

    private openSupplierInvoice(id?: number) {
        const idToOpen = id || this.inventory.supplierInvoiceId;
        if (this.inventory.supplierInvoiceId != null)
            this.messagingService.publish(Constants.EVENT_OPEN_TAB, new TabMessage(this.terms["economy.inventory.inventories.supplierinvoice"] + " " + idToOpen, idToOpen, SupplierInvoicesEditController, { id: idToOpen }, this.urlHelperService.getGlobalUrl('Shared/Economy/Supplier/Invoices/Views/edit.html')));
    }

    private openCustomerInvoice(id?: number) {
        if (this.inventory.customerInvoiceId != null)
            this.messagingService.publish(Constants.EVENT_OPEN_TAB, new TabMessage(this.terms["economy.inventory.inventories.customerinvoice"] + " " + this.inventory.customerInvoiceId, this.inventory.customerInvoiceId, CustomerInvoicesEditController, { id: this.inventory.customerInvoiceId }, this.urlHelperService.getGlobalUrl('Common/Customer/Invoices/Views/edit.html')));
    }

    /* Naming is wrong! Returning True actually means depreciations HASN'T started.
        Should probably use isWriteOffsStarted instead, the definition of which should changed to below, i.e.:
        this.inventory.accWriteOffAmount <= this.inventory.writeOffSum
    */
    public writeOfHasStarted(): boolean {
        if (this.inventory?.accWriteOffAmount > 0)
            return (this.inventory.accWriteOffAmount <= this.inventory.writeOffSum)
        else
            return true;
    }

    //ACTIONS
    public copy() {
        this.isNew = true;
        this.draft = true;
        this.isWriteOffsStarted = false;

        this.inventoryId = 0;
        this.inventory.statusName = "";
        this.inventory.inventoryId = undefined;
        this.inventory.writeOffSum = 0;
        this.inventory.endAmount = 0;
        this.inventory.writeOffPeriods = 0;
        this.amountChanged(null);

        this.inventoryTraceViewGridOptions.setData(null);
        this.filesHelper.files = [];

        this.inventoryService.getNextInventoryNr().then(x => {
            this.inventory.inventoryNr = x;
        });

        this.dirtyHandler.setDirty();
        this.messagingService.publish(Constants.EVENT_EDIT_NEW, { guid: this.guid });
    }

    public save() {
        this.progress.startSaveProgress((completion) => {

            //categories            
            _.forEach(this.inventory.categoryIds, (id) => {
                this.categoryRecords.push({
                    categoryId: id,
                    default: false,
                });
            });

            if (this.filesHelper.filesLoaded) {
                this.inventory.inventoryFiles = this.filesHelper.getAsDTOs();
            }

            if (this.inventory.customerInvoiceId == 0) {
                this.inventory.customerInvoiceId = null;
                this.inventory.customerInvoiceInfo = "";
            }

            if (this.inventory.supplierInvoiceId == 0) {
                this.inventory.supplierInvoiceId = null;
                this.inventory.supplierInvoiceInfo = "";
            }

            if (this.isNew)
                this.inventory.status = this.draft ? TermGroup_InventoryStatus.Draft : TermGroup_InventoryStatus.Active;
            else
                if (!this.draft && this.inventory.status == TermGroup_InventoryStatus.Draft)
                    this.inventory.status = TermGroup_InventoryStatus.Active;

            this.inventoryService.saveInventory(this.inventory, this.categoryRecords, this.inventory.accountingSettings, 0).then((result) => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0)
                        this.inventoryId = result.integerValue;

                    this.filesHelper.reset();
                    this.documentExpanderIsOpen = false;

                    completion.completed(Constants.EVENT_EDIT_SAVED, this.inventory);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid)
            .then(data => {
                if (this.isModal)
                    this.closeModal();
                else {
                    this.dirtyHandler.clean();
                    this.onLoadData();

                    if (this.isTraceViewOpened)
                        this.LoadTraceViews();
                }
            }, error => {

            });
    }

    public closeModal() {
        if (this.isModal) {
            this.modal.close({ inventoryId: this.inventoryId });
        }
    }

    protected initDelete() {
        // Show verification dialog
        var keys: string[] = [
            "core.warning",
            "economy.inventory.inventories.deleteinventorywarning",
        ];

        this.translationService.translateMany(keys).then((terms) => {
            var message = terms["economy.inventory.inventories.deleteinventorywarning"];

            var modal = this.notificationService.showDialog(terms["core.warning"], message, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
            modal.result.then(val => {
                if (val) {
                    this.delete();
                }
            });
        });
    }

    public delete() {
        this.progress.startDeleteProgress((completion) => {
            this.inventoryService.deleteInventory(this.inventory.inventoryId).then((result) => {
                if (result.success) {
                    completion.completed(this.inventory);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }).then(x => {
            super.closeMe(false);
        });
    }

    private executeAdjustFunction(option) {
        this.openInventoryAdjustmentDialog(option.id);
    }

    private executeDisposeFunction(option) {
        if (option.id === InventoryAdjustFunctions.WrittenOff) {
            this.inventory.status = TermGroup_InventoryStatus.WrittenOff;
            this.save();
        } else {
            this.openInventoryAdjustmentDialog(option.id);
        }
    }

    private openInventoryAdjustmentDialog(adjustmentType: number) {
        var modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Shared/Economy/Inventory/Inventories/Dialogs/InventoryAdjustment/InventoryAdjustment.html"),
            controller: InventoryAdjustmentController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'lg',
            resolve: {
                translationService: () => { return this.translationService },
                coreService: () => { return this.coreService },
                inventoryId: () => { return this.inventoryId },
                purchaseDate: () => { return this.inventory.purchaseDate },
                purchaseAmount: () => { return this.inventory.purchaseAmount },
                accWriteOffAmount: () => { return this.inventory.accWriteOffAmount },
                adjustmentType: () => { return adjustmentType },
                accountingSettings: () => { return this.inventory.accountingSettings },
                inventoryBaseAccounts: () => { return this.inventoryBaseAccounts },
                noteText: () => { return this.inventory.inventoryNr + " " + this.inventory.name }
            }
        });
        modal.result.then((x) => {
            //if (adjustmentType === InventoryAdjustFunctions.Sold || adjustmentType === InventoryAdjustFunctions.Discarded) {
            //    this.inventory.writeOffRemainingAmount = 0;
            //    this.save();
            //}
            //else if (adjustmentType === InventoryAdjustFunctions.OverWriteOff || adjustmentType === InventoryAdjustFunctions.WriteDown) {
            //    this.inventory.accWriteOffAmount -= x.decimalValue;
            //}
            this.onLoadData();
            this.LoadTraceViews();
        });
    }

    //Start InventoryTraceViewGrid
    private initInventoryTraceViewGrid() {
        this.inventoryTraceViewGridOptions = new SoeGridOptionsAg("economy.inventory.inventories.inventorytraceview", this.$timeout);
        this.inventoryTraceViewGridOptions.enableGridMenu = false;
        //this.inventoryTraceViewGridOptions.showGridFooter = true;
        //this.inventoryTraceViewGridOptions.showColumnFooter = false;
        this.inventoryTraceViewGridOptions.setMinRowsToShow(10);

    }

    private setupInventoryTraceViewGrid() {
        var keys: string[] = [
            "common.date",
            "common.type",
            "common.amount",
            "economy.accounting.voucher.voucher",
            "economy.inventory.inventories.invoice",
            "core.edit",
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total",
            "core.aggrid.totals.selected"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.inventoryTraceViewGridOptions.addColumnDate("date", terms["common.date"], null);
            this.inventoryTraceViewGridOptions.addColumnText("typeName", terms["common.type"], null);
            this.inventoryTraceViewGridOptions.addColumnNumber("amount", terms["common.amount"], null, { decimals: 2 });
            this.inventoryTraceViewGridOptions.addColumnNumber("voucherNr", terms["economy.accounting.voucher.voucher"], null, { clearZero: true });
            this.inventoryTraceViewGridOptions.addColumnNumber("invoiceNr", terms["economy.inventory.inventories.invoice"], null, { clearZero: true });
            this.inventoryTraceViewGridOptions.addColumnEdit(terms["core.edit"], (row) => this.openTraceRow(row), null, this.showTraceViewEditButton.bind(this));

            this.inventoryTraceViewGridOptions.addTotalRow("#totals-grid", {
                filtered: terms["core.aggrid.totals.filtered"],
                total: terms["core.aggrid.totals.total"],
                selected: terms["core.aggrid.totals.selected"]
            });
            this.inventoryTraceViewGridOptions.finalizeInitGrid();
        });
    }

    private showTraceViewEditButton({ hideEdit }) {
        return hideEdit == false;
    }

    private LoadTraceViews() {
        this.inventoryService.getInventoryTraceViews(this.inventoryId).then(x => {
            this.inventoryTraceViews = _.forEach(x, (row) => {
                row.hideEdit = true;
                if (row.voucherHeadId || row.supplierInvoiceId || row.customerInvoiceId)
                    row.hideEdit = false;
            })
            this.inventoryTraceViewGridOptions.setData(this.inventoryTraceViews);
        });
    }

    private openTraceRow(row) {
        if (row.voucherHeadId && row.voucherHeadId > 0)
            this.openVoucher(row);
        else if (row.invoiceId && row.invoiceId > 0 && row.type == 1)
            this.openSupplierInvoice(row.invoiceId);
    }

    protected openVoucher(row) {
        this.messagingService.publish(Constants.EVENT_OPEN_TAB, new TabMessage(this.terms["economy.accounting.voucher.voucher"] + " " + row.voucherNr, row.voucherHeadId, VouchersEditController, { id: row.voucherHeadId }, this.urlHelperService.getGlobalUrl('Economy/Accounting/Vouchers/Views/edit.html')));
    }

    //End of InventoryTraceViewGrid

    // HELP-METHODS
    private new() {
        this.isNew = true;
        this.draft = true;
        this.inventoryId = 0;
        this.inventory = {};
        this.categoryRecords = [];
        this.filesHelper.reset();

        this.inventory.inventoryFiles = [];
        this.inventory.purchaseAmount = this.isModal ? this.purchaseAmount : 0;
        this.inventory.writeOffAmount = this.isModal ? this.purchaseAmount : 0;
        this.inventory.writeOffRemainingAmount = this.isModal ? this.purchaseAmount : 0;
        this.inventory.endAmount = 0;
        this.inventory.writeOffSum = 0;
        this.inventory.categoryIds = [];
        this.inventoryService.getNextInventoryNr().then(x => {
            this.inventory.inventoryNr = x;
        });
        this.selectedWriteOffTemplateId = this.inventoryWriteOffTemplateId;
        this.selectedPurchaseDate = this.isModal ? this.purchaseDate : null;
    }

    private loadWriteOffTemplate(id: number) {

        if (id) {
            this.inventoryService.getInventoryWriteOffTemplate(id).then((x) => {
                this.inventoryWriteOffTemplate = x;
                this.inventory.voucherSeriesTypeId = this.inventoryWriteOffTemplate.voucherSeriesTypeId;
                this.inventory.inventoryWriteOffMethodId = this.inventoryWriteOffTemplate.inventoryWriteOffMethodId;
                this.inventory.periodType = _.filter(this.inventoryWriteOffMethods, i => i.inventoryWriteOffMethodId == this.inventoryWriteOffTemplate.inventoryWriteOffMethodId)[0].periodType;
                this.inventory.periodValue = _.filter(this.inventoryWriteOffMethods, i => i.inventoryWriteOffMethodId == this.inventoryWriteOffTemplate.inventoryWriteOffMethodId)[0].periodValue;
                this.setPeriodValuePercent();
                this.ConvertToInventoryAccountingSettings();
            });
        }
    }

    private setPeriodValuePercent() {
        var percent: number = 0;
        if (this.inventory.periodValue > 0)
            percent = 100 / this.inventory.periodValue;

        var periodName: string = _.find(this.periodTypes, i => i.id == this.inventory.periodType).name;

        var nbrOfYears = 0;

        this.info = percent.round(2) + "% per " + periodName.toLowerCase();

        if (this.inventory.periodType == TermGroup_InventoryWriteOffMethodPeriodType.Period)
            nbrOfYears = this.inventory.periodValue / 12;

        nbrOfYears = nbrOfYears.round(2);

        if (nbrOfYears > 0) {
            this.info += ", " + nbrOfYears.toString() + " " + this.terms["economy.inventory.inventories.years"];
        }
    }

    // VALIDATION

    private showDeleteButton(): boolean {
        return this.isDraftOrActive;
    }

    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.inventory) {
                if (!this.inventory.inventoryNr) {
                    mandatoryFieldKeys.push("economy.inventory.inventories.inventorynr");
                }
                if (!this.inventory.name) {
                    mandatoryFieldKeys.push("common.name");
                }
                if (!this.inventory.purchaseDate) {
                    mandatoryFieldKeys.push("economy.inventory.inventories.purchasedate");
                }
                if (!this.inventory.writeOffDate) {
                    mandatoryFieldKeys.push("economy.inventory.inventories.writeoffdate");
                }
                if (!this.inventory.voucherSeriesTypeId) {
                    mandatoryFieldKeys.push("economy.inventory.inventories.voucherseriestype");
                }
                if (!this.inventory.inventoryWriteOffMethodId) {
                    mandatoryFieldKeys.push("economy.inventory.inventories.writeoffmethod");
                }
            }
        });
    }
}