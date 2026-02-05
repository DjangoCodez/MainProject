


//Available methods for EmployeeGroupController

//get
export const getEmployeeGroup = (employeeGroupId: number, loadTimeDeviationCauseTimeCode: boolean, loadDayTypes: boolean, loadTimeAccumulators: boolean, loadTimeDeviationCauseRequests: boolean, loadTimeDeviationCauseAbsenceAnnouncements: boolean, loadLinkedTimeCodes: boolean, loadTimeDeviationCauses: boolean, loadTimeStampRounding: boolean, loadAttestTransitions: boolean, loadRuleWorkTimePeriod: boolean, loadStdAccounts: boolean, loadExternalCode: boolean) => `V2/Time/EmployeeGroup/${employeeGroupId}/${loadTimeDeviationCauseTimeCode}/${loadDayTypes}/${loadTimeAccumulators}/${loadTimeDeviationCauseRequests}/${loadTimeDeviationCauseAbsenceAnnouncements}/${loadLinkedTimeCodes}/${loadTimeDeviationCauses}/${loadTimeStampRounding}/${loadAttestTransitions}/${loadRuleWorkTimePeriod}/${loadStdAccounts}/${loadExternalCode}`;

//get
export const getEmployeeGroupsGrid = (employeeGroupId?: number) => `V2/Time/EmployeeGroup/Grid/${employeeGroupId || ''}`;

//post, takes args: (model: number)
export const saveEmployeeGroup = () => `V2/Time/EmployeeGroup`;

//delete
export const deleteEmployeeGroup = (employeeGroupId: number) => `V2/Time/EmployeeGroup/${employeeGroupId}`;

//get
export const getEmployeeGroupsDict = (addEmptyRow: boolean) => `V2/Time/EmployeeGroup/Dict/${addEmptyRow}`;

//get
export const getEmployeeGroupsDictSmall = () => `V2/Time/EmployeeGroup/DictSmall`;

//get
export const getEmployeeGroupId = (employeeId: number, dateString: string) => `V2/Time/EmployeeGroup/${employeeId}/${encodeURIComponent(dateString)}`;

//get
export const getTimeAccumulatorsDict = (addEmptyRow: boolean, includeVacationBalance: boolean, includeTimeAccountBalance: boolean) => `V2/Time/EmployeeGroup/TimeAccumulatorDict/${addEmptyRow}/${includeVacationBalance}/${includeTimeAccountBalance}`;


