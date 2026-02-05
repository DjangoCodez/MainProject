import { NumberUtility } from "./NumberUtility";
import { NumberCellEditor, INumberCellEditorParams } from "./ag-grid/editor/NumberCellEditor";
import { DateCellEditor, IDateCellEditorParams } from "./ag-grid/editor/DateCellEditor";
import { InputCellEditor } from "./ag-grid/editor/InputCellEditor";
import { CheckboxCellRenderer, ICheckboxCellRendererParams } from "./ag-grid/renderer/CheckboxCellRenderer";
import { IIconCellRendererParams, IconCellRenderer } from "./ag-grid/renderer/IconCellRenderer";
import { ObjectFieldHelper } from "./ObjectFieldHelper";
import { ElementHelper } from "./ag-grid/ElementHelper";
import { IShapeCellRendererParams, ShapeCellRenderer } from "./ag-grid/renderer/ShapeCellRenderer";
import { IExtendedTextCellRendererParams, ExtendedTextCellRenderer } from "./ag-grid/renderer/ExtendedTextCellRenderer";
import { ITypeAheadCellEditorParams, TypeAheadCellEditor } from "./ag-grid/editor/TypeAheadCellEditor";
import { GridEvent } from "./SoeGridOptions";
import { CheckboxFloatingFilter } from "./ag-grid/filter/CheckboxFloatingFilter";
import { SoeGridOptionsEvent } from "./Enumerations";
import { CalendarUtility } from "./CalendarUtility";
import { ColumnAggregationFooterGrid } from "./ag-grid/ColumnAggregationFooterGrid";
import { IAlignedGrid } from "./ag-grid/IAlignedGrid";
import { IDisplayTexts, TotalsGrid } from "./ag-grid/TotalsGrid";
import { Constants } from "../Util/Constants";
import { IMenuItem } from "./ag-grid/GridMenuBuilder";
import { CoreUtility } from "./CoreUtility";
import { RowDetailCellRenderer } from "./ag-grid/renderer/RowDetailCellRenderer";
import { RowNode, IAggFuncParams } from "@ag-grid-community/core/dist/cjs/main";
import { IconFilterCellRenderer } from "./ag-grid/renderer/IconFilterCellRenderer";
import PrintPdfDoc from "./ag-grid/pdfExport/printPdf";
import { StringUtility } from "./StringUtility";

declare var agGrid;

export enum GroupDisplayType {
    None = 0,
    MultipleColumns = 1,
    GroupRows = 2,
    Custom = 3,
}

export interface ISoeGridOptionsFactoryAg {
    create(name: string): ISoeGridOptionsAg;
}

export interface ISoeCellValueChanged {
    data: any;
    field: string;
    newValue: any;
    oldValue: any;
    soeData: any;
}

export interface IClassRules {
    [className: string]: (data: any) => boolean;
}

export interface IColumnAggregations {
    [field: string]: string | IColumnAggregate;
}

export interface IColumnAggregate {
    getSeed(): any;
    accumulator: (accumulate: any, next: any) => any;
    cellRenderer?: (params: { data: any, colDef: any, formatValue: (any) => string }) => string;
    cellClassRule?: IClassRules;
}

export interface IIconButtonConfiguration {
    iconClass: string;
    callback: (data: any) => void;
    show: FieldOrPredicate;
}

export interface ISingelValueConfiguration {
    predicate: (data: any) => boolean;
    field: string;
    editable?: boolean;
    cellClass?: string;
    cellRenderer?: (data: any, value: any) => string,
    spanTo?: Field;
}

export interface IDynamicSelectOptions {
    options: FieldOrEvaluator<any>;
    idField?: Field;
    displayField?: Field;
}

export class ColumnOptions {
    toolTip?: string;
    toolTipField?: string
    onChanged?: CellChangedCallback;
    editable?: boolean | { (data: any): boolean } | { (data: any, field: any): boolean };
    strikeThrough?: boolean | { (data: any): boolean } | { (data: any, field: any): boolean };
    cellStyle?: { (data: any, field: any): any };
    alignLeft?: boolean;
    alignRight?: boolean;
    pinned?: "left" | "right";
    enableHiding?: boolean;
    enableResizing?: boolean;
    isSubgrid?: boolean;
    cellClassRules?: IClassRules;
    formatter?: FieldOrPredicate;
    getter?: FieldOrPredicate;
    setter?: FieldOrPredicate;
    enableColumnMenu?: boolean;
    suppressSorting?: boolean;
    clearZero?: boolean;
    useSetFilter?: boolean;
    filterOptions?: any[];
    filterLabel?: string;
    shapeValueField?: string;
    shape?: string;
    useGradient?: boolean;
    colorField?: string;
    gradientField?: string;
    hide?: boolean;
    enableRowGrouping?: boolean;
    showRowGroup?: string | boolean;
    sort?: string;
    suppressFilter?: boolean;
    suppressFilterUpdate?: boolean;
    suppressMovable?: boolean;
    minWidth?: number;
    maxWidth?: number;
    suppressSizeToFit?: boolean;
    suppressExport?: boolean;
    addNotEditableCSS?: boolean;

    // Custom tooltip
    tooltipComponent?: string;
    tooltipComponentParams?: { (params: any): any };

    public static default(): ColumnOptions {
        return {} as ColumnOptions;
    }
}

export class TypeAheadColumnOptions extends ColumnOptions {
    typeAheadOptions: TypeAheadOptionsAg;
    error?: FieldOrEvaluator<string>;
    secondRow?: FieldOrEvaluator<string>;
    hideSecondRowSeparator?: boolean;
    displayField?: string;
    ignoreColumnOnGrouping?: boolean;
}

export class NumberColumnOptions extends ColumnOptions {
    decimals?: number;
    disabled?: FieldOrPredicate;
    formatAsText?: boolean;
    aggFuncOnGrouping?: string;
    allowAggFuncMenu?: boolean;
    ignoreFormatDecimals?: boolean;
    maxDecimals?: number;
}

export class TimeColumnOptions extends ColumnOptions {
    cellFilter?: string;
    handleAsTimeSpan?: boolean;
    minDigits?: number;
    minutesToTimeSpan?: boolean;
    minutesToDecimal?: boolean;
    aggFuncOnGrouping?: any;
    secondaryField?: string;
    showSeconds?: boolean;
    hideDays?: boolean;
    showGroupedAsNumber?: boolean;  // For example when using agg function count
    formatTimeWithSeconds?: boolean;
    formatTimeWithDays?: boolean;
}

export class DateColumnOptions extends ColumnOptions {
    disabled?: FieldOrPredicate;
    dateFormat?: string;
    minDate?: Date;
    maxDate?: Date;
}

export class DateTimeColumnOptions extends DateColumnOptions {
    showSeconds?: boolean;
}

export class IconColumnOptions extends ColumnOptions {
    icon?: string;
    onClick?: DataCallback;
    getNodeOnClick?: boolean;
    showIcon?: FieldOrPredicate;
    showTooltipFieldInFilter?: boolean;
    noPointer?: boolean;
}

export class TextColumnOptions extends ColumnOptions {
    buttonConfiguration?: IIconButtonConfiguration;
    usePlainText?: boolean;
}

export class BoolColumnOptions extends ColumnOptions {
    enableEdit?: boolean;
    setChecked?: boolean;
    disabledField?: string;
    termIndeterminate?: boolean;
}

export class SelectColumnOptions extends ColumnOptions {
    selectOptions: any[];
    dynamicSelectOptions?: IDynamicSelectOptions;
    collectionField?: string;
    displayField?: string;
    dropdownIdLabel?: string;
    dropdownValueLabel?: string;
    populateFilterFromGrid?: boolean;
    ignoreTextInFilter?: boolean;

    public static default(): SelectColumnOptions {
        return _.assign(ColumnOptions.default(), { selectOptions: [], dropdownValueLabel: "value", dropdownIdLabel: "id" } as SelectColumnOptions) as SelectColumnOptions;
    }
}

export class ShapeColumnOptions extends ColumnOptions {
    shapeField?: string;
    shapeWidth?: number;
    color?: string;
    attestGradient?: boolean;
    gradientField?: string;
    showIconField?: string;
    ctrlName?: string;
    isSubgrid?: boolean;
    showEmptyIcon?: (data) => string;
}

export class GroupingOptions {
    keepColumnsAfterGroup?: boolean;
    selectChildren?: boolean;
    groupSelectsFiltered?: boolean;
    keepGroupState?: boolean;
    totalTerm?: string;
    hideGroupPanel?: boolean;
    suppressCount?: boolean;
    minAutoGroupColumnWidth?: number
}

export declare type Field = string;
export declare type FieldOrEvaluator<T> = string | { (data: any): T }
export declare type Predicate = (data: any) => boolean;
export declare type TypedPredicate<T> = (data: any) => boolean;
export declare type FieldOrPredicate = string | Predicate;
export declare type ValueOrPredicate = boolean | Predicate;
export declare type IsTrueOrEvaluateField = boolean | string;

export declare type CellChangedCallback = (params: ISoeCellValueChanged) => void;
export declare type DataCallback = (data: any) => void;

export declare type MenuItem = IMenuItem | string;

export interface ISoeGridOptionsAg {
    alwaysShowHorizontalScroll: boolean;

    exportFilename: string;
    enableFiltering: boolean;
    disableHorizontalScrollbar: boolean;
    enableGridMenu: boolean;
    enableRowSelection: boolean;
    ignoreResizeToFit: boolean;
    immutableData: boolean;
    useGetRowNodeId(callback: any);
    useExternalFiltering: boolean;
    ignoreFiltering: boolean;
    ignoreResetFilterModel: boolean;
    showAlignedFooterGrid: boolean;
    groupDisplayType: GroupDisplayType;
    groupHideOpenParents: boolean;
    groupSelectsChildren: boolean;
    noRowGroupIndent: boolean;
    noDetailPadding: boolean;
    setStyle: string;
    autoHeight: boolean;
    isGridReady: boolean;
    keepApi: boolean;

    enableMasterDetail(detailsGridOptions: any, detailHeight?: number, detailHeightByChildCollectionName?: string, autoHeight?: boolean);
    enableMasterDetailWithDirective(directiveName: string);
    setDetailCellDataCallback(callback: any);
    detailGridOptions: any;
    setDetailsGridData(items: any[]);

    enableContextMenu: boolean;
    enableCharts: boolean;

    nbrOfColumns(): number;
    isColumnVisible(field: string): boolean;
    hideColumn(name: string);
    showColumn(name: string);
    setChildGridColumnVisibility(gridName: string, columnName: string, visibility: boolean);
    getColumnDefs(includeColumnHeader?: boolean, includeAll?: boolean): any[];
    getAllDisplayedColumns(): any[];
    clearColumnDefs(); //This actually clears the data, adding new
    resetColumnDefs(clearRows?: boolean);
    resetRowsForChildGrid(name: string);
    refreshHeaders();
    refreshHeadersForChildGrid(name: string);
    enableSingleSelection();
    useGrouping(includeFooter?: boolean, includeTotalFooter?: boolean, options?: GroupingOptions);
    setRowHeight(value: number);

    addColumns(columnDefs: any[]);
    addColumn(columnDef: any);
    addColumnHeader(field: string, headerName: string, options?: TextColumnOptions): any;
    addColumnBool(field: string, headerName: string, width: number, options?: BoolColumnOptions, headerColumnDef?: any): any;
    addColumnActive(field: string, headerName?: string, width?: number, onChanged?: CellChangedCallback): any;
    addColumnIcon(field: string, headerName?: string, width?: number, options?: IconColumnOptions, headerColumnDef?: any): any;
    addColumnShape(field: string, headerName?: string, width?: number, options?: ShapeColumnOptions, headerColumnDef?: any): any;
    addColumnEdit(toolTip: string, onClickEvent?: DataCallback, isSubgrid?: boolean, showIcon?: FieldOrPredicate): any;
    addColumnDelete(toolTip: string, onClick?: DataCallback, isSubgrid?: boolean, showIcon?: FieldOrPredicate, icon?: string, headerColumnDef?: any): any;
    addColumnPdf(toolTip: string, onClickEvent?: DataCallback): any;
    addColumnIsModified(field?: string, headerName?: string, width?: number, clickCallback?: (params: any) => void, headerColumnDef?: any): any;

    addColumnHtml(field: string, headerName: string, width: number, enableHiding?: boolean): any;
    addColumnText(field: string, headerName: string, width: number, options?: TextColumnOptions, headerColumnDef?: any): any;
    addColumnNumber(field: string, headerName: string, width: number, options?: NumberColumnOptions, headerColumnDef?: any): any;
    addColumnSelect(field: string, headerName: string, width: number, options?: SelectColumnOptions, headerColumnDef?: any): any;
    addColumnDateTime(field: string, headerName: string, width: number, enableHiding?: boolean, headerColumnDef?: any, cellFilter?: string, options?: DateTimeColumnOptions): any;
    addColumnDate(field: string, headerName: string, width: number, enableHiding?: boolean, headerColumnDef?: any, cellFilter?: string, options?: DateColumnOptions): any;
    addColumnTime(field: string, headerName: string, width: number, options?: TimeColumnOptions, headerColumnDef?: any): any;
    addColumnTimeSpan(field: string, headerName: string, width: number, options?: TimeColumnOptions, headerColumnDef?: any): any;
    addColumnTypeAhead(field: string, headerName?: string, width?: number, options?: TypeAheadColumnOptions, soeData?: any, headerColumnDef?: any): any;

    addGridMenuItem(menuItem: IMenuItem | string, colId?: string);

    setMinRowsToShow(nbrOfRows: number);
    setAutoHeight(setToAuto: boolean);
    updateGridHeightBasedOnActualRows(defaultHeight?: number);
    addFooterRow(elementQuerySelector: string, aggregations: IColumnAggregations, include?: (data) => boolean, readOnlyGrid?: boolean);
    addAggregatedFooterRow(elementQuerySelector: string, aggregations: IColumnAggregations, include?: (data) => boolean, readOnlyGrid?: boolean);
    addTotalRow(elementQuerySelector: string, texts: IDisplayTexts);
    removeTotalRow(elementQuerySelector: string);

    refreshRows(...datas: any[]);
    refreshRowsIgnoreFocus(...datas: any[]);
    refreshColumns();
    refreshColumnsForChildGrid(name: string);
    resetColumns();
    refreshCells(force?: boolean);
    refreshGrid();
    refreshChildGrid(name: string);
    autosizeColumns();
    finalizeInitGrid(ignoreAddMenu?: boolean, setIsActiveDefaultFilter?: boolean);
    clearData();
    getData();
    setData(rows: any[]);
    setDataForChildGrid(name: string, rows: any[]);
    addRow(row: any, setFocus?: boolean, columnToFocus?: Field | any, insertBeforeRow?: any): number;
    deleteRow(row: any);

    selectAllRows();
    selectRow(row: any, forceGridUpdate?: boolean);
    selectRows(rows: any[]);
    selectRowByVisibleIndex(index: number, forceGridUpdate?: boolean);
    unSelectRow(row: any, forceGridUpdate?: boolean);
    clearSelectedRows();

    getCurrentRow(): any;
    getRowIndexFor(data: any): number;
    getCurrentRowCol(): { row: any, column: any };
    startEditingCell(row: number | any, column: Field | any);
    startEditingColumn(column: Field | any);
    getRowIndex(row: any | number): number;
    stopEditing(cancel: boolean);
    refocusCell();
    clearFocusedCell();
    setFocusedCell(rowIndex: number, column: any);
    setFilterFocus();
    getColumnIndex(field: string);
    getSelectedRows(): any[];
    getSelectedNodes(): any[];
    getSelectedCount(): number;
    getSelectedIds(idField: string): number[];
    getFilteredRows(): any[];
    getRowId: (row) => any;
    setTooltipDelay: number;
    setFilter(field: string, filterModel: any);
    clearFilters();
    getFilterModels: () => any;
    getFilterValueModel(field: string): any;
    exportRows(format: string, allRows?: boolean);

    saveDefaultState(callback: (name: string, data: string) => ng.IPromise<any>): ng.IPromise<any>;
    restoreDefaultState(callback: (name: string) => ng.IPromise<string>);
    deleteDefaultState(callback: (name: string) => ng.IPromise<any>);
    saveState(callback: (name: string, data: string) => ng.IPromise<any>): ng.IPromise<any>;
    restoreState(callback: (name: string) => ng.IPromise<string>, saveCurrentAsDefault: boolean);
    deleteState(callback: (name: string) => ng.IPromise<any>, callback2: (name: string) => ng.IPromise<any>);

    subscribe(events: GridEvent[]);
    setStandardSubscriptions(rowSelectionCallback: (rows: any[]) => void);

    customTabToCellHandler: (params: any) => { rowIndex: number, column: any };
    customCreateDefaultColumnMenu: (defaultItems?: string[]) => MenuItem[];

    getLastEditableColumn(): any;
    isCellEditable(row: any, colDef: any): boolean;
    getNextVisibleColumn(column: any): any;
    getPreviousVisibleColumn(column: any): any;
    getVisibleRowByIndex(rowIndex: number): any;
    getNextRow(rowData: any, usingDetail?: boolean): { rowIndex: number, rowNode: any };
    getPreviousRow(rowData: any, usingDetail?: boolean): { rowIndex: number, rowNode: any };
    tabToNextCell();
    getColumnDefByField(field: string): any;
    getColumnByField(field: string): any;
    getSingelValueColumn(): any;
    sizeColumnToFit(): void;
    setSingelValueConfiguration(singleValueConfigurations: ISingelValueConfiguration[], setVisible?: boolean): void;
    translateText: (key: string, defaultValue: string) => string;

    groupRowsByColumn(column: Field | any, showRowGroup: string | boolean, groupDefaultExpanded?: number): void;
    ungroupColumn(column: Field | any): void;
    groupRowsByColumnAndHide(column: any, cellRenderer: string, index: number, suppressCount: boolean, useEmptyCell?: boolean, addCheckBox?: boolean, resizable?: boolean): void;
    expandMasterDetail(row: any, expand: boolean);
    addGroupAggFunction(name: string, aggFunc: (values) => void);
    addGroupCountAggFunction();
    addGroupAverageAggFunction();
    addGroupMedianAggFunction();
    addGroupTimeSpanSumAggFunction(emptyWhenZero: boolean, noSum?: boolean);
    addGroupTimeSpanMinAggFunction();
    addGroupTimeSpanMaxAggFunction();
    addGroupTimeSpanAverageAggFunction();
    addGroupTimeSpanMedianAggFunction();
    setAllGroupExpended(expanded: boolean, level?: number);

    setName(name: string);
    getName(): string;
    getNormalizedName(): string;

    //------- These helper functions operate on the data not the grid, I don't think they belong here.
    findInData<T>(predicate: TypedPredicate<T>): T;
    findAllInData<T>(predicate: TypedPredicate<T>): T[];
    sortFirst(sortprop?: string);
    sortUp(sortprop?: string);
    sortDown(sortprop?: string);
    sortLast(sortprop?: string);
    reNumberRows(sortprop?: string);
    //------- 

}

