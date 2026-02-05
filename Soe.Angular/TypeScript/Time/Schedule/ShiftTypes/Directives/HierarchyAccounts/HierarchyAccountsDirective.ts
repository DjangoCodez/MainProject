import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { ShiftTypeHierarchyAccountDTO } from "../../../../../Common/Models/ShiftTypeDTO";
import { AccountDTO } from "../../../../../Common/Models/AccountDTO";
import { AccountDimSmallDTO } from "../../../../../Common/Models/AccountDimDTO";
import { HierarchyAccountDialogController } from "./HierarchyAccountDialogController";
import { IQService } from "angular";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { CalendarUtility } from "../../../../../Util/CalendarUtility";
import { IAccountingService } from "../../../../../Shared/Economy/Accounting/AccountingService";
import { ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";
import { TermGroup, TermGroup_AttestRoleUserAccountPermissionType } from "../../../../../Util/CommonEnumerations";

export class HierarchyAccountsDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {

        return {
            templateUrl: urlHelperService.getUrl('Directives/HierarchyAccounts/HierarchyAccounts.html'),
            scope: {
                hierarchyAccounts: '=',
                defaultEmployeeAccountDimId: '=',
                readOnly: '=',
                onChange: '&'
            },
            restrict: 'E',
            replace: true,
            controller: HierarchyAccountsController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

class HierarchyAccountsController {

    // Init parameters
    private hierarchyAccounts: ShiftTypeHierarchyAccountDTO[];
    private defaultEmployeeAccountDimId: number;
    private readOnly: boolean;
    private onChange: Function;

    // Data
    private accounts: AccountDTO[] = [];
    private allAccountDims: AccountDimSmallDTO[] = [];
    private accountDims: AccountDimSmallDTO[] = [];
    private accountPermissionTypes: ISmallGenericType[] = [];
    private selectedHierarchyAccount: ShiftTypeHierarchyAccountDTO;

    //@ngInject
    constructor(
        private $uibModal,
        private $scope: ng.IScope,
        private $q: IQService,
        private urlHelperService: IUrlHelperService,
        private coreService: ICoreService,
        private sharedAccountingService: IAccountingService) {

        this.$q.all([
            this.loadAccountsByUserFromHierarchy()]).then(() => {
                this.$q.all([
                    this.loadAccountDims(),
                    this.loadAccountPermissionTypes()
                ]).then(() => {
                    this.setupWatchers();
                });
            });
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.hierarchyAccounts, (newVal, oldVal) => {
            _.forEach(this.hierarchyAccounts, a => {
                this.setAccountNames(a);
                this.setAccountPermissionTypeName(a);
            });
        });
    }

    // SERVICE CALLS

    private loadAccountsByUserFromHierarchy(): ng.IPromise<any> {
        return this.coreService.getAccountsFromHierarchyByUser(CalendarUtility.getDateToday(), CalendarUtility.getDateToday(), false, true).then(x => {
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

            _.forEach(this.hierarchyAccounts, a => {
                this.setAccountNames(a);
            });
        });
    }

    private loadAccountPermissionTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.AttestRoleUserAccountPermissionType, false, false, true).then(x => {
            this.accountPermissionTypes = x.filter(y => y.id !== TermGroup_AttestRoleUserAccountPermissionType.ReadOnly);
        });
    }

    // EVENTS

    private editHierarchyAccount(hierarchyAccount: ShiftTypeHierarchyAccountDTO) {
        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Schedule/ShiftTypes/Directives/HierarchyAccounts/HierarchyAccountDialog.html"),
            controller: HierarchyAccountDialogController,
            controllerAs: "ctrl",
            size: 'lg',
            resolve: {
                accountDims: () => { return this.accountDims },
                defaultEmployeeAccountDimId: () => { return this.defaultEmployeeAccountDimId },
                accountPermissionTypes: () => { return this.accountPermissionTypes },
                hierarchyAccount: () => { return hierarchyAccount }
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result && result.hierarchyAccount) {
                if (hierarchyAccount) {
                    // Update original account
                    this.setHierarchyAccountDataFromResult(hierarchyAccount, result.hierarchyAccount);
                } else {
                    // Add new account to the original collection
                    hierarchyAccount = new ShiftTypeHierarchyAccountDTO();
                    this.setHierarchyAccountDataFromResult(hierarchyAccount, result.hierarchyAccount);
                    if (!this.hierarchyAccounts)
                        this.hierarchyAccounts = [];
                    this.hierarchyAccounts.push(hierarchyAccount);
                }

                this.selectedHierarchyAccount = hierarchyAccount;

                if (this.onChange)
                    this.onChange();
            }
        });
    }

    private deleteHierarchyAccount(employeeAccount: ShiftTypeHierarchyAccountDTO) {
        _.pull(this.hierarchyAccounts, employeeAccount);

        if (this.onChange)
            this.onChange();
    }

    private setHierarchyAccountDataFromResult(hierarchyAccount: ShiftTypeHierarchyAccountDTO, resultHierarchyAccount: ShiftTypeHierarchyAccountDTO) {
        hierarchyAccount.shiftTypeHierarchyAccountId = resultHierarchyAccount.shiftTypeHierarchyAccountId;
        hierarchyAccount.accountId = resultHierarchyAccount.accountId;
        hierarchyAccount.accountPermissionType = resultHierarchyAccount.accountPermissionType;
        this.setAccountNames(hierarchyAccount);
        this.setAccountPermissionTypeName(hierarchyAccount);
    }

    private setAccountNames(hierarchyAccount: ShiftTypeHierarchyAccountDTO) {
        _.forEach(this.allAccountDims, dim => {
            this.setAccountName(hierarchyAccount, dim).then(result => {
                if (result)
                    return false;
            });
        });
    }

    private setAccountName(hierarchyAccount: ShiftTypeHierarchyAccountDTO, dim: AccountDimSmallDTO): ng.IPromise<boolean> {
        var deferral = this.$q.defer<boolean>();

        let account: AccountDTO = _.find(dim.accounts, a => a.accountId === hierarchyAccount.accountId);
        if (account) {
            hierarchyAccount.accountDimName = dim ? dim.name : '';
            hierarchyAccount.accountName = account.name;
            deferral.resolve(true);
        } else {
            this.sharedAccountingService.getAccountSmall(hierarchyAccount.accountId).then(x => {
                if (x) {
                    dim = _.find(this.allAccountDims, d => d.accountDimId === x.accountDimId);
                    hierarchyAccount.accountDimName = dim ? dim.name : '';
                    hierarchyAccount.accountName = x.name;
                    deferral.resolve(true);
                } else {
                    deferral.resolve(false);
                }
            });
        }

        return deferral.promise;
    }

    private setAccountPermissionTypeName(hierarchyAccount: ShiftTypeHierarchyAccountDTO) {
        hierarchyAccount.accountPermissionTypeName = this.accountPermissionTypes.find(t => t.id === hierarchyAccount.accountPermissionType).name;
    }
}
