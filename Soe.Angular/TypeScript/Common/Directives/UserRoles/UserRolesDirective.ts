import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ICoreService } from "../../../Core/Services/CoreService";
import { UserRolesDTO, CompanyRolesDTO, UserAttestRoleDTO, UserCompanyAttestRoleDTO, CompanyAttestRoleDTO, UserCompanyRoleDTO } from "../../../Common/Models/EmployeeUserDTO";
import { UserRolesDialogController } from "./UserRolesDialogController";
import { CoreUtility } from "../../../Util/CoreUtility";
import { CompanySettingType } from "../../../Util/CommonEnumerations";
import { SettingsUtility } from "../../../Util/SettingsUtility";

export class UserRolesDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Common/Directives/UserRoles/Views/UserRoles.html'),
            scope: {
                userId: '=',
                userRoles: '=',
                userRolesReadPermission: '=',
                userRolesModifyPermission: '=',
                attestRolesReadPermission: '=',
                attestRolesModifyPermission: '=',
                userRolesHasChanges: '=',
                userAttestRolesHasChanges: '=',
                onChange: '&'
            },
            restrict: 'E',
            replace: true,
            controller: UserRolesController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

export class UserRolesController {

    //Company settings
    private useAccountsHierarchy: boolean;
    private defaultEmployeeAccountDimId: number;
    private useLimitedEmployeeAccountDimLevels: boolean;
    private useExtendedEmployeeAccountDimLevels: boolean;
    private useIsNearestManagerOnAttestRoleUser: boolean;

    // Init parameters
    private userId: number;
    private userRolesHasChanges: boolean;
    private userAttestRolesHasChanges: boolean;

    // Data
    private userRoles: UserRolesDTO[];
    private companyRoles: CompanyRolesDTO[];

    // Flags
    private userRolesReadPermission: boolean;
    private userRolesModifyPermission: boolean;
    private attestRolesReadPermission: boolean;
    private attestRolesModifyPermission: boolean;
    private loading: boolean = false;

    // Events
    private onChange: Function;

    //@ngInject
    constructor(
        private $uibModal,
        private $scope: ng.IScope,
        private $q: ng.IQService,
        private urlHelperService: IUrlHelperService,
        private coreService: ICoreService) {
    }

    public $onInit() {
        this.setupWatchers();
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.userId, (newVal, oldVal) => {
            if (this.userId) {
                this.loading = true;
                this.$q.all([
                    this.loadCompanyRoles(),
                    this.loadUserRoles(),
                    this.loadCompanySettings(),
                ]).then(() => {
                    this.setSelected();
                });
            }
        });

