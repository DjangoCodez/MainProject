import '../../Module';

import { ScheduleService } from '../ScheduleService';
import { TimeService } from "../../Time/TimeService";
import { TimeService as SharedTimeService } from '../../../Shared/Time/Time/TimeService';
import { ScheduleService as SharedScheduleService } from "../../../Shared/Time/Schedule/ScheduleService";
import { ActivateValidationDirectiveFactory } from './ActivateValidationDirective';

angular.module("Soe.Time.Schedule.Activate.Module", ['Soe.Time'])
    .service("scheduleService", ScheduleService)
    .service("sharedScheduleService", SharedScheduleService)
    .service("sharedTimeService", SharedTimeService)
    .service("timeService", TimeService)
    .directive("activateValidation", ActivateValidationDirectiveFactory.create);
