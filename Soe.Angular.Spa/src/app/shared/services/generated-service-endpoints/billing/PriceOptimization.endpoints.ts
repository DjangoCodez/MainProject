


//Available methods for PriceOptimizationController

//get
export const getPriceOptimizationsForGrid = (allItemsSelection: number, status: number[], priceOptimizationId?: number) => `V2/Billing/PriceOptimization/Grid/${priceOptimizationId || ''}?allItemsSelection=${allItemsSelection}&status=${status}`;

//get
export const getPriceOptimization = (priceOptimizationId: number) => `V2/Billing/PriceOptimization/${priceOptimizationId}`;

//post, takes args: (priceOptimization: number)
export const savePriceOptimization = () => `V2/Billing/PriceOptimization`;

//post, takes args: (priceOptimizationModel: number)
export const deletePriceOptimizations = () => `V2/Billing/PriceOptimization/DeletePriceOptimizations`;

//post, takes args: (model: number)
export const changePriceOptimizationStatus = () => `V2/Billing/PriceOptimization/ChangeStatus`;

//delete
export const deletePriceOptimization = (priceOptimizationId: number) => `V2/Billing/PriceOptimization/${priceOptimizationId}`;

//get
export const getPriceOptimizationTraceRows = (priceOptimizationId: number) => `V2/Billing/PriceOptimization/TraceViews/${priceOptimizationId}`;

//get
export const getPriceOptimizationRow = (priceOptimizationId: number) => `V2/Billing/PriceOptimization/PriceOptimizationRow/${priceOptimizationId}`;

//post, takes args: (sysProductIds: number[])
export const getPriceOptimizationRowPrices = () => `V2/Billing/PriceOptimization/PriceOptimizationRow/Prices`;

//post, takes args: (model: number)
export const transferPriceOptimizationRowsToOrder = () => `V2/Billing/PriceOptimization/PriceOptimizationRow/Transfer`;


