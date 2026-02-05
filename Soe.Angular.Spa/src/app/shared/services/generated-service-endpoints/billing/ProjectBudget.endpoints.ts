


//Available methods for ProjectBudgetController

//get
export const getBudget = (budgetHeadId: number, loadRows: boolean) => `V2/Billing/Project/BudgetHead/${budgetHeadId}/${loadRows}`;

//get
export const createForecast = (budgetHeadId: number) => `V2/Billing/Project/BudgetHead/Forecast/${budgetHeadId}`;

//get
export const updateBudgetForecastResult = (budgetHeadId: number) => `V2/Billing/Project/BudgetHead/Forecast/Update/${budgetHeadId}`;

//get
export const getProjectBudgetChangeLogPerRow = (budgetRowId: number) => `V2/Billing/Project/BudgetHead/Rows/Log/${budgetRowId}`;

//post, takes args: (dto: number)
export const saveBudgetHead = () => `V2/Billing/Project/Budget`;

//post, takes args: (budgetHeadId: number)
export const migrateProjectBudgetHead = (budgetHeadId: number) => `V2/Billing/Project/Budget/Migrate?budgetHeadId=${budgetHeadId}`;

//delete
export const deleteBudget = (budgetHeadId: number) => `V2/Billing/Project/Budget/${budgetHeadId}`;

//post, takes args: (model: number)
export const getBalanceChangePerPeriod = () => `V2/Billing/Project/Budget/Result`;

//get
export const getBalanceChangeResult = (key: string) => `V2/Billing/Project/Budget/Result/${encodeURIComponent(key)}`;


