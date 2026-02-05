import { CalendarUtility } from "../../../Util/CalendarUtility";
import { TermGroup_TimeSchedulePlanningFollowUpCalculationType } from "../../../Util/CommonEnumerations";
import { CoreUtility } from "../../../Util/CoreUtility";
import { NumberUtility } from "../../../Util/NumberUtility";
import { AgChartOptionsBaseNew, AgChartOptionsLineNew, AgChartUtilityNew } from "../../../Util/ag-chart/AgChartUtilityNew";
import { EditController } from "./EditController";
import { ScheduleHandler } from "./ScheduleHandler";

declare var agCharts;

export class ChartHandler {
    private chartScriptUrl = "";

    private chartWidth = 0;
    private chartHeight = 400;

    private planningChartRef: any;
    private planningChartElem: Element;
    private planningChartOptions: any;
    private planningChartData: any;

    private staffingNeedsChartRef: any;
    private staffingNeedsChartElem: Element;
    private staffingNeedsChartOptions: any;
    private staffingNeedsChartData: any;

    constructor(private controller: EditController,
        private scheduleHandler: ScheduleHandler,
        private $timeout: ng.ITimeoutService,
        private $interval: ng.IIntervalService,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private $compile: ng.ICompileService
    ) {
        if (CoreUtility.isSupportAdmin)
            this.chartScriptUrl = "https://cdn.jsdelivr.net/npm/ag-charts-community@9.2.0/dist/umd/ag-charts-community.js";
        else
            this.chartScriptUrl = "https://cdn.jsdelivr.net/npm/ag-charts-community@9.2.0/dist/umd/ag-charts-community.min.js";
    }

    // PLANNING

    public renderPlanningAgChartRow() {
        let attachmentPoint = $('.planning-ag-chart-row');
        if (attachmentPoint?.length && attachmentPoint[0].children.length === 0) {
            let name = document.createElement('td');
            name.classList.add('staffing-needs-row-identifier');
            name.classList.add('link');
            name.setAttribute('colspan', '2');
            name.innerText = this.controller.terms["core.chart"];
            name.setAttribute('ng-attr-title', '{{(ctrl.showPlanningAgChart ? ctrl.terms["core.hide"] : ctrl.terms["core.show"]) + " " + ctrl.terms["core.chart"].toLowerCase()}}');
            name.setAttribute('data-ng-click', 'ctrl.showPlanningAgChart = !ctrl.showPlanningAgChart; ctrl.renderPlanningAgChart(ctrl.showPlanningAgChart);');

            let icon = document.createElement('i');
            icon.classList.add('far');
            icon.setAttribute('data-ng-class', "{'fa-chevron-down': ctrl.showPlanningAgChart, 'fa-chevron-right': !ctrl.showPlanningAgChart}");
            name.append(icon);

            let iconDiv = document.createElement('div');
            iconDiv.classList.add('margin-large-top');
            iconDiv.classList.add('margin-large-bottom');
            iconDiv.setAttribute('data-ng-if', 'ctrl.showPlanningAgChart');
            let reloadIcon = document.createElement('i');
            reloadIcon.classList.add('fal');
            reloadIcon.classList.add('fa-sync');
            reloadIcon.setAttribute('data-ng-if', 'ctrl.showPlanningAgChart');
            reloadIcon.style.fontSize = '20px';
            reloadIcon.setAttribute('ng-attr-title', '{{ctrl.terms["core.reload_data"]}}');
            reloadIcon.setAttribute('data-ng-click', '$event.stopPropagation(); ctrl.loadStaffingNeed();');
            iconDiv.append(reloadIcon);
            name.append(iconDiv);

            attachmentPoint.append(this.$compile(name)(this.$scope));

            let td = document.createElement('td');
            td.classList.add('planning-ag-chart');
            td.setAttribute('colspan', this.controller.dates.length.toString());
            attachmentPoint.append(td);
        }
    }

    public clearPlanningAgChartElem() {
        this.planningChartElem = null;
    }

    public renderPlanningAgChart() {
        if (this.controller.showPlanningAgChart) {
            if (!this.planningChartElem) {
                this.planningChartElem = document.createElement('div');
                this.planningChartElem.id = "chart";
                this.planningChartElem.setAttribute('data-ng-show', "ctrl.showPlanningAgChart");

                let attachmentPoint = $('.planning-ag-chart');
                if (attachmentPoint)
                    attachmentPoint.append(this.$compile(this.planningChartElem)(this.$scope));

                this.loadChartScript(this.planningChartElem).then(() => {
                    this.createOrUpdatePlanningAgChart();
                })
            } else {
                this.createOrUpdatePlanningAgChart();
            }
        }
    }

    private createOrUpdatePlanningAgChart() {
        this.setPlanningAgChartOptions();

        if (!this.planningChartRef) {
            this.planningChartRef = agCharts.AgCharts.create(this.planningChartOptions);
        } else {
            agCharts.AgCharts.update(this.planningChartRef, this.planningChartOptions);
        }
    }

