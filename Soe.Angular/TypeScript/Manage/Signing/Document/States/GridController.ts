import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { ISigningService } from "../../SigningService";
import { Feature, SoeModule, TermGroup_AttestEntity } from "../../../../Util/CommonEnumerations";
import { Constants } from "../../../../Util/Constants";
import { IPermissionRetrievalResponse } from "../../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../../Core/Handlers/ToolbarFactory";
import { IGridHandler } from "../../../../Core/Handlers/GridHandler";
import { IGridHandlerFactory } from "../../../../Core/Handlers/gridhandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../../Core/Handlers/controllerflowhandlerfactory";
import { IProgressHandlerFactory } from "../../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../../Core/Handlers/messaginghandlerfactory";
import { GridControllerBase2Ag } from "../../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../../Core/ICompositionGridController";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    // Filters
    signingEntityFilterOptions = [];
    signingEntities: any[] = [];
    signingStates: any[] = [];

    //@ngInject
    constructor(
        private signingService: ISigningService,
        gridHandlerFactory: IGridHandlerFactory,
        private translationService: ITranslationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory) {

        super(gridHandlerFactory, "Manage.Signing.Document.States", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onBeforeSetUpGrid(() => this.loadLookups())
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData1())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.guid = parameters.guid;
        this.isHomeTab = !!parameters.isHomeTab;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.reloadData(); });
        }

        this.flowHandler.start({ feature: Feature.Manage_Signing_Document_States, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Manage_Signing_Document_States].readPermission;
        this.modifyPermission = response[Feature.Manage_Signing_Document_States].modifyPermission;

        if (this.modifyPermission)
            this.messagingHandler.publishActivateAddTab();
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg as IGridHandler, () => this.reloadData());
    }

    public setupGrid() {
        // Columns
        var keys: string[] = [
            "common.type",
            "common.name",
            "common.description",
            "manage.attest.state.initial",
            "manage.attest.state.closed",
            "core.edit"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.gridAg.addColumnSelect("entityName", terms["common.type"], 100, { displayField: "entityName", selectOptions: null, populateFilterFromGrid: true, enableHiding: false });
            this.gridAg.addColumnShape("color", null, null, { shape: Constants.SHAPE_CIRCLE, toolTipField: "name", showIconField: "color", shapeField: "color", colorField: "color", attestGradient: true, gradientField: "useGradient" });
            this.gridAg.addColumnText("name", terms["common.name"], null);
            this.gridAg.addColumnText("description", terms["common.description"], null);
            this.gridAg.addColumnBool("initial", terms["manage.attest.state.initial"], 25, false);
            this.gridAg.addColumnBool("closed", terms["manage.attest.state.closed"], 25, false);
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this));

            this.gridAg.options.enableRowSelection = false;
            this.gridAg.finalizeInitGrid("manage.signing.state.states", true);
        });
    }

    protected loadLookups(): ng.IPromise<any> {
        return this.loadAttestEntities();
    }

    public loadAttestEntities(): ng.IPromise<any> {
        return this.signingService.getAttestEntitiesGenericList(false, true, SoeModule.Manage).then((x) => {
            this.signingEntities = x;
            _.forEach(x, (y: any) => {
                this.signingEntityFilterOptions.push({ value: y.name, label: y.name })
            });
            this.loadGridData1();
        });
    }

    public loadGridData1() {
        // Load data
        this.progress.startLoadingProgress([() => {
            return this.signingService.getAttestStates(TermGroup_AttestEntity.SigningDocument, SoeModule.Manage, false).then((x) => {
                _.forEach(x, (y: any) => {
                    var entity = _.find(this.signingEntities, e => e.id === y.entity);
                    y.entityName = entity.name;
                });
                this.setData(x);
            });
        }]);
    }

    private reloadData() {
        this.loadGridData1();
    }
}