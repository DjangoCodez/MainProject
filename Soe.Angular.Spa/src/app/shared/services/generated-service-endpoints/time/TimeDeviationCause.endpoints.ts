


//Available methods for TimeDeviationCauseController

//get
export const getTimeDeviationCausesGrid = (timeDeviationCauseId?: number) => `V2/Time/TimeDeviationCause/Grid/${timeDeviationCauseId || ''}`;

//get
export const getTimeDeviationCause = (timeDeviationCauseId: number) => `V2/Time/TimeDeviationCause/${timeDeviationCauseId}`;

//post, takes args: (model: number)
export const saveTimeDeviationCauses = () => `V2/Time/TimeDeviationCause`;

//delete
export const deleteTimeDeviationCause = (timeDeviationCauseId: number) => `V2/Time/TimeDeviationCause/${timeDeviationCauseId}`;

//get
export const getTimeDeviationCausesDict = (addEmptyRow: boolean, removeAbsence: boolean, removePresence: boolean) => `V2/Time/TimeDeviationCause/Dict/${addEmptyRow}/${removeAbsence}/${removePresence}`;

//get
export const getTimeDeviationCausesAbsenceDict = (addEmptyRow: boolean) => `V2/Time/TimeDeviationCause/Dict/Absence/${addEmptyRow}`;

//get
export const getTimeDeviationCauses = (employeeGroupId: number, getEmployeeGroups: boolean, onlyUseInTimeTerminal: boolean) => `V2/Time/TimeDeviationCause/${employeeGroupId}/${getEmployeeGroups}/${onlyUseInTimeTerminal}`;

//get
export const getTimeDeviationCausesAbsenceFromEmployeeId = (employeeId: number, date: string, onlyUseInTimeTerminal: boolean) => `V2/Time/TimeDeviationCause/FromEmployeeId/Absence/${employeeId}/${encodeURIComponent(date)}/${onlyUseInTimeTerminal}`;


