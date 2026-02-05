import { ICoreService } from "../../../../Core/Services/CoreService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { ISmallGenericType } from "../../../../Scripts/TypeLite.Net4";
import { CalendarUtility } from "../../../../Util/CalendarUtility";
import { SettingDataType, TermGroup, TermGroup_PerformanceTestInterval } from "../../../../Util/CommonEnumerations";
import { DashboardStatisticPeriodDTO, DashboardStatisticsDTO, DashboardStatisticType, UserGaugeSettingDTO } from "../../../Models/DashboardDTOs";
import { SmallGenericType } from "../../../Models/SmallGenericType";
import { WidgetControllerBase } from "../../Base/WidgetBase";

export class PerformanceAnalyzerGaugeDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {
        return WidgetControllerBase.getWidgetDirective(urlHelperService.getWidgetUrl('PerformanceAnalyzer', 'PerformanceAnalyzerGauge.html'), PerformanceAnalyzerGaugeController);
    }
}

class PerformanceAnalyzerGaugeController extends WidgetControllerBase {

    // Terms
    private terms: { [index: string]: string; };
    private serviceTypeName: string;
    private testLabel: string;
    private yAxisLabel: string;

    // Collections
    private serviceTypes: SmallGenericType[];
    private statisticTypes: DashboardStatisticType[];
    private servers: string[];
    private tests: string[];
    private intervals: ISmallGenericType[];
    private testResult: DashboardStatisticsDTO;

    // Settings
    private serviceType: number;

    // Parameters
    private selectedServer: string;
    private selectedTest: string;
    private statisticType: DashboardStatisticType;
    private interval: TermGroup_PerformanceTestInterval = TermGroup_PerformanceTestInterval.Hour;
    private dateFrom: Date;
    private dateTo: Date;

    // Flags
    private loading: boolean = false;

    // Properties
    private testResultChartOptions: any;
    private testResultChartData: any[] = [];

    private get isServiceTypeSelenium(): boolean {
        return (this.serviceType === 26);
    }

    private get isIntervalHour(): boolean {
        return this.interval === TermGroup_PerformanceTestInterval.Hour;
    }

    private get isIntervalDay(): boolean {
        return this.interval === TermGroup_PerformanceTestInterval.Day;
    }

    private get canLoad(): boolean {
        return !this.loading && this.statisticType && this.dateFrom && this.dateTo && this.dateFrom.isSameOrBeforeOnDay(this.dateTo);
    }

    //@ngInject
    constructor(
        $timeout: ng.ITimeoutService,
        $q: ng.IQService,
        private $window: ng.IWindowService,
        private coreService: ICoreService,
        private translationService: ITranslationService) {
        super($timeout, $q, null);
    }

