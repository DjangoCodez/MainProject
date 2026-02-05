import '../Module';

import { PeriodsDirectiveFactory } from './Directives/Periods/PeriodsDirective';
import { PlanningPeriodsValidationDirectiveFactory } from './Directives/PlanningPeriodsValidationDirective';
import { AccountingService as SharedAccountingService} from '../../../Shared/Economy/Accounting/AccountingService';

angular.module("Soe.Time.Time.PlanningPeriods.Module", ['Soe.Time.Time'])
    .directive("periods", PeriodsDirectiveFactory.create)
    .directive("planningPeriodsValidation", PlanningPeriodsValidationDirectiveFactory.create)
    .service("sharedAccountingService", SharedAccountingService);
