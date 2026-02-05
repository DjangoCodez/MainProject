import { ICoreService } from "../../../../Core/Services/CoreService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { CalendarUtility } from "../../../../Util/CalendarUtility";
import { SettingDataType, TermGroup, TimeTerminalSettingType, TimeTerminalType } from "../../../../Util/CommonEnumerations";
import { Constants } from "../../../../Util/Constants";
import { CoreUtility } from "../../../../Util/CoreUtility";
import { ISoeGridOptions } from "../../../../Util/SoeGridOptions";
import { UserGaugeSettingDTO } from "../../../Models/DashboardDTOs";
import { WidgetControllerBase } from "../../Base/WidgetBase";

export class TimeTerminalGaugeDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {
        return WidgetControllerBase.getWidgetDirective(urlHelperService.getWidgetUrl('TimeTerminal', 'TimeTerminalGauge.html'), TimeTerminalGaugeController);
    }
}

class TimeTerminalGaugeController extends WidgetControllerBase {

    private soeGridOptions: ISoeGridOptions;

    private showAllCompanies: boolean = false;
    private allCompanies: boolean = false;
    private onlyRegistered: boolean = false;
    private onlySynced: boolean = false;
    private timeTerminalType: number = 0;
    private timeTerminalTypes: any[];

    //Terms
    private terms: { [index: string]: string; };

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
            "common.dashboard.timeterminal.title",
            "common.name",
            "common.type",
            "common.days",
            "common.dashboard.timeterminal.lastsync",
            "common.dashboard.timeterminal.neversynced",
            "common.dashboard.timeterminal.timesincelastsync"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
            this.widgetTitle = terms["common.dashboard.timeterminal.title"];

            this.soeGridOptions.addColumnText("name", terms["common.name"], null);
            this.soeGridOptions.addColumnText("typeName", terms["common.type"], "30%");
            this.soeGridOptions.addColumnDateTime("lastSync", terms["common.dashboard.timeterminal.lastsync"], "20%");
            this.soeGridOptions.addColumnShape("lastSyncStateColor", null, "3%", "", Constants.SHAPE_CIRCLE, "syncStateTooltip", "", "");
        });
    }

    protected load() {
        super.load();
        this.coreService.getTimeTerminals(this.timeTerminalType, false, this.onlyRegistered, this.onlySynced, true, this.allCompanies, true, true).then((x) => {
            _.forEach(x, (row) => {
                if (row.type === TimeTerminalType.WebTimeStamp || row.type === TimeTerminalType.GoTimeStamp) {
                    row.lastSyncStateColor = "#FFFFFF"; // White
                } else if (!row.lastSync) {
                    row.syncStateTooltip = this.terms["common.dashboard.timeterminal.neversynced"];
                    row.lastSyncStateColor = "#D3D3D3"; // Gray
                } else {
                    var now: Date = new Date();
                    var diffMinutes = now.diffMinutes(row.lastSync);
                    var diffSec = diffMinutes * 60;

                    // Get sync interval for current terminal
                    var syncInterval: number = 900; // Default 15 minutes

                    _.forEach(row.timeTerminalSettings, (terminalSetting) => {
                        if (terminalSetting.type === TimeTerminalSettingType.SyncInterval) {
                            if (terminalSetting.intData) {
                                syncInterval = terminalSetting.intData;
                                return false;
                            }
                        }
                    });

                    let toolTip: string = '';
                    if (diffMinutes > (60 * 24)) {
                        toolTip = Math.floor(diffMinutes / (60 * 24)).toString() + " " + this.terms["common.days"].toLocaleLowerCase();
                    } else {
                        toolTip = CalendarUtility.minutesToTimeSpan(diffMinutes);
                    }
                    toolTip += " " + this.terms["common.dashboard.timeterminal.timesincelastsync"];
                    row.syncStateTooltip = toolTip;

                    if (diffSec < syncInterval) {
                        row.lastSyncStateColor = "#00FF00"; // Green
                    } else if (diffSec < (syncInterval * 3)) {
                        row.lastSyncStateColor = "#FFFF00"; // Yellow
                    } else {
                        row.lastSyncStateColor = "#FF0000"; // Red
                    }
                }
            });
            this.soeGridOptions.setData(x);
            super.loadComplete(x.length);
        });
    }

    public loadSettings() {
        this.loadTimeTerminalTypes();

        this.showAllCompanies = CoreUtility.isSupportAdmin;
        if (this.showAllCompanies) {
            var settingAllCompanies: UserGaugeSettingDTO = this.getUserGaugeSetting('AllCompanies');
            this.allCompanies = (settingAllCompanies ? settingAllCompanies.boolData : false);
        }

        var settingOnlyRegistered: UserGaugeSettingDTO = this.getUserGaugeSetting('OnlyRegistered');
        this.onlyRegistered = (settingOnlyRegistered ? settingOnlyRegistered.boolData : false);

        var settingOnlySynced: UserGaugeSettingDTO = this.getUserGaugeSetting('OnlySynced');
        this.onlySynced = (settingOnlySynced ? settingOnlySynced.boolData : false);

        var settingTerminalType: UserGaugeSettingDTO = this.getUserGaugeSetting('TerminalType');
        this.timeTerminalType = (settingTerminalType ? settingTerminalType.intData : 0);
    }

    private loadTimeTerminalTypes(): ng.IPromise<any> {
        this.timeTerminalTypes = [];
        return this.coreService.getTermGroupContent(TermGroup.TimeTerminalType, true, false).then((x) => {
            this.timeTerminalTypes = x;
        });
    }

    public saveSettings() {
        var settings: UserGaugeSettingDTO[] = [];

        var settingAllCompanies = new UserGaugeSettingDTO('AllCompanies', SettingDataType.Boolean);
        settingAllCompanies.boolData = this.allCompanies;
        settings.push(settingAllCompanies);

        var settingOnlyRegistered = new UserGaugeSettingDTO('OnlyRegistered', SettingDataType.Boolean);
        settingOnlyRegistered.boolData = this.onlyRegistered;
        settings.push(settingOnlyRegistered);

        var settingOnlySynced = new UserGaugeSettingDTO('OnlySynced', SettingDataType.Boolean);
        settingOnlySynced.boolData = this.onlySynced;
        settings.push(settingOnlySynced);

        var settingTerminalType = new UserGaugeSettingDTO('TerminalType', SettingDataType.Integer);
        settingTerminalType.intData = this.timeTerminalType;
        settings.push(settingTerminalType);

        this.coreService.saveUserGaugeSettings(this.widgetUserGauge.userGaugeId, settings).then(result => {
            if (result.success)
                this.widgetUserGauge.userGaugeSettings = settings;
        });
    }
}