        this.$scope.$on('reloadUserRoles', (e) => {
            this.loading = true;
            this.$q.all([
                this.loadUserRoles(),
                this.loadCompanyRoles]).then(() => {
                    this.setSelected();
                    this.userRolesHasChanges = false;
                    this.userAttestRolesHasChanges = false;
                });
        });
    }

    // SERVICE CALLS
    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];

        settingTypes.push(CompanySettingType.UseAccountHierarchy);
        settingTypes.push(CompanySettingType.DefaultEmployeeAccountDimEmployee);
        settingTypes.push(CompanySettingType.UseLimitedEmployeeAccountDimLevels);
        settingTypes.push(CompanySettingType.UseExtendedEmployeeAccountDimLevels);
        settingTypes.push(CompanySettingType.UseIsNearestManagerOnAttestRoleUser);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.useAccountsHierarchy = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseAccountHierarchy);
            this.defaultEmployeeAccountDimId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.DefaultEmployeeAccountDimEmployee);
            this.useLimitedEmployeeAccountDimLevels = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseLimitedEmployeeAccountDimLevels);
            this.useExtendedEmployeeAccountDimLevels = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseExtendedEmployeeAccountDimLevels);
            this.useIsNearestManagerOnAttestRoleUser = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseIsNearestManagerOnAttestRoleUser);
        });
    }

    private loadUserRoles(): ng.IPromise<any> {
        this.userRoles = [];
        return this.coreService.getUserRoles(this.userId, true).then(x => {
            this.userRoles = x;
        });
    }

    private loadCompanyRoles(): ng.IPromise<any> {
        this.companyRoles = [];
        return this.coreService.getCompanyRoles(soeConfig.isAdmin, CoreUtility.userId).then(x => {
            this.companyRoles = x;
        });
    }

    // ACTIONS

    private openUserRolesDialog() {
        var modal = this.$uibModal.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Common/Directives/UserRoles/Views/UserRolesDialog.html"),
            controller: UserRolesDialogController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'xl',
            windowClass: 'fullsize-modal',
            scope: this.$scope,
            resolve: {
                userRolesReadPermission: () => { return this.userRolesReadPermission; },
                userRolesModifyPermission: () => { return this.userRolesModifyPermission; },
                attestRolesReadPermission: () => { return this.attestRolesReadPermission; },
                attestRolesModifyPermission: () => { return this.attestRolesModifyPermission; },
                useAccountsHierarchy: () => { return this.useAccountsHierarchy; },
                defaultEmployeeAccountDimId: () => { return this.defaultEmployeeAccountDimId; },
                useLimitedEmployeeAccountDimLevels: () => { return this.useLimitedEmployeeAccountDimLevels; },
                useExtendedEmployeeAccountDimLevels: () => { return this.useExtendedEmployeeAccountDimLevels; },
                useIsNearestManagerOnAttestRoleUser: () => { return this.useIsNearestManagerOnAttestRoleUser; },
                userId: () => { return this.userId; },
                userRoles: () => { return this.userRoles; },
                companyRoles: () => { return this.companyRoles; }
            }
        });

        modal.result.then(result => {
            if (result && (result.roleHasChanges || result.attestRoleHasChanges)) {
                if (result.roleHasChanges)
                    this.userRolesHasChanges = true;
                if (result.attestRoleHasChanges)
                    this.userAttestRolesHasChanges = true;

                this.userRoles = _.filter(result.userRoles, r => r.roles.length > 0);

                _.forEach(result.userRoles, (userRole: UserRolesDTO) => {
                    let userRolesForCompany: UserRolesDTO = _.find(this.userRoles, r => r.actorCompanyId === userRole.actorCompanyId);
                    if (userRolesForCompany)
                        userRolesForCompany.defaultCompany = userRole.defaultCompany;
                });

                this.setSelected();

                if (this.onChange)
                    this.onChange();
            }
        });
    }

    // HELP-METHODS

    private get userIsMySelf(): boolean {
        return this.userId === CoreUtility.userId;
    }

    private get modifyCurrentUserPermission(): boolean {
        return soeConfig.isAdmin || !this.userIsMySelf;
    }

    private setSelected() {
        var defaultCompany = _.find(this.userRoles, r => r.defaultCompany);
        var defaultCompanyId: number = defaultCompany ? defaultCompany.actorCompanyId : 0;
        _.forEach(this.companyRoles, companyRole => {
            companyRole.defaultCompany = (companyRole.actorCompanyId === defaultCompanyId);

            let userRolesForCompany: UserRolesDTO = _.find(this.userRoles, r => r.actorCompanyId === companyRole.actorCompanyId);
            companyRole.hasUserRoles = userRolesForCompany && userRolesForCompany.roles.length > 0;
            companyRole.hasUserAttestRoles = userRolesForCompany && userRolesForCompany.attestRoles.length > 0;
            companyRole.userCompanyRoles = [];
            companyRole.userCompanyAttestRoles = [];

            if (userRolesForCompany) {
                userRolesForCompany.visible = (userRolesForCompany.roles.length > 0 || userRolesForCompany.attestRoles.length > 0);

                // Create UserCompanyRoleDTO from UserRoleDTO
                _.forEach(userRolesForCompany.roles, role => {
                    let companyUserRole: UserCompanyRoleDTO = _.find(companyRole.roles, r => r.roleId === role.roleId);
                    if (companyUserRole) {
                        let userCompanyRole: UserCompanyRoleDTO = new UserCompanyRoleDTO();
                        userCompanyRole.readOnly = !this.userRolesModifyPermission || !this.modifyCurrentUserPermission;
                        userCompanyRole.isModified = role.isModified;
                        userCompanyRole.isDelegated = role.isDelegated;
                        userCompanyRole.selected = true;
                        userCompanyRole.initiallySelected = true;
                        userCompanyRole.userCompanyRoleId = role.userCompanyRoleId;
                        userCompanyRole.tmpUserCompanyRoleId = role.tmpUserCompanyRoleId;
                        userCompanyRole.name = role.name;
                        userCompanyRole.roleId = role.roleId;
                        userCompanyRole.default = role.default;
                        userCompanyRole.dateFrom = role.dateFrom;
                        userCompanyRole.dateTo = role.dateTo;
                        userCompanyRole.state = role.state;
                        companyRole.userCompanyRoles.push(userCompanyRole);
                    }
                });

                // Create UserCompanyAttestRoleDTO from UserAttestRoleDTO
                _.forEach(userRolesForCompany.attestRoles, userAttestRole => {
                    let companyAttestRole: CompanyAttestRoleDTO = _.find(companyRole.attestRoles, r => r.attestRoleId === userAttestRole.attestRoleId);
                    if (companyAttestRole) {
                        let userCompanyAttestRole: UserCompanyAttestRoleDTO = new UserCompanyAttestRoleDTO();
                        userCompanyAttestRole.readOnly = !this.attestRolesModifyPermission || !this.modifyCurrentUserPermission;
                        userCompanyAttestRole.isModified = userAttestRole.isModified;
                        userCompanyAttestRole.isDelegated = userAttestRole.isDelegated;
                        userCompanyAttestRole.selected = true;
                        userCompanyAttestRole.initiallySelected = true;
                        userCompanyAttestRole.attestRoleUserId = userAttestRole.attestRoleUserId;
                        userCompanyAttestRole.tmpAttestRoleUserId = userAttestRole.tmpAttestRoleUserId;
                        userCompanyAttestRole.name = userAttestRole.name;
                        userCompanyAttestRole.moduleName = userAttestRole.moduleName;
                        userCompanyAttestRole.attestRoleId = userAttestRole.attestRoleId;
                        userCompanyAttestRole.accountId = userAttestRole.accountId;
                        userCompanyAttestRole.accountName = userAttestRole.accountName;
                        userCompanyAttestRole.accountDimId = userAttestRole.accountDimId;
                        userCompanyAttestRole.accountDimName = userAttestRole.accountDimName;
                        userCompanyAttestRole.isExecutive = userAttestRole.isExecutive;
                        userCompanyAttestRole.isNearestManager = userAttestRole.isNearestManager;
                        userCompanyAttestRole.accountPermissionType = userAttestRole.accountPermissionType;
                        userCompanyAttestRole.accountPermissionTypeName = userAttestRole.accountPermissionTypeName;
                        userCompanyAttestRole.dateFrom = userAttestRole.dateFrom;
                        userCompanyAttestRole.dateTo = userAttestRole.dateTo;
                        userCompanyAttestRole.maxAmount = userAttestRole.maxAmount;
                        userCompanyAttestRole.defaultMaxAmount = companyAttestRole.defaultMaxAmount;
                        userCompanyAttestRole.children = userAttestRole.children;
                        userCompanyAttestRole.state = userAttestRole.state;
                        userCompanyAttestRole.roleId = userAttestRole.roleId;
                        companyRole.userCompanyAttestRoles.push(userCompanyAttestRole);
                    }
                });
            }

            // Create UserCompanyRoleDTO from CompanyRole that doesnt exists in userCompanyRoles (they will be shown as unselected)
            _.forEach(companyRole.roles, role => {
                let userCompanyRoleDTO: UserCompanyRoleDTO = _.find(companyRole.userCompanyRoles, r => r.roleId === role.roleId);
                if (!userCompanyRoleDTO) {
                    let userCompanyRole: UserCompanyRoleDTO = new UserCompanyRoleDTO();
                    userCompanyRole.readOnly = !this.userRolesModifyPermission || !this.modifyCurrentUserPermission;
                    userCompanyRole.name = role.name;
                    userCompanyRole.roleId = role.roleId;
                    companyRole.userCompanyRoles.push(userCompanyRole);
                }
            });

            // Create UserCompanyAttestRoleDTO from CompanyAttestRole that doesnt exists in userCompanyAttestRoles (they will be shown as unselected)
            _.forEach(companyRole.attestRoles, companyAttestRole => {
                let userCompanyAttestRoleDTO: UserCompanyAttestRoleDTO = _.find(companyRole.userCompanyAttestRoles, r => r.attestRoleId === companyAttestRole.attestRoleId);
                if (!userCompanyAttestRoleDTO) {
                    let userCompanyAttestRole: UserCompanyAttestRoleDTO = new UserCompanyAttestRoleDTO();
                    userCompanyAttestRole.readOnly = !this.attestRolesModifyPermission || !this.modifyCurrentUserPermission;
                    userCompanyAttestRole.name = companyAttestRole.name;
                    userCompanyAttestRole.state = companyAttestRole.state;
                    userCompanyAttestRole.moduleName = companyAttestRole.moduleName;
                    userCompanyAttestRole.attestRoleId = companyAttestRole.attestRoleId;
                    userCompanyAttestRole.defaultMaxAmount = companyAttestRole.defaultMaxAmount;
                    userCompanyAttestRole.state = companyAttestRole.state;
                    companyRole.userCompanyAttestRoles.push(userCompanyAttestRole);
                }
            });
        });

        this.loading = false;
    }
}