export class GridOptionsFactoryAg implements ISoeGridOptionsFactoryAg {

    //@ngInject
    constructor(private $timeout: ng.ITimeoutService, private uiGridConstants: uiGrid.IUiGridConstants) { }

    create(name: string) {
        return new SoeGridOptionsAg(name, this.$timeout);
    }
}

export class SoeGridOptionsAg implements ISoeGridOptionsAg {

    // Grid api
    private gridApi: any;

    // Column api
    private columnApi: any;

    constructor(private name: string, private $timeout: ng.ITimeoutService) {

    }

    public static create(name: string, $timeout: ng.ITimeoutService, enableExpansion?: boolean, expansionTemplate?: string) {
        return new SoeGridOptionsAg(name, $timeout);
    }

    public gridOptions: any = {
        columnDefs: [
            {
                field: 'soe-row-selection',
                editable: false,
                headerName: '',
                checkboxSelection: true,
                headerCheckboxSelection: true,
                headerCheckboxSelectionFilteredOnly: true,
                pinned: 'left',
                lockPosition: true,
                width: 22,
                suppressMenu: true,
                sortable: false,
                suppressSizeToFit: true,
                suppressMovable: true,
                //suppressFilter: true,
                filter: false,
                resizable: false,
                suppressNavigable: true,
                suppressColumnsToolPanel: true,
                suppressExport: true,
                cellStyle: { padding: "0" }
            },
            {
                field: 'soe-ag-single-value-column',
                headerClass: "soe-ag-single-value-column",
                cellClass: "soe-ag-single-value-column",
                headerName: '',
                hide: true,
                width: 10,
                suppressMenu: true,
                sortable: false,
                suppressSizeToFit: true,
                suppressMovable: true,
                filter: false,
                resizable: false,
                suppressNavigable: true,
                suppressColumnsToolPanel: true,
                suppressExport: true,
                editable: ({ node }) => {
                    const { data } = node;
                    const config = _.find(this.singleValueConfigurations, c => c.predicate(data));
                    return config && (config.editable !== undefined ? config.editable : true);
                },
                colSpan: ({ data, column, columnApi }) => {
                    const config = _.find(this.singleValueConfigurations, c => c.predicate(data));
                    if (!config) {
                        return 0;
                    }

                    let columnsToSpan = 1;
                    let myPinned = column.pinned;

                    //if (config.spanTo) {
                    while (!!column) {
                        column = columnApi.getDisplayedColAfter(column);
                        if (column == null) {
                            break;
                            //return 0;
                        }

                        if (column.pinned !== myPinned) {
                            continue;
                        }

                        columnsToSpan++;

                        if ((config.spanTo != null) && (column.colId === config.spanTo)) {
                            break;
                        }
                    }
                    //}

                    return columnsToSpan;
                },
                valueGetter: ({ data }) => {
                    const config = _.find(this.singleValueConfigurations, c => c.predicate(data));
                    return config ? data[config.field] : undefined;
                },
                valueSetter: ({ data, newValue, oldValue }) => {
                    const config = _.find(this.singleValueConfigurations, c => c.predicate(data));
                    if (!config || oldValue === newValue) {
                        return false;
                    }

                    data[config.field] = newValue;
                    return true;
                },
                cellRenderer: ({ data, value }) => {
                    const config = _.find(this.singleValueConfigurations, c => c.predicate(data));
                    if (!config || !config.cellRenderer) {
                        return "<span>" + (value || "") + "</span>";
                    }

                    return config.cellRenderer(data, value);
                }
            }
        ],
        defaultColDef: {
            cellEditor: InputCellEditor,
            floatingFilter: true
        },
        rowData: [],
        debug: false,
        suppressPropertyNamesCheck: true,
        masterDetail: true,
        enableRangeSelection: true,
        chartThemes: ['ag-pastel'],
        singleClickEdit: true,
        rowSelection: 'multiple',
        suppressRowClickSelection: true,
        suppressScrollOnNewData: true,
        suppressMenuHide: true,
        suppressContextMenu: true,
        suppressDragLeaveHidesColumns: true,
        suppressNoRowsOverlay: true,
        groupDisplayType: "custom",
        suppressAggFuncInHeader: true,
        immutableData: false,
        //animateRows: true,
        cacheQuickFilter: true,
        alignedGrids: [],
        sideBar: false,
        tooltipShowDelay: 200,
        tooltipHideDelay: 2000,
        excelStyles: [
            {
                id: "excelDate",
                dataType: "dateTime",
                numberFormat: { format: "yyyy-MM-dd" },
            },
            {
                id: "excelTime",
                dataType: "dateTime",
                numberFormat: { format: "[hh]:mm;@" },
            }
        ],
        popupParent: document.querySelector('body'),
        // Events
        onGridReady: (params) => {
            const { api, columnApi } = params;
            if (this.keepApi)
                this.gridApi = api;
            this.columnApi = columnApi;
            this.updateGridHeightBasedOnNbrOfRows(this.minRowsToShow, api);
            this.alignedGrids.forEach(g => g.processMainGridInitialize(this.gridOptions));
            if (this.ignoreResizeToFit) {
                this.autosizeColumns();
            }
            else {
                if (api)
                    api.sizeColumnsToFit();
            }
            this.isGridReady = true;
        },
        onGridSizeChanged: (params) => {
            const { api } = params;
            if (!this.ignoreResizeToFit)
                api.sizeColumnsToFit();
        },
        getMainMenuItems: (params) => this.gridMenuItems[params.column.colId] || this.customCreateDefaultColumnMenu(params.defaultItems),

        postProcessPopup: function (params) {
            // check callback is for menu
            if (params.type !== 'columnMenu') {
                return;
            }

            var ePopup = params.ePopup;
            ePopup.style.zIndex = '1050';
        },

        onCellEditingStarted: (params) => {
            const { data, colDef, node } = params;

            this.setCellCancelledToken(node);
            this.beginCellEdit(data, colDef);
        },
        onCellValueChanged: (params) => {
            const { data, colDef, oldValue, newValue, node } = params;
            if (colDef.onCellValueChanged) {
                colDef.onCellValueChanged({ data, newValue, oldValue, colDef });
            }
            this.deleteCellCancelledToken(node);
            this.afterCellEdit(data, colDef, newValue, oldValue);
        },
        onCellEditingStopped: (params) => {
            const { data, colDef, node } = params;
            const hasCancelledToken = this.hasCellCancelledToken(node);

            if (hasCancelledToken) {
                this.cancelCellEdit(data, colDef);
            }

            this.deleteCellCancelledToken(node);
        },
        onCellFocused: (params) => {
            const { rowIndex, column, rowPinned, forceBrowserFocus } = params;

            this.cellFocused(rowIndex, column, rowPinned, forceBrowserFocus);
        },
        onModelUpdated: (params) => {
            const { api } = this.gridOptions;
            if (!api) {
                return;
            }

            const model = api.getModel();
            this.rowsVisibleChanged(_.map(_.filter(model.rowsToDisplay, (r) => !r.detail), (node: any) => node.data));
        },
        onRowDataChanged: () => {
            const { api } = this.gridOptions;
            if (api)
                api.forEachNode(this.propegateNodeIdToData);
        },
        onFilterChanged: () => {
            this.filterChanged();
        },
        rowSelected: (node) => {
            const { api } = this.gridOptions;
            this.rowSelectionChangedBatch(api.getSelectedRows());
        },
        onSelectionChanged: (row) => {
            const { api } = this.gridOptions;
            this.rowSelectionChangedBatch(api.getSelectedRows());
        },
        onRowDoubleClicked: (params) => {
            this.rowDoubleClicked(params.data);
        },
        onRowClicked: (params) => {
            this.rowClicked(params.data);
        },
        onColumnVisible: (params) => {
            this.columnVisible(params.column)
        },
        isExternalFilterPresent: () => {
            return this.useExternalFiltering;
        },
        doesExternalFilterPass: (node) => {
            //Not completly implemented
            return this.ignoreFiltering;
        },
        tabToNextCell: (params) => this.runGotoNextCell(params),

        localeTextFunc: (key, defaultValue) => this.translateText(key, defaultValue),

        isRowSelectable: (rowNode) => {
            return this.isRowSelectable(rowNode);
        },

        isRowMaster: (rowNode) => {
            return this.isRowMaster(rowNode);
        },
    };

    private singleValueConfigurations: ISingelValueConfiguration[];
    private defaultGridState: any;
    private alignedGrids: IAlignedGrid[] = [];
    private minRowsToShow: number;
    private gridMenuItems: {} = {};
    private openRowGroups: any;

    public exportFilename: string;
    public enableGridMenu = true;
    public ignoreResizeToFit: boolean;

    public useExternalFiltering: boolean;
    public ignoreFiltering: boolean;
    public ignoreResetFilterModel: boolean;
    public showAlignedFooterGrid = true;
    public isGridReady = false;
    public keepApi = false;

    public rowSelectionDisabled = false;

    public customTabToCellHandler: (params: any) => { rowIndex: number, column: any };

    public set getRowId(value: (row) => any) {
        this.gridOptions.getRowNodeId = value;
        this.gridOptions.immutableData = !!value;
    }

    public translateText: (key: string, defaultValue: string) => string = (key: string, defaultValue: string) => defaultValue;

    set setStyle(style: string) {
        if (this.gridApi)
            this.gridApi.style = style;
    }

    set autoHeight(set: boolean) {
        if (this.gridApi) {
            if (set)
                this.gridApi.setDomLayout("autoHeight");
            else
                this.gridApi.setDomLayout("normal");
        }
    }
    set setTooltipDelay(value: number) {
        this.gridOptions.tooltipShowDelay = value;
    }
    set enableFiltering(value: boolean) {
        this.gridOptions.defaultColDef.floatingFilter = value;
    }
    set alwaysShowHorizontalScroll(value: boolean) {
        this.gridOptions.alwaysShowHorizontalScroll = value;
    }
    set disableHorizontalScrollbar(value: boolean) {
        this.gridOptions.suppressHorizontalScroll = value;
    }

    set enableRowSelection(value: boolean) {
        this.rowSelectionDisabled = !value;
        this.$timeout(() => {
            if (this.rowSelectionDisabled)
                this.hideColumn('soe-row-selection');
            else
                this.showColumn('soe-row-selection');
        }, 500);    // Long timeout, I know, but we have problems with the column being hidden again after setting enableRowSelection = true
    }

    set groupDisplayType(type: GroupDisplayType) {

        switch (type) {
            case GroupDisplayType.Custom:
                {
                    this.gridOptions.groupDisplayType = "custom";
                    break;
                }
            case GroupDisplayType.GroupRows:
                {
                    this.gridOptions.groupDisplayType = "groupRows";
                    break;
                }
            case GroupDisplayType.MultipleColumns:
                {
                    this.gridOptions.groupDisplayType = "multipleColumns";
                    break;
                }
            case GroupDisplayType.None:
                {
                    this.gridOptions.groupDisplayType = "";
                    break;
                }
        }
    }

    set groupHideOpenParents(value: boolean) {
        this.gridOptions.groupHideOpenParents = value;
    }

    set groupSelectsChildren(value: boolean) {
        this.gridOptions.groupSelectsChildren = value;
    }

    set noRowGroupIndent(value: boolean) {
        this.gridOptions.noRowGroupIndent = value;
    }

    set noDetailPadding(value: boolean) {
        this.gridOptions.noDetailPadding = value;
    }

    public enableSingleSelection() {
        this.rowSelectionDisabled = false;
        this.gridOptions.rowSelection = 'single';
        this.gridOptions.suppressRowClickSelection = false;
        this.hideColumn("soe-row-selection")
        /*this.$timeout(() => {
            const { columnApi } = this.gridOptions;
            columnApi.setColumnVisible("soe-row-selection", false);
        });*/
    }

    public setRowHeight(value: number) {
        this.$timeout(() => {
            this.gridOptions.rowHeight = value;
        });
    }

    public useGrouping(includeFooter = true, includeTotalFooter = true, options: GroupingOptions = {}) {
        this.gridOptions.rowGroupPanelShow = options.hideGroupPanel ? 'never' : 'always';
        this.groupDisplayType = GroupDisplayType.None;
        this.gridOptions.suppressDragLeaveHidesColumns = options.keepColumnsAfterGroup;
        this.gridOptions.suppressMakeColumnVisibleAfterUnGroup = false;
        this.gridOptions.groupIncludeFooter = includeFooter;
        this.gridOptions.groupIncludeTotalFooter = includeTotalFooter;
        this.gridOptions.groupSelectsChildren = options.selectChildren;
        const totalTerm = options.totalTerm ? `${options.totalTerm}: ` : "Total: ";

        if (options.selectChildren) {
            this.gridOptions.groupSelectsFiltered = options.groupSelectsFiltered;
        }

        this.gridOptions.keepGroupState = options.keepGroupState;

        this.gridOptions.getRowClass = function (params) {
            if (params.node.group && params.node.id.startsWith("rowGroupFooter")) {
                return "ag-group-row-total";
            }
        }
        this.gridOptions.autoGroupColumnDef = {
            minWidth: options.minAutoGroupColumnWidth || undefined,
            resizable: true,
            sortable: true,
            suppressMenu: true,
            comparator: (valueA, valueB, nodeA, nodeB, isInverted) => {
                return AgGridUtility.groupComparator(nodeA, nodeB, valueA, valueB);
            },
            cellRendererParams: {
                suppressCount: options.suppressCount,
                footerValueGetter: function (params) {
                    let value = params.value;
                    let node = params.node;
                    if (value && node.field.toLowerCase().contains('date') && CalendarUtility.isValidDate(value)) {
                        const jsDate = CalendarUtility.convertToDate(value, CoreUtility.languageDateFormat);
                        if (jsDate)
                            value = jsDate.toLocaleDateString(CoreUtility.language);                        
                    }
                    return totalTerm + (value ?? "");
                }
            }
        }

        this.gridOptions.defaultGroupSortComparator = function (nodeA, nodeB) {
            return AgGridUtility.groupComparator(nodeA, nodeB, nodeA.key, nodeB.key);
        };

        this.gridOptions.onRowGroupOpened = (params) => {
            if (!this.ignoreResizeToFit)
                this.sizeColumnToFit();

            if (this.gridOptions.keepGroupState) {
                if (!this.openRowGroups) {
                    this.openRowGroups = {}
                }
                this.openRowGroups[params.node.level + params.node.key] = params.node.expanded;
            }
        }

        this.$timeout(() => {
            if (this.gridOptions.api) {
                this.gridOptions.api.addEventListener("columnRowGroupChanged", (params) => {
                    this.columnRowGroupChanged(params);

                    if (!this.ignoreResizeToFit)
                        this.sizeColumnToFit();
                });
            } else if (this.gridApi) {
                this.gridApi.addEventListener("columnRowGroupChanged", (params) => {
                    this.columnRowGroupChanged(params);

                    if (!this.ignoreResizeToFit)
                        this.sizeColumnToFit();
                });
            }
        });
    }

    set immutableData(value: boolean) {
        this.gridOptions.immutableData = value;
    }

    public useGetRowNodeId(callback: any) {
        if (callback)
            this.gridOptions.getRowNodeId = callback;
    }

    // MASTER DETAIL
    public enableMasterDetail(detailsGridOptions: any, detailHeight: number = undefined, detailHeightByChildCollectionName?: string, autoHeight?: boolean) {
        this.gridOptions.masterDetail = true;
        detailsGridOptions.gridOptions.localeTextFunc = this.gridOptions.localeTextFunc;
        this.gridOptions.detailCellRendererParams = {
            detailGridOptions: detailsGridOptions.gridOptions,
            onResize: (height, rowNodeId) => {
                const rowNode = this.getRowNodeFromId(rowNodeId);
                if (rowNode) {
                    this.setDetailRowHeight(rowNode, height);
                    this.updateGridHeightBasedOnActualRows();
                }
            }
        }

        if (detailHeight)
            this.gridOptions.detailRowHeight = detailHeight;
        else if (detailHeightByChildCollectionName) {
            (<any>this.gridOptions).getRowHeight = function (params) {
                var isDetailRow = params.node.detail;
                if (isDetailRow) {
                    var scrollHeight = 17;
                    var detailPanelHeight = (params.data[detailHeightByChildCollectionName].length * 28) + scrollHeight + 38;
                    // dynamically calculate detail row height
                    return detailPanelHeight;
                } else {
                    // for all non-detail rows, return 29, the default row height
                    return 28;
                }
            }
        }
        else if (autoHeight) {
            this.gridOptions.detailRowAutoHeight = true;
        }

        const { api } = this.gridOptions;

        if (api) {
            api.addEventListener("rowGroupOpened", (event) => {
                event.api.sizeColumnsToFit();
            });
        }
        else if (this.gridApi) {
            this.gridApi.addEventListener("rowGroupOpened", (event) => {
                event.api.sizeColumnsToFit();
            });
        }
    }

    public setDetailCellDataCallback(callback: any) {
        this.gridOptions.detailCellRendererParams.getDetailRowData = callback;
    }

    get detailGridOptions(): SoeGridOptionsAg {
        return this.gridOptions.detailCellRendererParams.detailGridOptions; //this._detailGridOptions;
    }
    set detailsGridOptions(gridOption: SoeGridOptionsAg) {
        this.gridOptions.detailCellRendererParams.detailGridOptions = gridOption;
    }

    public setDetailsGridData(items: any[]) {
        if (this.gridOptions.detailCellRendererParams && this.gridOptions.detailCellRendererParams.detailGridOptions) {
            this.gridOptions.detailCellRendererParams.detailGridOptions.setData(items);
            this.gridOptions.detailCellRendererParams.detailGridOptions.refreshRows();
        }
    }

    public enableMasterDetailWithDirective(directiveName: string) {
        this.gridOptions.angularCompileRows = true;
        this.gridOptions.masterDetail = true;

        this.gridOptions.detailCellRenderer = RowDetailCellRenderer;

        this.gridOptions.detailCellRendererParams = {
            directiveName: directiveName,
            onResize: (height, rowNodeId) => {
                const rowNode = this.getRowNodeFromId(rowNodeId);
                if (rowNode) {
                    this.setDetailRowHeight(rowNode, height);
                    this.updateGridHeightBasedOnActualRows();
                }
            }
        }

        this.gridOptions.api.addEventListener("rowGroupOpened", (event) => {
            this.updateGridHeightBasedOnActualRows();
        });
    }

    private getRowNodeFromId(rowNodeId: string): RowNode {
        return (<RowNode[]>this.gridOptions.api.rowModel.rowsToDisplay).find(x => x.id === rowNodeId);
    }

    public setDetailRowHeight(rowNode: RowNode, height: number) {
        if (rowNode.rowHeight !== height) {
            rowNode.setRowHeight(height);
            this.gridOptions.api.onRowHeightChanged();
        }
    }

