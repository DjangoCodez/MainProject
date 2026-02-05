


//Available methods for SupplierProductPriceListController

//get
export const getSupplierPricelistsBySupplier = (supplierId?: number) => `V2/Billing/Supplier/Product/Pricelist/Grid/${supplierId || ''}`;

//get
export const getSupplierPricelistById = (pricelistId: number) => `V2/Billing/Supplier/Product/Pricelist/${pricelistId}`;

//delete
export const deleteSupplierPricelist = (pricelistId: number) => `V2/Billing/Supplier/Product/Pricelist/${pricelistId}`;

//get
export const getSupplierPricelist = (pricelistId: number, includeComparison: boolean) => `V2/Billing/Supplier/Product/Pricelist/Prices/${pricelistId}/${includeComparison}`;

//post, takes args: (model: number)
export const getSupplierProductPriceCompare = () => `V2/Billing/Supplier/Product/Pricelist/Compare`;

//post, takes args: (model: number)
export const saveSupplierPricelist = () => `V2/Billing/Supplier/Product/Pricelist`;

//get
export const getSupplierPricelistImport = (importToPriceList: boolean, importPrices: boolean, multipleSuppliers: boolean) => `V2/Billing/Supplier/Product/Pricelist/Import/${importToPriceList}/${importPrices}/${multipleSuppliers}`;

//post, takes args: (model: number)
export const performPricelistImport = () => `V2/Billing/Supplier/Product/Pricelist/Import/Perform`;


