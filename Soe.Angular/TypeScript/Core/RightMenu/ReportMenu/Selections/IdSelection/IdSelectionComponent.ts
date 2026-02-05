import { IIdSelectionDTO } from "../../../../../Scripts/TypeLite.Net4";
import { IdSelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";
import { SmallGenericType } from "../../../../../Common/Models/SmallGenericType";

export class IdSelection {
    public static component(): ng.IComponentOptions {
        return {
            controller: IdSelection,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/Selections/IdSelection/IdSelectionView.html",
            bindings: {
                onSelected: "&",
                labelKey: "@",
                labelValue: '=?',
                hideLabel: "<",
                items: "<",
                selected: "<",
                userSelectionInput: "="
            }
        };
    }

    public static componentKey = "idSelection";

    //binding properties
    private labelKey: string;
    private labelValue: string;
    private items: SmallGenericType[];
    private onSelected: (_: { selection: IIdSelectionDTO }) => void = angular.noop;
    private userSelectionInput: IdSelectionDTO;

    private selected: number;

    //@ngInject
    constructor(private $scope: ng.IScope, private $timeout: ng.ITimeoutService) {
        this.$scope.$watch(() => this.userSelectionInput, () => {
            this.setSavedUserSelection();
        });
    }

    public $onInit() {
        if (!this.selected)
            this.selected = 0;
        
        this.propagateChange(this.selected);
    }

    private setSavedUserSelection() {
        if (!this.userSelectionInput)
            return;

        this.selected = this.userSelectionInput.id;

        this.propagateChange(this.selected);
    }

    private onChange() {
        this.$timeout(() => {
            this.propagateChange(this.selected);
        });
    }

    private propagateChange(selected: number) {
        const selection = new IdSelectionDTO(selected);
        this.onSelected({ selection: selection });
    }
}