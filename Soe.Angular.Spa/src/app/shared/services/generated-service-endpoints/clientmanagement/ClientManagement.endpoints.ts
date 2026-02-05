


//Available methods for ClientManagementController

//post, takes args: ()
export const initConnectionRequest = () => `V2/Shared/ClientManagement/ConnectionRequest`;

//get
export const getRequestStatus = (connectionRequestId: number) => `V2/Shared/ClientManagement/ConnectionRequest/${connectionRequestId}/Status`;

//get
export const getClients = () => `V2/Shared/ClientManagement/Clients`;

//get
export const getSupplierInvoiceOverview = () => `V2/Shared/ClientManagement/Suppliers/Invoices/Overview`;


