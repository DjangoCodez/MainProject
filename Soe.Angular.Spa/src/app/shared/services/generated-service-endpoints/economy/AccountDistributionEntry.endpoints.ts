


//Available methods for AccountDistributionEntryController

//get
export const getAccountDistributionEntries = (periodDate: string, accountDistributionType: number, onlyActive: boolean) => `V2/Economy/Accounting/AccountDistributionEntries/${encodeURIComponent(periodDate)}/${accountDistributionType}/${onlyActive}`;

//get
export const getAccountDistributionEntriesForHead = (accountDistributionHeadId: number) => `V2/Economy/Accounting/AccountDistributionEntriesForHead/${accountDistributionHeadId}`;

//get
export const getAccountDistributionEntriesForSource = (accountDistributionHeadId: number, registrationType: number, sourceId: number) => `V2/Economy/Accounting/AccountDistributionEntriesForSource/${accountDistributionHeadId}/${registrationType}/${sourceId}`;

//post, takes args: (model: number)
export const transferToAccountDistributionEntry = () => `V2/Economy/Accounting/AccountDistributionEntries/TransferToAccountDistributionEntry`;

//post, takes args: (model: number)
export const transferAccountDistributionEntryToVoucher = () => `V2/Economy/Accounting/AccountDistributionEntries/TransferAccountDistributionEntryToVoucher`;

//post, takes args: (model: number)
export const reverseAccountDistributionEntries = () => `V2/Economy/Accounting/AccountDistributionEntries/Reverse`;

//post, takes args: (model: number)
export const restoreAccountDistributionEntries = () => `V2/Economy/Accounting/AccountDistributionEntries/RestoreAccountDistributionEntries`;

//post, takes args: (model: number)
export const deleteAccountDistributionEntries = () => `V2/Economy/Accounting/AccountDistributionEntries/DeleteAccountDistributionEntries`;

//post, takes args: (model: number)
export const deleteAccountDistributionEntriesPermanently = () => `V2/Economy/Accounting/AccountDistributionEntries/DeleteAccountDistributionEntriesPermanently`;

//post, takes args: (accountDistributionHeadId: number, registrationType: number, sourceId: number)
export const deleteAccountDistributionEntriesForSource = (accountDistributionHeadId: number, registrationType: number, sourceId: number) => `V2/Economy/Accounting/AccountDistributionEntries/DeleteAccountDistributionEntriesForSource/${accountDistributionHeadId}/${registrationType}/${sourceId}`;


