import { IInvoiceProductDTO } from '../../models/webapi/generated-interfaces/InvoiceProductDTOs';
import { IStockInventoryFilterDTO, IStockInventoryHeadDTO, IStockDTO, IAccountingSettingsRowDTO, IStockTransactionDTO} from '../../models/webapi/generated-interfaces/SOECompModelDTOs';

export const defaultStockInventoryHeadDTO: IStockInventoryHeadDTO = {
    stockInventoryHeadId: 0,
    inventoryStart: undefined,
    inventoryStop: undefined,
    headerText: "Auto Test default",
    created: undefined,
    createdBy: '',
    modified: undefined,
    modifiedBy: '',
    stockId: 1127,
    stockName: "",
    stockCode: "",
    stockInventoryRows: [
        {
            stockInventoryRowId: 0,
            stockInventoryHeadId: 0,
            stockProductId: 7714,
            startingSaldo: 0,
            inventorySaldo: 0,
            difference: 0,
            productNumber: "002 Auto default",
            productName: "002 Auto default",
            productGroupId: undefined,
            productGroupCode: "",
            productGroupName: "",
            unit: "ss",
            avgPrice: 0,
            shelfId: 332,
            shelfCode: "Autodefault",
            shelfName: "Autodefault",
            orderedQuantity: 0,
            reservedQuantity: 0,
            transactionDate: undefined,
            created: undefined,
            createdBy: "",
            modified: undefined,
            modifiedBy: "",
        },
    ],
};


export const defaultStockInventoryFilterDTO: IStockInventoryFilterDTO = {
    stockId: 1127,
    productNrFrom: "",
    productNrTo: "",   
    shelfIds: [],       
    productGroupIds: [] 
};

export const defaultStockDTO: IStockDTO = {
    stockId: 0,
    name: "AutoTest default",
    code: "Auto default",
    isExternal: false,
    stockShelves: [
        {
            stockShelfId: -1,
            stockId: 0,
            code: "Auto default",
            name: "AutoRack default",
            stockName: "AutoTest default",
            isDelete: false,
        },
    ],
    stockShelfId: 0,
    stockShelfName: "",
    accountingSettings: [],
    deliveryAddressId: 0,
    state: 0,
    created: undefined,
    createdBy: "",
    modified: undefined,
    modifiedBy: "",
    saldo:0,
    avgPrice: 0,                    // Added with default value
    stockProductId: undefined,      // Added as optional
    purchaseTriggerQuantity: 0,     // Added with default value
    purchaseQuantity: 0,            // Added with default value
    deliveryLeadTimeDays: 0 ,
    stockProducts: []          // Added as optional with default value   
};

export const defaultInvoiceProductDTO: IInvoiceProductDTO = {
    sysProductId: undefined,
    sysPriceListHeadId: undefined,
    vatType: 0, // Assuming 0 as the default enum value for TermGroup_InvoiceProductVatType
    vatFree: false,
    ean: "",
    purchasePrice: 0,
    sysWholesellerName: "",
    calculationType: 0, // Assuming 0 as the default enum value for TermGroup_InvoiceProductCalculationType
    guaranteePercentage: undefined,
    timeCodeId: undefined,
    priceListOrigin: 0,
    showDescriptionAsTextRow: false,
    showDescrAsTextRowOnPurchase: false,
    dontUseDiscountPercent: false,
    useCalculatedCost: false,
    vatCodeId: undefined,
    householdDeductionType: undefined,
    householdDeductionPercentage: undefined,
    isStockProduct: false,
    weight: undefined,
    intrastatCodeId: undefined,
    sysCountryId: undefined,
    defaultGrossMarginCalculationType: undefined,
    isExternal: false,
    salesPrice: 0,
    isSupplementCharge: false,
    priceLists: [], // Assuming IPriceListDTO[] defaults to an empty array
    categoryIds: [], // Defaults to an empty array
    accountingSettings: [], // Assuming IAccountingSettingsRowDTO[] defaults to an empty array
    sysProductType: undefined,
};

export const defaultAccountingSettingsRowDTO: IAccountingSettingsRowDTO = {
    type: 0,
    accountDim1Nr: 0,
    account1Id: 0,
    account1Nr: "",
    account1Name: "",
    accountDim2Nr: 0,
    account2Id: 0,
    account2Nr: "",
    account2Name: "",
    accountDim3Nr: 0,
    account3Id: 0,
    account3Nr: "",
    account3Name: "",
    accountDim4Nr: 0,
    account4Id: 0,
    account4Nr: "",
    account4Name: "",
    accountDim5Nr: 0,
    account5Id: 0,
    account5Nr: "",
    account5Name: "",
    accountDim6Nr: 0,
    account6Id: 0,
    account6Nr: "",
    account6Name: "",
    percent: 0,
};

export const defaultStockTransaction: IStockTransactionDTO = {
    stockTransactionId: 0,
    stockProductId: 7740,
    actionType: 2, 
    quantity: 10,
    price: 250,
    note: "",
    created: undefined, 
    createdBy: "",
    voucherId: undefined, 
    voucherNr: "", 
    transactionDate: new Date("2025-01-24T00:00:00.000Z"),
    targetStockId: 30188, 
    parentStockTransactionId: undefined, 
    actionTypeName: "",
    reservedQuantity: 0, 
    productUnitConvertId: undefined, 
    productId: 30188,
    stockId: 1130,
    stockShelfId: 0, 
    purchaseId: undefined, 
    stockInventoryHeadId: undefined,
    sourceLabel: "", 
    sourceNr: "",
    originType: undefined ,
    childStockTransaction: ""
}




