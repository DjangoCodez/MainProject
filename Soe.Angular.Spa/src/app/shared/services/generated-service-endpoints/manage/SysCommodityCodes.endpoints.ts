


//Available methods for SysCommodityCodesController

//get
export const getCustomerCommodyCodes = (onlyActive: boolean) => `V2/Manage/System/CommodityCodes/${onlyActive}`;

//get
export const getCustomerCommodyCodesDict = (addEmpty: boolean) => `V2/Manage/System/CommodityCodes/Dict/${addEmpty}`;

//post, takes args: (model: number)
export const saveCustomerCommodityCodes = () => `V2/Manage/System/CommodityCodes/`;

//get
export const getCommodyCodes = (langId: number) => `V2/Manage/System/CommodityCodes/${langId}`;

//post, takes args: (model: number)
export const uploadCommodityCodesFile = () => `V2/Manage/System/Files/Intrastat/CommodityCodes`;


