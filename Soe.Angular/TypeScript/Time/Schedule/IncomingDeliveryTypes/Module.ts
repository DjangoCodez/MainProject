import '../Module';

import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../Core/Services/UrlHelperService";
import { IncomingDeliveryTypesValidationDirectiveFactory } from "./IncomingDeliveryTypesValidationDirective";

angular.module("Soe.Time.Schedule.IncomingDeliveryTypes.Module", ['Soe.Time.Schedule'])
    .directive("incomingDeliveryTypesValidation", IncomingDeliveryTypesValidationDirectiveFactory.create);
