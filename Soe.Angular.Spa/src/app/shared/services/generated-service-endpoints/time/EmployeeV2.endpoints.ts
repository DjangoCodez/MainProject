


//Available methods for EmployeeV2Controller

//get
export const getAllEmployeeSmallDTOs = (addEmptyRow: boolean, concatNumberAndName: boolean, getHidden: boolean, orderByName: boolean) => `V2/Time/Employee/Employees/${addEmptyRow}/${concatNumberAndName}/${getHidden}/${orderByName}`;

//get
export const getEmployeeListForPlanning = (employeeIds: string, getHidden: boolean, getInactive: boolean, loadSkills: boolean, loadAvailability: boolean, dateFrom: string, dateTo: string, includeSecondaryCategoriesOrAccounts: boolean, displayMode: number) => `V2/Time/Employee/Planning/${encodeURIComponent(employeeIds)}/${getHidden}/${getInactive}/${loadSkills}/${loadAvailability}/${encodeURIComponent(dateFrom)}/${encodeURIComponent(dateTo)}/${includeSecondaryCategoriesOrAccounts}/${displayMode}`;

//get
export const getEmployeeForUserWithTimeCode = (date: string) => `V2/Time/Employee/EmployeeForUser/TimeCode/${encodeURIComponent(date)}`;

//get
export const getEmployeesForGrid = (date: string, employeeIds: string, showInactive: boolean, showEnded: boolean, showNotStarted: boolean, setAge: boolean, loadPayrollGroups: boolean, loadAnnualLeaveGroups: boolean) => `V2/Time/Employee/EmployeesForGrid/${encodeURIComponent(date)}/${encodeURIComponent(employeeIds)}/${showInactive}/${showEnded}/${showNotStarted}/${setAge}/${loadPayrollGroups}/${loadAnnualLeaveGroups}`;

//get
export const getEmployeesForGridSmall = (date: string, employeeIds: string, showInactive: boolean, showEnded: boolean, showNotStarted: boolean) => `V2/Time/Employee/EmployeesForGridSmall/${encodeURIComponent(date)}/${encodeURIComponent(employeeIds)}/${showInactive}/${showEnded}/${showNotStarted}`;

//get
export const getEmployeesForGridDict = (dateFrom: string, dateTo: string, employeeIds: string, showInactive: boolean, showEnded: boolean, showNotStarted: boolean, filterOnAnnualLeaveAgreement: boolean) => `V2/Time/Employee/EmployeesForGridDict/${encodeURIComponent(dateFrom)}/${encodeURIComponent(dateTo)}/${encodeURIComponent(employeeIds)}/${showInactive}/${showEnded}/${showNotStarted}/${filterOnAnnualLeaveAgreement}`;

//post, takes args: (model: number)
export const getEmployeeAvailability = () => `V2/Time/Employee/Availability`;

//post, takes args: (model: number)
export const getAvailableEmployees = () => `V2/Time/Employee/AvailableEmployees/`;

//get
export const getEmployeeChildsDict = (employeeId: number, addEmptyRow: boolean) => `V2/Time/Employee/EmployeeChildsDict/${employeeId}/${addEmptyRow}`;

//get
export const getHiddenEmployeeId = () => `V2/Time/Employee/HiddenEmployeeId/`;

//get
export const getEmployeeChilds = (employeeId: number) => `V2/Time/Employee/EmployeeChild/${employeeId}`;

//get
export const getEmployeeChildsSmall = (employeeId: number, addEmptyRow: boolean) => `V2/Time/Employee/EmployeeChildSmall/${employeeId}/${addEmptyRow}`;


