import { WidgetControllerBase } from "../Base/WidgetBase";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { PriceRuleItemType, PriceRuleValueType } from "../../../Util/CommonEnumerations";

export class GnpWidgetDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return WidgetControllerBase.getWidgetDirective(urlHelperService.getGlobalUrl("Common/FormulaBuilder/Widgets/GnpWidget.html"), GnpWidgetController);
    }
}

class GnpWidgetController extends WidgetControllerBase {    

    //@ngInject
    constructor(
        $timeout: ng.ITimeoutService,
        $q: ng.IQService,             
        private translationService: ITranslationService) {
        super($timeout, $q);
    }
    
    protected setup(): ng.IPromise<any> {               

        this.widget.priceRuleType = PriceRuleItemType.GNP;
        this.widget.priceRuleValueType = PriceRuleValueType.Numeric;
        this.widget.widgetClass = 'gnp';
        this.widget.hasSettings = true;
        if (!this.widget.data)
            this.widget.data = { percent: 100, isExample: true };

        var keys: string[] = [
            "common.formulabuilder.expression.gnp"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.widget.title = terms["common.formulabuilder.expression.gnp"];
        });
    }    

}

