


//Available methods for BudgetController

//get
export const getBudgetList = (budgetType: number, actorCompanyId: number, budgetHeadId?: number) => `V2/Economy/Budget/Budget/${budgetType}/${actorCompanyId}?budgetHeadId=${budgetHeadId}`;

//get
export const getBudget = (budgetHeadId: number, loadRows: boolean) => `V2/Economy/Budget/BudgetHead/${budgetHeadId}/${loadRows}`;

//post, takes args: (dto: number)
export const saveBudgetHead = () => `V2/Economy/Budget/Budget`;

//delete
export const deleteBudget = (budgetHeadId: number) => `V2/Economy/Budget/Budget/${budgetHeadId}`;

//post, takes args: (model: number)
export const getBalanceChangePerPeriod = () => `V2/Economy/Budget/Budget/Result`;

//get
export const getBalanceChangeResult = (key: string) => `V2/Economy/Budget/Budget/Result/${encodeURIComponent(key)}`;


