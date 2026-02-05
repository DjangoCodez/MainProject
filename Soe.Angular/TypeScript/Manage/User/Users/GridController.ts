import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IUserService } from "../UserService";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { Feature, LicenseSettingType, SettingMainType, SoeEntityState, UserSettingType } from "../../../Util/CommonEnumerations";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { UserGridDTO } from "../../../Common/Models/UserDTO";
import { CoreUtility } from "../../../Util/CoreUtility";
import { Constants } from "../../../Util/Constants";
import { SoeGridOptionsEvent, IconLibrary, SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../Util/Enumerations";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { ToolBarUtility, ToolBarButton } from "../../../Util/ToolBarUtility";
import { HtmlUtility } from "../../../Util/HtmlUtility";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { CalendarUtility } from "../../../Util/CalendarUtility";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    // Terms:
    private terms: any;

    // soeConfig parameters
    private selectedLicenseId: number = 0;
    private selectedCompanyId: number = 0;
    private selectedRoleId: number = 0;
    private selectedUserId: number = 0;
    private hasValidLicenseToSupportLogin: boolean = false;

    // Data
    private supportAllowedCompanyIds: number[] = [];

    // Toolbar
    private toolbarInclude: any;

    // Footer
    private nrOfUsersOnLicense: number = 0;
    private maxNrOfUsersOnLicense: number = 0;

    // Flags
    private showInactive: boolean = false;
    private showEnded: boolean = false;
    private showNotStarted = false;
    private selectedCount: number = 0;
    private hasLicenseSettingsPermission = false;
    private showSso = false;

    //@ngInject
    constructor(
        private coreService: ICoreService,
        private userService: IUserService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private $filter: ng.IFilterService,
        private $timeout: ng.ITimeoutService,
        private $q: ng.IQService,
        private $window,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        urlHelperService: IUrlHelperService) {
        super(gridHandlerFactory, "Manage.User.Users", progressHandlerFactory, messagingHandlerFactory);

        this.toolbarInclude = urlHelperService.getViewUrl("gridHeader.html");

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onBeforeSetUpGrid(() => this.loadLicenseSettings())
            .onBeforeSetUpGrid(() => this.loadUserSettings())
            .onBeforeSetUpGrid(() => this.loadTerms())
            .onBeforeSetUpGrid(() => this.loadSupportLoginCompanies())
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
        this.selectedCompanyId = soeConfig.selectedCompanyId || 0;
        this.selectedRoleId = soeConfig.selectedRoleId || 0;
        this.selectedUserId = soeConfig.selectedUserId || 0;
        this.hasValidLicenseToSupportLogin = soeConfig.hasValidLicenseToSupportLogin || false;

        this.flowHandler.start([
            { feature: Feature.Manage_Users, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Manage_Preferences_LicenseSettings, loadReadPermissions: false, loadModifyPermissions: true },
        ]);
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Manage_Users].readPermission;
        this.modifyPermission = response[Feature.Manage_Users].modifyPermission;

        if (this.modifyPermission)
            this.messagingHandler.publishActivateAddTab();

        this.hasLicenseSettingsPermission = response[Feature.Manage_Preferences_LicenseSettings].modifyPermission;
    }

    public setupGrid() {
        this.gridAg.addColumnBool("isActive", this.terms["common.active"], 40, false, null, null, true);
        var colDefNr = this.gridAg.addColumnText("loginName", this.terms["manage.user.user.loginname"], 100);
        colDefNr.comparator = (valueA: string, valueB: string, nodeA: any, nodeB: any, isInverted: boolean) => {
            return valueA.padLeft(50, '0').toLowerCase().localeCompare(valueB.padLeft(50, '0').toLowerCase());
        };
        this.gridAg.addColumnText("name", this.terms["common.name"], null);
        this.gridAg.addColumnText("defaultRoleName", this.terms["manage.user.user.defaultrole"], null, true);
        var colDefEmail = this.gridAg.addColumnText("email", this.terms["common.contactaddresses.ecommenu.email"], null, true);
        colDefEmail.checkboxSelection = true;
        colDefEmail.headerCheckboxSelection = true;
        colDefEmail.headerCheckboxSelectionFilteredOnly = true;

        this.gridAg.addColumnShape("idLoginActiveColor", null, null, { shape: Constants.SHAPE_CIRCLE, toolTipField: "idLoginActiveTooltip", showIconField: "idLoginActiveColor" });
        if (this.showSso)
            this.gridAg.addColumnText("externalAuthId", this.terms["common.user.externalauthid"], null, true);
        else
            this.gridAg.addColumnText("softOneIdLoginName", this.terms["common.user.softoneidloginname"], null, true);
        var colDefEdit = this.gridAg.addColumnEdit(this.terms["core.edit"], this.edit.bind(this), false, "showEditIcon");
        colDefEdit.pinned = 'undefined'

        if (this.hasValidLicenseToSupportLogin)
            this.gridAg.addColumnIcon(null, "", null, { icon: 'fal fa-life-ring errorColor', onClick: this.supportLogin.bind(this), showIcon: "allowSupportLogin", toolTip: this.terms["manage.user.user.supportlogin"] });

        this.gridAg.addStandardMenuItems();
        this.gridAg.setExporterFilenamesAndHeader("manage.user.user.users");

        var events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.IsRowSelectable, (rowNode) => {
            // Only users with email address can be selected
            return rowNode.data && rowNode.data.email;
        }));
        events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (rowNode) => {
            this.$timeout(() => {
                this.selectedCount = this.gridAg.options.getSelectedCount();
            });
        }));
        this.gridAg.options.subscribe(events);

        this.addTotalRow();
        this.gridAg.options.finalizeInitGrid(undefined, true);
        this.gridAg.options.hideColumn('soe-row-selection');
    }

    private showEditIcon(): boolean {
        var show = !this.hasSelectedParams() || this.selectedUserId > 0 && this.selectedUserId == soeConfig.userId || this.selectedCompanyId > 0 && this.selectedCompanyId == soeConfig.actorCompanyId;
        this.doubleClickToEdit = show;
        return show;
    }

    private hasSelectedParams() {
        return this.selectedLicenseId > 0 || this.selectedCompanyId > 0 || this.selectedRoleId > 0 || this.selectedUserId > 0;
    }

    private addTotalRow() {
        this.gridAg.options.addTotalRow("#totals-grid", {
            prefixText: this.terms["manage.user.user.gridlicenseinfo"].format(this.nrOfUsersOnLicense.toString(), this.maxNrOfUsersOnLicense.toString()),
            filtered: this.terms["core.aggrid.totals.filtered"],
            total: this.terms["core.aggrid.totals.total"],
            tooltip: this.terms["manage.user.user.gridlicenseinfo.tooltip"]
        });
    }

    private replaceTotalRow() {
        this.gridAg.options.removeTotalRow("#totals-grid");
        this.addTotalRow();
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.reloadData());

        let group = ToolBarUtility.createGroup();
        group.buttons.push(new ToolBarButton("manage.user.user.sendactivationmessage", "manage.user.user.sendactivationmessage.tooltip", IconLibrary.FontAwesome, "fa-envelope",
            () => { this.askSendActivationMessage(); },
            () => { return this.selectedCount === 0 }))
        group.buttons.push(new ToolBarButton("manage.user.user.sendforgottenusername", "manage.user.user.sendforgottenusername.tooltip", IconLibrary.FontAwesome, "fa-envelope",
            () => { this.askSendForgottenUsername(); },
            () => { return this.selectedCount === 0 }))

        this.toolbar.addButtonGroup(group);

        this.toolbar.addInclude(this.toolbarInclude);
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total",
            "core.edit",
            "common.name",
            "common.active",
            "common.contactaddresses.ecommenu.email",
            "manage.user.user.loginname",
            "manage.user.user.defaultrole",
            "manage.user.user.gridlicenseinfo",
            "manage.user.user.gridlicenseinfo.tooltip",
            "manage.user.user.usernotactivated",
            "manage.user.user.useractivated",
            "manage.user.user.supportlogin",
            "common.user.externalauthid",
            "common.user.softoneidloginname"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private loadLicenseSettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];
        settingTypes.push(LicenseSettingType.SSO_Key);

        return this.coreService.getLicenseSettings(settingTypes).then(x => {
            let setting = SettingsUtility.getStringLicenseSetting(x, LicenseSettingType.SSO_Key);
            if (this.hasLicenseSettingsPermission && (setting || setting.length > 2))
                this.showSso = true;
        });
    }

    private loadUserSettings(): ng.IPromise<any> {
        const settingTypes: number[] = [];
        settingTypes.push(UserSettingType.ManageUserGridShowInactive);
        settingTypes.push(UserSettingType.ManageUserGridShowEnded);
        settingTypes.push(UserSettingType.ManageUserGridShowNotStarted);

        return this.coreService.getUserSettings(settingTypes).then(x => {
            this.showInactive = SettingsUtility.getBoolUserSetting(x, UserSettingType.ManageUserGridShowInactive);
            this.showEnded = SettingsUtility.getBoolUserSetting(x, UserSettingType.ManageUserGridShowEnded);
            this.showNotStarted = SettingsUtility.getBoolUserSetting(x, UserSettingType.ManageUserGridShowNotStarted);
        });
    }

    private loadLicenseInfo(): ng.IPromise<any> {
        return this.userService.getUserLicenseInfo().then(x => {
            this.nrOfUsersOnLicense = x.field1;
            this.maxNrOfUsersOnLicense = x.field2;
            this.replaceTotalRow();
        });
    }

    private loadSupportLoginCompanies(): ng.IPromise<any> {
        var deferral = this.$q.defer();

        if (this.hasValidLicenseToSupportLogin) {
            return this.userService.getCompaniesWithSupportLogin(this.selectedLicenseId).then(x => {
                this.supportAllowedCompanyIds = x;
            });
        } else {
            deferral.resolve();
        }

        return deferral.promise;
    }

    public loadGridData() {
        this.loadLicenseInfo();

        this.progress.startLoadingProgress([() => {
            var showEditIcon: boolean = this.showEditIcon();
            return this.load().then(x => {
                _.forEach(x, user => {
                    user.isActive = (user.state == SoeEntityState.Active);
                    if (user.idLoginActive) {
                        user['idLoginActiveColor'] = "#24a148"; // okColor
                        user['idLoginActiveTooltip'] = this.terms["manage.user.user.useractivated"];
                    } else {
                        user['idLoginActiveColor'] = "#da1e28"; // errorColor
                        user['idLoginActiveTooltip'] = this.terms["manage.user.user.usernotactivated"];
                    }
                    user['showEditIcon'] = showEditIcon;

                    if (this.hasValidLicenseToSupportLogin) {
                        let allowSupportLogin: boolean = false;
                        if (user.defaultActorCompanyId) {
                            if (_.includes(this.supportAllowedCompanyIds, user.defaultActorCompanyId))
                                allowSupportLogin = true;
                        }
                        user['allowSupportLogin'] = allowSupportLogin;
                    }
                });

                return x;
            }).then(data => {
                this.setData(data);
            });
        }]);
    }

    private load(): ng.IPromise<UserGridDTO[]> {
        if (this.selectedUserId)
            return this.userService.getUsers(soeConfig.selectedUserId, this.showInactive);
        else if (this.selectedRoleId)
            return this.userService.getUsersByRole(soeConfig.selectedRoleId, true, this.showInactive, this.showEnded, this.showNotStarted);
        else if (this.selectedCompanyId)
            return this.userService.getUsersByCompany(soeConfig.selectedCompanyId, true, this.showInactive, this.showEnded, this.showNotStarted, this.showEnded ? CalendarUtility.DefaultDateTime() : null);
        else if (this.selectedLicenseId)
            return this.userService.getUsersByLicense(soeConfig.selectedLicenseId, true, this.showInactive, this.showEnded, this.showNotStarted);
        else
            return this.userService.getUsersByCompany(CoreUtility.actorCompanyId, true, this.showInactive, this.showEnded, this.showNotStarted, this.showEnded ?CalendarUtility.DefaultDateTime() : null);
    }

    private reloadData() {
        this.loadGridData();
    }

    // EVENTS

    private showInactiveChanged() {
        this.$timeout(() => {
            this.coreService.saveBoolSetting(SettingMainType.User, UserSettingType.ManageUserGridShowInactive, this.showInactive);
            this.reloadData();
        });
    }

    private showEndedChanged() {
        this.$timeout(() => {
            this.coreService.saveBoolSetting(SettingMainType.User, UserSettingType.ManageUserGridShowEnded, this.showEnded);
            this.reloadData();
        });
    }

    private showNotStartedChanged() {
        this.$timeout(() => {
            this.coreService.saveBoolSetting(SettingMainType.User, UserSettingType.ManageUserGridShowNotStarted, this.showNotStarted);
            this.reloadData();
        });
    }

    public edit(row: UserGridDTO) {
        // Send message to TabsController
        if (this.readPermission || this.modifyPermission)
            this.messagingHandler.publishEditRow(row);
    }

    private supportLogin(row: UserGridDTO) {
        var url: string = "/soe/manage/companies/edit/remote/?user=" + row.userId + "&login=1";
        HtmlUtility.openInSameTab(this.$window, url);
    }

    private askSendActivationMessage() {
        let selectedUsers: UserGridDTO[] = this.gridAg.options.getSelectedRows();
        if (selectedUsers.length === 0)
            return;

        let activeCount: number = _.filter(selectedUsers, u => u.idLoginActive).length;

        // will able to force one at a time from grid.
        if (activeCount == 1) {
            activeCount = 0;
        }


        var keys: string[] = ["manage.user.user.sendactivationmessage"];
        keys.push(activeCount > 0 ? "manage.user.user.sendactivationmessage.alreadyactive" : "manage.user.user.sendactivationmessage.message");
        this.translationService.translateMany(keys).then(terms => {
            if (activeCount > 0) {
                let activeUsers: UserGridDTO[] = _.filter(selectedUsers, u => u.idLoginActive);
                let activeUsersString: string = '';
                _.forEach(activeUsers, user => {
                    if (activeUsersString.length > 0)
                        activeUsersString += '\n';
                    activeUsersString += '{0} {1}'.format(user.loginName, user.name);
                });
                this.notificationService.showDialogEx(terms["manage.user.user.sendactivationmessage"], terms["manage.user.user.sendactivationmessage.alreadyactive"].format(activeUsersString), SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK);
            } else {
                var modal = this.notificationService.showDialogEx(terms["manage.user.user.sendactivationmessage"], terms["manage.user.user.sendactivationmessage.message"].format(selectedUsers.length.toString()), SOEMessageBoxImage.Question, SOEMessageBoxButtons.OKCancel);
                modal.result.then(val => {
                    this.sendActivationMessage();
                }, (reason) => {
                    // User cancelled
                });
            }
        });
    }

    private sendActivationMessage() {
        this.userService.sendActivationEmail(this.gridAg.options.getSelectedIds("userId")).then(result => {
            var keys: string[] = [
                "manage.user.user.sendactivationmessage.status"
            ];

            keys.push(result.success ? "manage.user.user.sendactivationmessage.success.message" : "manage.user.user.sendactivationmessage.error.message");

            this.translationService.translateMany(keys).then(terms => {
                var message: string;
                if (result.success) {
                    message = terms["manage.user.user.sendactivationmessage.success.message"];
                } else {
                    if (result.errorMessage) {
                        message = result.errorMessage;
                    } else if (result.strings) {
                        message = terms["manage.user.user.sendactivationmessage.error.message"];
                        _.forEach(result.strings, email => {
                            message += "\n" + email
                        });
                    }
                }

                this.notificationService.showDialogEx(terms["manage.user.user.sendactivationmessage.status"], message, result.success ? SOEMessageBoxImage.Information : SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
            });
        });
    }

    private askSendForgottenUsername() {
        var count: number = this.gridAg.options.getSelectedCount();
        if (count === 0)
            return;

        var keys: string[] = [
            "manage.user.user.sendforgottenusername",
            "manage.user.user.sendforgottenusername.message"
        ];

        this.translationService.translateMany(keys).then(terms => {
            var message = terms["manage.user.user.sendforgottenusername.message"].format(count.toString());

            var modal = this.notificationService.showDialogEx(terms["manage.user.user.sendforgottenusername"], message, SOEMessageBoxImage.Question, SOEMessageBoxButtons.OKCancel);
            modal.result.then(val => {
                this.sendForgottenUsername();
            }, (reason) => {
                // User cancelled
            });
        });
    }

    private sendForgottenUsername() {
        this.userService.sendForgottenUsername(this.gridAg.options.getSelectedIds("userId")).then(result => {
            var keys: string[] = [
                "manage.user.user.sendforgottenusername.status"
            ];

            keys.push(result.success ? "manage.user.user.sendforgottenusername.success.message" : "manage.user.user.sendforgottenusername.error.message");

            this.translationService.translateMany(keys).then(terms => {
                var message: string;
                if (result.success) {
                    message = terms["manage.user.user.sendforgottenusername.success.message"];
                } else {
                    if (result.errorMessage) {
                        message = result.errorMessage;
                    } else if (result.strings) {
                        message = terms["manage.user.user.sendforgottenusername.error.message"];
                        _.forEach(result.strings, email => {
                            message += "\n" + email
                        });
                    }
                }

                this.notificationService.showDialogEx(terms["manage.user.user.sendforgottenusername.status"], message, result.success ? SOEMessageBoxImage.Information : SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
            });
        });
    }
}