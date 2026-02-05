import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { AccountingService } from "../../../Shared/Economy/Accounting/AccountingService";
import { InventoryService } from "../../../Shared/Economy/Inventory/InventoryService";
import { Feature } from "../../../Util/CommonEnumerations";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";


export class GridController extends GridControllerBase2Ag implements ICompositionGridController {
    voucherSeries: any;
    writeOffMethods: any;
    writeOffTemplates: any;

    //@ngInject
    constructor(
        private inventoryService: InventoryService,
        private translationService: ITranslationService,
        private accountingService: AccountingService,
        private $timeout: ng.ITimeoutService,
        private $q: ng.IQService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {

        super(gridHandlerFactory, "Economy.Inventory.InventoryWriteOffTemplates", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onPermissionsLoaded((feature, readOnly, modify) => {
                this.readPermission = readOnly;
                this.modifyPermission = modify

                if (this.modifyPermission) {
                    // Send messages to TabsController
                    this.messagingHandler.publishActivateAddTab();
                }
            })
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData())
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.loadGridData(); });
        }

        this.flowHandler.start({ feature: Feature.Economy_Inventory_WriteOffTemplates, loadReadPermissions: true, loadModifyPermissions: true });
    }

    public onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.loadGridData());
    }

    public setupGrid() {
        // Columns
        var keys: string[] = [
            "common.name",
            "common.description",
            "economy.inventory.inventorywriteofftemplate.writeoffmethod",
            "economy.inventory.inventorywriteofftemplate.voucherserie",
            "core.edit"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.gridAg.addColumnText("name", terms["common.name"], null, false);
            this.gridAg.addColumnText("description", terms["common.description"], null, false);
            this.gridAg.addColumnText("inventoryWriteOffName", terms["economy.inventory.inventorywriteofftemplate.writeoffmethod"], null, false);
            this.gridAg.addColumnText("voucherSeriesName", terms["economy.inventory.inventorywriteofftemplate.voucherserie"], null, false);
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this));

            this.gridAg.finalizeInitGrid("economy.inventory.inventorywriteofftemplates.inventorywriteofftemplate", true);
        });
    }

    private loadVoucherSeries(): ng.IPromise<any> {
        return this.accountingService.getVoucherSeriesTypes().then((x) => {
            this.voucherSeries = x || [];
        });
    }

    private loadWriteOffMethods(): ng.IPromise<any> {
        return this.inventoryService.getInventoryWriteOffMethodsDict().then((x) => {
            this.writeOffMethods = x || [];
        });
    }

    private loadWriteOffTemplates(): ng.IPromise<any> {
        return this.inventoryService.getInventoryWriteOffTemplates(false).then((x) => {
            this.writeOffTemplates = x || this.writeOffTemplates || [];
        });
    }


    public loadGridData() {
        //Load data
        this.$q.all([
            this.loadVoucherSeries(),
            this.loadWriteOffMethods(),
            this.loadWriteOffTemplates()
        ]).then(() => {
            this.writeOffTemplates.forEach(t => {
                t.inventoryWriteOffName = this.writeOffMethods.find(m => t.inventoryWriteOffMethodId == m.id).name || "";
                t.voucherSeriesName = this.voucherSeries.find(v => t.voucherSeriesTypeId == v.voucherSeriesTypeId).name || "";
            })
            this.setData(this.writeOffTemplates)
        })
    }

    edit(row) {
        // Send message to TabsController
        if (this.doubleClickToEdit && (this.readPermission || this.modifyPermission))
            this.messagingHandler.publishEditRow(row);
    }
}
