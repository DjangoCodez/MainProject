import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { ICompositionGridController } from "../../../../Core/ICompositionGridController";
import { IControllerFlowHandlerFactory } from "../../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../../Core/Handlers/ProgressHandlerFactory";
import { IToolbarFactory } from "../../../../Core/Handlers/ToolbarFactory";
import { GridControllerBase2Ag } from "../../../../Core/Controllers/GridControllerBase2Ag";
import { IMessagingHandlerFactory } from "../../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../../Core/Handlers/GridHandlerFactory";
import { IGridHandler } from "../../../../Core/Handlers/GridHandler";
import { Feature } from "../../../../Util/CommonEnumerations";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { Constants } from "../../../../Util/Constants";
import { ISystemService } from "../../SystemService"
import { ToolBarUtility, ToolBarButton } from "../../../../Util/ToolBarUtility";
import { IconLibrary, SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../../Util/Enumerations";
import { ScheduledJobLogController } from "./ScheduledJobLog/ScheduledJobLogController";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {
    //Terms
    terms: { [index: string]: string; };

    //@ngInject
    constructor(
        private translationService: ITranslationService,
        private systemService: ISystemService,
        private notificationService: INotificationService,
        private $timeout: ng.ITimeoutService,
        private $uibModal,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        private urlHelperService: IUrlHelperService,) {
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

    private setupGrid() {
        this.gridAg.options.enableRowSelection = false;
        this.gridAg.addStandardMenuItems();

        var translationKeys: string[] = [
            "core.edit",
            "common.dashboard.syslog.level",
            "common.description",
            "common.message",
            "common.name",
            "common.status",
            "common.time",
            "manage.system.scheduler.executiontime",
            "manage.system.scheduler.active",
            "manage.system.scheduler.noofactive",
            "manage.system.scheduler.runnow",
            "manage.system.scheduler.activate",
            "manage.system.scheduler.showhistory",
            "manage.system.scheduler.batchnr",
            "manage.system.scheduler.logfor",
            "manage.system.syscompany.syscompdb"
        ];

        this.translationService.translateMany(translationKeys).then(terms => {
            this.terms = terms;

            this.gridAg.addColumnText("name", terms["common.name"], null, false);
            this.gridAg.addColumnText("description", terms["common.description"], null, true);
            this.gridAg.addColumnText("databaseName", terms["manage.system.syscompany.syscompdb"], null, false);
            this.gridAg.addColumnDateTime("executeTime", terms["manage.system.scheduler.executiontime"], null);
            this.gridAg.addColumnText("stateName", terms["common.status"], null, true);
            this.gridAg.addColumnShape("stateColor", null, 55, { shape: Constants.SHAPE_CIRCLE, toolTipField: "attestStateColorText", showIconField: "stateColor" });
            this.gridAg.addColumnIcon(null, " ", null, { icon: "fal fa-play", onClick: this.runJob.bind(this), toolTip: this.terms["manage.system.scheduler.runnow"] });
            this.gridAg.addColumnIcon(null, " ", null, { icon: "fal fa-clock", onClick: this.activateJob.bind(this), toolTip: this.terms["manage.system.scheduler.activate"] });
            this.gridAg.addColumnIcon(null, " ", null, { icon: "fal fa-history", onClick: this.showJobHistory.bind(this), toolTip: this.terms["manage.system.scheduler.showhistory"] });
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this));

            this.gridAg.finalizeInitGrid("manage.system.scheduler.scheduledjobs", true);
        });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.reloadData());
        if (this.toolbar) {
            this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("manage.system.scheduler.noofjobsshort", "manage.system.scheduler.noofjobs", IconLibrary.FontAwesome, "fa-tally", () => {
                this.getNumberOfActiveJobs();
            })));
            //    this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("manage.system.scheduler.runtimeshort", "manage.system.scheduler.runtime", IconLibrary.FontAwesome, "fa-stopwatch", () => {

            //    })));
        }
    }

    private loadGridData(useCache: boolean = true) {
        this.gridAg.clearData();

        this.progress.startLoadingProgress([() => {
            return this.systemService.getScheduledJobs().then(x => {
                this.gridAg.setData(x);
            });
        }]);

    }

    private getNumberOfActiveJobs() {
        this.progress.startLoadingProgress([() => {
            return this.systemService.getNoOfActiveScheduledJobs().then(value => {
                this.notificationService.showDialog(this.terms["manage.system.scheduler.active"], this.terms["manage.system.scheduler.noofactive"] + ": " + value, SOEMessageBoxImage.Information, SOEMessageBoxButtons.OKCancel);
            });
        }]);
    }

    private runJob(job: any) {
        this.progress.startSaveProgress((completion) => {
            this.systemService.runScheduledJob(job.sysScheduledJobId).then(result => {
                if (!result.error) {
                    completion.completed();
                }
                else {
                    completion.failed(result.errorMessage);
                }
            });
        }, this.guid);
    }

    private activateJob(job: any) {
        this.progress.startSaveProgress((completion) => {
            this.systemService.runScheduledJobByService(job.sysScheduledJobId).then(result => {
                if (result.success) {
                    completion.completed();
                } else {
                    completion.failed(result.errorMessage);
                }
            });
        }, this.guid);
    }

    private showJobHistory(job: any) {
        this.$uibModal.open({
            templateUrl: this.urlHelperService.getUrl("ScheduledJobLog/Views/ScheduledJobLog.html"),
            controller: ScheduledJobLogController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'xl',
            windowClass: 'fullsize-modal',
            resolve: {
                title: () => { return this.terms["manage.system.scheduler.logfor"] + " " + job.name },
                sysScheduledJobId: () => { return job.sysScheduledJobId },
                terms: () => { return this.terms },
            }
        });
    }

    private reloadData() {
        this.loadGridData(false);
    }
}
