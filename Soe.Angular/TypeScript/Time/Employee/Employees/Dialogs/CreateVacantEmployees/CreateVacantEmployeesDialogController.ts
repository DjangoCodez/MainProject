import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { IEmployeeService } from "../../../EmployeeService";
import { CompanySettingType, SoeCategoryType } from "../../../../../Util/CommonEnumerations";
import { SettingsUtility } from "../../../../../Util/SettingsUtility";
import { CalendarUtility } from "../../../../../Util/CalendarUtility";
import { IEmployeeGroupSmallDTO, IActionResult } from "../../../../../Scripts/TypeLite.Net4";
import { CategoryDTO, CompanyCategoryRecordDTO } from "../../../../../Common/Models/Category";
import { AccountDTO } from "../../../../../Common/Models/AccountDTO";
import { AccountDimSmallDTO } from "../../../../../Common/Models/AccountDimDTO";
import { CreateVacantEmployeeDTO, EmployeeAccountDTO } from "../../../../../Common/Models/EmployeeUserDTO";
import { EditVacantEmployeeDialogController } from "./EditVacantEmployeeDialogController";
import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { SOEMessageBoxImage } from "../../../../../Util/Enumerations";
import { IAccountingService } from "../../../../../Shared/Economy/Accounting/AccountingService";

export class CreateVacantEmployeesDialogController {

    // Terms
    private terms: { [index: string]: string; };
    private firstNameLabelValue: string;
    private counterStartLabelValue: string;
    private employeeGroupLabel: string;
    private employeeGroupWorkTimeWeekLabelValue: string;

    // Company settings
    private useAccountsHierarchy: boolean = false;
    private defaultEmployeeAccountDimId: number = 0;

    // Data
    private employees: CreateVacantEmployeeDTO[] = [];
    private employeeGroups: IEmployeeGroupSmallDTO[] = [];
    private categories: CategoryDTO[] = [];

    // Properties
    private employeeNrStart: number = 1;
    private firstName: string;
    private counterStart: number = 1;
    private quantity: number = 0;
    private dateFrom: Date = CalendarUtility.getDateToday();
    private employeeGroup: IEmployeeGroupSmallDTO;
    private workTimeWeek: number = 0;
    private percent: number = 100;
    private accountRecords: EmployeeAccountDTO[] = [];
    private categoryRecords: CompanyCategoryRecordDTO[] = [];

    get workTimeWeekFormatted(): string {
        return CalendarUtility.minutesToTimeSpan(this.workTimeWeek);
    }
    set workTimeWeekFormatted(time: string) {
        var span = CalendarUtility.parseTimeSpan(time);
        this.workTimeWeek = CalendarUtility.timeSpanToMinutes(span);
    }

    // Flags
    private basisOpen: boolean = true;
    private previewOpen: boolean = false;

