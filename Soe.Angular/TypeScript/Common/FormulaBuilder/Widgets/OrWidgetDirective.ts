import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { WidgetControllerBase } from "../Base/WidgetBase";
import { PriceRuleItemType, SoeTimeRuleOperatorType } from "../../../Util/CommonEnumerations";
import { ITranslationService } from "../../../Core/Services/TranslationService";

export class OrWidgetDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return WidgetControllerBase.getWidgetDirective(urlHelperService.getGlobalUrl("Common/FormulaBuilder/Widgets/OrWidget.html"), OrWidgetController);
    }
}

class OrWidgetController extends WidgetControllerBase {
    //@ngInject
    constructor($timeout: ng.ITimeoutService,
        $q: ng.IQService,
        private translationService: ITranslationService) {
        super($timeout, $q);
    }

    protected setup(): ng.IPromise<any> {
        return this.$timeout(() => {
            this.widget.widgetClass = 'default';
            this.widget.timeRuleType = SoeTimeRuleOperatorType.TimeRuleOperatorOr;
            this.widget.priceRuleType = PriceRuleItemType.Or;

            return this.translationService.translate("core.or").then(term => {
                this.widget.title = term.toLocaleUpperCase();
            });
        });
    }
}

