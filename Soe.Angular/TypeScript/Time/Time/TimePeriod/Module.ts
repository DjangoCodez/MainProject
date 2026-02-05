import '../Module';

import { TimePeriodsDirectiveFactory } from "./Directives/timeperiodsdirective";
import { TimePeriodValidationDirectiveFactory } from "./Directives/TimePeriodValidationDirective";

angular.module("Soe.Time.Time.TimePeriod.Module", ['Soe.Time.Time'])
    .directive("timePeriods", TimePeriodsDirectiveFactory.create)
    .directive("timePeriodValidation", TimePeriodValidationDirectiveFactory.create)
