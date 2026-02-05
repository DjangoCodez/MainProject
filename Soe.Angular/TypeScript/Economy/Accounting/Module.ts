import '../Module';

import { BudgetValidationDirective } from "../../Economy/Accounting/Budget/Directives/BudgetValidationDirective";
import { BudgetGridDirectiveFactory } from "../../Economy/Accounting/Budget/Directives/BudgetGridDirective";
import { YearValidationDirective } from './AccountYear/AccountYearValidationDirective';

angular.module("Soe.Economy.Accounting", ['Soe.Economy', 'Soe.Economy.Common.Module'])
    .directive("budgetValidation", BudgetValidationDirective.create)
    .directive("budgetGrid", BudgetGridDirectiveFactory.create)
    .directive("yearValidation", YearValidationDirective.create);

