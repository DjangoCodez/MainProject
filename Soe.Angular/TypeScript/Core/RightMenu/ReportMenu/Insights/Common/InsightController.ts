import { MatrixColumnsSelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";
import { ReportUserSelectionDTO } from "../../../../../Common/Models/ReportDTOs";
import { ITranslationService } from "../../../../Services/TranslationService";

export class InsightController {
    public static component(): ng.IComponentOptions {
        return {
            controller: InsightController,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/Insights/Common/Insight.html",
            bindings: {
                userSelection: "=",
                matrixSelection: '<',
                rows: '<',
                hideTitle: "=",
                containerId: "=",
            },
            controllerAs: "ctrl",
        };
    }

    public static componentKey = "insight";

    // Data
    protected userSelection: ReportUserSelectionDTO;
    protected matrixSelection: MatrixColumnsSelectionDTO;
    protected rows: any[] = [];
    protected hideTitle: string;
    protected chartId: string;

    //@ngInject
    constructor(
        $scope: ng.IScope,
        $timeout: ng.ITimeoutService,
        translationService: ITranslationService) {
    }
}