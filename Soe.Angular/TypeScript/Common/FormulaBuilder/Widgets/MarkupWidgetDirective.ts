import { WidgetControllerBase } from "../Base/WidgetBase";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { PriceRuleItemType, PriceRuleValueType } from "../../../Util/CommonEnumerations";

export class MarkupWidgetDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return WidgetControllerBase.getWidgetDirective(urlHelperService.getGlobalUrl("Common/FormulaBuilder/Widgets/MarkupWidget.html"), MarkupWidgetController);
    }
}

class MarkupWidgetController extends WidgetControllerBase {
    //@ngInject
    constructor(
        $timeout: ng.ITimeoutService,
        $q: ng.IQService,
        private translationService: ITranslationService) {
        super($timeout, $q);
    }

    protected setup(): ng.IPromise<any> {
        this.widget.priceRuleType = PriceRuleItemType.Markup;
        this.widget.priceRuleValueType = PriceRuleValueType.PositivePercent;
        this.widget.widgetClass = 'markup';
        this.widget.hasSettings = true;
        if (!this.widget.data)
            this.widget.data = { percent: 0, isExample: true };

        var keys: string[] = [
            "common.formulabuilder.expression.markup"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.widget.title = terms["common.formulabuilder.expression.markup"];
        });
    }
}

