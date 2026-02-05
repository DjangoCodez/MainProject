


//Available methods for HandleBillingController

//post, takes args: (model: number)
export const searchCustomerInvoiceRows = () => `V2/Billing/HandleBilling/Search/`;

//post, takes args: (model: number)
export const orderRowChangeAttestState = () => `V2/Billing/HandleBilling/ChangeAttestState`;

//post, takes args: (model: number)
export const transferOrdersToInvoice = () => `V2/Billing/HandleBilling/TransferOrdersToInvoice`;

//post, takes args: (model: number)
export const batchSplitTimeRows = () => `V2/Billing/HandleBilling/BatchSplitTimeRows`;

//get
export const getExpenseRows = (customerInvoiceId: number, customerInvoiceRowId: number) => `V2/Billing/HandleBilling/ExpenseRows/${customerInvoiceId}/${customerInvoiceRowId}`;

//get
export const getProjectTimeBlocksForInvoiceRow = (invoiceId: number, customerInvoiceRowId: number) => `V2/Billing/HandleBilling/ProjectTimeBlock/${invoiceId}/${customerInvoiceRowId}`;

//get
export const getProjects = () => `V2/Billing/HandleBilling/Projects/`;

//get
export const getCustomers = () => `V2/Billing/HandleBilling/Customers/`;

//get
export const getOrders = () => `V2/Billing/HandleBilling/Orders/`;


