import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { InventoryService } from "../../../Shared/Economy/Inventory/InventoryService";
import { Feature, SettingMainType, TermGroup, UserSettingType } from "../../../Util/CommonEnumerations";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { IColumnAggregations } from "../../../Util/SoeGridOptionsAg";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { StringUtility } from "../../../Util/StringUtility";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { Constants } from "../../../Util/Constants";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    // Lookups     
    writeOffMethods: ISmallGenericType[];
    inventoryStatus: any[];
    selectableInventoryStatuses = [];
    selectedInventoryStatuses = [];
    categoriesDict: any[] = [];

    // Properties
    private _loadOpen = true;
    get loadOpen() {
        return this._loadOpen;
    }
    set loadOpen(item: boolean) {
        this._loadOpen = item;
        this.loadGridData();
    }

    private _loadClosed = false;
    get loadClosed() {
        return this._loadClosed;
    }
    set loadClosed(item: boolean) {
        this._loadClosed = item;
        this.loadGridData();
    }

    // Toolbar
    toolbarInclude: string;

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private inventoryService: InventoryService,
        private translationService: ITranslationService,
        protected coreService: ICoreService,
        private urlHelperService: IUrlHelperService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory
    ) {
        super(gridHandlerFactory, "Economy.Inventory.Inventories", progressHandlerFactory, messagingHandlerFactory);
        this.setIdColumnNameOnEdit("inventoryId");

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onPermissionsLoaded((feature, readOnly, modify) => {
                this.readPermission = readOnly;
                this.modifyPermission = modify;

                if (this.modifyPermission) {
                    // Send messages to TabsController
                    this.messagingHandler.publishActivateAddTab();
                }
            })
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onDoLookUp(() => this.doLookups())
            .onSetUpGrid(() => this.setupGrid());
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired( () => { this.loadGridData(); });
        }

        this.flowHandler.start({ feature: Feature.Economy_Inventory_Inventories, loadReadPermissions: true, loadModifyPermissions: true });

        this.toolbarInclude = this.urlHelperService.getViewUrl("gridHeader.html");
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg as IGridHandler, () => this.loadGridData());
        this.toolbar.addInclude(this.toolbarInclude);
    }

    private doLookups() {
        return this.$q.all(
            [this.loadUserSettings(), this.loadInventoryWriteOffMethods(), this.loadInventoryStatus()]
        ).then(() => {
            this.loadGridData()
        });
    }

    //SETUP
    protected setupGrid() {

        const keys: string[] = [
            "common.name",
            "common.description",
            "core.edit",
            "economy.inventory.inventories.status",
            "economy.inventory.inventories.inventorynr",
            "economy.inventory.inventories.accountnr",
            "economy.inventory.inventories.accountname",
            "economy.inventory.inventories.writeoffamount",
            "economy.inventory.inventories.writeoffremainingamount",
            "economy.inventory.inventories.purchasedate",
            "economy.inventory.inventories.purchaseamount",
            "economy.inventory.inventories.writeoffsum",
            "economy.inventory.inventories.accwriteoffamount",
            "economy.inventory.inventories.endamount",
            "economy.inventory.inventorywriteoffmethods.inventorywriteoffmethod",
            "economy.inventory.inventories.categories",
        ];

        return this.translationService.translateMany(keys).then((terms) => {

            // Columns
            this.gridAg.addColumnText("inventoryNr", terms["economy.inventory.inventories.inventorynr"], null);
            this.gridAg.addColumnText("name", terms["common.name"], null);
            this.gridAg.addColumnSelect("statusName", terms["economy.inventory.inventories.status"], null, { selectOptions: this.inventoryStatus, toolTipField: "statusName", displayField: "statusName" });
            this.gridAg.addColumnText("inventoryAccountNumberName", terms["economy.inventory.inventories.accountnr"], null);
            this.gridAg.addColumnNumber("writeOffAmount", terms["economy.inventory.inventories.writeoffamount"], null, { decimals: 2 });
            this.gridAg.addColumnNumber("writeOffRemainingAmount", terms["economy.inventory.inventories.writeoffremainingamount"], null, { decimals: 2 });
            this.gridAg.addColumnDate("purchaseDate", terms["economy.inventory.inventories.purchasedate"], null, true);
            this.gridAg.addColumnNumber("purchaseAmount", terms["economy.inventory.inventories.purchaseamount"], null, { decimals: 2 });
            this.gridAg.addColumnNumber("writeOffSum", terms["economy.inventory.inventories.writeoffsum"], null, { decimals: 2 });
            this.gridAg.addColumnNumber("accWriteOffAmount", terms["economy.inventory.inventories.accwriteoffamount"], null, { decimals: 2 });
            this.gridAg.addColumnNumber("endAmount", terms["economy.inventory.inventories.endamount"], null, { decimals: 2 });
            this.gridAg.addColumnText("description", terms["common.description"], null, true);
            this.gridAg.addColumnText("inventoryWriteOffMethod", terms["economy.inventory.inventorywriteoffmethods.inventorywriteoffmethod"], null, true);
            this.gridAg.addColumnText("categories", terms["economy.inventory.inventories.categories"], null, true);
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this));

            this.gridAg.options.addFooterRow("#sum-footer-grid", {
                "writeOffAmount": "sum",
                "writeOffRemainingAmount": "sum",
                "purchaseAmount": "sum",
                "writeOffSum": "sum",
                "accWriteOffAmount": "sum",
                "endAmount": "sum"
            } as IColumnAggregations);

            this.gridAg.finalizeInitGrid("economy.inventory.inventories.inventory", true);
        });
    }

    //LOOKUPS

    protected loadUserSettings(): ng.IPromise<any> {
        const settingTypes: number[] = [UserSettingType.InventoryPreSelectedStatuses];

        return this.coreService.getUserSettings(settingTypes).then(x => {
            const setting = SettingsUtility.getStringUserSetting(x, UserSettingType.InventoryPreSelectedStatuses);
            if (setting) {
                this.selectedInventoryStatuses = [];
                _.forEach(setting.split(','), (id) => {
                    this.selectedInventoryStatuses.push({ id: +id });
                });
            }
        });
    }

    private loadInventoryWriteOffMethods(): ng.IPromise<any> {
        return this.inventoryService.getInventoryWriteOffMethodsDict().then((x) => {
            this.writeOffMethods = x;
        });
    }

    private loadInventoryStatus(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.InventoryStatus, false, false).then((x) => {
            this.inventoryStatus = [];
            this.selectableInventoryStatuses = [];
            _.forEach(x, (row) => {
                this.inventoryStatus.push({ value: row.name, label: row.name });
                this.selectableInventoryStatuses.push({ id: row.id, label: row.name });
            });
        });
    }

    public loadGridData() {
        const statuses = this.getStatusesString();
        if (statuses === Constants.WEBAPI_STRING_EMPTY) {
            this.setData(undefined);
            return;
        }

        // Load data
        this.progress.startLoadingProgress([() => {
            return this.inventoryService.getInventories(this.getStatusesString()).then((data: any[]) => {
                data.forEach( (item, i:number) => {
                    item.inventoryAccountNumberName = item.inventoryAccountNr + " " + item.inventoryAccountName;
                    item.accWriteOffAmount = item.writeOffAmount - item.writeOffRemainingAmount;
                    item.inventoryWriteOffMethod = _.filter(this.writeOffMethods, m => m.id === item.inventoryWriteOffMethodId)[0].name;
                    if (item.purchaseDate)
                        item.purchaseDate = new Date(item.purchaseDate).date();
                });

                this.setData(data);
            });
        }]);
    }

    private statusSelectionComplete() {
        const statuses = this.getStatusesString();
        this.coreService.saveStringSetting(SettingMainType.User, UserSettingType.InventoryPreSelectedStatuses, statuses === Constants.WEBAPI_STRING_EMPTY ? "" : statuses);

        this.loadGridData();
    }

    private getStatusesString(): string {
        return StringUtility.getCollectionIdsStr(_.filter(this.selectedInventoryStatuses, (s) => s.id > 0));
    }
}
