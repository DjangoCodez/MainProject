import { IAlignedGrid } from "./IAlignedGrid";
import { IColumnAggregate, IColumnAggregations } from "../SoeGridOptionsAg";

export class ColumnAggregationFooterGrid implements IAlignedGrid {
    private readOnlyGrid: boolean = false;
    private options: any;
    private mainGridOptions: any;
    private grid: any;
    public agGrid: any;

    constructor(private aggregations: IColumnAggregations, columnDefs: any[], private includeFilter?: (data) => boolean, readOnlyGrid?:boolean) {
        this.options = {
            rowData: [{}],
            alignedGrids: [],
            headerHeight: 0,
            suppressCellSelection: true,
            sideBar: false
        };

        this.readOnlyGrid = (readOnlyGrid) ? true : false;
        this.applyColumns(columnDefs);
        this.prepareAggregations();

        if (!this.includeFilter) {
            this.includeFilter = () => true;
        }
    }

    public getOptions(): any {
        return this.options;
    }

    public setColumnDefs(columnDefs: any[], aggs: IColumnAggregations = null) {
        if (aggs) {
            this.aggregations = aggs;
            this.applyColumns(columnDefs);
            this.prepareAggregations();
        }
        else {
            this.applyColumns(columnDefs);
            this.runAggregations(columnDefs);
        }
    }

    public processMainGridInitialize(gridOptions: any): void {
        this.mainGridOptions = gridOptions;
        const { api } = this.mainGridOptions; 
        if (!this.readOnlyGrid)
            api.addEventListener("cellValueChanged", (params) => this.runAggregations(null));
        api.addEventListener("rowDataChanged", () => this.runAggregations(null));
        api.addEventListener("modelUpdated", () => this.runAggregations(null));
        api.addEventListener("viewportChanged", (params) => this.options.api.refreshCells());
    }

    public refresh(): void {
        if (!this.readOnlyGrid)
            this.runAggregations(null);
    }

    private prepareAggregations() {
        for (const field in this.aggregations) {
            if (!this.aggregations.hasOwnProperty(field)) {
                continue;
            }

            if (typeof this.aggregations[field] === "string") {
                this.aggregations[field] = this.getStandardAccumulator(this.aggregations[field] as string);
            }
        }
    }

    private runAggregations(columnDefs?: any[]) {
        if (!this.mainGridOptions.api)
            return

        _(columnDefs || this.options.columnDefs as any[])
            .map(colDef => {
                const { field } = colDef;
                return { aggregate: this.aggregations[field] as IColumnAggregate, field }
            })
            .filter(x => !!x.aggregate)
            .forEach(x => {
                const { aggregate, field } = x;
                let acc = aggregate.getSeed();

                this.mainGridOptions.api.forEachNodeAfterFilterAndSort((n: any) => {
                    if (n.group || !this.includeFilter(n.data))
                        return;

                    const next = this.getValue(n, field);
                    if (next)
                        acc = aggregate.accumulator(acc, next);
                });
                this.setValue(this.options.api.getRowNode(0), field, acc);
            });

        this.options.api.refreshCells({ force: true });
    }

    private getValue(node: any, field: string): any {
        return node.data[field];
    }

    private setValue(node: any, field: string, value: any) {
        node.data[field] = value;
    }

    private applyColumns(columnDefs: any[]) {
        const alteredColumnDefs = (columnDefs as any[])
            .map((colDef) => {
                const config = this.aggregations[colDef.field] as IColumnAggregate;
                if (config) {
                    //reuse column as it is, but reset any editable properties
                    const { width, field, pinned, valueGetter, valueFormatter, cellRenderer, cellRendererParams, cellClass, hide } = colDef;
                    const columnAttributes = { width, field, pinned, valueGetter, valueFormatter, cellRenderer, cellRendererParams, cellClass, hide };
                    const configAttibutes = { cellClassRules: config.cellClassRule, cellRenderer: config.cellRenderer};
                    return _.assign({}, columnAttributes, configAttibutes);
                }
                else {
                    //NOTE: you can also include pinning if you desire that "look" in the grid later.
                    const { width, field, hide } = colDef;
                    return { width, field, hide };
                }
            });

        if (this.options.api) {
            this.options.api.setColumnDefs(alteredColumnDefs);
        }
        this.options.columnDefs = alteredColumnDefs;
    }

    private getStandardAccumulator(type: string): IColumnAggregate {
        switch (type) {
            case "sum": return { getSeed: () => 0, accumulator: (acc, next) => acc + next } as IColumnAggregate;
            case "max": return { getSeed: () => Number.MIN_VALUE, accumulator: (acc, next) => Math.max(acc, next) } as IColumnAggregate;
            case "min": return { getSeed: () => Number.MAX_VALUE, accumulator: (acc, next) => Math.min(acc, next) } as IColumnAggregate;
            default: return { getSeed: () => 0, accumulator: (acc, next) => acc + next } as IColumnAggregate;
        }
    }
}