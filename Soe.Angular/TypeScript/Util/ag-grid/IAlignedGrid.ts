export interface IAlignedGrid {
    processMainGridInitialize(gridOptions: any): void;
    getOptions(): any;
    setColumnDefs(columnDefs: any[]);
    refresh(): void;
    agGrid: any;
}

export class Dummy { } // To make sure it loads properly. Interfaces disappear during transpile