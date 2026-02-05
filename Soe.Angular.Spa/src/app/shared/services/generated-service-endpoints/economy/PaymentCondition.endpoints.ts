


//Available methods for PaymentConditionController

//get
export const getPaymentConditionsGrid = (paymentConditionId?: number) => `V2/Economy/PaymentCondition/Grid/${paymentConditionId || ''}`;

//get
export const getSmallGenericTypePaymentConditions = (addEmptyRow: boolean) => `V2/Economy/PaymentCondition/SmallGenericType/?addEmptyRow=${addEmptyRow}`;

//get
export const getPaymentConditions = () => `V2/Economy/PaymentCondition/DTOs/`;

//get
export const getPaymentCondition = (paymentConditionId: number) => `V2/Economy/PaymentCondition/${paymentConditionId}`;

//post, takes args: (paymentConditionDTO: number)
export const savePaymentCondition = () => `V2/Economy/PaymentCondition/PaymentCondition`;

//delete
export const deletePaymentCondition = (paymentConditionId: number) => `V2/Economy/PaymentCondition/${paymentConditionId}`;


