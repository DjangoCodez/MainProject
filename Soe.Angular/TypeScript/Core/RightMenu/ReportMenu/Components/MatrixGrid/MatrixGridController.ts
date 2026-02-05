import { MatrixDefinition } from "../../../../../Common/Models/MatrixResultDTOs";
import { CalendarUtility } from "../../../../../Util/CalendarUtility";
import { MatrixDataType, TermGroup_MatrixGroupAggOption } from "../../../../../Util/CommonEnumerations";
import { CoreUtility } from "../../../../../Util/CoreUtility";
import { SoeGridOptionsEvent } from "../../../../../Util/Enumerations";
import { GridEvent } from "../../../../../Util/SoeGridOptions";
import { EmbeddedGridController } from "../../../../Controllers/EmbeddedGridController";
import { IGridHandlerFactory } from "../../../../Handlers/gridhandlerfactory";

export class MatrixGridController {
    public matrixGridHandler: EmbeddedGridController;
    private matrixGridInitialized: boolean = false;


    get totalsGridId() {
        return `totals-${this.containerId}`
    }

    get gridId() {
        return `grid-${this.containerId}`
    }

    public constructor(
        private $timeout,
        private containerId,
        gridHandlerFactory: IGridHandlerFactory,
    ) {
        this.matrixGridHandler = new EmbeddedGridController(gridHandlerFactory, "ReportMenu.MatrixGrid");
    }

    public setData(rows: any[]) {
        this.matrixGridHandler.gridAg.setData(rows);
    }

