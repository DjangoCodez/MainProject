import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { ICoreService } from "../../../Core/Services/CoreService";
import { EmployeeService } from "../EmployeeService";
import { EmployeeService as SharedEmployeeService } from "../../../Shared/Time/Employee/EmployeeService";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { Feature, CompanySettingType, UserSettingType, TermGroup, SoeEntityType, SettingMainType } from "../../../Util/CommonEnumerations";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { Constants } from "../../../Util/Constants";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { CoreUtility } from "../../../Util/CoreUtility";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { SoeGridOptionsEvent, IconLibrary, SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../Util/Enumerations";
import { ToolBarButtonGroup, ToolBarButton, ToolBarUtility } from "../../../Util/ToolBarUtility";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { CreateVacantEmployeesDialogController } from "./Dialogs/CreateVacantEmployees/CreateVacantEmployeesDialogController";
import { ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { EmployeeGridDTO } from "../../../Common/Models/EmployeeUserDTO";
import { InactivateEmployeesController } from "./Dialogs/InactivateEmployees/InactivateEmployeesController";
import { BatchUpdateController } from "../../../Common/Dialogs/BatchUpdate/BatchUpdateDirective";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    private modalInstance: any;

    // Terms
    private terms: { [index: string]: string; };

    // Permissions
    private gdprLogsModifyPermission = false;
    private createVacantPermission = false;
    private employmentPermission = false;
    private payrollGroupPermission = false;
    private socialSecPermission = false;
    private employmentPayrollReadPermission = false;
    private userMappingReadPermission = false;
    private hasMassUpdatePermission: boolean;

    // Company settings
    private useVacant = false;
    private useAccountsHierarchy = false;
    private useAnnualLeave = false;
    private disableAutoLoad = false;

    // Properties
    private _selectedDate: Date;
    private get selectedDate(): Date {
        return this._selectedDate;
    }
    private set selectedDate(date: Date) {
        if (!date)
            date = new Date().date();
        this._selectedDate = date;
    }

    // Data
    private rows: EmployeeGridDTO[] = [];
    private employmentTypes: ISmallGenericType[] = [];
    private genders: ISmallGenericType[] = [];

    // Toolbar
    private toolbarInclude: any;

    // Footer
    private nrOfEmployeesOnLicense = 0;
    private maxNrOfEmployeesOnLicense = 0;

    // Flags
    private showInactive = false;
    private showEnded = false;
    private showNotStarted = false;
    private initialLoadDone = false;
    private showMarkAsVacant = false;
    private gridHasSelectedRows = false;

    // Initial filters
    private startupFilter: any = {
        isActive: ['true']
    };
    private startupFilterVacant: any = {
        values: ['false']
    }

    //@ngInject
    constructor(
        private $scope: ng.IScope,
        private $uibModal,
        private urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private coreService: ICoreService,
        private employeeService: EmployeeService,
        private sharedEmployeeService: SharedEmployeeService,
        private $filter: ng.IFilterService,
        private $timeout: ng.ITimeoutService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {
        super(gridHandlerFactory, "Time.Employee.Employees", progressHandlerFactory, messagingHandlerFactory);

        this.selectedDate = new Date().date();
        this.toolbarInclude = urlHelperService.getViewUrl("gridHeader.html");
        this.modalInstance = $uibModal;

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onDoLookUp(() => this.loadCompanySettings())
            .onBeforeSetUpGrid(() => this.loadTerms())
            .onBeforeSetUpGrid(() => this.loadCompanySettings())
            .onBeforeSetUpGrid(() => this.loadUserSettings())
            .onBeforeSetUpGrid(() => this.loadEmploymentTypes())
            .onBeforeSetUpGrid(() => this.loadGenders())
            .onSetUpGrid(() => this.setupGrid())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))

        this.$scope.$on(Constants.EVENT_RELOAD_GRID, (e, a) => {
            // Called from TabController after using employee template
            this.reloadData();
        })
    }

    public onInit(parameters: any) {
        this.parameters = parameters;
        this.guid = parameters.guid;
        this.isHomeTab = !!parameters.isHomeTab;

        if (this.isHomeTab) {
            this.onTabActivetedAndModified(() => {
                if (this.initialLoadDone)
                    this.reloadData();
            });
        }

        this.flowHandler.start([
            { feature: Feature.Time_Employee_Employees, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Time_Employee_Employees_Create_Vacant_Employees, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Manage_GDPR_Logs, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Employment, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Time_Employee_PayrollGroups, loadReadPermissions: true, loadModifyPermissions: false },
            { feature: Feature.Time_Employee_SocialSec_Show, loadReadPermissions: true, loadModifyPermissions: false },
            { feature: Feature.Time_Employee_Employees_Edit_OtherEmployees_SocialSec, loadReadPermissions: true, loadModifyPermissions: false },
            { feature: Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Payroll, loadReadPermissions: true, loadModifyPermissions: false },
            { feature: Feature.Manage_Users_Edit_UserMapping, loadReadPermissions: true, loadModifyPermissions: false },
            { feature: Feature.Time_Employee_MassUpdateEmployeeFields, loadReadPermissions: true, loadModifyPermissions: true }
        ]);
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Time_Employee_Employees].readPermission;
        this.modifyPermission = response[Feature.Time_Employee_Employees].modifyPermission;
        this.gdprLogsModifyPermission = response[Feature.Manage_GDPR_Logs].modifyPermission;
        this.createVacantPermission = response[Feature.Time_Employee_Employees_Create_Vacant_Employees].modifyPermission;
        this.employmentPermission = response[Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Employment].readPermission;
        this.payrollGroupPermission = response[Feature.Time_Employee_PayrollGroups].readPermission;
        this.socialSecPermission = response[Feature.Time_Employee_SocialSec_Show].readPermission && response[Feature.Time_Employee_Employees_Edit_OtherEmployees_SocialSec].readPermission;
        this.employmentPayrollReadPermission = response[Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Payroll].readPermission;
        this.userMappingReadPermission = response[Feature.Manage_Users_Edit_UserMapping].readPermission;
        this.hasMassUpdatePermission = response[Feature.Time_Employee_MassUpdateEmployeeFields].modifyPermission;

        if (this.modifyPermission)
            this.messagingHandler.publishActivateAddTab();
    }

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total",
            "core.edit",
            "common.name",
            "common.start",
            "common.end",
            "common.user.blockedfromdate",
            "time.employee.employee.active",
            "time.employee.employee.employeenrshort",
            "time.employee.employee.socialsecnr",
            "time.employee.employee.sex",
            "time.employee.employee.age",
            "time.employee.employee.categories",
            "time.employee.employee.employeegroupname",
            "time.employee.employee.payrolgroupname",
            "time.employee.employee.roles",
            "time.employee.employee.vacationgroupname",
            "time.employee.employee.vacant",
            "time.employee.employee.gridlicenseinfo",
            "time.employee.employee.gridlicenseinfo.tooltip",
            "time.employee.employee.accountswithdefault",
            "time.employee.employment.percent",
            "time.employee.employee.employmentenddate",
            "time.employee.employee.employmenttype",
            "time.employee.annualleavegroup",
            "time.schedule.planning.worktimeweek"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        const settingTypes: number[] = [];
        settingTypes.push(CompanySettingType.TimeUseVacant);
        settingTypes.push(CompanySettingType.UseAccountHierarchy);
        settingTypes.push(CompanySettingType.UseAnnualLeave);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.useVacant = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimeUseVacant);
            this.useAccountsHierarchy = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseAccountHierarchy);
            this.useAnnualLeave = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseAnnualLeave);

            this.showMarkAsVacant = this.useVacant && CoreUtility.isSupportAdmin;
        });
    }

    private loadUserSettings(): ng.IPromise<any> {
        const settingTypes: number[] = [];
        settingTypes.push(UserSettingType.EmployeeGridDisableAutoLoad);
        settingTypes.push(UserSettingType.EmployeeGridShowEnded);
        settingTypes.push(UserSettingType.EmployeeGridShowNotStarted);

        return this.coreService.getUserSettings(settingTypes).then(x => {
            this.disableAutoLoad = SettingsUtility.getBoolUserSetting(x, UserSettingType.EmployeeGridDisableAutoLoad);
            this.showEnded = SettingsUtility.getBoolUserSetting(x, UserSettingType.EmployeeGridShowEnded);
            this.showNotStarted = SettingsUtility.getBoolUserSetting(x, UserSettingType.EmployeeGridShowNotStarted);
        });
    }

    private loadEmploymentTypes(): ng.IPromise<any> {
        return this.employeeService.getEmploymentEmploymentTypes(CoreUtility.languageId).then(x => {
            this.employmentTypes = x;
        });
    }

    private loadGenders(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.Sex, false, false, true).then(x => {
            this.genders = x;
        })
    }

    private loadLicenseInfo(): ng.IPromise<any> {
        return this.employeeService.getEmployeeLicenseInfo().then(x => {
            this.nrOfEmployeesOnLicense = x.field1;
            this.maxNrOfEmployeesOnLicense = x.field2;
            this.replaceTotalRow();
        });
    }

    public setupGrid() {
        this.gridAg.options.enableRowSelection = (this.gdprLogsModifyPermission || this.showMarkAsVacant);
        this.gridAg.options.useGrouping(false, false, { keepColumnsAfterGroup: true, selectChildren: false });
        this.gridAg.options.groupHideOpenParents = true;
        this.gridAg.options.addGroupTimeSpanSumAggFunction(true, true);
        
        this.gridAg.addColumnBool("isActive", this.terms["time.employee.employee.active"], 50, false, null, null, true);
        const colDefNr = this.gridAg.addColumnText("employeeNr", this.terms["time.employee.employee.employeenrshort"], 100);
        colDefNr.comparator = (valueA: string, valueB: string, nodeA: any, nodeB: any, isInverted: boolean) => {
            return valueA.padLeft(50, '0').toLowerCase().localeCompare(valueB.padLeft(50, '0').toLowerCase());
        };
        if (this.socialSecPermission)
            this.gridAg.addColumnText("socialSec", this.terms["time.employee.employee.socialsecnr"], 100, true);
        this.gridAg.addColumnText("name", this.terms["common.name"], null);
        if (this.useAccountsHierarchy)
            this.gridAg.addColumnText("accountNamesString", this.terms["time.employee.employee.accountswithdefault"], null, true, { enableRowGrouping: true });
        else
            this.gridAg.addColumnText("categoryNamesString", this.terms["time.employee.employee.categories"], null, true, { enableRowGrouping: true });
        if (this.employmentPermission) {
            this.gridAg.addColumnText("employeeGroupNamesString", this.terms["time.employee.employee.employeegroupname"], null, true, { enableRowGrouping: true });
            this.gridAg.addColumnSelect("employmentTypeString", this.terms["time.employee.employee.employmenttype"], null, { displayField: "employmentTypeString", selectOptions: this.employmentTypes, dropdownValueLabel: "name", enableHiding: true, enableRowGrouping: true });
            this.gridAg.addColumnDate("employmentStart", this.terms["common.start"], null, true, null, { enableRowGrouping: true });
            this.gridAg.addColumnDate("employmentStop", this.terms["common.end"], null, true, null, { enableRowGrouping: true });
            this.gridAg.addColumnDate("employmentEndDate", this.terms["time.employee.employee.employmentenddate"], null, true, null, { enableRowGrouping: true });
            this.gridAg.addColumnDate("userBlockedFromDate", this.terms["common.user.blockedfromdate"], null, true, null, { enableRowGrouping: true });
            this.gridAg.addColumnNumber("percent", this.terms["time.employee.employment.percent"], null, { enableHiding: true, decimals: 2, clearZero: true, enableRowGrouping: true });
            this.gridAg.addColumnTimeSpan("workTimeWeekFormatted", this.terms["time.schedule.planning.worktimeweek"], null, { enableHiding: true, enableRowGrouping: true, aggFuncOnGrouping: "sumTimeSpan", clearZero: true });
        }
        if (this.payrollGroupPermission)
            this.gridAg.addColumnText("payrollGroupNamesString", this.terms["time.employee.employee.payrolgroupname"], null, true, { enableRowGrouping: true });
        if (this.employmentPayrollReadPermission)
            this.gridAg.addColumnText("currentVacationGroupName", this.terms["time.employee.employee.vacationgroupname"], null, true, { enableRowGrouping: true });
        if (this.useAnnualLeave && this.employmentPermission)
            this.gridAg.addColumnText("annualLeaveGroupNamesString", this.terms["time.employee.annualleavegroup"], null, true, { enableRowGrouping: true });
        if (this.userMappingReadPermission)
            this.gridAg.addColumnText("roleNamesString", this.terms["time.employee.employee.roles"], null, true, { enableRowGrouping: true });
        if (this.socialSecPermission) {
            this.gridAg.addColumnSelect("sexString", this.terms["time.employee.employee.sex"], 100, { displayField: "sexString", selectOptions: this.genders, dropdownValueLabel: "name", enableHiding: true, enableRowGrouping: true, hide: true })
            this.gridAg.addColumnNumber("age", this.terms["time.employee.employee.age"], 60, { enableHiding: true, decimals: 0, clearZero: true, enableRowGrouping: true, hide: true });
        }
        if (this.useVacant)
            this.gridAg.addColumnBool("vacant", this.terms["time.employee.employee.vacant"], 80, false);
        this.gridAg.addColumnEdit(this.terms["core.edit"], this.edit.bind(this));

        this.gridAg.finalizeInitGrid("time.employee.employee.employees", true);

        const events: GridEvent[] = [];
        // Not loading data until user grid state is loaded
        // to be able to know what data to load.
        events.push(new GridEvent(SoeGridOptionsEvent.UserGridStateRestored, (hasState: boolean) => { this.loadGridData(); }));
        events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChanged, (row: uiGrid.IGridRow) => { this.gridSelectionChanged(); }));
        events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (row: uiGrid.IGridRow) => { this.gridSelectionChanged(); }));
        events.push(new GridEvent(SoeGridOptionsEvent.FilterChanged, (row: uiGrid.IGridRow) => { this.gridFilterChanged(); }));
        events.push(new GridEvent(SoeGridOptionsEvent.ColumnVisible, (column) => { this.gridColumnVisible(column); }));
        this.gridAg.options.subscribe(events);
    }

    private addTotalRow() {
        this.gridAg.options.addTotalRow("#totals-grid", {
            prefixText: this.terms["time.employee.employee.gridlicenseinfo"].format(this.nrOfEmployeesOnLicense.toString(), this.maxNrOfEmployeesOnLicense.toString()),
            filtered: this.terms["core.aggrid.totals.filtered"],
            total: this.terms["core.aggrid.totals.total"],
            tooltip: this.terms["time.employee.employee.gridlicenseinfo.tooltip"]
        });
    }

    private replaceTotalRow() {
        this.gridAg.options.removeTotalRow("#totals-grid");
        this.addTotalRow();
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.reloadData());

        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("time.employee.employee.delete.action.inactivate", "time.employee.employee.delete.action.inactivate", IconLibrary.FontAwesome, "fa-user-secret",
            () => { this.openInactivateDialog(); },
            () => { return !this.gridHasSelectedRows; },
            () => { return !this.gdprLogsModifyPermission; }
        )));

        if (this.hasMassUpdatePermission) {
            this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("common.batchupdate.title", "common.batchupdate.title", IconLibrary.FontAwesome, "fa-pencil",
                () => { this.openBatchUpdate(); }, () => { return !this.gridHasSelectedRows; }, () => { return false }
            )));
        }

        if (this.useVacant) {
            const vacantGroup: ToolBarButtonGroup = ToolBarUtility.createGroup();
            if (this.createVacantPermission) {
                vacantGroup.buttons.push(new ToolBarButton("time.employee.employee.createvacantemployees", "time.employee.employee.createvacantemployees", IconLibrary.FontAwesome, "fa-user-plus",
                    () => { this.openCreateVacantEmployees(); }
                ));
            }
            vacantGroup.buttons.push(new ToolBarButton("time.employee.employee.markasvacant", "time.employee.employee.markasvacant", IconLibrary.FontAwesome, "fa-newspaper",
                () => { this.initMarkEmployeesAsVacant(); },
                () => { return !this.gridHasSelectedRows; },
                () => { return !this.showMarkAsVacant; }
            ));
            this.toolbar.addButtonGroup(vacantGroup);
        }

        this.toolbar.addInclude(this.toolbarInclude);
    }

    private reloadGridData() {
        this.$timeout(() => {
            this.loadGridData();
        });
    }

    public loadGridData(employeeFilter: number[] = null) {
        this.loadLicenseInfo();

        if (this.disableAutoLoad) {
            this.messagingHandler.publishEvent(Constants.EVENT_SEARCH_EMPLOYEE, null);
            this.disableAutoLoad = false;
        } else {
            this.progress.startLoadingProgress([() => {
                return this.sharedEmployeeService.getEmployeesForGrid(this.payrollGroupPermission, this.showInactive, this.showEnded, this.showNotStarted, this.gridAg.options.isColumnVisible('age'), this.selectedDate, employeeFilter, this.gridAg.options.isColumnVisible('annualLeaveGroupNamesString')).then(x => {
                    this.initialLoadDone = true;
                    return x;
                }).then(resultRows => {
                    if (employeeFilter) {
                        _.forEach(resultRows, (resultRow: EmployeeGridDTO) => {
                            var row = (_.filter(this.rows, { employeeId: resultRow.employeeId }))[0];
                            angular.extend(row, resultRow);
                        });
                    }
                    else
                        this.rows = resultRows;

                    if (this.gridAg.options.isColumnVisible('sexString'))
                        this.setSexString();

                    this.gridHasSelectedRows = false;
                    this.setData(this.rows);
                    this.gridFilterChanged();

                    //Removed due to bug 32457
                    /*if (this.startupFilter) {
                        this.$timeout(() => {
                            this.gridAg.options.setFilter(this.startupFilter);
                            this.startupFilter = null;
                        }, 10);
                    }*/

                    if (this.useVacant && this.startupFilterVacant) {
                        this.$timeout(() => {
                            this.gridAg.options.setFilter('vacant', this.startupFilterVacant);
                        }, 10);
                    }
                });
            }]);
        }
    }

    private reloadData() {
        this.loadGridData();
    }

    private showInactiveChanged() {
        this.$timeout(() => {
            this.reloadData();
        });
    }

    private showEndedChanged() {
        this.$timeout(() => {
            this.coreService.saveBoolSetting(SettingMainType.User, UserSettingType.EmployeeGridShowEnded, this.showEnded);
            this.reloadData();
        });
    }

    private showNotStartedChanged() {
        this.$timeout(() => {
            this.coreService.saveBoolSetting(SettingMainType.User, UserSettingType.EmployeeGridShowNotStarted, this.showNotStarted);
            this.reloadData();
        });
    }

    private openInactivateDialog() {
        const employeeIds = this.gridAg.options.getSelectedIds('employeeId');
        if (!employeeIds)
            return;

        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Employee/Employees/Dialogs/InactivateEmployees/InactivateEmployees.html"),
            controller: InactivateEmployeesController,
            controllerAs: 'ctrl',
            bindToController: true,
            backdrop: 'static',
            size: 'lg',
            resolve: {
                employeeIds: () => { return employeeIds; },
            }
        }
        this.$uibModal.open(options).result.then(result => {
            this.reloadData();
        });
    }

    private initMarkEmployeesAsVacant() {
        const keys = [
            "time.employee.employee.markasvacant",
            "time.employee.employee.markasvacant.warning"
        ];

        this.translationService.translateMany(keys).then(terms => {
            const modal = this.notificationService.showDialogEx(terms["time.employee.employee.markasvacant"], terms["time.employee.employee.markasvacant.warning"].format(this.gridAg.options.getSelectedCount().toString()), SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
            modal.result.then(val => {
                if (val) {
                    this.markEmployeesAsVacant();
                };
            });
        });
    }

    private markEmployeesAsVacant() {
        const employeeIds = this.gridAg.options.getSelectedIds('employeeId');
        this.employeeService.markEmployeesAsVacant(employeeIds).then(result => {
            if (result.success) {
                this.reloadData();
            } else {
                this.translationService.translate("time.employee.employee.markasvacant.error").then(term => {
                    this.notificationService.showDialogEx(term, result.errorMessage, SOEMessageBoxImage.Error);
                });
            }
        });
    }

    private openCreateVacantEmployees() {
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Employee/Employees/Dialogs/CreateVacantEmployees/CreateVacantEmployeesDialog.html"),
            controller: CreateVacantEmployeesDialogController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'lg',
            windowClass: 'fullsize-modal',
            resolve: {

            }
        }
        this.$uibModal.open(options).result.then(result => {
            this.reloadData();
        }, (reason) => {
            // Cancelled
        });
    }

    private openBatchUpdate() {
        let selectedEmployeeIds = _.map(this.gridAg.options.getSelectedRows(), 'employeeId');
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Common/Dialogs/BatchUpdate/Views/BatchUpdate.html"),
            controller: BatchUpdateController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'lg',
            resolve: {
                entityType: () => { return SoeEntityType.Employee },
                selectedIds: () => { return selectedEmployeeIds }
            }
        }
        this.$uibModal.open(options).result.then(result => {
            // Reset cache
            this.loadGridData(selectedEmployeeIds);
        }, (reason) => {
            // Cancelled
        });
        this.$scope.$applyAsync();
    }

    // EVENTS

    private gridSelectionChanged() {
        this.$scope.$applyAsync(() => {
            this.gridHasSelectedRows = this.gridAg.options.getSelectedCount() > 0;
        });
    }

    private gridFilterChanged() {
        this.messagingHandler.publishEvent('employeesFiltered', { rows: this.gridAg.options.getFilteredRows(), totalCount: this.gridAg.options.getData().length });
    }

    private gridColumnVisible(column) {
        if (column && column.colDef) {
            if (column.colDef.field === 'age' && column.visible)
                this.loadGridData();
            else if (column.colDef.field === 'sexString' && column.visible) {
                this.setSexString();
                this.setData(this.rows);
            }
        }
    }

    public edit(row) {
        // Send message to TabsController
        if (this.readPermission || this.modifyPermission)
            this.messagingHandler.publishEditRow(row);
    }

    // HELP-METHODS

    private setSexString() {
        _.forEach(this.rows, row => {
            row.sexString = _.find(this.genders, g => g.id === row.sex).name;
        });
    }
}