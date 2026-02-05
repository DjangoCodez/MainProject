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
import { Feature, TermGroup } from "../../../../Util/CommonEnumerations";
import { ISystemService } from "../../SystemService"
import { ToolBarUtility, ToolBarButton } from "../../../../Util/ToolBarUtility";
import { IconLibrary} from "../../../../Util/Enumerations";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {
    private settingTypes: any;

    //@ngInject
    constructor(
        private translationService: ITranslationService,
        private coreService: ICoreService,
        private systemService: ISystemService,
        private $q: ng.IQService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {
        super(gridHandlerFactory, "Manage.System.Scheduler.ScheduledJobs", progressHandlerFactory, messagingHandlerFactory);

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
        if (this.readPermission || this.modifyPermission)
            this.messagingHandler.publishEditRow(row);
    }

    private doLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.loadSettingTypes()
        ])
    }

    private loadSettingTypes() {
        return this.coreService.getTermGroupContent(TermGroup.TestCaseSettingType, true, false, true).then(x => {
            this.settingTypes = x;
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
            this.gridAg.addColumnNumber("testCaseGroupId", terms["common.number"], null);
            this.gridAg.addColumnText("name", terms["common.name"], null, false);
            this.gridAg.addColumnText("description", terms["common.description"], null, true);
            this.gridAg.addColumnDateTime("executeTime", terms["manage.system.scheduler.executiontime"], null);
            this.gridAg.addColumnText("type", terms["common.type"], null, true);
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this));
            this.gridAg.options.addTotalRow("#totals-grid", {
                filtered: terms["core.aggrid.totals.filtered"],
                total: terms["core.aggrid.totals.total"]
            });

            this.gridAg.finalizeInitGrid("system.test.testcasegroups", true);
        });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.reloadData());
        if (this.toolbar) {
            this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("manage.system.scheduler.activate", "manage.system.scheduler.activate", IconLibrary.FontAwesome, "fa-play", () => {
                this.scheduleSelectedGroups();
            })));
        }
    }

    private loadGridData(useCache: boolean = true) {
        this.progress.startLoadingProgress([() => {
            return this.systemService.getTestCaseGroups().then(data => {
                this.gridAg.setData(data);
            });
        }]);

    }

    private scheduleSelectedGroups() {
        const rows = this.gridAg.options.getSelectedRows();
        let ids = rows.map(r => r.testCaseGroupId);
        this.systemService.scheduleTestCaseGroupsNow(ids)
            .then(r => this.reloadData())
    }

    private reloadData() {
        this.loadGridData(false);
    }
}