    set useCustomExporting(value: any) { this.gridOptions.useCustomExporting = value; }

    // CONTEXT MENU

    set enableContextMenu(value: boolean) {
        this.gridOptions.suppressContextMenu = !value;
    }

    // CHARTS

    set enableCharts(value: boolean) {
        this.gridOptions.enableCharts = value;
    }


    // COLUMNS
    public nbrOfColumns(): number {
        return this.gridOptions.columnDefs.length;
    }

    public createColumnSelection() {
        this.gridOptions.columnDefs.push({
            field: 'soe-row-selection',
            editable: false,
            headerName: '',
            hide: this.rowSelectionDisabled,
            checkboxSelection: true,
            headerCheckboxSelection: true,
            headerCheckboxSelectionFilteredOnly: true,
            pinned: 'left',
            width: 20,
            suppressMenu: true,
            sortable: false,
            suppressSizeToFit: true,
            suppressMovable: true,
            suppressColumnsToolPanel: true,
            filter: false,
            resizable: false,
            suppressNavigable: true,
            cellStyle: { padding: "0" }
        }
        );
    }

    public setSingelValueConfiguration(singleValueConfigurations: ISingelValueConfiguration[], setVisible: boolean = false): void {
        const { columnDefs } = this.gridOptions;

        const columnDef = _.find(columnDefs as any[], (c) => c.field === "soe-ag-single-value-column");
        columnDef.hide = false;

        if (setVisible)
            this.showColumn('soe-ag-single-value-column');

        columnDef.cellClassRules = {};
        //group all predicates by cellClass skipping empty ones
        const nonEmptyCellClassPredicates = _(singleValueConfigurations).filter(c => c.cellClass).groupBy(c => c.cellClass).value();
        for (const cellClass in nonEmptyCellClassPredicates) {
            //match any of the predicates for a specific css rule.
            columnDef.cellClassRules[cellClass] = ({ data }) => _.some(nonEmptyCellClassPredicates[cellClass], c => c.predicate(data));
        }

        this.singleValueConfigurations = singleValueConfigurations;
    }

    // Common - AG_grid has this notion of defaultColDef that should be used instead I guess.
    private createColumnDef(field: string, headerName: string, width: number, enableHiding: boolean = false, enableColumnResizing: boolean = true, enableFiltering: boolean = true, enableSorting: boolean = true, enableColumnMenu: boolean = false, enableCellEdit: boolean = false, enableRichFilterEditor: boolean = false, hide: boolean = false, enableRowGroup: boolean = false, sort: string = undefined, suppressFilterUpdate: boolean = undefined, suppressMovable: boolean = false, minWidth: number = undefined, maxWidth: number = undefined, suppressSizeToFit: boolean = undefined, suppressExport: boolean = false, showRowGroup: string | boolean = ''): any {
        const columnDef = {
            field: field,
            headerName: headerName,
            width: width,
            hide: hide,
            menuTabs: enableHiding ? ['generalMenuTab', 'filterMenuTab', 'columnsMenuTab'] : ["generalMenuTab", "filterMenuTab"],
            resizable: enableColumnResizing,
            filter: enableFiltering,
            suppressSyncValuesAfterDataChange: suppressFilterUpdate ? suppressFilterUpdate : false,
            sortable: enableSorting,
            suppressMenu: !enableColumnMenu,
            suppressMovable: suppressMovable,
            editable: enableCellEdit,
            enableRowGroup: enableRowGroup,
            unSortIcon: false,
            headerClass: ["soe-ag-header"],
            sort: sort,
            suppressColumnsToolPanel: !enableHiding,
            suppressExport: suppressExport,
            floatingFilterComponentParams: {
                suppressFilterButton: !enableRichFilterEditor
            },
            valueSetter: (params) => { //the default behavior doesn't really check if the values are different and fires cellValueChanged even if the value are the same. 
                const { newValue, oldValue, data } = params;
                const fld = params.colDef.field;
                const areDifferent = newValue !== oldValue;
                if (areDifferent) {
                    data[fld] = newValue;
                }

                return areDifferent;
            }
        }

        if (minWidth)
            columnDef['minWidth'] = minWidth;
        if (maxWidth)
            columnDef['maxWidth'] = maxWidth;
        if (suppressSizeToFit)
            columnDef['suppressSizeToFit'] = suppressSizeToFit;

        if (showRowGroup)
            columnDef['showRowGroup'] = showRowGroup;

        return columnDef;
    }

    // Extremely simple for now
    private createGroupColumnDef(headerName: string, showRowGroup: string, rowGroupIndex: number, cellRenderer: string, suppressCount: boolean, width: number, useEmptyCell: boolean = false, addCheckBox: boolean = false, resizable: boolean = false): any {
        const columnDef = {
            headerName: headerName,
            showRowGroup: showRowGroup,
            rowGroupIndex: rowGroupIndex,
            cellRenderer: cellRenderer,
            cellRendererParams: null,
            width: width,
            filter: false,
            suppressMenu: true,
            resizable: resizable,
            headerCheckboxSelection: addCheckBox
        }

        var cellRenderParams = {
            checkbox: addCheckBox,
            suppressCount: suppressCount ? true : false
        };
        if (useEmptyCell)
            cellRenderParams['innerRenderer'] = () => { return '<span>&nbsp;&nbsp;</span>' };
        columnDef.cellRendererParams = cellRenderParams;

        return columnDef;
    }

    public addColumn(columnDef: any) {
        this.gridOptions.columnDefs.push(columnDef);
    }

    public addChild(headerColumnDef: any, columnDef: any) {
        if (!headerColumnDef || !columnDef)
            return;
        if (!headerColumnDef.children)
            headerColumnDef.children = [];
        headerColumnDef.children.push(columnDef);
    }

    public isColumnVisible(field: string): boolean {
        let column = this.getColumnByField(field);
        return (column && column.isVisible());
    }

    public hideColumn(name: string) {
        if (this.columnApi) {
            this.columnApi.setColumnVisible(name, false);
        }
        else {
            const { columnApi } = this.gridOptions;
            if (columnApi)
                columnApi.setColumnVisible(name, false);
        }
    }

    public showColumn(name: string) {
        if (this.columnApi) {
            this.columnApi.setColumnVisible(name, true);
        }
        else {
            const { columnApi } = this.gridOptions;
            if (columnApi)
                columnApi.setColumnVisible(name, true);
        }
    }

    public setChildGridColumnVisibility(gridName: string, columnName: string, visibility: boolean) {
        var detailGridInfo = this.gridOptions.api.getDetailGridInfo(gridName);
        if (detailGridInfo) {
            detailGridInfo.columnApi.setColumnVisible(columnName, visibility);
            detailGridInfo.api.sizeColumnsToFit();
        }
    }

    public getColumnDefs(includeColumnHeader: boolean = false, includeAll: boolean = false): any[] {
        var allColumnsDefs = [];
        var columnDefs = includeAll ? this.gridOptions.columnDefs : this.gridOptions.columnDefs.filter((c) => !c.field.startsWithCaseInsensitive("soe-"));
        _.forEach(columnDefs, (colDef) => {
            if (colDef.children) {
                _.forEach(colDef.children, (colDefChild) => {
                    allColumnsDefs.push(colDefChild);
                });

                if (includeColumnHeader)
                    allColumnsDefs.push(colDef);
            }
            else {
                allColumnsDefs.push(colDef);
            }
        });
        return allColumnsDefs;
    }

    public getAllDisplayedColumns(): any[] {
        const { columnApi } = this.gridOptions;
        if (!columnApi) {
            return;
        }

        return _.filter(columnApi.getAllDisplayedColumns(), (c) => c.visible === true);
    }

    public clearColumnDefs() {
        if (this.gridOptions && this.gridOptions.api)
            this.gridOptions.api.setRowData([]);
    }

    public resetColumnDefs(clearRows: boolean = true) {
        if (clearRows && this.gridOptions.api)
            this.gridOptions.api.setRowData([]);

        this.gridOptions.columnDefs = [];

        this.createColumnSelection();
    }

    public resetRowsForChildGrid(name: string) {
        var detailGridInfo = this.gridOptions.api.getDetailGridInfo(name);
        /*console.log("details info", detailGridInfo);
        this.gridOptions.api.forEachDetailGridInfo(function (detailGridApi) {
            console.log("details info foreach", detailGridApi);
        });*/
        if (detailGridInfo)
            detailGridInfo.api.setRowData([]);
    }

    public refreshHeaders() {
        this.gridOptions.api.refreshHeader();
    }

    public refreshHeadersForChildGrid(name: string) {
        var detailGridInfo = this.gridOptions.api.getDetailGridInfo(name);
        if (detailGridInfo)
            detailGridInfo.api.refreshHeader();
    }

    public getNextVisibleColumn = (column: any): any => {
        return this.gridOptions.columnApi.getDisplayedColAfter(column);
    }

    public getPreviousVisibleColumn = (column: any): any => {
        return this.gridOptions.columnApi.getDisplayedColBefore(column);
    }

    public getLastEditableColumn(): any {
        var colList = this.gridOptions.columnApi.getAllGridColumns().filter(c => c.visible && ((c.colDef.editable === "function" && c.colDef.editable()) || (c.colDef.editable !== "function" && c.colDef.editable))) as Array<any>;
        if (colList && colList.length > 0) {
            return colList[colList.length - 1];
        }
        return null;
    }

    public sizeColumnToFit(): void {
        if (this.gridApi) {
            this.gridApi.sizeColumnsToFit();
        }
        else {
            const { api } = this.gridOptions;
            if (api)
                api.sizeColumnsToFit();
        }
    }

    public autosizeColumns() {
        const { columnApi } = this.gridOptions;
        if (!columnApi) {
            return;
        }

        this.$timeout(() => {
            columnApi.autoSizeColumns(columnApi.getAllDisplayedColumns());
        }, 500);
    }

    public tabToNextCell = _.debounce(() => {
        this.gridOptions.api.tabToNextCell();
    }, 100, { leading: true, trailing: false });

    public getColumnDefByField(field: string): any {
        const { columnDefs } = this.gridOptions;
        const foundColDef = _.find(columnDefs, (colDef: any) => colDef.field === field);
        return foundColDef;
    }

    public getColumnByField(field: string): any {
        const columnDef = this.getColumnDefByField(field);
        if (this.gridOptions.columnApi && columnDef) {
            return this.gridOptions.columnApi.getColumn(columnDef.field);
        }
    }

    public getSingelValueColumn() {
        return this.getColumnByField("soe-ag-single-value-column");
    }

    public groupRowsByColumn(column: Field | any, showRowGroup: string | boolean, groupDefaultExpanded: number = 0): void {
        let columnDef = column;
        if (typeof column === "string") {
            columnDef = this.getColumnDefByField(column);
        } else if (column.colId) { //ag-grid columns have colId
            columnDef = column.colDef;
        }
        columnDef.showRowGroup = showRowGroup;
        columnDef.rowGroup = true;
        columnDef.hide = true;

        if (columnDef.cellRendererParams) {
            columnDef.cellRendererParams.suppressCount = true;
            columnDef.cellRendererParams.innerRenderer = columnDef.cellRenderer;
        }

        columnDef['parentCellRenderer'] = columnDef.cellRenderer;
        columnDef.cellRenderer = "group";

        this.gridOptions.groupDefaultExpanded = groupDefaultExpanded;
    }

    public ungroupColumn(column: Field | any) {
        let columnDef = column;
        if (typeof column === "string") {
            columnDef = this.getColumnDefByField(column);
        } else if (column.colId) { //ag-grid columns have colId
            columnDef = column.colDef;
        }
        columnDef.showRowGroup = false;
        columnDef.rowGroup = false;
        columnDef.hide = false;
        columnDef.cellRenderer = columnDef['parentCellRenderer'] ?? undefined;
    }

    public groupRowsByColumnAndHide(column: any, cellRenderer: string, index: number, suppressCount: boolean, useEmptyCell: boolean = false, addCheckBox: boolean = false, resizable: boolean = false): void {
        const groupColumn = this.createGroupColumnDef(column.headerName, column.field, index, cellRenderer, suppressCount, column.width, useEmptyCell, addCheckBox, resizable);
        if (groupColumn) {
            this.gridOptions.columnDefs.splice(index, 0, groupColumn);

            column.rowGroup = true;
            column.hide = true;

            if (!this.ignoreResizeToFit)
                this.sizeColumnToFit();
        }
    }

    public isRowExpanded(row: any): boolean {
        const nodeId = this.getRowNodeIdFromData(row);
        const node = this.gridApi ? this.gridApi.getRowNode(nodeId) : this.gridOptions.api.getRowNode(nodeId);
        return node && node.expanded;
    }

    public expandMasterDetail(row: any, expand: boolean) {
        const nodeId = this.getRowNodeIdFromData(row);
        const node = this.gridApi ? this.gridApi.getRowNode(nodeId) : this.gridOptions.api.getRowNode(nodeId);
        if (node) {
            node.setExpanded(expand);
        }
    }

    public addGroupAggFunction(name: string, aggFunc: (aggFuncParams) => void) {
        this.gridApi ? this.gridApi.addAggFunc(name, aggFunc) : this.gridOptions.api.addAggFunc(name, aggFunc);
    }

    public addGroupCountAggFunction() {
        // Built in count doesnt seem to work
        this.addGroupAggFunction("soeCount", (params) => {
            if (params && params.values)
                return params.values.length;
        });
    }

    public addGroupAverageAggFunction() {
        // Built in avg doesnt seem to work
        this.addGroupAggFunction("soeAvg", (params) => {
            if (params && params.values)
                return _.sum(params.values) / params.values.length;
        });
    }

    public addGroupMedianAggFunction() {
        this.addGroupAggFunction("median", (params) => {
            if (params && params.values)
                return NumberUtility.median(params.values);
        });
    }

    public addGroupTimeSpanSumAggFunction(emptyWhenZero: boolean, noSum = false) {
        const sumTimeSpan = (aggFuncParams: IAggFuncParams) => {
            let sumMinutes = 0;

            if (!noSum) {
                if (aggFuncParams && aggFuncParams.values && aggFuncParams.values.length) {
                    for (let value of aggFuncParams.values) {
                        if (aggFuncParams.colDef['minutesToTimeSpan'] && StringUtility.isNumeric(value))
                            sumMinutes += value;
                        else
                            sumMinutes += CalendarUtility.timeSpanToMinutes(value);
                    }
                }
            }
            return ((emptyWhenZero && sumMinutes <= 0) || noSum) ? '' : CalendarUtility.minutesToTimeSpan(sumMinutes);
        }
        this.addGroupAggFunction("sumTimeSpan", (values) => { return sumTimeSpan(values) });
    }

    public addGroupTimeSpanMinAggFunction() {
        this.addGroupAggFunction("minTimeSpan", (params) => {
            let minMinutes = undefined;
            if (params && params.values) {
                for (let value of params.values) {
                    let minutes;
                    if (StringUtility.isNumeric(value))
                        minutes = value;
                    else
                        minutes = CalendarUtility.timeSpanToMinutes(value);
                    if (minMinutes === undefined || minMinutes > minutes)
                        minMinutes = minutes;
                }
            }
            return CalendarUtility.minutesToTimeSpan(minMinutes || 0);
        });
    }

    public addGroupTimeSpanMaxAggFunction() {
        this.addGroupAggFunction("maxTimeSpan", (params) => {
            let maxMinutes = undefined;
            if (params && params.values) {
                for (let value of params.values) {
                    let minutes;
                    if (StringUtility.isNumeric(value))
                        minutes = value;
                    else
                        minutes = CalendarUtility.timeSpanToMinutes(value);
                    if (maxMinutes === undefined || maxMinutes < minutes)
                        maxMinutes = minutes;
                }
            }
            return CalendarUtility.minutesToTimeSpan(maxMinutes || 0);
        });
    }

    public addGroupTimeSpanAverageAggFunction() {
        const avgTimeSpan = (aggFuncParams: IAggFuncParams) => {
            let sumMinutes = 0;

            if (aggFuncParams && aggFuncParams.values && aggFuncParams.values.length) {
                for (let value of aggFuncParams.values) {
                    if (aggFuncParams.colDef['minutesToTimeSpan'] && StringUtility.isNumeric(value))
                        sumMinutes += value;
                    else
                        sumMinutes += CalendarUtility.timeSpanToMinutes(value);
                }
            }
            return CalendarUtility.minutesToTimeSpan(sumMinutes / aggFuncParams.values.length);
        }
        this.addGroupAggFunction("avgTimeSpan", (values) => { return avgTimeSpan(values) });
    }

    public addGroupTimeSpanMedianAggFunction() {
        const medianTimeSpan = (aggFuncParams: IAggFuncParams) => {
            let minutesValues: number[] = [];

            if (aggFuncParams && aggFuncParams.values && aggFuncParams.values.length) {
                for (let value of aggFuncParams.values) {
                    if (aggFuncParams.colDef['minutesToTimeSpan'] && StringUtility.isNumeric(value))
                        minutesValues.push(value);
                    else
                        minutesValues.push(CalendarUtility.timeSpanToMinutes(value));
                }
            }
            return CalendarUtility.minutesToTimeSpan(NumberUtility.median(minutesValues));
        }
        this.addGroupAggFunction("medianTimeSpan", (values) => { return medianTimeSpan(values) });
    }

    public setAllGroupExpended(expanded: boolean, level?: number) {
        this.gridOptions.api.forEachNode((n) => {
            if (level !== undefined) {
                if (n.level === level && n.group)
                    n.setExpanded(expanded);
            } else {
                if (n.group) {
                    n.setExpanded(expanded);
                }
            }
        });
    }

    public addColumns(columnDefs: any[]) {
        _.forEach(columnDefs, colDef => {
            this.addColumn(colDef);
        });
    }

    public addColumnHeader(field: string, headerName: string, options?: TextColumnOptions): any {
        const columnDef = this.createColumnDefText(field, headerName, 0, options);
        columnDef.children = [];
        this.addColumn(columnDef);
        return columnDef;
    }

