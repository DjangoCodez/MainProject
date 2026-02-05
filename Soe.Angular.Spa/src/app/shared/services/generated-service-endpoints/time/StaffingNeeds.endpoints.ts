


//Available methods for StaffingNeedsController

//get
export const getStaffingNeedsLocationsGrid = (locationId?: number) => `V2/Time/StaffingNeeds/StaffingNeedsLocation/Grid/${locationId || ''}`;

//get
export const getStaffingNeedsLocation = (locationId: number) => `V2/Time/StaffingNeeds/StaffingNeedsLocation/${locationId}`;

//post, takes args: (model: number)
export const saveStaffingNeedsLocation = () => `V2/Time/StaffingNeeds/StaffingNeedsLocation`;

//delete
export const deleteStaffingNeedsLocation = (locationId: number) => `V2/Time/StaffingNeeds/StaffingNeedsLocation/${locationId}`;

//get
export const getStaffingNeedsLocationGroupsGrid = (locationGroupId?: number) => `V2/Time/StaffingNeeds/StaffingNeedsLocationGroup/Grid/${locationGroupId || ''}`;

//get
export const getStaffingNeedsLocationGroupsDict = (addEmptyRow: boolean, includeAccountName: boolean) => `V2/Time/StaffingNeeds/StaffingNeedsLocationGroup/Dict/${addEmptyRow}/${includeAccountName}`;

//get
export const getStaffingNeedsLocationGroup = (locationGroupId: number) => `V2/Time/StaffingNeeds/StaffingNeedsLocationGroup/${locationGroupId}`;

//post, takes args: (model: number)
export const saveStaffingNeedsLocationGroup = () => `V2/Time/StaffingNeeds/StaffingNeedsLocationGroup`;

//delete
export const deleteStaffingNeedsLocationGroup = (locationGroupId: number) => `V2/Time/StaffingNeeds/StaffingNeedsLocationGroup/${locationGroupId}`;

//get
export const getStaffingNeedsRulesGrid = (ruleId?: number) => `V2/Time/StaffingNeeds/StaffingNeedsRule/Grid/${ruleId || ''}`;

//get
export const getStaffingNeedsRule = (ruleId: number) => `V2/Time/StaffingNeeds/StaffingNeedsRule/${ruleId}`;

//post, takes args: (model: number)
export const saveStaffingNeedsRule = () => `V2/Time/StaffingNeeds/StaffingNeedsRule`;

//delete
export const deleteStaffingNeedsRule = (ruleId: number) => `V2/Time/StaffingNeeds/StaffingNeedsRule/${ruleId}`;

//get
export const getTimeScheduleTaskGeneratedNeeds = (timeScheduleTaskId: number) => `V2/Time/StaffingNeeds/GetTimeScheduleTaskGeneratedNeeds/${timeScheduleTaskId}`;

//post, takes args: (model: number)
export const deleteGeneratedNeeds = () => `V2/Time/StaffingNeeds/DeleteGeneratedNeeds/`;


