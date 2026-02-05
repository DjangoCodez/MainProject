import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { AgChartUtility, AgChartOptionsLine } from "../../../../../Util/ag-chart/AgChartUtility";
import { IPayrollService } from "../../../PayrollService";
import { CalendarUtility } from "../../../../../Util/CalendarUtility";
import { NumberUtility } from "../../../../../Util/NumberUtility";
import { SoeEmployeeTimePeriodValueType } from "../../../../../Util/CommonEnumerations";
import { Constants } from "../../../../../Util/Constants";

declare var agCharts;

export class PayrollCalculationEmployeeChartsDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Time/Payroll/PayrollCalculation/Directives/PayrollCalculationEmployeeCharts/PayrollCalculationEmployeeCharts.html'),
            scope: {
                timePeriodId: '=',
                employeeId: '=',
                totalCount: '=',
                chartsCreated: '='
            },
            restrict: 'E',
            replace: true,
            controller: PayrollCalculationEmployeeChartsController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

export class PayrollCalculationEmployeeChartsController {

    // Terms
    private terms: { [index: string]: string; };
    private chartInfoRow1: string;
    private chartInfoRow2: string;

    // Init parameters
    private timePeriodId: number;
    private employeeId;
    private totalCount: number;
    private chartsCreated: boolean;
    private chartsInitialized: boolean = false;

    // Charts
    private containerWidth: number = 0;
    private chartWidth: number = 0;
    private chartHeight: number = 300;

