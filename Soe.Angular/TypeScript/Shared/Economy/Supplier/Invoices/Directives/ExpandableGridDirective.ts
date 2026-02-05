import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { SupplierInvoiceDTO } from "../../../../../Common/Models/InvoiceDTO";
import { IAccountDimSmallDTO } from "../../../../../Scripts/TypeLite.Net4";
import { ISoeGridOptions, SoeGridOptions, TypeAheadOptions } from "../../../../../Util/SoeGridOptions";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { IAccountingService } from "../../../../../Shared/Economy/Accounting/AccountingService";
import { ISupplierService } from "../../../../../Shared/Economy/Supplier/SupplierService";
import { SupplierInvoiceRowDTO } from "../../../../../Common/Models/SupplierInvoiceRowDTO";
import { AccountDimSmallDTO } from "../../../../../Common/Models/AccountDimDTO";

//@ngInject
export function expandableGridDirective(urlHelperService: IUrlHelperService): ng.IDirective {
    return {
        restrict: "A",
        templateUrl: urlHelperService.getViewUrl("ExpandableTemplate.html"),
        controller: ExpandableGridController,
        controllerAs: "ctrl",
        bindToController: true,
        scope: {
            supplierInvoiceId: "="
        }
    };
}

class ExpandableGridController {
    public infoText: string;
    public otherData: string;
    public supplierOrderNumber: string;
    public ourProjectNumber: string;
    public progressBusy = true;
    private supplierInvoiceId: number;

    // Data
    invoice: SupplierInvoiceDTO;
    public accountDims: AccountDimSmallDTO[];

    // Subgrids
    protected accountingGridOptions: ISoeGridOptions;
    protected paymentsGridOptions: ISoeGridOptions;

    //@ngInject
    constructor(private $timeout: ng.ITimeoutService, private uiGridConstants: uiGrid.IUiGridConstants, private translationService: ITranslationService, private accountingService: IAccountingService, private supplierService: ISupplierService, private $q: ng.IQService) {
        this.$q.all([this.setUpAccounting(), this.setUpPayments()]).then(() => {
            this.progressBusy = false;
            this.getExpandableGridData();
        });
    }

    private setUpAccounting(): ng.IPromise<any> {
        var deferral = this.$q.defer();
        this.accountingGridOptions = new SoeGridOptions("Economy.Supplier.Invoices.Accounting", this.$timeout, this.uiGridConstants);
        this.accountingGridOptions.enableGridMenu = false;
        this.accountingGridOptions.showGridFooter = false;
        this.accountingGridOptions.enableFiltering = false;
        this.accountingGridOptions.setMinRowsToShow(5);


        this.accountingService.getAccountDimsSmall(false, false, false, false).then((x) => {
            this.accountDims = x;
            var keys: string[] = [
                //"economy.supplier.invoice.project",
                "common.text",
                "common.debit",
                "common.credit"
            ];
            var index = 0;
            this.translationService.translateMany(keys).then((terms) => {
                _.forEach(x, (y: any) => {
                    index = index + 1;
                    //this.accountingGridOptions.addColumnText("dim" + index + "Nr" + "<BR/>" + "dim" + index + "Name", y.name, "15%");

                    var options = new TypeAheadOptions('item.accountNr as item.numberName for item in grid.appScope.directiveCtrl.filterAccounts(' + index + ', $viewValue) | limitTo:100');
                    options.secondRowBinding = 'dim' + index + 'Name';
                    options.errorBinding = 'dim' + index + 'Error';
                    options.additionalData = { dimIndex: index };
                    options.getSecondRowBindingValue = this.getSecondRowBindingValue.bind(this);
                    options.allowNavigationFromTypeAhead = this.allowNavigationFromTypeAhead.bind(this);


                    var col = this.accountingGridOptions.addColumnTypeAhead("dim" + index + "Nr", options, y.name, "10%");
                    col.cellEditableCondition = false;
                });
                //this.accountingGridOptions.addColumnText("project", terms['economy.supplier.invoice.project'], "15%");
                this.accountingGridOptions.addColumnText("text", terms['common.text'], null);
                this.accountingGridOptions.addColumnText("debitAmount", terms['common.debit'], "10%");
                this.accountingGridOptions.addColumnText("creditAmount", terms["common.credit"], "10%");

                deferral.resolve();
            });

        });
        return deferral.promise;
    }

    protected getSecondRowBindingValue(entity, colDef) {
        var acc = this.findAccount(entity, colDef);
        return acc ? acc.name : null;
    }

    protected findAccount(entity, colDef) {
        var nrToFind = entity['dim' + colDef.soeData.additionalData.dimIndex + 'Nr'];

        if (!nrToFind)
            return null;

        var found = this.accountDims[colDef.soeData.additionalData.dimIndex - 1].accounts.filter(acc => acc.accountNr === nrToFind);

        if (found.length) {
            var acc = found[0];
            return acc;
        }

        return null;
    }

    protected allowNavigationFromTypeAhead(entity, colDef) {
        return false;
    }

    public filterAccounts(dimIndex, filter) {
        return _.orderBy(this.accountDims[dimIndex].accounts.filter(acc => {
            if (parseInt(filter))
                return acc.accountNr.startsWithCaseInsensitive(filter);

            return acc.accountNr.startsWithCaseInsensitive(filter) || acc.name.contains(filter);
        }), 'accountNr');
    }

    private setUpPayments(): ng.IPromise<any> {
        var deferral = this.$q.defer();
        this.paymentsGridOptions = new SoeGridOptions("Economy.Supplier.Invoices.Payments", this.$timeout, this.uiGridConstants);
        this.paymentsGridOptions.enableGridMenu = false;
        this.paymentsGridOptions.showGridFooter = false;
        this.paymentsGridOptions.enableFiltering = false;
        this.paymentsGridOptions.setMinRowsToShow(5);

        var keys: string[] = [
            "common.date",
            "common.amount",
            "economy.supplier.invoice.currency",
            "economy.supplier.invoice.foreignamount"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.paymentsGridOptions.addColumnDate("payDate", terms['common.date'], null);
            this.paymentsGridOptions.addColumnText("amount", terms['common.amount'], null);
            this.paymentsGridOptions.addColumnText("currencyCode", terms['economy.supplier.invoice.currency'], null);
            this.paymentsGridOptions.addColumnText("amountCurrency", terms["economy.supplier.invoice.foreignamount"], null);
            deferral.resolve();
        });
        return deferral.promise;
    }

    private getExpandableGridData(): void {
        if (this.supplierInvoiceId > 0) {
            this.supplierService.getInvoice(this.supplierInvoiceId,false, false, false).then((x) => {
                this.invoice = x;
                if (this.invoice.originDescription)
                    this.infoText = this.invoice.originDescription;
                //this.otherData = "Other data";
                this.supplierOrderNumber = this.invoice.referenceYour;
                this.ourProjectNumber = this.invoice.projectNr;
                this.invoice.accountingRows = SupplierInvoiceRowDTO.toAccountingRowDTOs(this.invoice.supplierInvoiceRows);
                this.accountingGridOptions.setData(this.invoice.accountingRows);
            });
            this.supplierService.getPaymentRowsSmall(this.supplierInvoiceId).then((x) => {
                this.paymentsGridOptions.setData(x);
            });
        }
    }
}
