import { IPriceListDTO, IProductPricesRequestDTO, IProductPricesRowRequestDTO } from "../../Scripts/TypeLite.Net4";
import { SoeEntityState } from "../../Util/CommonEnumerations";

export class PriceListDTO implements IPriceListDTO {
    created: Date;
    createdBy: string;
    modified: Date;
    modifiedBy: string;
    price: number;
    priceListId: number;
    priceListTypeId: number;
    productId: number;
    quantity: number;
    sysPriceListTypeName: string;
    state: SoeEntityState;
    startDate: Date;
    stopDate: Date;
    name: string;

    //extension
    isModified: boolean;
    constructor() {
        this.productId = 0;
        this.price = 0;
        this.quantity = 0; 
        this.state = SoeEntityState.Active;
    }
}
export class ProductPricesRequestDTO implements IProductPricesRequestDTO {
    checkProduct: boolean;
    copySysProduct: boolean;
    currencyId: number;
    customerId: number;
    includeCustomerPrices: boolean;
    priceListTypeId: number;
    products: IProductPricesRowRequestDTO[];
    returnFormula: boolean;
    timeRowIsLoadingProductPrice: boolean;
    wholesellerId: number;
}

export class ProductPricesRowRequestDTO implements IProductPricesRowRequestDTO {
    productId: number;
    quantity: number;
    tempRowId: number;
    wholesellerName: string;
    purchasePrice: number;
    constructor(tempRowId: number, productId: number, quantity: number, wholesellerName: string, purchasePrice:number) {
        this.tempRowId = tempRowId;
        this.productId = productId;
        this.quantity = quantity;
        this.wholesellerName = wholesellerName;
        this.purchasePrice = purchasePrice;
    }
}



