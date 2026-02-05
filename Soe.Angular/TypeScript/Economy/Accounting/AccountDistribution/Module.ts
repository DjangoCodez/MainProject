import '../Module';
import '../../../Shared/Economy/Module';

import { AccountDistributionValidationDirectiveFactory } from "../../../Shared/Economy/Accounting/AccountDistribution/AccountDistributionValidationDirective";
import { DistributionRowsDirectiveFactory } from "../../../Common/Directives/DistributionRows/DistributionRowsDirective";

angular.module("Soe.Economy.Accounting.AccountDistribution.Module", ['Soe.Economy.Accounting', 'Soe.Shared.Economy'])
    .directive("accountDistributionValidation", AccountDistributionValidationDirectiveFactory.create);
    //.directive("distributionRows", DistributionRowsDirectiveFactory.create);
