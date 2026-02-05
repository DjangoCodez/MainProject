import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { EditController } from "./EditController";
import { EditController as BillingOrdersEditController } from "../../../Shared/Billing/Orders/EditController";
import { EditController as BillingProjectsEditController } from "../../../Shared/Billing/Projects/EditController";
import { EditController as CommonCustomersEditController } from "../../../Common/Customer/Customers/EditController";
import { Constants } from "../../../Util/Constants";
import { ICompositionTabsController } from "../../../Core/ICompositionTabsController";
import { ITabHandlerFactory } from "../../../Core/Handlers/tabhandlerfactory";
import { ITabHandler } from "../../../Core/Handlers/TabHandler";
import { MatrixGridController } from "./MatrixGridController";
import { ExpenseEditController } from "./ExpenseEditController";
import { Feature } from "../../../Util/CommonEnumerations";
import { ICoreService } from "../../../Core/Services/CoreService";

export class TabsController implements ICompositionTabsController {
    terms: any;
    hasExpensePermission: boolean;
    //@ngInject
    constructor(private urlHelperService: IUrlHelperService,
        tabHandlerFactory: ITabHandlerFactory,
        private $q: ng.IQService,
        private coreService: ICoreService,
        private translationService: ITranslationService,
        private messagingService: IMessagingService) {

        // Setup base class
        const part = "billing.project.timesheet.";

        this.tabs = tabHandlerFactory.create()
            .onGetRowIdentifier(row => {
                return (row) ? row.id : 0;
            })
            .onGetRowEditName(row => { return (row) ? row.name : "" })
            .onSetupTabs(() => {
                this.setupTabs();
            })
            //.onEdit(row => this.edit(row))
            .initialize(part + "timesheet", part + "timesheet", part);

        // Subscribe
        this.messagingService.subscribe(Constants.EVENT_NEW_ORDER, () => {
            return this.translationService.translate("common.customer.invoices.neworder").then((term) => {
                this.tabs.addEditTab(null, BillingOrdersEditController, null, this.urlHelperService.getGlobalUrl("shared/billing/orders/views/edit.html"), term);
            });
        });
            this.messagingService.subscribe(Constants.EVENT_NEW_PROJECT, () => {
                return this.translationService.translate("billing.projects.list.new_project").then((term) => {
                    this.tabs.addEditTab(null, BillingProjectsEditController, null, this.urlHelperService.getGlobalUrl("billing/projects/views/edit.html"), term);
                });
        });

        this.messagingService.subscribe(Constants.EVENT_OPEN_ORDER, (x) => {
            this.editOrder(x);
        });

        this.messagingService.subscribe(Constants.EVENT_OPEN_EDITCUSTOMER, (x) => {
            this.editCustomer(x);
        });

        this.messagingService.subscribe(Constants.EVENT_OPEN_EDITPROJECT, (x) => {
            this.editProject(x);
        });
    }

    private setupTabs() {
        this.$q.all([
            this.loadTerms(),
            this.loadModifyPermissions()
        ]).then(() => {
            this.tabs.addHomeTab(EditController, null, this.urlHelperService.getViewUrl("edit.html"));
            this.tabs.addNewTab(this.terms["billing.project.timesheet.weekreport"], "matrix", MatrixGridController, this.urlHelperService.getCoreViewUrl("gridCompositionAgTotals.html"), null, false, false);
            if (this.hasExpensePermission)
                this.tabs.addNewTab(this.terms["billing.order.expenses"], "expense", ExpenseEditController, this.urlHelperService.getViewUrl("editExpense.html"), null, false, false);
        });
        return this.translationService.translate("billing.project.timesheet.weekreport").then((term) => {
            
        });
    }

    private editOrder(row: any) {
        const activeTab = this.tabs.getTabByIdentifier("order_" + row.id);
        if (activeTab) {
            this.tabs.setActiveTab(activeTab);
        } else {
            this.tabs.addNewTab(row.name, "order_" + row.id, BillingOrdersEditController, this.urlHelperService.getGlobalUrl("shared/billing/orders/views/edit.html"), { id: row.id, feature: Feature.Billing_Order_Status },true,true);
        }
    }

    private editCustomer(row: any) {
        const activeTab = this.tabs.getTabByIdentifier("customer_" + row.id);
        if (activeTab) {
            this.tabs.setActiveTab(activeTab);
        } else {
            this.tabs.addNewTab(row.name, "customer" + row.id, CommonCustomersEditController, this.urlHelperService.getGlobalUrl("common/customer/customers/views/edit.html"), { id: row.id }, true, true);
        }
    }

    protected editProject(row: any) {
        const activeTab = this.tabs.getTabByIdentifier("project_" + row.id);
        if (activeTab) {
            this.tabs.setActiveTab(activeTab);
        } else {
            this.tabs.addNewTab(row.name, "project_" + row.id, BillingProjectsEditController, this.urlHelperService.getGlobalUrl("billing/projects/views/edit.html"), { id: row.id }, true, true);
        }
    }

    //LOOKUPS
    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "billing.project.timesheet.weekreport",
            "billing.order.expenses"
        ];
        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private loadModifyPermissions() {
        const featureIds: number[] = [];
        featureIds.push(Feature.Billing_Order_Orders_Edit_Expenses);

        return this.coreService.hasModifyPermissions(featureIds).then((x) => {
            this.hasExpensePermission = x[Feature.Billing_Order_Orders_Edit_Expenses];
        });
    }

    public tabs: ITabHandler;
}