


//Available methods for PayrollPriceTypeController

//get
export const getPayrollPriceTypesGrid = (payrollPriceTypeId?: number) => `V2/Time/PayrollPriceType/Grid/${payrollPriceTypeId || ''}`;

//get
export const getPayrollPriceType = (payrollPriceTypeId: number) => `V2/Time/PayrollPriceType/${payrollPriceTypeId}`;

//post, takes args: (model: number)
export const savePayrollPriceType = () => `V2/Time/PayrollPriceType`;

//delete
export const deletePayrollPriceType = (payrollPriceTypeId: number) => `V2/Time/PayrollPriceType/${payrollPriceTypeId}`;


