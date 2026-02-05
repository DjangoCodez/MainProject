import { ICoreService } from "../../../../Core/Services/CoreService";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { CoreUtility } from "../../../../Util/CoreUtility";
//import { ISoeGridOptions, SoeGridOptions } from "../../../../Util/SoeGridOptions";
import { SettingsUtility } from "../../../../Util/SettingsUtility";
import { IAccountingService } from "../AccountingService";
import { CalendarUtility } from "../../../../Util/CalendarUtility";
import { IFocusService } from "../../../../Core/Services/FocusService";
import { AccountingRowsController } from "../../../../Common/Directives/AccountingRows/AccountingRowsDirective";
import { VoucherEditSaveFunctions, IconLibrary, SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../../Util/Enumerations";
import { ToolBarUtility, ToolBarButton } from "../../../../Util/ToolBarUtility";
import { VoucherRowDTO } from "../../../../Common/Models/VoucherRowDTO";
import { AccountingRowDTO } from "../../../../Common/Models/AccountingRowDTO";
import { VoucherHeadDTO } from "../../../../Common/Models/VoucherHeadDTO";
import { TermGroup_AccountStatus, Feature, CompanySettingType, TermGroup_CurrencyType, SoeEntityType, SoeEntityImageType, ActionResultSave, UserSettingType, SettingMainType } from "../../../../Util/CommonEnumerations";
import { Constants } from "../../../../Util/Constants";
import { IShortCutService } from "../../../../Core/Services/ShortCutService";
import { EditControllerBase2 } from "../../../../Core/Controllers/EditControllerBase2";
import { ICompositionEditController } from "../../../../Core/ICompositionEditController";
import { IProgressHandlerFactory } from "../../../../Core/Handlers/progresshandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../../Core/Handlers/controllerflowhandlerfactory";
import { IValidationSummaryHandlerFactory } from "../../../../Core/Handlers/validationsummaryhandlerfactory";
import { IMessagingHandlerFactory } from "../../../../Core/Handlers/messaginghandlerfactory";
import { IDirtyHandlerFactory } from "../../../../Core/Handlers/DirtyHandlerFactory";
import { IToolbarFactory } from "../../../../Core/Handlers/ToolbarFactory";
import { IPermissionRetrievalResponse } from "../../../../Core/Handlers/ControllerFlowHandler";
import { FilesHelper } from "../../../../Common/Files/FilesHelper";
import { EmbeddedGridController } from "../../../../Core/Controllers/EmbeddedGridController";
import { IGridHandlerFactory } from "../../../../Core/Handlers/gridhandlerfactory";
import { IAccountPeriodDTO, IAccountYearDTO } from "../../../../Scripts/TypeLite.Net4";
import { IRequestReportService } from "../../../Reports/RequestReportService";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Config
    isTemplates = false;

    accountYearIsOpen = false;
    
    // Permissions
    modifyAccountPeriodPermission = false;
    reportPermission = false;

    // Company settings
    allowUnbalancedVoucher = false;
    allowEditVoucher = false;
    allowEditVoucherDate = false;
    showEnterpriseCurrency = false;
    defaultVoucherSeriesId = 0;
    defaultVoucherSeriesTypeId = 0;
    templateVoucherSeriesId = 0;

    // User Settings
    private keepNewVoucherAfterSave: boolean;

    // Subgrids
    private historyGridHandler: EmbeddedGridController;

    // Data
    voucher: VoucherHeadDTO;

    // Lookups 
    voucherSeries: any[];
    templates: any;
    accountYear: IAccountYearDTO;
    accountPeriod: IAccountPeriodDTO;

    //Flags
    isLoading = false;
    isSaving = false;
    loadingHistory = false;
    historyLoaded = false;
    updateTabOnLoad = false;

    // Household Properties
    private createHouseholdVoucher: boolean;
    private householdDate: Date;
    private householdAmount: number;
    private householdRowIds: number[];
    private householdInvoiceNbrs: string[];
    private householdVoucherSeriesId: number;
    private householdProductAccountId: number;
    private paymentFromTaxAgencyAccountId: number;
    private productId: number;

    // Properties

    private voucherHeadId: number;
    private sequenceNumber: number;
    private revertVatVoucherId: number = null;
    private showEditVoucherNrButton = false;
    public documentExpanderIsOpen = false;
    public historyExpanderIsOpen = false;

    private _isLocked = false;
    get isLocked() {
        // Existing vouchers can only be edited if company setting says so.
        // Vouchers in a closed or locked period cannot be edited.
        // Templates can always be edited.

        this._isLocked = !this.isNew && !this.allowEditVoucher;

        if (this.voucher) {
            if (this.voucher.status == TermGroup_AccountStatus.Closed || this.voucher.status == TermGroup_AccountStatus.Locked)
                this._isLocked = true;

            if (this.voucher.template)
                this._isLocked = false;
        }

        return this._isLocked;
    }

    private _isDateLocked = false;
    get isDateLocked() {
        // Existing vouchers can only be edited if company setting says so.
        // Vouchers in a closed or locked period cannot be edited.
        // Templates can always be edited.

        this._isDateLocked = !this.isNew && !this.allowEditVoucherDate

        if (this.voucher) {
            if (this.voucher.status == TermGroup_AccountStatus.Closed || this.voucher.status == TermGroup_AccountStatus.Locked)
                this._isDateLocked = true;

            if (this.voucher.template)
                this._isDateLocked = false;
        }

        return this._isDateLocked;
    }

    private voucherSeriesManuallyChangedId = 0;
    private _selectedVoucherSeries: any;
    get selectedVoucherSeries() {
        return this._selectedVoucherSeries;
    }
    set selectedVoucherSeries(item: any) {
        if (item) {
            this._selectedVoucherSeries = item;

            if (this.voucher) {
                if (this.voucherSeriesManuallyChangedId !== 0 || (item.voucherSeriesId !== this.defaultVoucherSeriesId && item.voucherSeriesId !== this.templateVoucherSeriesId))
                    this.voucherSeriesManuallyChangedId = item.voucherSeriesId;

                if (this.voucher.voucherSeriesId !== item.voucherSeriesId) {
                    this.voucher.voucherSeriesId = item.voucherSeriesId;
                    this.setSeqNbr();
                }

                if (this.isNew) {
                    if (!this.selectedDate)
                        this.selectedDate = (item && item.voucherDateLatest ? CalendarUtility.convertToDate(item.voucherDateLatest) : new Date());
                }
            }
        }
    }

    private _selectedDate: Date;
    get selectedDate() {
        return this._selectedDate;
    }
    set selectedDate(date: Date) {
        if (!date || !(date instanceof Date))
            return;

        const previousDate = this._selectedDate;
        this._selectedDate = date.date();

        if (this._selectedDate && !this.isLoading) {
            this.loadAccountYear(this._selectedDate).then((ok: boolean) => {
                if (!ok) {
                    this._selectedDate = previousDate;
                    return;
                }
                else if (!this.accountPeriod || !this.selectedDate.isWithinRange(this.accountPeriod.from, this.accountPeriod.to) ) {
                    this.loadAccountPeriod(this.accountYear.accountYearId);
                }
            })
        }

        if (previousDate && previousDate !== this._selectedDate) {
            this.setVoucherDateOnAccountingRows();
        }

        if (this.voucher)
            this.voucher.date = this._selectedDate;
    }

    private _selectedTemplate: any;
    get selectedTemplate() {
        return this._selectedTemplate;
    }
    set selectedTemplate(item: any) {
        this._selectedTemplate = item;
        this.loadTemplate();
    }

    private showNavigationButtons = true;
    private voucherIds: number[];

    public showInfoMessage = false;
    public infoMessage: string;
    public infoButtons: ToolBarButton[] = [];

    // Functions
    saveFunctions: any = [];

    private edit: ng.IFormController;
    private filesHelper: FilesHelper;

    private isPrinting: boolean = false;

    //@ngInject
    constructor(
        private $uibModal,
        private $timeout: ng.ITimeoutService,
        private coreService: ICoreService,
        private readonly requestReportService: IRequestReportService,
        private accountingService: IAccountingService,
        private translationService: ITranslationService,
        private messagingService: IMessagingService,
        private notificationService: INotificationService,
        private urlHelperService: IUrlHelperService,
        private focusService: IFocusService,
        private $window: ng.IWindowService,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        progressHandlerFactory: IProgressHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        shortCutService: IShortCutService) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        // Config parameters
        if (soeConfig.isTemplates)
            this.isTemplates = true;
        
        this.showEditVoucherNrButton = CoreUtility.isSupportSuperAdmin;

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onLoadData(() => this.loadData(false)) //this.doLookups())
            .onDoLookUp(() => this.doLookups()) //this.doLookups())
            .onSetUpGUI(() => this.setupGUI())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));

        shortCutService.bindSave($scope, () => { this.startSave(false, this.isTemplates); });
        if (!this.isTemplates) {
            shortCutService.bindPrint($scope, () => { this.startSave(true); });
        }

        this.historyGridHandler = new EmbeddedGridController(gridHandlerFactory, "historyGrid");
        this.historyGridHandler.gridAg.options.setMinRowsToShow(5)

        this.setupMessageListners();
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Economy_Accounting_Vouchers_Edit].readPermission;
        this.modifyPermission = response[Feature.Economy_Accounting_Vouchers_Edit].modifyPermission;
        this.modifyAccountPeriodPermission = response[Feature.Economy_Accounting_AccountPeriods].modifyPermission;
        this.reportPermission = response[Feature.Economy_Distribution_Reports_Selection].readPermission && response[Feature.Economy_Distribution_Reports_Selection_Download].readPermission;
    }

    // SETUP
    private accountingRowsDirective: AccountingRowsController;
    public registerAccountingRows(control: AccountingRowsController) {
        this.accountingRowsDirective = control;
    }

    public onInit(parameters: any) {
        this.voucherHeadId = parameters.id;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);
        this.filesHelper = new FilesHelper(this.coreService, this.$q, this.dirtyHandler, true, SoeEntityType.Voucher, SoeEntityImageType.Unknown, () => this.voucherHeadId);

        if (parameters.updateTab)
            this.updateTabOnLoad = true;

        if (parameters.createHousehold) {
            this.createHouseholdVoucher = true;
            this.productId = parameters.productId;
            this.householdDate = parameters.date;
            this.householdAmount = parameters.amount;
            this.householdRowIds = parameters.ids;
            this.householdInvoiceNbrs = parameters.nbrs;
            this.selectedDate = parameters.date;
        }
        else if (parameters.ids && parameters.ids.length > 0) {
            this.voucherIds = parameters.ids;
        }
        else {
            this.showNavigationButtons = false;
        }

        this.flowHandler.start([
            { feature: Feature.Economy_Accounting_Vouchers_Edit, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Economy_Accounting_AccountPeriods, loadModifyPermissions: true },
            { feature: Feature.Economy_Distribution_Reports_Selection, loadReadPermissions: true },
            { feature: Feature.Economy_Distribution_Reports_Selection_Download, loadReadPermissions: true }
        ]);
    }

    private setupMessageListners() {

        this.messagingService.subscribe(Constants.EVENT_ACCOUNTING_ROWS_READY, (guid) => {
            if (this.createHouseholdVoucher) {
                this.newHouseholdVoucher();
            }
        }, this.$scope);

        this.messagingService.subscribe(Constants.EVENT_SELECT_ACCOUNTDISTRIBUTION_DIALOG, (parentGuid) => {
            if (parentGuid == this.guid) {
                this.$scope.$broadcast('accountDistributionName', this.voucher.voucherNr + ", " + this.selectedVoucherSeries.voucherSeriesTypeName + ", " + this.voucher.date.toLocaleDateString());
            }
        }, this.$scope);

    }

    private doLookups() {
        if (this.createHouseholdVoucher) {
            return this.$q.all([
                this.loadCompanySettings(),
                this.loadUserSettings(),
                this.loadHouseholdProductAccountId(),
                this.loadDefaultSettingVoucherSeriesTypeId()
            ])
        }
        else {
            return this.$q.all([
                this.loadCompanySettings(),
                this.loadUserSettings(),
                this.loadDefaultSettingVoucherSeriesTypeId()
            ]);
        }
    }

    private setupGUI() {
        this.setupSubGrids();
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => this.copy(), () => this.isNew);

        // Invert
        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("economy.accounting.voucher.invert", "economy.accounting.voucher.invert", IconLibrary.FontAwesome, "fa-exchange", () => {
            this.invert();
        }, () => {
            return this.isNew;
        })));

        // Print
        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("", "economy.accounting.voucher.printaccountingorder", IconLibrary.FontAwesome, "fa-print", () => {
            this.printAccountingOrder(this.voucherHeadId);
        }, () => {
            return this.isNew || this.isPrinting;
        })));


        // Functions
        const keys: string[] = [
            "core.save",
            "economy.accounting.voucher.saveandprint",
            "economy.accounting.voucher.saveastemplate"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.saveFunctions.push({ id: VoucherEditSaveFunctions.Save, name: terms["core.save"] + " (Ctrl+S)" });
            if (!this.isTemplates) {
                this.saveFunctions.push({ id: VoucherEditSaveFunctions.SaveAndPrint, name: terms["economy.accounting.voucher.saveandprint"] + " (Ctrl+P)" });
                this.saveFunctions.push({ id: VoucherEditSaveFunctions.SaveAsTemplate, name: terms["economy.accounting.voucher.saveastemplate"] });
            }
        });

        //Navigation
        this.toolbar.setupNavigationGroup(null, () => { return this.isNew }, (voucherHeadId) => {
            this.voucherHeadId = voucherHeadId;
            this.loadData(true);
        }, this.voucherIds, this.voucherHeadId);
    }

    private setupSubGrids() {

        const keys: string[] = [
            "economy.accounting.voucher.historytype",
            "economy.accounting.voucher.historyfield",
            "economy.accounting.voucher.historychange",
            "economy.accounting.voucher.historydate",
            "economy.accounting.voucher.historytime",
            "common.user",
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.historyGridHandler.gridAg.addColumnText("eventType", terms['economy.accounting.voucher.historytype'], null);
            this.historyGridHandler.gridAg.addColumnText("fieldModified", terms['economy.accounting.voucher.historyfield'], null);
            this.historyGridHandler.gridAg.addColumnText("eventText", terms['economy.accounting.voucher.historychange'], null);
            this.historyGridHandler.gridAg.addColumnDate("dateTime", terms["economy.accounting.voucher.historydate"], null);
            this.historyGridHandler.gridAg.addColumnText("time", terms['economy.accounting.voucher.historytime'], null);
            this.historyGridHandler.gridAg.addColumnText("userName", terms['common.user'], null);

            this.historyGridHandler.gridAg.options.enableGridMenu = false;
            this.historyGridHandler.gridAg.finalizeInitGrid("economy.accounting.voucher.history", false);
        });
    }

    // LOOKUPS

    private setTabLabel(voucherNr: number) {
        const labelKey = this.isTemplates || (this.voucher && this.voucher.template) ? "economy.accounting.voucher.template" : "economy.accounting.voucher.voucher";
        this.translationService.translate(labelKey).then((term) => {
            this.messagingService.publish(Constants.EVENT_SET_TAB_LABEL, {
                guid: this.guid,
                label: term + " " + voucherNr,
                id: this.voucher.voucherHeadId,
            });
        });
    }

    private loadData(updateTab: boolean): ng.IPromise<any> {

        const deferral = this.$q.defer();
        if (this.voucherHeadId > 0) {
            this.isLoading = true;
            this.accountingService.getVoucher(this.voucherHeadId, false, true, true, false).then((x) => {
                this.voucher = x;
                this.isNew = false;
                this.selectedDate = new Date(<any>this.voucher.date);
                this.voucher.accountingRows = VoucherRowDTO.toAccountingRowDTOs(this.voucher.rows);

                if (this.filesHelper.filesLoaded) {
                    this.filesHelper.loadFiles(true);
                }

                if (updateTab || this.updateTabOnLoad) {
                    this.setTabLabel(this.voucher.voucherNr);
                    this.dirtyHandler.clean();
                    this.updateTabOnLoad = false;
                }

                this.loadAccountYear(this.selectedDate).then(() => {
                    this.setVoucherSeries(this.voucher.voucherSeriesId);
                    this.isLoading = false;
                    deferral.resolve();
                })
                
            });
        }
        else {
            if (!this.createHouseholdVoucher) {
                this.loadAccountYear(undefined, soeConfig.accountYearId).then(() => {
                    this.new(true);

                    deferral.resolve();
                });
            }
            else {
                this.isNew = true;
                this.voucherHeadId = 0;
                this.voucher = new VoucherHeadDTO();
                this.voucher.accountingRows = [];
                this.voucher.status = TermGroup_AccountStatus.Open;

                deferral.resolve();
            }
        }

        return deferral.promise;
    }

    private loadVoucherHistory() {
        if (this.voucherHeadId > 0 && !this.historyLoaded) {
            this.loadingHistory = true;
            this.accountingService.getVoucherRowHistory(this.voucherHeadId).then((x) => {
                this.historyGridHandler.gridAg.setData(x);
                this.loadingHistory = false;
                this.historyLoaded = true;
            });
        }
    }

    private loadCompanySettings(): ng.IPromise<any> {
        const settingTypes: number[] = [];

        settingTypes.push(CompanySettingType.AccountingAllowUnbalancedVoucher);
        settingTypes.push(CompanySettingType.AccountingAllowEditVoucher);
        settingTypes.push(CompanySettingType.AccountingAllowEditVoucherDate);
        settingTypes.push(CompanySettingType.AccountingShowEnterpriseCurrency);
        settingTypes.push(CompanySettingType.AccountCustomerPaymentFromTaxAgency);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.allowUnbalancedVoucher = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.AccountingAllowUnbalancedVoucher);
            this.allowEditVoucher = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.AccountingAllowEditVoucher);
            this.allowEditVoucherDate = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.AccountingAllowEditVoucherDate);
            this.showEnterpriseCurrency = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.AccountingShowEnterpriseCurrency);
            this.paymentFromTaxAgencyAccountId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCustomerPaymentFromTaxAgency);
        });
    }

    private loadUserSettings(): ng.IPromise<any> {
        const settingTypes: number[] = [UserSettingType.KeepNewVoucherAfterSave];

        return this.coreService.getUserSettings(settingTypes).then(x => {
            this.keepNewVoucherAfterSave = SettingsUtility.getBoolUserSetting(x, UserSettingType.KeepNewVoucherAfterSave, false);
        });
    }

    private loadDefaultVoucherSeriesId(accountYearId:number): ng.IPromise<any> {
        return this.accountingService.getDefaultVoucherSeriesId(accountYearId, CompanySettingType.AccountingVoucherSeriesTypeManual).then((voucherSerieId: number) => {
            this.defaultVoucherSeriesId = voucherSerieId;
        });
    }
    
    private loadAccountYear(voucherDate: Date,  accountYearId = 0 ): ng.IPromise<boolean> {
        const deferral = this.$q.defer<boolean>();
        
        const previousAccountYearId = this.accountYear ? this.accountYear.accountYearId : 0;

        const getYearPromise = accountYearId ? this.accountingService.getAccountYear(accountYearId, false, false) : this.accountingService.getAccountYearByDate(voucherDate);

       
        getYearPromise.then((year: IAccountYearDTO) => {
            if (!year) {
                this.translationService.translate("economy.accounting.voucher.missingaccountyearfordate").then((term) => 
                {
                    this.notificationService.showErrorDialog("", term, "");
                    deferral.resolve(false);
                });
                return;
            }
            this.accountYear = year;
            this.accountYearIsOpen = year.status === TermGroup_AccountStatus.Open;

            if (!this.accountYearIsOpen && this.isNew) {
                this.translationService.translate("economy.accounting.voucher.accountyearclosed").then((term) => {
                    this.notificationService.showErrorDialog("", term, "");
                    deferral.resolve(false);
                });
                return;
            }

            if (previousAccountYearId !== this.accountYear.accountYearId) {
                this.$q.all([
                    this.loadTemplates(this.accountYear.accountYearId),
                    this.loadVoucherSeries(this.accountYear.accountYearId),
                    this.loadDefaultVoucherSeriesId(this.accountYear.accountYearId),
                    this.loadAccountPeriod(this.accountYear.accountYearId, false),
                    this.loadHousholdVoucherSeries(this.accountYear.accountYearId)
                ]).then(() => {
                    this.setDefaultValuesFromVoucherSeries();
                    if (this.voucher && this.voucher.voucherSeriesTypeId) {
                        this.setVoucherSeriesByType(this.voucher.voucherSeriesTypeId);
                    }
                    else if (this.voucher)  {
                        this.setDefaultVoucherSerieId();
                    }

                    deferral.resolve(true);
                });
            }
            else if (voucherDate && this.accountPeriod && !voucherDate.isWithinRange(this.accountPeriod.from, this.accountPeriod.to) ) {
                this.loadAccountPeriod(this.accountYear.accountYearId, false).then(() => {
                    deferral.resolve(true);
                })
            }
            else {
                deferral.resolve(true);
            }
        });

        return deferral.promise;
    }

    private loadTemplates(accountYearId: number): ng.IPromise<any> {
        return this.accountingService.getVoucherTemplatesDict(accountYearId).then((x) => {
            this.templates = x;
        });
    }

    private loadDefaultSettingVoucherSeriesTypeId(): ng.IPromise<any> {
        const settingTypes: number[] = [UserSettingType.VoucherSeriesSelection];
        return this.coreService.getUserSettings(settingTypes).then(x => {
            this.defaultVoucherSeriesTypeId = SettingsUtility.getIntUserSetting(x, UserSettingType.VoucherSeriesSelection, 0, false);
        });
    }

    private loadVoucherSeries(accountYearId: number): ng.IPromise<any> {
        //Never use cache since latest or start number might have been updated else where
        return this.accountingService.getVoucherSeriesByYear(accountYearId, true, false).then((result) => {
            this.voucherSeries = result;
            const templateVoucherSerie: any[] = this.voucherSeries.filter(s => s.voucherSeriesTypeIsTemplate);

            if (templateVoucherSerie && templateVoucherSerie.length > 0)
                this.templateVoucherSeriesId = templateVoucherSerie[0].voucherSeriesId;

            if (!this.isTemplates) {
                this.voucherSeries = this.voucherSeries.filter(x => !x.voucherSeriesTypeIsTemplate);
            }
        });
    }

    private setDefaultValuesFromVoucherSeries(): void {  // Remember template VoucherSeriesId
        if (this.defaultVoucherSeriesTypeId) {
            const defaultType = this.voucherSeries.filter(x => x.voucherSeriesTypeId === this.defaultVoucherSeriesTypeId);
            if (defaultType && defaultType.length > 0) {
                this.defaultVoucherSeriesId = defaultType[0].voucherSeriesId;
            }
        }
    }

    private loadAccountPeriod(accountYearId: number, forceRefresh = false): ng.IPromise<any> {
         
        if (!this.selectedDate || accountYearId === 0 || !accountYearId) {
            this.accountPeriod = null;
            return;
        }

        return this.accountingService.getAccountPeriod(accountYearId, new Date(this.selectedDate.toString()), false, forceRefresh).then((x) => {
            this.accountPeriod = x;
            
            //Validate period is open
            if (this.accountPeriod.status !== TermGroup_AccountStatus.Open && this.isNew === true) {
                const keys: string[] = [
                    "core.warning",
                    "economy.accounting.voucher.periodnotopen",
                    "economy.accounting.voucher.periodnotopenmodify"
                ];
                this.translationService.translateMany(keys).then((terms) => {
                    if (this.modifyAccountPeriodPermission) {
                        const modal = this.notificationService.showDialog(terms["core.warning"], terms["economy.accounting.voucher.periodnotopenmodify"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                        modal.result.then(val => {
                            if (val) {
                                this.openAccountingPeriod();
                            }
                        });
                    }
                    else {
                        this.notificationService.showDialog(terms["core.warning"], terms["economy.accounting.voucher.periodnotopen"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
                    }
                });
            }
        });
    }

    private loadHouseholdProductAccountId(): ng.IPromise<any> {
        return this.accountingService.getHouseholdProductAccountId(this.productId).then((x) => {
            this.householdProductAccountId = x;
        });
    }

    private loadHousholdVoucherSeries(accountYearId: number): ng.IPromise<any> {
        if (accountYearId === 0 || !this.createHouseholdVoucher) {
            this.householdVoucherSeriesId = 0;
            return null;
        }
        return this.accountingService.getDefaultVoucherSeriesId(accountYearId, CompanySettingType.CustomerPaymentVoucherSeriesType).then((x) => {
            this.householdVoucherSeriesId = x;
        });
    }

    // ACTIONS
    private saveKeepVoucherSetting() {
        this.$timeout(() => {
            this.coreService.saveBoolSetting(SettingMainType.User, UserSettingType.KeepNewVoucherAfterSave, this.keepNewVoucherAfterSave);
        }, 100);
    }

    private openAccountingPeriod() {
        return this.accountingService.updateAccountPeriodStatus(this.accountPeriod.accountPeriodId, TermGroup_AccountStatus.Open).then((result) => {
            if (result.success)
                this.loadAccountPeriod(this.accountYear.accountYearId, true);
            else {
                const keys: string[] = [
                    "core.error",
                    "economy.accounting.voucher.openperioderrornext",
                    "economy.accounting.voucher.openperioderror"
                ];
                this.translationService.translateMany(keys).then((terms) => {
                    if (result.errorNumber === ActionResultSave.AccountPeriodNotOpen)
                        this.notificationService.showDialog(terms["core.error"], terms["economy.accounting.voucher.openperioderrornext"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
                    else
                        this.notificationService.showDialog(terms["core.error"], terms["economy.accounting.voucher.openperioderror"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);

                });
            }
        });
    }

    public startSave(print = false, saveAsTemplate = false) {
        this.$scope.$broadcast('stopEditing', { functionComplete: () => { this.save(print, saveAsTemplate) } });
    }

    public save(print = false, saveAsTemplate = false) {
        this.$timeout(() => {
            if (this.isSaving)
                return;

            this.showInfoMessage = false;
            this.infoMessage = "";
            this.infoButtons = [];

            this.isSaving = true;
            if (this['edit'].$invalid || !this.dirtyHandler.isDirty) {
                console.warn("Save called with invalid form");
                this.isSaving = false;
                return;
            }

            if (this.isNew) {
                this.voucher.status = TermGroup_AccountStatus.Open;
                this.voucher.accountPeriodId = this.accountPeriod ? this.accountPeriod.accountPeriodId : 0;
            }

            const files = this.filesHelper.getAsDTOs();

            let updateTab = false;
            if (saveAsTemplate || this.isTemplates) {
                if (files.length > 0) {
                    this.translationService.translate("economy.accounting.voucher.templateswithattachments").then((term) => {
                        this.notificationService.showDialog(term["core.error"], term, SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
                    });
                    this.isSaving = false;
                    return;
                }
                if (!this.templateVoucherSeriesId) {
                    this.translationService.translate("economy.accounting.voucher.missingtemplatevoucherserie").then((term) => {
                        this.notificationService.showDialog(term["core.error"], term, SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
                    });
                    this.isSaving = false;
                    return;
                }
                //saving a existing voucher as a template?
                if (!this.voucher.template && this.voucher.voucherHeadId) {
                    this.voucher.voucherHeadId = 0;
                    updateTab = true;
                }
                this.voucher.template = true;
                this.voucher.voucherSeriesId = this.templateVoucherSeriesId;
            }

            // Clear empty amounts
            this.voucher.accountingRows.forEach(r => {
                if (!r.debitAmount)
                    r.setDebitAmount(TermGroup_CurrencyType.BaseCurrency, 0);
                if (!r.debitAmountEntCurrency)
                    r.setDebitAmount(TermGroup_CurrencyType.EnterpriseCurrency, 0);
                if (!r.debitAmountLedgerCurrency)
                    r.setDebitAmount(TermGroup_CurrencyType.LedgerCurrency, 0);
                if (!r.debitAmountCurrency)
                    r.setDebitAmount(TermGroup_CurrencyType.TransactionCurrency, 0);
                if (!r.creditAmount)
                    r.setCreditAmount(TermGroup_CurrencyType.BaseCurrency, 0);
                if (!r.creditAmountEntCurrency)
                    r.setCreditAmount(TermGroup_CurrencyType.EnterpriseCurrency, 0);
                if (!r.creditAmountLedgerCurrency)
                    r.setCreditAmount(TermGroup_CurrencyType.LedgerCurrency, 0);
                if (!r.creditAmountCurrency)
                    r.setCreditAmount(TermGroup_CurrencyType.TransactionCurrency, 0);
            });

            const accountYearId = this.accountYear.accountYearId;

            this.progress.startSaveProgress((completion) => {
                this.accountingService.saveVoucher(this.voucher, this.voucher.accountingRows, this.householdRowIds ? this.householdRowIds : null, files, this.revertVatVoucherId).then((result) => {
                    if (result.success) {
                        const newVoucherHeadId = result.integerValue;

                        if (newVoucherHeadId && newVoucherHeadId > 0)
                            this.voucherHeadId = newVoucherHeadId;

                        this.sequenceNumber = Number(result.value);
                        const keys: string[] = [];
                        keys.push(this.isTemplates ? "economy.accounting.voucher.templatesavedmessage" : "economy.accounting.voucher.savedmessage");
                        keys.push("economy.accounting.voucher.seqnbrchanged");
                        this.translationService.translateMany(keys).then(terms => {

                            this.accountingService.calculateAccountBalanceForAccountsFromVoucher(accountYearId).then(() => {
                                if (this.accountingRowsDirective)
                                    this.accountingRowsDirective.reloadAccountBalances();
                            });

                            // If a new template was saved, update the Template list
                            if (this.voucher.template)
                                this.loadTemplates(accountYearId);

                            if (print)
                                this.printAccountingOrder(newVoucherHeadId);
                        });
                        if (this.filesHelper.filesLoaded) {
                            this.documentExpanderIsOpen = false;
                            if (!this.isNew)
                                this.filesHelper.loadFiles(true);
                        }
                        completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.voucher);
                        this.isSaving = false;

                        // Add button
                        this.infoButtons.push(new ToolBarButton(null, "economy.accounting.voucher.askprint", IconLibrary.FontAwesome, "fa-print", () => {
                                this.printAccountingOrder(newVoucherHeadId);
                            }, 
                            () => {
                                return this.isPrinting;
                            }, null
                        ));
                    } else {
                        completion.failed(result.errorMessage);
                        this.isSaving = false;
                    }
                }, error => {
                    completion.failed(error.message);
                    this.isSaving = false;
                });
            }, this.guid)
                .then(data => {
                    this.dirtyHandler.clean();

                    if (this.isNew) {
                        this.translationService.translate("economy.accounting.voucher.vouchercreated").then((term) => {
                            this.infoMessage = term.format(this.sequenceNumber.toString());
                        });
                        this.showInfoMessage = !this.voucher.template;

                        if (this.keepNewVoucherAfterSave) {
                            this.loadVoucherSeries(accountYearId).then(() => {
                                this.loadData(true);
                            });
                        }
                        else {
                            // Reload voucher series to get latest number and date
                            this.loadVoucherSeries(accountYearId).then(() => {
                                this.new(true, !this.voucher.template);
                            });
                        }
                    }
                    else {
                        this.translationService.translate("economy.accounting.voucher.voucherupdated").then((term) => {
                            this.infoMessage = term.format(this.voucher.voucherNr.toString());
                        });
                        this.showInfoMessage = !this.voucher.template;

                        this.loadData(updateTab);

                        if (this.historyExpanderIsOpen) {
                            this.historyLoaded = false;
                            this.loadVoucherHistory();
                        }
                    }
                }, error => {

                });
        });
    }

    protected initDelete() {
        // Decided in sprint 81-1 that the general warning dialog is sufficent
        this.delete();
        // Show verification dialog
        /*var keys: string[] = [
            "core.warning",
            "economy.accounting.voucher.deletetemplatewarning",
            "economy.accounting.voucher.deletevoucherwarning"
        ];
        this.translationService.translateMany(keys).then((terms) => {
            var message;
            if (this.voucher.template) {
                message = terms["economy.accounting.voucher.deletetemplatewarning"];
            } else {
                message = terms["economy.accounting.voucher.deletevoucherwarning"];
            }

            var modal = this.notificationService.showDialog(terms["core.warning"], message, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
            modal.result.then(val => {
                if (val) {
                    this.delete();
                }
            });
        });*/
    }

    public delete() {
        if (this.voucher.template) {
            this.progress.startDeleteProgress((completion) => {
                this.accountingService.deleteVoucher(this.voucher.voucherHeadId).then((result) => {
                    if (result.success) {
                        completion.completed(this.voucher);
                        // Reload template list
                        if (this.voucher.template)
                            this.loadTemplates(this.voucher.accountYearId);

                        this.new(true);
                    } else {
                        completion.failed(result.errorMessage);
                    }
                }, error => {
                    completion.failed(error.message);
                });
            }).then(x => {
                super.closeMe(false);
            });
        }
        else {
            this.progress.startDeleteProgress((completion) => {
                this.accountingService.deleteVouchersOnlySuperSupport([this.voucher.voucherHeadId]).then((result) => {
                    if (result.success) {
                        completion.completed(this.voucher);
                        this.new(true);
                    } else {
                        completion.failed(result.errorMessage);
                    }
                }, error => {
                    completion.failed(error.message);
                });
            }).then(x => {
                super.closeMe(false);
            });
        }
    }

    private setVoucherDateOnAccountingRows() {
        if (!this.voucher?.accountingRows?.length) return;
        this.voucher.accountingRows.forEach((row) => {
            row.date = this.selectedDate ?? new Date().date();
            if (!row.parentRowId)
                this.$scope.$broadcast('checkAccountDistribution', row, 1);
        });
    }

    public copy() {
        this.revertVatVoucherId = null;

        // Clear all fields but the rows and the voucher series
        this.new(false, false, true);
        const seriesId = this.voucher.voucherSeriesId;
        this.setVoucherSeries(seriesId);
        this.selectedDate = this.voucher.date;

        this.dirtyHandler.setDirty();

        // Clear the IDs of all rows
        AccountingRowDTO.clearRowIds(this.voucher.accountingRows, true);

        // Set tab name to "New voucher"
        this.messagingService.publish(Constants.EVENT_EDIT_NEW, {
            guid: this.guid,
        });
    }

    private invert(voucherSeriesTypeId?: number) {
        if (this.voucher.vatVoucher) {
            this.revertVatVoucherId = this.voucher.voucherHeadId;
            this.voucher.vatVoucher = false;
        }

        // Remember head information
        var seriesId = this.voucher.voucherSeriesId;
        var date = this.voucher.date;
        var text = this.voucher.text;

        // Clear all fields but the rows
        this.new(false);
        if (voucherSeriesTypeId)
            this.setVoucherSeriesByType(voucherSeriesTypeId);
        else
            this.setVoucherSeries(seriesId);
        this.selectedDate = date;
        this.voucher.text = text;

        // Clear the IDs of all rows
        AccountingRowDTO.clearRowIds(this.voucher.accountingRows, true);
        // Swap debit and credit
        AccountingRowDTO.invertAmounts(this.voucher.accountingRows);

        // Set tab name to "New voucher"
        this.messagingService.publish(Constants.EVENT_EDIT_NEW, {
            guid: this.guid
        });
        this.dirtyHandler.setDirty();

        this.updateAccountingRowsGrid();
    }

    private printAccountingOrder(voucherHeadId: number): void {
        this.isPrinting = true;
        this.requestReportService
        .printVoucher(voucherHeadId)
        .then(() => {
            this.isPrinting = false;
        });
    }

    private loadTemplate() {
        if (this.selectedTemplate && this.voucher) {
            this.accountingService.getVoucher(this.selectedTemplate, false, true, true, false).then((template:VoucherHeadDTO) => {
                var counter: number = 1;

                this.voucher.vatVoucher = template.vatVoucher; 
                if (!this.voucher.note)
                    this.voucher.note = template.note;
                if (!this.voucher.text)
                    this.voucher.text = template.text;

                this.voucher.accountingRows = [];
                _.forEach(template.rows, (row: VoucherRowDTO) => {
                    // Convert to AccountingRowDTO
                    var accRow: AccountingRowDTO = VoucherRowDTO.toAccountingRowDTO(row);
                    // Clear the IDs
                    accRow.clearRowIds(false);

                    // Set row nr
                    if (!accRow.rowNr) {
                        accRow.rowNr = counter;
                        accRow.tempRowId = counter - 1;
                        counter++;
                    }

                    // Add to collection
                    this.voucher.accountingRows.push(accRow);

                });
                this.updateAccountingRowsGrid(true);
            });
        }
    }

    private updateAccountingRowsGrid(setRowItemAccountsOnAllRows = false) {
        this.$timeout(() => {
            this.$scope.$broadcast('rowsAdded', { setRowItemAccountsOnAllRows: setRowItemAccountsOnAllRows});
        });
    }

    private executeSaveFunction(option) {
        switch (option.id) {
            case VoucherEditSaveFunctions.Save:
                this.startSave();
                break;
            case VoucherEditSaveFunctions.SaveAndPrint:
                this.startSave(true);
                break;
            case VoucherEditSaveFunctions.SaveAsTemplate:
                this.startSave(false,true);
                break;
        }
    }

    private askEditVoucherNr() {
        // Show verification dialog
        var result: any;
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getViewUrl("editVoucherNrDialog.html"),
            controller: EditVoucherNrDialogController,
            controllerAs: "ctrl",
            size: 'md',
            resolve: {
                newVoucherNr: () => { return this.voucher.voucherNr },
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result && result.newVoucherNr && Number(result.newVoucherNr)) {
                this.editVoucherNr(Number(result.newVoucherNr));
            }
        });
    }

    private editVoucherNr(newVoucherNr: number) {
        this.progress.startSaveProgress((completion) => {
            this.accountingService.editVoucherNrOnlySuperSupport(this.voucherHeadId, newVoucherNr).then((result) => {
                if (result.success) {
                    this.translationService.translate("economy.accounting.voucher.vouchernredited").then((term) => {
                        completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.voucher, false, term.format(newVoucherNr.toString()));
                    });
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, null);
    }

    // HELP-METHODS

    private setDefaultVoucherSerieId() {
        const voucherSeridesId = this.isTemplates ? this.templateVoucherSeriesId : this.defaultVoucherSeriesId;
        this.setVoucherSeries(voucherSeridesId);
    }

    private new(clearRows: boolean, keepVoucherSeriesId = false, keepText = false) {
        this.isNew = true;
        this.voucherHeadId = 0;
        let tempVoucherSeriesId = 0;
        let tempText = '';

        let tempRows: AccountingRowDTO[];
        if (!clearRows)
            tempRows = this.voucher.accountingRows;

        if (keepVoucherSeriesId)
            tempVoucherSeriesId = this.voucher.voucherSeriesId;

        if (keepText)
            tempText = this.voucher.text;

        this.voucher = new VoucherHeadDTO();
        this.voucher.status = TermGroup_AccountStatus.Open;
        if (this.selectedDate) {
            this.voucher.date = this.selectedDate;
        }

        if (!clearRows)
            this.voucher.accountingRows = tempRows;
        else
            this.voucher.accountingRows = [];

        if (keepText)
            this.voucher.text = tempText;

        this.selectedTemplate = null;

        if (!this.createHouseholdVoucher) {
            if (keepVoucherSeriesId) {
                this.voucher.voucherSeriesId = tempVoucherSeriesId;
                this.setVoucherSeries(this.voucher.voucherSeriesId);
                this.setSeqNbr();
            }
            else {
                this.setDefaultVoucherSerieId();
            }
            
            this.messagingService.publish(Constants.EVENT_EDIT_NEW, {
                guid: this.guid
            });
        }

        this.filesHelper.reset();
        this.$timeout(() => this.focusService.focusByName("ctrl_selectedDate"), 300);
    }

    private newHouseholdVoucher() {
        const keys: string[] = [
            "economy.import.payment.invoicenr",
            "economy.accounting.voucher.householdvouchertext",
            "common.customer.invoices.invoicenr"
        ];

        this.voucher.voucherSeriesId = this.householdVoucherSeriesId;
        if (this.householdDate)
            this.selectedDate = this.householdDate;

        this.setVoucherSeries(this.voucher.voucherSeriesId);

        this.translationService.translateMany(keys).then((terms) => {
            this.voucher.text = terms["economy.accounting.voucher.householdvouchertext"];
            if (this.householdInvoiceNbrs.length > 0) {
                this.voucher.text += ". " + terms["common.customer.invoices.invoicenr"] + ": ";
                var first: boolean = true;
                _.forEach(this.householdInvoiceNbrs, (x) => {
                    if (first) {
                        this.voucher.text += x;
                        first = false;
                    }
                    else {
                        this.voucher.text += ", " + x;
                    }
                });
            }
            this.createHouseholdVoucher = false;
        });

        this.accountingRowsDirective.createDefaultAccountingRow(this.householdProductAccountId, this.householdAmount, false);
        this.accountingRowsDirective.createDefaultAccountingRow(this.paymentFromTaxAgencyAccountId, this.householdAmount, true);

        this.dirtyHandler.setDirty();
    }

    private getVoucherSeries(voucherSeriesId: number): any {
        return _.find(this.voucherSeries, { voucherSeriesId: voucherSeriesId });
    }

    private setVoucherSeries(voucherSeriesId: number) {
        if (this.voucherSeries && this.voucherSeries.length > 0) {
            this.selectedVoucherSeries = this.getVoucherSeries(voucherSeriesId);
        }
    }

    private getVoucherSeriesByType(voucherSeriesTypeId: number): any {
        return this.voucherSeries.find( x=> x.voucherSeriesTypeId === voucherSeriesTypeId);
    }

    private setVoucherSeriesByType(voucherSeriesTypeId: number) {
        if (this.voucherSeries && this.voucherSeries.length > 0) {
            this.selectedVoucherSeries = this.getVoucherSeriesByType(voucherSeriesTypeId);
        }
    }

    private setSeqNbr() {
        // Get next sequence number for selected voucher series
        const series = this.selectedVoucherSeries;
        this.voucher.voucherNr = (series && series.voucherNrLatest ? series.voucherNrLatest : 0) + 1;
    }

    private showAccountingOrder() {
        // this.buttonPrintAccountingOrder.Visibility = Converters.BooleanToVisibility(reportPermission && this.accountingOrderReportId > 0);
    }

    private showDeleteButton(): boolean {
        return this.modifyPermission && !this.isNew && (this.voucher.template || CoreUtility.isSupportSuperAdmin);
    }

    public showValidationError() {
        const errors = this['edit'].$error;
        
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            // Mandatory fields
            if (this.voucher) {
                if (!this.voucher.voucherSeriesId || !this.selectedVoucherSeries)
                    mandatoryFieldKeys.push("economy.accounting.voucher.voucherseries");
                if (!this.voucher.voucherNr)
                    mandatoryFieldKeys.push("economy.accounting.voucher.vouchernr");
                if (!this.voucher.date)
                    mandatoryFieldKeys.push("common.date");
            }

            // Account year
            if (errors['accountYearStatus'])
                validationErrorKeys.push("economy.accounting.voucher.accountyearclosed");

            // Account period
            if (errors['accountPeriod'])
                validationErrorKeys.push("economy.accounting.voucher.accountperiodmissing");
            if (errors['accountPeriodStatus'])
                validationErrorKeys.push("economy.accounting.voucher.accountperiodclosed");

            // Voucher series
            if (errors['defaultVoucherSeries'])
                validationErrorKeys.push("economy.accounting.voucher.defaultvoucherseriesmissing");

            // Row validation
            if (errors['accountStandard'])
                validationErrorKeys.push("economy.accounting.voucher.accountstandardmissing");
            if (errors['accountInternal'])
                validationErrorKeys.push("economy.accounting.voucher.accountinternalmissing");
            if (errors['rowAmount'])
                validationErrorKeys.push("economy.accounting.voucher.invalidrowamount");
            if (errors['amountDiff'])
                validationErrorKeys.push("economy.accounting.voucher.unbalancedrows");
        });
    }

    public isDisabled() {
        return !this.dirtyHandler.isDirty || this.edit.$invalid;
    }

    public addVoucherRowIfEmptyAndFocusIt() {
        if (this.voucher.accountingRows && this.voucher.accountingRows.length === 0) {
            var row = this.accountingRowsDirective.addRow();
        }

        this.$scope.$broadcast('focusRow', { row: 1 });
    }

    public onEditCompletedForBalancedAccount() {
        this.$timeout(() => {
            this.focusService.focusByName("btnSave");
        });
    }
}

export class EditVoucherNrDialogController {

    public result: any = {};
    private newVoucherNr: number;

    constructor(private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance, newVoucherNr: number) {
        this.newVoucherNr = newVoucherNr;
    }

    public cancel() {
        this.result.newVoucherNr = null;
        this.$uibModalInstance.close();
    }

    public ok() {
        this.result.newVoucherNr = this.newVoucherNr;
        this.$uibModalInstance.close(this.result);
    }
}
