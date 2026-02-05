import { PayrollProductRowSelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";
import { Guid } from "../../../../../Util/StringUtility";

interface ISelectionRowModel {
    key: string;
    canDelete: boolean;
    selection: PayrollProductRowSelectionDTO;
}

export class PayrollProductSelection {
    public static component(): ng.IComponentOptions {
        const options: ng.IComponentOptions = {
            controller: PayrollProductSelection,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/Selections/PayrollProductSelection/PayrollProductSelectionView.html",
            bindings: {
                onSelected: "&",
                labelKey: "@",
                hidelabel: "<",
                hideProducts: "<",
                canAdd: "<",
                userSelectionInput: "="
            }
        };

        return options;
    }

    public static componentKey = "payrollProductSelection";

    private rowSelections: ISelectionRowModel[] = [];
    private onSelected: (_: { selections: PayrollProductRowSelectionDTO[] }) => void = () => { };
    private userSelectionInput: PayrollProductRowSelectionDTO[];

    //@ngInject
    constructor(private $scope: ng.IScope) {
        this.rowSelections.push({
            key: "",
            canDelete: false,
            selection: null
        });

        this.$scope.$watch(() => this.userSelectionInput, () => {
            this.setSavedUserSelection();
        });
    }

    private setSavedUserSelection() {
        if (!this.userSelectionInput)
            return;

        this.rowSelections = [];
        _.forEach(this.userSelectionInput, selection => {
            this.rowSelections.push({ key: selection.key+ Math.floor((Math.random() * 1234) + 1), canDelete: true, selection: selection });
        });

        this.propagateAggregatedSelection();
    }

    private onPayrollSelected(key: string, selection: PayrollProductRowSelectionDTO) {
        this.applyForKey(key, (index) => {
            this.rowSelections[index].selection = selection;
        });

        this.propagateAggregatedSelection();
    }

    private removeSelection(key: string) {
        this.applyForKey(key, (index) => {
            this.rowSelections.splice(index, 1);
        });

        this.propagateAggregatedSelection();
    }

    private addSelection() {
        this.rowSelections.push({
            key: Guid.newGuid(),
            canDelete: true,
            selection: null
        });
    }

    private applyForKey(key: string, callback: (index: number) => void) {
        const index = this.rowSelections.findIndex(r => r.key === key);
        if (index > -1) {
            callback(index);
        }
    }

    private propagateAggregatedSelection() {
        const selections = this.rowSelections.filter(s => s.selection).map(s => s.selection);

        this.onSelected({ selections });
    }
}