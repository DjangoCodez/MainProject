import { UserAttestRoleDTO, UserCompanyRoleDTO } from "../../../../../Common/Models/EmployeeUserDTO";
import { UserCompanyRoleDelegateHistoryUserDTO } from "../../../../../Common/Models/UserDTO";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { CalendarUtility } from "../../../../../Util/CalendarUtility";
import { IUserService } from "../../../UserService";
import { AttestRoleDialogController } from "./AttestRoleDialogController";
import { RoleDialogController } from "./RoleDialogController";

export class DelegateHistoryDialogController {

    // Terms
    private terms: { [index: string]: string; };

    // Data
    private targetUser: UserCompanyRoleDelegateHistoryUserDTO;

    // Properties
    private headDateFrom: Date;
    private headDateTo: Date;
    private searchUserCondition: string;

    //@ngInject
    constructor(
        private $uibModal,
        private $scope: ng.IScope,
        private $q: ng.IQService,
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        private userService: IUserService,
        private userId: number) {

        this.$q.all([
            this.loadTerms()
        ]).then(() => {
            this.headDateFrom = this.headDateTo = CalendarUtility.getDateToday();
        });
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    // EVENTS

    private searchUser() {
        this.userService.getTargetUserForDelegation(this.userId, this.searchUserCondition).then(x => {
            this.targetUser = x;
            if (!this.targetUser)
                this.clearSearchUser();
        });
    }

    private clearSearchUser() {
        this.searchUserCondition = '';
    }

    private addRole() {
        this.openRoleDialog(null);
    }

    private editRole(role: UserCompanyRoleDTO) {
        this.openRoleDialog(role);
    }

    private deleteRole(role: UserCompanyRoleDTO) {
        _.pull(this.targetUser.targetRoles, role);
    }

    private addAttestRole() {
        this.openAttestRoleDialog(null);
    }

    private editAttestRole(attestRole: UserAttestRoleDTO) {
        this.openAttestRoleDialog(attestRole);
    }

    private deleteAttestRole(attestRole: UserAttestRoleDTO) {
        _.pull(this.targetUser.targetAttestRoles, attestRole);
    }

    private cancel() {
        this.$uibModalInstance.close();
    }

    private save() {
        this.$uibModalInstance.close({ targetUser: this.targetUser });
    }

    // ACTIONS

    private openRoleDialog(role: UserCompanyRoleDTO) {
        let isNew = !role;

        if (!role) {
            role = new UserCompanyRoleDTO();
            role.dateFrom = this.headDateFrom;
            role.dateTo = this.headDateTo;
        }

        var modal = this.$uibModal.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Manage/User/Users/Directives/DelegateHistory/RoleDialog.html"),
            controller: RoleDialogController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'md',
            scope: this.$scope,
            resolve: {
                roles: () => { return this.targetUser.possibleTargetRoles; },
                role: () => { return role; }
            }
        });

        modal.result.then(result => {
            if (result && result.role) {
                if (isNew) {
                    this.targetUser.targetRoles.push(result.role);
                } else {
                    role.roleId = result.role.roleId;
                    role.name = result.role.name;
                    role.dateFrom = result.role.dateFrom;
                    role.dateTo = result.role.dateTo;
                }
            }
        });
    }

    private openAttestRoleDialog(attestRole: UserAttestRoleDTO) {
        let isNew = !attestRole;

        if (!attestRole) {
            attestRole = new UserAttestRoleDTO();
            attestRole.dateFrom = this.headDateFrom;
            attestRole.dateTo = this.headDateTo;
        }

        var modal = this.$uibModal.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Manage/User/Users/Directives/DelegateHistory/AttestRoleDialog.html"),
            controller: AttestRoleDialogController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'lg',
            scope: this.$scope,
            resolve: {
                attestRoles: () => { return this.targetUser.possibleTargetAttestRoles; },
                attestRole: () => { return attestRole; }
            }
        });

        modal.result.then(result => {
            if (result && result.attestRole) {
                if (isNew) {
                    this.targetUser.targetAttestRoles.push(result.attestRole);
                } else {
                    attestRole.attestRoleUserId = result.attestRole.attestRoleUserId;
                    attestRole.attestRoleId = result.attestRole.attestRoleId;
                    attestRole.name = result.attestRole.name;
                    attestRole.dateFrom = result.attestRole.dateFrom;
                    attestRole.dateTo = result.attestRole.dateTo;
                }
            }
        });
    }

    // HELP-METHODS

    private get enableSave(): boolean {
        if (!this.targetUser)
            return false;

        return (this.targetUser.targetRoles.length > 0 || this.targetUser.targetAttestRoles.length > 0);
    }
}
