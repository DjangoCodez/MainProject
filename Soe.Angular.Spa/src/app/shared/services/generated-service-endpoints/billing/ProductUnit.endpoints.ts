


//Available methods for ProductUnitController

//get
export const getProductUnits = (productUnitId?: number) => `V2/Billing/Product/ProductUnit/Grid/${productUnitId || ''}`;

//get
export const getProductUnitsDict = () => `V2/Billing/Product/ProductUnit/Dict`;

//get
export const getProductUnit = (productUnitId: number) => `V2/Billing/Product/ProductUnit/${productUnitId}`;

//post, takes args: (model: number)
export const saveProductUnit = () => `V2/Billing/Product/ProductUnit`;

//delete
export const deleteProductUnit = (productUnitId: number) => `V2/Billing/Product/ProductUnit/${productUnitId}`;


