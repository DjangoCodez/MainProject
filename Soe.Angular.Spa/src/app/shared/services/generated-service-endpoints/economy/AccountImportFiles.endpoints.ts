


//Available methods for AccountImportFilesController

//get
export const getPaymentImports = (importType: number, allItemsSelection: number) => `V2/Economy/Account/PaymentImports/${importType}/${allItemsSelection}`;

//post, takes args: (model: number)
export const savePaymentImportHeader = () => `V2/Economy/Account/PaymentImportHeader/`;

//post, takes args: (model: number)
export const startPaymentImport = () => `V2/Economy/Account/PaymentImport/`;

//get
export const getImportedIoInvoices = (batchId: number, importType: number) => `V2/Economy/Account/ImportedIoInvoices/${batchId}/${importType}`;

//get
export const getPaymentImport = (importId: number) => `V2/Economy/Account/PaymentImport/${importId}`;

//post, takes args: (model: number)
export const updatePaymentImportIO = () => `V2/Economy/Account/PaymentImportIO/`;

//post, takes args: (savePaymentItems: number)
export const updatePaymentImportIODTOS = () => `V2/Economy/Account/PaymentImportIODTOsUpdate/`;

//post, takes args: (savePaymentItems: number)
export const updateCustomerPaymentImportIODTOS = () => `V2/Economy/Account/CustomerPaymentImportIODTOsUpdate/`;

//post, takes args: (savePaymentItems: number)
export const updatePaymentImportIODTOSStatus = () => `V2/Economy/Account/PaymentImportIODTOsUpdateStatus/`;

//post, takes args: ()
export const paymentImport = () => `V2/Economy/Account/PaymentFileImport`;

//post, takes args: (dataStorageIds: number[])
export const importFinvoiceFiles = (dataStorageIds: number[]) => `V2/Economy/Account/FinvoiceImport/?dataStorageIds=${dataStorageIds}`;

//post, takes args: ()
export const importFinvoiceAttachments = () => `V2/Economy/Account/FinvoiceImport/Attachments/`;

//delete
export const deletePaymentImportHeader = (batchId: number, importType: number) => `V2/Economy/Account/ImportedIoInvoices/${batchId}/${importType}`;

//delete
export const deletePaymentImportIO = (paymentImportIOId: number) => `V2/Economy/Account/PaymentImportIO/${paymentImportIOId}`;


