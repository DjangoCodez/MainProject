import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IProductService } from "../../../../Shared/Billing/Products/ProductService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { ProductRowDTO } from "../../../../Common/Models/InvoiceDTO";
import { SOEMessageBoxImage, SOEMessageBoxButtons, ProductRowsRowFunctions } from "../../../../Util/Enumerations";
import { ISoeGridOptionsAg, SoeGridOptionsAg } from "../../../../Util/SoeGridOptionsAg";
import { ActionResultSelect } from "../../../../Util/CommonEnumerations";
import { ProductPricesRequestDTO, ProductPricesRowRequestDTO } from "../../../../Common/Models/PriceListDTO";

export class ChangeWholesellerController {

    // Company settings
    private useAutoSearch: boolean = false;

    // Terms
    private terms: any;

    // Search
    private name: string;
    private number: string;
    private loadingPrices: boolean = false;
    private selectedValue: any;

    private visibleRows: ProductRowDTO[];

    private soeGridOptions: ISoeGridOptionsAg;

    // Properties
    get isWholeseller(): boolean {
        return this.functionType === ProductRowsRowFunctions.ChangeWholeseller;
    }

    get isDeductionType(): boolean {
        return this.functionType === ProductRowsRowFunctions.ChangeDeductionType;
    }

    //@ngInject
    constructor(
        private $uibModalInstance,
        private $timeout: ng.ITimeoutService,
        private translationService: ITranslationService,
        private productService: IProductService,
        private notificationService: INotificationService,
        private $q: ng.IQService,
        private productRows: ProductRowDTO[],
        private values: any[],
        private priceListTypeId: number,
        private customerId: number,
        private currencyId: number, 
        private functionType: ProductRowsRowFunctions,
    ) {

        this.soeGridOptions = new SoeGridOptionsAg("Billing.Dialogs.ChangeWholeseller.ProductRows", $timeout);
        this.soeGridOptions.enableGridMenu = false;
        this.soeGridOptions.setMinRowsToShow(8);

        this.visibleRows = productRows.map(r => ({ ...r } as ProductRowDTO)) //copy, else ag-grid-index will be changed in product rows grid resulting in difficulties when updating the grid values.

        this.$q.all([
            this.loadTerms()]).then(() => {
                this.setupGrid();
            });
    }

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "billing.productrows.productnr",
            "billing.productrows.text",
            "billing.order.syswholeseller",
            "billing.productrows.dialogs.changingwholeseller",
            "billing.productrows.dialogs.failedwholesellerchange",
            "common.name",
            "core.warning",
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total",
            "core.aggrid.totals.selected",
            "billing.products.taxdeductiontype",
            "billing.productrows.pricenotfoundforselectedwholeseller"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private setupGrid() {
        this.soeGridOptions.addColumnText("productNr", this.terms["billing.productrows.productnr"], null);
        this.soeGridOptions.addColumnText("text", this.terms["common.name"], null);

        switch (this.functionType) {
            case ProductRowsRowFunctions.ChangeWholeseller:
                this.soeGridOptions.addColumnText("sysWholesellerName", this.terms["billing.order.syswholeseller"], null);
                break;
            case ProductRowsRowFunctions.ChangeDeductionType:
                this.soeGridOptions.addColumnText("householdDeductionTypeText", this.terms["billing.products.taxdeductiontype"], null);
                break;
        }


        this.soeGridOptions.addTotalRow("#totals-grid", {
            filtered: this.terms["core.aggrid.totals.filtered"],
            total: this.terms["core.aggrid.totals.total"],
            selected: this.terms["core.aggrid.totals.selected"]
        });

        this.soeGridOptions.finalizeInitGrid();

        this.soeGridOptions.setData(this.visibleRows);
        this.$timeout(() => this.soeGridOptions.selectAllRows());
    }

    buttonCancelClick() {
        this.close(null);
    }

    buttonOkClick() {
        switch (this.functionType) {
            case ProductRowsRowFunctions.ChangeWholeseller:
                this.changeWholeseller();
                break;
            case ProductRowsRowFunctions.ChangeDeductionType:
                this.changeDeductionType();
                break;
        }
    }

    changeWholeseller() {
        this.loadingPrices = true;
        const selectedRows = this.soeGridOptions.getSelectedRows();
        const products: ProductPricesRowRequestDTO[] = [];
        _.forEach(selectedRows, (row: ProductRowDTO) => {
            const priceRow = new ProductPricesRowRequestDTO(row.tempRowId, row.productId, row.quantity, null, null);
            products.push(priceRow);
        });

        const getPricesDTO = new ProductPricesRequestDTO();
        getPricesDTO.products = products;
        getPricesDTO.priceListTypeId = this.priceListTypeId;
        getPricesDTO.customerId = this.customerId;
        getPricesDTO.currencyId = this.currencyId;
        getPricesDTO.wholesellerId = this.selectedValue.id;
        getPricesDTO.copySysProduct = true;
        getPricesDTO.includeCustomerPrices = true;
        getPricesDTO.checkProduct = true;

        this.productService.getProductPrices(getPricesDTO).then((result: any) => {
            this.loadingPrices = false;

            if (result.length > 0 && _.filter(result, { success: true }).length > 0)
                this.close(result);
            else {
                let msg = this.terms["billing.productrows.dialogs.failedwholesellerchange"];
                if (result.length > 0 && result[0].errorNumber === ActionResultSelect.PriceNotFound) 
                    msg = msg + "\n" + this.terms["billing.productrows.pricenotfoundforselectedwholeseller"].format(this.selectedValue ? this.selectedValue.name : "?");
                else if ((result.length > 0) && (result[0].errorMessage))
                    msg = msg + "\n" + result[0].errorMessage;

                this.notificationService.showDialog(this.terms["core.warning"], msg, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
            }
        });
    }

    changeDeductionType() {
        const selectedRows = this.soeGridOptions.getSelectedRows();
        const deductionType = _.find(this.values, (v) => v.value === this.selectedValue.value);
        const rows = [];
        if (deductionType) {
            _.forEach(selectedRows, (row: ProductRowDTO) => {
                rows.push({ rowId: row.tempRowId, deductionId: deductionType.value, deductionName: deductionType.label });
            });
        }
        this.close(rows);
    }

    close(result: any) {
        if (!result) {
            this.$uibModalInstance.dismiss('cancel');
        }
        else {
            this.$uibModalInstance.close(result);
        }
    }
}