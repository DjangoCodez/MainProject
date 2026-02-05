


//Available methods for SupplierInvoiceController

//get
export const getFile = (fileId: number) => `V2/Economy/SupplierInvoice/File/${fileId}`;

//get
export const getSupplierInvoiceImageByFileId = (fileId: number) => `V2/Economy/SupplierInvoice/Invoice/ImageByFileId/${fileId}`;

//get
export const getSupplierInvoiceImage = (invoiceId: number) => `V2/Economy/SupplierInvoice/Invoice/SupplierInvoiceImage/${invoiceId}`;

//get
export const getSupplierInvoiceImageFromEdi = (ediEntryId: number) => `V2/Economy/SupplierInvoice/Invoice/SupplierInvoiceImage/edi/${ediEntryId}`;

//get
export const getInvoices = (loadOpen: boolean, loadClosed: boolean, onlyMine: boolean, allItemsSelection: number, projectId: number, includeChildProjects: boolean) => `V2/Economy/SupplierInvoice/Invoice/${loadOpen}/${loadClosed}/${onlyMine}/${allItemsSelection}/${projectId}/${includeChildProjects}`;

//get
export const getInvoicesForSupplier = (loadOpen: boolean, loadClosed: boolean, onlyMine: boolean, allItemsSelection: number, supplierId: number) => `V2/Economy/SupplierInvoice/Invoice/${loadOpen}/${loadClosed}/${onlyMine}/${allItemsSelection}/${supplierId}`;

//post, takes args: (model: number)
export const getInvoicesForProjectCentral = () => `V2/Economy/SupplierInvoice/Invoice/ProjectCentral/`;

//post, takes args: (filterModels: number)
export const getFilteredSupplierInvoices = () => `V2/Economy/SupplierInvoice/Invoice/Filtered/`;

//get
export const getInvoice = (invoiceId: number, loadProjectRows: boolean, loadOrderRows: boolean, loadProject: boolean, loadImage: boolean) => `V2/Economy/SupplierInvoice/Invoice/${invoiceId}/${loadProjectRows}/${loadOrderRows}/${loadProject}/${loadImage}`;

//get
export const getInvoiceTraceViews = (invoiceId: number) => `V2/Economy/SupplierInvoice/Invoice/GetInvoiceTraceViews/${invoiceId}`;

//get
export const getSupplierInvoiceText = (type: number, invoiceId?: number, ediEntryId?: number) => `V2/Economy/SupplierInvoice/Invoice/BlockPayment?type=${type}&invoiceId=${invoiceId}&ediEntryId=${ediEntryId}`;

//post, takes args: (model: number)
export const blockSupplierInvoicePayment = () => `V2/Economy/SupplierInvoice/Invoice/BlockPayment/`;

//post, takes args: (model: number)
export const invoiceTextAction = () => `V2/Economy/SupplierInvoice/Invoice/InvoiceTextAction/`;

//post, takes args: (model: number)
export const supplierInvoiceNrAlreadyExist = () => `V2/Economy/SupplierInvoice/Invoice/SupplierInvoiceNrAlreadyExist`;

//get
export const getOrder = (invoiceId: number, includeRows: boolean) => `V2/Economy/SupplierInvoice/GetOrder/${invoiceId}/${includeRows}`;

//get
export const getOrderForSupplier = (orderNr: string) => `V2/Economy/SupplierInvoice/GetOrder/${encodeURIComponent(orderNr)}`;

//get
export const getOrdersForSupplierInvoiceEdit = () => `V2/Economy/SupplierInvoice/GetOrdersForSupplierInvoiceEdit/`;

//get
export const getProjectsList = (type: number, active?: boolean, getHidden?: boolean, getFinished?: boolean) => `V2/Economy/SupplierInvoice/Project/${type}/?active=${active}&getHidden=${getHidden}&getFinished=${getFinished}`;

//get
export const getEmployees = (active: boolean, addRoleInfo: boolean, addCategoryInfo: boolean, addEmployeeGroupInfo: boolean, addProjectDefaultTimeCode: boolean, addEmployments: boolean, useShowOtherEmployeesPermission: boolean, getHidden: boolean, loadFactors: boolean, addPayrollGroupInfo: boolean, loadEmployeeVacation: boolean) => `V2/Economy/SupplierInvoice/Employees/${active}/${addRoleInfo}/${addCategoryInfo}/${addEmployeeGroupInfo}/${addProjectDefaultTimeCode}/${addEmployments}/${useShowOtherEmployeesPermission}/${getHidden}/${loadFactors}/${addPayrollGroupInfo}/${loadEmployeeVacation}`;

//get
export const getTimeCodes = (timeCodeType: number, active: boolean, loadPayrollProducts: boolean) => `V2/Economy/SupplierInvoice/TimeCodes/${timeCodeType}/${active}/${loadPayrollProducts}/`;

//get
export const getSupplierInvoiceProjectTransactions = (invoiceId: number) => `V2/Economy/SupplierInvoice/SupplierInvoiceProjectTransactions/${invoiceId}/`;

//get
export const getSupplierInvoiceAccountingRows = (invoiceId: number) => `V2/Economy/SupplierInvoice/Invoice/AccountingRows/${invoiceId}`;

//get
export const getSupplierInvoicesCostOverview = (notLinked: boolean, partiallyLinked: boolean, linked: boolean, allItemsSelection: number) => `V2/Economy/SupplierInvoice/Invoice/CostOverview/${notLinked}/${partiallyLinked}/${linked}/${allItemsSelection}`;

