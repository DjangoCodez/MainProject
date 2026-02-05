


//Available methods for AccountBalanceController

//get
export const getAccountBalanceByAccount = (accountId: number, loadYear: boolean) => `V2/Economy/AccountBalance/ByAccount/${accountId}/${loadYear}`;

//post, takes args: (accountYearId: number)
export const calculateAccountBalanceForAccounts = (accountYearId: number) => `V2/Economy/AccountBalance/CalculateForAccounts/${accountYearId}`;

//post, takes args: ()
export const calculateForAccountsAllYears = () => `V2/Economy/AccountBalance/CalculateForAccountsAllYears`;

//post, takes args: (accountId: number)
export const calculateAccountBalanceForAccountInAccountYears = (accountId: number) => `V2/Economy/AccountBalance/CalculateForAccountInAccountYears/${accountId}`;

//post, takes args: (model: number)
export const calculateAccountBalanceForAccountsFromVoucher = () => `V2/Economy/AccountBalance/CalculateAccountBalanceForAccountsFromVoucher`;

//post, takes args: (accountYearId: number)
export const getAccountBalances = (accountYearId: number) => `V2/Economy/AccountBalance/GetAccountBalances/${accountYearId}`;