    public setPlanningChartData(data: any[]) {
        this.planningChartData = data;
    }

    private setPlanningAgChartOptions() {
        const calculationType: TermGroup_TimeSchedulePlanningFollowUpCalculationType = this.planningChartData.calculationType;
        const isTime = calculationType === TermGroup_TimeSchedulePlanningFollowUpCalculationType.Hours;
        const isDayView = this.planningChartData.isDayView;

        let options = new AgChartOptionsBaseNew();
        options.height = this.chartHeight;
        options.width = this.chartWidth;
        options.title = this.planningChartData.title;
        options.paddingTop = 10;
        options.paddingBottom = 10;
        options.paddingLeft = 15;
        options.paddingRight = 15;
        options.axes = [
            {
                position: 'left',
                type: 'number',
                label: {
                    formatter: function (params) {
                        return ''
                    }
                }
            },
            {
                position: 'bottom',
                type: 'category',
                label: {
                    formatter: function (params) {
                        return ''
                    }
                }
            }
        ];
        options.series = [];

        for (let serie of this.planningChartData.series) {
            const opacity = serie.fillOpacity ? Number(serie.fillOpacity) : 1;

            let ser = {
                type: serie.type,
                xKey: 'date',
                yKey: serie.key,
                yName: serie.name,
                stroke: serie.fill,
                strokeOpacity: opacity,
                tooltip: {
                    renderer: function (params) {
                        const htmlStart = '<div class="ag-chart-tooltip-content" style="border-top: 1px solid #b6b6b6; text-align: right;">';
                        const divEnd = '</div>';
                        const date = isDayView ? CalendarUtility.toFormattedTime(params.datum.date) : CalendarUtility.toFormattedDate(params.datum.date);
                        const label = params.yName;
                        const rawValue = params.datum[params.yKey];
                        const value = isTime ? CalendarUtility.minutesToTimeSpan(rawValue) : rawValue.round(0).toLocaleString();

                        return '<div class="ag-chart-tooltip-title" style="background-color: ' + serie.fill + '; opacity: ' + opacity + '; color: #1e1e1e">' +
                            date +
                            divEnd +
                            htmlStart + label + " " + value + divEnd;
                    }
                },
                highlightStyle: {
                    item: {
                        fill: serie.fill,
                        fillOpacity: (opacity / 2),
                        stroke: serie.fill,
                        strokeWidth: 2,
                    }
                }
            };

            if (serie.type === 'line') {
                ser['marker'] = {
                    fill: serie.fill,
                    stroke: serie.fill,
                    size: 4
                };
            } else if (serie.type === 'bar') {
                ser['fill'] = serie.fill;
                ser['fillOpacity'] = opacity;
            }

            options.series.push(ser);
        }

        this.planningChartOptions = AgChartUtilityNew.createDefaultBaseChart(this.planningChartElem, this.planningChartData.rows, options);
    }

    // STAFFING NEEDS

    public renderStaffingNeedsAgChartRow() {
        let attachmentPoint = $('.staffing-needs-ag-chart-row');
        if (attachmentPoint?.length && attachmentPoint[0].children.length === 0) {
            let name = document.createElement('td');
            name.classList.add('staffing-needs-row-identifier');
            name.classList.add('link');
            name.setAttribute('colspan', '{{ctrl.showStaffingNeedsAgChart ? 1 : 2}}');
            name.innerText = this.controller.terms["core.chart"];
            name.setAttribute('ng-attr-title', '{{(ctrl.showStaffingNeedsAgChart ? ctrl.terms["core.hide"] : ctrl.terms["core.show"]) + " " + ctrl.terms["core.chart"].toLowerCase()}}');
            name.setAttribute('data-ng-click', 'ctrl.showStaffingNeedsAgChart = !ctrl.showStaffingNeedsAgChart; ctrl.renderStaffingNeedsAgChart(ctrl.showStaffingNeedsAgChart);');

            let icon = document.createElement('i');
            icon.classList.add('far');
            icon.setAttribute('data-ng-class', "{\'fa-chevron-down\': ctrl.showStaffingNeedsAgChart, \'fa-chevron-right\': !ctrl.showStaffingNeedsAgChart}");
            name.append(icon);
            attachmentPoint.append(this.$compile(name)(this.$scope));

            let td = document.createElement('td');
            td.classList.add('staffing-needs-ag-chart');
            td.setAttribute('colspan', (this.controller.dates.length + 1).toString());
            attachmentPoint.append(td);
        }
    }

