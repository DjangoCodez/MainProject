


//Available methods for DeliveryConditionController

//get
export const getDeliveryConditionsGrid = (deliveryConditionId?: number) => `V2/Billing/DeliveryCondition/Grid/${deliveryConditionId || ''}`;

//get
export const getDeliveryConditions = (addEmptyRow: boolean) => `V2/Billing/DeliveryCondition/Dict/${addEmptyRow}`;

//get
export const getDeliveryCondition = (deliveryConditionId: number) => `V2/Billing/DeliveryCondition/${deliveryConditionId}`;

//post, takes args: (deliveryConditionDTO: number)
export const saveDeliveryCondition = () => `V2/Billing/DeliveryCondition`;

//delete
export const deleteDeliveryCondition = (deliveryConditionId: number) => `V2/Billing/DeliveryCondition/${deliveryConditionId}`;


