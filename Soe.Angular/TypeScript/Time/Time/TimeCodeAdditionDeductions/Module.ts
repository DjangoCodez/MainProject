import '../Module';

import { TimeCodePayrollProductsDirectiveFactory } from '../../Directives/TimeCodePayrollProducts/TimeCodePayrollProductsDirective';
import { TimeCodeInvoiceProductsDirectiveFactory } from '../../Directives/TimeCodeInvoiceProducts/TimeCodeInvoiceProductsDirective';

angular.module("Soe.Time.Time.TimeCodeAdditionDeductions.Module", ['Soe.Time.Time'])
    .directive("timeCodePayrollProducts", TimeCodePayrollProductsDirectiveFactory.create)
    .directive("timeCodeInvoiceProducts", TimeCodeInvoiceProductsDirectiveFactory.create);
