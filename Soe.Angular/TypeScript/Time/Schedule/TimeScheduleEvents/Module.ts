import '../Module';

import { ReceiversListDirectiveFactory } from "../../../Common/Directives/ReceiversList/ReceiversListDirective";
import { AvailableEmployeesDirectiveFactory } from '../../../Common/Directives/AvailableEmployees/AvailableEmployeesDirective';
import { TimeScheduleEventValidationDirectiveFactory } from "./TimeScheduleEventValidationDirective";
import { ScheduleService as SharedScheduleService } from '../../../Shared/Time/Schedule/ScheduleService';

angular.module("Soe.Time.Schedule.TimeScheduleEvents.Module", ['Soe.Time.Schedule'])
    .service("sharedScheduleService", SharedScheduleService)
    .directive("receiversList", ReceiversListDirectiveFactory.create)
    .directive("availableEmployees", AvailableEmployeesDirectiveFactory.create)
    .directive("timeScheduleEventValidation", TimeScheduleEventValidationDirectiveFactory.create);
