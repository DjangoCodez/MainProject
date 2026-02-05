


//Available methods for CompanyGroupAdministrationController

//get
export const getCompanyGroupAdministrationGrid = (companyGroupAdministrationId?: number) => `V2/Economy/CompanyGroupAdministration/Grid/${companyGroupAdministrationId || ''}`;

//get
export const getCompanyGroupAdministration = (companyGroupAdministrationId: number) => `V2/Economy/CompanyGroupAdministration/${companyGroupAdministrationId}`;

//get
export const getGetChildCompaniesDict = () => `V2/Economy/CompanyGroupAdministration/ConsolidatingAccounting/ChildCompaniesDict/`;

//get
export const getCompanyGroupMappingHeadsDict = (addEmptyRow: boolean) => `V2/Economy/CompanyGroupAdministration/ConsolidatingAccounting/CompanyGroupMappingHeadsDict/${addEmptyRow}`;

//post, takes args: (model: number)
export const saveCompanyGroupAdministration = () => `V2/Economy/CompanyGroupAdministration/CompanyGroupAdministration`;

//delete
export const deleteCompanyGroupAdministration = (companyGroupAdministrationId: number) => `V2/Economy/CompanyGroupAdministration/${companyGroupAdministrationId}`;


