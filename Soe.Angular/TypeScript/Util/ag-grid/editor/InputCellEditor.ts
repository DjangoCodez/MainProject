import { ICellEditorParams, ICellEditorComp } from "./ICellEditorComp";
import { DelayedTabToNextCellStrategy } from "./DelayedTabToNextCell";

declare type Params = ICellEditorParams;

export class InputCellEditor implements ICellEditorComp {
    protected cellElement: HTMLInputElement;
    protected params: Params;
    protected highlightAllOnFocus: boolean;
    protected focusAfterAttached: boolean;
    protected handleEnterKey: any;
    protected tabToNextCell: Function;

    public init?(params: ICellEditorParams): void {
        this.handleEnterKey = this.createHandlerForTabToNextCellOnEnter();
        this.cellElement = this.buildCellElement(params as any);
        this.params = params;

        const tabStrategy = new DelayedTabToNextCellStrategy(params.api, params.rowIndex, params.column.colId);
        this.tabToNextCell = tabStrategy.tabToNextCell.bind(tabStrategy);
    }

    public afterGuiAttached?(): void {
        let eInput = this.cellElement;
        eInput.focus();
        if (this.highlightAllOnFocus) {
            eInput.select();
        } else {
            // when we started editing, we want the carot at the end, not the start.
            // this comes into play in two scenarios: a) when user hits F2 and b)
            // when user hits a printable character, then on IE (and only IE) the carot
            // was placed after the first character, thus 'apply' would end up as 'pplea'
            let length = eInput.value ? eInput.value.length : 0;
            if (length > 0) {
                eInput.setSelectionRange(length, length);
            }
        }
    }

    public getGui(): HTMLElement {
        return this.cellElement;
    }

    public getValue(): any {
        return this.params.parseValue(this.cellElement.value);
    }


    public destroy?(): void {
        this.cellElement.blur();
        this.cellElement.removeEventListener("keydown", this.handleNavigationKeys);
        this.cellElement.removeEventListener("keydown", this.handleEnterKey);
    }

    protected buildCellElement(params: Params): HTMLInputElement {
        const inputElement = document.createElement("input");
        inputElement.classList.add("ag-cell-edit-input");
        inputElement.setAttribute("type", "text");
        inputElement.value = params.value || "";

        let startValue: string;

        // cellStartedEdit is only false if we are doing fullRow editing

        this.focusAfterAttached = true;

        let keyPressBackspaceOrDelete = params.keyPress === 8 || params.keyPress === 46;

        if (keyPressBackspaceOrDelete) {
            startValue = '';
        } else if (params.charPress) {
            startValue = params.charPress;
        } else {
            startValue = this.getStartValue(params);
            if (params.keyPress !== 113) {
                this.highlightAllOnFocus = true;
            }
        }

        if (!_.isNil(startValue)) {
            inputElement.value = startValue;
        }

        inputElement.addEventListener("keydown", this.handleEnterKey);
        inputElement.addEventListener("keydown", this.handleNavigationKeys);

        return inputElement;
    }

    private getStartValue(params: ICellEditorParams) {
        let formatValue = params.formatValue || params.column.getColDef().refData;
        return formatValue ? params.formatValue(params.value) : params.value;
    }

    private handleNavigationKeys(event: KeyboardEvent) {
        const key = event.which || event.keyCode;

        const isArrowKey = key > 36 && key < 41;
        const isPageKey = key > 32 && key < 35;
        const isNavigationKey = isArrowKey || isPageKey;

        if (isNavigationKey) {
            event.stopPropagation();

            if (event.keyCode !== 37 && event.keyCode !== 39) {
                event.preventDefault();
            }
        }
    }

    private createHandlerForTabToNextCellOnEnter() {
        return (e: KeyboardEvent) => {
            const key = e.which || e.keyCode;
            const isEnterKey = key === 13;
            
            if (isEnterKey) {
                e.stopPropagation();
                this.tabToNextCell();
            }
        };
    }
}