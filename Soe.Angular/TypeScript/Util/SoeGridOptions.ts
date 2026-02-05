import { CalendarUtility } from "./CalendarUtility";
import { NumberUtility } from "./NumberUtility";
import { SoeGridOptionsEvent } from "./Enumerations";
import { Constants } from "./Constants";

export interface ISoeGridOptionsFactory {
    create(name: string): ISoeGridOptions
}

export interface ISoeGridOptions {

    exporterCsvFilename: string;
    exporterPdfFilename: string;
    exporterPdfHeader: UIGridExportHeader;

    enableGridMenu: boolean;
    enableColumnMenus: boolean;
    enableColumnResizing: boolean;

    enableFiltering: boolean;
    enableSorting: boolean;

    showColumnFooter: boolean;
    showGridFooter: boolean;

    enableHorizontalScrollbar: boolean;
    enableVerticalScrollbar: boolean;

    enableRowSelection: boolean;
    enableRowHeaderSelection: boolean;
    enableFullRowSelection: boolean;
    enableSelectionBatchEvent: boolean;
    enableDoubleClick: boolean;

    useExternalFiltering: boolean;

    multiSelect: boolean;
    noUnselect: boolean;
    rowSelectDisabledProperty: string;
    rowSelectDisabledPropertyInvert: boolean;

    rowTemplate: string;

    enableExpandable: boolean;
    enableExpandableRowHeader: boolean;
    expandableRowTemplate: string;
    expandableRowHeight: number;
    expandableRowScope: any;

    useCustomExporting: boolean;

    nbrOfColumns(): number;
    addColumn(columnDef: any);
    removeColumn(index: number, nbrOfColumns?: number);
    hideColumn(name: string);
    showColumn(name: string);
    getColumnDefs(): uiGrid.IColumnDef[];
    clearColumnDefs();
    groupColumns(columns: string[]);
    enableCellEdit(index: number, enable: boolean);
    visibleColumn(index: number, visible: boolean);
    isColumnVisible(index: number): boolean;

    addColumnBool(field: string, displayName: string, width: string, enableCellEdit?: boolean, clickEvent?: string, clickEventField?: string, disabledField?: string, termIndeterminate?: boolean): uiGrid.IColumnDef;
    addColumnActive(field: string, displayName?: string, width?: string, changeEventHandlerName?: string): uiGrid.IColumnDef;
    addColumnIcon(field: string, icon: string, toolTip: string, clickEvent: string, showIconField?: string, showIconFunction?: string, displayName?: string, width?: string, enableHiding?: boolean, enableResizing?: boolean, ctrlName?: string, isSubgrid?: boolean, tooltipField?: string): uiGrid.IColumnDef;
    addColumnShape(field: string, displayName?: string, width?: string, shapeField?: string, shape?: string, toolTipField?: string, toolTip?: string, showIconField?: string, showIconFunction?: string, ctrlName?: string, isSubgrid?: boolean, attestGradient?: boolean): uiGrid.IColumnDef;
    addColumnEdit(toolTip: string, clickEvent?: string, ctrlName?: string, isSubgrid?: boolean): uiGrid.IColumnDef;
    addColumnDelete(toolTip: string, onDeleteEvent?: string, ctrlName?: string, isSubgrid?: boolean, showIconField?: string, showIconFunction?: string): uiGrid.IColumnDef;
    addColumnPdf(toolTip: string): uiGrid.IColumnDef;
    addColumnIsModified(field?: string, displayName?: string, width?: string, clickEvent?: string, ctrlName?: string): uiGrid.IColumnDef;

    addColumnText(field: string, displayName: string, width: string, enableHiding?: boolean, toolTipField?: string, toolTip?: string, className?: string, classFunction?: string, shapeValueField?: string, shape?: string, buttonIcon?: string, buttonFunction?: string, ctrlName?: string, showButtonField?: string): uiGrid.IColumnDef;
    addColumnHtml(field: string, displayName: string, width: string, enableHiding?: boolean): uiGrid.IColumnDef;
    addColumnNumber(field: string, displayName: string, width: string, enableHiding?: boolean, decimals?: number, type?: string, disabledField?: string, disabledFieldFunction?: string, onChangeEvent?: string, alignLeft?: boolean, toolTipField?: string, toolTip?: string, className?: string, classFunction?: string): uiGrid.IColumnDef;
    addColumnSelect(field: string, displayName: string, width: string, selectOptions: any[], enableHiding?: boolean, enableCellEdit?: boolean, fieldValue?: string, dropdownIdLabel?: string, dropdownValueLabel?: string, onChangeEvent?: string, ctrlName?: string, collectionField?: string, shapeValueField?: string, shape?: string): uiGrid.IColumnDef;
    addColumnMultiSelect(field: string, displayName: string, width: string, selectOptions: any[], enableHiding?: boolean, enableCellEdit?: boolean, fieldValue?: string, dropdownIdLabel?: string, dropdownValueLabel?: string, placeholder?: string): uiGrid.IColumnDef;
    addColumnDateTime(field: string, displayName: string, width: string, enableHiding?: boolean, cellFilter?: string, toolTipField?: string, toolTip?: string, className?: string, classFunction?: string): uiGrid.IColumnDef;
    addColumnDate(field: string, displayName: string, width: string, enableHiding?: boolean, cellFilter?: string, toolTipField?: string, toolTip?: string, className?: string, classFunction?: string): uiGrid.IColumnDef;
    addColumnTime(field: string, displayName: string, width: string, enableHiding?: boolean, cellFilter?: string, columnDefType?: string): uiGrid.IColumnDef;
    addColumnTimeSpan(field: string, displayName: string, width: string, enableHiding?: boolean, treatUndefinedAsEmpty?: boolean): uiGrid.IColumnDef;
    addColumnTypeAhead(field: string, typeAheadOptions: TypeAheadOptions, displayName?: string, width?: string, minChars?: number, wait?: number): uiGrid.IColumnDef;

    addGridMenuItem(menuItem: uiGrid.IMenuItem);

    setMinRowsToShow(nbrOfRows: number);

    refreshGrid();
    refreshRows();
    refreshColumns();
    resize();
    clearData();
    getData();
    setData(rows: any[]);
    addRow(row: any, setFocus?: boolean, focusColumnNumber?: number);
    deleteRow(row: any);
    setRowInvisible(gridRow: any);
    clearRowInvisible(gridRow: any);

    selectAllRows();
    selectRow(row: any, forceGridUpdate?: boolean);
    selectRowByVisibleIndex(index: number, forceGridUpdate?: boolean);
    unSelectRow(row: any, forceGridUpdate?: boolean);
    clearSelectedRows();

    findInData<T>(predicate: (item: any) => boolean): T;
    findAllInData<T>(predicate: (item: any) => boolean): T[];
    getCurrentRow(): any;
    getCurrentRowCol(): uiGrid.cellNav.IRowCol<any>;
    scrollToFocus(row: any, columnIndex: number, refocusCell?: boolean);
    focusRowByIndex(rowIndex: number, columnIndex: number);
    focusRowByRow(row: any, columnIndex: number);
    focusColumn(columnIndex: number);
    refocusCell();
    getColumnIndex(field: string);
    getCurrentCellValue(): string;
    sortFirst(sortprop?: string);
    sortUp(sortprop?: string);
    sortDown(sortprop?: string);
    sortLast(sortprop?: string);
    reNumberRows(sortprop?: string);

    getSelectedRows(): any[];
    getSelectedCount(): number;
    getSelectedIds(idField: string): number[];

    getDirectiveName(): string;

    getFilteredRows(): any[];

    clearFilters();

    saveDefaultState(callback: (name: string, data: string) => ng.IPromise<any>): ng.IPromise<any>;
    restoreDefaultState(callback: (name: string) => ng.IPromise<string>);
    deleteDefaultState(callback: (name: string) => ng.IPromise<any>);
    saveState(callback: (name: string, data: string) => ng.IPromise<any>): ng.IPromise<any>;
    restoreState(callback: (name: string) => ng.IPromise<string>, saveCurrentAsDefault: boolean);
    deleteState(callback: (name: string) => ng.IPromise<any>, callback2: (name: string) => ng.IPromise<any>);

    cancelCellEdit(rowEntity: any, colDef: any);

    subscribe(events: GridEvent[]);

    enableDynamicHeight();
    updateDynamicHeight(row: any);

    enableAutoHeight();
}

export class GridOptionsFactory implements ISoeGridOptionsFactory {

    //@ngInject
    constructor(private $timeout: ng.ITimeoutService, private uiGridConstants: uiGrid.IUiGridConstants, private gridControllerName = 'ctrl') { }

    create(name: string) {
        return new SoeGridOptions(name, this.$timeout, this.uiGridConstants, this.gridControllerName);
    }
}

export class SoeGridOptions implements ISoeGridOptions {
    constructor(private name: string, private $timeout: ng.ITimeoutService, private uiGridConstants: uiGrid.IUiGridConstants, public gridControllerName = 'ctrl') {
    }

    public static create(name: string, $timeout: ng.ITimeoutService, uiGridConstants: uiGrid.IUiGridConstants, enableExpansion?: boolean, expansionTemplate?: string, gridControllerName = 'ctrl') {
        let options = new SoeGridOptions(name, $timeout, uiGridConstants, gridControllerName);

        if (enableExpansion) {
            options.enableExpandable = true;
            options.enableExpandableRowHeader = true;
            options.expandableRowTemplate = expansionTemplate;
            options.expandableRowHeight = 150;
            options.expandableRowScope = {};         
        }
        return options;
    }

