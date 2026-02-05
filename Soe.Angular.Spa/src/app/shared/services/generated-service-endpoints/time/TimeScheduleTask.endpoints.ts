


//Available methods for TimeScheduleTaskController

//get
export const getTimeScheduleTasksGrid = (timeScheduleTaskId?: number) => `V2/Time/TimeScheduleTask/Grid?timeScheduleTaskId=${timeScheduleTaskId}`;

//get
export const getTimeScheduleTasksDict = (addEmptyRow: boolean) => `V2/Time/TimeScheduleTask/Dict/${addEmptyRow}`;

//get
export const getTimeScheduleTask = (timeScheduleTaskId: number, loadAccounts: boolean, loadExcludedDates: boolean, loadAccountHierarchyAccount: boolean) => `V2/Time/TimeScheduleTask/${timeScheduleTaskId}/${loadAccounts}/${loadExcludedDates}/${loadAccountHierarchyAccount}`;

//post, takes args: (timeScheduleTask: number)
export const saveTimeScheduleTask = () => `V2/Time/TimeScheduleTask`;

//delete
export const deleteTimeScheduleTask = (timeScheduleTaskId: number) => `V2/Time/TimeScheduleTask/${timeScheduleTaskId}`;

//get
export const getTimeScheduleTaskTypesGrid = (timeScheduleTaskTypeId?: number) => `V2/Time/TimeScheduleTaskType/Grid?timeScheduleTaskTypeId=${timeScheduleTaskTypeId}`;

//get
export const getTimeScheduleTaskTypesDict = (addEmptyRow: boolean) => `V2/Time/TimeScheduleTaskType/Dict/${addEmptyRow}`;

//get
export const getTimeScheduleTaskType = (timeScheduleTaskTypeId: number) => `V2/Time/TimeScheduleTaskType/${timeScheduleTaskTypeId}`;

//post, takes args: (timeScheduleTaskType: number)
export const saveTimeScheduleTaskType = () => `V2/Time/TimeScheduleTaskType`;

//delete
export const deleteTimeScheduleTaskType = (timeScheduleTaskTypeId: number) => `V2/Time/TimeScheduleTaskType/${timeScheduleTaskTypeId}`;

//get
export const getRecurrenceDescription = (pattern: string) => `V2/Time/Recurrence/Description/${encodeURIComponent(pattern)}`;


