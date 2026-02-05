import { ICoreService } from "../../../../Core/Services/CoreService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { SOEMessageBoxButtons, SOEMessageBoxImage, SOEMessageBoxSize } from "../../../../Util/Enumerations";
import { ISoeGridOptions } from "../../../../Util/SoeGridOptions";
import { WidgetControllerBase } from "../../Base/WidgetBase";

export class SoftOneStatusGaugeDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {
        return WidgetControllerBase.getWidgetDirective(urlHelperService.getWidgetUrl('SoftOneStatus', 'SoftOneStatusGauge.html'), SoftOneStatusGaugeController);
    }
}

class SoftOneStatusGaugeController extends WidgetControllerBase {

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
            "common.dashboard.softonestatus.title",
            "common.date",
            "common.subject",
            "core.show"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.widgetTitle = terms["common.dashboard.softonestatus.title"];

            this.soeGridOptions.addColumnDate("date", terms["common.date"], "25%");
            this.soeGridOptions.addColumnText("title", terms["common.subject"], null);
            this.soeGridOptions.addColumnIcon(null, "fal fa-file-search", terms["core.show"], "showNews");
        });
    }

    protected load() {
        super.load();
        //this.coreService.getCompanyNewsWidgetData().then(x => {
        //    this.soeGridOptions.setData(x);
        //    super.loadComplete(x.length);
        //});
    }

    private showNews(item) {
        this.notificationService.showDialog(item.title, item.text, SOEMessageBoxImage.None, SOEMessageBoxButtons.OK, SOEMessageBoxSize.Large);
    }
}