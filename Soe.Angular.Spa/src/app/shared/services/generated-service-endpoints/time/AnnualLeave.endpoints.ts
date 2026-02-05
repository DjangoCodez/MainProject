


//Available methods for AnnualLeaveController

//post, takes args: (model: number)
export const calculateAnnualLeaveTransactions = () => `V2/Time/AnnualLeave/CalculateTransactions`;

//get
export const getAnnualLeaveGroupsGrid = (annualLeaveGroupId?: number) => `V2/Time/AnnualLeave/Grid/${annualLeaveGroupId || ''}`;

//get
export const getAnnualLeaveGroupsDict = (addEmptyRow: boolean) => `V2/Time/AnnualLeave/Dict/${addEmptyRow}`;

//get
export const getAnnualLeaveGroup = (annualLeaveGroupId: number) => `V2/Time/AnnualLeave/${annualLeaveGroupId}`;

//get
export const getAnnualLeaveGroupLimits = (type: number) => `V2/Time/AnnualLeave/AnnualLeaveGroup/Limits/${type}`;

//post, takes args: (model: number)
export const saveAnnualLeaveGroup = () => `V2/Time/AnnualLeave`;

//delete
export const deleteAnnualLeaveGroup = (annualLeaveGroupId: number) => `V2/Time/AnnualLeave/${annualLeaveGroupId}`;

//get
export const getAnnualLeaveTransaction = (annualLeaveTransactionId: number) => `V2/Time/AnnualLeave/Transaction/${annualLeaveTransactionId}`;

//post, takes args: (model: number)
export const getAnnualLeaveTransactionGridData = () => `V2/Time/AnnualLeave/Transaction/Grid`;

//post, takes args: (model: number)
export const saveAnnualLeaveTransaction = () => `V2/Time/AnnualLeave/Transaction`;

//delete
export const deleteAnnualLeaveTransaction = (annualLeaveTransactionId: number) => `V2/Time/AnnualLeave/Transaction/${annualLeaveTransactionId}`;


