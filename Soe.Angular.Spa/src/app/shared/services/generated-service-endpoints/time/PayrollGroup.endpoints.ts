


//Available methods for PayrollGroupController

//get
export const getPayrollGroupsGrid = () => `V2/Time/Payroll/PayrollGroup/Grid`;

//get
export const getPayrollGroupsSmall = (addEmptyRow: boolean) => `V2/Time/Payroll/PayrollGroup/SmallDict/${addEmptyRow}`;

//get
export const getPayrollGroupsDict = (addEmptyRow: boolean) => `V2/Time/Payroll/PayrollGroup/Dict/${addEmptyRow}`;

//get
export const getPayrollGroups = () => `V2/Time/Payroll/PayrollGroup`;

//get
export const getPayrollGroup = (payrollGroupId: number, includePriceTypes: boolean, includePriceFormulas: boolean, includeSettings: boolean, includePayrollGroupReports: boolean, includeTimePeriod: boolean, includeAccounts: boolean, includePayrollGroupVacationGroup: boolean, includePayrollGroupPayrollProduct: boolean) => `V2/Time/Payroll/PayrollGroup/${payrollGroupId}/${includePriceTypes}/${includePriceFormulas}/${includeSettings}/${includePayrollGroupReports}/${includeTimePeriod}/${includeAccounts}/${includePayrollGroupVacationGroup}/${includePayrollGroupPayrollProduct}`;

//get
export const getCompanyPayrollGroupReports = (checkRolePermission: boolean) => `V2/Time/Payroll/PayrollGroup/Reports/${checkRolePermission}`;

//get
export const priceTypesExistsInPayrollGroup = (payrollGroupId: number, priceTypeIds: string) => `V2/Time/Payroll/PayrollGroup/PriceTypesExists/${payrollGroupId}/${encodeURIComponent(priceTypeIds)}`;

//post, takes args: (payrollGroup: number)
export const savePayrollGroup = () => `V2/Time/Payroll/PayrollGroup`;

//delete
export const deletePayrollGroup = (payrollGroupId: number) => `V2/Time/Payroll/PayrollGroup/${payrollGroupId}`;


