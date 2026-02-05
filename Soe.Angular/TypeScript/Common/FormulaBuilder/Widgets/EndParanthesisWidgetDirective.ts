import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { WidgetControllerBase } from "../Base/WidgetBase";
import { PriceRuleItemType, SoeTimeRuleOperatorType } from "../../../Util/CommonEnumerations";

export class EndParanthesisWidgetDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return WidgetControllerBase.getWidgetDirective(urlHelperService.getGlobalUrl("Common/FormulaBuilder/Widgets/EndParanthesisWidget.html"), EndParanthesisWidgetDirectiveController);
    }
}

class EndParanthesisWidgetDirectiveController extends WidgetControllerBase {
    //@ngInject
    constructor($timeout: ng.ITimeoutService, $q: ng.IQService) {
        super($timeout, $q);
    }

    protected setup(): ng.IPromise<any> {
        return this.$timeout(() => {
            this.widget.widgetClass = 'default';
            this.widget.priceRuleType = PriceRuleItemType.EndParanthesis;
            this.widget.timeRuleType = SoeTimeRuleOperatorType.TimeRuleOperatorEndParanthesis;
            this.widget.title = ")";
        });
    }
}

