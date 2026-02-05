import { ICellRendererParams, ICellRendererComp } from "./ICellRendererComp";
import { FieldOrEvaluator } from "../../SoeGridOptionsAg";
import { ObjectFieldHelper } from "../../ObjectFieldHelper";

export interface IExtendedTextCellRendererParams {
    error?: FieldOrEvaluator<string>;
    secondRow?: FieldOrEvaluator<string>;
    ignoreColumnOnGrouping?: boolean;
}

declare type Params = ICellRendererParams & IExtendedTextCellRendererParams;

export class ExtendedTextCellRenderer implements ICellRendererComp {
    private cellElement: HTMLElement;
    private error?: FieldOrEvaluator<string>;
    private secondRow?: FieldOrEvaluator<string>;
    private separator: string;

    public init?(params: any): void {
        this.error = params.error;
        this.secondRow = params.secondRow;
        this.separator = params.separator;
        this.cellElement = this.buildCellElement(params);
    }

    public getGui(): HTMLElement {
        return this.cellElement;
    }

    // Optional - Gets called once by grid after editing is finished - if your editor needs to do any cleanup,
    // do it here
    public destroy?(): void {
    }

    // Mandatory - Get the cell to refresh. Return true if the refresh succeeded, otherwise return false.
    // If you return false, the grid will remove the component from the DOM and create
    // a new component in it's place with the new values.
    public refresh(params: ICellRendererParams): boolean {
        if (this.error) {
            const errorElement = this.cellElement.querySelector(".soe-ag-ext-error") as HTMLElement;
            const displayElement = this.cellElement.querySelector(".soe-ag-ext-display") as HTMLElement;
            this.applyErrorValue(ObjectFieldHelper.getFieldFrom(params.data, this.error), errorElement, displayElement);
        }

        if (this.secondRow) {
            const secondRowElement = this.cellElement.querySelector(".soe-ag-ext-display .soe-ag-ext-display-second") as HTMLElement;
            secondRowElement.innerText = ObjectFieldHelper.getFieldFrom(params.data, this.secondRow);
        }

        const displayValueElement = this.cellElement.querySelector(".soe-ag-ext-display .soe-ag-ext-display-main") as HTMLElement;
        displayValueElement.innerText = this.getValue(params);

        return true;
    }

    private buildCellElement(params: Params): HTMLElement {
        const { error, data } = params;

        let container: HTMLSpanElement;

        if (error) {
            const errorValue = ObjectFieldHelper.getFieldFrom(data, error);
            container = document.createElement("div");

            const errorElement = document.createElement("div");
            const displayElement = this.buildDisplayOutputElement(params);

            errorElement.classList.add("soe-ag-ext-error");

            container.appendChild(errorElement);
            container.appendChild(displayElement);

            this.applyErrorValue(errorValue, errorElement, displayElement);

        }
        else {
            container = this.buildDisplayOutputElement(params);
        }

        //if (params.colDef && params.colDef.showRowGroup)
        //    container.classList.add("margin-large-left");

        container.classList.add("soe-ag-ext-container");
        return container;
    }

    private buildDisplayOutputElement(params: Params): HTMLDivElement {
        const { secondRow, node, ignoreColumnOnGrouping } = params;
        const data = this.getProperData(params);
        const container = document.createElement("div");
        container.classList.add("soe-ag-ext-display");

        if (!node.group) {
            container.appendChild(this.buildDisplayValueElement(params));
            if (secondRow) {
                const secondRowElement = document.createElement("span");
                secondRowElement.classList.add("soe-ag-ext-display-second");
                var secondValue = ObjectFieldHelper.getFieldFrom(data, secondRow);
                if (secondValue && this.separator)
                    secondValue = this.separator + " " + secondValue;
                secondRowElement.innerText = secondValue;

                container.classList.add("soe-ag-ext-display-multi");
                container.appendChild(secondRowElement);
            }
        }
        else {
            if (params.colDef && !params.colDef.showRowGroup && ignoreColumnOnGrouping)
                return container;

            const { colDef } = params;
            container.appendChild(this.buildDisplayValueElementFromColDef(data, colDef, true, params.value));

            if (secondRow) {
                const secondRowElement = document.createElement("span");
                secondRowElement.classList.add("soe-ag-ext-display-second");
                var secondValue = ObjectFieldHelper.getFieldFrom(data, secondRow);
                if (secondValue && this.separator)
                    secondValue = this.separator + " " + secondValue;
                secondRowElement.innerText = secondValue;

                container.classList.add("soe-ag-ext-display-multi");
                container.appendChild(secondRowElement);
            }
        }

        return container;
    }

    private getProperData(params: Params): any {
        const { node, data } = params;

        return node.group && node.allLeafChildren.length > 0 ? node.allLeafChildren[0].data : data;
    }

    private buildDisplayValueElement(params: Params): HTMLElement {
        const valueElement = document.createElement("span");
        valueElement.className = "soe-ag-ext-display-main"
        valueElement.innerText = this.getValue(params);

        return valueElement;
    }

    private buildDisplayValueElementFromColDef(data: any, colDef: any, useValue?: boolean, value?: any): HTMLElement {
        const valueElement = document.createElement("span");
        valueElement.className = "soe-ag-ext-display-main"
        valueElement.innerText = useValue && value ? value : ObjectFieldHelper.getFieldFrom(data, colDef.field);

        return valueElement;
    }

    private applyErrorValue(value: string, errorElement: HTMLElement, displayElement: HTMLElement) {
        if (value) {
            errorElement.classList.remove("hidden");
            displayElement.classList.add("hidden");
            errorElement.innerText = value;
        }
        else {
            errorElement.classList.add("hidden");
            displayElement.classList.remove("hidden");
        }
    }

    private getValue(params: ICellRendererParams) {
        const { valueFormatted, value } = params;

        return valueFormatted || value || "";
    }
}