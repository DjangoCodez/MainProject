import { EmployeeAccountDTO } from "../../../../../Common/Models/EmployeeUserDTO";
import { AccountDimSmallDTO } from "../../../../../Common/Models/AccountDimDTO";
import { AccountDTO } from "../../../../../Common/Models/AccountDTO";
import { CalendarUtility } from "../../../../../Util/CalendarUtility";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { CoreUtility } from "../../../../../Util/CoreUtility";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";

export class EmployeeAccountDialogController {

    private employeeAccount: EmployeeAccountDTO;
    private isNew: boolean;
    private readOnly = false;
    private defaultLabel: string;

    private _selectedAccountDim: AccountDimSmallDTO;
    private get selectedAccountDim(): AccountDimSmallDTO {
        return this._selectedAccountDim;
    }
    private set selectedAccountDim(accountDim: AccountDimSmallDTO) {
        this._selectedAccountDim = accountDim;

        this.childAccountDim = accountDim ? this.accountDims.find(d => d.level === accountDim.level + 1) : null;
        this.subChildAccountDim = accountDim ? this.accountDims.find(d => d.level === accountDim.level + 2) : null;

        this.showChildren = !this.useLimitedEmployeeAccountDimLevels && (accountDim && accountDim.level < _.max(this.accountDims.map(d => d.level)));
        if (!this.showChildren && this.employeeAccount.children)
            this.employeeAccount.children = [];
    }
    private selectableAccountDims: AccountDimSmallDTO[];
    private defaultAccountDim: AccountDimSmallDTO;
    private childAccountDim: AccountDimSmallDTO;
    private subChildAccountDim: AccountDimSmallDTO;
    private showChildren: boolean = false;
    private showSubChildren: boolean = false;

    private accounts: AccountDTO[];
    private originalAccounts: AccountDTO[];
    private loadOtherEmployeeAccounts = false;

    private _selectedAccount: AccountDTO;
    private get selectedAccount(): AccountDTO {
        return this._selectedAccount;
    }
    private set selectedAccount(account: AccountDTO) {
        this._selectedAccount = account;
        this.employeeAccount.accountId = account ? account.accountId : 0;

        this.accounts = [];
        if (this.selectedAccount && this.selectedAccount.accountId) {
            this.coreService.getAccountsFromHierarchy(this.selectedAccount.accountId, true, true).then(x => {
                this.accounts = x;
            });
        }
    }

    //@ngInject
    constructor(
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private $q: ng.IQService,
        private $timeout: ng.ITimeoutService,
        private translationService: ITranslationService,
        private coreService: ICoreService,
        private accountDims: AccountDimSmallDTO[],
        private defaultEmployeeAccountDimId: number,
        private useLimitedEmployeeAccountDimLevels: boolean,
        private useExtendedEmployeeAccountDimLevels: boolean,
        private hasAllowToAddOtherEmployeeAccounts: boolean,
        employeeAccount: EmployeeAccountDTO) {

        this.translationService.translate("time.employee.employeeaccount.default").then(term => {
            this.defaultLabel = "/" + term;
        });

        this.setupAccountDims(accountDims);

        this.showSubChildren = this.useExtendedEmployeeAccountDimLevels;
        this.isNew = !employeeAccount;

        this.employeeAccount = new EmployeeAccountDTO();
        angular.merge(this.employeeAccount, employeeAccount);

        if (this.employeeAccount.addedOtherEmployeeAccount && !this.hasAllowToAddOtherEmployeeAccounts)
            this.readOnly = true;

        if (this.isNew) {
            this.selectDefaultAccountDim();
            this.employeeAccount.default = true;
            this.employeeAccount.dateFrom = CalendarUtility.getDateToday();
        } else {
            if (this.employeeAccount.addedOtherEmployeeAccount && this.hasAllowToAddOtherEmployeeAccounts && !this.loadOtherEmployeeAccounts) {
                this.loadOtherEmployeeAccounts = true;
                this.selectedAccountDim = _.find(this.accountDims, a => a.name === this.employeeAccount.accountDimName);
                this.selectDefaultAccountDim();

                this.loadSiblings().then(() => {
                    // For some reason this.employeeAccount is reset somewhere, so clone it again
                    this.employeeAccount = new EmployeeAccountDTO();
                    angular.merge(this.employeeAccount, employeeAccount);
                    this.selectExistingAccounts();
                });
            } else {
                this.selectExistingAccounts();
            }
        }
    }

    private setupAccountDims(accountDims: AccountDimSmallDTO[]) {
        this.defaultAccountDim = _.find(this.accountDims, a => a.accountDimId === this.defaultEmployeeAccountDimId);
        this.selectableAccountDims = this.defaultAccountDim ? this.accountDims.filter(d => d.level <= this.defaultAccountDim.level) : accountDims;
    }

    private selectExistingAccounts() {
        _.forEach(this.accountDims, dim => {
            let account: AccountDTO = _.find(dim.accounts, a => a.accountId === this.employeeAccount.accountId);
            if (account) {
                this.selectedAccountDim = dim;
                this.selectedAccount = account;
                return;
            }
        });

        this.selectDefaultAccountDim();

        if (this.employeeAccount.children) {
            this.employeeAccount.children.forEach(child => {
                this.childAccountChanged(child).then(() => {
                    if (child.children) {
                        child.children.forEach(subChild => {
                            this.subChildAccountChanged(child, subChild);
                        });
                    }
                });
            });
        }
    }

    private selectDefaultAccountDim() {
        if (!this.selectedAccountDim)
            this.selectedAccountDim = _.find(this.accountDims, a => a.accountDimId === this.defaultEmployeeAccountDimId);
    }

