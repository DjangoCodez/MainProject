


//Available methods for StockInventoryController

//get
export const getStockInventories = (includeCompleted: boolean, stockInventoryId?: number) => `V2/Billing/Stock/StockInventories/${includeCompleted}/${stockInventoryId || ''}`;

//get
export const getStockInventory = (stockInventoryHeadId: number) => `V2/Billing/Stock/StockInventory/${stockInventoryHeadId}`;

//get
export const getStockInventoryRows = (stockInventoryHeadId: number) => `V2/Billing/Stock/StockInventoryRows/${stockInventoryHeadId}`;

//post, takes args: (filter: number)
export const generateStockInventoryRows = () => `V2/Billing/Stock/GenerateRows`;

//post, takes args: (model: number)
export const saveStockInventoryRows = () => `V2/Billing/Stock/SaveInventory`;

//get
export const closeStockInventory = (stockInventoryHeadId: number) => `V2/Billing/Stock/CloseInventory/${stockInventoryHeadId}`;

//delete
export const deleteStockInventory = (stockInventoryHeadId: number) => `V2/Billing/Stock/StockInventory/${stockInventoryHeadId}`;

//post, takes args: (model: number)
export const importStockInventory = () => `V2/Billing/Stock/ImportStockInventory`;


