
import { EmployeeService as SharedEmployeeService } from "../../Employee/EmployeeService";
import { ScheduleService as SharedScheduleService } from "../ScheduleService";
import { TimeService as SharedTimeService } from "../../Time/TimeService";
import { EmployeeVacationSummaryDirectiveFactory } from "../../Directives/EmployeeVacationSummary/EmployeeVacationSummaryDirective";

var module = angular.module("Soe.Shared.Time.Schedule.Absencerequests.Module", [])    
    .service("sharedEmployeeService", SharedEmployeeService)        
    .service("sharedTimeService", SharedTimeService)
    .service("sharedScheduleService", SharedScheduleService)
    .directive("employeeVacationSummary", EmployeeVacationSummaryDirectiveFactory.create);

export default module;