//get
export const getSupplierInvoiceOrderRows = (invoiceId: number) => `V2/Economy/SupplierInvoice/Invoice/OrderRows/${invoiceId}`;

//get
export const getSupplierInvoiceProjectRows = (invoiceId: number) => `V2/Economy/SupplierInvoice/Invoice/ProjectRows/${invoiceId}`;

//get
export const getSupplierInvoiceCostAllocationRows = (invoiceId: number) => `V2/Economy/SupplierInvoice/Invoice/OrderProjectRows/${invoiceId}`;

//post, takes args: (model: number)
export const saveSupplierInvoiceCostAllocationRows = () => `V2/Economy/SupplierInvoice/Invoice/OrderProjectRows/`;

//post, takes args: (model: number)
export const getSupplierCentralCountersAndBalance = () => `V2/Economy/SupplierInvoice/SupplierCentralCountersAndBalance/`;

//post, takes args: (model: number)
export const saveInvoice = () => `V2/Economy/SupplierInvoice/Invoice/`;

//delete
export const deleteInvoice = (invoiceId: number, deleteProject: boolean) => `V2/Economy/SupplierInvoice/Invoice/${invoiceId}/${deleteProject}`;

//delete
export const deleteInvoices = (invoices: string) => `V2/Economy/SupplierInvoice/DeleteDraftInvoices/${encodeURIComponent(invoices)}`;

//post, takes args: (invoiceId: number, accountingRows: number)
export const saveSupplierInvoiceAttestAccountingRows = (invoiceId: number) => `V2/Economy/SupplierInvoice/Invoice/AttestAccountingRows/${invoiceId}`;

//post, takes args: (invoiceId: number, model: number)
export const saveSupplierInvoiceAccountingRows = (invoiceId: number) => `V2/Economy/SupplierInvoice/Invoice/AccountingRows/${invoiceId}`;

//post, takes args: (ediSourceType: number)
export const addScanningEntrys = (ediSourceType: number) => `V2/Economy/SupplierInvoice/AddScanningEntrys/${ediSourceType}`;

//post, takes args: (model: number)
export const transferSupplierInvoicesToDefinitive = () => `V2/Economy/SupplierInvoice/TransferInvoicesToDefinitive/`;

//post, takes args: (model: number)
export const transferSupplierInvoicesToVouchers = () => `V2/Economy/SupplierInvoice/TransferInvoicesToVoucher/`;

//get
export const transferSupplierInvoicesToVouchersResult = (key: string) => `V2/Economy/SupplierInvoice/TransferInvoicesToVoucher/${encodeURIComponent(key)}`;

//post, takes args: (model: number)
export const hideUnhandled = () => `V2/Economy/SupplierInvoice/Invoice/HideUnhandled/`;

//post, takes args: (itemsToTransfer: number)
export const transferEdiToInvoices = () => `V2/Economy/SupplierInvoice/TransferEdiToInvoices/`;

//post, takes args: (itemsToTransfer: number)
export const transferEdiToOrder = () => `V2/Economy/SupplierInvoice/TransferEdiToOrder/`;

//post, takes args: (model: number)
export const transferEdiState = () => `V2/Economy/SupplierInvoice/TransferEdiState/`;

//get
export const getEdiEntry = (ediEntryId: number, loadSuppliers: boolean) => `V2/Economy/SupplierInvoice/GetEdiEntry/${ediEntryId}/${loadSuppliers}`;

//post, takes args: (ediEntries: number)
export const transferEdiState0 = () => `V2/Economy/SupplierInvoice/UpdateEdiEntries/`;

//post, takes args: (ediEntryIds: number[])
export const generateReportForEdi = (ediEntryIds: number[]) => `V2/Economy/SupplierInvoice/GenerateReportForEdi/?ediEntryIds=${ediEntryIds}`;

//post, takes args: (ediEntryIds: number[])
export const generateReportForFinvoice = (ediEntryIds: number[]) => `V2/Economy/SupplierInvoice/GenerateReportForFinvoice/?ediEntryIds=${ediEntryIds}`;

//get
export const getEdiEntryFromInvoice = (invoiceId: number) => `V2/Economy/SupplierInvoice/GetEdiEntryFromInvoice/${invoiceId}`;

//post, takes args: (dataStorageIds: number[])
export const saveSupplierInvoicesForUploadedImages = (dataStorageIds: number[]) => `V2/Economy/SupplierInvoice/SaveInvoicesForImages/?dataStorageIds=${dataStorageIds}`;

//post, takes args: (invoiceId: number, attestGroupId: number)
export const saveSupplierInvoiceAttestGroup = (invoiceId: number, attestGroupId: number) => `V2/Economy/SupplierInvoice/Invoice/SupplierInvoiceChangeAttestGroup/${invoiceId}/${attestGroupId}`;

//post, takes args: (ediEntryId: number)
export const saveSupplierFromFinvoice = (ediEntryId: number) => `V2/Economy/SupplierInvoice/SaveSupplierFromFinvoice/${ediEntryId}`;

//post, takes args: (model: number)
export const transferSupplierProductRows = () => `V2/Economy/SupplierInvoice/Invoice/ProductRows/Transfer`;

//get
export const getSupplierInvoiceProductRows = (invoiceId: number) => `V2/Economy/SupplierInvoice/Invoice/ProductRows/${invoiceId}`;

//post, takes args: (model: number)
export const transferSupplierInvoicesToOrder = () => `V2/Economy/SupplierInvoice/Invoice/TransferToOrder`;

//get
export const getScanningInterpretation = (ediEntryId: number) => `V2/Economy/SupplierInvoice/Scanning/Interpretation/${ediEntryId}`;


