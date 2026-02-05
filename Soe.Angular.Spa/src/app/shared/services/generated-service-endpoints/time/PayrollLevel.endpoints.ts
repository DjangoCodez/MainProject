


//Available methods for PayrollLevelController

//get
export const getPayrollLevelsGrid = (payrollLevelId?: number) => `V2/Time/PayrollLevel/Grid/${payrollLevelId || ''}`;

//get
export const getPayrollLevel = (payrollLevelId: number) => `V2/Time/PayrollLevel/${payrollLevelId}`;

//post, takes args: (model: number)
export const savePayrollLevel = () => `V2/Time/PayrollLevel`;

//delete
export const deletePayrollLevel = (payrollLevelId: number) => `V2/Time/PayrollLevel/${payrollLevelId}`;