    public gridOptions: any = {
        //dummy column added because of ui-grid not showing filter when selection is active and no columns is present (not calulating height correctly)
        //hidden in renderingComplete
        columnDefs: [],
        data: [],

        exporterCsvFilename: undefined,
        exporterPdfFilename: undefined,
        exporterPdfHeader: undefined,
        exporterPdfFooter(currentPage, pageCount) {
            return { text: "{0} ({1})".format(currentPage.toString(), pageCount.toString()), style: 'footerStyle' };
        },
        exporterSuppressColumns: ['icon', 'edit', 'delete', 'pdf'],
        enableGridMenu: true,
        exporterMenuExcel: false,
        enableCellSelection: false,
        showGridFooter: true,
        rowTemplate: '<div data-ng-repeat="(colRenderIndex, col) in colContainer.renderedColumns track by col.uid" class="ui-grid-cell" data-ng-class="{ \'ui-grid-row-header-cell\': col.isRowHeader }" ui-grid-cell data-ng-dblclick="grid.appScope.' + this.gridControllerName + '.edit(row.entity)" data-ng-click="grid.appScope.' + this.gridControllerName + '.rowClicked(row.entity)"></div>',
        enableExpandable: false,
        enableExpandableRowHeader: false,
        enableOnDblClickExpand: false,
        enableCellEdit: false,
        enableCellEditOnFocus: true,
        rowSelectDisabledProperty: "isSelectDisable",
        rowSelectDisabledPropertyInvert: false,
        deltaRowDataMode: true,

        cellEditableCondition: function ($scope) {
            if ($scope.row.entity.isReadOnly) {
                return $scope.row.entity.isReadOnly === false;
            }
            else {
                return true;
            }
        },

        isRowSelectable: function (row) {
            if (this.rowSelectDisabledPropertyInvert === true) {
                if (row.entity[this.rowSelectDisabledProperty] === false) {
                    return false;
                }
                else {
                    return true;
                }
            }
            else {
                if (row.entity[this.rowSelectDisabledProperty] === true) {
                    return false;
                }
                else {
                    return true;
                }
            }
        },

        exporterFieldCallback: (grid, row, col, input) => {
            if (this.gridOptions.useCustomExporting)
                return this.export(row, col, input);
            else
                return input;
        },

        onRegisterApi: (gridApi) => {
            this.onRegisterApi(gridApi, this);
        },
    };

    private gridApi: uiGrid.IGridApi;
    private defaultGridState: any;

    set exporterCsvFilename(value: string) { this.gridOptions.exporterCsvFilename = value; }
    set exporterPdfFilename(value: string) { this.gridOptions.exporterPdfFilename = value; }
    set exporterPdfHeader(value: UIGridExportHeader) { this.gridOptions.exporterPdfHeader = value; }

    get enableGridMenu() { return this.gridOptions.enableGridMenu; }

    set enableGridMenu(value: boolean) { this.gridOptions.enableGridMenu = value; }
    set enableColumnMenus(value: boolean) { this.gridOptions.enableColumnMenus = value; }
    set enableColumnResizing(value: boolean) { this.gridOptions.enableColumnResizing = value; }

    set enableFiltering(value: boolean) { this.gridOptions.enableFiltering = value; }

    set enableSorting(value: boolean) { this.gridOptions.enableSorting = value; }
    set enableDoubleClick(value: boolean) {
        if (value) {
            this.gridOptions.rowTemplate = '<div data-ng-repeat="(colRenderIndex, col) in colContainer.renderedColumns track by col.uid" class="ui-grid-cell" data-ng-class="{ \'ui-grid-row-header-cell\': col.isRowHeader }" ui-grid-cell data-ng-dblclick="grid.appScope.' + this.gridControllerName + '.edit(row.entity)"></div>';
        } else {
            this.gridOptions.rowTemplate = '<div data-ng-repeat="(colRenderIndex, col) in colContainer.renderedColumns track by col.uid" class="ui-grid-cell" data-ng-class="{ \'ui-grid-row-header-cell\': col.isRowHeader }" ui-grid-cell data-ng-click="grid.appScope.' + this.gridControllerName + '.rowClicked(row.entity)"></div>';
        }
    }
    set showColumnFooter(value: boolean) { this.gridOptions.showColumnFooter = value; }
    set showGridFooter(value: boolean) { this.gridOptions.showGridFooter = value; }

    set enableHorizontalScrollbar(value: boolean) { this.gridOptions.enableHorizontalScrollbar = value ? this.uiGridConstants.scrollbars.ALWAYS : this.uiGridConstants.scrollbars.NEVER; }
    set enableVerticalScrollbar(value: boolean) { this.gridOptions.enableVerticalScrollbar = value ? this.uiGridConstants.scrollbars.ALWAYS : this.uiGridConstants.scrollbars.NEVER; }

    set useExternalFiltering(value: boolean) { this.gridOptions.useExternalFiltering = value; }

    set enableRowSelection(value: boolean) { this.gridOptions.enableRowSelection = value; }
    set enableRowHeaderSelection(value: boolean) { this.gridOptions.enableRowHeaderSelection = value; }
    set enableFullRowSelection(value: boolean) { this.gridOptions.enableFullRowSelection = value; }
    set enableSelectionBatchEvent(value: boolean) { this.gridOptions.enableSelectionBatchEvent = value; }
    set multiSelect(value: boolean) { this.gridOptions.multiSelect = value; }
    set noUnselect(value: boolean) { this.gridOptions.noUnselect = value; } //rowSelectDisabledPropertyInvert
    set rowSelectDisabledProperty(value: string) { this.gridOptions.rowSelectDisabledProperty = value };
    set rowSelectDisabledPropertyInvert(value: boolean) { this.gridOptions.rowSelectDisabledPropertyInvert = value };

    set rowTemplate(value: string) { this.gridOptions.rowTemplate = value; }

    set enableExpandable(value: boolean) { this.gridOptions.enableExpandable = value; }
    set enableExpandableRowHeader(value: boolean) { this.gridOptions.enableExpandableRowHeader = value; }
    set expandableRowTemplate(value: string) { this.gridOptions.expandableRowTemplate = value; }
    set expandableRowHeight(value: number) { this.gridOptions.expandableRowHeight = value; }
    set expandableRowScope(value: any) { this.gridOptions.expandableRowScope = value; }

    set useCustomExporting(value: any) { this.gridOptions.useCustomExporting = value; }

    onRegisterApi(gridApi, gridOptions) {
        this.gridApi = gridApi;
        this.enableDoubleClick = true;

        // cellNav
        if (this.gridApi.cellNav) {
            if (this.gridOptions.enableFullRowSelection != false) {
                // Work around to get row selected when using cellNav
                gridApi.cellNav.on.navigate(null, (newRowCol) => {
                    if (gridOptions.gridApi.selection)//crashes sometimes if there is no selection
                        gridOptions.gridApi.selection.selectRow(newRowCol.row.entity);
                });
            }
        }

        // edit
        if (this.gridApi.edit) {
            this.gridApi.edit.on.beginCellEdit(null, (rowEntity, colDef) => {
                this.beginCellEdit(rowEntity, colDef);
            });

            this.gridApi.edit.on.afterCellEdit(null, (rowEntity, colDef, newValue, oldValue) => {
                this.afterCellEdit(rowEntity, colDef, newValue, oldValue);
            });

            this.gridApi.edit.on.cancelCellEdit(null, (rowEntity, colDef) => {
                this.cancelCellEdit(rowEntity, colDef);
            });
        }

        /*if (this.gridApi.core && this.gridApi.core.on) {
            this.gridApi.core.on.sortChanged(null, (grid, sortColumns) => {
                var groupRows = _.filter(this.gridApi.core.getVisibleRows(grid), r => r.groupHeader);
                if (groupRows.length > 0) {
                    //console.log("group 1", groupRows);
                    groupRows = _.sortBy(groupRows, [function (o) { return o.entity['$$' + sortColumns[0].uid].value; }]);
                    this.gridApi.grouping.clearGrouping();
                    this.gridOptions.data = groupRows;
                    console.log("group 2", groupRows, this.gridApi.core.getVisibleRows(grid));
                    //this.gridApi.core.notifyDataChange(this.uiGridConstants.dataChange.ALL);
                }
            });
        }*/

        // expandable
        if (this.gridApi.expandable && this.gridApi.expandable.on) {
            this.gridApi.expandable.on.rowExpandedStateChanged(null, (row) => {
                this.rowExpandedStateChanged(row);
            });
        }

        // filtered
        if (this.gridApi.core && this.gridApi.core.on) {
            this.gridApi.core.on.rowsRendered(null, () => {
                this.rowsVisibleChanged(this.gridApi.core.getVisibleRows(this.gridApi.grid));
            });
        }

        // selection
        if (this.gridApi.selection) {
            this.gridApi.selection.on.rowSelectionChanged(null, (row) => {
                this.rowSelectionChanged(row);
            });

            this.gridApi.selection.on.rowSelectionChangedBatch(null, (rows) => {
                this.rowSelectionChangedBatch(rows);
            });
        }

        // navigate
        if (this.gridApi.cellNav) {
            this.gridApi.cellNav.on.navigate(null, (newRowCol, oldRowCol) => {
                this.navigate(newRowCol, oldRowCol);
            });
        }

        // render
        if (this.gridApi.core && this.gridApi.core.on) {
            gridApi.core.on.renderingComplete(null, () => {
                this.renderingComplete();
            });
        }

        if (this.gridApi.core && this.gridApi.core.on) {
            gridApi.core.on.rowsRendered(null, () => {
                this.rowsRendered();
            });
        }

        //Filtering
        if (this.gridApi.core && this.gridApi.core.on) {
            gridApi.core.on.filterChanged(null, () => {
                console.log("filter changed");
                this.filterChanged();
            });
        }

        // Property disableScrolling is added in uiGridTypeaheadEditor
        if (this.gridApi.grid && this.dynamicHeight) {
            this.gridApi.grid['disableScrolling'] = true;
        }
    }

    // COLUMNS

    // Width can be defined the following ways:
    // Fixed size (in pixels): "100"
    // Percentage (of available width): "50%"
    // Star(s): "*", "**", etc.
    //   Two columns with "*" and "**" will take up 33% and 66%.
    // Passing null as the width parameter will be same as one star
    // Read more here: https://github.com/angular-ui/ui-grid/wiki/Defining-columns

    // Another good reference page: http://brianhann.com/6-ways-to-take-control-of-how-your-ui-grid-data-is-displayed/

    public nbrOfColumns(): number {
        return this.gridOptions.columnDefs.length;
    }

    // Common
    private createColumnDef(field: string, displayName: string, width: string, enableHiding: boolean = false, enableColumnResizing: boolean = true, enableFiltering: boolean = true, enableSorting: boolean = true, enableColumnMenu: boolean = true, enableCellEdit: boolean = false): uiGrid.IColumnDef {
        var columnDef = {
            field: field,
            displayName: displayName,
            width: width,
            enableHiding: enableHiding,
            enableColumnResizing: enableColumnResizing,
            enableFiltering: enableFiltering,
            enableSorting: enableSorting,
            enableColumnMenu: enableColumnMenu,
            enableCellEdit: enableCellEdit,
            enableCellEditOnFocus: true,
            cellTooltip: true
        }

        return columnDef;
    }

    public addColumn(columnDef: uiGrid.IColumnDef) {
        this.gridOptions.columnDefs.push(columnDef);
    }

