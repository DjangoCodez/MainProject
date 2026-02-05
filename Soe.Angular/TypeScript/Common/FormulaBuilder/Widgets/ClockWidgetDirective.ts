import { WidgetControllerBase } from "../Base/WidgetBase";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { SoeTimeRuleOperatorType, SoeTimeRuleComparisonOperator } from "../../../Util/CommonEnumerations";
import { CalendarUtility } from "../../../Util/CalendarUtility";

export class ClockWidgetDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return WidgetControllerBase.getWidgetDirective(urlHelperService.getGlobalUrl("Common/FormulaBuilder/Widgets/ClockWidget.html"), ClockWidgetController);
    }
}

class ClockWidgetController extends WidgetControllerBase {

    private get minutes(): string {
        return CalendarUtility.minutesToTimeSpan(this.getMinutesFromData());
    }

    private set minutes(value: string) {
        let prev: boolean = this.prevDay;
        let next: boolean = this.nextDay;

        let mins = CalendarUtility.timeSpanToMinutes(CalendarUtility.toFormattedTime(value));
        if (prev)
            mins = this.subtractDay(mins);
        else if (next)
            mins = this.addDay(mins);

        this.setMinutesToData(mins);
    }

    private get prevDay(): boolean {
        return (this.getMinutesFromData() < 0);
    }

    private set prevDay(value: boolean) {
        let mins: number = this.getMinutesFromData();
        mins = value ? this.subtractDay(mins) : this.addDay(mins);
        this.setMinutesToData(mins);
    }

    private get nextDay(): boolean {
        return (this.getMinutesFromData() >= (24 * 60));
    }

    private set nextDay(value: boolean) {
        let mins: number = this.getMinutesFromData();
        mins = value ? this.addDay(mins) : this.subtractDay(mins);
        this.setMinutesToData(mins);
    }

    //@ngInject
    constructor(
        $timeout: ng.ITimeoutService,
        $q: ng.IQService,
        private translationService: ITranslationService) {
        super($timeout, $q);
    }

    protected setup(): ng.IPromise<any> {
        this.widget.timeRuleType = SoeTimeRuleOperatorType.TimeRuleOperatorClock;
        this.widget.widgetClass = 'clock';
        this.widget.titleIcon = 'fal fa-clock';
        this.widget.hasSettings = true;
        if (!this.widget.data)
            this.widget.data = { minutes: 0, comparisonOperator: SoeTimeRuleComparisonOperator.Unspecified };

        var keys: string[] = [
            "common.formulabuilder.expression.clock"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.widget.title = terms["common.formulabuilder.expression.clock"];
        });
    }

    // HELP-METHODS

    private getMinutesFromData(): number {
        return this.widget.data ? this.widget.data.minutes || 0 : 0;
    }

    private setMinutesToData(mins: number) {
        this.widget.data.minutes = mins;
    }

    private addDay(mins: number): number {
        return mins + (24 * 60);
    }

    private subtractDay(mins: number): number {
        return mins - (24 * 60);
    }
}

