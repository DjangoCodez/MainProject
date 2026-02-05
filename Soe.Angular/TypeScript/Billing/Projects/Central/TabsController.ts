import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { ICoreService } from "../../../Core/Services/CoreService";
import { EditController } from "./EditController";
import { GridController as CustomerInvoicesGridController } from "../../../Common/Customer/Invoices/gridcontroller";
import { GridController as ProductRowsGridController } from "./ProductRowsGridController";
import { EditController as BillingOrdersEditController } from "../../../Shared/Billing/Orders/EditController";
import { EditController as BillingInvoicesEditController } from "../../../Shared/Billing/Invoices/EditController";
import { EditController as CommonCustomersEditController } from "../../../Common/Customer/Customers/EditController";
import { EditController as BillingProjectEditController } from "../../../Shared/Billing/Projects/EditController";
import { EditController as AnalyticsEditController } from "./AnalyticsEditController";
import { GridController as ProjectSupplierInvoicesGridController } from "./SupplierInvoicesGridController";
import { EditController as TimeSheetController } from "../../../Billing/Projects/TimeSheets/EditController";
import { Constants } from "../../../Util/Constants";
import { SoeOriginType, SoeOriginStatusClassification, Feature } from "../../../Util/CommonEnumerations";
import { EditController as SupplierInvoicesEditController } from "../../../Shared/Economy/Supplier/Invoices/EditController";
import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { ITabHandlerFactory } from "../../../Core/Handlers/tabhandlerfactory";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";
import { HtmlUtility } from "../../../Util/HtmlUtility";
import { AngularFeatureCheckService } from "../../../Core/Services/AngularFeatureCheckService";

export class TabsController implements ICompositionTabsController {

    terms: any;
    hasOrderPermission: boolean;
    hasSupplierInvoicePermission: boolean;
    hasCustomerInvoicePermission: boolean;
    hasEditCustomerPermission: boolean;
    hasEditProjectPermission: boolean;
    hasTimesPermission: boolean;
    hasProductRowsPermission: boolean;

    //@ngInject
    constructor(
        private $window: ng.IWindowService,
        private urlHelperService: IUrlHelperService,
        tabHandlerFactory: ITabHandlerFactory,
        private coreService: ICoreService,
        private $q: ng.IQService,
        private translationService: ITranslationService,
        private messagingService: IMessagingService,
        private angularFeatureCheckService: AngularFeatureCheckService) {

        // Setup base class
        const part = "billing.projects.list.";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => {
                return (row) ? row.id : 0;
            })
            .onGetRowEditName(row => { return row.number ? row.number : ""; })
            .onSetupTabs(() => {
                this.setupTabs();
            })
            //.onEdit(row => this.edit(row))
            .initialize(part + "project", part + "projects", part + "new_project");

        this.messagingService.subscribe(Constants.EVENT_NEW_ORDER, (x) => {
            this.addOrder(x);
        });

        this.messagingService.subscribe(Constants.EVENT_OPEN_ORDER, (x) => {
            this.editOrder(x.row);
        });

        this.messagingService.subscribe(Constants.EVENT_OPEN_INVOICE, (x) => {
            this.editCustomerInvoice(x);
        });

        this.messagingService.subscribe(Constants.EVENT_OPEN_EDITCUSTOMER, (x) => {
            if (this.hasEditCustomerPermission)
                this.editCustomer(x);
        });

        this.messagingService.subscribe(Constants.EVENT_OPEN_EDITPROJECT, (x) => {
            if (this.hasEditProjectPermission)
                this.editProject(x);
        });

