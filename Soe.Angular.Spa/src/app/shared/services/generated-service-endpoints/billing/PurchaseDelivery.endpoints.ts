//Available methods for PurchaseDeliveryController

//get
export const getDeliveryList = (
  allItemsSelection: number,
  purchaseDeliveryId?: number
) =>
  `V2/Billing/Purchase/Deliveries/${allItemsSelection}?purchaseDeliveryId=${
    purchaseDeliveryId || ''
  }`;

//get
export const getDelivery = (purchaseDeliveryId: number) =>
  `V2/Billing/Purchase/Delivery/${purchaseDeliveryId}`;

//get
export const getDeliveryRowsFromPurchase = (
  purchaseId: number,
  supplierId: number
) => `V2/Billing/Purchase/Delivery/PurchaseRows/${purchaseId}/${supplierId}`;

//get
export const getPurchaseDeliveryRowsByPurchaseId = (purchaseId: number) =>
  `V2/Billing/Purchase/DeliveryRows/${purchaseId}`;

//get
export const getDeliveryRows = (purchaseDeliveryId: number) =>
  `V2/Billing/Purchase/Delivery/Rows/${purchaseDeliveryId}`;

//post, takes args: (model: number)
export const saveDelivery = () => `V2/Billing/Purchase/Delivery`;
