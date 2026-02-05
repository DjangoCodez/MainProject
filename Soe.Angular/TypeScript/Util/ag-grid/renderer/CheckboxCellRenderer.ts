import { ICellRendererParams, ICellRendererComp } from "./ICellRendererComp";

export interface ICheckboxCellRendererParams {
    disabled: boolean | string;
}

declare type Params = ICellRendererParams & ICheckboxCellRendererParams;

export class CheckboxCellRenderer implements ICellRendererComp {
    private cellElement: HTMLDivElement;

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

    private buildGuiElement(params: Params): HTMLDivElement {
        const { value, api, disabled, data, node, rowIndex, colDef, context } = params;
        const inputElement = document.createElement("input");
        inputElement.setAttribute("type", "checkbox");
        inputElement.checked = value;
        inputElement.disabled = typeof disabled === "string" && data ? data[disabled] : disabled;
        inputElement.addEventListener("change", (e) => {
            const isChecked = (e.target as any).checked;
            data[colDef.field] = isChecked;
            api.dispatchEvent({
                type: 'cellValueChanged',
                node,
                data,
                oldValue: value,
                newValue: isChecked,
                rowIndex,
                colDef,
                context,
                api
            });
        });

        let container: HTMLDivElement = document.createElement("div");
        container.appendChild(inputElement);
        container.style.textAlign = "center";

        return container;
    }
}
