


//Available methods for EmployeeScheduleController

//get
export const getPlacementsForGrid = (onlyLatest: boolean, addEmptyPlacement: boolean) => `V2/Time/EmployeeSchedule/Grid/${onlyLatest}/${addEmptyPlacement}`;

//post, takes args: (model: number)
export const controlActivations = () => `V2/Time/EmployeeSchedule/ControlActivations`;

//post, takes args: (model: number)
export const controlActivation = () => `V2/Time/EmployeeSchedule/ControlActivation`;

//post, takes args: (model: number)
export const employeeSchedule = () => `V2/Time/EmployeeSchedule/EmployeeSchedule/Activate`;

//post, takes args: (model: number)
export const deletePlacement = () => `V2/Time/EmployeeSchedule/Delete`;

//get
export const getTimeScheduleTemplateHeadsForActivate = () => `V2/Time/EmployeeSchedule/TimeScheduleTemplateHead/Activate/`;

//get
export const getTimeScheduleTemplatePeriodsForActivate = (timeScheduleTemplateHeadId: number) => `V2/Time/EmployeeSchedule/TimeScheduleTemplatePeriod/Activate/${timeScheduleTemplateHeadId}`;


