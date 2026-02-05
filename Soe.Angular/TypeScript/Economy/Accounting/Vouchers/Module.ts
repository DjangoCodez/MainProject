import '../Module';
import '../../Accounting/Module'

import { VoucherValidationDirectiveFactory } from "./VoucherValidationDirective";
import { DistributionRowsDirectiveFactory } from "../../../Common/Directives/DistributionRows/DistributionRowsDirective";
//import { traceRowsDirective } from "../../../Common/Directives/TraceRows/TraceRows";

angular.module("Soe.Economy.Accounting.Vouchers.Module", ['Soe.Economy.Accounting'])
    .directive("voucherValidation", VoucherValidationDirectiveFactory.create);
    //.directive("traceRows", traceRowsDirective);