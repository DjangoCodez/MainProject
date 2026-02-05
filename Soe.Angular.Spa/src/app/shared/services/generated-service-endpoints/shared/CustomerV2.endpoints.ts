


//Available methods for CustomerV2Controller

//post, takes args: (model: number)
export const getCustomersBySearch = () => `V2/Shared/Customer/Search`;

//get
export const getCustomers = (onlyActive: boolean) => `V2/Shared/Customer?onlyActive=${onlyActive}`;

//get
export const getCustomersForGrid = (onlyActive: boolean, customerId?: number) => `V2/Shared/Customer/Grid?onlyActive=${onlyActive}&customerId=${customerId}`;

//get
export const getCustomersByCompanyDict = (onlyActive: boolean, addEmptyRow: boolean) => `V2/Shared/Customer/Dict?onlyActive=${onlyActive}&addEmptyRow=${addEmptyRow}`;

//get
export const getCustomer = (customerId: number, loadActor: boolean, loadAccount: boolean, loadNote: boolean, loadCustomerUser: boolean, loadContactAddresses: boolean, loadCategories: boolean) => `V2/Shared/Customer/${customerId}/${loadActor}/${loadAccount}/${loadNote}/${loadCustomerUser}/${loadContactAddresses}/${loadCategories}`;

//get
export const getCustomerForExport = (customerId: number) => `V2/Shared/Customer/Export/${customerId}`;

//get
export const getCashCustomer = () => `V2/Shared/Customer/CashCustomer`;

//post, takes args: (model: number)
export const getCustomerCentralCountersAndBalance = () => `V2/Shared/Customer/CustomerCentralCountersAndBalance/`;

//post, takes args: (model: number)
export const getCustomerStatistics = () => `V2/Shared/Customer/Statistics`;

//post, takes args: (model: number)
export const getSalesStatisticsGridData = () => `V2/Shared/Customer/StatisticsAllCustomers`;

//get
export const getNextCustomerNr = () => `V2/Shared/Customer/NextCustomerNr`;

//get
export const getCustomerReferences = (customerId: number, addEmptyRow: boolean) => `V2/Shared/Customer/Reference/${customerId}/${addEmptyRow}`;

//get
export const getCustomerEmailAddresses = (customerId: number, loadContactPersonsEmails: boolean, addEmptyRow: boolean) => `V2/Shared/Customer/Email/${customerId}/${loadContactPersonsEmails}/${addEmptyRow}`;

//get
export const getCustomerGlnNumbers = (customerId: number, addEmptyRow: boolean) => `V2/Shared/Customer/GLN/${customerId}/${addEmptyRow}`;

//delete
export const deleteCustomer = (customerId: number) => `V2/Shared/Customer/${customerId}`;

//post, takes args: (saveModel: number)
export const saveCustomer = () => `V2/Shared/Customer`;

//post, takes args: (model: number)
export const updateCustomersState = () => `V2/Shared/Customer/UpdateState`;

//post, takes args: (items: number)
export const updateIsPrivatePerson = () => `V2/Shared/Customer/UpdateIsPrivatePerson`;

//post, takes args: (customerUpdateGridDto: number)
export const updateGrid = () => `V2/Shared/Customer/UpdateGrid`;

//post, takes args: (model: number)
export const getEInvoiceRecipients = () => `V2/Shared/Customer/SearchEinvoiceRecipients`;


