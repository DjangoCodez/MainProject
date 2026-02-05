import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { CalendarUtility } from "../../../../../Util/CalendarUtility";
import { EmployeeAccountDTO, ValidatePossibleDeleteOfEmployeeAccountDTO, ValidatePossibleDeleteOfEmployeeAccountRowDTO } from "../../../../../Common/Models/EmployeeUserDTO";
import { EmployeeAccountDialogController } from "./EmployeeAccountDialogController";
import { AccountDimSmallDTO } from "../../../../../Common/Models/AccountDimDTO";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { IQService, ITimeoutService } from "angular";
import { AccountDTO } from "../../../../../Common/Models/AccountDTO";
import { IAccountingService } from "../../../../../Shared/Economy/Accounting/AccountingService";
import { IScheduleService } from "../../../../Schedule/ScheduleService";
import { IActionResult } from "../../../../../Scripts/TypeLite.Net4";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { SOEMessageBoxButtons, SOEMessageBoxImage } from "../../../../../Util/Enumerations";
import { CoreUtility } from "../../../../../Util/CoreUtility";

export class EmployeeAccountsDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Time/Employee/Employees/Directives/EmployeeAccounts/Views/EmployeeAccounts.html'),
            scope: {
                employeeAccounts: '=',
                defaultEmployeeAccountDimId: '=',                
                useLimitedEmployeeAccountDimLevels: '=',
                useExtendedEmployeeAccountDimLevels: '=',
                hasAllowToAddOtherEmployeeAccounts: '=',
                readOnly: '=',
                onChange: '&'
            },
            restrict: 'E',
            replace: true,
            controller: EmployeeAccountsController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

export class EmployeeAccountsController {
    // Init parameters
    private employeeAccounts: EmployeeAccountDTO[];
    private defaultEmployeeAccountDimId: number;
    private useLimitedEmployeeAccountDimLevels: boolean;
    private useExtendedEmployeeAccountDimLevels: boolean;
    private hasAllowToAddOtherEmployeeAccounts: boolean;
    private readOnly: boolean;
    private onChange: Function;

    // Data
    private accounts: AccountDTO[] = [];
    private allAccountDims: AccountDimSmallDTO[] = [];
    private accountDims: AccountDimSmallDTO[] = [];
    private selectedEmployeeAccount: EmployeeAccountDTO;

    private filteredEmployeeAccounts: EmployeeAccountDTO[] = [];
    private hiddenCount: number = 0;

    // Flags
    private showHistory = false;

