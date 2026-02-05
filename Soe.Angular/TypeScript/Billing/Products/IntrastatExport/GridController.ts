import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { Feature, IntrastatReportingType, SoeOriginType } from "../../../Util/CommonEnumerations";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { IconLibrary, SOEMessageBoxButtons, SOEMessageBoxImage } from "../../../Util/Enumerations";
import { ToolBarButton, ToolBarUtility } from "../../../Util/ToolBarUtility";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { ICoreService } from "../../../Core/Services/CoreService";
import { StringUtility } from "../../../Util/StringUtility";
import { HtmlUtility } from "../../../Util/HtmlUtility";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { Constants } from "../../../Util/Constants";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {
    // Collections
    private selectionTypes: any[];

    // Selections
    private selectedDateFrom: Date;
    private selectedDateTo: Date;
    private selectedType: IntrastatReportingType;

    // Flags
    gridDataLoaded = false;

    //Permissions
    private editCustomerInvoice: boolean = false;
    private editSupplierInvoice: boolean = false;

    //@ngInject
    constructor(
        private $window: ng.IWindowService,
        private coreService: ICoreService,
        private translationService: ITranslationService,
        private urlHelperService: IUrlHelperService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        private notificationService: INotificationService) {
        super(gridHandlerFactory, "common.intrastat.reportingandexport", progressHandlerFactory, messagingHandlerFactory)

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(response => this.onPermissionsLoaded(response))
            .onSetUpGrid(() => this.setupGrid())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onLoadGridData(() => this.search());
    }

    public onInit(parameters: any) {
        this.messagingHandler.onGridDataReloadRequired(x => { this.search(); });

        this.flowHandler.start([
            { feature: Feature.Economy_Intrastat_ReportsAndExport, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Economy_Supplier_Invoice_Invoices_Edit, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Economy_Customer_Invoice_Invoices_Edit, loadReadPermissions: true, loadModifyPermissions: true }
        ]);
    }

    private onPermissionsLoaded(permissions: IPermissionRetrievalResponse) {
        this.readPermission = permissions[Feature.Economy_Intrastat_ReportsAndExport].readPermission;
        this.modifyPermission = permissions[Feature.Economy_Intrastat_ReportsAndExport].modifyPermission;

        if (this.modifyPermission) {
            // Set dates
            const today: Date = CalendarUtility.getDateToday();
            this.selectedDateFrom = new Date(today.getFullYear(), today.getMonth() - 1, 1);
            this.selectedDateTo = new Date(today.getFullYear(), today.getMonth(), 0);

            // Set selection type
            this.selectedType = IntrastatReportingType.Both;
        }

        this.editCustomerInvoice = permissions[Feature.Economy_Customer_Invoice_Invoices_Edit].modifyPermission;
        this.editSupplierInvoice = permissions[Feature.Economy_Supplier_Invoice_Invoices_Edit].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg, () => this.search());

        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("economy.reports.createfile", "economy.reports.createfile", IconLibrary.FontAwesome, "fal fa-download", () => {
            this.createExportFile();
        }, () => {
            return this.selectedType === IntrastatReportingType.Both;
        })));

        this.toolbar.addInclude(this.urlHelperService.getViewUrl("searchHeader.html"));
    }

    public search() {
        if (!this.selectedDateFrom || !this.selectedDateTo)
            return;

        this.gridAg.clearData();
        this.progress.startLoadingProgress([() => {
            return this.coreService.getIntrastatTransactionsForExport(this.selectedType, this.selectedDateFrom, this.selectedDateTo).then((x) => {
                this.setData(x);
                this.gridAg.options.setAllGroupExpended(true, 0);
            });
        }]);
    }

    private setupGrid() {
        this.gridAg.options.enableRowSelection = false;

        const translationKeys: string[] = [
            "common.type",
            "common.name",
            "common.intrastat.vatnr",
            "common.country",
            "common.countryoforigin",
            "common.customer.invoices.productnr",
            "common.customer.invoices.productname",
            "common.customer.invoices.quantity",
            "common.customer.invoices.unit",
            "common.commoditycodes.netweight",
            "common.commoditycodes.otherquantity",
            "common.commoditycodes.code",
            "economy.accounting.liquidityplanning.transactiontype",
            "common.customer.invoices.seqnr",
            "common.number",
            "common.customer.invoices.invoicedate",
            "common.voucherdate",
            "common.customer.invoices.amount",
            "common.customer.invoices.foreignamount",
            "common.customer.invoices.currencycode",
            "common.startdate",
            "common.stopdate",
            "common.intrastat.import",
            "common.intrastat.export",
            "common.intrastat.both"
        ];

        this.translationService.translateMany(translationKeys).then((terms) => {
            this.selectionTypes = [];
            this.selectionTypes.push({ id: IntrastatReportingType.Import, name: terms["common.intrastat.import"] });
            this.selectionTypes.push({ id: IntrastatReportingType.Export, name: terms["common.intrastat.export"] });
            this.selectionTypes.push({ id: IntrastatReportingType.Both, name: terms["common.intrastat.both"] });

            const originTypeCol = this.gridAg.addColumnText("originTypeName", terms["common.type"], null, false, { enableRowGrouping: true, enableHiding: false });
            originTypeCol.rowGroup = true;

            this.gridAg.addColumnText("name", terms["common.name"], null, false, { enableRowGrouping: true, enableHiding: false });
            this.gridAg.addColumnText("vatNr", terms["common.intrastat.vatnr"], null, false, { enableRowGrouping: true, enableHiding: false });
            this.gridAg.addColumnText("country", terms["common.country"], null, false, { enableRowGrouping: true, enableHiding: false });
            const originCountryCol = this.gridAg.addColumnText("originCountry", terms["common.countryoforigin"], null, false, { enableRowGrouping: true, enableHiding: false });
            originCountryCol.rowGroup = true;

            this.gridAg.addColumnText("productNr", terms["common.customer.invoices.productnr"], null, true, { enableRowGrouping: true, enableHiding: true });
            this.gridAg.addColumnText("productName", terms["common.customer.invoices.productname"], null, true, { enableRowGrouping: true, enableHiding: true });
            this.gridAg.addColumnNumber("quantity", terms["common.customer.invoices.quantity"], null, { enableHiding: true, aggFuncOnGrouping: 'sum' });
            this.gridAg.addColumnText("productUnitCode", terms["common.customer.invoices.unit"], null, true, { enableHiding: true });
            this.gridAg.addColumnNumber("netWeight", terms["common.commoditycodes.netweight"], null, { enableHiding: false, decimals: 3, editable: true, maxDecimals: 3, aggFuncOnGrouping: 'sum' });
            this.gridAg.addColumnText("otherQuantity", terms["common.commoditycodes.otherquantity"], null, true, { enableHiding: false, editable: false });
            this.gridAg.addColumnText("intrastatCodeName", terms["common.commoditycodes.code"], null, true, { enableRowGrouping: true, enableHiding: false });
            this.gridAg.addColumnText("intrastatTransactionTypeName", terms["economy.accounting.liquidityplanning.transactiontype"], null, true, { enableRowGrouping: true, enableHiding: false });
            this.gridAg.addColumnText("seqNr", terms["common.customer.invoices.seqnr"], null, true, { enableHiding: true });
            this.gridAg.addColumnText("originNr", terms["common.number"], null, true, { enableRowGrouping: true, enableHiding: true });
            this.gridAg.addColumnDate("invoiceDate", terms["common.customer.invoices.invoicedate"], null, true, null, { enableHiding: true, enableRowGrouping: true });
            this.gridAg.addColumnDate("voucherDate", terms["common.voucherdate"], null, true, null, { enableHiding: true, enableRowGrouping: true });
            this.gridAg.addColumnNumber("amount", terms["common.customer.invoices.amount"], null, { enableHiding: true, decimals: 2, aggFuncOnGrouping: 'sum' });
            this.gridAg.addColumnNumber("amountCurrency", terms["common.customer.invoices.foreignamount"], null, { enableHiding: true, decimals: 2, aggFuncOnGrouping: 'sum' });
            this.gridAg.addColumnText("currencyCode", terms["common.customer.invoices.currencycode"], null, true);
            this.gridAg.addColumnEdit("edit", this.editRow.bind(this), false, row => this.editable(row));

            this.gridAg.options.useGrouping(true, false, { keepColumnsAfterGroup: false, selectChildren: false });

            this.gridAg.finalizeInitGrid("common.intrastat.reportingandexport", false);
        });
    }

    private createExportFile() {
        this.progress.startWorkProgress((completion) => {
            let selection = {};
            selection['Special'] = this.selectedType === IntrastatReportingType.Export ? "export" : "import";
            selection['DateFrom'] = this.selectedDateFrom;
            selection['DateTo'] = this.selectedDateTo;
            selection['HasDateInterval'] = true;

            return this.coreService.createIntrastatExport(selection).then((result) => {
                if (result.success) {
                    if (!StringUtility.isEmpty(result.stringValue)) {
                        HtmlUtility.openInSameTab(this.$window, result.stringValue);
                    }
                    else {
                        const keys: string[] = [
                            "core.info",
                            "core.noresultfromselection"
                        ];

                        this.translationService.translateMany(keys).then((terms) => {
                            this.notificationService.showDialog(terms["core.info"], terms["core.noresultfromselection"], SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK);
                        });
                    }
                }
                completion.completed("", true)
            });
        });
    }

    private editRow(row) {
        this.messagingHandler.publishEvent(Constants.EVENT_OPEN_INVOICE, { originType: row.originType, associatedId: row.originId, tabSuffix: row.originNr });
    }

    private editable(row) {
        if (!row) return false;
        if (row.originType === SoeOriginType.SupplierInvoice)
            return this.editSupplierInvoice;
        if (row.originType === SoeOriginType.CustomerInvoice)
            return this.editCustomerInvoice;
    }   
}