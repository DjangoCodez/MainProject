


//Available methods for TimeScheduleEventController

//get
export const getTimeScheduleEventsGrid = (timeScheduleEventId?: number) => `V2/Time/Schedule/TimeScheduleEvent/Grid/${timeScheduleEventId || ''}`;

//get
export const getTimeScheduleEventsDict = (addEmptyRow: boolean) => `V2/Time/Schedule/TimeScheduleEvent/Dict/${addEmptyRow}`;

//get
export const getTimeScheduleEvent = (timeScheduleEventId: number) => `V2/Time/Schedule/TimeScheduleEvent/TimeScheduleEvent/${timeScheduleEventId}`;

//post, takes args: (timeScheduleEvent: number)
export const saveTimeScheduleEvent = () => `V2/Time/Schedule/TimeScheduleEvent/TimeScheduleEvent`;

//delete
export const deleteTimeScheduleEvent = (timeScheduleEventId: number) => `V2/Time/Schedule/TimeScheduleEvent/TimeScheduleEvent/${timeScheduleEventId}`;


