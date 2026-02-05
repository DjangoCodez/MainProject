


//Available methods for ConnectController

//get
export const getImports = (module: number) => `V2/Core/Connect/Imports/${module}`;

//get
export const getImport = (importId: number) => `V2/Core/Connect/ImportEdit/${importId}`;

//get
export const getSysImportDefinitions = (module: number) => `V2/Core/Connect/SysImportDefinitions/${module}`;

//get
export const getSysImportHeads = () => `V2/Core/Connect/SysImportHeads/`;

//get
export const getImportBatches = (importHeadType: number, allItemsSelection: number) => `V2/Core/Connect/Batches/${importHeadType}/${allItemsSelection}`;

//get
export const getImportGridColumns = (importHeadType: number) => `V2/Core/Connect/ImportGridColumns/${importHeadType}`;

//get
export const getImportIOResult = (importHeadType: number, batchId: string) => `V2/Core/Connect/ImportIOResult/${importHeadType}/${encodeURIComponent(batchId)}`;

//post, takes args: (import: number)
export const saveImport = () => `V2/Core/Connect/ImportEdit/`;

//post, takes args: (model: number)
export const importFile = () => `V2/Core/Connect/ImportFile/`;

//post, takes args: (model: number)
export const importIO = () => `V2/Core/Connect/ImportIO/`;

//post, takes args: (customerIODTOs: number)
export const saveCustomerIODTO = () => `V2/Core/Connect/Connect/CustomerIODTO/`;

//post, takes args: (customerInvoiceIODTOs: number)
export const saveCustomerInvoiceIODTO = () => `V2/Core/Connect/Connect/CustomerInvoiceIODTO/`;

//post, takes args: (customerInvoiceRowIODTOs: number)
export const saveCustomerInvoiceRowIODTO = () => `V2/Core/Connect/Connect/CustomerInvoiceRowIODTO/`;

//post, takes args: (supplierIODTOs: number)
export const saveSupplierIODTO = () => `V2/Core/Connect/Connect/SupplierIODTO/`;

//post, takes args: (supplierInvoiceIODTOs: number)
export const saveSupplieInvoiceIODTO = () => `V2/Core/Connect/Connect/SupplierInvoiceIODTO/`;

//post, takes args: (voucherIODTOs: number)
export const saveVoucherIODTO = () => `V2/Core/Connect/Connect/VoucherIODTO/`;

//post, takes args: (projectIODTOs: number)
export const saveProjectIODTO = () => `V2/Core/Connect/Connect/ProjectIODTO/`;

//post, takes args: (files: number)
export const getImportSelectionGrid = () => `V2/Core/Connect/Connect/ImportSelectionGrid/`;

//delete
export const deleteImport = (importId: number) => `V2/Core/Connect/Connect/ImportEdit/${importId}`;


