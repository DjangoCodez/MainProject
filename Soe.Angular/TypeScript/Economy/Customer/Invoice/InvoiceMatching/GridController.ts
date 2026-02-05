import { GridControllerBase } from "../../../../Core/Controllers/GridControllerBase";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../../Core/Services/UrlHelperService";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { ToolBarUtility, ToolBarButton } from "../../../../Util/ToolBarUtility";
import { IconLibrary, SoeGridOptionsEvent, InsecureDebtsButtonFunctions } from "../../../../Util/Enumerations";
import { GridEvent } from "../../../../Util/SoeGridOptions";
import { ICommonCustomerService } from "../../../../Common/Customer/CommonCustomerService";
import { Feature, SoeInvoiceType } from "../../../../Util/CommonEnumerations";
import { Constants } from "../../../../Util/Constants";

export class GridController extends GridControllerBase {
    private customerInvoicePermission: boolean;
    private terms: { [index: string]: string; };

    //@ngInject
    constructor($http,
        $templateCache,
        $timeout: ng.ITimeoutService,
        $uibModal,
        private $filter: ng.IFilterService,
        coreService: ICoreService,
        private commonCustomerService: ICommonCustomerService,
        translationService: ITranslationService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        uiGridConstants: uiGrid.IUiGridConstants,
        private $q: ng.IQService) {

        super("Soe.Economy.Customer.Invoice.Matches", "common.customer.customer.customers", Feature.Economy_Customer_Invoice_Matching, $http, $templateCache, $timeout, $uibModal, coreService, translationService, urlHelperService, messagingService, notificationService, uiGridConstants);
        this.$q.all([this.loadModifyPermissions()]).then(x => this.setupMatchesGrid());
    }

    public setupMatchesGrid() {
        this.soeGridOptions.showColumnFooter = true;
        // Columns
        var keys: string[] = [
            "economy.common.paymentmethods.customernr",
            "common.customer",
            "economy.supplier.invoice.matches.openposts",
            "economy.supplier.invoice.currencycode",
            "common.amount",
            "core.edit",
            "common.customer.invoices.invoice"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
            super.addColumnNumber("number", terms["economy.common.paymentmethods.customernr"], null, null, null, "");
            super.addColumnText("name", terms["common.customer"], "50%");
            super.addColumnNumber("count", terms["economy.supplier.invoice.matches.openposts"], null, null, null, "");
            super.addColumnNumber("sum", terms["common.amount"], null, null, 2, "");
            this.soeGridOptions.addColumnEdit(terms["core.edit"]);
        });
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

    private addSumFooter(column: uiGrid.IColumnDefOf<any>) {
        column.aggregationType = this.uiGridConstants.aggregationTypes.sum;
        column.aggregationHideLabel = true;
        column.width = "100";
        this.addSumAggregationFooterToColumns(column);
    }

    public loadGridData() {
        this.search();
    }

    public openCustomerInvoice(row: any) {
        this.messagingService.publish(Constants.EVENT_OPEN_EDITCUSTOMERINVOICE, {
            id: row.invoiceId,
            name: this.terms["common.customer.invoices.invoice"] + " " + row.seqNr
        });
    }

    public search() {
        this.commonCustomerService.getMatchingCustomerSupplier(SoeInvoiceType.CustomerInvoice).then((x: any[]) => {
            super.gridDataLoaded(x);
        });
    }
}
