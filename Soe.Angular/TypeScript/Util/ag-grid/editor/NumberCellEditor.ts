import { FieldOrPredicate } from "../../SoeGridOptionsAg";
import { ICellEditorComp, ICellEditorParams } from "./ICellEditorComp";
import { ObjectFieldHelper } from "../../ObjectFieldHelper";

export interface INumberCellEditorParams {
    isDisabled: FieldOrPredicate;
}

declare type Params = ICellEditorParams & INumberCellEditorParams;

export class NumberCellEditor implements ICellEditorComp {
    private cellElement: HTMLInputElement;

    public init?(params: ICellEditorParams): void {
        this.cellElement = this.buildCellElement(params as any);
    }

    // Gets called once after GUI is attached to DOM.
    // Useful if you want to focus or highlight a component
    // (this is not possible when the element is not attached)
    public afterGuiAttached?(): void {
        this.cellElement.focus();
    }

    // Return the DOM element of your editor, this is what the grid puts into the DOM
    public getGui(): HTMLElement {
        return this.cellElement;
    }

    // Should return the final value to the grid, the result of the editing
    public getValue(): any {
        return this.cellElement.value;
    }

    // Gets called once by grid after editing is finished
    // if your editor needs to do any cleanup, do it here
    public destroy?(): void {
        this.cellElement.blur();
    }

    // Gets called once after initialised.
    // If you return true, the editor will appear in a popup
    isPopup?(): boolean {
        return false;
    }

    // Gets called once before editing starts, to give editor a chance to
    // cancel the editing before it even starts.
    isCancelBeforeStart?(): boolean;

    // Gets called once when editing is finished (eg if enter is pressed).
    // If you return true, then the result of the edit will be ignored.
    isCancelAfterEnd?(): boolean;

    // If doing full row edit, then gets called when tabbing into the cell.
    focusIn?(): boolean;

    // If doing full row edit, then gets called when tabbing out of the cell.
    focusOut?(): boolean;


    private buildCellElement(params: Params): HTMLInputElement {
        const { isDisabled, node } = params;
        const inputElement = document.createElement("input");

        if (ObjectFieldHelper.IsEvaluatedTrue(node.data, isDisabled)) {
            inputElement.setAttribute("disabled", "disabled");
        }

        return inputElement;
    }
}