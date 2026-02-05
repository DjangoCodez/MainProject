


//Available methods for BillingProductController

//get
export const getGridInvoiceProducts = (active: boolean, loadProductUnitAndGroup: boolean, loadAccounts: boolean, loadCategories: boolean, loadTimeCode: boolean) => `V2/Billing/Product/Products/${active}/${loadProductUnitAndGroup}/${loadAccounts}/${loadCategories}/${loadTimeCode}`;

//get
export const getProductsDict = () => `V2/Billing/Product/Products/Dict`;

//get
export const getProductsSmall = () => `V2/Billing/Product/Products/Small`;

//get
export const getProductForSelect = () => `V2/Billing/Product/Products/ForSelect`;

//get
export const getProduct = (productId: number) => `V2/Billing/Product/Products/${productId}`;

//get
export const getProductRowsProduct = (productId: number) => `V2/Billing/Product/ProductRows/${productId}`;

//get
export const getProductsForCleanup = (lastUsedDate: string) => `V2/Billing/Product/Products/GetProductsForCleanup/${encodeURIComponent(lastUsedDate)}`;

//post, takes args: (model: number)
export const getProductRowsProducts = () => `V2/Billing/Product/ProductRows/List/`;

//post, takes args: (model: number)
export const getProductExternalUrls = () => `V2/Billing/Product/ExternalUrls/`;

//get
export const getProductAccounts = (rowId: number, productId: number, projectId: number, customerId: number, employeeId: number, vatType: number, getSalesAccounts: boolean, getPurchaseAccounts: boolean, getVatAccounts: boolean, getInternalAccounts: boolean, isTimeProjectRow: boolean, tripartiteTrade: boolean) => `V2/Billing/Product/Accounts/${rowId}/${productId}/${projectId}/${customerId}/${employeeId}/${vatType}/${getSalesAccounts}/${getPurchaseAccounts}/${getVatAccounts}/${getInternalAccounts}/${isTimeProjectRow}/${tripartiteTrade}`;

//get
export const getHouseholdDeductionTypes = (addEmptyRow: boolean) => `V2/Billing/Product/HouseholdDeductionType/${addEmptyRow}`;

//get
export const getLiftProducts = () => `V2/Billing/Product/LiftProducts`;

//get
export const getProductsGrid = (active: boolean, loadProductUnitAndGroup: boolean, loadAccounts: boolean, loadCategories: boolean, loadTimeCode: boolean, productId?: number) => `V2/Billing/Product/Products/Grid/${active}/${loadProductUnitAndGroup}/${loadAccounts}/${loadCategories}/${loadTimeCode}/${productId || ''}`;

//get
export const getProducts = (active: boolean, loadProductUnitAndGroup: boolean, loadAccounts: boolean, loadCategories: boolean, loadTimeCode: boolean) => `V2/Billing/Product/Products/${active}/${loadProductUnitAndGroup}/${loadAccounts}/${loadCategories}/${loadTimeCode}`;

//get
export const searchInvoiceProducts = (number: string, name: string) => `V2/Billing/Product/Search/${encodeURIComponent(number)}/${encodeURIComponent(name)}`;

//get
export const searchInvoiceProductsExtended = (number: string, name: string, group: string, text: string) => `V2/Billing/Product/Search/Extended/${encodeURIComponent(number)}/${encodeURIComponent(name)}/${encodeURIComponent(group)}/${encodeURIComponent(text)}`;

//post, takes args: (model: number)
export const copyExternalInvoiceProduct = () => `V2/Billing/Product/CopyInvoiceProduct`;

//post, takes args: (model: number)
export const saveInvoiceProduct = () => `V2/Billing/Product/SaveInvoiceProduct`;

//post, takes args: (model: number)
export const updateProductState = () => `V2/Billing/Product/Products/UpdateState`;

//delete
export const deleteProduct = (productId: number) => `V2/Billing/Product/Products/${productId}`;

//post, takes args: (productIds: number[])
export const deleteProducts = () => `V2/Billing/Product/Products/DeleteProducts/`;

//post, takes args: (productIds: number[])
export const inactivateProducts = () => `V2/Billing/Product/Products/InactivateProducts/`;

//post, takes args: (model: number)
export const searchInvoiceProductPrices = () => `V2/Billing/Product/Prices/Search`;

//get
export const getProductPrice = (priceListTypeId: number, productId: number, customerId: number, currencyId: number, wholesellerId: number, quantity: number, returnFormula: boolean, copySysProduct: boolean) => `V2/Billing/Product/Prices/${priceListTypeId}/${productId}/${customerId}/${currencyId}/${wholesellerId}/${quantity}/${returnFormula}/${copySysProduct}`;

//post, takes args: (model: number)
export const getProductPrices = () => `V2/Billing/Product/Prices/Collection`;

//get
export const getProductPriceDecimal = (priceListTypeId: number, productId: number) => `V2/Billing/Product/Prices/${priceListTypeId}/${productId}`;

//get
export const getProductPriceForCustomerInvoice = (productId: number, customerInvoiceId: number, quantity: number) => `V2/Billing/Product/Prices/CustomerInvoice/${productId}/${customerInvoiceId}/${quantity}`;

//get
export const getPriceListPrices = (priceListTypeId: number, loadAll: boolean) => `V2/Billing/Product/Prices/PriceList/${priceListTypeId}/${loadAll}`;

//post, takes args: (model: number)
export const savePriceListPrices = () => `V2/Billing/Product/Prices/PriceList/`;

//post, takes args: (model: number)
export const getCustomerInvoiceProductStatistics = () => `V2/Billing/Product/Invoice/Statistics/`;

//get
export const getVVSProductGroupsForSearch = () => `V2/Billing/Product/VVSGroupsForSearch`;


