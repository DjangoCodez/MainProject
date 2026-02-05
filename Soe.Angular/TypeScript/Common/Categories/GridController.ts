import { ICoreService } from "../../Core/Services/CoreService";
import { ITranslationService } from "../../Core/Services/TranslationService";
import { IMessagingService } from "../../Core/Services/MessagingService";
import { INotificationService } from "../../Core/Services/NotificationService";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../Core/Services/UrlHelperService";
import { CoreUtility } from "../../Util/CoreUtility";
import { GridControllerBase2Ag } from "../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../Core/ICompositionGridController";
import { IControllerFlowHandlerFactory } from "../../Core/Handlers/controllerflowhandlerfactory";
import { IProgressHandlerFactory } from "../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../Core/Handlers/messaginghandlerfactory";
import { IGridHandlerFactory } from "../../Core/Handlers/gridhandlerfactory";
import { IToolbarFactory } from "../../Core/Handlers/ToolbarFactory";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {
    // Filters

    accountFilterOptions: Array<any> = [];
    private categoryType: number;
    private feature: number;

    //@ngInject
    constructor($http,
        $templateCache,
        $timeout: ng.ITimeoutService,

        $uibModal,
        private $filter: ng.IFilterService,
        private coreService: ICoreService,
        private translationService: ITranslationService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        private urlHelperService: IUrlHelperService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {

        super(gridHandlerFactory, "common.categories.categories", progressHandlerFactory, messagingHandlerFactory);

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
        
        this.categoryType = CoreUtility.getCategoryType(soeConfig.feature);
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.loadGridData(); });
        }

        this.flowHandler.start({ feature: soeConfig.feature, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg, () => this.loadGridData());
    }

    public setupGrid() {
        // Columns
        var keys: string[] = [
            "common.code",
            "common.name",
            "common.categories.groupname",
            "common.categories.childrengroupname",
            "core.edit"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.gridAg.addColumnText("code", terms["common.code"], null, true);
            this.gridAg.addColumnText("name", terms["common.name"], null);
            this.gridAg.addColumnText("categoryGroupName", terms["common.categories.groupname"], null, true);
            this.gridAg.addColumnText("childrenNamesString", terms["common.categories.childrengroupname"], null, true);
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this));

            this.gridAg.finalizeInitGrid("common.categories.categories", true)
        });
    }

    public loadGridData(useCache: boolean = false) {
        // Load data
        return this.coreService.getCategories(this.categoryType, false, true, true, false).then(x => {
            this.setData(x);
        });
    }

    edit(row) {
        // Send message to TabsController
        if (this.doubleClickToEdit && (this.readPermission || this.modifyPermission))
            this.messagingHandler.publishEditRow(row);
    }
}