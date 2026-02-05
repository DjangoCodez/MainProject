


//Available methods for ProjectProductController

//get
export const getTimeCodes = () => `V2/Billing/ProjectProduct/TimeCode`;

//get
export const getPriceLists = (comparisonPriceListTypeId: number, priceListTypeId: number, loadAll: boolean, priceDate: string) => `V2/Billing/ProjectProduct/PriceList/${comparisonPriceListTypeId}/${priceListTypeId}/${loadAll}/${encodeURIComponent(priceDate)}`;

//get
export const getProductRows = (projectId: number, originType: number, includeChildProjects: boolean, fromDate: string, toDate: string) => `V2/Billing/ProjectProduct/ProductRows/${projectId}/${originType}/${includeChildProjects}/${encodeURIComponent(fromDate)}/${encodeURIComponent(toDate)}`;


