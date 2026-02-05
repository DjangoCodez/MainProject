


//Available methods for DayTypeController

//get
export const getDayTypesGrid = (dayTypeId?: number) => `V2/Time/DayType/Grid/${dayTypeId || ''}`;

//get
export const getDayTypesDict = (addEmptyRow: boolean) => `V2/Time/DayType/Dict/${addEmptyRow}`;

//get
export const getDayType = (dayTypeId: number) => `V2/Time/DayType/${dayTypeId}`;

//get
export const getDayTypesAndWeekdays = () => `V2/Time/DayType/DayTypeAndWeekday`;

//post, takes args: (model: number)
export const saveDayType = () => `V2/Time/DayType`;

//delete
export const deleteDayType = (dayTypeId: number) => `V2/Time/DayType/${dayTypeId}`;

//get
export const getDayTypesByCompanyDict = (addEmptyRow: boolean, onlyHolidaySalary: boolean) => `V2/Time/DayType/GetDayTypesByCompanyDict/${addEmptyRow}/${onlyHolidaySalary}`;

//get
export const getDaysOfWeekDict = (addEmptyRow: boolean) => `V2/Time/DayType/DayOfWeek/Dict/${addEmptyRow}`;


