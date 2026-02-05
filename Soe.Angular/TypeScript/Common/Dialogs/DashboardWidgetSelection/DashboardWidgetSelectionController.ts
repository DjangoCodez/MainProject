import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../Core/Services/UrlHelperService";
import { ISoeGridOptions, SoeGridOptions } from "../../../Util/SoeGridOptions";
import { ISysGaugeDTO, IUserGaugeDTO } from "../../../Scripts/TypeLite.Net4";
import { SoeModule } from "../../../Util/CommonEnumerations";

export class DashboardWidgetSelectionController {
    private title: string;
    private soeGridOptions: ISoeGridOptions;
    private sysGauges: ISysGaugeDTO[];
    private gauges: ISysGaugeDTO[];
    private onlyNotAdded: boolean = true;

    private _onlyCurrentModule: boolean = true;
    get onlyCurrentModule(): boolean {
        return this._onlyCurrentModule;
    }
    set onlyCurrentModule(value: boolean) {
        this._onlyCurrentModule = value;
        this.loadSysGauges();
    }

    //@ngInject
    constructor(
        private $uibModalInstance,
        private $window: ng.IWindowService,
        private $timeout: ng.ITimeoutService,
        private translationService: ITranslationService,
        private coreService: ICoreService,
        private urlHelperService: IUrlHelperService,
        private module: SoeModule,
        private userGauges: IUserGaugeDTO[],
        uiGridConstants) {
        this.loadSysGauges();
        this.soeGridOptions = new SoeGridOptions("Common.Dialogs.DashboardWidgetSelection", $timeout, uiGridConstants);
        this.setupGrid();
    }

    private setupGrid() {
        this.soeGridOptions.enableGridMenu = false;
        this.soeGridOptions.enableColumnMenus = false;
        this.soeGridOptions.enableFiltering = false;
        this.soeGridOptions.enableSorting = true;
        this.soeGridOptions.showGridFooter = true;
        this.soeGridOptions.setMinRowsToShow(10);

        // Columns
        var keys: string[] = [
            "common.dashboard.selectwidgets",
            "common.dashboard.panel"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.title = terms["common.dashboard.selectwidgets"]
            this.soeGridOptions.addColumnText("name", terms["common.dashboard.panel"], null);
        });
    }

    private loadSysGauges() {
        this.sysGauges = [];
        this.coreService.getSysGagues(this.onlyCurrentModule ? this.module : SoeModule.None).then(x => {
            _.each(x, (y) => {
                if (!_.find(this.sysGauges, sg => sg.name === y.name))
                    this.sysGauges.push(y);
            });
            this.updateGauges();
        });
    }

    private updateGauges() {
        this.gauges = this.sysGauges;
        if (this.gauges && this.onlyNotAdded) {
            this.gauges = this.gauges.filter(g => _.find(this.userGauges, ug => ug.sysGaugeId === g.sysGaugeId) == undefined);
        }
        this.soeGridOptions.setData(_.orderBy(this.gauges, 'name'));
    }

    private edit(row) {
        // Double click row
        this.soeGridOptions.selectRow(row);
        this.choose();
    }

    private close() {
        this.$uibModalInstance.dismiss('cancel');
    }

    private choose() {
        this.$uibModalInstance.close(this.soeGridOptions.getSelectedRows());
    }
}