import '../Module';

import { TimeAbsenceRuleRowsDirectiveFactory } from './Directives/TimeAbsenceRuleRows/TimeAbsenceRuleRowsDirective';

angular.module("Soe.Time.Time.TimeAbsenceRules.Module", ['Soe.Time.Time'])
    .directive("timeAbsenceRuleRows", TimeAbsenceRuleRowsDirectiveFactory.create);
