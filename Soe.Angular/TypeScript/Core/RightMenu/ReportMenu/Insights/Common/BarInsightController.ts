import { SmallGenericType } from "../../../../../Common/Models/SmallGenericType";
import { AgChartOptionsBar, AgChartUtility } from "../../../../../Util/ag-chart/AgChartUtility";
import { CalendarUtility } from "../../../../../Util/CalendarUtility";
import { MatrixDataType, TermGroup_InsightChartTypes } from "../../../../../Util/CommonEnumerations";
import { NumberUtility } from "../../../../../Util/NumberUtility";
import { ITranslationService } from "../../../../Services/TranslationService";
import { InsightControllerBase } from "./InsightControllerBase";

export class BarInsightController extends InsightControllerBase {
    public static component(): ng.IComponentOptions {
        return {
            controller: BarInsightController,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/Insights/Common/BarInsight.html",
            bindings: {
                userSelection: "=",
                matrixSelection: '<',
                rows: '<',
                hideTitle: "=",
                containerId: "=",
            }
        };
    }

    public static componentKey = "barInsight";

    // Data
    private groupByColumnFieldFirst: string;
    private groupByColumnFieldSecond: string;

    // Selectors
    private stacked: boolean = false;
    private selectedValueType: number;
    private valueTypes: SmallGenericType[] = [];

    // Charts
    private yKeys: string[] = [];
    private yNames: string[] = [];

    //@ngInject
    constructor(
        $scope: ng.IScope,
        $timeout: ng.ITimeoutService,
        translationService: ITranslationService) {

        super($scope, $timeout, translationService);
    }

    public $onInit() {
        this.init();
        let keys: string[] = [
            "common.percent",
            "common.quantity"
        ];

        super.setup(keys).then(() => {
            this.setupValueTypes();
            this.createChart();
        });
    }

    protected createColumns() {
        super.createColumns();

        if (this.columns.length > 0) {
            let groupedColumns = _.filter(this.columns, c => c.options && c.options.groupBy);
            if (groupedColumns.length > 0) {
                this.groupByColumnFieldFirst = groupedColumns[0].field;
                if (groupedColumns.length > 1)
                    this.groupByColumnFieldSecond = groupedColumns[1].field;
                else if (this.columns.length > 1)
                    this.groupByColumnFieldSecond = this.columns[1].field;

            } else {
                this.groupByColumnFieldFirst = this.columns[0].field;
            }
        }
    }

    private setupValueTypes() {
        this.valueTypes = [];
        this.valueTypes.push(new SmallGenericType(0, this.terms["common.quantity"]));
        this.valueTypes.push(new SmallGenericType(1, this.terms["common.percent"]));
        this.selectedValueType = this.matrixSelection.valueType;
    }

    // CHART