    // Boolean (checkbox)
    private createColumnDefBool(field: string, headerName: string, width: number, enableHiding: boolean = true, enableColumnResizing: boolean = true, enableFiltering: boolean = true, enableSorting: boolean = true, enableColumnMenu: boolean = true, enableCellEdit: boolean = false, disabledField: string = "", termIndeterminate: boolean = false, useSetFilter: boolean = false, filterOptions: any[] = null, filterLabel?: string, setChecked: boolean = false, hide: boolean = false, enableRowGroup: boolean = false, sort: string = undefined, suppressFilterUpdate: boolean = undefined, suppressMovable: boolean = false, minWidth: number = undefined, maxWidth: number = undefined, suppressSizeToFit: boolean = undefined, suppressExport: boolean = false): any {
        const columnDef = this.createColumnDef(field, headerName, width, enableHiding, enableColumnResizing, enableFiltering, enableSorting, enableColumnMenu, enableCellEdit, undefined, hide, enableRowGroup, sort, suppressFilterUpdate, suppressMovable, minWidth, maxWidth, suppressSizeToFit, suppressExport);
        columnDef.editable = false;
        columnDef.enableCellEditOnFocus = false;

        //The cell renderer is also responsible for setting the new value, but will only dispatch a cellChangedEvent, not running the normal flow (eg valueParser is never executed).
        //Alt.solution: you could probably create a cellEditor that creates an checkbox input, that sets the inverse of the current value and closes (stopEditting) on startup (never really displaying the edit mode). 
        //The advantage is that you'll get the normal change value flow in ag-grid. 
        columnDef.cellRenderer = CheckboxCellRenderer;
        columnDef.cellRendererParams = {
            disabled: enableCellEdit ? disabledField : !enableCellEdit,
        } as ICheckboxCellRendererParams;

        if (enableFiltering) {
            columnDef.floatingFilterComponent = CheckboxFloatingFilter;
            columnDef.floatingFilterComponentParams = {
                canBeIndeterminate: typeof termIndeterminate === "boolean" ? termIndeterminate : true,
                setChecked: setChecked,
                suppressFilterButton: true
            };

            columnDef.filter = "agSetColumnFilter";

            columnDef.filterValueGetter = (params) => {
                const { data, column } = params;
                return data[column.colDef.field] ? data[column.colDef.field].toString() : 'false';
            };

            if (filterOptions) {
                columnDef.comparator = (valueA: string, valueB: string, nodeA: any, nodeB: any, isInverted: boolean) => {
                    if (valueA === valueB) {
                        return 0;
                    }
                    const result = filterOptions[0][filterLabel] === valueA ? 1 : -1;
                    return result * (isInverted ? -1 : 1)
                };
                columnDef.filterParams = {
                    values: filterOptions.map((o) => o[filterLabel ? filterLabel : 'value']),
                    newRowsAction: 'keep',
                    cellRenderer: (params) => {
                        let opt = _.find(filterOptions, { 'value': params.value });
                        return opt ? opt.text : '';
                    },
                    sortable: false,
                };
            }
            else {
                columnDef.comparator = (valueA: string, valueB: string, nodeA: any, nodeB: any, isInverted: boolean) => {
                    if (valueA === valueB) {
                        return 0;
                    }
                    const result = valueA === "true" ? 1 : -1;
                    return result * (isInverted ? -1 : 1)
                };

                columnDef.filterParams = {
                    values: ["true", "false"],
                    newRowsAction: 'keep'
                };
            }
        }

        columnDef.keyCreator = ({ value }) => {
            if (value == null) {
                return ''; 
            }
            return value.toString();
        };

        this.setSoeType(columnDef, Constants.GRID_COLUMN_TYPE_BOOL);

        return columnDef;
    }
    public addColumnBool(field: string, headerName: string, width: number, options?: BoolColumnOptions, headerColumnDef: any = null): any {
        const { enableEdit, disabledField, termIndeterminate, onChanged, useSetFilter, filterOptions, filterLabel, setChecked, hide, enableHiding, suppressFilter, enableRowGrouping, sort, suppressFilterUpdate, suppressMovable, minWidth, maxWidth, suppressSizeToFit, suppressExport } = options;
        const columnDef = this.createColumnDefBool(field, headerName, width, enableHiding, true, !suppressFilter, true, false, enableEdit, disabledField, termIndeterminate, useSetFilter, filterOptions, filterLabel, setChecked, hide, enableRowGrouping, sort, suppressFilterUpdate, suppressMovable, minWidth, maxWidth, suppressSizeToFit, suppressExport);
        this.addCellValueChangedEventListener(columnDef, onChanged);
        if (headerColumnDef)
            this.addChild(headerColumnDef, columnDef)
        else
            this.addColumn(columnDef);

        this.applyOptions(columnDef, options);
        return columnDef;
    }

    // Active
    private createColumnDefActive(field: string, headerName?: string, width?: number): any {
        const columnDef = this.createColumnDefBool("isActive", headerName ? headerName : "", width || 60, false, true, true, false, false, true, "disableActive", undefined, undefined, undefined, undefined, undefined, undefined, undefined, undefined, undefined, undefined, undefined, width || 60);
        columnDef.pinnedLeft = true;
        return columnDef;
    }
    public addColumnActive(field: string, headerName?: string, width?: number, onChanged?: CellChangedCallback): any {
        const columnDef = this.createColumnDefActive(field, headerName, width);

        this.addCellValueChangedEventListener(columnDef, onChanged);
        this.addColumn(columnDef);
       
        return columnDef;
    }

    // Icon
    public createColumnDefIcon(field: string, headerName?: string, width?: number, options?: IconColumnOptions): any {
        options = this.setDefaultOptionsIfNeeded(options, IconColumnOptions.default());
        let { enableHiding, enableResizing, icon, isSubgrid, onClick, getNodeOnClick, showIcon, toolTip, toolTipField, suppressSorting, suppressFilter, filterOptions, filterLabel, hide, showTooltipFieldInFilter, suppressExport, noPointer, tooltipComponent, tooltipComponentParams } = options;
        if (suppressFilter === undefined) {
            suppressFilter = false;
        }

        const columnDef = this.createColumnDef(field ? field : "icon", headerName ? headerName : " ", width || 22, enableHiding, enableResizing, !suppressFilter, !suppressSorting, true, false, undefined, hide);
        if (suppressExport) {
            columnDef.suppressExport = suppressExport;
        }
        if (toolTipField)
            columnDef.tooltipField = toolTipField;

        if (tooltipComponent) {
            columnDef.tooltipComponent = tooltipComponent;
            columnDef.tooltipComponentParams = tooltipComponentParams;
        }

        columnDef.cellRenderer = IconCellRenderer;
        columnDef.cellRendererParams = {
            onClick,
            getNodeOnClick,
            icon,
            isSubgrid,
            showIcon,
            toolTip,
            noPointer
        } as IIconCellRendererParams;

        if (suppressSorting === false) {
            columnDef.comparator = (valueA, valueB) => {
                if (!valueA)
                    return 1;
                if (!valueB)
                    return -1;

                return valueA.toLowerCase().localeCompare(valueB.toLowerCase());
            };
        }

        if (!suppressFilter) {
            columnDef.filter = "agSetColumnFilter";
            if (filterOptions && filterOptions.length > 0) {
                columnDef.filterValueGetter = (params) => {
                    const { data, column } = params;
                    let x = data[column.colDef.tooltipField];
                    return x;
                };

                columnDef.filterParams = {
                    values: filterOptions.map((o) => o[filterLabel]),
                    newRowsAction: 'keep',
                    suppressMiniFilter: true,
                    cellRenderer: IconFilterCellRenderer,
                    toolTip: toolTip,
                };
            }
            else if (showTooltipFieldInFilter) {
                columnDef.filterValueGetter = (params) => {
                    const { data, column } = params;
                    return data[column.colDef.field] + ":" + (data[column.colDef.tooltipField] ? data[column.colDef.tooltipField] : " ");
                };

                columnDef.filterParams = {
                    values: (params) => {
                        const { field, tooltipField } = params.colDef;
                        var values = [];
                        _.forEach(_.uniqBy(this.getData(), field), (item) => {
                            values.push(item[field] + ":" + (item[tooltipField] ? item[tooltipField] : " "));
                        });
                        params.success(values);
                    },
                    newRowsAction: 'keep',
                    suppressMiniFilter: true,
                    cellRenderer: IconFilterCellRenderer,
                };
            }
            else {
                columnDef.filterValueGetter = (params) => {
                    const { data, column } = params;
                    return data[column.colDef.field] ? data[column.colDef.field] : "fal fa-empty-set";
                };

                columnDef.filterParams = {
                    values: (params) => {
                        const { field } = params.colDef;
                        var values = [];
                        _.forEach(_.uniqBy(this.getData(), field), (item) => {
                            values.push(item[field] ? item[field] : "fal fa-empty-set");
                        });
                        values.sort();
                        params.success(values);
                    },
                    newRowsAction: 'keep',
                    suppressMiniFilter: true,
                    cellRenderer: IconFilterCellRenderer,
                };
            }
        }

        columnDef.floatingFilterComponentParams.suppressFilterButton = suppressFilter;

        columnDef.suppressSizeToFit = true;
        columnDef.resizable = false;
        columnDef.suppressMenu = true;
        columnDef.menuTabs = [];
        columnDef.cellStyle = { padding: "0" };

        this.setSoeType(columnDef, Constants.GRID_COLUMN_TYPE_ICON);

        return columnDef;
    }
    public addColumnIcon(field: string, headerName?: string, width?: number, options?: IconColumnOptions, headerColumnDef: any = null): any {
        const columnDef = this.createColumnDefIcon(field, headerName, width, options);
        columnDef.pinned = options && options.pinned;
        columnDef.cellClass = "text-center";
        if (headerColumnDef)
            this.addChild(headerColumnDef, columnDef)
        else
            this.addColumn(columnDef);

        this.applyOptions(columnDef, options);
        return columnDef;
    }

    // Edit
    private createColumnDefEdit(toolTip: string, onClickEvent?: DataCallback, isSubgrid?: boolean, showIcon?: FieldOrPredicate): any {
        const options = {
            icon: "fal fa-pencil iconEdit",
            suppressFilter: true,
            toolTip,
            onClick: onClickEvent,
            isSubgrid,
            showIcon,
            suppressExport: true
        } as IconColumnOptions;

        const columnDef = this.createColumnDefIcon("edit", null, null, options);

        columnDef.pinned = 'right';
        return columnDef;
    }
    public addColumnEdit(toolTip: string, onClickEvent?: DataCallback, isSubgrid?: boolean, showIcon?: FieldOrPredicate): any {
        const columnDef = this.createColumnDefEdit(toolTip, onClickEvent, isSubgrid, showIcon);
        this.addColumn(columnDef);
        return columnDef;
    }

    // Delete
    public createColumnDefDelete(toolTip: string, onClick?: DataCallback, isSubGrid?: boolean, showIcon?: FieldOrPredicate, icon?: string): any {
        const options = {
            icon: icon ? icon : "fal fa-times iconDelete",
            suppressFilter: true,
            toolTip,
            onClick,
            isSubGrid,
            showIcon
        } as IconColumnOptions;
        var columnDef = this.createColumnDefIcon("delete", null, null, options);

        columnDef.pinned = 'right';
        return columnDef;
    }
    public addColumnDelete(toolTip: string, onClick?: DataCallback, isSubgrid?: boolean, showIcon?: FieldOrPredicate, icon?: string, headerColumnDef: any = null): any {
        const columnDef = this.createColumnDefDelete(toolTip, onClick, isSubgrid, showIcon, icon);
        if (headerColumnDef)
            this.addChild(headerColumnDef, columnDef)
        else
            this.addColumn(columnDef);
        return columnDef;
    }

    // PDF
    private createColumnDefPdf(toolTip: string, onClickEvent?: DataCallback): any {
        const options = {
            icon: "fal fa-file-pdf",
            toolTip,
            onClick: onClickEvent
        } as IconColumnOptions;
        const columnDef = this.createColumnDefIcon("pdf", null, null, options);

        return columnDef;
    }
    public addColumnPdf(toolTip: string, onClickEvent?: DataCallback): any {
        const columnDef = this.createColumnDefPdf(toolTip, onClickEvent);
        this.addColumn(columnDef);
        return columnDef;
    }

    // IsModified
    public createColumnDefIsModified(field?: string, headerName?: string, width?: number, clickCallback?: (params: any) => void): any {
        field = field || "isModified";
        headerName = headerName || "";
        width = width || 20;
        const columnDef = this.createColumnDef(field, headerName, width, false, false, false, false, false, false);
        columnDef.pinned = "left";
        columnDef.suppressMovable = true;
        columnDef.suppressSizeToFit = true;
        columnDef.valueFormatter = () => ""; //displayNothing
        columnDef.cellClass = "fal fa-asterisk" + (!!clickCallback ? " link" : "");
        columnDef.cellClassRules = {
            "soe-ag-gridColumnIsModified": (params) => params.value,
            "colorTransparent": (params) => !params.value
        };

        if (typeof clickCallback === "function") {
            columnDef.onCellClicked = clickCallback;
        }

        this.setSoeType(columnDef, Constants.GRID_COLUMN_TYPE_MODIFIED);

        return columnDef;
    }
    public addColumnIsModified(field?: string, headerName?: string, width?: number, clickCallback?: (params: any) => void, headerColumnDef: any = null) {
        const columnDef = this.createColumnDefIsModified(field, headerName, width, clickCallback);
        if (headerColumnDef)
            this.addChild(headerColumnDef, columnDef)
        else
            this.addColumn(columnDef);
        return columnDef;
    }

    // HTML
    private createColumnDefHtml(field: string, headerName: string, width: number, enableHiding: boolean = false): any {
        throw "NotImplemented"; //NOSONAR
    }
    public addColumnHtml(field: string, headerName: string, width: number, enableHiding: boolean = false): any {
        const columnDef = this.createColumnDefHtml(field, headerName, width, enableHiding);
        this.addColumn(columnDef);
        return columnDef;
    }

    // Text
    public createColumnDefText(field: string, headerName: string, width: number, options?: TextColumnOptions): any {
        options = this.setDefaultOptionsIfNeeded(options, TextColumnOptions.default());
        const { enableHiding, toolTipField, cellClassRules, alignRight, toolTip, buttonConfiguration, formatter, enableColumnMenu, enableResizing, suppressSorting, shapeValueField, shape, useGradient, gradientField, hide, enableRowGrouping, showRowGroup, sort, suppressFilterUpdate, suppressMovable, minWidth, maxWidth, suppressSizeToFit, suppressExport, pinned, usePlainText } = options;
        const columnDef = this.createColumnDef(field, headerName, width, enableHiding, enableResizing, undefined, !suppressSorting, enableColumnMenu, false, false, hide, enableRowGrouping, sort, suppressFilterUpdate, suppressMovable, minWidth, maxWidth, suppressSizeToFit, suppressExport, showRowGroup);

        columnDef.pinned = pinned;
        columnDef.cellClass = alignRight ? "grid-text-right" : "text-left";

        columnDef.filter = 'agTextColumnFilter';
        columnDef.filterParams = {
            debounceMs: 1000,
            defaultOption: 'contains',
            newRowsAction: 'keep'
        };

        if (options.filterOptions && Array.isArray(options.filterOptions)) {
            columnDef.filterParams.filterOptions = options.filterOptions;
            columnDef.floatingFilterComponentParams.suppressFilterButton = false;
        }

        

        columnDef.tooltipField = toolTipField;
        columnDef.cellClassRules = cellClassRules;

        if (formatter) {
            columnDef.valueFormatter = formatter;
        }
        else {
            columnDef.valueFormatter = ({ value }) => {
                return value || "";
            };
        }

        columnDef.keyCreator = ({ value }) => {
            if (value == null || value === '') {
                return ''; 
            }
            return value;
        };

        columnDef.comparator = (valueA: string, valueB: string, nodeA: any, nodeB: any, isInverted: boolean) => {
            if (!valueA)
                return 1;
            if (!valueB)
                return -1;
            if (typeof valueA === "number" || !isNaN(+valueA)) {
                if (typeof valueB === "string" && isNaN(+valueB)) {
                    return 1;
                }
                if (+valueA === +valueB) {
                    return 0;
                }
                if (+valueA > +valueB) {
                    return -1;
                }
                if (+valueA < +valueB) {
                    return 1;
                }
            }
            else if (typeof valueA === "string") {
                if (typeof valueB === "number" || !isNaN(+valueB))
                    return -1;
                return valueA.toLowerCase().localeCompare(valueB.toLowerCase());
            }
            else {
                return 0;
            }
        };

        if (!usePlainText) {
            //TODO: Should encapsulate into CellRenderer
            columnDef.cellRenderer = (params: { valueFormatted: any, eGridCell: HTMLElement, data: any }) => {
                const { valueFormatted, eGridCell, data } = params;
                const fieldOutput = valueFormatted;

                if (toolTip && !toolTipField) {
                    eGridCell.setAttribute("title", toolTip);
                }

                if (shapeValueField && shape) {
                    let shapeSpan = document.createElement('span');
                    shapeSpan.innerHTML = ShapeCellRenderer.getShapeTemplate(data, shape, useGradient, gradientField, data[shapeValueField]);

                    const container = document.createElement('span');
                    container.appendChild(shapeSpan);

                    let textSpan = document.createElement('span');
                    textSpan.classList.add('shape-text');
                    textSpan.innerHTML = fieldOutput;
                    container.appendChild(textSpan);

                    return container;
                }

                if (buttonConfiguration && !!ObjectFieldHelper.getFieldFrom(data, buttonConfiguration.show)) {
                    const button = document.createElement('button');
                    button.classList.add("gridCellIcon");
                    ElementHelper.appendConcatClasses(button, buttonConfiguration.iconClass);
                    button.addEventListener('click', (e: Event) => {
                        (buttonConfiguration.callback || $.noop)(data);
                        e.stopPropagation();
                    });

                    const container = document.createElement('span');
                    container.appendChild(button);

                    let textSpan = document.createElement('span');
                    textSpan.innerHTML = fieldOutput;
                    container.appendChild(textSpan);

                    return container;
                }

                return valueFormatted;
            }
        }

        this.setSoeType(columnDef, Constants.GRID_COLUMN_TYPE_TEXT);

        return columnDef;
    }
    addColumnText(field: string, headerName: string, width: number, options?: TextColumnOptions, headerColumnDef: any = null): any {
        const columnDef = this.createColumnDefText(field, headerName, width, options);
        if (headerColumnDef)
            this.addChild(headerColumnDef, columnDef)
        else
            this.addColumn(columnDef);

        this.applyOptions(columnDef, options);
        return columnDef;
    }

