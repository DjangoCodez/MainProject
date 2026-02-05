import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IToolbar } from "../../../Core/Handlers/Toolbar";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IPurchaseDeliveryGridDTO, IPurchaseGridDTO } from "../../../Scripts/TypeLite.Net4";
import { IPurchaseService } from "../../../Shared/Billing/Purchase/Purchase/PurchaseService";
import { Feature, SettingMainType, SoeOriginStatus, TermGroup, TermGroup_ChangeStatusGridAllItemsSelection, UserSettingType } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { SoeGridOptionsEvent } from "../../../Util/Enumerations";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { GridEvent } from "../../../Util/SoeGridOptions";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    private allItemsSelectionDict: any[];

    private _allItemsSelection: any;
    private selectedPurchaseStatus: number[] = [];

    get allItemsSelection() {
        return this._allItemsSelection;
    }
    set allItemsSelection(item: any) {
        this._allItemsSelection = item;
        this.updateItemsSelection();
    }

    // Permissions
    purchaseEditPermission: boolean;

    // Flags
    progressBusy: boolean;
    awaitingDelivery: boolean;
    activated: boolean;
    doReload: boolean;

    //@ngInject
    constructor(
        private $scope,
        private $q: ng.IQService,
        private coreService: ICoreService,
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
            .onDoLookUp(() => this.onDoLookups())
            .onSetUpGrid(() => this.setupGrid());

        this.onTabActivetedAndModified(() => this.loadGridData());
    }

    public onInit(parameters: any) {
        this.parameters = parameters;
        this.guid = parameters.guid;
        this.isHomeTab = !!parameters.isHomeTab;
        this.awaitingDelivery = parameters.awaitingDelivery;

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
                { feature: Feature.Billing_Purchase_Delivery_List, loadReadPermissions: true, loadModifyPermissions: true },
                { feature: Feature.Billing_Purchase_Delivery_Edit, loadReadPermissions: true, loadModifyPermissions: true },
                { feature: Feature.Billing_Purchase_Purchase_Edit, loadReadPermissions: false, loadModifyPermissions: true }
            ]);
            this.activated = true;
        }
        else if (this.doReload) {
            this.loadGridData();
            this.doReload = false;
        }
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Billing_Purchase_Delivery_List].modifyPermission;
        this.modifyPermission = response[Feature.Billing_Purchase_Delivery_Edit].modifyPermission;
        this.purchaseEditPermission = response[Feature.Billing_Purchase_Purchase_Edit].modifyPermission;
        if (this.modifyPermission) {
            this.messagingHandler.publishActivateAddTab();
        }
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg as IGridHandler, () => this.reloadGridFromFilter());
        this.toolbar.addInclude(this.urlHelperService.getViewUrl("gridHeader.html"));
    }

    private onDoLookups(): ng.IPromise<any> {
        return this.$q.all([this.loadSelectionTypes(), this.loadUserSettings(), this.loadPurchaseStatus()]);
    }

    private loadPurchaseStatus(): ng.IPromise<any> {     
        this.selectedPurchaseStatus = [];
        return this.purchaseService.getPurchaseStatus().then(data => {           
            _.forEach(data, (row) => {
                if (!(row.id == SoeOriginStatus.PurchaseDeliveryCompleted || row.id == SoeOriginStatus.Origin)) {
                    this.selectedPurchaseStatus.push(row.id);
                }
            });
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

    public setupGrid() {
        // Columns
        const keys: string[] = [
            "billing.purchase.purchasenr",
            "billing.purchase.delivery.deliveryno",
            "billing.purchase.delivery.deliverydate",
            "billing.purchase.supplierno",
            "billing.purchase.suppliername",
            "billing.purchase.delivery.createddate",
            "billing.purchase.purchasedate",
            "billing.purchase.delivery.new_delivery",
            "core.edit",
            "billing.purchase.origindescription"
        ];

        this.translationService.translateMany(keys).then((terms) => {

            this.doubleClickToEdit = false;

            if (this.awaitingDelivery) {
                this.gridAg.addColumnText("purchaseNr", terms["billing.purchase.purchasenr"], null);
                this.gridAg.addColumnText("supplierNr", terms["billing.purchase.supplierno"], null);
                this.gridAg.addColumnText("supplierName", terms["billing.purchase.suppliername"], null);
                this.gridAg.addColumnDate("purchaseDate", terms["billing.purchase.purchasedate"], null);
                this.gridAg.addColumnText("origindescription", terms["billing.purchase.origindescription"], null);

                if (this.modifyPermission)
                    this.gridAg.addColumnIcon(null, "", null, { icon: "fal fa-plus", onClick: this.createDelivery.bind(this), toolTip: terms["billing.purchase.delivery.new_delivery"] });

                if (this.purchaseEditPermission) 
                    this.gridAg.addColumnEdit(terms["core.edit"], this.editRow.bind(this));

                this.gridAg.finalizeInitGrid("billing.purchase.list.purchase", true);
            }
            else {
                this.gridAg.addColumnText("deliveryNr", terms["billing.purchase.delivery.deliveryno"], null);
                this.gridAg.addColumnDate("deliveryDate", terms["billing.purchase.delivery.deliverydate"], null);
                this.gridAg.addColumnText("supplierNr", terms["billing.purchase.supplierno"], null);
                this.gridAg.addColumnText("supplierName", terms["billing.purchase.suppliername"], null);
                this.gridAg.addColumnText("purchaseNr", terms["billing.purchase.purchasenr"], null);
                this.gridAg.addColumnDate("created", terms["billing.purchase.delivery.createddate"], null);

                if (this.modifyPermission) 
                    this.gridAg.addColumnEdit(terms["core.edit"], this.editRow.bind(this));

                this.gridAg.finalizeInitGrid("billing.purchase.delivery.delivery", true);
            }

            const events: GridEvent[] = [];
            events.push(new GridEvent(SoeGridOptionsEvent.RowDoubleClicked, (row) => { this.editRow(row); }));
            this.gridAg.options.subscribe(events);
        });
    }

    public editRow(row) {
        if ((this.awaitingDelivery && this.purchaseEditPermission) || (!this.awaitingDelivery && this.modifyPermission))
            this.messagingHandler.publishEditRow({ isDelivery: !this.awaitingDelivery, row: row, createDelivery: false });
    }

    public createDelivery(row) {
        if (this.modifyPermission)
            this.messagingHandler.publishEditRow({ isDelivery: !this.awaitingDelivery, row: row, createDelivery: true });
    }

    public updateItemsSelection() {
        this.coreService.saveIntSetting(SettingMainType.User, UserSettingType.BillingPurchaseAllItemsSelection, this.allItemsSelection)
        this.reloadGridFromFilter();
    }

    public reloadGridFromFilter = _.debounce(() => {
        this.loadGridData();
    }, 1000, { leading: false, trailing: true });

    public loadGridData() {
        return this.progress.startLoadingProgress([
            () => {
                if (this.awaitingDelivery) {

                    return this.purchaseService.getPurchaseOrders(TermGroup_ChangeStatusGridAllItemsSelection.All, this.selectedPurchaseStatus).then((data: IPurchaseGridDTO[]) => {
                        this.setData(data);
                    });
                }
                else {
                    return this.purchaseService.getDeliveries(this.allItemsSelection).then((data: IPurchaseDeliveryGridDTO[]) => {
                        this.setData(data);
                    });
                }
            }
        ]);
        
    }
}