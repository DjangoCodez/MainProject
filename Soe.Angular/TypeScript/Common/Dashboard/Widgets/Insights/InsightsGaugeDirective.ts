import { IGridHandlerFactory } from "../../../../Core/Handlers/gridhandlerfactory";
import { MatrixGridController } from "../../../../Core/RightMenu/ReportMenu/Components/MatrixGrid/MatrixGridController";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { IReportService } from "../../../../Core/Services/ReportService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { ReportUserSelectionType, SettingDataType, SoeModule, SoeReportType } from "../../../../Util/CommonEnumerations";
import { UserGaugeSettingDTO } from "../../../Models/DashboardDTOs";
import { MatrixDefinition } from "../../../Models/MatrixResultDTOs";
import { MatrixColumnsSelectionDTO } from "../../../Models/ReportDataSelectionDTO";
import { WidgetControllerBase } from "../../Base/WidgetBase";

export class InsightsGaugeDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {
        return WidgetControllerBase.getWidgetDirective(urlHelperService.getWidgetUrl('Insights', 'InsightsGauge.html'), InsightsGaugeController);
    }
}

interface ModuleItem {
    id: SoeModule;
    name: string;
}

class SettingsHandler {
    private _module: ModuleItem;

    private _report;

    private _columnSelection: any;
    private _dataSelection: any;

    //arrays
    public modules: ModuleItem[];
    public reports: any[];
    public columnSelections: any[];
    public dataSelections: any[];

    //flags
    public enableReports = false;
    public enableSelections = false;

    //getters & setters
    get module() {
        return this._module;
    }
    set module(val: ModuleItem) {
        this._module = val;

        this.reports = [];
        this.dataSelections = [];
        this.columnSelections = [];

        this.report = undefined;
        this.dataSelection = undefined;
        this.columnSelection = undefined;

        this.enableReports = false;
        this.enableSelections = false;

        this.setValidCallback(this.isSettingsValid);
        this.loadReports();
    }

    get report() {
        return this._report;
    }
    set report(val) {
        this._report = val;

        this.dataSelections = [];
        this.columnSelections = [];

        this.dataSelection = undefined;
        this.columnSelection = undefined;

        this.enableSelections = false;

        this.setValidCallback(this.isSettingsValid);
        this.loadSelections();
    }

    get columnSelection() {
        return this._columnSelection;
    }
    set columnSelection(val) {
        this._columnSelection = val;
        this.setValidCallback(this.isSettingsValid);
    }

    get dataSelection() {
        return this._dataSelection;
    }
    set dataSelection(val) {
        this._dataSelection = val;
        this.setValidCallback(this.isSettingsValid);
    }

    get isSettingsValid(): boolean {
        return (!!this.module && !!this.report && !!this.dataSelection && !!this.columnSelection);
    }

    public constructor(
        private $q,
        private $timeout: ng.ITimeoutService,
        private reportService: IReportService,
        private setValidCallback: (val: boolean) => void,
    ) {}

    public setInitialModule(val) {
        this._module = val;
    }
    public setInitialReport(val) {
        this._report = val;
    }

    //#region Fetch
    private loadReports() {
        if (!this.module || !this.module.id) return;
        this.reportService.getReportsForMenu(this.module.id, SoeReportType.Analysis).then(menuItems => {
            this.reports = [...menuItems];
            this.$timeout(0).then(() => this.enableReports = true);
        })
    }

    private loadSelections() {
        if (!this.report || !this.report.reportId) return
        this.$q.all([
            this.loadColumnSelections(),
            this.loadDataSelections(),
        ]).then(() => {
            this.$timeout(0).then(() => this.enableSelections = true);
        })
    }

    private loadColumnSelections(): ng.IPromise<any> {
        return this.reportService.getReportUserSelections(this.report.reportId, ReportUserSelectionType.InsightsColumnSelection).then(selections => {
            this.columnSelections = [...selections];
        })
    }

    private loadDataSelections(): ng.IPromise<any> {
        return this.reportService.getReportUserSelections(this.report.reportId, ReportUserSelectionType.DataSelection).then(selections => {
            this.dataSelections = [...selections];
        })
    }
    //#endregion
}

