


//Available methods for AccountController

//get
export const getStdAccounts = () => `V2/Economy/Account/GetStdAccounts`;

//get
export const getAccountStdsDict = (addEmptyRow: boolean) => `V2/Economy/Account/AccountStd/${addEmptyRow}`;

//get
export const getAccountStdsNameNumber = (addEmptyRow: boolean, accountTypeId?: number) => `V2/Economy/Account/AccountStdNumberName/${addEmptyRow}/${accountTypeId || ''}`;

//get
export const getAccountsFromHierarchyByUserSetting = (dateFrom: string, dateTo: string, useMaxAccountDimId: boolean, includeVirtualParented: boolean, includeOnlyChildrenOneLevel: boolean, useDefaultEmployeeAccountDimEmployee: boolean) => `V2/Economy/Account/AccountsFromHierarchyByUserSetting/${encodeURIComponent(dateFrom)}/${encodeURIComponent(dateTo)}/${useMaxAccountDimId}/${includeVirtualParented}/${includeOnlyChildrenOneLevel}/${useDefaultEmployeeAccountDimEmployee}`;

//get
export const getAccountsInternalsByCompany = (loadAccount: boolean, loadAccountDim: boolean, loadAccountMapping: boolean) => `V2/Economy/Account/AccountsInternals/${loadAccount}/${loadAccountDim}/${loadAccountMapping}`;

//get
export const getAccountDimInternals = (active?: boolean) => `V2/Economy/Account/GetAccountDimInternals/${active || ''}`;

//get
export const getAccountDimGrid = (onlyStandard: boolean, onlyInternal: boolean, accountDimId?: number) => `V2/Economy/Account/AccountDim/Grid/${onlyStandard}/${onlyInternal}/${accountDimId || ''}`;

//get
export const accountDimByAccountDimId = (accountDimId: number, loadInactiveDims: boolean) => `V2/Economy/Account/AccountDim/${accountDimId}/${loadInactiveDims}`;

//get
export const accountDimByAccountDimIdSmall = (accountDimId: number, onlyStandard: boolean, onlyInternal: boolean, loadAccounts: boolean, loadInternalAccounts: boolean, loadParent: boolean, loadInactives: boolean, loadInactiveDims: boolean, includeParentAccounts: boolean) => `V2/Economy/Account/AccountDimByAccountDimIdSmall/${accountDimId}/${onlyStandard}/${onlyInternal}/${loadAccounts}/${loadInternalAccounts}/${loadParent}/${loadInactives}/${loadInactiveDims}/${includeParentAccounts}`;

//get
export const accountDim = (onlyStandard: boolean, onlyInternal: boolean, loadAccounts: boolean, loadInternalAccounts: boolean, loadParent: boolean, loadInactives: boolean, loadInactiveDims: boolean, includeParentAccounts: boolean) => `V2/Economy/Account/AccountDim/${onlyStandard}/${onlyInternal}/${loadAccounts}/${loadInternalAccounts}/${loadParent}/${loadInactives}/${loadInactiveDims}/${includeParentAccounts}`;

//get
export const getAccountDimsSmall = (onlyStandard: boolean, onlyInternal: boolean, loadAccounts: boolean, loadInternalAccounts: boolean, loadParent: boolean, loadInactives: boolean, loadInactiveDims: boolean, includeParentAccounts: boolean, ignoreHierarchyOnly: boolean, actorCompanyId: number, includeOrphanAccounts: boolean) => `V2/Economy/Account/GetAccountDimsSmall/${onlyStandard}/${onlyInternal}/${loadAccounts}/${loadInternalAccounts}/${loadParent}/${loadInactives}/${loadInactiveDims}/${includeParentAccounts}/${ignoreHierarchyOnly}/${actorCompanyId}/${includeOrphanAccounts}`;

//get
export const getShiftTypeAccountDim = (loadAccounts: boolean, useCache: boolean) => `V2/Economy/Account/AccountDim/ShiftType/${loadAccounts}/${useCache}`;

//get
export const getAccountDimChars = () => `V2/Economy/Account/AccountDim/Chars`;

//post, takes args: (model: number)
export const saveAccountDim = () => `V2/Economy/Account/AccountDim`;

//delete
export const deleteAccountDim = (accDimIds: number[]) => `V2/Economy/Account/AccountDim?accDimIds=${accDimIds}`;

//get
export const getProjectAccountDim = () => `V2/Economy/Account/AccountDim/Project`;

//get
export const getAccountYears = (addEmptyRow: boolean, excludeNew: boolean) => `V2/Economy/Account/AccountYearDict/${addEmptyRow}/${excludeNew}`;

//get
export const getAccount = (accountId: number) => `V2/Economy/Account/Account/ById/${accountId}`;

//get
export const getAccountsGrid = (accountDimId: number, accountYearId: number, setLinkedToShiftType: boolean, getCategories: boolean, setParent: boolean, ignoreHierarchyOnly: boolean, accountId?: number) => `V2/Economy/Account/GetAccountsGrid/${accountDimId}/${accountYearId}/${setLinkedToShiftType}/${getCategories}/${setParent}/${ignoreHierarchyOnly}/${accountId || ''}`;

//get
export const getAccountsDict = (accountDimId: number, addEmptyRow: boolean) => `V2/Economy/Account/AccountDict/${accountDimId}/${addEmptyRow}`;

//get
export const getAccountsSmall = (accountDimId: number, accountYearId: number) => `V2/Economy/Account/GetAccountsSmall/${accountDimId}/${accountYearId}`;

//post, takes args: (model: number)
export const saveAccount = () => `V2/Economy/Account/Account`;

//post, takes args: (model: number)
export const saveAccountSmall = () => `V2/Economy/Account/Account/Small`;

//get
export const getChildrenAccounts = (parentAccountId: number) => `V2/Economy/Account/AccountChildren/${parentAccountId}`;

//post, takes args: (model: number)
export const updateAccountsState = () => `V2/Economy/Account/Account/UpdateState`;

//delete
export const deleteAccount = (accountId: number) => `V2/Economy/Account/Account/${accountId}`;

//get
export const validateAccount = (accountNr: string, accountId: number, accountDimId: number) => `V2/Economy/Account/Account/Validate/${encodeURIComponent(accountNr)}/${accountId}/${accountDimId}`;

//get
export const getAccountMappings = (accountId: number) => `V2/Economy/Account/AccountMapping/${accountId}`;

//get
export const getSysAccountStdTypes = () => `V2/Economy/Account/SysAccountStdType/`;

//get
export const getSysVatAccounts = (sysCountryId: number, addEmptyRow: boolean) => `V2/Economy/Account/SysVatAccount/${sysCountryId}/${addEmptyRow}`;

//get
export const getSysVatRate = (sysVatAccountId: number) => `V2/Economy/Account/SysVatRate/${sysVatAccountId}`;

//get
export const getSysAccountSruCodes = (addEmptyRow: boolean) => `V2/Economy/Account/SysAccountSruCode/${addEmptyRow}`;


