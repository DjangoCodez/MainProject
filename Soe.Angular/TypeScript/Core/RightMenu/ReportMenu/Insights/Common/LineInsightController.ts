import { AgChartOptionsLine, AgChartUtility } from "../../../../../Util/ag-chart/AgChartUtility";
import { CalendarUtility } from "../../../../../Util/CalendarUtility";
import { NumberUtility } from "../../../../../Util/NumberUtility";
import { ITranslationService } from "../../../../Services/TranslationService";
import { InsightControllerBase } from "./InsightControllerBase";

export class LineInsightController extends InsightControllerBase {
    public static component(): ng.IComponentOptions {
        return {
            controller: LineInsightController,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/Insights/Common/LineInsight.html",
            bindings: {
                userSelection: "=",
                matrixSelection: '<',
                rows: '<',
                hideTitle: "=",
                containerId: "=",
            }
        };
    }

    public static componentKey = "lineInsight";

    private xKey: string;
    private yKey: string;

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

        let columnName: string = this.getColumnName();
        this.xKey = columnName;
        this.yKey = this.getColumnName(1);
        let valueDecimals = this.getColumnDecimals(1);

        let group: _.Dictionary<any[]>;
        if (this.isColumnDate())
            group = _.groupBy(this.rows, r => CalendarUtility.convertToDate(r[columnName]).toFormattedDate(this.dateFormat));
        else if (this.isColumnTime())
            group = _.groupBy(this.rows, r => CalendarUtility.minutesToTimeSpan(r[columnName]));
        else if (this.isColumnDecimal())
            group = _.groupBy(this.rows, r => r[columnName].round(this.decimals));
        else
            group = _.groupBy(this.rows, r => r[columnName]);

        let labels: string[] = Object.keys(group);
        if (this.isColumnInteger() || this.isColumnDecimal()) {
            _.forEach(_.orderBy(labels, l => parseFloat(l).round(0)), label => {
                this.addChartData(parseFloat(label).round(this.decimals) + (this.labelPostValue ? (' ' + this.labelPostValue) : ''), group[label], valueDecimals);
            });
        } else if (this.isColumnDate()) {
            _.forEach(_.orderBy(labels, l => CalendarUtility.convertToDate(l)), label => {
                this.addChartData(CalendarUtility.convertToDate(label), group[label][0][this.yKey], valueDecimals);
            });
        } else if (this.isColumnTime()) {
            _.forEach(_.orderBy(labels, l => l.toString().padLeft(5, '0')), label => {
                this.addChartData(label.padLeft(5, '0'), group[label], valueDecimals);
            });
        } else {
            _.forEach(_.orderBy(labels, l => l.toString()), label => {
                this.addChartData(label, group[label], valueDecimals);
            });
        }
    }

    protected setChartOptions() {
        let options: AgChartOptionsLine = new AgChartOptionsLine();
        super.setupContainer(options);

        options.axes = [
            {
                position: 'left',
                type: 'number',
                label: {
                    formatter: function (params) {
                        return NumberUtility.printDecimal(params.value, 0, 0)
                    }
                }
            },
            {
                position: 'bottom',
                type: 'time',
                label: {
                    formatter: function (params) {
                        return CalendarUtility.toFormattedDate(params.value)
                    }
                }
            }
        ];
        options.series = [
            {
                type: 'line',
                xKey: this.xKey,
                yKey: this.yKey,
                yName: this.getColumnTitle(1),
                stroke: '#80A0C3',
                marker: {
                    fill: '#94BAE3',
                    stroke: '#80A0C3'
                },
                tooltip: {
                    renderer: function (params) {
                        const htmlStart = '<div class="ag-chart-tooltip-content" style="border-top: 1px solid #536880; text-align: right;">';
                        const divEnd = '</div>';
                        const date = CalendarUtility.toFormattedDate(params.datum[params.xKey]);
                        const value = NumberUtility.printDecimal(params.datum[params.yKey], params.datum.decimals, params.datum.decimals);

                        return '<div class="ag-chart-tooltip-title" style="background-color: #80A0C3; color: #FFFFFF">' +
                            date +
                            divEnd +
                            htmlStart + value + divEnd;
                    }
                },
                highlightStyle: {
                    fill: '#EEEEEE',
                    stroke: '#CCCCCC'
                }
            }
        ];

        return AgChartUtility.createDefaultLineChart(this.chartElem, this.chartData, options);
    }

    // HELP-METHODS

    private addChartData(label: any, value: any, decimals: number) {
        if (!label || label === 'null')
            label = this.terms["core.notspecified"];
        else if (this.isColumnBoolean()) {
            label = (label.toLowerCase() === 'true' ? this.terms["core.yes"] : this.terms["core.no"]);
        }

        this.chartData.push({ [this.xKey]: label, [this.yKey]: value, decimals: decimals });
    }
}