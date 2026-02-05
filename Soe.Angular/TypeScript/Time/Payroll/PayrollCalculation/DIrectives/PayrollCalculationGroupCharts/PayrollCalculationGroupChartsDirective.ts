import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { PayrollCalculationEmployeePeriodDTO } from "../../../../../Common/Models/TimeEmployeeTreeDTO";
import { AgChartUtility, AgChartOptionsBar } from "../../../../../Util/ag-chart/AgChartUtility";
import { NumberUtility } from "../../../../../Util/NumberUtility";
import { IPayrollService } from "../../../PayrollService";
import { TermGroup, TermGroup_AverageSalaryCostChartSeriesType } from "../../../../../Util/CommonEnumerations";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";

declare var agCharts;

export class PayrollCalculationGroupChartsDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Time/Payroll/PayrollCalculation/Directives/PayrollCalculationGroupCharts/PayrollCalculationGroupCharts.html'),
            scope: {
                rows: '=',
                timePeriodId: '=',
                totalCount: '=',
                chartsCreated: '='
            },
            restrict: 'E',
            replace: true,
            controller: PayrollCalculationGroupChartsController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

export class PayrollCalculationGroupChartsController {

    // Terms
    private terms: { [index: string]: string; };
    private chartInfoRow1: string;
    private chartInfoRow2: string;

    // Init parameters
    private rows: PayrollCalculationEmployeePeriodDTO[];
    private timePeriodId: number;
    private totalCount: number;
    private chartsCreated: boolean;
    private chartsInitialized: boolean = false;

    // Charts
    private containerWidth: number = 0;
    private chartWidth: number = 0;
    private chartHeight: number = 300;

    private salaryCostChartElem: Element;
    private salaryCostOptions: any;
    private salaryCostData: any[];

