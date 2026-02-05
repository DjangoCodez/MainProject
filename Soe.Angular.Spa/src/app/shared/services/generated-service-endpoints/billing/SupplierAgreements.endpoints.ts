


//Available methods for SupplierAgreementsController

//get
export const getSupplierAgreementProviders = () => `V2/Billing/InvoiceSupplierAgreements/Providers/`;

//get
export const getSupplierAgreements = (providerType: number) => `V2/Billing/InvoiceSupplierAgreements/${providerType}`;

//post, takes args: (model: number)
export const saveSupplierAgreementDiscount = () => `V2/Billing/InvoiceSupplierAgreements/Discount/`;

//post, takes args: (model: number)
export const saveSupplierAgreements = () => `V2/Billing/InvoiceSupplierAgreements/Import/`;

//delete
export const deleteSupplierAgreements = (wholesellerId: number, priceListTypeId: number) => `V2/Billing/InvoiceSupplierAgreements/${wholesellerId}/${priceListTypeId}`;


