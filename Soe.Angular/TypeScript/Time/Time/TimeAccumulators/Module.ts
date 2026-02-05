import '../Module';

import { TimeCodesDirectiveFactory } from './Directives/TimeCodes/TimeCodesDirective';
import { PayrollProductsDirectiveFactory } from './Directives/PayrollProducts/PayrollProductsDirective';
import { InvoiceProductsDirectiveFactory } from './Directives/InvoiceProducts/InvoiceProductsDirective';
import { EmployeeGroupRulesDirectiveFactory } from './Directives/EmployeeGroupRules/EmployeeGroupRulesDirective';
import { TimeWorkReductionEarningGroupRulesDirectiveFactory } from './Directives/TimeWorkReductionEarningGroupRules/TimeWorkReductionEarningGroupRulesDirective';

angular.module("Soe.Time.Time.TimeAccumulators.Module", ['Soe.Time.Time'])
    .directive("timeAccumulatorTimeCodes", TimeCodesDirectiveFactory.create)
    .directive("timeAccumulatorPayrollProducts", PayrollProductsDirectiveFactory.create)
    .directive("timeAccumulatorInvoiceProducts", InvoiceProductsDirectiveFactory.create)
    .directive("timeAccumulatorEmployeeGroupRules", EmployeeGroupRulesDirectiveFactory.create)
    .directive("timeAccumulatorTimeWorkReductionEarningGroupRules", TimeWorkReductionEarningGroupRulesDirectiveFactory.create);

