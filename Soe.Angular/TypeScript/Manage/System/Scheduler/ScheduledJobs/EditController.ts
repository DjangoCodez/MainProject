import { ICompositionEditController } from "../../../../Core/ICompositionEditController";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { IPermissionRetrievalResponse } from "../../../../Core/Handlers/ControllerFlowHandler";
import { IControllerFlowHandlerFactory } from "../../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../../Core/Handlers/ProgressHandlerFactory";
import { IToolbarFactory } from "../../../../Core/Handlers/ToolbarFactory";
import { IValidationSummaryHandlerFactory } from "../../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { EditControllerBase2 } from "../../../../Core/Controllers/EditControllerBase2";
import { IMessagingHandlerFactory } from "../../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../../Core/Handlers/DirtyHandlerFactory";
import { GridEvent } from "../../../../Util/SoeGridOptions";
import { Feature, TermGroup, SettingDataType, ScheduledJobState, ScheduledJobRetryType, ScheduledJobRecurrenceType } from "../../../../Util/CommonEnumerations";
import { Constants } from "../../../../Util/Constants";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { SOEMessageBoxImage, SOEMessageBoxButtons, IconLibrary, SoeGridOptionsEvent } from "../../../../Util/Enumerations";
import { ISystemService } from "../../SystemService";
import { ISoeGridOptionsAg, SoeGridOptionsAg } from "../../../../Util/SoeGridOptionsAg";
import { SysScheduledJobDTO, SysJobSettingDTO } from "../../../../Common/Models/SysJobDTO";
import { JobSettingDialogController } from "./Dialogs/JobSettingDialogController";
import { CalendarUtility } from "../../../../Util/CalendarUtility";
import { ToolBarButtonGroup, ToolBarUtility, ToolBarButton } from "../../../../Util/ToolBarUtility";
import { EditRecurrenceIntervalController } from "../../../../Common/Dialogs/EditRecurrenceInterval/EditRecurrenceIntervalController";
import { SmallGenericType } from "../../../../Common/Models/SmallGenericType";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    private sysScheduledJobId: number;
    private users: SmallGenericType[];
    private recurrenceTypes: any[];
    private retryTypes: any[];
    private jobStates: any[];
    private logLevels: any[];
    private settingTypes: any[];
    private registeredJobs: any[];
    private priceManagementTypes: any[];
    private orderReportTemplates: any[];
    private invoiceReportTemplates: any[];

    private job: SysScheduledJobDTO;

    private monitoringButtons = new Array<ToolBarButtonGroup>();
    private monitoringGrid: ISoeGridOptionsAg;

    private terms: { [index: string]: string; };

    //Flags
    private loadingLog = false;
    private logLoaded = false;

    //@ngInject
    constructor(
        private $uibModal,
        private $timeout,
        private $q: ng.IQService,
        private coreService: ICoreService,
        private systemService: ISystemService,
        private urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory,
        private notificationService: INotificationService,
        private translationService: ITranslationService) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.doLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    public onInit(parameters: any) {
        this.sysScheduledJobId = parameters.id;
        this.guid = parameters.guid;

        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.monitoringGrid = SoeGridOptionsAg.create("LogGrid", this.$timeout);
        this.monitoringGrid.setMinRowsToShow(25);

        this.flowHandler.start([{ feature: Feature.Manage_System, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Manage_System].readPermission;
        this.modifyPermission = response[Feature.Manage_System].modifyPermission;
    }

    private doLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.loadTerms(),
            this.getUsers(),
            this.getRecurrenceTypes(),
            this.getRetryTypes(),
            this.getStates(),
            this.getLogLevels(),
            this.getRegisteredJobs(),
            this.getSettingTypes()]).then(() => {
                this.setupLogGridColumns();
                if (this.sysScheduledJobId && this.sysScheduledJobId > 0)
                    this.load();
                else
                    this.new();
            });
    }

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "core.yes",
            "core.no",
            "manage.system.scheduler.batchnr",
            "common.dashboard.syslog.level",
            "common.time",
            "common.message",
            "common.dashboard.reload",
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private getUsers(): ng.IPromise<any> {
        return this.coreService.getUsersDict(false, false, true, false).then(x => {
            this.users = x;
        });
    }

    private getRecurrenceTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.ScheduledJobRecurrenceType, false, false).then(x => {
            this.recurrenceTypes = x;
        });
    }

    private getRetryTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.ScheduledJobRetryType, false, false).then(x => {
            this.retryTypes = x;
        });
    }

    private getStates(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.ScheduledJobState, false, false, true).then(x => {
            this.jobStates = x;
        });
    }

    private getLogLevels(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.ScheduledJobLogLevel, false, false).then(x => {
            this.logLevels = x;
        });
    }

    private getRegisteredJobs(): ng.IPromise<any> {
        return this.systemService.getRegisteredJobs(false).then(x => {
            this.registeredJobs = x;
        });
    }

    private getSettingTypes(): ng.IPromise<any> {
        this.settingTypes = [];
        return this.coreService.getTermGroupContent(TermGroup.JobSettings, false, false).then(x => {
            this.settingTypes.push({ id: 1, name: _.find(x, { 'id': 6 }).name });   // String
            this.settingTypes.push({ id: 3, name: _.find(x, { 'id': 12 }).name });  // Bool
            this.settingTypes.push({ id: 4, name: _.find(x, { 'id': 14 }).name });  // Date
            this.settingTypes.push({ id: 6, name: _.find(x, { 'id': 16 }).name });  // Time
            this.settingTypes.push({ id: 2, name: _.find(x, { 'id': 8 }).name });   // Int
            this.settingTypes.push({ id: 5, name: _.find(x, { 'id': 10 }).name });  // Decimal
        });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => this.copy(), () => this.isNew);
    }

    private setupLogGridColumns() {
        this.monitoringButtons.push(ToolBarUtility.createGroup(new ToolBarButton("", "common.dashboard.reload", IconLibrary.FontAwesome, "fa-sync", () => {
            this.loadLog()
        }, () => {
            return this.isNew;
        })));

        this.monitoringGrid.enableRowSelection = false;
        this.monitoringGrid.addColumnNumber("batchNr", this.terms["manage.system.scheduler.batchnr"], null, { enableHiding: true });
        this.monitoringGrid.addColumnText("logLevelName", this.terms["common.dashboard.syslog.level"], null);
        this.monitoringGrid.addColumnDateTime("time", this.terms["common.time"], null);
        this.monitoringGrid.addColumnText("message", this.terms["common.message"], null);
        this.monitoringGrid.addColumnIcon(null, "", null, { icon: "iconEdit fal fa-search", onClick: this.showLogEntry.bind(this) });

        var events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.RowDoubleClicked, (row) => { this.showLogEntry(row); }));
        this.monitoringGrid.subscribe(events);

        this.monitoringGrid.finalizeInitGrid();
    }

    protected copy() {
        this.isNew = true;
        this.job.sysScheduledJobId = undefined;
        this.dirtyHandler.setDirty();
    }

    private load() {
        return this.systemService.getScheduledJob(this.sysScheduledJobId, true, true).then(job => {
            this.job = job;
            this.isNew = false;

            _.forEach(this.job.sysJobSettings, setting => {
                setting.setValue(this.terms["core.yes"], this.terms["core.no"]);
            });
        });
    }

    private loadLog() {
        if (!this.sysScheduledJobId || this.sysScheduledJobId === 0)
            return;

        this.loadingLog = true;
        return this.systemService.getScheduledJobLog(this.sysScheduledJobId).then(log => {
            this.monitoringGrid.setData(log);
            this.loadingLog = false;
        });
    }

    private save() {
        this.progress.startSaveProgress((completion) => {
            this.systemService.saveScheduledJob(this.job).then((result) => {
                if (result.success) {
                    this.sysScheduledJobId = result.integerValue;
                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.job);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid)
            .then(data => {
                this.dirtyHandler.clean();
                this.load();
            }, error => {

            });
    }

    public delete() {
        this.progress.startDeleteProgress((completion) => {
            this.systemService.deleteSchedueldJob(this.job.sysScheduledJobId).then((result) => {
                if (result.success) {
                    completion.completed(null);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }).then(x => {
            super.closeMe(false);
        });
    }

    private new() {
        this.isNew = true;
        this.job = new SysScheduledJobDTO();
        this.job.executeUserId = soeConfig.userId;
        this.job.executeTime = CalendarUtility.getDateNow();
        this.job.state = ScheduledJobState.Inactive
        this.job.retryTypeForExternalError = ScheduledJobRetryType.Abort;
        this.job.retryTypeForInternalError = ScheduledJobRetryType.Abort;
        this.job.retryCount = 0;
        this.job.recurrenceType = ScheduledJobRecurrenceType.RunOnce;
        this.job.recurrenceCount = 0;
        this.job.recurrenceInterval = "{0} {1} {2} {3} {4}".format(Constants.CRONTAB_ALL_SELECTED, Constants.CRONTAB_ALL_SELECTED, Constants.CRONTAB_ALL_SELECTED, Constants.CRONTAB_ALL_SELECTED, Constants.CRONTAB_ALL_SELECTED);

        this.job.sysJobSettings = [];
    }

    private editSetting(setting: SysJobSettingDTO) {
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Manage/System/Scheduler/ScheduledJobs/Dialogs/Views/JobSettingDialog.html"),
            controller: JobSettingDialogController,
            controllerAs: "ctrl",
            size: 'md',
            resolve: {
                settingTypes: () => { return this.settingTypes },
                setting: () => { return setting },
            }
        }
        this.$uibModal.open(options).result.then(result => {
            if (result && result.setting) {
                if (!result.setting.sysJobSettingId || result.setting.sysJobSettingId === 0) {
                    setting = new SysJobSettingDTO();
                    this.setValues(result.setting, setting);
                    this.job.sysJobSettings.push(setting);
                } else {
                    let existing = _.find(this.job.sysJobSettings, s => s.sysJobSettingId === result.setting.sysJobSettingId);
                    if (existing)
                        this.setValues(result.setting, existing);
                }

                this.dirtyHandler.setDirty();
            }
        });
    }

    private setValues(settingFrom: SysJobSettingDTO, settingTo: SysJobSettingDTO) {
        settingTo.boolData = undefined;
        settingTo.dateData = undefined;
        settingTo.decimalData = undefined;
        settingTo.intData = undefined;
        settingTo.strData = undefined;
        settingTo.timeData = undefined;

        switch (settingFrom.dataType) {
            case SettingDataType.Boolean:
                settingTo.boolData = settingFrom.boolData;
                break;
            case SettingDataType.Date:
                settingTo.dateData = settingFrom.dateData;
                break;
            case SettingDataType.Decimal:
                settingTo.decimalData = settingFrom.decimalData;
                break;
            case SettingDataType.Integer:
                settingTo.intData = settingFrom.intData;
                break;
            case SettingDataType.String:
                settingTo.strData = settingFrom.strData;
                break;
            case SettingDataType.Time:
                settingTo.timeData = settingFrom.timeData;
                break;
        }

        settingTo.dataType = settingFrom.dataType;
        settingTo.name = settingFrom.name;
        settingTo.setValue(this.terms["core.yes"], this.terms["core.no"]);
    }

    private deleteSetting(setting: SysJobSettingDTO) {
        _.pull(this.job.sysJobSettings, setting);

        this.dirtyHandler.setDirty();
    }

    private runJob() {
        this.progress.startSaveProgress((completion) => {
            this.systemService.runScheduledJob(this.job.sysScheduledJobId).then(result => {
                if (!result.error) {
                    completion.completed();
                }
                else {
                    completion.failed(result.errorMessage);
                }
            });
        }, this.guid);
    }

    private activateJob() {
        this.progress.startSaveProgress((completion) => {
            this.systemService.runScheduledJobByService(this.job.sysScheduledJobId).then(result => {
                if (result.success) {
                    completion.completed();
                }
                else {
                    completion.failed(result.errorMessage);
                }
            });
        }, this.guid);
    }

    private editRecurrence() {
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Common/Dialogs/EditRecurrenceInterval/EditRecurrenceInterval.html"),
            controller: EditRecurrenceIntervalController,
            controllerAs: "ctrl",
            size: 'lg',
            resolve: {
                singleSelectTime: () => { return false },
                interval: () => { return this.job.recurrenceInterval }
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result && result.interval) {
                this.job.recurrenceInterval = result.interval;
                this.dirtyHandler.setDirty();
            }
        });
    }

    public showLogEntry(row: any) {
        this.notificationService.showDialog(this.terms["core.info"], row.message, SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK);
    }

    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.job) {
                if (!this.job.sysJobId)
                    mandatoryFieldKeys.push("manage.system.scheduler.job");
                if (!this.job.name)
                    mandatoryFieldKeys.push("common.name");
                if (!this.job.databaseName)
                    mandatoryFieldKeys.push("manage.system.scheduler.databasename");
                if (!this.job.executeTime)
                    mandatoryFieldKeys.push("manage.system.scheduler.validateexecutiontime");
            }
        });
    }
}