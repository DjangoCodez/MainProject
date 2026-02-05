


//Available methods for ClientCompanyController

//get
export const getConnectionRequest = (code: string) => `V2/Shared/ClientCompany/ConnectionRequest/${encodeURIComponent(code)}`;

//post, takes args: (dto: number)
export const saveServiceUser = () => `V2/Shared/ClientCompany/ConnectionRequest/Accept`;

//get
export const getServiceUserGrid = (userId: number) => `V2/Shared/ClientCompany/ServiceUser/Grid/${userId}`;

//get
export const getServiceUser = (serviceUserId: number) => `V2/Shared/ClientCompany/ServiceUser/${serviceUserId}`;


