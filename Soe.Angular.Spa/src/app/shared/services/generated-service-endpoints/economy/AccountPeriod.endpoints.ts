


//Available methods for AccountPeriodController

//get
export const getAccountPeriodDict = (accountYearId: number, addEmptyRow: boolean) => `V2/Economy/AccountPeriod/AccountPeriod/${accountYearId}/${addEmptyRow}`;

//get
export const getAccountPeriods = (accountYearId: number) => `V2/Economy/AccountPeriod/AccountPeriods/${accountYearId}`;

//get
export const getAccountPeriod = (accountYearId: number, date: string, includeAccountYear: boolean) => `V2/Economy/AccountPeriod/AccountPeriod/${accountYearId}/${encodeURIComponent(date)}/${includeAccountYear}`;

//get
export const getAccountPeriodId = (accountYearId: number, date: string) => `V2/Economy/AccountPeriod/AccountPeriod/Id/${accountYearId}/${encodeURIComponent(date)}`;

//post, takes args: (accountPeriodId: number, status: number)
export const updateAccountPeriodStatus = (accountPeriodId: number, status: number) => `V2/Economy/AccountPeriod/AccountPeriod/UpdateStatus/${accountPeriodId}/${status}`;


