


//Available methods for TimeScheduleTypeController

//get
export const getTimeScheduleTypesDict = (getAll: boolean, addEmptyRow: boolean) => `V2/Time/Schedule/TimeScheduleType/Dict/${getAll}/${addEmptyRow}`;

//get
export const getTimeScheduleTypes = (getAll: boolean, onlyActive: boolean, loadFactors: boolean) => `V2/Time/Schedule/TimeScheduleType/${getAll}/${onlyActive}/${loadFactors}`;

//get
export const getScheduleTypes = (getAll: boolean, onlyActive: boolean, loadFactors: boolean, loadTimeDeviationCauses: boolean) => `V2/Time/Schedule/TimeScheduleType/ScheduleType/${getAll}/${onlyActive}/${loadFactors}/${loadTimeDeviationCauses}`;

//get
export const getScheduleTypesForGrid = (timeScheduleTypeId?: number) => `V2/Time/Schedule/TimeScheduleType/ScheduleType/Grid/${timeScheduleTypeId || ''}`;

//get
export const getScheduleType = (timeScheduleTypeId: number, loadFactors: boolean) => `V2/Time/Schedule/TimeScheduleType/ScheduleType/${timeScheduleTypeId}/${loadFactors}`;

//post, takes args: (model: number)
export const saveScheduleType = () => `V2/Time/Schedule/TimeScheduleType/ScheduleType`;

//post, takes args: (model: number)
export const updateScheduleTypesState = () => `V2/Time/Schedule/TimeScheduleType/ScheduleType/UpdateState`;

//delete
export const deleteScheduleType = (timeScheduleTypeId: number) => `V2/Time/Schedule/TimeScheduleType/ScheduleType/${timeScheduleTypeId}`;


