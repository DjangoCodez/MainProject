


//Available methods for TimeWorkAccountController

//get
export const getTimeWorkAccountsGrid = (timeWorkAccountId?: number) => `V2/Time/TimeWorkAccount/Grid/${timeWorkAccountId || ''}`;

//get
export const getTimeWorkAccount = (timeWorkAccountId: number, includeYears: boolean) => `V2/Time/TimeWorkAccount/${timeWorkAccountId}/${includeYears}`;

//post, takes args: (model: number)
export const saveTimeWorkAccount = () => `V2/Time/TimeWorkAccount`;

//delete
export const deleteTimeWorkAccount = (timeWorkAccountId: number) => `V2/Time/TimeWorkAccount/${timeWorkAccountId}`;

//get
export const getTimeWorkAccountYear = (timeWorkAccountYearId: number, timeWorkAccountId: number, includeEmployees: boolean, loadWorkTimeWeek: boolean) => `V2/Time/TimeWorkAccount/TimeWorkAccountYear/${timeWorkAccountYearId}/${timeWorkAccountId}/${includeEmployees}/${loadWorkTimeWeek}`;

//get
export const getTimeWorkAccountLastYear = (timeWorkAccountId: number, addYear: boolean) => `V2/Time/TimeWorkAccount/TimeWorkAccountLastYear/${timeWorkAccountId}/${addYear}`;

//post, takes args: (model: number)
export const saveTimeWorkAccountYear = () => `V2/Time/TimeWorkAccount/TimeWorkAccountYear`;

//delete
export const deleteTimeWorkAccountYear = (timeWorkAccountYearId: number, timeWorkAccountId: number) => `V2/Time/TimeWorkAccount/TimeWorkAccountYear/${timeWorkAccountYearId}/${timeWorkAccountId}`;

//post, takes args: (model: number)
export const calculateYearEmployee = () => `V2/Time/TimeWorkAccount/TimeWorkAccountYear/CalculateYearEmployee`;

//post, takes args: (model: number)
export const sendSelection = () => `V2/Time/TimeWorkAccount/TimeWorkAccountYear/SendSelection`;

//get
export const getCalculationBasis = (timeWorkAccountYearEmployeeId: number, employeeId: number) => `V2/Time/TimeWorkAccount/TimeWorkAccountYear/CalculationBasis/${timeWorkAccountYearEmployeeId}/${employeeId}`;

//post, takes args: (model: number)
export const getPensionExport = () => `V2/Time/TimeWorkAccount/TimeWorkAccountYear/GetPensionExport`;

//post, takes args: (model: number)
export const generateOutcome = () => `V2/Time/TimeWorkAccount/TimeWorkAccountYear/GenerateOutcome`;

//post, takes args: (model: number)
export const generateUnPaidBalance = () => `V2/Time/TimeWorkAccount/TimeWorkAccountYear/GenerateUnusedPaidBalance`;

//get
export const getPayrollTimePeriods = () => `V2/Time/TimeWorkAccount/TimeWorkAccountYear/TimePeriods`;

//get
export const getPayrollProductIdsByType = (level1: number, level2: number) => `V2/Time/TimeWorkAccount/TimeWorkAccountYear/GetPayrollProductIdsByType/${level1}/${level2}`;

//get
export const getPayrollProductsSmall = () => `V2/Time/TimeWorkAccount/TimeWorkAccountYear/GetPayrollProductsSmall`;

//get
export const getPaymentDate = (timeWorkAccountYearId: number, timeWorkAccountYearEmployeeId: number) => `V2/Time/TimeWorkAccount/TimeWorkAccountYear/GetPaymentDate/${timeWorkAccountYearId}/${timeWorkAccountYearEmployeeId}/`;

//get
export const getTimeAccumulators = () => `V2/Time/TimeWorkAccount/TimeWorkAccountYear/GetTimeAccumulators`;

//post, takes args: (model: number)
export const reverseTransaction = () => `V2/Time/TimeWorkAccount/TimeWorkAccountYear/ReverseTransaction`;

//post, takes args: (model: number)
export const reversePaidBalance = () => `V2/Time/TimeWorkAccount/TimeWorkAccountYear/ReversePaidBalance`;

//delete
export const deleteTimeWorkAccountYearEmployeeRow = (timeWorkAccountYearId: number, timeWorkAccountYearEmployeeId: number, employeeId: number) => `V2/Time/TimeWorkAccount/TimeWorkAccountYear/DeleteTimeWorkAccountYearEmployeeRow/${timeWorkAccountYearId}/${timeWorkAccountYearEmployeeId}/${employeeId}`;


