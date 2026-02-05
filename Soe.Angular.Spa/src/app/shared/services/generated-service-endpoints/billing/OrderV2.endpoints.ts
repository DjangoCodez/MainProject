


//Available methods for OrderV2Controller

//get
export const getOrder = (invoiceId: number, includeCategories: boolean, includeRows: boolean) => `V2/Billing/Order/${invoiceId}/${includeCategories}/${includeRows}`;

//get
export const getAccountRows = (invoiceId: number) => `V2/Billing/Order/AccountRows/${invoiceId}`;

//get
export const getSplitAccountingRows = (customerInvoiceRowId: number, excludeVatRows: boolean) => `V2/Billing/Order/SplitAccountingRows/${customerInvoiceRowId}/${excludeVatRows}`;

//get
export const getOrderTemplates = () => `V2/Billing/Order/Template`;

//get
export const getOrderTemplates0 = (originType: number) => `V2/Billing/Order/Templates/${originType}`;

//get
export const useEdi = () => `V2/Billing/Order/Edi`;

//get
export const getEdiEntryInfo = (ediEntryId: number) => `V2/Billing/Order/GetEdiEntryInfo/${ediEntryId}`;

//get
export const originUsers = (invoiceId: number) => `V2/Billing/Order/OriginUsers/${invoiceId}`;

//get
export const canUserCreateInvoice = (currentAttestStateId: number) => `V2/Billing/Order/CanUserCreateInvoice/${currentAttestStateId}`;

//get
export const getOrderTraceViews = (orderId: number) => `V2/Billing/Order/GetOrderTraceViews/${orderId}`;

//get
export const checkCustomerCreditLimit = (customerId: number, creditLimit: number) => `V2/Billing/Order/CreditLimit/${customerId}/${creditLimit}`;

//get
export const getOpenOrdersDict = () => `V2/Billing/Order/OpenDict/`;

//get
export const getOrderSummary = (invoiceId: number, projectId: number) => `V2/Billing/Order/Summary/${invoiceId}/${projectId}`;

//post, takes args: (model: number)
export const saveOrder = () => `V2/Billing/Order`;

//post, takes args: (orderId: number)
export const unlockOrder = (orderId: number) => `V2/Billing/Order/Unlock/${orderId}`;

//post, takes args: (orderId: number)
export const closeOrder = (orderId: number) => `V2/Billing/Order/Close/${orderId}`;

//post, takes args: (orderId: number, userId: number)
export const updateReadyState = (orderId: number, userId: number) => `V2/Billing/Order/UpdateReadyState/${orderId}/${userId}`;

//post, takes args: (orderId: number, orderNr: string, userIds: string)
export const sendReminderForReadyState = (orderId: number, orderNr: string, userIds: string) => `V2/Billing/Order/SendReminderForReadyState/${orderId}/${encodeURIComponent(orderNr)}/${encodeURIComponent(userIds)}`;

//post, takes args: (orderId: number, userIds: string)
export const clearReadyState = (orderId: number, userIds: string) => `V2/Billing/Order/ClearReadyState/${orderId}/${encodeURIComponent(userIds)}`;

//post, takes args: (orderId: number, keepAsPlanned: boolean)
export const setOrderKeepAsPlanned = (orderId: number, keepAsPlanned: boolean) => `V2/Billing/Order/SetOrderKeepAsPlanned/${orderId}/${keepAsPlanned}`;

//post, takes args: (model: number)
export const searchCustomerInvoiceRows = () => `V2/Billing/Order/HandleBilling/Search/`;

//post, takes args: (model: number)
export const orderRowChangeAttestState = () => `V2/Billing/Order/HandleBilling/ChangeAttestState`;

//post, takes args: (model: number)
export const transferOrdersToInvoice = () => `V2/Billing/Order/HandleBilling/TransferOrdersToInvoice`;

//post, takes args: (model: number)
export const batchSplitTimeRows = () => `V2/Billing/Order/HandleBilling/BatchSplitTimeRows`;

//delete
export const deleteOrder = (invoiceId: number, deleteProject: boolean) => `V2/Billing/Order/${invoiceId}/${deleteProject}`;

//post, takes args: (customerInvoiceRowId: number)
export const recalculateTimeRows = (customerInvoiceRowId: number) => `V2/Billing/Order/RecalculateTimeRow/${customerInvoiceRowId}`;


