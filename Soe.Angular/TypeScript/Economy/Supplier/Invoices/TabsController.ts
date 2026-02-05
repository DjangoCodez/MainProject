import { GridController } from "../../../Shared/Economy/Supplier/Invoices/GridController";
import { CostGridController } from "./CostsGridController";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { EditController } from "../../../Shared/Economy/Supplier/Invoices/EditController";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";
import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { ITabHandlerFactory } from "../../../Core/Handlers/tabhandlerfactory";
import { ICoreService } from "../../../Core/Services/CoreService";
import { Feature } from "../../../Util/CommonEnumerations";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { Constants } from "../../../Util/Constants";
import { EditController as InvoiceEditController } from "../../../Shared/Billing/Purchase/Purchase/EditController";
import { EditController as BillingOrdersEditController } from "../../../Shared/Billing/Orders/EditController";
import { HtmlUtility } from "../../../Util/HtmlUtility";

export class TabsController implements ICompositionTabsController {
    private terms: any = [];
    private modifyPermissions: any = [];

    //@ngInject
    constructor(private coreService: ICoreService,
        tabHandlerFactory: ITabHandlerFactory,
        private urlHelperService: IUrlHelperService,
        protected messagingService: IMessagingService,
        private translationService: ITranslationService,
        private $q: ng.IQService,
        private $window) {

        const part = "economy.supplier.invoice.";
        
        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => row.supplierInvoiceId)
            .onGetRowEditName(row => row.invoiceNr ? row.invoiceNr : "")
            .onSetupTabs(() => { this.setupTabs(); })
            .onEdit(row => this.edit(row))
            .initialize(part + "invoice", part + "invoices", part + "new");

        this.messagingService.subscribe(Constants.EVENT_OPEN_PURCHASE, (x) => {           
            this.editPurchase(x);
        });
        this.messagingService.subscribe(Constants.EVENT_OPEN_ORDER, (x) => {
            this.editOrder(x);
        });
        this.messagingService.subscribe(Constants.EVENT_OPEN_INVOICE, (x) => {
            this.editSupplierInvoice(x);
        });
    }  

    protected setupTabs() {
        this.$q.all([
            this.loadTerms(),
            this.loadModifyPermissions()
        ]).then(() => {
            this.tabs.enableRemoveAll();
            this.tabs.enableAddTab(() => this.add());
            this.tabs.addNewTab(this.terms["economy.supplier.invoice.invoices"], null, GridController, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"), { isHomeTab: true, setup: true }, false, true);
            if (this.modifyPermissions[Feature.Economy_Supplier_Invoice_Project] || this.modifyPermissions[Feature.Economy_Supplier_Invoice_Order]) {
                this.tabs.addNewTab(this.terms["economy.supplier.invoice.costoverview"], null, CostGridController, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"), { isHomeTab: true, setup: true }, false, false);
            }

            // Navigate to invoice from new Angular
            const invoiceIdQP = HtmlUtility.getQueryParameterByName(this.$window.location, "invoiceId")
            if (soeConfig.invoiceId > 0) {
                this.editSupplierInvoice({ row: { id: soeConfig.invoiceId, supplierinvoiceId: soeConfig.invoiceId, supplierInvoiceSeqNr: soeConfig.invoiceNr, type: "supplier" } });
            } else if (soeConfig.invoiceId === 0 && invoiceIdQP) {
                this.add();
            }
        });
    }

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "economy.supplier.invoice.invoices",
            "billing.purchase.list.purchase",
            "economy.supplier.invoice.invoice",
            "economy.supplier.invoice.costoverview"
        ];
        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private loadModifyPermissions(): ng.IPromise<any> {
        var featureIds: number[] = [];
        featureIds.push(Feature.Economy_Supplier_Invoice_Project);
        featureIds.push(Feature.Economy_Supplier_Invoice_Order);

        return this.coreService.hasModifyPermissions(featureIds).then((x) => {
            this.modifyPermissions = x;
        });
    }

    protected add() {
        this.tabs.addCreateNewTab(EditController, this.urlHelperService.getGlobalUrl("Shared/Economy/Supplier/Invoices/Views/edit.html"));
    }

    protected edit(row: any) {
        if (!row.ediType || row.ediType == 0)
            this.tabs.addEditTab(row, EditController, { id: this.getEditIdentifier(row) }, this.urlHelperService.getGlobalUrl("Shared/Economy/Supplier/Invoices/Views/edit.html"));
    }
    protected getEditIdentifier(row: any): any {
        return row.supplierInvoiceId;
    }
   
       
    protected editPurchase(rowAndIds: any) {
        const activeTab = this.tabs.getTabByParameters(rowAndIds.row.id,"purchase");
        if (activeTab) {
            this.tabs.setActiveTab(activeTab);
        } else {
            this.tabs.addEditTab(rowAndIds.row, InvoiceEditController, { id: rowAndIds.row.id, type: "purchase", ids: [] }, this.urlHelperService.getGlobalUrl("billing/purchase/purchase/views/edit.html"), this.terms["billing.purchase.list.purchase"] + " " + rowAndIds.row.purchaseNr);
        }
    }

    protected editOrder(row: any) {
        const activeTab = this.tabs.getTabByIdentifier("order_" + row.originId);
        if (activeTab) {
            this.tabs.setActiveTab(activeTab);
        } else {

            this.tabs.addNewTab(row.name, "order_" + row.originId, BillingOrdersEditController, this.urlHelperService.getGlobalUrl("shared/billing/orders/views/edit.html"), { id: row.originId, feature: Feature.Billing_Order_Status }, true, true);
        }
    }

    protected editSupplierInvoice(row: any) {
        const activeTab = this.tabs.getTabByParameters(row.row.supplierinvoiceId, "supplier");
        if (activeTab) {
            this.tabs.setActiveTab(activeTab);
        } else {
            this.tabs.addEditTab(row, EditController, { id: row.row.id, type: "supplier" }, this.urlHelperService.getGlobalUrl("Shared/Economy/Supplier/Invoices/Views/edit.html"), this.terms["economy.supplier.invoice.invoice"] + " " + row.row.supplierInvoiceSeqNr);
        }
    }

    public tabs: ITabHandler;
}