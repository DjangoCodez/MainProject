import '../Module';
import '../../../Common/Dialogs/SelectEmployees/Module';

import { EmployeeService } from '../../Employee/EmployeeService';
import { ScheduleService } from '../ScheduleService';
import { EmployeeService as SharedEmployeeService } from '../../../Shared/Time/Employee/EmployeeService';
import { ScheduleService as SharedScheduleService } from "../../../Shared/Time/Schedule/Scheduleservice";
import { TimeService } from '../../Time/timeservice';
import { TemplateGroupsValidationDirectiveFactory } from './TemplateGroupsValidationDirective';

angular.module("Soe.Time.Schedule.TemplateGroups.Module", ['Soe.Time.Schedule', 'Soe.Common.Dialogs.SelectEmployees.Module'])
    .service("employeeService", EmployeeService)
    .service("scheduleService", ScheduleService)
    .service("sharedEmployeeService", SharedEmployeeService)
    .service("sharedScheduleService", SharedScheduleService)
    .service("timeService", TimeService)
    .directive("templateGroupsValidation", TemplateGroupsValidationDirectiveFactory.create);
