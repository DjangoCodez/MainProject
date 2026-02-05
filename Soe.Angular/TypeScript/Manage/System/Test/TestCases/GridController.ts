import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { ICompositionGridController } from "../../../../Core/ICompositionGridController";
import { IEditControllerFlowHandler } from "../../../../Core/Handlers/ControllerFlowHandler";
import { IControllerFlowHandlerFactory } from "../../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandler } from "../../../../Core/Handlers/ProgressHandler";
import { IProgressHandlerFactory } from "../../../../Core/Handlers/ProgressHandlerFactory";
import { IToolbar } from "../../../../Core/Handlers/Toolbar";
import { IToolbarFactory } from "../../../../Core/Handlers/ToolbarFactory";
import { IValidationSummaryHandler } from "../../../../Core/Handlers/ValidationSummaryHandler";
import { IValidationSummaryHandlerFactory } from "../../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { GridControllerBase2Ag } from "../../../../Core/Controllers/GridControllerBase2Ag";
import { ISoeGridOptionsAg, SoeGridOptionsAg } from "../../../../Util/SoeGridOptionsAg";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { IMessagingHandlerFactory } from "../../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../../Core/Handlers/GridHandlerFactory";
import { IGridHandler } from "../../../../Core/Handlers/GridHandler";
import { Feature, ScheduledJobState, TermGroup } from "../../../../Util/CommonEnumerations";
import { lang } from "moment";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { Constants } from "../../../../Util/Constants";
import { ISystemService } from "../../SystemService"
import { CalendarUtility } from "../../../../Util/CalendarUtility";
import { ToolBarUtility, ToolBarButton } from "../../../../Util/ToolBarUtility";
import { IconLibrary, SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../../Util/Enumerations";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {
    //Terms
    terms: { [index: string]: string; };
    private testTypes: any;

    //@ngInject
    constructor(
        private translationService: ITranslationService,
        private coreService: ICoreService,
        private systemService: ISystemService,
        private notificationService: INotificationService,
        private $timeout: ng.ITimeoutService,
        private $filter: ng.IFilterService,
        private $uibModal,
        private $q: ng.IQService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        private urlHelperService: IUrlHelperService,) {
        super(gridHandlerFactory, "Manage.System.Test.TestCases", progressHandlerFactory, messagingHandlerFactory);

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
            .onDoLookUp(() => this.doLookups())
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

    edit(row) {
        // Send message to TabsController
        if (this.readPermission || this.modifyPermission)
            this.messagingHandler.publishEditRow(row);
    }

    private doLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.loadTestTypes()
        ])
    }

    private loadTestTypes() {
        return this.coreService.getTermGroupContent(TermGroup.TestCaseType, true, false, true).then(x => {
            this.testTypes = x;
        });
    }

    private setupGrid() {
        var translationKeys: string[] = [
            "common.name",
            "common.description",
            "common.number",
            "manage.system.syscompany.syscompdb",
            "manage.system.scheduler.executiontime",
            "common.status",
            "common.type",
            "core.edit",
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total",
            "manage.system.scheduler.active",
            "manage.system.scheduler.noofactive",
            "manage.system.scheduler.runnow",
            "manage.system.scheduler.activate",
            "manage.system.scheduler.showhistory",
            "manage.system.scheduler.batchnr",
            "common.dashboard.syslog.level",
            "common.time",
            "common.message",
            "manage.system.scheduler.logfor"
        ];

        this.translationService.translateMany(translationKeys).then(terms => {
            this.terms = terms;

            this.gridAg.addColumnNumber("testCaseId", terms["common.number"], null);
            this.gridAg.addColumnText("name", terms["common.name"], null, false);
            this.gridAg.addColumnText("description", terms["common.description"], null, true);
            //this.gridAg.addColumnText("databaseName", terms["manage.system.syscompany.syscompdb"], null, false);
            this.gridAg.addColumnText("testCaseTypeName", terms["common.type"], null, false);
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this));

            this.gridAg.addStandardMenuItems();

            this.gridAg.options.addTotalRow("#totals-grid", {
                filtered: terms["core.aggrid.totals.filtered"],
                total: terms["core.aggrid.totals.total"]
            });

            this.gridAg.options.finalizeInitGrid();
            this.gridAg.options.enableMasterDetailWithDirective("row-detail");
        });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.reloadData());
    }


    private loadGridData(useCache: boolean = true) {
        this.progress.startLoadingProgress([() => {
            return this.systemService.getTestCases().then(data => {
                data.forEach(r => {
                    if (!r.testCaseType)
                        r.testCaseType = 0;
                    r["testCaseTypeName"] = this.testTypes.find(t => t.id === r.testCaseType).name;
                })
                this.setData(data);
            });
        }]);

    }


    private reloadData() {
        this.loadGridData(false);
    }
}