    // Number        
    public createColumnDefNumber(field: string, headerName: string, width: number, options?: NumberColumnOptions): any {
        options = this.setDefaultOptionsIfNeeded(options, NumberColumnOptions.default());
        const { enableHiding, toolTipField, cellClassRules, toolTip, pinned, alignLeft, decimals, disabled, enableColumnMenu, enableResizing, hide, formatAsText, enableRowGrouping, aggFuncOnGrouping, allowAggFuncMenu, ignoreFormatDecimals, getter, setter, sort, suppressFilterUpdate, suppressMovable, minWidth, maxWidth, suppressSizeToFit, suppressExport, maxDecimals } = options;
        const columnDef = this.createColumnDef(field, headerName, width, enableHiding, enableResizing, undefined, undefined, enableColumnMenu, undefined, undefined, hide, enableRowGrouping, sort, suppressFilterUpdate, suppressMovable, minWidth, maxWidth, suppressSizeToFit, suppressExport);

        // Cell class rules
        columnDef.cellClassRules = cellClassRules;

        columnDef.pinned = pinned;
        columnDef.cellClass = alignLeft ? "text-left" : "grid-text-right";

        columnDef.tooltipField = toolTipField;
        //columnDef.floatingFilterComponent = NumberFloatingFilter;
        columnDef.filter = "agNumberColumnFilter";
        columnDef.filterParams = {
            filterOptions: [
                {
                    displayKey: 'startsWith',
                    displayName: this.translateText("startsWith", "Starts With"),
                    predicate: function (filterValue, cellValue) {
                        return cellValue ? cellValue.toString().startsWith(filterValue.toString()) : false;
                    }
                },
                {
                    displayKey: 'contains',
                    displayName: this.translateText("contains", "Contains"),
                    predicate: function (filterValue, cellValue) {
                        return cellValue ? cellValue.toString().contains(filterValue.toString()) : false;
                    }
                },
                'lessThan',
                'equals',
                'greaterThan'
            ],
            defaultOption: "startsWith",
            newRowsAction: 'keep'
        };
        columnDef.floatingFilterComponentParams.suppressFilterButton = false;

        //Grouping
        if (aggFuncOnGrouping)
            columnDef.aggFunc = aggFuncOnGrouping;
        if (allowAggFuncMenu)
            columnDef.enableValue = allowAggFuncMenu;

        // Value getter
        if (getter)
            columnDef.valueGetter = getter;

        // Value setter
        if (getter)
            columnDef.valueSetter = setter;

        //Sort
        columnDef.comparator = (val1, val2) => {
            if (typeof val1 === "string") {
                const value1 = NumberUtility.parseDecimal(val1);
                const value2 = NumberUtility.parseDecimal(val2);
                if (value1 === value2) {
                    return 0;
                }
                if (value1 > value2) {
                    return -1;
                }
                if (value1 < value2) {
                    return 1;
                }
            }
            else {
                if (val1 === val2) {
                    return 0;
                }
                if (val1 > val2) {
                    return -1;
                }
                if (val1 < val2) {
                    return 1;
                }
            }
        };

        columnDef.valueFormatter = (params) => {
            let { value } = params;
            const { api } = this.gridOptions;
            const editingCells = api ? api.getEditingCells() : [];
            if (_.isNil(value)) {
                return "";
            }

            if (typeof value === "string") {
                value = NumberUtility.parseDecimal(value);
            }

            if (typeof value !== "number" || isNaN(value) || (options.clearZero && value == 0)) {
                return "";
            }

            if (ignoreFormatDecimals && editingCells && editingCells.length > 0) {
                const rowNode = params.node;
                if (rowNode != undefined && rowNode.rowIndex != undefined && _.find(editingCells, c => c.rowIndex === rowNode.rowIndex))
                    return value;
                else
                    return formatAsText ? value : NumberUtility.printDecimal(value, decimals, maxDecimals);
            }
            else {
                return formatAsText ? value : NumberUtility.printDecimal(value, decimals, maxDecimals);
            }
        };
        columnDef.valueParser = (params) => {
            return NumberUtility.parseDecimal(params.newValue);
        };

        if (toolTip && !toolTipField) {
            columnDef.cellRenderer = (params: { value: any, eGridCell: HTMLElement }) => {
                const { eGridCell, value } = params;
                eGridCell.setAttribute("title", toolTip);
                return value || "";
            }
        }

        if (disabled) {
            columnDef.cellEditor = NumberCellEditor;
            columnDef.cellEditorParams = {
                isDisabled: disabled
            } as INumberCellEditorParams;
        }

        columnDef.keyCreator = ({ value }) => {
            if (value == null || value === '') {
                return '';
            }
            if (typeof value === "string") {
                value = NumberUtility.parseDecimal(value);
            }
            if (typeof value !== "number" || isNaN(value)) {
                return ''; 
            }
            return formatAsText ? value.toString() : NumberUtility.printDecimal(value, decimals, maxDecimals);
        };

        this.setSoeType(columnDef, Constants.GRID_COLUMN_TYPE_NUMBER);

        return columnDef;
    }
    addColumnNumber(field: string, headerName: string, width: number, options?: NumberColumnOptions, headerColumnDef: any = null): any {
        const columnDef = this.createColumnDefNumber(field, headerName, width, options);
        if (headerColumnDef)
            this.addChild(headerColumnDef, columnDef)
        else
            this.addColumn(columnDef);

        this.applyOptions(columnDef, options);
        return columnDef;
    }

    // Select (combobox)
    private createColumnDefSelect(field: string, headerName: string, width: number, options?: SelectColumnOptions): any {
        options = this.setDefaultOptionsIfNeeded(options, SelectColumnOptions.default());

        const { enableHiding, hide, displayField, selectOptions, dynamicSelectOptions, dropdownIdLabel, dropdownValueLabel, shapeValueField, shape, toolTipField, toolTip, useGradient, colorField, gradientField, populateFilterFromGrid, ignoreTextInFilter, enableRowGrouping, suppressFilter, suppressFilterUpdate, suppressMovable, minWidth, maxWidth, suppressSizeToFit, suppressExport, cellClassRules, pinned } = options;
        const columnDef = this.createColumnDef(field, headerName, width, enableHiding, undefined, !suppressFilter, undefined, undefined, undefined, undefined, hide, enableRowGrouping, undefined, suppressFilterUpdate, suppressMovable, minWidth, maxWidth, suppressSizeToFit, suppressExport);

        // Pin
        columnDef.pinned = pinned;

        // Cell class rules
        columnDef.cellClassRules = cellClassRules;

        //It is possible that the dynamic options doesn't use the same property names as with the overall options.
        if (dynamicSelectOptions) {
            dynamicSelectOptions.displayField = dynamicSelectOptions.displayField || dropdownValueLabel;
            dynamicSelectOptions.idField = dynamicSelectOptions.idField || dropdownIdLabel;
        }

        //Filter
        columnDef.floatingFilterComponentParams.suppressFilterButton = false;
        columnDef.comparator = (value1, value2) => {
            if (value1 === value2) {
                return 0;
            }
            if (value1 > value2) {
                return 1;
            }
            if (value1 < value2) {
                return -1;
            }
        };

        columnDef["soe-displayField"] = displayField;
        columnDef["soe-selectOptions"] = selectOptions;
        columnDef["soe-dropdownValueLabel"] = dropdownValueLabel;
        columnDef["soe-dropdownIdLabel"] = dropdownIdLabel;
        columnDef["soe-dynamicOptions"] = dynamicSelectOptions;

        //RichSelectCellEditor reuses the columns formatter when rendering items.
        columnDef.valueGetter = (params: any) => {
            const { data, colDef } = params;
            if (!data)
                return;
            const displayFld = colDef["soe-displayField"];
            return data[displayFld];
        };

        columnDef.valueSetter = (params: any) => {
            const { newValue, data, colDef } = params;
            const opt: Array<any> = colDef["soe-selectOptions"];
            const optionValueField: string = colDef["soe-dropdownIdLabel"];
            const dynamicOptions: IDynamicSelectOptions = columnDef["soe-dynamicOptions"];

            if (!data)
                return;

            let id = null;
            if (!newValue) {
                return false;
            }

            if (dynamicOptions) {
                id = newValue[dynamicOptions.idField];
            }
            else {
                const newOption = _.find(opt, (o) => o[dropdownValueLabel] === newValue[dropdownValueLabel]);
                if (_.isNil(newOption)) {
                    return false;
                }

                id = newValue[optionValueField];
            }

            data[field] = id;
            data[colDef["soe-displayField"]] = newValue[dropdownValueLabel];

            return true;
        };

        if (dynamicSelectOptions) {
            columnDef.cellEditor = "agRichSelectCellEditor";
            columnDef.cellEditorParams = function (params) {
                var optionData = ObjectFieldHelper.getFieldFrom<any[]>(params.data, dynamicSelectOptions.options);

                return {
                    values: optionData,
                    formatValue: function (value) {
                        return value ? (typeof value === "string" ? value : (value[dynamicSelectOptions.displayField] || "")) : "";
                    }
                };
            }
            /*
            columnDef.cellEditorParams = {
                values: (data) => ObjectFieldHelper.getFieldFrom<any[]>(data, dynamicSelectOptions.options),
                formatValue: (value) => value ? (typeof value === "string" ? value : (value[dynamicSelectOptions.displayField] || "")) : ""
            }; //as IDynamicRichSelectCellEditorParams;
            */
        }
        else {
            columnDef.cellEditor = 'agRichSelectCellEditor';
            columnDef.cellEditorParams = {
                values: selectOptions,
                formatValue: (value) => value ? (typeof value === "string" ? value : (value ? (value[dropdownValueLabel] || "") : "")) : ""
            };
        }

        columnDef.filter = "agSetColumnFilter";
        if (this.enableFiltering) {
            if (populateFilterFromGrid) {
                columnDef.filterParams = {
                    newRowsAction: 'keep',
                };
            }
            else {
                columnDef.filterParams = {
                    values: selectOptions.map((o) => o[dropdownValueLabel]),
                    newRowsAction: 'keep',
                };
            }
        }

        if (toolTipField) {
            columnDef.tooltipField = toolTipField;
        }

        if (shapeValueField && shape) {
            columnDef.cellRenderer = ShapeCellRenderer;
            columnDef.cellRendererParams = {
                colorField: colorField,
                isSelect: true,
                shape,
                toolTip: toolTip,
                displayField: displayField,
                useGradient: useGradient,
                gradientField: gradientField,
                ignoreTextInFilter: ignoreTextInFilter,
                width: width,
            } as IShapeCellRendererParams;
        }

        columnDef.keyCreator = ({ value }) => {
            if (value == null || value === '') {
                return '';
            }
            return value;
        };

        this.setSoeType(columnDef, Constants.GRID_COLUMN_TYPE_SELECT);

        return columnDef;
    }
    public addColumnSelect(field: string, headerName: string, width: number, options?: SelectColumnOptions, headerColumnDef: any = null): any {
        var columnDef = this.createColumnDefSelect(field, headerName, width, options);
        if (headerColumnDef)
            this.addChild(headerColumnDef, columnDef)
        else
            this.addColumn(columnDef);

        this.applyOptions(columnDef, options);
        return columnDef;
    }

    // DateTime
    private createColumnDefDateTime(field: string, headerName: string, width: number, enableHiding: boolean = false, cellFilter: string = null, options?: DateTimeColumnOptions): any {
        if (!options)
            options = new DateTimeColumnOptions();
        options.enableHiding = enableHiding;

        const { cellClassRules, alignRight, enableColumnMenu, enableResizing, disabled, hide, enableRowGrouping, suppressFilterUpdate, suppressMovable, minWidth, maxWidth, suppressSizeToFit, suppressExport } = options;
        const columnDef = this.createColumnDef(field, headerName, width, enableHiding, enableResizing, undefined, undefined, enableColumnMenu, null, true, hide, enableRowGrouping, undefined, suppressFilterUpdate, suppressMovable, minWidth, maxWidth, suppressSizeToFit, suppressExport);

        //Sort
        columnDef.comparator = (date1, date2) => {
            if (!date1)
                return -1;
            if (!date2)
                return 1;

            const canCompareAbsoluteTime = (v) => {
                return !!v.getTime && typeof v.getTime === "function";
            }

            const getCompareValue = (v) => canCompareAbsoluteTime(v) ? v.getTime() : v;

            const a = getCompareValue(date1);
            const b = getCompareValue(date2);

            if (a == b) {
                return 0;
            }
            if (a > b) {
                return 1;
            }
            if (a < b) {
                return -1;
            }
        };

        if (options.toolTipField)
            columnDef.tooltipField = options.toolTipField;

        columnDef.cellClassRules = cellClassRules;
        columnDef.cellClass = alignRight ? "grid-text-right" : "text-left";

        //Filter
        columnDef.filter = 'agDateColumnFilter';
        columnDef.filterParams = {
            comparator: function (filterLocalDateAtMidnight, cellValue) {
                if (!cellValue)
                    return -1;
                var gridDate = CalendarUtility.toFormattedDate(filterLocalDateAtMidnight);
                var filterDate = CalendarUtility.toFormattedDate(cellValue);
                if (gridDate == filterDate) {
                    return 0;
                }
                if (gridDate > filterDate) {
                    return -1;
                }
                if (gridDate < filterDate) {
                    return 1;
                }
            },
            browserDatePicker: true,
            newRowsAction: 'keep',
            inRangeInclusive: true,
        }

        columnDef.valueFormatter = ({ value }) => {
            let formatted = '';
            if (value) {
                let jsDate = CalendarUtility.convertToDate(value);
                if (jsDate)
                    return jsDate.toFormattedDateTime(options.showSeconds);
            }
            return formatted;
        };
        columnDef.valueParser = (params) => {
            return CalendarUtility.toFormattedDateAndTime(params.newValue);
        };
        columnDef.valueSetter = (params: any) => {
            const { newValue, data, colDef } = params;
            data[colDef.field] = CalendarUtility.convertToDate(newValue, CoreUtility.languageDateFormat); // newValue;
            return true;
        };
        columnDef.cellEditor = DateCellEditor;
        columnDef.cellEditorParams = {
            isDisabled: disabled,
            dateFormat: CoreUtility.languageDateFormat.toLowerCase(),
        } as IDateCellEditorParams;

        columnDef.keyCreator = ({ value }) => {
            if (value == null || value === '') {
                return '';
            }
            let jsDate = CalendarUtility.convertToDate(value);
            if (jsDate) {
                return jsDate.toFormattedDateTime(options.showSeconds);
            }
            return '';
        };

        this.setSoeType(columnDef, Constants.GRID_COLUMN_TYPE_DATETIME);

        return columnDef;
    }
    public addColumnDateTime(field: string, headerName: string, width: number, enableHiding: boolean = false, headerColumnDef: any = null, cellFilter: string = null, options?: DateTimeColumnOptions): any {
        const columnDef = this.createColumnDefDateTime(field, headerName, width, enableHiding, cellFilter, options);
        if (headerColumnDef)
            this.addChild(headerColumnDef, columnDef)
        else
            this.addColumn(columnDef);

        this.applyOptions(columnDef, options);
        return columnDef;
    }

    // Date
    private createColumnDefDate(field: string, headerName: string, width: number, enableHide: boolean = false, cellFilter: string = null, options?: DateColumnOptions): any {
        if (!options)
            options = new DateColumnOptions();
        options.enableHiding = enableHide;

        const { enableHiding, cellClassRules, alignRight, enableColumnMenu, enableResizing, suppressFilter, disabled, hide, enableRowGrouping, suppressFilterUpdate, suppressMovable, minWidth, maxWidth, suppressSizeToFit, suppressExport, pinned, sort } = options;
        const columnDef = this.createColumnDef(field, headerName, width, enableHiding, enableResizing, !suppressFilter, undefined, enableColumnMenu, null, true, hide, enableRowGrouping, sort, suppressFilterUpdate, suppressMovable, minWidth, maxWidth, suppressSizeToFit, suppressExport);

        columnDef.pinned = pinned;

        columnDef.cellClassRules = cellClassRules;
        columnDef.cellClass = alignRight ? "grid-text-right" : "text-left";

        //Sort
        columnDef.comparator = (date1, date2) => {
            if (!date1)
                return -1;
            if (!date2)
                return 1;

            const canCompareAbsoluteTime = (v) => {
                return !!v.getTime && typeof v.getTime === "function";
            }

            const getCompareValue = (v) => canCompareAbsoluteTime(v) ? v.getTime() : v;

            const a = getCompareValue(date1);
            const b = getCompareValue(date2);

            if (a == b) {
                return 0;
            }
            if (a > b) {
                return 1;
            }
            if (a < b) {
                return -1;
            }
        };

        if (options.toolTipField)
            columnDef.tooltipField = options.toolTipField;

        //Filter
        if (!suppressFilter) {
            columnDef.filter = 'agDateColumnFilter';
            columnDef.filterParams = {
                comparator: function (filterLocalDateAtMidnight, cellValue) {
                    if (!cellValue)
                        return -1;
                    var gridDate = CalendarUtility.toFormattedDate(filterLocalDateAtMidnight);
                    var filterDate = CalendarUtility.toFormattedDate(cellValue);
                    if (gridDate == filterDate) {
                        return 0;
                    }
                    if (gridDate > filterDate) {
                        return -1;
                    }
                    if (gridDate < filterDate) {
                        return 1;
                    }
                },
                includeBlanksInLessThan: true,
                browserDatePicker: true,
                newRowsAction: 'keep',
                inRangeInclusive: true,
            }
        }

        columnDef.valueFormatter = ({ value, node }) => {
            let formatted;
            if (node.group)
            {
                //already formated by keyCreator
                formatted = value;
            }
            else if (value) {
                const jsDate = CalendarUtility.convertToDate(value);
                if (jsDate)
                    formatted = options.dateFormat ? jsDate.toFormattedDate(options.dateFormat) : jsDate.toLocaleDateString(CoreUtility.language);
            }
            return formatted;
        };

        columnDef.keyCreator = ({ value }) => {
            let formatted = '';
            if (value) {
                const jsDate = CalendarUtility.convertToDate(value);
                if (jsDate) {
                    if (options.dateFormat) {
                        // For some reason grouping on two digit year does not work properly.
                        // The solution is to group it on four digits, but keeping options.dateFormat.
                        //  That will still cause it to be displayed with two dogits.
                        let fmt = options.dateFormat === 'YY' ? 'YYYY' : options.dateFormat;
                        formatted = jsDate.toFormattedDate(fmt);
                    } else {
                        formatted = jsDate.toLocaleDateString(CoreUtility.language);
                    }
                }
            }
            return formatted;
        };

        columnDef.valueParser = (params) => {
            return CalendarUtility.toFormattedDate(params.newValue);
        };
        columnDef.valueSetter = (params: any) => {
            const { newValue, data, colDef } = params;
            data[colDef.field] = CalendarUtility.convertToDate(newValue, CoreUtility.languageDateFormat); // newValue;
            return true;
        };
        columnDef.cellEditor = DateCellEditor;
        columnDef.cellEditorParams = {
            isDisabled: disabled,
            dateFormat: CoreUtility.languageDateFormat.toLowerCase(),
            minDate: options.minDate,
            maxDate: options.maxDate,
        } as IDateCellEditorParams;

        this.setSoeType(columnDef, Constants.GRID_COLUMN_TYPE_DATE);

        return columnDef;
    }
    public addColumnDate(field: string, headerName: string, width: number, enableHiding: boolean = false, headerColumnDef: any = null, cellFilter: string = null, options?: DateColumnOptions) {
        const columnDef = this.createColumnDefDate(field, headerName, width, enableHiding, cellFilter, options);
        if (headerColumnDef)
            this.addChild(headerColumnDef, columnDef)
        else
            this.addColumn(columnDef);

        this.applyOptions(columnDef, options);
        return columnDef;
    }

