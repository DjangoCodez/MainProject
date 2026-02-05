


//Available methods for RequestReportController

//post, takes args: (model: number)
export const printProjectReport = () => `V2/RequestReport/Print/Project`;

//get
export const printVoucher = (voucherHeadId: number, queue: boolean) => `V2/RequestReport/Print/Voucher/${voucherHeadId}?queue=${queue}`;

//post, takes args: (model: number)
export const printVoucherList = () => `V2/RequestReport/Print/VoucherList`;

//get
export const printAccount = (accountId: number, queue: boolean) => `V2/RequestReport/Print/Account/${accountId}?queue=${queue}`;

//post, takes args: (model: number)
export const printSupplierBalanceList = () => `V2/RequestReport/Print/SupplierBalanceList`;

//post, takes args: (model: number)
export const printCustomerBalanceList = () => `V2/RequestReport/Print/CustomerBalanceList`;

//post, takes args: (model: number)
export const printInvoicesJournal = () => `V2/RequestReport/Print/InvoicesJournal`;

//post, takes args: (model: number)
export const printIOVoucher = () => `V2/RequestReport/Print/IOVoucher`;

//post, takes args: (model: number)
export const printIOCustomerInvoice = () => `V2/RequestReport/Print/IOCustomerInvoice`;

//get
export const printStockInventory = (reportId: number, stockInventoryHeadId: number, queue: boolean) => `V2/RequestReport/Print/StockInventory?reportId=${reportId}&stockInventoryHeadId=${stockInventoryHeadId}&queue=${queue}`;

//post, takes args: (model: number)
export const printHouseholdTaxDeduction = () => `V2/RequestReport/Print/HouseholdTaxDeduction`;

//post, takes args: (model: number)
export const printCustomerInvoice = () => `V2/RequestReport/Print/CustomerInvoice`;


