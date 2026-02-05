import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IAccountingService } from "../../../Shared/Economy/Accounting/AccountingService";
import { TabMessage } from "../../../Core/Controllers/TabsControllerBase1";
import { EditController as VouchersEditController } from "../../../Shared/Economy/Accounting/Vouchers/EditController";
import { Feature, BalanceErrorStatus, LiquidityPlanningTransactionType, UserSettingType, SettingMainType } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { GroupDisplayType } from "../../../Util/SoeGridOptionsAg";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { EditController as SupplierInvoiceEditController } from "../../../Shared/Economy/Supplier/Invoices/EditController";
import { EditController as CustomerInvoiceEditController } from "../../../Shared/Billing/Invoices/EditController";
import { ToolBarUtility, ToolBarButton } from "../../../Util/ToolBarUtility";
import { IconLibrary } from "../../../Util/Enumerations";
import { ManualTransactionController } from "./Dialogs/ManualTransactionController";
import { LiquidityPlanningDTO } from "../../../Common/Models/LiquidityPlanningDTO";
import { NumberUtility } from "../../../Util/NumberUtility";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { ICoreService } from "../../../Core/Services/CoreService";

declare var agCharts;

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    //modal
    private modalInstance: any;

    fromDate: Date;
    toDate: Date;
    exclusionDate: Date;
    openingBalance = 0;

    valueIn = 0;
    valueOut = 0;
    totalIn = 0;
    totalOut = 0;
    private rows = [];

    paymentStatuses = [];
    selectedPaymentStatuses = [];

    //Chart
    private liquidityLineGraphElem: Element;
    private liquidityLineGraphOptions: any;
    private liquidityLineGraphData: any[] = [];
    private enableGraph: boolean = false;
    private containerWidth: number = 0;
    private chartWidth: number = 0;
    private chartYAxisTerm: string;

    // Flags
    preselectUnpaid: boolean;
    preselectPaidUnchecked: boolean;

    get searchDisabled() {
        return !this.fromDate || !this.toDate;
    }

    // Group column
    autoGroupColumn: any;

    // Grid header and footer
    toolbarInclude: any;
    gridFooterComponentUrl: any;

    //@ngInject
    constructor($uibModal,
        private $timeout: ng.ITimeoutService,
        protected coreService: ICoreService,
        private accountingService: IAccountingService,
        private translationService: ITranslationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private urlHelperService: IUrlHelperService) {
        super(gridHandlerFactory, "Economy.Accounting.LiquidityPlanning", progressHandlerFactory, messagingHandlerFactory);

        this.toolbarInclude = this.urlHelperService.getViewUrl("filterHeader.html");
        this.gridFooterComponentUrl = this.urlHelperService.getViewUrl("gridFooter.html");

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onPermissionsLoaded((feature, readOnly, modify) => {
                this.readPermission = readOnly;
                this.modifyPermission = modify

                if (this.modifyPermission) {
                    this.messagingHandler.publishActivateAddTab();
                }
            })
            .onBeforeSetUpGrid(() => this.loadUserSettings())
            .onSetUpGrid(() => this.onSetUpGrid())
            //.onDoLookUp(() => this.onDoLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onLoadGridData(() => this.loadGridData());

        this.modalInstance = $uibModal;
    }

    public onInit(parameters: any) {
        const today = CalendarUtility.getDateToday();
        this.fromDate = new Date(today.year(), today.getMonth(), 1);
        this.toDate = new Date(today.year(), today.getMonth() + 1, 0);

        this.flowHandler.start({ feature: Feature.Economy_Accounting_LiquidityPlanning, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private loadUserSettings(): ng.IPromise<any> {
        const settingTypes: number[] = [
            UserSettingType.LiquidityPlanningPreSelectUnpaid,
            UserSettingType.LiquidityPlanningPreSelectPaidUnchecked,
        ];

        return this.coreService.getUserSettings(settingTypes).then(x => {
            this.preselectUnpaid = SettingsUtility.getBoolUserSetting(x, UserSettingType.LiquidityPlanningPreSelectUnpaid, false);
            this.preselectPaidUnchecked = SettingsUtility.getBoolUserSetting(x, UserSettingType.LiquidityPlanningPreSelectPaidUnchecked, false);
        });
    }

    private saveSelection() {
        _.forEach(this.paymentStatuses, (s) => {
            this.coreService.saveBoolSetting(SettingMainType.User, s.id, _.some(this.selectedPaymentStatuses, { 'id': s.id }));
        });
    }

    public edit(row: any) {
        if (!row)
            return; 

        if (row.transactionType === LiquidityPlanningTransactionType.CustomerInvoice) {
            this.translationService.translate("common.customerinvoice").then((term) => {
                const message = new TabMessage(
                    `${term} ${row.invoiceNr}`,
                    row.invoiceId,
                    CustomerInvoiceEditController,
                    { id: row.invoiceId },
                    this.urlHelperService.getGlobalUrl("Shared/Billing/Invoices/Views/edit.html")
                );
                this.messagingHandler.publishEvent(Constants.EVENT_OPEN_TAB, message);
            });
        }
        else if (row.transactionType === LiquidityPlanningTransactionType.SupplierInvoice) {
            this.translationService.translate("economy.supplier.invoice.invoice").then((term) => {
                const message = new TabMessage(
                    `${term} ${row.invoiceNr}`,
                    row.invoiceId,
                    SupplierInvoiceEditController,
                    { id: row.invoiceId },
                    this.urlHelperService.getGlobalUrl("Shared/Economy/Supplier/Invoices/Views/edit.html")
                );
                this.messagingHandler.publishEvent(Constants.EVENT_OPEN_TAB, message);
            });
        }
        else if (row.transactionType === LiquidityPlanningTransactionType.Manual){
            this.createEditTransaction(row);
        }
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg, () => { this.search(); });
        this.toolbar.addInclude(this.toolbarInclude);
        
        
        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("economy.accounting.liquidityplanning.manualtransaction", "economy.accounting.liquidityplanning.manualtransactiontooltip", IconLibrary.FontAwesome, "fa-plus", () => {
            this.createEditTransaction(null);
        })));
    }

    private onSetUpGrid() {
        const translationKeys: string[] = [
            "common.date",
            "economy.accounting.liquidityplanning.valuein",
            "economy.accounting.liquidityplanning.valueout",
            "common.balance",
            "economy.accounting.liquidityplanning.transactiontype",
            "economy.accounting.liquidityplanning.specification",   
            "economy.accounting.liquidityplanning.liquidity",
            "economy.supplier.suppliercentral.unpaiedinvoices",
            "economy.accounting.liquidityplanning.paidunchecked",
            "economy.accounting.liquidityplanning.paidchecked"
        ];

        this.translationService.translateMany(translationKeys).then((terms) => {
            this.chartYAxisTerm = terms["economy.accounting.liquidityplanning.liquidity"];

            //Set name
            this.gridAg.options.setName("economy.supplier.invoice.liquidityplanning.liquidityplanning");

            // Prevent double click
            this.doubleClickToEdit = false;

            // Disable selection
            this.gridAg.options.enableRowSelection = false;

            // Enable auto column for grouping
            this.gridAg.options.groupDisplayType = GroupDisplayType.Custom;
            

            this.autoGroupColumn = this.gridAg.addColumnDate("date", terms["common.date"], null, null, null, { enableRowGrouping: true });
            this.autoGroupColumn.rowGroup = true;
            this.autoGroupColumn.hide = true;
            this.gridAg.addColumnText("transactionTypeName", terms["economy.accounting.liquidityplanning.transactiontype"], null, true, { enableRowGrouping: true });
            this.gridAg.addColumnText("specification", terms["economy.accounting.liquidityplanning.specification"], null, true, { enableRowGrouping: true });
            this.gridAg.addColumnNumber("valueIn", terms["economy.accounting.liquidityplanning.valuein"], null, { enableHiding: false, decimals: 2, enableRowGrouping: true, aggFuncOnGrouping: 'sum'});
            this.gridAg.addColumnNumber("valueOut", terms["economy.accounting.liquidityplanning.valueout"], null, { enableHiding: false, decimals: 2, enableRowGrouping: true, aggFuncOnGrouping: 'sum' });
            this.gridAg.addColumnNumber("total", terms["common.balance"], null, { enableHiding: false, decimals: 2, enableRowGrouping: true, aggFuncOnGrouping: 'sum' });
            this.gridAg.options.addColumnEdit(terms["core.edit"], this.edit.bind(this), null, this.showEditButton.bind(this));
            //this.gridAg.addColumnIcon(null, "", null, { icon: "fal fa-pen", onClick: this.edit.bind(this), showIcon: this.showEditButton.bind(this) });

            this.gridAg.options.useGrouping(true, false);
            this.gridAg.finalizeInitGrid("economy.supplier.invoice.liquidityplanning.liquidityplanning", true)

            // Add payment status alternatives
            this.paymentStatuses.push({ id: UserSettingType.LiquidityPlanningPreSelectUnpaid, label: terms["economy.supplier.suppliercentral.unpaiedinvoices"] });
            if (this.preselectUnpaid)
                this.selectedPaymentStatuses.push({ id: UserSettingType.LiquidityPlanningPreSelectUnpaid, label: terms["economy.supplier.suppliercentral.unpaiedinvoices"] });
            this.paymentStatuses.push({ id: UserSettingType.LiquidityPlanningPreSelectPaidUnchecked, label: terms["economy.accounting.liquidityplanning.paidunchecked"] });
            if (this.preselectPaidUnchecked)
                this.selectedPaymentStatuses.push({ id: UserSettingType.LiquidityPlanningPreSelectPaidUnchecked, label: terms["economy.accounting.liquidityplanning.paidunchecked"] });

            this.setData("")
        });
    }

    private showEditButton(row) {
        return row && (row.transactionType === LiquidityPlanningTransactionType.CustomerInvoice || row.transactionType === LiquidityPlanningTransactionType.SupplierInvoice || row.transactionType === LiquidityPlanningTransactionType.Manual);
    }

    private createEditTransaction(row: any) {
        if (!row) {
            row = new LiquidityPlanningDTO();
            row.transactionType = LiquidityPlanningTransactionType.Manual;
        }

        const modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Economy/Accounting/LiquidityPlanning/Dialogs/ManualTransaction.html"),
            controller: ManualTransactionController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'md',
            resolve: {
                translationService: () => { return this.translationService },
                accountingService: () => { return this.accountingService },
                trans: () => { return row },
            }
        });

        modal.result.then((result: any) => {
            if (result) {
                if (result.delete) {
                    this.progress.startDeleteProgress((completion) => {
                        this.accountingService.deleteLiquidityPlanningTransaction(result.item.liquidityPlanningTransactionId).then((data) => {
                            completion.completed(data, true);
                        });
                    }).then(() => {
                        this.search();
                    });
                }
                else if (result.item) {
                    this.progress.startSaveProgress((completion) => {
                        this.accountingService.saveLiquidityPlanningTransaction(result.item).then((data) => {
                            completion.completed(data, true);
                        });
                    }, this.guid).then(() => {
                        this.search();
                    });
                }
            }
        });
    }

    private search() {
        this.progress.startWorkProgress((completion) => {
            this.accountingService.getLiquidityPlanning(this.fromDate, this.toDate, this.exclusionDate, this.openingBalance, _.some(this.selectedPaymentStatuses, { 'id': UserSettingType.LiquidityPlanningPreSelectUnpaid }), _.some(this.selectedPaymentStatuses, { 'id': UserSettingType.LiquidityPlanningPreSelectPaidUnchecked }), _.some(this.selectedPaymentStatuses, { 'id': UserSettingType.LiquidityPlanningPreSelectPaidChecked })).then((data) => {
                this.enableGraph = data == undefined ? false : true;
                this.rows = data;
                _.forEach(data, (row) => {
                    row.date = CalendarUtility.convertToDate(row.date);
                });
                this.gridAg.setData(data);
                this.setData(data)
                this.Summarize(data);
                this.createCharts();
                this.dataWasUpdated()
                completion.completed(data, true);

            }, error => {
                completion.failed(error.message);
            });
        });
    }

    private searchNew() {
        this.progress.startWorkProgress((completion) => {
            this.accountingService.getLiquidityPlanningNew(this.fromDate, this.toDate, this.exclusionDate, this.openingBalance, _.some(this.selectedPaymentStatuses, { 'id': UserSettingType.LiquidityPlanningPreSelectUnpaid }), _.some(this.selectedPaymentStatuses, { 'id': UserSettingType.LiquidityPlanningPreSelectPaidUnchecked }), _.some(this.selectedPaymentStatuses, { 'id': UserSettingType.LiquidityPlanningPreSelectPaidChecked })).then((data) => {
                this.enableGraph = data == undefined ? false : true;
                this.rows = data;
                _.forEach(data, (row) => {
                    row.date = CalendarUtility.convertToDate(row.date);
                });
                this.gridAg.setData(data);
                this.setData(data)
                this.Summarize(data);
                this.createCharts();
                this.dataWasUpdated()
                completion.completed(data, true);

            }, error => {
                completion.failed(error.message);
            });
        });
    }

    private Summarize(data) {
        this.valueIn = 0;
        this.valueOut = 0;
        this.totalIn = 0;
        this.totalOut = 0;

        let first: boolean = true;
        _.forEach(data, (y: any) => {
            if (first) {
                this.totalIn = y.total;
                first = false;
            }

            if (y.transactionType === LiquidityPlanningTransactionType.CustomerInvoice || y.transactionType === LiquidityPlanningTransactionType.SupplierInvoice || y.transactionType === LiquidityPlanningTransactionType.Manual) {
                this.valueIn += y.valueIn;
                this.valueOut -= y.valueOut;
                this.totalOut += y.total;
            }
            else {
                this.totalOut = y.total;
            }
        });
    }

    public loadGridData() {
        this.setData(''); //height modification
        this.dataWasUpdated()
    }

    private dataWasUpdated() {
        this.messagingHandler.publishEvent('liquidityDataFiltered', { rows: this.gridAg.options.getFilteredRows(), totalCount: this.gridAg.options.getData().length });
    }
    //chart
    private setLiquidityLineGraphData() {
        this.liquidityLineGraphData = []
        const sumPerDay = {}
        this.rows.forEach(r => {
            const date = r.date//r.date.toISOString().split('T')[0]
            //sumPerDay[date] = sumPerDay[date] == undefined ? r.total : sumPerDay[date] + r.total
            if (!sumPerDay[date]) {
                sumPerDay[date] = { total: r.total, rows: [] }
            }
            else {
                sumPerDay[date].total += r.total;
            }
            if (r.transactionType > 1) {
                sumPerDay[date].rows.push(r)
            }
        })

        Object.keys(sumPerDay).forEach(e => {
            this.liquidityLineGraphData.push({ date: new Date(e), outgoingLiquidity: sumPerDay[e].total, rows: sumPerDay[e].rows })
        });
    }

    private createCharts() {
        if (!agCharts)
            return;

        if (this.liquidityLineGraphElem)
            this.liquidityLineGraphElem.innerHTML = '';
        else
            this.liquidityLineGraphElem = document.querySelector('#liquidityLineGraph');

        this.setLiquidityLineGraphData();
        this.liquidityLineGraphOptions = this.createDefaultPieChart(this.liquidityLineGraphElem, this.liquidityLineGraphData);
        agCharts.AgChart.create(this.liquidityLineGraphOptions);
    }

    private createDefaultPieChart(container: Element, data: any[]) {
        const options = {
            container: container,
            data: data,
            fontFamily: 'Roboto Condensed',
            fontSize: 14,
            autoSize: true,
            padding: {
                top: 20,
                right: 40,
                bottom: 20,
                left: 20,
            },
            navigator: {
                enabled: false,
            },
            legend: {
                enabled: false
            },
            background: {
                visible: true
            },
            series: [{
                type: 'line',
                xKey: 'date',
                yKey: 'outgoingLiquidity',
                tooltipRenderer: function (params) {
                    const htmlStart = '<div class="ag-chart-tooltip-content" style="border-top: 1px solid #CCCCCC">';
                    const divEnd = '</div>';
                    const rowsLen = 12
                    const rows = params.datum.rows;
                    const date = CalendarUtility.toFormattedDate(params.datum.date);
                    const value = NumberUtility.printDecimal(params.datum.outgoingLiquidity, 2, 2);
                    let html = "";

                    const items = rows.length > rowsLen ? rows.slice(0, rowsLen) : rows;

                    items.forEach(r => {
                        html += htmlStart + r.transactionTypeName + ": " + r.specification + " (" + NumberUtility.printDecimal(r.total, 2, 2) + ")" + divEnd;
                    })

                    if (rows.length > rowsLen) {
                        html += htmlStart + "..." + divEnd;
                    }

                    return '<div class="ag-chart-tooltip-title" style="background-color: #e3e3e3; color: #333333">' +
                        date + " (" + value + ")" +
                        divEnd +
                        html;
                },
            }],
            axes: [
                {
                    position: 'bottom',
                    type: 'time',
                    label: {
                        formatter: function (params) {
                            return CalendarUtility.toFormattedDate(params.value)
                        },
                    }
                },
                {
                    position: 'left',
                    type: 'number',
                    title: {
                        text: this.chartYAxisTerm,
                    },
                    label: {
                        formatter: function (params) {
                            return NumberUtility.printDecimal(params.value, 0, 0)
                        }
                    }
                },
            ],
        };

        return options;
    }
}
