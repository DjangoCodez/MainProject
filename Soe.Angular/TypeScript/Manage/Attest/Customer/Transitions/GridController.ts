import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IAttestService } from "../../AttestService";
import { Feature, TermGroup_AttestEntity, SoeModule } from "../../../../Util/CommonEnumerations";
import { GridControllerBase2Ag } from "../../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../../Core/ICompositionGridController";
import { IControllerFlowHandlerFactory } from "../../../../Core/Handlers/controllerflowhandlerfactory";
import { IProgressHandlerFactory } from "../../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../../Core/Handlers/messaginghandlerfactory";
import { IGridHandlerFactory } from "../../../../Core/Handlers/gridhandlerfactory";
import { IPermissionRetrievalResponse } from "../../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../../Core/Handlers/ToolbarFactory";
import { IGridHandler } from "../../../../Core/Handlers/GridHandler";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    // Filters
    attestEntityFilterOptions = [];
    attestStateFilterOptions = [];


    //@ngInject
    constructor(
        private attestService: IAttestService,
        private translationService: ITranslationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {

        super(gridHandlerFactory, "Manage.Attest.Customer.Transitions", progressHandlerFactory, messagingHandlerFactory);
        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData());
    }


    onInit(parameters: any) {
        this.parameters = parameters;
        this.guid = parameters.guid;
        this.isHomeTab = !!parameters.isHomeTab;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.reloadData(); });
        }

        this.flowHandler.start({ feature: Feature.Manage_Attest_Customer_AttestTransitions, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Manage_Attest_Customer_AttestTransitions].readPermission;
        this.modifyPermission = response[Feature.Manage_Attest_Customer_AttestTransitions].modifyPermission;

        if (this.modifyPermission)
            this.messagingHandler.publishActivateAddTab();
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg as IGridHandler, () => this.reloadData());
    }

    public setupGrid() {
        // Columns
        var keys: string[] = [
            "manage.attest.transition.entityname",
            "common.name",
            "manage.attest.transition.atteststatefrom",
            "manage.attest.transition.atteststateto",
            "core.edit"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.gridAg.addColumnSelect("entityName", terms["manage.attest.transition.entityname"], 100, { displayField: "entityName", selectOptions: null, populateFilterFromGrid: true } )
            this.gridAg.addColumnText("name", terms["common.name"], null);
            this.gridAg.addColumnSelect("attestStateFrom", terms["manage.attest.transition.atteststatefrom"], 100, { displayField: "attestStateFrom", selectOptions: null, populateFilterFromGrid: true } )
            this.gridAg.addColumnSelect("attestStateTo", terms["manage.attest.transition.atteststateto"], 100, { displayField: "attestStateTo", selectOptions: null, populateFilterFromGrid: true } )
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this));

            this.gridAg.options.enableRowSelection = false;
            this.gridAg.finalizeInitGrid("manage.attest.transition.transitions", true);
        });
    }

    private reloadData() {
        this.loadGridData();
    }

    public loadGridData() {
        // Load data
        this.progress.startLoadingProgress([() => {
            return this.attestService.getAttestTransitionGridDTOs(TermGroup_AttestEntity.Unknown, SoeModule.Billing, true).then((data) => {
                this.setData(data);
            });
        }]);
    }
}
