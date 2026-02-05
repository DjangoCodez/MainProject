import { SmallGenericType } from "../../../../../Common/Models/SmallGenericType";
import { AgChartOptionsDoughnut, AgChartOptionsPie, AgChartUtility } from "../../../../../Util/ag-chart/AgChartUtility";
import { CalendarUtility } from "../../../../../Util/CalendarUtility";
import { MatrixDataType, TermGroup_InsightChartTypes } from "../../../../../Util/CommonEnumerations";
import { ITranslationService } from "../../../../Services/TranslationService";
import { InsightControllerBase } from "./InsightControllerBase";

export class PieInsightController extends InsightControllerBase {
    public static component(): ng.IComponentOptions {
        return {
            controller: PieInsightController,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/Insights/Common/PieInsight.html",
            bindings: {
                userSelection: "=",
                matrixSelection: '<',
                rows: '<',
                hideTitle: "=",
                containerId: "=",
            }
        };
    }

    public static componentKey = "pieInsight";

    // Selectors
    private selectedValueType: number;
    private valueTypes: SmallGenericType[] = [];

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

        let columnName: string = this.getColumnName();
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

        if (this.isColumnInteger() || this.isColumnDecimal()) {
            _.forEach(_.orderBy(labels, l => parseFloat(l).round(0)), label => {
                this.addChartData(parseFloat(label).round(this.decimals) + (this.labelPostValue ? (' ' + this.labelPostValue) : ''), this.isPercent ? (group[label].length / this.rows.length * 100).round(0) : group[label].length);
            });
        } else if (this.isColumnTime()) {
            _.forEach(_.orderBy(labels, l => l.toString().padLeft(5, '0')), label => {
                this.addChartData(label.padLeft(5, '0'), this.isPercent ? (group[label].length / this.rows.length * 100) : group[label].length);
            });
        } else {
            _.forEach(_.orderBy(labels, l => l.toString()), label => {
                this.addChartData(label, this.isPercent ? (group[label].length / this.rows.length * 100) : group[label].length);
            });
        }
    }

    protected setChartOptions() {
        let options: AgChartOptionsPie | AgChartOptionsDoughnut = this.useDoughnut ? new AgChartOptionsDoughnut() : new AgChartOptionsPie();
        super.setupContainer(options);

        options.minAngle = 20;
        options.angleName = this.isPercent ? '%' : this.terms["core.pieces.short"].toLocaleLowerCase();

        if (this.useDoughnut)
            return AgChartUtility.createDefaultDoughnutChart(this.chartElem, this.chartData, options);
        else
            return AgChartUtility.createDefaultPieChart(this.chartElem, this.chartData, options);
    }

    // HELP-METHODS

    private get useDoughnut(): boolean {
        return this.matrixSelection.chartType === TermGroup_InsightChartTypes.Doughnut;
    }

    private get isQuantity(): boolean {
        return this.selectedValueType === 0;
    }

    private get isPercent(): boolean {
        return this.selectedValueType === 1;
    }

    private addChartData(label: string, value: any) {
        if (!label || label === 'null')
            label = this.terms["core.notspecified"];
        else if (this.isColumnBoolean()) {
            label = (label.toLowerCase() === 'true' ? this.terms["core.yes"] : this.terms["core.no"]);
        }

        this.chartData.push({ label: label, value: value });
    }
}