import { WidgetControllerBase } from "../Base/WidgetBase";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { PriceRuleItemType, PriceRuleValueType } from "../../../Util/CommonEnumerations";

export class PriceBasedMarkupWidgetDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return WidgetControllerBase.getWidgetDirective(urlHelperService.getGlobalUrl("Common/FormulaBuilder/Widgets/PriceBasedMarkupWidget.html"), PriceBasedMarkupWidgetController);
    }
}

class PriceBasedMarkupWidgetController extends WidgetControllerBase {
    //@ngInject
    constructor(
        $timeout: ng.ITimeoutService,
        $q: ng.IQService,
        private translationService: ITranslationService) {
        super($timeout, $q);
    }

    protected setup(): ng.IPromise<any> {
        this.widget.priceRuleType = PriceRuleItemType.PriceBasedMarkup;
        this.widget.priceRuleValueType = PriceRuleValueType.PositivePercent;
        this.widget.widgetClass = 'pricebasedmarkup';
        this.widget.hasSettings = true;
        if (!this.widget.data)
            this.widget.data = { percent: 0, isExample: true };

        return this.translationService.translate("common.formulabuilder.expression.pricebasedmarkup").then(term => {
            this.widget.title = term;
        });
    }
}