    public setupMatrixGrid(def: MatrixDefinition): ng.IPromise<any> {
        this.matrixGridHandler.gridAg.options.resetColumnDefs(true);

        this.matrixGridHandler.gridAg.options.addGroupCountAggFunction();
        this.matrixGridHandler.gridAg.options.addGroupAverageAggFunction();
        this.matrixGridHandler.gridAg.options.addGroupMedianAggFunction();
        this.matrixGridHandler.gridAg.options.addGroupTimeSpanSumAggFunction(false);
        this.matrixGridHandler.gridAg.options.addGroupTimeSpanMinAggFunction();
        this.matrixGridHandler.gridAg.options.addGroupTimeSpanMaxAggFunction();
        this.matrixGridHandler.gridAg.options.addGroupTimeSpanAverageAggFunction();
        this.matrixGridHandler.gridAg.options.addGroupTimeSpanMedianAggFunction();

        // TODO: Experimental
        if (CoreUtility.isSupportAdmin) {
            this.matrixGridHandler.gridAg.options.enableContextMenu = true;
            this.matrixGridHandler.gridAg.options.enableCharts = true;
        }

        let hasGroupBy: boolean = def.matrixDefinitionColumns.filter(c => c.hasOptions && c.options.groupBy).length > 0;
        let groupColList: any[] = [];
        _.forEach(def.matrixDefinitionColumns, colDef => {
            let alignLeft: boolean = colDef.hasOptions && colDef.options.alignLeft;
            let alignRight: boolean = colDef.hasOptions && colDef.options.alignRight;
            let clearZero = colDef.hasOptions && !!colDef.options.clearZero;
            let groupBy: boolean = colDef.hasOptions && !!colDef.options.groupBy;

            let useTimeColumn = colDef.matrixDataType === MatrixDataType.Time && (colDef.options.minutesToTimeSpan || colDef.options.minutesToDecimal || !colDef.hasOptions);

            let groupByOption: TermGroup_MatrixGroupAggOption = (colDef.hasOptions && colDef.options.groupOption ? colDef.options.groupOption : TermGroup_MatrixGroupAggOption.Sum);
            let aggFunction: string;
            switch (groupByOption) {
                case TermGroup_MatrixGroupAggOption.Sum:
                    aggFunction = useTimeColumn ? 'sumTimeSpan' : 'sum';
                    break;
                case TermGroup_MatrixGroupAggOption.Min:
                    aggFunction = useTimeColumn ? 'minTimeSpan' : 'min';
                    break;
                case TermGroup_MatrixGroupAggOption.Max:
                    aggFunction = useTimeColumn ? 'maxTimeSpan' : 'max';
                    break;
                case TermGroup_MatrixGroupAggOption.Count:
                    aggFunction = 'soeCount';
                    break;
                case TermGroup_MatrixGroupAggOption.Average:
                    aggFunction = useTimeColumn ? 'avgTimeSpan' : 'soeAvg';
                    break;
                case TermGroup_MatrixGroupAggOption.Median:
                    aggFunction = useTimeColumn ? 'medianTimeSpan' : 'median';
                    break;
            }

            switch (colDef.matrixDataType) {
                case (MatrixDataType.Boolean):
                    let colBool = this.matrixGridHandler.gridAg.addColumnBoolEx(colDef.field, colDef.title, null, { enableRowGrouping: true });
                    if (groupBy)
                        groupColList.push(colBool);
                    break;
                case (MatrixDataType.Date):
                    let dateFormat: string = colDef.hasOptions ? CalendarUtility.getDateFormatForMatrix(colDef.options.dateFormatOption) : '';
                    let colDate = this.matrixGridHandler.gridAg.addColumnDate(colDef.field, colDef.title, null, true, null, { alignRight: alignRight, dateFormat: dateFormat, enableRowGrouping: true });
                    if (groupBy)
                        groupColList.push(colDate);
                    break;
                case (MatrixDataType.DateAndTime):
                    let colDateTime = this.matrixGridHandler.gridAg.addColumnDateTime(colDef.field, colDef.title, null, true, null, { alignRight: alignRight, enableRowGrouping: true });
                    if (groupBy)
                        groupColList.push(colDateTime);
                    break;
                case (MatrixDataType.Decimal):
                    let colDec = this.matrixGridHandler.gridAg.addColumnNumber(colDef.field, colDef.title, null, { alignLeft: alignLeft, decimals: colDef.hasOptions ? colDef.options.decimals : 2, clearZero: clearZero, aggFuncOnGrouping: aggFunction, enableRowGrouping: true });
                    if (groupBy)
                        groupColList.push(colDec);
                    break;
                case (MatrixDataType.Integer):
                    let colInt = this.matrixGridHandler.gridAg.addColumnNumber(colDef.field, colDef.title, null, { alignLeft: alignLeft, decimals: 0, clearZero: clearZero, aggFuncOnGrouping: aggFunction, enableRowGrouping: true });
                    if (groupBy)
                        groupColList.push(colInt);
                    break;
                case (MatrixDataType.String):
                    let colStr = this.matrixGridHandler.gridAg.addColumnText(colDef.field, colDef.title, null, true, { alignRight: alignRight, enableRowGrouping: true });
                    if (groupBy)
                        groupColList.push(colStr);
                    break;
                case (MatrixDataType.Time):
                    let colTime;
                    if (useTimeColumn)
                        colTime = this.matrixGridHandler.gridAg.addColumnTime(colDef.field, colDef.title, null, { alignLeft: alignLeft, minutesToTimeSpan: ((colDef.hasOptions && colDef.options.minutesToTimeSpan) || !colDef.hasOptions), minutesToDecimal: colDef.hasOptions && colDef.options.minutesToDecimal, clearZero: clearZero, aggFuncOnGrouping: aggFunction, showGroupedAsNumber: groupByOption === TermGroup_MatrixGroupAggOption.Count, enableRowGrouping: true });
                    else
                        colTime = this.matrixGridHandler.gridAg.addColumnNumber(colDef.field, colDef.title, null, { alignLeft: alignLeft, decimals: 0, clearZero: clearZero, aggFuncOnGrouping: aggFunction, enableRowGrouping: true });
                    if (groupBy)
                        groupColList.push(colTime);
                    break;
            }
        });

        if (groupColList.length > 0) {
            _.forEach(groupColList, col => {
                this.matrixGridHandler.gridAg.options.groupRowsByColumn(col, true);
            });
        }

        return this.$timeout(() => {
            if (!this.matrixGridInitialized) {
                this.matrixGridHandler.gridAg.options.ignoreResizeToFit = true;
                this.matrixGridHandler.gridAg.options.enableGridMenu = true;

                this.matrixGridHandler.gridAg.options.useGrouping(true, true, { keepColumnsAfterGroup: false, selectChildren: false });
                this.matrixGridHandler.gridAg.options.groupHideOpenParents = false;

                let events: GridEvent[] = [];
                events.push(new GridEvent(SoeGridOptionsEvent.ColumnRowGroupChanged, (params) => { this.columnRowGroupChanged(params); }));
                this.matrixGridHandler.gridAg.options.subscribe(events);

                this.matrixGridHandler.gridAg.finalizeInitGrid("core.export", true, this.totalsGridId);
                this.matrixGridInitialized = true;
            } else {
                // resetColumnDefs will also remove the grid menu, need to add it again here
                this.matrixGridHandler.gridAg.options.finalizeInitGrid(false);
            }
        });
    }

    private columnRowGroupChanged(params) {
        let groupedFields: string[] = [];
        _.forEach(params.columns, groupedCol => {
            groupedFields.push(groupedCol.colDef.field);
        });
        _.forEach(this.matrixGridHandler.gridAg.options.getColumnDefs(), colDef => {
            if (!_.includes(groupedFields, colDef.field)) {
                colDef.showRowGroup = false;
                colDef.rowGroup = false;
                colDef.hide = false;
                colDef.cellRenderer = (params: { valueFormatted: any, eGridCell: HTMLElement, data: any }) => {
                    return params.valueFormatted;
                }
            }
        });
    }

    matrixDefinition(matrixDefinition: any) {
        throw new Error("Method not implemented.");
    }
    toggleShowMatrixGrid() {
        throw new Error("Method not implemented.");
    }
}