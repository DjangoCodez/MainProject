import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { WidgetControllerBase } from "../Base/WidgetBase";
import { PriceRuleItemType } from "../../../Util/CommonEnumerations";

export class MultiplicationWidgetDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return WidgetControllerBase.getWidgetDirective(urlHelperService.getGlobalUrl("Common/FormulaBuilder/Widgets/MultiplicationWidget.html"), MultiplicationWidgetController);
    }
}

class MultiplicationWidgetController extends WidgetControllerBase {
    //@ngInject
    constructor($timeout: ng.ITimeoutService, $q: ng.IQService) {
        super($timeout, $q);
    }

    protected setup(): ng.IPromise<any> {
        return this.$timeout(() => {
            this.widget.widgetClass = 'default';
            this.widget.priceRuleType = PriceRuleItemType.Multiplication;
            this.widget.title = "*";
        });
    }
}

