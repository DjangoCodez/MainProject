import { CompanyRolesDTO, UserRolesDTO, UserAttestRoleDTO, UserCompanyAttestRoleDTO, UserCompanyRoleDTO } from "../../../Common/Models/EmployeeUserDTO";
import { AttestRoleDialogController } from "./AttestRoleDialogController";
import { UserAttestRoleAccountDialogController } from "./UserAttestRoleAccountDialogController";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { SmallGenericType } from "../../../Common/Models/SmallGenericType";
import { CoreUtility } from "../../../Util/CoreUtility";
import { SoeEntityState, TermGroup_AttestRoleUserAccountPermissionType } from "../../../Util/CommonEnumerations";
import { UserRoleDialogController } from "./UserRoleDialogController";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { SOEMessageBoxButtons, SOEMessageBoxImage } from "../../../Util/Enumerations";

export class UserRolesDialogController {

    // Init parameters
    private userRoles: UserRolesDTO[];
    private companyRoles: CompanyRolesDTO[];

    // Flags
    private showAllCompanies: boolean = false;
    private allRowsExpanded: boolean = false;
    private roleHasChanges: boolean = false;
    private attestRoleHasChanges: boolean = false;

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private $uibModal,
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private userRolesReadPermission: boolean,
        private userRolesModifyPermission: boolean,
        private attestRolesReadPermission: boolean,
        private attestRolesModifyPermission: boolean,
        private useAccountsHierarchy: boolean,
        private defaultEmployeeAccountDimId: boolean,
        private useLimitedEmployeeAccountDimLevels: boolean,        
        private useExtendedEmployeeAccountDimLevels: boolean,
        private useIsNearestManagerOnAttestRoleUser: boolean,
        private userId: number,
        userRoles: UserRolesDTO[],
        companyRoles: CompanyRolesDTO[]
    ) {

        // Copy roles to be able to cancel edit
        this.userRoles = [];
        _.forEach(userRoles, userRole => {
            let role = new UserRolesDTO();
            angular.extend(role, userRole);
            this.userRoles.push(role);
        });

        this.companyRoles = [];
        _.forEach(companyRoles, companyRole => {
            let role = new CompanyRolesDTO();
            angular.extend(role, companyRole);
            this.companyRoles.push(role);

            if (!this.userRolesModifyPermission || !this.modifyCurrentUserPermission) {
                role.readOnly = true;
                _.forEach(role.roles, r => {
                    r.readOnly = true;
                })
            }
            if (!this.attestRolesModifyPermission || !this.modifyCurrentUserPermission) {
                _.forEach(role.attestRoles, r => {
                    r.readOnly = true;
                })
            }
        });

        if (this.userIsMySelf) {
            // If editing my self, I can not remove current role
            let companyRole = _.find(this.companyRoles, r => r.actorCompanyId === CoreUtility.actorCompanyId);
            if (companyRole) {
                let role = _.find(companyRole.roles, r => r.roleId === CoreUtility.roleId);
                if (role)
                    role.readOnly = true;
            }
        }

        this.setVisible();

        // If only one company, expand it
        if (this.visibleCompanyRoles.length === 1) {
            this.visibleCompanyRoles[0].expanded = true;
            this.allRowsExpanded = true;
        }
    }

    // EVENTS

    private expandAllRows() {
        _.forEach(this.companyRoles, roles => {
            roles.expanded = true;
        });
        this.allRowsExpanded = true;
    }

    private collapseAllRows() {
        _.forEach(this.companyRoles, roles => {
            roles.expanded = false;
        });
        this.allRowsExpanded = false;
    }

    private companyRoleDefaultSelected(companyRole: CompanyRolesDTO) {
        if (companyRole.readOnly)
            return;

        companyRole.defaultCompany = !companyRole.defaultCompany;
        companyRole.isModified = true;
        this.roleHasChanges = true;

        if (companyRole.defaultCompany) {
            // Unselect all other default
            _.forEach(_.filter(this.companyRoles, r => r.defaultCompany && r.actorCompanyId !== companyRole.actorCompanyId), r => {
                r.defaultCompany = false;
                r.isModified = true;
            });
        }
    }

    private roleSelected(role: UserCompanyRoleDTO) {
        if (role.readOnly || role.isDelegated)
            return;

        role.selected = !role.selected;

        role.isModified = ((role.initiallySelected && !role.selected) || (!role.initiallySelected && role.selected));
        if (role.isModified)
            this.roleHasChanges = true;

        if (role.selected) {
            if (!this.hasDefaultRole)
                this.roleDefaultSelected(role);
        } else {
            if (role.default)
                role.default = false;
        }
    }

    private roleDefaultSelected(role: UserCompanyRoleDTO) {
        if (role.readOnly || role.isDelegated)
            return;

        role.default = !role.default;
        role.isModified = true;
        this.roleHasChanges = true;
    }

    private createUserRole(companyRole: CompanyRolesDTO, copyFrom: UserCompanyRoleDTO) {
        let minUserCompanyRoleId: number = _.min(companyRole.roles.map(a => a.tmpUserCompanyRoleId));

        var userCompanyRole: UserCompanyRoleDTO = new UserCompanyRoleDTO();
        userCompanyRole.userCompanyRoleId = 0;
        userCompanyRole.tmpUserCompanyRoleId = (minUserCompanyRoleId || 0) - 1;
        userCompanyRole.roleId = copyFrom ? copyFrom.roleId : 0;
        userCompanyRole.isModified = true;
        this.roleSelected(userCompanyRole);
        companyRole.userCompanyRoles.push(userCompanyRole);

        this.editUserRole(companyRole, userCompanyRole, true);
    }

    private editUserRole(companyRole: CompanyRolesDTO, userCompanyRoleDTO: UserCompanyRoleDTO, isNew: boolean = false) {
        if (userCompanyRoleDTO.isDelegated) {
            this.translationService.translate('common.user.role.isdelegated').then(term => {
                this.notificationService.showDialogEx('', term, SOEMessageBoxImage.Forbidden);
            });
            return;
        }

        let roles: SmallGenericType[] = [];
        _.forEach(_.filter(companyRole.roles, r => r.roleId), role => {
            if (!_.find(roles, r => r.id === role.roleId)) {
                roles.push(new SmallGenericType(role.roleId, role.name));
            }
        });

        var modal = this.$uibModal.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Common/Directives/UserRoles/Views/UserRoleDialog.html"),
            controller: UserRoleDialogController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'md',
            scope: this.$scope,
            resolve: {
                companyRoles: () => { return roles; },
                userCompanyRole: () => { return userCompanyRoleDTO; },
                roleReadOnly: () => { return !!userCompanyRoleDTO.userCompanyRoleId; }
            }
        });

        modal.result.then(result => {
            if (result && result.userCompanyRole) {
                userCompanyRoleDTO.isModified = result.userCompanyRole.isModified;
                userCompanyRoleDTO.roleId = result.userCompanyRole.roleId;
                userCompanyRoleDTO.name = _.find(roles, r => r.id === result.userCompanyRole.roleId).name;
                userCompanyRoleDTO.dateFrom = result.userCompanyRole.dateFrom
                userCompanyRoleDTO.dateTo = result.userCompanyRole.dateTo;

                if (userCompanyRoleDTO.isModified) {
                    this.roleHasChanges = true;
                    if (userCompanyRoleDTO.roleId || userCompanyRoleDTO.dateFrom || userCompanyRoleDTO.dateTo) {
                        userCompanyRoleDTO.selected = true;
                    }
                }
            } else {
                if (isNew)
                    _.pull(companyRole.userCompanyRoles, userCompanyRoleDTO);
            }
        }, (reason) => {
            if (isNew)
                _.pull(companyRole.userCompanyRoles, userCompanyRoleDTO);
        });
    }

    private attestRoleSelected(attestRole: UserCompanyAttestRoleDTO) {
        if (attestRole.readOnly || attestRole.isDelegated)
            return;

        attestRole.selected = !attestRole.selected;
        attestRole.isModified = ((attestRole.initiallySelected && !attestRole.selected) || (!attestRole.initiallySelected && attestRole.selected));
        if (attestRole.isModified)
            this.attestRoleHasChanges = true;
    }

    private createUserAttestRole(companyRole: CompanyRolesDTO) {
        let minAttestRoleUserId: number = _.min(companyRole.userCompanyAttestRoles.map(a => a.tmpAttestRoleUserId));

        var userCompanyAttestRole: UserCompanyAttestRoleDTO = new UserCompanyAttestRoleDTO();
        userCompanyAttestRole.attestRoleUserId = 0;
        userCompanyAttestRole.tmpAttestRoleUserId = (minAttestRoleUserId || 0) - 1;
        userCompanyAttestRole.attestRoleId = 0;
        userCompanyAttestRole.accountId = 0;
        userCompanyAttestRole.accountPermissionType = TermGroup_AttestRoleUserAccountPermissionType.Complete;
        userCompanyAttestRole.isModified = true;
        userCompanyAttestRole.children = [];
        userCompanyAttestRole.state = SoeEntityState.Active;
        this.attestRoleSelected(userCompanyAttestRole);
        companyRole.userCompanyAttestRoles.push(userCompanyAttestRole);
        this.editUserAttestRole(companyRole, userCompanyAttestRole, true);
    }

    private editUserAttestRole(companyRole: CompanyRolesDTO, userCompanyAttestRoleDTO: UserCompanyAttestRoleDTO, isNew: boolean = false) {
        if (userCompanyAttestRoleDTO.isDelegated) {
            this.translationService.translate('common.user.role.isdelegated').then(term => {
                this.notificationService.showDialogEx('', term, SOEMessageBoxImage.Forbidden);
            });
            return;
        }

        var modal;

        if (this.useAccountsHierarchy) {
            modal = this.$uibModal.open({
                templateUrl: this.urlHelperService.getGlobalUrl("Common/Directives/UserRoles/Views/UserAttestRoleAccountDialog.html"),
                controller: UserAttestRoleAccountDialogController,
                controllerAs: 'ctrl',
                backdrop: 'static',
                size: 'xl',
                windowClass: 'fullsize-modal',
                scope: this.$scope,
                resolve: {
                    companyId: () => { return companyRole.actorCompanyId; },
                    attestRole: () => { return userCompanyAttestRoleDTO; },
                    companyAttestRoles: () => { return companyRole.attestRoles; },
                    defaultEmployeeAccountDimId: () => { return this.defaultEmployeeAccountDimId; },
                    useExtendedEmployeeAccountDimLevels: () => { return this.useExtendedEmployeeAccountDimLevels },
                    useLimitedEmployeeAccountDimLevels: () => { return this.useLimitedEmployeeAccountDimLevels },
                    useIsNearestManagerOnAttestRoleUser: () => {
                        return this.useIsNearestManagerOnAttestRoleUser;
                    }
                }
            });
        } else {
            modal = this.$uibModal.open({
                templateUrl: this.urlHelperService.getGlobalUrl("Common/Directives/UserRoles/Views/AttestRoleDialog.html"),
                controller: AttestRoleDialogController,
                controllerAs: 'ctrl',
                backdrop: 'static',
                size: 'md',
                scope: this.$scope,
                resolve: {
                    attestRole: () => { return userCompanyAttestRoleDTO; }
                }
            });
        }

        modal.result.then(result => {
            if (result && result.attestRole) {
                userCompanyAttestRoleDTO.isModified = result.attestRole.isModified;

                userCompanyAttestRoleDTO.attestRoleId = result.attestRole.attestRoleId;
                userCompanyAttestRoleDTO.name = result.attestRole.name;
                userCompanyAttestRoleDTO.moduleName = result.attestRole.moduleName;
                userCompanyAttestRoleDTO.maxAmount = result.attestRole.maxAmount
                userCompanyAttestRoleDTO.accountDimId = result.attestRole.accountDimId;
                userCompanyAttestRoleDTO.accountDimName = result.attestRole.accountDimName;
                userCompanyAttestRoleDTO.accountId = result.attestRole.accountId;
                userCompanyAttestRoleDTO.accountName = result.attestRole.accountName;
                userCompanyAttestRoleDTO.isExecutive = result.attestRole.isExecutive;
                userCompanyAttestRoleDTO.isNearestManager = result.attestRole.isNearestManager;
                userCompanyAttestRoleDTO.accountPermissionTypeName = result.attestRole.accountPermissionTypeName;
                userCompanyAttestRoleDTO.accountPermissionType = result.attestRole.accountPermissionType;
                userCompanyAttestRoleDTO.dateFrom = result.attestRole.dateFrom
                userCompanyAttestRoleDTO.dateTo = result.attestRole.dateTo;
                userCompanyAttestRoleDTO.children = result.attestRole.children;
                userCompanyAttestRoleDTO.state = result.attestRole.state;
                userCompanyAttestRoleDTO.roleId = result.attestRole.roleId;

                _.forEach(result.attestRole.children, (child: UserAttestRoleDTO) => {
                    if (child.attestRoleUserId != 0) {
                        let userAttestRoleChild: UserAttestRoleDTO = _.find(userCompanyAttestRoleDTO.children, r => r.attestRoleUserId === child.attestRoleUserId);
                        if (userAttestRoleChild) {
                            userAttestRoleChild.accountId = child.accountId;
                            userAttestRoleChild.accountName = child.accountName;
                            userAttestRoleChild.dateFrom = child.dateFrom;
                            userAttestRoleChild.dateTo = child.dateTo;
                            userAttestRoleChild.isExecutive = child.isExecutive;
                            userAttestRoleChild.isNearestManager = child.isNearestManager;
                            userAttestRoleChild.accountPermissionType = child.accountPermissionType;
                            userAttestRoleChild.accountPermissionTypeName = child.accountPermissionTypeName;
                        }
                    }

                    if (child.children && child.children.length > 0) {
                        _.forEach(child.children, subChild => {
                            if (subChild.attestRoleUserId != 0) {
                                let userAttestRoleChild: UserAttestRoleDTO = _.find(userCompanyAttestRoleDTO.children, r => r.attestRoleUserId === subChild.attestRoleUserId);
                                if (userAttestRoleChild) {
                                    userAttestRoleChild.accountId = subChild.accountId;
                                    userAttestRoleChild.accountName = subChild.accountName;
                                    userAttestRoleChild.dateFrom = subChild.dateFrom;
                                    userAttestRoleChild.dateTo = subChild.dateTo;
                                    userAttestRoleChild.isExecutive = subChild.isExecutive;
                                    userAttestRoleChild.isNearestManager = subChild.isNearestManager;
                                    userAttestRoleChild.accountPermissionType = subChild.accountPermissionType;
                                    userAttestRoleChild.accountPermissionTypeName = subChild.accountPermissionTypeName;
                                }
                            }
                        });
                    }
                });

                if (userCompanyAttestRoleDTO.isModified) {
                    this.attestRoleHasChanges = true;
                    if (userCompanyAttestRoleDTO.accountId || userCompanyAttestRoleDTO.accountDimId || userCompanyAttestRoleDTO.isExecutive || userCompanyAttestRoleDTO.isNearestManager || userCompanyAttestRoleDTO.accountPermissionType > 0 || userCompanyAttestRoleDTO.dateFrom || userCompanyAttestRoleDTO.dateTo || userCompanyAttestRoleDTO.maxAmount) {
                        userCompanyAttestRoleDTO.selected = true;
                    }
                }
            } else {
                if (isNew)
                    _.pull(companyRole.userCompanyAttestRoles, userCompanyAttestRoleDTO);
            }
        }, (reason) => {
            if (isNew)
                _.pull(companyRole.userCompanyAttestRoles, userCompanyAttestRoleDTO);
        });
    }

    private cancel() {
        this.$uibModalInstance.close();
    }

    private ok() {
        if (this.validateDefaultCompanyAndRole()) {
            this.validateRoleDates().then(passed => {
                if (passed) {
                    if (this.roleHasChanges || this.attestRoleHasChanges)
                        this.copyModifiedRoles();

                    this.$uibModalInstance.close({ roleHasChanges: this.roleHasChanges, attestRoleHasChanges: this.attestRoleHasChanges, userRoles: this.userRoles, companyRoles: this.companyRoles });
                }
            });
        }
    }

    // VALIDATION

    private validateDefaultCompanyAndRole(): boolean {
        let defaultCompany = _.find(this.companyRoles, r => r.defaultCompany);
        if (!defaultCompany) {
            let keys: string[] = [
                "common.user.roles.defaultcompanymandatory.title",
                "common.user.roles.defaultcompanymandatory.message"
            ];

            this.translationService.translateMany(keys).then(terms => {
                this.notificationService.showDialog(terms["common.user.roles.defaultcompanymandatory.title"], terms["common.user.roles.defaultcompanymandatory.message"], SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK);
            });
            return false;
        }

        if (_.filter(defaultCompany.userCompanyRoles, r => r.default).length === 0) {
            let keys: string[] = [
                "common.user.roles.defaultrolemandatory.title",
                "common.user.roles.defaultrolemandatory.message"
            ];

            this.translationService.translateMany(keys).then(terms => {
                this.notificationService.showDialog(terms["common.user.roles.defaultrolemandatory.title"], terms["common.user.roles.defaultrolemandatory.message"], SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK);
            });
            return false;
        }

        return true;
    }

    private validateRoleDates(): ng.IPromise<boolean> {
        var deferral = this.$q.defer<boolean>();

        let allRolesValid: boolean = true;
        let date = CalendarUtility.getDateToday();

        _.forEach(this.companyRoles, companyRole => {
            _.forEach(companyRole.userCompanyRoles, role => {
                if ((role.dateFrom && role.dateFrom.isAfterOnDay(date)) || (role.dateTo && role.dateTo.isBeforeOnDay(date))) {
                    allRolesValid = false;
                    return false;
                }
            });
        });

        if (!allRolesValid) {
            let keys: string[] = [
                "common.user.roles.invaliddateinterval.title",
                "common.user.roles.invaliddateinterval.message"
            ];

            this.translationService.translateMany(keys).then(terms => {
                let modal = this.notificationService.showDialog(terms["common.user.roles.invaliddateinterval.title"], terms["common.user.roles.invaliddateinterval.message"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                modal.result.then(val => {
                    deferral.resolve(true);
                }, (reason) => {
                    deferral.resolve(false);
                });
            });
        } else {
            deferral.resolve(true);
        }

        return deferral.promise;
    }

    // HELP-METHODS

    private get userIsMySelf(): boolean {
        return this.userId === CoreUtility.userId;
    }

    private get modifyCurrentUserPermission(): boolean {
        return soeConfig.isAdmin || !this.userIsMySelf;
    }

    private setVisible() {
        _.forEach(this.companyRoles, companyRole => {
            companyRole.visible = this.showAllCompanies || companyRole.hasUserRoles || companyRole.hasUserAttestRoles || this.companyRoles.length === 1;
        });

        this.allRowsExpanded = _.filter(this.companyRoles, c => !c.expanded).length === 0;
    }

    private get visibleCompanyRoles(): CompanyRolesDTO[] {
        return _.filter(this.companyRoles, r => r.visible);
    }

    private getUserRoleForCompany(actorCompanyId: number) {
        return _.find(this.userRoles, r => r.actorCompanyId === actorCompanyId);
    }

    private get hasDefaultRole(): boolean {
        let hasDefault: boolean = false;
        _.forEach(this.companyRoles, companyRole => {
            if (_.filter(companyRole.userCompanyRoles, r => r.default).length > 0) {
                hasDefault = true;
                return false;
            }
        });

        return hasDefault;
    }

    private copyModifiedRoles() {
        _.forEach(this.companyRoles, companyRole => {
            // Get user roles for current company
            let userRolesForCompany: UserRolesDTO = this.getUserRoleForCompany(companyRole.actorCompanyId);

            // Roles
            let modifiedRoles: UserCompanyRoleDTO[] = _.filter(companyRole.userCompanyRoles, r => r.isModified);
            if (modifiedRoles.length > 0) {
                if (!userRolesForCompany) {
                    userRolesForCompany = this.copyCompanyRoleToUserRole(companyRole);
                    this.userRoles.push(userRolesForCompany);
                }

                _.forEach(modifiedRoles, role => {
                    // Check if current role exist in user roles
                    let userRole: UserCompanyRoleDTO = null;
                    if (userRolesForCompany) {
                        if (role.userCompanyRoleId)
                            userRole = _.find(userRolesForCompany.roles, r => r.userCompanyRoleId === role.userCompanyRoleId);
                        else if (role.tmpUserCompanyRoleId)
                            userRole = _.find(userRolesForCompany.roles, r => r.tmpUserCompanyRoleId === role.tmpUserCompanyRoleId);
                        else
                            userRole = _.find(userRolesForCompany.roles, r => r.roleId === role.roleId);
                    }

                    if (role.selected) {
                        if (!userRole) {
                            // Role was selected, add it to user roles
                            userRole = new UserCompanyRoleDTO();
                            userRole.name = role.name;
                            userRole.tmpUserCompanyRoleId = role.tmpUserCompanyRoleId;
                            userRolesForCompany.roles.push(userRole);
                        }

                        userRole.roleId = role.roleId;
                        userRole.name = role.name;
                        userRole.dateFrom = role.dateFrom;
                        userRole.dateTo = role.dateTo;
                        userRole.default = role.default;
                        userRole.isModified = true;
                    } else {
                        if (userRole) {
                            // Role was unselected, remove it from user roles
                            _.pull(userRolesForCompany.roles, userRole);
                        }
                    }
                });
            }

            if (userRolesForCompany)
                userRolesForCompany.defaultCompany = companyRole.defaultCompany;

            // Attest roles
            let modifiedAttestRoles: UserCompanyAttestRoleDTO[];
            modifiedAttestRoles = _.filter(companyRole.userCompanyAttestRoles, r => r.isModified);
            if (modifiedAttestRoles.length > 0) {
                if (!userRolesForCompany) {
                    userRolesForCompany = this.copyCompanyRoleToUserRole(companyRole);
                    this.userRoles.push(userRolesForCompany);
                }

                let newUserAttestRoles: UserAttestRoleDTO[] = [];
                _.forEach(modifiedAttestRoles, userCompanyAttestRole => {
                    // Check if current attest role exist in user attest roles
                    let userAttestRole: UserAttestRoleDTO = null;
                    if (userRolesForCompany) {
                        if (userCompanyAttestRole.attestRoleUserId === 0)
                            userAttestRole = _.find(userRolesForCompany.attestRoles, r => r.tmpAttestRoleUserId === userCompanyAttestRole.tmpAttestRoleUserId);
                        else
                            userAttestRole = _.find(userRolesForCompany.attestRoles, r => r.attestRoleUserId === userCompanyAttestRole.attestRoleUserId);
                    }

                    if (userCompanyAttestRole.selected) {
                        if (!userAttestRole) {
                            let tmpIds: number[] = newUserAttestRoles.map(a => a.tmpAttestRoleUserId).concat(userRolesForCompany.attestRoles.map(a => a.tmpAttestRoleUserId));
                            let minAttestRoleUserId: number = _.min(tmpIds);

                            // Role was selected, add it to user attest roles
                            let newUserAttestRole: UserAttestRoleDTO = new UserAttestRoleDTO();
                            newUserAttestRole.isModified = true;
                            newUserAttestRole.attestRoleUserId = 0;
                            newUserAttestRole.tmpAttestRoleUserId = (minAttestRoleUserId || 0) - 1;
                            newUserAttestRole.attestRoleId = userCompanyAttestRole.attestRoleId;
                            newUserAttestRole.accountId = userCompanyAttestRole.accountId;
                            newUserAttestRole.accountName = userCompanyAttestRole.accountName;
                            newUserAttestRole.accountDimId = userCompanyAttestRole.accountDimId;
                            newUserAttestRole.accountDimName = userCompanyAttestRole.accountDimName;
                            newUserAttestRole.name = userCompanyAttestRole.name;
                            newUserAttestRole.moduleName = userCompanyAttestRole.moduleName;
                            newUserAttestRole.dateFrom = userCompanyAttestRole.dateFrom;
                            newUserAttestRole.dateTo = userCompanyAttestRole.dateTo;
                            newUserAttestRole.maxAmount = userCompanyAttestRole.maxAmount;
                            newUserAttestRole.isExecutive = userCompanyAttestRole.isExecutive;
                            newUserAttestRole.isNearestManager = userCompanyAttestRole.isNearestManager;
                            newUserAttestRole.accountPermissionType = userCompanyAttestRole.accountPermissionType;
                            newUserAttestRole.accountPermissionTypeName = userCompanyAttestRole.accountPermissionTypeName;
                            newUserAttestRole.children = userCompanyAttestRole.children;
                            newUserAttestRole.state = userCompanyAttestRole.state;
                            newUserAttestRole.roleId = userCompanyAttestRole.roleId;
                            newUserAttestRoles.push(newUserAttestRole);
                        } else {
                            //Role is only modified                                                              
                            userAttestRole.isModified = true;
                            userAttestRole.attestRoleId = userCompanyAttestRole.attestRoleId;
                            userAttestRole.dateFrom = userCompanyAttestRole.dateFrom;
                            userAttestRole.dateTo = userCompanyAttestRole.dateTo;
                            userAttestRole.maxAmount = userCompanyAttestRole.maxAmount;
                            userAttestRole.accountId = userCompanyAttestRole.accountId;
                            userAttestRole.accountName = userCompanyAttestRole.accountName;
                            userAttestRole.accountDimId = userCompanyAttestRole.accountDimId;
                            userAttestRole.accountDimName = userCompanyAttestRole.accountDimName;
                            userAttestRole.name = userCompanyAttestRole.name;
                            userAttestRole.isExecutive = userCompanyAttestRole.isExecutive;
                            userAttestRole.isNearestManager = userCompanyAttestRole.isNearestManager;
                            userAttestRole.accountPermissionType = userCompanyAttestRole.accountPermissionType;
                            userAttestRole.accountPermissionTypeName = userCompanyAttestRole.accountPermissionTypeName;
                            userAttestRole.children = userCompanyAttestRole.children;
                            userAttestRole.state = userCompanyAttestRole.state;
                            userAttestRole.roleId = userCompanyAttestRole.roleId;
                        }
                    } else {
                        if (userAttestRole) {
                            // Role was unselected, remove it from user attest roles
                            _.pull(userRolesForCompany.attestRoles, userAttestRole);
                        }
                    }
                });
                userRolesForCompany.attestRoles.push(...newUserAttestRoles);
            }
        });
    }

    private copyCompanyRoleToUserRole(companyRole: CompanyRolesDTO): UserRolesDTO {
        var userRole: UserRolesDTO = new UserRolesDTO();
        userRole.actorCompanyId = companyRole.actorCompanyId;
        userRole.companyName = companyRole.companyName;
        userRole.defaultCompany = companyRole.defaultCompany;
        userRole.attestRoles = [];
        userRole.roles = [];

        return userRole;
    }
}
