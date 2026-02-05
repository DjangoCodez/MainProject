


//Available methods for UserV2Controller

//get
export const getSmallGenericUsers = (addEmptyRow: boolean, includeKey: boolean, useFullName: boolean, includeLoginName: boolean) => `V2/Manage/System?addEmptyRow=${addEmptyRow}&includeKey=${includeKey}&useFullName=${useFullName}&includeLoginName=${includeLoginName}`;

//get
export const getSmallDTOUsers = (setDefaultRoleName: boolean, active?: boolean, skipNonEmployeeUsers?: boolean, includeEmployeesWithSameAccountOnAttestRole?: boolean, includeEmployeeCategories?: boolean, showEnded?: boolean) => `V2/Manage/System?setDefaultRoleName=${setDefaultRoleName}&active=${active}&skipNonEmployeeUsers=${skipNonEmployeeUsers}&includeEmployeesWithSameAccountOnAttestRole=${includeEmployeesWithSameAccountOnAttestRole}&includeEmployeeCategories=${includeEmployeeCategories}&showEnded=${showEnded}`;

//get
export const getUsersByLicense = (licenseId: number, setDefaultRoleName: boolean, includeInactive: boolean, includeEnded: boolean, includeNotStarted: boolean) => `V2/Manage/System/ByLicense/${licenseId}/${setDefaultRoleName}/${includeInactive}/${includeEnded}/${includeNotStarted}`;

//get
export const getUsersByCompany = (actorCompanyId: number, setDefaultRoleName: boolean, includeInactive: boolean, includeEnded: boolean, includeNotStarted: boolean) => `V2/Manage/System/ByCompany/${actorCompanyId}/${setDefaultRoleName}/${includeInactive}/${includeEnded}/${includeNotStarted}`;

//get
export const getUsersByCompanyDate = (actorCompanyId: number, setDefaultRoleName: boolean, includeInactive: boolean, includeEnded: boolean, userCompanyRoleDate: string) => `V2/Manage/System/ByCompany/${actorCompanyId}/${setDefaultRoleName}/${includeInactive}/${includeEnded}/${encodeURIComponent(userCompanyRoleDate)}`;

//get
export const getUsersByRole = (roleId: number, setDefaultRoleName: boolean, includeInactive: boolean, includeEnded: boolean, includeNotStarted: boolean) => `V2/Manage/System/ByRole/${roleId}/${setDefaultRoleName}/${includeInactive}/${includeEnded}/${includeNotStarted}`;

//get
export const getUserLicenseInfo = () => `V2/Manage/System/GetUserLicenseInfo`;

//get
export const companiesWithSupportLogin = (selectedLicenseId: number) => `V2/Manage/System/CompaniesWithSupportLogin/${selectedLicenseId}`;

//get
export const getUser = (userId: number) => `V2/Manage/System/${userId}`;

//get
export const getCurrentUser = () => `V2/Manage/System/Current/`;

//get
export const getUserNamesWithLogin = () => `V2/Manage/System/UsernamesWithLogin/`;

//get
export const getUsersWithoutEmployees = (companyId: number, includeUserId: number, addEmptyRow: boolean) => `V2/Manage/System/UsersWithoutEmployees/${companyId}/${includeUserId}/${addEmptyRow}`;

//get
export const getUserForEdit = (userId: number, currentUserId: number) => `V2/Manage/System/UserForEdit/${userId}/${currentUserId}`;

//get
export const getAccountIdsFromHierarchyByUser = (dateFrom: string, dateTo: string, useMaxAccountDimId: boolean, includeVirtualParented: boolean, includeOnlyChildrenOneLevel: boolean, onlyDefaultAccounts: boolean, useEmployeeAccountIfNoAttestRole: boolean, includeAbstract: boolean) => `V2/Manage/System/AccountIdsFromHierarchyByUser/${encodeURIComponent(dateFrom)}/${encodeURIComponent(dateTo)}/${useMaxAccountDimId}/${includeVirtualParented}/${includeOnlyChildrenOneLevel}/${onlyDefaultAccounts}/${useEmployeeAccountIfNoAttestRole}/${includeAbstract}`;

//get
export const getAccountsFromHierarchyByUser = (dateFrom: string, dateTo: string, useMaxAccountDimId: boolean, includeVirtualParented: boolean, includeOnlyChildrenOneLevel: boolean, onlyDefaultAccounts: boolean, useEmployeeAccountIfNoAttestRole: boolean) => `V2/Manage/System/AccountsFromHierarchyByUser/${encodeURIComponent(dateFrom)}/${encodeURIComponent(dateTo)}/${useMaxAccountDimId}/${includeVirtualParented}/${includeOnlyChildrenOneLevel}/${onlyDefaultAccounts}/${useEmployeeAccountIfNoAttestRole}`;

//get
export const getAccountsFromHierarchyByUserSetting = (dateFrom: string, dateTo: string, useMaxAccountDimId: boolean, includeVirtualParented: boolean, includeOnlyChildrenOneLevel: boolean, useDefaultEmployeeAccountDimEmployee: boolean) => `V2/Manage/System/AccountsFromHierarchyByUserSetting/${encodeURIComponent(dateFrom)}/${encodeURIComponent(dateTo)}/${useMaxAccountDimId}/${includeVirtualParented}/${includeOnlyChildrenOneLevel}/${useDefaultEmployeeAccountDimEmployee}`;

//post, takes args: (model: number)
export const getDefaultRoleId = () => `V2/Manage/System/DefaultRole`;

//post, takes args: (model: number)
export const validateSaveUser = () => `V2/Manage/System/ValidateSaveUser`;

//get
export const validateInactivateUser = (userId: number) => `V2/Manage/System/ValidateInactivateUser/${userId}`;

//get
export const validateDeleteUser = (userId: number) => `V2/Manage/System/ValidateDeleteUser/${userId}`;

//get
export const validateImmediateDeleteUser = (userId: number) => `V2/Manage/System/ValidateImmediateDeleteUser/${userId}`;

//post, takes args: (model: number)
export const sendActivationEmail = () => `V2/Manage/System/SendActivationEmail`;

//post, takes args: (model: number)
export const sendForgottenUsername = () => `V2/Manage/System/SendForgottenUsername`;

//post, takes args: (model: number)
export const deleteUser = () => `V2/Manage/System/Delete`;


