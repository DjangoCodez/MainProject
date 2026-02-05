


//Available methods for EmployeeCollectiveAgreementController

//get
export const getEmployeeCollectiveAgreementsGrid = (collectiveAgreementId?: number) => `V2/Time/Employee/EmployeeCollectiveAgreement/Grid/${collectiveAgreementId || ''}`;

//get
export const getEmployeeCollectiveAgreementsDict = (addEmptyRow: boolean) => `V2/Time/Employee/EmployeeCollectiveAgreement/Dict/${addEmptyRow}`;

//get
export const getEmployeeCollectiveAgreements = () => `V2/Time/Employee/EmployeeCollectiveAgreement`;

//get
export const getEmployeeCollectiveAgreement = (employeeCollectiveAgreementId: number) => `V2/Time/Employee/EmployeeCollectiveAgreement/${employeeCollectiveAgreementId}`;

//post, takes args: (employeeCollectiveAgreementDTO: number)
export const saveEmployeeCollectiveAgreement = () => `V2/Time/Employee/EmployeeCollectiveAgreement`;

//delete
export const deleteEmployeeCollectiveAgreement = (employeeCollectiveAgreementId: number) => `V2/Time/Employee/EmployeeCollectiveAgreement/${employeeCollectiveAgreementId}`;


