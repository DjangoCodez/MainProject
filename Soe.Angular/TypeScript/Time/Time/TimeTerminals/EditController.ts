import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { TimeTerminalDTO, TimeTerminalSettingDTO } from "../../../Common/Models/TimeTerminalDTO";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ITimeService } from "../TimeService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { NotificationService } from "../../../Core/Services/NotificationService";
import { IFocusService } from "../../../Core/Services/FocusService";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { Feature, TermGroup, TimeTerminalType, TimeTerminalSettingType, TimeTerminalSettingDataType, SoeCategoryType, SoeCategoryRecordEntity, TermGroup_Country, CompanySettingType, TermGroup_Languages, TermGroup_GoTimeStampIdentifyType, TermGroup_TimeTerminalAttendanceViewSortOrder, TermGroup_TimeTerminalLogLevel } from "../../../Util/CommonEnumerations";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { ISmallGenericType, IStringKeyValue } from "../../../Scripts/TypeLite.Net4";
import { Constants } from "../../../Util/Constants";
import { AccountDimSmallDTO } from "../../../Common/Models/AccountDimDTO";
import { CoreUtility } from "../../../Util/CoreUtility";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { AccountDTO } from "../../../Common/Models/AccountDTO";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { HtmlUtility } from "../../../Util/HtmlUtility";
import { ToolBarButton, ToolBarUtility } from "../../../Util/ToolBarUtility";
import { IconLibrary, SOEMessageBoxImage } from "../../../Util/Enumerations";
import { Guid } from "../../../Util/StringUtility";
import { IToolbar } from "../../../Core/Handlers/Toolbar";
import { IntKeyValue, SmallGenericType } from "../../../Common/Models/SmallGenericType";