    private isSaveDisabled(): boolean {
        let invalid: boolean = false;

        if (!this.isEmployeeAccountValid(this.employeeAccount))
            invalid = true;

        if (!invalid) {
            _.forEach(this.employeeAccount.children, child => {
                if (!this.isEmployeeAccountValid(child)) {
                    invalid = true;
                    return false;
                }

                if (!invalid) {
                    _.forEach(child.children, subChild => {
                        if (!this.isEmployeeAccountValid(subChild)) {
                            invalid = true;
                            return false;
                        }
                    });
                }
            })
        }

        return invalid;
    }

    private isEmployeeAccountValid(employeeAccount: EmployeeAccountDTO): boolean {
        return (employeeAccount?.accountId &&
            employeeAccount.dateFrom &&
            (!employeeAccount.dateTo || CalendarUtility.convertToDate(employeeAccount.dateTo).isSameOrAfterOnDay(CalendarUtility.convertToDate(employeeAccount.dateFrom))));
    }

    // EVENTS

    private loadOtherEmployeeAccountsChanged() {
        this.$timeout(() => {
            if (this.loadOtherEmployeeAccounts) {
                this.employeeAccount.default = false;
                this.loadSiblings();
            } else {
                // CheckBox unselected, restore original accounts
                this.restoreOriginalAccounts(true);
                // If selected accout does not exist in original account list, unselect it
                this.reselectAccount();
            }
        });
    }

    private addChild() {
        if (!this.employeeAccount.children)
            this.employeeAccount.children = [];

        let newChild = new EmployeeAccountDTO();
        newChild.dateFrom = this.employeeAccount.dateFrom;
        newChild.dateTo = this.employeeAccount.dateTo;
        newChild.mainAllocation = this.employeeAccount.mainAllocation;
        newChild.default = this.employeeAccount.default;

        this.employeeAccount.children.push(newChild);
    }

    private deleteChild(child: EmployeeAccountDTO) {
        _.pull(this.employeeAccount.children, child);
    }

    private childAccountChanged(child: EmployeeAccountDTO): ng.IPromise<any> {
        return this.$timeout(() => {
            let account = this.accounts ? this.accounts.find(a => a.accountId === child.accountId) : undefined;
            child.accountName = account ? account.name : '';
            child.accountNumberName = account ? account.numberName : '';

            return this.coreService.getAccountsFromHierarchy(child.accountId, true, true).then(x => {
                child['accounts'] = x;
            });
        });
    }

    private addSubChild(child: EmployeeAccountDTO) {
        if (!child.children)
            child.children = [];

        let newSubChild = new EmployeeAccountDTO();
        newSubChild.accountDimId = this.subChildAccountDim?.accountDimId;
        newSubChild.accountDimName = this.subChildAccountDim?.name;
        newSubChild.dateFrom = child.dateFrom;
        newSubChild.dateTo = child.dateTo;
        newSubChild.mainAllocation = child.mainAllocation;
        newSubChild.default = child.default;

        child.children.push(newSubChild);
    }

    private deleteSubChild(child: EmployeeAccountDTO, subChild: EmployeeAccountDTO) {
        _.pull(child.children, subChild);
    }

    private subChildAccountChanged(child: EmployeeAccountDTO, subChild: EmployeeAccountDTO) {
        this.$timeout(() => {
            let account = child['accounts'].find(a => a.accountId === subChild.accountId);
            subChild.accountName = account ? account.name : '';
            subChild.accountNumberName = account ? account.numberName : '';
        });
    }

    public cancel() {
        this.restoreOriginalAccounts();
        this.$uibModalInstance.close();
    }

    public ok() {
        if (this.loadOtherEmployeeAccounts)
            this.employeeAccount.addedOtherEmployeeAccount = true;

        let clone: EmployeeAccountDTO = CoreUtility.cloneDTO(this.employeeAccount);

        if (this.loadOtherEmployeeAccounts)
            this.restoreOriginalAccounts();

        this.$uibModalInstance.close({ employeeAccount: clone });
    }

    // HELP-METHODS

    private loadSiblings(): ng.IPromise<any> {
        const deferral = this.$q.defer<any>();

        // Get selected account or first account in list if no account is selected
        let accountId = 0;
        if (this.selectedAccount && this.selectedAccount.accountId > 0) {
            accountId = this.selectedAccount.accountId;
        } else if (this.selectedAccountDim && this.selectedAccountDim.accounts.length > 0) {
            accountId = this.selectedAccountDim.accounts[0].accountId;
        }

        // Fetch sibling accounts (accounts with same parent)
        if (accountId) {
            this.originalAccounts = this.selectedAccountDim.accounts;
            return this.coreService.getSiblingAccounts(accountId).then(x => {
                this.selectedAccountDim.accounts = x;
                this.reselectAccount();
            });
        } else {
            deferral.resolve();
        }

        return deferral.promise;
    }

    private reselectAccount() {
        if (this.selectedAccount && _.includes(this.selectedAccountDim.accounts.map(a => a.accountId), this.selectedAccount.accountId))
            this.selectedAccount = this.selectedAccountDim.accounts.find(a => a.accountId === this.selectedAccount.accountId);
        else
            this.selectedAccount = new AccountDTO;
    }

    private restoreOriginalAccounts(force = false) {
        if (this.loadOtherEmployeeAccounts || force) {
            // Restore back to original account, if dialog is opened again
            this.selectedAccountDim.accounts = this.originalAccounts;
        }
    }
}
