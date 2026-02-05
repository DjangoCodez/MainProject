import { AgChartOptionsTreemap, AgChartUtility } from "../../../../../Util/ag-chart/AgChartUtility";
import { ITranslationService } from "../../../../Services/TranslationService";
import { InsightControllerBase } from "./InsightControllerBase";

declare var agCharts;

export class TreemapInsightController extends InsightControllerBase {
    public static component(): ng.IComponentOptions {
        return {
            controller: TreemapInsightController,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/Insights/Common/TreemapInsight.html",
            bindings: {
                userSelection: "=",
                matrixSelection: '<',
                rows: '<',
                hideTitle: "=",
                containerId: "=",
            }
        };
    }

    public static componentKey = "treemapInsight";
    private treeMapChartData: any = {};

    //@ngInject
    constructor(
        $scope: ng.IScope,
        $timeout: ng.ITimeoutService,
        translationService: ITranslationService) {

        super($scope, $timeout, translationService);
    }

    public $onInit() {
        this.init();
        super.setup().then(() => {
            this.createChart();
        });
    }

    protected createColumns() {
        super.createColumns();
    }

    // CHART

    protected setChartOptions() {
        let options: AgChartOptionsTreemap = new AgChartOptionsTreemap();
        super.setupContainer(options);

        this.addChartSeries(options);

        return AgChartUtility.createDefaultTreemapChart(this.chartElem, this.treeMapChartData, options);
    }

    // HELP-METHODS

    private addChartSeries(options) {
        if (!options.series)
            options.series = [];

        options.series = [
            {
                type: 'treemap',
                colorParents: true,
                colorDomain: [0, 1, 2, 3, 4],
                colorRange: ['#020305', '#09202C', '#1D3B4F', '#2C5C74', '#3D819A'],
                gradient: false,
                tooltip: {
                    renderer: (params) => {
                        return {
                            content: `Some more information can be presented here..`
                        };
                    },
                },
            },
        ];
    }

    protected setChartData() {
        this.treeMapChartData = {
            label: 'Root',
            children: []
        };

        const level1_group = _.groupBy(this.rows, r => r.accountInternalName1);
        const level1_group_arr = Object.keys(level1_group);
        _.forEach(level1_group_arr, level1_item => {
            if (level1_item != "undefined") {
                const level1 = {
                    label: level1_item,
                    children: [],
                    color: 0
                }

                const level2_group = _.groupBy(_.filter(this.rows, r => r.accountInternalName1 == level1_item), r => r.accountInternalName2);
                const level2_group_arr = Object.keys(level2_group);
                _.forEach(level2_group_arr, level2_item => {
                    if (level2_item != "undefined") {
                        const level2 = {
                            label: level2_item,
                            children: [],
                            color: 1
                        };

                        const level3_group = _.groupBy(_.filter(this.rows, r => r.accountInternalName1 == level1_item && r.accountInternalName2 == level2_item), r => r.accountInternalName3);
                        const level3_group_arr = Object.keys(level3_group);
                        _.forEach(level3_group_arr, level3_item => {
                            if (level3_item != "undefined") {
                                const level3 = {
                                    label: level3_item,
                                    children: [],
                                    color: 2
                                };

                                const level4_group = _.groupBy(_.filter(this.rows, r => r.accountInternalName1 == level1_item && r.accountInternalName2 == level2_item && r.accountInternalName3 == level3_item), r => r.accountInternalName4);
                                const level4_group_arr = Object.keys(level4_group);
                                _.forEach(level4_group_arr, level4_item => {
                                    if (level4_item != "undefined") {
                                        const level4 = {
                                            label: level4_item,
                                            children: [],
                                            color: 3
                                        };

                                        const level5_group = _.groupBy(_.filter(this.rows, r => r.accountInternalName1 == level1_item && r.accountInternalName2 == level2_item && r.accountInternalName3 == level3_item && r.accountInternalName4 == level4_item), r => r.accountInternalName5);
                                        const level5_group_arr = Object.keys(level5_group);
                                        _.forEach(level5_group_arr, level5_item => {
                                            const level5 = {
                                                label: level5_item,
                                                color: 4,
                                                size: 200
                                            };

                                            level4.children.push(level5);
                                        });

                                        level3.children.push(level4);
                                    }
                                });

                                level2.children.push(level3);
                            }
                        });

                        level1.children.push(level2);
                    }
                });

                this.treeMapChartData.children.push(level1);
            }
        });
    }
}