class InsightsGaugeController extends WidgetControllerBase {

    private analysisJsonRows: any[];
    private matrixDefinition: MatrixDefinition;
    private showGraph: boolean = true;
    private matrixSelection: MatrixColumnsSelectionDTO;
    private selectedReportUserSelection: any;
    private insightSettings: SettingsHandler;
    private matrixGridHandler: MatrixGridController;

    //@ngInject
    constructor(
        $timeout: ng.ITimeoutService,
        $q: ng.IQService,
        private $scope: any,
        private $uibModal,
        private coreService: ICoreService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private reportService: IReportService,
        gridHandlerFactory: IGridHandlerFactory,
        uiGridConstants: uiGrid.IUiGridConstants)
        {
            super($timeout, $q, uiGridConstants);
            this.matrixGridHandler = new MatrixGridController($timeout, this.widgetId, gridHandlerFactory);
            this.insightSettings = new SettingsHandler(this.$q, this.$timeout, this.reportService, this.setIsSettingsValid);
        }

    private setIsSettingsValid(val: boolean) {
        this.widgetSettingsValid = val
        // console.log(this.widgetSettingsValid)
        // this.$scope.applyAsync(() => this.widgetSettingsValid = val);
    }

    protected setup(): ng.IPromise<any> {
        let deferral = this.$q.defer();

        this.widgetSettingsValid = true;
        this.widgetHasSettings = true;
        this.widgetCss = "col-sm-6";

        var keys: string[] = [
            "common.dashboard.reports.title",
            "common.created",
            "common.report.report.report",
            "common.dashboard.header.economy",
            "common.dashboard.header.billing",
            "common.dashboard.header.time",
            "core.download",
        ];


        this.translationService.translateMany(keys).then(terms => {
            let modules = [
                {name: terms["common.dashboard.header.time"], id: SoeModule.Time},
                {name: terms["common.dashboard.header.economy"], id: SoeModule.Economy},
                {name: terms["common.dashboard.header.billing"], id: SoeModule.Billing},
            ]
            this.insightSettings.modules = modules;

            this.loadSettings();
            deferral.resolve();
        });

        return deferral.promise;
    }

    protected load() {
        if (!this.insightSettings.isSettingsValid) {
            // this.widgetSettingsValid = false;
            return;
        }

        // this.widgetSettingsValid = true;
        super.load()

        return this.coreService.getInsightsWidgetData(this.insightSettings.report.reportId, this.insightSettings.dataSelection.id, this.insightSettings.columnSelection.id).then(data => {
            const lowerCase = str => str[0].toLowerCase() + str.slice(1);
            const objectKeysToLowerCase = function (input) {
                if (typeof input !== 'object') return input;
                if (Array.isArray(input)) return input.map(objectKeysToLowerCase);
                return Object.keys(input).reduce(function (newObj, key) {
                    let val = input[key];
                    let newVal = (typeof val === 'object') && val !== null ? objectKeysToLowerCase(val) : val;
                    newObj[lowerCase(key)] = newVal;
                    return newObj;
                }, {});
            };


            let result = JSON.parse(data.json);

            this.analysisJsonRows = result.jsonRows;
            this.selectedReportUserSelection = JSON.parse(data.selection).map(s => objectKeysToLowerCase(s));
            this.matrixSelection = <MatrixColumnsSelectionDTO>this.selectedReportUserSelection.find(s => s.typeName === 'MatrixColumnsSelectionDTO');


            this.matrixDefinition = new MatrixDefinition();
            angular.extend(this.matrixDefinition, result.matrixDefinition);
            this.matrixDefinition.setTypes();

            if (this.showGraph) {
                this.$timeout(() => {
                    this.$scope.$broadcast('refreshChart');
                });
            } else {
                this.matrixGridHandler.setupMatrixGrid(this.matrixDefinition).then(() => {
                    this.matrixGridHandler.setData(this.analysisJsonRows);
                })
            }
            super.loadComplete(this.analysisJsonRows.length)
        });
    }


