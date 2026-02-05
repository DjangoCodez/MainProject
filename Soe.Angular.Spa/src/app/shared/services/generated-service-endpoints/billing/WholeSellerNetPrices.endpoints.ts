//Available methods for WholeSellerNetPricesController

//get
export const getWholeSellers = (
  onlyCurrentCountry: boolean,
  onlySeparateFile: boolean
) =>
  `Billing/WholesellerNetPrices/Wholesellers/${onlyCurrentCountry}/${onlySeparateFile}`;

//get
export const getNetPrices = (sysWholeSellerId: number) =>
  `Billing/WholesellerNetPrices/${sysWholeSellerId}`;

//post, takes args: (model: number)
export const deleteNetPriceRows = () =>
  `Billing/WholesellerNetPrices/Rows/Delete`;

//post, takes args: (model: number)
export const saveNetPrices = () => `Billing/WholesellerNetPrices/Import/`;