    private averageSalaryCostSeriesTypes: ISmallGenericType[];
    private averageSalaryCostChartElem: Element;
    private averageSalaryCostOptions: any;
    private averageSalaryCostData: any[];

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private $timeout: ng.ITimeoutService,
        private translationService: ITranslationService,
        private coreService: ICoreService,
        private payrollService: IPayrollService) {
    }

    // INIT

    public $onInit() {
        this.$q.all([
            this.loadTerms(),
            this.loadAverageSalaryCostSeriesTypes()
        ]).then(() => {
            this.setupWatchers();
        });
    }

    private setupWatchers() {
        if (!this.rows)
            this.rows = [];

        this.$scope.$watch(() => this.rows, () => {
            this.$timeout(() => {
                if (this.chartsInitialized && this.terms)
                    this.createCharts();
            });
        });

        this.$scope.$watch(() => this.chartsCreated, () => {
            this.$timeout(() => {
                if (this.chartsCreated && !this.chartsInitialized) {
                    this.chartsInitialized = true;
                    this.createCharts();
                }
            });
        });
    }

    private createCharts() {
        if (!agCharts || !this.rows)
            return;

        this.setChartSizes(2).then(() => {
            this.createSalaryCostChart();
            this.createAverageSalaryCostChart();
        });

        this.setChartInfo();
    }

    private createSalaryCostChart() {
        if (this.salaryCostChartElem)
            this.salaryCostChartElem.innerHTML = '';
        else
            this.salaryCostChartElem = document.querySelector('#salaryCostChart');

        let options = new AgChartOptionsBar();
        options.height = this.chartHeight;
        options.width = this.chartWidth;
        options.legendPosition = 'bottom';

        this.setSalaryCostData();

        options.title = this.terms["time.payroll.payrollcalculation.chart.group.salarycost"];
        options.series = [
            {
                type: 'column',
                xKey: 'key',
                yKeys: this.getSalaryCostYKeys(),
                yNames: this.getSalaryCostYNames(),
                grouped: true,
                tooltip: {
                    renderer: function (params) {
                        return '<div class="ag-chart-tooltip-title" style="background-color:' + params.color + '">' +
                            params.yName +
                            '</div>' +
                            '<div class="ag-chart-tooltip-content" style="text-align: right;">' +
                            NumberUtility.printDecimal(params.datum[params.yKey], 0, 0) +
                            '</div>';
                    }
                },
                highlightStyle: {
                    fill: '#EEEEEE',
                    stroke: '#CCCCCC'
                }
            }
        ];
        options.axes = [
            {
                position: 'left',
                type: 'number',
                label: {
                    formatter: function (params) {
                        return NumberUtility.printDecimal(params.value, 0, 0);
                    }
                }
            },
            {
                position: 'bottom',
                type: 'category',
                tick: {
                    width: 0
                }
            }
        ];
        this.salaryCostOptions = AgChartUtility.createDefaultBarChart(this.salaryCostChartElem, this.salaryCostData, options);
        agCharts.AgChart.create(this.salaryCostOptions);
    }

    private createAverageSalaryCostChart() {
        if (this.averageSalaryCostChartElem)
            this.averageSalaryCostChartElem.innerHTML = '';
        else
            this.averageSalaryCostChartElem = document.querySelector('#averageSalaryCostChart');

        let options = new AgChartOptionsBar();
        options.height = this.chartHeight;
        options.width = this.chartWidth;
        options.legendPosition = 'bottom';

        this.loadAverageSalaryCost().then(() => {
            options.title = this.terms["time.payroll.payrollcalculation.chart.group.averagesalarycost"];
            options.series = [
                {
                    type: 'column',
                    xKey: 'key',
                    yKeys: ['prevMen', 'prevWomen', 'prevTotal', 'prevMedian', 'currentMen', 'currentWomen', 'currentTotal', 'currentMedian'],
                    yNames: this.getAverageSalaryCostYNames(),
                    grouped: true,
                    tooltip: {
                        renderer: function (params) {
                            return '<div class="ag-chart-tooltip-title" style="background-color:' + params.color + '">' +
                                params.yName +
                                '</div>' +
                                '<div class="ag-chart-tooltip-content" style="text-align: right;">' +
                                NumberUtility.printDecimal(params.datum[params.yKey], 0, 0) +
                                '</div>';
                        }
                    },
                    highlightStyle: {
                        fill: '#EEEEEE',
                        stroke: '#CCCCCC'
                    }
                }
            ];
            options.axes = [
                {
                    position: 'left',
                    type: 'number',
                    label: {
                        formatter: function (params) {
                            return NumberUtility.printDecimal(params.value, 0, 0);
                        }
                    }
                },
                {
                    position: 'bottom',
                    type: 'category',
                    tick: {
                        width: 0
                    }
                }
            ];
            this.averageSalaryCostOptions = AgChartUtility.createDefaultBarChart(this.averageSalaryCostChartElem, this.averageSalaryCostData, options);
            agCharts.AgChart.create(this.averageSalaryCostOptions);
        });
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        let keys: string[] = [
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total",
            "time.payroll.payrollcalculation.benefit",
            "time.payroll.payrollcalculation.compensation",
            "time.payroll.payrollcalculation.deduction",
            "time.payroll.payrollcalculation.employmenttax",
            "time.payroll.payrollcalculation.grossalary",
            "time.payroll.payrollcalculation.netsalary",
            "time.payroll.payrollcalculation.tax",
            "time.payroll.payrollcalculation.chart.group.info",
            "time.payroll.payrollcalculation.chart.group.salarycost",
            "time.payroll.payrollcalculation.chart.group.averagesalarycost",
            "time.payroll.payrollcalculation.chart.group.averagesalarycost.currentyear",
            "time.payroll.payrollcalculation.chart.group.averagesalarycost.prevyear"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;

            this.chartInfoRow1 = this.terms["time.payroll.payrollcalculation.chart.group.info"];
        });
    }

    private loadAverageSalaryCostSeriesTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.AverageSalaryCostChartSeriesType, false, false, true).then(x => {
            this.averageSalaryCostSeriesTypes = x;
        });
    }

    // DATA

    private setSalaryCostData() {
        this.salaryCostData = [];

        let gross: number = 0;
        let benefitInvertExcluded: number = 0;
        let compensation: number = 0;
        let deduction: number = 0;
        let tax: number = 0;
        let employmentTax: number = 0;
        let net: number = 0;

        _.forEach(this.rows, row => {
            gross += row.periodSumGross;
            benefitInvertExcluded += row.periodSumBenefitInvertExcluded;
            compensation += row.periodSumCompensation;
            deduction += Math.abs(row.periodSumDeduction);
            tax += Math.abs(row.periodSumTax);
            employmentTax += Math.abs(row.periodSumEmploymentTax);
            net += row.periodSumNet;
        });

        this.salaryCostData.push({
            key: '',
            gross: gross,
            benefit: benefitInvertExcluded,
            compensation: compensation,
            deduction: deduction,
            tax: tax,
            employmentTax: employmentTax,
            net: net
        });
    }

    private loadAverageSalaryCost(): ng.IPromise<any> {
        this.averageSalaryCostData = [];

        return this.payrollService.getAverageSalaryCostForEmployees(this.timePeriodId, this.rows.map(r => r.employeeId)).then(x => {
            if (x.data.length === 2) {
                let prevMen = _.find(x.data[0].values, v => v.type == TermGroup_AverageSalaryCostChartSeriesType.Men).value || 0;
                let prevWomen = _.find(x.data[0].values, v => v.type == TermGroup_AverageSalaryCostChartSeriesType.Women).value || 0;
                let prevTotal = _.find(x.data[0].values, v => v.type == TermGroup_AverageSalaryCostChartSeriesType.Total).value || 0;
                let prevMedian = _.find(x.data[0].values, v => v.type == TermGroup_AverageSalaryCostChartSeriesType.Median).value || 0;
                let currentMen = _.find(x.data[1].values, v => v.type == TermGroup_AverageSalaryCostChartSeriesType.Men).value || 0;
                let currentWomen = _.find(x.data[1].values, v => v.type == TermGroup_AverageSalaryCostChartSeriesType.Women).value || 0;
                let currentTotal = _.find(x.data[1].values, v => v.type == TermGroup_AverageSalaryCostChartSeriesType.Total).value || 0;
                let currentMedian = _.find(x.data[1].values, v => v.type == TermGroup_AverageSalaryCostChartSeriesType.Median).value || 0;
                this.averageSalaryCostData.push({
                    key: "{0} | {1}".format(this.terms["time.payroll.payrollcalculation.chart.group.averagesalarycost.prevyear"], this.terms["time.payroll.payrollcalculation.chart.group.averagesalarycost.currentyear"]),
                    prevMen: prevMen,
                    prevWomen: prevWomen,
                    prevTotal: prevTotal,
                    prevMedian: prevMedian,
                    currentMen: currentMen,
                    currentWomen: currentWomen,
                    currentTotal: currentTotal,
                    currentMedian: currentMedian
                });
            }
        });
    }

    // HELP-METHODS

    private setChartSizes(nbrOfChartsPerRow: number): ng.IPromise<any> {
        return this.$timeout(() => {
            this.containerWidth = document.getElementById('infoContainer').offsetWidth;
            this.chartWidth = Math.floor((this.containerWidth - ((nbrOfChartsPerRow - 1) * 48)) / nbrOfChartsPerRow);
            if (this.chartWidth > 800)
                this.chartWidth = 800;
        });
    }

    private setChartInfo() {
        if (this.terms) {
            if (this.totalCount === undefined)
                this.totalCount = this.rows.length;

            this.chartInfoRow2 = "{0} {1}".format(this.terms["core.aggrid.totals.total"], this.totalCount.toString());
            if (this.totalCount !== this.rows.length)
                this.chartInfoRow2 += " ({0} {1})".format(this.terms["core.aggrid.totals.filtered"], this.rows.length.toString());
        }
    }

    private getSalaryCostYKeys(): string[] {
        let keys: string[] = [];
        if (this.salaryCostData.length > 0) {
            let data = this.salaryCostData[0];

            if (data.gross)
                keys.push('gross');
            if (data.benefit)
                keys.push('benefit');
            if (data.compensation)
                keys.push('compensation');
            if (data.deduction)
                keys.push('deduction');
            if (data.tax)
                keys.push('tax');
            if (data.employmentTax)
                keys.push('employmentTax');
            if (data.net)
                keys.push('net');
        }

        return keys;
    }

    private getSalaryCostYNames(): string[] {
        let names: string[] = [];
        if (this.salaryCostData.length > 0) {
            let data = this.salaryCostData[0];

            if (data.gross)
                names.push(this.terms["time.payroll.payrollcalculation.grossalary"]);
            if (data.benefit)
                names.push(this.terms["time.payroll.payrollcalculation.benefit"]);
            if (data.compensation)
                names.push(this.terms["time.payroll.payrollcalculation.compensation"]);
            if (data.deduction)
                names.push(this.terms["time.payroll.payrollcalculation.deduction"]);
            if (data.tax)
                names.push(this.terms["time.payroll.payrollcalculation.tax"]);
            if (data.employmentTax)
                names.push(this.terms["time.payroll.payrollcalculation.employmenttax"]);
            if (data.net)
                names.push(this.terms["time.payroll.payrollcalculation.netsalary"]);
        }

        return names;
    }

    private getAverageSalaryCostYNames(): string[] {
        let names: string[] = [];
        names.push(this.averageSalaryCostSeriesTypes.find(t => t.id == TermGroup_AverageSalaryCostChartSeriesType.Men).name);
        names.push(this.averageSalaryCostSeriesTypes.find(t => t.id == TermGroup_AverageSalaryCostChartSeriesType.Women).name);
        names.push(this.averageSalaryCostSeriesTypes.find(t => t.id == TermGroup_AverageSalaryCostChartSeriesType.Total).name);
        names.push(this.averageSalaryCostSeriesTypes.find(t => t.id == TermGroup_AverageSalaryCostChartSeriesType.Median).name);
        names.push(this.averageSalaryCostSeriesTypes.find(t => t.id == TermGroup_AverageSalaryCostChartSeriesType.Men).name);
        names.push(this.averageSalaryCostSeriesTypes.find(t => t.id == TermGroup_AverageSalaryCostChartSeriesType.Women).name);
        names.push(this.averageSalaryCostSeriesTypes.find(t => t.id == TermGroup_AverageSalaryCostChartSeriesType.Total).name);
        names.push(this.averageSalaryCostSeriesTypes.find(t => t.id == TermGroup_AverageSalaryCostChartSeriesType.Median).name);

        return names;
    }
}