//Available methods for CompanyGroupMappingsController

//get
export const getCompanyGroupMappingHeads = (
  companyGroupMappingHeadId?: number
) =>
  `V2/Economy/Accounting/ConsolidatingAccounting/CompanyGroupMappingHeads/Grid/${companyGroupMappingHeadId || ''}`;

//get
export const getCompanyGroupMappingHead = (companyGroupMappingHeadId: number) =>
  `V2/Economy/Accounting/ConsolidatingAccounting/CompanyGroupMappingHeads/${companyGroupMappingHeadId}`;

//post, takes args: (model: number)
export const saveCompanyGroupMappingHead = () =>
  `V2/Economy/Accounting/ConsolidatingAccounting/CompanyGroupMappingHeads`;

//delete
export const deleteCompanyGroupMappingHead = (
  companyGroupMappingHeadId: number
) =>
  `V2/Economy/Accounting/ConsolidatingAccounting/CompanyGroupMappingHeads/${companyGroupMappingHeadId}`;

//get
export const checkCompanyGroupMappingHeadNumberIsExists = (
  companyGroupMappingHeadId: number,
  companyGroupMappingHeadNumber: number
) =>
  `V2/Economy/Accounting/ConsolidatingAccounting/CompanyGroupMappingHeads/Exists/CompanyGroupMappingHeadNumber/${companyGroupMappingHeadId}/${companyGroupMappingHeadNumber}`;
