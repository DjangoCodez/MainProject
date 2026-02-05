import { GridControllerBase2Ag } from "../../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../../Core/ICompositionGridController";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IControllerFlowHandlerFactory } from "../../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../../Core/Handlers/GridHandlerFactory";
import { IGridHandler } from "../../../../Core/Handlers/GridHandler";
import { IToolbarFactory } from "../../../../Core/Handlers/ToolbarFactory";
import { Feature, TermGroup_AttestEntity } from "../../../../Util/CommonEnumerations";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { ISigningService } from "../../SigningService";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    //@ngInject
    constructor(
        private translationService: ITranslationService,
        private urlHelperService: IUrlHelperService,
        private $filter: ng.IFilterService,
        private signingService: ISigningService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        private $uibModal) {

        super(gridHandlerFactory, "Manage.Signing.Document.Templates", progressHandlerFactory, messagingHandlerFactory);  

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onPermissionsLoaded((feature, readOnly, modify) => {
                this.readPermission = readOnly;
                this.modifyPermission = modify;

                if (this.modifyPermission) {
                    // Send messages to TabsController
                    this.messagingHandler.publishActivateAddTab();
                }
            })            
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    public onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;
        this.guid = this.parameters.guid;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.reloadData(); });
        }

        this.flowHandler.start({ feature: Feature.Manage_Signing_Document_Templates, loadReadPermissions: true, loadModifyPermissions: true });
    }    

    public edit(row: any) {
        // Send message to TabsController
        if (this.readPermission || this.modifyPermission)
            this.messagingHandler.publishEditRow(row);
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.reloadData());
    }

    public setupGrid() {
        this.gridAg.options.enableRowSelection = false;

        let keys: string[] = [
            "common.name",
            "common.description",
            "core.edit"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.gridAg.addColumnText("name", terms["common.name"], null);
            this.gridAg.addColumnText("description", terms["common.description"], null);
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this))

            this.gridAg.finalizeInitGrid("manage.signing.template.templates", true);
        });
    }

    public loadGridData() {
        // Load data
        this.signingService.getAttestWorkFlowTemplates(TermGroup_AttestEntity.SigningDocument).then((x) => {
            this.gridAg.setData(x);
        });
    }

    private reloadData() {
        this.loadGridData();
    }
}
