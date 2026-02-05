import { ISoeGridOptionsAg, SoeGridOptionsAg } from "../../../../Util/SoeGridOptionsAg";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { ProductRowDTO } from "../../../../Common/Models/InvoiceDTO";
import { SoeOriginStatusClassification } from "../../../../Util/CommonEnumerations";

export class SelectProductRowController {

    // Terms
    private terms: any;

    // Values
    private classification: SoeOriginStatusClassification;
    private selectedCustomerInvoice: number;
    private selectedPosition: number;
    private moveSelection: string;
    private selectLabel: string;


    // Flags
    private working: boolean = false;
    private showStatus: boolean = false;
    private showNumbersSelect: boolean;
    private showPositionSelect: boolean;
    private rowsCopied: boolean = false;

    // Grid
    private soeGridOptions: ISoeGridOptionsAg;

    //@ngInject
    constructor(
        private $uibModalInstance,
        private $timeout: ng.ITimeoutService,
        private translationService: ITranslationService,
        private $q: ng.IQService,
        private productRows: ProductRowDTO[],
        private toolbarTitle: string) {
    }

    private $onInit() {
        this.soeGridOptions = new SoeGridOptionsAg("Billing.Dialogs.CopyProductRows.ProductRows", this.$timeout);
        this.soeGridOptions.enableGridMenu = false;
        this.soeGridOptions.enableSingleSelection();
        this.soeGridOptions.setMinRowsToShow(8);

        this.$q.all([
            //this.loadReadOnlyPermissions(),
            this.loadTerms()]).then(() => {
                this.setupGrid();
            });
    }

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "common.rownr",
            "common.name",
            "core.succeeded",
            "core.failed",
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private setupGrid() {
    
        const keys: string[] = [
            "common.rownr",
            "common.name",
            "common.date",
            "billing.productrows.productnr",
            "billing.productrows.text",
            "billing.productrows.quantity",
        ];
        this.translationService.translateMany(keys).then((terms) => {

            const numberCol = this.soeGridOptions.addColumnText("rowNr", terms["common.rownr"], null);
            numberCol.name = "numberCol";

            const productCol = this.soeGridOptions.addColumnText("productNr", terms["billing.productrows.productnr"], null);
            productCol.name = "productCol";
            const textCol = this.soeGridOptions.addColumnText("text", terms["common.name"], null);
            textCol.name = "textCol";
            this.soeGridOptions.addColumnNumber("quantity", terms["billing.productrows.quantity"], null);
            this.soeGridOptions.addColumnDate("date", terms["common.date"], null);

            this.soeGridOptions.finalizeInitGrid();

            this.soeGridOptions.setData(this.productRows);
        })
    }

    buttonCancelClick() {
        this.close(0);
    }

    buttonOkClick() {
        console.log("ok", this.soeGridOptions.getSelectedRows());
        const selectedRows = this.soeGridOptions.getSelectedRows();
        if (selectedRows && selectedRows.length === 1) {
            this.close(selectedRows[0].customerInvoiceRowId);
        }
        else {
            this.close(0);
        }
    }

    close(result: any) {
        if (!result) {
            this.$uibModalInstance.dismiss('cancel');
        } else {
            this.$uibModalInstance.close(result);
        }
    }
}