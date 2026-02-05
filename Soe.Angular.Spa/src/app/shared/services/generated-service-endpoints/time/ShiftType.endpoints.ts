


//Available methods for ShiftTypeController

//get
export const getShiftTypeGrid = (loadAccounts: boolean, loadSkills: boolean, loadEmployeeStatisticsTargets: boolean, setTimeScheduleTemplateBlockTypeName: boolean, setCategoryNames: boolean, setAccountingString: boolean, setSkillNames: boolean, setTimeScheduleTypeName: boolean, loadHierarchyAccounts: boolean, shiftTypeId?: number) => `V2/Time/Schedule/ShiftType/Grid/${loadAccounts}/${loadSkills}/${loadEmployeeStatisticsTargets}/${setTimeScheduleTemplateBlockTypeName}/${setCategoryNames}/${setAccountingString}/${setSkillNames}/${setTimeScheduleTypeName}/${loadHierarchyAccounts}/${shiftTypeId || ''}`;

//get
export const getShiftType = (shiftTypeId: number, loadAccounts: boolean, loadSkills: boolean, loadEmployeeStatisticsTargets: boolean, setEmployeeStatisticsTargetsTypeName: boolean, loadCategories: boolean, loadHierarchyAccounts: boolean) => `V2/Time/Schedule/ShiftType/${shiftTypeId}/${loadAccounts}/${loadSkills}/${loadEmployeeStatisticsTargets}/${setEmployeeStatisticsTargetsTypeName}/${loadCategories}/${loadHierarchyAccounts}`;

//get
export const getShiftTypes = (loadAccountInternals: boolean, loadAccounts: boolean, loadSkills: boolean, loadEmployeeStatisticsTargets: boolean, setTimeScheduleTemplateBlockTypeName: boolean, setCategoryNames: boolean, setAccountingString: boolean, setSkillNames: boolean, setTimeScheduleTypeName: boolean, loadHierarchyAccounts: boolean) => `V2/Time/Schedule/ShiftType/${loadAccountInternals}/${loadAccounts}/${loadSkills}/${loadEmployeeStatisticsTargets}/${setTimeScheduleTemplateBlockTypeName}/${setCategoryNames}/${setAccountingString}/${setSkillNames}/${setTimeScheduleTypeName}/${loadHierarchyAccounts}`;

//get
export const getShiftTypesDict = (addEmptyRow: boolean) => `V2/Time/Schedule/ShiftType/Dict/${addEmptyRow}`;

//get
export const getShiftTypesForUsersCategories = () => `V2/Time/Schedule/ShiftType/GetShiftTypesForUsersCategories/`;

//get
export const getShiftTypeIdsForUser = () => `V2/Time/Schedule/ShiftType/GetShiftTypeIdsForUser/`;

//post, takes args: (model: number)
export const saveShiftType = () => `V2/Time/Schedule/ShiftType`;

//delete
export const deleteShiftType = (shiftTypeId: number) => `V2/Time/Schedule/ShiftType/${shiftTypeId}`;

//delete
export const deleteShiftTypes = (shiftTypeIds: string) => `V2/Time/Schedule/ShiftType/${encodeURIComponent(shiftTypeIds)}`;


