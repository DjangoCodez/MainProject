import { IWidget } from "../../Models/FormulaBuilderDTOs";

export class WidgetControllerBase {
    protected widget: IWidget;

    constructor(
        protected $timeout: ng.ITimeoutService,
        protected $q: ng.IQService) {
    }

    $onInit() {
        // Need timeout so widget constructor is called before setup
        this.$timeout(() => {
            this.setup().then(() => {
                this.setupComplete();
            });
        });
    }

    protected setup(): ng.IPromise<any> {
        // Override in widget for setup
        var deferral = this.$q.defer();
        deferral.resolve();
        return deferral.promise;
    }

    protected setupComplete() {
        // Override in widget for loading data etc
    }

    public static getWidgetDirective(templateUrl, controller) {
        return {
            templateUrl: templateUrl,
            scope: {
                widget: '='
            },
            require: '^widget',
            restrict: 'E',
            replace: true,
            controller: controller,
            controllerAs: 'ctrl',
            bindToController: true,
            link: function (scope, element, attrs, widgetCtrl) {
                widgetCtrl.addSubScope(scope);
            }
        };
    }
}