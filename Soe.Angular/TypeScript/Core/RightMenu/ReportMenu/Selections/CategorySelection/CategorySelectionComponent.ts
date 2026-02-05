import { IIdListSelectionDTO } from "../../../../../Scripts/TypeLite.Net4";
import { IdListSelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";
import { SmallGenericType } from "../../../../../Common/Models/SmallGenericType";
import { IReportDataService } from "../../ReportDataService";

export class CategorySelection {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: CategorySelection,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/Selections/CategorySelection/CategorySelectionView.html",
            bindings: {
                onSelected: "&",
                labelKey: "@",
                hideLabel: "<",
                userSelectionInput: "="
            }
        };

        return options;
    }

    public static componentKey = "categorySelection";

    //binding properties
    private labelKey: string;
    private onSelected: (_: { selection: IIdListSelectionDTO }) => void = angular.noop;
    private userSelectionInput: IdListSelectionDTO;

    private selectedCategories: SmallGenericType[] = [];
    private categories: SmallGenericType[] = [];

    private delaySetSavedUserSelection: boolean = false;

    //@ngInject
    constructor(private $scope: ng.IScope, private reportDataService: IReportDataService) {
        this.reportDataService.getEmployeeCategories().then(x => {
            this.categories = x;
            if (this.delaySetSavedUserSelection)
                this.setSavedUserSelection();
        });

        this.$scope.$watch(() => this.userSelectionInput, () => {
            this.setSavedUserSelection();
        });
    }

    public $onInit() {
        this.selectedCategories = [];

        this.propagateSelection();
    }

    private setSavedUserSelection() {
        if (!this.userSelectionInput)
            return;

        if (this.categories.length === 0) {
            this.delaySetSavedUserSelection = true;
            return;
        }

        this.selectedCategories = [];
        if (this.userSelectionInput.ids && this.userSelectionInput.ids.length > 0)
            this.selectedCategories = _.filter(this.categories, c => _.includes(this.userSelectionInput.ids, c.id));

        this.propagateSelection();
    }

    private propagateSelection() {
        let selection: IIdListSelectionDTO = new IdListSelectionDTO(this.selectedCategories.map(c => c.id));

        this.onSelected({ selection: selection });
    }
}