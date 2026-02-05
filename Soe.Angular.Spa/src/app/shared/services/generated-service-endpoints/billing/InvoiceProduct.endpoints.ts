


//Available methods for InvoiceProductController

//get
export const getInvoiceProductsSmall = (excludeExternal: boolean) => `V2/Billing/InvoiceProduct/Small/${excludeExternal}`;

//get
export const getInvoiceProducts = (invoiceProductVatType: number, addEmptyRow: boolean) => `V2/Billing/InvoiceProduct/Small/${invoiceProductVatType}/${addEmptyRow}`;


