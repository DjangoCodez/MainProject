


//Available methods for OrderIntrastatController

//get
export const getIntrastatTransactions = (originId: number) => `V2/Billing/Order/Intrastat/Transactions/${originId}`;

//get
export const getIntrastatTransactionsForExport = (intrastatReportingType: number, fromDate: string, toDate: string) => `V2/Billing/Order/Intrastat/Transactions/ForExport/${intrastatReportingType}/${encodeURIComponent(fromDate)}/${encodeURIComponent(toDate)}`;

//post, takes args: (model: number)
export const saveIntrastatTransactions = () => `V2/Billing/Order/Intrastat/Transactions/`;

//post, takes args: (selection: number)
export const createIntrastatExport = () => `V2/Billing/Order/Intrastat/Transactions/Export/`;