    // Time
    private createColumnDefTime(field: string, headerName: string, width: number, options?: TimeColumnOptions): any {
        if (!options)
            options = new TimeColumnOptions();

        const columnDef = this.createColumnDefText(field, headerName, width, options);
        columnDef.cellClass = (options && options.alignLeft ? "text-left" : "text-right");
        if (columnDef.cellClass === "text-right")
            columnDef.headerClass += ' text-right';
        columnDef["soeColumnType"] = "time";

        if (options.minutesToTimeSpan) {
            columnDef['minutesToTimeSpan'] = true;
            columnDef.filter = "agTextColumnFilter";
            columnDef.filterParams = {
                filterOptions: [
                    {
                        displayKey: 'lessThan',
                        displayName: 'Less Than',
                        predicate: function (filterValue, cellValue) {
                            return <number>cellValue < CalendarUtility.timeSpanToMinutes(filterValue);
                        }
                    },
                    {
                        displayKey: 'equals',
                        displayName: 'Equals',
                        predicate: function (filterValue, cellValue) {
                            return <number>cellValue == CalendarUtility.timeSpanToMinutes(filterValue);
                        }
                    },
                    {
                        displayKey: 'greaterThan',
                        displayName: 'Greater Than',
                        predicate: function (filterValue, cellValue) {
                            return <number>cellValue > CalendarUtility.timeSpanToMinutes(filterValue);
                        }
                    }
                ],
                includeBlanksInEquals: false,
                includeBlanksInLessThan: true,
                includeBlanksInGreaterThan: false,
                defaultOption: "equals",
                newRowsAction: 'keep'
            };
            columnDef.floatingFilterComponentParams.suppressFilterButton = false;
        } else if (options.minutesToDecimal) {
            columnDef['minutesToDecimal'] = true;
            columnDef.filter = "agNumberColumnFilter";
            columnDef.filterParams = {
                filterOptions: [
                    {
                        displayKey: 'startsWith',
                        displayName: this.translateText("startsWith", "Starts With"),
                        predicate: function (filterValue, cellValue) {
                            return cellValue ? cellValue.toString().startsWith(filterValue.toString()) : false;
                        }
                    },
                    {
                        displayKey: 'contains',
                        displayName: this.translateText("contains", "Contains"),
                        predicate: function (filterValue, cellValue) {
                            return cellValue ? cellValue.toString().contains(filterValue.toString()) : false;
                        }
                    },
                    'lessThan',
                    'equals',
                    'greaterThan'
                ],
                defaultOption: "startsWith",
                newRowsAction: 'keep'
            };
        } else {
            columnDef.filter = "date";
        }

        //Grouping
        if (options.aggFuncOnGrouping)
            columnDef.aggFunc = options.aggFuncOnGrouping;

        if (options.formatter) {
            columnDef.valueFormatter = options.formatter;
        } else {
            columnDef.valueFormatter = (params) => {
                if (params) {
                    var value = params.value;
                    if (options) {
                        var secondaryValue = options.secondaryField ? params.data[options.secondaryField] : undefined;
                        if (options.clearZero && CalendarUtility.isTimeZero(value) && (!options.secondaryField || CalendarUtility.isTimeZero(secondaryValue))) {
                            return '';
                        } else if (options.showGroupedAsNumber) {
                            return value;
                        } else if (options.minutesToTimeSpan) {
                            return CalendarUtility.minutesToTimeSpan(value, options.formatTimeWithDays, options.formatTimeWithSeconds);
                        } else if (options.minutesToDecimal) {
                            return CalendarUtility.minutesToDecimal(value);
                        }
                    }

                    return CalendarUtility.toFormattedTime(value, options.showSeconds);
                } else {
                    return '';
                }
            };
        }

        columnDef.keyCreator = ({ value }) => {
            if (value == null || value === '') {
                return ''; 
            }
            if (options.minutesToTimeSpan) {
                return CalendarUtility.minutesToTimeSpan(value, options.formatTimeWithDays, options.formatTimeWithSeconds);
            } else if (options.minutesToDecimal) {
                return CalendarUtility.minutesToDecimal(value);
            }
            return CalendarUtility.toFormattedTime(value, options.showSeconds);
        };

        columnDef.valueParser = (params) => {
            if (options.handleAsTimeSpan) {
                var timeSpan = CalendarUtility.parseTimeSpan(params.newValue);
                var timeParts = timeSpan.split(':');
                if (timeParts.length < 2)
                    return;
                var oldDate = CalendarUtility.convertToDate(params.oldValue);
                return new Date(oldDate.getFullYear(), oldDate.getMonth(), oldDate.getDate(), _.toNumber(timeParts[0]), _.toNumber(timeParts[1]), 0);
            }
        };

        //columnDef.editableCellTemplate = '<input data-ui-grid-editor type="time" ng-model="MODEL_COL_FIELD"></input>';

        this.setSoeType(columnDef, Constants.GRID_COLUMN_TYPE_TIME);

        return columnDef;
    }
    public addColumnTime(field: string, headerName: string, width: number, options?: TimeColumnOptions, headerColumnDef: any = null): any {
        const columnDef = this.createColumnDefTime(field, headerName, width, options);
        if (headerColumnDef)
            this.addChild(headerColumnDef, columnDef)
        else
            this.addColumn(columnDef);

        this.applyOptions(columnDef, options);
        return columnDef;
    }

    // TimeSpan
    private createColumnDefTimeSpan(field: string, headerName: string, width: number, options?: TimeColumnOptions): any {
        const columnDef = this.createColumnDefText(field, headerName, width, options);
        columnDef.cellClass = (options && options.alignLeft ? "text-left" : "text-right");
        if (columnDef.cellClass === "text-right")
            columnDef.headerClass += ' text-right';
        columnDef["soeColumnType"] = "timeSpan";

        columnDef.valueFormatter = ({ value }) => {
            let valueFormatted: string = undefined;
            if (value) {
                valueFormatted = value.toString();

                if (options && options.hideDays && valueFormatted.contains('.')) {
                    let parts = valueFormatted.split('.');
                    if (parts.length === 2) {
                        let days = parseInt(parts[0], 10);
                        let span = parts[1];
                        let minutes = (days * 24 * 60) + CalendarUtility.timeSpanToMinutes(span);
                        valueFormatted = CalendarUtility.minutesToTimeSpan(minutes);
                    }
                } else {
                    if (options && options.minDigits)
                        valueFormatted = valueFormatted.padLeft(options.minDigits, "0");
                    else if (valueFormatted.startsWith('0'))
                        //Fix ex 00:10 --> 0:10
                        valueFormatted = valueFormatted.substring(1, valueFormatted.length);

                    //Fix ex 10:45:00 --> 10:45 or 1.08:00:00 --> 1.08:00
                    if (valueFormatted.split(':').length > 2 && valueFormatted.endsWith(':00') && valueFormatted.length > 6)
                        valueFormatted = valueFormatted.substring(0, valueFormatted.length - 3);
                }
            }

            if (options && options.clearZero && (!valueFormatted || CalendarUtility.isTimeZero(valueFormatted)))
                valueFormatted = '';

            return valueFormatted;
        };

        columnDef.keyCreator = ({ value }) => {
            if (value == null || value === '') {
                return ''; 
            }
            let formatted = value.toString();

            if (options && options.hideDays && formatted.contains('.')) {
                let parts = formatted.split('.');
                if (parts.length === 2) {
                    let days = parseInt(parts[0], 10);
                    let span = parts[1];
                    let minutes = (days * 24 * 60) + CalendarUtility.timeSpanToMinutes(span);
                    formatted = CalendarUtility.minutesToTimeSpan(minutes);
                }
            } else {
                if (options && options.minDigits)
                    formatted = formatted.padLeft(options.minDigits, "0");
                else if (formatted.startsWith('0'))
                    formatted = formatted.substring(1, formatted.length);

                if (formatted.split(':').length > 2 && formatted.endsWith(':00') && formatted.length > 6)
                    formatted = formatted.substring(0, formatted.length - 3);
            }

            if (options && options.clearZero && (!formatted || CalendarUtility.isTimeZero(formatted)))
                return '';

            return formatted;
        };

        //TODO: Create timespan filter
        //columnDef.cellFilter = "minutesToTimeSpan";
        //columnDef.editableCellTemplate = '<input data-ui-grid-editor ng-model="MODEL_COL_FIELD" onFocus="this.select()"></input>';
        //(<any>columnDef).filters = [
        //    {
        //        condition: (term, value, row, column) => {
        //            return term ? Util.CalendarUtility.minutesToTimeSpan(value).contains(term) : true;
        //        }
        //    }
        //];
        if (options && options.aggFuncOnGrouping) {
            columnDef.aggFunc = options.aggFuncOnGrouping;
        }

        this.setSoeType(columnDef, Constants.GRID_COLUMN_TYPE_TIMESPAN);

        return columnDef;
    }
    public addColumnTimeSpan(field: string, headerName: string, width: number, options?: TimeColumnOptions, headerColumnDef: any = null): any {
        const columnDef = this.createColumnDefTimeSpan(field, headerName, width, options);
        columnDef.cellClass = (options && options.alignLeft ? "text-left" : "text-right");
        if (headerColumnDef)
            this.addChild(headerColumnDef, columnDef)
        else
            this.addColumn(columnDef);

        this.applyOptions(columnDef, options);
        return columnDef;
    }

    // Shape
    public createColumnDefShape(field: string, headerName?: string, width?: number, options?: ShapeColumnOptions): any {
        const columnDef = this.createColumnDef(field, headerName ? headerName : "", width || 22, false, false, undefined, undefined, undefined, undefined, true, undefined, undefined, undefined, undefined, options.suppressMovable);
        columnDef.minWidth = width;
        columnDef.maxWidth = width;
        if (options && options.suppressExport)
            columnDef.suppressExport = options.suppressExport;

        columnDef.headerClass = 'shape-column';
        columnDef.cellClass = 'shape-column';

        columnDef.comparator = (a, b, nodeA, nodeB, isInverted) => {
            if (!a && b) return 1;
            if (a && !b) return -1;
            if (a === b) return 0;
            if (a < b) return 1;
            return -1 * (isInverted ? -1 : 1);
        };

        columnDef.filter = "agSetColumnFilter";
        if (options.toolTipField) {
            columnDef.tooltipField = options.toolTipField;
        }

        columnDef.cellRenderer = ShapeCellRenderer;
        columnDef.cellRendererParams = {
            shape: options.shape,
            color: options.color,
            showIcon: options.showIconField,
            toolTip: options.toolTip,
            displayField: options.toolTipField,
            useGradient: options.attestGradient,
            gradientField: options.gradientField,
            colorField: options.colorField,
            width: options.shapeWidth,
            showEmptyIcon: options.showEmptyIcon

        } as IShapeCellRendererParams;

        this.setSoeType(columnDef, Constants.GRID_COLUMN_TYPE_SHAPE);

        return columnDef;
    }
    public addColumnShape(field: string, headerName?: string, width?: number, options?: ShapeColumnOptions, headerColumnDef: any = null): any {
        const columnDef = this.createColumnDefShape(field, headerName, width, options);
        if (options.pinned)
            columnDef.pinned = options.pinned;

        if (headerColumnDef)
            this.addChild(headerColumnDef, columnDef)
        else
            this.addColumn(columnDef);

        this.applyOptions(columnDef, options);
        return columnDef;
    }

    // Type ahead
    private createColumnDefTypeAhead(field: string, headerName?: string, width?: number, options?: TypeAheadColumnOptions): any {
        const { error, secondRow, hideSecondRowSeparator, displayField, typeAheadOptions, cellClassRules, useSetFilter, filterOptions, enableRowGrouping, suppressFilterUpdate, suppressMovable, minWidth, maxWidth, suppressSizeToFit, suppressExport, ignoreColumnOnGrouping, hide } = options;
        const columnDef = this.createColumnDef(field, headerName, width, false, true, true, undefined, undefined, undefined, undefined, hide, enableRowGrouping, undefined, suppressFilterUpdate, suppressMovable, minWidth, maxWidth, suppressSizeToFit, suppressExport);

        if (useSetFilter) {
            columnDef.filter = "agSetColumnFilter";
            columnDef.filterParams = {
                values: filterOptions.map((o) => o[typeAheadOptions.displayField]),
                newRowsAction: 'keep',
            };
        }
        else {
            columnDef.filter = 'agTextColumnFilter';
            columnDef.filterParams = {
                defaultOption: 'contains',
                newRowsAction: 'keep'
            };
        }
        columnDef.floatingFilterComponentParams.suppressFilterButton = false;

        columnDef.cellClassRules = (cellClassRules) ? cellClassRules : {};

        if (error) {
            columnDef.cellClassRules["invalid-cell"] = (params) => !!ObjectFieldHelper.getFieldFrom(params.data, error);
        }

        if (displayField) {
            columnDef.tooltipField = displayField;
            columnDef.valueFormatter = (params => params.data[displayField]);
        }

        //If there are not error or secondRow options, there is not need for any special renderer. 
        if (error || secondRow) {
            columnDef.cellRenderer = ExtendedTextCellRenderer;
            columnDef.cellRendererParams = {
                secondRow,
                error,
                separator: hideSecondRowSeparator ? '' : "-",
                ignoreColumnOnGrouping
            } as IExtendedTextCellRendererParams;
        }

        columnDef.cellEditor = TypeAheadCellEditor;
        columnDef.cellEditorParams = {
            typeAheadOptions,
            field
        } as ITypeAheadCellEditorParams;

        columnDef.keyCreator = ({ value, data }) => {
            
            const displayValue = displayField ? data[displayField] : value;
            if (displayValue == null || displayValue === '') {
                return ''; 
            }
            return displayValue;
        };

        this.applyOptions(columnDef, options);

        this.setSoeType(columnDef, Constants.GRID_COLUMN_TYPE_TYPEAHEAD);

        return columnDef;
    }
    public addColumnTypeAhead(field: string, headerName?: string, width?: number, options?: TypeAheadColumnOptions, soeData?: any, headerColumnDef?: any): any {
        const columnDef = this.createColumnDefTypeAhead(field, headerName, width, options);
        if (headerColumnDef)
            this.addChild(headerColumnDef, columnDef)
        else
            this.addColumn(columnDef);
        this.setSoeAdditionalData(columnDef, soeData || {});

        this.applyOptions(columnDef, options);
        return columnDef;
    }

    private setSoeAdditionalData(colDef: any, data: any) {
        colDef.soeData = data;
    }

    private setSoeType(colDef: any, typename: string) {
        colDef.soeType = typename;
    }

    // GRID MENU

    public addGridMenuItem(menuItem: IMenuItem | string, colId: string = "soe-grid-menu-column") {
        const items = (this.gridMenuItems[colId] || (this.gridMenuItems[colId] = [])) as (IMenuItem | string)[];
        items.push(menuItem);
    }

    // LAYOUT

    public setMinRowsToShow(nbrOfRows: number) {
        if (this.gridOptions.api) {
            this.updateGridHeightBasedOnNbrOfRows(nbrOfRows, this.gridOptions.api);
        }
        this.minRowsToShow = nbrOfRows;
    }

    public setAutoHeight(setToAuto: boolean) {
        if (this.gridOptions.api) {
            setToAuto === true ? this.gridOptions.api.setDomLayout("autoHeight") : this.gridOptions.api.setDomLayout("normal");
        }

        const gridElement = this.getHostDiv(this.gridOptions.api) as HTMLElement;
        if (setToAuto === true) {
            if (gridElement)
                gridElement.style.height = ""
        } else {
            this.updateGridHeightBasedOnActualRows()
        }
    }

    public refreshGrid() {
        this.gridOptions.api.redrawRows();
    }

    public refreshChildGrid(name: string) {
        var detailGridInfo = this.gridOptions.api.getDetailGridInfo(name);
        if (detailGridInfo) {
            detailGridInfo.api.redrawRows();

            let cell = detailGridInfo.api.getFocusedCell();
            if (cell) {
                detailGridInfo.api.setFocusedCell(cell.rowIndex, cell.column);
            }
        }
    }

    public refreshRows(...datas: any[]) {
        const { api } = this.gridOptions;
        const refreshOptions = {
            rowNodes: null,
            force: true
        };

        if (!api) {
            if (!this.gridApi)
                return;

            if (datas && datas.length > 0) {

                const nodes = _(datas)
                    .map(d => this.getRowNodeIdFromData(d))
                    .map(id => this.gridApi.getRowNode(id))
                    .value();

                refreshOptions.rowNodes = nodes;
                refreshOptions.force = true;
            }

            const editingCells = this.gridApi.getEditingCells()
            const cellInEditMode = editingCells.length > 0 ? editingCells[0] : null;

            this.gridApi.stopEditing(true);
            this.gridApi.refreshCells(refreshOptions);
            if (cellInEditMode) {
                this.gridApi.startEditingCell({
                    rowIndex: cellInEditMode.rowIndex,
                    colKey: cellInEditMode.column
                });
            }

            _.forEach(this.alignedGrids, g => g.refresh());
        }
        else {
            if (datas && datas.length > 0) {

                const nodes = _(datas)
                    .map(d => this.getRowNodeIdFromData(d))
                    .map(id => api.getRowNode(id))
                    .value();

                refreshOptions.rowNodes = nodes;
                refreshOptions.force = true;
            }

            const editingCells = api.getEditingCells()
            const cellInEditMode = editingCells.length > 0 ? editingCells[0] : null;

            api.stopEditing(true);
            api.refreshCells(refreshOptions);
            if (cellInEditMode) {
                api.startEditingCell({
                    rowIndex: cellInEditMode.rowIndex,
                    colKey: cellInEditMode.column
                });
            }

            _.forEach(this.alignedGrids, g => g.refresh());
        }
    }

    public refreshRowsIgnoreFocus(...datas: any[]) {
        const { api } = this.gridOptions;
        const refreshOptions = {
            rowNodes: null,
            force: true
        };

        if (!api) {
            if (!this.gridApi)
                return;

            if (datas && datas.length > 0) {

                const nodes = _(datas)
                    .map(d => this.getRowNodeIdFromData(d))
                    .map(id => this.gridApi.getRowNode(id))
                    .value();

                refreshOptions.rowNodes = nodes;
                refreshOptions.force = true;
            }

            this.gridApi.stopEditing(true);
            this.gridApi.refreshCells(refreshOptions);

            _.forEach(this.alignedGrids, g => g.refresh());
        }
        else {
            if (datas && datas.length > 0) {

                const nodes = _(datas)
                    .map(d => this.getRowNodeIdFromData(d))
                    .map(id => api.getRowNode(id))
                    .value();

                refreshOptions.rowNodes = nodes;
                refreshOptions.force = true;
            }

            api.stopEditing(true);
            api.refreshCells(refreshOptions);

            _.forEach(this.alignedGrids, g => g.refresh());
        }
    }

    public refreshColumns() {
        this.gridOptions.api.refreshHeader();
    }

    public refreshColumnsForChildGrid(name: string) {
        const detailGridInfo = this.gridOptions.api.getDetailGridInfo(name);
        if (detailGridInfo)
            detailGridInfo.api.refreshHeader();
    }

    public resetColumns() {
        this.gridOptions.api.setColumnDefs(this.gridOptions.columnDefs);
    }

