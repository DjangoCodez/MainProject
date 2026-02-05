


//Available methods for AccountYearController

//get
export const getCurrentAccountYear = () => `V2/Economy/AccountYear/Current`;

//get
export const getSelectedAccountYear = () => `V2/Economy/AccountYear/Selected`;

//get
export const getAccountYears = (addEmptyRow: boolean, excludeNew: boolean) => `V2/Economy/AccountYear/Dict/${addEmptyRow}/${excludeNew}`;

//get
export const getAccountYearIdByDate = (date: string) => `V2/Economy/AccountYear/Id/${encodeURIComponent(date)}`;

//get
export const getAccountYearId = (id: number, loadPeriods: boolean) => `V2/Economy/AccountYear/${id}/${loadPeriods}`;

//get
export const getAccountYear = (date: string) => `V2/Economy/AccountYear/${encodeURIComponent(date)}`;

//get
export const getAllAccountYears = (getPeriods: boolean, excludeNew: boolean) => `V2/Economy/AccountYear/All/${getPeriods}/${excludeNew}`;

//post, takes args: (model: number)
export const saveAccountYear = () => `V2/Economy/AccountYear`;

//post, takes args: (accountYearId: number)
export const copyVoucherTemplatesFromPreviousAccountYear = (accountYearId: number) => `V2/Economy/AccountYear/CopyVoucherTemplates/${accountYearId}`;

//post, takes args: (accountYearId: number)
export const copyGrossProfitCodes = (accountYearId: number) => `V2/Economy/AccountYear/CopyGrossProfitCodes/${accountYearId}`;

//delete
export const deleteAccountYear = (accountYearId: number) => `V2/Economy/AccountYear/${accountYearId}`;


