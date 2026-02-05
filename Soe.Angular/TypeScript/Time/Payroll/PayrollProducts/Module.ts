import '../Module';

import { AccountingSettingsDirectiveFactory } from '../../../Common/Directives/AccountingSettings/AccountingSettingsDirective';
import { PayrollProductSettingsDirectiveFactory } from './Directives/PayrollProductSettings/PayrollProductSettingsDirective';
import { PayrollProductPriceTypesAndFormulasDirectiveFactory } from './Directives/PayrollProductPriceTypesAndFormulas/PayrollProductPriceTypesAndFormulasDirective';
import { ExtraFieldRecordsDirectiveFactory } from '../../../Common/Directives/ExtraFields/ExtraFieldsDirective';

angular.module("Soe.Time.Payroll.PayrollProducts.Module", ['Soe.Time.Payroll'])
    .directive("accountingSettings", AccountingSettingsDirectiveFactory.create)
    .directive("payrollProductSettings", PayrollProductSettingsDirectiveFactory.create)
    .directive("payrollProductPriceTypesAndFormulas", PayrollProductPriceTypesAndFormulasDirectiveFactory.create)
    .directive("extraFields", ExtraFieldRecordsDirectiveFactory.create)
