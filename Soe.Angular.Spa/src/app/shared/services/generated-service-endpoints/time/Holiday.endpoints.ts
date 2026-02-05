


//Available methods for HolidayController

//get
export const getHolidaysGrid = (holidayId?: number) => `V2/Time/Holiday/Grid?holidayId=${holidayId}`;

//get
export const getHolidaysSmall = (dateFromString: string, dateToString: string) => `V2/Time/Holiday/Small/${encodeURIComponent(dateFromString)}/${encodeURIComponent(dateToString)}`;

//get
export const getHoliday = (holidayId: number) => `V2/Time/Holiday/${holidayId}`;

//post, takes args: (model: number)
export const saveHoliday = () => `V2/Time/Holiday`;

//delete
export const deleteHoliday = (holidayId: number) => `V2/Time/Holiday/${holidayId}`;

//get
export const getHolidayTypesDict = () => `V2/Time/Holiday/SysHolidayTypes/Dict`;

//post, takes args: (model: number)
export const onAddHoliday = () => `V2/Time/Holiday/OnAddHoliday`;

//post, takes args: (model: number)
export const onUpdateHoliday = () => `V2/Time/Holiday/OnUpdateHoliday`;

//post, takes args: (model: number)
export const onDeleteHoliday = () => `V2/Time/Holiday/OnDeleteHoliday`;


