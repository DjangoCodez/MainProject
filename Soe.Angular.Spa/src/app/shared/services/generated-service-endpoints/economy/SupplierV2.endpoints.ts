


//Available methods for SupplierV2Controller

//get
export const getSuppliers = (onlyActive: boolean, supplierId?: number) => `V2/Economy/Supplier/Supplier/?onlyActive=${onlyActive}&supplierId=${supplierId}`;

//get
export const getSuppliersDict = (onlyActive: boolean, addEmptyRow: boolean) => `V2/Economy/Supplier/Supplier/Dict/?onlyActive=${onlyActive}&addEmptyRow=${addEmptyRow}`;

//post, takes args: (dto: number)
export const getSuppliersBySearch = () => `V2/Economy/Supplier/Supplier/BySearch/`;

//get
export const getSupplier = (supplierId: number, loadActor: boolean, loadAccount: boolean, loadContactAddresses: boolean, loadCategories: boolean) => `V2/Economy/Supplier/Supplier/${supplierId}/${loadActor}/${loadAccount}/${loadContactAddresses}/${loadCategories}`;

//get
export const getSupplierForExport = (supplierId: number) => `V2/Economy/Supplier/Supplier/Export/${supplierId}`;

//get
export const getNextSupplierNr = () => `V2/Economy/Supplier/Supplier/NextSupplierNr/`;

//post, takes args: (model: number)
export const saveSupplier = () => `V2/Economy/Supplier/Supplier`;

//delete
export const deleteSupplier = (supplierId: number) => `V2/Economy/Supplier/Supplier/${supplierId}`;

//post, takes args: (model: number)
export const updateSuppliersState = () => `V2/Economy/Supplier/Supplier/UpdateState`;

//post, takes args: (items: number)
export const updateIsPrivatePerson = () => `V2/Economy/Supplier/Supplier/UpdateIsPrivatePerson`;

//post, takes args: (ediEntryIds: number[])
export const generateReportForEdi = (ediEntryIds: number[]) => `V2/Economy/Supplier/GenerateReportForEdi/?ediEntryIds=${ediEntryIds}`;

//get
export const getOrdersForSupplierInvoiceEdit = () => `V2/Economy/Supplier/GetOrdersForSupplierInvoiceEdit/`;

//post, takes args: (ediEntries: number)
export const updateEdiEntrys = () => `V2/Economy/Supplier/UpdateEdiEntries/`;

//post, takes args: (ediEntryIds: number[])
export const generateReportForFinvoice = (ediEntryIds: number[]) => `V2/Economy/Supplier/GenerateReportForFinvoice/?ediEntryIds=${ediEntryIds}`;

//post, takes args: (itemsToTransfer: number)
export const transferEdiToInvoices = () => `V2/Economy/Supplier/TransferEdiToInvoices/`;

//post, takes args: (itemsToTransfer: number)
export const transferEdiToOrder = () => `V2/Economy/Supplier/TransferEdiToOrder/`;

//post, takes args: (model: number)
export const transferEdiState = () => `V2/Economy/Supplier/TransferEdiState/`;


