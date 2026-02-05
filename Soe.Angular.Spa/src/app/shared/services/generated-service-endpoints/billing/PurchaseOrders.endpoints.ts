//Available methods for PurchaseOrdersController

//get
export const getPurchaseStatus = () => `V2/Billing/PurchaseOrders/Status`;

//get
export const getPurchaseList = (
  allItemsSelection: number,
  status: number[],
  purchaseId?: number
) =>
  `V2/Billing/PurchaseOrders/Orders?allItemsSelection=${allItemsSelection}&status=${status}&purchaseId=${purchaseId}`;

//get
export const getPurchase = (purchaseId: number) =>
  `V2/Billing/PurchaseOrders/Order/${purchaseId}`;

//get
export const getPurchaseRows = (purchaseId: number) =>
  `V2/Billing/PurchaseOrders/Order/Rows/${purchaseId}`;

//get
export const getPurchaseRowsForOrder = (invoiceId: number) =>
  `V2/Billing/PurchaseOrders/Order/Rows/ForOrder/${invoiceId}`;

//get
export const getPurchaseTraceViews = (purchaseId: number) =>
  `V2/Billing/PurchaseOrders/TraceViews/${purchaseId}`;

//post, takes args: (model: number)
export const savePurchase = () => `V2/Billing/PurchaseOrders`;

//post, takes args: (model: number)
export const savePurchaseStatus = () => `V2/Billing/PurchaseOrders/Status`;

//delete
export const deletePurchase = (purchaseId: number) =>
  `V2/Billing/PurchaseOrders/${purchaseId}`;

//get
export const getOpenPurchasesForSelect = (forDelivery: boolean) =>
  `V2/Billing/PurchaseOrders/ForSelect/${forDelivery}`;

//get
export const getOpenPurchasesForSelectDict = (forDelivery: boolean) =>
  `V2/Billing/PurchaseOrders/ForSelectDict/${forDelivery}`;

//post, takes args: (dto: number)
export const updatePurchaseFromOrder = () =>
  `V2/Billing/PurchaseOrders/UpdatePurchaseFromOrder`;

//post, takes args: (rows: number)
export const createPurchaseFromStockSuggestion = () =>
  `V2/Billing/PurchaseOrders/CreatePurchaseFromStockSuggestion`;

//post, takes args: (dto: number)
export const sendPurchaseAsEmail = () =>
  `V2/Billing/PurchaseOrders/Order/Email`;

//post, takes args: (dto: number)
export const sendPurchasesAsEmail = () =>
  `V2/Billing/PurchaseOrders/Orders/Email`;

//get
export const getDeliveryAddresses = (customerOrderId: number) =>
  `V2/Billing/PurchaseOrders/DeliveryAddresses/${customerOrderId}`;
