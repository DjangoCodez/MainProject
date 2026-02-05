import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { WidgetControllerBase } from "../Base/WidgetBase";
import { SoeTimeRuleOperatorType } from "../../../Util/CommonEnumerations";
import { ITranslationService } from "../../../Core/Services/TranslationService";

export class AndWidgetDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return WidgetControllerBase.getWidgetDirective(urlHelperService.getGlobalUrl("Common/FormulaBuilder/Widgets/AndWidget.html"), AndWidgetController);
    }
}

class AndWidgetController extends WidgetControllerBase {
    //@ngInject
    constructor($timeout: ng.ITimeoutService,
        $q: ng.IQService,
        private translationService: ITranslationService) {
        super($timeout, $q);
    }

    protected setup(): ng.IPromise<any> {
        return this.$timeout(() => {
            this.widget.widgetClass = 'default';
            this.widget.timeRuleType = SoeTimeRuleOperatorType.TimeRuleOperatorAnd;

            var keys: string[] = [
                "core.and"
            ];

            return this.translationService.translateMany(keys).then(terms => {
                this.widget.title = terms["core.and"].toLocaleUpperCase();
            });
        });
    }
}

