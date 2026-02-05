import '../Module';

import { AccountDimsDirectiveFactory } from '../../../Common/Directives/accountdims/accountdimsdirective';
import { SalesBudgetValidationDirective } from './Directives/BudgetValidationDirective';
import { SalesBudgetGridDirectiveFactory } from './Directives/BudgetGridDirective';

angular.module("Soe.Economy.Accounting.SalesBudget.Module", ['Soe.Economy.Accounting'])
    .directive("salesBudgetValidation", SalesBudgetValidationDirective.create)
    .directive("salesBudgetGrid", SalesBudgetGridDirectiveFactory.create)
    .directive("accountDims", AccountDimsDirectiveFactory.create);
