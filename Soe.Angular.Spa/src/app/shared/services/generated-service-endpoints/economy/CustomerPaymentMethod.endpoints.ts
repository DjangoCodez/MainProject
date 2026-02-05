


//Available methods for CustomerPaymentMethodController

//get
export const getCustomerLedger = (invoiceId: number) => `V2/Economy/CustomerPaymentMethod/CustomerLedger/${invoiceId}`;

//post, takes args: (model: number)
export const saveCustomerLedger = () => `V2/Economy/CustomerPaymentMethod/CustomerLedger/`;

//delete
export const deleteCustomerLedger = (invoiceId: number) => `V2/Economy/CustomerPaymentMethod/CustomerLedger/${invoiceId}`;

//get
export const getInvoiceForPayment = (invoiceId: number) => `V2/Economy/CustomerPaymentMethod/Invoice/Payment/${invoiceId}`;

//get
export const getUnpaidInvoices = (customerId: number) => `V2/Economy/CustomerPaymentMethod/Invoice/Unpaid/${customerId}`;

//get
export const getUnpaidInvoicesForDialog = (customerId: number) => `V2/Economy/CustomerPaymentMethod/Invoice/Unpaid/Dialog/${customerId}`;

//post, takes args: (model: number)
export const getCustomerCentralCountersAndBalance = () => `V2/Economy/CustomerPaymentMethod/CustomerCentralCountersAndBalance/`;

//post, takes args: (model: number)
export const transferCustomerInvoicesToDefinitive = () => `V2/Economy/CustomerPaymentMethod/TransferCustomerInvoicesToDefinitive/`;

//post, takes args: (model: number)
export const transferCustomerInvoicesToVoucher = () => `V2/Economy/CustomerPaymentMethod/TransferCustomerInvoicesToVoucher/`;

//post, takes args: (model: number)
export const exportInvoicesToSOP = () => `V2/Economy/CustomerPaymentMethod/ExportInvoicesToSOP/`;

//post, takes args: (model: number)
export const exportInvoicesToUniMicro = () => `V2/Economy/CustomerPaymentMethod/ExportInvoicesToUniMicro/`;

//post, takes args: (model: number)
export const exportInvoicesToDIRegnskap = () => `V2/Economy/CustomerPaymentMethod/ExportInvoicesToDIRegnskap/`;

//get
export const getPaymentInformationViewsDict = (addEmptyRow: boolean) => `V2/Economy/CustomerPaymentMethod/PaymentInformation/${addEmptyRow}`;

//get
export const getPaymentInformationViews = (supplierId: number) => `V2/Economy/CustomerPaymentMethod/PaymentInformation/${supplierId}`;

//get
export const getPaymentMethodsCustomerGrid = (addEmptyRow: boolean, includePaymentInformationRows: boolean, includeAccount: boolean, onlyCashSales: boolean, paymentMethodId?: number) => `V2/Economy/CustomerPaymentMethod/PaymentMethodCustomerGrid/${addEmptyRow}/${includePaymentInformationRows}/${includeAccount}/${onlyCashSales}/${paymentMethodId || ''}`;

//get
export const getPaymentMethods = (addEmptyRow: boolean, includePaymentInformationRows: boolean, includeAccount: boolean, onlyCashSales: boolean) => `V2/Economy/CustomerPaymentMethod/PaymentMethod/${addEmptyRow}/${includePaymentInformationRows}/${includeAccount}/${onlyCashSales}`;

//get
export const getPaymentMethod = (paymentMethodId: number, loadAccount: boolean, loadPaymentInformationRow: boolean) => `V2/Economy/CustomerPaymentMethod/PaymentMethod/${paymentMethodId}/${loadAccount}/${loadPaymentInformationRow}`;

//post, takes args: (paymentMethod: number)
export const savePaymentMethod = () => `V2/Economy/CustomerPaymentMethod/PaymentMethod`;

//delete
export const deletePaymentMethod = (paymentMethodId: number) => `V2/Economy/CustomerPaymentMethod/PaymentMethod/${paymentMethodId}`;

//get
export const getSysPaymentMethodsDict = (addEmptyRow: boolean) => `V2/Economy/CustomerPaymentMethod/SysPaymentMethod/${addEmptyRow}`;


