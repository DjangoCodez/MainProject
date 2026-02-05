import { ICoreService } from "../../../../Core/Services/CoreService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { SettingDataType } from "../../../../Util/CommonEnumerations";
import { HtmlUtility } from "../../../../Util/HtmlUtility";
import { ISoeGridOptions } from "../../../../Util/SoeGridOptions";
import { UserGaugeSettingDTO } from "../../../Models/DashboardDTOs";
import { WidgetControllerBase } from "../../Base/WidgetBase";

export class SysLogGaugeDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {
        return WidgetControllerBase.getWidgetDirective(urlHelperService.getWidgetUrl('SysLog', 'SysLogGauge.html'), SysLogGaugeController);
    }
}

class SysLogGaugeController extends WidgetControllerBase {

    private soeGridOptions: ISoeGridOptions;
    private nbrOfRecords: number = 1000;

    //@ngInject
    constructor(
        $timeout: ng.ITimeoutService,
        $q: ng.IQService,
        private $window: ng.IWindowService,
        private coreService: ICoreService,
        private translationService: ITranslationService,
        uiGridConstants: uiGrid.IUiGridConstants) {
        super($timeout, $q, uiGridConstants);

        this.soeGridOptions = this.createGrid();
    }

    protected setup(): ng.IPromise<any> {
        this.widgetHasSettings = true;
        this.loadSettings();

        var keys: string[] = [
            "core.show",
            "common.company",
            "common.message",
            "common.time",
            "common.dashboard.syslog.title",
            "common.dashboard.syslog.level"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.widgetTitle = terms["common.dashboard.syslog.title"];

            this.soeGridOptions.addColumnText("level", terms["common.dashboard.syslog.level"], "15%");
            this.soeGridOptions.addColumnDateTime("date", terms["common.time"], null);
            this.soeGridOptions.addColumnText("companyName", terms["common.company"], "15%");
            this.soeGridOptions.addColumnText("message", terms["common.message"], null);
            this.soeGridOptions.addColumnIcon(null, "fal fa-file-search", terms["core.show"], "showLog");
        });
    }

    protected load() {
        super.load();
        this.coreService.getSysLogWidgetData(soeConfig.clientIpNr, this.nbrOfRecords).then(x => {
            this.soeGridOptions.setData(x);
            super.loadComplete(x.length);
        });
    }

    public loadSettings() {
        var setting: UserGaugeSettingDTO = this.getUserGaugeSetting('NbrOfRecords');
        this.nbrOfRecords = (setting ? setting.intData : 1000);
    }

    public saveSettings() {
        var settings: UserGaugeSettingDTO[] = [];
        var setting = new UserGaugeSettingDTO('NbrOfRecords', SettingDataType.Integer);
        setting.intData = this.nbrOfRecords;
        settings.push(setting);

        this.coreService.saveUserGaugeSettings(this.widgetUserGauge.userGaugeId, settings).then(result => {
            if (result.success)
                this.widgetUserGauge.userGaugeSettings = settings;
        });
    }

    private showLog(item) {
        // Open edit page
        var url: string = "/soe/manage/support/logs/edit/";
        if (item)
            url += "?log=" + item.sysLogId;
        HtmlUtility.openInNewTab(this.$window, url);
    }
}