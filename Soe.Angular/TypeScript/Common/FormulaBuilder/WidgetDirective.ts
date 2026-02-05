import { IUrlHelperService } from "../../Core/Services/UrlHelperService";
import { WidgetControllerBase } from "./Base/WidgetBase";
import { StringUtility } from "../../Util/StringUtility";
import { IWidget } from "../Models/FormulaBuilderDTOs";

export class WidgetDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl("/Common/FormulaBuilder/Views/widget.html"),
            scope: {
                isReadonly: '=',
                widget: '=',
                isFormula: '=',
                onRemove: '&'
            },
            restrict: 'E',
            replace: true,
            controller: WidgetController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

class WidgetController {
    private isReadonly: boolean;
    private widget: IWidget;
    private isFormula: boolean;
    private onRemove: Function;
    private subScope: ng.IScope;

    //@ngInject
    constructor(private $compile,
        private $scope: ng.IScope,
        private $timeout: ng.ITimeoutService,
        private $element) {
    }

    public $onInit() {
        this.compile();
    }

    private compile() {
        // Create widget and place it inside the body
        var el = this.$compile('<{0} widget="ctrl.widget"></{0}>'.format(StringUtility.snake_case(this.widget.widgetName, '-')))(this.$scope);
        var body = this.$element.find('.panel-body');
        body.append(el[0]);
    }

    private remove() {
        if (this.onRemove)
            this.onRemove();
    }

    public addSubScope(scope) {
        this.subScope = scope;
    }

    private getWidgetCtrl(): WidgetControllerBase {
        return this.subScope['ctrl'];
    }
}