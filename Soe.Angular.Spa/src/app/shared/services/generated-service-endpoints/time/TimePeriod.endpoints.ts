


//Available methods for TimePeriodController

//get
export const getTimePeriodHeadsDict = (type: number, accountId: number, addEmptyRow: boolean) => `V2/Time/TimePeriod/TimePeriodHeadsDict/${type}/${accountId}/${addEmptyRow}`;

//get
export const getPlanningPeriodGrid = (timePeriodHeadId?: number) => `V2/Time/TimePeriod/PlanningPeriod/Grid/?timePeriodHeadId=${timePeriodHeadId}`;

//get
export const getTimePeriodHead = (timePeriodHeadId: number, loadPeriods: boolean) => `V2/Time/TimePeriod/TimePeriodHead/?timePeriodHeadId=${timePeriodHeadId}&loadPeriods=${loadPeriods}`;

//post, takes args: (model: number)
export const saveTimePeriod = () => `V2/Time/TimePeriod/TimePeriodHead/`;

//delete
export const deleteTimePeriodHead = (timePeriodHeadId: number, removePeriodLinks: boolean) => `V2/Time/TimePeriod/TimePeriodHead/${timePeriodHeadId}/${removePeriodLinks}`;

//get
export const getTimePeriodHeadsIncludingPeriodsForType = (type: number) => `V2/Time/TimePeriod/TimePeriodHeadIncPeriods/${type}`;

//get
export const getTimePeriodsDict = (timePeriodHeadId: number, addEmptyRow: boolean) => `V2/Time/TimePeriod/TimePeriodsDict/${timePeriodHeadId}/${addEmptyRow}`;

//get
export const getDistributionRulesForGrid = (payrollProductDistributionRuleHeadId?: number) => `V2/Time/TimePeriod/PlanningPeriod/DistributionRules/Grid/${payrollProductDistributionRuleHeadId || ''}`;

//get
export const getDistributionRuleHead = (headId: number) => `V2/Time/TimePeriod/PlanningPeriod/DistributionRule/Rule/${headId}`;

//post, takes args: (model: number)
export const saveDistributionRuleHead = () => `V2/Time/TimePeriod/PlanningPeriod/DistributionRule/Save`;

//delete
export const deleteDistributionRuleHead = (headId: number) => `V2/Time/TimePeriod/PlanningPeriod/DistributionRule/Delete?headId=${headId}`;


