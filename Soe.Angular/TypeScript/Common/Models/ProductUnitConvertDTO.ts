import { IProductUnitConvertDTO } from "../../Scripts/TypeLite.Net4";

export class ProductUnitConvertDTO implements IProductUnitConvertDTO {
    baseProductUnitId: number;
    baseProductUnitName: string;
    convertFactor: number;
    isDeleted: boolean;
    isModified: boolean;
    productId: number;
    productName: string;
    productNr: string;
    productUnitConvertId: number;
    productUnitId: number;
    productUnitName: string;

    get nameAndConvertFactor(): string {
        return this.productUnitConvertId > 0 ? this.productUnitName + ' (' + this.convertFactor + ')' : "";
    }
}

