


//Available methods for VacationGroupController

//get
export const getVacationGroups = (loadTypeNames: boolean, loadOnlyActive: boolean) => `V2/Time/Employee/VacationGroup/${loadTypeNames}/${loadOnlyActive}`;

//get
export const getVacationGroup = (vacationGroupId: number) => `V2/Time/Employee/VacationGroup/${vacationGroupId}`;

//get
export const getVacationGroupForEmployee = (employeeId: number, dateString: string) => `V2/Time/Employee/VacationGroup/Employee/${employeeId}/${encodeURIComponent(dateString)}`;

//delete
export const deleteVacationGroup = (vacationGroupId: number) => `V2/Time/Employee/DeleteVacationGroup/${vacationGroupId}`;

//post, takes args: (vacationGroup: number)
export const saveVacationGroup = () => `V2/Time/Employee/VacationGroup`;

//get
export const getVacationGroupsDict = (addEmptyRow: boolean) => `V2/Time/Employee/VacationGroup/Dict/${addEmptyRow}`;

//get
export const getVacationGroupEndDates = (vacationGroupIds: number[]) => `V2/Time/Employee/VacationGroup/EndDate?vacationGroupIds=${vacationGroupIds}`;


