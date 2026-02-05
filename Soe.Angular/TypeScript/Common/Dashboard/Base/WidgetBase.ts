import { IUserGaugeDTO } from "../../../Scripts/TypeLite.Net4";
import { UserGaugeSettingDTO } from "../../Models/DashboardDTOs";
import { ISoeGridOptions, SoeGridOptions } from "../../../Util/SoeGridOptions";
import { Guid } from "../../../Util/StringUtility";

export class WidgetControllerBase {
    protected widgetTitle: string;
    protected widgetCss: string;    // Default size is 'col-sm-4', can be overridden in widget
    protected widgetUserGauge: IUserGaugeDTO;
    protected widgetHasSettings: boolean = false;
    protected widgetHasRecordCount: boolean;
    protected widgetRecordCount: number = 0;
    protected widgetIsLoading: boolean = false;
    protected settingsMode: boolean = false;
    protected widgetSettingsValid: boolean;
    protected widgetId: string = Guid.newGuid();

    constructor(
        protected $timeout: ng.ITimeoutService,
        protected $q: ng.IQService,
        private uiGridConstants: uiGrid.IUiGridConstants) {

        // Need timeout so widget constructor is called before setup
        this.$timeout(() => {
            this.setup().then(() => {
                this.setupComplete();
            });
        });
    }

    $onInit() {
        this.widgetCss = "col-sm-4";
        this.widgetHasRecordCount = true;
        this.widgetRecordCount = 0;
    }

    protected setup(): ng.IPromise<any> {
        // Override in widget for setup
        var deferral = this.$q.defer();
        deferral.resolve();
        return deferral.promise;
    }

    protected createGrid(): ISoeGridOptions {
        var soeGridOptions: ISoeGridOptions = new SoeGridOptions("", this.$timeout, this.uiGridConstants);
        soeGridOptions.enableGridMenu = false;
        soeGridOptions.enableColumnMenus = false;
        soeGridOptions.enableFiltering = false;
        soeGridOptions.showGridFooter = false;

        return soeGridOptions;
    }

    protected setupComplete() {
        // Override in widget for loading data etc
        this.load();
    }

    protected load() {
        // Override in widget for loading data
        this.widgetIsLoading = true;
    }

    protected loadComplete(nbrOfRecords: number) {
        // Use timeout just to get the spinner running on fast loads
        this.widgetRecordCount = nbrOfRecords;
        this.$timeout(() => {
            this.widgetIsLoading = false;
            this.widgetSettingsValid = true;
        }, 900);
    }

    public reload() {
        // Override in widget for reloading data
        this.load();
    }

    public loadSettings() {
        // Override in widget for loading user settings
    }

    public saveSettings() {
        // Override in widget for saving user settings
    }

    public cancelSettings() {
        this.loadSettings();
    }

    protected getUserGaugeSetting(name: string): UserGaugeSettingDTO {
        var setting: UserGaugeSettingDTO;
        if (this.widgetUserGauge.userGaugeSettings)
            setting = _.find(this.widgetUserGauge.userGaugeSettings, ugs => ugs.name === name);

        return setting;
    }

    public static getWidgetDirective(templateUrl, controller) {
        return {
            templateUrl: templateUrl,
            scope: {
                widgetTitle: '=',
                widgetCss: '=',
                widgetUserGauge: '=',
                widgetHasSettings: '=',
                widgetHasRecordCount: '=',
                widgetRecordCount: '=',
                widgetSettingsMode: '=',
                widgetSettingsValid: '=',
                widgetIsLoading: '='
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