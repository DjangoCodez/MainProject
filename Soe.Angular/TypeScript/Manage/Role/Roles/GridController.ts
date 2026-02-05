import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { Feature, Permission } from "../../../Util/CommonEnumerations";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { HtmlUtility } from "../../../Util/HtmlUtility";
import { IRoleService } from "../RoleService";
import { RoleGridDTO } from "../../../Common/Models/RoleDTOs";
import { ISelectedItemsService } from "../../../Core/Services/SelectedItemsService";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    // Terms:
    private terms: any;

    // soeConfig parameters
    private selectedLicenseId: number = 0;
    private selectedLicenseNr: number = 0;
    private selectedCompanyId: number = 0;
    private isAuthorizedForEdit: boolean = false;

    // Permissions
    private editPermissionsPermission: boolean = false;
    private usersPermission: boolean = false;

    //@ngInject
    constructor(
        $scope: ng.IScope,
        private roleService: IRoleService,
        private translationService: ITranslationService,
        private $window,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        private selectedItemsService: ISelectedItemsService,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {
        super(gridHandlerFactory, "Manage.Role.Roles", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onBeforeSetUpGrid(() => this.loadTerms())
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData());
    }

    // SETUP

    public onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => {
                this.reloadData();
            });
        }

        this.selectedLicenseId = soeConfig.selectedLicenseId || 0;
        this.selectedLicenseNr = soeConfig.selectedLicenseNr || 0;
        this.selectedCompanyId = soeConfig.selectedCompanyId || 0;
        this.isAuthorizedForEdit = soeConfig.isAuthorizedForEdit && (<string>soeConfig.isAuthorizedForEdit).toLowerCase() == 'true';

        this.flowHandler.start([
            { feature: Feature.Manage_Roles, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Manage_Roles_Edit_Permission, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Manage_Users, loadReadPermissions: true, loadModifyPermissions: true },
        ]);
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Manage_Roles].readPermission;
        this.modifyPermission = response[Feature.Manage_Roles].modifyPermission;
        this.editPermissionsPermission = response[Feature.Manage_Roles_Edit_Permission].modifyPermission;
        this.usersPermission = response[Feature.Manage_Users].modifyPermission;

        if (this.modifyPermission)
            this.messagingHandler.publishActivateAddTab();
    }

    public setupGrid() {
        this.gridAg.options.enableRowSelection = false;
        this.gridAg.addColumnActive("isActive", this.terms["common.active"], null, (params) => this.selectedItemsService.CellChanged(params));
        this.gridAg.addColumnText("name", this.terms["common.name"], null);
        this.gridAg.addColumnText("externalCodesString", this.terms["common.externalcode"], 50);
        this.gridAg.addColumnNumber("sort", this.terms["common.level"], 40);
        if (this.editPermissionsPermission && this.isAuthorizedForEdit) {
            this.gridAg.addColumnIcon(null, "", 40, { icon: "fal fa-user-lock", onClick: this.showReadPermissions.bind(this), toolTip: this.terms["manage.role.role.readpermission"], suppressFilter:true })
            this.gridAg.addColumnIcon(null, "", 40, { icon: "fal fa-user-edit", onClick: this.showModifyPermissions.bind(this), toolTip: this.terms["manage.role.role.modifypermission"], suppressFilter: true })
        }
        if (this.usersPermission)
            this.gridAg.addColumnIcon(null, "", 40, { icon: "fal fa-user-friends", onClick: this.showUsers.bind(this), toolTip: this.terms["manage.role.role.showusers"], suppressFilter: true })
        if (this.isAuthorizedForEdit)
            this.gridAg.addColumnEdit(this.terms["core.edit"], this.edit.bind(this));

        this.gridAg.finalizeInitGrid("manage.role.role.roles", true);
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.reloadData());
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "common.active",
            "core.edit",
            "common.name",
            "common.externalcode",
            "common.level",
            "manage.role.role.readpermission",
            "manage.role.role.modifypermission",
            "manage.role.role.showusers"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    public loadGridData() {
        this.progress.startLoadingProgress([() => {
            return this.roleService.getAllRoles(this.selectedCompanyId).then(x => {
                return x;
            }).then(data => {
                this.setData(data);
            });
        }]);
    }

    private reloadData() {
        this.loadGridData();
    }
   
    // EVENTS

    private showReadPermissions(row: RoleGridDTO) {
        let url = `/soe/manage/roles/edit/permission/?license=${this.selectedLicenseId}&licenseNr=${this.selectedLicenseNr}&company=${this.selectedCompanyId}&role=${row.roleId}&permission=${Permission.Readonly}`;
        HtmlUtility.openInNewTab(this.$window, url);
    }

    private showModifyPermissions(row: RoleGridDTO) {
        let url = `/soe/manage/roles/edit/permission/?license=${this.selectedLicenseId}&licenseNr=${this.selectedLicenseNr}&company=${this.selectedCompanyId}&role=${row.roleId}&permission=${Permission.Modify}`;
        HtmlUtility.openInNewTab(this.$window, url);
    }

    private showUsers(row: RoleGridDTO) {
        let url = `/soe/manage/users/?license=${this.selectedLicenseId}&licenseNr=${this.selectedLicenseNr}&company=${this.selectedCompanyId}&role=${row.roleId}`;
        HtmlUtility.openInNewTab(this.$window, url);
    }

    public edit(row: RoleGridDTO) {
        // Send message to TabsController
        if (this.readPermission || this.modifyPermission)
            this.messagingHandler.publishEditRow(row);
    }
}