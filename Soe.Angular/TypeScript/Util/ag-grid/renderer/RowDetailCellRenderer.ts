import { ICellRendererParams, ICellRendererComp } from "@ag-grid-community/core/dist/cjs/main";
import { DataCallback } from "../../SoeGridOptionsAg";

export interface IRowDetailCellRendererParams {
    directiveName: string;
    onResize?: DataCallback;
    disabled: boolean | string;
}

declare type Params = ICellRendererParams & IRowDetailCellRendererParams;

export class RowDetailCellRenderer implements ICellRendererComp {
    private cellElement: HTMLElement;

    init?(params: any): void {
        this.cellElement = this.buildGuiElement(params);
    }

    getGui(): HTMLElement {
        return this.cellElement;
    }

    destroy?(): void;

    refresh(params: any): boolean {
        return true;
    }

    private buildGuiElement(params: Params): HTMLElement {
        const { data, node, onResize } = params;

        const directive: HTMLElement = document.createElement(params.directiveName);
        directive.setAttribute('data', 'data');
        directive.setAttribute('row-node-id', 'rowNodeId');
        directive.setAttribute('resize-callback', 'onResize');
        directive.setAttribute('grid-node-id', node.id);
        
        params.$scope.data = data;
        params.$scope.rowNodeId = node.id;
        params.$scope.onResize = onResize;
        params.$scope.params = params;

        params["$compile"](directive)(params.$scope);

        return directive;
    }
}