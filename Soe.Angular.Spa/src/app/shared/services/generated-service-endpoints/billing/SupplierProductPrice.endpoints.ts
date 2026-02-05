


//Available methods for SupplierProductPriceController

//get
export const getSupplierProductPrices = (supplierProductId: number) => `V2/Billing/Supplier/Product/Price/List/${supplierProductId}`;

//get
export const getSupplierInvoiceProductPrice = (supplierProductId: number, currentDate: string, quantity: number, currencyId: number) => `V2/Billing/Supplier/Product/Price/${supplierProductId}/${encodeURIComponent(currentDate)}/${quantity}/${currencyId}`;

//get
export const getInvoiceProductPrice = (productId: number, supplierId: number, currentDate: string, quantity: number, currencyId: number) => `V2/Billing/Supplier/Product/Price/${productId}/${supplierId}/${encodeURIComponent(currentDate)}/${quantity}/${currencyId}`;


