import '../Module';

import { ScheduleService } from "./ScheduleService";
import { ScheduleService as SharedScheduleService } from "../../Shared/Time/Schedule/ScheduleService";
import { TimeService } from '../Time/TimeService';

angular.module("Soe.Time.Schedule", ['Soe.Time'])
    .service("timeService", TimeService)
    .service("scheduleService", ScheduleService)
    .service("sharedScheduleService", SharedScheduleService);

