


//Available methods for AccountDimController

//get
export const getAccountDims = () => `V2/Economy/Accounting/AccountDim/`;

//get
export const getAccountDimStd = () => `V2/Economy/Accounting/AccountDimStd/`;

//get
export const getProjectAccountDim = () => `V2/Economy/Accounting/AccountDim/Project`;

//get
export const getShiftTypeAccountDim = (loadAccounts: boolean) => `V2/Economy/Accounting/AccountDim/ShiftType/${loadAccounts}`;

//get
export const getAccountDimBySieNr = (sieDimNr: number) => `V2/Economy/Accounting/AccountDim/bySieNr/${sieDimNr}`;

//get
export const getAccountDimChars = () => `V2/Economy/Accounting/AccountDim/Chars`;

//get
export const validateAccountDim = (accountDimNr: number, accountDimId: number) => `V2/Economy/Accounting/AccountDim/Validate/${accountDimNr}/${accountDimId}`;

//post, takes args: (model: number)
export const saveAccountDim = () => `V2/Economy/Accounting/AccountDim`;

//delete
export const deleteAccountDim = (accDimIds: number[]) => `V2/Economy/Accounting/AccountDim?accDimIds=${accDimIds}`;


