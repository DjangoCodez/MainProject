import { IIdListSelectionDTO } from "../../../../../Scripts/TypeLite.Net4";
import { IdListSelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";
import { SmallGenericType } from "../../../../../Common/Models/SmallGenericType";
import { IReportDataService } from "../../ReportDataService";

export class TimeAccumulatorSelection {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: TimeAccumulatorSelection,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/Selections/TimeAccumulatorSelection/TimeAccumulatorSelectionView.html",
            bindings: {
                onSelected: "&",
                labelKey: "@",
                hideLabel: "<",
                userSelectionInput: "=",
                includeVacationBalance: "@",
                includeWorkTimeAccountBalance: "@"
            }
        };

        return options;
    }

    public static componentKey = "timeAccumulatorSelection";

    //binding properties
    private includeVacationBalance: boolean;
    private includeWorkTimeAccountBalance: boolean;
    private labelKey: string;
    private onSelected: (_: { selection: IIdListSelectionDTO }) => void = angular.noop;
    private userSelectionInput: IdListSelectionDTO;

    private selectedAccumulators: SmallGenericType[] = [];
    private accumulators: SmallGenericType[] = [];

    private delaySetSavedUserSelection: boolean = false;

    //@ngInject
    constructor(private $scope: ng.IScope, private reportDataService: IReportDataService) {

        this.$scope.$watch(() => this.userSelectionInput, () => {
            this.setSavedUserSelection();
        });
    }

    public $onInit() {
        this.reportDataService.getTimeAccumulators(this.includeVacationBalance, this.includeWorkTimeAccountBalance).then(x => {
            this.accumulators = x;
            if (this.delaySetSavedUserSelection)
                this.setSavedUserSelection();
        });

        this.selectedAccumulators = [];

        this.propagateSelection();
    }

    private setSavedUserSelection() {
        if (!this.userSelectionInput)
            return;

        if (this.accumulators.length === 0) {
            this.delaySetSavedUserSelection = true;
            return;
        }

        this.selectedAccumulators = [];
        if (this.userSelectionInput.ids && this.userSelectionInput.ids.length > 0)
            this.selectedAccumulators = _.filter(this.accumulators, a => _.includes(this.userSelectionInput.ids, a.id));

        this.propagateSelection();
    }

    private propagateSelection() {
        let selection: IIdListSelectionDTO = new IdListSelectionDTO(this.selectedAccumulators.map(g => g.id));

        this.onSelected({ selection: selection });
    }
}