import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { ICompositionGridController } from "../../../../Core/ICompositionGridController";
import { IControllerFlowHandlerFactory } from "../../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../../Core/Handlers/ProgressHandlerFactory";
import { IToolbarFactory } from "../../../../Core/Handlers/ToolbarFactory";
import { GridControllerBase2Ag } from "../../../../Core/Controllers/GridControllerBase2Ag";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { IMessagingHandlerFactory } from "../../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../../Core/Handlers/GridHandlerFactory";
import { IGridHandler } from "../../../../Core/Handlers/GridHandler";
import { Feature } from "../../../../Util/CommonEnumerations";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { ISystemService } from "../../SystemService"
import { ToolBarUtility, ToolBarButton } from "../../../../Util/ToolBarUtility";
import { IconLibrary } from "../../../../Util/Enumerations";
import { ISelectedItemsService } from "../../../../Core/Services/SelectedItemsService";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {
    //Terms
    terms: { [index: string]: string; };

    //@ngInject
    constructor(
        private $scope: ng.IScope,
        private translationService: ITranslationService,
        private coreService: ICoreService,
        private systemService: ISystemService,
        private notificationService: INotificationService,
        private $timeout: ng.ITimeoutService,
        private $filter: ng.IFilterService,
        private $uibModal,
        private $q: ng.IQService,
        private selectedItemsService: ISelectedItemsService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        private urlHelperService: IUrlHelperService, ) {
        super(gridHandlerFactory, "Manage.System.Import.StandardDefinitions", progressHandlerFactory, messagingHandlerFactory);

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
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData());
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.reloadData(); });
        }

        this.flowHandler.start({ feature: Feature.Manage_System, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.reloadData());
    }

    private setupGrid() {
        var translationKeys: string[] = [
            "common.name",
            "core.edit",
        ];

        this.translationService.translateMany(translationKeys).then(terms => {
            this.terms = terms;

            this.gridAg.options.enableRowSelection = false;

            this.gridAg.addColumnText("name", terms["common.name"], null, false);
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this));

            this.gridAg.finalizeInitGrid("manage.system.import.standarddefinitions", true)
        });
    }

    private loadGridData(useCache: boolean = true) {
        this.gridAg.clearData();

        this.progress.startLoadingProgress([() => {
            return this.systemService.getStandardDefinitions().then(data => {
                this.gridAg.setData(_.orderBy(data, 'name'));
            });
        }]);

    }

    edit(row) {
        if (this.doubleClickToEdit && (this.readPermission || this.modifyPermission)) 
            this.messagingHandler.publishEditRow(row);
    }

    private reloadData() {
        this.loadGridData(false);
    }
}