import { CompanyAttestRoleDTO, UserCompanyAttestRoleDTO, UserAttestRoleDTO } from "../../../Common/Models/EmployeeUserDTO";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { SOEMessageBoxImage } from "../../../Util/Enumerations";
import { ICoreService } from "../../../Core/Services/CoreService";
import { AccountDTO } from "../../Models/AccountDTO";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { AccountDimSmallDTO } from "../../../Common/Models/AccountDimDTO";
import { IQService } from "angular";
import { ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { SoeEntityState, TermGroup, TermGroup_AttestRoleUserAccountPermissionType } from "../../../Util/CommonEnumerations";
import { SmallGenericType } from "../../Models/SmallGenericType";

export class UserAttestRoleAccountDialogController {

    // Filters
    private amountFilter: any;

    private userCompanyAttestRoleDTO: UserCompanyAttestRoleDTO;
    private isNew: boolean;

    private userPermittedAccounts: AccountDTO[] = [];
    private accountDims: AccountDimSmallDTO[] = [];
    private accountPermissionTypes: ISmallGenericType[] = [];
    private roles: ISmallGenericType[] = [];

    private _selectedAttestRole: CompanyAttestRoleDTO;
    public get selectedAttestRole(): CompanyAttestRoleDTO {
        return this._selectedAttestRole;
    }

    public set selectedAttestRole(companyAttestRoleDTO: CompanyAttestRoleDTO) {
        this._selectedAttestRole = companyAttestRoleDTO;
        if (companyAttestRoleDTO) {
            this.userCompanyAttestRoleDTO.attestRoleId = companyAttestRoleDTO.attestRoleId;
            this.userCompanyAttestRoleDTO.name = companyAttestRoleDTO.name;
            this.userCompanyAttestRoleDTO.moduleName = companyAttestRoleDTO.moduleName;
            this.userCompanyAttestRoleDTO.defaultMaxAmount = companyAttestRoleDTO.defaultMaxAmount;
            this.userCompanyAttestRoleDTO.state = companyAttestRoleDTO.state;
            if (!this.userCompanyAttestRoleDTO.attestRoleUserId) {
                this.userCompanyAttestRoleDTO.isExecutive = companyAttestRoleDTO.isExecutive;
                this.userCompanyAttestRoleDTO.isNearestManager = companyAttestRoleDTO.isNearestManager;
                this.userCompanyAttestRoleDTO.showAllCategories = companyAttestRoleDTO.showAllCategories;
                this.userCompanyAttestRoleDTO.showUncategorized = companyAttestRoleDTO.showUncategorized;
            }
        }
    }
    private _selectedAccountDim: AccountDimSmallDTO;
    private get selectedAccountDim(): AccountDimSmallDTO {
        return this._selectedAccountDim;
    }
    private set selectedAccountDim(accountDim: AccountDimSmallDTO) {
        this._selectedAccountDim = accountDim;

        this.childAccountDim = accountDim ? this.accountDims.find(d => d.level === accountDim.level + 1) : null;
        this.subChildAccountDim = accountDim ? this.accountDims.find(d => d.level === accountDim.level + 2) : null;

        if (accountDim) {
            this.userCompanyAttestRoleDTO.accountDimId = accountDim.accountDimId;
            this.userCompanyAttestRoleDTO.accountDimName = accountDim.name;
        }

        this.showChildren = !this.useLimitedEmployeeAccountDimLevels && (accountDim && accountDim.level < _.max(this.accountDims.map(d => d.level)));
        if (!this.showChildren && this.userCompanyAttestRoleDTO.children)
            this.userCompanyAttestRoleDTO.children = [];
    }

    private selectableAccountDims: AccountDimSmallDTO[];
    private defaultAccountDim: AccountDimSmallDTO;
    private childAccountDim: AccountDimSmallDTO;
    private subChildAccountDim: AccountDimSmallDTO;
    private showChildren: boolean = false;
    private showSubChildren: boolean = false;

    private accounts: AccountDTO[] = [];

    private _selectedAccount: AccountDTO;
    private get selectedAccount(): AccountDTO {
        return this._selectedAccount;
    }
    private set selectedAccount(account: AccountDTO) {
        this._selectedAccount = account;
        if (account) {
            this.userCompanyAttestRoleDTO.accountId = account.accountId;
            this.userCompanyAttestRoleDTO.accountName = account.name;
            this.accounts = [];
            this.coreService.getAccountsFromHierarchy(this.selectedAccount.accountId, true, true).then(x => {
                this.accounts = x;
            });
        }
    }

    //@ngInject
    constructor(
        private $timeout: ng.ITimeoutService,
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private $filter: ng.IFilterService,
        private $q: IQService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private coreService: ICoreService,
        private companyId: number,
        attestRole: UserCompanyAttestRoleDTO,
        private companyAttestRoles: CompanyAttestRoleDTO[],
        private defaultEmployeeAccountDimId: number,
        private useExtendedEmployeeAccountDimLevels: boolean,
        private useLimitedEmployeeAccountDimLevels: boolean,
        private useIsNearestManagerOnAttestRoleUser: boolean
    ) {

        this.amountFilter = this.$filter("amount");

        this.showSubChildren = this.useExtendedEmployeeAccountDimLevels;
        this.isNew = !attestRole || attestRole.attestRoleUserId == 0;
        this.companyAttestRoles = companyAttestRoles.filter(w => w.state == SoeEntityState.Active || (!this.isNew && attestRole.attestRoleId == w.attestRoleId));
        this.userCompanyAttestRoleDTO = new UserCompanyAttestRoleDTO();
        angular.extend(this.userCompanyAttestRoleDTO, attestRole);

        if (this.userCompanyAttestRoleDTO.children) {
            this.userCompanyAttestRoleDTO.children = this.userCompanyAttestRoleDTO.children.map(a => {
                let aObj = new UserAttestRoleDTO();
                angular.extend(aObj, a);

                if (aObj.children) {
                    aObj.children = aObj.children.map(b => {
                        let bObj = new UserAttestRoleDTO();
                        angular.extend(bObj, b);
                        return bObj;
                    });
                }

                return aObj;
            });
        }

        this.$q.all([
            this.loadAccountsByUserFromHierarchy()]).then(() => {
                this.$q.all([
                    this.loadAccountDims(),
                    this.loadAccountPermissionTypes(),
                    this.loadRoles()
                ]).then(() => {
                    this.setup();

                    if (!this.isNew) {

                        if (this.userCompanyAttestRoleDTO.children) {
                            this.userCompanyAttestRoleDTO.children.forEach(child => {
                                this.childAccountChanged(child);
                                if (child.children) {
                                    child.children.forEach(subChild => {
                                        this.subChildAccountChanged(child, subChild);
                                    });
                                }
                            });
                        }
                    }
                });
            });
    }

    private setup() {
        this.selectedAttestRole = _.find(this.companyAttestRoles, x => x.attestRoleId === this.userCompanyAttestRoleDTO.attestRoleId);
        if (this.userCompanyAttestRoleDTO.accountId === 0) {
            this.selectedAccountDim = _.find(this.accountDims, a => a.accountDimId === this.defaultEmployeeAccountDimId);
        } else {
            _.forEach(this.accountDims, dim => {
                let account: AccountDTO = _.find(dim.accounts, a => a.accountId === this.userCompanyAttestRoleDTO.accountId);
                if (account) {
                    this.selectedAccountDim = dim;
                    this.selectedAccount = account;
                    return;
                }
            });
        }
    }

    private setupAccountDims() {
        this.defaultAccountDim = _.find(this.accountDims, a => a.accountDimId === this.defaultEmployeeAccountDimId);
        this.selectableAccountDims = this.defaultAccountDim ? this.accountDims.filter(d => d.level <= this.defaultAccountDim.level) : this.accountDims;
    }

    // SERVICE CALLS

    private loadAccountsByUserFromHierarchy(): ng.IPromise<any> {
        return this.coreService.getAccountsFromHierarchyByUser(CalendarUtility.getDateToday(), CalendarUtility.getDateToday(), false, false, false, true, false, this.companyId).then(x => {
            this.userPermittedAccounts = x;
        });
    }

    private loadAccountDims(): ng.IPromise<any> {
        this.accountDims = [];
        return this.coreService.getAccountDimsSmall(false, true, true, false, true, false, false, false, this.companyId).then(x => {

            // Only add account dims that the user is permitted to see
            let permittedAccountDimIds: number[] = _.uniq(_.map(this.userPermittedAccounts, a => a.accountDimId));
            _.forEach(x, dim => {
                if (dim.accounts && dim.accounts.length > 0)
                    dim.accounts = _.sortBy(dim.accounts, t => t.name);
                if (_.includes(permittedAccountDimIds, dim.accountDimId))
                    this.accountDims.push(dim);
            });

            this.setupAccountDims();
        });
    }

    private loadAccountPermissionTypes(): ng.IPromise<any> {
        this.accountPermissionTypes = [];
        return this.coreService.getTermGroupContent(TermGroup.AttestRoleUserAccountPermissionType, false, false, true).then(x => {
            this.accountPermissionTypes = x.filter(y => y.id !== TermGroup_AttestRoleUserAccountPermissionType.ReadOnly);
            if (!this.userCompanyAttestRoleDTO.accountPermissionType)
                this.userCompanyAttestRoleDTO.accountPermissionType = TermGroup_AttestRoleUserAccountPermissionType.Complete;
        });
    }

    private loadRoles(): ng.IPromise<any> {
        return this.coreService.getRolesByUserDict(this.companyId).then(userRoles => {
            this.roles = userRoles;

            // Add empty
            this.roles.splice(0, 0, new SmallGenericType(0, ' '));
        })
    }

    private isSaveDisabled(): boolean {
        let invalid: boolean = false;

        if (!this.isAttestRoleValid(this.userCompanyAttestRoleDTO))
            invalid = true;

        if (!invalid) {
            _.forEach(this.userCompanyAttestRoleDTO.children, child => {
                if (!this.isAttestRoleValid(child)) {
                    invalid = true;
                    return false;
                }

                if (!invalid) {
                    _.forEach(child.children, subChild => {
                        if (!this.isAttestRoleValid(subChild)) {
                            invalid = true;
                            return false;
                        }
                    });
                }
            })
        }

        return invalid;
    }

    private isAttestRoleValid(attestRole: UserCompanyAttestRoleDTO | UserAttestRoleDTO): boolean {
        return (attestRole &&
            !!attestRole.accountId &&
            (!attestRole.dateFrom || !attestRole.dateTo || attestRole.dateTo.isSameOrAfterOnDay(attestRole.dateFrom)));
    }

    // EVENTS

    private addChild() {
        if (!this.userCompanyAttestRoleDTO.children)
            this.userCompanyAttestRoleDTO.children = [];

        let newChild = new UserAttestRoleDTO();
        newChild.attestRoleUserId = 0;
        newChild.accountId = 0;
        newChild.accountDimName = this.childAccountDim?.name;
        newChild.accountPermissionType = TermGroup_AttestRoleUserAccountPermissionType.Complete;
        newChild.dateFrom = this.userCompanyAttestRoleDTO.dateFrom;
        newChild.dateTo = this.userCompanyAttestRoleDTO.dateTo;
        newChild.isExecutive = this.userCompanyAttestRoleDTO.isExecutive;
        newChild.isNearestManager = this.userCompanyAttestRoleDTO.isNearestManager;

        this.userCompanyAttestRoleDTO.children.push(newChild);

        this.setDirty();
    }

    private childAccountChanged(child: UserAttestRoleDTO) {
        this.$timeout(() => {
            let account = this.accounts.find(a => a.accountId === child.accountId);
            child.accountName = account ? account.name : '';

            this.coreService.getAccountsFromHierarchy(child.accountId, true, true).then(x => {
                child['accounts'] = x;
            });

            this.setDirty();
        });
    }

    private deleteChild(child) {
        _.pull(this.userCompanyAttestRoleDTO.children, child);

        this.setDirty();
    }

    private addSubChild(child: UserAttestRoleDTO) {
        if (!child.children)
            child.children = [];

        let newSubChild = new UserAttestRoleDTO();
        newSubChild.accountDimName = this.subChildAccountDim?.name;
        newSubChild.accountPermissionType = child.accountPermissionType;
        newSubChild.dateFrom = child.dateFrom;
        newSubChild.dateTo = child.dateTo;
        newSubChild.isExecutive = child.isExecutive;
        newSubChild.isNearestManager = child.isNearestManager;

        child.children.push(newSubChild);

        this.setDirty();
    }

    private subChildAccountChanged(child: UserAttestRoleDTO, subChild: UserAttestRoleDTO) {
        this.$timeout(() => {
            let account = child['accounts'].find(a => a.accountId === subChild.accountId);
            subChild.accountName = account ? account.name : '';

            this.setDirty();
        });
    }

    private deleteSubChild(child: UserAttestRoleDTO, subChild: UserAttestRoleDTO) {
        _.pull(child.children, subChild);

        this.setDirty();
    }

    public cancel() {
        this.$uibModalInstance.close();
    }

    public ok() {
        let dateFrom = CalendarUtility.convertToDate(this.userCompanyAttestRoleDTO.dateFrom);
        let dateTo = CalendarUtility.convertToDate(this.userCompanyAttestRoleDTO.dateTo);
        if (this.userCompanyAttestRoleDTO.maxAmount > this.userCompanyAttestRoleDTO.defaultMaxAmount) {
            var keys: string[] = [
                "common.user.invalidmaxamount.title",
                "common.user.invalidmaxamount.message"
            ];
            this.translationService.translateMany(keys).then(terms => {
                this.notificationService.showDialogEx(terms["common.user.invalidmaxamount.title"], terms["common.user.invalidmaxamount.message"].format(this.amountFilter(this.userCompanyAttestRoleDTO.defaultMaxAmount)), SOEMessageBoxImage.Forbidden);
            });
        } else if (dateFrom && dateTo && dateFrom > dateTo) {
            this.translationService.translate("error.invaliddaterange").then(term => {
                this.notificationService.showDialogEx("", term, SOEMessageBoxImage.Forbidden);
            });
        } else {
            this.userCompanyAttestRoleDTO.name = this.selectedAttestRole ? this.selectedAttestRole.name : '';
            this.userCompanyAttestRoleDTO.moduleName = this.selectedAttestRole ? this.selectedAttestRole.moduleName : '';
            this.userCompanyAttestRoleDTO.accountPermissionTypeName = _.find(this.accountPermissionTypes, t => t.id === this.userCompanyAttestRoleDTO.accountPermissionType).name;
            _.forEach(this.userCompanyAttestRoleDTO.children, child => {
                let account: AccountDTO = _.find(this.accounts, x => x.accountId === child.accountId);
                child.accountName = account.name;
                child.accountPermissionTypeName = _.find(this.accountPermissionTypes, t => t.id === child.accountPermissionType).name;

                _.forEach(child.children, subChild => {
                    subChild.accountPermissionTypeName = _.find(this.accountPermissionTypes, t => t.id === subChild.accountPermissionType).name;
                });
            });
            this.$uibModalInstance.close({ attestRole: this.userCompanyAttestRoleDTO });
        }
    }

    // HELP-METHODS

    private setDirty() {
        this.userCompanyAttestRoleDTO.isModified = true;
    }
}
