


//Available methods for TimeWorkReductionController

//get
export const getTimeAccumulatorsForReductionDict = () => `V2/Time/TimeWorkReduction/TimeAccumulatorsForReductionDict`;

//get
export const getTimeWorkReductionsGrid = (timeWorkReductionReconciliationId?: number) => `V2/Time/TimeWorkReduction/Reconciliation/Grid/${timeWorkReductionReconciliationId || ''}`;

//get
export const getTimeWorkReduction = (timeWorkReductionReconciliationId: number) => `V2/Time/TimeWorkReduction/Reconciliation/${timeWorkReductionReconciliationId}`;

//post, takes args: (timeWorkReductionReconciliation: number)
export const saveTimeWorkReductionReconciliation = () => `V2/Time/TimeWorkReduction/Reconciliation/`;

//delete
export const deleteTimeWorkReductionReconciliation = (timeWorkReductionReconciliationId: number) => `V2/Time/TimeWorkReduction/Reconciliation/${timeWorkReductionReconciliationId}`;

//post, takes args: (timeWorkReductionReconciliationYear: number)
export const saveTimeWorkReductionReconciliationYear = () => `V2/Time/TimeWorkReduction/Reconciliation/Year/`;

//delete
export const deleteTimeWorkReductionReconciliationYear = (timeWorkReductionReconciliationYearId: number) => `V2/Time/TimeWorkReduction/Reconciliation/Year/?timeWorkReductionReconciliationYearId=${timeWorkReductionReconciliationYearId}`;

//get
export const getTimeWorkReductionReconciliationEmployee = (timeWorkReductionReconciliationYearId: number) => `V2/Time/TimeWorkReduction/Reconciliation/Year/Employee/?timeWorkReductionReconciliationYearId=${timeWorkReductionReconciliationYearId}`;

//post, takes args: (model: number)
export const calculateYearEmployee = () => `V2/Time/TimeWorkReduction/Reconciliation/Year/Employee/Calculate`;

//post, takes args: (model: number)
export const generateOutcome = () => `V2/Time/TimeWorkReduction/Reconciliation/Year/Employee/GenerateOutcome`;

//post, takes args: (model: number)
export const reverseTransactions = () => `V2/Time/TimeWorkReduction/Reconciliation/Year/Employee/ReverseTransactions`;

//post, takes args: (model: number)
export const getPensionExport = () => `V2/Time/TimeWorkReduction/Reconciliation/Year/Employee/GetPensionExport`;


