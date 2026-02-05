import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../../../Core/Services/MessagingService";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { ICommonCustomerService } from "../../../CommonCustomerService";
import { ISoeGridOptionsAg, SoeGridOptionsAg } from "../../../../../Util/SoeGridOptionsAg";


export class SelectUnpaidInvoiceController {
    // Terms
    private terms: any;

    // Invoices
    private invoices: any;

    // Grid Options
    private soeGridOptions: ISoeGridOptionsAg;

    // Flags
    progressBusy: boolean = true;

    //@ngInject
    constructor(
        private $scope,
        private $uibModalInstance,
        private $window: ng.IWindowService,
        private $timeout: ng.ITimeoutService,
        private translationService: ITranslationService,
        private messagingService: IMessagingService,
        private coreService: ICoreService,
        private urlHelperService: IUrlHelperService,
        private commonCustomerService: ICommonCustomerService,
        uiGridConstants: uiGrid.IUiGridConstants,
        private $q: ng.IQService,
        private customerId: number
    ) {

        this.soeGridOptions = new SoeGridOptionsAg("Common.Dialogs.SelectUsers", this.$timeout);
        this.soeGridOptions.enableGridMenu = false;
        this.soeGridOptions.enableFiltering = true;
        this.soeGridOptions.enableRowSelection = true;
        this.soeGridOptions.setMinRowsToShow(10);
    }

    $onInit() {
        this.setupGrid();
        this.loadGridData();
    }

    private loadGridData(): ng.IPromise<any> {
        return this.commonCustomerService.getUnpaidCustomerInvoicesForDialog(this.customerId).then(invoices => {
            this.invoices = invoices;
            this.soeGridOptions.setData(this.invoices);
            this.progressBusy = false;
        });
    }

    private setupGrid() {
        var keys: string[] = [
            "common.customer.invoices.customer",
            "common.customer.invoices.invoicenr",
            "common.customer.invoices.invoiceamount",
            "common.customer.invoices.payableamount",
            "common.customer.invoices.currencycode",
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total",
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
            this.soeGridOptions.addColumnText("actorCustomerName", this.terms["common.customer.invoices.customer"], null);
            this.soeGridOptions.addColumnText("invoiceNr", this.terms["common.customer.invoices.invoicenr"], null);
            this.soeGridOptions.addColumnNumber("totalAmountCurrency", this.terms["common.customer.invoices.invoiceamount"], null, { decimals: 2});
            this.soeGridOptions.addColumnNumber("remainingAmount", this.terms["common.customer.invoices.payableamount"], null, { decimals: 2 });
            this.soeGridOptions.addColumnText("currencyCode", this.terms["common.customer.invoices.currencycode"], null);

            this.soeGridOptions.finalizeInitGrid();

            this.$timeout(() => {
                this.soeGridOptions.addTotalRow("#totals-grid", {
                    filtered: this.terms["core.aggrid.totals.filtered"],
                    total: this.terms["core.aggrid.totals.total"]
                });

                this.soeGridOptions.enableSingleSelection();
            });
        });
    }

    buttonCancelClick() {
        this.close(null);
    }

    buttonOkClick() {
        this.close(this.soeGridOptions.getSelectedRows());
    }

    close(result: any) {
        if (!result) {
            this.$uibModalInstance.dismiss('cancel');
        }
        else {
            this.$uibModalInstance.close({ rows: result });
        }
    }
}