export declare type SettingType = boolean | string | number | Date;

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Terms
    private terms: { [index: string]: string; };
    private secondsLabel: string;
    private minutesLabel: string;
    private hoursLabel: string;
    private selectAccountsWithoutImmediateStampInfoLabel: string;
    private ipFilterInfoLabel: string;
    private terminalGroupNameInfoLabel: string;
    private logLevelInfoLabel: string;

    // Company settings
    private useAccountHierarchy: boolean = false;
    private defaultEmployeeAccountDimId: number = 0;
    private possibilityToRegisterAdditionsInTerminal = false;

    // Lookups
    private types: ISmallGenericType[];
    private identifyTypes: ISmallGenericType[];
    private accountDims: AccountDimSmallDTO[];
    private internalAccounts: AccountDTO[];
    private internalAccounts2: AccountDTO[];
    private breakTimeDeviationCauses: ISmallGenericType[];
    private countries: ISmallGenericType[];
    private languages: ISmallGenericType[];
    private timeZones: IStringKeyValue[];
    private attendanceViewSortOrders: ISmallGenericType[];
    private terminalGroupNames: string[];
    private logLevels: ISmallGenericType[];

    private limitAccountIds: number[] = [];
    private limitAccountIds2: number[] = [];
    private distanceWorkButtonIcons: any[] = [];
    private breakButtonIcons: any[] = [];

    // Data
    private timeTerminalId: number;
    private terminal: TimeTerminalDTO;

    // Defalt values
    private readonly SYNC_INTERVAL_DEFAULT: number = 900;   // 15 minutes
    private readonly SYNC_INTERVAL_MIN: number = 900;       // 15 minutes
    private readonly INACTIVITY_DELAY_DEFAULT: number = 15; // 15 seconds
    private readonly INACTIVITY_DELAY_MIN: number = 3;      // 3 seconds

    // Properties
    private get isSupportAdmin(): boolean {
        return CoreUtility.isSupportAdmin;
    }

    private get goTimeStampUrl(): string {
        return this.terminal && this.terminal.isGoTimeStampType && this.terminal.actorCompanyId && this.terminal.timeTerminalId ? 'https://terminal.softone.se/LogIn?c={0}&t={1}'.format(this.terminal.actorCompanyId.toString(), this.terminal.timeTerminalId.toString()) : '';
    }

    private _selectedGroupName: string;
    private get selectedGroupName(): string {
        return this._selectedGroupName;
    }
    private set selectedGroupName(item: string) {
        this.terminalGroupName = item;
    }

    // Flags
    private expanderStartPageOpen = false;
    private expanderAccountingOpen = false;
    private expanderAdditionsOpen = false;
    private expanderStampingOpen = false;
    private expanderGlobalizationOpen = false;
    private expanderLimitsOpen = false;
    private expanderAttendanceViewOpen = false;
    private expanderGroupingOpen = false;
    private expanderInformationOpen = false;
    private expanderNewEmployeeOpen = false;
    private expanderMiscSettingsOpen = false;

    // Toolbar
    private settingsToolbar: IToolbar;

    //@ngInject
    constructor(
        private $scope: ng.IScope,
        protected $uibModal,
        private $q: ng.IQService,
        private $timeout: ng.ITimeoutService,
        private $window,
        private coreService: ICoreService,
        private timeService: ITimeService,
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

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onLoadData(() => this.onLoadData())
            .onDoLookUp(() => this.onDoLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    public onInit(parameters: any) {
        this.timeTerminalId = parameters.id;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);
        this.navigatorRecords = parameters.navigatorRecords;
        this.flowHandler.start([{ feature: Feature.Time_Preferences_TimeSettings_TimeTerminals_Edit, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Time_Preferences_TimeSettings_TimeTerminals_Edit].readPermission;
        this.modifyPermission = response[Feature.Time_Preferences_TimeSettings_TimeTerminals_Edit].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => this.copy(), () => this.isNew);
        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("time.time.timeterminal.migrate", "time.time.timeterminal.migrate.tooltip", IconLibrary.FontAwesome, "fa-wand-magic", () => {
            this.migrate();
        }, () => { return this.isNew }, () => {
            return this.terminal.isGoTimeStampType;
        })));

        this.toolbar.setupNavigationRecords(this.navigatorRecords, this.timeTerminalId, recordId => {
            if (recordId !== this.timeTerminalId) {
                this.timeTerminalId = recordId;
                this.onLoadData();
            }
        });

        this.settingsToolbar = toolbarFactory.createEmpty();
        let group = ToolBarUtility.createGroup();
        group.buttons.push(new ToolBarButton("core.expandall", "core.expandall", IconLibrary.FontAwesome, "fa-chevron-double-down", () => { this.expandAllSettings(); }));
        group.buttons.push(new ToolBarButton("core.collapseall", "core.collapseall", IconLibrary.FontAwesome, "fa-chevron-double-up", () => { this.collapseAllSettings(); }));
        this.settingsToolbar.addButtonGroup(group);
    }

    private onDoLookups(): ng.IPromise<any> {
        this.setupBreakButtonIcons();
        this.setupDistanceWorkButtonIcons();

        return this.loadTerms().then(() => {
            return this.$q.all([
                this.loadTerms(),
                this.loadCompanySettings(),
                this.loadTypes(),
                this.loadIdentifyTypes(),
                this.loadAccounts(),
                this.loadDeviationCauses(),
                this.loadCountries(),
                this.loadLanguages(),
                this.loadTimeZones(),
                this.loadAttendanceViewSortOrders(),
                this.loadTerminalGroupNames(),
                this.loadLogLevels()
            ]);
        });
    }

    private onLoadData(): ng.IPromise<any> {
        if (this.timeTerminalId) {
            return this.loadTerminal();
        } else {
            this.new();
        }
    }

    // LOOKUPS

    private setupBreakButtonIcons() {
        this.breakButtonIcons.push({ id: 'fa-mug-hot', name: ' ', icon: "fas fa-mug-hot" });
        this.breakButtonIcons.push({ id: 'fa-mug', name: ' ', icon: "fas fa-mug" });
        this.breakButtonIcons.push({ id: 'fa-coffee', name: ' ', icon: "fas fa-coffee" });
        this.breakButtonIcons.push({ id: 'fa-coffee-pot', name: ' ', icon: "fas fa-coffee-pot" });
        this.breakButtonIcons.push({ id: 'fa-mug-tea', name: ' ', icon: "fas fa-mug-tea" });
        this.breakButtonIcons.push({ id: 'fa-utensils', name: ' ', icon: "fas fa-utensils" });
        this.breakButtonIcons.push({ id: 'fa-utensils-alt', name: ' ', icon: "fas fa-utensils-alt" });
        this.breakButtonIcons.push({ id: 'fa-hamburger', name: ' ', icon: "fas fa-hamburger" });
        this.breakButtonIcons.push({ id: 'fa-burger-soda', name: ' ', icon: "fas fa-burger-soda" });
        this.breakButtonIcons.push({ id: 'fa-pizza-slice', name: ' ', icon: "fas fa-pizza-slice" });
        this.breakButtonIcons.push({ id: 'fa-salad', name: ' ', icon: "fas fa-salad" });
        this.breakButtonIcons.push({ id: 'fa-snooze', name: ' ', icon: "fas fa-snooze" });
        this.breakButtonIcons.push({ id: 'fa-pause', name: ' ', icon: "fas fa-pause" });
    }

    private setupDistanceWorkButtonIcons() {
        this.distanceWorkButtonIcons.push({ id: 'fa-home', name: ' ', icon: "fas fa-home" });
        this.distanceWorkButtonIcons.push({ id: 'fa-house-user', name: ' ', icon: "fas fa-house-user" });
        this.distanceWorkButtonIcons.push({ id: 'fa-building', name: ' ', icon: "fas fa-building" });
        this.distanceWorkButtonIcons.push({ id: 'fa-hotel', name: ' ', icon: "fas fa-hotel" });
        this.distanceWorkButtonIcons.push({ id: 'fa-car', name: ' ', icon: "fas fa-car" });
        this.distanceWorkButtonIcons.push({ id: 'fa-truck', name: ' ', icon: "fas fa-truck" });
        this.distanceWorkButtonIcons.push({ id: 'fa-subway', name: ' ', icon: "fas fa-subway" });
        this.distanceWorkButtonIcons.push({ id: 'fa-train', name: ' ', icon: "fas fa-train" });
        this.distanceWorkButtonIcons.push({ id: 'fa-plane', name: ' ', icon: "fas fa-plane" });
    }

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "core.time.seconds",
            "core.time.minutes",
            "core.time.hours",
            "time.time.timeterminal.defaultbreakbuttonname",
            "time.time.timeterminal.ipfilter.info",
            "time.time.timeterminal.timeterminal",
            "time.time.timeterminal.groupname.info",
            "time.time.timeterminal.selectaccountswithoutimmediatestamp.info",
            "time.time.timeterminal.loglevel.info"
        ];
        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;

            this.secondsLabel = this.terms["core.time.seconds"].toLocaleLowerCase();
            this.minutesLabel = this.terms["core.time.minutes"].toLocaleLowerCase();
            this.hoursLabel = this.terms["core.time.hours"].toLocaleLowerCase();
            this.selectAccountsWithoutImmediateStampInfoLabel = this.terms["time.time.timeterminal.selectaccountswithoutimmediatestamp.info"];
            this.ipFilterInfoLabel = this.terms["time.time.timeterminal.ipfilter.info"];
            this.terminalGroupNameInfoLabel = this.terms["time.time.timeterminal.groupname.info"];
            this.logLevelInfoLabel = this.terms["time.time.timeterminal.loglevel.info"];
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        let settingTypes: number[] = [];
        settingTypes.push(CompanySettingType.UseAccountHierarchy);
        settingTypes.push(CompanySettingType.DefaultEmployeeAccountDimEmployee);
        settingTypes.push(CompanySettingType.PossibilityToRegisterAdditionsInTerminal);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.useAccountHierarchy = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseAccountHierarchy);
            this.defaultEmployeeAccountDimId = this.limitToAccountDimId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.DefaultEmployeeAccountDimEmployee);
            this.possibilityToRegisterAdditionsInTerminal = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.PossibilityToRegisterAdditionsInTerminal);

            this.limitToAccount = this.useAccountHierarchy;
        });
    }

    private loadTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.TimeTerminalType, false, true).then(x => {
            this.types = x;
        });
    }

    private loadIdentifyTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.GoTimeStampIdentifyType, false, true, true).then(x => {
            this.identifyTypes = x;
            if (this.identifyTypes.length > 0)
                this.identifyType = this.identifyTypes[0].id;
        });
    }

    private loadAccounts(): ng.IPromise<any> {
        return this.coreService.getAccountDimsSmall(false, true, true, false, true, false, true, true).then(x => {
            this.accountDims = x;

            // Add empty
            let emptyDim: AccountDimSmallDTO = new AccountDimSmallDTO();
            emptyDim.accountDimId = 0;
            emptyDim.name = '';
            this.accountDims.splice(0, 0, emptyDim);

            _.forEach(this.accountDims, dim => {
                // Add empty accounts
                let emptyAcc: AccountDTO = new AccountDTO();
                emptyAcc.accountId = 0;
                emptyAcc.name = '';
                if (!dim.accounts)
                    dim.accounts = [];
                dim.accounts.splice(0, 0, emptyAcc);
            });
        });
    }

    private loadDeviationCauses(): ng.IPromise<any> {
        return this.timeService.getTimeDeviationCausesDict(true, false).then(x => {
            this.breakTimeDeviationCauses = x;
        });
    }

    private loadCountries(): ng.IPromise<any> {
        return this.coreService.getSysCountries(false, false).then(x => {
            this.countries = x;
        });
    }

    private loadLanguages(): ng.IPromise<any> {
        return this.coreService.getSysLanguages(false, true).then(x => {
            this.languages = x;
        });
    }

    private loadTimeZones(): ng.IPromise<any> {
        return this.timeService.getTimeZones().then(x => {
            this.timeZones = x;
        });
    }

    private loadAttendanceViewSortOrders(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.TimeTerminalAttendanceViewSortOrder, false, false, true).then(x => {
            this.attendanceViewSortOrders = x;
            if (this.attendanceViewSortOrders.length > 0)
                this.attendanceViewSortOrder = this.attendanceViewSortOrders[0].id;
        });
    }

    private loadTerminalGroupNames(): ng.IPromise<any> {
        return this.timeService.getTerminalGroupNames().then(x => {
            this.terminalGroupNames = x;
            this.terminalGroupNames.splice(0, 0, '');
        });
    }

    private loadLogLevels(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.TimeTerminalLogLevel, false, false, true).then(x => {
            this.logLevels = x;
            if (this.logLevels.length > 0)
                this.logLevel = this.logLevels[0].id;
        });
    }

    // SERVICE CALLS

    private loadTerminal(): ng.IPromise<any> {
        return this.timeService.getTimeTerminal(this.timeTerminalId).then(x => {
            this.isNew = false;

            this.terminal = x;
            this.messagingHandler.publishSetTabLabel(this.guid, this.terms["time.time.timeterminal.timeterminal"] + ' ' + this.terminal.name);
            this.loadSelectedCategories().then(() => {
                this.populateFromSettings();
                this.dirtyHandler.clean();
            });
        });
    }

    private loadSelectedCategories(): ng.IPromise<any> {
        return this.coreService.getCompanyCategoryRecords(SoeCategoryType.Employee, SoeCategoryRecordEntity.TimeTerminal, this.timeTerminalId, false).then(x => {
            this.categoryIds = x.map(c => c.categoryId);
        });
    }

    private new() {
        this.isNew = true;

        this.timeTerminalId = 0;
        this.terminal = new TimeTerminalDTO();
        this.terminal.type = TimeTerminalType.GoTimeStamp;
        this.terminal.registered = false;
        this.terminal.isActive = true;

        // Default settings
        this.terminal.timeTerminalSettings = [];
        this.identifyType = TermGroup_GoTimeStampIdentifyType.EmployeeNumberAndTag;
        this.countryId = TermGroup_Country.SE;
        this.setDefaultLanguage();
        this.attendanceViewSortOrder = TermGroup_TimeTerminalAttendanceViewSortOrder.Name;
        this.logLevel = TermGroup_TimeTerminalLogLevel.Warning;
        this.showActionBreak = true;
        this.breakButtonName = this.terms["time.time.timeterminal.defaultbreakbuttonname"];        
        this.showCurrentSchedule = true;
        this.showNextSchedule = true;
        this.showBreakAccumulator = true;
        this.showOtherAccumulators = true;
        this.showTimeStampHistory = true;
        this.showUnreadInformation = false;
        this.syncInterval = this.SYNC_INTERVAL_DEFAULT;
        this.inactivityDelay = this.INACTIVITY_DELAY_DEFAULT;
        this.maximizeWindow = true;

        this.focusService.focusById("ctrl_terminal_name");
    }

    // EVENTS

    private typeChanged() {
        this.$timeout(() => {
            if (this.isNew)
                this.terminal.registered = !this.terminal.isTimeStampType;
        });
    }

    private copyUrl() {
        HtmlUtility.copyTextToClipboard(this.goTimeStampUrl);
    }

    private openUrl() {
        HtmlUtility.openInNewTab(this.$window, this.goTimeStampUrl);
    }

    private expandAllSettings() {
        this.expanderStartPageOpen = true;
        this.expanderAccountingOpen = true;
        this.expanderAdditionsOpen = true;
        this.expanderStampingOpen = true;
        this.expanderGlobalizationOpen = true;
        this.expanderLimitsOpen = true;
        this.expanderAttendanceViewOpen = true;
        this.expanderGroupingOpen = true;
        this.expanderInformationOpen = true;
        this.expanderNewEmployeeOpen = true;
        this.expanderMiscSettingsOpen = true;
    }

    private collapseAllSettings() {
        this.expanderStartPageOpen = false;
        this.expanderAccountingOpen = false;
        this.expanderAdditionsOpen = false;
        this.expanderStampingOpen = false;
        this.expanderGlobalizationOpen = false;
        this.expanderLimitsOpen = false;
        this.expanderAttendanceViewOpen = false;
        this.expanderGroupingOpen = false;
        this.expanderInformationOpen = false;
        this.expanderNewEmployeeOpen = false;
        this.expanderMiscSettingsOpen = false;
    }

    private accountDimChanged() {
        this.$timeout(() => {
            this.internalAccounts = this.accountDim ? this.accountDim.accounts : [];
            this.setAccountDim2();
        });
    }

    private accountDim2Changed() {
        this.$timeout(() => {
            this.internalAccounts2 = this.accountDim2 ? this.accountDim2.accounts : [];
        });
    }

    private limitToAccountDimChanged() {
        this.limitAccountIds = [];
    }

    private accountDimsInHierarchyChanged() {
        this.$timeout(() => {
            this.setAccountDim2();
        });
    }

    private setAccountDim2() {
        if (this.accountDimsInHierarchy) {
            const topLevel = this.accountDim.level;
            const subDim = this.accountDims.find(d => d.level === topLevel + 1);
            if (subDim)
                this.accountDim2 = subDim;
        }
    }

    private selectAccountsWithoutImmediateStampChanged() {
        this.$timeout(() => {
            if (!this.selectAccountsWithoutImmediateStamp)
                this.rememberAccountsAfterBreak = false;
        });
    }

    private showActionBreakChanged() {
        this.$timeout(() => {
            this.setDefaultBreakNameAndIcon();
        });
    }

    private breakTimeDeviationCauseChanged() {

    }

    private breakButtonIconSelected(option) {
        this.breakButtonIcon = option.id;
        this.setDirty();
    }

    private showActionBreakAltChanged() {
        this.$timeout(() => {
            this.setDefaultBreakAltNameAndIcon();
        });
    }

    private breakAltTimeDeviationCauseChanged() {

    }

    private breakAltButtonIconSelected(option) {
        this.breakAltButtonIcon = option.id;
        this.setDirty();
    }

    private useDistanceWorkChanged() {
        this.$timeout(() => {
            this.setDefaultDistanceWorkIcon();
        });
    }

    private distanceWorkButtonIconSelected(option) {
        this.distanceWorkButtonIcon = option.id;
        this.setDirty();
    }

    private selectLanguage(language: ISmallGenericType) {
        if (!language)
            return;

        // Toggle selected
        language['selected'] = !language['selected'];

        // If default language is unselected, also unselect it as default
        if (!language['selected'] && language['default'])
            language['default'] = false;

        // Must have a default language, set first selected
        if (!_.find(this.languages, l => l['default'])) {
            let firstLanguage = _.find(this.languages, l => l['selected']);
            if (firstLanguage)
                firstLanguage['default'] = true;
        }

        this.setDirty();
    }

    private selectDefaultLanguage(language: ISmallGenericType) {
        if (!language['dafault']) {
            this.languages.forEach(l => l['default'] = false);
            language['default'] = true;
            this.setTimeZoneBasedOnLanguage();
            this.setDirty();
        }
    }

    // ACTIONS

    protected copy() {
        super.copy();
        this.terminal.timeTerminalId = 0;
        this.timeTerminalId = 0;
        this.isNew = true;

        this.terminal.registered = false;
        this.terminal.lastSync = null;
        this.terminal.lastSyncStateColor = "#D3D3D3"; // Gray
        this.terminal.created = null;
        this.terminal.createdBy = undefined;
        this.terminal.modified = null;
        this.terminal.modifiedBy = undefined;

        this.focusService.focusById("ctrl_terminal_name");
    }

    private migrate() {
        let oldId = this.timeTerminalId;
        let oldName = this.terminal.name;
        this.copy();

        let newGuid = Guid.newGuid();
        this.messagingHandler.publishSetTabGuid(this.guid, newGuid);
        this.guid = newGuid;
        this.messagingHandler.publishEditRow({ timeTerminalId: oldId, name: oldName }, { doNotActivateTab: true });

        this.$timeout(() => {
            this.terminal.type = TimeTerminalType.GoTimeStamp;

            // Default new settings
            this.showActionBreak = true;
            this.showCurrentSchedule = true;
            this.showNextSchedule = true;
            this.showBreakAccumulator = !this.hideInformation;
            this.showOtherAccumulators = !this.hideInformation && !this.showOnlyBreakAccumulator;
            this.showTimeStampHistory = true;
            this.showUnreadInformation = false;

            // Migrate settings
            this.identifyType = this.onlyStampWithTag ? TermGroup_GoTimeStampIdentifyType.OnlyTag : TermGroup_GoTimeStampIdentifyType.EmployeeNumberAndTag;

            let language = _.find(this.languages, l => l.id == this.countryId);
            if (language)
                this.selectLanguage(language);
            else
                this.setDefaultLanguage();

            let keys: string[] = [
                "time.time.timeterminal.migrate.donetitle",
                "time.time.timeterminal.migrate.donemessage"
            ];
            this.translationService.translateMany(keys).then(terms => {
                this.notificationService.showDialogEx(terms["time.time.timeterminal.migrate.donetitle"], terms["time.time.timeterminal.migrate.donemessage"], SOEMessageBoxImage.Information);
            });
        });
    }

    private setSettingsForSave() {
        // Start page
        this.setStringSetting(TimeTerminalSettingType.StartpageSubject, this.startpageSubject);
        this.setStringSetting(TimeTerminalSettingType.StartpageShortText, this.startpageShortText);
        this.setStringSetting(TimeTerminalSettingType.StartpageText, this.startpageText);
        this.setIntSetting(TimeTerminalSettingType.IdentifyType, this.identifyType);

        // Accounting
        this.setIntSetting(TimeTerminalSettingType.AccountDim, this.accountDim ? this.accountDim.accountDimId : 0);
        this.setStringSetting(TimeTerminalSettingType.AccountDim, this.accountDim ? this.accountDim.name : null);
        this.setBoolSetting(TimeTerminalSettingType.LimitSelectableAccounts, this.limitSelectableAccounts);
        if (!this.limitSelectableAccounts)
            this.selectedAccountIds = [];
        this.setStringSetting(TimeTerminalSettingType.SelectedAccounts, this.selectedAccountIds ? this.selectedAccountIds.join(',') : '');

        this.setBoolSetting(TimeTerminalSettingType.AccountDimsInHierarchy, this.accountDimsInHierarchy);

        this.setIntSetting(TimeTerminalSettingType.AccountDim2, this.accountDim2 ? this.accountDim2.accountDimId : 0);
        this.setStringSetting(TimeTerminalSettingType.AccountDim2, this.accountDim2 ? this.accountDim2.name : null);
        this.setBoolSetting(TimeTerminalSettingType.LimitSelectableAccounts2, this.limitSelectableAccounts2);
        if (!this.limitSelectableAccounts2)
            this.selectedAccountIds2 = [];
        this.setStringSetting(TimeTerminalSettingType.SelectedAccounts2, this.selectedAccountIds2 ? this.selectedAccountIds2.join(',') : '');
        this.setBoolSetting(TimeTerminalSettingType.SelectAccountsWithoutImmediateStamp, this.selectAccountsWithoutImmediateStamp);
        this.setBoolSetting(TimeTerminalSettingType.RememberAccountsAfterBreak, this.rememberAccountsAfterBreak);

        this.setIntSetting(TimeTerminalSettingType.InternalAccountDim1Id, this.internalAccount ? this.internalAccount.accountId : 0);

        // Additions
        this.setBoolSetting(TimeTerminalSettingType.LimitSelectableAdditions, this.limitSelectableAdditions);
        if (!this.limitSelectableAdditions)
            this.selectedAdditions = [];
        this.setStringSetting(TimeTerminalSettingType.SelectedAdditions, this.selectedAdditions ? this.selectedAdditions.map(a => a.hashedString).join(',') : '');

        // Stamping
        this.setBoolSetting(TimeTerminalSettingType.OnlyStampWithTag, this.onlyStampWithTag);
        this.setBoolSetting(TimeTerminalSettingType.OnlyDigitsInCardNumber, this.onlyDigitsInCardNumber);
        this.setBoolSetting(TimeTerminalSettingType.ForceCorrectTypeTimelineOrder, this.forceCorrectTypeTimelineOrder);

        this.setBoolSetting(TimeTerminalSettingType.ShowActionBreak, this.showActionBreak);
        this.setBoolSetting(TimeTerminalSettingType.BreakIsPaid, this.showActionBreak ? this.breakIsPaid : false);
        this.setIntSetting(TimeTerminalSettingType.BreakTimeDeviationCause, this.showActionBreak ? this.breakTimeDeviationCauseId : 0);
        this.setStringSetting(TimeTerminalSettingType.BreakButtonName, this.showActionBreak ? this.breakButtonName : '');
        this.setStringSetting(TimeTerminalSettingType.BreakButtonIcon, this.showActionBreak ? (this.breakButtonIcon ? this.breakButtonIcon : this.breakButtonIcons[0].id) : '');

        if (!this.showActionBreak)
            this.showActionBreakAlt = false;

        this.setBoolSetting(TimeTerminalSettingType.ShowActionBreakAlt, this.showActionBreakAlt);
        this.setBoolSetting(TimeTerminalSettingType.BreakAltIsPaid, this.showActionBreakAlt ? this.breakAltIsPaid : false);
        this.setIntSetting(TimeTerminalSettingType.BreakAltTimeDeviationCause, this.showActionBreakAlt ? this.breakAltTimeDeviationCauseId : 0);
        this.setStringSetting(TimeTerminalSettingType.BreakAltButtonName, this.showActionBreakAlt ? this.breakAltButtonName : '');
        this.setStringSetting(TimeTerminalSettingType.BreakAltButtonIcon, this.showActionBreakAlt ? (this.breakAltButtonIcon ? this.breakAltButtonIcon : this.breakButtonIcons[0].id) : '');

        this.setBoolSetting(TimeTerminalSettingType.ForceCauseIfOutOfSchedule, this.forceCause);
        this.setIntSetting(TimeTerminalSettingType.ForceCauseGraceMinutes, this.forceCauseGraceTime);
        this.setIntSetting(TimeTerminalSettingType.ForceCauseGraceMinutesOutsideSchedule, this.forceCauseGraceTimeOutside);
        this.setIntSetting(TimeTerminalSettingType.ForceCauseGraceMinutesInsideSchedule, this.forceCauseGraceTimeInside);
        this.setBoolSetting(TimeTerminalSettingType.IgnoreForceCauseOnBreak, this.ignoreForceCauseOnBreak);
        this.setBoolSetting(TimeTerminalSettingType.ValidateNoSchedule, this.validateNoSchedule);
        this.setBoolSetting(TimeTerminalSettingType.ValidateAbsence, this.validateAbsence);
        this.setBoolSetting(TimeTerminalSettingType.ShowNotificationWhenStamping, this.showNotificationWhenStamping);
        this.setBoolSetting(TimeTerminalSettingType.UseAutoStampOut, this.useAutoStampOut);
        this.setIntSetting(TimeTerminalSettingType.UseAutoStampOutTime, this.useAutoStampOutTimeInMinutes);
        this.setBoolSetting(TimeTerminalSettingType.UseDistanceWork, this.useDistanceWork);
        this.setStringSetting(TimeTerminalSettingType.DistanceWorkButtonName, this.useDistanceWork ? this.distanceWorkButtonName : '');
        this.setStringSetting(TimeTerminalSettingType.DistanceWorkButtonIcon, this.useDistanceWork ? (this.distanceWorkButtonIcon ? this.distanceWorkButtonIcon : this.distanceWorkButtonIcons[0].id) : '');

        // Globalization
        this.setIntSetting(TimeTerminalSettingType.SysCountryId, this.countryId);

        let defaultLang = _.find(this.languages, l => l['selected'] && l['default']);
        if (!defaultLang)
            defaultLang = _.find(this.languages, l => l['selected']);
        if (!defaultLang) {
            this.setDefaultLanguage();
            defaultLang = _.find(this.languages, l => l['selected'] && l['default']);
        }

        if (defaultLang) {
            let childLangs: number[] = _.filter(this.languages, l => l['selected'] && !l['default']).map(l => l.id);
            this.setIntSetting(TimeTerminalSettingType.Languages, defaultLang.id, childLangs);
        }

        this.setStringSetting(TimeTerminalSettingType.TimeZone, this.timeZone);
        this.setIntSetting(TimeTerminalSettingType.AdjustTime, this.adjustTime);

        // Limits
        this.setIntSetting(TimeTerminalSettingType.LimitTimeTerminalToAccountDim, this.limitToAccount ? this.limitToAccountDimId : 0);
        this.setBoolSetting(TimeTerminalSettingType.LimitTimeTerminalToAccount, this.limitToAccount);
        if (this.limitToAccount) {
            if (this.limitAccountIds.length > 0) {
                let parentAccountId: number = this.limitAccountIds[0];
                let childrenAccountIds: number[] = [];
                if (this.limitAccountIds.length > 1)
                    childrenAccountIds = _.filter(this.limitAccountIds, a => a !== parentAccountId);

                this.setIntSetting(TimeTerminalSettingType.LimitAccount, parentAccountId, _.uniq(childrenAccountIds));
                this.setStringSetting(TimeTerminalSettingType.LimitAccount, null);
            }
        } else {
            this.limitAccountIds = [];
            this.setIntSetting(TimeTerminalSettingType.LimitAccount, null);
            this.setStringSetting(TimeTerminalSettingType.LimitAccount, null);
        }

        this.setBoolSetting(TimeTerminalSettingType.LimitTimeTerminalToCategories, this.limitToCategories);
        this.terminal.categoryIds = this.limitToCategories ? this.categoryIds : [];
        this.setStringSetting(TimeTerminalSettingType.IpFilter, this.ipFilter);

        // Attendace view
        this.setBoolSetting(TimeTerminalSettingType.HideAttendanceView, this.attendanceViewHide);
        this.setBoolSetting(TimeTerminalSettingType.ShowTimeInAttendanceView, this.attendanceViewShowTime);
        this.setBoolSetting(TimeTerminalSettingType.ShowPicturesInAttendenceView, this.attendanceViewShowPictures);
        this.setIntSetting(TimeTerminalSettingType.AttendanceViewSortOrder, this.attendanceViewSortOrder);
        this.setStringSetting(TimeTerminalSettingType.TerminalGroupName, this.terminalGroupName);

        // Information
        this.setBoolSetting(TimeTerminalSettingType.HideInformationButton, this.hideInformation);
        this.setBoolSetting(TimeTerminalSettingType.ShowOnlyBreakAcc, this.showOnlyBreakAccumulator);
        this.setBoolSetting(TimeTerminalSettingType.ShowCurrentSchedule, this.showCurrentSchedule);
        this.setBoolSetting(TimeTerminalSettingType.ShowNextSchedule, this.showNextSchedule);
        this.setBoolSetting(TimeTerminalSettingType.ShowBreakAcc, this.showBreakAccumulator);
        this.setBoolSetting(TimeTerminalSettingType.ShowOtherAcc, this.showOtherAccumulators);
        this.setBoolSetting(TimeTerminalSettingType.ShowTimeStampHistory, this.showTimeStampHistory);
        this.setBoolSetting(TimeTerminalSettingType.ShowUnreadInformation, this.showUnreadInformation);

        // New employee
        this.setBoolSetting(TimeTerminalSettingType.NewEmployee, this.useNewEmployee);
        this.setBoolSetting(TimeTerminalSettingType.ForceSocialSecNbr, this.forceSocialSecNbr);

        // Misc
        if (!this.syncInterval)
            this.syncInterval = this.SYNC_INTERVAL_DEFAULT;
        if (this.syncInterval < this.SYNC_INTERVAL_MIN)
            this.syncInterval = this.SYNC_INTERVAL_MIN;
        this.setIntSetting(TimeTerminalSettingType.SyncInterval, this.syncInterval);

        if (!this.inactivityDelay)
            this.inactivityDelay = this.INACTIVITY_DELAY_DEFAULT;
        if (this.inactivityDelay < this.INACTIVITY_DELAY_MIN)
            this.inactivityDelay = this.INACTIVITY_DELAY_MIN;
        this.setIntSetting(TimeTerminalSettingType.InactivityDelay, this.inactivityDelay);

        this.setBoolSetting(TimeTerminalSettingType.MaximizeWindow, this.maximizeWindow);
        this.setIntSetting(TimeTerminalSettingType.LogLevel, this.logLevel);
    }

    private save() {
        if (!this.validateSave())
            return;

        this.setSettingsForSave();

        this.progress.startSaveProgress((completion) => {
            this.timeService.saveTimeTerminal(this.terminal).then(result => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0) {
                        if (this.timeTerminalId == 0) {
                            if (this.navigatorRecords) {
                                this.navigatorRecords.push(new SmallGenericType(result.integerValue, this.terminal.name));
                                this.toolbar.setSelectedRecord(result.integerValue);
                            } else {
                                this.reloadNavigationRecords(result.integerValue);
                            }
                        }
                        this.timeTerminalId = result.integerValue;
                        this.terminal.timeTerminalId = this.timeTerminalId;
                        this.loadTerminalGroupNames();
                        this.loadTerminal();
                        completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.terminal);
                    }
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid).then(data => {
            this.dirtyHandler.clean();
            this.onLoadData();
        }, error => {
        });
    }

    private reloadNavigationRecords(selectedRecord) {
        this.navigatorRecords = [];
        this.timeService.getTimeTerminals(TimeTerminalType.Unknown, false, false, false, true, false, true, true).then(data => {
            _.forEach(data, (row) => {
                this.navigatorRecords.push(new SmallGenericType(row.timeTerminalId, row.name));
            });
            this.toolbar.setupNavigationRecords(this.navigatorRecords, selectedRecord, recordId => {
                if (recordId !== this.timeTerminalId) {
                    this.timeTerminalId = recordId;
                    this.onLoadData();
                }
            });
            this.toolbar.setSelectedRecord(selectedRecord);
        });
    }

    // HELP-METHODS

    private setDefaultBreakNameAndIcon() {
        if (this.showActionBreak && !this.breakButtonIcon)
            this.breakButtonIcon = this.breakButtonIcons[0].id;

        if (this.showActionBreak && !this.breakButtonName)
            this.breakButtonName = this.terms["time.time.timeterminal.defaultbreakbuttonname"];
    }

    private setDefaultBreakAltNameAndIcon() {
        if (this.showActionBreakAlt && !this.breakAltButtonIcon)
            this.breakAltButtonIcon = this.breakButtonIcons[0].id;

        if (this.showActionBreakAlt && !this.breakAltButtonName)
            this.breakAltButtonName = this.terms["time.time.timeterminal.defaultbreakbuttonname"];
    }

    private setDefaultDistanceWorkIcon() {
        if (this.useDistanceWork && !this.distanceWorkButtonIcon)
            this.distanceWorkButtonIcon = this.distanceWorkButtonIcons[0].id;
    }

    private setDefaultLanguage() {
        let lang = _.find(this.languages, l => l.id === CoreUtility.languageId);
        if (lang) {
            this.selectLanguage(lang);
            this.selectDefaultLanguage(lang);
        }
    }

    private setTimeZoneBasedOnLanguage() {
        let lang = _.find(this.languages, l => l['default']);
        if (!lang)
            lang = _.find(this.languages, l => l.id === CoreUtility.languageId);

        switch (lang.id) {
            case TermGroup_Languages.Unknown:
            case TermGroup_Languages.Swedish:
                this.timeZone = 'W. Europe Standard Time';
                break;
            case TermGroup_Languages.English:
                this.timeZone = 'GMT Standard Time';
                break;
            case TermGroup_Languages.Finnish:
                this.timeZone = 'FLE Standard Time';
                break;
            case TermGroup_Languages.Norwegian:
                this.timeZone = 'Central Europe Standard Time';
                break;
            case TermGroup_Languages.Danish:
                this.timeZone = 'Romance Standard Time';
                break;
        }
    }

    private setDirty() {
        this.dirtyHandler.setDirty();
    }

    private populateFromSettings() {
        // Startpage
        this.startpageSubject = <string>this.getSettingValue(TimeTerminalSettingType.StartpageSubject);
        this.startpageShortText = <string>this.getSettingValue(TimeTerminalSettingType.StartpageShortText);
        this.startpageText = <string>this.getSettingValue(TimeTerminalSettingType.StartpageText);
        this.identifyType = <number>this.getSettingValue(TimeTerminalSettingType.IdentifyType);

        // Accounting
        let accountDimId: SettingType = this.getSettingValue(TimeTerminalSettingType.AccountDim);
        if (accountDimId) {
            this.accountDim = _.find(this.accountDims, a => a.accountDimId == accountDimId);
            if (this.accountDim) {
                this.internalAccounts = this.accountDim.accounts;
                let internalAccountId: SettingType = this.getSettingValue(TimeTerminalSettingType.InternalAccountDim1Id);
                this.internalAccount = _.find(this.internalAccounts, a => a.accountId == internalAccountId);
            }
        }

        this.limitSelectableAccounts = <boolean>this.getSettingValue(TimeTerminalSettingType.LimitSelectableAccounts);
        let accSetting: string = <string>this.getSettingValue(TimeTerminalSettingType.SelectedAccounts);
        this.selectedAccountIds = accSetting ? accSetting.split(',').map(Number) : [];

        let accountDimId2: SettingType = this.getSettingValue(TimeTerminalSettingType.AccountDim2);
        if (accountDimId2) {
            this.accountDim2 = _.find(this.accountDims, a => a.accountDimId == accountDimId2);
            if (this.accountDim2) {
                this.internalAccounts2 = this.accountDim2.accounts;
            }
        }

        this.limitSelectableAccounts2 = <boolean>this.getSettingValue(TimeTerminalSettingType.LimitSelectableAccounts2);
        let accSetting2: string = <string>this.getSettingValue(TimeTerminalSettingType.SelectedAccounts2);
        this.selectedAccountIds2 = accSetting2 ? accSetting2.split(',').map(Number) : [];

        this.accountDimsInHierarchy = <boolean>this.getSettingValue(TimeTerminalSettingType.AccountDimsInHierarchy);
        this.selectAccountsWithoutImmediateStamp = <boolean>this.getSettingValue(TimeTerminalSettingType.SelectAccountsWithoutImmediateStamp);
        this.rememberAccountsAfterBreak = <boolean>this.getSettingValue(TimeTerminalSettingType.RememberAccountsAfterBreak);

        // Additions
        // Saved as comma separated hashed string (type#id, type#id, type#id)
        this.limitSelectableAdditions = <boolean>this.getSettingValue(TimeTerminalSettingType.LimitSelectableAdditions);
        this.selectedAdditions = [];
        let additionsSetting: string = <string>this.getSettingValue(TimeTerminalSettingType.SelectedAdditions);
        if (additionsSetting) {
            let hashedString: string[] = additionsSetting.split(',');
            if (hashedString.length > 0) {
                hashedString.forEach(str => {
                    let ids = str.split('#');
                    if (ids.length === 2)
                        this.selectedAdditions.push(new IntKeyValue(parseInt(ids[0]), parseInt(ids[1])));
                });
            }
        }

        // Stamping
        this.onlyStampWithTag = <boolean>this.getSettingValue(TimeTerminalSettingType.OnlyStampWithTag);
        this.onlyDigitsInCardNumber = <boolean>this.getSettingValue(TimeTerminalSettingType.OnlyDigitsInCardNumber);
        this.forceCorrectTypeTimelineOrder = <boolean>this.getSettingValue(TimeTerminalSettingType.ForceCorrectTypeTimelineOrder);

        this.showActionBreak = <boolean>this.getSettingValue(TimeTerminalSettingType.ShowActionBreak);
        this.breakIsPaid = <boolean>this.getSettingValue(TimeTerminalSettingType.BreakIsPaid);
        this.breakTimeDeviationCauseId = <number>this.getSettingValue(TimeTerminalSettingType.BreakTimeDeviationCause);
        this.breakButtonName = <string>this.getSettingValue(TimeTerminalSettingType.BreakButtonName);
        this.breakButtonIcon = <string>this.getSettingValue(TimeTerminalSettingType.BreakButtonIcon);
        this.setDefaultBreakNameAndIcon();

        this.showActionBreakAlt = <boolean>this.getSettingValue(TimeTerminalSettingType.ShowActionBreakAlt);
        this.breakAltIsPaid = <boolean>this.getSettingValue(TimeTerminalSettingType.BreakAltIsPaid);
        this.breakAltTimeDeviationCauseId = <number>this.getSettingValue(TimeTerminalSettingType.BreakAltTimeDeviationCause);
        this.breakAltButtonName = <string>this.getSettingValue(TimeTerminalSettingType.BreakAltButtonName);
        this.breakAltButtonIcon = <string>this.getSettingValue(TimeTerminalSettingType.BreakAltButtonIcon);
        this.setDefaultBreakAltNameAndIcon();

        this.forceCause = <boolean>this.getSettingValue(TimeTerminalSettingType.ForceCauseIfOutOfSchedule);
        this.forceCauseGraceTime = <number>this.getSettingValue(TimeTerminalSettingType.ForceCauseGraceMinutes);
        this.forceCauseGraceTimeOutside = <number>this.getSettingValue(TimeTerminalSettingType.ForceCauseGraceMinutesOutsideSchedule);
        this.forceCauseGraceTimeInside = <number>this.getSettingValue(TimeTerminalSettingType.ForceCauseGraceMinutesInsideSchedule);
        this.ignoreForceCauseOnBreak = <boolean>this.getSettingValue(TimeTerminalSettingType.IgnoreForceCauseOnBreak);
        this.validateNoSchedule = <boolean>this.getSettingValue(TimeTerminalSettingType.ValidateNoSchedule);
        this.validateAbsence = <boolean>this.getSettingValue(TimeTerminalSettingType.ValidateAbsence);
        this.showNotificationWhenStamping = <boolean>this.getSettingValue(TimeTerminalSettingType.ShowNotificationWhenStamping);
        this.useAutoStampOut = <boolean>this.getSettingValue(TimeTerminalSettingType.UseAutoStampOut);
        this.useAutoStampOutTimeInMinutes = <number>this.getSettingValue(TimeTerminalSettingType.UseAutoStampOutTime);
        this.useDistanceWork = <boolean>this.getSettingValue(TimeTerminalSettingType.UseDistanceWork);
        this.distanceWorkButtonName = <string>this.getSettingValue(TimeTerminalSettingType.DistanceWorkButtonName);
        this.distanceWorkButtonIcon = <string>this.getSettingValue(TimeTerminalSettingType.DistanceWorkButtonIcon);
        this.setDefaultDistanceWorkIcon();

        // Globalization
        this.countryId = <number>this.getSettingValue(TimeTerminalSettingType.SysCountryId);

        let langSetting = this.getSetting(TimeTerminalSettingType.Languages);
        if (langSetting) {
            let defaultLang = _.find(this.languages, l => l.id === langSetting.intData);
            if (defaultLang) {
                defaultLang['selected'] = true;
                defaultLang['default'] = true;
            } else {
                this.setDefaultLanguage();
            }

            if (langSetting.children) {
                _.forEach(langSetting.children, child => {
                    let childLang = _.find(this.languages, l => l.id === child.intData);
                    if (childLang)
                        childLang['selected'] = true;
                });
            }
        }

        this.timeZone = <string>this.getSettingValue(TimeTerminalSettingType.TimeZone);
        if (!this.timeZone)
            this.setTimeZoneBasedOnLanguage();

        this.adjustTime = <number>this.getSettingValue(TimeTerminalSettingType.AdjustTime);
        //this.syncClockWithServer = <boolean>this.getSettingValue(TimeTerminalSettingType.SyncClockWithServer);
        //this.syncClockWithServerDiff = <number>this.getSettingValue(TimeTerminalSettingType.SyncClockWithServerDiff);

        // Limits
        this.limitToAccountDimId = <number>this.getSettingValue(TimeTerminalSettingType.LimitTimeTerminalToAccountDim);
        if (!this.limitToAccountDimId)
            this.limitToAccountDimId = this.defaultEmployeeAccountDimId;

        this.limitToAccount = this.useAccountHierarchy || <boolean>this.getSettingValue(TimeTerminalSettingType.LimitTimeTerminalToAccount);
        this.limitAccountIds = [];
        if (this.limitToAccount) {
            this.limitToAccountDimId = <number>this.getSettingValue(TimeTerminalSettingType.LimitTimeTerminalToAccountDim);
            if (!this.limitToAccountDimId)
                this.limitToAccountDimId = this.defaultEmployeeAccountDimId;

            let limitAccSetting = this.getSetting(TimeTerminalSettingType.LimitAccount);
            if (limitAccSetting) {
                // Account was previously stored as string
                // Now it's stored as int, with children
                if (limitAccSetting.intData)
                    this.limitAccountIds.push(limitAccSetting.intData);
                else
                    this.limitAccountIds.push(parseInt(limitAccSetting.strData, 10));

                if (limitAccSetting.children)
                    this.limitAccountIds.push(...limitAccSetting.children.map(c => c.intData));
            }
        }

        this.limitToCategories = <boolean>this.getSettingValue(TimeTerminalSettingType.LimitTimeTerminalToCategories);
        this.ipFilter = <string>this.getSettingValue(TimeTerminalSettingType.IpFilter);

        // Attendance view
        this.attendanceViewHide = <boolean>this.getSettingValue(TimeTerminalSettingType.HideAttendanceView);
        this.attendanceViewShowTime = <boolean>this.getSettingValue(TimeTerminalSettingType.ShowTimeInAttendanceView);
        this.attendanceViewShowPictures = <boolean>this.getSettingValue(TimeTerminalSettingType.ShowPicturesInAttendenceView);
        this.attendanceViewSortOrder = <number>this.getSettingValue(TimeTerminalSettingType.AttendanceViewSortOrder) || TermGroup_TimeTerminalAttendanceViewSortOrder.Name;
        this.terminalGroupName = <string>this.getSettingValue(TimeTerminalSettingType.TerminalGroupName);

        // Information
        this.hideInformation = <boolean>this.getSettingValue(TimeTerminalSettingType.HideInformationButton);
        this.showOnlyBreakAccumulator = <boolean>this.getSettingValue(TimeTerminalSettingType.ShowOnlyBreakAcc);
        this.showCurrentSchedule = <boolean>this.getSettingValue(TimeTerminalSettingType.ShowCurrentSchedule);
        this.showNextSchedule = <boolean>this.getSettingValue(TimeTerminalSettingType.ShowNextSchedule);
        this.showBreakAccumulator = <boolean>this.getSettingValue(TimeTerminalSettingType.ShowBreakAcc);
        this.showOtherAccumulators = <boolean>this.getSettingValue(TimeTerminalSettingType.ShowOtherAcc);
        this.showTimeStampHistory = <boolean>this.getSettingValue(TimeTerminalSettingType.ShowTimeStampHistory);
        this.showUnreadInformation = <boolean>this.getSettingValue(TimeTerminalSettingType.ShowUnreadInformation);

        // New employee
        this.useNewEmployee = <boolean>this.getSettingValue(TimeTerminalSettingType.NewEmployee);
        this.forceSocialSecNbr = <boolean>this.getSettingValue(TimeTerminalSettingType.ForceSocialSecNbr);

        // Misc
        this.syncInterval = <number>this.getSettingValue(TimeTerminalSettingType.SyncInterval);
        this.inactivityDelay = <number>this.getSettingValue(TimeTerminalSettingType.InactivityDelay);
        this.maximizeWindow = <boolean>this.getSettingValue(TimeTerminalSettingType.MaximizeWindow);

        const logLevelSetting = this.getSettingValue(TimeTerminalSettingType.LogLevel);
        this.logLevel = logLevelSetting !== null ? <number>logLevelSetting : TermGroup_TimeTerminalLogLevel.Warning;
    }

    private getSetting(type: TimeTerminalSettingType): TimeTerminalSettingDTO {
        return _.find(this.terminal.timeTerminalSettings, s => s.type == type && !s.parentId);
    }

    private getSettingValue(type: TimeTerminalSettingType): SettingType {
        const setting = this.getSetting(type);
        if (setting) {
            switch (setting.dataType) {
                case TimeTerminalSettingDataType.Boolean:
                    return setting.boolData;
                case TimeTerminalSettingDataType.String:
                    return setting.strData;
                case TimeTerminalSettingDataType.Integer:
                    return setting.intData;
                case TimeTerminalSettingDataType.Decimal:
                    return setting.decimalData;
                case TimeTerminalSettingDataType.Date:
                    return CalendarUtility.convertToDate(setting.dateData);
                case TimeTerminalSettingDataType.Time:
                    return CalendarUtility.convertToDate(setting.timeData);
            }
        }

        return null;
    }

    private setStringSetting(type: TimeTerminalSettingType, value: string) {
        let setting: TimeTerminalSettingDTO = this.getSetting(type);
        if (!setting) {
            setting = new TimeTerminalSettingDTO(type, TimeTerminalSettingDataType.String);
            this.terminal.timeTerminalSettings.push(setting);
        }
        setting.strData = value;
    }

    private setIntSetting(type: TimeTerminalSettingType, value: number, children: number[] = []) {
        let setting: TimeTerminalSettingDTO = this.getSetting(type);
        if (!setting) {
            setting = new TimeTerminalSettingDTO(type, TimeTerminalSettingDataType.Integer);
            this.terminal.timeTerminalSettings.push(setting);
        } else if (setting.dataType !== TimeTerminalSettingDataType.Integer)
            setting.dataType = TimeTerminalSettingDataType.Integer;

        setting.intData = value;

        if (children) {
            setting.children = [];
            _.forEach(children, child => {
                let childSetting = new TimeTerminalSettingDTO(type, TimeTerminalSettingDataType.Integer);
                childSetting.intData = child;
                setting.children.push(childSetting);
            });
        }
    }

    private setDecimalSetting(type: TimeTerminalSettingType, value: number) {
        let setting: TimeTerminalSettingDTO = this.getSetting(type);
        if (!setting) {
            setting = new TimeTerminalSettingDTO(type, TimeTerminalSettingDataType.Decimal);
            this.terminal.timeTerminalSettings.push(setting);
        }
        setting.decimalData = value;
    }

    private setBoolSetting(type: TimeTerminalSettingType, value: boolean) {
        let setting: TimeTerminalSettingDTO = this.getSetting(type);
        if (!setting) {
            setting = new TimeTerminalSettingDTO(type, TimeTerminalSettingDataType.Boolean);
            this.terminal.timeTerminalSettings.push(setting);
        }
        setting.boolData = value;
    }

    private setDateSetting(type: TimeTerminalSettingType, value: Date) {
        let setting: TimeTerminalSettingDTO = this.getSetting(type);
        if (!setting) {
            setting = new TimeTerminalSettingDTO(type, TimeTerminalSettingDataType.Date);
            this.terminal.timeTerminalSettings.push(setting);
        }
        setting.dateData = value;
    }

    private setTimeSetting(type: TimeTerminalSettingType, value: Date) {
        let setting: TimeTerminalSettingDTO = this.getSetting(type);
        if (!setting) {
            setting = new TimeTerminalSettingDTO(type, TimeTerminalSettingDataType.Time);
            this.terminal.timeTerminalSettings.push(setting);
        }
        setting.timeData = value;
    }

    // SETTINGS

    private startpageSubject: string;
    private startpageShortText: string;
    private startpageText: string;
    private identifyType: number;

    private accountDim: AccountDimSmallDTO;
    private limitSelectableAccounts: boolean;
    private selectedAccountIds: number[];

    private accountDim2: AccountDimSmallDTO;
    private limitSelectableAccounts2: boolean;
    private selectedAccountIds2: number[];

    private accountDimsInHierarchy: boolean;
    private selectAccountsWithoutImmediateStamp: boolean;
    private rememberAccountsAfterBreak: boolean;

    private internalAccount: AccountDTO;

    private limitSelectableAdditions: boolean;
    private selectedAdditions: IntKeyValue[];

    private countryId: number;
    private timeZone: string;
    private adjustTime: number;

    private attendanceViewHide: boolean;
    private attendanceViewShowTime: boolean;
    private attendanceViewShowPictures: boolean;
    private attendanceViewSortOrder: number;
    private terminalGroupName: string;
    private logLevel: number;

    private onlyStampWithTag: boolean;
    private onlyDigitsInCardNumber: boolean;
    private forceCorrectTypeTimelineOrder: boolean;

    private showActionBreak: boolean;
    private breakIsPaid: boolean;
    private breakTimeDeviationCauseId: number;
    private breakButtonName: string;
    private breakButtonIcon: string;

    private showActionBreakAlt: boolean;
    private breakAltIsPaid: boolean;
    private breakAltTimeDeviationCauseId: number;
    private breakAltButtonName: string;
    private breakAltButtonIcon: string;

    private forceCause: boolean;
    private forceCauseGraceTime: number;
    private forceCauseGraceTimeOutside: number;
    private forceCauseGraceTimeInside: number;
    private ignoreForceCauseOnBreak: boolean;
    private validateNoSchedule: boolean;
    private validateAbsence: boolean;
    private showNotificationWhenStamping: boolean;
    private useAutoStampOut: boolean;
    private useAutoStampOutTime: Date;

    private get useAutoStampOutTimeInMinutes(): number {
        return this.useAutoStampOutTime ? (this.useAutoStampOutTime.getHours() * 60 + this.useAutoStampOutTime.getMinutes()) : 0;
    }
    private set useAutoStampOutTimeInMinutes(value: number) {
        this.useAutoStampOutTime = CalendarUtility.getDateToday().addMinutes(value);
    }

    private limitToAccount: boolean;
    private limitToAccountDimId: number;
    private limitToCategories: boolean;
    private categoryIds: number[];
    private ipFilter: string;

    private hideInformation: boolean;
    private showOnlyBreakAccumulator: boolean;
    private showCurrentSchedule: boolean;
    private showNextSchedule: boolean;
    private showBreakAccumulator: boolean;
    private showOtherAccumulators: boolean;
    private showTimeStampHistory: boolean;
    private showUnreadInformation: boolean;

    private useDistanceWork: boolean;
    private distanceWorkButtonName: string;
    private distanceWorkButtonIcon: string;

    private useNewEmployee: boolean;
    private forceSocialSecNbr: boolean;

    private syncInterval: number;
    private inactivityDelay: number;
    private maximizeWindow: boolean;

    // VALIDATION

    private validateSave(): boolean {
        if (this.limitToAccount && (!this.limitToAccountDimId || this.limitAccountIds.length === 0)) {
            let keys: string[] = [
                "error.unabletosave_title",
                "time.time.timeterminal.nolimitedaccountsselected"
            ];
            this.translationService.translateMany(keys).then(terms => {
                this.notificationService.showDialogEx(terms["error.unabletosave_title"], terms["time.time.timeterminal.nolimitedaccountsselected"], SOEMessageBoxImage.Forbidden);
            });
            return false;
        }

        if (this.limitToCategories && this.categoryIds.length === 0) {
            let keys: string[] = [
                "error.unabletosave_title",
                "time.time.timeterminal.nolimitedcategoriesselected"
            ];
            this.translationService.translateMany(keys).then(terms => {
                this.notificationService.showDialogEx(terms["error.unabletosave_title"], terms["time.time.timeterminal.nolimitedcategoriesselected"], SOEMessageBoxImage.Forbidden);
            });
            return false;
        }

        return true;
    }

    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.terminal) {
                if (!this.terminal.name)
                    mandatoryFieldKeys.push("common.name");
            }
        });
    }
}