    public refreshCells(force = false) {
        const refreshOptions = {
            rowNodes: null,
            columns: null,
            force: force
        };

        this.gridOptions.api.refreshCells(refreshOptions);
    }

    public finalizeInitGrid(ignoreAddMenu?: boolean, setIsActiveDefaultFilter? : boolean) {
        if (!this.gridOptions.api) {
            return;
        }
        if (this.enableGridMenu && !ignoreAddMenu) {
            this.gridOptions.columnDefs.push({
                field: 'soe-grid-menu-column',
                headerName: '',
                pinned: 'right',
                width: 20,
                sortable: false,
                suppressSizeToFit: true,
                suppressMovable: true,
                filter: false,
                resizable: false,
                suppressNavigable: true,
                suppressColumnsToolPanel: true,
                suppressExport: true,
                menuTabs: ['generalMenuTab', 'columnsMenuTab']
            });
        }

        this.gridOptions.api.setColumnDefs(this.gridOptions.columnDefs); //Columns added after the grid initialization will not be displayed, reset the columns. Rows and 
        //this.alignedGrids.forEach(g => g.setColumnDefs(this.gridOptions.columnDefs)); //Removed because of task 41068

        // Fallback...
        if (this.enableRowSelection === false)
            this.hideColumn('soe-row-selection');

        this.refreshGrid();

        if (setIsActiveDefaultFilter) {
            this.setFilter('isActive', { values: ['true'] });
        }

        if (!this.ignoreResizeToFit) {
            this.$timeout(() => {
                if (this.gridOptions.api)
                    this.gridOptions.api.sizeColumnsToFit();
            }, 500);
        }
    }

    private applyOptions(columnDef: any, options: ColumnOptions): void {
        if (!options) {
            return;
        }

        if (typeof options.editable === "function") {
            columnDef.editable = ({ node }) => (options.editable as Function)(node.data, columnDef.field);
        } else if (typeof options.editable === "boolean") {
            columnDef.editable = options.editable;
        }

        if (options.addNotEditableCSS) {
            if (!columnDef.cellClassRules)
                columnDef.cellClassRules = {};

            if (typeof options.editable === "function") {
                columnDef.cellClassRules["gridCellDisabled"] = (gridRow) => !(options.editable as Function)(gridRow.data);
            } else if (typeof options.editable === "boolean") {
                columnDef.cellClassRules["gridCellDisabled"] = () => !options.editable;
            }
        }

        if (options.strikeThrough) {
            if (!columnDef.cellClassRules)
                columnDef.cellClassRules = {};

            if (typeof options.strikeThrough === "function") {
                columnDef.cellClassRules["strike-through"] = ({ node }) => (options.strikeThrough as Function)(node.data, columnDef.field);
            } else if (typeof options.strikeThrough === "boolean") {
                columnDef.cellClassRules["strike-through"] = () => options.strikeThrough;
            }
        }

        if (options.cellStyle) {
            if (typeof options.cellStyle === "function") {
                columnDef.cellStyle = ({ node }) => (options.cellStyle as Function)(node.data, columnDef.field);
            }
        }

        this.addCellValueChangedEventListener(columnDef, options.onChanged);
    }

    private setDefaultOptionsIfNeeded<T extends ColumnOptions>(options: T, defaultOptions: T): T {
        return _.assignWith<T, T>(options, defaultOptions, (destinationValue, srcValue) => _.isUndefined(destinationValue) ? srcValue : destinationValue);
    }

    // DATA

    public clearData() {
        if (this.gridOptions.api) {
            this.gridOptions.api.setRowData([]);
        }
        else {
            this.gridOptions.rowData = [];
        }
    }

    public setData(rows: any[]) {
        const { api, keepGroupState } = this.gridOptions;
        if (rows !== null) {
            if (api) {
                let filterModel = api.getFilterModel();
                this.gridOptions.rowData = rows;
                api.setRowData(rows);
                //setTimeout(() => {
                api.forEachNode(this.propegateNodeIdToData);
                //}, 100);
                if (!this.ignoreResetFilterModel)
                    api.setFilterModel(filterModel);
                if (!this.ignoreResizeToFit)
                    api.sizeColumnsToFit();
            } else if (this.gridApi) {
                let filterModel = this.gridApi.getFilterModel();
                this.gridApi.rowData = rows;
                this.gridApi.setRowData(rows);
                this.gridApi.forEachNode(this.propegateNodeIdToData);
                if (!this.ignoreResetFilterModel)
                    this.gridApi.setFilterModel(filterModel);
                if (!this.ignoreResizeToFit)
                    this.gridApi.sizeColumnsToFit();
            } else {
                this.gridOptions.rowData = rows;

                if (!this.ignoreResizeToFit && this.gridOptions.api)
                    this.gridOptions.api.sizeColumnsToFit();
            }
            if (keepGroupState && this.openRowGroups) {
                api.forEachNode((node, idx) => {
                    if (this.openRowGroups[node.level + node.key]) {
                        node.setExpanded(this.openRowGroups[node.level + node.key]);
                    }
                })
            }
        } else {
            this.clearData();
        }
    }

    public setDataForChildGrid(name: string, rows: any[]) {
        var detailGridInfo = this.gridOptions.api.getDetailGridInfo(name);
        if (rows !== null) {
            if (detailGridInfo) {
                detailGridInfo.api.setRowData(rows);
                //setTimeout(() => {
                detailGridInfo.api.forEachNode(this.propegateNodeIdToData);
                //}, 100);
            }
        }
    }

    public getData() {
        return this.gridOptions.rowData;
    }

    public addRow(row: any, setFocus: boolean = false, columnToFocus: Field | any = null, insertBeforeRow: any = null): number {
        if (!row) {
            return null;
        }

        const { api } = this.gridOptions;
        if (!api) {
            this.gridOptions.rowData.push(row);
            return;
        }

        const dataTrans = { add: [row] };

        if (insertBeforeRow) {
            var beforeNode = api.getRowNode(this.getRowNodeIdFromData(insertBeforeRow));
            dataTrans["addIndex"] = beforeNode.rowIndex + 1; //not recommended way to do so. Should be sorting by rowNr instead.
        }
        const nodeTrans = api.applyTransaction(dataTrans);
        const addedNode = nodeTrans.add[0]
        this.propegateNodeIdToData(addedNode);

        if (setFocus) {
            this.startEditingCell(row, columnToFocus);
        }

        return addedNode.rowIndex;
    }

    public deleteRow(row: any) {
        const dataTrans = { remove: [row] };
        this.gridOptions.api.applyTransaction(dataTrans);
    }

    public getVisibleRowByIndex(rowIndex: number): any {
        return this.gridOptions.api.getDisplayedRowAtIndex(rowIndex);
    };

    public getNextRow(rowData: any, usingDetail: boolean = false): any {
        const nodeId = this.getRowNodeIdFromData(rowData);
        const rowNode = this.gridOptions.api.getRowNode(nodeId);
        const nextRowIndex = rowNode.childIndex + (usingDetail ? 2 : 1); //In budget indexes get scrambled (plus one?) by using details per cell

        return { rowIndex: nextRowIndex, rowNode: this.gridOptions.api.getDisplayedRowAtIndex(nextRowIndex) };
    }

    public getPreviousRow(rowData: any, usingDetail: boolean = false): any {
        const nodeId = this.getRowNodeIdFromData(rowData);
        const rowNode = this.gridOptions.api.getRowNode(nodeId);
        const previousRowIndex = rowNode.childIndex - (usingDetail ? 2 : 1); //In budget indexes get scrambled (plus one?) by using details per cell

        return { rowIndex: previousRowIndex, rowNode: this.gridOptions.api.getDisplayedRowAtIndex(previousRowIndex) };
    }

    public addFooterRow(elementQuerySelector: string, aggregations: IColumnAggregations, includeFilter?: (data) => boolean, readOnlyGrid?: boolean): void {
        this.addAlignedGrid(elementQuerySelector, "soe-ag-footer-row", new ColumnAggregationFooterGrid(aggregations, this.getColumnDefs(false, true), includeFilter, readOnlyGrid));
    }

    public addAggregatedFooterRow(elementQuerySelector: string, aggregations: IColumnAggregations, includeFilter?: (data) => boolean, readOnlyGrid?: boolean): ColumnAggregationFooterGrid {
        const footerGrid = new ColumnAggregationFooterGrid(aggregations, this.getColumnDefs(), includeFilter, readOnlyGrid)
        this.addAlignedGrid(elementQuerySelector, "soe-ag-footer-row", footerGrid);
        return footerGrid;
    }

    public addTotalRow(elementQuerySelector: string, texts: IDisplayTexts): TotalsGrid {
        const totalsGrid: TotalsGrid = new TotalsGrid(elementQuerySelector, texts);
        this.addAlignedGrid(elementQuerySelector, "soe-ag-totals-row", totalsGrid);
        return totalsGrid;
    }

    public removeTotalRow(elementQuerySelector: string): void {
        _.pullAll(this.alignedGrids, _.filter(this.alignedGrids, g => g['id'] == elementQuerySelector));
        $(elementQuerySelector).empty();
    }

    private getHostDiv(api: any): any {
        if (!api?.gridBodyCon)
            return null;
        
        return api.ctrlsService.gridCtrl.eGridHostDiv;
    }

    private addAlignedGrid(elementQuerySelector, typeClass: string, newAlignedGrid: IAlignedGrid) {
        const { api } = this.gridOptions;
        if (!api) {
            throw "addAlignedGrid: Grid has to be initialized before adding aligned grids.";
        }

        this.alignedGrids.push(newAlignedGrid);
        newAlignedGrid.processMainGridInitialize(this.gridOptions);

        const options = newAlignedGrid.getOptions();

        this.alignGrid(options);

        const eGrid = $(this.getHostDiv(api)).siblings(elementQuerySelector)[0];

        if (eGrid) {
            eGrid.classList.add("soe-ag-aligned-grid", typeClass);
            const gridHeightInPixels = parseInt(eGrid.style.height.replace("px", ""));
            if (!isNaN(gridHeightInPixels)) {
                options.rowHeight = gridHeightInPixels;
            }

            newAlignedGrid.agGrid = new agGrid.Grid(eGrid, options);
        }
    }

    private alignGrid(newGridOptionToAlign: any) {
        this.gridOptions.alignedGrids.push(newGridOptionToAlign)
        newGridOptionToAlign.suppressHorizontalScroll = true;
        newGridOptionToAlign.sideBar = false;
    }

    public selectRow(row: any, forceGridUpdate: boolean = false) {
        const nodeId = this.getRowNodeIdFromData(row);
        if (nodeId) {
            const node = this.gridOptions.api.getRowNode(nodeId);
            if (node)
                node.setSelected(true);
        }
    }

    public selectRows(rows: any[]) {
        _.forEach(rows, row => {
            this.selectRow(row);
        });
    }

    public selectAllRows() {
        this.gridOptions.api.forEachNode((node: any) => {
            node.setSelected(true);
        });
    }

    public selectRowByVisibleIndex(index: number, forceGridUpdate: boolean = false) {
        const node = this.gridOptions.api.getRowNode(index);
        if (node)
            node.setSelected(true);
    }

    public unSelectRow(row: any, forceGridUpdate: boolean = false) {
        const nodeId = this.getRowNodeIdFromData(row);
        if (nodeId) {
            const { api } = this.gridOptions;
            const node = api.getRowNode(nodeId);
            if (node)
                node.setSelected(false);

            if (forceGridUpdate) {
                this.refreshGrid();
            }
        }
    }

    public clearSelectedRows() {
        const { api } = this.gridOptions;
        api.forEachNode(node => node.setSelected(false));
    }

    public getCurrentRow(): any {
        const { api } = this.gridOptions;

        if (api) {
            const cellInfo = api.getFocusedCell();
            if (cellInfo) {
                const row = api.getDisplayedRowAtIndex(cellInfo.rowIndex);
                return (row) ? row.data : undefined;
            } else
                return undefined;
        }

        return null;
    }

    public getRowIndexFor(data: any): number {
        const { api } = this.gridOptions;
        const nodeId = this.getRowNodeIdFromData(data);
        const node = api.getRowNode(nodeId);

        return node?.rowIndex ?? 0;
    }

    public getCurrentRowCol(): { row: any, column: any } /*uiGrid.cellNav.IRowCol<any>*/ {
        const { api } = this.gridOptions;
        const cell = api.getFocusedCell();
        const row = this.getCurrentRow();
        return { row, column: cell.column };
    }

    public getSelectedRows(): any[] {
        if (this.gridOptions.api)
            return this.gridOptions.api.getSelectedRows();
        else
            return [];
    }

    public getSelectedNodes(): any[] {
        if (this.gridOptions.api)
            return this.gridOptions.api.getSelectedNodes();
        else
            return [];
    }

    public getFilteredRows(): any[] {
        const datas = [];
        if (this.gridOptions.api) {
            this.gridOptions.api.forEachNodeAfterFilterAndSort(({ data }) => datas.push(data));
        }

        return datas;
    }

    public getSelectedCount(): number {
        return this.getSelectedRows().length;
    }

    public getSelectedIds(idField: string): number[] {
        return _.map(this.getSelectedRows(), row => row[idField] as number);
    }

    public startEditingCell(row: any | number, column: Field | any) {
        const waitForGridApi = () => {
            if (this.gridOptions.api) {
                const { api } = this.gridOptions;
                const rowIndex = this.getRowIndex(row);
                const colKey = this.getColKey(column);

                //Some sort of work around to be able open in edit mode when scolling.
                api.ensureIndexVisible(rowIndex);
                api.ensureColumnVisible(colKey);

                api.setFocusedCell(rowIndex, colKey);

                this.$timeout(() => {
                    api.startEditingCell({ rowIndex, colKey });
                }, 500, false);
            } else {
                setTimeout(waitForGridApi, 100);
            }
        };

        waitForGridApi();
    }

    public stopEditing(cancel: boolean) {
        const { api } = this.gridOptions;
        if(api)
            api.stopEditing(cancel);
    }

    public startEditingColumn(column: Field | any) {
        this.startEditingCell(this.getCurrentRow(), column);
    }

    public getRowIndex(row: any | number): number {
        const { api } = this.gridOptions;

        if (!row) {
            return api.getFirstDisplayedRow();
        }
        if (typeof row === 'number') {
            return row;
        }
        else {
            return api.getRowNode(this.getRowNodeIdFromData(row)).rowIndex;
        }
    }

    private getColKey(column: Field | any): any {
        const columnDefs = this.gridOptions.columnDefs as { field: string }[];

        if (!column) {
            //find first user defined column.    
            return _.find(columnDefs, (c) => !c.field.startsWithCaseInsensitive("soe-"));
        }

        if (typeof column === "string") {
            //it's the field
            return column;
        }

        //colId if Column or field of ColumnDef
        return column.colId || column.field;
    }

    public refocusCell() {
        const api = this.gridOptions.api;

        const cell = api.getFocusedCell()
        api.clearFocusedCell();
        api.setFocusedCell(cell.rowIndex, cell.column, cell.floating);
    }

    public setFocusedCell(rowIndex: number, column: any) {
        const api = this.gridOptions.api;
        const colKey = this.getColKey(column);
        api.setFocusedCell(rowIndex, colKey);
    }

    public clearFocusedCell() {
        this.gridOptions.api.clearFocusedCell();
    }

    public setFilterFocus() {
        $("div[ref='eFloatingFilterInput'] input[ref='eInput']")[0].focus();
    }

    public getColumnIndex(field: string) {
        return _.findIndex(this.gridOptions.columnDefs, (colDef: any) => colDef.field === field);
    }

    public findInData<T>(predicate: TypedPredicate<T>): T {
        return _.find<T>(this.gridOptions.rowData, predicate);
    }

    public findAllInData<T>(predicate: TypedPredicate<T>): T[] {
        return _.filter<T>(this.gridOptions.rowData, predicate);
    }

    public sortFirst(sortprop?: string) {
        throw "NotImplemented"; //NOSONAR
        //if (!sortprop)
        //    sortprop = 'sort';

        //// Get current row
        //var rowItem = this.getCurrentRow();
        //if (rowItem != null && rowItem[sortprop] > 1) {
        //    // Move row to the top
        //    rowItem[sortprop] = -1;
        //    this.reNumberRows(sortprop);
        //    this.scrollToFocus(rowItem, 1);
        //}
    }

    public sortUp(sortprop?: string) {
        throw "NotImplemented"; //NOSONAR
        //if (!sortprop)
        //    sortprop = 'sort';
        //// Get current row
        //var rowItem = this.getCurrentRow();
        //if (rowItem != null && rowItem[sortprop] > 1) {
        //    var filterObj = {};
        //    filterObj[sortprop] = rowItem[sortprop] - 1;
        //    // Get previous row
        //    var prevRowItem = (_.filter(this.gridOptions.data, filterObj))[0];

        //    // Move row up
        //    if (prevRowItem != null) {
        //        // Multiply each row number by 10, to be able to insert row numbers in between
        //        this.multiplyRowNr(sortprop);

        //        // Move current row before previous row
        //        rowItem[sortprop] -= 19;
        //        this.reNumberRows(sortprop);
        //        this.scrollToFocus(rowItem, 1);
        //    }
        //}
    }

    public sortDown(sortprop?: string) {
        throw "NotImplemented"; //NOSONAR
        //if (!sortprop)
        //    sortprop = 'sort';

        //// Get current row
        //var rowItem = this.getCurrentRow();
        //if (rowItem != null && rowItem[sortprop] < this.gridOptions.data.length) {
        //    var filterObj = {};
        //    filterObj[sortprop] = rowItem[sortprop] + 1;
        //    // Get next row
        //    var nextRowItem = (_.filter(this.gridOptions.data, filterObj))[0];
        //    // Move row down
        //    if (nextRowItem) {
        //        // Multiply each row number by 10, to be able to insert row numbers in between
        //        this.multiplyRowNr(sortprop);

        //        // Move current row after next row                    
        //        rowItem[sortprop] += 12;
        //        this.reNumberRows(sortprop);
        //        this.scrollToFocus(rowItem, 1);
        //    }
        //}
    }

    public sortLast(sortprop?: string) {
        throw "NotImplemented"; //NOSONAR
        //if (!sortprop)
        //    sortprop = 'sort';

        //// Get current row
        //var rowItem = this.getCurrentRow();
        //if (rowItem && rowItem[sortprop] < this.gridOptions.data.length) {
        //    // Move row to the bottom
        //    rowItem[sortprop] = Util.NumberUtility.max(this.gridOptions.data, sortprop) + 2;
        //    this.reNumberRows(sortprop);
        //    this.scrollToFocus(rowItem, 1);
        //}
    }

    public reNumberRows(sortprop?: string) {
        sortprop = sortprop || "sort";

        const rows = this.gridOptions.rowData as any[];
        _(rows)
            .sortBy(sortprop)
            .forEach((row, index) => {
                const propValue = index + 1;
                if (row[sortprop] !== propValue) {
                    row[sortprop] = propValue;
                    row.isModified = true;
                }
            });
    }

