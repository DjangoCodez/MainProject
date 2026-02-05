


//Available methods for CategoryController

//get
export const getCategoryTypesByPermission = () => `V2/Core/Category`;

//get
export const getCategoryGrid = (soeCategoryTypeId: number, categoryId?: number) => `V2/Core/Category/Grid/${soeCategoryTypeId}/${categoryId || ''}`;

//get
export const getCategoriesGrid = (soeCategoryTypeId: number, loadCompanyCategoryRecord: boolean, loadChildren: boolean, loadCategoryGroups: boolean) => `V2/Core/Category?soeCategoryTypeId=${soeCategoryTypeId}&loadCompanyCategoryRecord=${loadCompanyCategoryRecord}&loadChildren=${loadChildren}&loadCategoryGroups=${loadCategoryGroups}`;

//get
export const getCategoriesDict = (categoryType: number, addEmptyRow: boolean, excludeCategoryId?: number) => `V2/Core/Category/Dict?categoryType=${categoryType}&addEmptyRow=${addEmptyRow}&excludeCategoryId=${excludeCategoryId}`;

//get
export const getCategories = (employeeId: number, categoryType: number, isAdmin: boolean, includeSecondary: boolean, addEmptyRow: boolean) => `V2/Core/Category/ForRoleFromType/${employeeId}/${categoryType}/${isAdmin}/${includeSecondary}/${addEmptyRow}`;

//get
export const getCompCategoryRecords = (soeCategoryTypeId: number, categoryRecordEntity: number, recordId: number) => `V2/Core/Category/Category/CompCategoryRecords/${soeCategoryTypeId}/${categoryRecordEntity}/${recordId}`;

//get
export const getCategoryAccounts = (accountId: number, loadCategory: boolean) => `V2/Core/Category/AccountsByAccount/${accountId}/${loadCategory}`;

//get
export const getCategory = (categoryId: number) => `V2/Core/Category/${categoryId}`;

//post, takes args: (model: number)
export const saveCategory = () => `V2/Core/Category`;

//delete
export const deleteCategory = (categoryId: number) => `V2/Core/Category/${categoryId}`;


