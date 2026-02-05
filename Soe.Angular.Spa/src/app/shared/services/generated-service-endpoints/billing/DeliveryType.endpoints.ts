


//Available methods for DeliveryTypeController

//get
export const getDeliveryTypesGrid = (deliveryTypeId?: number) => `V2/Billing/DeliveryType/Grid?deliveryTypeId=${deliveryTypeId}`;

//get
export const getDeliveryTypesDict = (addEmptyRow: boolean) => `V2/Billing/DeliveryType/Dict/${addEmptyRow}`;

//get
export const getDeliveryType = (deliveryTypeId: number) => `V2/Billing/DeliveryType/${deliveryTypeId}`;

//post, takes args: (deliveryType: number)
export const saveDeliveryType = () => `V2/Billing/DeliveryType`;

//delete
export const deleteDeliveryType = (deliveryTypeId: number) => `V2/Billing/DeliveryType/${deliveryTypeId}`;


