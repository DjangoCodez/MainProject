import '../Module';
import '../../Accounting/Module'

import { AccountDimValidationDirectiveFactory } from './AccountDimValidationDirective';

angular.module("Soe.Economy.Accounting.AccountDims.Module", ['Soe.Economy.Accounting'])
    .directive("accountDimValidation", AccountDimValidationDirectiveFactory.create);