    public removeColumn(index: number, nbrOfColumns?: number) {
        if (!nbrOfColumns)
            nbrOfColumns = 1;
        this.gridOptions.columnDefs.splice(index, nbrOfColumns);
        this.gridOptions.columnDefs[0].hide;
    }

    public hideColumn(name: string) {
        var col: uiGrid.IColumnDef = _.find(this.gridOptions.columnDefs, c => c['name'] === name);
        if (col) {
            col.visible = false;
            this.refreshColumns();
        }
    }

    public showColumn(name: string) {
        var col: uiGrid.IColumnDef = _.find(this.gridOptions.columnDefs, c => c['name'] === name);
        if (col) {
            col.visible = true;
            this.refreshColumns();
        }
    }

    public getColumnDefs(): uiGrid.IColumnDef[] {
        return this.gridOptions.columnDefs;
    }

    public clearColumnDefs() {
        this.gridOptions.columnDefs = [];
    }

    public groupColumns(columns: string[]) {
        if (this.gridApi) {
            this.gridApi.grouping.clearGrouping();
            _.forEach(columns, (column) => {
                if (this.gridApi.grouping)
                    this.gridApi.grouping.groupColumn(column);
            });
        }
    }

    public enableCellEdit(index: number, enable: boolean) {
        this.gridOptions.columnDefs[index].enableCellEdit = enable;
    }

    public visibleColumn(index: number, visible: boolean) {
        this.gridOptions.columnDefs[index].visible = visible;
        this.refreshColumns();
    }

    public isColumnVisible(index: number): boolean {
        return this.gridOptions.columnDefs[index].visible;
    }

    // Boolean (checkbox)
    private createColumnDefBool(field: string, displayName: string, width: string, enableHiding: boolean = true, enableColumnResizing: boolean = true, enableFiltering: boolean = true, enableSorting: boolean = true, enableColumnMenu: boolean = true, enableCellEdit: boolean = false, clickEvent: string = "", clickEventField: string = "", disabledField: string = "", termIndeterminate: boolean = false): uiGrid.IColumnDef {
        var columnDef = this.createColumnDef(field, displayName, width, enableHiding, enableColumnResizing, enableFiltering, enableSorting, enableColumnMenu, enableCellEdit);
        columnDef.type = 'boolean';  // Used for sorting
        columnDef.headerCellTemplate = 'uiGrid/checkBoxHeaderCellTemplate.html';
        columnDef.cellTemplate = '<div><input type="checkbox" data-ng-model="row.entity.' + field + '"';

        if (!enableCellEdit)
            columnDef.cellTemplate += ' disabled="disabled"';
        else if (disabledField) {
            if (disabledField.startsWithCaseInsensitive('!'))
                columnDef.cellTemplate += ' data-ng-disabled="!row.entity.' + disabledField.substring(1) + '"';
            else
                columnDef.cellTemplate += ' data-ng-disabled="row.entity.' + disabledField + '"';
        }

        if (clickEvent) {
            columnDef.cellTemplate += ' data-ng-click="$event.stopPropagation();grid.appScope.ctrl.' + clickEvent + '(row.entity';
            if (clickEventField)
                columnDef.cellTemplate += '.' + clickEventField + ', row.entity';
            columnDef.cellTemplate += ')"';
        }

        columnDef.cellTemplate += '/></div > ';

        (<any>columnDef).filter = {
            term: (field === 'isActive' ? true : null)
        };
        (<any>columnDef.filter).toggle = (x) => {
            if (x.term === true) {
                x.term = false;
                x.termIndeterminate = false;
            } else if (x.term === false) {
                x.term = null;
                x.termIndeterminate = null;
            } else {
                x.term = true;
                x.termIndeterminate = true;
            }
        }
        (<any>columnDef.filter).termIndeterminate = termIndeterminate ? termIndeterminate : true;

        return columnDef;
    }
    public addColumnBool(field: string, displayName: string, width: string, enableCellEdit: boolean = false, clickEvent?: string, clickEventField?: string, disabledField?: string, termIndeterminate?: boolean): uiGrid.IColumnDef {
        var columnDef = this.createColumnDefBool(field, displayName, width, true, true, true, true, true, enableCellEdit, clickEvent, clickEventField, disabledField, termIndeterminate);
        this.addColumn(columnDef);
        return columnDef;
    }

    // Active
    private createColumnDefActive(field: string, displayName?: string, width?: string, changeEventHandlerName: string = 'selectItem'): uiGrid.IColumnDef {
        var columnDef = this.createColumnDefBool("isActive", displayName ? displayName : "", width ? width : "80", false, false, true, false, false, true, changeEventHandlerName, field, "disableActive");
        columnDef.pinnedLeft = true;
        return columnDef;
    }
    public addColumnActive(field: string, displayName?: string, width?: string, changeEventHandlerName?: string): uiGrid.IColumnDef {
        var columnDef = this.createColumnDefActive(field, displayName, width, changeEventHandlerName);
        this.addColumn(columnDef);
        return columnDef;
    }

    // Icon
    public createColumnDefIcon(field: string, icon: string, toolTip: string, clickEvent: string, showIconField?: string, showIconFunction?: string, displayName?: string, width?: string, enableHiding?: boolean, enableResizing?: boolean, ctrlName?: string, isSubgrid?: boolean, tooltipField?: string): uiGrid.IColumnDef {
        var columnDef = this.createColumnDef(field ? field : "icon", displayName ? displayName : "", width ? width : "20", enableHiding, enableResizing);
        if (!ctrlName)
            ctrlName = 'ctrl';

        var icons: string[] = [];
        if (icon)
            icons = icon.split("|");
        if (icons.length > 1) {
            // Multiple icons stacked
            columnDef.cellTemplate = '<button type="button" class="gridCellIcon" title=\"' + toolTip + '\"';

            if (showIconField) {
                if (showIconField.startsWithCaseInsensitive('!'))
                    columnDef.cellTemplate += ' data-ng-hide="row.entity.' + showIconField.substr(1) + '"';
                else
                    columnDef.cellTemplate += ' data-ng-show="row.entity.' + showIconField + '"';
            } else if (showIconFunction)
                columnDef.cellTemplate += ' data-ng-show="grid.appScope.' + (isSubgrid ? "" : (ctrlName + '.')) + showIconFunction + '(row.entity)"';

            if (clickEvent)
                columnDef.cellTemplate += ' data-ng-click="$event.stopPropagation();grid.appScope.' + (isSubgrid ? "" : (ctrlName + '.')) + clickEvent + '(row.entity)"';
            else
                columnDef.cellTemplate += ' style="cursor: default;"';

            columnDef.cellTemplate += '><span class="fa fa-stack stacked-container">';

            var i: number = 0;
            _.forEach(icons, icn => {
                columnDef.cellTemplate += '<i class="' + icn;
                columnDef.cellTemplate += i === 0 ? ' stacked-primary' : ' stacked-secondary';
                columnDef.cellTemplate += '"/>';
                i++;
            });

            columnDef.cellTemplate += "</span></button>";
        } else {
            // Single icon
            columnDef.cellTemplate = '<button type="button" class="gridCellIcon ' + (icon ? icon : "{{COL_FIELD}}") + '"';
            
            if (tooltipField)
                columnDef.cellTemplate += ' ng-attr-title="{{row.entity.' + tooltipField + '}}"';
            else if (toolTip)
                columnDef.cellTemplate += ' title=\"' + toolTip + '\"';

            if (showIconField) {
                if (showIconField.startsWithCaseInsensitive('!'))
                    columnDef.cellTemplate += ' data-ng-hide="row.entity.' + showIconField.substr(1) + '"';
                else
                    columnDef.cellTemplate += ' data-ng-show="row.entity.' + showIconField + '"';
            } else if (showIconFunction)
                columnDef.cellTemplate += ' data-ng-show="grid.appScope.' + (isSubgrid ? "" : (ctrlName + '.')) + showIconFunction + '(row.entity)"';

            if (clickEvent)
                columnDef.cellTemplate += ' data-ng-click="$event.stopPropagation();grid.appScope.' + (isSubgrid ? "" : (ctrlName + '.')) + clickEvent + '(row.entity)"';
            else
                columnDef.cellTemplate += ' style="cursor: default;"';

            columnDef.cellTemplate += '></button>';
        }

        columnDef.enableFiltering = false;
        columnDef.enableColumnMenu = false;

        this.setSoeType(columnDef, Constants.GRID_COLUMN_TYPE_ICON);

        return columnDef;
    }
    public addColumnIcon(field: string, icon: string, toolTip: string, clickEvent: string, showIconField?: string, showIconFunction?: string, displayName?: string, width?: string, enableHiding?: boolean, enableResizing?: boolean, ctrlName?: string, isSubgrid?: boolean, tooltipField?: string): uiGrid.IColumnDef {
        var columnDef = this.createColumnDefIcon(field, icon, toolTip, clickEvent, showIconField, showIconFunction, displayName, width, enableHiding, enableHiding, ctrlName, isSubgrid, tooltipField);
        this.addColumn(columnDef);
        return columnDef;
    }

    // Edit
    private createColumnDefEdit(toolTip: string, clickEvent?: string, ctrlName?: string, isSubgrid?: boolean): uiGrid.IColumnDef {
        if (!clickEvent)
            clickEvent = "edit";
        if (!ctrlName)
            ctrlName = 'ctrl';

        var columnDef = this.createColumnDefIcon("edit", "fal fa-pencil iconEdit", toolTip, "edit", null, null, null, null, false, false, ctrlName, isSubgrid);
        columnDef.cellTemplate = '<button type="button" class="gridCellIcon fal fa-pencil iconEdit" title=\"' + toolTip + '\" data-ng-hide="row.entity.hideEdit" data-ng-click="$event.stopPropagation();grid.appScope.' + (isSubgrid ? "" : (ctrlName + '.')) + clickEvent + '(row.entity)"></button>';

        // TODO: Uncomment this when pin GUI bug is fixed
        //columnDef["pinnedRight"] = true;
        return columnDef;
    }
    public addColumnEdit(toolTip: string, clickEvent?: string, ctrlName?: string, isSubgrid?: boolean): uiGrid.IColumnDef {
        var columnDef = this.createColumnDefEdit(toolTip, clickEvent, ctrlName, isSubgrid);
        this.addColumn(columnDef);
        return columnDef;
    }

