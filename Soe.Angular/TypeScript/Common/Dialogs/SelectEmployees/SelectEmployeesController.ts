import { EmbeddedGridController } from "../../../Core/Controllers/EmbeddedGridController";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { IProgressHandler } from "../../../Core/Handlers/ProgressHandler";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { EmployeeService as SharedEmployeeService } from "../../../Shared/Time/Employee/EmployeeService";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { CompanySettingType, Feature } from "../../../Util/CommonEnumerations";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { EmployeeGridDTO } from "../../Models/EmployeeUserDTO";

export class SelectEmployeesController {
    // Terms
    private terms: { [index: string]: string; };

    // Permissions
    private employmentPermission: boolean = false;
    private payrollGroupPermission: boolean = false;

    // Company settings
    private useAccountsHierarchy: boolean = false;

    // Data
    private employees: EmployeeGridDTO[] = [];

    // Grid
    private gridHandler: EmbeddedGridController;

    private progress: IProgressHandler;

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private $uibModalInstance,
        private translationService: ITranslationService,
        private coreService: ICoreService,
        private sharedEmployeeService: SharedEmployeeService,
        private progressHandlerFactory: IProgressHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {

        if (progressHandlerFactory)
            this.progress = progressHandlerFactory.create();

        this.gridHandler = new EmbeddedGridController(gridHandlerFactory, "SelectEmployees");

        this.$q.all([
            this.loadTerms(),
            this.loadReadOnlyPermissions(),
            this.loadCompanySettings()
        ]).then(() => {
            this.setupGrid();
            this.loadEmployees();
        });
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total",
            "core.edit",
            "common.name",
            "common.start",
            "common.end",
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
            "time.schedule.planning.worktimeweek"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private loadReadOnlyPermissions() {
        var featureIds: number[] = [];
        featureIds.push(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Employment);
        featureIds.push(Feature.Time_Employee_PayrollGroups);

        return this.coreService.hasReadOnlyPermissions(featureIds).then((x) => {
            this.employmentPermission = x[Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Employment];
            this.payrollGroupPermission = x[Feature.Time_Employee_PayrollGroups];
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];
        settingTypes.push(CompanySettingType.UseAccountHierarchy);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.useAccountsHierarchy = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseAccountHierarchy);
        });
    }

    // SETUP

    private setupGrid() {
        this.gridHandler.gridAg.options.enableGridMenu = false;
        this.gridHandler.gridAg.options.enableFiltering = true;
        this.gridHandler.gridAg.options.enableRowSelection = true;
        this.gridHandler.gridAg.options.disableHorizontalScrollbar = true;
        this.gridHandler.gridAg.options.setMinRowsToShow(20);

        var colDefNr = this.gridHandler.gridAg.addColumnText("employeeNr", this.terms["time.employee.employee.employeenrshort"], 100);
        colDefNr.comparator = (valueA: string, valueB: string, nodeA: any, nodeB: any, isInverted: boolean) => {
            return valueA.padLeft(50, '0').toLowerCase().localeCompare(valueB.padLeft(50, '0').toLowerCase());
        };
        this.gridHandler.gridAg.addColumnText("name", this.terms["common.name"], null);
        if (this.useAccountsHierarchy)
            this.gridHandler.gridAg.addColumnText("accountNamesString", this.terms["time.employee.employee.accountswithdefault"], null, true);
        else
            this.gridHandler.gridAg.addColumnText("categoryNamesString", this.terms["time.employee.employee.categories"], null, true);
        if (this.employmentPermission) {
            this.gridHandler.gridAg.addColumnText("employeeGroupNamesString", this.terms["time.employee.employee.employeegroupname"], null, true);
            this.gridHandler.gridAg.addColumnNumber("percent", this.terms["time.employee.employment.percent"], null, { enableHiding: true, decimals: 2, clearZero: true });
            this.gridHandler.gridAg.addColumnTimeSpan("workTimeWeekFormatted", this.terms["time.schedule.planning.worktimeweek"], null, { enableHiding: true, clearZero: true });
        }
        if (this.payrollGroupPermission)
            this.gridHandler.gridAg.addColumnText("payrollGroupNamesString", this.terms["time.employee.employee.payrolgroupname"], null, true);

        this.gridHandler.gridAg.finalizeInitGrid("common.dialogs.selectemployees", true);
    }

    private loadEmployees() {
        this.progress.startLoadingProgress([() => {
            return this.sharedEmployeeService.getEmployeesForGrid(this.payrollGroupPermission, false, false, false, false, CalendarUtility.getDateToday()).then(x => {
                return x;
            }).then(data => {
                this.employees = data;

                this.setData();
            });
        }]);
    }

    private setData() {
        this.gridHandler.gridAg.setData(this.employees);
    }

    // EVENTS

    private cancel() {
        this.$uibModalInstance.dismiss('cancel');
    }

    private ok() {
        this.$uibModalInstance.close({ selectedEmployees: this.gridHandler.gridAg.options.getSelectedRows() });
    }
}