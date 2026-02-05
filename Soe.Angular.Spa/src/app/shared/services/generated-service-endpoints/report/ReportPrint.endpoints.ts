


//Available methods for ReportPrintController

//get
export const getReportPrintUrl = (sysReportTemplateTypeId: number, id: number) => `V2/ReportPrint/Print/Url/${sysReportTemplateTypeId}/${id}`;

//get
export const getBalanceListReportPrintUrl = (reportId: number, sysReportTemplateType: number, invoiceIds: number[]) => `V2/ReportPrint/Print/BalanceList/Url/${reportId}/${sysReportTemplateType}?invoiceIds=${invoiceIds}`;

//post, takes args: (model: number)
export const getBalanceListReportPrintUrl0 = () => `V2/ReportPrint/Print/BalanceList/Url/`;

//post, takes args: (customerInvoiceIds: number[])
export const invoiceInterestPrintUrl = (customerInvoiceIds: number[]) => `V2/ReportPrint/Print/InvoiceInterestPrintUrl/?customerInvoiceIds=${customerInvoiceIds}`;

//post, takes args: (model: number)
export const productListPrintUrl = () => `V2/ReportPrint/Print/ProductListReportUrl/`;

//post, takes args: (voucherListIds: number[])
export const getVoucherListPrintUrl = (voucherListIds: number[]) => `V2/ReportPrint/Print/VoucherListPrintUrl/?voucherListIds=${voucherListIds}`;

//get
export const getInterestRateCalculationPrintUrl = (reportId: number, sysReportTemplateType: number, invoiceIds: number[]) => `V2/ReportPrint/Print/InterestRateCalculationPrintUrl/${reportId}/${sysReportTemplateType}?invoiceIds=${invoiceIds}`;

//post, takes args: (model: number)
export const getCustomerInvoiceIOReportUrl = () => `V2/ReportPrint/Print/CustomerInvoiceIOReportUrl/`;

//post, takes args: (model: number)
export const getVoucherHeadIOReportUrl = () => `V2/ReportPrint/Print/VoucherHeadIOReportUrl/`;

//get
export const getDefaultAccountingOrderPrintUrl = (voucherHeadId: number) => `V2/ReportPrint/Print/DefaultAccountingOrderPrintUrl/${voucherHeadId}`;

//post, takes args: (model: number)
export const getOrderPrintUrl = () => `V2/ReportPrint/Print/OrderPrintUrl/`;

//post, takes args: (model: number)
export const getOrderPrintUrlSingle = () => `V2/ReportPrint/Print/OrderPrintUrl/Single`;

//post, takes args: (model: number)
export const getPurchasePrintUrl = () => `V2/ReportPrint/Print/PurchasePrintUrl/`;

//post, takes args: (model: number)
export const sendReport = () => `V2/ReportPrint/Print/SendReport/`;

//post, takes args: (model: number)
export const getProjectTransactionsPrintUrl = () => `V2/ReportPrint/Print/ProjectTransactionsPrintUrl/`;

//get
export const getPayrollProductReportPrintUrl = (reportId: number, sysReportTemplateType: number, productIds: number[]) => `V2/ReportPrint/Print/PayrollProductListPrintUrl/${reportId}/${sysReportTemplateType}?productIds=${productIds}`;

//get
export const getChecklistPrintUrl = (invoiceId: number, headRecordId: number, reportId: number) => `V2/ReportPrint/Print/ChecklistPrintUrl/${invoiceId}/${headRecordId}/${reportId}`;

//post, takes args: (model: number)
export const getTimeEmployeeSchedulePrintUrl = () => `V2/ReportPrint/Print/TimeEmployeeSchedulePrintUrl/`;

//post, takes args: (model: number)
export const getTimeScheduleTasksAndDeliverysReportPrintUrl = () => `V2/ReportPrint/Print/TimeScheduleTasksAndDeliverysReportPrintUrl/`;

//post, takes args: (model: number)
export const getHouseholdTaxDeductionPrintUrl = () => `V2/ReportPrint/Print/HouseholdTaxDeduction/`;

//post, takes args: (model: number)
export const stockInventoryPrintUrl = () => `V2/ReportPrint/Print/StockInventoryPrintUrl/`;


