import { WidgetControllerBase } from "../Base/WidgetBase";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { PriceRuleItemType, PriceRuleValueType } from "../../../Util/CommonEnumerations";

export class SupplierAgreementWidgetDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return WidgetControllerBase.getWidgetDirective(urlHelperService.getGlobalUrl("Common/FormulaBuilder/Widgets/SupplierAgreementWidget.html"), SupplierAgreementWidgetController);
    }
}

class SupplierAgreementWidgetController extends WidgetControllerBase {
    //@ngInject
    constructor(
        $timeout: ng.ITimeoutService,
        $q: ng.IQService,
        private translationService: ITranslationService) {
        super($timeout, $q);
    }

    protected setup(): ng.IPromise<any> {
        this.widget.priceRuleType = PriceRuleItemType.SupplierAgreement;
        this.widget.priceRuleValueType = PriceRuleValueType.PositivePercent;
        this.widget.widgetClass = 'supplieragreement';
        this.widget.hasSettings = true;
        if (!this.widget.data)
            this.widget.data = { percent: 0, isExample: true };

        var keys: string[] = [
            "common.formulabuilder.expression.supplieragreement"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.widget.title = terms["common.formulabuilder.expression.supplieragreement"];
        });
    }
}

