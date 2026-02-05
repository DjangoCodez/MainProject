


//Available methods for TimeEarnedDaysController

//post, takes args: (model: number)
export const loadEarnedHolidaysContent = () => `V2/Time/EarnedHoliday/Load/`;

//post, takes args: (model: number)
export const createTransactionsForEarnedHoliday = () => `V2/Time/EarnedHoliday/CreateTransactions/`;

//post, takes args: (model: number)
export const deleteTransactionsForEarnedHolidayContent = () => `V2/Time/EarnedHoliday/DeleteTransactions/`;

//get
export const getYears = (yearsBack: number) => `V2/Time/EarnedHoliday/Years/${yearsBack}`;

//get
export const getHolidays = (year: number, onlyRedDay: boolean, onlyHistorical: boolean) => `V2/Time/EarnedHoliday/Holidays/${year}/${onlyRedDay}/${onlyHistorical}`;


