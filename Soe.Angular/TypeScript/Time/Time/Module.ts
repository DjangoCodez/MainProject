import '../Module';

import { TimeService } from "./TimeService";
import { TimeService as SharedTimeService } from "../../Shared/Time/Time/TimeService";

angular.module("Soe.Time.Time", ['Soe.Time'])
    .service("timeService", TimeService)
    .service("sharedTimeService", SharedTimeService);

