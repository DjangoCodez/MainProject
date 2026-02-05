import { IUrlHelperService, UrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { EmployeeTimeWorkAccountDTO, TimeWorkAccountDTO, TimeWorkAccountYearEmployeeDTO } from "../../../../../Common/Models/EmployeeUserDTO";
import { TranslationService } from "../../../../../Core/Services/TranslationService";
import { PayrollService } from "../../../../Payroll/PayrollService";
import { EmployeeTimeWorkAccountDialogController } from "./EmployeeTimeWorkAccountDialogController";
import { CoreService } from "../../../../../Core/Services/CoreService";
import { TermGroup } from "../../../../../Util/CommonEnumerations";
import { ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";

export class EmployeeTimeWorkAccountDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Time/Employee/Employees/Directives/EmployeeTimeWorkAccount/Views/EmployeeTimeWorkAccount.html'),
            scope: {
                employeeId: '=',
                readOnly: '=',
                onChange: '&',
                employeeTimeWorkAccounts: '='
            },
            restrict: 'E',
            replace: true,
            controller:  EmployeeTimeWorkAccountController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

export class EmployeeTimeWorkAccountController {

    // Init parameters
    private employeeId: number;

    private readOnly: boolean;

    // Events
    private onChange: Function;

    // Data
    private timeWorkAccounts: TimeWorkAccountDTO[];
    private timeWorkAccountYearEmployee: TimeWorkAccountYearEmployeeDTO[];
    private employeeTimeWorkAccounts: EmployeeTimeWorkAccountDTO[];
    private timeWorkAccountYearEmployeeStatus: ISmallGenericType[];
    private timeWorkAccountYearEmployeeMethods: ISmallGenericType[];
     
    //@ngInject
    constructor(
        private $uibModal,
        private urlHelperService: UrlHelperService,
        private translationService: TranslationService,
        private payrollService: PayrollService,
        private coreService: CoreService,
        private $q: ng.IQService,
        private $scope: ng.IScope) {
    }

    public $onInit() {
        this.$q.all([
            this.loadTimeWorkAccountYearEmployeeStatus(),
            this.loadTimeWorkAccountYearEmployeeMethods(),
            this.loadTimeWorkAccounts(),
            this.loadTimeWorkAccountYearEmployee(),
            this.loadEmployeeTimeWorkAccount()
        ]).then(() => {
            this.load();
        });
      
    }
 
    // LOOKUPS
    private loadTimeWorkAccountYearEmployeeMethods(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.TimeWorkAccountWithdrawalMethod, true, true).then(x => {
            this.timeWorkAccountYearEmployeeMethods = x;
        });
    }
    private loadTimeWorkAccountYearEmployeeStatus(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.TimeWorkAccountYearEmployeeStatus, true, true).then(x => {
            this.timeWorkAccountYearEmployeeStatus = x;
        });
    }

    private loadTimeWorkAccounts(): ng.IPromise<any> {
        return this.payrollService.getTimeWorkAccounts().then(x => {
            this.timeWorkAccounts = x;
        });
    }

    private loadTimeWorkAccountYearEmployee(): ng.IPromise<any> {
        return this.payrollService.getTimeWorkAccountYearEmployee(this.employeeId).then(x => {
            this.timeWorkAccountYearEmployee = x;
        });
    }
    

    private loadEmployeeTimeWorkAccount(): ng.IPromise<any> {
        return this.payrollService.getEmployeeTimeWorkAccount(this.employeeId, true).then((x) => {
            this.employeeTimeWorkAccounts = x;
        });
    }

    private load(): void {
        _.forEach(this.timeWorkAccountYearEmployee, (yearEmployee: TimeWorkAccountYearEmployeeDTO) => {
            yearEmployee.selectedWithdrawalMethodName = _.find(this.timeWorkAccountYearEmployeeMethods, u => u.id === yearEmployee.selectedWithdrawalMethod).name;
            yearEmployee.statusName = _.find(this.timeWorkAccountYearEmployeeStatus, u => u.id === yearEmployee.status).name;
        });
    }

    // EVENTS

    // HELP-METHODS

    private editWorkTimeAccount(worktimeAccount: EmployeeTimeWorkAccountDTO) {
        
        let options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Employee/Employees/Directives/EmployeeTimeWOrkAccount/Views/EmployeeTimeWorkAccountDialog.html"),
            controller: EmployeeTimeWorkAccountDialogController,
            controllerAs: "ctrl",
            size: 'md',
            resolve: {
                timeWorkAccounts: () => { return this.timeWorkAccounts },
                employeeTimeWorkAccount: () => { return worktimeAccount },
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result && result.workTimeAccount) {

                if (!worktimeAccount) {
                    // Add new
                    worktimeAccount = new EmployeeTimeWorkAccountDTO();
                    this.updateWorkTimeAccount(worktimeAccount, result.workTimeAccount);
                    this.employeeTimeWorkAccounts.push(worktimeAccount);
                } else {
                    // Update original 
                    let original = _.find(this.employeeTimeWorkAccounts, f => f.employeeTimeWorkAccountId === worktimeAccount.employeeTimeWorkAccountId);
                    if (original)
                        this.updateWorkTimeAccount(original, result.workTimeAccount);
                }
            }
            if (this.onChange)
                this.onChange();
            
        });
    }

    private updateWorkTimeAccount(worktimeAccount: EmployeeTimeWorkAccountDTO, input: EmployeeTimeWorkAccountDTO) {
        
        let account = _.find(this.timeWorkAccounts, u => u.timeWorkAccountId === input.timeWorkAccountId);
        let accountName: string = account ? account.name : '';

        worktimeAccount.timeWorkAccountId = input.timeWorkAccountId;
        worktimeAccount.timeWorkAccountName = accountName;
        worktimeAccount.dateFrom = input.dateFrom;
        worktimeAccount.dateTo = input.dateTo;
    }

    private delete(worktimeAccount: EmployeeTimeWorkAccountDTO) {
        
        _.pull(this.employeeTimeWorkAccounts, worktimeAccount);

        if (this.onChange)
            this.onChange();
    }

}