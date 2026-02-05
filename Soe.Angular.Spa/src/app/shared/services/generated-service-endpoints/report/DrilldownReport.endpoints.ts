


//Available methods for DrilldownReportController

//get
export const getDrilldownReports = (onlyOriginal: boolean, onlyStandard: boolean) => `V2/Report/DrilldownReports/${onlyOriginal}/${onlyStandard}`;

//get
export const getDrilldownReport = (reportId: number, accountPerioIdFrom: number, accountPeriodIdTo: number, budgetHeadId: number) => `V2/Report/DrilldownReport/${reportId}/${accountPerioIdFrom}/${accountPeriodIdTo}/${budgetHeadId}`;

//post, takes args: (dto: number)
export const getDrilldownReportVoucherRows = () => `V2/Report/DrilldownReport/VoucherRows/`;


