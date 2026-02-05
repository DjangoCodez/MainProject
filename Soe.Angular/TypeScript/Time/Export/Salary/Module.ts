import '../../Module';
import { TimeService } from '../../Time/TimeService';
import { TimeService as SharedTimeService } from "../../../Shared/Time/Time/TimeService";
angular.module("Soe.Time.Export.Salary.Module", ['Soe.Time'])
    .service("timeService", TimeService)
    .service("sharedTimeService", SharedTimeService)