    // Delete
    public createColumnDefDelete(toolTip: string, onDeleteEvent?: string, ctrlName?: string, isSubGrid?: boolean, showIconField?: string, showIconFunction?: string): uiGrid.IColumnDef {
        if (!onDeleteEvent)
            onDeleteEvent = "onDeleteEvent";

        var columnDef = this.createColumnDefIcon("delete", "fal fa-times iconDelete", toolTip, onDeleteEvent, showIconField, showIconFunction, null, null, false, false, ctrlName, isSubGrid);

        // TODO: Uncomment this when pin GUI bug is fixed
        //columnDef["pinnedRight"] = true;

        return columnDef;
    }
    public addColumnDelete(toolTip: string, onDeleteEvent?: string, ctrlName?: string, isSubgrid?: boolean, showIconField?: string, showIconFunction?: string): uiGrid.IColumnDef {
        var columnDef = this.createColumnDefDelete(toolTip, onDeleteEvent, ctrlName, isSubgrid, showIconField, showIconFunction);
        this.addColumn(columnDef);
        return columnDef;
    }

    // PDF
    private createColumnDefPdf(toolTip: string, onPdfEvent?: string): uiGrid.IColumnDef {
        if (!onPdfEvent)
            onPdfEvent = "showPdf";

        var columnDef = this.createColumnDefIcon("pdf", "fal fa-file-pdf", toolTip, onPdfEvent);
        return columnDef;
    }
    public addColumnPdf(toolTip: string): uiGrid.IColumnDef {
        var columnDef = this.createColumnDefPdf(toolTip);
        this.addColumn(columnDef);
        return columnDef;
    }

    // IsModified
    public createColumnDefIsModified(field?: string, displayName?: string, width?: string, clickEvent?: string, ctrlName?: string): uiGrid.IColumnDef {
        if (!field)
            field = "isModified";
        if (!ctrlName)
            ctrlName = 'ctrl';

        var columnDef = this.createColumnDef(field, displayName ? displayName : "", width ? width : "20", false, false, false, false, false);
        columnDef.pinnedLeft = true;

        var template: string = '<div class="ui-grid-cell-contents fal fa-asterisk';
        if (clickEvent)
            template += ' link';
        template += '" data-ng-class="{gridColumnIsModified: row.entity.' + field + ', colorTransparent: !row.entity.' + field + '} "';
        if (clickEvent)
            template += ' data-ng-click="$event.stopPropagation(); grid.appScope.' + ctrlName + '.' + clickEvent + '(row.entity)"';
        template += '></div>';

        columnDef.cellTemplate = template;

        return columnDef;
    }
    public addColumnIsModified(field?: string, displayName?: string, width?: string, clickEvent?: string, ctrlName?: string) {
        var columnDef = this.createColumnDefIsModified(field, displayName, width, clickEvent, ctrlName);
        this.addColumn(columnDef);
        return columnDef;
    }

    // Text
    public createColumnDefText(field: string, displayName: string, width: string, enableHiding: boolean = false, toolTipField?: string, toolTip?: string, className?: string, classFunction?: string, shapeValueField?: string, shape?: string, buttonIcon?: string, buttonFunction?: string, ctrlName?: string, showButtonField?: string): uiGrid.IColumnDef {
        var columnDef = this.createColumnDef(field, displayName, width, enableHiding);
        columnDef.filter = {
            condition: this.uiGridConstants.filter.CONTAINS,
            term: null
        };

        /*var cellTemplate = '<div class="ui-grid-cell-contents ngCellText" ng-class="col.colIndex()"><span style="position:absolute;">';
            cellTemplate += this.getShapeTemplateGradient(shape, null, shapeValueField);
            cellTemplate += '</span><span style="margin-left:25px;" ng-cell-text ng-bind="row.entity.' + field + '" ></span></div>';*/
        if (toolTipField || toolTip || className || (buttonIcon && buttonFunction)) {

            //Grouped
            var template = '<span ng-if="row.groupHeader" ng-cell-text class="ui-grid-cell-contents">{{COL_FIELD CUSTOM_FILTERS}}</span><span ng-if="!col.groupHeader">';

            //Not grouped
            template += '<div class="ui-grid-cell-contents ngCellText" ';

            // ToolTip
            if (toolTipField)
                template += ' ng-attr-title="{{row.entity.' + toolTipField + '}}"';
            else if (toolTip)
                template += ' title="' + toolTip + '"';

            // Class
            if (className) {
                template += ' ng-class="{\'' + className + '\':';
                template += classFunction ? ' grid.appScope.ctrl.' + classFunction + '(row.entity) }"' : ' true}"';
            }

            template += '>';

            if (buttonIcon && buttonFunction) {
                template += '<button class="gridCellIcon ' + buttonIcon + '" title=\"' + toolTip + '\" data-ng-click="$event.stopPropagation();grid.appScope.' + (ctrlName || 'ctrl') + '.' + buttonFunction + '(row.entity)"';
                if (showButtonField) {
                    template += ' ng-if="row.entity.' + showButtonField + '"';
                }
                template += '></button>';
            }

            template += '<span ng-cell-text ng-bind="row.entity.' + field + '"></span>';

            template += '</div></span>';

            columnDef.cellTemplate = template;
        }

        return columnDef;
    }
    public addColumnText(field: string, displayName: string, width: string, enableHiding: boolean = false, toolTipField?: string, toolTip?: string, className?: string, classFunction?: string, shapeValueField?: string, shape?: string, buttonIcon?: string, buttonFunction?: string, ctrlName?: string, showButtonField?: string): uiGrid.IColumnDef {
        var columnDef = this.createColumnDefText(field, displayName, width, enableHiding, toolTipField, toolTip, className, classFunction, shapeValueField, shape, buttonIcon, buttonFunction, ctrlName, showButtonField);
        this.addColumn(columnDef);
        return columnDef;
    }

    // HTML
    private createColumnDefHtml(field: string, displayName: string, width: string, enableHiding: boolean = false): uiGrid.IColumnDef {
        var columnDef = this.createColumnDefText(field, displayName, width, enableHiding);
        columnDef.cellTemplate = '<div data-ng-bind-html="row.entity.' + field + '"/></div>';
        return columnDef;
    }
    public addColumnHtml(field: string, displayName: string, width: string, enableHiding: boolean = false): uiGrid.IColumnDef {
        var columnDef = this.createColumnDefHtml(field, displayName, width, enableHiding);
        this.addColumn(columnDef);
        return columnDef;
    }

    // Number
    public createColumnDefNumber(field: string, displayName: string, width: string, enableHiding: boolean, decimals: number, type: string = "number", disabledField?: string, disabledFieldFunction?: string, onChangeEvent?: string, alignLeft?: boolean, toolTipField?: string, toolTip?: string, className?: string, classFunction?: string): uiGrid.IColumnDef {
        var columnDef = this.createColumnDefText(field, displayName, width, enableHiding);
        columnDef.type = type;
        columnDef.cellClass = alignLeft ? "text-left" : "ui-grid-cell-text-right";

        var dec = decimals ? ' | number:' + decimals : '';

        var template = '<div class="ui-grid-cell-contents ngCellText" ';

        // ToolTip
        if (toolTipField)
            template += ' ng-attr-title="{{row.entity.' + toolTipField + '}}"';
        else if (toolTip)
            template += ' title="' + toolTip + '"';

        // Class
        if (className) {
            template += ' ng-class="{\'' + className + '\': grid.appScope.ctrl.' + classFunction + '(row.entity) }"';
        }

        template += ' ng-class="col.colIndex()">';
        template += '<span ng-if="!col.grouping || col.grouping.groupPriority === undefined || col.grouping.groupPriority === null || ( row.groupHeader && col.grouping.groupPriority === row.treeLevel )" ng-cell-text >{{COL_FIELD CUSTOM_FILTERS' + dec + '}}</span>';
        template += '<span ng-if="!(!col.grouping || col.grouping.groupPriority === undefined || col.grouping.groupPriority === null || ( row.groupHeader && col.grouping.groupPriority === row.treeLevel ))" ng-cell-text data-ng-bind="row.entity.' + field + dec + '"></span>';
        template += '</div>';

        columnDef.cellTemplate = template;

        //columnDef.cellTemplate = '<div class="ui-grid-cell-contents ngCellText" ng-class="col.colIndex()"><span ng-cell-text data-ng-bind="row.entity.' + field + dec + '"></span></div>';
        // 2016-07-04 Håkan:
        // Removed numeric type on input field due to IE not handling comma
        // Make sure you run NumberUtility.parseDecimal(entity[field]) in AfterCellEdit
        //columnDef.editableCellTemplate = '<div><form name="inputForm"><input type="INPUT_TYPE" ng-class="\'colt\' + col.uid" ui-grid-editor ng-model="MODEL_COL_FIELD"';
        columnDef.editableCellTemplate = '<div><form name="inputForm"><input ng-class="\'colt\' + col.uid" ui-grid-editor ng-model="MODEL_COL_FIELD"';
        if (decimals)
            columnDef.editableCellTemplate += ' "decimal="' + decimals + '"';
        if (onChangeEvent)
            columnDef.editableCellTemplate += ' data-ng-change="grid.appScope.ctrl.' + onChangeEvent + '(row.entity)"';
        if (disabledField != null)
            columnDef.editableCellTemplate += ' data-ng-disabled="row.entity.' + disabledField + '"';
        else if (disabledFieldFunction != null)
            columnDef.editableCellTemplate += ' data-ng-disabled="grid.appScope.ctrl.' + disabledFieldFunction + '(row.entity)"';

        columnDef.editableCellTemplate += '></form></div>';
        return columnDef;
    }
    public addColumnNumber(field: string, displayName: string, width: string, enableHiding: boolean = false, decimals: number = null, type?: string, disabledField?: string, disabledFieldFunction?: string, onChangeEvent?: string, alignLeft?: boolean, toolTipField?: string, toolTip?: string, className?: string, classFunction?: string): uiGrid.IColumnDef {
        var columnDef = this.createColumnDefNumber(field, displayName, width, enableHiding, decimals, type, disabledField, disabledFieldFunction, onChangeEvent, alignLeft, toolTipField, toolTip, className, classFunction);
        this.addColumn(columnDef);
        return columnDef;
    }

