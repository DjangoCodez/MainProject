


//Available methods for AbsenceRequestController

//get
export const getAbsenceRequest = (employeeRequestId: number) => `V2/Time/AbsenceRequest/Get/${employeeRequestId}`;

//get
export const getAbsenceRequestGrid = (employeeId: number, loadPreliminary: boolean, loadDefinitive: boolean, employeeRequestId?: number) => `V2/Time/AbsenceRequest/GetGrid/${employeeRequestId || ''}?employeeId=${employeeId}&loadPreliminary=${loadPreliminary}&loadDefinitive=${loadDefinitive}`;

//post, takes args: (model: number)
export const saveAbsenceRequest = () => `V2/Time/AbsenceRequest/Save`;

//delete
export const deleteAbsenceRequest = (employeeRequestId: number) => `V2/Time/AbsenceRequest/Delete/${employeeRequestId}`;

//get
export const getAbsenceRequestHistory = (absenceRequestId: number) => `V2/Time/AbsenceRequest/Absencerequest/History/${absenceRequestId}`;

//post, takes args: (model: number)
export const getAbsenceRequestAffectedShifts = () => `V2/Time/AbsenceRequest/Absencerequest/AffectedShifts`;

//post, takes args: (model: number)
export const getAbsenceAffectedShiftsFromShift = () => `V2/Time/AbsenceRequest/Absence/Shifts`;

//post, takes args: (model: number)
export const getAbsenceAffectedShifts = () => `V2/Time/AbsenceRequest/Absence/AffectedShifts`;

//post, takes args: (model: number)
export const getShiftsForQuickAbsence = () => `V2/Time/AbsenceRequest/Absence/ShiftsForQuickAbsence`;

//post, takes args: (model: number)
export const performAbsencePlanningAction = () => `V2/Time/AbsenceRequest/Absence/PerformPlanning`;

//post, takes args: (model: number)
export const evaluateAbsenceRequestPlannedShiftsAgainstWorkRules = () => `V2/Time/AbsenceRequest/Absence/WorkRules`;

//get
export const getEmployeesForAbsencePlanning = (dateFrom: string, dateTo: string, mandatoryEmployeeId: number, excludeCurrentUserEmployee: boolean, timeScheduleScenarioHeadId: number) => `V2/Time/AbsenceRequest/Employee/${encodeURIComponent(dateFrom)}/${encodeURIComponent(dateTo)}/${mandatoryEmployeeId}/${excludeCurrentUserEmployee}/${timeScheduleScenarioHeadId}`;


