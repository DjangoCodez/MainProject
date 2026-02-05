import { WidgetControllerBase } from "../Base/WidgetBase";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { PriceRuleItemType, PriceRuleValueType } from "../../../Util/CommonEnumerations";

export class GainWidgetDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return WidgetControllerBase.getWidgetDirective(urlHelperService.getGlobalUrl("Common/FormulaBuilder/Widgets/GainWidget.html"), GainWidgetController);
    }
}

class GainWidgetController extends WidgetControllerBase {
    //@ngInject
    constructor(
        $timeout: ng.ITimeoutService,
        $q: ng.IQService,
        private translationService: ITranslationService) {
        super($timeout, $q);
    }

    protected setup(): ng.IPromise<any> {
        this.widget.priceRuleType = PriceRuleItemType.Gain;
        this.widget.priceRuleValueType = PriceRuleValueType.PositivePercent;
        this.widget.widgetClass = 'gain';
        this.widget.hasSettings = true;
        if (!this.widget.data)
            this.widget.data = { percent: 0, isExample: false };

        var keys: string[] = [
            "common.formulabuilder.expression.gain"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.widget.title = terms["common.formulabuilder.expression.gain"];
        });
    }
}