    //#region Settings
    public loadSettings() {
        const module = this.getUserGaugeSetting("Module");
        module && (this.insightSettings.setInitialModule({id: module.intData, name: module.strData}))

        const report = this.getUserGaugeSetting("Report");
        report && (this.insightSettings.setInitialReport({reportId: report.intData, name: report.strData}))

        const dataSelection = this.getUserGaugeSetting("DataSelection");
        dataSelection && (this.insightSettings.dataSelection = {id: dataSelection.intData, name: dataSelection.strData})

        const columnSelection = this.getUserGaugeSetting("ColumnSelection");
        columnSelection && (this.insightSettings.columnSelection = {id: columnSelection.intData, name: columnSelection.strData})

        const showAsGraph = this.getUserGaugeSetting("ShowAsGraph");
        showAsGraph && (this.showGraph = showAsGraph.boolData)

        const title = this.getUserGaugeSetting("Title");
        title && (this.widgetTitle = title.strData || " ")
    }

    public saveSettings() {
        if (!this.insightSettings.isSettingsValid) {
            // this.widgetSettingsValid = false;
            return
        }

        let settings: UserGaugeSettingDTO[] = [];

        let moduleSetting = new UserGaugeSettingDTO("Module", SettingDataType.Integer)
        moduleSetting.intData = this.insightSettings.module.id;
        moduleSetting.strData = this.insightSettings.module.name;
        settings.push(moduleSetting);

        let reportSetting = new UserGaugeSettingDTO("Report", SettingDataType.Integer)
        reportSetting.intData = this.insightSettings.report.reportId;
        reportSetting.strData = this.insightSettings.report.name;
        settings.push(reportSetting);

        let dataSelectionSetting = new UserGaugeSettingDTO("DataSelection", SettingDataType.Integer)
        dataSelectionSetting.intData = this.insightSettings.dataSelection.id;
        dataSelectionSetting.strData = this.insightSettings.dataSelection.name;
        settings.push(dataSelectionSetting);

        let columnSelectionSetting = new UserGaugeSettingDTO("ColumnSelection", SettingDataType.Integer)
        columnSelectionSetting.intData = this.insightSettings.columnSelection.id;
        columnSelectionSetting.strData = this.insightSettings.columnSelection.name;
        settings.push(columnSelectionSetting);

        let showAsGraphSetting = new UserGaugeSettingDTO("ShowAsGraph", SettingDataType.Boolean)
        showAsGraphSetting.boolData = this.showGraph;
        settings.push(showAsGraphSetting);

        let titleSetting = new UserGaugeSettingDTO("Title", SettingDataType.String)
        titleSetting.strData = this.widgetTitle;
        settings.push(titleSetting);

        this.coreService.saveUserGaugeSettings(this.widgetUserGauge.userGaugeId, settings).then(result => {
            if (result.success) {
                this.widgetUserGauge.userGaugeSettings = settings;
            }
        });
    }
    //#endregion

    //#region Helpers
    private lowerCase = str => str[0].toLowerCase() + str.slice(1);

    private convertKeysToLowerCase(obj) {
        var output = {};
        for (let i in obj) {
            if (Object.prototype.toString.apply(obj[i]) === '[object Object]') {
               output[this.lowerCase(i)] = this.convertKeysToLowerCase(obj[i]);
            }else if(Object.prototype.toString.apply(obj[i]) === '[object Array]'){
                output[this.lowerCase(i)]=[];
                output[this.lowerCase(i)].push(this.convertKeysToLowerCase(obj[i][0]));
            } else {
                output[this.lowerCase(i)] = obj[i];
            }
        }
        return output;
    };

    private objectKeysToLowerCase = function (input) {
        if (typeof input !== 'object') return input;
        if (Array.isArray(input)) return input.map(this.objectKeysToLowerCase);
        return Object.keys(input).reduce(function (newObj, key) {
            let val = input[key];
            let newVal = (typeof val === 'object') && val !== null ? this.objectKeysToLowerCase(val) : val;
            newObj[key.toLowerCase()] = newVal;
            return newObj;
        }, {});
    };
    //#endregion
}