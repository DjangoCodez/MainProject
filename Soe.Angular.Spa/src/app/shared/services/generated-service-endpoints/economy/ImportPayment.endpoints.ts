


//Available methods for ImportPaymentController

//get
export const getPaymentImports = (allItemsSelection: number, paymentImportId?: number) => `V2/Economy/ImportPayment/PaymentImports?allItemsSelection=${allItemsSelection}&paymentImportId=${paymentImportId}`;

//post, takes args: (model: number)
export const savePaymentImportHeader = () => `V2/Economy/ImportPayment/PaymentImportHeader/`;

//post, takes args: (model: number)
export const startPaymentImport = () => `V2/Economy/ImportPayment/PaymentImport/`;

//delete
export const deletePaymentImportHeader = (batchId: number, importType: number) => `V2/Economy/ImportPayment/ImportedIoInvoices/${batchId}/${importType}`;

//get
export const getImportedIoInvoices = (batchId: number, importType: number) => `V2/Economy/ImportPayment/ImportedIoInvoices/${batchId}/${importType}`;

//get
export const getPaymentImport = (importId: number) => `V2/Economy/ImportPayment/PaymentImport/${importId}`;

//post, takes args: (model: number)
export const savePaymentImportIOs = () => `V2/Economy/ImportPayment/SavePaymentImportIOs/`;

//post, takes args: (model: number)
export const updatePaymentImportIO = () => `V2/Economy/ImportPayment/PaymentImportIO/`;

//post, takes args: (savePaymentItems: number)
export const updatePaymentImportIODTOS = () => `V2/Economy/ImportPayment/PaymentImportIODTOsUpdate/`;

//post, takes args: (savePaymentItems: number)
export const updateCustomerPaymentImportIODTOS = () => `V2/Economy/ImportPayment/CustomerPaymentImportIODTOsUpdate/`;

//post, takes args: (savePaymentItems: number)
export const updatePaymentImportIODTOSStatus = () => `V2/Economy/ImportPayment/PaymentImportIODTOsUpdateStatus/`;

//post, takes args: ()
export const paymentImport = () => `V2/Economy/ImportPayment/PaymentFileImport`;

//post, takes args: (dataStorageIds: number[])
export const importFinvoiceFiles = (dataStorageIds: number[]) => `V2/Economy/ImportPayment/FinvoiceImport/?dataStorageIds=${dataStorageIds}`;

//post, takes args: ()
export const importFinvoiceAttachments = () => `V2/Economy/ImportPayment/FinvoiceImport/Attachments/`;

//delete
export const deletePaymentImportIO = (paymentImportIOId: number) => `V2/Economy/ImportPayment/PaymentImportIO/${paymentImportIOId}`;

//post, takes args: (model: number)
export const getPaymentMethodsDict = () => `V2/Economy/ImportPayment/PaymentMethods`;

//get
export const getPaymentMethodsForImport = (originTypeId: number) => `V2/Economy/ImportPayment/PaymentMethods/ForImport/${originTypeId}`;

//get
export const getSysPaymentTypes = () => `V2/Economy/ImportPayment/PaymentTypes/`;


