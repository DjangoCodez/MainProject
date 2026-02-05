import '../Module';

import { DistributionCodeValidationDirective } from './Directives/DistributionCodeValidationDirective';

angular.module("Soe.Economy.Accounting.DistributionCodes.Module", ['Soe.Economy.Accounting'])
    .directive("distributionCodeValidation", DistributionCodeValidationDirective.create);

