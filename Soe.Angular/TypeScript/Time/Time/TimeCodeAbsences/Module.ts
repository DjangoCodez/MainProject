import '../Module';
import '../../Schedule/Module';
import { TimeCodePayrollProductsDirectiveFactory } from '../../Directives/TimeCodePayrollProducts/TimeCodePayrollProductsDirective';
import { TimeCodeInvoiceProductsDirectiveFactory } from '../../Directives/TimeCodeInvoiceProducts/TimeCodeInvoiceProductsDirective';

angular.module("Soe.Time.Time.TimeCodeAbsences.Module", ['Soe.Time.Time', 'Soe.Time.Schedule'])
    .directive("timeCodePayrollProducts", TimeCodePayrollProductsDirectiveFactory.create)
    .directive("timeCodeInvoiceProducts", TimeCodeInvoiceProductsDirectiveFactory.create);
