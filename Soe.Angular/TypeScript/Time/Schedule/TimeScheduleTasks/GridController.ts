import { ITranslationService } from "../../../Core/Services/TranslationService";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { IScheduleService } from "../ScheduleService";
import { Feature, CompanySettingType } from "../../../Util/CommonEnumerations";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ISmallGenericType } from "../../../Scripts/TypeLite.Net4";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {
    private terms: any;
    private useAccountsHierarchy: boolean;

    private taskTypes: ISmallGenericType[] = [];
    
    //@ngInject
    constructor(
        private $q: ng.IQService,
        private $timeout: ng.ITimeoutService,
        private scheduleService: IScheduleService,
        private coreService: ICoreService,
        private translationService: ITranslationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {
        super(gridHandlerFactory, "Time.Schedule.TimeScheduleTasks", progressHandlerFactory, messagingHandlerFactory);
        this.useRecordNavigatorInEdit('timeScheduleTaskId', 'name');
        this.onTabActivetedAndModified(() => this.reloadData());
        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onPermissionsLoaded((feature, readOnly, modify) => {
                this.readPermission = readOnly;
                this.modifyPermission = modify

                if (this.modifyPermission) {
                    // Send messages to TabsController
                    this.messagingHandler.publishActivateAddTab();
                }
            })
            .onBeforeSetUpGrid(() => this.loadLookups())
            .onSetUpGrid(() => this.setUpGrid())
            .onLoadGridData(() => this.loadGridData())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;
        this.guid = this.parameters.guid;

        this.flowHandler.start({ feature: Feature.Time_Schedule_StaffingNeeds_Tasks, loadReadPermissions: true, loadModifyPermissions: true });
    }
    
    private loadLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.loadTerms(),
            this.loadCompanySettings(),
            this.loadTaskTypes()]);
    }

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "core.edit",
            "core.notspecified",
            "common.active",
            "common.name",
            "common.dailyrecurrencepattern",
            "common.dailyrecurrencepattern.rangetype",
            "common.dailyrecurrencepattern.startdate",
            "common.description",
            "common.user.attestrole.accounthierarchy",
            "time.schedule.shifttype.shifttype",
            "time.schedule.timescheduletask.starttime",
            "time.schedule.timescheduletask.stoptime",
            "time.schedule.timescheduletask.length",
            "time.schedule.timescheduletasktype.allowoverlapping",
            "time.schedule.timescheduletasktype.dontassignbreakleftovers",
            "time.schedule.timescheduletasktype.isstaffingneedsfrequency",
            "time.schedule.timescheduletasktype.nbrofpersons",
            "time.schedule.timescheduletasktype.onlyoneemployee",
            "time.schedule.timescheduletasktype.type"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        const settingTypes: number[] = [CompanySettingType.UseAccountHierarchy];

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.useAccountsHierarchy = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseAccountHierarchy);
        });
    }

    private loadTaskTypes(): ng.IPromise<any> {
        this.taskTypes = [];
        return this.scheduleService.getTimeScheduleTaskTypesGrid(true).then(x => {
            _.forEach(x, (row: any) => {
                this.taskTypes.push({ id: row.timeScheduleTaskTypeId, name: row.name });
            });
        });
    }

    private setUpGrid() {
        this.gridAg.addColumnBool("isActive", this.terms["common.active"], 40, false, null, null, true);
        this.gridAg.addColumnText("name", this.terms["common.name"], null, false);
        this.gridAg.addColumnText("description", this.terms["common.description"], null, true);
        this.gridAg.addColumnTime("startTime", this.terms["time.schedule.timescheduletask.starttime"], 50, { enableHiding: true });
        this.gridAg.addColumnTime("stopTime", this.terms["time.schedule.timescheduletask.stoptime"], 50, { enableHiding: true });
        this.gridAg.addColumnTime("length", this.terms["time.schedule.timescheduletask.length"], 50, { enableHiding: true, minutesToTimeSpan: true });
        this.gridAg.addColumnText("shiftTypeName", this.terms["time.schedule.shifttype.shifttype"], null, true);
        if (this.taskTypes.length > 0)
            this.gridAg.addColumnText("typeName", this.terms["time.schedule.timescheduletasktype.type"], null, true);
        if (this.useAccountsHierarchy)
            this.gridAg.addColumnText("accountName", this.terms["common.user.attestrole.accounthierarchy"], null, true);
        this.gridAg.addColumnText("recurrenceStartsOnDescription", this.terms["common.dailyrecurrencepattern.startdate"], 50, true);
        this.gridAg.addColumnText("recurrenceEndsOnDescription", this.terms["common.dailyrecurrencepattern.rangetype"], 100, true);
        this.gridAg.addColumnText("recurrencePatternDescription", this.terms["common.dailyrecurrencepattern"], null, true);
        this.gridAg.addColumnNumber("nbrOfPersons", this.terms["time.schedule.timescheduletasktype.nbrofpersons"], 50, { enableHiding: true });
        this.gridAg.addColumnBoolEx("onlyOneEmployee", this.terms["time.schedule.timescheduletasktype.onlyoneemployee"], 50, { enableHiding: true });
        this.gridAg.addColumnBoolEx("dontAssignBreakLeftovers", this.terms["time.schedule.timescheduletasktype.dontassignbreakleftovers"], 50, { enableHiding: true });
        this.gridAg.addColumnBoolEx("allowOverlapping", this.terms["time.schedule.timescheduletasktype.allowoverlapping"], 50, { enableHiding: true });
        this.gridAg.addColumnBoolEx("isStaffingNeedsFrequency", this.terms["time.schedule.timescheduletasktype.isstaffingneedsfrequency"], 50, { enableHiding: true });
        this.gridAg.addColumnEdit(this.terms["core.edit"], this.edit.bind(this));

        this.gridAg.options.enableRowSelection = false;
        this.gridAg.finalizeInitGrid("time.schedule.timescheduletask.tasks", true, undefined, true);
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.grid, () => this.reloadData());
    }

    private loadGridData(useCache: boolean = true) {
        this.gridAg.clearData();

        this.progress.startLoadingProgress([() => {
            return this.scheduleService.getTimeScheduleTasksGrid(useCache).then(x => {
                _.forEach(x, task => {
                    if (!task.shiftTypeName)
                        task.shiftTypeName = this.terms["core.notspecified"];
                });

                return x;
            }).then(data => {
                this.setData(data);
            });
        }]);
    }

    private reloadData() {
        this.loadGridData(false);
    }
}
