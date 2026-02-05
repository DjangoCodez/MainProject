import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { EmployeeUserDTO, UserRolesDTO } from "../../../Common/Models/EmployeeUserDTO";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { NotificationService } from "../../../Core/Services/NotificationService";
import { IFocusService } from "../../../Core/Services/FocusService";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { Feature, SaveEmployeeUserResult, ContactAddressItemType, UserReplacementType, SoeEntityState, DeleteUserAction, LicenseSettingType, CompanySettingType } from "../../../Util/CommonEnumerations";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { Constants } from "../../../Util/Constants";
import { CoreUtility } from "../../../Util/CoreUtility";
import { IconLibrary, EditUserFunctions } from "../../../Util/Enumerations";
import { ContactAddressItemDTO } from "../../../Common/Models/ContactAddressDTOs";
import { ToolBarUtility, ToolBarButton } from "../../../Util/ToolBarUtility";
import { Guid } from "../../../Util/StringUtility";
import { UserReplacementDTO } from "../../../Common/Models/UserDTO";
import { IUserService } from "../UserService";
import { HtmlUtility } from "../../../Util/HtmlUtility";
import { DeleteUserController } from "./Dialogs/DeleteUser/DeleteUserController";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    private userId: number;
    private user: EmployeeUserDTO;
    private selectedCompanyId: number = 0;
    private selectedLicenseId: number = 0;

    private userIsMySelf: boolean = false;

    // Lookups
    private contactAddressItems: ContactAddressItemDTO[];
    private userRoles: UserRolesDTO[];
    private userRolesHasChanges: boolean;
    private userAttestRolesHasChanges: boolean;
    private userHasChanges: boolean;
    private replacementUsers: any[];
    private replacementUser: UserReplacementDTO;

    // Permissions
    private readPermissions: any[];
    private modifyPermissions: any[];
    private contactReadPermission: boolean = false;
    private contactModifyPermission: boolean = false;
    private userMappingReadPermission: boolean = false;
    private userMappingModifyPermission: boolean = false;
    private attestRoleMappingReadPermission: boolean = false;
    private attestRoleMappingModifyPermission: boolean = false;
    private delegateMySelfReadPermission: boolean = false;
    private delegateMySelfModifyPermission: boolean = false;
    private delegateOtherUsersReadPermission: boolean = false;
    private delegateOtherUsersModifyPermission: boolean = false;
    private userReplacementReadPermission: boolean = false;
    private userReplacementModifyPermission: boolean = false;
    private userSessionsReadPermission: boolean = false;
    private gdprLogsReadPermission: boolean = false;
    private gdprLogsModifyPermission: boolean = false;

    private get delegateMySelfPermission(): boolean {
        return this.delegateMySelfReadPermission || this.delegateMySelfModifyPermission;
    }

    private get delegateOtherUsersPermission(): boolean {
        return this.delegateOtherUsersReadPermission || this.delegateOtherUsersModifyPermission;
    }

    private get delegateModifyPermission(): boolean {
        return this.modifyPermission && ((this.userIsMySelf && this.delegateMySelfModifyPermission) || (!this.userIsMySelf && this.delegateOtherUsersModifyPermission));
    }

    // License settings
    private showSso: boolean = false;

    // Company settings
    private useAccountHierarchy: boolean = false;

    // Flags
    private isContactAddressesValid: boolean = true;
    private contactAddressesValidationErrors: string;
    private isLoginNameValid: boolean = false;
    private delegateHistoryExpanderOpen: boolean = false;

    private modal;
    private isModal = false;
    private modalInstance: any;
    private showLifetime: boolean = false;

    //@ngInject
    constructor(
        private $scope: ng.IScope,
        protected $uibModal,
        private $q: ng.IQService,
        private $timeout: ng.ITimeoutService,
        private $window,
        private coreService: ICoreService,
        private userService: IUserService,
        private urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        private notificationService: NotificationService,
        private focusService: IFocusService,
        progressHandlerFactory: IProgressHandlerFactory,
        private controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.modalInstance = $uibModal;

        $scope.$on(Constants.EVENT_ON_INIT_MODAL, (e, parameters) => {
            parameters.guid = Guid.newGuid();
            this.isModal = true;
            this.modal = parameters.modal;

            this.onInit(parameters);
        });

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onLoadData(() => this.onLoadData())
            .onDoLookUp(() => this.onDoLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    public onInit(parameters: any) {
        this.userId = parameters.id;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.selectedCompanyId = soeConfig.selectedCompanyId || 0;
        this.selectedLicenseId = soeConfig.selectedLicenseId || 0;

        this.flowHandler.start([{ feature: Feature.Manage_Users_Edit, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    public save() {
        this.initSaveEmployeeUser();
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Manage_Users_Edit].readPermission;
        this.modifyPermission = response[Feature.Manage_Users_Edit].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(false, null, () => this.isNew);

        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("manage.user.user.delete", "manage.user.user.delete.title", IconLibrary.FontAwesome, "fa-user-secret", () => {
            this.openDeleteDialog();
        }, () => { return this.dirtyHandler.isDirty }, () => {
            return !this.gdprLogsModifyPermission || !this.user || !this.user.userId;
        })));
        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("manage.user.user.showsessions", "manage.user.user.showsessions.title", IconLibrary.FontAwesome, "fa-sign-in", () => {
            this.showSessions();
        }, null, () => {
            return !this.userSessionsReadPermission || !this.user || !this.user.userId;
        })));
    }

    private onDoLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.loadLicenseSettings(),
            this.loadReadOnlyPermissions(),
            this.loadModifyPermissions(),
            this.loadCompanySettings()
        ]);
    }

    private onLoadData(): ng.IPromise<any> {
        if (this.userId) {
            return this.loadUser();
        } else {
            this.new();
        }
    }

    // LOOKUPS

    private loadReadOnlyPermissions(): ng.IPromise<any> {
        var features: number[] = [];

        features.push(Feature.Manage_Users_Edit);
        features.push(Feature.Manage_Users_Edit_UserMapping);
        features.push(Feature.Manage_Users_Edit_AttestRoleMapping);
        features.push(Feature.Manage_Users_Edit_AttestReplacementMapping);
        features.push(Feature.Manage_Users_Edit_Sessions);
        features.push(Feature.Manage_Users_Edit_Delegate_MySelf);
        features.push(Feature.Manage_Users_Edit_Delegate_OtherUsers);
        features.push(Feature.Manage_GDPR_Logs);

        return this.coreService.hasReadOnlyPermissions(features).then(x => {
            this.readPermissions = x;
        });
    }

    private loadModifyPermissions(): ng.IPromise<any> {
        var features: number[] = [];

        features.push(Feature.Manage_Users_Edit);
        features.push(Feature.Manage_Users_Edit_UserMapping);
        features.push(Feature.Manage_Users_Edit_AttestRoleMapping);
        features.push(Feature.Manage_Users_Edit_AttestReplacementMapping);
        features.push(Feature.Manage_GDPR_Logs);
        features.push(Feature.Manage_Users_Edit_Delegate_MySelf);
        features.push(Feature.Manage_Users_Edit_Delegate_OtherUsers);

        return this.coreService.hasModifyPermissions(features).then(x => {
            this.modifyPermissions = x;
        });
    }

    private setPermissions() {
        // Read

        // Personal
        this.contactReadPermission = (this.userIsMySelf && this.readPermissions[Feature.Manage_Users_Edit]) || this.readPermissions[Feature.Manage_Users_Edit];
        this.userMappingReadPermission = this.readPermissions[Feature.Manage_Users_Edit_UserMapping];
        this.attestRoleMappingReadPermission = this.readPermissions[Feature.Manage_Users_Edit_AttestRoleMapping];
        this.delegateMySelfReadPermission = this.readPermissions[Feature.Manage_Users_Edit_Delegate_MySelf];
        this.delegateOtherUsersReadPermission = this.readPermissions[Feature.Manage_Users_Edit_Delegate_OtherUsers];
        this.userReplacementReadPermission = this.readPermissions[Feature.Manage_Users_Edit_AttestReplacementMapping];
        this.userSessionsReadPermission = this.readPermissions[Feature.Manage_Users_Edit_Sessions];
        this.gdprLogsReadPermission = this.readPermissions[Feature.Manage_GDPR_Logs];

        // Modify

        // Personal
        this.contactModifyPermission = (this.userIsMySelf && this.modifyPermissions[Feature.Manage_Users_Edit]) || this.modifyPermissions[Feature.Manage_Users_Edit];
        this.userMappingModifyPermission = this.modifyPermissions[Feature.Manage_Users_Edit_UserMapping];
        this.attestRoleMappingModifyPermission = this.modifyPermissions[Feature.Manage_Users_Edit_AttestRoleMapping];
        this.delegateMySelfModifyPermission = this.modifyPermissions[Feature.Manage_Users_Edit_Delegate_MySelf];
        this.delegateOtherUsersModifyPermission = this.modifyPermissions[Feature.Manage_Users_Edit_Delegate_OtherUsers];
        this.userReplacementModifyPermission = this.modifyPermissions[Feature.Manage_Users_Edit_AttestReplacementMapping];
        if (this.userReplacementReadPermission)
            this.loadReplacementUsers();

        this.gdprLogsModifyPermission = this.modifyPermissions[Feature.Manage_GDPR_Logs];
    }

    private loadLicenseSettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];
        settingTypes.push(LicenseSettingType.SSO_Key);
        settingTypes.push(LicenseSettingType.LifetimeSecondsEnabledOnUser);

        return this.coreService.getLicenseSettings(settingTypes).then(x => {
            let setting = SettingsUtility.getStringLicenseSetting(x, LicenseSettingType.SSO_Key);
            if (setting || setting.length > 2)
                this.showSso = true;

            let lts = SettingsUtility.getBoolLicenseSetting(x, LicenseSettingType.LifetimeSecondsEnabledOnUser);
            if (lts)
                this.showLifetime = true;
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];
        settingTypes.push(CompanySettingType.UseAccountHierarchy);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.useAccountHierarchy = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseAccountHierarchy);
        });
    }

    private loadReplacementUsers(): ng.IPromise<any> {
        return this.coreService.getUsersDict(true, false, false, false).then(x => {
            this.replacementUsers = x;
        });
    }

    private loadUserReplacement() {
        this.replacementUser = null;
        if (this.user.userId) {
            this.coreService.getUserReplacement(UserReplacementType.AttestFlow, this.user.userId).then(x => {
                this.replacementUser = x;
            });
        }
    }

    private loadUser(): ng.IPromise<any> {
        return this.userService.getUserForEdit(this.userId, CoreUtility.userId).then(x => {
            this.isNew = false;

            // Must set userIsMySelf before setting permissions
            this.userIsMySelf = (x && x.userId === CoreUtility.userId);
            this.setPermissions();

            this.user = x;
            this.messagingHandler.publishSetTabLabel(this.guid, "{0} - {1}".format(this.user.loginName, this.user.name));

            // Some related info needs to be loaded after the employee itself has been loaded
            this.$q.all([
                this.validateLoginName(),
                this.loadContactAddressItems()
            ]).then(() => {
                if (this.userReplacementReadPermission)
                    this.loadUserReplacement();

                this.dirtyHandler.clean();
            });
        });
    }

    private loadContactAddressItems(): ng.IPromise<any> {
        var deferral = this.$q.defer();

        this.contactAddressItems = [];
        if (!this.userId)
            deferral.resolve();
        else {
            return this.userService.getContactAddressItems(this.user.actorContactPersonId).then((x) => {
                this.contactAddressItems = x;
            });
        }

        return deferral.promise;
    }

    private new() {
        this.isNew = true;

        this.userIsMySelf = false;
        this.setPermissions();

        this.userId = 0;
        this.user = new EmployeeUserDTO();
        this.user.licenseId = CoreUtility.licenseId;
        this.user.actorCompanyId = CoreUtility.actorCompanyId;

        this.user.langId = CoreUtility.languageId;
        this.user.isMobileUser = true;

        this.validateLoginName();

        this.focusService.focusById("ctrl_user_firstName");
    }

    // TOOLBAR

    private openDeleteDialog() {
        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Manage/User/Users/Dialogs/DeleteUser/deleteUser.html"),
            controller: DeleteUserController,
            controllerAs: 'ctrl',
            bindToController: true,
            backdrop: 'static',
            size: 'lg',
            resolve: {
                userId: () => { return this.userId; },
                userText: () => { return '({0}) {1}'.format(this.user.loginName, this.user.name); },
                employeeId: () => { return this.user.employeeId; },
                employeeNr: () => { return this.user.employeeNr; }
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            this.messagingHandler.publishReloadGrid(this.guid);
            var action: DeleteUserAction = result.action;
            if (action == DeleteUserAction.Inactivate || action == DeleteUserAction.RemoveInfo)
                this.loadUser();
            else
                this.closeMe(true);
        });
    }

    private showSessions() {
        var url: string = "/soe/manage/users/edit/sessions/?user=" + this.userId + "&license=" + this.selectedLicenseId + "&company=" + this.selectedCompanyId;
        HtmlUtility.openInNewTab(this.$window, url);
    }

    // EVENTS

    public closeModal(modified: boolean) {
        if (this.isModal) {
            if (this.userId) {
                this.modal.close({ modified: modified, id: this.userId });
            } else {
                this.modal.dismiss();
            }
        }
    }

    private addDefaultContactAddresRow() {

    }

    private contactAddressesChanged() {
        this.user.saveUser = true;
        this.setDirty();
    }

    private selectContactAddress() {
        this.$scope.$broadcast('selectContactAddresRow', { index: 0 });
    }

    private emailCopyChanged() {
        this.user.saveUser = true;
        this.setDirty();
    }

    private userInfoChanged(result) {
        this.$timeout(() => {
            this.validateLoginName();

            if (result.result.function == EditUserFunctions.DisconnectEmployee)
                this.user.disconnectExistingEmployee = true;

            this.user.saveUser = true;
            this.setDirty();
        });
    }

    private validateLoginName() {
        this.isLoginNameValid = !!(this.user && this.user.loginName && this.user.loginName.length > 0);
    }

    private userRolesChanged() {
        this.user.saveUser = true;
        this.setDirty();
    }

    private userActiveChanged() {
        this.user.saveUser = true;
        this.setDirty();
    }

    private replacementUserValuesChanged() {
        this.user.saveUser = true;
        this.setDirty();
    }

    // ACTIONS

    private initSaveEmployeeUser() {
        this.user.saveEmployee = false;
        //this.user.saveUser = !!this.user.loginName && !this.user.disconnectExistingUser;

        this.validateSaveUser().then(passed => {
            if (passed) {
                this.saveEmployeeUser();
            }
        });
    }

    private saveEmployeeUser() {
        if (!this.user.name)
            this.user.name = this.user.firstName + " " + this.user.lastName;

        if (this.userRoles && this.userRoles.length > 0) {
            let defaultUserRole = this.userRoles.find(r => r.defaultCompany);
            if (defaultUserRole)
                this.user.defaultActorCompanyId = defaultUserRole.actorCompanyId;
        }

        if (!this.user.actorCompanyId)
            this.user.actorCompanyId = this.user.defaultActorCompanyId;

        // Attestflow replacement user
        if (this.replacementUser) {
            this.replacementUser.actorCompanyId = CoreUtility.actorCompanyId;
            this.replacementUser.originUserId = this.user.userId;
            this.replacementUser.type = UserReplacementType.AttestFlow;
            if (!this.replacementUser.replacementUserId)
                this.replacementUser.state = SoeEntityState.Deleted;
        }

        this.progress.startSaveProgress((completion) => {
            this.userService.saveEmployeeUser(this.user, this.contactAddressItems, this.replacementUser, this.userRolesHasChanges, this.userAttestRolesHasChanges, this.userRoles).then(result => {
                if (result.success) {
                    if (result['intDict']) {
                        this.userId = result['intDict'][SaveEmployeeUserResult.UserId];
                        this.user.userId = this.userId;
                    }

                    if (this.userHasChanges)
                        this.$scope.$broadcast('reloadUserInfo');

                    if (this.userRoles && (this.userRolesHasChanges || this.userAttestRolesHasChanges))
                        this.$scope.$broadcast('reloadUserRoles');

                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.user);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid).then(data => {
            if (this.isModal)
                this.closeModal(true);
            else
                this.onLoadData();
        }, error => {
        });
    }

    // HELP-METHODS

    private setDirty() {
        this.dirtyHandler.setDirty();
    }

    // VALIDATION

    private validateSaveUser(): ng.IPromise<boolean> {
        return this.userService.validateSaveUser(this.user, this.contactAddressItems).then(result => {
            return this.notificationService.showValidateSaveEmployee(result);
        });
    }

    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.user) {
                var errors = this['edit'].$error;

                var noPermission: boolean = false;

                if (!this.user.firstName)
                    mandatoryFieldKeys.push("common.firstname");

                if (!this.user.lastName)
                    mandatoryFieldKeys.push("common.lastname");

                if (errors['contactAddress'])
                    validationErrorStrings.push(this.contactAddressesValidationErrors);

                if (!this.user.loginName) {
                    mandatoryFieldKeys.push("manage.user.user.loginname");
                    if (!this.modifyPermission)
                        noPermission = true;
                }

                if (errors['role']) {
                    validationErrorKeys.push("manage.user.user.missingrole");
                    if (!this.modifyPermission)
                        noPermission = true;
                }

                if (errors['defaultRole']) {
                    validationErrorKeys.push("manage.user.user.missingdefaultrole");
                    if (!this.modifyPermission)
                        noPermission = true;
                }

                if (noPermission) {
                    if ((mandatoryFieldKeys.length + validationErrorKeys.length + validationErrorStrings.length) > 1)
                        validationErrorKeys.push("manage.user.user.nomodifypermissions");
                    else
                        validationErrorKeys.push("manage.user.user.nomodifypermission");
                }
            }
        });
    }
}