    public renderStaffingNeedsAgChart() {
        if (this.controller.showStaffingNeedsAgChart) {
            if (!this.staffingNeedsChartElem) {
                this.staffingNeedsChartElem = document.createElement('div');
                this.staffingNeedsChartElem.id = "chart";
                this.staffingNeedsChartElem.setAttribute('data-ng-show', "ctrl.showStaffingNeedsAgChart");

                let attachmentPoint = $('.staffing-needs-ag-chart');
                if (attachmentPoint)
                    attachmentPoint.append(this.$compile(this.staffingNeedsChartElem)(this.$scope));

                this.loadChartScript(this.staffingNeedsChartElem).then(() => {
                    this.createOrUpdateStaffingNeedsAgChart();
                })
            } else {
                this.createOrUpdateStaffingNeedsAgChart();
            }
        }
    }

    private createOrUpdateStaffingNeedsAgChart() {
        this.setStaffingNeedsAgChartOptions();
        if (!this.staffingNeedsChartRef) {
            this.staffingNeedsChartRef = agCharts.AgCharts.create(this.staffingNeedsChartOptions);
        } else {
            this.staffingNeedsChartOptions.data = this.staffingNeedsChartData.rows;
            agCharts.AgCharts.update(this.staffingNeedsChartRef, this.staffingNeedsChartOptions);
        }
    }

    public setStaffingNeedsChartData(data: any) {
        this.staffingNeedsChartData = data;
    }

    private setStaffingNeedsAgChartOptions() {
        let options = new AgChartOptionsLineNew();
        options.height = this.chartHeight;
        options.width = this.chartWidth;
        options.title = "";
        options.paddingLeft = this.getStaffingNeedsOptionsPaddingLeft();
        options.paddingTop = 0;
        options.paddingBottom = 5;
        options.paddingRight = 0;
        options.legendEnabled = false;

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
                tick: {
                    interval: 3600000   // One hour
                },
                gridLine: {
                    style: [
                        {
                            stroke: "#dfdfdf", // @soe-border-color
                            lineDash: [],
                        },
                    ],
                },
                label: {
                    formatter: function (params) {
                        return CalendarUtility.toFormattedTime(params.value)
                    }
                }
            }
        ];

        if (this.staffingNeedsChartData.maxValue < 10) {
            // Only specify tick inteval if maxValue is low.
            // If not specified it will handle it on it own, mostly well,
            // but if maxValue for example is as low as 2, there will be multiple ticks with 1 and 2.
            options.axes[0].tick = { interval: 1 };
        }

        options.series = [
            {
                type: 'line',
                xKey: 'date',
                yKey: 'value',
                yName: this.staffingNeedsChartData.name,
                stroke: '#00CC00',
                marker: {
                    fill: '#00CC00',
                    stroke: '#00CC00',
                    size: 2
                },
                tooltip: {
                    renderer: function (params) {
                        const htmlStart = '<div class="ag-chart-tooltip-content" style="border-top: 1px solid #b6b6b6; text-align: right;">';
                        const divEnd = '</div>';
                        const date = CalendarUtility.toFormattedTime(params.datum.date);
                        const label = params.yName;
                        const value = NumberUtility.printDecimal(params.datum.value, 0, 0);

                        return '<div class="ag-chart-tooltip-title" style="background-color: #00CC00; color: #FFFFFF">' +
                            date +
                            divEnd +
                            htmlStart + label + " " + value + divEnd;
                    }
                },
                highlightStyle: {
                    item: {
                        fill: '#00CC00',
                        stroke: '#00CC00',
                        strokeWidth: 4,
                    }
                }
            }
        ];

        this.staffingNeedsChartOptions = AgChartUtilityNew.createDefaultLineChart(this.staffingNeedsChartElem, this.staffingNeedsChartData.rows, options);
    }

    private getStaffingNeedsOptionsPaddingLeft(): number {
        if (!this.staffingNeedsChartData)
            return 0;

        // Padding depends on number of digits
        const nbrOfDigits = (this.staffingNeedsChartData.maxValue + 3).toString().length;
        return 62 - (nbrOfDigits * 7);
    }

    // HELP-METHODS

    private loadChartScript(chartElem: Element): ng.IPromise<any> {
        const deferral = this.$q.defer<any>();

        if (!this.isScriptLoaded && chartElem) {
            let form = chartElem.closest("form");
            if (form) {
                let script = document.createElement('script');
                script.setAttribute('src', this.chartScriptUrl);
                script.setAttribute('type', "text/javascript");
                form.parentElement.appendChild(script);

                // Script will take a little while to load,
                // so first there is a loop until script is created,
                // then another timeout for the script to load.
                let cancel = this.$interval(() => {
                    if (this.isScriptLoaded) {
                        this.$interval.cancel(cancel);
                        this.$timeout(() => {
                            deferral.resolve();
                        }, 200);
                    }
                }, 200, 10);
            }
        } else {
            deferral.resolve();
        }

        return deferral.promise;
    }

    private get isScriptLoaded(): boolean {
        return !!document.querySelector("script[src*='" + this.chartScriptUrl + "']");
    }
}