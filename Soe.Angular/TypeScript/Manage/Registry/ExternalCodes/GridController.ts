import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IRegistryService } from "../RegistryService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { Feature } from "../../../Util/CommonEnumerations";


export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    // Terms:
    private terms: { [index: string]: string; };

    

    //@ngInject
    constructor(
        private coreService: ICoreService,
        private registryService: IRegistryService,
        private translationService: ITranslationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {
        super(gridHandlerFactory, "Manage.Registry.ExternalCodes", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData());
    }

    public onInit(parameters: any) {
        this.parameters = parameters;
        this.guid = parameters.guid;
        this.isHomeTab = !!parameters.isHomeTab;

        if (this.isHomeTab) {
            this.onTabActivetedAndModified(() => {
                this.reloadData();
            });
        }

        this.flowHandler.start([
            { feature: Feature.Manage_Preferences_Registry_ExternalCodes, loadReadPermissions: true, loadModifyPermissions: true },
        ]);
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Manage_Preferences_Registry_ExternalCodes].readPermission;
        this.modifyPermission = response[Feature.Manage_Preferences_Registry_ExternalCodes].modifyPermission;

        if (this.modifyPermission)
            this.messagingHandler.publishActivateAddTab();
    }


    public setupGrid() {
        // Columns
        var keys: string[] = [
            "core.edit",
            "manage.registry.externalcodes.externalcode",
            "common.type",
            "manage.registry.externalcodes.recordname"
        ];

        this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;

            this.gridAg.addColumnText("entityName", terms["common.type"], null);
            this.gridAg.addColumnText("externalCode", terms["manage.registry.externalcodes.externalcode"], null);
            this.gridAg.addColumnText("recordName", terms["manage.registry.externalcodes.recordname"], null);
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this));

            this.gridAg.options.enableRowSelection = false;
            this.gridAg.finalizeInitGrid("manage.registry.externalcodes.externalcodes", false);
        });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.reloadData());
    }

    public loadGridData(useCache: boolean = true) {
        this.progress.startLoadingProgress([() => {
            return this.registryService.getCompanyExternalCodesGrid().then(data => {
                
                this.setData(data);
            });
        }]);
    }

    private reloadData() {
        this.loadGridData(false);
    }

    public edit(row) {
        // Send message to TabsController
        if (this.readPermission || this.modifyPermission)
            this.messagingHandler.publishEditRow(row);
    }
}