


//Available methods for InventoryV2Controller

//get
export const getInventories = (statuses: string, inventoryId?: number) => `V2/Economy/Inventory/Inventories?statuses=${encodeURIComponent(statuses)}&inventoryId=${inventoryId}`;

//get
export const getInventoriesDict = () => `V2/Economy/Inventory/Inventories/Dict`;

//get
export const getInventory = (inventoryId: number) => `V2/Economy/Inventory/${inventoryId}`;

//get
export const getNextInventoryNr = () => `V2/Economy/Inventory/NextInventoryNr`;

//post, takes args: (model: number)
export const saveInventory = () => `V2/Economy/Inventory`;

//post, takes args: (model: number)
export const saveAdjustment = () => `V2/Economy/Inventory/Adjustment`;

//post, takes args: (model: number)
export const saveNotesAndDescription = () => `V2/Economy/Inventory/NotesAndDescription`;

//get
export const getInventoryTraceViews = (inventoryId: number) => `V2/Economy/Inventory/InventoryTraceViews/${inventoryId}`;

//delete
export const deleteInventory = (inventoryId: number) => `V2/Economy/Inventory/Inventory/${inventoryId}`;


