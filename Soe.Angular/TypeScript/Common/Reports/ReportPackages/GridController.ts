import { ICoreService } from "../../../Core/Services/CoreService";
import { IReportService } from "../../../Core/Services/ReportService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../Core/Services/UrlHelperService";
import { HtmlUtility } from "../../../Util/HtmlUtility";
import { SoeModule } from "../../../Util/CommonEnumerations";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {
    //@ngInject
    constructor($http,
        $templateCache,
        $timeout: ng.ITimeoutService,
        $uibModal,
        private $window,
        private $filter: ng.IFilterService,
        coreService: ICoreService,
        private reportService: IReportService,
        private translationService: ITranslationService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {

        super(gridHandlerFactory, "common.report.reportpackages", progressHandlerFactory, messagingHandlerFactory);

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

        this.flowHandler.start({ feature: soeConfig.feature, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg, () => this.loadGridData());
    }

    public setupGrid() {
        var keys: string[] = [
            "common.name",
            "common.description",
            "core.edit",
            "common.report.reportpackage.show",
            "common.report.reportpackage.editselection"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.gridAg.options.enableRowSelection = false;

            this.gridAg.addColumnText("name", terms["common.name"], null);
            this.gridAg.addColumnText("description", terms["common.description"], null);

            if (this.modifyPermission)
                this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this));
            
            this.gridAg.addColumnIcon(null, " ", null, { icon: "fal fa-print fa-stack-1x|fas fa-search fa-stack-1x iconEdit", toolTip: terms["common.report.reportpackage.show"], onClick: this.view.bind(this), suppressFilter: true });
            this.gridAg.addColumnIcon(null, " ", null, { icon: "fal fa-print fa-stack-1x|fas fa-pencil fa-stack-1x iconEdit", toolTip: terms["common.report.reportpackage.editselection"], onClick: this.editselection.bind(this), suppressFilter: true });

            this.gridAg.finalizeInitGrid("common.report.reportpackages", true);
        });
    }

    public loadGridData() {
        // Load data
        this.reportService.getReportPackagesForModule(soeConfig.module, false).then((x) => {
            this.setData(x);
        });
    }

    protected view(row) {
        HtmlUtility.openInSameTab(this.$window, "/soe/" + SoeModule[soeConfig.module].toLowerCase() + "/distribution/reports/?package=" + row.reportPackageId + "&m=" + soeConfig.module);
    }

    protected editselection(row) {
        HtmlUtility.openInSameTab(this.$window, "/soe/" + SoeModule[soeConfig.module].toLowerCase() + "/distribution/reports/selection/?package=" + row.reportPackageId);
    }
}