import '../Module';

import { EmployeeService as SharedEmployeeService } from "../../../Shared/Time/Employee/EmployeeService";
import { ScheduleService } from "../../Schedule/ScheduleService";

angular.module("Soe.Time.Time.TimeCalendar.Module", ['Soe.Time.Time'])
    .service("sharedEmployeeService", SharedEmployeeService)
    .service("scheduleService", ScheduleService);
