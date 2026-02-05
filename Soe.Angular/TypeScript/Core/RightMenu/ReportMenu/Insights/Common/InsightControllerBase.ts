import { MatrixColumnSelectionDTO, MatrixColumnsSelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";
import { ReportUserSelectionDTO } from "../../../../../Common/Models/ReportDTOs";
import { AgChartOptionsBase } from "../../../../../Util/ag-chart/AgChartUtility";
import { CalendarUtility } from "../../../../../Util/CalendarUtility";
import { MatrixDataType } from "../../../../../Util/CommonEnumerations";
import { Guid } from "../../../../../Util/StringUtility";
import { ITranslationService } from "../../../../Services/TranslationService";

declare var agCharts;

export class InsightControllerBase {

    // Terms
    protected termKeys: string[] = [];
    protected terms: { [index: string]: string; };
    protected title: string;

    // Data
    protected userSelection: ReportUserSelectionDTO;
    protected matrixSelection: MatrixColumnsSelectionDTO;
    protected rows: any[];
    protected hideTitle: string;

    protected columns: MatrixColumnSelectionDTO[] = [];

    // Charts
    protected minChartHeight: number = 400;
    protected minChartWidth: number = 400;
    protected maxChartHeight: number = 800;
    protected maxChartWidth: number = 1200;

    protected chartElem: Element;
    protected chartData: any[] = [];

    protected containerId: string;

    get elementId() {
        return `chart-${this.containerId}`
    }

    //@ngInject
    constructor(
        private $scope: ng.IScope,
        private $timeout: ng.ITimeoutService,
        private translationService: ITranslationService) {
    }

    protected setup(additionalTermKeys: string[] = []): ng.IPromise<any> {
        this.termKeys = [
            "core.yes",
            "core.no",
            "core.notspecified",
            "core.pieces.short",
            "core.reportmenu.insights.basedon"
        ]

        if (additionalTermKeys.length > 0)
            this.termKeys = _.concat(this.termKeys, additionalTermKeys);

        return this.loadTerms().then(() => {
            this.createColumns();

            if (this.matrixSelection.insightId !== 1 && this.matrixSelection.insightName)
                this.title = this.matrixSelection.insightName
            else if (this.columns.length > 0)
                this.title = this.getColumnTitle();

            this.$scope.$on('refreshChart', () => this.refreshChart());
        });
    }

    protected init() {
        let el = document.getElementById("chart");
        el.id = this.elementId;
    }

    private loadTerms(): ng.IPromise<any> {
        return this.translationService.translateMany(this.termKeys).then(terms => {
            this.terms = terms;
        });
    }

    protected setupContainer(options: AgChartOptionsBase) {
        // let container = document.querySelector('#report-menu-overview-container');
        let container = document.getElementById(this.containerId);
        let height = 0;
        let width = 0;

        if (container) {
            height = _.max([this.minChartHeight, (container.clientHeight - 70)]);
            width = _.max([this.minChartWidth, container.clientWidth]);
        }
        if (height > this.maxChartHeight)
            height = this.maxChartHeight;
        if (width > this.maxChartWidth)
            width = this.maxChartWidth;

        let el = document.getElementById(this.elementId)
        if (!el) {
            options.height = height;
            options.width = width;
        } else {
            //let autoheight handle the rest...
            el.style.height = `${height}px`;
            // el.style.width = `${width}px` -> max/min is handled
            el.style.maxHeight = `${this.maxChartHeight}px`
            el.style.maxWidth = `${this.maxChartWidth}px`
            el.style.minHeight = `${this.minChartHeight}px`
            el.style.minWidth = `${this.minChartWidth}px`
        }

        options.title = this.hideTitle ? "" : this.title + " " + this.terms["core.reportmenu.insights.basedon"].format(this.rows.length.toString());
        options.legendPosition = 'bottom';
    }

    protected createColumns() {
        // Override in child class
        this.columns = this.matrixSelection ? _.orderBy(this.matrixSelection.columns, c => c.sort) : [];
    }

    // EVENTS

    private redraw(keepData: boolean = false) {
        this.$timeout(() => {
            this.createChart(keepData);
        });
    }

    private refreshChart() {
        this.createColumns();
        this.redraw(false);
    }

    // CHART

    protected createChart(keepData: boolean = false) {
        if (!agCharts || this.columns.length === 0)
            return;

        if (this.chartElem)
            this.chartElem.innerHTML = '';
        else {
            this.chartElem = document.getElementById(this.elementId);
        }

        if (!keepData)
            this.setChartData();

        let opt = this.setChartOptions() as AgChartOptionsBase;
        agCharts.AgChart.create(opt);
    }

    protected setChartData() {
        // Override in child class
    }

    protected setChartOptions(): any {
        // Override in child class
    }

    // HELP-METHODS

    protected get dateFormat(): string {
        let column = this.getColumn();
        if (column && column.hasOptions)
            return CalendarUtility.getDateFormatForMatrix(column.options.dateFormatOption);

        return '';
    }

    protected get decimals(): number {
        let column = this.getColumn();
        if (column && column.hasOptions)
            return column.options.decimals;

        // Default two decimals if not specified
        return 2;
    }

    protected get labelPostValue(): string {
        let column = this.getColumn();
        if (column && column.hasOptions)
            return column.options.labelPostValue;

        return '';
    }
   
    protected get minutesToDecimal(): boolean {
        let column = this.getColumn();
        if (column && column.hasOptions)
            return column.options.minutesToDecimal;

        return false;
    }
    protected get minutesToTimeSpan(): boolean {
        let column = this.getColumn();
        if (column && column.hasOptions)
            return column.options.minutesToTimeSpan;

        return false;
    }

    protected getColumn(index: number = 0): MatrixColumnSelectionDTO {
        return this.columns.length > index ? this.columns[index] : null;
    }

    protected getColumnTitle(index: number = 0): string {
        return this.columns.length > index ? this.columns[index].title : '';
    }

    protected getColumnName(index: number = 0): string {
        return this.columns.length > index ? this.columns[index].field : '';
    }

    protected getColumnDataType(index: number = 0): MatrixDataType {
        return this.columns.length > index ? this.columns[index].matrixDataType : MatrixDataType.String;
    }

    protected getColumnDecimals(index: number = 0): number {
        let column = this.getColumn(index);
        if (column && column.hasOptions)
            return column.options.decimals;

        // Default two decimals if not specified
        return 2;
    }

    protected isColumnString(index: number = 0): boolean {
        return this.getColumnDataType(index) === MatrixDataType.String;
    }

    protected isColumnInteger(index: number = 0): boolean {
        return this.getColumnDataType(index) === MatrixDataType.Integer;
    }

    protected isColumnBoolean(index: number = 0): boolean {
        return this.getColumnDataType(index) === MatrixDataType.Boolean;
    }

    protected isColumnDate(index: number = 0): boolean {
        return this.getColumnDataType(index) === MatrixDataType.Date;
    }

    protected isColumnDecimal(index: number = 0): boolean {
        return this.getColumnDataType(index) === MatrixDataType.Decimal;
    }

    protected isColumnTime(index: number = 0): boolean {
        return this.getColumnDataType(index) === MatrixDataType.Time;
    }

    protected isColumnDateAndTime(index: number = 0): boolean {
        return this.getColumnDataType(index) === MatrixDataType.DateAndTime;
    }
}