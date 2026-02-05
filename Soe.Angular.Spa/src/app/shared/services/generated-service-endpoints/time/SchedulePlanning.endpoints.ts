


//Available methods for SchedulePlanningController

//get
export const getShiftsForDay = (employeeId: number, date: string, blockTypes: string, includeBreaks: boolean, includeGrossNetAndCost: boolean, link: string, loadQueue: boolean, loadDeviationCause: boolean, loadTasks: boolean, includePreliminary: boolean, timeScheduleScenarioHeadId: number) => `V2/Time/SchedulePlanning/Shift/${employeeId}/${encodeURIComponent(date)}/${encodeURIComponent(blockTypes)}/${includeBreaks}/${includeGrossNetAndCost}/${encodeURIComponent(link)}/${loadQueue}/${loadDeviationCause}/${loadTasks}/${includePreliminary}/${timeScheduleScenarioHeadId}`;

//get
export const getLinkedShifts = (timeScheduleTemplateBlockId: number) => `V2/Time/SchedulePlanning/LinkedShifts/${timeScheduleTemplateBlockId}`;

//post, takes args: (model: number)
export const getShifts = () => `V2/Time/SchedulePlanning/Shift/Search`;

//post, takes args: (model: number)
export const saveShifts = () => `V2/Time/SchedulePlanning/Shift`;

//post, takes args: (model: number)
export const deleteShifts = () => `V2/Time/SchedulePlanning/Shift/DeleteShifts`;

//post, takes args: (model: number)
export const dragShift = () => `V2/Time/SchedulePlanning/Shift/Drag`;

//post, takes args: (model: number)
export const dragShifts = () => `V2/Time/SchedulePlanning/Shift/DragMultiple`;

//post, takes args: (model: number)
export const splitShift = () => `V2/Time/SchedulePlanning/Shift/Split`;

//get
export const getShiftAccountingRows = (timeScheduleTemplateBlockIds: string) => `V2/Time/SchedulePlanning/ShiftAccounting/${encodeURIComponent(timeScheduleTemplateBlockIds)}`;

//get
export const getShiftHistory = (timeScheduleTemplateBlockIds: string) => `V2/Time/SchedulePlanning/ShiftHistory/${encodeURIComponent(timeScheduleTemplateBlockIds)}`;

//get
export const getShiftRequestStatus = (timeScheduleTemplateBlockId: number) => `V2/Time/SchedulePlanning/ShiftRequest/Status/${timeScheduleTemplateBlockId}`;

//get
export const checkIfTooEarlyToSend = (startTime: string) => `V2/Time/SchedulePlanning/ShiftRequest/CheckIfTooEarlyToSend/${encodeURIComponent(startTime)}`;

//delete
export const removeRecipientFromShiftRequest = (timeScheduleTemplateBlockId: number, userId: number) => `V2/Time/SchedulePlanning/ShiftRequest/${timeScheduleTemplateBlockId}/${userId}`;

//delete
export const undoShiftRequest = (timeScheduleTemplateBlockId: number) => `V2/Time/SchedulePlanning/ShiftRequest/${timeScheduleTemplateBlockId}`;

//post, takes args: (model: number)
export const evaluateDragShiftAgainstWorkRules = () => `V2/Time/SchedulePlanning/EvaluateWorkRule/Drag`;

//post, takes args: (model: number)
export const evaluateDragShiftsAgainstWorkRules = () => `V2/Time/SchedulePlanning/EvaluateWorkRule/DragMultiple`;

//post, takes args: (model: number)
export const evaluatePlannedShiftsAgainstWorkRules = () => `V2/Time/SchedulePlanning/EvaluateWorkRule/Planned`;

//post, takes args: (model: number)
export const evaluateSplitShiftAgainstWorkRules = () => `V2/Time/SchedulePlanning/EvaluateWorkRule/Split`;


