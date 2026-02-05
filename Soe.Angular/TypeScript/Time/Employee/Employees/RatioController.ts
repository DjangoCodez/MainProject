import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { Guid } from "../../../Util/StringUtility";
import { Constants } from "../../../Util/Constants";
import { EmployeeGridDTO } from "../../../Common/Models/EmployeeUserDTO";
import { AgChartUtility, AgChartOptionsPie, AgChartOptionsBar } from "../../../Util/ag-chart/AgChartUtility";
import { TermGroup_Sex, TermGroup } from "../../../Util/CommonEnumerations";
import { ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { ICoreService } from "../../../Core/Services/CoreService";
import { NumberUtility } from "../../../Util/NumberUtility";

declare var agCharts;

export class RatioController {

    // Terms
    private terms: { [index: string]: string; };
    private chartInfoRow1: string;
    private chartInfoRow2: string;

    // Data
    private rows: EmployeeGridDTO[];
    private genders: ISmallGenericType[] = [];

    private guid: Guid;

    // Charts
    private containerWidth: number = 0;
    private chartHeight: number = 500;
    private chartWidth: number = 500;
    private chartsCreated: boolean = false;

    private employmentTypeChartElem: Element;
    private employmentTypeData: any[] = [];

    private workPercentageChartElem: Element;
    private workPercentageData: any[] = [];

    private workTimeWeekChartElem: Element;
    private workTimeWeekData: any[] = [];

    private genderChartElem: Element;
    private genderData: any[] = [];

    private genderAgeChartElem: Element;
    private genderAgeData: any[] = [];

    //@ngInject
    constructor(
        private $timeout: ng.ITimeoutService,
        private translationService: ITranslationService,
        private messagingService: IMessagingService,
        private coreService: ICoreService) {

        this.loadTerms();
    }

    onInit(parameters: any) {
        // Save guid created for this tab, to use in EVENT_TAB_ACTIVATED below
        this.guid = parameters.guid;
    }

    public $onInit() {
        if (this.genders.length === 0)
            this.loadGenders();

        this.messagingService.subscribe('employeesFiltered', (data) => {
            this.rows = data.rows;

            this.chartInfoRow2 = "{0} {1}".format(this.terms["core.aggrid.totals.total"], data.totalCount);
            if (data.totalCount !== this.rows.length)
                this.chartInfoRow2 += " ({0} {1})".format(this.terms["core.aggrid.totals.filtered"], this.rows.length.toString());
        });

        this.messagingService.subscribe(Constants.EVENT_TAB_ACTIVATED, (x) => {
            if (this.guid === x) {
                // Need a little timeout, first time charts are created,
                // otherwise they end up corrupted.
                this.$timeout(() => {
                    this.createCharts()
                }, this.chartsCreated ? 0 : 200);
            }
        });
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total",
            "core.pieces.short",
            "time.employee.employee.employees",
            "time.employee.employee.ratios.chartinfo",
            "time.employee.employee.ratios.gender",
            "time.employee.employee.ratios.genderage",
            "time.employee.employee.percent",
            "time.employee.employee.employmenttype",
            "time.schedule.planning.worktimeweek"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
            this.chartInfoRow1 = this.terms["time.employee.employee.ratios.chartinfo"].format(this.terms["time.employee.employee.employees"]);
        });
    }

    private loadGenders(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.Sex, false, false, true).then(x => {
            this.genders = x;
        });
    }

    // CHARTS

    private createCharts() {
        if (!agCharts)
            return;

        //this.setChartSizes(3).then(() => {
        // Employment type
        if (this.employmentTypeChartElem)
            this.employmentTypeChartElem.innerHTML = '';
        else
            this.employmentTypeChartElem = document.querySelector('#employmentTypeChart');
        this.setEmploymentTypeData();
        agCharts.AgChart.create(this.setEmploymentTypeChartOptions());

        // Work percentage
        if (this.workPercentageChartElem)
            this.workPercentageChartElem.innerHTML = '';
        else
            this.workPercentageChartElem = document.querySelector('#workPercentageChart');
        this.setWorkPercentageData();
        agCharts.AgChart.create(this.setWorkPercentageChartOptions());

        // Work time week
        if (this.workTimeWeekChartElem)
            this.workTimeWeekChartElem.innerHTML = '';
        else
            this.workTimeWeekChartElem = document.querySelector('#workTimeWeekChart');
        this.setWorkTimeWeekData();
        agCharts.AgChart.create(this.setWorkTimeWeekChartOptions());

        // Gender
        this.genderData = [];
        if (_.filter(this.rows, r => r.sexString).length > 0) {
            if (this.genderChartElem)
                this.genderChartElem.innerHTML = '';
            else
                this.genderChartElem = document.querySelector('#genderChart');
            this.setGenderData();
            agCharts.AgChart.create(this.setGenderChartOptions());
        }

        // Age per gender
        this.genderAgeData = [];
        if (_.filter(this.rows, r => r.sexString && r.age > 0).length > 0) {
            if (this.genderAgeChartElem)
                this.genderAgeChartElem.innerHTML = '';
            else
                this.genderAgeChartElem = document.querySelector('#genderAgeChart');
            this.setGenderAgeData();
            agCharts.AgChart.create(this.setGenderAgeChartOptions());
        }

        this.chartsCreated = true;
        //});
    }

    // DATA

    private setEmploymentTypeData() {
        this.employmentTypeData = [];

        let group: _.Dictionary<EmployeeGridDTO[]> = _.groupBy(this.rows, r => r.employmentTypeString);
        let labels: string[] = Object.keys(group);
        _.forEach(_.orderBy(labels, l => l), label => {
            this.employmentTypeData.push({ label: label, value: group[label].length });
        });
    }

    private setWorkPercentageData() {
        this.workPercentageData = [];

        let group: _.Dictionary<EmployeeGridDTO[]> = _.groupBy(this.rows, r => r.percent.round(0));
        let labels: string[] = Object.keys(group);
        _.forEach(_.orderBy(labels, l => l.toString().padLeft(3, '0')), label => {
            this.workPercentageData.push({ label: parseFloat(label).round(0) + '%', value: group[label].length });
        });
    }

    private setWorkTimeWeekData() {
        this.workTimeWeekData = [];

        let group: _.Dictionary<EmployeeGridDTO[]> = _.groupBy(this.rows, r => r.workTimeWeekFormatted);
        let labels: string[] = Object.keys(group);
        _.forEach(_.orderBy(labels, l => l.toString().padLeft(5, '0')), label => {
            this.workTimeWeekData.push({ label: label, value: group[label].length });
        });
    }

    private setGenderData() {
        let group: _.Dictionary<EmployeeGridDTO[]> = _.groupBy(this.rows, r => r.sex);
        if (this.rows.filter(r => r.sex == TermGroup_Sex.Male).length > 0)
            this.genderData.push({ label: _.find(this.genders, g => g.id == TermGroup_Sex.Male).name, value: (group[TermGroup_Sex.Male].length / this.rows.length * 100).round(0) });
        if (this.rows.filter(r => r.sex == TermGroup_Sex.Female).length > 0)
            this.genderData.push({ label: _.find(this.genders, g => g.id == TermGroup_Sex.Female).name, value: (group[TermGroup_Sex.Female].length / this.rows.length * 100).round(0) });
        if (this.rows.filter(r => r.sex == TermGroup_Sex.Unknown).length > 0)
            this.genderData.push({ label: _.find(this.genders, g => g.id == TermGroup_Sex.Unknown).name, value: (group[TermGroup_Sex.Unknown].length / this.rows.length * 100).round(0) });
    }

    private setGenderAgeData() {
        let ageData = _.filter(this.rows, r => r.age);

        // 0-18
        let range1 = _.filter(ageData, d => d.age <= 17);
        this.genderAgeData.push({ label: '0-17', male: this.getGenderPart(range1, TermGroup_Sex.Male, ageData.length), female: this.getGenderPart(range1, TermGroup_Sex.Female, ageData.length), unknown: this.getGenderPart(range1, TermGroup_Sex.Unknown, ageData.length) });

        // 18-24
        let range2 = _.filter(ageData, d => d.age >= 18 && d.age <= 24);
        this.genderAgeData.push({ label: '18-24', male: this.getGenderPart(range2, TermGroup_Sex.Male, ageData.length), female: this.getGenderPart(range2, TermGroup_Sex.Female, ageData.length), unknown: this.getGenderPart(range2, TermGroup_Sex.Unknown, ageData.length) });

        // 25-34
        let range3 = _.filter(ageData, d => d.age >= 25 && d.age <= 34);
        this.genderAgeData.push({ label: '25-34', male: this.getGenderPart(range3, TermGroup_Sex.Male, ageData.length), female: this.getGenderPart(range3, TermGroup_Sex.Female, ageData.length), unknown: this.getGenderPart(range3, TermGroup_Sex.Unknown, ageData.length) });

        // 35-44
        let range4 = _.filter(ageData, d => d.age >= 35 && d.age <= 44);
        this.genderAgeData.push({ label: '35-44', male: this.getGenderPart(range4, TermGroup_Sex.Male, ageData.length), female: this.getGenderPart(range4, TermGroup_Sex.Female, ageData.length), unknown: this.getGenderPart(range4, TermGroup_Sex.Unknown, ageData.length) });

        // 45-54
        let range5 = _.filter(ageData, d => d.age >= 45 && d.age <= 54);
        this.genderAgeData.push({ label: '45-54', male: this.getGenderPart(range5, TermGroup_Sex.Male, ageData.length), female: this.getGenderPart(range5, TermGroup_Sex.Female, ageData.length), unknown: this.getGenderPart(range5, TermGroup_Sex.Unknown, ageData.length) });

        // 55-64
        let range6 = _.filter(ageData, d => d.age >= 55 && d.age <= 64);
        this.genderAgeData.push({ label: '55-64', male: this.getGenderPart(range6, TermGroup_Sex.Male, ageData.length), female: this.getGenderPart(range6, TermGroup_Sex.Female, ageData.length), unknown: this.getGenderPart(range6, TermGroup_Sex.Unknown, ageData.length) });

        // 65-
        let range7 = _.filter(ageData, d => d.age >= 65);
        this.genderAgeData.push({ label: '65-', male: this.getGenderPart(range7, TermGroup_Sex.Male, ageData.length), female: this.getGenderPart(range7, TermGroup_Sex.Female, ageData.length), unknown: this.getGenderPart(range7, TermGroup_Sex.Unknown, ageData.length) });
    }

    private getGenderPart(range: EmployeeGridDTO[], gender: TermGroup_Sex, total: number) {
        return (_.filter(range, r => r.sex === gender).length / total * 100).round(0);
    }

    // HELP-METHODS

    //private setChartSizes(nbrOfChartsPerRow: number): ng.IPromise<any> {
    //    return this.$timeout(() => {
    //        this.containerWidth = document.getElementById('infoContainer').offsetWidth;
    //        this.chartWidth = Math.floor((this.containerWidth - ((nbrOfChartsPerRow - 1) * 17)) / nbrOfChartsPerRow);
    //        if (this.chartWidth > 500)
    //            this.chartWidth = 500;
    //    });
    //}

    private setEmploymentTypeChartOptions() {
        let options = new AgChartOptionsPie();
        options.height = this.chartHeight;
        options.width = this.chartWidth;
        options.minAngle = 400;
        options.angleName = this.terms["core.pieces.short"].toLocaleLowerCase();
        options.title = this.terms["time.employee.employee.employmenttype"];
        return AgChartUtility.createDefaultPieChart(this.employmentTypeChartElem, this.employmentTypeData, options);
    }

    private setWorkPercentageChartOptions() {
        let options = new AgChartOptionsPie();
        options.height = this.chartHeight;
        options.width = this.chartWidth;
        options.minAngle = 400;
        options.angleName = this.terms["core.pieces.short"].toLocaleLowerCase();
        options.title = this.terms["time.employee.employee.percent"];
        return AgChartUtility.createDefaultPieChart(this.workPercentageChartElem, this.workPercentageData, options);
    }

    private setWorkTimeWeekChartOptions() {
        let options = new AgChartOptionsPie();
        options.height = this.chartHeight;
        options.width = this.chartWidth;
        options.minAngle = 400;
        options.angleName = this.terms["core.pieces.short"].toLocaleLowerCase();
        options.title = this.terms["time.schedule.planning.worktimeweek"];
        return AgChartUtility.createDefaultPieChart(this.workTimeWeekChartElem, this.workTimeWeekData, options);
    }

    private setGenderChartOptions() {
        let options = new AgChartOptionsPie();
        options.height = this.chartHeight;
        options.width = this.chartWidth;
        options.paddingBottom = 40;
        options.minAngle = 400;
        options.angleName = '%';
        options.title = this.terms["time.employee.employee.ratios.gender"];
        options.legendPosition = 'bottom';
        return AgChartUtility.createDefaultPieChart(this.genderChartElem, this.genderData, options);
    }

    private setGenderAgeChartOptions() {
        let options = new AgChartOptionsBar();
        options.height = this.chartHeight;
        options.width = this.chartWidth;
        options.width = this.chartWidth * 2 + 15;
        options.legendPosition = 'bottom';
        options.title = this.terms["time.employee.employee.ratios.genderage"];
        options.series = [
            {
                type: 'column',
                xKey: 'label',
                yKeys: this.getYKeys(),
                yNames: this.getYNames(),
                grouped: true,
                tooltip: {
                    renderer: function (params) {
                        return '<div class="ag-chart-tooltip-title" style="background-color:' + params.color + '">' +
                            params.yName +
                            '</div>' +
                            '<div class="ag-chart-tooltip-content" style="text-align: right;">' +
                            NumberUtility.printDecimal(params.datum[params.yKey], 0, 0) + '%' +
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
                        return NumberUtility.printDecimal(params.value, 0, 0) + '%';
                    }
                }
            },
            {
                position: 'bottom',
                type: 'category'
            }
        ];

        return AgChartUtility.createDefaultBarChart(this.genderAgeChartElem, this.genderAgeData, options);
    }

    private getYKeys(): string[] {
        let keys: string[] = [];

        if (_.filter(this.genderAgeData, d => d.male > 0).length > 0)
            keys.push('male');
        if (_.filter(this.genderAgeData, d => d.female > 0).length > 0)
            keys.push('female');
        if (_.filter(this.genderAgeData, d => d.unknown > 0).length > 0)
            keys.push('unknown');

        return keys;
    }

    private getYNames(): string[] {
        let names: string[] = [];

        if (_.filter(this.genderAgeData, d => d.male > 0).length > 0)
            names.push(this.genders.find(g => g.id == TermGroup_Sex.Male).name);
        if (_.filter(this.genderAgeData, d => d.female > 0).length > 0)
            names.push(this.genders.find(g => g.id == TermGroup_Sex.Female).name);
        if (_.filter(this.genderAgeData, d => d.unknown > 0).length > 0)
            names.push(this.genders.find(g => g.id == TermGroup_Sex.Unknown).name);

        return names;
    }
}