//TODO: Copied from ag-grid documentation. Placeholder until getting typings is in place

export interface ICellRendererComp {
    // Optional - Params for rendering. The same params that are passed to the cellRenderer function.
    init?(params: any): void;

    // Mandatory - Return the DOM element of your editor, this is what the grid puts into the DOM
    getGui(): HTMLElement;

    // Optional - Gets called once by grid after editing is finished - if your editor needs to do any cleanup,
    // do it here
    destroy?(): void;

    // Mandatory - Get the cell to refresh. Return true if the refresh succeeded, otherwise return false.
    // If you return false, the grid will remove the component from the DOM and create
    // a new component in it's place with the new values.
    refresh(params: any): boolean;
}

export interface ICellRendererParams {
    value: any;
    valueFormatted?: any;
    getValue(): any;
    setValue(value: any): void;
    formatValue(value: any): any;
    node: any;
    data: any;
    column: any;
    colDef: any;
    $scope?: ng.IScope;
    rowIndex: number;
    api: any;
    columnApi: any;
    context: any;
    refreshCell(): void;
    eGridCell: HTMLElement;
    eParentOfValue: HTMLElement;
}

export class Dummy { } // To make sure it loads properly. Interfaces disappear during transpile