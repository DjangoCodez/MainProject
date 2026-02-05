import { ICoreService } from "../../../../Core/Services/CoreService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { SettingDataType, TermGroup, TermGroup_TimeStampAttendanceGaugeShowMode } from "../../../../Util/CommonEnumerations";
import { ISoeGridOptions } from "../../../../Util/SoeGridOptions";
import { TimeStampAttendanceGaugeDTO, UserGaugeSettingDTO } from "../../../Models/DashboardDTOs";
import { WidgetControllerBase } from "../../Base/WidgetBase";

export class TimeStampAttendanceGaugeDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {
        return WidgetControllerBase.getWidgetDirective(urlHelperService.getWidgetUrl('TimeStampAttendance', 'TimeStampAttendanceGauge.html'), TimeStampAttendanceGaugeController);
    }
}

class TimeStampAttendanceGaugeController extends WidgetControllerBase {
    private soeGridOptions: ISoeGridOptions;

    private items: TimeStampAttendanceGaugeDTO[] = [];

    private settingOnlyIn: boolean = false;
    private settingShowMode: number = 0;
    private showModes: any[];

    // Terms
    private terms: { [index: string]: string; };


    //@ngInject
    constructor(
        private $scope: ng.IScope,
        $timeout: ng.ITimeoutService,
        $q: ng.IQService,
        private $uibModal,
        private coreService: ICoreService,
        private urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        uiGridConstants: uiGrid.IUiGridConstants) {
        super($timeout, $q, uiGridConstants);

        this.soeGridOptions = this.createGrid();
    }

    protected setup(): ng.IPromise<any> {
        var deferral = this.$q.defer();
        this.widgetHasSettings = true;
        this.widgetCss = 'col-sm-6';


        this.loadSettings();
        this.loadShowModes();
        this.setupGrid();

        deferral.resolve();
        return deferral.promise;
    }

    protected setupGrid(): ng.IPromise<any> {
        var keys: string[] = [
            "common.time",
            "common.type",
            "common.name",
            "common.dashboard.timestampattendance.title",
            "common.dashboard.timestampattendance.terminal",
            "common.dashboard.timestampattendance.settingshowmode",
            "common.dashboard.timestampattendance.settingonlyin"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;

            this.soeGridOptions.addColumnText("timeStr", this.terms["common.time"], "10%");
            this.soeGridOptions.addColumnText("typeName", this.terms["common.type"], "17%");
            this.soeGridOptions.addColumnText("name", this.terms["common.name"], null);
            this.soeGridOptions.addColumnText("timeTerminalName", this.terms["common.dashboard.timestampattendance.terminal"], "15%");
            _.forEach(this.soeGridOptions.getColumnDefs(), (colDef: uiGrid.IColumnDef) => {
                if (colDef.field === "timeStr" || colDef.field === "typeName" || colDef.field === "name") {
                    var cellcls: string = colDef.cellClass ? colDef.cellClass.toString() : "";
                    colDef.cellClass = (grid: any, row, col, rowRenderIndex, colRenderIndex) => {
                        return cellcls + (row.entity.isMissing ? " invalid-cell" : "");
                    };
                }
            });

            this.setTitle();
        });
    }

    protected load() {
        super.load();

        this.coreService.getTimeStampAttendanceWidgetData(this.settingShowMode, this.settingOnlyIn).then((x) => {
            this.items = x;
            this.soeGridOptions.setData(this.items);
            super.loadComplete(this.items.length);
        });
        super.loadComplete(0);
    }

    public loadSettings() {
        var settingShowMode: UserGaugeSettingDTO = this.getUserGaugeSetting('ShowMode');
        this.settingShowMode = (settingShowMode ? settingShowMode.intData : TermGroup_TimeStampAttendanceGaugeShowMode.AllLast24Hours);

        var settingOnlyIn: UserGaugeSettingDTO = this.getUserGaugeSetting('OnlyIn');
        this.settingOnlyIn = (settingOnlyIn ? settingOnlyIn.boolData : false);
    }

    protected loadShowModes() {
        this.showModes = [];
        return this.coreService.getTermGroupContent(TermGroup.TimeStampAttendanceGaugeShowMode, false, false).then(x => {
            this.showModes = x;
        });
    }

    public saveSettings() {
        var settings: UserGaugeSettingDTO[] = [];

        var showModeSetting = new UserGaugeSettingDTO('ShowMode', SettingDataType.Integer);
        showModeSetting.intData = this.settingShowMode;
        settings.push(showModeSetting);

        var onlyInSetting = new UserGaugeSettingDTO('OnlyIn', SettingDataType.Boolean);
        onlyInSetting.boolData = this.settingOnlyIn;
        settings.push(onlyInSetting);

        this.coreService.saveUserGaugeSettings(this.widgetUserGauge.userGaugeId, settings).then(result => {
            if (result.success)
                this.widgetUserGauge.userGaugeSettings = settings;

            this.setTitle();
        });
    }

    private setTitle() {
        this.widgetTitle = this.terms["common.dashboard.timestampattendance.title"];
    }
}