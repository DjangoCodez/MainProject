


//Available methods for ProductUnitConvertController

//get
export const getProductUnitConverts = (productId: number, addEmptyRow: boolean) => `V2/Billing/ProductUnitConvert/${productId}/${addEmptyRow}`;

//post, takes args: (unitConvertDTOs: number)
export const saveProductUnitConvert = () => `V2/Billing/ProductUnitConvert/SaveProductUnitConvert`;

//post, takes args: (model: number)
export const parseProductUnitConversionFile = () => `V2/Billing/ProductUnitConvert/Parse`;


