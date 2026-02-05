import { AccountDimSmallDTO } from "../../../../../../../Common/Models/AccountDimDTO";
import { AccountDTO } from "../../../../../../../Common/Models/AccountDTO";
import { EmployeeTemplateEmployeeAccountDTO } from "../../../../../../../Common/Models/EmployeeTemplateDTOs";
import { ICoreService } from "../../../../../../../Core/Services/CoreService";
import { ITranslationService } from "../../../../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../../../../Core/Services/UrlHelperService";
import { CalendarUtility } from "../../../../../../../Util/CalendarUtility";
import { CompanySettingType } from "../../../../../../../Util/CommonEnumerations";
import { SettingsUtility } from "../../../../../../../Util/SettingsUtility";

export class EmployeeAccountFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Time/Employee/EmployeeTemplates/Directives/TemplateDesigner/Components/EmployeeAccount/EmployeeAccount.html'),
            scope: {
                model: '=',
                employmentStartDate: '=',
                isEditMode: '=',
                setDefault: '=',
                onChange: '&'
            },
            restrict: 'E',
            replace: true,
            controller: EmployeeAccountController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

export class EmployeeAccountController {

    // Init parameters
    private model: string;
    private employmentStartDate: Date;
    private employeeAccount: EmployeeTemplateEmployeeAccountDTO;
    private isEditMode: boolean;
    private setDefault: boolean;
    private onChange: Function;

    // Company settings
    private defaultEmployeeAccountDimId: number;
    private useLimitedEmployeeAccountDimLevels: boolean;
    private useExtendedEmployeeAccountDimLevels: boolean;

    // Data
    private accountDims: AccountDimSmallDTO[] = [];
    private accounts: AccountDTO[] = [];
    private childAccounts: AccountDTO[] = [];
    private subChildAccounts: AccountDTO[] = [];

    // Properties
    private _selectedAccountDim: AccountDimSmallDTO;
    private get selectedAccountDim(): AccountDimSmallDTO {
        return this._selectedAccountDim;
    }
    private set selectedAccountDim(accountDim: AccountDimSmallDTO) {
        this._selectedAccountDim = accountDim;

        this.childAccountDim = accountDim ? this.accountDims.find(d => d.level === accountDim.level + 1) : null;
        this.subChildAccountDim = accountDim ? this.accountDims.find(d => d.level === accountDim.level + 2) : null;

        this.showChild = (accountDim && accountDim.level < _.max(this.accountDims.map(d => d.level)));
    }
    private get selectedAccountDimName(): string {
        return this.selectedAccountDim ? this.selectedAccountDim.name + (this.isEditMode ? ' *' : '') : '';
    }

    private childAccountDim: AccountDimSmallDTO;
    private subChildAccountDim: AccountDimSmallDTO;
    private showChild = false;
    private showSubChild = false;

    private _selectedAccount: AccountDTO;
    private get selectedAccount(): AccountDTO {
        return this._selectedAccount;
    }
    private set selectedAccount(account: AccountDTO) {
        this._selectedAccount = account;
        this.loadChildAccounts();
    }

    private _selectedChildAccount: AccountDTO;
    private get selectedChildAccount(): AccountDTO {
        return this._selectedChildAccount;
    }
    private set selectedChildAccount(account: AccountDTO) {
        this._selectedChildAccount = account;
        this.loadSubChildAccounts();
    }

    private defaultLabel: string;

