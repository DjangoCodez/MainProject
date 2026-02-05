import '../Module';
import '../../Time/Module';

import { PayrollGroupAccountsDirectiveFactory } from './Directives/PayrollGroupAccounts/PayrollGroupAccountsDirective';
import { PayrollGroupPayrollProductsDirectiveFactory } from './Directives/PayrollGroupPayrollProducts/PayrollGroupPayrollProductsDirective';
import { PayrollGroupPriceFormulasDirectiveFactory } from './Directives/PayrollGroupPriceFormulas/PayrollGroupPriceFormulasDirective';
import { PayrollGroupPriceTypesDirectiveFactory } from './Directives/PayrollGroupPriceTypes/PayrollGroupPriceTypesDirective';
import { PayrollGroupReportsDirectiveFactory } from './Directives/PayrollGroupReports/PayrollGroupReportsDirective';
import { PayrollGroupVacationGroupsDirectiveFactory } from './Directives/PayrollGroupVacationGroups/PayrollGroupVacationGroupsDirective';
import { PayrollGroupValidationDirectiveFactory } from './Directives/PayrollGroupValidationDirective';

angular.module("Soe.Time.Employee.PayrollGroups.Module", ['Soe.Time.Employee', 'Soe.Time.Time'])
    .directive("payrollGroupAccounts", PayrollGroupAccountsDirectiveFactory.create)
    .directive("payrollGroupPayrollProducts", PayrollGroupPayrollProductsDirectiveFactory.create)
    .directive("payrollGroupPriceFormulas", PayrollGroupPriceFormulasDirectiveFactory.create)
    .directive("payrollGroupPriceTypes", PayrollGroupPriceTypesDirectiveFactory.create)
    .directive("payrollGroupReports", PayrollGroupReportsDirectiveFactory.create)
    .directive("payrollGroupVacationGroups", PayrollGroupVacationGroupsDirectiveFactory.create)
    .directive("payrollGroupValidation", PayrollGroupValidationDirectiveFactory.create);


