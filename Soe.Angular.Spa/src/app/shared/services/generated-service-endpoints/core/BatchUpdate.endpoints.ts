


//Available methods for BatchUpdateController

//get
export const getBatchUpdateForEntity = (entityType: number) => `V2/Core/Category/BatchUpdate/GetBatchUpdateForEntity/${entityType}`;

//post, takes args: (model: number)
export const refreshBatchUpdateOptions = () => `V2/Core/Category/BatchUpdate/RefreshBatchUpdateOptions`;

//get
export const getContactAddressItemsDict = (entityType: number) => `V2/Core/Category/BatchUpdate/FilterOptions/${entityType}`;

//post, takes args: (model: number)
export const performBatchUpdate = () => `V2/Core/Category/BatchUpdate/PerformBatchUpdate`;


