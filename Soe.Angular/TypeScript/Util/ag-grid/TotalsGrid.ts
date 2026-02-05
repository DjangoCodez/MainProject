import { NumberUtility } from "../NumberUtility";
import { IAlignedGrid } from "./IAlignedGrid";

export interface ITotalRow {
    allCount: number;
    filteredCount: number;
    selectedCount: number;
}

export interface IDisplayTexts {
    prefixText?: string;
    total: string;
    filtered: string;
    tooltip?: string;
    selected?: string;
}

export class TotalsGrid implements IAlignedGrid {
    private id: string;
    private options;
    private totals: ITotalRow;
    private mainGridOptions: any;
    public agGrid: any;

    constructor(id: string, private texts: IDisplayTexts) {
        this.id = id;

        this.totals = {
            allCount: null,
            filteredCount: null,
            selectedCount: null,
        };

        this.options = {
            rowData: [this.totals],
            alignedGrids: [],
            columnDefs: [],
            isFullWidthCell: () => true,
            floatingFilter: false,
            headerHeight: 0,
            suppressContextMenu: true,
            sideBar: false,
            suppressCellSelection: true,
            fullWidthCellRenderer: this.cellRenderer
        };
    }
    public getOptions(): any {
        return this.options;
    }
    public setColumnDefs(columnDefs: any[]) {
        return;
    }

    public refresh(): void {
        this.refreshCounts();
    }

    public processMainGridInitialize(gridOptions: any): void {
        this.mainGridOptions = gridOptions;
        const { api } = this.mainGridOptions;
        api.addEventListener("modelUpdated", (params) => this.refreshCounts());
        api.addEventListener("rowSelected", (params) => this.refreshCounts());
    }

    private cellRenderer = (params) => {
        const data = params.data as ITotalRow;
        const divElement = document.createElement("div");
        divElement.classList.add("soe-ag-totals-row-part", "pull-right");

        if (data.allCount === null || data.allCount === undefined) {
            return divElement;
        }

        if (this.texts.prefixText) {
            const prefixTextElement = document.createElement("span");
            prefixTextElement.classList.add("soe-ag-totals-row-part");
            prefixTextElement.innerText = this.texts.prefixText;
            divElement.appendChild(prefixTextElement);
        }

        const totalCountElement = document.createElement("span");
        totalCountElement.classList.add("soe-ag-totals-row-part", "soe-ag-grid-totals-all-count");
        totalCountElement.innerText = data.allCount ? this.texts.total + " " + NumberUtility.printDecimal(data.allCount) : "";
        totalCountElement.innerText = data.allCount ? this.texts.total + " " + NumberUtility.printDecimal(data.allCount) : "";
        divElement.appendChild(totalCountElement);

        if (data.filteredCount < data.allCount) {
            const filteredCountElement = document.createElement("span");
            filteredCountElement.classList.add("soe-ag-totals-row-part", "soe-ag-grid-totals-filtered-count");
            filteredCountElement.innerText = "(" + this.texts.filtered + " " + NumberUtility.printDecimal(data.filteredCount) + ")";
            divElement.appendChild(filteredCountElement);
        }

        if (this.texts.selected && data.selectedCount > 0) {
            const selectedCountElement = document.createElement("span");
            selectedCountElement.classList.add("soe-ag-totals-row-part", "soe-ag-grid-totals-filtered-count");
            selectedCountElement.innerText = "(" + this.texts.selected + " " + NumberUtility.printDecimal(data.selectedCount) + ")";
            divElement.appendChild(selectedCountElement);
        }

        if (this.texts.tooltip) {
            $(divElement).attr('title', this.texts.tooltip);
        }

        return divElement;
    };

    private refreshCounts(): void {
        this.totals.allCount = 0;
        this.totals.filteredCount = 0;
        this.totals.selectedCount = 0;

        this.mainGridOptions.api.forEachLeafNode((node) => {
            this.totals.allCount++
            if (node.selected)
                this.totals.selectedCount++;
        });
        this.mainGridOptions.api.forEachNodeAfterFilterAndSort((n) => this.totals.filteredCount += n.group ? 0 : 1);

        if (this.options.api)
            this.options.api.redrawRows(); // full width rows doesn't have any cells.
    }
}