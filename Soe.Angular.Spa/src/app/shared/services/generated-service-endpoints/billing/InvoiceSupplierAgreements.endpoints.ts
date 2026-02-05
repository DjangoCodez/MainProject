


//Available methods for InvoiceSupplierAgreementsController

//get
export const getSupplierAgreementProviders = () => `Billing/InvoiceSupplierAgreements/Providers/`;

//get
export const getSupplierAgreements = (providerType: number) => `Billing/InvoiceSupplierAgreements/${providerType}`;

//post, takes args: (model: number)
export const saveSupplierAgreementDiscount = () => `Billing/InvoiceSupplierAgreements/Discount/`;

//post, takes args: (model: number)
export const saveSupplierAgreements = () => `Billing/InvoiceSupplierAgreements/Import/`;

//delete
export const deleteSupplierAgreements = (wholesellerId: number, priceListTypeId: number) => `Billing/InvoiceSupplierAgreements/${wholesellerId}/${priceListTypeId}`;


