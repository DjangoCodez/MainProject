export class AgChartOptionsBase {
    theme: string = 'ag-pastel';
    autoSize: boolean = true;
    height: number;
    width: number;
    title: string;
    paddingTop: number = 20;
    paddingRight: number = 20;
    paddingBottom: number = 20;
    paddingLeft: number = 20;
    backgroundVisible: boolean = false;

    axes: any[];
    series: any[];

    legendEnabled: boolean = true;
    legendPosition: 'top' | 'right' | 'bottom' | 'left' = 'bottom';
}

export class AgChartOptionsBar extends AgChartOptionsBase {
    constructor() {
        super();
    }
}

export class AgChartOptionsLine extends AgChartOptionsBase {
    constructor() {
        super();
        this.paddingRight = 40;
    }
}

export class AgChartOptionsPie extends AgChartOptionsBase {
    minAngle: number = 0;
    angleName: string;

    constructor() {
        super();
        if (this.minAngle < 360) {
            this.paddingTop = 40;
            this.paddingBottom = 40;
        }
    }
}

export class AgChartOptionsDoughnut extends AgChartOptionsPie {
    innerRadiusOffset?: number = -70;
}

export class AgChartOptionsArea extends AgChartOptionsBase {
    constructor() {
        super();
    }
}

export class AgChartOptionsTreemap extends AgChartOptionsBase {
    data: any;
    title: any;
    subtitle: any;

    constructor() {
        super();
    }
}

export class AgChartUtility {

    private static createDefaultChart(container: Element, data: any[], options: AgChartOptionsBase) {
        const opt = {
            theme: options.theme,
            autoSize: options.autoSize,
            padding: {
                top: options.paddingTop,
                right: options.paddingRight,
                bottom: options.paddingBottom,
                left: options.paddingLeft
            },
            background: {
                visible: options.backgroundVisible
            },
            container: container,
            data: data,
            title: {
                text: options.title
            }
        }

        if (options.height)
            opt['height'] = options.height;
        if (options.width)
            opt['width'] = options.width;

        return opt;
    }

    public static createDefaultBarChart(container: Element, data: any[], options: AgChartOptionsBar) {
        let baseOpt = this.createDefaultChart(container, data, options);

        const opt = {
            legend: {
                enabled: options.legendEnabled,
                position: options.legendPosition
            },
            axes: options.axes,
            series: options.series
        };

        angular.extend(opt, baseOpt);

        return opt;
    }

    public static createDefaultLineChart(container: Element, data: any[], options: AgChartOptionsLine) {
        let baseOpt = this.createDefaultChart(container, data, options);

        const opt = {
            navigator: {
                enabled: false,
            },
            legend: {
                enabled: options.legendEnabled,
                position: options.legendPosition
            },
            axes: options.axes,
            series: options.series
        };

        angular.extend(opt, baseOpt);

        return opt;
    }

    public static createDefaultPieChart(container: Element, data: any[], options: AgChartOptionsPie) {
        let baseOpt = this.createDefaultChart(container, data, options);

        const opt = {
            series: [{
                type: 'pie',
                labelKey: 'label',
                angleKey: 'value',
                angleName: options.angleName,
                label: {
                    minAngle: options.minAngle || 0
                },
                highlightStyle: {
                    fill: '#EEEEEE',
                    stroke: '#CCCCCC'
                },
                tooltip: {
                    renderer: function (params) {
                        return '<div class="ag-chart-tooltip-title" style="background-color:' + params.color + '">' +
                            params.datum[params.labelKey] +
                            '</div>' +
                            '<div class="ag-chart-tooltip-content">' +
                            params.datum[params.angleKey].toFixed(0) + ' ' + params.angleName +
                            '</div>';
                    }
                }
            }],
            legend: {
                enabled: options.legendEnabled,
                position: options.legendPosition,
                layoutHorizontalSpacing: 20,
            }
        };

        angular.extend(opt, baseOpt);

        return opt;
    }

    public static createDefaultDoughnutChart(container: Element, data: any[], options: AgChartOptionsDoughnut) {
        let opt = this.createDefaultPieChart(container, data, options);

        if (options.innerRadiusOffset)
            opt.series[0]['innerRadiusOffset'] = options.innerRadiusOffset;

        return opt;
    }

    public static createDefaultAreaChart(container: Element, data: any[], options: AgChartOptionsArea) {
        let baseOpt = this.createDefaultChart(container, data, options);

        const opt = {
            navigator: {
                enabled: false,
            },
            legend: {
                enabled: options.legendEnabled,
                position: options.legendPosition
            },
            axes: options.axes,
            series: options.series
        };

        angular.extend(opt, baseOpt);

        return opt;
    }

    public static createDefaultTreemapChart(container: Element, data: any, options: AgChartOptionsTreemap) {
        let baseOpt = this.createDefaultChart(container, undefined, options);

        const opt = {
            type: 'hierarchy',
            data: undefined,
            series: options.series
        };

        angular.extend(opt, baseOpt);

        // Need to be set after extending base options
        opt.data = data;

        return opt;
    }
}