import { ICoreService } from "../../../../Core/Services/CoreService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { SystemInfoLogLevel, SystemInfoType } from "../../../../Util/CommonEnumerations";
import { ISoeGridOptions } from "../../../../Util/SoeGridOptions";
import { WidgetControllerBase } from "../../Base/WidgetBase";

export class SystemInfoGaugeDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {
        return WidgetControllerBase.getWidgetDirective(urlHelperService.getWidgetUrl('SystemInfo', 'SystemInfoGauge.html'), SystemInfoGaugeController);
    }
}

class SystemInfoGaugeController extends WidgetControllerBase {

    private soeGridOptions: ISoeGridOptions;

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
        this.widgetCss = 'col-sm-6';

        var keys: string[] = [
            "common.dashboard.systeminfo.title",
            "common.dashboard.systeminfo.date",
            "common.dashboard.systeminfo.typename",
            "common.dashboard.systeminfo.message",
            "core.deleterow",
            "common.dashboard.systeminfo.skill",
            "common.dashboard.systeminfo.activatedschema",
            "common.dashboard.systeminfo.timestamp",
            "common.dashboard.systeminfo.orderplanning",
            "common.dashboard.systeminfo.remaindermail",
            "common.dashboard.systeminfo.personsskill",
            "common.dashboard.systeminfo.expires",
            "common.dashboard.systeminfo.days",
            "common.dashboard.systeminfo.schemaexpires",
            "common.dashboard.systeminfo.employeewithemployeenumber",
            "common.dashboard.systeminfo.hasnoschema",
            "common.dashboard.systeminfo.notfound",
            "common.dashboard.systeminfo.haspreliminaryshift",
            "common.dashboard.systeminfo.order",
            "common.dashboard.systeminfo.hasplannedtimenotscheduled",
            "common.dashboard.systeminfo.sick",
            "common.dashboard.systeminfo.sendedremaindermails",
            "common.dashboard.systeminfo.experiencemonths",
            "common.dashboard.systeminfo.birthday"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
            this.widgetTitle = terms["common.dashboard.systeminfo.title"];

            this.soeGridOptions.addColumnDate("date", terms["common.dashboard.systeminfo.date"], "20%");
            this.soeGridOptions.addColumnText("typeName", terms["common.dashboard.systeminfo.typename"], "20%");
            this.soeGridOptions.addColumnText("text", terms["common.dashboard.systeminfo.message"], null, false, "messageTooltip");
            this.soeGridOptions.addColumnIcon("img", null, null, null, null, null, null, null, false, null, null, null, null);
            this.soeGridOptions.addColumnDelete(this.terms["core.deleterow"], "deleteMessage");
        });
    }

    protected load() {
        super.load();
        this.coreService.getSystemInfoWidgetData().then((x) => {
            _.forEach(x, (row) => {
                //set typenames
                switch (row.type) {
                    case SystemInfoType.EmployeeSkill_Ends:
                        row.typeName = this.terms["common.dashboard.systeminfo.skill"];
                        break;
                    case SystemInfoType.EmployeeSchedule_Ends:
                    case SystemInfoType.ClosePreliminaryTimeScheduleTemplateBlocks:
                        row.typeName = this.terms["common.dashboard.systeminfo.activatedschema"];
                        break;
                    case SystemInfoType.TimeStamp_EmployeeMissing:
                    case SystemInfoType.TimeStamp_TimeScheduleTemplatePeriodMissing:
                        row.typeName = this.terms["common.dashboard.systeminfo.timestamp"];
                        break;
                    case SystemInfoType.ReminderOrderSchedule:
                        row.typeName = this.terms["common.dashboard.systeminfo.orderplanning"];
                        break;
                    case SystemInfoType.ReminderIllness:
                        row.typeName = this.terms["common.dashboard.systeminfo.remaindermail"];
                        break;
                }
                //Tooltips
                if (row.text.length > 0) {
                    switch (row.type) {
                        case SystemInfoType.EmployeeSkill_Ends:
                            row.messageTooltip = row.text.format(this.terms["common.dashboard.systeminfo.personsskill"], this.terms["common.dashboard.systeminfo.expires"], this.terms["common.dashboard.systeminfo.days"]);
                            row.text = row.messageTooltip;
                            break;
                        case SystemInfoType.EmployeeSchedule_Ends:
                            row.messageTooltip = row.text.format(this.terms["common.dashboard.systeminfo.schemaexpires"], this.terms["common.dashboard.systeminfo.days"]);
                            row.text = row.messageTooltip;
                            break;
                        case SystemInfoType.TimeStamp_TimeScheduleTemplatePeriodMissing:
                            row.messageTooltip = row.text.format(this.terms["common.dashboard.systeminfo.employeewithemployeenumber"], this.terms["common.dashboard.systeminfo.hasnoschema"]);
                            row.text = row.messageTooltip;
                            break;
                        case SystemInfoType.TimeStamp_EmployeeMissing:
                            row.messageTooltip = row.text.format(this.terms["common.dashboard.systeminfo.employeewithemployeenumber"], this.terms["common.dashboard.systeminfo.notfound"]);
                            row.text = row.messageTooltip;
                            break;
                        case SystemInfoType.ClosePreliminaryTimeScheduleTemplateBlocks:
                            row.messageTooltip = row.text.format(this.terms["common.dashboard.systeminfo.haspreliminaryshift"], this.terms["common.dashboard.systeminfo.days"]);
                            row.text = row.messageTooltip;
                            break;
                        case SystemInfoType.ReminderOrderSchedule:
                            row.messageTooltip = row.text.format(this.terms["common.dashboard.systeminfo.order"], this.terms["common.dashboard.systeminfo.hasplannedtimenotscheduled"]);
                            row.text = row.messageTooltip;
                            break;
                        case SystemInfoType.ReminderIllness:
                            row.messageTooltip = row.text.format(this.terms["common.dashboard.systeminfo.sick"], this.terms["common.dashboard.systeminfo.sendedremaindermails"]);
                            row.text = row.messageTooltip;
                            break;
                    }
                }
                //setImages
                switch (row.logLevel) {
                    case SystemInfoLogLevel.Information:
                        row.img = "fal fa-info-circle infoColor";
                        break;
                    case SystemInfoLogLevel.Warning:
                        row.img = "fal fa-exclamation-circle warningColor";
                        break;
                    case SystemInfoLogLevel.Error:
                        row.img = "fal fa-exclamation-triangle errorColor";
                        break;
                }

            });
            this.soeGridOptions.setData(x);
            super.loadComplete(x.length);
        });
    }

    private deleteMessage(row) {
        super.load();
        this.coreService.deleteSystemInfoLogRow(row.systemInfoLogId).then(x => {
            this.reload();
        });
    }
}