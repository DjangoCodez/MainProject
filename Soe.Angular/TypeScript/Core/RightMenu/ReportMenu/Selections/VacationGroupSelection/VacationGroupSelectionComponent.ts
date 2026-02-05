import { IIdListSelectionDTO } from "../../../../../Scripts/TypeLite.Net4";
import { IdListSelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";
import { SmallGenericType } from "../../../../../Common/Models/SmallGenericType";
import { IReportDataService } from "../../ReportDataService";

export class VacationGroupSelection {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: VacationGroupSelection,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/Selections/VacationGroupSelection/VacationGroupSelectionView.html",
            bindings: {
                onSelected: "&",
                labelKey: "@",
                hideLabel: "<",
                userSelectionInput: "="
            }
        };

        return options;
    }

    public static componentKey = "vacationGroupSelection";

    //binding properties
    private labelKey: string;
    private onSelected: (_: { selection: IIdListSelectionDTO }) => void = angular.noop;
    private userSelectionInput: IdListSelectionDTO;

    private selectedVacationGroups: SmallGenericType[] = [];
    private vacationGroups: SmallGenericType[] = [];

    private delaySetSavedUserSelection: boolean = false;

    //@ngInject
    constructor(private $scope: ng.IScope, private reportDataService: IReportDataService) {
        this.reportDataService.getVacationGroups().then(x => {
            this.vacationGroups = x;
            if (this.delaySetSavedUserSelection)
                this.setSavedUserSelection();
        });

        this.$scope.$watch(() => this.userSelectionInput, () => {
            this.setSavedUserSelection();
        });
    }

    public $onInit() {
        this.selectedVacationGroups = [];

        this.propagateSelection();
    }

    private setSavedUserSelection() {
        if (!this.userSelectionInput)
            return;

        if (this.vacationGroups.length === 0) {
            this.delaySetSavedUserSelection = true;
            return;
        }

        this.selectedVacationGroups = [];
        if (this.userSelectionInput.ids && this.userSelectionInput.ids.length > 0)
            this.selectedVacationGroups = _.filter(this.vacationGroups, v => _.includes(this.userSelectionInput.ids, v.id));

        this.propagateSelection();
    }

    private propagateSelection() {
        let selection: IIdListSelectionDTO = new IdListSelectionDTO(this.selectedVacationGroups.map(g => g.id));

        this.onSelected({ selection: selection });
    }
}