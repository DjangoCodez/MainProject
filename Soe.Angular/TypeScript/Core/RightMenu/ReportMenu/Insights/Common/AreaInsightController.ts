import { AgChartOptionsArea, AgChartUtility } from "../../../../../Util/ag-chart/AgChartUtility";
import { CalendarUtility } from "../../../../../Util/CalendarUtility";
import { StringUtility } from "../../../../../Util/StringUtility";
import { ITranslationService } from "../../../../Services/TranslationService";
import { InsightControllerBase } from "./InsightControllerBase";

declare var agCharts;

export class AreaInsightController extends InsightControllerBase {
    public static component(): ng.IComponentOptions {
        return {
            controller: AreaInsightController,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/Insights/Common/AreaInsight.html",
            bindings: {
                userSelection: "=",
                matrixSelection: '<',
                rows: '<',
                hideTitle: "=",
                containerId: "=",
            }
        };
    }

    public static componentKey = "areaInsight";

    private xKey: string;
    private yKeys: string[];
    private altKeys: string[];

    //@ngInject
    constructor(
        $scope: ng.IScope,
        $timeout: ng.ITimeoutService,
        translationService: ITranslationService) {

        super($scope, $timeout, translationService);
    }

    public $onInit() {
        this.init();
        super.setup().then(() => {
            this.createChart();
        });
    }

    protected createColumns() {
        super.createColumns();
    }

    // CHART

    protected setChartData() {
        this.chartData = [];

        // Only supports date as first column
        if (!this.isColumnDate() || this.columns.length < 2)
            return;

        let primaryColumnName: string = this.getColumnName(0);
        let secondaryColumnName: string = this.getColumnName(1);
        let altColumnName: string = "";
        let altKeysGroup: _.Dictionary<any[]> = null;
        if (this.getColumnName(2))
            altColumnName = this.getColumnName(2);

        this.xKey = primaryColumnName;

        let xKeyGroup: _.Dictionary<any[]> = _.groupBy(this.rows, r => r[primaryColumnName]);
        let xKeys: string[] = _.orderBy(Object.keys(xKeyGroup), k => k);

        let yKeysGroup: _.Dictionary<any[]> = _.groupBy(this.rows, r => r[secondaryColumnName]);
        this.yKeys = Object.keys(yKeysGroup);

        if (altColumnName != "") { 
            altKeysGroup = _.groupBy(this.rows, r => r[altColumnName]);
            this.altKeys = Object.keys(altKeysGroup);
        }   

        for (let label of xKeys) {
            let tot = 0;
            for (let yKey of this.yKeys) {
                let value = 0;
                if (altColumnName != "") {
                    value = this.rows.filter(r => r[primaryColumnName] === label && r[secondaryColumnName].toString() === yKey).reduce(function (a, b) {
                        return a + b[altColumnName];
                    }, 0);
                } else {
                    value = this.rows.filter(r => r[primaryColumnName] === label && r[secondaryColumnName].toString() === yKey).length || 0;
                }
                tot += value;
                this.addChartData(CalendarUtility.convertToDate(label), yKey, value);
            }
            this.addChartData(CalendarUtility.convertToDate(label), 'TOT', Math.round(tot *100)/100 );
        }
    }

    protected setChartOptions() {
        let options: AgChartOptionsArea = new AgChartOptionsArea();
        super.setupContainer(options);
        let tickCount = this.getTickCount();

        options.axes = [
            {
                position: 'left',
                type: 'number'
                //label: {
                //    formatter: function (params) {
                //        return NumberUtility.printDecimal(params.value, 0, 0)
                //    }
                //}
            },
            {
                position: 'bottom',
                type: 'time',
                label: {
                    rotation: 320,
                    formatter: function (params) {
                        return CalendarUtility.toFormattedDate(params.value)
                    }
                },
                tick: {
                    count: tickCount
                }
            }
        ];
        options.paddingLeft = 50;

        if (this.isColumnInteger(1) || this.isColumnDecimal(1))
            this.yKeys = _.orderBy(this.yKeys, y => StringUtility.toFloat(y));
        else
            this.yKeys = _.orderBy(this.yKeys, y => y);

        _.forEach(this.yKeys, key => {
            this.addChartSeries(options, key);
        });

        return AgChartUtility.createDefaultAreaChart(this.chartElem, this.chartData, options);
    }

    // HELP-METHODS

    private addChartData(date: Date, key: any, value: number) {
        let data = this.chartData.find(d => CalendarUtility.convertToDate(d[this.xKey]).isSameDayAs(date));
        if (data) {
            if (data[key])
                data[key] += value;
            else
                data[key] = value;
        } else {
            this.chartData.push({ [this.xKey]: date, [key]: value });
        }
    }

    private addChartSeries(options, key: any) {
        if (!options.series)
            options.series = [];

        options.series.push({
            type: 'area',
            xKey: this.xKey,
            yKey: key,
            yName: key,
            stacked: true,
            tooltip: {
                renderer: function (params) {
                    return `<div class='ag-chart-tooltip-title' style='background-color: ${params.color}; color: #FFFFFF'>${CalendarUtility.toFormattedDate(params.datum[params.xKey])}</div>` +
                        `<div class='ag-chart-tooltip-content' style='border-top: 1px solid #CCCCCC; text-align: right;'>${params.yKey}: ${params.datum[params.yKey]}</div>` +
                        `<div class='ag-chart-tooltip-content' style = 'border-top: 1px solid #CCCCCC; text-align: right;' >Tot: ${params.datum['TOT']} </div>`;
                }
            },
            highlightStyle: {
                fill: '#EEEEEE',
                stroke: '#CCCCCC'
            }
        });
    }

    private getTickCount() {
        let tc; //default is 10
        let dateFrom = CalendarUtility.convertToDate(this.chartData[0].date);
        let dateTo = CalendarUtility.convertToDate(this.chartData[this.chartData.length - 1].date);
        let dateDiff = CalendarUtility.getDaysBetweenDates(dateFrom, dateTo);

        if (dateDiff <= 31) {
            tc = agCharts.time.day;
        }
        else if (dateDiff <= 125) {
            tc = agCharts.time.week;
        }
        else {
            tc = agCharts.time.month;
        }
        return tc;
    }
}