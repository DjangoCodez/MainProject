


//Available methods for InvoiceProjectController

//get
export const getProjectList = (projectId: number, projectStatuses: number[], onlyMine: boolean) => `V2/Billing/InvoiceProject/ProjectList?projectId=${projectId}&projectStatuses=${projectStatuses}&onlyMine=${onlyMine}`;

//get
export const getProjects = (onlyActive: boolean, hidden: boolean, setStatusName: boolean, includeManagerName: boolean, loadOrders: boolean, projectStatus: number) => `V2/Billing/InvoiceProject/${onlyActive}/${hidden}/${setStatusName}/${includeManagerName}/${loadOrders}/${projectStatus}`;

//get
export const getProject = (projectId: number) => `V2/Billing/InvoiceProject/${projectId}`;

//get
export const getTimeProject = (projectId: number) => `V2/Billing/InvoiceProject/GetTimeProject/${projectId}`;

//get
export const getProjectGridDTO = (projectId: number) => `V2/Billing/InvoiceProject/GridDTO/${projectId}`;

//get
export const getProjectsSmall = (onlyActive: boolean, hidden: boolean, sortOnNumber: boolean) => `V2/Billing/InvoiceProject/Project/Small/${onlyActive}/${hidden}/${sortOnNumber}`;

//post, takes args: (model: number)
export const changeProjectStatus = () => `V2/Billing/InvoiceProject`;

//get
export const getProjectTraceViews = (projectId: number) => `V2/Billing/InvoiceProject/GetProjectTraceViews/${projectId}`;

//get
export const getProjectCentralStatus = (projectId: number, includeChildProjects: boolean, loadDetails: boolean, from: string, to: string) => `V2/Billing/InvoiceProject/ProjectCentralStatus/${projectId}/${includeChildProjects}/${loadDetails}/${encodeURIComponent(from)}/${encodeURIComponent(to)}`;

//get
export const getBudgetHeadGridForProject = (projectId: number, actorCompanyId: number) => `V2/Billing/InvoiceProject/BudgetHead/${projectId}/${actorCompanyId}`;

//post, takes args: (model: number)
export const getProjectsBySearch = () => `V2/Billing/InvoiceProject/Search/`;

//post, takes args: (project: number)
export const saveProject = () => `V2/Billing/InvoiceProject/Save`;

//delete
export const deleteProject = (projectId: number) => `V2/Billing/InvoiceProject/${projectId}`;

//get
export const getProjectUsers = (projectId: number, loadTypeNames: boolean) => `V2/Billing/InvoiceProject/Users/${projectId}/${loadTypeNames}`;


