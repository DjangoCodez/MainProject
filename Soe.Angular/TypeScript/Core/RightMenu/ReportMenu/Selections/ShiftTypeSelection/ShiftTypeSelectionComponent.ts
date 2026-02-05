import { IIdListSelectionDTO } from "../../../../../Scripts/TypeLite.Net4";
import { IdListSelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";
import { SmallGenericType } from "../../../../../Common/Models/SmallGenericType";
import { IReportDataService } from "../../ReportDataService";

export class ShiftTypeSelection {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: ShiftTypeSelection,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/Selections/ShiftTypeSelection/ShiftTypeSelectionView.html",
            bindings: {
                onSelected: "&",
                labelKey: "@",
                hideLabel: "<",
                userSelectionInput: "="
            }
        };

        return options;
    }

    public static componentKey = "shiftTypeSelection";

    //binding properties
    private labelKey: string;
    private onSelected: (_: { selection: IIdListSelectionDTO }) => void = angular.noop;
    private userSelectionInput: IdListSelectionDTO;

    private selectedShiftTypes: SmallGenericType[] = [];
    private shiftTypes: SmallGenericType[] = [];

    private delaySetSavedUserSelection: boolean = false;

    //@ngInject
    constructor(private $scope: ng.IScope, private reportDataService: IReportDataService) {
        this.reportDataService.getShiftTypes().then(x => {
            this.shiftTypes = x;
            if (this.delaySetSavedUserSelection)
                this.setSavedUserSelection();
        });

        this.$scope.$watch(() => this.userSelectionInput, () => {
            this.setSavedUserSelection();
        });
    }

    public $onInit() {
        this.selectedShiftTypes = [];

        this.propagateSelection();
    }

    private setSavedUserSelection() {
        if (!this.userSelectionInput)
            return;

        if (this.shiftTypes.length === 0) {
            this.delaySetSavedUserSelection = true;
            return;
        }

        this.selectedShiftTypes = [];
        if (this.userSelectionInput.ids && this.userSelectionInput.ids.length > 0)
            this.selectedShiftTypes = _.filter(this.shiftTypes, s => _.includes(this.userSelectionInput.ids, s.id));

        this.propagateSelection();
    }

    private propagateSelection() {
        let selection: IIdListSelectionDTO = new IdListSelectionDTO(this.selectedShiftTypes.map(g => g.id));

        this.onSelected({ selection: selection });
    }
}