


//Available methods for SysCompanyController

//get
export const getSysCompanies = (sysCompanyId?: number) => `V2/Manage/System/SysCompany/Grid/${sysCompanyId || ''}`;

//get
export const getSysCompanyDict = () => `V2/Manage/System/SysCompany/SysCompanyDict`;

//get
export const getSysCompany = (sysCompanyId: number) => `V2/Manage/System/SysCompany/${sysCompanyId}`;

//get
export const getSysCompanyByApiKey = (companyApiKey: string, sysCompDbId: number) => `V2/Manage/System/SysCompany/${encodeURIComponent(companyApiKey)}/${sysCompDbId}`;

//post, takes args: (sysCompanyDTO: number)
export const saveSysCompany = () => `V2/Manage/System/SysCompany/`;