    // Select (combobox)
    private createColumnDefSelect(field: string, displayName: string, width: string, selectOptions: any[], enableHiding?: boolean, enableCellEdit?: boolean): uiGrid.IColumnDef {
        var columnDef = this.createColumnDef(field, displayName, width, enableHiding);
        (<any>columnDef).filter = {
            type: this.uiGridConstants.filter.SELECT,
            selectOptions: selectOptions
        };

        columnDef.enableCellEdit = enableCellEdit;

        return columnDef;
    }
    public addColumnSelect(field: string, displayName: string, width: string, selectOptions: any[], enableHiding: boolean = true, enableCellEdit: boolean = false, fieldValue: string = "", dropdownIdLabel: string = "id", dropdownValueLabel: string = "value", onChangeEvent: string = "", ctrlName?: string, collectionField?: string, shapeValueField?: string, shape?: string): uiGrid.IColumnDef {
        var columnDef = this.createColumnDefSelect(field, displayName, width, selectOptions, enableHiding, enableCellEdit);
        if (enableCellEdit) {
            let cellTemplate = '<div class="ui-grid-cell-contents ngCellText" ng-class="col.colIndex()"><span ng-cell-text ng-bind="row.entity.' + fieldValue + '"></span></div>';

            let editCellTemplate = '<div><form name="inputForm">';
            if (collectionField)
                editCellTemplate += '<select ng-class="\'colt\' + col.uid" ui-grid-edit-dropdown ng-model="MODEL_COL_FIELD" ng-options="field[editDropdownIdLabel] as field[editDropdownValueLabel] for field in row.entity.' + collectionField + '"';
            else
                editCellTemplate += '<select ng-class="\'colt\' + col.uid" ui-grid-edit-dropdown ng-model="MODEL_COL_FIELD" ng-options="field[editDropdownIdLabel] as field[editDropdownValueLabel] for field in editDropdownOptionsArray"';
            if (onChangeEvent)
                editCellTemplate += ' data-ng-change="grid.appScope.' + (ctrlName || 'ctrl') + '.' + onChangeEvent + '(row.entity)"';
            editCellTemplate += '></select></form></div>';

            columnDef.cellTemplate = cellTemplate;
            columnDef.editableCellTemplate = editCellTemplate; //'ui-grid/dropdownEditor';   
            columnDef.editDropdownValueLabel = dropdownValueLabel;
            columnDef.editDropdownIdLabel = dropdownIdLabel;
            columnDef.editDropdownOptionsArray = selectOptions;

            this.setSoeType(columnDef, Constants.GRID_COLUMN_TYPE_SELECT);
        }
        else if (shapeValueField && shape) {
            let cellTemplate = '<div class="ui-grid-cell-contents ngCellText" ng-class="col.colIndex()"><span style="position:absolute;">';
            cellTemplate += this.getShapeTemplateGradient(shape, shapeValueField);
            cellTemplate += '</span><span style="margin-left:25px;" ng-cell-text ng-bind="row.entity.' + field + '" ></span></div>';
            columnDef.cellTemplate = cellTemplate;
        }
        this.addColumn(columnDef);

        return columnDef;
    }

    // MultiSelect
    private createColumnDefMultiSelect(field: string, displayName: string, width: string, selectOptions: any[], enableHiding?: boolean, enableCellEdit?: boolean): uiGrid.IColumnDef {
        var columnDef = this.createColumnDef(field, displayName, width, enableHiding);
        //(<any>columnDef).filter = {
        //    //type: this.uiGridConstants.filter.SELECT,
        //    selectOptions: selectOptions
        //};

        columnDef.enableCellEdit = enableCellEdit;

        return columnDef;
    }
    public addColumnMultiSelect(field: string, displayName: string, width: string, selectOptions: any[], enableHiding: boolean = true, enableCellEdit: boolean = false, fieldValue: string = "", dropdownIdLabel: string = "id", dropdownValueLabel: string = "name", placeholder: string = ""): uiGrid.IColumnDef {
        var columnDef = this.createColumnDefMultiSelect(field, displayName, width, selectOptions, enableHiding, enableCellEdit);

        columnDef.cellTemplate = '<div class="ui-grid-cell-contents ngCellText" ng-class="col.colIndex()"><span ng-cell-text ng-bind="row.entity.' + fieldValue + '"></span></div>';

        if (width === null)
            width = "100";

        if (enableCellEdit) {
            columnDef['selectOptions'] = selectOptions;
            var editCellTemplate = '<ui-grid-ui-select class="ui-grid-ui-select">&nbsp;';
            editCellTemplate += '<ui-select multiple ng-model="MODEL_COL_FIELD" ng-disabled="disabled" append-to-body="true" style="width: ' + width + 'px;">';
            editCellTemplate += '<ui-select-match placeholder="' + placeholder + '">{{ $item.' + dropdownValueLabel + ' }}</ui-select-match>';
            editCellTemplate += '<ui-select-choices repeat="item in col.colDef.selectOptions"><span>{{ item.' + dropdownValueLabel + ' }}</span></ui-select-choices>';
            editCellTemplate += '</ui-select>';
            editCellTemplate += '</ui-grid-ui-select>';
            columnDef.editableCellTemplate = editCellTemplate;
        }
        this.addColumn(columnDef);

        return columnDef;
    }

    // DateTime
    private createColumnDefDateTime(field: string, displayName: string, width: string, enableHiding: boolean = false, cellFilter: string = null, toolTipField?: string, toolTip?: string, className?: string, classFunction?: string): uiGrid.IColumnDef {
        var columnDef = this.createColumnDefText(field, displayName, width, enableHiding, toolTipField, toolTip, className, classFunction);
        columnDef.type = 'date'; // Used for sorting
        //columnDef.cellClass = "text-right";
        if (!cellFilter)
            cellFilter = "date:'yyyy-MM-dd HH:mm'";
        columnDef.cellFilter = cellFilter;
        // TODO: DatePicker dropdown filter
        // TODO: Other Locations/Languages
        return columnDef;
    }
    public addColumnDateTime(field: string, displayName: string, width: string, enableHiding: boolean = false, cellFilter: string = null, toolTipField?: string, toolTip?: string, className?: string, classFunction?: string): uiGrid.IColumnDef {
        var columnDef = this.createColumnDefDateTime(field, displayName, width, enableHiding, cellFilter, toolTipField, toolTip, className, classFunction);
        this.addColumn(columnDef);
        return columnDef;
    }

    // Date
    private createColumnDefDate(field: string, displayName: string, width: string, enableHiding: boolean = false, cellFilter: string = null, toolTipField?: string, toolTip?: string, className?: string, classFunction?: string): uiGrid.IColumnDef {
        var columnDef = this.createColumnDefText(field, displayName, width, enableHiding, toolTipField, toolTip, className, classFunction);
        columnDef.type = 'date'; // Used for sorting
        //columnDef.cellClass = "text-right";
        if (!cellFilter)
            cellFilter = "date:'shortDate'";

        columnDef.cellFilter = cellFilter;
        columnDef.editableCellTemplate = '<div><form name="inputForm"><div ui-grid-edit-datepicker datepicker-options="datepickerOptions" ng-class="\'colt\' + col.uid"></div></form></div>'
        columnDef.filterHeaderTemplate = '<div><div><date-filter col-filter="col.filters[0]" is-from="true"></date-filter></div><div><date-filter col-filter="col.filters[1]" is-from="false"></date-filter></div></div>';
        (<any>columnDef).filters = [
            {
                condition: (term, value, row, column) => {
                    return term ? new Date(term.addMinutes(term.localTimeZoneOffsetFromDefault())).isSameOrBeforeOnDay(value) : true;
                }
            }, {
                condition: (term, value, row, column) => {
                    return term ? new Date(term.addMinutes(term.localTimeZoneOffsetFromDefault())).isSameOrAfterOnDay(value) : true;
                }
            }
        ];
        return columnDef;
    }
    public addColumnDate(field: string, displayName: string, width: string, enableHiding: boolean = false, cellFilter: string = null, toolTipField?: string, toolTip?: string, className?: string, classFunction?: string): uiGrid.IColumnDef {
        var columnDef = this.createColumnDefDate(field, displayName, width, enableHiding, cellFilter, toolTipField, toolTip, className, classFunction);
        this.addColumn(columnDef);
        return columnDef;
    }

    // Time
    private createColumnDefTime(field: string, displayName: string, width: string, enableHiding: boolean = false, cellFilter: string = null, columnDefType: string = null): uiGrid.IColumnDef {
        var columnDef = this.createColumnDefText(field, displayName, width, enableHiding);
        if (!columnDefType)
            columnDefType = 'date';
        columnDef.type = columnDefType;
        columnDef.cellClass = "text-right";
        if (!cellFilter)
            cellFilter = "date:'shortTime'";
        columnDef.cellFilter = cellFilter;
        columnDef.editableCellTemplate = '<input data-ui-grid-editor type="time" ng-model="MODEL_COL_FIELD"></input>';
        return columnDef;
    }
    public addColumnTime(field: string, displayName: string, width: string, enableHiding: boolean = false, cellFilter: string = null, columnDefType: string = null): uiGrid.IColumnDef {
        var columnDef = this.createColumnDefTime(field, displayName, width, enableHiding, cellFilter, columnDefType);
        this.addColumn(columnDef);
        return columnDef;
    }

    // TimeSpan
    private createColumnDefTimeSpan(field: string, displayName: string, width: string, enableHiding: boolean = false, treatUndefinedAsEmpty: boolean = false): uiGrid.IColumnDef {
        var columnDef = this.createColumnDefText(field, displayName, width, enableHiding);
        columnDef.type = "number";
        columnDef.cellClass = "text-right";
        if (treatUndefinedAsEmpty)
            columnDef.cellFilter = "minutesToTimeSpan:false:false:false:true";
        else
            columnDef.cellFilter = "minutesToTimeSpan";
        columnDef.editableCellTemplate = '<input data-ui-grid-editor ng-model="MODEL_COL_FIELD" onFocus="this.select()"></input>';
        (<any>columnDef).filters = [
            {
                condition: (term, value, row, column) => {
                    return term ? CalendarUtility.minutesToTimeSpan(value).contains(term) : true;
                }
            }
        ];
        return columnDef;
    }
    public addColumnTimeSpan(field: string, displayName: string, width: string, enableHiding: boolean = false, treatUndefinedAsEmpty: boolean = false): uiGrid.IColumnDef {
        var columnDef = this.createColumnDefTimeSpan(field, displayName, width, enableHiding, treatUndefinedAsEmpty);
        this.addColumn(columnDef);
        return columnDef;
    }

