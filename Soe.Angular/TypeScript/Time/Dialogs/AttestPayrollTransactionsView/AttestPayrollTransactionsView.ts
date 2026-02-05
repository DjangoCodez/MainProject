import { ISoeGridOptions, SoeGridOptions } from "../../../Util/SoeGridOptions";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { AttestPayrollTransactionDTO } from "../../../Common/Models/AttestPayrollTransactionDTO";

export class AttestPayrollTransactionsView {

    protected soeGridOptions: ISoeGridOptions;

    private terms: any;

    //@ngInject
    constructor(private $uibModalInstance,
        private $scope: ng.IScope,
        private $q: ng.IQService,
        private $timeout: ng.ITimeoutService,
        private translationService: ITranslationService,
        private uiGridConstants: uiGrid.IUiGridConstants,
        private transactions: AttestPayrollTransactionDTO[]) {

        this.soeGridOptions = new SoeGridOptions("Soe.Time.Dialogs.AttestPayrollTransactionsView", this.$timeout, this.uiGridConstants);
        this.soeGridOptions.enableGridMenu = false;
        this.soeGridOptions.enableColumnMenus = false;
        this.soeGridOptions.enableFiltering = false;
        this.soeGridOptions.enableSorting = true;
        this.soeGridOptions.showGridFooter = false;
        this.soeGridOptions.setMinRowsToShow(20);

        this.setup();
    }

    private setup() {
        this.setupGridColumns();
    }

    private setupGridColumns(): ng.IPromise<any> {
        var keys: string[] = [
            "time.payrollproduct.payrollproduct",
            "time.payroll.retroactive.review.originalunitprice",
            "time.payroll.retroactive.review.retrounitprice",
            "time.payroll.retroactive.review.specifiedunitprice",
            "time.payroll.retroactive.review.specifyunitprice",
            "time.payroll.retroactive.review.amount",
            "time.payroll.retroactive.transactiontype",
            "common.accounting",
            "common.quantity",
            "common.amount",
            "common.price",
            "common.transaction",
            "common.date"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
            this.soeGridOptions.addColumnText("timePayrollTransactionId", this.terms["common.transaction"], "8%");
            this.soeGridOptions.addColumnText("payrollProductName", this.terms["time.payrollproduct.payrollproduct"], "17%");
            this.soeGridOptions.addColumnDate("date", this.terms["common.date"], "10%");
            this.soeGridOptions.addColumnText("quantityString", this.terms["common.quantity"], "7,2%");
            this.soeGridOptions.addColumnText("retroTransactionType", this.terms["time.payroll.retroactive.transactiontype"], "13%");
            this.soeGridOptions.addColumnNumber("unitPrice", this.terms["common.price"], "12%", null, 2);
            this.soeGridOptions.addColumnNumber("amount", this.terms["common.amount"], "10%", null, 2);
            this.soeGridOptions.addColumnText("accountingShortString", this.terms["common.accounting"], null, false, "accountingLongString");

            this.soeGridOptions.setData(this.transactions);
        });
    }

    //Events        

    private cancel() {
        this.$uibModalInstance.dismiss('cancel');
    }
}