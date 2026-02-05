import { ITextSelectionDTO } from "../../../../../Scripts/TypeLite.Net4";
import { TextSelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";

export class TextSelection {
    public static component(): ng.IComponentOptions {
        return  {
            controller: TextSelection,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/Selections/TextSelection/TextSelectionView.html",
            bindings: {
                onSelected: "&",
                labelKey: "@",
                hideLabel: "<",
                userSelectionInput: "="
            }
        };
    }

    public static componentKey = "textSelection";

    //binding properties
    private labelKey: string;
    private onSelected: (_: { selection: ITextSelectionDTO }) => void = angular.noop;
    private selected: string;
    private userSelectionInput: TextSelectionDTO;

    //@ngInject
    constructor(private $scope: ng.IScope, private $timeout: ng.ITimeoutService) {
        this.$scope.$watch(() => this.userSelectionInput, () => {
            this.setSavedUserSelection();
        });
    }

    public $onInit() {
        this.selected = undefined;
        this.propagateChange(this.selected);
    }

    private setSavedUserSelection() {
        if (!this.userSelectionInput)
            return;

        this.selected = this.userSelectionInput.text;
        this.propagateChange(this.selected);
    }

    private onChange() {
        this.$timeout(() => {
            this.propagateChange(this.selected);
        });
    }

    private propagateChange(selected: string) {
        const selection = new TextSelectionDTO(selected);
        this.onSelected({ selection: selection });
    }
}