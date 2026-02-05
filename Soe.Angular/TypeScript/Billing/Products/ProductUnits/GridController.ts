import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IProductService } from "../../../Shared/Billing/Products/ProductService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { Feature } from "../../../Util/CommonEnumerations";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    //@ngInject
    constructor(
        private productService: IProductService,
        private translationService: ITranslationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {
        super(gridHandlerFactory, "Billing.Products.ProductUnits", progressHandlerFactory, messagingHandlerFactory)

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onPermissionsLoaded((feature, readOnly, modify) => {
                this.readPermission = readOnly;
                this.modifyPermission = modify

                if (this.modifyPermission) {
                    this.messagingHandler.publishActivateAddTab();
                }
            })
            .onSetUpGrid(() => this.setUpGrid())
            .onLoadGridData(() => this.loadGridData())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    public onInit(parameters: any) {
        this.messagingHandler.onGridDataReloadRequired(x => { this.loadGridData(true); });

        this.flowHandler.start({ feature: Feature.Billing_Preferences_ProductSettings_ProductUnit_Edit, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg, () => this.loadGridData());
    }

    public loadGridData(refreshCache = false) {
        this.gridAg.clearData();

        this.progress.startLoadingProgress([() => {
            return this.productService.getProductUnits(refreshCache).then((x) => {
                this.setData(x);
            });
        }]);
    }

    private setUpGrid() {
        const translationKeys: string[] = [
            "common.code",
            "common.name",
            "core.edit"
        ];

        this.translationService.translateMany(translationKeys).then((terms) => {
            this.gridAg.addColumnText("code", terms["common.code"], null, true);
            this.gridAg.addColumnText("name", terms["common.name"], null, true);
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this));

            this.gridAg.finalizeInitGrid("billing.product.productunit.productunits", true)
        });
    }

    edit(row) {
        if (this.doubleClickToEdit && (this.readPermission || this.modifyPermission))
            this.messagingHandler.publishEditRow(row);
    }
}