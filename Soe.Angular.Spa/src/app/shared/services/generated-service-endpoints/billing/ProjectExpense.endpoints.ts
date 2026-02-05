


//Available methods for ProjectExpenseController

//get
export const getExpenseRow = (expenseRowId: number) => `V2/Billing/ProjectExpense/Row/${expenseRowId}`;

//get
export const getExpenseRows = (customerInvoiceId: number, customerInvoiceRowId: number) => `V2/Billing/ProjectExpense/Rows/${customerInvoiceId}/${customerInvoiceRowId}`;

//post, takes args: (model: number)
export const getExpenseRowsForGridFiltered = () => `V2/Billing/ProjectExpense/Rows/Filtered`;

//post, takes args: (model: number)
export const saveExpenseRowsValidation = () => `V2/Billing/ProjectExpense/Rows/Validate`;

//post, takes args: (model: number)
export const saveExpenseRow = () => `V2/Billing/ProjectExpense/Rows/`;

//delete
export const deleteExpenseRow = (expenseRowId: number) => `V2/Billing/ProjectExpense/Row/${expenseRowId}`;

//get
export const getExpenseReportUrl = (invoiceId: number, projectId: number) => `V2/Billing/ProjectExpense/Report/${invoiceId}/${projectId}`;


