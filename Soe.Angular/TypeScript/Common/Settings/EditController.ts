import { EditControllerBase2 } from "../../Core/Controllers/EditControllerBase2";
import { ICompositionEditController } from "../../Core/ICompositionEditController";
import { ICoreService } from "../../Core/Services/CoreService";
import { IUrlHelperService } from "../../Core/Services/UrlHelperService";
import { IProgressHandlerFactory } from "../../Core/Handlers/progresshandlerfactory";
import { IControllerFlowHandlerFactory } from "../../Core/Handlers/controllerflowhandlerfactory";
import { IValidationSummaryHandlerFactory } from "../../Core/Handlers/validationsummaryhandlerfactory";
import { IMessagingHandlerFactory } from "../../Core/Handlers/messaginghandlerfactory";
import { IDirtyHandlerFactory } from "../../Core/Handlers/DirtyHandlerFactory";
import { IPermissionRetrievalResponse } from "../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../Core/Handlers/ToolbarFactory";
import { UserCompanySettingEditDTO } from "../Models/UserCompanySettingsDTOs";
import { Feature, SettingMainType, SoeEntityType, TermGroup_TrackChangesAction } from "../../Util/CommonEnumerations";
import { ITranslationService } from "../../Core/Services/TranslationService";
import { TrackChangesDTO } from "../Models/TrackChangesDTO";
import { SOEMessageBoxImage, SOEMessageBoxButtons, SOEMessageBoxSize } from "../../Util/Enumerations";
import { INotificationService } from "../../Core/Services/NotificationService";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Init parameters
    private settingMainType: SettingMainType;
    private feature: Feature;

    // Terms
    private terms: { [index: string]: string; };

    // Settings
    private settings: UserCompanySettingEditDTO[];

    private _selectedSetting: UserCompanySettingEditDTO;
    private get selectedSetting(): UserCompanySettingEditDTO {
        return this._selectedSetting;
    }
    private set selectedSetting(setting: UserCompanySettingEditDTO) {
        this._selectedSetting = setting;

        _.filter(this.settings, s => s.settingTypeId !== setting.settingTypeId).forEach(s => s.isEditing = false);
    }

    // Filter
    private filterAll: string;
    private filterGroupLevel1: string;
    private filterGroupLevel2: string;
    private filterGroupLevel3: string;
    private filterName: string;

    private get footerInfo(): string {
        if (!this.terms || !this.settings)
            return '';

        let total = this.settings.length;
        let filtered = this.filteredSettings.length;

        if (total === filtered)
            return "{0} {1}".format(this.terms["core.aggrid.totals.total"], total.toString());
        else
            return "{0} {1} ({2} {3})".format(this.terms["core.aggrid.totals.total"], total.toString(), this.terms["core.aggrid.totals.filtered"], filtered.toString());
    }

    private get filteredSettings(): UserCompanySettingEditDTO[] {
        return _.filter(this.settings, s =>
            (!this.filterAll || s.groupLevel1.contains(this.filterAll) || s.groupLevel2.contains(this.filterAll) || s.groupLevel3.contains(this.filterAll) || s.name.contains(this.filterAll)) &&
            (!this.filterGroupLevel1 || s.groupLevel1.contains(this.filterGroupLevel1)) &&
            (!this.filterGroupLevel2 || s.groupLevel2.contains(this.filterGroupLevel2)) &&
            (!this.filterGroupLevel3 || s.groupLevel3.contains(this.filterGroupLevel3)) &&
            (!this.filterName || s.name.contains(this.filterName)));
    }

    //@ngInject
    constructor(
        private $timeout: ng.ITimeoutService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private coreService: ICoreService,
        urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        private controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.settingMainType = soeConfig.settingMainType;
        this.feature = soeConfig.feature;

        this.saveButtonTemplateUrl = urlHelperService.getCoreComponent("saveButtonComposition.html");

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onDoLookUp(() => this.loadTerms())
            .onLoadData(() => this.loadSettings());
    }

    // SETUP

    public onInit(parameters: any) {
        this.guid = parameters.guid;

        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: this.feature, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[this.feature].readPermission;
        this.modifyPermission = response[this.feature].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        //this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => { /*this.copy()*/ }, () => this.isNew);
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        let keys: string[] = [
            "core.aggrid.totals.total",
            "core.aggrid.totals.filtered",
            "common.from",
            "common.history",
            "common.by",
            "common.to",
            "common.usercompanysetting.change.insert",
            "common.usercompanysetting.change.update"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private loadSettings(): ng.IPromise<any> {
        return this.coreService.getLicenseSettingsForEdit().then(x => {
            this.settings = x;

            //let s1 = new UserCompanySettingEditDTO();
            //s1.settingType = LicenseSettingType.SSO_Key + 1;
            //s1.name = "Test datum"
            //s1.dataType = SettingDataType.Date;
            //this.settings.push(s1)

            //let s2 = new UserCompanySettingEditDTO();
            //s2.settingType = LicenseSettingType.SSO_Key + 2;
            //s2.name = "Test int"
            //s2.dataType = SettingDataType.Integer;
            //this.settings.push(s2)

            //let s3 = new UserCompanySettingEditDTO();
            //s3.settingType = LicenseSettingType.SSO_Key + 3;
            //s3.name = "Test decimal"
            //s3.dataType = SettingDataType.Decimal;
            //this.settings.push(s3)

            //let s4 = new UserCompanySettingEditDTO();
            //s4.settingType = LicenseSettingType.SSO_Key + 4;
            //s4.name = "Test tid"
            //s4.dataType = SettingDataType.Time;
            //this.settings.push(s4)

            //let s5 = new UserCompanySettingEditDTO();
            //s5.settingType = LicenseSettingType.SSO_Key + 4;
            //s5.name = "Test dropdown"
            //s5.dataType = SettingDataType.Integer;
            //let options: SmallGenericType[] = [];
            //options.push(new SmallGenericType(1, "Värde 1"));
            //options.push(new SmallGenericType(2, "Värde 2"));
            //options.push(new SmallGenericType(3, "Värde 3"));
            //s5.options = options;
            //this.settings.push(s5)
        });
    }

    private loadTrackChanges(setting: UserCompanySettingEditDTO) {
        if (!setting)
            return;

        this.coreService.getTrackChanges(this.getEntity(), setting.userCompanySettingId, false).then((changes: TrackChangesDTO[]) => {
            var msg: string = '';
            _.forEach(changes, change => {
                switch (change.action) {
                    case TermGroup_TrackChangesAction.Insert:
                        msg += this.terms["common.usercompanysetting.change.insert"];
                        if (change.created)
                            msg += ' {0}'.format(change.created.toFormattedDateTime());
                        if (change.createdBy)
                            msg += ' {0} {1}'.format(this.terms["common.by"], change.createdBy);
                        msg += '\n';
                        break;
                    case TermGroup_TrackChangesAction.Update:
                        msg += this.terms["common.usercompanysetting.change.update"];
                        if (change.created)
                            msg += ' {0}'.format(change.created.toFormattedDateTime());
                        if (change.createdBy)
                            msg += ' {0} {1}'.format(this.terms["common.by"], change.createdBy);
                        msg += ', ';
                        msg += '{0} {1} {2} {3}'.format(this.terms["common.from"].toLocaleLowerCase(), change.fromValue, this.terms["common.to"].toLocaleLowerCase(), change.toValue);
                        msg += '\n';
                        break;
                }
            });

            this.notificationService.showDialogEx(this.terms["common.history"], msg, SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK, { size: SOEMessageBoxSize.Large });
        });
    }

    // EVENTS

    private valueChanged(setting: UserCompanySettingEditDTO) {
        this.$timeout(() => {
            setting.isModified = true;
            setting.isEditing = false;
        });
    }

    private showHistory(setting: UserCompanySettingEditDTO) {
        this.loadTrackChanges(setting);
    }

    // ACTIONS

    private save() {
        this.progress.startSaveProgress((completion) => {
            this.coreService.saveUserCompanySettings(_.filter(this.settings, s => s.isModified)).then(result => {
                if (result.success) {
                    completion.completed();
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid)
            .then(data => {
                this.dirtyHandler.clean();
                this.loadSettings();
            }, error => {

            });
    }

    // HELP-METHODS

    private setModified() {
        this.dirtyHandler.setDirty();
    }

    private getEntity(): SoeEntityType {
        switch (this.settingMainType) {
            case SettingMainType.Application:
                return SoeEntityType.UserCompanySetting_Application;
            case SettingMainType.License:
                return SoeEntityType.UserCompanySetting_License;
            case SettingMainType.Company:
                return SoeEntityType.UserCompanySetting_Company;
            case SettingMainType.UserAndCompany:
                return SoeEntityType.UserCompanySetting_UserAndCompany;
            case SettingMainType.User:
                return SoeEntityType.UserCompanySetting_User;
        }
    }
}