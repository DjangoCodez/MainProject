import { IBoolSelectionDTO } from "../../../../../Scripts/TypeLite.Net4";
import { BoolSelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";

export class BoolSelection {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: BoolSelection,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/Selections/BoolSelection/BoolSelectionView.html",
            bindings: {
                onSelected: "&",
                labelKey: "@",
                hideLabel: "<",
                isDisabled:"<",
                userSelectionInput: "="
            }
        };

        return options;
    }

    public static componentKey = "boolSelection";

    //binding properties
    private labelKey: string;
    private isDisabled: boolean;

    private onSelected: (_: { selection: IBoolSelectionDTO }) => void = angular.noop;
    private selected: boolean;
    private userSelectionInput: BoolSelectionDTO;

    //@ngInject
    constructor(private $scope: ng.IScope, private $timeout: ng.ITimeoutService) {
        this.$scope.$watch(() => this.userSelectionInput, () => {
            this.setSavedUserSelection();
        });
    }

    public $onInit() {
        this.selected = false;

        this.propagateChange(this.selected);
    }

    private setSavedUserSelection() {
        if (!this.userSelectionInput)
            return;

        this.selected = this.userSelectionInput.value;
        this.propagateChange(this.selected);
    }

    private onChange() {
        this.$timeout(() => {
            this.propagateChange(this.selected);
        });
    }

    private propagateChange(selected: boolean) {
        const selection = new BoolSelectionDTO(selected);
        this.onSelected({ selection: selection });
    }
}