    // Shape
    public createColumnDefShape(field: string, displayName?: string, width?: string, shapeField?: string, shape?: string, toolTipField?: string, toolTip?: string, showIconField?: string, showIconFunction?: string, ctrlName?: string, isSubgrid?: boolean, attestGradient?: boolean): uiGrid.IColumnDef {
        var columnDef = this.createColumnDef(field, displayName ? displayName : "", width ? width : "22");
        columnDef.enableColumnMenu = false;
        columnDef.type = "shape";
        columnDef.sortingAlgorithm = (a: any, b: any) => {
            if (!a && b) return 1;
            if (a && !b) return -1;
            if (a === b) return 0;
            if (a < b) return 1;
            return -1;
        }
        this.setSoeType(columnDef, Constants.GRID_COLUMN_TYPE_SHAPE);

        // TODO: shapeField not supported yet!
        // Be able to bind a property of the row to show different shapes
        if (!ctrlName)
            ctrlName = 'ctrl';

        var template = '<div class="ui-grid-cell-contents-shape"';

        // ToolTip
        if (toolTipField)
            template += ' ng-attr-title="{{row.entity.' + toolTipField + '}}"';
        else if (toolTip)
            template += ' title="' + toolTip + '"';

        if (showIconField)
            template += ' data-ng-show="row.entity.' + showIconField + '"';
        else if (showIconFunction)
            template += ' data-ng-show="grid.appScope.' + (isSubgrid ? "" : (ctrlName + '.')) + showIconFunction + '(row.entity)"';

        template += '>';

        // Shape
        if (attestGradient)
            template += this.getShapeTemplateGradient(shape, field);
        else
            template += this.getShapeTemplate(shape, null, shape === Constants.SHAPE_RECTANGLE ? parseInt(width, 10) - 3 : undefined);

        template += '</div>';
        columnDef.filterHeaderTemplate = '<div class="ui-grid-filter-container" ng-repeat="colFilter in col.filters"><shape-filter></shape-filter></div>';

        columnDef.cellTemplate = template;

        return columnDef;
    }
    public addColumnShape(field: string, displayName?: string, width?: string, shapeField?: string, shape?: string, toolTipField?: string, toolTip?: string, showIconField?: string, showIconFunction?: string, ctrlName?: string, isSubgrid?: boolean, attestGradient?: boolean): uiGrid.IColumnDef {
        var columnDef = this.createColumnDefShape(field, displayName, width, shapeField, shape, toolTipField, toolTip, showIconField, showIconFunction, ctrlName, isSubgrid, attestGradient);
        this.addColumn(columnDef);
        return columnDef;
    }

    // Type ahead
    private createColumnDefTypeAhead(field: string, typeAheadOptions: TypeAheadOptions, displayName?: string, width?: string, minChars?: number, wait?: number): uiGrid.IColumnDef {
        var columnDef = this.createColumnDef(field, displayName, width);
        columnDef.enableSorting = false;
        columnDef.enableColumnMenu = false;

        if (typeAheadOptions.errorBinding) {
            columnDef.cellTemplate =
                '<div class="editable-cell-height ui-grid-cell-contents" ng-class="{\'invalid-cell\': row.entity.' + typeAheadOptions.errorBinding + `, \'${typeAheadOptions.secondRowBinding ? 'grid-double-text-row' : ''}\' : !row.entity.` + typeAheadOptions.errorBinding + '}">' +
                '<div ng-show="row.entity.' + typeAheadOptions.errorBinding + '" data-ng-bind="row.entity.' + typeAheadOptions.errorBinding + '"></div>' +
                '<div ng-show="!row.entity.' + typeAheadOptions.errorBinding + '">' +
                '<div data-ng-bind="MODEL_COL_FIELD"></div>' +
                '<div class="single-row-text">{{row.entity.' + typeAheadOptions.secondRowBinding + '}}</div>' +
                '</div>' +
                '</div>';
        } else if (typeAheadOptions.displayField) {
            var template = '<div class="ui-grid-cell-contents ngCellText" ng-attr-title="{{row.entity.' + typeAheadOptions.displayField + '}}" ng-class="col.colIndex()">';
            template += '<span ng-cell-text>{{row.entity.' + typeAheadOptions.displayField + '}}</span>';
            template += '</div>';
            columnDef.cellTemplate = template;
        } else {
            columnDef.cellTemplate = `<div class="${typeAheadOptions.secondRowBinding ? 'grid-double-text-row' : ''} editable-cell-height">` +
                '<div data-ng-bind="MODEL_COL_FIELD"></div>' +
                (typeAheadOptions.secondRowBinding ? '<div class="single-row-text">{{row.entity.' + typeAheadOptions.secondRowBinding + '}}</div>' : '') +
                '</div>';
        }

        columnDef.editableCellTemplate = '<div><form name="inputForm">';

        if (typeAheadOptions.buttonClickEvent)
            columnDef.editableCellTemplate += '<div class="input-group">';

        columnDef.editableCellTemplate += '<input type="text" ui-grid-typeahead-editor class="form-control ui-grid-type-ahead-editable';
        if (typeAheadOptions.buttonClickEvent)
            columnDef.editableCellTemplate += ' ui-grid-type-ahead-editable-with-button';
        columnDef.editableCellTemplate += '" autocomplete="off" data-ng-model="MODEL_COL_FIELD" typeahead-on-select="grid.appScope.' + typeAheadOptions.ctrlName + '.selectedItemChanged(' + typeAheadOptions.selectedItemChangedMethodParameters + ')" uib-typeahead="' + typeAheadOptions.typeAhead + '" typeahead-show-hint="false" typeahead-min-length="' + (minChars ? minChars : 0) + '" typeahead-wait-ms="' + (wait ? wait : 0) + '" typeahead-editable="true" typeahead-append-to-body="true" />';

        if (typeAheadOptions.buttonClickEvent) {
            columnDef.editableCellTemplate += '<span class="input-group-btn">' +
                '<button type="button" class="no-border-radius leave-typeahead-alone ui-grid-type-ahead-button ' + typeAheadOptions.buttonIcon + '"' +
                ' data-l10n-bind data-l10n-bind-title="\'' + typeAheadOptions.buttonTooltipKey + '\'"' +
                ' data-ng-click="$event.stopPropagation();grid.appScope.' + typeAheadOptions.ctrlName + '.' + typeAheadOptions.buttonClickEvent + '(row.entity)"></button>' +
                '</span></div>';
        }

        columnDef.editableCellTemplate += '</form></div>';

        return columnDef;
    }
    public addColumnTypeAhead(field: string, typeAheadOptions: TypeAheadOptions, displayName?: string, width?: string, minChars?: number, wait?: number): uiGrid.IColumnDef {
        var columnDef = this.createColumnDefTypeAhead(field, typeAheadOptions, displayName, width, minChars, wait);
        this.addColumn(columnDef);
        this.setSoeType(columnDef, Constants.GRID_COLUMN_TYPE_TYPEAHEAD);
        this.setSoeAdditionalData(columnDef, typeAheadOptions);
        return columnDef;
    }

    private setSoeAdditionalData(colDef: any, data: TypeAheadOptions) {
        colDef.soeData = data;
    }

    private setSoeType(colDef: any, typename: string) {
        colDef.soeType = typename;
    }

    private getShapeTemplate(shape: string, entityProp?: string, width: number = 20): string {
        var template = '<svg height="20" width="{0}">'.format(width.toString());

        switch (shape) {
            case Constants.SHAPE_CIRCLE:
                template += '<circle class="shape" cx="10" cy="10" r="8"';
                break;
            case Constants.SHAPE_SQUARE:
                template += '<rect class="shape" width="20" height="20"';
                break;
            case Constants.SHAPE_RECTANGLE:
                template += '<rect class="shape" width="{0}" height="20"'.format(width.toString());
                break;
            case Constants.SHAPE_TRIANGLE_DOWN:
                template += '<polygon class="shape" points="0,2 20,2 10,19"';
                break;
            case Constants.SHAPE_TRIANGLE_LEFT:
                template += '<polygon class="shape" points="18,0 1,10 18,20"';
                break;
            case Constants.SHAPE_TRIANGLE_RIGHT:
                template += '<polygon class="shape" points="1,0 18,10 1,20"';
                break;
            case Constants.SHAPE_TRIANGLE_UP:
                template += '<polygon class="shape" points="10,1 0,18 20,18"';
                break;
            default:
                // Default to square
                template += '<rect class="shape" width="20" height="20"';
        }
        if (entityProp)
            template += ' ng-style="{\'fill\':row.entity.' + entityProp + '}" />';
        else

            template += ' ng-style="{\'fill\':COL_FIELD}" />';
        template += '</svg>';

        return template;
    }

    private getShapeTemplateGradient(shape: string, entityProp?: string): string {
        var gradientProp = "useGradient";
        var template = '<svg height="20" width="20" ng-if="row.entity.' + entityProp + ' && !row.entity.useGradient">';

        switch (shape) {
            case Constants.SHAPE_CIRCLE:
                template += '<circle class="shape" cx="10" cy="10" r="8"';
                break;
            case Constants.SHAPE_SQUARE:
                template += '<rect class="shape" width="20" height="20"';
                break;
            case Constants.SHAPE_TRIANGLE_DOWN:
                template += '<polygon class="shape" points="0,2 20,2 10,19"';
                break;
            case Constants.SHAPE_TRIANGLE_LEFT:
                template += '<polygon class="shape" points="18,0 1,10 18,20"';
                break;
            case Constants.SHAPE_TRIANGLE_RIGHT:
                template += '<polygon class="shape" points="1,0 18,10 1,20"';
                break;
            case Constants.SHAPE_TRIANGLE_UP:
                template += '<polygon class="shape" points="10,1 0,18 20,18"';
                break;
            default:
                // Default to square
                template += '<rect class="shape" width="20" height="20"';
        }
        if (entityProp)
            template += ' ng-style="{\'fill\':row.entity.' + entityProp + '}"/>';
        else

            template += ' ng-style="{\'fill\':COL_FIELD}" />';
        template += '</svg>';

        //Add gradient
        template += '<svg height="20" width="20" ng-if="row.entity.' + gradientProp + '"><defs>' +
            '<linearGradient id="grad1" x1="0%" y1="50%" x2="50%" y2="0%" >' +
            '<stop offset="0%" style="stop-color:rgb(51,255,0);stop-opacity:1" />' +
            '<stop offset="100%" style="stop-color:rgb(255,0,0);stop-opacity:1" />' +
            '</linearGradient></defs>' +
            '<circle class="shape" cx="10" cy="10" r="8" fill="url(#grad1)" /></svg>'

        return template;
    }

