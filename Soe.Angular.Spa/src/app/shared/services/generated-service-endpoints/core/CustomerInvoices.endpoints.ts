


//Available methods for CustomerInvoicesController

//get
export const getInvoices = (classification: number, originType: number, loadOpen: boolean, loadClosed: boolean, onlyMine: boolean, loadActive: boolean, allItemsSelection: number, billing: boolean) => `V2/Core/CustomerInvoices/${classification}/${originType}/${loadOpen}/${loadClosed}/${onlyMine}/${loadActive}/${allItemsSelection}/${billing}`;

//post, takes args: (model: number)
export const getInvoicesForProjectCentral = () => `V2/Core/CustomerInvoices`;

//post, takes args: (model: number)
export const getInvoicesForProjectCentral0 = () => `V2/Core/CustomerInvoices/ProjectCentral/`;

//post, takes args: (model: number)
export const getInvoicesForCustomerCentral = () => `V2/Core/CustomerInvoices/CustomerCentral/`;

//post, takes args: (filterModels: number)
export const getFilteredCustomerInvoices = () => `V2/Core/CustomerInvoices/Filtered/`;

//post, takes args: (model: number)
export const transferCustomerInvoices = () => `V2/Core/CustomerInvoices/Transfer`;

//get
export const getReminderPrintedInformation = (invoiceId: number) => `V2/Core/CustomerInvoices/ReminderInformation/${invoiceId}`;

//get
export const getReminderPrintedInformation0 = (customerId: number, originType: number, classification: number, registrationType: number, orderByNumber: boolean) => `V2/Core/CustomerInvoices/NumbersDict/${customerId}/${originType}/${classification}/${registrationType}/${orderByNumber}`;

//get
export const getCustomerInvoiceRowsForInvoice = (invoiceId: number) => `V2/Core/CustomerInvoices/Rows/${invoiceId}`;

//get
export const getCustomerInvoiceRowsSmallForInvoice = (invoiceId: number) => `V2/Core/CustomerInvoices/RowsSmall/${invoiceId}`;

//get
export const getServiceOrdersForAgreementDetails = (invoiceId: number) => `V2/Core/CustomerInvoices/ServiceOrdersForAgreement/${invoiceId}`;

//post, takes args: (model: number)
export const copyCustomerInvoiceRows = () => `V2/Core/CustomerInvoices/CopyRows/`;

//get
export const getPendingCustomerInvoiceReminders = (customerId: number, loadCustomer: boolean, loadProduct: boolean) => `V2/Core/CustomerInvoices/PendingReminders/${customerId}/${loadCustomer}/${loadProduct}`;

//get
export const getPendingCustomerInvoiceInterests = (customerId: number, loadCustomer: boolean, loadProduct: boolean) => `V2/Core/CustomerInvoices/PendingInterests/${customerId}/${loadCustomer}/${loadProduct}`;

//post, takes args: (model: number)
export const getInvoicesBySearch = () => `V2/Core/CustomerInvoices/SearchSmall/`;

//delete
export const deletePendingCustomerInvoiceReminders = (customerId: number) => `V2/Core/CustomerInvoices/PendingReminders/${customerId}`;

//delete
export const deletePendingCustomerInvoiceInterests = (customerId: number) => `V2/Core/CustomerInvoices/PendingInterests/${customerId}`;