    private selectedSubChildAccount: AccountDTO;
    private dateFrom: Date;
    private dateTo: Date;
    private mainAllocation: boolean;
    private default: boolean;

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private $timeout: ng.ITimeoutService,
        private translationService: ITranslationService,
        private coreService: ICoreService) {

        this.translationService.translate("time.employee.employeeaccount.default").then(term => {
            this.defaultLabel = "/" + term;
        });

        this.$q.all([
            this.loadCompanySettings(),
            this.loadAccounts()
        ]).then(() => {
            this.loadAccountDims().then(() => {
                if (this.model) {
                    this.setModel();
                } else {
                    if (!this.isEditMode)
                        this.selectedAccountDim = this.accountDims.find(a => a.accountDimId === this.defaultEmployeeAccountDimId);

                    if (this.setDefault)
                        this.default = true;
                    else
                        this.default = false;
                }
            });
        });

        if (!this.isEditMode) {
            this.$scope.$watch(() => this.employmentStartDate, (newVal, oldVal) => {
                if (newVal !== oldVal)
                    this.dateFrom = newVal;
            });
        }
    }

    // SERVICE CALLS

    private loadCompanySettings(): ng.IPromise<any> {
        const settingTypes: number[] = [];
        settingTypes.push(CompanySettingType.DefaultEmployeeAccountDimEmployee);
        settingTypes.push(CompanySettingType.UseLimitedEmployeeAccountDimLevels);
        settingTypes.push(CompanySettingType.UseExtendedEmployeeAccountDimLevels);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.defaultEmployeeAccountDimId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.DefaultEmployeeAccountDimEmployee);
            this.useLimitedEmployeeAccountDimLevels = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseLimitedEmployeeAccountDimLevels);
            this.useExtendedEmployeeAccountDimLevels = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseExtendedEmployeeAccountDimLevels);
            this.showChild = this.useLimitedEmployeeAccountDimLevels;
            this.showSubChild = this.useExtendedEmployeeAccountDimLevels;
        });
    }

    private loadAccountDims(): ng.IPromise<any> {
        this.accountDims = [];
        return this.coreService.getAccountDimsSmall(false, true, true, false, true, false, false).then(x => {
            // Only add account dims that the user is permitted to see
            const permittedAccountDimIds: number[] = _.uniq(_.map(this.accounts, a => a.accountDimId));

            // Add default account dim from company setting if not already present
            if (!permittedAccountDimIds.includes(this.defaultEmployeeAccountDimId))
                permittedAccountDimIds.push(this.defaultEmployeeAccountDimId);

            _.forEach(x, dim => {
                if (_.includes(permittedAccountDimIds, dim.accountDimId))
                    this.accountDims.push(dim);
            });

            this.addDefaultEmployeeAccountDimIfMissing();
        });
    }

    private loadAccounts(): ng.IPromise<any> {
        return this.coreService.getAccountsFromHierarchyByUser(CalendarUtility.getDateToday(), CalendarUtility.getDateToday()).then(x => {
            this.accounts = x;
        });
    }

    private addDefaultEmployeeAccountDimIfMissing() {
        if (this.accounts.length > 0) {
            // First account account is the highest in the hierarchy.
            // Check if first account's parentAccountDimId matches the defaultEmployeeAccountDimId.
            // In that case, we need to add the parent account to the accounts list for that account dim to be selectable.
            // This is needed in case the user has no direct access to accounts in the default employee account dim,
            // but still needs to add it as top level for the employee it's creating.
            const firstAccount = this.accounts[0];
            const parentAccountDimId = firstAccount.accountDim?.parentAccountDimId;
            if (parentAccountDimId && parentAccountDimId === this.defaultEmployeeAccountDimId && firstAccount.virtualParentAccountId) {
                // Create a simple place holder account for the parent account
                const parentAccount: AccountDTO = new AccountDTO();
                parentAccount.accountId = firstAccount.virtualParentAccountId;
                parentAccount.name = firstAccount['parentHierachy'][firstAccount.virtualParentAccountId];
                parentAccount.accountDimId = parentAccountDimId;
                this.accounts.push(parentAccount);

                // Add the parent account to the account dim's accounts list
                const parentDim = this.accountDims.find(d => d.accountDimId === parentAccountDimId);
                if (!parentDim.accounts)
                    parentDim.accounts = [];
                parentDim.accounts.push(parentAccount);
            }
        }
    }

    private loadChildAccounts(): ng.IPromise<any> {
        const deferral = this.$q.defer();

        this.childAccounts = [];
        if (this.selectedAccount) {
            this.coreService.getAccountsFromHierarchy(this.selectedAccount.accountId, true, true).then(x => {
                this.childAccounts = x.filter(y => this.accounts.map(a => a.accountId).includes(y.accountId));

                const emptyAccount: AccountDTO = new AccountDTO();
                emptyAccount.accountId = 0;
                emptyAccount.name = '';
                this.childAccounts.splice(0, 0, emptyAccount);

                deferral.resolve();
            });
        } else {
            deferral.resolve();
        }

        return deferral.promise;
    }

    private loadSubChildAccounts(): ng.IPromise<any> {
        const deferral = this.$q.defer();

        this.subChildAccounts = [];
        if (this.selectedChildAccount) {
            this.coreService.getAccountsFromHierarchy(this.selectedChildAccount.accountId, true, true).then(x => {
                this.subChildAccounts = x.filter(y => this.accounts.map(a => a.accountId).includes(y.accountId));

                const emptyAccount: AccountDTO = new AccountDTO();
                emptyAccount.accountId = 0;
                emptyAccount.name = '';
                this.subChildAccounts.splice(0, 0, emptyAccount);

                deferral.resolve();
            });
        } else {
            deferral.resolve();
        }

        return deferral.promise;
    }

    // EVENTS

    private setDirty() {
        if (this.onChange) {
            this.$timeout(() => {
                this.onChange({ jsonString: this.getJsonFromModel() });
            });
        }
    }

    // HELP-METHODS

    private getJsonFromModel(): string {
        this.employeeAccount = new EmployeeTemplateEmployeeAccountDTO();
        this.employeeAccount.accountDimId = this.selectedAccountDim?.accountDimId;
        this.employeeAccount.accountId = this.selectedAccount?.accountId;
        this.employeeAccount.childAccountId = this.selectedChildAccount?.accountId;
        this.employeeAccount.subChildAccountId = this.selectedSubChildAccount?.accountId;
        this.employeeAccount.dateFrom = this.dateFrom;
        this.employeeAccount.dateFromString = this.dateFrom ? this.dateFrom.toDateTimeString() : undefined;
        this.employeeAccount.dateTo = this.dateTo;
        this.employeeAccount.dateToString = this.dateTo ? this.dateTo.toDateTimeString() : undefined;
        this.employeeAccount.mainAllocation = this.mainAllocation;
        this.employeeAccount.default = this.default;

        return JSON.stringify(this.employeeAccount);
    }

    private setModel() {
        this.employeeAccount = new EmployeeTemplateEmployeeAccountDTO();
        angular.extend(this.employeeAccount, JSON.parse(this.model));
        this.employeeAccount.fixDates();

        if (this.employeeAccount.accountDimId)
            this.selectedAccountDim = this.accountDims.find(a => a.accountDimId === this.employeeAccount.accountDimId);
        else if (!this.isEditMode)
            this.selectedAccountDim = this.accountDims.find(a => a.accountDimId === this.defaultEmployeeAccountDimId);

        if (this.selectedAccountDim && this.employeeAccount.accountId) {
            this._selectedAccount = this.accounts.find(a => a.accountId === this.employeeAccount.accountId);
            this.loadChildAccounts().then(() => {
                if (this.selectedAccount && this.employeeAccount.childAccountId) {
                    this._selectedChildAccount = this.childAccounts.find(ca => ca.accountId === this.employeeAccount.childAccountId);
                    this.loadSubChildAccounts().then(() => {
                        if (this.selectedChildAccount && this.employeeAccount.subChildAccountId) {
                            this.selectedSubChildAccount = this.subChildAccounts.find(sca => sca.accountId === this.employeeAccount.subChildAccountId);
                        }
                    });
                }
            });
        }

        if (this.employeeAccount.dateFrom)
            this.dateFrom = this.employeeAccount.dateFrom;
        if (this.employeeAccount.dateTo)
            this.dateTo = this.employeeAccount.dateTo;
        if (this.employeeAccount.mainAllocation)
            this.mainAllocation = true;
        if (this.employeeAccount.default)
            this.default = true;
        else
            this.default = false;
    }
}