        this.messagingService.subscribe(Constants.EVENT_OPEN_PROJECTCENTRAL, (x) => {
            if (x.row) {
                switch (x.row.originType) {
                    case SoeOriginType.Order:
                        this.editOrder(x.row, true);
                        break;
                    case SoeOriginType.CustomerInvoice:
                        this.editCustomerInvoice(x.row, true);
                        break;
                    case SoeOriginType.SupplierInvoice:
                        this.editSupplierInvoice(x.row);
                        break;
                    default:
                        this.edit(x.row);
                }
            }
        });
    }

    protected setupTabs() {
        this.$q.all([
            this.loadTerms(),
            this.loadModifyPermissions()
        ]).then(() => {
            this.tabs.addHomeTab(EditController, null, this.urlHelperService.getViewUrl("edit.html"));
            if (this.hasOrderPermission)
                this.tabs.addNewTab(this.terms["common.customer.invoices.orders"], "projectcentral_order", CustomerInvoicesGridController, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"), { classification: SoeOriginStatusClassification.OrdersAll, isHomeTab: true, isProjectCentral: true }, false, false, true);
            if (this.hasCustomerInvoicePermission)
                this.tabs.addNewTab(this.terms["common.customer.invoices.customerinvoices"], "projectcentral_customerinvoices", CustomerInvoicesGridController, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"), { classification: SoeOriginStatusClassification.CustomerInvoicesAll, isHomeTab: true, isProjectCentral: true }, false, false, true);
            if (this.hasSupplierInvoicePermission)
                this.tabs.addNewTab(this.terms["billing.project.central.supplierinvoices"], "projectcentral_supplierinvoices", ProjectSupplierInvoicesGridController, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"), { isHomeTab: true }, false, false, true);
            if (this.hasTimesPermission)
                this.tabs.addNewTab(this.terms["billing.project.timesheet.timesheet"], "projectcentral_timesheet", TimeSheetController, this.urlHelperService.getGlobalUrl("billing/projects/timesheets/views/edit.html"), { isHomeTab: true, isProjectCentral: true }, false, false, true);
            if (this.hasProductRowsPermission)
                this.tabs.addNewTab(this.terms["billing.order.productrows"], "projectcentral_productrows", ProductRowsGridController, this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"), { isHomeTab: true, isProjectCentral: true }, false, false, true);
            if (this.hasEditProjectPermission)
                this.tabs.addNewTab(this.terms["billing.project.central.analytics"], "projectcentral_analytics", AnalyticsEditController, this.urlHelperService.getGlobalUrl("billing/projects/central/views/analyticsEdit.html"), { isHomeTab: true, isProjectCentral: true }, false, false, true);
            this.tabs.enableRemoveAll();
        });
    }

    protected add() {
        this.tabs.addCreateNewTab(EditController);
    }

    protected edit(row: any) {
        this.tabs.addEditTab(row, EditController, null, this.urlHelperService.getViewUrl("edit.html"));
    }

    protected addOrder(x: any) {
        this.tabs.addEditTab(x, BillingOrdersEditController, x, this.urlHelperService.getGlobalUrl("shared/billing/orders/views/edit.html"), this.terms["billing.project.central.neworder"], true);
    }

    protected editOrder(row: any, useAssociated = false) {
        const activeTab = this.tabs.getTabByIdentifier( this.getEditIdentifier(row, useAssociated));
        if (activeTab) {
            this.tabs.setActiveTabIndex(activeTab.index);
        } else {
            this.tabs.addEditTab(row, BillingOrdersEditController, { id: useAssociated ? row.associatedId : row.customerInvoiceId, isProjectCentral: true }, this.urlHelperService.getGlobalUrl("shared/billing/orders/views/edit.html"), this.terms["billing.project.central.order"] + " " + row.invoiceNr, true);
        }
    }

    protected editCustomerInvoice(row: any, useAssociated = false) {
        const activeTab = this.tabs.getTabByIdentifier( this.getEditIdentifier(row, useAssociated));
        if (activeTab) {
            this.tabs.setActiveTabIndex(activeTab.index);
        } else {
            this.tabs.addEditTab(row, BillingInvoicesEditController, { id: useAssociated ? row.associatedId : row.customerInvoiceId, isProjectCentral: true }, this.urlHelperService.getGlobalUrl("shared/billing/invoices/views/edit.html"), this.terms["billing.project.central.customerinvoice"] + " " + row.invoiceNr, true);
        }
    }

    protected editSupplierInvoice(row: any) {
        const activeTab = this.tabs.getTabByIdentifier(this.getEditIdentifier(row, true));
        if (activeTab) {
            this.tabs.setActiveTabIndex(activeTab.index);
        } else {
            this.tabs.addEditTab(row, SupplierInvoicesEditController, { id: row.associatedId, isProjectCentral: true }, this.urlHelperService.getGlobalUrl("shared/economy/supplier/invoices/views/edit.html"), this.terms["billing.project.central.supplierinvoice"] + " " + row.invoiceNr, true);
        }
    }

    protected editCustomer(row: any) {
        const activeTab = this.tabs.getTabByIdentifier("customer_" + row.id);
        if (activeTab) {
            this.tabs.setActiveTabIndex(activeTab.index);
        } else {
            this.tabs.addEditTab(row, CommonCustomersEditController, { id: row.id }, this.urlHelperService.getGlobalUrl("common/customer/customers/views/edit.html"),row.name, true);
        }
    }

    protected editProject(row: any) {
        const activeTab = this.tabs.getTabByIdentifier("project_" + row.id);

        if (this.angularFeatureCheckService.shouldUseAngularSpa(Feature.Billing_Project_Edit_Budget)) {
            HtmlUtility.openInNewTab(this.$window, "/soe/billing/project/list/?projectid=" + row.id);
        }
        else if (activeTab) {
            this.tabs.setActiveTabIndex(activeTab.index);
        } else {
            this.tabs.addEditTab(row,BillingProjectEditController, { id: row.id }, this.urlHelperService.getGlobalUrl("billing/projects/views/edit.html"),row.name, true);
        }
    }

    protected getEditIdentifier(row: any, useAssociated = false): any {
        return useAssociated ? row.associatedId : row.customerInvoiceId;
    }

    //LOOKUPS
    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "billing.project.central.projectcentral",
            "common.customer.invoices.orders",
            "common.customer.invoices.customerinvoices",
            "billing.project.central.supplierinvoices",
            "billing.project.central.neworder",
            "billing.project.central.order",
            "billing.project.central.customerinvoice",
            "billing.project.central.supplierinvoice",
            "billing.project.timesheet.timesheet",
            "billing.order.productrows",
            "billing.project.central.analytics",
        ];
        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private loadModifyPermissions() {
        const featureIds: number[] = [];
        featureIds.push(Feature.Billing_Order_Status);
        featureIds.push(Feature.Billing_Invoice_Status);
        featureIds.push(Feature.Economy_Supplier_Invoice_Status);
        featureIds.push(Feature.Billing_Customer_Customers_Edit);
        featureIds.push(Feature.Billing_Project_Edit);
        featureIds.push(Feature.Billing_Project_Central_TimeSheetUser);
        featureIds.push(Feature.Billing_Order_Orders_Edit_ProductRows);
        featureIds.push(Feature.Billing_Invoice_Invoices_Edit_ProductRows);

        return this.coreService.hasModifyPermissions(featureIds).then((x) => {
            this.hasOrderPermission = x[Feature.Billing_Order_Status];
            this.hasCustomerInvoicePermission = x[Feature.Billing_Invoice_Status];
            this.hasSupplierInvoicePermission = x[Feature.Economy_Supplier_Invoice_Status];
            this.hasEditCustomerPermission = x[Feature.Billing_Customer_Customers_Edit];
            this.hasEditProjectPermission = x[Feature.Billing_Project_Edit];
            this.hasTimesPermission = x[Feature.Billing_Project_Central_TimeSheetUser];
            this.hasProductRowsPermission = x[Feature.Billing_Order_Orders_Edit_ProductRows] || x[Feature.Billing_Invoice_Invoices_Edit_ProductRows];
        });
    }

    public tabs: ITabHandler;
}