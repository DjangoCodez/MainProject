import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { AgChartUtility, AgChartOptionsBar } from "../../../../../Util/ag-chart/AgChartUtility";
import { Constants } from "../../../../../Util/Constants";
import { AttestEmployeeDayDTO } from "../../../../../Common/Models/TimeEmployeeTreeDTO";
import { AttestPayrollTransactionDTO } from "../../../../../Common/Models/AttestPayrollTransactionDTO";
import { CalendarUtility } from "../../../../../Util/CalendarUtility";
import { ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";
import { SmallGenericType } from "../../../../../Common/Models/SmallGenericType";
import { ProductSmallDTO } from "../../../../../Common/Models/ProductDTOs";
import { queue } from "jquery";

declare var agCharts;

export class AttestEmployeeChartsDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Time/Directives/AttestEmployee/Directives/AttestEmployeeCharts/AttestEmployeeCharts.html'),
            scope: {
                contentEmployee: '=',
                chartsCreated: '='
            },
            restrict: 'E',
            replace: true,
            controller: AttestEmployeeChartsController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

export class AttestEmployeeChartsController {

    // Terms
    private terms: { [index: string]: string; };
    private chartInfoRow1: string;
    private chartInfoRow2: string;

    // Init parameters
    private contentEmployee: AttestEmployeeDayDTO[];
    private chartsCreated: boolean;
    private chartsInitialized: boolean = false;

    // Charts
    private containerWidth: number = 0;
    private chartWidth: number = 0;
    private chartHeight: number = 300;

    private payrollProductChartElem: Element;
    private payrollProductOptions: any;
    private payrollProductData: any[];

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private $timeout: ng.ITimeoutService,
        private translationService: ITranslationService) {
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
        this.$scope.$watchGroup([() => this.contentEmployee], () => {
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
        if (!agCharts || !this.contentEmployee)
            return;

        this.setChartSizes(1).then(() => {
            this.createPayrollProductChart();
        });

        this.setChartInfo();
    }

    private createPayrollProductChart() {
        if (this.payrollProductChartElem)
            this.payrollProductChartElem.innerHTML = '';
        else
            this.payrollProductChartElem = document.querySelector('#payrollProductChart');

        let options = new AgChartOptionsBar();
        options.height = this.chartHeight;
        options.width = this.chartWidth;
        options.legendPosition = 'bottom';

        this.setPayrollProductData();

        options.title = this.terms["time.time.attest.chart.employee.payrollproduct"];
        options.series = [
            {
                type: 'column',
                xKey: 'key',
                yKeys: _.orderBy(this.products, p => p.number.padLeft(10, '0')).map(p => p.productId),
                yNames: _.orderBy(this.products, p => p.number.padLeft(10, '0')).map(p => p.numberName),
                grouped: true,
                tooltip: {
                    renderer: function (params) {
                        return '<div class="ag-chart-tooltip-title" style="background-color:' + params.color + '">' +
                            params.yName +
                            '</div>' +
                            '<div class="ag-chart-tooltip-content" style="text-align: right;">' +
                            CalendarUtility.minutesToTimeSpan(params.datum[params.yKey]) +
                            " ({0}%)".format((parseInt(params.datum[params.yKey], 10) / parseInt(params.datum['total'], 10) * 100).round(1).toString()) +
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
                tick: {
                    count: 5,
                },
                label: {
                    formatter: function (params) {
                        return CalendarUtility.minutesToTimeSpan(params.value);
                    }
                }
            },
            {
                position: 'bottom',
                type: 'category',
                tick: {
                    width: 0
                },
            }
        ];
        this.payrollProductOptions = AgChartUtility.createDefaultBarChart(this.payrollProductChartElem, this.payrollProductData, options);
        agCharts.AgChart.create(this.payrollProductOptions);
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        let keys: string[] = [
            "time.time.attest.chart.employee.info",
            "time.time.attest.chart.employee.payrollproduct"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;

            this.chartInfoRow1 = this.terms["time.time.attest.chart.employee.info"];
        });
    }

    // DATA

    private products: ProductSmallDTO[] = [];
    private setPayrollProductData() {
        this.payrollProductData = [];

        let dataRows: any[] = [];

        _.forEach(this.contentEmployee, day => {

            let group: _.Dictionary<AttestPayrollTransactionDTO[]> = _.groupBy(day.attestPayrollTransactions, r => r.payrollProductId);
            let productIds: string[] = Object.keys(group);

            _.forEach(productIds, productId => {
                let quantity = _.sum(group[productId].map(g => g.quantity));
                if (quantity !== 0) {
                    let dataItem = dataRows.find(d => d.productId === productId);
                    if (dataItem)
                        dataItem.quantity += quantity;
                    else {
                        let prod = new ProductSmallDTO();
                        prod.productId = parseInt(productId, 10);
                        prod.number = group[productId][0].payrollProductNumber;
                        prod.name = group[productId][0].payrollProductName;
                        prod.numberName = "{0} {1}".format(prod.number, prod.name);

                        this.products.push(prod);

                        dataRows.push({
                            productId: productId,
                            productNr: group[productId][0].payrollProductNumber,
                            productName: group[productId][0].payrollProductName,
                            quantity: quantity
                        });
                    }
                }
            });
        });

        let data: any = { key: '' };
        let total: number = 0;
        if (dataRows.length > 0) {
            _.forEach(_.orderBy(dataRows, r => r.productNr.padLeft(10, '0')), dataItem => {
                if (dataItem.quantity > 0) {
                    data[dataItem.productId] = dataItem.quantity;
                    total += dataItem.quantity;
                }
            });
        }
        data.total = total;
        this.payrollProductData.push(data);
    }

    // HELP-METHODS

    private setChartSizes(nbrOfChartsPerRow: number): ng.IPromise<any> {
        return this.$timeout(() => {
            this.containerWidth = document.getElementById('infoContainer').offsetWidth;
            this.chartWidth = Math.floor((this.containerWidth - ((nbrOfChartsPerRow - 1) * 17)) / nbrOfChartsPerRow);
            if (this.chartWidth > 1000)
                this.chartWidth = 1000;
        });
    }

    private setChartInfo() {
    }
}