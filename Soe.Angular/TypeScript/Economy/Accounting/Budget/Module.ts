import '../Module';

import { AccountDimsDirectiveFactory } from '../../../Common/Directives/accountdims/accountdimsdirective';

angular.module("Soe.Economy.Accounting.Budget.Module", ['Soe.Economy.Accounting'])
    .directive("accountDims", AccountDimsDirectiveFactory.create);
