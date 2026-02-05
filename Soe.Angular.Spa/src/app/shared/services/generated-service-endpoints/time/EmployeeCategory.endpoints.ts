


//Available methods for EmployeeCategoryController

//get
export const categorySmallGenericTypes = (soeCategoryTypeId: number, addEmptyRow: boolean) => `V2/Time/EmployeeCategory/SmallGenericTypes/${soeCategoryTypeId}/${addEmptyRow}`;

//get
export const getCategoriesDict = (soeCategoryTypeId: number, categoryId: number, addEmptyRow: boolean) => `V2/Time/EmployeeCategory/GetCategoriesDict/${soeCategoryTypeId}/${categoryId}/${addEmptyRow}`;

//get
export const getCategoryGroupsDict = (soeCategoryTypeId: number, addEmptyRow: boolean) => `V2/Time/EmployeeCategory/GetCategoryGroupsDict/${soeCategoryTypeId}/${addEmptyRow}`;

//get
export const getCategoriesGrid = (soeCategoryTypeId: number, loadCompanyCategoryRecord: boolean, loadChildren: boolean, loadCategoryGroups: boolean) => `V2/Time/EmployeeCategory/Grid/${soeCategoryTypeId}/${loadCompanyCategoryRecord}/${loadChildren}/${loadCategoryGroups}`;

//get
export const getCategory = (categoryId: number) => `V2/Time/EmployeeCategory/Category/${categoryId}`;

//post, takes args: (model: number)
export const save = () => `V2/Time/EmployeeCategory`;

//delete
export const deleteCategory = (categoryId: number) => `V2/Time/EmployeeCategory/${categoryId}`;

//get
export const getCategories = (employeeId: number, categoryType: number, isAdmin: boolean, includeSecondary: boolean, addEmptyRow: boolean) => `V2/Time/EmployeeCategory/ForRoleFromType/${employeeId}/${categoryType}/${isAdmin}/${includeSecondary}/${addEmptyRow}`;

//get
export const getCategoryAccounts = (accountId: number, loadCategory: boolean) => `V2/Time/EmployeeCategory/AccountsByAccount/${accountId}/${loadCategory}`;