    protected setup(): ng.IPromise<any> {
        let deferral = this.$q.defer();

        this.widgetHasSettings = true;
        this.widgetHasRecordCount = false;
        this.widgetCss = 'col-sm-12';
        this.loadSettings();

        this.dateFrom = this.dateTo = new Date();

        this.$q.all([
            this.loadTerms(),
            this.loadIntervals(),
            this.loadServiceTypes(),
            this.loadTypes()
        ]).then(() => {
            this.setServiceTypeName();
            deferral.resolve();
        });

        return deferral.promise;
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "core.time.seconds",
            "core.time.milliseconds",
            "common.chart.nodata",
            "common.dashboard.performanceanalyzer.result",
            "common.dashboard.performanceanalyzer.test",
            "common.dashboard.performanceanalyzer.title",
            "common.dashboard.performanceanalyzer.xaxis",
            "common.dashboard.performanceanalyzer.yaxis",
            "common.dashboard.performanceanalyzer.data.min",
            "common.dashboard.performanceanalyzer.data.max",
            "common.dashboard.performanceanalyzer.data.average",
            "common.dashboard.performanceanalyzer.data.median",
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
            this.widgetTitle = this.terms["common.dashboard.performanceanalyzer.title"];
        });
    }

    private loadIntervals(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.PerformanceTestInterval, false, true, true).then(x => {
            this.intervals = x;
        });
    }

    private loadServiceTypes(): ng.IPromise<any> {
        return this.coreService.getDashboardStatisticServiceTypes().then(x => {
            this.serviceTypes = x;
        });
    }

    private loadTypes(): ng.IPromise<any> {
        return this.coreService.getDashboardStatisticTypes(this.serviceType).then(x => {
            this.statisticTypes = x;

            if (this.isServiceTypeSelenium) {
                this.servers = [];
                this.tests = [];
                let typeNames: string[] = _.map(this.statisticTypes, t => t.name);
                _.forEach(typeNames, name => {
                    let parts = name.split('#');
                    if (parts.length === 2) {
                        if (!_.includes(this.servers, parts[1]))
                            this.servers.push(parts[1]);
                        if (!_.includes(this.tests, parts[0]))
                            this.tests.push(parts[0]);
                    }
                });
                this.servers.sort();
            } else {
                this.tests = this.statisticTypes.map(s => s.name);
                if (this.tests.length === 1) {
                    this.selectedTest = this.tests[0];
                    this.setStatisticType();
                }
            }

            this.tests.sort();
        })
    }

    protected load() {
        if (!this.canLoad)
            return;

        this.loading = true;
        super.load();

        return this.coreService.getPerformanceTestResults(this.statisticType.key, this.dateFrom, this.dateTo, this.interval).then(x => {
            this.testResult = x;
            this.createTestResultChartData();
            super.loadComplete(0);
            this.loading = false;
        })
    }

    public loadSettings() {
        var setting: UserGaugeSettingDTO = this.getUserGaugeSetting('ServiceType');
        this.serviceType = (setting ? setting.intData : 26);    // 26 = SoftOne.Status.Shared.ServiceType.Selenium
        this.widgetSettingsValid = true;
    }

    public saveSettings() {
        this.statisticType = null;
        this.testResultChartData = [];
        this.testResult = null;

        var settings: UserGaugeSettingDTO[] = [];
        var setting = new UserGaugeSettingDTO('ServiceType', SettingDataType.Integer);
        setting.intData = this.serviceType;
        settings.push(setting);

        this.coreService.saveUserGaugeSettings(this.widgetUserGauge.userGaugeId, settings).then(result => {
            if (result.success) {
                this.widgetUserGauge.userGaugeSettings = settings;
                this.loadTypes();
                this.setServiceTypeName();
            }
        });
    }

    // EVENTS

    private setStatisticType() {
        this.$timeout(() => {
            this.statisticType = null;
            if (this.isServiceTypeSelenium) {
                if (this.selectedServer && this.selectedTest) {
                    this.statisticType = _.find(this.statisticTypes, t => t.name === "{0}#{1}".format(this.selectedTest, this.selectedServer));
                }
            } else {
                if (this.selectedTest) {
                    this.statisticType = _.find(this.statisticTypes, t => t.name === this.selectedTest);
                }
            }
        });
    }

    // CHART

    private setTestResultChartOptions() {
        // Chart options
        var chart: any = {};
        chart.type = 'lineChart';
        chart.height = 500;
        chart.margin = { top: 0, right: -1, bottom: 40, left: 40 };
        chart.noData = this.terms["common.chart.nodata"];
        chart.showControls = false;
        chart.showLegend = true;
        chart.showValues = true;
        chart.showXAxis = true;
        chart.showYAxis = true;
        chart.reduceXTicks = false;
        chart.xScale = d3.time.scale();
        chart.clipEdge = false;
        chart.focusEnable = false;
        chart.useInteractiveGuideline = true;
        chart.tooltip = {
            contentGenerator: (e) => {
                var series = e.series[0];
                if (series.value === null)
                    return;

                let data;
                if (e.data)
                    data = e.data;
                else if (e.point)
                    data = e.point;
                else
                    return;

                let date: Date = CalendarUtility.convertToDate(data.x);
                let dateString: string = this.isIntervalHour ? date.toFormattedTime() : date.toFormattedDate();

                var header =
                    "<thead>" +
                    "<tr>" +
                    "<td colspan='3' class='key'><strong>" + dateString + "</strong></td>" +
                    "</tr>" +
                    "</thead>";

                var rows =
                    "<tr>" +
                    "<td class='legend-color-guide'><div style='background-color: " + series.color + ";'></div></td>" +
                    "<td class='key'>" + series.key + "</td>" +
                    "<td class='x-value'>" + (parseFloat(series.value).round(2).toLocaleString()) + "</td>" +
                    "</tr>";

                return "<table>" +
                    header +
                    "<tbody>" +
                    rows +
                    "</tbody>" +
                    "</table>";
            }
        }
        if (this.isIntervalHour) {
            chart.xAxis = {
                tickFormat: (d: number) => { return new Date(d).toFormattedTime(); },
                ticks: d3.time.hours,
                staggerLabels: true,
            };
        } else {
            chart.xAxis = {
                tickFormat: (d: number) => { return new Date(d).toFormattedDate(); },
                ticks: d3.time.days,
            };
        }
        //chart.xAxis.axisLabel = this.terms["common.dashboard.performanceanalyzer.xaxis"];
        chart.xAxis.showMaxMin = false;

        chart.yAxis = {
            //axisLabel: this.terms["common.dashboard.performanceanalyzer.yaxis"],
            tickFormat: function (d) { return parseFloat(d).round(1); },
            showMaxMin: false,
        };

        // Set options to chart
        this.testResultChartOptions = {};
        this.testResultChartOptions.chart = chart;
    }

    private createTestResultChartData() {
        var minData = [];
        var maxData = [];
        var averageData = [];
        var medianData = [];

        _.forEach(this.testResult.dashboardStatisticRows, row => {
            if (row.name === 'Min')
                this.createPeriodData(minData, row.dashboardStatisticPeriods);
            else if (row.name === 'Max')
                this.createPeriodData(maxData, row.dashboardStatisticPeriods);
            else if (row.name === 'Average')
                this.createPeriodData(averageData, row.dashboardStatisticPeriods);
            else if (row.name === 'Median')
                this.createPeriodData(medianData, row.dashboardStatisticPeriods);
        });

        this.testResultChartData = [];
        this.testResultChartData.push({ values: minData, key: this.terms["common.dashboard.performanceanalyzer.data.min"], color: 'rgba(0, 255, 0, 1.0)', type: 'line', yAxis: 1 });
        this.testResultChartData.push({ values: maxData, key: this.terms["common.dashboard.performanceanalyzer.data.max"], color: 'rgba(255, 0, 0, 1.0)', type: 'line', yAxis: 1 });
        this.testResultChartData.push({ values: averageData, key: this.terms["common.dashboard.performanceanalyzer.data.average"], color: 'rgba(0, 0, 255, 1.0)', type: 'line', yAxis: 1 });
        this.testResultChartData.push({ values: medianData, key: this.terms["common.dashboard.performanceanalyzer.data.median"], color: 'rgba(128, 128, 128, 1.0)', type: 'line', yAxis: 1 });

        this.setTestResultChartOptions();
    }

    private createPeriodData(arr: any[], periods: DashboardStatisticPeriodDTO[]) {
        let useSeconds: boolean = this.isServiceTypeSelenium;

        _.forEach(periods, period => {
            arr.push({ x: period.from, y: period.value / (useSeconds ? 1000 : 1) });
        });
    }

    // HELP-METHODS

    private setServiceTypeName() {
        this.serviceTypeName = (this.serviceType && this.serviceTypes) ? _.find(this.serviceTypes, t => t.id === this.serviceType).name : '';
        this.widgetTitle = this.terms["common.dashboard.performanceanalyzer.title"] + ' ' + this.serviceTypeName;
        this.testLabel = this.isServiceTypeSelenium ? this.terms["common.dashboard.performanceanalyzer.test"] : this.terms["common.dashboard.performanceanalyzer.result"];
        this.yAxisLabel = this.isServiceTypeSelenium ? this.terms["core.time.seconds"] : this.terms["core.time.milliseconds"];
    }
}