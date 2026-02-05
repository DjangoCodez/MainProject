


//Available methods for DistributionCodeController

//get
export const getDistributionCodesGrid = (distributionId?: number) => `V2/Economy/Accounting/DistributionCode/Grid/${distributionId || ''}`;

//get
export const getDistributionCodes = (includePeriods: boolean, budgetType?: number, fromDate?: number, toDate?: number) => `V2/Economy/Accounting/DistributionCode?includePeriods=${includePeriods}&budgetType=${budgetType}&fromDate=${encodeURIComponent(String(fromDate))}&toDate=${encodeURIComponent(String(toDate))}`;

//get
export const getDistributionCodesDict = (addEmptyRow: boolean) => `V2/Economy/Accounting/DistributionCode/Dict?addEmptyRow=${addEmptyRow}`;

//get
export const getDistributionCodesByType = (distributionCodeType: number, loadPeriods: boolean) => `V2/Economy/Accounting/DistributionCodesByType/${distributionCodeType}/${loadPeriods}`;

//get
export const getDistributionCode = (distributionCodeHeadId: number) => `V2/Economy/Accounting/DistributionCode/${distributionCodeHeadId}`;

//post, takes args: (model: number)
export const saveDistributionCode = () => `V2/Economy/Accounting/DistributionCode`;

//delete
export const deleteDistributionCode = (distributionCodeHeadId: number) => `V2/Economy/Accounting/DistributionCode/${distributionCodeHeadId}`;


