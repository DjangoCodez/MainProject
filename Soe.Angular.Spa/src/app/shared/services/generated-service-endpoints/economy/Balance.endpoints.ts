


//Available methods for BalanceController

//get
export const getAccountYearBalance = (accountYearId: number) => `V2/Economy/Accounting/Balance/${accountYearId}`;

//get
export const getAccountYearBalanceFromPreviousYear = (accountYearId: number) => `V2/Economy/Accounting/Balance/Transfer/${accountYearId}`;

//post, takes args: (model: number)
export const saveAccountYearBalances = () => `V2/Economy/Accounting/Balance`;


