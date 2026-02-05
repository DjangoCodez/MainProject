


//Available methods for GrossProfitCodeController

//get
export const getGrossProfitCodesGrid = (grossProfitCodeId?: number) => `V2/Economy/Account/GrossProfitCode/Grid?grossProfitCodeId=${grossProfitCodeId}`;

//get
export const getGrossProfitCodes = () => `V2/Economy/Account/GrossProfitCode/`;

//get
export const getGrossProfitCodesByYear = (accountYearId: number) => `V2/Economy/Account/GrossProfitCode/ByYear/${accountYearId}`;

//get
export const getGrossProfitCode = (grossProfitCodeId: number) => `V2/Economy/Account/GrossProfitCode/${grossProfitCodeId}`;

//post, takes args: (grossProfitCodeDTO: number)
export const saveGrossProfitCode = () => `V2/Economy/Account/GrossProfitCode`;

//delete
export const deleteGrossProfitCode = (grossProfitCodeId: number) => `V2/Economy/Account/GrossProfitCode/${grossProfitCodeId}`;


