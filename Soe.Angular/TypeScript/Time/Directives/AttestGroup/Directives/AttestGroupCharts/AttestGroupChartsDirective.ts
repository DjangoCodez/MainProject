import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { AgChartUtility, AgChartOptionsBar } from "../../../../../Util/ag-chart/AgChartUtility";
import { Constants } from "../../../../../Util/Constants";
import { AttestEmployeePeriodDTO } from "../../../../../Common/Models/TimeEmployeeTreeDTO";
import { CalendarUtility } from "../../../../../Util/CalendarUtility";
import { ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";
import { TermGroup, TermGroup_Sex } from "../../../../../Util/CommonEnumerations";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { StringKeyValue } from "../../../../../Common/Models/StringKeyValue";
import { IQService } from "angular";

declare var agCharts;

export class AttestGroupChartsDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Time/Directives/AttestGroup/Directives/AttestGroupCharts/AttestGroupCharts.html'),
            scope: {
                contentGroup: '=',
                chartsCreated: '='
            },
            restrict: 'E',
            replace: true,
            controller: AttestGroupChartsController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

export class AttestGroupChartsController {

    // Terms
    private terms: { [index: string]: string; };
    private chartInfoRow1: string;
    private chartInfoRow2: string;

    // Init parameters
    private contentGroup: AttestEmployeePeriodDTO[];
    private totalCount: number;
    private chartsCreated: boolean;
    private chartsInitialized: boolean = false;

    // Data
    private presenceKeyLabels: StringKeyValue[] = [];
    private absenceKeyLabels: StringKeyValue[] = [];

    // Charts
    private containerWidth: number = 0;
    private chartWidth: number = 0;
    private chartHeight: number = 300;
    private showUnknownGender: boolean = false;
    private showStacked: boolean = false;

    private presenceChartElem: Element;
    private presenceData: any[];

    private absenceChartElem: Element;
    private absenceData: any[];

    //@ngInject
    constructor(
        private $q: IQService,
        private $scope: ng.IScope,
        private $timeout: ng.ITimeoutService,
        private translationService: ITranslationService,
        private coreService: ICoreService) {
    }

    // INIT

    public $onInit() {
        this.$q.all([
            this.loadTerms()
        ]).then(() => {
            this.setupWatchers();
        });

        this.$scope.$on(Constants.EVENT_RELOAD_GROUP_CHARTS, (e, parameters) => {
            if (this.chartsInitialized && this.terms)
                this.createCharts();
        });
    }

    private setupWatchers() {
        this.$scope.$watchGroup([() => this.contentGroup, () => this.showStacked], () => {
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

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        let keys: string[] = [
            "core.aggrid.totals.total",
            "core.aggrid.totals.filtered",
            "common.gender.men",
            "common.gender.women",
            "common.gender.unknown",
            "common.total",
            "time.time.attest.chart.group.info",
            "time.time.attest.presence",
            "time.time.attest.time",
            "time.time.attest.payed",
            "time.time.attest.sums.workedscheduledtime",
            "time.time.attest.sums.timeaccumulatorovertime",
            "time.time.attest.sums.addedtime",
            "time.time.attest.sums.weekendsalary",
            "time.time.attest.sums.duty",
            "time.time.attest.sums.obaddition",
            "time.time.attest.sums.obaddition50",
            "time.time.attest.sums.obaddition70",
            "time.time.attest.sums.obaddition100",
            "time.time.attest.sums.compensationandaddition",
            "time.time.attest.sums.compensationandaddition50",
            "time.time.attest.sums.compensationandaddition70",
            "time.time.attest.sums.compensationandaddition100",
            "time.time.attest.absence",
            "time.time.attest.sums.absence",
            "time.time.attest.sums.absencevacation",
            "time.time.attest.sums.absencesick",
            "time.time.attest.sums.leaveofabsence",
            "time.time.attest.sums.absenceparentalleave",
            "time.time.attest.sums.absencetempparentalleave"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;

            this.chartInfoRow1 = this.terms["time.time.attest.chart.group.info"];

            this.presenceKeyLabels.push(new StringKeyValue('presenceTimeInfo', this.terms["time.time.attest.time"]));
            this.presenceKeyLabels.push(new StringKeyValue('presencePayedTimeInfo', this.terms["time.time.attest.payed"]));
            this.presenceKeyLabels.push(new StringKeyValue('sumTimeWorkedScheduledTimeText', this.terms["time.time.attest.sums.workedscheduledtime"]));
            this.presenceKeyLabels.push(new StringKeyValue('sumTimeAccumulatorOverTimeText', this.terms["time.time.attest.sums.timeaccumulatorovertime"]));
            this.presenceKeyLabels.push(new StringKeyValue('sumGrossSalaryAdditionalTimeText', this.terms["time.time.attest.sums.addedtime"]));
            this.presenceKeyLabels.push(new StringKeyValue('sumGrossSalaryWeekendSalaryText', this.terms["time.time.attest.sums.weekendsalary"]));
            this.presenceKeyLabels.push(new StringKeyValue('sumGrossSalaryDutyText', this.terms["time.time.attest.sums.duty"]));
            this.presenceKeyLabels.push(new StringKeyValue('sumGrossSalaryOBAdditionText', this.terms["time.time.attest.sums.obaddition"]));
            this.presenceKeyLabels.push(new StringKeyValue('sumGrossSalaryOBAddition50Text', this.terms["time.time.attest.sums.obaddition50"]));
            this.presenceKeyLabels.push(new StringKeyValue('sumGrossSalaryOBAddition70Text', this.terms["time.time.attest.sums.obaddition70"]));
            this.presenceKeyLabels.push(new StringKeyValue('sumGrossSalaryOBAddition100Text', this.terms["time.time.attest.sums.obaddition100"]));
            this.presenceKeyLabels.push(new StringKeyValue('sumGrossSalaryOvertimeText', this.terms["time.time.attest.sums.compensationandaddition"]));
            this.presenceKeyLabels.push(new StringKeyValue('sumGrossSalaryOvertime50Text', this.terms["time.time.attest.sums.compensationandaddition50"]));
            this.presenceKeyLabels.push(new StringKeyValue('sumGrossSalaryOvertime70Text', this.terms["time.time.attest.sums.compensationandaddition70"]));
            this.presenceKeyLabels.push(new StringKeyValue('sumGrossSalaryOvertime100Text', this.terms["time.time.attest.sums.compensationandaddition100"]));

            this.absenceKeyLabels.push(new StringKeyValue('sumGrossSalaryAbsenceText', this.terms["time.time.attest.sums.absence"]));
            this.absenceKeyLabels.push(new StringKeyValue('sumGrossSalaryAbsenceVacationText', this.terms["time.time.attest.sums.absencevacation"]));
            this.absenceKeyLabels.push(new StringKeyValue('sumGrossSalaryAbsenceSickText', this.terms["time.time.attest.sums.absencesick"]));
            this.absenceKeyLabels.push(new StringKeyValue('sumGrossSalaryAbsenceLeaveOfAbsenceText', this.terms["time.time.attest.sums.leaveofabsence"]));
            this.absenceKeyLabels.push(new StringKeyValue('sumGrossSalaryAbsenceParentalLeaveText', this.terms["time.time.attest.sums.absenceparentalleave"]));
            this.absenceKeyLabels.push(new StringKeyValue('sumGrossSalaryAbsenceTemporaryParentalLeaveText', this.terms["time.time.attest.sums.absencetempparentalleave"]));
        });
    }

    // CHARTS

    private createCharts() {
        if (!agCharts || !this.contentGroup)
            return;

        this.setChartSizes(2).then(() => {
            this.setPresenceAndAbsenceData();
            this.createPresenceChart();
            this.createAbsenceChart();
        });

        this.setChartInfo();
    }

    private createPresenceChart() {
        if (this.presenceChartElem)
            this.presenceChartElem.innerHTML = '';
        else
            this.presenceChartElem = document.querySelector('#presenceChart');

        let options = new AgChartOptionsBar();
        options.height = this.chartHeight;
        options.width = this.chartWidth;
        options.legendPosition = 'bottom';

        if (!this.showStacked && (this.presenceData.length > 5 || this.absenceData.length > 5) || this.presenceData.length > 10 || this.absenceData.length > 10)
            options.width *= 2;

        options.title = this.terms["time.time.attest.presence"];
        options.series = [
            {
                type: 'column',
                xKey: 'label',
                yKeys: this.getPresenceAndAbsenceYKeys(),
                yNames: this.getPresenceAndAbsenceYNames(),
                grouped: !this.showStacked,
                tooltip: {
                    renderer: function (params) {
                        return '<div class="ag-chart-tooltip-title" style="background-color:' + params.color + '">' +
                            params.datum.label + ": " + params.yName +
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
            }
        ];
        agCharts.AgChart.create(AgChartUtility.createDefaultBarChart(this.presenceChartElem, this.presenceData, options));
    }

    private createAbsenceChart() {
        if (this.absenceChartElem)
            this.absenceChartElem.innerHTML = '';
        else
            this.absenceChartElem = document.querySelector('#absenceChart');

        let options = new AgChartOptionsBar();
        options.height = this.chartHeight;
        options.width = this.chartWidth;
        options.legendPosition = 'bottom';

        if (!this.showStacked && (this.presenceData.length > 5 || this.absenceData.length > 5) || this.presenceData.length > 10 || this.absenceData.length > 10)
            options.width *= 2;

        options.title = this.terms["time.time.attest.absence"];
        options.series = [
            {
                type: 'column',
                xKey: 'label',
                yKeys: this.getPresenceAndAbsenceYKeys(),
                yNames: this.getPresenceAndAbsenceYNames(),
                grouped: !this.showStacked,
                tooltip: {
                    renderer: function (params) {
                        return '<div class="ag-chart-tooltip-title" style="background-color:' + params.color + '">' +
                            params.datum.label + ": " + params.yName +
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
            }
        ];
        agCharts.AgChart.create(AgChartUtility.createDefaultBarChart(this.absenceChartElem, this.absenceData, options));
    }

    // DATA

    private setPresenceAndAbsenceData() {
        this.presenceData = [];
        this.absenceData = [];

        let maleData = _.filter(this.contentGroup, e => e.employeeSex === TermGroup_Sex.Male);
        let femaleData = _.filter(this.contentGroup, e => e.employeeSex === TermGroup_Sex.Female);
        let unknownData = _.filter(this.contentGroup, e => e.employeeSex === TermGroup_Sex.Unknown);

        _.forEach(this.presenceKeyLabels.map(k => k.key), key => {
            let data = this.getGenderBasedData(key, this.presenceKeyLabels.find(k => k.key === key).value, maleData, femaleData, unknownData);
            if (data)
                this.presenceData.push(data);
        });

        _.forEach(this.absenceKeyLabels.map(k => k.key), key => {
            let data = this.getGenderBasedData(key, this.absenceKeyLabels.find(k => k.key === key).value, maleData, femaleData, unknownData);
            if (data)
                this.absenceData.push(data);
        });
    }

    // HELP-METHODS

    private setChartSizes(nbrOfChartsPerRow: number): ng.IPromise<any> {
        return this.$timeout(() => {
            this.containerWidth = document.getElementById('infoContainer').offsetWidth;
            this.chartWidth = Math.floor((this.containerWidth - ((nbrOfChartsPerRow - 1) * 60)) / nbrOfChartsPerRow);
        });
    }

    private setChartInfo() {
        if (this.terms) {
            if (this.totalCount === undefined)
                this.totalCount = this.contentGroup.length;

            this.chartInfoRow2 = "{0} {1}".format(this.terms["core.aggrid.totals.total"], this.totalCount.toString());
            if (this.totalCount !== this.contentGroup.length)
                this.chartInfoRow2 += " ({0} {1})".format(this.terms["core.aggrid.totals.filtered"], this.contentGroup.length.toString());
        }
    }

    private getGenderBasedData(fieldName: string, label: string, maleData: AttestEmployeePeriodDTO[], femaleData: AttestEmployeePeriodDTO[], unknownData: AttestEmployeePeriodDTO[]) {
        let maleValue = this.getGenderData(fieldName, maleData);
        let femaleValue = this.getGenderData(fieldName, femaleData);
        let unknownValue = this.getGenderData(fieldName, unknownData);
        if (unknownValue > 0)
            this.showUnknownGender = true;

        let totalValue = maleValue + femaleValue + unknownValue;

        if (totalValue === 0)
            return null;

        return { key: fieldName, label: label, male: maleValue, female: femaleValue, unknown: unknownValue, total: totalValue };
    }

    private getGenderData(fieldName: string, data: AttestEmployeePeriodDTO[]) {
        let value: number = 0;
        _.forEach(data, row => {
            value += CalendarUtility.timeSpanToMinutes(row[fieldName]);
        });
        return value;
    }

    private getPresenceAndAbsenceYKeys(): string[] {
        let keys: string[] = [];
        keys.push('male');
        keys.push('female');
        if (this.showUnknownGender)
            keys.push('unknown');
        if (!this.showStacked)
            keys.push('total');

        return keys;
    }

    private getPresenceAndAbsenceYNames(): string[] {
        let names: string[] = [];
        names.push(this.terms["common.gender.men"]);
        names.push(this.terms["common.gender.women"]);
        if (this.showUnknownGender)
            names.push(this.terms["common.gender.unknown"]);
        if (!this.showStacked)
            names.push(this.terms["common.total"]);

        return names;
    }
}
