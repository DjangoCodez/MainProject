import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { Feature, SoeReportTemplateType, TermGroup, SoeOriginType } from "../../../Util/CommonEnumerations";
import { IProjectService } from "../../../Shared/Billing/Projects/ProjectService";
import { Constants } from "../../../Util/Constants";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { CoreUtility } from "../../../Util/CoreUtility";


export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    private fromDate: Date = null;
    private toDate: Date = null;
    private projectId: number = null;
    private includeChildProjects: boolean = null;
    private originTypes: ISmallGenericType[];
    private attestStates = [];

    private _selectedOriginType: SoeOriginType = SoeOriginType.Order;
    get selectedOriginType() {
        return this._selectedOriginType;
    }
    set selectedOriginType(item: any) {

        if (item != this._selectedOriginType)
            this.setData("");
        
        this._selectedOriginType = item;
    }

    //Permissions
    private hasOrderPermission: boolean;
    private hasInvoicePermission: boolean;
    private hasOrderRowsPermission: boolean;
    private hasInvoiceRowsPermission: boolean;
    private hasSalesPricePermission: boolean;
    private hasPurchasePricePermission: boolean;

    private activated = false;


    //@ngInject
    constructor(
        private $scope,
        private $q: ng.IQService,
        private coreService: ICoreService,
        private projectService: IProjectService,
        private translationService: ITranslationService,
        private urlHelperService: IUrlHelperService,
        private messagingService: IMessagingService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {
        super(gridHandlerFactory, "Billing.Projects.Project.ProductRows", progressHandlerFactory, messagingHandlerFactory);
        this.onTabActivetedAndModified(() => this.reloadGridFromFilter());

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onBeforeSetUpGrid(() => this.beforeSetupGrid())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.setData(""));
        this.onTabActivated(() => this.tabActivated());
    }

    private tabActivated() {
        if (!this.activated) {
            this.flowHandler.start([
                { feature: Feature.Billing_Project_List, loadReadPermissions: false, loadModifyPermissions: true },
            ]
            );
            this.activated = true;
        }
    }

    private loadModifyPermissions(): ng.IPromise<any> {
        const featureIds: number[] = [
            Feature.Billing_Order_Orders_Edit_ProductRows,
            Feature.Billing_Invoice_Invoices_Edit_ProductRows,
            Feature.Billing_Product_Products_ShowSalesPrice,
            Feature.Billing_Product_Products_ShowPurchasePrice,
            Feature.Billing_Invoice_Invoices_Edit,
            Feature.Billing_Order_Orders_Edit,
        ];
        return this.coreService.hasModifyPermissions(featureIds).then((x) => {
            this.modifyPermission = x[soeConfig.feature];
            this.hasOrderRowsPermission = x[Feature.Billing_Order_Orders_Edit_ProductRows];
            this.hasInvoiceRowsPermission = x[Feature.Billing_Invoice_Invoices_Edit_ProductRows];
            this.hasSalesPricePermission = x[Feature.Billing_Product_Products_ShowSalesPrice];
            this.hasPurchasePricePermission = x[Feature.Billing_Product_Products_ShowPurchasePrice];
            this.hasOrderPermission = x[Feature.Billing_Order_Orders_Edit];
            this.hasInvoicePermission = x[Feature.Billing_Invoice_Invoices_Edit];
        });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg as IGridHandler, () => this.loadGridData());
        this.toolbar.addInclude(this.urlHelperService.getViewUrl("productRowsGridHeader.html"));
    }

    public onInit(parameters: any) {
        this.messagingService.subscribe(Constants.EVENT_LOAD_PROJECTCENTRALDATA, (x) => {
            if (this.projectId != x.projectId)
                this.setData("");
            this.projectId = x.projectId;
            this.includeChildProjects = x.includeChildProjects;
            this.fromDate = x.fromDate;
            this.toDate = x.toDate;
        });

        this.$scope.$on('onTabActivated', (e, a) => {
            if (a == this.guid)
                this.tabActivated();
        });

        this.parameters = parameters;
        this.guid = parameters.guid;
        this.isHomeTab = !!parameters.isHomeTab;
    }

    private beforeSetupGrid(): ng.IPromise<any> {
        return this.loadModifyPermissions().then(() => {
            return this.$q.all([
                this.loadOriginTypes(),
            ])
        })
    }

    private loadOriginTypes(): ng.IPromise<any> {
        this.selectedOriginType = SoeOriginType.Order;
        return this.coreService.getTermGroupContent(TermGroup.OriginType, false, false).then((types) => {
            this.originTypes = [];
            types.forEach(type => {
                if ((type.id === SoeOriginType.Order && this.hasOrderPermission) ||
                    (type.id === SoeOriginType.CustomerInvoice && this.hasInvoicePermission))
                    this.originTypes.push(type);
            })
        });
    }

    public setupGrid() {

        // Columns
        const keys: string[] = [
            "billing.productrows.discount",
            "billing.productrows.purchaseprice",
            "billing.productrows.purchasepricesum",
            "billing.productrows.marginalincomeratio.short",
            "billing.productrows.marginalincome.short",
            "billing.productrows.productunit",
            "billing.productrows.quantity",
            "common.date",
            "common.description",
            "common.productnr",
            "common.createdby",
            "common.created",
            "common.modified",
            "common.modifiedby",
            "billing.productrows.amount",
            "billing.productrows.sumamount",
            "common.customer.invoices.rowstatus",
            "billing.project.project",
            "billing.order.projectnr",
            "billing.order.ordernr",
            "common.invoicenr",
            "common.customer.invoices.articlename",
            "billing.product.materialcode",
            "billing.product.productgroup",
            "common.customer.customer.wholesellername",
            "economy.supplier.supplier.supplier",
            "common.customer.invoices.invoicedate",
            "billing.order.invoicedate",
            "economy.supplier.invoice.openpdf",
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.gridAg.options.setName("productrowsGrid");

            this.gridAg.addColumnIcon("rowTypeIcon", null, null, null);
            this.gridAg.addColumnText("projectName", terms["billing.project.project"], null, true, { enableRowGrouping: true, enableHiding: true });
            this.gridAg.addColumnText("projectNumber", terms["billing.order.projectnr"], null, true, { enableRowGrouping: true, enableHiding: true });

            if (this.hasInvoicePermission) {
                this.gridAg.addColumnText("customerInvoiceNumber", terms["common.invoicenr"], null, true, { enableRowGrouping: true, enableHiding: true, hide: true, buttonConfiguration: { iconClass: "iconEdit fal fa-pencil", show: (row) => true, callback: this.openOrderInvoice.bind(this) } });
            }

            if (this.hasOrderPermission) {
                this.gridAg.addColumnText("orderNumber", terms["billing.order.ordernr"], null, true, { enableRowGrouping: true, enableHiding: true, buttonConfiguration: { iconClass: "iconEdit fal fa-pencil", show: (row) => true, callback: this.openOrderInvoice.bind(this) } });
            }
            this.gridAg.addColumnDate("invoiceDate", terms["common.customer.invoices.invoicedate"], null, true, null, { enableRowGrouping: true, enableHiding: true, hide: true })
            this.gridAg.addColumnDate("orderDate", terms["billing.order.invoicedate"], null, true, null, { enableRowGrouping: true, enableHiding: true, hide: true })

            this.gridAg.addColumnText("articleNumber", terms["common.productnr"], null, true, { enableRowGrouping: true, enableHiding: true });
            this.gridAg.addColumnText("articleName", terms["common.customer.invoices.articlename"], null, true, { enableRowGrouping: true, enableHiding: true });
            this.gridAg.addColumnText("productGroupName", terms["billing.product.productgroup"], null, true, { enableRowGrouping: true, enableHiding: true });
            this.gridAg.addColumnText("materialCode", terms["billing.product.materialcode"], null, true, { enableRowGrouping: true, enableHiding: true });
            this.gridAg.addColumnText("description", terms["common.description"], null, true, { enableRowGrouping: true, enableHiding: true });
            this.gridAg.addColumnText("supplierName", terms["economy.supplier.supplier.supplier"], null, true, { enableRowGrouping: true, enableHiding: true });
            this.gridAg.addColumnSelect("attestState", terms["common.customer.invoices.rowstatus"], null, {
                populateFilterFromGrid: true,
                toolTipField: "attestState", displayField: "attestState", selectOptions: this.attestStates, shape: Constants.SHAPE_CIRCLE, shapeValueField: "attestColor", colorField: "attestColor", enableHiding: true, enableRowGrouping: true
            });

            this.gridAg.addColumnText("unit", terms["billing.productrows.productunit"], null, true, { enableRowGrouping: true, enableHiding: true });
            this.gridAg.addColumnNumber("quantity", terms["billing.productrows.quantity"], null, { decimals: 2, maxDecimals: 4, aggFuncOnGrouping: "sum" });

            if (this.hasPurchasePricePermission) {
                this.gridAg.addColumnNumber("purchasePrice", terms["billing.productrows.purchaseprice"], null, { decimals: 2, maxDecimals: 4 });
                this.gridAg.addColumnNumber("purchaseAmount", terms["billing.productrows.purchasepricesum"], null, { decimals: 2, maxDecimals: 4, aggFuncOnGrouping: "sum" });
                this.gridAg.addColumnIcon(null, null, null, { icon: "fal fa-file-pdf", onClick: this.showPdf.bind(this), showIcon: this.showIcon.bind(this), toolTip: terms["economy.supplier.invoice.openpdf"], suppressFilter: true });
            }


            if (this.hasSalesPricePermission) {
                this.gridAg.addColumnNumber("salesPrice", terms["billing.productrows.amount"], null, { decimals: 2, maxDecimals: 4 });
                this.gridAg.addColumnNumber("salesAmount", terms["billing.productrows.sumamount"], null, { decimals: 2, maxDecimals: 4, aggFuncOnGrouping: "sum" });

            }

            if (this.hasSalesPricePermission && this.hasPurchasePricePermission) {
                this.gridAg.addColumnNumber("marginalIncome", terms["billing.productrows.marginalincome.short"], null, { decimals: 2, maxDecimals: 4, aggFuncOnGrouping: "sum" });
                this.gridAg.addColumnNumber("marginalIncomeRatio", terms["billing.productrows.marginalincomeratio.short"], null, { decimals: 2, maxDecimals: 4});
                this.gridAg.addColumnNumber("discountPercent", terms["billing.productrows.discount"], null, { decimals: 2, maxDecimals: 4 });
            }

            this.gridAg.addColumnDate("date", terms["common.date"], null, true, null, { enableRowGrouping: true, enableHiding: true})
            this.gridAg.addColumnDate("created", terms["common.created"], null, true, null, { enableRowGrouping: true, enableHiding: true, hide:true })
            this.gridAg.addColumnText("createdBy", terms["common.createdby"], null, true, { enableRowGrouping: true, enableHiding: true, hide:true });
            this.gridAg.addColumnDate("modified", terms["common.modified"], null, true, null, { enableRowGrouping: true, enableHiding: true, hide:true })
            this.gridAg.addColumnText("modifiedBy", terms["common.modifiedby"], null, true, { enableRowGrouping: true, enableHiding: true, hide:true });


            this.gridAg.options.useGrouping();
            this.gridAg.finalizeInitGrid("billing.projects.productrows", true);
        });
    }

    private setRowTypeIcon(row: any) {
        if (row.isTimeProjectRow) {
            row.rowTypeIcon = 'fal fa-clock';
        }
        else {
            row.rowTypeIcon = 'fal fa-box-alt';
        }
    }

    private openOrderInvoice(row: any) {
        row.invoiceNr = row.invoiceNumber;
        row.customerInvoiceId = row.invoiceId;
        row.associatedId = row.invoiceId;
        if (this.selectedOriginType === SoeOriginType.Order) {
            this.messagingService.publish(Constants.EVENT_OPEN_ORDER, { row });
        }
        else if (this.selectedOriginType === SoeOriginType.CustomerInvoice) {
            this.messagingService.publish(Constants.EVENT_OPEN_INVOICE, row);
        }
    }

    private showPdf(row) {
        //Show picture in new browser tab (not sure if PDF:s work same way)   
        if (row.supplierInvoiceId && row.supplierInvoiceId > 0) {
            const imageUrl = "/ajax/downloadReport.aspx?templatetype=" + SoeReportTemplateType.SupplierInvoiceImage + "&invoiceId=" + row.supplierInvoiceId + "&c=" + CoreUtility.actorCompanyId;
            console.log(imageUrl)
            window.open(imageUrl, '_blank');
        }
        else if (row.ediEntryId && row.ediEntryId > 0) {
            var ediPdfReportUrl: string = "/ajax/downloadReport.aspx?templatetype=" + SoeReportTemplateType.SymbrioEdiSupplierInvoice + "&edientryid=" + row.ediEntryId;
            window.open(ediPdfReportUrl, '_blank');
        }
    }

    private showIcon(row) {
        if (!row) return false
        return (row.ediEntryId && row.ediEntryId > 0) || (row.supplierInvoiceId && row.supplierInvoiceId > 0)
    }



    public loadGridData() {
        // Load data
        //this.projectService.getProjectProductRows()
        this.projectService.getProjectProductRows(this.projectId, this.selectedOriginType, this.includeChildProjects, this.fromDate, this.toDate).then(productRows => {
            productRows.forEach(r => {
                this.setRowTypeIcon(r);
                r.customerInvoiceNumber = r.invoiceNumber;
                r.orderNumber = r.invoiceNumber;
                r.orderDate = r.invoiceDate;
            })

            if (this.selectedOriginType === SoeOriginType.Order) {
                this.gridAg.options.showColumn("orderNumber");
                this.gridAg.options.showColumn("orderDate");
                this.gridAg.options.hideColumn("customerInvoiceNumber");
                this.gridAg.options.hideColumn("invoiceDate");
            } else if (this.selectedOriginType === SoeOriginType.CustomerInvoice) {
                this.gridAg.options.hideColumn("orderNumber");
                this.gridAg.options.hideColumn("orderDate");
                this.gridAg.options.showColumn("customerInvoiceNumber");
                this.gridAg.options.showColumn("invoiceDate");
            }

            this.gridAg.setData(productRows);
        })
    }

    public reloadGridFromFilter = _.debounce(() => {
        this.loadGridData();
    }, 1000, { leading: false, trailing: true });

}
