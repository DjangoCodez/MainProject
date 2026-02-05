import '../Module';
import '../../../Shared/Economy/Module';

import { CommonCustomerService } from "../../../Common/Customer/CommonCustomerService";
//import { traceRowsDirective } from "../../../Common/Directives/TraceRows/TraceRows";

angular.module("Soe.Economy.Import.Payments.Module", ['Soe.Shared.Economy', 'Soe.Economy.Import'])
    .service("commonCustomerService", CommonCustomerService)
    //.directive("traceRows", traceRowsDirective);