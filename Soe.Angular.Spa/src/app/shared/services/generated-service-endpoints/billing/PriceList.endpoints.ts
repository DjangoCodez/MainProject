


//Available methods for PriceListController

//get
export const getPriceListTypesGrid = (priceListTypeId?: number) => `V2/Billing/PriceList/PriceListTypes/Grid?priceListTypeId=${priceListTypeId}`;

//get
export const getPriceListTypes = () => `V2/Billing/PriceList/PriceListTypes/`;

//get
export const getPriceListType = (priceListTypeId: number) => `V2/Billing/PriceList/PriceListTypes/${priceListTypeId}`;

//post, takes args: (priceListTypeDTO: number)
export const savePriceList = () => `V2/Billing/PriceList/PriceList`;

//post, takes args: (model: number)
export const savePriceListType = () => `V2/Billing/PriceList/PriceListTypes/`;

//delete
export const deletePriceListType = (priceListTypeId: number) => `V2/Billing/PriceList/PriceListTypes/${priceListTypeId}`;

//post, takes args: (model: number)
export const performPriceUpdate = () => `V2/Billing/PriceList/PriceListTypes/PriceUpdate`;

//get
export const getPriceLists = (priceListTypeId: number) => `V2/Billing/PriceList/PriceListTypes/${priceListTypeId}/PriceLists`;

//get
export const getPriceListsDict = (addEmptyRow: boolean) => `V2/Billing/PriceList/Dict?addEmptyRow=${addEmptyRow}`;

//get
export const getProductPriceLists = (comparisonPriceListTypeId: number, priceListTypeId: number, loadAll: boolean, priceDate: string) => `V2/Billing/PriceList/ProductPriceList/${comparisonPriceListTypeId}/${priceListTypeId}/${loadAll}/${encodeURIComponent(priceDate)}`;


