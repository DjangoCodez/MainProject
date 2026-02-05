


//Available methods for TimeCodeController

//get
export const getTimeCodes = (timeCodeType: number, onlyActive: boolean, loadPayrollProducts: boolean, onlyWithInvoiceProduct: boolean) => `V2/Time/TimeCode/${timeCodeType}/${onlyActive}/${loadPayrollProducts}/${onlyWithInvoiceProduct}`;

//get
export const getTimeCodesGrid = (timeCodeType: number, onlyActive: boolean, loadPayrollProducts: boolean, timeCodeId?: number) => `V2/Time/GetTimeCodesGrid/${timeCodeType}/${onlyActive}/${loadPayrollProducts}/${timeCodeId || ''}`;

//get
export const getTimeCodesDictByType = (timeCodeType: number, onlyActive: boolean, addEmptyRow: boolean, concatCodeAndName: boolean, loadPayrollProducts: boolean, onlyWithInvoiceProduct: boolean) => `V2/Time/TimeCode/${timeCodeType}/${onlyActive}/${addEmptyRow}/${concatCodeAndName}/${loadPayrollProducts}/${onlyWithInvoiceProduct}`;

//get
export const getTimeCode = (timeCodeType: number, timeCodeId: number, loadInvoiceProducts: boolean, loadPayrollProducts: boolean, loadTimeCodeDeviationCauses: boolean, loadEmployeeGroups: boolean) => `V2/Time/TimeCode/${timeCodeType}/${timeCodeId}/${loadInvoiceProducts}/${loadPayrollProducts}/${loadTimeCodeDeviationCauses}/${loadEmployeeGroups}`;

//get
export const getTimeCodeBreaks = (addEmptyRow: boolean) => `V2/Time/TimeCode/Break/${addEmptyRow}`;

//get
export const getTimeCodeAdditionDeductions = (checkInvoiceProduct: boolean) => `V2/Time/TimeCode/AdditionDeduction/${checkInvoiceProduct}`;

//post, takes args: (timeCode: number)
export const saveTimeCode = () => `V2/Time/TimeCode/`;

//post, takes args: (model: number)
export const updateTimeCodeState = () => `V2/Time/TimeCode/UpdateState`;

//delete
export const deleteTimeCode = (timeCodeId: number) => `V2/Time/TimeCode/${timeCodeId}`;

//get
export const getTimeCodesDict = (addEmptyRow: boolean, concatCodeAndName: boolean, includeType: boolean) => `V2/Time/TimeCode/Dict/${addEmptyRow}/${concatCodeAndName}/${includeType}`;

//get
export const getTimeCodeRankingGrid = (timeCodeRankingGroupId?: number) => `V2/Time/TimeCodeRankingGrid/${timeCodeRankingGroupId || ''}`;

//get
export const getTimeCodeRankings = (id: number) => `V2/Time/TimeCodeRanking/${id}`;

//post, takes args: (inputRankingGroup: number, isDelete: boolean)
export const validateTimeCodeRanking = (isDelete: boolean) => `V2/Time/TimeCodeRanking/Validate/${isDelete}`;

//post, takes args: (inputRanking: number)
export const saveTimeCodeRanking = () => `V2/Time/TimeCodeRanking/`;

//delete
export const deleteTimeCodeRanking = (id: number) => `V2/Time/TimeCodeRanking/Delete/${id}`;

//get
export const getTimeCodePayrollProducts = () => `V2/Time/TimeCode/PayrollProducts`;

//get
export const getTimeCodeInvoiceProducts = () => `V2/Time/TimeCode/InvoiceProducts`;


