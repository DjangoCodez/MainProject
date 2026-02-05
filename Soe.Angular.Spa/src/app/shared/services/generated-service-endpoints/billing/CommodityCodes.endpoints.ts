


//Available methods for CommodityCodesController

//get
export const getCustomerCommodyCodes = (onlyActive: boolean) => `V2/Billing/CommodityCodes/${onlyActive}`;

//get
export const getCustomerCommodyCodesDict = (addEmpty: boolean) => `V2/Billing/CommodityCodes/Dict/${addEmpty}`;

//post, takes args: (model: number)
export const saveCustomerCommodityCodes = () => `V2/Billing/CommodityCodes`;