    private salaryHistoryChartElem: Element;
    private salaryHistoryOptions: any;
    private salaryHistoryData: any[];

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private $timeout: ng.ITimeoutService,
        private translationService: ITranslationService,
        private payrollService: IPayrollService) {
    }

    // INIT

    public $onInit() {
        this.loadTerms().then(() => {
            this.setupWatchers();
        });

        this.$scope.$on(Constants.EVENT_RELOAD_CHARTS, (e, parameters) => {
            if (this.chartsInitialized && this.terms)
                this.createCharts();
        });

    }

    private setupWatchers() {
        this.$scope.$watchGroup([() => this.timePeriodId, () => this.employeeId], () => {
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
        if (!agCharts || !this.timePeriodId || !this.employeeId)
            return;

        this.setChartSizes(1).then(() => {
            if (this.salaryHistoryChartElem)
                this.salaryHistoryChartElem.innerHTML = '';
            else
                this.salaryHistoryChartElem = document.querySelector('#salaryHistoryChart');

            this.loadSalaryHistory();
        });
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        let keys: string[] = [
            //"core.aggrid.totals.filtered",
            //"core.aggrid.totals.total",
            "time.payroll.payrollcalculation.chart.employee.info",
            "time.payroll.payrollcalculation.chart.employee.salaryhistory",
            "time.payroll.payrollcalculation.employmenttax",
            "time.payroll.payrollcalculation.grossalary",
            "time.payroll.payrollcalculation.netsalary"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;

            this.chartInfoRow1 = this.terms["time.payroll.payrollcalculation.chart.employee.info"];
        });
    }

    // DATA

    private loadSalaryHistory = _.debounce(() => {
        this.salaryHistoryData = [];

        return this.payrollService.getSalaryHistoryForEmployee(this.timePeriodId, this.employeeId).then(x => {
            _.forEach(x.data, dateData => {
                let grossSalary = _.find(dateData.values, v => v.type == SoeEmployeeTimePeriodValueType.GrossSalary).value || 0;
                let netSalary = _.find(dateData.values, v => v.type == SoeEmployeeTimePeriodValueType.NetSalary).value || 0;
                let employmentTax = _.find(dateData.values, v => v.type == SoeEmployeeTimePeriodValueType.EmploymentTaxCredit).value || 0;
                this.salaryHistoryData.push({ date: dateData.date, grossValue: grossSalary, netValue: netSalary, employmentTax: employmentTax });
            });

            this.setChartOptions();
            this.setChartInfo();
            agCharts.AgChart.create(this.salaryHistoryOptions);
        });
    }, 200);

    // HELP-METHODS

    private setChartSizes(nbrOfChartsPerRow: number): ng.IPromise<any> {
        return this.$timeout(() => {
            this.containerWidth = document.getElementById('infoContainer').offsetWidth;
            this.chartWidth = Math.floor((this.containerWidth - ((nbrOfChartsPerRow - 1) * 17)) / nbrOfChartsPerRow);
        });
    }

    private setChartOptions() {
        let options = new AgChartOptionsLine();
        options.height = this.chartHeight;
        options.width = this.chartWidth;

        options.title = this.terms["time.payroll.payrollcalculation.chart.employee.salaryhistory"];
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
                        return CalendarUtility.toFormattedYearMonth(params.value)
                    }
                }
            }
        ];
        options.series = [
            {
                type: 'line',
                xKey: 'date',
                yKey: 'grossValue',
                yName: this.terms["time.payroll.payrollcalculation.grossalary"],
                stroke: '#0000FF',
                marker: {
                    fill: '#8080FF',
                    stroke: '#0000FF'
                },
                tooltip: {
                    renderer: function (params) {
                        const htmlStart = '<div class="ag-chart-tooltip-content" style="border-top: 1px solid #000080; text-align: right;">';
                        const divEnd = '</div>';
                        const date = CalendarUtility.toFormattedDate(params.datum.date);
                        const value = NumberUtility.printDecimal(params.datum.grossValue, 0, 0);

                        return '<div class="ag-chart-tooltip-title" style="background-color: #0000FF; color: #FFFFFF">' +
                            date +
                            divEnd +
                            htmlStart + value + divEnd;
                    }
                },
                highlightStyle: {
                    fill: '#EEEEEE',
                    stroke: '#CCCCCC'
                }
            },
            {
                type: 'line',
                xKey: 'date',
                yKey: 'netValue',
                yName: this.terms["time.payroll.payrollcalculation.netsalary"],
                stroke: '#008000',
                marker: {
                    fill: '#00CC00',
                    stroke: '#008000'
                },
                tooltip: {
                    renderer: function (params) {
                        const htmlStart = '<div class="ag-chart-tooltip-content" style="border-top: 1px solid #003300; text-align: right;">';
                        const divEnd = '</div>';
                        const date = CalendarUtility.toFormattedDate(params.datum.date);
                        const value = NumberUtility.printDecimal(params.datum.netValue, 0, 0);

                        return '<div class="ag-chart-tooltip-title" style="background-color: #008000; color: #FFFFFF">' +
                            date +
                            divEnd +
                            htmlStart + value + divEnd;
                    }
                },
                highlightStyle: {
                    fill: '#EEEEEE',
                    stroke: '#CCCCCC'
                }
            },
            {
                type: 'line',
                xKey: 'date',
                yKey: 'employmentTax',
                yName: this.terms["time.payroll.payrollcalculation.employmenttax"],
                stroke: '#FF0000',
                marker: {
                    fill: '#FF8080',
                    stroke: '#FF0000'
                },
                tooltip: {
                    renderer: function (params) {
                        const htmlStart = '<div class="ag-chart-tooltip-content" style="border-top: 1px solid #800000; text-align: right;">';
                        const divEnd = '</div>';
                        const date = CalendarUtility.toFormattedDate(params.datum.date);
                        const value = NumberUtility.printDecimal(params.datum.employmentTax, 0, 0);

                        return '<div class="ag-chart-tooltip-title" style="background-color: #FF0000; color: #FFFFFF">' +
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

        this.salaryHistoryOptions = AgChartUtility.createDefaultLineChart(this.salaryHistoryChartElem, this.salaryHistoryData, options);
    }

    private setChartInfo() {
    }
}