    // GRID MENU
    public addGridMenuItem(menuItem: uiGrid.IMenuItem, count: number = 10, time: number = 150) {
        if (!this.gridApi || !this.gridApi.core || !this.gridApi.grid || !this.gridApi.grid['gridMenuScope']) {
            if (count-- > 0)
                this.$timeout(() => this.addGridMenuItem(menuItem, count, time * 2), time);
            return;
        }
        this.gridApi.core.addToGridMenu(this.gridApi.grid, [menuItem]);
    }

    // LAYOUT

    public setMinRowsToShow(nbrOfRows: number) {
        this.gridOptions.minRowsToShow = nbrOfRows;
    }

    public refreshGrid() {
        if (this.gridApi && this.gridApi.core)
            this.gridApi.core.notifyDataChange(this.uiGridConstants.dataChange.ALL);
    }

    public refreshRows() {
        if (this.gridApi && this.gridApi.core)
            this.gridApi.core.refreshRows();
    }

    public refreshColumns() {
        if (this.gridApi && this.gridApi.core)
            this.gridApi.core.notifyDataChange(this.uiGridConstants.dataChange.COLUMN);
    }

    public resize() {
        if (this.gridApi && this.gridApi.core) {
            this.gridApi.core.handleWindowResize();
        }
    }
    // DATA

    public clearData() {
        if (this.gridOptions.data) {
            this.gridOptions.data.length = 0;
        }
    }

    public setData(rows: any[]) {
        if (rows !== null) {
            this.gridOptions.data = rows;
        } else {
            this.clearData();
        }
    }

    public getData() {
        return this.gridOptions.data;
    }

    public addRow(row: any, setFocus: boolean = false, focusColumnNumber: number = 0) {
        if (row !== null) {
            this.gridOptions.data.push(row);
            if (setFocus) {
                this.focusRowByRow(row, focusColumnNumber);
            }
        }
    }

    public deleteRow(row: any) {
        var index: number = this.gridOptions.data.indexOf(row);
        this.gridOptions.data.splice(index, 1);
    }

    public setRowInvisible(gridRow: uiGrid.IGridRow) {
        if (this.gridApi && this.gridApi.core)
            this.gridApi.core['setRowInvisible'](gridRow);
    }

    public clearRowInvisible(gridRow: uiGrid.IGridRow) {
        if (this.gridApi && this.gridApi.core)
            this.gridApi.core.clearRowInvisible(gridRow);
    }

    public selectRow(row: any, forceGridUpdate: boolean = false) {
        if (this.gridApi && this.gridApi.selection) {
            // Needed if the row has just been added
            if (forceGridUpdate && this.gridApi.grid)
                this.gridApi.grid.modifyRows(this.gridOptions.data);
            this.gridApi.selection.selectRow(row, null);
        }
    }
    public selectAllRows() {
        if (this.gridApi && this.gridApi.selection) {
            this.gridApi.selection.selectAllRows(null);
        }
    }

    public selectRowByVisibleIndex(index: number, forceGridUpdate: boolean = false) {
        if (this.gridApi && this.gridApi.grid && this.gridApi.selection) {
            // Needed if the row has just been added
            if (forceGridUpdate)
                this.gridApi.grid.modifyRows(this.gridOptions.data);
            this.gridApi.selection.selectRowByVisibleIndex(index, null);
        }
    }

    public unSelectRow(row: any, forceGridUpdate: boolean = false) {
        if (this.gridApi && this.gridApi.selection) {
            // Needed if the row has just been added
            if (forceGridUpdate && this.gridApi.grid)
                this.gridApi.grid.modifyRows(this.gridOptions.data);
            this.gridApi.selection.unSelectRow(row, null);
        }
    }

    public clearSelectedRows() {
        if (this.gridApi !== null && this.gridApi.grid !== null && this.gridApi.selection !== null) {
            this.gridApi.selection.clearSelectedRows();
        }
    }

    public findInData<T>(predicate: (item: any) => boolean) {
        return _.find(this.gridOptions.data, predicate);
    }

    public findAllInData<T>(predicate: (item: any) => boolean) {
        return _.filter(this.gridOptions.data, predicate);
    }

    public getCurrentRow(): any {
        if (this.gridApi !== null && this.gridApi.cellNav !== null)
            var rowCol = this.gridApi.cellNav.getFocusedCell();

        if (rowCol !== null) {
            return rowCol.row.entity;
        }

        return null;
    }

    public getCurrentRowCol(): uiGrid.cellNav.IRowCol<any> {
        if (this.gridApi !== null && this.gridApi.cellNav !== null)
            var rowCol = this.gridApi.cellNav.getFocusedCell();

        if (rowCol !== null) {
            return rowCol;
        }

        return null;
    }

    public getSelectedRows(): any[] {

        if (this.gridApi && this.gridApi.selection)
            return this.gridApi.selection.getSelectedRows();
        else
            return [];
    }

    public getFilteredRows(): any[] {
        if (this.gridApi)
            return this.gridApi.core.getVisibleRows(this.gridApi.grid);
        else
            return [];
    }

    public getSelectedCount(): number {
        if (this.gridApi && this.gridApi.selection) {
            return this.gridApi.grid['selection'].selectedCount;
        }
        else {
            return 0;
        }
    }

    public getSelectedIds(idField: string): number[] {
        var rows = this.getSelectedRows();
        var ids = [];
        _.forEach(rows, (row) => {
            ids.push(row[idField]);
        });

        return ids;
    }

    public getDirectiveName(): string {
        return this.gridControllerName;
    }

    public getCurrentCellValue(): string {
        var values = [];
        var currentSelection = this.gridApi.cellNav.getCurrentSelection();
        for (var i = 0; i < currentSelection.length; i++) {
            values.push(currentSelection[i].row.entity[currentSelection[i].col.name]);
        }
        return values.toString();
    }

    public scrollToFocus(row: any, columnIndex: number, refocusCell: boolean = false) {
        if (this.gridOptions.data.length === 0)
            return;

        if (row == null) {
            row = this.gridOptions.data[0];
        }

        if (this.gridApi && this.gridApi.cellNav) {
            if (refocusCell) {
                if (typeof row === 'number') {
                    row = this.gridOptions.data[row];
                }
                this.$timeout(() => {
                    this.gridApi.grid.scrollTo(row).then(() => this.gridApi.cellNav.scrollToFocus(row, this.gridOptions.columnDefs[columnIndex]));
                }, 100);
            }
            else {
                this.$timeout(() => {
                    if (typeof row === 'number') {
                        this.gridApi.cellNav.scrollToFocus(this.gridOptions.data[row], this.gridOptions.columnDefs[columnIndex]);
                    } else {
                        this.gridApi.cellNav.scrollToFocus(row, this.gridOptions.columnDefs[columnIndex]);
                    }
                }, 100);

            }
        }
    }

    public focusRowByIndex(rowIndex: number, columnIndex: number) {
        this.scrollToFocus(rowIndex, columnIndex);
    }

    public focusRowByRow(row: any, columnIndex: number) {
        this.scrollToFocus(row, columnIndex);
    }

    public focusColumn(columnIndex: number) {
        this.scrollToFocus(this.getCurrentRow(), columnIndex);
    }

    public refocusCell() {
        // Get current column
        var currentCol = this.getCurrentRowCol();
        if (!currentCol)
            return;

        var field = currentCol.col.field;
        var index = this.getColumnIndex(field);

        // Need to move focus to another column first
        this.focusColumn(index !== 0 ? 0 : index + 1);
        this.$timeout(() => {
            this.focusColumn(index);
        });
    }

    public getColumnIndex(field: string) {
        return _.findIndex(this.getColumnDefs(), (colDef: uiGrid.IColumnDef) => colDef.field === field);
    }

    public sortFirst(sortprop?: string) {
        if (!sortprop)
            sortprop = 'sort';

        // Get current row
        var rowItem = this.getCurrentRow();
        if (rowItem != null && rowItem[sortprop] > 1) {
            // Move row to the top
            rowItem[sortprop] = -1;
            this.reNumberRows(sortprop);
            this.scrollToFocus(rowItem, 1);
        }
    }

    public sortUp(sortprop?: string) {
        if (!sortprop)
            sortprop = 'sort';
        // Get current row
        var rowItem = this.getCurrentRow();
        if (rowItem != null && rowItem[sortprop] > 1) {
            var filterObj = {};
            filterObj[sortprop] = rowItem[sortprop] - 1;
            // Get previous row
            var prevRowItem = (_.filter(this.gridOptions.data, filterObj))[0];

            // Move row up
            if (prevRowItem != null) {
                // Multiply each row number by 10, to be able to insert row numbers in between
                this.multiplyRowNr(sortprop);

                // Move current row before previous row
                rowItem[sortprop] -= 19;
                this.reNumberRows(sortprop);
                this.scrollToFocus(rowItem, 1);
            }
        }
    }

    public sortDown(sortprop?: string) {
        if (!sortprop)
            sortprop = 'sort';

        // Get current row
        var rowItem = this.getCurrentRow();
        if (rowItem != null && rowItem[sortprop] < this.gridOptions.data.length) {
            var filterObj = {};
            filterObj[sortprop] = rowItem[sortprop] + 1;
            // Get next row
            var nextRowItem = (_.filter(this.gridOptions.data, filterObj))[0];
            // Move row down
            if (nextRowItem) {
                // Multiply each row number by 10, to be able to insert row numbers in between
                this.multiplyRowNr(sortprop);

                // Move current row after next row                    
                rowItem[sortprop] += 12;
                this.reNumberRows(sortprop);
                this.scrollToFocus(rowItem, 1);
            }
        }
    }

    public sortLast(sortprop?: string) {
        if (!sortprop)
            sortprop = 'sort';

        // Get current row
        var rowItem = this.getCurrentRow();
        if (rowItem && rowItem[sortprop] < this.gridOptions.data.length) {
            // Move row to the bottom
            rowItem[sortprop] = NumberUtility.max(this.gridOptions.data, sortprop) + 2;
            this.reNumberRows(sortprop);
            this.scrollToFocus(rowItem, 1);
        }
    }

    public reNumberRows(sortprop?: string) {
        if (!sortprop)
            sortprop = 'sort';

        //this sorts inline, keeping all data-bindings and references intact.
        this.gridOptions.data.sort((r1, r2) => {
            if (r1[sortprop] < r2[sortprop])
                return -1;

            if (r1[sortprop] > r2[sortprop])
                return 1;

            return 0;
        });
        var i: number = 0;
        _.forEach(this.gridOptions.data, (row: any) => {
            i++;
            if (row[sortprop] !== i) {
                row[sortprop] = i;
                row.isModified = true;
            }
        });
    }

