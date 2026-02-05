


//Available methods for TimeBreakTemplateController

//get
export const getTimeBreakTemplatesGrid = (timeBreakTemplateId?: number) => `V2/Time/BreakTemplate/Grid/${timeBreakTemplateId || ''}`;

//get
export const getTimeBreakTemplate = (timeBreakTemplateId: number) => `V2/Time/BreakTemplate/${timeBreakTemplateId}`;

//post, takes args: (model: number)
export const validateTimeBreakTemplate = () => `V2/Time/BreakTemplate/Validate`;

//post, takes args: (model: number)
export const saveTimeBreakTemplate = () => `V2/Time/BreakTemplate`;

//delete
export const deleteTimeBreakTemplate = (timeBreakTemplateId: number) => `V2/Time/BreakTemplate/${timeBreakTemplateId}`;


