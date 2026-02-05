import '../Module';
import '../../../Shared/Economy/Module';

import { exportDetailsDirective } from "./Directives/exportDetailsDirective";

angular.module("Soe.Economy.Export.PaymentServices.Module", ['Soe.Shared.Economy', 'Soe.Economy.Export'])
    .directive("exportDetails", exportDetailsDirective);
