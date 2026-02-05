import { GridControllerBase } from "../../../../Core/Controllers/GridControllerBase";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../../Core/Services/UrlHelperService";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { ICommonCustomerService } from "../../../../Common/Customer/CommonCustomerService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { GridEvent } from "../../../../Util/SoeGridOptions";
import { SoeGridOptionsEvent, IconLibrary } from "../../../../Util/Enumerations";
import { ToolBarUtility, ToolBarButton } from "../../../../Util/ToolBarUtility";
import { Feature, TermGroup, SoeOriginType } from "../../../../Util/CommonEnumerations";

export class GridController extends GridControllerBase {

    public searchModel: any = {};
    currencyTypes: any[];
    private customerInvoicePermission: boolean;

    // Data        

    originTypes: any[];
    defaultFilterOriginType: string;
    allItemsSelectionDict: any[];

    filteredTotal: number = 0;
    selectedTotal: number = 0;

    toolbarInclude: any;

    private _allItemsSelection: any;

    get allItemsSelection() {
        return this._allItemsSelection;
    }

    set allItemsSelection(item: any) {
        this._allItemsSelection = item;
        //if (this.setupComplete)
        //    this.updateItemsSelection();
    }

    private terms;

    //@ngInject
    constructor($http,
        $templateCache,
        $timeout: ng.ITimeoutService,
        $uibModal,
        private $filter: ng.IFilterService,
        coreService: ICoreService,
        translationService: ITranslationService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        uiGridConstants: uiGrid.IUiGridConstants,
        private $q: ng.IQService) {

        super("Soe.Economy.Customer.Invoice.Statistics", "economy.customer.invoice.statistics.statistics", Feature.Economy_Customer_Invoice_Statistics, $http, $templateCache, $timeout, $uibModal, coreService, translationService, urlHelperService, messagingService, notificationService, uiGridConstants);

        this.toolbarInclude = urlHelperService.getViewUrl("searchHeader.html");

        this.$q.all([
            this.loadModifyPermissions(),
            this.loadSelectionTypes(),
            this.loadOriginTypes(),
        ]).then(x => this.setupStatisticsGrid());
    }

    private loadCustomerStatistics() {
        this.startLoad();

        var keys: string[] = [
            "common.customerinvoice",
            "common.order",
            "common.offer",
            "common.contract",
        ];

        this.translationService.translateMany(keys).then((terms) => {

            /*this.commonCustomerService.getCustomerStatisticsAllCustomers(this.allItemsSelection).then((x) => {

                //console.log("getCustomerStatisticsAllCustomers: ", x);
                
                _.forEach(x, (y) => {
                    switch (y.originType) {
                        case SoeOriginType.CustomerInvoice:
                            y.originType = terms["common.customerinvoice"];
                            break;
                        case SoeOriginType.Order:
                            y.originType = terms["common.order"];
                            break;
                        case SoeOriginType.Offer:
                            y.originType = terms["common.offer"];
                            break;
                        case SoeOriginType.Contract:
                            y.originType = terms["common.contract"];
                            break;
                    }
                });

                super.gridDataLoaded(x);

            });*/
        });

    }

    public setupStatisticsGrid() {

        this.soeGridOptions.subscribe([new GridEvent(SoeGridOptionsEvent.RowSelectionChanged, (row: uiGrid.IGridRow) => {
            this.selectedTotal = 0;
            this.soeGridOptions.getSelectedRows().forEach(row => this.selectedTotal += row.sumAmount);
        }), new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (row: uiGrid.IGridRow) => {
            this.selectedTotal = 0;
            this.soeGridOptions.getSelectedRows().forEach(row => this.selectedTotal += row.totalAmountCurrency);
        }), new GridEvent(SoeGridOptionsEvent.RowsVisibleChanged, (rows: uiGrid.IGridRow[]) => {
            this.filteredTotal = 0;
            rows.forEach(row => this.filteredTotal += +row.entity.sumAmount);
        })]);

        this.buttonGroups.push(ToolBarUtility.createGroup(new ToolBarButton("core.search", "core.search", IconLibrary.FontAwesome, "fal fa-search", () => {
            this.loadCustomerStatistics();
        })));

