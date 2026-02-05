


//Available methods for SupplierPurchaseProductController

//post, takes args: (model: number)
export const getSupplierProductList = () => `V2/Billing/Supplier/Product/Products/`;

//post, takes args: (model: number)
export const getSupplierProductListDict = () => `V2/Billing/Supplier/Product/Products/Dict`;

//get
export const getSupplierProductsSmall = (supplierId: number) => `V2/Billing/Supplier/Product/Products/Small/${supplierId}`;

//get
export const getSupplierProductsDict = (supplierId: number) => `V2/Billing/Supplier/Product/Products/Dict/${supplierId}`;

//get
export const getSupplierProduct = (supplierProductId: number) => `V2/Billing/Supplier/Product/${supplierProductId}`;

//get
export const getSupplierByInvoiceProduct = (invoiceProductId: number) => `V2/Billing/Supplier/Product/Suppliers/${invoiceProductId}`;

//get
export const getSupplierProductByInvoiceProduct = (invoiceProductId: number, supplierId: number) => `V2/Billing/Supplier/Product/${invoiceProductId}/${supplierId}`;

//post, takes args: (model: number)
export const saveProduct = () => `V2/Billing/Supplier/Product`;

//delete
export const deleteProduct = (supplierProductId: number) => `V2/Billing/Supplier/Product/${supplierProductId}`;

//post, takes args: (model: number)
export const performPriceUpdate = () => `V2/Billing/Supplier/Product/Products/PriceUpdate`;


