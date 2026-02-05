


//Available methods for SkillController

//get
export const getSkillsGrid = (skillId?: number) => `V2/Time/Skill/Grid/${skillId || ''}`;

//get
export const getSkillsDict = (addEmptyRow: boolean) => `V2/Time/Skill/Dict/${addEmptyRow}`;

//get
export const getSkill = (skillId: number) => `V2/Time/Skill/${skillId}`;

//post, takes args: (model: number)
export const saveSkill = () => `V2/Time/Skill`;

//delete
export const deleteSkill = (skillId: number) => `V2/Time/Skill/${skillId}`;

//get
export const getSkillTypesGrid = (skillTypeId?: number) => `V2/Time/Skill/SkillType/Grid/${skillTypeId || ''}`;

//get
export const getSkillTypesDict = (addEmptyRow: boolean) => `V2/Time/Skill/SkillType/Dict/${addEmptyRow}`;

//get
export const getSkillType = (skillTypeId: number) => `V2/Time/Skill/SkillType/${skillTypeId}`;

//post, takes args: (model: number)
export const saveSkillType = () => `V2/Time/Skill/SkillType`;

//post, takes args: (model: number)
export const updateSkillTypesState = () => `V2/Time/Skill/SkillType/UpdateState`;

//delete
export const deleteSkillType = (skillTypeId: number) => `V2/Time/Skill/SkillType/${skillTypeId}`;

//get
export const getEmployeeSkills = (employeeId: number) => `V2/Time/Skill/Employee/${employeeId}`;

//get
export const employeeHasShiftTypeSkills = (employeeId: number, shiftTypeId: number, date: string) => `V2/Time/Skill/Employee/${employeeId}/${shiftTypeId}/${encodeURIComponent(date)}`;

//get
export const matchEmployeesByShiftTypeSkills = (shiftTypeId: number) => `V2/Time/Skill/MatchEmployees/${shiftTypeId}`;

//get
export const getEmployeePostSkills = (employeePostId: number) => `V2/Time/Skill/EmployeePost/${employeePostId}`;

//get
export const employeePostHasShiftTypeSkills = (employeePostId: number, shiftTypeId: number, date: string) => `V2/Time/Skill/EmployeePost/${employeePostId}/${shiftTypeId}/${encodeURIComponent(date)}`;


