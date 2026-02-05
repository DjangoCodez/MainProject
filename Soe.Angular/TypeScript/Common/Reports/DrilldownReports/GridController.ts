import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IReportService } from "../../../Core/Services/ReportService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../Util/Enumerations";
import { Feature, SoeReportTemplateType, DistributionCodeBudgetType, SoeOriginType } from "../../../Util/CommonEnumerations";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";

import { IAccountingService } from "../../../Shared/Economy/Accounting/AccountingService";
import { GroupDisplayType } from "../../../Util/SoeGridOptionsAg";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    // Grid header and footer
    gridHeaderComponentUrl: any;
    gridFooterComponentUrl: any;

    // Collections
    terms: any;

    // Lookups
    reports: Array<any> = [];
    budgets: Array<any> = [];
    accountYearsFrom: any[];
    accountYearsTo: any[];
    accountPeriodsFrom: any[];
    accountPeriodsTo: any[];
    accountPeriodsFromDict: any[];
    accountPeriodsToDict: any[];

    // Properties
    private reportId: number;
    private periodFrom: any;
    private periodTo: any;
    private accountYearFrom: number;
    private accountYearTo: number;
    private sysReportTemplateTypeId: number;

    // Properties
    _budgetId: number;
    get budgetId() {
        return this._budgetId;
    }
    set budgetId(item: number) {
        this._budgetId = item;
        if (this._selectedReport) {
            if (this.gridAg.options.nbrOfColumns() > 0)
                this.gridAg.options.resetColumnDefs();
            this.hideShowColumns();
        }
    }

    _selectedReport: any;
    get selectedReport() {
        return this._selectedReport;
    }
    set selectedReport(item: boolean) {
        this._selectedReport = item;
        if (this._selectedReport) {
            this.reportId = this._selectedReport.reportId;
            this.sysReportTemplateTypeId = this._selectedReport.sysReportTemplateTypeId;
            if (this.gridAg.options.nbrOfColumns() > 0)
                this.gridAg.options.resetColumnDefs();
            this.hideShowColumns();
        }
    }

    reportGroups: Array<any> = [];
    voucherRows: Array<any> = [];

    // Flags
    totalsGridCreated: boolean = false;

    //Search dto
    search: any;

    //@ngInject
    constructor(
        private $timeout: ng.ITimeoutService,
        private $q: ng.IQService,
        private reportService: IReportService,
        private accountingService: IAccountingService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private urlHelperService: IUrlHelperService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        private $scope: ng.IScope) {
        super(gridHandlerFactory, "Common.Reports.DrilldownReports", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onPermissionsLoaded((feature, readOnly, modify) => {
                this.readPermission = readOnly;
                this.modifyPermission = modify;
                if (this.modifyPermission) {
                    // Send messages to TabsController
                    this.messagingHandler.publishActivateAddTab();
                }
            })
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onDoLookUp(() => this.onDoLookUp())
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;

        this.gridHeaderComponentUrl = this.urlHelperService.getViewUrl("filterHeader.html");

        this.accountYearFrom = soeConfig.accountYearId;
        this.accountYearTo = soeConfig.accountYearId;

        this.search = {
            voucherDateFrom: null,
            voucherDateTo: null,
            voucherSeriesIdFrom: 0,
            voucherSeriesIdTo: 0,
            debitFrom: 0,
            debitTo: 0,
            creditFrom: 0,
            creditTo: 0,
            amounFrom: 0,
            amountTo: 0,
            voucherText: "",
            createdFrom: null,
            createdTo: null,
            createdBy: "",
            dim1AccountId: 0,
            dim1AccountFr: "",
            dim1AccountTo: "",
            dim2AccountId: 0,
            dim2AccountFr: "",
            dim2AccountTo: "",
            dim3AccountId: 0,
            dim3AccountFr: "",
            dim3AccountTo: "",
            dim4AccountId: 0,
            dim4AccountFr: "",
            dim4AccountTo: "",
            dim5AccountId: 0,
            dim5AccountFr: "",
            dim5AccountTo: "",
            dim6AccountId: 0,
            dim6AccountFr: "",
            dim6AccountTo: ""
        }

        this.flowHandler.start({ feature: Feature.Economy_Distribution_DrillDownReports, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
    }

    private onDoLookUp(): ng.IPromise<any> {
        return this.$q.all([
            this.loadReports(),
            this.loadAccountYears(),
            this.getAccountDimStd(),
            this.loadBudgets()]).then(() => {
                this.setupGrid();
            });
    }

    public setupGrid() {
        if (this.gridAg.options.nbrOfColumns() > 0)
            this.gridAg.options.resetColumnDefs();
        this.setupGridColumns();
    }

    private setupGridColumns() {
        var keys = [
            "core.warning",
            "common.reports.drilldown.reportgroupname",
            "common.reports.drilldown.reportheadername",
            "common.reports.drilldown.accountnrshort",
            "common.reports.drilldown.accountname",
            "common.reports.drilldown.periodamount",
            "common.reports.drilldown.yearamount",
            "common.reports.drilldown.yearamountshort",
            "common.reports.drilldown.openingbalance",
            "common.reports.drilldown.prevyearamount",
            "common.reports.drilldown.prevperiodamount",
            "common.reports.drilldown.budgetperiodamount",
            "common.reports.drilldown.budgettoperiodamount",
            "common.reports.drilldown.periodprevperioddiff",
            "common.reports.drilldown.yearprevyeardiff",
            "common.reports.drilldown.periodbudgetdiff",
            "common.reports.drilldown.yearbudgetdiff",
            "common.reports.drilldown.vouchernr",
            "common.reports.drilldown.voucherseriesname",
            "common.reports.drilldown.vouchertext",
            "common.reports.drilldown.voucherdate",
            "common.reports.drilldown.debit",
            "common.reports.drilldown.credit",
            "economy.accounting.voucher.voucher",
            "common.credit",
            "common.rownr",
            "common.reports.drilldown.vernr",
            "core.edit",
            "common.reports.drilldown.invalidperiods",
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total",
            "common.openinvoice",
        ];
            
        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;

            //Set name
            this.gridAg.options.setName("common.report.report.report");

            // Prevent double click
            this.doubleClickToEdit = false;

            // Hide filters
            this.gridAg.options.enableFiltering = false;

            // Enable auto column for grouping
            this.gridAg.options.groupDisplayType = GroupDisplayType.Custom;

            // Details
            this.gridAg.enableMasterDetail(false);
            this.gridAg.options.setDetailCellDataCallback((params) => {
                this.loadVoucherRows(params);
            });

            this.gridAg.detailOptions.addColumnNumber("rowNr", this.terms["common.rownr"], null);
            this.gridAg.detailOptions.addColumnText("voucherNr", this.terms["common.reports.drilldown.vernr"], null);
            this.gridAg.detailOptions.addColumnText("voucherSeriesName", this.terms["common.reports.drilldown.voucherseriesname"], null);
            this.gridAg.detailOptions.addColumnText("voucherText", this.terms["common.reports.drilldown.vouchertext"], null);
            this.gridAg.detailOptions.addColumnDate("voucherDate", this.terms["common.reports.drilldown.voucherdate"], null);
            this.gridAg.detailOptions.addColumnNumber("debit", this.terms["common.reports.drilldown.debit"], null, { enableHiding: false, decimals: 2 });
            this.gridAg.detailOptions.addColumnNumber("credit", this.terms["common.credit"], null, { enableHiding: false, decimals: 2 });
            this.gridAg.detailOptions.addColumnIcon(null, "", null, { icon: "fal fa-file-alt", onClick: this.openInvoice.bind(this), toolTip: this.terms["common.openinvoice"], showIcon: (row) => row.invoiceId && row.invoiceId > 0});
            this.gridAg.detailOptions.addColumnEdit(this.terms["core.edit"], this.openVoucher.bind(this));
            this.gridAg.detailOptions.finalizeInitGrid();

            // Master
            var groupCol = this.gridAg.addColumnText("reportGroupName", terms["common.reports.drilldown.reportgroupname"], null);
            var headerCol = this.gridAg.addColumnText("reportHeaderName", this.terms["common.reports.drilldown.reportheadername"], null);

            var accountNrCol = this.gridAg.addColumnText("accountNrCount", this.terms["common.reports.drilldown.accountnrshort"], null);
            accountNrCol.cellRenderer = 'agGroupCellRenderer';

            this.gridAg.addColumnText("accountName", this.terms["common.reports.drilldown.accountname"], null);

            //columns for balance sheet report
            //if (this.sysReportTemplateTypeId == SoeReportTemplateType.BalanceReport) {
            this.gridAg.addColumnNumber("openingBalance", terms["common.reports.drilldown.openingbalance"], null, { enableHiding: false, decimals: 2, aggFuncOnGrouping: 'sum'});
            this.gridAg.addColumnNumber("periodAmount", terms["common.reports.drilldown.periodamount"], null, { enableHiding: true, decimals: 2, aggFuncOnGrouping: 'sum' });
            this.gridAg.addColumnNumber("yearAmount", terms["common.reports.drilldown.yearamount"] + " / " + terms["common.reports.drilldown.yearamountshort"], null, { enableHiding: true, decimals: 2, aggFuncOnGrouping: 'sum' });
            //}
            //columns for result report with bugdet
            //else if ((this.sysReportTemplateTypeId == SoeReportTemplateType.ResultReport || this.sysReportTemplateTypeId == SoeReportTemplateType.ResultReportV2) && this.budgetId != 0) {
                //this.gridAg.addColumnNumber("periodAmount", terms["common.reports.drilldown.periodamount"], null, { enableHiding: true, decimals: 2, aggFuncOnGrouping: 'sum' });
            this.gridAg.addColumnNumber("budgetPeriodAmount", terms["common.reports.drilldown.budgetperiodamount"], null, { enableHiding: false, decimals: 2, aggFuncOnGrouping: 'sum' });
            this.gridAg.addColumnNumber("periodBudgetDiff", terms["common.reports.drilldown.periodbudgetdiff"], null, { enableHiding: false, decimals: 2, aggFuncOnGrouping: 'sum' });
                //this.gridAg.addColumnNumber("yearAmount", terms["common.reports.drilldown.yearamount"] + " / " + terms["common.reports.drilldown.yearamountshort"], null, { enableHiding: true, decimals: 2, aggFuncOnGrouping: 'sum' });
            this.gridAg.addColumnNumber("budgetToPeriodEndAmount", terms["common.reports.drilldown.budgettoperiodamount"], null, { enableHiding: false, decimals: 2, aggFuncOnGrouping: 'sum' });
            this.gridAg.addColumnNumber("yearBudgetDiff", terms["common.reports.drilldown.yearbudgetdiff"], null, { enableHiding: false, decimals: 2, aggFuncOnGrouping: 'sum' });
            //}
            //standard columns
            //else {
                //this.gridAg.addColumnNumber("periodAmount", terms["common.reports.drilldown.periodamount"], null, { enableHiding: true, decimals: 2, aggFuncOnGrouping: 'sum' });
            this.gridAg.addColumnNumber("prevPeriodAmount", terms["common.reports.drilldown.prevperiodamount"], null, { enableHiding: false, decimals: 2, aggFuncOnGrouping: 'sum' });
            this.gridAg.addColumnNumber("periodPrevPeriodDiff", terms["common.reports.drilldown.periodprevperioddiff"], null, { enableHiding: false, decimals: 2, aggFuncOnGrouping: 'sum' });
                //this.gridAg.addColumnNumber("yearAmount", terms["common.reports.drilldown.yearamount"] + " / " + terms["common.reports.drilldown.yearamountshort"], null, { enableHiding: true, decimals: 2, aggFuncOnGrouping: 'sum' });
            this.gridAg.addColumnNumber("prevYearAmount", terms["common.reports.drilldown.prevyearamount"], null, { enableHiding: false, decimals: 2, aggFuncOnGrouping: 'sum' });
            this.gridAg.addColumnNumber("yearPrevYearDiff", terms["common.reports.drilldown.yearprevyeardiff"], null, { enableHiding: false, decimals: 2, aggFuncOnGrouping: 'sum' });
            //}

            //if (!this.totalsGridCreated) {
            //    //Set up totals row
            //    this.gridAg.options.addTotalRow("#totals-grid", {
            //        filtered: this.terms["core.aggrid.totals.filtered"],
            //        total: this.terms["core.aggrid.totals.total"]
            //    });
            //}

            this.gridAg.options.groupRowsByColumnAndHide(groupCol, 'agGroupCellRenderer', 0, true, null, null, true);
            this.gridAg.options.groupRowsByColumnAndHide(headerCol, 'agGroupCellRenderer', 1, true, null, null, true);

            this.gridAg.finalizeInitGrid("common.report.report.report", true);
            this.setData("")

            //Hide/Show
            this.hideShowColumns();
        });

    }

    private hideShowColumns() {
        if (this.sysReportTemplateTypeId == SoeReportTemplateType.BalanceReport) {
            this.gridAg.options.showColumn("openingBalance");
            this.gridAg.options.hideColumn("prevPeriodAmount");
            this.gridAg.options.hideColumn("periodPrevPeriodDiff");
            this.gridAg.options.hideColumn("prevYearAmount");
            this.gridAg.options.hideColumn("yearPrevYearDiff");
            this.gridAg.options.hideColumn("budgetPeriodAmount");
            this.gridAg.options.hideColumn("periodBudgetDiff");
            this.gridAg.options.hideColumn("budgetToPeriodEndAmount");
            this.gridAg.options.hideColumn("yearBudgetDiff");
        }
        else if ((this.sysReportTemplateTypeId == SoeReportTemplateType.ResultReport || this.sysReportTemplateTypeId == SoeReportTemplateType.ResultReportV2) && this.budgetId != 0) {
            this.gridAg.options.showColumn("budgetPeriodAmount");
            this.gridAg.options.showColumn("periodBudgetDiff");
            this.gridAg.options.showColumn("budgetToPeriodEndAmount");
            this.gridAg.options.showColumn("yearBudgetDiff");
            this.gridAg.options.hideColumn("openingBalance");
            this.gridAg.options.hideColumn("prevPeriodAmount");
            this.gridAg.options.hideColumn("periodPrevPeriodDiff");
            this.gridAg.options.hideColumn("prevYearAmount");
            this.gridAg.options.hideColumn("yearPrevYearDiff");
        }
        else {
            this.gridAg.options.showColumn("prevPeriodAmount");
            this.gridAg.options.showColumn("periodPrevPeriodDiff");
            this.gridAg.options.showColumn("prevYearAmount");
            this.gridAg.options.showColumn("yearPrevYearDiff");
            this.gridAg.options.hideColumn("openingBalance");
            this.gridAg.options.hideColumn("budgetPeriodAmount");
            this.gridAg.options.hideColumn("periodBudgetDiff");
            this.gridAg.options.hideColumn("budgetToPeriodEndAmount");
            this.gridAg.options.hideColumn("yearBudgetDiff");
        }
        this.gridAg.options.sizeColumnToFit();
    }

    private openVoucher(row) {
        // Send message to TabsController
        if (this.readPermission || this.modifyPermission)
            this.messagingHandler.publishEditRow({ row: row, openInvoice: false, originType: SoeOriginType.None, editName: row.voucherNr });
    }

    private openInvoice(row) {
        // Send message to TabsController
        if (this.readPermission || this.modifyPermission)
            this.messagingHandler.publishEditRow({ row: row, openInvoice: true, originType: row.invoiceOriginType, editName: row.invoiceNr });
    }

    private loadAccountYears(): ng.IPromise<any> {
        return this.accountingService.getAccountYearDict(false).then((x) => {
            this.accountYearsFrom = x;
            this.accountYearsTo = x;

            this.accountYearFrom = soeConfig.accountYearId;
            this.accountYearFromChanged(soeConfig.accountYearId);
        });
    }

    private loadReports(): ng.IPromise<any> {
        return this.reportService.getDrilldownReportsDict(false, false).then((x) => {
            this.reports = x;
            if (this.reports.length > 0)
                this.selectedReport = this.reports[0];
        });
    }

    private loadBudgets(): ng.IPromise<any> {
        return this.accountingService.getBudgetHeadsForGrid(DistributionCodeBudgetType.AccountingBudget).then((x) => {
            this.budgets.push({ id: 0, name: '' });
            _.forEach(x, (y: any) => {
                this.budgets.push({ id: y.budgetHeadId, name: y.name })
            });
            this.budgetId = 0;
        });
    }

    private getAccountDimStd(): ng.IPromise<any> {
        return this.accountingService.getAccountDimStd().then((x) => {
            this.search['dim1AccountId'] = x.accountDimId;
        });
    }

    private accountYearFromChanged(accountYearId: number) {
        this.accountingService.getAccountPeriods(accountYearId).then((x) => {
            this.accountPeriodsFrom = x;

            _.forEach(this.accountPeriodsFrom, (y) => {
                y.from = new Date(<any>y.from).date();
                y.to = new Date(<any>y.to).date();
            });

            if (this.accountPeriodsFrom.length > 0)
                this.periodFrom = this.accountPeriodsFrom[0];
        });
        this.accountYearTo = accountYearId;
        this.accountYearToChanged(accountYearId);
    }

    private accountYearToChanged(accountYearId: number) {
        this.accountingService.getAccountPeriods(accountYearId).then((x) => {
            this.accountPeriodsTo = x;

            _.forEach(this.accountPeriodsTo, (y) => {
                y.from = new Date(<any>y.from).date();
                y.to = new Date(<any>y.to).date();
            });

            if (this.accountPeriodsTo.length > 0)
                this.periodTo = this.accountPeriodsTo[this.accountPeriodsTo.length - 1];
        });
    }

    private accountPeriodFromChanged(accountPeriod: any) {
        this.periodFrom = accountPeriod;
        if (this.accountYearTo === this.accountYearFrom)
            this.periodTo = _.find(this.accountPeriodsTo, { accountPeriodId: accountPeriod.accountPeriodId });
    }

    private createDrilldown() {
        if (this.reportId > 0 && this.periodFrom && this.periodTo) {
            if (this.periodFrom.from <= this.periodTo.from) {
                this.search['voucherDateFrom'] = this.periodFrom.from;
                this.search['voucherDateTo'] = this.periodTo.to;
                this.progress.startLoadingProgress([() => {
                    return this.reportService.getDrilldownReport(this.reportId, this.periodFrom.accountPeriodId, this.periodTo.accountPeriodId, this.budgetId).then((x) => {
                        _.forEach(x, (y) => {
                            y["expander"] = "";
                        });
                    this.setData(x);
                    }, error => {
                    });
                }]);
            }
            else {
                this.notificationService.showDialog(this.terms["core.warning"], this.terms["common.reports.drilldown.invalidperiods"], SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK);
            }
        }
    }

    private loadVoucherRows(params: any) {
        if (!params.data['rowsLoaded']) {
            this.search['dim1AccountFr'] = params.data.accountNr;
            this.search['dim1AccountTo'] = params.data.accountNr;
            this.progress.startLoadingProgress([() => {
                return this.reportService.getDrilldownVoucherRows(this.search).then((x) => {
                    params.data['rows'] = x;
                    params.data['rowsLoaded'] = true;
                });
            }]).then(() => {
                params.successCallback(params.data['rows']);
            });

        }
        else {
            params.successCallback(params.data['rows']);
        }
    }

    private setDates() {
        var fromPeriod = _.filter(this.accountPeriodsFrom, p => p.id == this.periodFrom);

        var firstDate = new Date(
            parseInt(fromPeriod[0].name.substring(0, 4)),
            parseInt(fromPeriod[0].name.substring(4, 6)) - 1,
            1);

        var toPeriod = _.filter(this.accountPeriodsTo, p => p.id == this.periodTo);

        var lastDate = new Date(
            parseInt(toPeriod[0].name.substring(0, 4)),
            parseInt(toPeriod[0].name.substring(4, 6)),
            0);

        this.search['voucherDateFrom'] = firstDate;
        this.search['voucherDateTo'] = lastDate;
    }

}