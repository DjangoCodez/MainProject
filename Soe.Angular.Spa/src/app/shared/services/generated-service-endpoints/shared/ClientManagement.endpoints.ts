


//Available methods for ClientManagementController

//post, takes args: ()
export const initRequest = () => `V2/Shared/ClientManagement/ConnectionRequest`;

//get
export const getRequestStatus = (connectionRequestId: number) => `V2/Shared/ClientManagement/ConnectionRequest/${connectionRequestId}/status`;

//get
export const getClients = () => `V2/Shared/ClientManagement/Clients`;


