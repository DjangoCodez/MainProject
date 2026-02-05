


//Available methods for InvoicePaymentConditionController

//get
export const getPaymentConditions = () => `V2/Billing/Invoice/PaymentCondition/`;

//get
export const getPaymentConditions0 = (paymentConditionId: number) => `V2/Billing/Invoice/PaymentCondition/${paymentConditionId}`;

//post, takes args: (paymentConditionDTO: number)
export const savePaymentCondition = () => `V2/Billing/Invoice/PaymentCondition`;

//delete
export const deletePaymentCondition = (paymentConditionId: number) => `V2/Billing/Invoice/PaymentCondition/${paymentConditionId}`;


