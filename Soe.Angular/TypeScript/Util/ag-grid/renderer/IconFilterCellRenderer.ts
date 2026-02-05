import { ICellRendererComp, ICellRendererParams } from "@ag-grid-community/core/dist/cjs/main";
import { IIconCellRendererParams } from "./IconCellRenderer";
import { ElementHelper } from "../ElementHelper";

declare type Params = ICellRendererParams & IIconCellRendererParams;

export class IconFilterCellRenderer implements ICellRendererComp {
    private cellElement: HTMLElement;

    public refresh(params: any): boolean {
        return false;
    }

    public getGui(): HTMLElement {
        return this.cellElement;
    }

    public init(params: Params): void {
        const { icon, toolTip } = params;
        this.cellElement = this.buildGuiElement(params);
    }

    private buildGuiElement(params: ICellRendererParams): HTMLSpanElement {
        return this.buildForSingleIcon(params);
    }

    private buildForSingleIcon(params: ICellRendererParams): HTMLSpanElement {
        const { value } = params;
        if (value && value.includes(":")) {
            var parts = value.split(":");

            const container = document.createElement("div");
            let el = document.createElement('span');
            el.classList.add('gridCellIcon');
            ElementHelper.appendConcatClasses(el, parts[0]);

            let text = document.createElement('span');
            text.classList.add('margin-small-left');
            text.innerHTML = parts[1];

            container.appendChild(el);
            container.appendChild(text);

            return container;
        }
        else {
            let el = document.createElement('span');
            el.classList.add('gridCellIcon');
            ElementHelper.appendConcatClasses(el, value);
            return el;
        }
    }
}