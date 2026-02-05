import { IIdListSelectionDTO } from "../../../../../Scripts/TypeLite.Net4";
import { IdListSelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";
import { SmallGenericType } from "../../../../../Common/Models/SmallGenericType";

export class IdListSelection {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: IdListSelection,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/Selections/IdListSelection/IdListSelectionView.html",
            bindings: {
                onSelected: "&",
                labelKey: "@",
                hideLabel: "<",
                items: "<",
                selected: "<",
                showSelected: "<",
                userSelectionInput: "="
            }
        };

        return options;
    }

    public static componentKey = "idListSelection";

    //binding properties
    private labelKey: string;    
    private onSelected: (_: { selection: IdListSelectionDTO }) => void = angular.noop;
    private userSelectionInput: IdListSelectionDTO;

    private items: SmallGenericType[];
    private selected: SmallGenericType[] = [];
    private showSelected: boolean;

    //@ngInject
    constructor(private $scope: ng.IScope, private $timeout: ng.ITimeoutService) {
        this.$scope.$watch(() => this.userSelectionInput, () => {
            this.setSavedUserSelection();
        });
    }

    public $onInit() {
        if (!this.selected)
            this.selected = [];
        
        this.propagateChange();
    }

    private setSavedUserSelection() {
        if (!this.userSelectionInput)
            return;

        this.selected = [];
        if (this.userSelectionInput.ids && this.userSelectionInput.ids.length > 0)
            this.selected = _.filter(this.items, a => _.includes(this.userSelectionInput.ids, a.id));

        this.propagateChange();
    }

    private onChange() {
        this.$timeout(() => {
            this.propagateChange();
        });
    }

    private propagateChange() { 
        let selectedIds = this.selected.map(s => s.id)
        const selection = new IdListSelectionDTO(selectedIds);

        if (!selection)
            return;

        this.onSelected({selection });
    }
}