//Available methods for InvoicePaymentController

//get
export const getPaymentTraceViews = (paymentRowId: number) =>
  `Billing/InvoicePayment/Payment/GetPaymentTraceViews/${paymentRowId}`;

//post, takes args: (model: number)
export const saveCashPayments = () =>
  `Billing/InvoicePayment/Payment/CashPayment/`;
