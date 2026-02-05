


//Available methods for CompanyGroupTransferController

//get
export const getCompanyGroupVoucherHistory = (accountYearId: number, transferType: number) => `V2/Economy/Accounting/ConsolidatingAccounting/ConsolidatingAccounting/CompanyGroupVoucherHistory/${accountYearId}/${transferType}`;

//post, takes args: (accountYearId: number)
export const saveCompanyGroupVoucherSeries = (accountYearId: number) => `V2/Economy/Accounting/ConsolidatingAccounting/ConsolidatingAccounting/CompanyGroupVoucherSerie/${accountYearId}`;

//post, takes args: (model: number)
export const companyGroupTransfer = () => `V2/Economy/Accounting/ConsolidatingAccounting/ConsolidatingAccounting/CompanyGroupTransfer/`;

//post, takes args: (companyGroupTransferHeadId: number)
export const deleteCompanyGroupTransfer = (companyGroupTransferHeadId: number) => `V2/Economy/Accounting/ConsolidatingAccounting/ConsolidatingAccounting/CompanyGroupTransfer/Delete/${companyGroupTransferHeadId}`;


