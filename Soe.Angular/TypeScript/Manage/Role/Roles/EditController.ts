import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { INotificationService, NotificationService } from "../../../Core/Services/NotificationService";
import { IFocusService } from "../../../Core/Services/FocusService";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { Feature, Permission, SoeEntityState } from "../../../Util/CommonEnumerations";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { Constants } from "../../../Util/Constants";
import { Guid } from "../../../Util/StringUtility";
import { RoleEditDTO } from "../../../Common/Models/RoleDTOs";
import { IRoleService } from "../RoleService";
import { ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { HtmlUtility } from "../../../Util/HtmlUtility";
import { ToolBarButton, ToolBarButtonGroup, ToolBarUtility } from "../../../Util/ToolBarUtility";
import { IconLibrary, SOEMessageBoxButtons, SOEMessageBoxImage } from "../../../Util/Enumerations";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    private roleId: number;
    private role: RoleEditDTO;
    private selectedLicenseId: number = 0;
    private selectedLicenseNr: number = 0;
    private selectedCompanyId: number = 0;

    // Lookups
    private startPages: ISmallGenericType[];
    private companyRoles: ISmallGenericType[] = [];

    // Permissions
    private readPermissions: any[];
    private modifyPermissions: any[];
    private editPermissionsPermission: boolean = false;
    private usersPermission: boolean = false;

    // Flags
    private showConfigureInfo: boolean = false;


    //@ngInject
    constructor(
        private $window,
        protected $uibModal,
        private $q: ng.IQService,
        private coreService: ICoreService,
        private roleService: IRoleService,
        urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        private focusService: IFocusService,
        private notificationService: INotificationService,
        progressHandlerFactory: IProgressHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);
        this.isNew = false;
        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onLoadData(() => this.onLoadData())
            .onDoLookUp(() => this.onDoLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    public onInit(parameters: any) {
        this.roleId = parameters.id;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.selectedLicenseId = soeConfig.selectedLicenseId || 0;
        this.selectedLicenseNr = soeConfig.selectedLicenseNr || 0;
        this.selectedCompanyId = soeConfig.selectedCompanyId || 0;

        this.flowHandler.start([
            { feature: Feature.Manage_Roles_Edit, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Manage_Roles_Edit_Permission, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Manage_Users, loadReadPermissions: true, loadModifyPermissions: true }
        ]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Manage_Roles_Edit].readPermission;
        this.modifyPermission = response[Feature.Manage_Roles_Edit].modifyPermission;
        this.editPermissionsPermission = response[Feature.Manage_Roles_Edit_Permission].modifyPermission;
        this.usersPermission = response[Feature.Manage_Users].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => this.copy(), () => this.isNew);

        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("manage.role.role.showusers", "manage.role.role.showusers", IconLibrary.FontAwesome, "fa-user-friends", () => {
            this.showUsers();
        }, () => { return this.dirtyHandler.isDirty }, () => {
            return !this.usersPermission || !this.role || !this.role.roleId;
        })));

        if (this.editPermissionsPermission) {
            let permissionGroup: ToolBarButtonGroup = ToolBarUtility.createGroup(new ToolBarButton("manage.role.role.readpermission", "manage.role.role.readpermission", IconLibrary.FontAwesome, "fa-user-lock",
                () => { this.showReadPermissions(); }
            ));
            permissionGroup.buttons.push(new ToolBarButton("manage.role.role.modifypermission", "manage.role.role.modifypermission", IconLibrary.FontAwesome, "fa-user-edit",
                () => { this.showModifyPermissions(); }
            ));
            this.toolbar.addButtonGroup(permissionGroup);
        }
    }

    private onDoLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.loadStartPages()
        ]);
    }

    private onLoadData(): ng.IPromise<any> {
        if (this.roleId) {
            return this.loadRole();
        } else {
            this.new();
        }
    }

    // LOOKUPS

    private loadStartPages(): ng.IPromise<any> {
        return this.roleService.getStartPages().then(x => {
            this.startPages = x;
        });
    }

    private loadCompanyRoles(): ng.IPromise<any> {
        return this.coreService.getCompanyRolesDict(true, false).then(x => {
            this.companyRoles = x;
        });
    }

    private loadRole(): ng.IPromise<any> {
        return this.roleService.getRole(this.roleId).then(x => {
            this.isNew = false;
            this.role = x;
            if (this.role.state == SoeEntityState.Active)
                this.role.active = true;
            else
                this.role.active = false;
            this.messagingHandler.publishSetTabLabel(this.guid, this.role.name);
        });
    }

    private new() {
        this.isNew = true;
        this.roleId = 0;
        this.role = new RoleEditDTO();
        this.role.active = true;

        if (this.companyRoles.length === 0)
            this.loadCompanyRoles();
    }

    // ACTIONS

    protected copy() {
        this.role.templateRoleId = this.roleId;
        this.roleId = this.role.roleId = 0;
        this.role.name = '';
        this.role.isAdmin = false;
        this.isNew = true;
        this.loadCompanyRoles();
        this.focusService.focusById("ctrl_role_name");
    }

    private showReadPermissions() {
        let url = `/soe/manage/roles/edit/permission/?license=${this.selectedLicenseId}&licenseNr=${this.selectedLicenseNr}&company=${this.selectedCompanyId}&role=${this.roleId}&permission=${Permission.Readonly}`;
        HtmlUtility.openInNewTab(this.$window, url);
    }

    private showModifyPermissions() {
        let url = `/soe/manage/roles/edit/permission/?license=${this.selectedLicenseId}&licenseNr=${this.selectedLicenseNr}&company=${this.selectedCompanyId}&role=${this.roleId}&permission=${Permission.Modify}`;
        HtmlUtility.openInNewTab(this.$window, url);
    }

    private showUsers() {
        let url = `/soe/manage/users/?license=${this.selectedLicenseId}&licenseNr=${this.selectedLicenseNr}&company=${this.selectedCompanyId}&role=${this.roleId}`;
        HtmlUtility.openInNewTab(this.$window, url);
    }

    private save() {
        if (this.role.active === true)
            this.role.state = SoeEntityState.Active;
        else
            this.role.state = SoeEntityState.Inactive;

        var savingFlag = false;
        if (!this.role.active) {
            this.roleService.getRoleHasUsers(this.roleId).then(result => {
                if (result.errorMessage !== undefined) {
                    this.notificationService.showDialog("", result.errorMessage, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
                }
                else {
                    this.progress.startSaveProgress((completion) => {
                        this.roleService.saveRole(this.role).then(result => {
                            if (result.success) {
                                this.roleId = result.integerValue;
                                this.role.roleId = this.roleId;

                                this.showConfigureInfo = this.isNew;
                                this.role.active = this.isNew ? true : this.role.active;
                                completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.role);
                                this.dirtyHandler.clean();
                            } else {
                                completion.failed(result.errorMessage);
                            }
                        }, error => {
                            completion.failed(error.message);
                        });
                    }, this.guid).then(data => {
                        this.onLoadData();
                    }, error => {
                    });
                }


            });
        }
        else
            savingFlag = true;

        if (savingFlag) {
            this.progress.startSaveProgress((completion) => {
                this.roleService.saveRole(this.role).then(result => {
                    if (result.success) {
                        this.roleId = result.integerValue;
                        this.role.roleId = this.roleId;

                        this.showConfigureInfo = this.isNew;
                        this.role.active = this.isNew ? true : this.role.active;
                        completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.role);
                        this.dirtyHandler.clean();
                    } else {
                        completion.failed(result.errorMessage);
                    }
                }, error => {
                    completion.failed(error.message);
                });
            }, this.guid).then(data => {
                this.onLoadData();
            }, error => {
            });
        }
    }

    private delete() {
        if (this.role.active) {
            this.roleService.getRoleHasUsers(this.roleId).then(result => {
                if (result.errorMessage !== undefined) {
                    this.notificationService.showDialog("", result.errorMessage, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
                }
                else {
                    this.progress.startDeleteProgress((completion) => {
                        this.roleService.deleteRole(this.roleId).then(result => {
                            if (result.success) {
                                completion.completed(this.role, true);
                            } else {
                                completion.failed(result.errorMessage);
                            }
                        }, error => {
                            completion.failed(error.message);
                        });
                    }).then(x => {
                        super.closeMe(true);
                    });
                }
            })
        }
        else {
            this.progress.startDeleteProgress((completion) => {
                this.roleService.deleteRole(this.roleId).then(result => {
                    if (result.success) {
                        completion.completed(this.role, true);
                    } else {
                        completion.failed(result.errorMessage);
                    }
                }, error => {
                    completion.failed(error.message);
                });
            }).then(x => {
                super.closeMe(true);
            });
        }
    }

    // HELP-METHODS

    private setDirty() {
        this.dirtyHandler.setDirty();
    }

    // VALIDATION

    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.role) {
                if (!this.role.name)
                    mandatoryFieldKeys.push("common.name");
            }
        });
    }
}