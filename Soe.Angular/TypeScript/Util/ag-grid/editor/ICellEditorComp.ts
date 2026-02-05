export interface ICellEditorComp {

    // gets called once after the editor is created
    init?(params: ICellEditorParams): void;

    // Gets called once after GUI is attached to DOM.
    // Useful if you want to focus or highlight a component
    // (this is not possible when the element is not attached)
    afterGuiAttached?(): void;

    // Return the DOM element of your editor, this is what the grid puts into the DOM
    getGui(): HTMLElement;

    // Should return the final value to the grid, the result of the editing
    getValue(): any;

    // Gets called once by grid after editing is finished
    // if your editor needs to do any cleanup, do it here
    destroy?(): void;

    // Gets called once after initialised.
    // If you return true, the editor will appear in a popup
    isPopup?(): boolean;

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
}

export interface ICellEditorParams {
    value: any;
    keyPress: number;
    charPress: string;
    column: any; //Column;
    node: any;// RowNode;
    rowIndex: number;
    api: any; //GridApi;
    columnApi: any; // ColumnApi;
    cellStartedEdit: boolean;
    context: any;
    $scope: any;
    onKeyDown: (event: KeyboardEvent) => void;
    stopEditing: () => void;
    eGridCell: HTMLElement;
    parseValue: (value: any) => any;
    formatValue: (value: any) => any;
}