import { AccountDimSmallDTO } from "../../../../../Common/Models/AccountDimDTO";
import { AccountDTO } from "../../../../../Common/Models/AccountDTO";
import { ITranslationService } from "../../../../Services/TranslationService";

interface MultiSelectViewModel {
    id: string;
    label: string;
}

export class AccountRangeSelection {
    public static component(): ng.IComponentOptions {
        return {
            controller: AccountRangeSelection,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/Selections/AccountRangeSelection/AccountRangeSelectionView.html",
            bindings: {
                filterRanges: "<",
                selectableRangeNames: "<",
                setValueOnBlur: "=?"
            }
        } as ng.IComponentOptions;
    }

    public static componentKey = "accountRangeSelection";

    private filterRanges: NamedFilterRange[];
    private selectableRangeNames: AccountDimSmallDTO[];
    private setValueOnBlur: boolean = false;

    private isFromSelected: boolean = false;
    private isToSelected: boolean = false;

    //@ngInject
    constructor(private $scope: ng.IScope, $timeout: ng.ITimeoutService, translationService: ITranslationService) {
        $scope.$watch(() => this.filterRanges, (newVal, oldVal) => {
            for (let range of newVal) {
                if (!range.accountFrom && range.selectionFrom) {
                    range.accountFrom = new AccountDTO();
                    range.accountFrom.numberName = range.selectionFrom;
                }

                if (!range.accountTo && range.selectionTo) {
                    range.accountTo = new AccountDTO();
                    range.accountTo.numberName = range.selectionTo;
                }
            }
        });
    }

    public $onInit() {
    }

    private addRow() {
        const row = new NamedFilterRange(this.selectableRangeNames);
        row.selectedSelection = this.selectableRangeNames[0];
        this.filterRanges.push(row);
    }
    private deleteRow(selection: number) {
        if (this.filterRanges.length != 1)
            this.filterRanges.splice(selection, 1);
    }

    private onRangeNameChanged(item: AccountDimSmallDTO, filterRange: NamedFilterRange) {
        filterRange.selectedSelection = item;
        filterRange.accountFrom = new AccountDTO();
        filterRange.selectionFrom = "";
        filterRange.accountTo = new AccountDTO();
        filterRange.selectionTo = "";
    }

    private onOptionFromChanged(item: AccountDTO, filterRange: NamedFilterRange) {
        filterRange.accountFrom = item;
        filterRange.selectionFrom = item.accountNr;
        this.isFromSelected = true;
        if (!filterRange.selectionTo)
            this.onOptionToChanged(item, filterRange);
        const commonAncestorElement = document.activeElement?.closest('div.row') as HTMLElement | null;
        const rangeToElement = commonAncestorElement?.querySelector<HTMLInputElement>("#filterRangeAccountTo input");
        if (rangeToElement) {
            setTimeout(() => { rangeToElement.focus() }, 50);
        }
    }

    private onOptionToChanged(item: AccountDTO, filterRange: NamedFilterRange) {
        filterRange.accountTo = item;
        filterRange.selectionTo = item.accountNr;
        this.isToSelected = true;
    }

    private onFromFocus(): void {
        this.isFromSelected = false;
    }

    private onToFocus(): void {
        this.isToSelected = false;
    }

    private onFromBlur(value: string, filterRange: NamedFilterRange): void {
        if (this.setValueOnBlur && value && !this.isFromSelected) {
            filterRange.selectionFrom = value;
            filterRange.accountFrom = new AccountDTO();
            filterRange.accountFrom.numberName = value;
        }
    }

    private onToBlur(value: string, filterRange: NamedFilterRange): void {
        if (this.setValueOnBlur && value && !this.isToSelected) {
            filterRange.selectionTo = value;
            filterRange.accountTo = new AccountDTO();
            filterRange.accountTo.numberName = value;
        }
    }
}

export class NamedFilterRange {

    constructor(public availableSelectionNames: AccountDimSmallDTO[]) {
        this.selectionFrom = "";
        this.selectionTo = "";
        this.selectedSelection = null;
        this.accountFrom = new AccountDTO();
        this.accountTo = new AccountDTO();
    }

    public selectedSelection: AccountDimSmallDTO;
    public selectionFrom: string;
    public selectionTo: string;
    public accountFrom: AccountDTO;
    public accountTo: AccountDTO;

    public getIndexOfSelected(): number {
        if (!this.selectedSelection) return -1;
        return this.availableSelectionNames.findIndex(x => x.accountDimId === this.selectedSelection.accountDimId);
    }
}