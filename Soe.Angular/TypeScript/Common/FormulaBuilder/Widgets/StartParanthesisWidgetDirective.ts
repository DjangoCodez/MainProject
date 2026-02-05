import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { WidgetControllerBase } from "../Base/WidgetBase";
import { PriceRuleItemType, SoeTimeRuleOperatorType } from "../../../Util/CommonEnumerations";

export class StartParanthesisWidgetDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return WidgetControllerBase.getWidgetDirective(urlHelperService.getGlobalUrl("Common/FormulaBuilder/Widgets/StartParanthesisWidget.html"), StartParanthesisWidgetDirectiveController);
    }
}

class StartParanthesisWidgetDirectiveController extends WidgetControllerBase {
    //@ngInject
    constructor($timeout: ng.ITimeoutService, $q: ng.IQService) {
        super($timeout, $q);
    }

    protected setup(): ng.IPromise<any> {
        return this.$timeout(() => {
            this.widget.widgetClass = 'default';
            this.widget.priceRuleType = PriceRuleItemType.StartParanthesis;
            this.widget.timeRuleType = SoeTimeRuleOperatorType.TimeRuleOperatorStartParanthesis;
            this.widget.title = "(";
        });
    }
}

