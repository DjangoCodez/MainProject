import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { IProductService } from "../../../../Shared/Billing/Products/ProductService";
import { ProductRowDTO } from "../../../../Common/Models/InvoiceDTO";
import { ISoeGridOptionsAg, SoeGridOptionsAg } from "../../../../Util/SoeGridOptionsAg";

export class ChangeDiscountController {
    // Terms
    private terms: any;

    // Search
    private discountPercentage: number = undefined;
    private discount2Percentage: number = undefined;
    private supplementChargePercentage: number = undefined;
    private _marginalIncomeRatio: number = undefined;

    public get marginalIncomeRatio(): number {
        return this._marginalIncomeRatio;
    }
    public set marginalIncomeRatio(value: number) {
        if (value < 100) {
            this._marginalIncomeRatio = value;
        }
    }

    // Grid
    private soeGridOptions: ISoeGridOptionsAg;

    //@ngInject
    constructor(
        private $uibModalInstance,
        private $window: ng.IWindowService,
        private $timeout: ng.ITimeoutService,
        private translationService: ITranslationService,
        private messagingService: IMessagingService,
        private coreService: ICoreService,
        private urlHelperService: IUrlHelperService,
        private productService: IProductService,
        uiGridConstants: uiGrid.IUiGridConstants,
        private $q: ng.IQService,
        private productRows: ProductRowDTO[], 
        private useAdditionalDiscount: boolean = false,
    ) {
        this.soeGridOptions = new SoeGridOptionsAg("Billing.Dialogs.CopyProductRows.ProductRows", this.$timeout);
        this.soeGridOptions.enableGridMenu = false;
        this.soeGridOptions.enableFiltering = true;
        this.soeGridOptions.setMinRowsToShow(8);

        this.$q.all([
            this.loadTerms()]).then(() => {
                this.setupGrid();
            });
    }

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "billing.productrows.productnr",
            "billing.productrows.text",
            "billing.productrows.dialogs.discountpercent",
            "billing.productrows.dialogs.discount2percent",
            "billing.productrows.dialogs.supplementpercent",
            "billing.productrows.marginalincomeratio.short",
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private setupGrid() {
        this.soeGridOptions.addColumnText("productNr", this.terms["billing.productrows.productnr"], null);
        this.soeGridOptions.addColumnText("text", this.terms["common.name"], null);
        this.soeGridOptions.addColumnNumber("discountPercent", this.terms["billing.productrows.dialogs.discountpercent"], null, { decimals: 2 });
        if (this.useAdditionalDiscount)
            this.soeGridOptions.addColumnNumber("discount2Percent", this.terms["billing.productrows.dialogs.discount2percent"], null, { decimals: 2 });
        this.soeGridOptions.addColumnNumber("supplementCharge", this.terms["billing.productrows.dialogs.supplementpercent"], null, { decimals: 2 });
        this.soeGridOptions.addColumnNumber("marginalIncomeRatio", this.terms["billing.productrows.marginalincomeratio.short"], null, { decimals: 2 });

        this.soeGridOptions.addTotalRow("#totals-grid", {
            filtered: this.terms["core.aggrid.totals.filtered"],
            total: this.terms["core.aggrid.totals.total"]
        });

        this.soeGridOptions.finalizeInitGrid();
        this.soeGridOptions.setData(this.productRows);
        this.$timeout(() => this.soeGridOptions.selectAllRows());
    }

    buttonCancelClick() {
        this.close(null, null, null, null, null);
    }

    buttonOkClick() {
        this.close(this.soeGridOptions.getSelectedRows(), this.discountPercentage, this.discount2Percentage, this.supplementChargePercentage, this.marginalIncomeRatio);
    }

    close(result: any, discount: number, discount2: number, supplement: number, ratio: number) {
        if (!result) {
            this.$uibModalInstance.dismiss('cancel');
        }
        else {
            this.$uibModalInstance.close({ rows: result, discount: discount, discount2: discount2, supplement: supplement, ratio: ratio });
        }
    }
}