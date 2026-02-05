import '../Module';
import { ScheduleService } from '../../Schedule/ScheduleService';
import { TimeRuleValidationDirectiveFactory } from './TimeRuleValidationDirective';
import { ImportTimeRulesValidationDirectiveFactory } from './Dialogs/ImportTimeRules/ImportTimeRulesValidationDirective';
import { ImportTimeRulesMatchingValidationDirectiveFactory } from './Dialogs/ImportTimeRulesMatching/ImportTimeRulesMatchingValidationDirective';
import { ExpressionContainerDirectiveFactory } from '../../../Common/FormulaBuilder/ExpressionContainerDirective';
import { FormulaContainerDirectiveFactory } from '../../../Common/FormulaBuilder/FormulaContainerDirective';
import { WidgetDirectiveFactory } from '../../../Common/FormulaBuilder/WidgetDirective';
import { ScheduleInWidgetDirectiveFactory } from '../../../Common/FormulaBuilder/Widgets/ScheduleInWidgetDirective';
import { ScheduleOutWidgetDirectiveFactory } from '../../../Common/FormulaBuilder/Widgets/ScheduleOutWidgetDirective';
import { ClockWidgetDirectiveFactory } from '../../../Common/FormulaBuilder/Widgets/ClockWidgetDirective';
import { BalanceWidgetDirectiveFactory } from '../../../Common/FormulaBuilder/Widgets/BalanceWidgetDirective';
import { NotWidgetDirectiveFactory } from '../../../Common/FormulaBuilder/Widgets/NotWidgetDirective';
import { StartParanthesisWidgetDirectiveFactory } from '../../../Common/FormulaBuilder/Widgets/StartParanthesisWidgetDirective';
import { EndParanthesisWidgetDirectiveFactory } from '../../../Common/FormulaBuilder/Widgets/EndParanthesisWidgetDirective';
import { AndWidgetDirectiveFactory } from '../../../Common/FormulaBuilder/Widgets/AndWidgetDirective';
import { OrWidgetDirectiveFactory } from '../../../Common/FormulaBuilder/Widgets/OrWidgetDirective';

angular.module("Soe.Time.Time.TimeRules.Module", ['Soe.Time.Time'])
    .service("scheduleService", ScheduleService)
    .directive("timeRuleValidation", TimeRuleValidationDirectiveFactory.create)
    .directive("importTimeRulesValidation", ImportTimeRulesValidationDirectiveFactory.create)
    .directive("importTimeRulesMatchingValidation", ImportTimeRulesMatchingValidationDirectiveFactory.create)
    .directive("expressionContainer", ExpressionContainerDirectiveFactory.create)
    .directive("formulaContainer", FormulaContainerDirectiveFactory.create)
    .directive("widget", WidgetDirectiveFactory.create)
    .directive("scheduleInWidget", ScheduleInWidgetDirectiveFactory.create)
    .directive("scheduleOutWidget", ScheduleOutWidgetDirectiveFactory.create)
    .directive("clockWidget", ClockWidgetDirectiveFactory.create)
    .directive("balanceWidget", BalanceWidgetDirectiveFactory.create)
    .directive("notWidget", NotWidgetDirectiveFactory.create)
    .directive("startParanthesisWidget", StartParanthesisWidgetDirectiveFactory.create)
    .directive("endParanthesisWidget", EndParanthesisWidgetDirectiveFactory.create)
    .directive("andWidget", AndWidgetDirectiveFactory.create)
    .directive("orWidget", OrWidgetDirectiveFactory.create);