    // FILTERS

    public clearFilters() {
        this.gridOptions.api.setFilterModel(null);
    }

    public getFilterModels() {
        return this.gridOptions.api ? this.gridOptions.api.getFilterModel() : null;
    }

    public getFilterValueModel(field: string) {
        const instance = this.gridOptions.api.getFilterInstance(field);
        //now only handles boolean values
        return instance && instance.appliedModel ? instance.appliedModel.values : undefined;
    }

    public setFilter(field: string, filterModel: any) {
        const instance = this.gridOptions.api.getFilterInstance(field);
        instance.setModel(filterModel);
        this.gridOptions.api.onFilterChanged();
    }

    

    // EXPORT

    public exportRows(format: string, allRows?: boolean) {
        const { api } = this.gridOptions;
        let exportCaller: () => void;

        // Get columns to export
        const columnDefs = this.gridOptions.columnApi.getAllDisplayedColumns().map(c => c.colDef);
        const columnKeys: string[] = _.filter(columnDefs, c => !c.suppressExport).map(c => c.field);

        switch (format) {
            case "pdf":
                exportCaller = () => {
                    PrintPdfDoc(this.gridOptions, this.exportFilename + ".pdf");
                }
                break;
            case "csv":
                exportCaller = () => api.exportDataAsCsv({
                    fileName: this.exportFilename + ".csv",
                    columnKeys: columnKeys
                });
                break;
            case "excel":
                exportCaller = () => api.exportDataAsExcel({
                    fileName: this.exportFilename + ".xlsx",
                    sheetName: this.exportFilename,
                    columnKeys: columnKeys,
                    processCellCallback: ({ value, column }) => {
                        if (value && column.colDef) {
                            const columnType = column.colDef.soeColumnType;
                            const isTime: boolean = columnType && (columnType === "time" || columnType === "timeSpan");

                            if (!!value.toFormattedDate && typeof value.toFormattedDate === "function") {
                                return isTime ? (value as Date).toFormattedTime(true) : (value as Date).toFormattedDate("YYYY-MM-DD");
                            } else if (isTime) {
                                //should have excelTime class on it to get right datatype in excel
                                if (column.colDef['minutesToTimeSpan'])
                                    value = CalendarUtility.minutesToTimeSpan(value);
                                return CalendarUtility.parseTimeSpan(value, true, true, true, true);
                            }
                        }

                        return value;
                    }
                });
                break;
            default:
                throw "Unsupported export format";
        }

        let filterModel: any = null;
        if (allRows) {
            filterModel = api.getFilterModel();
            api.setFilterModel(null);
        }

        exportCaller();

        if (filterModel) {
            api.setFilterModel(filterModel);
        }
    }

    // SAVE STATE

    public saveDefaultState(callback: (name: string, data: string) => ng.IPromise<any>): ng.IPromise<any> {
        const { columnApi } = this.gridOptions;

        const state = columnApi.getColumnState();
        return callback(this.getNormalizedName(), JSON.stringify(state));
    }

    public restoreDefaultState(callback: (name: string) => ng.IPromise<string>) {
        // First check if there is a default (sys) state
        let sysExists: boolean = false;
        if (callback) {
            callback(this.getNormalizedName()).then((state) => {
                if (state && state.length > 0) {
                    sysExists = true;
                    this.setColumnStateOnAll(JSON.parse(state));
                };
            });
        }

        // Next get full grid state
        if (!sysExists && this.defaultGridState) {
            this.setColumnStateOnAll(this.defaultGridState);
        }
    }

    private setColumnWidthFromStateWorkAround(state: any[]) {
        if (state) {
            let columnDefs = this.gridOptions.api.getColumnDefs();
            state.forEach(x => {
                const colDef = columnDefs[x.colId]
                if (colDef && x.width) {
                    colDef.width = x.width;
                }
            });

            this.gridOptions.api.setColumnDefs(columnDefs)
        }
    }

    private setColumnStateOnAll(state: any) {
        const { api, columnApi, alignedGrids } = this.gridOptions;

        columnApi.applyColumnState({
            state: state,
            applyOrder: true
        });

        (alignedGrids as any[]).map(g => g.columnApi).forEach(capi => capi.applyColumnState(state));

        this.setColumnWidthFromStateWorkAround(state);
        this.fixDateFilterInputs(api, columnApi);
    }

    public deleteDefaultState(callback: (name: string) => ng.IPromise<any>) {
        callback(this.getNormalizedName()).then((result) => {
            if (result.success) {
                this.restoreDefaultState(null);
            }
        });
    }

    public saveState(callback: (name: string, data: string) => ng.IPromise<any>): ng.IPromise<any> {
        const { columnApi } = this.gridOptions;
        const state = columnApi.getColumnState();

        return callback(this.getNormalizedName(), JSON.stringify(state));
    }

    public restoreState(callback: (name: string) => ng.IPromise<string>, saveCurrentAsDefault: boolean) {
        // No need to restore state if user can't save any
        if (!this.enableGridMenu) {
            return;
        }

        callback(this.getNormalizedName()).then((state) => {
            // Save default state to be able to restore to that in method restoreDefaultState
            const { api, columnApi } = this.gridOptions;
            if (state && state.length > 0) {
                const fn = () => {
                    

                    if (columnApi) {
                        if (saveCurrentAsDefault) {
                            this.defaultGridState = columnApi.getColumnState();
                        }

                        try {
                            this.setColumnStateOnAll(JSON.parse(state));
                            if (!this.ignoreResizeToFit)
                                this.sizeColumnToFit();

                            this.userGridStateRestored(true);
                        }
                        catch (e) {
                            console.warn("AgGridOptions: Couldn't restore state. Probably trying to restore a ui-grid state", e);
                        }
                    }
                    else
                        this.$timeout(fn, 100);
                };
                fn();
            } else {
                this.fixDateFilterInputs(api, columnApi);
                this.userGridStateRestored(false);
            }
        });
    }

    public deleteState(callback: (name: string) => ng.IPromise<any>, callback2: (name: string) => ng.IPromise<any>) {
        callback(this.getNormalizedName()).then((result) => {
            if (result.success) {
                this.restoreDefaultState(callback2);
            }
        });
    }

    public isCellEditable(row: any, colDef: any): boolean {
        if (colDef.editable === undefined || colDef.editable === null) {
            return false;
        }

        if (typeof colDef.editable === "function") {
            const nodeId = this.getRowNodeIdFromData(row);
            const node = this.gridOptions.api.getRowNode(nodeId);
            const column = this.gridOptions.columnApi.getColumn(colDef);
            const { api, columnApi, context } = this.gridOptions;

            return colDef.editable({ node, column, colDef, api, columnApi, context });
        }

        return colDef.editable;
    }

    public setName(name: string) {
        this.name = name;
    }

    public getName(): string {
        return this.name;
    }

    public getNormalizedName() {
        return this.name.replace(/\./g, '_');
    }

    public setStandardSubscriptions(rowSelectionCallback: (rows: any[]) => void) {
        const events: GridEvent[] = [];
        if (rowSelectionCallback) {
            events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (rows: any[]) => {
                rowSelectionCallback(rows);
            }));
        }

        this.subscribe(events);
    }

    // EVENTS
    private events: GridEvent[];
    public subscribe(events: GridEvent[]) {
        if (!this.events)
            this.events = events;
        else
            events.forEach(x => this.events.push(x));
    }

    private userGridStateRestored(hasState: boolean) {
        const func = this.getEventFunction(SoeGridOptionsEvent.UserGridStateRestored);
        if (func)
            func(hasState);
    }

    private beginCellEdit(rowEntity, colDef) {
        const funcs = this.getEventFunctions(SoeGridOptionsEvent.BeginCellEdit);
        funcs.forEach(f => f(rowEntity, colDef));
    }

    private afterCellEdit(rowEntity, colDef, newValue, oldValue) {
        const changed = colDef.comparator
            ? colDef.comparator(newValue, oldValue)
            : newValue != oldValue;

        if (changed)
            rowEntity.isModified = true;
        const funcs = this.getEventFunctions(SoeGridOptionsEvent.AfterCellEdit);
        funcs.forEach(f => f(rowEntity, colDef, newValue, oldValue));
    }

    public cancelCellEdit(rowEntity, colDef) {
        const func = this.getEventFunction(SoeGridOptionsEvent.CancelCellEdit);
        if (func)
            func(rowEntity, colDef);
    }

    private cellFocused(rowindex, column, rowPinned, forceBrowserFocus) {
        const funcs = this.getEventFunctions(SoeGridOptionsEvent.CellFocused);
        funcs.forEach(f => f(rowindex, column, rowPinned, forceBrowserFocus));
    }

    private filterChanged() {
        const func: any = this.getEventFunction(SoeGridOptionsEvent.FilterChanged);
        if (func)
            func();
    }

    private rowSelectionChanged(row) {
        const func: any = this.getEventFunction(SoeGridOptionsEvent.RowSelectionChanged);
        if (func)
            func(row);
    }

    private rowSelectionChangedBatch(rows) {
        var func: any = this.getEventFunction(SoeGridOptionsEvent.RowSelectionChangedBatch);
        if (func)
            func(rows);
    }

    private rowsVisibleChanged(rows) {
        const func: any = this.getEventFunction(SoeGridOptionsEvent.RowsVisibleChanged);
        if (func)
            func(rows);
    }

    private rowDoubleClicked(row) {
        const func: any = this.getEventFunction(SoeGridOptionsEvent.RowDoubleClicked);
        if (func)
            func(row);
    }

    private rowClicked(row) {
        const func: any = this.getEventFunction(SoeGridOptionsEvent.RowClicked);
        if (func)
            func(row);
    }

    private columnVisible(column) {
        const func: any = this.getEventFunction(SoeGridOptionsEvent.ColumnVisible);
        if (func)
            func(column);
    }

    private isRowSelectable(row) {
        const func: any = this.getEventFunction(SoeGridOptionsEvent.IsRowSelectable);
        if (func)
            return func(row);
        else
            return true;
    }

    private isRowMaster(row) {
        const func: any = this.getEventFunction(SoeGridOptionsEvent.IsRowMaster);
        if (func)
            return func(row);
        else
            return true;
    }

    private navigate(newRowCol, oldRowCol) {
        var func: any = this.getEventFunction(SoeGridOptionsEvent.Navigate);
        if (func)
            func(newRowCol, oldRowCol);
    }

    export(row, col, input) {
        const func: any = this.getEventFunction(SoeGridOptionsEvent.Export);
        if (func)
            return func(row, col, input);
        else
            return input;
    }

    private renderingComplete() {
        const func: any = this.getEventFunction(SoeGridOptionsEvent.RenderingComplete);
        if (func)
            func();
    }

    private rowsRendered() {
        const func: any = this.getEventFunction(SoeGridOptionsEvent.RowsRendered);
        if (func)
            func();
    }

    private columnRowGroupChanged(params) {
        const func: any = this.getEventFunction(SoeGridOptionsEvent.ColumnRowGroupChanged);
        if (func)
            func(params);
    }

    private getEventFunction(ev: SoeGridOptionsEvent): any {
        let ret = null;

        if (this.events) {
            _.forEach(this.events, (event) => {
                if (event.event == ev) {
                    ret = event.func;
                }
            });
        }

        return ret;
    }

    private getEventFunctions(ev: SoeGridOptionsEvent): any {
        if (this.events)
            return this.events.filter(e => e.event === ev).map(e => e.func);

        return [];
    }

    // HELP METHODS

    private propegateNodeIdToData(rowNode: any) {
        if (rowNode.group) {
            return;
        }
        rowNode.data["ag_node_id"] = rowNode.id;
    }

    private getRowNodeIdFromData(rowData: any): string {
        return rowData["ag_node_id"];
    }

    private setCellCancelledToken(rowNode: any): void {
        rowNode["ag_cell_cancelled"] = true;
    }

    private hasCellCancelledToken(rowNode: any): any {
        return !!rowNode["ag_cell_cancelled"];
    }

    private deleteCellCancelledToken(rowNode: any): void {
        delete rowNode["ag_cell_cancelled"];
    }

    private runGotoNextCell(params): { rowIndex: number, column: any } {
        if (!!this.customTabToCellHandler && typeof this.customTabToCellHandler === "function") {
            return this.customTabToCellHandler(params);
        }

        return params.nextCellPosition;
    }

    private addCellValueChangedEventListener(columnDef: any, onChanged?: CellChangedCallback) {
        if (onChanged) {
            columnDef.onCellValueChanged = ({ data, newValue, oldValue, colDef }) => {
                onChanged({ data, newValue, oldValue, field: colDef.field, soeData: data } as ISoeCellValueChanged);
            }
        }
    }

    private updateGridHeightBasedOnNbrOfRows(rowCountToDisplay: number, api) {
        if (_.isNil(rowCountToDisplay))
            return;

        this.minRowsToShow = rowCountToDisplay;

        const gridElement = this.getHostDiv(api) as HTMLElement;
        if (!gridElement)
            return;

        let { rowHeight, headerHeight, floatingFiltersHeight } = this.gridOptions;
        rowHeight = rowHeight || 25;
        headerHeight = headerHeight || 25;
        floatingFiltersHeight = floatingFiltersHeight || 20;

        const expectedGridHeight = (this.minRowsToShow * rowHeight + headerHeight + floatingFiltersHeight).toString() + "px";
        if (gridElement.style.height !== expectedGridHeight) {
            gridElement.style.height = expectedGridHeight;
        }
    }

    public updateGridHeightBasedOnActualRows(defaultHeight?: number) {
        if (!defaultHeight)
            defaultHeight = 29;

        // Set grid total height
        let groupHeaderHeight: number = this.gridOptions.groupHeaderHeight || defaultHeight;
        let headerHeight: number = this.gridOptions.headerHeight || defaultHeight;
        let filterHeight: number = 32;
        let rowHeight: number = this.gridOptions.rowHeight || defaultHeight;

        // Add one extra row height. Otherwise in some resolutions the horizontal scroll will cover the last row.
        let totalHeight: number = groupHeaderHeight + headerHeight + filterHeight + (rowHeight * 2);

        if (this.gridOptions.api) {
            this.gridOptions.api.forEachNode((rowNode: RowNode) => {
                totalHeight += rowNode.rowHeight;
                if (rowNode.expanded && rowNode.detailNode)
                    totalHeight += rowNode.detailNode.rowHeight;
            });
            this.updateGridHeight(totalHeight);
        }
    }

    private updateGridHeight(height: number) {
        const gridElement = this.getHostDiv(this.gridOptions.api) as HTMLElement;
        const expectedGridHeight = height.toString() + "px";

        if (gridElement.style.height !== expectedGridHeight) {
            gridElement.style.height = expectedGridHeight;
        }
    }

    public customCreateDefaultColumnMenu = (agGridDefaultItems?: string[]): MenuItem[] => {
        return agGridDefaultItems;
    }

    public fixDateFilterInputs(api, columnApi) {
        if (columnApi == null)
            return;

        const columns = columnApi.getAllColumns();
        const dateFilterColumns = _.filter(columns, c => c.colDef.filter === "agDateColumnFilter");
        const columnFilters = _.map(dateFilterColumns, c => api.getFilterInstance(c));

        _.each(columnFilters, f => {
            if (f.eGui) {
                const inputs = f.eGui.querySelectorAll('input[type=date].ag-input-field-input');
                _.each(inputs, i => i.setAttribute("max", "9999-12-31"));
            }
        });

        const headers = this.getHostDiv(api).querySelectorAll("input[type=date].ag-input-field-input");

        _.each(headers, i => i.setAttribute("max", "9999-12-31"));
    }
}

export class TypeAheadOptionsAg {
    buttonConfig?: {
        icon: string,
        tooltipKey: string,
        click: DataCallback
    };
    source: (filter: string) => any[];
    minLength?: number;
    delay?: number;
    displayField?: string;
    dataField?: string;
    autoSelect?: boolean;
    updater: DataCallback;
    allowNavigationFromTypeAhead?: (value: any, data: any, colDef: any) => boolean = () => true;
    useScroll?: boolean;
}

export class AgGridUtility {
    static groupComparator(nodeA, nodeB, valueA, valueB) {
        let colDef;
        let isTimeSpan: boolean = false;
        let isDate: boolean = false;
        let isNumber: boolean = false;

        if (nodeA && nodeA.rowGroupColumn && nodeA.rowGroupColumn.colDef)
            colDef = nodeA.rowGroupColumn.colDef;
        else if (nodeB && nodeB.rowGroupColumn && nodeB.rowGroupColumn.colDef)
            colDef = nodeB.rowGroupColumn.colDef;

        if (colDef && colDef.soeType) {
            if (colDef.soeType === Constants.GRID_COLUMN_TYPE_TIMESPAN)
                isTimeSpan = true;
            else if (colDef.soeType === Constants.GRID_COLUMN_TYPE_DATE)
                isDate = true;
            else if (colDef.soeType === Constants.GRID_COLUMN_TYPE_NUMBER)
                isNumber = true;
        }

        if (isTimeSpan) {
            // TimeSpan
            const minutesA: number = CalendarUtility.timeSpanToMinutes(valueA);
            const minutesB: number = CalendarUtility.timeSpanToMinutes(valueB);
            if (minutesA === minutesB)
                return 0;
            else if (_.isNil(minutesA) || _.isNaN(minutesA))
                return 1;
            else if (_.isNil(minutesB) || _.isNaN(minutesB))
                return -1;
            else
                return minutesA > minutesB ? 1 : -1;
        } else if (isDate) {
            // Date
            const dateA: Date = CalendarUtility.convertToDate(valueA, CoreUtility.languageDateFormat);
            const dateB: Date = CalendarUtility.convertToDate(valueB, CoreUtility.languageDateFormat);

            if (dateA === dateB)
                return 0;
            else
                return (dateA > dateB) ? 1 : -1;
        } else if (isNumber) {
            // Number
            if (valueA === valueB)
                return 0;
            else if (_.isNil(valueA) || _.isNaN(valueA))
                return 1;
            else if (_.isNil(valueB) || _.isNaN(valueB))
                return -1;
            else
                return parseFloat(valueA) > parseFloat(valueB) ? 1 : -1;
        } else {
            // String
            if (valueA === valueB)
                return 0;
            else
                return (valueA > valueB) ? 1 : -1;
        }
    }
}

class SelectValuesContainer {
    constructor(private getApi: () => any, private valuesField: FieldOrEvaluator<any[]>, private displayField?: string) {
    }

    public get length() {
        const values = this.getDisplayabeValues();

        return values.length;
    }

    private getDisplayabeValues(): string[] {
        const data = this.getCurrentData();
        const values = ObjectFieldHelper.getFieldFrom(data, this.valuesField);

        return this.displayField ? values.map(v => v[this.displayField]) : values;
    }

    private getCurrentData(): any {
        const api = this.getApi();
        const { rowIndex } = api.getFocusedCell();
        const node = api.getDisplayedRowAtIndex(rowIndex);

        return node.data;
    }
}