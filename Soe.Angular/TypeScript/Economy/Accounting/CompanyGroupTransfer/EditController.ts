import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { Feature, TermGroup, TermGroup_Languages, DistributionCodeBudgetType, CompanyGroupTransferStatus, SoeEntityState, CompanyGroupTransferType } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { IFieldSettingDTO, ISmallGenericType, ISysTermDTO } from "../../../Scripts/TypeLite.Net4";
import { ISoeGridOptionsAg, TypeAheadOptionsAg, SoeGridOptionsAg } from "../../../Util/SoeGridOptionsAg";
import { SoeGridOptionsEvent, SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../Util/Enumerations";
import { SmallGenericType } from "../../../Common/Models/SmallGenericType";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { lang } from "moment";
import { IAccountingService } from "../../../Shared/Economy/Accounting/AccountingService";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    //Lookups
    private terms: { [index: string]: string; };
    private accountYears: any[];
    private accountPeriods: any[];
    private voucherSeries: any[];
    private transferTypes: any[];
    private companyGroupAdministrations: any[];
    private budgets: any[];
    private filteredBudgets: any[];
    private childCompanies: any[];
    private filteredChildCompanies: any[];
    private childCompanyBudgets: any[];
    private filteredChildCompanyBudgets: any[];
    private hasEditVoucherPermissions;
    private hasEditBudgetPermissions;

    // Grid
    protected transferGridOptions: ISoeGridOptionsAg;
    private transferDetailsGridOptions: ISoeGridOptionsAg;

    // Properties
    protected selectedVoucherSerie: any;
    protected selectedPeriodFrom: any;
    protected selectedPeriodTo: any;
    protected selectedCompanyTo: any;
    protected selectedBudgetChildCompany: any;
    protected log: string[];

    // Flags
    private logExpanderIsOpen: boolean = false;
    private setupIsDone = false;

    private _selectedTransferType: any;
    get selectedTransferType() {
        return this._selectedTransferType;
    }
    set selectedTransferType(value: any) {
        this._selectedTransferType = value;
        if (value && value.id > 0)
            this.transferTypeChanged();
    }

    private _selectedBudgetMaster: any;
    get selectedBudgetMaster() {
        return this._selectedBudgetMaster;
    }
    set selectedBudgetMaster(value: any) {
        this._selectedBudgetMaster = value;
        if (value)
            this.filterChildBudgets();
        else
            this.selectedBudgetChildCompany = undefined;
    }

    private _selectedCompanyFrom: any;
    get selectedCompanyFrom() {
        return this._selectedCompanyFrom;
    }
    set selectedCompanyFrom(value: any) {
        this._selectedCompanyFrom = value;
        if(value && value > 0 && this.setupIsDone)
            this.loadBudgets(this._selectedCompanyFrom);
    }

    private _selectedAccountYear: any;
    get selectedAccountYear() {
        return this._selectedAccountYear;
    }
    set selectedAccountYear(value: any) {
        this._selectedAccountYear = value;

        if (this._selectedAccountYear) {
            this.progress.startLoadingProgress([
                () => this.loadVoucherSeries(this._selectedAccountYear.accountYearId),
            ]);

            if (this.selectedTransferType && this.selectedTransferType.id === TransferTypeEnum.Budget)
                this.filterMasterBudgets();
        }

        //Set periods
        this.accountPeriods = this._selectedAccountYear.formattedPeriods;
        this.selectedPeriodFrom = this.accountPeriods[0];
        this.selectedPeriodTo = this.accountPeriods[this.accountPeriods.length - 1];
    }

    get showBudgetFields() {
        return this.selectedTransferType && this.selectedTransferType.id === TransferTypeEnum.Budget;
    }

    get showBalanceFields() {
        return this.selectedTransferType && this.selectedTransferType.id === TransferTypeEnum.Balance;
    }

    //@ngInject
    constructor(
        private $timeout: ng.ITimeoutService,
        private accountingService: IAccountingService,
        urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        private notificationService: INotificationService,
        private translationService: ITranslationService,
        private messagingService: IMessagingService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.doLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    public onInit(parameters: any) {
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.transferGridOptions = new SoeGridOptionsAg("TransferGrid", this.$timeout);
        this.transferDetailsGridOptions = SoeGridOptionsAg.create("TransferGrid", this.$timeout);

        this.flowHandler.start([{ feature: Feature.Economy_Accounting_CompanyGroup_Transfers, loadReadPermissions: true, loadModifyPermissions: true },
                                { feature: Feature.Economy_Accounting_Vouchers_Edit, loadReadPermissions: true, loadModifyPermissions: true },
                                { feature: Feature.Economy_Accounting_Budget_Edit, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Economy_Accounting_CompanyGroup_Transfers].readPermission;
        this.modifyPermission = response[Feature.Economy_Accounting_CompanyGroup_Transfers].modifyPermission;
        this.hasEditVoucherPermissions = response[Feature.Economy_Accounting_CompanyGroup_Transfers].modifyPermission;
        this.hasEditBudgetPermissions = response[Feature.Economy_Accounting_Budget_Edit].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(false);
    }

    // LOOKUPS
    private doLookups() {
        return this.progress.startLoadingProgress([
            () => this.loadTerms(),
            () => this.loadAccountingYears(),
            () => this.loadCompanyGroupAdministrations(),
            () => this.loadChildCompanies(),
            () => this.loadVoucherHistory(soeConfig.accountYearId, { id: 1 }),
        ]).then(
            () => this.loadBudgets(undefined)
        ).then(() => {
            // Set up transfer types
            this.transferTypes = [];
            this.transferTypes.push({ id: 1, name: this.terms["common.reports.drilldown.periodamount"] });
            this.transferTypes.push({ id: 2, name: this.terms["common.reports.drilldown.budget"] }); 
            this.transferTypes.push({ id: 3, name: this.terms["economy.accounting.balance.balance"] });
            this.selectedTransferType = this.transferTypes[0];

            // Filter child companies
            this.filteredChildCompanies = [];
            _.forEach(this.childCompanies, (c) => {
                if (_.find(this.companyGroupAdministrations, { 'childActorCompanyId': c.id }))
                    this.filteredChildCompanies.push(c);
            });

            this.setupGrid();

            this.setupIsDone = true;
        });
    }

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "economy.accounting.companygroup.companygroupvoucher",
            "common.reports.drilldown.budget",
            "common.reports.drilldown.periodamount",
            "economy.accounting.companygroup.completedtransfers",
            "common.date",
            "common.missingrequired",
            "core.warning",
            "economy.accounting.accountyear",
            "economy.accounting.voucherseriestype",
            "economy.accounting.companygroup.periodfrom",
            "economy.accounting.companygroup.periodto",
            "economy.accounting.companygroup.companyfrom",
            "economy.accounting.companygroup.companyto",
            "economy.accounting.companygroup.companygroupbudget",
            "economy.accounting.companygroup.budgetname",
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total",
            "economy.accounting.companygroup.transfererror",
            "economy.accounting.companygroup.accountyear",
            "economy.accounting.companygroup.periodfrom",
            "economy.accounting.companygroup.periodto",
            "economy.accounting.companygroup.voucherserie",
            "economy.accounting.companygroup.transfertype",
            "economy.accounting.companygroup.created",
            "common.name",
            "common.status",
            "core.delete",
            "common.company",
            "common.period",
            "common.reports.drilldown.vouchernr",
            "common.text",
            "economy.accounting.companygroup.conversionrate",
            "economy.accounting.companygroup.transfered",
            "economy.accounting.companygroup.childbudget",
            "economy.accounting.companygroup.masterbudget",
            "economy.accounting.balance.balance"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private loadAccountingYears(): ng.IPromise<any> {
        return this.accountingService.getAccountYears(true).then((x) => {
            this.accountYears = x;
            _.forEach(this.accountYears, (year) => {
                var periodsDict: any[] = [];
                _.forEach(year.periods, (period) => {
                    periodsDict.push({ id: period.accountPeriodId, value: CalendarUtility.toFormattedYearMonth(period.from) });
                });
                year['formattedPeriods'] = periodsDict;
            });
            this.selectedAccountYear = _.find(this.accountYears, { 'accountYearId': soeConfig.accountYearId });
        });
    }

    private loadCompanyGroupAdministrations(): ng.IPromise<any> {
        return this.accountingService.getCompanyAdministrations().then((x) => {
            this.companyGroupAdministrations = x;
        });
    }

    private loadChildCompanies(): ng.IPromise<any> {
        return this.accountingService.getChildCompaniesDict().then((x) => {
            this.childCompanies = x;
        });
    }

    private loadBudgets(actorCompanyId): ng.IPromise<any> {
        return this.accountingService.getBudgetHeadsForGrid(DistributionCodeBudgetType.AccountingBudget, actorCompanyId).then((x) => {
            if (!actorCompanyId) {
                this.budgets = x;

                this.filterMasterBudgets();
            }
            else {
                this.childCompanyBudgets = x;

                this.filterChildBudgets();
            }
        });
    }

    private filterMasterBudgets() {
        return this.translationService.translate("economy.accounting.companygroup.createbudget").then((term) => {
            // Empty
            this.selectedBudgetChildCompany = undefined;
            this.filteredChildCompanyBudgets = undefined;

            // Add budgets for current accountyear
            this.filteredBudgets = _.filter(this.budgets, (b) => b.accountYearId === this.selectedAccountYear.accountYearId);

            // Insert empty row
            if(this.filteredBudgets)
                this.filteredBudgets.splice(0, 0, { budgetHeadId: 0, name: term });
        });
    }

    private filterChildBudgets() {
        this.$timeout(() => {
            // Empty
            this.selectedBudgetChildCompany = undefined;
            this.filteredChildCompanyBudgets = undefined;

            // Add budgets for current accountyear
            if (this.selectedBudgetMaster.budgetHeadId === 0)
                this.filteredChildCompanyBudgets = this.childCompanyBudgets;
            else 
                this.filteredChildCompanyBudgets = _.filter(this.childCompanyBudgets, (b) => b.accountingYear === this.selectedBudgetMaster.accountingYear && b.noOfPeriods === this.selectedBudgetMaster.noOfPeriods);
        });
    }

    private loadVoucherSeries(accountYearId: number): ng.IPromise<any> {
        return this.accountingService.getVoucherSeriesByYear(accountYearId, false, false).then((x) => {
            this.voucherSeries = x;
            const companyGroupVoucherSerie = _.find(this.voucherSeries, { 'voucherSeriesTypeName': this.terms["economy.accounting.companygroup.companygroupvoucher"] });
            if (!companyGroupVoucherSerie) {
                this.addCompanyGroupVoucherSerie(accountYearId);
            }
            else {
                this.$timeout(() => {
                    this.selectedVoucherSerie = companyGroupVoucherSerie;
                });
            }
        });

    }

    private loadVoucherHistory(accountYearId: number, transferType: any): ng.IPromise<any> {
        if (!transferType)
            return;

        return this.accountingService.getCompanyGroupVoucherHistory(accountYearId, transferType.id).then((x) => {
            _.forEach(x, (y) => {
                y.date = CalendarUtility.convertToDate(y.date);
            });
            this.transferGridOptions.setData(x);
        });
    }

    private setupGrid() {

        this.transferGridOptions.enableGridMenu = false;
        this.transferGridOptions.enableRowSelection = false;
        this.transferGridOptions.setMinRowsToShow(20);

        this.transferDetailsGridOptions.enableRowSelection = false;

        this.transferGridOptions.enableMasterDetail(this.transferDetailsGridOptions);
        this.transferGridOptions.setDetailCellDataCallback((params) => {
            this.$timeout(() => {
                if (this.selectedTransferType.id === CompanyGroupTransferType.Consolidation) {
                    this.transferDetailsGridOptions.showColumn("voucherNr");
                    this.transferDetailsGridOptions.hideColumn("budgetName");
                    this.transferDetailsGridOptions.showColumn("voucherText");
                    this.transferDetailsGridOptions.showColumn("voucherSeriesName");
                    this.transferDetailsGridOptions.showColumn("accountPeriodText");
                }
                else {
                    this.transferDetailsGridOptions.showColumn("budgetName");
                    this.transferDetailsGridOptions.hideColumn("voucherNr");
                    this.transferDetailsGridOptions.hideColumn("voucherText");
                    this.transferDetailsGridOptions.hideColumn("voucherSeriesName");
                    this.transferDetailsGridOptions.hideColumn("accountPeriodText");
                }

                if (params.data) {
                    params.successCallback(params.data.companyGroupTransferRows);
                }
            });
        });

        // Details grid
        // Utfall
        this.transferDetailsGridOptions.addColumnText("voucherNr", this.terms["common.reports.drilldown.vouchernr"], 80, { pinned: "left", enableRowGrouping: true, enableHiding: true, buttonConfiguration: { iconClass: "iconEdit fal fa-pencil", show: (row) => this.hasEditVoucherPermissions && row.voucherHeadId, callback: this.openItem.bind(this) } });
        this.transferDetailsGridOptions.addColumnText("budgetName", this.terms["common.name"], 80, { pinned: "left", enableRowGrouping: true, enableHiding: true, buttonConfiguration: { iconClass: "iconEdit fal fa-pencil", show: (row) => this.hasEditBudgetPermissions && row.budgetHeadId, callback: this.openItem.bind(this) } });
        this.transferDetailsGridOptions.addColumnText("childActorCompanyNrName", this.terms["common.company"], null);
        this.transferDetailsGridOptions.addColumnText("accountPeriodText", this.terms["common.period"], null);
        this.transferDetailsGridOptions.addColumnText("status", this.terms["common.status"], 120);
        this.transferDetailsGridOptions.addColumnText("voucherText", this.terms["common.text"], null);
        this.transferDetailsGridOptions.addColumnText("voucherSeriesName", this.terms["economy.accounting.companygroup.voucherserie"], null);
        this.transferDetailsGridOptions.addColumnNumber("conversionFactor", this.terms["economy.accounting.companygroup.conversionrate"], null, { enableHiding: true, decimals: 4 });
        this.transferDetailsGridOptions.addColumnDateTime("created", this.terms["economy.accounting.companygroup.transfered"], null);
        this.transferDetailsGridOptions.addColumnDelete(this.terms["core.delete"], this.deleteVoucher.bind(this), null, (row) => { return this.isVoucherDeletable(row) });
        // Budget


        // Grid
        this.transferGridOptions.addColumnText("accountYearText", this.terms["economy.accounting.companygroup.accountyear"], null).cellRenderer = 'agGroupCellRenderer';
        this.transferGridOptions.addColumnText("fromAccountPeriodText", this.terms["economy.accounting.companygroup.periodfrom"], null);
        this.transferGridOptions.addColumnText("toAccountPeriodText", this.terms["economy.accounting.companygroup.periodto"], null);
        this.transferGridOptions.addColumnText("transferTypeName", this.terms["economy.accounting.companygroup.transfertype"], null);
        this.transferGridOptions.addColumnText("transferStatusName", this.terms["common.status"], null);
        this.transferGridOptions.addColumnDateTime("transferDate", this.terms["economy.accounting.companygroup.created"], null);
        this.transferGridOptions.addColumnDelete(this.terms["core.delete"], this.deleteTransfer.bind(this), null, (row) => { return this.isTransferDeletable(row) });

        this.transferGridOptions.addTotalRow("#totals-grid", {
            filtered: this.terms["core.aggrid.totals.filtered"],
            total: this.terms["core.aggrid.totals.total"]
        });

        this.transferGridOptions.finalizeInitGrid();
    }

    public isTransferDeletable(row: any): boolean {
        return row.transferStatus === CompanyGroupTransferStatus.Transfered || row.status === CompanyGroupTransferStatus.PartlyDeleted;
    }

    public isVoucherDeletable(row: any) {
        return row.voucherHeadId > 0;
    }

    // ACTIONS
    public transferTypeChanged() {
        this.loadVoucherHistory(this.selectedAccountYear.accountYearId, this.selectedTransferType);
    }

    public openItem(row: any) {
        if (row)
            this.messagingService.publish(Constants.EVENT_OPEN_OFFER, row);
    }

    public deleteTransfer(row: any) {
        const keys = [
            "core.warning",
            "economy.accounting.companygroup.deletetransfer",
            "economy.accounting.companygroup.deletingbudget",
            "economy.accounting.companygroup.deletingbalance"
        ];
        this.translationService.translateMany(keys).then((terms) => {
            let message = "";
            switch (row.transferType) {
                case CompanyGroupTransferType.Consolidation:
                    message = terms["economy.accounting.companygroup.deletetransfer"];
                    break;
                case CompanyGroupTransferType.Budget:
                    message = terms["economy.accounting.companygroup.deletingbudget"];
                    break;
                case CompanyGroupTransferType.Balance:
                    message = terms["economy.accounting.companygroup.deletingbalance"];
                    break;
            }
            var modal = this.notificationService.showDialog(terms["core.warning"], message, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
            modal.result.then(val => {
                if (val) {
                    if (row.onlyVoucher) {
                        this.progress.startSaveProgress((completion) => {
                            this.accountingService.deleteVoucherOnlySuperSupport(row.voucherHeadId, true).then((result) => {
                                if (result.success) {
                                    completion.completed(null);
                                } else {
                                    completion.failed(result.errorMessage);
                                }
                            }, error => {
                                completion.failed(error.message);
                            });
                        }, this.guid).then(x => {
                            this.loadVoucherHistory(this.selectedAccountYear.accountYearId, this.selectedTransferType);
                        });
                    }
                    else {
                        this.progress.startSaveProgress((completion) => {
                            this.accountingService.deleteCompanyGroupTransfer([row.companyGroupTransferHeadId]).then((result) => {
                                if (result.success) {
                                    completion.completed(null);
                                } else {
                                    completion.failed(result.errorMessage);
                                }
                            }, error => {
                                completion.failed(error.message);
                            });
                        }, this.guid).then(x => {
                            this.loadVoucherHistory(this.selectedAccountYear.accountYearId, this.selectedTransferType);
                        });
                    }
                }
            });
        });
    }

    public deleteVoucher(row: any) {
        var keys: string[] = [
            "core.warning",
            "economy.accounting.companygroup.deletevoucher",
        ];
        this.translationService.translateMany(keys).then((terms) => {
            var modal = this.notificationService.showDialog(terms["core.warning"], terms["economy.accounting.companygroup.deletevoucher"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
            modal.result.then(val => {
                if (val) {
                    this.progress.startSaveProgress((completion) => {
                        this.accountingService.deleteVoucherOnlySuperSupport(row.voucherHeadId, true).then((result) => {
                            if (result.success) {
                                completion.completed(null);
                            } else {
                                completion.failed(result.errorMessage);
                            }
                        }, error => {
                            completion.failed(error.message);
                        });
                    }, this.guid).then(x => {
                        this.loadVoucherHistory(this.selectedAccountYear.accountYearId, this.selectedTransferType);
                    });
                }
            });
        });
    }

    public initTransfer() {
        // Empty log
        this.log = [];

        // Validate
        let message: string = "";

        const accountYearId: number = this.selectedAccountYear ? this.selectedAccountYear.accountYearId : undefined;
        const voucherSeriesId: number = this.selectedVoucherSerie ? this.selectedVoucherSerie.voucherSeriesId : undefined;
        const periodFrom: number = this.selectedPeriodFrom ? this.selectedPeriodFrom.id : undefined;
        const periodTo: number = this.selectedPeriodTo ? this.selectedPeriodTo.id : undefined;

        if (!accountYearId || accountYearId === 0)
            message += this.terms["common.missingrequired"].format(this.terms["economy.accounting.accountyear"]) + "\n";

        if (this.selectedTransferType.id === TransferTypeEnum.Result) {
            if (!voucherSeriesId || voucherSeriesId === 0)
                message += this.terms["common.missingrequired"].format(this.terms["economy.accounting.voucherseriestype"]) + "\n";

            if (!periodFrom || periodFrom === 0)
                message += this.terms["common.missingrequired"].format(this.terms["economy.accounting.companygroup.periodfrom"]) + "\n";

            if (!periodTo || periodTo === 0)
                message += this.terms["common.missingrequired"].format(this.terms["economy.accounting.companygroup.periodto"]) + "\n";
        }
        else if (this.selectedTransferType.id === TransferTypeEnum.Budget) {
            if (!this.selectedBudgetMaster)
                message += this.terms["common.missingrequired"].format(this.terms["economy.accounting.companygroup.masterbudget"]) + "\n";

            if (!this.selectedCompanyFrom || this.selectedCompanyFrom === 0)
                message += this.terms["common.missingrequired"].format(this.terms["economy.accounting.companygroup.companyfrom"]) + "\n";

            if (!this.selectedBudgetChildCompany || this.selectedBudgetChildCompany.budgedHeadId === 0)
                message += this.terms["common.missingrequired"].format(this.terms["economy.accounting.companygroup.childbudget"]) + "\n";
        }
        
        if (message.length > 0)
            this.notificationService.showDialog(this.terms["core.warning"], message, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
        else
            this.transfer(accountYearId, voucherSeriesId, periodFrom, periodTo, this.selectedCompanyFrom, this.selectedBudgetMaster ? this.selectedBudgetMaster.budgetHeadId : 0, this.selectedBudgetChildCompany ? this.selectedBudgetChildCompany.budgetHeadId : 0);
    }

    public transfer(accountYearId: number, voucherSeriesId: number, periodFrom: number, periodTo: number, budgetCompanyFrom: number, budgetMaster: number, budgetChild: number) {
        this.progress.startSaveProgress((completion) => {
            this.accountingService.transferCompanyGroup(this.selectedTransferType.id, accountYearId, voucherSeriesId, periodFrom, periodTo, false, budgetCompanyFrom, budgetMaster, budgetChild).then((result) => {
                if (!result.success) {
                    this.logExpanderIsOpen = true;
                    if (result.value2) {
                        this.log = result.value2.$values;
                        completion.completed();
                    }
                    else if (result.errorNumber) {
                        completion.failed(result.errorMessage);
                    }
                    else {
                        completion.completed();
                    }
                }
                else {
                    if (this.selectedTransferType.id !== CompanyGroupTransferType.Consolidation && result.value2) {
                        this.logExpanderIsOpen = true;
                        this.log = result.value2.$values;
                    }
                    completion.completed();
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid)
            .then(data => {
                if (budgetMaster === 0) 
                    this.loadBudgets(undefined);

                this.selectedBudgetChildCompany = undefined;
                this.selectedCompanyFrom = undefined;

                this.loadVoucherHistory(this.selectedAccountYear.accountYearId, this.selectedTransferType);
            }, error => {
        });
    }

    public addCompanyGroupVoucherSerie(accountYearId: number) {
        this.progress.startSaveProgress((completion) => {
            this.accountingService.saveCompanyGroupVoucherSeries(accountYearId).then((result) => {
                if (result.success) {
                    this.voucherSeries.push(result.value);
                    this.$timeout(() => {
                        this.selectedVoucherSerie = _.find(this.voucherSeries, { 'voucherSeriesTypeName': this.terms["economy.accounting.companygroup.companygroupvoucher"] });
                    });
                    completion.completed();
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(this.terms["economy.accounting.companygroup.transfererror"]);
            });
        }, this.guid);
    }

    public saveNext() {
    }
}

enum TransferTypeEnum {
    Result = 1,
    Budget = 2,
    Balance = 3,
}