import '../Module';

import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../Core/Services/UrlHelperService";
import { EmployeeService } from "../../Employee/EmployeeService";
import { EmployeeService as SharedEmployeeService } from "../../../Shared/Time/Employee/EmployeeService";
import { TimeService } from "../../Time/TimeService";
import { TimeService as SharedTimeService } from "../../../Shared/Time/Time/TimeService";
import { EmployeeVacationSummaryDirectiveFactory } from "../../../Shared/Time/Directives/EmployeeVacationSummary/EmployeeVacationSummaryDirective";

angular.module("Soe.Time.Schedule.Absencerequests.Module", ['Soe.Time.Schedule'])
    .service("employeeService", EmployeeService)    
    .service("sharedEmployeeService", SharedEmployeeService)    
    .service("timeService", TimeService)
    .service("sharedTimeService", SharedTimeService)
    .directive("employeeVacationSummary", EmployeeVacationSummaryDirectiveFactory.create);

