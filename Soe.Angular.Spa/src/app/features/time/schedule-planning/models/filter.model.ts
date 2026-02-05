import { TermGroup_TimeScheduleTemplateBlockType } from '@shared/models/generated-interfaces/Enumerations';

export class SchedulePlanningFilter {
  employeeIds: number[] = [];
  showAllEmployees = false;
  shiftTypeIds: number[] = [];
  showHiddenShifts = false;
  blockTypes: TermGroup_TimeScheduleTemplateBlockType[] = [];
}
