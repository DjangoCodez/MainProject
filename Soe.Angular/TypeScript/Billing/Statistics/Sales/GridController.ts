import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { SoeGridOptionsEvent } from "../../../Util/Enumerations";
import { ICommonCustomerService } from "../../../Common/Customer/CommonCustomerService";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { SoeOriginType, Feature, TermGroup } from "../../../Util/CommonEnumerations";
import { AccountDimSmallDTO } from "../../../Common/Models/AccountDimDTO";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { Constants } from "../../../Util/Constants";
import { ICustomerStatisticsDTO } from "../../../Scripts/TypeLite.Net4";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    public searchModel: any = {};
    currencyTypes: any[];
    private customerInvoicePermission: boolean;

    // Data             
    originTypes: any[];
    orderTypes: any[];
    accountDims: AccountDimSmallDTO[];
    allItemsSelectionDict: any[];

    // Summaries
    selectedQuantity = 0;
    selectedPrice = 0;
    selectedAmount = 0;
    selectedPurchasePrice = 0;
    selectedMarginalIncome = 0;
    filteredQuantity = 0;
    filteredPrice = 0;
    filteredAmount = 0;
    filteredPurchasePrice = 0;
    filteredMarginalIncome = 0;

    toolbarInclude: any;
    gridFooterComponentUrl: any;

    // Columns
    private orderCategoriesColumn: any;
    private contractCategoriesColumn: any;
    private attestStateColumn: any;
    private accountDim1ColumnName: string;

    // Terms
    terms: { [index: string]: string; };

    // Column labels
    numbersColumnHeader: string = "";
    dateColumnHeader: string = "";

    // Selection
    private selectedDateFrom: Date;
    private selectedDateTo: Date;
    private _selectedOriginType: any;
    get selectedOriginType() {
        return this._selectedOriginType;
    }
    set selectedOriginType(item: any) {
        this._selectedOriginType = item;
        if (this.selectedOriginType) {
            if (this.selectedOriginType === SoeOriginType.Order && this.orderCategoriesColumn && this.contractCategoriesColumn) {
                this.gridAg.options.showColumn("attestStateName");
                this.gridAg.options.showColumn("orderTypeName");
                this.gridAg.options.showColumn("orderCategory"); 
                this.gridAg.options.hideColumn("contractCategory");
                this.gridAg.options.sizeColumnToFit();
                this.setData('');
            }
            else if (this.selectedOriginType === SoeOriginType.Contract && this.contractCategoriesColumn && this.orderCategoriesColumn) {
                this.gridAg.options.hideColumn("attestStateName");
                this.gridAg.options.hideColumn("orderTypeName");
                this.gridAg.options.hideColumn("orderCategory");
                this.gridAg.options.showColumn("contractCategory");
                this.gridAg.options.sizeColumnToFit();
                this.setData('');
            }
            else if (this.selectedOriginType === SoeOriginType.Offer && this.attestStateColumn) {
                this.gridAg.options.showColumn("attestStateName");
                this.gridAg.options.hideColumn("orderTypeName");
                this.gridAg.options.hideColumn("orderCategory");
                this.gridAg.options.hideColumn("contractCategory");
                this.gridAg.options.sizeColumnToFit();
                this.setData('');
            }
            else {
                if (this.contractCategoriesColumn && this.orderCategoriesColumn) {
                    this.gridAg.options.hideColumn("attestStateName");
                    this.gridAg.options.hideColumn("orderTypeName");
                    this.gridAg.options.hideColumn("orderCategory");
                    this.gridAg.options.hideColumn("contractCategory");
                    this.gridAg.options.sizeColumnToFit();
                    this.setData('');
                }
            }

            //this.setHeaderLabels();
        }
    }

    //@ngInject
    constructor(
        private $timeout: ng.ITimeoutService,
        private coreService: ICoreService,
        private commonCustomerService: ICommonCustomerService,
        private translationService: ITranslationService,
        private urlHelperService: IUrlHelperService,
        private $q: ng.IQService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {

        super(gridHandlerFactory, "Soe.Billing.Statistics.Sales", progressHandlerFactory, messagingHandlerFactory);

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
            .onBeforeSetUpGrid(() => this.doBeforeGrid())
            .onDoLookUp(() => this.doLookup())
            .onSetUpGrid(() => this.setupStatisticsGrid())
            .onLoadGridData(() => this.setData(""));

        this.doubleClickToEdit = false;
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.loadCustomerStatistics(); });
        }

        // Set footer
        this.gridFooterComponentUrl = this.urlHelperService.getViewUrl("gridFooter.html");

        this.flowHandler.start({ feature: Feature.Billing_Statistics, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg as IGridHandler, () => this.loadCustomerStatistics());
        this.toolbar.addInclude(this.urlHelperService.getViewUrl("searchHeader.html"));
    }

    private loadModifyPermissions(): ng.IPromise<any> {
        const featureIds: number[] = [Feature.Economy_Customer_Invoice];

        return this.coreService.hasReadOnlyPermissions(featureIds)
            .then((x) => {
                if (x[Feature.Economy_Customer_Invoice]) {
                    this.customerInvoicePermission = true;
                }
            });
    }

    private doBeforeGrid(): ng.IPromise<any>
    {
        return this.loadAccountDims();
    }

    private doLookup(): ng.IPromise<any> {
        return this.$q.all([
            this.loadModifyPermissions(),
            this.loadSelectionTypes(),
            this.loadOriginTypes(),
            this.loadOrderTypes(),
        ]).then(() => {
            // Set dates
            const today: Date = CalendarUtility.getDateToday();
            this.selectedDateTo = today;
            this.selectedDateFrom = new Date(today.getFullYear(), today.getMonth() - 1, 1);
        });
    }

    public setupStatisticsGrid() {
        // Columns
        const keys: string[] = [
            "common.number",
            "common.type",
            "common.invoicenr",
            "common.productnr",
            "common.name",
            "common.quantity",
            "common.customer.invoices.amountexvat",
            "common.purchaseprice",
            "common.price",
            "common.date",
            "common.customer.customer.marginalincome",
            "common.customer.customer.marginalincomeratio",
            "common.customer",
            "common.customer.customer.customercategory",
            "common.customer.customer.productcategory",
            "common.customer.customer.ordercategory",
            "common.customer.customer.contractcategory",
            "common.customer.customer.customernr",
            "common.contactaddresses.addressrow.postaladdress",
            "common.contactaddresses.addressrow.postalcode",
            "common.customer.customer.filtered",
            "common.customer.customer.selected",
            "common.customer.customer.ordercostcentre",
            "common.customer.customer.orderproject",
            "common.customer.customer.wholesellername",
            "common.customer.customer.marginalincomeratioprocent",
            "common.customerinvoice",
            "common.startdate",
            "common.customer.invoices.invoicedate",
            "billing.offer.offerdate",
            "billing.order.invoicedate",
            "common.order",
            "common.offer",
            "common.contract",
            "billing.order.ordertype",
            "billing.order.owners",
            "billing.order.selectusers.responsible",
            "billing.order.ourreference",
            "common.customer.invoices.articlename",
            "common.customer.invoices.currencycode",
            "common.customer.invoices.foreignamount",
            "common.country",
            "common.customer.customer.payingcustomer",
            "common.customer.invoices.ordernr",
            "common.customer.invoices.rowstatus",
            "billing.product.materialcode",
            "billing.product.productgroup",
            "billing.product.productcategories",
            "billing.product.headproductcategories"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;

            // Set name
            this.gridAg.options.setName("statisticsGrid");

            // Add aggregation
            this.gridAg.options.addGroupAverageAggFunction();

            this.gridAg.addColumnText("customerName", terms["common.customer"], null, true, { enableRowGrouping: true, enableHiding: true });
            this.gridAg.addColumnText("customerPostalAddress", terms["common.contactaddresses.addressrow.postaladdress"], null, true, { enableRowGrouping: true, enableHiding: true });
            this.gridAg.addColumnText("customerPostalCode", terms["common.contactaddresses.addressrow.postalcode"], null, true, { enableRowGrouping: true, enableHiding: true });
            this.gridAg.addColumnText("customerCountry", terms["common.country"], null, true, { enableRowGrouping: true, enableHiding: true });
            this.gridAg.addColumnText("orderNr", terms["common.customer.invoices.ordernr"], null, true, { enableRowGrouping: true, enableHiding: true });
            this.gridAg.addColumnText("payingCustomerName", terms["common.customer.customer.payingcustomer"], null, true, { enableRowGrouping: true, enableHiding: true });
            
            this.gridAg.addColumnDate("date", terms["common.date"], null, true, null, { enableRowGrouping: true, enableHiding: true });
            this.gridAg.addColumnText("invoiceNr", terms["common.number"], null, true, { enableRowGrouping: true, enableHiding: true });

            this.gridAg.addColumnText("productName", terms["common.customer.invoices.articlename"], null, true, { enableRowGrouping: true });
            this.attestStateColumn = this.gridAg.addColumnSelect("attestStateName", this.terms["common.customer.invoices.rowstatus"], null, {
                selectOptions: null, populateFilterFromGrid: true, toolTipField: "attestStateName", displayField: "attestStateName", shape: Constants.SHAPE_CIRCLE, shapeValueField: "attestStateColor", colorField: "attestStateColor", enableHiding: true, enableRowGrouping: true,
            });
            this.attestStateColumn.hide = true;
            this.gridAg.addColumnNumber("productQuantity", terms["common.quantity"], null, { enableRowGrouping: true, aggFuncOnGrouping: 'sum', enableHiding: true /*enableColumnMenu: true, allowAggFuncMenu: true*/ });

            this.gridAg.addColumnText("orderTypeName", terms["billing.order.ordertype"], null, true, { enableRowGrouping: true, enableHiding: true, hide: true });
            this.gridAg.addColumnText("customerCategory", terms["common.customer.customer.customercategory"], null, true, { enableRowGrouping: true, enableHiding: true });
            this.gridAg.addColumnText("productCategory", terms["billing.product.productcategories"], null, true, { enableRowGrouping: true, enableHiding: true });
            this.contractCategoriesColumn = this.gridAg.addColumnText("contractCategory", terms["common.customer.customer.contractcategory"], null, true, { enableRowGrouping: true, enableHiding: true, hide: true });
            this.orderCategoriesColumn = this.gridAg.addColumnText("orderCategory", terms["common.customer.customer.ordercategory"], null, true, { enableRowGrouping: true, enableHiding: true, hide: true });

            if(this.accountDim1ColumnName)
                this.gridAg.addColumnText("costCentre", this.accountDim1ColumnName, null, true, { enableRowGrouping: true, enableHiding: true }); //TODO change costcentre term

            this.gridAg.addColumnText("projectNr", terms["common.customer.customer.orderproject"], null, true, { enableRowGrouping: true, enableHiding: true });   //TODO change projectnr term
            this.gridAg.addColumnText("wholeSellerName", terms["common.customer.customer.wholesellername"], null, true, { enableRowGrouping: true, enableHiding: true });
            this.gridAg.addColumnText("referenceOur", terms["billing.order.ourreference"], null, true, { enableRowGrouping: true, enableHiding: true });
            this.gridAg.addColumnText("originUsers", terms["billing.order.owners"], null, true, { enableRowGrouping: true, enableHiding: true });
            this.gridAg.addColumnText("mainUserName", terms["billing.order.selectusers.responsible"], null, true, { enableRowGrouping: true, enableHiding: true });
            const numberOptions = { decimals: 2, enableHiding: true, enableRowGrouping: true, aggFuncOnGrouping: 'sum' };
            this.gridAg.addColumnNumber("productPrice", terms["common.price"], null, numberOptions);
            this.gridAg.addColumnNumber("productSumAmount", terms["common.customer.invoices.amountexvat"], null, numberOptions);
            this.gridAg.addColumnNumber("productSumAmountCurrency", terms["common.customer.invoices.foreignamount"], null, numberOptions);
            this.gridAg.addColumnText("currencyCode", terms["common.customer.invoices.currencycode"], null, true, { enableRowGrouping: true, enableHiding: true });

            this.gridAg.addColumnNumber("productPurchasePrice", terms["common.purchaseprice"], null, numberOptions);
            this.gridAg.addColumnNumber("productMarginalIncome", terms["common.customer.customer.marginalincome"], null, numberOptions);

            const ratioColumn = this.gridAg.addColumnNumber("productMarginalRatio", terms["common.customer.customer.marginalincomeratioprocent"], null, { decimals: 2, enableHiding: true, enableRowGrouping: true });
            ratioColumn.valueGetter = (params) => {
                if (!params.node.group)
                    return {
                        marginalIncome: params.data.productMarginalIncome, sumAmount: params.data.productSumAmountCurrency, toString: () => { return params.data.productMarginalRatio; }
                    };
            }
            ratioColumn.valueFormatter = (params) => {
                return params && params.value ? params.value : 0;
            }
            ratioColumn.aggFunc = (params) => {
                let marginalIncome = 0;
                let sumAmount = 0;
                params.values.forEach((value) => {
                    marginalIncome += value ? value.marginalIncome : 0;
                    sumAmount += value ? value.sumAmount : 0;
                });
                if (marginalIncome === 0)
                    return 0;
                else if (sumAmount === 0)
                    return 100;
                else
                    return ((marginalIncome / sumAmount) * 100).round(2);
            }

            this.gridAg.addColumnText("timeCodeName", terms["billing.product.materialcode"], null, true, { enableRowGrouping: true, enableHiding: true, hide: true });
            this.gridAg.addColumnText("productGroupName", terms["billing.product.productgroup"], null, true, { enableRowGrouping: true, enableHiding: true, hide: true });
            this.gridAg.addColumnText("parentProductCategories", terms["billing.product.headproductcategories"], null, true, { enableRowGrouping: true, enableHiding: true, hide: true });
            

            // Setup events
            const events: GridEvent[] = [];
            events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChanged, (row: any) => {
                this.summarizeSelected();
            }));
            events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (row: any) => {
                this.summarizeSelected();
            }));
            events.push(new GridEvent(SoeGridOptionsEvent.RowsVisibleChanged, (rows: any[]) => {
                this.summarizeFiltered();
            }));
            this.gridAg.options.subscribe(events);

            this.gridAg.options.useGrouping();
            this.gridAg.finalizeInitGrid("billing.statistics.sales", true);
        });
    }

    private summarizeSelected() {
        this.selectedQuantity = 0;
        this.selectedPrice = 0;
        this.selectedAmount = 0;
        this.selectedPurchasePrice = 0;
        this.selectedMarginalIncome = 0;
        this.$timeout(() => {
            _.forEach(this.gridAg.options.getSelectedRows(), (y: any) => {
                if (y) {
                    this.selectedQuantity += y.productQuantity;
                    this.selectedPrice += y.productPrice;
                    this.selectedAmount += y.productSumAmount;
                    this.selectedPurchasePrice += y.productPurchasePrice;
                    this.selectedMarginalIncome += y.productMarginalIncome;
                }
            });
        });
    }

    private summarizeFiltered() {
        this.filteredQuantity = 0;
        this.filteredPrice = 0;
        this.filteredAmount = 0;
        this.filteredPurchasePrice = 0;
        this.filteredMarginalIncome = 0;
        var rows = this.gridAg.options.getFilteredRows();
        _.forEach(rows, (y: any) => {
            if (y) {
                this.filteredQuantity += y.productQuantity;
                this.filteredPrice += y.productPrice;
                this.filteredAmount += y.productSumAmount;
                this.filteredPurchasePrice += y.productPurchasePrice;
                this.filteredMarginalIncome += y.productMarginalIncome;
            }
        });
    }

    private loadCustomerStatistics() {
        this.gridAg.clearData();

        this.progress.startLoadingProgress([() => {
            return this.commonCustomerService.getCustomerStatisticsAllCustomers(this.selectedOriginType, this.selectedDateFrom, this.selectedDateTo.addDays(1)).then((x: ICustomerStatisticsDTO[]) => {
                _.forEach(x, (y) => {
                    if (!y['productGroupName'])
                        y['productGroupName'] = "";
                    if (!y['productCategory'])
                        y['productCategory'] = "";
                    if (!y['parentProductCategories'])
                        y['parentProductCategories'] = "";
                });
                this.setData(x);
                this.summarizeFiltered();
            });
        }]);
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
                    this.originTypes.push({ value: row.id, label: row.name });
                if (row.id === SoeOriginType.CustomerInvoice)
                    this.selectedOriginType = row.id;
            });
        });
    }

    private loadOrderTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.OrderType, false, false).then((x) => {
            this.orderTypes = [];
            _.forEach(x, (row) => {
                this.orderTypes.push({ value: row.id, label: row.name });
            });
        });
    }

    private loadAccountDims(): ng.IPromise<any> {
        return this.coreService.getAccountDimsSmall(false, false, false, false, false, false).then((x) => {
            this.accountDims = x;
            if (this.accountDims.length > 1)
                this.accountDim1ColumnName = this.accountDims[1].name;
        });
    }
}
