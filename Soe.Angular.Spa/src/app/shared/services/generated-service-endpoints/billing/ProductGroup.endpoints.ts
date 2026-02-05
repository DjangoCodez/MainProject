


//Available methods for ProductGroupController

//get
export const getProductGroupsGrid = (productGroupId?: number) => `V2/Billing/ProductGroups/Grid/${productGroupId || ''}`;

//get
export const getProductGroups = () => `V2/Billing/ProductGroups`;

//get
export const getProductGroup = (productGroupId: number) => `V2/Billing/ProductGroups/${productGroupId}`;

//post, takes args: (productGroupDTO: number)
export const saveProductGroup = () => `V2/Billing/ProductGroups/ProductGroup`;

//delete
export const deleteProductGroup = (productGroupId: number) => `V2/Billing/ProductGroups/${productGroupId}`;


