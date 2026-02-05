import '../Module';

import { EmployeeAccountsDirectiveFactory } from '../../../Common/Directives/EmployeeAccounts/EmployeeAccountsDirective';
import { AccountsForDimDirectiveFactory } from '../../../Common/Directives/AccountsForDim/AccountsForDimDirective';
import { CategoriesDirectiveFactory } from '../../../Common/Directives/Categories/CategoriesDirective';
import { TimeStampAdditionsDirectiveFactory } from './Directives/TimeStampAdditions/TimeStampAdditionsDirective';

angular.module("Soe.Time.Time.TimeTerminals.Module", ['Soe.Time.Time'])
    .directive("employeeAccounts", EmployeeAccountsDirectiveFactory.create)
    .directive("accountsForDim", AccountsForDimDirectiveFactory.create)
    .directive("categories", CategoriesDirectiveFactory.create)
    .directive("timeStampAdditions", TimeStampAdditionsDirectiveFactory.create);
