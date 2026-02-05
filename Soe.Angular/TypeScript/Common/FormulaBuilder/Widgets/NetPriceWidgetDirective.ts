import { WidgetControllerBase } from "../Base/WidgetBase";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { PriceRuleItemType, PriceRuleValueType } from "../../../Util/CommonEnumerations";

export class NetPriceWidgetDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return WidgetControllerBase.getWidgetDirective(urlHelperService.getGlobalUrl("Common/FormulaBuilder/Widgets/NetPriceWidget.html"), NetPriceWidgetController);
    }
}

class NetPriceWidgetController extends WidgetControllerBase {
    //@ngInject
    constructor(
        $timeout: ng.ITimeoutService,
        $q: ng.IQService,
        private translationService: ITranslationService) {
        super($timeout, $q);
    }

    protected setup(): ng.IPromise<any> {
        this.widget.priceRuleType = PriceRuleItemType.NetPrice;
        this.widget.priceRuleValueType = PriceRuleValueType.Numeric;
        this.widget.widgetClass = 'netprice';
        this.widget.hasSettings = true;
        if (!this.widget.data)
            this.widget.data = { percent: 100, isExample: true };

        return this.translationService.translate("common.formulabuilder.expression.netprice").then(term => {
            this.widget.title = term;
        });
    }
}

