


//Available methods for AccountProvisionController

//get
export const getAccountProvisionBaseColumns = (timePeriodId: number) => `V2/Time/Payroll/AccountProvision/AccountProvisionBase/Columns/${timePeriodId}`;

//get
export const getAccountProvisionBase = (timePeriodId: number) => `V2/Time/Payroll/AccountProvision/AccountProvisionBase/${timePeriodId}`;

//get
export const lockAccountProvisionBase = (timePeriodId: number) => `V2/Time/Payroll/AccountProvision/AccountProvisionBase/Lock/${timePeriodId}`;

//get
export const unLockAccountProvisionBase = (timePeriodId: number) => `V2/Time/Payroll/AccountProvision/AccountProvisionBase/Unlock/${timePeriodId}`;

//post, takes args: (provisions: number)
export const saveAccountProvisionBase = () => `V2/Time/Payroll/AccountProvision/AccountProvisionBase`;

//get
export const getAccountProvisionTransactions = (timePeriodId: number) => `V2/Time/Payroll/AccountProvision/AccountProvisionTransaction/${timePeriodId}`;

//post, takes args: (model: number)
export const updateAccountProvisionTransactions = () => `V2/Time/Payroll/AccountProvision/AccountProvisionTransaction/Update`;

//post, takes args: (model: number)
export const saveAttestForAccountProvision = () => `V2/Time/Payroll/AccountProvision/AccountProvisionTransaction/Attest`;