    //@ngInject
    constructor(
        private $uibModal,
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private $q: ng.IQService,
        private $timeout: ng.ITimeoutService,
        private urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private coreService: ICoreService,
        private employeeService: IEmployeeService,
        private sharedAccountingService: IAccountingService) {

        this.$q.all([
            this.loadTerms(),
            this.loadCompanySettings(),
            this.getNextEmployeeNumber(),
            this.loadEmployeeGroups()
        ]).then(() => {
            if (!this.useAccountsHierarchy)
                this.loadCategories();
        });
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "common.errormessage",
            "time.employee.employee.vacant",
            "time.employee.employee.firstname",
            "time.employee.employee.lastname",
            "time.employee.employeegroup.employeegroup"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
            this.firstNameLabelValue = this.terms["time.employee.employee.firstname"].toLocaleLowerCase();
            this.firstName = this.terms["time.employee.employee.vacant"];
            this.counterStartLabelValue = this.terms["time.employee.employee.lastname"].toLocaleLowerCase();
            this.employeeGroupLabel = this.terms["time.employee.employeegroup.employeegroup"];
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];
        settingTypes.push(CompanySettingType.UseAccountHierarchy);
        settingTypes.push(CompanySettingType.DefaultEmployeeAccountDimEmployee);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.useAccountsHierarchy = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseAccountHierarchy);
            this.defaultEmployeeAccountDimId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.DefaultEmployeeAccountDimEmployee);
        });
    }

    private getNextEmployeeNumber(): ng.IPromise<any> {
        return this.employeeService.getLastUsedEmployeeSequenceNumber().then(number => {
            this.employeeNrStart = number + 1;
        });
    }

    private loadEmployeeGroups(): ng.IPromise<any> {
        return this.employeeService.getEmployeeGroupsSmall().then(x => {
            this.employeeGroups = x;
        });
    }

    private loadCategories(): ng.IPromise<any> {
        return this.coreService.getCategories(SoeCategoryType.Employee, false, false, false, true).then(x => {
            this.categories = x;
        });
    }

    // EVENTS

    private employeeGroupChanged() {
        this.$timeout(() => {
            this.resetWorkTimeWeek();
            this.clearPreview();
        });
    }

    private workTimeWeekChanged() {
        this.$timeout(() => {
            this.calculateEmploymentPercent();
            this.clearPreview();
        });
    }

    private preview() {
        this.clearPreview();

        let employeeGroupName = this.employeeGroup ? _.find(this.employeeGroups, g => g.employeeGroupId === this.employeeGroup.employeeGroupId).name : '';

        for (let i = 0; i < this.quantity; i++) {
            let employee: CreateVacantEmployeeDTO = new CreateVacantEmployeeDTO();
            employee.employeeNr = (this.employeeNrStart + i).toString();
            employee.firstName = this.firstName;
            employee.lastName = (this.counterStart + i).toString();
            employee.employmentDateFrom = this.dateFrom;
            employee.employeeGroupId = this.employeeGroup.employeeGroupId;
            employee.employeeGroupName = employeeGroupName;
            employee.workTimeWeek = this.workTimeWeek;
            employee.percent = this.percent;

            if (this.useAccountsHierarchy) {
                employee.accounts = this.accountRecords;
                this.setAccountNames(employee);
            } else {
                employee.categories = this.categoryRecords;
                this.setCategoryNames(employee);
            }

            this.employees.push(employee);
        }

        this.previewOpen = (this.employees.length > 0);
        this.basisOpen = !this.previewOpen;
    }

    private editRow(employee: CreateVacantEmployeeDTO) {
        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Employee/Employees/Dialogs/CreateVacantEmployees/EditVacantEmployeeDialog.html"),
            controller: EditVacantEmployeeDialogController,
            controllerAs: "ctrl",
            size: 'lg',
            resolve: {
                useAccountsHierarchy: () => { return this.useAccountsHierarchy },
                defaultEmployeeAccountDimId: () => { return this.defaultEmployeeAccountDimId },
                employeeGroups: () => { return this.employeeGroups },
                employee: () => { return employee },
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result && result.employee) {
                if (!employee) {
                    employee = new CreateVacantEmployeeDTO();
                    this.employees.push(employee);
                }

                let resEmployee: CreateVacantEmployeeDTO = result.employee;
                employee.employeeNr = resEmployee.employeeNr;
                employee.firstName = resEmployee.firstName;
                employee.lastName = resEmployee.lastName;
                employee.employmentDateFrom = resEmployee.employmentDateFrom;
                employee.employeeGroupId = resEmployee.employeeGroupId;
                employee.employeeGroupName = resEmployee.employeeGroupName;
                employee.workTimeWeek = resEmployee.workTimeWeek;
                employee.percent = resEmployee.percent;
                employee.accounts = resEmployee.accounts;
                this.setAccountNames(employee);
                employee.categories = resEmployee.categories;
                this.setCategoryNames(employee);
            }
        });
    }

    private deleteRow(employee: CreateVacantEmployeeDTO) {
        _.pull(this.employees, employee);
    }

    private ok() {
        this.employeeService.createVacantEmployees(this.employees).then((result: IActionResult) => {
            if (result.success)
                this.$uibModalInstance.close();
            else
                this.notificationService.showDialogEx(this.terms["common.errormessage"], result.errorMessage, SOEMessageBoxImage.Error);
        });
    }

    private cancel() {
        this.$uibModalInstance.dismiss('cancel');
    }

    // HELP-METHODS

    private resetWorkTimeWeek() {
        if (this.employeeGroup && this.percent == 100) {
            this.workTimeWeek = this.employeeGroup.ruleWorkTimeWeek;
        }
    }

    private calculateEmploymentPercent() {
        if (this.employeeGroup) {
            this.percent = this.workTimeWeek > 0 ? (100 * (this.workTimeWeek / this.employeeGroup.ruleWorkTimeWeek)).round(2) : 0;
        }
    }

    private setAccountNames(employee: CreateVacantEmployeeDTO) {
        employee.accountNames = '';
        _.forEach(employee.accounts, employeeAccount => {
            if (employee.accountNames.length > 0)
                employee.accountNames += ', ';
            employee.accountNames += employeeAccount.accountName;
            if (employeeAccount.default)
                employee.accountNames += ' (*)';

            _.forEach(employeeAccount.children, child => {
                employee.accountNames += ' - ';
                employee.accountNames += child.accountName;
                if (child.default)
                    employee.accountNames += ' (*)';
            });
        });
    }

    private setCategoryNames(employee: CreateVacantEmployeeDTO) {
        employee.categoryNames = '';
        _.forEach(employee.categories, category => {
            let cat = _.find(this.categories, c => c.categoryId === category.categoryId);
            if (cat) {
                if (employee.categoryNames)
                    employee.categoryNames += ', ';
                employee.categoryNames += cat.name;
                if (category.default)
                    employee.categoryNames += ' (*)';
            }
        });
    }

    private clearPreview() {
        if (this.employees.length > 0) {
            this.employees = [];
            this.previewOpen = false;
        }
    }
}
