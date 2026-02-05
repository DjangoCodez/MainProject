


//Available methods for TimeLeisureCodeController

//get
export const getTimeLeisureCodesGrid = (timeLeisureCodeId?: number) => `V2/Time/TimeLeisureCode/Grid/${timeLeisureCodeId || ''}`;

//get
export const getTimeLeisureCode = (timeLeisureCodeId: number) => `V2/Time/TimeLeisureCode/${timeLeisureCodeId}`;

//post, takes args: (model: number)
export const saveTimeLeisureCode = () => `V2/Time/TimeLeisureCode`;

//delete
export const deleteTimeLeisureCode = (timeLeisureCodeId: number) => `V2/Time/TimeLeisureCode/${timeLeisureCodeId}`;

//get
export const getEmployeeGroupTimeLeisureCodesGrid = (employeeGroupTimeLeisureCodeId?: number) => `V2/Time/TimeLeisureCode/EmployeeGroup/Grid/${employeeGroupTimeLeisureCodeId || ''}`;

//get
export const getEmployeeGroupTimeLeisureCode = (employeeGroupTimeLeisureCodeId: number) => `V2/Time/TimeLeisureCode/EmployeeGroup/${employeeGroupTimeLeisureCodeId}`;

//post, takes args: (model: number)
export const saveEmployeeGroupTimeLeisureCode = () => `V2/Time/TimeLeisureCode/EmployeeGroup`;

//delete
export const deleteEmployeeGroupTimeLeisureCode = (employeeGroupTimeLeisureCodeId: number) => `V2/Time/TimeLeisureCode/EmployeeGroup/${employeeGroupTimeLeisureCodeId}`;


