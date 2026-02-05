import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { Feature } from "../../../Util/CommonEnumerations";
import { IStockService } from "../../../Shared/Billing/Stock/StockService";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {
    //@ngInject
    constructor(
        private stockService: IStockService,
        private translationService: ITranslationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {
        super(gridHandlerFactory, "Soe.Billing.Stock.Stocks", progressHandlerFactory, messagingHandlerFactory);

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

        this.flowHandler.start({ feature: Feature.Billing_Stock, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg, () => this.loadGridData());
    }

    public setupGrid() {
        // Columns
        var keys: string[] = [
            "common.code",
            "common.name",
            "core.edit",
            "billing.stock.stocks.isexternal"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.gridAg.addColumnText("code", terms["common.code"], null);
            this.gridAg.addColumnText("name", terms["common.name"], null);
            this.gridAg.addColumnBool("isExternal", terms["billing.stock.stocks.isexternal"], null);
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this));

            this.gridAg.finalizeInitGrid("billing.stock.stocks.stocks", true);
        });
    }

    public loadGridData() {
        this.progress.startLoadingProgress([() => {
            return this.stockService.getStocks(false).then((x) => {
                this.setData(x);
            });
        }]);
    }

}