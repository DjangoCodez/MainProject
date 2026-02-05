import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { ISupplierService } from "../../../Shared/Economy/Supplier/SupplierService";
import { Feature } from "../../../Util/CommonEnumerations";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    //@ngInject
    constructor(
        private supplierService: ISupplierService,
        private translationService: ITranslationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {
        super(gridHandlerFactory, "Economy.Supplier.AttestGroups", progressHandlerFactory, messagingHandlerFactory);
        this.useRecordNavigatorInEdit('attestWorkFlowHeadId', 'name');
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

            this.onTabActivetedAndModified(() => { this.loadGridData(false); });
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.guid = parameters.guid;
        this.isHomeTab = !!parameters.isHomeTab;
        
        this.flowHandler.start({ feature: Feature.Economy_Preferences_SuppInvoiceSettings_AttestGroups, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg, () => this.loadGridData(false));
    }

    public setupGrid() {
        // Columns
        var keys: string[] = [
            "common.code",
            "common.name",
            "core.edit"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.gridAg.addColumnText("code", terms["common.code"], 30);
            this.gridAg.addColumnText("name", terms["common.name"], null);
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this));

            this.gridAg.finalizeInitGrid("economy.supplier.attestgroup.attestgroups", true);
        });
    }

    public loadGridData(useCache: boolean = true) {
        
        // Load data
        this.progress.startLoadingProgress([() => {
            return this.supplierService.getAttestWorkFlowGroups(useCache).then((x) => {
                this.setData(x);
            });
        }]);
    }

}
