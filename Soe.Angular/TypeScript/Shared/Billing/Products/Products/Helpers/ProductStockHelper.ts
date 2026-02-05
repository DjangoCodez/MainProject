import { StockDTO } from "../../../../../Common/Models/StockDTO";
import { IProductService } from "../../ProductService";

export class ProductStockHelper {

    public stocksForProduct: StockDTO[] = [];
    public stocks: any[] = [];

    //@ngInject
    constructor(
        private productService: IProductService
    ) {

    }

    public onRenderStockExpander(productId: number) {
        this.loadStocksForProduct(productId);
    }

    private loadStocks() {
        this.stocks = [];
        _.forEach(this.stocksForProduct, (x) => {
            this.stocks.push({ id: x.stockId, name: x.name });
        });
    }

    public loadStocksForProduct(productId:number): ng.IPromise<any> {
        return this.productService.getStocksByProduct(productId).then((x) => {
            this.stocksForProduct = x;
            this.loadStocks();
        });
    }
}
