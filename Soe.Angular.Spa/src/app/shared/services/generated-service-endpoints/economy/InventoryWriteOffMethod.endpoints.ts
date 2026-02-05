


//Available methods for InventoryWriteOffMethodController

//get
export const getInventoryWriteOffMethodsGrid = (writeOffMethodId?: number) => `V2/Economy/Inventory/InventoryWriteOffMethod/Grid/${writeOffMethodId || ''}`;

//get
export const getInventoryWriteOffMethodsDict = (addEmptyValue: boolean) => `V2/Economy/Inventory/InventoryWriteOffMethod/Dict?addEmptyValue=${addEmptyValue}`;

//get
export const getInventoryWriteOffMethod = (inventoryWriteOffMethodId: number) => `V2/Economy/Inventory/InventoryWriteOffMethod/${inventoryWriteOffMethodId}`;

//post, takes args: (inventoryWriteOffMethodDTO: number)
export const saveInventoryWriteOffMethod = () => `V2/Economy/Inventory/InventoryWriteOffMethod`;

//delete
export const deleteInventoryWriteOffMethod = (inventoryWriteOffMethodId: number) => `V2/Economy/Inventory/InventoryWriteOffMethod/${inventoryWriteOffMethodId}`;


