import '../Module';

import { TimeScheduleTaskValidationDirectiveFactory } from "./TimeScheduleTaskValidationDirective";

angular.module("Soe.Time.Schedule.TimeScheduleTasks.Module", ['Soe.Time.Schedule'])
    .directive("timeScheduleTaskValidation", TimeScheduleTaskValidationDirectiveFactory.create);