        this.soeGridOptions.showColumnFooter = true;
        // Columns
        var keys: string[] = [
            "common.number",
            "common.type",
            "common.invoicenr",
            "common.productnr",
            "common.name",
            "common.quantity",
            "common.amount",
            "common.purchaseprice",
            "common.price",
            "common.date",
            "common.customer.customer.marginalincome",
            "common.customer.customer.marginalincomeratio",
            "common.customer.customer.customername",
            "common.customer.customer.customercategory",
            "common.customer.customer.productcategory",
            "common.customer.customer.customernr",
            "common.contactaddresses.addressrow.postaladdress",
            "common.contactaddresses.addressrow.postalcode",
            "common.customer.invoices.totalfiltered",
            "common.customer.invoices.totalselected"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;

            super.addColumnText("customerNr", terms["common.customer.customer.customernr"], null, true);
            super.addColumnText("customerName", terms["common.customer.customer.customername"], null, true);

            super.addColumnText("customerPostalAddress", terms["common.contactaddresses.addressrow.postaladdress"], null, true);
            super.addColumnText("customerPostalCode", terms["common.contactaddresses.addressrow.postalcode"], null, true);

            super.addColumnDate("date", terms["common.date"], null, true);
            super.addColumnSelect("originType", terms["common.type"], null, this.originTypes, true);
            // Default set originType filter on "SoeOriginType.CustomerInvoice"
            if (this.defaultFilterOriginType != "") {
                var colindex = this.soeGridOptions.getColumnIndex('originType');
                var defCol = this.soeGridOptions.getColumnDefs()[colindex];
                defCol.filter.term = this.defaultFilterOriginType;
            }

            super.addColumnText("invoiceNr", terms["common.invoicenr"], null, true);
            super.addColumnText("productNr", terms["common.productnr"], null, true);
            super.addColumnText("productName", terms["common.name"], null, true);
            var quantitySum = super.addColumnNumber("productQuantity", terms["common.quantity"], null);
            quantitySum.aggregationType = this.uiGridConstants.aggregationTypes.sum;
            this.addSumFilteredAndSelectedFooterToColumns(quantitySum);

            super.addColumnText("customerCategory", terms["common.customer.customer.customercategory"], null, true);
            super.addColumnText("productCategory", terms["common.customer.customer.productcategory"], null, true);

            //TODO add more columns

            //super.addColumnText("contractCategory", terms["common.customer.customer.contractcategory"] null, true);
            //super.addColumnText("project", terms["common.customer.customer.contractcategory"] null, true);
            //super.addColumnText("costcentre", terms["common.customer.customer.contractcategory"] null, true);
            //super.addColumnText("wholeseller", terms["common.customer.customer.contractcategory"] null, true);

            var amountSum = super.addColumnNumber("productPrice", terms["common.price"], null, true, 2);
            amountSum.aggregationType = this.uiGridConstants.aggregationTypes.sum;
            this.addSumFilteredAndSelectedFooterToColumns(amountSum);

            var sumAmountSum = super.addColumnNumber("productSumAmount", terms["common.amount"], null, true, 2);
            sumAmountSum.aggregationType = this.uiGridConstants.aggregationTypes.sum;
            this.addSumFilteredAndSelectedFooterToColumns(sumAmountSum);

            var purchasePriceSum = super.addColumnNumber("productPurchasePrice", terms["common.purchaseprice"], null, true, 2);
            purchasePriceSum.aggregationType = this.uiGridConstants.aggregationTypes.sum;
            this.addSumFilteredAndSelectedFooterToColumns(purchasePriceSum);

            var marginalIncome = super.addColumnNumber("productMarginalIncome", terms["common.customer.customer.marginalincome"], null, true, 2);
            marginalIncome.aggregationType = this.uiGridConstants.aggregationTypes.sum;
            this.addSumFilteredAndSelectedFooterToColumns(marginalIncome);

            super.addColumnNumber("productMarginalRatio", terms["common.customer.customer.marginalincomeratio"], null, true, 2);

            super.addColumnText("productCategory", terms["common.customer.customer.productcategory"], null, true);

            this.addFilteredAndSelectedTotalsToFirstColumn(terms["common.customer.invoices.totalfiltered"], terms["common.customer.invoices.totalselected"]);

            this.stopProgress();
        });
    }

    private addSumFooter(column: uiGrid.IColumnDefOf<any>) {
        column.aggregationType = this.uiGridConstants.aggregationTypes.sum;
        column.aggregationHideLabel = true;
        column.width = "100";
        this.addSumAggregationFooterToColumns(column);
    }

    private loadModifyPermissions(): ng.IPromise<any> {
        var featureIds: number[] = [];
        featureIds.push(Feature.Economy_Customer_Invoice);

        return this.coreService.hasReadOnlyPermissions(featureIds)
            .then((x) => {
                if (x[Feature.Economy_Customer_Invoice]) {
                    this.customerInvoicePermission = true;
                }
            });
    }

    private loadSelectionTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.ChangeStatusGridAllItemsSelection, false, true, true).then((x) => {
            this.allItemsSelectionDict = x;
        });
    }

    private loadOriginTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.OriginType, false, false).then((x) => {
            this.originTypes = [];
            _.forEach(x, (row) => {
                if (row.id === SoeOriginType.CustomerInvoice ||
                    row.id === SoeOriginType.Order ||
                    row.id === SoeOriginType.Offer ||
                    row.id === SoeOriginType.Contract)
                    this.originTypes.push({ value: row.name, label: row.name });
                if (row.id === SoeOriginType.CustomerInvoice)
                    this.defaultFilterOriginType = row.name;
            });
        });
    }

    public search() {
        //this.startWork();                        
        this.loadCustomerStatistics();
    }

    public loadGridData() {
        //wait for the user to select a customer (and selection) and manually click the search button
        this.stopProgress();
    }

    private setupToolBar() {
        this.setupDefaultToolBar(false); //no refresh and clearfilter buttons
    }
}
