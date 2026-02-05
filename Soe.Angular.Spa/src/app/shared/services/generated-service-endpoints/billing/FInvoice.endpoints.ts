


//Available methods for FInvoiceController

//get
export const getEdiEntrysWithStateCheck = (classification: number, originType: number) => `V2/Billing/FInvoice/Edi/EdiEntryViews/${classification}/${originType}`;

//get
export const getEdiEntrysCountWithStateCheck = (classification: number, originType: number) => `V2/Billing/FInvoice/Edi/EdiEntryViews/Count/${classification}/${originType}`;

//post, takes args: (model: number)
export const getFilteredEdiEntrys = () => `V2/Billing/FInvoice/Edi/EdiEntryViews/Filtered/`;

//get
export const getFinvoiceEntrys = (classification: number, allItemsSelection: number, onlyUnHandled: boolean) => `V2/Billing/FInvoice/Edi/FinvoiceEntryViews/${classification}/${allItemsSelection}/${onlyUnHandled}`;

//post, takes args: (model: number)
export const importFinvoiceAttachments = () => `V2/Billing/FInvoice/FinvoiceImport/Attachments/`;

//post, takes args: (model: number)
export const uploadInvoiceFile = () => `V2/Billing/FInvoice/Files/Invoice`;


