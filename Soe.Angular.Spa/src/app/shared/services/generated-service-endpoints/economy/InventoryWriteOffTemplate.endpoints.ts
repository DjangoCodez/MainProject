


//Available methods for InventoryWriteOffTemplateController

//get
export const getInventoryWriteOffTemplatesDict = (addEmptyRow: boolean) => `V2/Economy/Inventory/InventoryWriteOffTemplate/Dict/${addEmptyRow}`;

//get
export const getInventoryWriteOffTemplateGrid = (inventoryWriteOffTemplateId?: number) => `V2/Economy/Inventory/InventoryWriteOffTemplate/Grid/${inventoryWriteOffTemplateId || ''}`;

//get
export const getInventoryWriteOffTemplate = (inventoryWriteOffTemplateId: number) => `V2/Economy/Inventory/InventoryWriteOffTemplate/${inventoryWriteOffTemplateId}`;

//get
export const getInventoryWriteOffTemplates = () => `V2/Economy/Inventory/InventoryWriteOffTemplate`;

//post, takes args: (model: number)
export const saveInventoryWriteOffTemplate = () => `V2/Economy/Inventory/InventoryWriteOffTemplate`;

//delete
export const deleteInventoryWriteOffTemplate = (inventoryWriteOffTemplateId: number) => `V2/Economy/Inventory/InventoryWriteOffTemplate/${inventoryWriteOffTemplateId}`;


