import { ShiftTypeHierarchyAccountDTO } from "../../../../../Common/Models/ShiftTypeDTO";
import { AccountDimSmallDTO } from "../../../../../Common/Models/AccountDimDTO";
import { AccountDTO } from "../../../../../Common/Models/AccountDTO";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { TermGroup, TermGroup_AttestRoleUserAccountPermissionType } from "../../../../../Util/CommonEnumerations";
import { ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";

export class HierarchyAccountDialogController {

    private hierarchyAccount: ShiftTypeHierarchyAccountDTO;
    private isNew: boolean;

    private selectedAccountDim: AccountDimSmallDTO;

    private accounts: AccountDTO[];

    private _selectedAccount: AccountDTO;
    private get selectedAccount(): AccountDTO {
        return this._selectedAccount;
    }
    private set selectedAccount(account: AccountDTO) {
        this._selectedAccount = account;
        this.hierarchyAccount.accountId = account ? account.accountId : 0;
        this.loadAccountsFromHierarchy();
    }

    //@ngInject
    constructor(
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private $timeout: ng.ITimeoutService,
        private coreService: ICoreService,
        private accountDims: AccountDimSmallDTO[],
        private defaultEmployeeAccountDimId: number,
        private accountPermissionTypes: ISmallGenericType[],
        hierarchyAccount: ShiftTypeHierarchyAccountDTO) {

        this.isNew = !hierarchyAccount;

        this.hierarchyAccount = new ShiftTypeHierarchyAccountDTO();
        angular.merge(this.hierarchyAccount, hierarchyAccount);
        if (!this.hierarchyAccount.accountPermissionType)
            this.hierarchyAccount.accountPermissionType = TermGroup_AttestRoleUserAccountPermissionType.Complete;

        if (this.isNew) {
            this.selectedAccountDim = _.find(this.accountDims, a => a.accountDimId === defaultEmployeeAccountDimId);
        } else {
            _.forEach(this.accountDims, dim => {
                let account: AccountDTO = _.find(dim.accounts, a => a.accountId === this.hierarchyAccount.accountId);
                if (account) {
                    this.selectedAccountDim = dim;
                    this.selectedAccount = account;
                    return;
                }
            });
        }
    }

    // SERVICE CALLS

    private loadAccountsFromHierarchy() {
        this.accounts = [];
        if (!this.selectedAccount)
            return;

        this.coreService.getAccountsFromHierarchy(this.selectedAccount.accountId, true, true).then(x => {
            this.accounts = x;
        });
    }

    // EVENTS

    public cancel() {
        this.$uibModalInstance.close();
    }

    public ok() {
        this.$uibModalInstance.close({ hierarchyAccount: this.hierarchyAccount });
    }
}
