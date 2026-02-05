


//Available methods for TimeCodeBreakGroupController

//get
export const getTimeCodeBreakGroupsGrid = (timeCodeBreakId?: number) => `V2/Time/TimeCodeBreakGroup/Grid?timeCodeBreakId=${timeCodeBreakId}`;

//get
export const getTimeCodeBreakGroup = (timeCodeBreakGroupId: number) => `V2/Time/TimeCodeBreakGroup/${timeCodeBreakGroupId}`;

//post, takes args: (model: number)
export const saveTimeCodeBreakGroup = () => `V2/Time/TimeCodeBreakGroup`;

//delete
export const deleteTimeCodeBreakGroup = (timeCodeBreakGroupId: number) => `V2/Time/TimeCodeBreakGroup/${timeCodeBreakGroupId}`;


