import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { InventoryService } from "../../../Shared/Economy/Inventory/InventoryService";
import { Feature, TermGroup  } from "../../../Util/CommonEnumerations";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {
    periodTypes: any[];
    writeOffTypes: any[];
    writeOffMethods: any;

    //@ngInject
    constructor(
        private inventoryService: InventoryService,
        private translationService: ITranslationService,
        private coreService: ICoreService,
        private $q: ng.IQService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory
    ) {
        super(gridHandlerFactory, "Economy.Inventory.InventoryWriteOffMethods", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onPermissionsLoaded((feature, readOnly, modify) => {
                this.readPermission = readOnly;
                this.modifyPermission = modify

                if (this.modifyPermission) {
                    // Send messages to TabsController
                    this.messagingHandler.publishActivateAddTab();
                }
            })
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.loadGridData(); });
        }

        this.flowHandler.start({ feature: Feature.Economy_Inventory_WriteOffMethods, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg, () => this.loadGridData());
    }

    public setupGrid() {
        // Columns
        var keys: string[] = [
            "common.name",
            "common.description",
            "economy.inventory.inventorywriteoffmethod.periodvalue",
            "economy.inventory.inventorywriteoffmethod.type",
            "economy.inventory.inventorywriteoffmethod.periodtype",
            "core.edit"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.gridAg.addColumnText("name", terms["common.name"], null, false);
            this.gridAg.addColumnText("description", terms["common.description"], null, false);
            this.gridAg.addColumnText("periodValue", terms["economy.inventory.inventorywriteoffmethod.periodvalue"], null, false);
            this.gridAg.addColumnText("periodTypeName", terms["economy.inventory.inventorywriteoffmethod.periodtype"], null, false);
            this.gridAg.addColumnText("typeName", terms["economy.inventory.inventorywriteoffmethod.type"], null, false);

            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this));
        });

        this.gridAg.finalizeInitGrid("economy.inventory.inventorywriteoffmethods.inventorywriteoffmethod", true);
    }

    private loadPeriodTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.InventoryWriteOffMethodPeriodType, false, false).then((x) => {
            this.periodTypes = x || [];
        });
    }

    private loadWriteOffTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.InventoryWriteOffMethodType, false, false).then((x) => {
            this.writeOffTypes = x || [];
        });
    }

    private loadWriteOffMethods(): ng.IPromise<any> {
        return this.inventoryService.getInventoryWriteOffMethods().then((x) => {
            this.writeOffMethods = x || [];
        })
    }

    public loadGridData() {
        // Load data

        this.$q.all([
            this.loadPeriodTypes(),
            this.loadWriteOffTypes(),
            this.loadWriteOffMethods()
        ]).then(() => {
            this.writeOffMethods.forEach(m => {
                m.periodTypeName = this.periodTypes.find(t => t.id == m.periodType).name || "";
                m.typeName = this.writeOffTypes.find(w => w.id == m.type).name || "";
            })
            this.setData(this.writeOffMethods)
        })
    }

    edit(row) {
        // Send message to TabsController
        if (this.doubleClickToEdit && (this.readPermission || this.modifyPermission))
            this.messagingHandler.publishEditRow(row);
    }
}