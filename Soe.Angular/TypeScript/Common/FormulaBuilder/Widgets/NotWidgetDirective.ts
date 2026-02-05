import { WidgetControllerBase } from "../Base/WidgetBase";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { SoeTimeRuleOperatorType } from "../../../Util/CommonEnumerations";
import { ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { ITimeService as ISharedTimeService } from "../../../Shared/Time/Time/TimeService";

export class NotWidgetDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return WidgetControllerBase.getWidgetDirective(urlHelperService.getGlobalUrl("Common/FormulaBuilder/Widgets/NotWidget.html"), NotWidgetController);
    }
}

class NotWidgetController extends WidgetControllerBase {

    private timeCodes: ISmallGenericType[] = [];

    //@ngInject
    constructor(
        $timeout: ng.ITimeoutService,
        $q: ng.IQService,
        private translationService: ITranslationService,
        private sharedTimeService: ISharedTimeService) {
        super($timeout, $q);
    }

    protected setup(): ng.IPromise<any> {
        this.widget.timeRuleType = SoeTimeRuleOperatorType.TimeRuleOperatorNot;
        this.widget.widgetClass = 'not';
        this.widget.titleIcon = 'fal fa-not-equal';
        this.widget.hasSettings = true;
        if (!this.widget.data)
            this.widget.data = { leftValueId: 0 };

        if (this.widget.isFormula)
            this.loadTimeCodes();

        return this.loadTerms();
    }

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "common.formulabuilder.expression.not",
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.widget.title = terms["common.formulabuilder.expression.not"];
        });
    }

    private loadTimeCodes(): ng.IPromise<any> {
        return this.sharedTimeService.getTimeRuleTimeCodesLeft().then(x => {
            this.timeCodes = x;
        });
    }
}

