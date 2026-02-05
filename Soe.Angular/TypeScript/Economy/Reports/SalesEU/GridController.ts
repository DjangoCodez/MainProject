import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IReportService } from "../../../Core/Services/ReportService";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { Feature, DatePeriodType } from "../../../Util/CommonEnumerations";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { IAccountingService } from "../../../Shared/Economy/Accounting/AccountingService";
import { CoreUtility } from "../../../Util/CoreUtility";
import { ToolBarUtility, ToolBarButton } from "../../../Util/ToolBarUtility";
import { IconLibrary, SOEMessageBoxImage } from "../../../Util/Enumerations";
import { ExportUtility } from "../../../Util/ExportUtility";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { IColumnAggregations } from "../../../Util/SoeGridOptionsAg";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    private _reportPeriod: number;
    get reportPeriod() {
        return this._reportPeriod;
    }
    set reportPeriod(value: number) {
        this._reportPeriod = value;
        switch (this._reportPeriod) {
            case DatePeriodType.Month: this.SetMonths(this.startDate, this.stopDate);//CalendarUtility.getDateToday().addMonths(-5), CalendarUtility.getDateToday());
                break;
            case DatePeriodType.Quarter: this.SetQuarters(this.startDate, this.stopDate);//CalendarUtility.getDateToday().addMonths(-5), CalendarUtility.getDateToday());
                break;
        }
    }

    private _dateSelection: number;
    get dateSelection():number {
        return this._dateSelection;
    }
    set dateSelection(value: number) {
        this._dateSelection = value;
        this.loadGridData();
    }

    private dateSelectionDict: any[];
    private startDate: Date;
    private stopDate: Date;

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private reportService: IReportService,
        private translationService: ITranslationService,
        private urlHelperService: IUrlHelperService,
        private accountingService: IAccountingService,
        private notificationService: INotificationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
    ) {
        super(gridHandlerFactory, "Economy.Distribution.SalesEU", progressHandlerFactory, messagingHandlerFactory);

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

            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData());
    }

    $onInit() {
        this.SetDates().then(() => {
            this.reportPeriod = DatePeriodType.Month;
        });
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;

        this.flowHandler.start({ feature: Feature.Economy_Preferences_Currency, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private SetDates(): ng.IPromise<any> {
        return this.accountingService.getAccountYear(soeConfig.accountYearId).then((x) => {
            this.startDate = CalendarUtility.convertToDate(x.from);
            this.stopDate = CalendarUtility.convertToDate(x.to);
        });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.loadGridData());
        this.toolbar.addInclude(this.urlHelperService.getViewUrl("gridHeader.html"));
        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("economy.reports.createfile", "economy.reports.createfile", IconLibrary.FontAwesome, "fa-download", () => { this.getTaxExportFile(); }, () => { return !this.dateSelection })));
    }

    public setupGrid() {
        const keys: string[] = [
            "common.customer.customer.customernr",
            "common.customer",
            "common.customer.customer.vatnr",
            "economy.reports.valueofsaleofgoods",
            "economy.reports.valueofsaleofservices",
            "economy.reports.valueofTriangulationSales",
            "common.customer.invoices.invoicenr",
            "common.customer.invoices.invoicedate",
            "common.customer.invoices.amountexvat"
        ];

        this.gridAg.options.enableRowSelection = false;

        this.translationService.translateMany(keys).then((terms) => {
            // Details
            this.gridAg.enableMasterDetail(true);
            this.gridAg.options.setDetailCellDataCallback((params) => {
                this.gridAg.detailOptions.enableRowSelection = false;
                this.getSalesEUDetails(params);
            });
            
            this.gridAg.detailOptions.addColumnNumber("invoiceNr", terms["common.customer.invoices.invoicenr"], null, { alignLeft: true, enableHiding: false });
            this.gridAg.detailOptions.addColumnDate("invoiceDate", terms["common.customer.invoices.invoicedate"], null, true);
            this.gridAg.detailOptions.addColumnNumber("totalAmountExVat", terms["common.customer.invoices.amountexvat"], null, { enableHiding: false, decimals: 2 });
            this.gridAg.detailOptions.addColumnNumber("sumGoodsSale", terms["economy.reports.valueofsaleofgoods"], null, { enableHiding: false, decimals: 2 });
            this.gridAg.detailOptions.addColumnNumber("sumServiceSale", terms["economy.reports.valueofsaleofservices"], null, { enableHiding: false, decimals: 2 });
            this.gridAg.detailOptions.finalizeInitGrid();
            
            //Master
            this.gridAg.addColumnText("customerName", terms["common.customer"], null);
            this.gridAg.addColumnText("customerNr", terms["common.customer.customer.customernr"], null);
            this.gridAg.addColumnText("vatNr", terms["common.common.customer.customer.vatnr"], null);

            this.gridAg.addColumnNumber("sumGoodsSale", terms["economy.reports.valueofsaleofgoods"], null, { enableHiding: false, decimals: 2 });
            this.gridAg.addColumnNumber("sumServiceSale", terms["economy.reports.valueofsaleofservices"], null, { enableHiding: false, decimals: 2 });
            this.gridAg.addColumnNumber("sumTriangulationSales", terms["economy.reports.valueofTriangulationSales"], null, { enableHiding: false, decimals: 2 });

            this.gridAg.options.addFooterRow("#sum-footer-grid", {
                "sumGoodsSale": "sum",
                "sumServiceSale": "sum",
                "sumTriangulationSales": "sum"
            } as IColumnAggregations);

            this.gridAg.addStandardMenuItems();
            this.gridAg.setExporterFilenamesAndHeader("economy.accounting.vatcode.vatcodes");
            this.gridAg.options.finalizeInitGrid();
            this.setData('');
        });
    }

    public loadGridData() {
        var startDate = this.GetIntervallStartDate();
        var stopDate = this.GetIntervallStopDate();
        if (startDate && stopDate) {
            this.progress.startLoadingProgress([() => {
                return this.reportService.getSalesEU(startDate, stopDate).then((data) => {
                    for (var i = 0; i < data.length; i++) {
                        var row = data[i];
                        row['expander'] = "";
                    }
                    this.setData(data);
                });
            }]);
        }
    }

    private getSalesEUDetails(params: any) {
        if (!params.data['rowsLoaded']) {
            var startDate = this.GetIntervallStartDate();
            var stopDate = this.GetIntervallStopDate();
            return this.reportService.getSalesEUDetails(params.data.actorId, startDate, stopDate).then((rows) => {
                params.data['rows'] = rows;
                params.data['rowsLoaded'] = true;
                params.successCallback(params.data['rows']);
            });
        }
        else {
            params.successCallback(params.data['rows']);
        }
    }

    private SetMonths(startDate: Date, stopDate: Date) {
        this.dateSelectionDict = []; 

        var locale = CoreUtility.language;
        while (startDate < stopDate)
        {
            var month = startDate.getMonth();
            this.dateSelectionDict.push({ id: startDate.getFullYear().toString() + month.toString(), name: startDate.toLocaleString(locale, { month: "long" }) + " - " + startDate.getFullYear() })
            startDate = startDate.addMonths(1);
        }
    }

    private SetQuarters(startDate: Date, stopDate: Date) {
        this.dateSelectionDict = []; 
        while (startDate < stopDate) {
            var quarter = CalendarUtility.getQuarter(startDate);
            var year = startDate.getFullYear();
            this.dateSelectionDict.push({ id: year.toString() + quarter.toString(), name: quarter + " - " + year })
            startDate = startDate.addMonths(3);
        }
    }

    private GetIntervallStartDate() : Date {
        if (this.dateSelection) {
            var year = Number(this.dateSelection.toString().substr(0, 4));
            var month = Number(this.dateSelection.toString().substr(4, 2));
            if (this.reportPeriod == DatePeriodType.Quarter) {
                month = ((month - 1) * 3);
            }

            const newDate = new Date(year, month, 1, 0, 0, 0, 0);
            return newDate;
        }
    }

    private GetIntervallStopDate(): Date {
        if (this.dateSelection) {
            let stopDate = this.GetIntervallStartDate();

            if (this.reportPeriod == DatePeriodType.Month) {
                stopDate = stopDate.addMonths(1).addDays(-1);
            }
            else if (this.reportPeriod == DatePeriodType.Quarter) {
                stopDate = stopDate.addMonths(3).addDays(-1);
            }
            return stopDate;
        }
    }

    private getTaxExportFile() {
        const startDate = this.GetIntervallStartDate();
        const stopDate = this.GetIntervallStopDate();
        if (this.dateSelection && startDate && stopDate) {
            this.reportService.getSalesEUExportData(this.reportPeriod, startDate, stopDate).then((result) => {
                //if (result.success) {
                    //console.log("result", result);
                    ExportUtility.Export(result, "Periodisk.txt");
                //}
                //else {
                //    //this.translationService.translateMany(keys).then(terms => {
                //        this.notificationService.showDialogEx("Error", result.errorMessage, SOEMessageBoxImage.Error);
                //    //})
                //}
            });
        }
    }
}