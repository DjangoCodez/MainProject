


//Available methods for ScheduleCycleController

//get
export const getScheduleCycleRuleTypesGrid = (scheduleCycleRuleTypeId?: number) => `V2/Time/Schedule/ScheduleCycleRuleType/Grid/${scheduleCycleRuleTypeId || ''}`;

//get
export const getScheduleCycleRuleType = (scheduleCycleRuleTypeId: number) => `V2/Time/Schedule/ScheduleCycleRuleType/${scheduleCycleRuleTypeId}`;

//post, takes args: (model: number)
export const saveScheduleCycleRuleType = () => `V2/Time/Schedule/ScheduleCycleRuleType`;

//delete
export const deleteScheduleCycleRuleType = (scheduleCycleRuleTypeId: number) => `V2/Time/Schedule/ScheduleCycleRuleType/${scheduleCycleRuleTypeId}`;

//get
export const getScheduleCycleRuleTypesDict = (addEmptyRow: boolean) => `V2/Time/Schedule/ScheduleCycleRuleType/Dict/${addEmptyRow}`;

//get
export const getScheduleCyclesGrid = (scheduleCycleId?: number) => `V2/Time/Schedule/ScheduleCycle/Grid/${scheduleCycleId || ''}`;

//get
export const getScheduleCyclesDict = (addEmptyRow: boolean) => `V2/Time/Schedule/ScheduleCycle/Dict/${addEmptyRow}`;

//get
export const getScheduleCycle = (scheduleCycleId: number) => `V2/Time/Schedule/ScheduleCycle/${scheduleCycleId}`;

//post, takes args: (scheduleCycle: number)
export const saveScheduleCycle = () => `V2/Time/Schedule/ScheduleCycle`;

//delete
export const deleteScheduleCycle = (scheduleCycleId: number) => `V2/Time/Schedule/ScheduleCycle/${scheduleCycleId}`;