    // FILTERS

    public clearFilters() {
        // Clear all column filters
        _.forEach(this.gridApi.grid.columns, (col: any) => {
            if (col.enableFiltering) {
                if (col.colDef.type === "boolean") {
                    // Set default filter on boolean column
                    col.filter.term = (col.colDef.field === "isActive") ? true : null;
                    col.filter.termIndeterminate = true;
                } else if (col.colDef.type === "date" || col.colDef.type === "shape") {
                    col.filters.forEach(filter => filter.term = undefined);
                } else {
                    col.filter.term = undefined;
                }
            }
        });
    }

    // SAVE STATE

    public saveDefaultState(callback: (name: string, data: string) => ng.IPromise<any>): ng.IPromise<any> {
        var gridState = this.gridApi.saveState.save();
        return callback(this.getGridName(), JSON.stringify(gridState));
    }

    public restoreDefaultState(callback: (name: string) => ng.IPromise<string>) {
        // First check if there is a default (sys) state
        var sysExists: boolean = false;
        if (callback) {
            callback(this.getGridName()).then((state) => {
                if (state && state.length > 0) {
                    sysExists = true;
                    this.gridApi.saveState.restore(<any>this, JSON.parse(state));
                };
            });
        }

        // Next get full grid state
        if (!sysExists && this.defaultGridState)
            this.gridApi.saveState.restore(<any>this, this.defaultGridState);
    }

    public deleteDefaultState(callback: (name: string) => ng.IPromise<any>) {
        callback(this.getGridName()).then((result) => {
            if (result.success) {
                this.restoreDefaultState(null);
            }
        });
    }

    public saveState(callback: (name: string, data: string) => ng.IPromise<any>): ng.IPromise<any> {
        var gridState = this.gridApi.saveState.save();
        return callback(this.getGridName(), JSON.stringify(gridState));
    }

    public restoreState(callback: (name: string) => ng.IPromise<string>, saveCurrentAsDefault: boolean) {
        // No need to restore state if user can't save any
        if (!this.gridOptions.enableGridMenu)
            return;

        callback(this.getGridName()).then((state) => {
            // Save default state to be able to restore to that in method restoreDefaultState
            if (state && state.length > 0) {
                var fn = () => {
                    if (this.gridApi && this.gridApi.saveState) {
                        if (saveCurrentAsDefault)
                            this.defaultGridState = this.gridApi.saveState.save();
                        this.$timeout(() => { this.gridApi.saveState.restore(<any>this, JSON.parse(state)); }, 10);
                    }
                    else {
                        this.$timeout(fn, 100);
                    }
                };
                fn();
            }
        });
    }

    public deleteState(callback: (name: string) => ng.IPromise<any>, callback2: (name: string) => ng.IPromise<any>) {
        callback(this.getGridName()).then((result) => {
            if (result.success) {
                this.restoreDefaultState(callback2);
            }
        });
    }

    // EVENTS

    // TODO: Testa detta:
    //private events2: {};
    //this.events2[SoeGridOptionsEvent[SoeGridOptionsEvent.BeginCellEdit]] = [];

    private events: GridEvent[];
    public subscribe(events: GridEvent[]) {
        if (!this.events)
            this.events = events;
        else
            events.forEach(x => this.events.push(x));
    }

    private beginCellEdit(rowEntity, colDef) {
        var funcs = this.getEventFunctions(SoeGridOptionsEvent.BeginCellEdit);
        funcs.forEach(f => f(rowEntity, colDef));
    }

    private afterCellEdit(rowEntity, colDef, newValue, oldValue) {
        if (newValue != oldValue)
            rowEntity.isModified = true;
        var funcs = this.getEventFunctions(SoeGridOptionsEvent.AfterCellEdit);
        funcs.forEach(f => f(rowEntity, colDef, newValue, oldValue));
    }

    public cancelCellEdit(rowEntity, colDef) {
        var func = this.getEventFunction(SoeGridOptionsEvent.CancelCellEdit);
        if (func)
            func(rowEntity, colDef);
    }

    private rowSelectionChanged(row) {
        var func: any = this.getEventFunction(SoeGridOptionsEvent.RowSelectionChanged);
        if (func)
            func(row);
    }

    private rowsVisibleChanged(rows) {
        var func: any = this.getEventFunction(SoeGridOptionsEvent.RowsVisibleChanged);
        if (func)
            func(rows);
    }

    private rowSelectionChangedBatch(rows) {
        var func: any = this.getEventFunction(SoeGridOptionsEvent.RowSelectionChangedBatch);
        if (func)
            func(rows);
    }

    private navigate(newRowCol, oldRowCol) {
        var func: any = this.getEventFunction(SoeGridOptionsEvent.Navigate);
        if (func)
            func(newRowCol, oldRowCol);
    }

    export(row, col, input) {
        var func: any = this.getEventFunction(SoeGridOptionsEvent.Export);
        if (func)
            return func(row, col, input);
        else
            return input;
    }

    private rowExpandedStateChanged(row) {
        let func: any = this.getEventFunction(row.isExpanded ? SoeGridOptionsEvent.RowExpanded : SoeGridOptionsEvent.RowCollapsed);
        if (func)
            func(row);

        this.updateDynamicHeight(row, true);
    }

    private renderingComplete() {
        //created in constructor to fix ui-grid problems...

        var func: any = this.getEventFunction(SoeGridOptionsEvent.RenderingComplete);
        if (func)
            func();
    }

    private rowsRendered() {
        var func: any = this.getEventFunction(SoeGridOptionsEvent.RowsRendered);
        if (func)
            func();
    }

    private filterChanged() {
        var func: any = this.getEventFunction(SoeGridOptionsEvent.FilterChanged);
        if (func)
            func();
    }

    public updateDynamicHeight(row, isAuto?) {
        if (this.dynamicHeight) {
            if (this.useAutoHeight) {
                $('.grid' + this.gridApi.grid['id']).height('auto');
                $('.grid' + this.gridApi.grid['id'] + ' .ui-grid-canvas').height('auto');
                return;
            }

            if (!row.entity.subGridOptions)
                return;

            var height = row.entity.subGridOptions.gridOptions.data.length * row.grid.options.rowHeight + row.grid.headerHeight + row.grid.scrollbarHeight;
            var old = row.expandedRowHeight;
            row.expandedRowHeight = height;

            if (row.isExpanded && !isAuto)
                height -= old;

            if (!row.isExpanded)
                height = -1 * height;

            var lastRow = row;
            var r = row.grid.parentRow;
            while (r) {
                r.expandedRowHeight += height;
                lastRow = r;
                r = r.grid.parentRow;
            }

            if (lastRow) {
                var gridId = lastRow.grid.id;
                this.$timeout(() => {
                    var heightNeeded = $('.grid' + gridId + ' .ui-grid-viewport').prop('scrollHeight') - $('.grid' + gridId + ' .ui-grid-viewport').innerHeight();

                    if (heightNeeded > 0) {
                        var cheight = $('.grid' + gridId).height();
                        $('.grid' + gridId).height(cheight + heightNeeded);
                        $('.grid' + gridId + ' .ui-grid-canvas').height('auto');
                    }
                }, 100);
            }
        }
    }

    private dynamicHeight = false;
    public enableDynamicHeight() {
        this.dynamicHeight = true;
    }

    private useAutoHeight: boolean;
    public enableAutoHeight() {
        this.useAutoHeight = true;
    }

    private getEventFunction(ev: SoeGridOptionsEvent): any {
        var ret = null;

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

    private getGridName() {
        return this.name.replace(/\./g, '_');
    }

    private multiplyRowNr(sortprop?: string) {
        if (!sortprop)
            sortprop = 'sort';

        _.forEach(this.gridOptions.data, (x: any) => {
            x[sortprop] *= 10;
        });
    }
}

export class GridEvent {
    constructor(public event: SoeGridOptionsEvent, public func = (...args: any[]) => { }) {
    }
}

export class UIGridMenuItem implements uiGrid.IMenuItem {
    title: string;
    icon: string;
    action: ($event: ng.IAngularEvent) => void;
    shown: () => boolean;
    active: () => boolean;
    context: any;
    leaveOpen: boolean;
    order: number;

    constructor(title: string, order: number, icon?: string, context?: any, action?: ($event, context) => void) {
        this.title = title;
        this.order = order;
        this.icon = icon;
        if (action) {
            this.action = ($event) => { action($event, context) };
        }
    }
}

export class UIGridExportHeader {
    text: string;
    style: string;
}

export class TypeAheadOptions {
    // NOTE: dont forget to call this.setupTypeAhead(); in your controller since that is what enables most callbacks
    // NOTE: For a concerete example, just check AccountingRowsDirective.ts
    // NOTE: You also need to check AccountingRows.Html, because you need to add grid-keypress="directiveCtrl.handleKeyPressInEditCell" to your grid to allow navigation from the typeahead.

    // INTERESTING FACT: There is actually a race condition in allowNavigationFromTypeAhead
    // where it will use the old model value instead of the new.
    // That does not matter (in my use case, dont know about yours) though since it can only happen when the typeahead is open,
    // which in turn makes sure the right value gets set afterwards.Just in case you get confused in debugging.

    displayField: string;
    secondRowBinding: string;               // Example: Set to 'name' if the object have a prop named name that you want to bind to.
    selectedItemChangedMethodParameters: string = 'colDef, data, $item';  // The params to the selectedItemChanged method.
    ctrlName: string = 'directiveCtrl';     // The controller name, used for binding stuff to the controller
    typeAhead: string;                      // The actual type-ahead binding, see the typeahead documentation. example: 'item in grid.appScope.ctrl.items | filter:$viewValue | limitTo:5'
    additionalData: any;                    // Any additional data you might need in your callbacks.
    errorBinding: string;                   // Binding to an eventual error message.

    getSecondRowBindingValue: Function;     // Called with entity, colDef. Used to get the value that will be set on the secondRowBinding
    allowNavigationFromTypeAhead: Function; // Called with entity, colDef. Used to allow/prevent navigation from the typeahead. 
    onBlur: Function;                       // Called with entity, colDef. Used to do whatever you want. Runs after all other bindings on edit-end. 

    buttonIcon: string;
    buttonTooltipKey: string;
    buttonClickEvent: string;

    constructor(typeAhead: string) {
        this.typeAhead = typeAhead;         // typeAhead is required.
    }
}