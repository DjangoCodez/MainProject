export class AgChartOptionsBaseNew {
    theme: string = 'ag-default';
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

export class AgChartOptionsBarNew extends AgChartOptionsBaseNew {
    constructor() {
        super();
    }
}

export class AgChartOptionsLineNew extends AgChartOptionsBaseNew {
    constructor() {
        super();
        this.paddingRight = 40;
    }
}

export class AgChartOptionsPieNew extends AgChartOptionsBaseNew {
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

export class AgChartOptionsDoughnutNew extends AgChartOptionsPieNew {
    innerRadiusOffset?: number = -70;
}

export class AgChartOptionsAreaNew extends AgChartOptionsBaseNew {
    constructor() {
        super();
    }
}

export class AgChartOptionsTreemapNew extends AgChartOptionsBaseNew {
    data: any;
    title: any;
    subtitle: any;

    constructor() {
        super();
    }
}

export class AgChartUtilityNew {

    private static createDefaultChart(container: Element, data: any[], options: AgChartOptionsBaseNew) {
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
                text: options.title,
                fontSize: 12,
            },
        }

        if (options.height)
            opt['height'] = options.height;
        if (options.width)
            opt['width'] = options.width;

        return opt;
    }

    public static createDefaultBaseChart(container: Element, data: any[], options: AgChartOptionsBaseNew) {
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

    public static createDefaultBarChart(container: Element, data: any[], options: AgChartOptionsBarNew) {
        let baseOpt = this.createDefaultBaseChart(container, data, options);

        const opt = {
        };

        angular.extend(opt, baseOpt);

        return opt;
    }

    public static createDefaultLineChart(container: Element, data: any[], options: AgChartOptionsLineNew) {
        let baseOpt = this.createDefaultBaseChart(container, data, options);

        const opt = {
            navigator: {
                enabled: false,
            },
        };

        angular.extend(opt, baseOpt);

        return opt;
    }

    public static createDefaultPieChart(container: Element, data: any[], options: AgChartOptionsPieNew) {
        let baseOpt = this.createDefaultChart(container, data, options);

        const opt = {
            series: [{
                type: 'pie',
                angleKey: 'value',
                angleName: options.angleName,
                calloutLabelKey: 'label',
                calloutLabel: {
                    minAngle: options.minAngle || 0
                },
                highlightStyle: {
                    item: {
                        fill: '#EEEEEE',
                        stroke: '#CCCCCC'
                    }
                },
                tooltip: {
                    renderer: function (params) {
                        return '<div class="ag-chart-tooltip-title" style="background-color:' + params.color + '">' +
                            params.datum[params.sectorLabelKey] +
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
            }
        };

        angular.extend(opt, baseOpt);

        return opt;
    }

    public static createDefaultDoughnutChart(container: Element, data: any[], options: AgChartOptionsDoughnutNew) {
        let opt = this.createDefaultPieChart(container, data, options);

        if (options.innerRadiusOffset)
            opt.series[0]['innerRadiusOffset'] = options.innerRadiusOffset;

        return opt;
    }

    public static createDefaultAreaChart(container: Element, data: any[], options: AgChartOptionsAreaNew) {
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

    public static createDefaultTreemapChart(container: Element, data: any, options: AgChartOptionsTreemapNew) {
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