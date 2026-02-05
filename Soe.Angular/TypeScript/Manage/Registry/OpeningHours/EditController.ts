import { IMessagingHandler } from "../../../Core/Handlers/MessagingHandler";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IEditControllerFlowHandler, IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IDirtyHandler } from "../../../Core/Handlers/DirtyHandler";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IProgressHandler } from "../../../Core/Handlers/ProgressHandler";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IToolbar } from "../../../Core/Handlers/Toolbar";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IValidationSummaryHandler } from "../../../Core/Handlers/ValidationSummaryHandler";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { IRegistryService } from "../RegistryService";
import { ICoreService } from "../../../Core/Services/CoreService";
import { CoreUtility } from "../../../Util/CoreUtility";
import { Guid } from "../../../Util/StringUtility";
import { CompanySettingType, Feature, TermGroup } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { OpeningHoursDTO } from "../../../Common/Models/OpeningHoursDTO";
import { SmallGenericType } from "../../../Common/Models/SmallGenericType";
import { IFocusService } from "../../../Core/Services/focusservice";
import { AccountDTO } from "../../../Common/Models/AccountDTO";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { CalendarUtility } from "../../../Util/CalendarUtility";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    //Company Settings
    private useAccountsHierarchy: boolean;
    companySettingMinLength: number = 15;

    private openingHoursId: number;
    private openingHours: OpeningHoursDTO;

    private accounts: AccountDTO[];

    private days: SmallGenericType[] = [];

    private _selectedAccount: AccountDTO;
    
    public get selectedAccount(): AccountDTO {
        return this._selectedAccount;
    }
    public set selectedAccount(account: AccountDTO) {
        this._selectedAccount = account;
        if (account) {
            this.openingHours.accountId = account.accountId;
            this.openingHours.accountName = account.name;
        }
    }

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private $timeout: ng.ITimeoutService,
        private registryService: IRegistryService,
        private coreService: ICoreService,
        private focusService: IFocusService,
        urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        private controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onLoadData(() => this.onLoadData())
            .onDoLookUp(() => this.onDoLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    // SETUP

    public onInit(parameters: any) {
        this.openingHoursId = parameters.id;
        this.guid = parameters.guid;

        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: Feature.Manage_Preferences_Registry_OpeningHours, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Manage_Preferences_Registry_OpeningHours].readPermission;
        this.modifyPermission = response[Feature.Manage_Preferences_Registry_OpeningHours].modifyPermission;
    }

    private onDoLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.loadCompanySettings(),
            this.loadDays(),
            this.loadAccountStringIdsByUserFromHierarchy()
        ]).then(() => {
        });
    }

    private onLoadData(): ng.IPromise<any> {
        if (this.openingHoursId) {
            return this.loadData();
        } else {
            this.new();
        }
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => { this.copy() }, () => this.isNew);
    }

    // SERVICE CALLS

    private loadDays(): ng.IPromise<any> {
        this.days = [];
        return this.coreService.getTermGroupContent(TermGroup.StandardDayOfWeek, true, true).then((x) => {
            this.days = x;
        });
    }

    private loadData() {
        return this.registryService.getOpeningHour(this.openingHoursId).then(x => {
            this.openingHours = x;
            this.isNew = false;
            this.selectedAccount = _.find(this.accounts, a => a.accountId == this.openingHours.accountId);

            if (this.openingHours.accountId > 0) {
                // Insert empty
                var account: AccountDTO = new AccountDTO();
                account.accountId = 0;
                account.name = "";
                this.accounts.splice(0, 0, account);
            }
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];

        settingTypes.push(CompanySettingType.TimeSchedulePlanningDayViewMinorTickLength);
        settingTypes.push(CompanySettingType.UseAccountHierarchy);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.useAccountsHierarchy = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseAccountHierarchy);
            this.companySettingMinLength = SettingsUtility.getIntCompanySetting(x, CompanySettingType.TimeSchedulePlanningDayViewMinorTickLength);
            if (this.companySettingMinLength == 0)
                this.companySettingMinLength = 15;
        });
    }

    private loadAccountStringIdsByUserFromHierarchy(): ng.IPromise<any> {
        this.accounts = [];
        return this.coreService.getAccountsFromHierarchyByUserSetting(CalendarUtility.getDateNow(), CalendarUtility.getDateNow(), true, false, false, true).then(x => {
            this.accounts = x;
        });
    }

    // ACTIONS

    private new() {
        this.isNew = true;
        this.openingHoursId = 0;
        this.openingHours = new OpeningHoursDTO();
        this.openingHours.standardWeekDay = 0;
        this.openingHours.accountId = 0;
        this.openingHours.accountName = "";

        this.focusService.focusById("ctrl_openingHours_name");
    }

    public save() {
        this.progress.startSaveProgress((completion) => {
            this.openingHours.actorCompanyId = CoreUtility.actorCompanyId;
            this.registryService.saveOpeningHours(this.openingHours).then((result) => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0)
                        this.openingHoursId = result.integerValue;

                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.openingHours);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid)
            .then(data => {
                this.dirtyHandler.clean();
                this.loadData();
            }, error => {

            });
    }

    public delete() {
        this.progress.startDeleteProgress((completion) => {
            this.registryService.deleteOpeningHours(this.openingHours.openingHoursId).then((result) => {
                if (result.success) {
                    completion.completed(this.openingHours, true);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }).then(x => {
            this.closeMe(false);
        });
    }

    protected copy() {
        super.copy();

        this.openingHours.openingHoursId = this.openingHoursId = 0;
        this.openingHours.name = undefined;

        this.dirtyHandler.isDirty = true;
        this.focusService.focusById("ctrl_openingHours_name");
    }

    // EVENTS

    private standardWeekDayChanged() {
        this.$timeout(() => {
            // If day is selected, clear the specific date
            if (this.openingHours.standardWeekDay !== 0)
                this.openingHours.specificDate = null;
        });
    }

    // VALIDATION

    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.openingHours) {
                if (!this.openingHours.name)
                    mandatoryFieldKeys.push("common.name");
                if (!this.openingHours.openingTime)
                    mandatoryFieldKeys.push("manage.registry.openinghours.openingtime");
                if (!this.openingHours.closingTime)
                    mandatoryFieldKeys.push("manage.registry.openinghours.closingtime");
            }
        });
    }
}