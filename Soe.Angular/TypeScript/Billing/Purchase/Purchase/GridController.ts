import { SelectEmailController } from "../../../Common/Dialogs/SelectEmail/SelectEmailController";
import { SelectReportController } from "../../../Common/Dialogs/SelectReport/SelectReportController";
import { PurchaseGridDTO } from "../../../Common/Models/PurchaseDTO";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IProgressHandler } from "../../../Core/Handlers/ProgressHandler";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { IReportService } from "../../../Core/Services/ReportService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { IPurchaseService } from "../../../Shared/Billing/Purchase/Purchase/PurchaseService";
import { CompanySettingType, EmailTemplateType, Feature, PurchaseDeliveryStatus, SettingMainType, SoeOriginStatus, SoeReportTemplateType, SoeStatusIcon, TermGroup, UserSettingType } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { PurchaseButtonFunctions } from "../../../Util/Enumerations";
import { FlaggedEnum } from "../../../Util/EnumerationsUtility";
import { HtmlUtility } from "../../../Util/HtmlUtility";
import { SettingsUtility } from "../../../Util/SettingsUtility";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {
    gridFooterComponentUrl: string;

    //Settings
    private defaultEmailTemplatePurchase: number;
    private defaultReportTemplatePurchase: number;

    // Collections
    private allItemsSelectionDict: any[];

    private _allItemsSelection: any;
    get allItemsSelection() {
        return this._allItemsSelection;
    }
    set allItemsSelection(item: any) {
        this._allItemsSelection = item;
        this.updateItemsSelection();
    }

    private _loadOpen = false;
    get loadOpen() {
        return this._loadOpen;
    }
    set loadOpen(item: boolean) {
        this._loadOpen = item;
        this.reloadGridFromFilter();
    }

    private _loadClosed = false;
    get loadClosed() {
        return this._loadClosed;
    }
    set loadClosed(item: boolean) {
        this._loadClosed = item;
        this.reloadGridFromFilter();
    }

    private buttonFunctions = [];
    private purchaseStatus: ISmallGenericType[] = [];
    private selectedPurchaseStatus: ISmallGenericType[] = [];
  
    // Flags
    activated: boolean;
    doReload: boolean;

    //@ngInject
    constructor(
        private $scope,
        private $uibModal,
        private $q: ng.IQService,
        private $window,
        private coreService: ICoreService,
        private reportService: IReportService,
        private purchaseService: IPurchaseService,
        private urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        private messagingService: IMessagingService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {
        super(gridHandlerFactory, "Billing.Purchase.Purchase", progressHandlerFactory, messagingHandlerFactory);
        this.setIdColumnNameOnEdit("purchaseId");
        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            //.onLoadGridData(() => this.loadGridData())
            .onDoLookUp( () => this.onDoLookups())
            .onSetUpGrid(() => this.setupGrid());

        this.onTabActivetedAndModified(() => this.loadGridData());
    }

    public onInit(parameters: any) {
        this.parameters = parameters;
        this.guid = parameters.guid;
        this.isHomeTab = !!parameters.isHomeTab;

        this.gridFooterComponentUrl = this.urlHelperService.getViewUrl("gridFooter.html");

        // Set selections
        this._loadOpen = true;
        this._loadClosed = false;

        this.messagingService.subscribe(Constants.EVENT_TAB_ACTIVATED, (x) => {
            this.onControllActivated(x);
        });

        this.$scope.$on('onTabActivated', (e, a) => {
            this.onControllActivated(a);
        });
    }

    public onControllActivated(tabGuid: any) {
        if (tabGuid !== this.guid)
            return;

        if (!this.activated) {
            this.flowHandler.start([
                { feature: Feature.Billing_Purchase_Purchase_List, loadReadPermissions: true, loadModifyPermissions: true },
                { feature: Feature.Billing_Purchase_Purchase_Edit, loadReadPermissions: true, loadModifyPermissions: true }
            ]);
            this.activated = true;
        }
        else if (this.doReload) {
            this.loadGridData();
            this.doReload = false;
        }
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Billing_Purchase_Purchase_List].modifyPermission;
        this.modifyPermission = response[Feature.Billing_Purchase_Purchase_Edit].modifyPermission;
        if (this.modifyPermission) {
            this.messagingHandler.publishActivateAddTab();
        }
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg as IGridHandler, () => this.reloadGridFromFilter());
        this.toolbar.addInclude(this.urlHelperService.getViewUrl("gridHeader.html"));
    }

    private onDoLookups(): ng.IPromise<any> {
        return this.$q.all([this.loadSelectionTypes(), this.loadCompanySettings(), this.loadUserSettings(), this.loadPurchaseStatus()]);
    }

    private loadPurchaseStatus(): ng.IPromise<any> {
        const defaultStatusList = [SoeOriginStatus.Origin, SoeOriginStatus.PurchaseDone, SoeOriginStatus.PurchaseSent, SoeOriginStatus.PurchaseAccepted];
        this.purchaseStatus = [];
        this.selectedPurchaseStatus = [];

        return this.purchaseService.getPurchaseStatus().then(data => {
            this.purchaseStatus = data;
            _.forEach(this.purchaseStatus, (ps: any) => {
                var defaultSelection = _.find(defaultStatusList, s => s === ps.id);
                if (defaultSelection) {
                    this.selectedPurchaseStatus.push(ps);
                }                
            });

        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        const settingTypes: number[] = [CompanySettingType.BillingDefaultEmailTemplatePurchase, CompanySettingType.BillingDefaultPurchaseOrderReportTemplate];
        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.defaultEmailTemplatePurchase = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingDefaultEmailTemplatePurchase);
            this.defaultReportTemplatePurchase = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingDefaultPurchaseOrderReportTemplate);
        });
    }

    private loadUserSettings(): ng.IPromise<any> {
        const settingTypes: number[] = [UserSettingType.BillingPurchaseAllItemsSelection];
        return this.coreService.getUserSettings(settingTypes).then(x => {
            this.allItemsSelection = SettingsUtility.getIntUserSetting(x, UserSettingType.BillingPurchaseAllItemsSelection, 1, false);
        });
    }

    private loadSelectionTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.ChangeStatusGridAllItemsSelection, false, true, true).then((x) => {
            this.allItemsSelectionDict = x;
        });
    }

    public executeButtonFunction(option) {
        const purchaseIds: number[] = this.gridAg.options.getSelectedRows().map(r => r.purchaseId);
        if (purchaseIds.length === 0 || !option)
            return;


        switch (option.id) {
            case PurchaseButtonFunctions.Print:
                this.printPurchases(purchaseIds);
                break;
            case PurchaseButtonFunctions.SendAsEmail:
                this.sendPurchasesAsEmail(purchaseIds);
                break;
        }

    }

    private sendPurchasesAsEmail(purchaseIds) {
        this.translationService.translateMany(["billing.purchase.list.purchase"])
            .then(types => {
            return this.reportService.getReportsForType([SoeReportTemplateType.PurchaseOrder], true, false)
                .then(reports => {

                    const reportsSmall = reports.map(r => { return { id: r.reportId, name: `${r.reportNr} ${r.reportName}` } });
                    const modal = this.$uibModal.open({
                        templateUrl: this.urlHelperService.getGlobalUrl("Common/Dialogs/SelectEmail/SelectEmail.html"),
                        controller: SelectEmailController,
                        controllerAs: 'ctrl',
                        backdrop: 'static',
                        size: 'lg',
                        resolve: {
                            translationService: () => this.translationService,
                            coreService: () => this.coreService,
                            defaultEmail: () => null,
                            defaultEmailTemplateId: () => this.defaultEmailTemplatePurchase,
                            recipients: () => null,
                            attachments: () => null,
                            attachmentsSelected: () => null,
                            checklists: () => null,
                            types: () => types,
                            grid: () => true,
                            type: () => EmailTemplateType.PurchaseOrder,
                            showReportSelection: () => true,
                            reports: () => reportsSmall,
                            defaultReportTemplateId: () => this.defaultReportTemplatePurchase,
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
                                var recipients = _.filter(result.recipients, r => r.id > 0);
                                var singleRecipient = _.find(result.recipients, r => r.name);
                                this.purchaseService.sendPurchasesAsEmail(purchaseIds, result.emailTemplateId, result.languageId)
                                    .then(res => {
                                        if (res.success) {
                                            completion.completed(null, false, terms["common.sent"]);
                                        }
                                    })
                            });

                        });
                    })
            })
        })
    }

    private printPurchases(purchaseIds) {
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
                showEmail: () => false,
                copyValue: () => false,
                reports: () => null,
                defaultReportId: () => null,
                langId: () => -1,
                showReminder: () => false,
                showLangSelection: () => true,
                showSavePrintout: () => false,
                savePrintout: () => false
            }
        });

        modal.result.then((result: any) => {
            if ((result) && (result.reportId)) {
                this.reportService.getPurchaseOrderPrintUrl(purchaseIds, null, result.reportId, result.languageId)
                    .then((url) => {
                        HtmlUtility.openInSameTab(this.$window, url);
                    });
            }
        });
    }

    public setupGrid() {
        // Columns
        const keys: string[] = [
            "billing.purchase.purchasenr",
            "billing.purchase.supplierno",
            "billing.purchase.suppliername",
            "billing.purchase.purchasedate",
            "billing.purchase.purchasestatus",
            "billing.purchase.deliverydate",
            "billing.purchase.confirmeddate",
            "billing.purchase.sendasemail",
            "billing.project.project",
            "billing.purchaserow.totalexvat",
            "core.edit",
            "core.print",
            "common.currency",
            "billing.purchase.origindescription",
            "billing.purchase.deliverystatus",
            "billing.purchase.foreignamount",
            "common.status"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.gridAg.addColumnText("purchaseNr", terms["billing.purchase.purchasenr"], null);
            this.gridAg.addColumnText("projectNr", terms["billing.project.project"], null, true, { enableHiding: true });
            this.gridAg.addColumnText("supplierNr", terms["billing.purchase.supplierno"], null);
            this.gridAg.addColumnText("supplierName", terms["billing.purchase.suppliername"], null);
            this.gridAg.addColumnText("origindescription", terms["billing.purchase.origindescription"], null, true, { enableHiding: true});
            this.gridAg.addColumnText("statusName", terms["billing.purchase.purchasestatus"], null);
            this.gridAg.addColumnNumber("totalAmountExVat", terms["billing.purchaserow.totalexvat"], null, { decimals: 2 });
            this.gridAg.addColumnNumber("totalAmountExVatCurrency", terms["billing.purchase.foreignamount"], null, { decimals: 2, enableHiding:true,hide:true });
            this.gridAg.addColumnText("currencyCode", terms["common.currency"], null, true, { enableHiding: true, hide: true });
            this.gridAg.addColumnIcon("deliveryStatusIcon", terms["common.status"], 30, { suppressSorting: false, enableHiding: true, toolTipField: "deliveryStatusText", showTooltipFieldInFilter: true });
            this.gridAg.addColumnDate("purchaseDate", terms["billing.purchase.purchasedate"], null);
            this.gridAg.addColumnDate("deliveryDate", terms["billing.purchase.deliverydate"], null);
            this.gridAg.addColumnDate("confirmedDate", terms["billing.purchase.confirmeddate"], null);
            this.gridAg.addColumnIcon("statusIconValue", null, null, { showIcon: this.showStatusIcon.bind(this), toolTipField: "statusIconMessage", suppressSorting: false, showTooltipFieldInFilter: true });

            if (this.modifyPermission) {
                this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this));
            }
            this.gridAg.finalizeInitGrid("billing.purchase.list.purchase", true);

            this.buttonFunctions.push({
                id: PurchaseButtonFunctions.Print, name: terms["core.print"]
            });
            this.buttonFunctions.push({
                id: PurchaseButtonFunctions.SendAsEmail, name: terms["billing.purchase.sendasemail"]
            });

            this.gridAg.options.getColumnDefs().forEach(f => {
                let cellcls: string = f.cellClass ? f.cellClass.toString() : "";
                f.cellClass = (grid: any) => {                        
                   return cellcls + " " + f.deliveryStatusIcon; 
                };
            });
        });
    }

    public showStatusIcon(row: any): boolean {
        return row.statusIconValue;
    }

    public updateItemsSelection() {
        this.coreService.saveIntSetting(SettingMainType.User, UserSettingType.BillingPurchaseAllItemsSelection, this.allItemsSelection);
        this.reloadGridFromFilter();
    }

    public reloadGridFromFilter = _.debounce(() => {
        this.loadGridData();
    }, 1000, { leading: false, trailing: true });

    public loadGridData() {
        let selectedPurchaseStatusIds = [];
        _.forEach(this.selectedPurchaseStatus, (ps: any) => {            
            selectedPurchaseStatusIds.push(ps.id);            
        });

        this.purchaseService.getPurchaseOrders(this.allItemsSelection, selectedPurchaseStatusIds).then((data: PurchaseGridDTO[]) => {
            
            this.setInformationIconAndTooltip(data);
            
            this.setData(data);
        });
    }

    public setInformationIconAndTooltip(rows: PurchaseGridDTO[]) {

        const lateText = this.translationService.translateInstant("billing.purchase.late");
        const theRestText = this.purchaseStatus.find(x => x.id === SoeOriginStatus.Origin).name + "/" +
            this.purchaseStatus.find(x => x.id === SoeOriginStatus.PurchaseDone).name + "/" +
            this.purchaseStatus.find(x => x.id === SoeOriginStatus.PurchaseSent).name;
        const mailText = this.translationService.translateInstant("common.customer.invoices.emailsent"); 

        // Get status icons
        const flaggedEnum: FlaggedEnum.IFlaggedEnum = FlaggedEnum.create(SoeStatusIcon, SoeStatusIcon.ElectronicallyDistributed + 1);
        
        rows.forEach(row => {
            switch (row.deliveryStatus) {
                case PurchaseDeliveryStatus.Delivered:
                    row.deliveryStatusIcon = "fas fa-circle okColor";
                    row["deliveryStatusText"] = row.statusName;
                    break;
                case PurchaseDeliveryStatus.PartlyDelivered:
                    row.deliveryStatusIcon = "fas fa-circle infoColor";
                    row["deliveryStatusText"] = row.statusName;
                    break;
                case PurchaseDeliveryStatus.Accepted:
                    row.deliveryStatusIcon = "fas fa-circle yellowColor";
                    row["deliveryStatusText"] = row.statusName;
                    break;
                case PurchaseDeliveryStatus.Late:
                    row.deliveryStatusIcon = "fas fa-circle errorColor";
                    row["deliveryStatusText"] = lateText;
                    break;
                default:
                    row.deliveryStatusIcon = "fas fa-circle mediumGrayColor";
                    row["deliveryStatusText"] = theRestText;
                    break;
            }

            const statusIcons: FlaggedEnum.IFlaggedEnum = new flaggedEnum(row.statusIcon);
            const statusIconArray = statusIcons.toNumbersArray();

            if (_.includes(statusIconArray, SoeStatusIcon.Email)) {
                row.statusIconValue = "fal fa-envelope";
                row.statusIconMessage = mailText;
            }
        });
    }

    public onPurchaseStatusSelectionChanged() {
        this.reloadGridFromFilter();
    }
}