    //@ngInject
    constructor(
        private $uibModal,
        private $scope: ng.IScope,
        private $q: IQService,
        private $timeout: ITimeoutService,
        private urlHelperService: IUrlHelperService,
        private notificationService: INotificationService,
        private translationService: ITranslationService,
        private coreService: ICoreService,
        private scheduleService: IScheduleService,
        private sharedAccountingService: IAccountingService) {

        this.$q.all([
            this.loadAccountsByUserFromHierarchy()]).then(() => {
                this.$q.all([
                    this.loadAccountDims()
                ]);
            });

        this.setupWatchers();
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.employeeAccounts, (newVal, oldVal) => {
            if (this.allAccountDims.length > 0) {
                _.forEach(this.employeeAccounts, a => {
                    this.setAccountNames(a);
                });
            }

            this.filterEmployeeAccounts();
        });
    }

    // SERVICE CALLS

    private loadAccountsByUserFromHierarchy(): ng.IPromise<any> {
        return this.coreService.getAccountsFromHierarchyByUser(CalendarUtility.getDateToday(), CalendarUtility.getDateToday()).then(x => {
            this.accounts = x;
        });
    }

    private loadAccountDims(): ng.IPromise<any> {
        this.accountDims = [];
        return this.coreService.getAccountDimsSmall(false, true, true, false, true, false, false).then(x => {
            this.allAccountDims = x;

            // Only add account dims that the user is permitted to see
            let permittedAccountDimIds: number[] = _.uniq(_.map(this.accounts, a => a.accountDimId));
            _.forEach(x, dim => {
                if (_.includes(permittedAccountDimIds, dim.accountDimId))
                    this.accountDims.push(dim);
            });

            _.forEach(this.employeeAccounts, a => {
                this.setAccountNames(a);
            });
        });
    }

    private validateChangeEmployeeAccount(model: ValidatePossibleDeleteOfEmployeeAccountDTO, showWarning: boolean): ng.IPromise<boolean> {
        const deferral = this.$q.defer<boolean>();

        this.scheduleService.validatePossibleDeleteOfEmployeeAccount(model).then((result: IActionResult) => {
            if (result.success) {
                if (showWarning) {
                    this.translationService.translateMany(["time.employee.employeeaccount.validatepossibledelete.warning.title", "time.employee.employeeaccount.validatepossibledelete.warning.message"]).then(terms => {
                        const modal = this.notificationService.showDialogEx(terms["time.employee.employeeaccount.validatepossibledelete.warning.title"], terms["time.employee.employeeaccount.validatepossibledelete.warning.message"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                        modal.result.then(val => {
                            deferral.resolve(true);
                        }, (reason) => {
                            deferral.resolve(false);
                        });
                    });
                } else {
                    deferral.resolve(true);
                }
            } else {
                this.translationService.translateMany(["time.employee.employeeaccount.validatepossibledelete.error.title", "time.employee.employeeaccount.validatepossibledelete.error.message"]).then(terms => {
                    // 2217: Employee account is used in time schedule template blocks
                    const message = (result.errorNumber === 2217 ? terms["time.employee.employeeaccount.validatepossibledelete.error.message"] : result.errorMessage);
                    this.notificationService.showErrorDialog(terms["time.employee.employeeaccount.validatepossibledelete.error.title"], message, null);

                    deferral.resolve(false);
                });
            }
        });

        return deferral.promise;
    }

    // EVENTS

    private showHistoryChanged() {
        this.$timeout(() => {
            this.filterEmployeeAccounts();
        });
    }

    private editEmployeeAccount(employeeAccount: EmployeeAccountDTO) {
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Employee/Employees/Directives/EmployeeAccounts/Views/EmployeeAccountDialog.html"),
            controller: EmployeeAccountDialogController,
            controllerAs: "ctrl",
            size: 'xl',
            windowClass: 'fullsize-modal',
            resolve: {
                accountDims: () => { return this.accountDims },
                defaultEmployeeAccountDimId: () => { return this.defaultEmployeeAccountDimId },
                useLimitedEmployeeAccountDimLevels: () => { return this.useLimitedEmployeeAccountDimLevels },
                useExtendedEmployeeAccountDimLevels: () => { return this.useExtendedEmployeeAccountDimLevels },
                hasAllowToAddOtherEmployeeAccounts: () => { return this.hasAllowToAddOtherEmployeeAccounts },
                employeeAccount: () => { return employeeAccount }
            }
        }
        this.$uibModal.open(options).result.then(result => {
            if (result?.employeeAccount) {
                // If modifying an existing employee account, check if it has been shortened
                let updatedEmployeeAccount: EmployeeAccountDTO = result.employeeAccount;

                let originalFlattened = this.flattenEmployeeAccounts(employeeAccount);
                let updatedFlattened = this.flattenEmployeeAccounts(updatedEmployeeAccount);
                let models: ValidatePossibleDeleteOfEmployeeAccountDTO[] = [];

                let existingOnDefaultDim = updatedFlattened.filter(f => f.employeeAccountId && f.accountDimId === this.defaultEmployeeAccountDimId);
                if (existingOnDefaultDim.length > 0) {
                    existingOnDefaultDim.forEach(empAccount => {
                        let originalEmployeeAccount = originalFlattened.find(e => e.employeeAccountId === empAccount.employeeAccountId);
                        if (originalEmployeeAccount) {
                            let row = this.hasEmployeeAccountBeenShortened(originalEmployeeAccount, empAccount);
                            if (row) {
                                let model = new ValidatePossibleDeleteOfEmployeeAccountDTO();
                                model.employeeId = originalEmployeeAccount.employeeId;
                                model.rows = [row];
                                models.push(model);
                            }
                        }
                    });
                }

                let nbrPassed = 0;
                let counter = 0;
                if (models.length > 0) {
                    models.forEach(model => {
                        counter++;
                        this.validateChangeEmployeeAccount(model, counter === models.length).then(passed => {
                            if (passed) {
                                nbrPassed++;
                                if (nbrPassed === models.length)
                                    this.updateEmployeeAccount(employeeAccount, updatedEmployeeAccount)
                            }
                        });
                    });
                } else {
                    this.updateEmployeeAccount(employeeAccount, updatedEmployeeAccount);
                }
            }
        });
    }

    private flattenEmployeeAccounts(updatedEmployeeAccount: EmployeeAccountDTO): EmployeeAccountDTO[] {
        let flattened: EmployeeAccountDTO[] = [];

        if (updatedEmployeeAccount) {
            flattened.push(CoreUtility.cloneDTO(updatedEmployeeAccount));

            if (updatedEmployeeAccount?.children) {
                updatedEmployeeAccount.children.forEach(child => {
                    flattened.push(CoreUtility.cloneDTO(child));

                    if (child?.children) {
                        child.children.forEach(subChild => {
                            flattened.push(CoreUtility.cloneDTO(subChild));
                        });
                    }
                });
            }
        }
        return flattened;
    }

    private hasEmployeeAccountBeenShortened(originalEmployeeAccount: EmployeeAccountDTO, updatedEmployeeAccount: EmployeeAccountDTO): ValidatePossibleDeleteOfEmployeeAccountRowDTO {
        const oldDateFrom = CalendarUtility.convertToDate(originalEmployeeAccount.dateFrom);
        const oldDateTo = CalendarUtility.convertToDate(originalEmployeeAccount.dateTo);
        const newDateFrom = CalendarUtility.convertToDate(updatedEmployeeAccount.dateFrom);
        const newDateTo = CalendarUtility.convertToDate(updatedEmployeeAccount.dateTo);

        // Check if account has been shortened in beginning or end
        const employeeAccuntShortenedInBeginning = newDateFrom.isAfterOnDay(oldDateFrom);
        const employeeAccuntShortenedInEnd = (!!(oldDateTo && newDateTo && newDateTo.isBeforeOnDay(oldDateTo)) ||
            !!(!oldDateTo && newDateTo));

        let row: ValidatePossibleDeleteOfEmployeeAccountRowDTO;

        if (employeeAccuntShortenedInBeginning) {
            row = new ValidatePossibleDeleteOfEmployeeAccountRowDTO();
            row.employeeAccountId = originalEmployeeAccount.employeeAccountId;
            row.dateFrom = oldDateFrom;
            row.dateTo = newDateFrom.addDays(-1);
        } else if (employeeAccuntShortenedInEnd) {
            row = new ValidatePossibleDeleteOfEmployeeAccountRowDTO();
            row.employeeAccountId = originalEmployeeAccount.employeeAccountId;
            row.dateFrom = newDateTo.addDays(1);
            row.dateTo = oldDateTo;
        }

        return row;
    }

    private updateEmployeeAccount(originalEmployeeAccount: EmployeeAccountDTO, updatedEmployeeAccount: EmployeeAccountDTO) {
        if (!originalEmployeeAccount) {
            originalEmployeeAccount = new EmployeeAccountDTO();
            if (!this.employeeAccounts)
                this.employeeAccounts = [];
            this.employeeAccounts.push(originalEmployeeAccount);
        }

        originalEmployeeAccount.mainAllocation = updatedEmployeeAccount.mainAllocation;
        originalEmployeeAccount.default = updatedEmployeeAccount.default;
        originalEmployeeAccount.accountId = updatedEmployeeAccount.accountId;
        originalEmployeeAccount.dateFrom = updatedEmployeeAccount.dateFrom;
        originalEmployeeAccount.dateTo = updatedEmployeeAccount.dateTo;
        originalEmployeeAccount.children = updatedEmployeeAccount.children;
        originalEmployeeAccount.addedOtherEmployeeAccount = updatedEmployeeAccount.addedOtherEmployeeAccount;
        this.setAccountNames(originalEmployeeAccount);

        this.selectedEmployeeAccount = originalEmployeeAccount;

        this.filterEmployeeAccounts();

        if (this.onChange)
            this.onChange();
    }

    private initDeleteEmployeeAccount(employeeAccount: EmployeeAccountDTO) {
        if (employeeAccount.employeeAccountId) {
            let model = new ValidatePossibleDeleteOfEmployeeAccountDTO();
            model.employeeId = employeeAccount.employeeId;

            let row = new ValidatePossibleDeleteOfEmployeeAccountRowDTO();
            row.employeeAccountId = employeeAccount.employeeAccountId;
            row.dateFrom = employeeAccount.dateFrom;
            row.dateTo = employeeAccount.dateTo;
            model.rows = [row];

            this.validateChangeEmployeeAccount(model, true).then(passed => {
                if (passed)
                    this.deleteEmployeeAccount(employeeAccount);
            });
        } else {
            this.deleteEmployeeAccount(employeeAccount);
        }
    }

    private deleteEmployeeAccount(employeeAccount: EmployeeAccountDTO) {
        _.pull(this.employeeAccounts, employeeAccount);

        // Set default on first
        if (employeeAccount.default && this.employeeAccounts.length > 0 && _.filter(this.employeeAccounts, a => a.default).length === 0) {
            this.employeeAccounts[0].default = true;
        }

        this.filterEmployeeAccounts();

        if (this.onChange)
            this.onChange();
    }

    private setAccountNames(employeeAccount: EmployeeAccountDTO) {
        this.setAccountName(employeeAccount);

        // Children
        _.forEach(employeeAccount.children, child => {
            this.setAccountName(child);

            // Sub children
            _.forEach(child.children, subChild => {
                this.setAccountName(subChild);
            });
        });
    }

    private setAccountName(employeeAccount: EmployeeAccountDTO) {
        let dim: AccountDimSmallDTO;
        let account: AccountDTO;
        _.forEach(this.allAccountDims, d => {
            account = _.find(d.accounts, a => a.accountId === employeeAccount.accountId);
            if (account) {
                dim = d;
                return false;
            }
        });

        if (account) {
            employeeAccount.accountDimId = dim?.accountDimId;
            employeeAccount.accountDimName = dim?.name;
            employeeAccount.accountName = account.name;
            employeeAccount.accountNumberName = account.numberName;
        } else {
            this.sharedAccountingService.getAccountSmall(employeeAccount.accountId).then(x => {
                if (x) {
                    dim = _.find(this.allAccountDims, d => d.accountDimId === x.accountDimId);
                    employeeAccount.accountDimId = dim?.accountDimId;
                    employeeAccount.accountDimName = dim?.name;
                    employeeAccount.accountName = x.name;
                    employeeAccount.accountNumberName = x.numberAndName;
                }
            });
        }
    }

    private filterEmployeeAccounts() {
        if (this.showHistory) {
            this.filteredEmployeeAccounts = this.employeeAccounts;
        } else {
            const dateThreshold = new Date(CalendarUtility.getDateToday().addMonths(-3));
            this.filteredEmployeeAccounts = this.employeeAccounts?.filter(entry => {
                const entryDateTo = entry.dateTo ? new Date(entry.dateTo) : null;
                return !entry.dateTo || entryDateTo > dateThreshold;
            });
        }

        if (this.employeeAccounts != null && this.filteredEmployeeAccounts != null)
            this.hiddenCount = this.employeeAccounts.length - this.filteredEmployeeAccounts.length;
    }
}