


//Available methods for ExportFilesController

//get
export const getPaymentServiceRecords = (invoiceExportId?: number) => `V2/Economy/Export/Invoices/GetPaymentServiceRecordsGrid/${invoiceExportId || ''}`;

//get
export const getExportedIOInvoices = (invoiceExportId: number) => `V2/Economy/Export/Invoices/GetExportedIOInvoices/${invoiceExportId}`;

//get
export const getInvoicesForPaymentService = (paymentService: number) => `V2/Economy/Export/Invoices/PaymentService/GetInvoicesForPaymentService/${paymentService}`;

//get
export const getSAFTTransactionsForExport = (fromDate: string, toDate: string) => `V2/Economy/Export/Invoices/Saft/Transactions/${encodeURIComponent(fromDate)}/${encodeURIComponent(toDate)}`;

//get
export const createSAFTExport = (fromDate: string, toDate: string) => `V2/Economy/Export/Invoices/Saft/Export/${encodeURIComponent(fromDate)}/${encodeURIComponent(toDate)}`;

//post, takes args: (items: number, paymentService: number)
export const saveCustomerInvoicePaymentService = (paymentService: number) => `V2/Economy/Export/Invoices/PaymentService/Invoices/${paymentService}`;


