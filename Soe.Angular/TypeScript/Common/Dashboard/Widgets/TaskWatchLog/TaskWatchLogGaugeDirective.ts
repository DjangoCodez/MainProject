import { ICoreService } from "../../../../Core/Services/CoreService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { ISmallGenericType } from "../../../../Scripts/TypeLite.Net4";
import { CalendarUtility } from "../../../../Util/CalendarUtility";
import { TermGroup, TermGroup_PerformanceTestInterval, TermGroup_TaskWatchLogResultCalculationType } from "../../../../Util/CommonEnumerations";
import { DashboardStatisticPeriodDTO, DashboardStatisticsDTO } from "../../../Models/DashboardDTOs";
import { WidgetControllerBase } from "../../Base/WidgetBase";

export class TaskWatchLogGaugeDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {
        return WidgetControllerBase.getWidgetDirective(urlHelperService.getWidgetUrl('TaskWatchLog', 'TaskWatchLogGauge.html'), TaskWatchLogGaugeController);
    }
}

class TaskWatchLogGaugeController extends WidgetControllerBase {

    // Terms
    private terms: { [index: string]: string; };

    // Collections
    private tasks: any[];
    private intervals: ISmallGenericType[];
    private calculationTypes: ISmallGenericType[];
    private taskResult: DashboardStatisticsDTO;

    // Parameters
    private dateFrom: Date;
    private dateTo: Date;
    private selectedCompany: number;
    private selectedUser: number;
    private selectedTask: any;
    private interval: TermGroup_PerformanceTestInterval = TermGroup_PerformanceTestInterval.Hour;
    private calculationType: TermGroup_TaskWatchLogResultCalculationType = TermGroup_TaskWatchLogResultCalculationType.Record;

    // Flags
    private loadingTasks: boolean = false;
    private loading: boolean = false;

    // Properties
    private taskResultChartOptions: any;
    private taskResultChartData: any[] = [];

    private get isIntervalHour(): boolean {
        return this.interval === TermGroup_PerformanceTestInterval.Hour;
    }

    private get isIntervalDay(): boolean {
        return this.interval === TermGroup_PerformanceTestInterval.Day;
    }

    private get canLoadTasks(): boolean {
        return !this.loadingTasks && this.dateFrom && this.dateTo && this.dateFrom.isSameOrBeforeOnDay(this.dateTo);
    }

    private get canLoad(): boolean {
        return !this.loading && this.selectedTask && this.dateFrom && this.dateTo && this.dateFrom.isSameOrBeforeOnDay(this.dateTo);
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

        this.widgetHasSettings = false;
        this.widgetHasRecordCount = false;
        this.widgetCss = 'col-sm-12';
        this.loadSettings();

        this.dateFrom = this.dateTo = new Date();

        this.$q.all([
            this.loadTerms(),
            this.loadIntervals(),
            this.loadCalculationTypes()
        ]).then(() => {
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
            "common.dashboard.taskwatchlog.title",
            "common.dashboard.performanceanalyzer.data.min",
            "common.dashboard.performanceanalyzer.data.max",
            "common.dashboard.performanceanalyzer.data.average",
            "common.dashboard.performanceanalyzer.data.median",
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
            this.widgetTitle = this.terms["common.dashboard.taskwatchlog.title"];
        });
    }

    private loadIntervals(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.PerformanceTestInterval, false, true, true).then(x => {
            this.intervals = x;
        });
    }

    private loadCalculationTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.TaskWatchLogResultCalculationType, false, false, true).then(x => {
            this.calculationTypes = x;
        });
    }

    private loadTasks(): ng.IPromise<any> {
        return this.coreService.getTaskWatchLogTasks(this.dateFrom, this.dateTo, this.selectedCompany, this.selectedUser).then(x => {
            this.tasks = x.map(t => {
                return { name: t, displayName: _.last(t.split('_')) };
            });

            this.tasks = _.sortBy(this.tasks, 'displayName');
        });
    }

    protected load() {
        if (!this.canLoad)
            return;

        this.loading = true;
        super.load();

        return this.coreService.getTaskWatchLogResults(this.selectedTask.name, this.dateFrom, this.dateTo, this.interval, this.calculationType, this.selectedCompany, this.selectedUser).then(x => {
            this.taskResult = x;
            this.createTaskResultChartData();
            super.loadComplete(0);
            this.loading = false;
        })
    }

    public loadSettings() {
    }

    public saveSettings() {
    }

    // CHART

    private setTaskResultChartOptions() {
        // Chart options
        var chart: any = {};
        chart.type = 'multiBarChart';
        chart.stacked = false;
        chart.height = 500;
        chart.margin = { top: 0, right: -1, bottom: 40, left: 40 };
        chart.noData = this.terms["common.chart.nodata"];
        chart.showControls = false;
        chart.showLegend = true;
        chart.showValues = true;
        chart.reduceXTicks = false;
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
        chart.xAxis.showMaxMin = false;

        chart.yAxis = {
            tickFormat: function (d) { return parseFloat(d).round(1); },
            showMaxMin: true,
        };

        // Set options to chart
        this.taskResultChartOptions = {};
        this.taskResultChartOptions.chart = chart;
    }

    private createTaskResultChartData() {
        var minData = [];
        var maxData = [];
        var averageData = [];
        var medianData = [];

        _.forEach(this.taskResult.dashboardStatisticRows, row => {
            if (row.name === 'Min')
                this.createPeriodData(minData, row.dashboardStatisticPeriods);
            else if (row.name === 'Max')
                this.createPeriodData(maxData, row.dashboardStatisticPeriods);
            else if (row.name === 'Average')
                this.createPeriodData(averageData, row.dashboardStatisticPeriods);
            else if (row.name === 'Median')
                this.createPeriodData(medianData, row.dashboardStatisticPeriods);
        });

        this.taskResultChartData = [];
        this.taskResultChartData.push({ values: minData, key: this.terms["common.dashboard.performanceanalyzer.data.min"], color: 'rgba(0, 255, 0, 1.0)', type: 'bar', yAxis: 1 });
        this.taskResultChartData.push({ values: maxData, key: this.terms["common.dashboard.performanceanalyzer.data.max"], color: 'rgba(255, 0, 0, 1.0)', type: 'bar', yAxis: 1 });
        this.taskResultChartData.push({ values: averageData, key: this.terms["common.dashboard.performanceanalyzer.data.average"], color: 'rgba(0, 0, 255, 1.0)', type: 'bar', yAxis: 1 });
        this.taskResultChartData.push({ values: medianData, key: this.terms["common.dashboard.performanceanalyzer.data.median"], color: 'rgba(128, 128, 128, 1.0)', type: 'bar', yAxis: 1 });

        this.setTaskResultChartOptions();
    }

    private createPeriodData(arr: any[], periods: DashboardStatisticPeriodDTO[]) {
        _.forEach(periods, period => {
            arr.push({ x: period.from, y: period.value });
        });
    }
}