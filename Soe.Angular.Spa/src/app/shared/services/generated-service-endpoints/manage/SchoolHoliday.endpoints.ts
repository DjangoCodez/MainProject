


//Available methods for SchoolHolidayController

//get
export const getSchoolHolidaysGrid = (schoolHolidayId?: number) => `V2/Manage/Calendar/SchoolHoliday/Grid/${schoolHolidayId || ''}`;

//get
export const getSchoolHoliday = (schoolHolidayId: number) => `V2/Manage/Calendar/SchoolHoliday/${schoolHolidayId}`;

//post, takes args: (schoolHolidayDTO: number)
export const saveSchoolHoliday = () => `V2/Manage/Calendar/SchoolHoliday`;

//delete
export const deleteSchoolHoliday = (schoolHolidayId: number) => `V2/Manage/Calendar/SchoolHoliday/${schoolHolidayId}`;


