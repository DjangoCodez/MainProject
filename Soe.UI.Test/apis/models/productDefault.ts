import { ISupplierProductDTO } from "../../models/webapi/generated-interfaces/SupplierProductDTOs";

export const defaultSupplierProductDTO: ISupplierProductDTO = {
    supplierProductId: 0,
    supplierId: 1592,
    supplierProductNr: "145235655",
    supplierProductName: "4512kjhhh",
    supplierProductUnitId: 2,
    supplierProductCode: "",
    packSize: 0,
    deliveryLeadTimeDays: 0,
    productId: 342,
    priceRows: [
        {
            supplierProductPriceId: 0,
            supplierProductId: 0,
            quantity: 110,
            price: 440,
            currencyCode: "SEK",
            startDate: new Date("1970-01-27T00:00:00.000Z"),
            endDate: new Date("1970-01-31T00:00:00.000Z"),
            state: 0,
            currencyId: 2,
            sysCurrencyId: 0
        }
    ],
    createdBy: "",
    modifiedBy: ""
};