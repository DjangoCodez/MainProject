import { ICoreService } from "../../../../Core/Services/CoreService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { SoeReportTemplateType } from "../../../../Util/CommonEnumerations";
import { CoreUtility } from "../../../../Util/CoreUtility";
import { ISoeGridOptions } from "../../../../Util/SoeGridOptions";
import { WidgetControllerBase } from "../../Base/WidgetBase";

export class ReportGaugeDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {
        return WidgetControllerBase.getWidgetDirective(urlHelperService.getWidgetUrl('Report', 'ReportGauge.html'), ReportGaugeController);
    }
}

class ReportGaugeController extends WidgetControllerBase {

    private soeGridOptions: ISoeGridOptions;

    //@ngInject
    constructor(
        $timeout: ng.ITimeoutService,
        $q: ng.IQService,
        private $uibModal,
        private coreService: ICoreService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        uiGridConstants: uiGrid.IUiGridConstants) {
        super($timeout, $q, uiGridConstants);

        this.soeGridOptions = this.createGrid();
    }

    protected setup(): ng.IPromise<any> {
        var keys: string[] = [
            "common.dashboard.reports.title",
            "common.created",
            "common.report.report.report",
            "core.download"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.widgetTitle = terms["common.dashboard.reports.title"];

            this.soeGridOptions.addColumnDateTime("created", terms["common.created"], "25%");
            this.soeGridOptions.addColumnText("reportName", terms["common.report.report.report"], null);
            this.soeGridOptions.addColumnIcon(null, "fal fa-file-download", terms["core.downloadfile"], "GetReport");
        });
    }

    protected load() {
        super.load();
        this.coreService.getReportWidgetData().then(x => {
            this.soeGridOptions.setData(x);
            super.loadComplete(x.length);
        });
    }

    private GetReport(item) {
        var reportPrintoutId = item.reportPrintoutId;
        var userid = CoreUtility.userId;
        var reportUrl: string = "/ajax/downloadReport.aspx?templatetype=" + SoeReportTemplateType.Unknown + "&reportprintoutid=" + reportPrintoutId + "&reportuserid=" + userid;
        window.open(reportUrl, '_blank');
    }
}