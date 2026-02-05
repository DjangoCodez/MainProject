import { WidgetControllerBase } from "../Base/WidgetBase";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { SoeTimeRuleOperatorType, SoeTimeRuleComparisonOperator } from "../../../Util/CommonEnumerations";
import { CalendarUtility } from "../../../Util/CalendarUtility";

export class ScheduleInWidgetDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return WidgetControllerBase.getWidgetDirective(urlHelperService.getGlobalUrl("Common/FormulaBuilder/Widgets/ScheduleInWidget.html"), ScheduleInWidgetController);
    }
}

class ScheduleInWidgetController extends WidgetControllerBase {

    private get minutes(): string {
        return CalendarUtility.minutesToTimeSpan(this.widget.data ? this.widget.data.minutes || 0 : 0);
    }

    private set minutes(value: string) {
        this.widget.data.minutes = CalendarUtility.timeSpanToMinutes(CalendarUtility.toFormattedTime(value));
    }

    //@ngInject
    constructor(
        $timeout: ng.ITimeoutService,
        $q: ng.IQService,
        private translationService: ITranslationService) {
        super($timeout, $q);
    }

    protected setup(): ng.IPromise<any> {
        if (!this.widget.data)
            this.widget.data = { minutes: 0, comparisonOperator: SoeTimeRuleComparisonOperator.TimeRuleComparisonClockPositive };

        this.widget.timeRuleType = SoeTimeRuleOperatorType.TimeRuleOperatorScheduleIn;
        this.widget.widgetClass = this.widget.data.isStandby ? 'standby' : 'schedule';
        this.widget.titleIcon = 'fal fa-sign-in';
        this.widget.hasSettings = true;

        var keys: string[] = [
            "common.formulabuilder.expression.schedulein",
            "common.formulabuilder.expression.standbyin"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.widget.title = this.widget.data.isStandby ? terms["common.formulabuilder.expression.standbyin"] : terms["common.formulabuilder.expression.schedulein"];
        });
    }
}

