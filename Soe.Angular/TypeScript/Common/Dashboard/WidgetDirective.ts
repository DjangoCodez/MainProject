import { IUrlHelperService } from "../../Core/Services/UrlHelperService";
import { StringUtility } from "../../Util/StringUtility";
import { WidgetControllerBase } from "./Base/WidgetBase";
import { WidgetModel } from "./DashboadDirective";

export class WidgetDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getViewUrl('widget.html'),
            scope: {
                widget: '=',
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
    private widget: WidgetModel;
    private showSettings: boolean = false;
    private isLoading: boolean = false;
    private onRemove: Function;
    private subScope: ng.IScope;

    //@ngInject
    constructor(private $compile, private $scope, private $element) {
    }

    public $onInit() {
        this.compile();
        this.setupWatchers();
    }

    private setupWatchers() {
        this.$scope.$on('reload', () => this.reload());
    }

    private compile() {
        // Create widget and place it inside the body
        var directive = this.widget.directive;
        directive = StringUtility.snake_case(directive, '-');
        var el = this.$compile('<' + directive + ' widget-title="ctrl.widget.title" widget-css="ctrl.widget.cssClasses" widget-user-gauge="ctrl.widget.userGauge" widget-has-settings="ctrl.widget.hasSettings" widget-has-record-count="ctrl.widget.hasRecordCount" widget-record-count="ctrl.widget.recordCount" widget-settings-mode="ctrl.showSettings" widget-settings-valid="ctrl.widget.settingsValid" widget-is-loading="ctrl.widget.isLoading"></' + directive + '>')(this.$scope);
        this.$element.find('.panel-body').append(el);
    }

    private toggleSettings() {
        this.showSettings = !this.showSettings;
    }

    private cancelSettings() {
        var widgetCtrl: WidgetControllerBase = this.getWidgetCtrl();
        if (widgetCtrl) {
            widgetCtrl.loadSettings();
            widgetCtrl.cancelSettings();
        }
        this.reload();
        this.toggleSettings();
    }

    private saveSettings() {
        var widgetCtrl: WidgetControllerBase = this.getWidgetCtrl();
        if (widgetCtrl)
            widgetCtrl.saveSettings();
        this.reload();
        this.toggleSettings();
    }

    private reload() {
        var widgetCtrl: WidgetControllerBase = this.getWidgetCtrl();
        if (widgetCtrl) {
            widgetCtrl.reload();
        } else {
            // A very hard reload
            this.subScope.$destroy();
            this.$element.find('.panel-body').children().remove();
            this.compile();
        }
    }

    private remove() {
        this.onRemove();
    }

    public addSubScope(scope) {
        this.subScope = scope;
    }

    private getWidgetCtrl(): WidgetControllerBase {
        return this.subScope['ctrl'];
    }
}