    protected setChartData() {
        this.chartData = [];
        this.yKeys = [];
        this.yNames = [];

        let columnName: string = this.getColumnName();
        
        _.forEach(_.filter(this.rows, r => ((!r[columnName] || r[columnName] === 'null') && r[columnName] != '0')), r => {
            r[columnName] = this.terms["core.notspecified"];
        });
        if (this.groupByColumnFieldSecond) {
            _.forEach(_.filter(this.rows, r => ((!r[this.groupByColumnFieldSecond] || r[this.groupByColumnFieldSecond] === 'null') && r[this.groupByColumnFieldSecond] != '0')), r => {
                r[this.groupByColumnFieldSecond] = this.terms["core.notspecified"];
            });
        }

        if (this.isColumnBoolean()) {
            _.forEach(this.rows, r => {
                r[columnName] = (!!r[columnName] ? this.terms["core.yes"] : this.terms["core.no"]);
            });
        }
        if (this.isColumnTime()) {
            if (this.minutesToDecimal) {
                this.columns[0].matrixDataType = MatrixDataType.Decimal;
                this.columns[0].options.decimals = 2;
                _.forEach(this.rows, r => r[columnName] = (r[columnName] / 60).round(2));
            } else if (this.minutesToTimeSpan) {
                _.forEach(this.rows, r => r[columnName] = CalendarUtility.minutesToTimeSpan(r[columnName]));
            } else {
                this.columns[0].matrixDataType = MatrixDataType.Integer;
            }
        }
        let group: _.Dictionary<any[]>;
        if (this.isColumnDate())
            group = _.groupBy(this.rows, r => CalendarUtility.convertToDate(r[columnName]).toFormattedDate(this.dateFormat));
        else if (this.isColumnTime())
            group = _.groupBy(this.rows, r => r[columnName]);
        else if (this.isColumnDecimal())
            group = _.groupBy(this.rows, r => r[columnName].round(this.decimals));
        else
            group = _.groupBy(this.rows, r => r[columnName]);

        let labels: string[] = Object.keys(group);

        if (this.groupByColumnFieldSecond) {
            let allGroupSecond = _.groupBy(this.rows, r => r[this.groupByColumnFieldSecond]);
            let labelsSecond: string[] = Object.keys(allGroupSecond);
            _.forEach(labelsSecond, labelSecond => {
                this.yKeys.push(labelSecond);
                this.yNames.push(labelSecond);
            });

            _.forEach(labels, label => {
                let groupSecond;
                if (this.isColumnDate())
                    groupSecond = _.groupBy(_.filter(this.rows, r => CalendarUtility.convertToDate(r[this.groupByColumnFieldFirst]).toFormattedDate(this.dateFormat) === label), r => r[this.groupByColumnFieldSecond]);
                else if (this.isColumnTime())
                    groupSecond = _.groupBy(_.filter(this.rows, r => r[this.groupByColumnFieldFirst] === label), r => r[this.groupByColumnFieldSecond]);
                else if (this.isColumnDecimal())
                    groupSecond = _.groupBy(_.filter(this.rows, r => r[this.groupByColumnFieldFirst].round(this.decimals) === label), r => r[this.groupByColumnFieldSecond]);
                else
                    groupSecond = _.groupBy(_.filter(this.rows, r => r[this.groupByColumnFieldFirst] === label), r => r[this.groupByColumnFieldSecond]);

                let data = { label: label, unitName: this.isPercent ? '%' : this.terms["core.pieces.short"].toLocaleLowerCase(), showCategoryLabel: true };
                _.forEach(labelsSecond, labelSecond => {
                    if (groupSecond[labelSecond])
                        data[labelSecond] = this.isPercent ? (groupSecond[labelSecond].length / this.rows.length * 100).round(0) : groupSecond[labelSecond].length;
                    else
                        data[labelSecond] = 0;
                });
                this.chartData.push(data);
            });
        } else {
            let data = { label: this.groupByColumnFieldFirst, unitName: this.isPercent ? '%' : this.terms["core.pieces.short"].toLocaleLowerCase(), showCategoryLabel: false };
            if (this.isColumnInteger() || this.isColumnDecimal()) {
                _.forEach(_.orderBy(labels, l => parseFloat(l).round(this.decimals)), label => {
                    this.yKeys.push(label);
                    this.yNames.push(parseFloat(label).round(this.decimals) + (this.labelPostValue ? (' ' + this.labelPostValue) : ''));
                    data[label] = this.isPercent ? (group[label].length / this.rows.length * 100).round(0) : group[label].length;
                });
            } else if (this.isColumnTime()) {
                _.forEach(_.orderBy(labels, l => l.toString().padLeft(5, '0')), label => {
                    this.yKeys.push(label.padLeft(5, '0'));
                    this.yNames.push(label.padLeft(5, '0'));
                    data[label] = this.isPercent ? (group[label].length / this.rows.length * 100).round(0) : group[label].length;
                });
            } else {
                _.forEach(_.orderBy(labels, l => l.toString()), label => {
                    this.yKeys.push(label);
                    this.yNames.push(label);
                    data[label] = this.isPercent ? (group[label].length / this.rows.length * 100).round(0) : group[label].length;
                });
            }
            this.chartData.push(data);
        }
    }

    protected setChartOptions() {
        let options: AgChartOptionsBar = new AgChartOptionsBar();
        super.setupContainer(options);

        options.series = [
            {
                type: this.useColumn ? 'column' : 'bar',
                xKey: 'label',
                yKeys: this.yKeys,
                yNames: this.yNames,
                grouped: !this.stacked,
                tooltip: {
                    renderer: function (params) {
                        return '<div class="ag-chart-tooltip-title" style="background-color:' + params.color + '">' +
                            params.yName +
                            '</div>' +
                            '<div class="ag-chart-tooltip-content" style="text-align: right;">' +
                            NumberUtility.printDecimal(params.datum[params.yKey], 0, 0) + ' ' + params.datum.unitName +
                            '</div>';
                    }
                },
                highlightStyle: {
                    fill: '#EEEEEE',
                    stroke: '#CCCCCC'
                }
            }
        ];

        if (this.useColumn) {
            options.axes = [
                {
                    type: 'number',
                    position: 'left',
                    label: {
                        formatter: function (params) {
                            return NumberUtility.printDecimal(params.value, 0, 0);
                        }
                    }
                },
                {
                    type: 'category',
                    position: 'bottom',
                    label: {
                        formatter: function (params) {
                            return params.axis.boundSeries[0]._data[0].showCategoryLabel ? params.value : '';
                        }
                    }
                }
            ];
        } else {
            options.axes = [
                {
                    type: 'number',
                    position: 'bottom',
                    label: {
                        formatter: function (params) {
                            return NumberUtility.printDecimal(params.value, 0, 0);
                        }
                    }
                },
                {
                    type: 'category',
                    position: 'left',
                    label: {
                        formatter: function (params) {
                            return params.axis.boundSeries[0]._data[0].showCategoryLabel ? params.value : '';
                        }
                    }
                }
            ];
        }

        return AgChartUtility.createDefaultBarChart(this.chartElem, this.chartData, options);
    }

    // HELP-METHODS

    private get useColumn(): boolean {
        return this.matrixSelection.chartType === TermGroup_InsightChartTypes.Column;
    }

    private get isQuantity(): boolean {
        return this.selectedValueType === 0;
    }

    private get isPercent(): boolean {
        return this.selectedValueType === 1;
    }
}