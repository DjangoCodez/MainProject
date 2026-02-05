import '../Module';

import { IncomingDeliveryRowsValidationDirectiveFactory } from "./IncomingDeliveryRowsValidationDirective";

angular.module("Soe.Time.Schedule.IncomingDeliveries.Module", ['Soe.Time.Schedule'])
    .directive("incomingDeliveryRowsValidation", IncomingDeliveryRowsValidationDirectiveFactory.create);
