import '../../../Core/Module';

import { DistributionRowsDirectiveFactory } from "../../Directives/DistributionRows/DistributionRowsDirective";

angular.module("Soe.Common.Dialogs.SelectAccountDistribution.Module", ['Soe.Core'])
    .directive("distributionRows", DistributionRowsDirectiveFactory.create);
