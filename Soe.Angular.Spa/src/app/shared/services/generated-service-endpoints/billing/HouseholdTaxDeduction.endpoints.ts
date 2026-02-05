


//Available methods for HouseholdTaxDeductionController

//get
export const getHouseholdTaxDeductionRowsByCustomer = (customerId: number, addEmptyRow: boolean, showAllApplicants: boolean) => `V2/Billing/HouseholdTaxDeduction/HouseholdTaxDeduction/Customer/${customerId}/${addEmptyRow}/${showAllApplicants}`;

//get
export const getHouseholdTaxDeductionRows = (classificationGroup: number, taxDeductionType: number) => `V2/Billing/HouseholdTaxDeduction/HouseholdTaxDeduction/${classificationGroup}/${taxDeductionType}`;

//get
export const getHouseholdTaxDeductionRowsApply = () => `V2/Billing/HouseholdTaxDeduction/HouseholdTaxDeduction/Apply/`;

//get
export const getHouseholdTaxDeductionRowsApplied = () => `V2/Billing/HouseholdTaxDeduction/HouseholdTaxDeduction/Applied/`;

//get
export const getHouseholdTaxDeductionRowsDenied = () => `V2/Billing/HouseholdTaxDeduction/HouseholdTaxDeduction/Denied/`;

//get
export const getHouseholdTaxDeductionRowsReceived = () => `V2/Billing/HouseholdTaxDeduction/HouseholdTaxDeduction/Received/`;

//get
export const getHouseholdTaxDeductionRowInfo = (invoiceId: number, customerInvoiceRowId: number) => `V2/Billing/HouseholdTaxDeduction/HouseholdTaxRowInfo/${invoiceId}/${customerInvoiceRowId}`;

//get
export const getHouseholdTaxDeductionRowForEdit = (customerInvoiceRowId: number) => `V2/Billing/HouseholdTaxDeduction/HouseholdTaxDeductionRowForEdit/${customerInvoiceRowId}`;

//get
export const getLastUsedSequenceNumber = (entityName: string) => `V2/Billing/HouseholdTaxDeduction/HouseholdSequenceNumber/${encodeURIComponent(entityName)}`;

//post, takes args: (item: number)
export const saveHouseholdTaxDeductionRowForEdit = () => `V2/Billing/HouseholdTaxDeduction/HouseholdTaxDeductionRowForEdit/`;

//post, takes args: (model: number)
export const saveHouseholdTaxReceived = () => `V2/Billing/HouseholdTaxDeduction/HouseholdTaxDeduction/SaveReceived`;

//post, takes args: (model: number)
export const saveHouseholdTaxPartiallyApproved = () => `V2/Billing/HouseholdTaxDeduction/HouseholdTaxDeduction/SaveReceived/Partially`;

//post, takes args: (model: number)
export const saveHouseholdTaxApplied = () => `V2/Billing/HouseholdTaxDeduction/HouseholdTaxDeduction/SaveApplied`;

//post, takes args: (model: number)
export const saveHouseholdTaxWithdrawApplied = () => `V2/Billing/HouseholdTaxDeduction/HouseholdTaxDeduction/SaveWithdrawApplied`;

//post, takes args: (model: number)
export const saveHouseholdTaxDenied = () => `V2/Billing/HouseholdTaxDeduction/HouseholdTaxDeduction/SaveDenied`;

//post, takes args: (model: number)
export const getHouseholdTaxDeductionPrintUrl = () => `V2/Billing/HouseholdTaxDeduction/Print/HouseholdTaxDeduction/`;

//delete
export const deleteHouseholdTaxDeductionRow = (rowId: number) => `V2/Billing/HouseholdTaxDeduction/HouseholdTaxDeduction/Delete/${rowId}`;


