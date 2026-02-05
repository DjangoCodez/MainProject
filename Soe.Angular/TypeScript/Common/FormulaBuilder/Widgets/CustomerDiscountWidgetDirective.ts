import { WidgetControllerBase } from "../Base/WidgetBase";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { PriceRuleItemType, PriceRuleValueType } from "../../../Util/CommonEnumerations";

export class CustomerDiscountWidgetDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return WidgetControllerBase.getWidgetDirective(urlHelperService.getGlobalUrl("Common/FormulaBuilder/Widgets/CustomerDiscountWidget.html"), CustomerDiscountWidgetController);
    }
}

class CustomerDiscountWidgetController extends WidgetControllerBase {
    //@ngInject
    constructor(
        $timeout: ng.ITimeoutService,
        $q: ng.IQService,
        private translationService: ITranslationService) {
        super($timeout, $q);
    }

    protected setup(): ng.IPromise<any> {
        this.widget.priceRuleType = PriceRuleItemType.CustomerDiscount;
        this.widget.priceRuleValueType = PriceRuleValueType.NegativePercent;        
        this.widget.widgetClass = 'customerdiscount';
        this.widget.hasSettings = true;
        if (!this.widget.data)
            this.widget.data = { percent: 0, isExample: true };

        var keys: string[] = [
            "common.formulabuilder.expression.customerdiscount"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.widget.title = terms["common.formulabuilder.expression.customerdiscount"];
        });
    }
}

