import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { Feature, CompanySettingType } from "../../../Util/CommonEnumerations";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { ToolBarUtility, ToolBarButton } from "../../../Util/ToolBarUtility";
import { IconLibrary } from "../../../Util/Enumerations";
import { IScheduleService } from "../ScheduleService";
import { CreatePostsController } from "./Dialogs/CreatePostsController";
import { AccountDTO } from "../../../Common/Models/AccountDTO";
import { CalendarUtility } from "../../../Util/CalendarUtility";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    // Terms:
    private terms: any;

    // Company settings
    private useAccountsHierarchy: boolean = false;

    // Accounts
    private accounts: AccountDTO[];

    //@ngInject
    constructor(
        private coreService: ICoreService,
        private scheduleService: IScheduleService,
        private translationService: ITranslationService,
        private $filter: ng.IFilterService,
        private $timeout: ng.ITimeoutService,
        private $uibModal,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        private urlHelperService: IUrlHelperService) {
        super(gridHandlerFactory, "Time.Schedule.EmployeePosts", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onBeforeSetUpGrid(() => this.loadCompanySettings())
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

        this.loadAccountStringIdsByUserFromHierarchy();

        this.flowHandler.start([
            { feature: Feature.Time_Schedule_StaffingNeeds_EmployeePost, loadReadPermissions: true, loadModifyPermissions: true },
        ]);
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Time_Schedule_StaffingNeeds_EmployeePost].readPermission;
        this.modifyPermission = response[Feature.Time_Schedule_StaffingNeeds_EmployeePost].modifyPermission;

        if (this.modifyPermission)
            this.messagingHandler.publishActivateAddTab();
    }

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];

        settingTypes.push(CompanySettingType.UseAccountHierarchy);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.useAccountsHierarchy = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseAccountHierarchy);
        });
    }

    private loadAccountStringIdsByUserFromHierarchy(): ng.IPromise<any> {
        this.accounts = [];
        return this.coreService.getAccountsFromHierarchyByUserSetting(CalendarUtility.getDateNow(), CalendarUtility.getDateNow(), true).then(x => {
            this.accounts = x;
        });
    }

    public setupGrid() {
        // Columns
        const keys: string[] = [
            "common.active",
            "common.name",
            "common.description",
            "time.schedule.employeepost.datefrom",
            "time.schedule.employeepost.dateto",
            "time.schedule.employeepost.worktimeweek",
            "time.employee.employment.percent",
            "time.schedule.employeepost.employeegroupname",
            "core.edit",
            "common.user.attestrole.accounthierarchy",
            "time.schedule.employeepost.nbrofdaysinweek",
            "time.schedule.employeepost.dayofweeks",
            "time.schedule.employeepost.weekendtype",
            "time.schedule.employeepost.vacationweekdays",
            "time.schedule.employeepost.schedulecycle",
            "time.schedule.employeepost.skills",
            "core.deleteselectedwarning"
        ];

        this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;

            this.gridAg.addColumnText("name", terms["common.name"], null);
            this.gridAg.addColumnText("description", terms["common.description"], null);
            this.gridAg.addColumnDate("dateFrom", terms["time.schedule.employeepost.datefrom"], 100, true);
            if (this.useAccountsHierarchy)
                this.gridAg.addColumnText("accountName", terms["common.user.attestrole.accounthierarchy"], null, true);
            this.gridAg.addColumnTime("workTimeWeek", terms["time.schedule.employeepost.worktimeweek"], 150, { enableHiding: true, minutesToTimeSpan: true });
            this.gridAg.addColumnNumber("workTimePercent", terms["time.employee.employment.percent"], 150, { enableHiding: true, decimals: 2 });
            this.gridAg.addColumnText("employeeGroupName", terms["time.schedule.employeepost.employeegroupname"], null, true);
            this.gridAg.addColumnText("skillNames", terms["time.schedule.employeepost.skills"], null, true);
            this.gridAg.addColumnText("dayOfWeeksGridString", terms["time.schedule.employeepost.vacationweekdays"], null, true);
            this.gridAg.addColumnNumber("workDaysWeek", terms["time.schedule.employeepost.nbrofdaysinweek"], null, { enableHiding: true, decimals: 0 });
            this.gridAg.addColumnText("scheduleCycleDTO.name", terms["time.schedule.employeepost.schedulecycle"], null, true);
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this));

            this.gridAg.finalizeInitGrid("time.employee.employee.employees", true);
        });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.reloadData());

        let group = ToolBarUtility.createGroup(new ToolBarButton("time.schedule.employeepost.createposts", "time.schedule.employeepost.createposts", IconLibrary.FontAwesome, "fa-plus",
            () => { this.openCreateDialog(); }
        ));
        group.buttons.push(new ToolBarButton("time.schedule.employeepost.deleteposts", "time.schedule.employeepost.deleteposts", IconLibrary.FontAwesome, "fa-times",
            () => { this.deleteEmployeePosts(); },
            () => { return this.gridAg.options.getSelectedCount() === 0 }
        ));

        this.toolbar.addButtonGroup(group);
    }

    public loadGridData() {
        this.progress.startLoadingProgress([() => {
            return this.scheduleService.getEmployeePosts(null, true).then(x => {
                return x;
            }).then(data => {
                this.setData(data);
            });
        }]);
    }

    private reloadData() {
        this.loadGridData();
    }

    private openCreateDialog() {
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Schedule/EmployeePosts/Dialogs/Views/createPosts.html"),
            controller: CreatePostsController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'lg',
            resolve: {

            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            this.reloadData();
        });
    }

    private deleteEmployeePosts() {
        this.progress.startDeleteProgress((completion) => {
            this.scheduleService.deleteEmployeePosts(this.gridAg.options.getSelectedIds("employeePostId")).then((result) => {
                if (result.success) {
                    completion.completed(null, true);
                    this.reloadData();
                } else {
                    completion.failed(result.errorMessage);
                    this.reloadData();
                }
            }, error => {
                completion.failed(error.message);
                this.reloadData();
            });
        }, null, this.terms["core.deleteselectedwarning"])
            .catch((reason) => { });
    }

    public edit(row) {
        // Send message to TabsController
        if (this.readPermission || this.modifyPermission)
            this.messagingHandler.publishEditRow(row);
    }
}