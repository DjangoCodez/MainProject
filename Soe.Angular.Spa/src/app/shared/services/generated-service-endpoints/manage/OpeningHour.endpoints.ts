


//Available methods for OpeningHourController

//get
export const getOpeningHoursDict = (addEmptyRow: boolean, includeDateInName: boolean) => `V2/Manage/Registry/OpeningHours/Dict?addEmptyRow=${addEmptyRow}&includeDateInName=${includeDateInName}`;

//get
export const getOpeningHours = (fromDate: string, toDate: string, openingHoursId?: number) => `V2/Manage/Registry/OpeningHours?fromDate=${encodeURIComponent(fromDate)}&toDate=${encodeURIComponent(toDate)}&openingHoursId=${openingHoursId}`;

//get
export const getOpeningHour = (openingHoursId: number) => `V2/Manage/Registry/OpeningHours/${openingHoursId}`;

//post, takes args: (openingHoursDTO: number)
export const saveOpeningHours = () => `V2/Manage/Registry/OpeningHours`;

//delete
export const deleteOpeningHours = (openingHoursId: number) => `V2/Manage/Registry/OpeningHours/${openingHoursId}`;


