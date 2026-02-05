import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IScheduleService } from "../ScheduleService";
import { ITimeService } from "../../Time/timeservice";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IProgressHandler } from "../../../Core/Handlers/ProgressHandler";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { CompanySettingType, Feature, TermGroup } from "../../../Util/CommonEnumerations";
import { SettingsUtility } from "../../../Util/SettingsUtility";

enum ViewMode {
    Employee,
    Attest
}

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    // Terms
    private terms: { [index: string]: string; };

    // Toolbar
    private toolbarInclude: any;

    //Data
    private timeDeviationCauses: any[] = [];
    private statuses: any[] = [];
    private resultStatuses: any[] = [];

    // Properties
    private feature: Feature = soeConfig.feature;
    private viewMode: ViewMode = ViewMode.Employee;
    private employeeId: number = (soeConfig.employeeId) ? soeConfig.employeeId : 0;
    private employeegroupId: number = (soeConfig.employeeGroupId) ? soeConfig.employeeGroupId : 0;
    private useAccountsHierarchy: boolean;

    private get isEmployeeMode(): boolean {
        return this.viewMode == ViewMode.Employee;
    }

    private get isAttestRoleMode(): boolean {
        return this.viewMode == ViewMode.Attest;
    }

    private _loadPreliminary: boolean = true;
    get loadPreliminary() {
        return this._loadPreliminary;
    }
    set loadPreliminary(item: boolean) {
        this._loadPreliminary = item;
        this.loadGridData();
    }

    private _loadDefinitive: boolean = false;
    get loadDefinitive() {
        return this._loadDefinitive;
    }
    set loadDefinitive(item: boolean) {
        this._loadDefinitive = item;
        this.loadGridData();
    }

    //@ngInject
    constructor(
        private $timeout: ng.ITimeoutService,
        private $q: ng.IQService,
        private urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        private scheduleService: IScheduleService,
        private timeService: ITimeService,
        private coreService: ICoreService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {
        super(gridHandlerFactory, "Time.Schedule.Absencerequests", progressHandlerFactory, messagingHandlerFactory);

        this.feature = soeConfig.feature;
        if (this.feature === Feature.Time_Schedule_AbsenceRequestsUser)
            this.viewMode = ViewMode.Employee;
        else if (this.feature == Feature.Time_Schedule_AbsenceRequests)
            this.viewMode = ViewMode.Attest;

        this.toolbarInclude = this.urlHelperService.getViewUrl("gridHeader.html");

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            //.onDoLookUp(() => this.loadCompanySettings())
            .onBeforeSetUpGrid(() => this.loadCompanySettings())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onBeforeSetUpGrid(() => this.loadTerms())
            .onBeforeSetUpGrid(() => this.loadTimeDeviationCauses())
            .onBeforeSetUpGrid(() => this.loadStatuses())
            .onBeforeSetUpGrid(() => this.loadResultStatuses())
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    // SETUP

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
            { feature: soeConfig.feature, loadReadPermissions: true, loadModifyPermissions: true },
        ]);
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[soeConfig.feature].readPermission;
        this.modifyPermission = response[soeConfig.feature].modifyPermission;

        if (this.modifyPermission)
            this.messagingHandler.publishActivateAddTab();
    }

    public setupGrid() {

        this.gridAg.options.useGrouping(false, false, { keepColumnsAfterGroup: false, selectChildren: false });
        this.gridAg.options.groupHideOpenParents = true;

        this.gridAg.addColumnSelect("timeDeviationCauseName", this.terms["common.time.timedeviationcause"], null, { displayField: "timeDeviationCauseName", selectOptions: this.timeDeviationCauses, dropdownValueLabel: "name", enableHiding: true, enableRowGrouping: true });
        this.gridAg.addColumnDate("start", this.terms["common.from"], 135, false, null, { suppressSizeToFit: true, enableRowGrouping: true });
        this.gridAg.addColumnDate("stop", this.terms["common.to"], 135, false, null, { suppressSizeToFit: true, enableRowGrouping: true });
        this.gridAg.addColumnDate("created", this.terms["core.created"], 135, true, null, { suppressSizeToFit: true, enableRowGrouping: true });
        this.gridAg.addColumnSelect("statusName", this.terms["common.status"], null, { displayField: "statusName", selectOptions: this.statuses, dropdownValueLabel: "name", enableHiding: true, enableRowGrouping: true });
        this.gridAg.addColumnSelect("resultStatusName", this.terms["time.schedule.absencerequests.result"], null, { displayField: "resultStatusName", selectOptions: this.resultStatuses, dropdownValueLabel: "name", enableHiding: true, enableRowGrouping: true });
        if (this.isAttestRoleMode) {
            this.gridAg.addColumnText("employeeName", this.terms["common.employee"], null, false, { enableRowGrouping: true });
            if (this.useAccountsHierarchy)
                this.gridAg.addColumnText("accountNamesString", this.terms["time.employee.employee.accountswithdefault"], null, true, { enableRowGrouping: true });
            else
                this.gridAg.addColumnText("categoryNamesString", this.terms["time.employee.employee.categories"], null, true, { enableRowGrouping: true });
        }
        this.gridAg.addColumnText("comment", this.terms["common.note"], null, true);
        this.gridAg.addColumnEdit(this.terms["core.edit"], this.edit.bind(this));

        this.gridAg.options.enableRowSelection = false;
        this.gridAg.finalizeInitGrid("time.schedule.absencerequests.absencerequests", true);
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.reloadData());
        this.toolbar.addInclude(this.toolbarInclude);
    }

    // SERVICE CALLS

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];
      
        settingTypes.push(CompanySettingType.UseAccountHierarchy);

        return this.coreService.getCompanySettings(settingTypes).then(x => {         
            this.useAccountsHierarchy = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseAccountHierarchy);        
        });
    }

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "core.created",
            "core.edit",
            "common.employee",
            "common.from",
            "common.to",
            "common.note",
            "common.status",
            "common.time.timedeviationcause",
            "time.employee.employee.categories",
            "time.employee.employee.accountswithdefault",
            "time.schedule.absencerequests.result"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private loadTimeDeviationCauses(): ng.IPromise<any> {
        let deferral = this.$q.defer();

        if (this.isEmployeeMode) {
            this.timeService.getTimeDeviationCauseRequestsDict(this.employeegroupId, false).then(x => {
                this.timeDeviationCauses = x;
                deferral.resolve();
            });
        } else {
            this.timeService.getTimeDeviationCausesDict(false, false).then(x => {
                this.timeDeviationCauses = x;
                deferral.resolve();
            });
        }

        return deferral.promise;
    }

    private loadStatuses(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.EmployeeRequestStatus, false, false).then((x) => {
            this.statuses = x;
        });
    }

    private loadResultStatuses(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.EmployeeRequestResultStatus, false, false).then((x) => {
            this.resultStatuses = x;
        });
    }

    public loadGridData() {
        this.progress.startLoadingProgress([() => {
            return this.scheduleService.getAbsenceRequests(this.employeeId, this.loadPreliminary, this.loadDefinitive).then(x => {
                return x;
            }).then(data => {
                this.setData(data);
            });
        }]);
    }

    private reloadData() {
        this.loadGridData();
    }

    // ACTIONS

    public edit(row) {
        // Send message to TabsController
        if (this.readPermission || this.modifyPermission)
            this.messagingHandler.publishEditRow(row);
    }
}
