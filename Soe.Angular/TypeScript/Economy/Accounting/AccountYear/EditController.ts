import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { IValidationSummaryHandler } from "../../../Core/Handlers/ValidationSummaryHandler";
import { IMessagingHandler } from "../../../Core/Handlers/MessagingHandler";
import { IEditControllerFlowHandler, IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IProgressHandler } from "../../../Core/Handlers/ProgressHandler";
import { IToolbar } from "../../../Core/Handlers/Toolbar";
import { IDirtyHandler } from "../../../Core/Handlers/DirtyHandler";
import { Guid } from "../../../Util/StringUtility";
import { IAccountingService } from "../../../Shared/Economy/Accounting/AccountingService";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { Constants } from "../../../Util/Constants";
import { Feature, TermGroup, TermGroup_AccountStatus } from "../../../Util/CommonEnumerations";
import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { ISoeGridOptionsAg, SoeGridOptionsAg, TypeAheadOptionsAg } from "../../../Util/SoeGridOptionsAg";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { SoeGridOptionsEvent, SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../Util/Enumerations";
import { VoucherSeriesDTO } from "../../../Common/Models/VoucherSeriesDTO";
import { AccountYearDTO, AccountPeriodDTO } from "../../../Common/Models/AccountYear";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { HtmlUtility } from "../../../Util/HtmlUtility";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Params
    private latestTo: Date;
    private accountYearId: number;

    // Lookups
    terms: { [index: string]: string; };
    accountStatuses: any = [];
    budgetSubTypes: any = [];
    voucherSeriesTypes: any = [];
    filteredVoucherSeriesTypes: any = [];
    voucherSeries: VoucherSeriesDTO[];
    accountYears: AccountYearDTO[];

    // Permissions
    grossProfitCodesPermission;
    voucherTemplatesPermission;

    // Entity
    private accountYear: AccountYearDTO;

    // Sub grids
    private periodsGridOptions: ISoeGridOptionsAg;
    private voucherSeriesGridOptions: ISoeGridOptionsAg;
    private voucherTemplatesGridOptions: ISoeGridOptionsAg;
    private grossProfitCodesGridOptions: ISoeGridOptionsAg;

    private selectedPeriodStatus = 1;
    private selectedVoucherSeriesType = undefined;
    private previousStatus = undefined;
    private previouseAccountYearFrom = null;
    private previouseAccountYearTo = null;

    // Flags
    private enableChangePeriods = true;
    private copyVoucherSeries = false;
    private keepNumberSeries = false;
    private isAccountYearOpen = true;
    private hasShorteningOverlap = false;
    private hasExtendingOverlap = false;
    private hasPeriodNotStarted = false;
    get enableDeleteYear() {
        return this.accountYear && this.accountYear.status < 2 && (this.accountYear.periods && this.accountYear.periods.length ? !_.some(this.accountYear.periods, (p) => p.status > 1) : true);
    }

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private $timeout: ng.ITimeoutService,
        protected $window: ng.IWindowService,
        private accountingService: IAccountingService,
        private coreService: ICoreService,
        urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        private translationService: ITranslationService,
        private controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory,
        protected notificationService: INotificationService,
    ) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.deleteButtonTemplateUrl = urlHelperService.getCoreComponent("deleteButtonComposition.html");
        this.saveButtonTemplateUrl = urlHelperService.getCoreComponent("saveButtonComposition.html");

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onDoLookUp(() => this.onDoLookups())
            .onLoadData(() => this.onLoadData());
    }

    public onInit(parameters: any) {
        this.accountYearId = parameters.id;
        if (parameters.latestTo)
            this.latestTo = new Date(parameters.latestTo);
        this.guid = parameters.guid;

        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([
            { feature: Feature.Economy_Accounting_AccountPeriods, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Economy_Preferences_VoucherSettings_GrossProfitCodes_Edit, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Economy_Accounting_Vouchers_Edit, loadReadPermissions: true, loadModifyPermissions: true }
        ]);

        this.periodsGridOptions = new SoeGridOptionsAg("Economy.Accounting.AccountYear.Periods", this.$timeout);
        this.voucherSeriesGridOptions = new SoeGridOptionsAg("Common.Customer.AccountYear.VoucherSeries", this.$timeout);
        this.voucherTemplatesGridOptions = new SoeGridOptionsAg("Economy.Accounting.AccountYear.VoucherTemplates", this.$timeout);
        this.grossProfitCodesGridOptions = new SoeGridOptionsAg("Economy.Accounting.AccountYear.GrossProfitCodes", this.$timeout);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Economy_Accounting_AccountPeriods].readPermission;
        this.modifyPermission = response[Feature.Economy_Accounting_AccountPeriods].modifyPermission;
        this.grossProfitCodesPermission = response[Feature.Economy_Preferences_VoucherSettings_GrossProfitCodes_Edit].modifyPermission;
        this.voucherTemplatesPermission = response[Feature.Economy_Accounting_Vouchers_Edit].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => this.copy(), () => this.isNew);
    }

    private onDoLookups(): ng.IPromise<any> {
        if (this.accountYearId && this.accountYearId > 0) {
            this.isNew = false;
            return this.progress.startLoadingProgress([
                () => this.loadTerms(),
                () => this.loadAccountYears(),
                () => this.loadAccountStatuses(),
                () => this.loadVoucherSeriesTypes(),
                () => this.loadBudgetSubTypes(),
                () => this.loadVoucherSeriesMapping(),
                () => this.loadVoucherTemplates(),
                () => this.loadGrossProfitCodes(),
            ]).then(() => {
                _.forEach(this.voucherSeries, (s) => {
                    var type = _.find(this.voucherSeriesTypes, (t) => t.voucherSeriesTypeId === s.voucherSeriesTypeId);
                    if (type) {
                        s["startNr"] = type.startNr;
                        if (s.voucherNrLatest < type.startNr)
                            s.voucherNrLatest = undefined;
                        s["voucherSeriesTypeNr"] = type.voucherSeriesTypeNr;
                    }

                    if (s.voucherDateLatest)
                        s.voucherDateLatest = CalendarUtility.convertToDate(s.voucherDateLatest);
                });

                this.setupSubGrids();
            });
        }
        else {
            this.isNew = true;
            return this.progress.startLoadingProgress([
                () => this.loadTerms(),
                () => this.loadAccountYears(),
                () => this.loadAccountStatuses(),
                () => this.loadVoucherSeriesTypes(),
                () => this.loadBudgetSubTypes(),
            ]).then(() => {
                this.new();
                this.setupSubGrids();
            });
        }
    }

    accountYearStatusChanged(item) {
        this.isAccountYearOpen = (item === TermGroup_AccountStatus.Open);
    }

    private onLoadData(updateCaption = false): ng.IPromise<any> {
        if (!this.accountYearId || this.accountYearId === 0)
            return;

        return this.accountingService.getAccountYear(this.accountYearId, true, false).then((x) => {
            this.accountYear = x;

            this.accountYear.from = CalendarUtility.convertToDate(this.accountYear.from);
            this.accountYear.to = CalendarUtility.convertToDate(this.accountYear.to);
            this.previouseAccountYearFrom = CalendarUtility.convertToDate(this.accountYear.from);
            this.previouseAccountYearTo = CalendarUtility.convertToDate(this.accountYear.to);
            this.enableChangePeriods = (this.accountYear.status === TermGroup_AccountStatus.New);
            this.isAccountYearOpen = (this.accountYear.status === TermGroup_AccountStatus.Open);
            _.forEach(this.accountYear.periods, (p) => {
                p.from = CalendarUtility.convertToDate(p.from);
                this.setPeriodValues(p);

                if (p.status > TermGroup_AccountStatus.New)
                    this.enableChangePeriods = false;
            });

            this.periodsGridOptions.setData(this.accountYear.periods);
            this.voucherSeriesGridOptions.setData(this.voucherSeries);

            this.filterVoucherSeriesTypes();

            this.previousStatus = this.accountYear.status;

            if (updateCaption)
                this.updateTabCaption();
        });
    }

    private setPeriodValues(item) {
        const status = _.find(this.accountStatuses, o => o.id === item.status);
        if (status)
            item['statusName'] = status.name;

        const monthName = _.find(this.budgetSubTypes, t => t.id === (item.from.getMonth() + 1));
        if (monthName)
            item['monthName'] = monthName.name;

        item["periodName"] = item.from.getFullYear().toString() + "-" + (item.from.getMonth() + 1).toString();
        item['statusIcon'] = this.getStatusIcon(item.status);
    }

    private getStatusIcon(status: number): string {
        switch (status) {
            case TermGroup_AccountStatus.New:
                return "fas fa-circle";
            case TermGroup_AccountStatus.Open:
                return "fas fa-circle okColor";
            case TermGroup_AccountStatus.Closed:
                return "fas fa-circle warningColor";
            case TermGroup_AccountStatus.Locked:
                return "fas fa-circle errorColor";
            default:
                return "";
        }
    }

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "core.warning",
            "common.name",
            "common.number",
            "common.period",
            "common.status",
            "core.time.month",
            "economy.accounting.voucherseriestype.voucherseriestypenr",
            "economy.accounting.accountyear.startnumber",
            "economy.accounting.accountyear.lastnumber",
            "economy.accounting.accountyear.lastvoucherdate",
            "economy.accounting.accountyear.accountyear",
            "economy.accounting.accountyear.newaccountyear",
            "economy.accounting.accountyear.changestatusinvalidsingle",
            "economy.accounting.accountyear.changestatusinvalidmultiple",
            "common.date",
            "common.text",
            "economy.accounting.voucher.voucherseries",
            "economy.accounting.voucher.vatvoucher",
            "economy.accounting.voucher.sourcetype",
            "economy.accounting.voucher.vouchermodified",
            "common.code",
            "economy.accounting.grossprofitcode.accountyear",
            "common.description",
            "economy.accounting.accountyear.changestatusinvalidvouchersingle",
            "economy.accounting.accountyear.changestatusinvalidvouchermultiple",
            "common.error.extending.year",
            "common.error.shortening.year",
            "common.error.remove.account.year.period"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private loadAccountStatuses(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.AccountStatus, false, false, true).then(x => {
            this.accountStatuses = x;
        });
    }

    private loadVoucherSeriesTypes(): ng.IPromise<any> {
        return this.accountingService.getVoucherSeriesTypes(false).then((x) => {
            this.voucherSeriesTypes = x;
        });
    }

    private loadAccountYears(): ng.IPromise<any> {
        this.accountYears = [];
        return this.accountingService.getAccountYears(false, false, false).then((x) => {

            this.accountYears = x.map(o => {
                let obj = new AccountYearDTO();
                angular.extend(obj, o);
                obj.fixDates();
                return obj;
            });

            this.accountYears = _.orderBy(this.accountYears, (s) => s.from);
        });
    }

    private loadVoucherSeriesMapping(resetData: boolean = false) {
        return this.accountingService.getVoucherSeriesByYear(this.accountYearId, false, false).then((x) => {
            this.voucherSeries = x;
            if (resetData) {
                _.forEach(this.voucherSeries, (s) => {
                    var type = _.find(this.voucherSeriesTypes, (t) => t.voucherSeriesTypeId === s.voucherSeriesTypeId);
                    if (type) {
                        s["startNr"] = type.startNr;
                        if (s.voucherNrLatest < type.startNr)
                            s.voucherNrLatest = undefined;
                        s["voucherSeriesTypeNr"] = type.voucherSeriesTypeNr;
                    }

                    if (s.voucherDateLatest)
                        s.voucherDateLatest = CalendarUtility.convertToDate(s.voucherDateLatest);
                });

                this.voucherSeriesGridOptions.setData(this.voucherSeries);
            }
        });
    }

    private loadVoucherTemplates(): ng.IPromise<any> {
        if (!this.accountYearId || this.accountYearId === 0)
            return;

        return this.accountingService.getVoucherTemplates(this.accountYearId).then((x) => {
            this.voucherTemplatesGridOptions.setData(x);
        });
    }

    private loadGrossProfitCodes(): ng.IPromise<any> {
        if (!this.accountYearId || this.accountYearId === 0)
            return;

        return this.accountingService.getGrossProfitCodesForAccountYear(this.accountYearId).then((x) => {
            _.forEach(x, (y) => {
                y['accountYearId'] = y['accountDateFrom'].slice(0, 10) + " - " + y['accountDateTo'].slice(0, 10);
            });
            return x;
        }).then(data => {
            this.grossProfitCodesGridOptions.setData(data);
        });
    }

    // Used for month name
    private loadBudgetSubTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.AccountingBudgetSubType, false, false, true).then(x => {
            this.budgetSubTypes = x;
        });
    }

    private setupSubGrids() {
        this.periodsGridOptions.enableGridMenu = false;
        this.periodsGridOptions.setMinRowsToShow(15);

        this.periodsGridOptions.addColumnIsModified("isModified", "", 36);
        this.periodsGridOptions.addColumnNumber("periodNr", this.terms["common.number"], 40, { enableHiding: true, pinned: "left" });
        this.periodsGridOptions.addColumnText("periodName", this.terms["common.period"], null, { enableHiding: true });
        this.periodsGridOptions.addColumnText("monthName", this.terms["core.time.month"], null, { enableHiding: true });
        this.periodsGridOptions.addColumnText("statusName", this.terms["common.status"], null, { enableHiding: true });
        this.periodsGridOptions.addColumnIcon("statusIcon", null, 30, { enableHiding: true, toolTipField: "statusName", showTooltipFieldInFilter: true, pinned: 'right' });

        this.periodsGridOptions.finalizeInitGrid();

        this.voucherSeriesGridOptions.enableGridMenu = false;
        this.voucherSeriesGridOptions.setMinRowsToShow(10);
        this.voucherSeriesGridOptions.enableRowSelection = false;

        this.voucherSeriesGridOptions.addColumnIsModified("isModified", "", 36);
        this.voucherSeriesGridOptions.addColumnNumber("voucherSeriesTypeNr", this.terms["economy.accounting.voucherseriestype.voucherseriestypenr"], 50);
        this.voucherSeriesGridOptions.addColumnText("voucherSeriesTypeName", this.terms["common.name"], null);
        this.voucherSeriesGridOptions.addColumnNumber("startNr", this.terms["economy.accounting.accountyear.startnumber"], null);
        this.voucherSeriesGridOptions.addColumnNumber("voucherNrLatest", this.terms["economy.accounting.accountyear.lastnumber"], null, { editable: (data) => data && !data.voucherDateLatest && this.accountYear.status < TermGroup_AccountStatus.Closed });
        this.voucherSeriesGridOptions.addColumnDate("voucherDateLatest", this.terms["economy.accounting.accountyear.lastvoucherdate"], null);
        this.voucherSeriesGridOptions.addColumnDelete(this.terms["core.deleterow"], this.deleteRowVoucherSeries.bind(this), null, (data) => data && !data.voucherDateLatest && this.accountYear.status < TermGroup_AccountStatus.Closed);

        const events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.AfterCellEdit, (entity, colDef, newValue, oldValue) => {
            this.afterCellEditVoucherSeries(entity, colDef, newValue, oldValue);
        }));
        this.voucherSeriesGridOptions.subscribe(events);

        this.voucherSeriesGridOptions.finalizeInitGrid();

        this.voucherTemplatesGridOptions.enableGridMenu = false;
        this.voucherTemplatesGridOptions.setMinRowsToShow(15);

        this.voucherTemplatesGridOptions.addColumnNumber("voucherNr", this.terms["common.number"], 30);
        this.voucherTemplatesGridOptions.addColumnDate("date", this.terms["common.date"], 60);
        this.voucherTemplatesGridOptions.addColumnText("text", this.terms["common.text"], null);
        this.voucherTemplatesGridOptions.addColumnSelect("voucherSeriesTypeName", this.terms["economy.accounting.voucher.voucherseries"], 60, { displayField: "voucherSeriesTypeName", selectOptions: this.voucherSeriesTypes, populateFilterFromGrid: true });
        this.voucherTemplatesGridOptions.addColumnBool("vatVoucher", this.terms["economy.accounting.voucher.vatvoucher"], 30, { enableEdit: false });
        this.voucherTemplatesGridOptions.addColumnText("sourceTypeName", this.terms["economy.accounting.voucher.sourcetype"], 60, { hide: true, enableHiding: true });
        this.voucherTemplatesGridOptions.addColumnIcon(null, null, 40, { icon: "fal fa-paperclip", showIcon: (row) => row.hasDocuments });
        this.voucherTemplatesGridOptions.addColumnIcon("modified", null, 40, { icon: "fal fa-exclamation-circle warningColor", toolTip: this.terms["economy.accounting.voucher.vouchermodified"], showIcon: (row) => row.hasHistoryRows });

        this.voucherTemplatesGridOptions.finalizeInitGrid();

        this.grossProfitCodesGridOptions.enableGridMenu = false;
        this.grossProfitCodesGridOptions.setMinRowsToShow(15);

        this.grossProfitCodesGridOptions.addColumnText("code", this.terms["common.code"], null);
        this.grossProfitCodesGridOptions.addColumnText("name", this.terms["common.name"], null);
        this.grossProfitCodesGridOptions.addColumnText("accountYearId", this.terms["economy.accounting.grossprofitcode.accountyear"], null);
        this.grossProfitCodesGridOptions.addColumnText("description", this.terms["common.description"], null);

        this.grossProfitCodesGridOptions.finalizeInitGrid();

    }

    private deleteRowVoucherSeries(row: VoucherSeriesDTO) {
        row.isDeleted = true;
        this.voucherSeriesGridOptions.setData(_.filter(this.voucherSeries, (s) => !s.isDeleted));
        this.filterVoucherSeriesTypes();
        this.dirtyHandler.setDirty();
    }

    private afterCellEditVoucherSeries(entity, colDef, newValue, oldValue) {
        if (!newValue || newValue === oldValue)
            return;

        if (colDef.field === 'voucherNrLatest') {
            if (newValue > 0 && newValue > entity.startNr && !entity.voucherDateLatest) {
                entity.isModified = true;
                this.dirtyHandler.setDirty();
            }
            else {
                entity.voucherNrLatest = oldValue;
                this.voucherSeriesGridOptions.refreshRows(entity);
            }
        }
    }

    public save() {
        // Get series to save
        const seriesToSave = _.filter(this.voucherSeries, (s) => (s.isModified && !s.isDeleted) || (s.isDeleted && s.voucherSeriesId > 0));

        this.progress.startSaveProgress((completion) => {
            this.accountingService.saveAccountYear(this.accountYear, seriesToSave, this.keepNumberSeries).then((result) => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0)
                        this.accountYearId = result.integerValue;

                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.accountYear);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid)
            .then(data => {
                if (this.accountYear.status !== this.previousStatus) {
                    // Clean
                    this.dirtyHandler.clean();

                    // Release cache
                    this.accountingService.getVoucherSeriesByYear(this.accountYearId, true, false);

                    this.$timeout(() => {
                        // Reload in order reset header dropdown
                        HtmlUtility.openInSameTab(this.$window, "/soe/economy/accounting/yearend/?ay=" + this.accountYearId);
                    }, 100);
                }
                else {
                    this.isNew = false;
                    this.dirtyHandler.clean();
                    this.loadVoucherSeriesMapping(true);
                    this.onLoadData(true);

                    // Release cache
                    this.accountingService.getVoucherSeriesByYear(this.accountYearId, true, false);
                }
            }, error => { });
    }

    public delete() {
        this.progress.startDeleteProgress((completion) => {
            this.accountingService.deleteAccountYear(this.accountYearId).then((result) => {
                if (result.success) {
                    completion.completed(this.accountYear);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }).then(x => {
            this.new();
            this.updateTabCaption();
        });
    }

    private copyVoucherTemplates() {
        if (this.dirtyHandler.isDirty)
            return;

        this.progress.startSaveProgress((completion) => {
            this.accountingService.copyVoucherTemplatesFromPreviousYear(this.accountYear.accountYearId).then((result) => {
                if (result.success) {
                    completion.completed(Constants.EVENT_EDIT_SAVED, this.accountYear);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid)
            .then(data => {
                this.loadVoucherTemplates();
            }, error => { });
    }

    private copyGrossProfitCodes() {
        if (this.dirtyHandler.isDirty)
            return;

        this.progress.startSaveProgress((completion) => {
            this.accountingService.copyGrossProfitCodesFromPreviousYear(this.accountYear.accountYearId).then((result) => {
                if (result.success) {
                    completion.completed(Constants.EVENT_EDIT_SAVED, this.accountYear);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid)
            .then(data => {
                this.loadGrossProfitCodes();
            }, error => { });
    }

    private new() {
        this.isNew = true;
        this.enableChangePeriods = true;

        this.accountYear = new AccountYearDTO();
        this.accountYear.status = TermGroup_AccountStatus.New;
        this.accountYear.periods = [];
        if (this.latestTo) {
            var year = this.latestTo.getFullYear();
            this.accountYear.from = new Date(this.latestTo.addDays(1));
            this.accountYear.to = new Date(this.latestTo.setFullYear(year + 1));

            this.previouseAccountYearFrom = null;
            this.previouseAccountYearTo = null;
            // Genereate new periods
            this.generatePeriods();
        }

        this.voucherSeries = [];
        _.forEach(this.voucherSeriesTypes, (type) => {
            var serie = new VoucherSeriesDTO();
            serie.voucherSeriesTypeId = type.voucherSeriesTypeId;
            serie.voucherSeriesTypeName = type.name;
            serie["voucherSeriesTypeNr"] = type.voucherSeriesTypeNr;
            serie["startNr"] = type.startNr;
            serie.isModified = true;
            this.voucherSeries.push(serie);
        });

        this.voucherSeriesGridOptions.setData(this.voucherSeries);

        this.dirtyHandler.setDirty();
    }

    public copy() {
        this.isNew = true;

        this.accountYearId = undefined;
        this.accountYear.accountYearId = undefined;
        this.accountYear.status = TermGroup_AccountStatus.New;
        this.accountYear.from = undefined;
        this.accountYear.to = undefined;
        this.previouseAccountYearFrom = null;
        this.previouseAccountYearTo = null;
        this.accountYear.periods = [];

        _.forEach(this.voucherSeries, (s) => {
            s.voucherSeriesId = undefined;
            s.voucherNrLatest = undefined;
            s.voucherDateLatest = undefined;
            s.isModified = true;
        });

        this.periodsGridOptions.setData(_.filter(this.accountYear.periods, (p) => !p.isDeleted));
        this.voucherSeriesGridOptions.setData(this.voucherSeries);

        this.dirtyHandler.setDirty();

        this.updateTabCaption();
    }

    public dateChanged() {
        this.$timeout(() => {
            // Get year and month
            const yearFrom = this.accountYear.from.getFullYear();
            const yearTo = this.accountYear.to.getFullYear();
            const monthFrom = this.accountYear.from.getMonth();
            const monthTo = this.accountYear.to.getMonth();


            const obj = new AccountYearDTO();
            angular.extend(obj, this.accountYear);


            this.accountYear.from = new Date(yearFrom, monthFrom, 1);
            this.accountYear.to = new Date(yearTo, monthTo + 1, 0);

            if (this.accountYear.from > this.accountYear.to)
                this.accountYear.to = this.accountYear.from;

            if (this.validateYearPeriod()) {

                // Remove old periods
                const tempPeriods = [];
                _.forEach(this.accountYear.periods, (py) => {
                    if (py.accountPeriodId > 0) {
                        py.isDeleted = true;
                        tempPeriods.push(py);
                    }
                });

                this.accountYear.periods = tempPeriods;

                // Genereate new periods
                this.generatePeriods();
            }
        });
    }

    private validateYearPeriod(): boolean {
        this.hasShorteningOverlap = false;
        this.hasExtendingOverlap = false;
        this.hasPeriodNotStarted = false;

        const yearPeriod = this.accountYears.find(x => x.accountYearId != this.accountYear.accountYearId && ((x.from <= this.accountYear.to && this.accountYear.to <= x.to) || (x.from <= this.accountYear.from && this.accountYear.from <= x.to)));
        if (yearPeriod == null) {
            if (this.previouseAccountYearTo != null && (this.previouseAccountYearFrom != this.accountYear.from || this.previouseAccountYearTo != this.accountYear.to)) {
                let canClearPeriods = true;
                _.forEach(this.accountYear.periods, (py) => {
                    if (py.status !== TermGroup_AccountStatus.New) {
                        canClearPeriods = false;
                    }
                });
                if (!canClearPeriods) {
                    this.hasPeriodNotStarted = true;
                    this.showValidationMessage('common.error.remove.account.year.period');
                    return false;
                }
            }
            return true;
        }
        if (this.previouseAccountYearTo < this.accountYear.to || this.previouseAccountYearFrom > this.accountYear.from) {
            //Extending a year
            this.hasExtendingOverlap = true;
            this.showValidationMessage('common.error.extending.year');

        } else if (this.previouseAccountYearTo > this.accountYear.to || this.previouseAccountYearFrom < this.accountYear.from) {
            //Shortening a year
            this.hasShorteningOverlap = true;
            this.showValidationMessage('common.error.shortening.year');
        }

        return false;
    }
    private showValidationMessage(errorKey: string): void {
        this.notificationService.showDialog(
            this.terms["core.warning"],
            this.terms[errorKey],
            SOEMessageBoxImage.Warning,
            SOEMessageBoxButtons.OK
        );
    }

    public changeStatusOnPeriod() {
        const selectedRows = this.periodsGridOptions.getSelectedRows();
        let invalidCount = 0;
        let invalidVouchersCount = 0;
        _.forEach(selectedRows, (row: AccountPeriodDTO) => {
            switch (this.selectedPeriodStatus) {
                case TermGroup_AccountStatus.New:
                    if (row.status === TermGroup_AccountStatus.Open) {
                        if (!row.hasExistingVouchers) {
                            row.status = this.selectedPeriodStatus;
                            this.setPeriodValues(row);
                        }
                        else {
                            invalidVouchersCount++;
                        }
                    }
                    else {
                        invalidCount++;
                    }
                    break;
                case TermGroup_AccountStatus.Open:
                    if (row.status === TermGroup_AccountStatus.New || row.status === TermGroup_AccountStatus.Closed) {
                        row.status = this.selectedPeriodStatus;
                        this.setPeriodValues(row);
                    }
                    else {
                        invalidCount++;
                    }
                    break;
                case TermGroup_AccountStatus.Closed:
                    if (row.status === TermGroup_AccountStatus.Open) {
                        row.status = this.selectedPeriodStatus;
                        this.setPeriodValues(row);
                    }
                    else {
                        invalidCount++;
                    }
                    break;
                case TermGroup_AccountStatus.Locked:
                    if (row.status === TermGroup_AccountStatus.Closed) {
                        row.status = this.selectedPeriodStatus;
                        this.setPeriodValues(row);
                    }
                    else {
                        invalidCount++;
                    }
                    break;
            }
        });

        // Set status on year
        if (_.some(this.accountYear.periods, (p) => p.status === TermGroup_AccountStatus.Open))
            this.accountYear.status = TermGroup_AccountStatus.Open;
        else
            this.accountYear.status = _.first(_.orderBy(this.accountYear.periods, (p) => p.status)).status;

        // Set flag
        this.enableChangePeriods = _.filter(this.accountYear.periods, (p) => p.status > TermGroup_AccountStatus.New).length === 0;

        // Reset rows
        this.periodsGridOptions.setData(_.filter(this.accountYear.periods, (p => !p.isDeleted)))

        this.dirtyHandler.setDirty();

        if (invalidCount > 0 || invalidVouchersCount > 0) {
            let message = "";
            if (invalidVouchersCount > 0)
                message = invalidVouchersCount === 1 ? this.terms["economy.accounting.accountyear.changestatusinvalidvouchersingle"].format(_.find(this.accountStatuses, o => o.id === this.selectedPeriodStatus).name) : invalidCount.toString() + " " + this.terms["economy.accounting.accountyear.changestatusinvalidvouchermultiple"].format(_.find(this.accountStatuses, o => o.id === this.selectedPeriodStatus).name) + "\n";
            if (invalidCount > 0)
                message = invalidCount === 1 ? this.terms["economy.accounting.accountyear.changestatusinvalidsingle"].format(_.find(this.accountStatuses, o => o.id === this.selectedPeriodStatus).name) : invalidCount.toString() + " " + this.terms["economy.accounting.accountyear.changestatusinvalidmultiple"].format(_.find(this.accountStatuses, o => o.id === this.selectedPeriodStatus).name);
            this.notificationService.showDialog(this.terms["core.warning"], message, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
        }
    }

    public addVoucherSerie() {
        var newS = _.find(this.voucherSeriesTypes, (v) => v.voucherSeriesTypeId === this.selectedVoucherSeriesType);
        if (newS) {
            var newRow = new VoucherSeriesDTO();
            newRow.isModified = true;
            newRow.voucherSeriesTypeId = newS.voucherSeriesTypeId;
            newRow.voucherSeriesTypeName = newS.name;
            newRow["voucherSeriesTypeNr"] = newS.voucherSeriesTypeNr;
            newRow['startNr'] = newS.startNr;

            this.voucherSeries.push(newRow);
            this.voucherSeriesGridOptions.setData(this.voucherSeries);

            this.filterVoucherSeriesTypes();

            this.dirtyHandler.setDirty();
        }
    }

    public generatePeriods() {
        for (let i = 0; i < CalendarUtility.getMonthsBetweenDates(this.accountYear.from, this.accountYear.to); i++) {
            let period = new AccountPeriodDTO();
            period.from = new Date(this.accountYear.from.getFullYear(), this.accountYear.from.getMonth() + i, 1);
            period.periodNr = i + 1;
            period.status = TermGroup_AccountStatus.New;
            period.isModified = true;

            this.setPeriodValues(period);

            this.accountYear.periods.push(period);
        }
        this.periodsGridOptions.setData(_.filter(this.accountYear.periods, (p) => !p.isDeleted));
    }

    private filterVoucherSeriesTypes() {
        this.filteredVoucherSeriesTypes = [];
        _.forEach(this.voucherSeriesTypes, (type) => {
            if (!_.find(this.voucherSeries, (s) => s.voucherSeriesTypeId === type.voucherSeriesTypeId))
                this.filteredVoucherSeriesTypes.push({ id: type.voucherSeriesTypeId, name: type.name });
        });
    }

    private updateTabCaption() {
        this.messagingHandler.publishSetTabLabel(this.guid, this.isNew ? this.terms["economy.accounting.accountyear.newaccountyear"] : this.terms["economy.accounting.accountyear.accountyear"] + " " + this.accountYear.yearFromTo);
    }

    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {

            if (this.hasExtendingOverlap) {
                validationErrorStrings.push(this.terms['common.error.extending.year']);
            }
            if (this.hasShorteningOverlap) {
                validationErrorStrings.push(this.terms['common.error.shortening.year']);
            }
            if (this.hasPeriodNotStarted) {
                validationErrorStrings.push(this.terms['common.error.remove.account.year.period']);
            }
        });
    }
}