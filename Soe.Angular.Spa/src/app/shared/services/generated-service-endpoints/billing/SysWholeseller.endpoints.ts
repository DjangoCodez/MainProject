


//Available methods for SysWholesellerController

//get
export const getSysWholeseller = (sysWholesellerId: number, loadSysWholesellerEdi: boolean, loadSysEdiMsg: boolean, loadSysEdiType: boolean) => `V2/Billing/SysWholeseller/SysWholeseller/${sysWholesellerId}/${loadSysWholesellerEdi}/${loadSysEdiMsg}/${loadSysEdiType}`;

//get
export const getSmallGenericSysWholesellers = (addEmptyRow: boolean) => `V2/Billing/SysWholeseller/SysWholesellers/Small/?addEmptyRow=${addEmptyRow}`;

//get
export const getSmallGenericSysWholesellersAll = () => `V2/Billing/SysWholeseller/SysWholesellers/Small/All`;

//get
export const getSysWholesellers = () => `V2/Billing/SysWholeseller/SysWholesellers/`;

//get
export const getSysWholesellersByCompany = (onlyNotUsed: boolean, addEmptyRow: boolean) => `V2/Billing/SysWholeseller/SysWholesellersByCompany/${onlyNotUsed}/${addEmptyRow}`;


