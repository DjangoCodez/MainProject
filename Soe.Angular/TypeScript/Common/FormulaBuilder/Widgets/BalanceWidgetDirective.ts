import { WidgetControllerBase } from "../Base/WidgetBase";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { SoeTimeRuleOperatorType, SoeTimeRuleComparisonOperator } from "../../../Util/CommonEnumerations";
import { ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { ITimeService as ISharedTimeService } from "../../../Shared/Time/Time/TimeService";
import { CalendarUtility } from "../../../Util/CalendarUtility";

export class BalanceWidgetDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return WidgetControllerBase.getWidgetDirective(urlHelperService.getGlobalUrl("Common/FormulaBuilder/Widgets/BalanceWidget.html"), BalanceWidgetController);
    }
}

class BalanceWidgetController extends WidgetControllerBase {

    private get minutes(): string {
        return CalendarUtility.minutesToTimeSpan(this.widget.data ? this.widget.data.minutes || 0 : 0);
    }

    private set minutes(value: string) {
        this.widget.data.minutes = CalendarUtility.timeSpanToMinutes(CalendarUtility.parseTimeSpan(value));
    }

    private timeCodesLeft: ISmallGenericType[] = [];
    private timeCodesRight: ISmallGenericType[] = [];
    private comparisonOperators: ISmallGenericType[] = [];

    //@ngInject
    constructor(
        $timeout: ng.ITimeoutService,
        $q: ng.IQService,
        private translationService: ITranslationService,
        private sharedTimeService: ISharedTimeService) {
        super($timeout, $q);
    }

    protected setup(): ng.IPromise<any> {
        this.widget.timeRuleType = SoeTimeRuleOperatorType.TimeRuleOperatorBalance;
        this.widget.widgetClass = 'balance';
        this.widget.titleIcon = 'fal fa-balance-scale';
        this.widget.hasSettings = true;
        if (!this.widget.data)
            this.widget.data = { leftValueId: 0, rightValueId: 0, comparisonOperator: SoeTimeRuleComparisonOperator.TimeRuleComparisonOperatorLessThan, minutes: 0 };

        if (this.widget.isFormula) {
            this.loadTimeCodesLeft();
            this.loadTimeCodesRight();
            this.setupComparisonOperators();
        }

        return this.loadTerms();
    }

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "common.formulabuilder.expression.balance",
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.widget.title = terms["common.formulabuilder.expression.balance"];
        });
    }

    private loadTimeCodesLeft(): ng.IPromise<any> {
        return this.sharedTimeService.getTimeRuleTimeCodesLeft().then(x => {
            this.timeCodesLeft = x;
        });
    }

    private loadTimeCodesRight(): ng.IPromise<any> {
        return this.sharedTimeService.getTimeRuleTimeCodesRight().then(x => {
            this.timeCodesRight = x;
        });
    }

    private setupComparisonOperators() {
        this.comparisonOperators.push({ id: SoeTimeRuleComparisonOperator.TimeRuleComparisonOperatorLessThan, name: '<' });
        this.comparisonOperators.push({ id: SoeTimeRuleComparisonOperator.TimeRuleComparisonOperatorLessThanOrEqualsTo, name: '<=' });
        this.comparisonOperators.push({ id: SoeTimeRuleComparisonOperator.TimeRuleComparisonOperatorEqualsTo, name: '=' });
        this.comparisonOperators.push({ id: SoeTimeRuleComparisonOperator.TimeRuleComparisonOperatorGreaterThanOrEqualsTo, name: '>=' });
        this.comparisonOperators.push({ id: SoeTimeRuleComparisonOperator.TimeRuleComparisonOperatorGreaterThan, name: '>' });
    }
}

