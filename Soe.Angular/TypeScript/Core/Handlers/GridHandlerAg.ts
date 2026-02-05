import { INotificationService } from "../Services/NotificationService";
import { ICoreService } from "../Services/CoreService";
import { ITranslationService } from "../Services/TranslationService";
import { SoeGridOptionsAg, ISoeGridOptionsAg, CellChangedCallback, FieldOrPredicate, IconColumnOptions, NumberColumnOptions, SelectColumnOptions, TypeAheadColumnOptions, DataCallback, TextColumnOptions, TimeColumnOptions, DateColumnOptions, ShapeColumnOptions, BoolColumnOptions, DateTimeColumnOptions } from "../../Util/SoeGridOptionsAg";
import { SOEMessageBoxImage, SOEMessageBoxButtons } from "../../Util/Enumerations";
import { GridMenuBuilder, IMenuItem } from "../../Util/ag-grid/GridMenuBuilder";
import { ToolBarButtonGroup, ToolBarUtility } from "../../Util/ToolBarUtility";
import { Constants } from "../../Util/Constants";
import { IMessagingService } from "../Services/MessagingService";

export interface IGridHandlerAg {
    //TypeAhead
    currentlyEditing: {
        entity: any;
        colDef: any; //ag.grid.ColDef
    };

    findNextRow(row);
    findNextRowInfo(row, usingDetail?: boolean): { rowIndex: number, rowNode: any };
    findPreviousRowInfo(row, usingDetail?: boolean): { rowIndex: number, rowNode: any };

    setExporterFilenamesAndHeader(baseText: string);
    addGridMenuItems(items: IMenuItem[] | string[]);
    addStandardMenuItems();
    setupSortGroup(sortProp: string, disabled?: () => void, hidden?: () => void);
    clearData();
    clearFilters();

    enableMasterDetail(addDefaultExpanderCol: boolean, detailHeight?: number, detailHeightByChildCollectionName?: string, autoHeight?: boolean);

    setData(data: any);
    finalizeInitGrid(exportFileName: string, addTotals: boolean, totalsRowName?: string, setIsActiveDefaultFilter?: boolean);

    findInData(predicate: (item: any) => boolean): any[];

    saveState();
    deleteState();
    setDefaultState();
    saveDefaultState();
    deleteDefaultState();
    restoreState();

    addColumnBool(field: string, displayName: string, width: number, enableCellEdit?: boolean, onChanged?: CellChangedCallback, disabledField?: string, termIndeterminate?: boolean, useSetFilter?: boolean, filterOptions?: any[], filterLabel?: string, setChecked?: boolean): any;
    addColumnBoolEx(field: string, displayName: string, width: number, options?: BoolColumnOptions, headerColumnDef?: any): any;
    addColumnActive(field: string, displayName?: string, width?: number, onChanged?: CellChangedCallback): any;
    addColumnIcon(field: string, headerName?: string, width?: number, options?: IconColumnOptions, headerColumnDef?: any): any;
    addColumnEdit(toolTip: string, onClickEvent?: DataCallback, isSubgrid?: boolean, showIcon?: FieldOrPredicate): any;
    addColumnDelete(toolTip: string, onClick?: DataCallback, isSubgrid?: boolean, showIcon?: FieldOrPredicate, icon?: string, headerColumnDef?: any): any;
    addColumnPdf(toolTip: string, onClickEvent?: DataCallback): any;
    addColumnIsModified(field?: string, displayName?: string, width?: number, headerColumnDef?: any): any;
    addColumnText(field: string, displayName: string, width: number, enableHiding?: boolean, options?: TextColumnOptions, headerColumnDef?: any): any;
    addColumnHtml(field: string, displayName: string, width: number, enableHiding?: boolean): any;
    addColumnNumber(field: string, headerName: string, width: number, options?: NumberColumnOptions, headerColumnDef?: any): any;
    addColumnSelect(field: string, headerName: string, width: number, options?: SelectColumnOptions, headerColumnDef?: any): any;
    addColumnDateTime(field: string, displayName: string, width: number, enableHiding?: boolean, cellFilter?: string, options?: DateTimeColumnOptions): any;
    addColumnDate(field: string, displayName: string, width: number, enableHiding?: boolean, cellFilter?: string, options?: DateColumnOptions, headerColumnDef?: any): any;
    addColumnTime(field: string, displayName: string, width: number, options?: TimeColumnOptions, headerColumnDef?: any): any;
    addColumnTimeSpan(field: string, displayName: string, width: number, options?: TimeColumnOptions, headerColumnDef?: any): any;
    addColumnShape(field: string, displayName?: string, width?: number, options?: ShapeColumnOptions, headerColumnDef?: any): any;
    addColumnTypeAhead(field: string, headerName?: string, width?: number, options?: TypeAheadColumnOptions, soeData?: any, headerColumnDef?: any): any;

    options: ISoeGridOptionsAg;
    detailOptions: ISoeGridOptionsAg;
}

export class GridHandlerAg implements IGridHandlerAg {
    //Terms
    terms: { [index: string]: string; };

    //TypeAhead
    public currentlyEditing: {
        entity: any;
        colDef: any;
    };
    private preventNextEnterKey = false;
    //TypeAhead

    constructor(name: string, private $timeout: ng.ITimeoutService, private uiGridConstants: uiGrid.IUiGridConstants, private coreService: ICoreService,
        private translationService: ITranslationService, private notificationService: INotificationService, private messagingService: IMessagingService) {
        this.options = SoeGridOptionsAg.create(name, $timeout);
        this.options.translateText = this.translator;
        this.initGridText();
    }

    private translator = (key: string, defaultValue: string) => {
        const termKey = "core.aggrid." + key;
        if (this.terms && this.terms[termKey])
            return this.terms[termKey];
        else {
            const term = this.translationService.translateInstant(termKey);
            if (term !== termKey) {
                return term;
            } else {
                return defaultValue;
            }
        }
    }

    initGridText() {
        const keys: string[] = [
            "core.yes",
            "core.no",
            "core.aggrid.pinColumn",
            "core.aggrid.pinLeft",
            "core.aggrid.pinRight",
            "core.aggrid.noPin",
            "core.aggrid.autosizeAllColumns",
            "core.aggrid.equals",
            "core.aggrid.notEqual",
            "core.aggrid.lessThan",
            "core.aggrid.lessThanOrEqual",
            "core.aggrid.greaterThan",
            "core.aggrid.greaterThanOrEqual",
            "core.aggrid.inRange",
            "core.aggrid.startsWith",
            "core.aggrid.endsWith",
            "core.aggrid.contains",
            "core.aggrid.notContains",
            "core.aggrid.selectAll",
            "core.aggrid.searchOoo",
            "core.aggrid.filterOoo",
            "core.aggrid.rowGroupColumnsEmptyMessage",
            "core.aggrid.group",
            "core.aggrid.andCondition",
            "core.aggrid.orCondition",
            "core.aggrid.blanks"
        ];
        this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    setExporterFilenamesAndHeader(translationKey: string) {
        if (translationKey) {
            const keys: string[] = [
                translationKey
            ];

            this.translationService.translateMany(keys).then((terms) => {
                this.options.exportFilename = terms[translationKey];
            });
        }
    }

    addGridMenuItems(items: IMenuItem[] | string[]) {
        _.forEach<IMenuItem | string>(items, (item) => {
            this.options.addGridMenuItem(item);
        });
    }

    clearData() {
        this.options.clearData();
    }
    clearFilters() {
        this.options.clearFilters();
    }

    enableMasterDetail(addDefaultExpanderCol: boolean, detailHeight: number = undefined, detailHeightByChildCollectionName?: string, autoHeight?: boolean) {
        if (addDefaultExpanderCol) {
            const expanderColumn = this.options.addColumnText("expander", "", 100, {
                //pinned: "left",
                enableResizing: false,
                enableHiding: false
            });
            expanderColumn.cellRenderer = 'agGroupCellRenderer';
            expanderColumn.filter = false;
            expanderColumn.suppressSizeToFit = true;
            expanderColumn.width = 20;
            expanderColumn.suppressExport = true;

            expanderColumn.cellClass = "soe-ag-cell-expander";
        }

        this.detailOptions = SoeGridOptionsAg.create("detailGridOptions", this.$timeout);

        this.options.enableMasterDetail(this.detailOptions, detailHeight, detailHeightByChildCollectionName, autoHeight);
    }

    setData(data: any) {
        this.options.setData(data);
    }

    findInData(predicate: (item: any) => boolean): any[] {
        return this.options.findAllInData(predicate);
    }

    saveState() {
        this.options.saveState((name, data) => this.coreService.saveUserGridState(name, data).then(() => this.restoreState()));
    }
    setDefaultState() {
        this.options.restoreDefaultState((name) => this.coreService.getSysGridState(name));
    };
    deleteState() {
        this.options.deleteState((name) => this.coreService.deleteUserGridState(name), (name) => this.coreService.getSysGridState(name));
    };
    deleteDefaultState() {
        const keys: string[] = [
            "core.warning",
            "core.uigrid.deletedefaultstatewarning",
            "core.enterpassword",
            "core.wrongpassword"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            var modal = this.notificationService.showDialogEx(terms["core.warning"], terms["core.uigrid.deletedefaultstatewarning"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel, { showTextBox: true, textBoxLabel: terms["core.enterpassword"] });
            modal.result.then(val => {
                modal.result.then(result => {
                    if (result.result) {
                        if (result.textBoxValue === 'Fiskpinne36!') {
                            this.options.deleteDefaultState((name) => this.coreService.deleteSysGridState(name));
                        } else {
                            this.notificationService.showDialogEx(terms["core.warning"], terms["core.wrongpassword"], SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK);
                        }
                    }
                });
            });
        });
    };
    saveDefaultState() {
        const keys: string[] = [
            "core.warning",
            "core.uigrid.savedefaultstatewarning",
            "core.enterpassword",
            "core.wrongpassword"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            const modal = this.notificationService.showDialogEx(terms["core.warning"], terms["core.uigrid.savedefaultstatewarning"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel, { showTextBox: true, textBoxLabel: terms["core.enterpassword"] });
            modal.result.then(result => {
                if (result.result) {
                    if (result.textBoxValue === 'Fiskpinne36!') {
                        this.options.saveDefaultState((name, data) => this.coreService.saveSysGridState(name, data));
                    } else {
                        this.notificationService.showDialogEx(terms["core.warning"], terms["core.wrongpassword"], SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK);
                    }
                }
            });
        });
    };
    restoreState() {
        this.options.restoreState((name) => this.coreService.getUserGridState(name), true);
    };

    // Boolean (checkbox)      
    addColumnBool(field: string, displayName: string, width: number, enableCellEdit?: boolean, onChanged?: CellChangedCallback, disabledField?: string, termIndeterminate?: boolean, useSetFilter?: boolean, filterOptions?: any[], filterLabel?: string, setChecked?: boolean): any {
        //return this.options.addColumnBool(field, displayName, width, enableCellEdit, onChanged, disabledField, termIndeterminate);
        return this.options.addColumnBool(field, displayName, width, { enableEdit: enableCellEdit, onChanged: onChanged, disabledField: disabledField, termIndeterminate: termIndeterminate, useSetFilter: useSetFilter, filterOptions: filterOptions ? filterOptions : [{ value: 'true', text: this.terms["core.yes"] }, { value: 'false', text: this.terms["core.no"] }], filterLabel: filterLabel, setChecked: setChecked });
    }

    addColumnBoolEx(field: string, displayName: string, width: number, options?: BoolColumnOptions, headerColumnDef?: any): any {
        //return this.options.addColumnBool(field, displayName, width, enableCellEdit, onChanged, disabledField, termIndeterminate);
        return this.options.addColumnBool(field, displayName, width, options, headerColumnDef);
    }

    // Active
    addColumnActive(field: string, displayName?: string, width?: number, onChanged?: CellChangedCallback): any {
        return this.options.addColumnActive(field, displayName, width, onChanged);
    }

    addColumnIcon(field: string, headerName?: string, width?: number, options?: IconColumnOptions, headerColumnDef?: any): any {
        return this.options.addColumnIcon(field, headerName, width, options, headerColumnDef);
    }

    // Edit
    addColumnEdit(toolTip: string, onClickEvent?: DataCallback, isSubgrid?: boolean, showIcon?: FieldOrPredicate): any {
        return this.options.addColumnEdit(toolTip, onClickEvent, isSubgrid, showIcon);
    }

    // Delete
    addColumnDelete(toolTip: string, onClick?: DataCallback, isSubgrid?: boolean, showIcon?: FieldOrPredicate, icon?: string, headerColumnDef?: any): any {
        return this.options.addColumnDelete(toolTip, onClick, isSubgrid, showIcon, icon, headerColumnDef);
    }

    // PDF
    addColumnPdf(toolTip: string, onClickEvent?: DataCallback): any {
        return this.options.addColumnPdf(toolTip, onClickEvent);
    }

    // IsModified
    addColumnIsModified(field?: string, displayName?: string, width?: number, clickCallback?: (params: any) => void, headerColumnDef?: any): any {
        return this.options.addColumnIsModified(field, displayName, width, clickCallback, headerColumnDef);
    }

    // HTML
    addColumnHtml(field: string, displayName: string, width: number, enableHiding: boolean = false): any {
        return this.options.addColumnHtml(field, displayName, width, enableHiding);
    }

    // Text
    addColumnText(field: string, headerName: string, width: number, enableHiding: boolean, options?: TextColumnOptions, headerColumnDef?: any): any {
        if (!options)
            options = new TextColumnOptions();
        options.enableHiding = enableHiding;
        return this.options.addColumnText(field, headerName, width, options, headerColumnDef);
    }

    // Number
    addColumnNumber(field: string, headerName: string, width: number, options?: NumberColumnOptions, headerColumnDef?: any): any {
        return this.options.addColumnNumber(field, headerName, width, options, headerColumnDef);
    }

    // Select
    addColumnSelect(field: string, headerName: string, width: number, options?: SelectColumnOptions, headerColumnDef?: any): any {
        return this.options.addColumnSelect(field, headerName, width, options, headerColumnDef);
    }

    // DateTime
    addColumnDateTime(field: string, displayName: string, width: number, enableHiding: boolean = false, cellFilter: string = null, options?: DateTimeColumnOptions): any {
        return this.options.addColumnDateTime(field, displayName, width, enableHiding, null, cellFilter, options);
    }

    // Date
    addColumnDate(field: string, displayName: string, width: number, enableHiding: boolean = false, cellFilter: string = null, options?: DateColumnOptions, headerColumnDef?: any): any {
        return this.options.addColumnDate(field, displayName, width, enableHiding, headerColumnDef, cellFilter, options);
    }

    // Time
    addColumnTime(field: string, displayName: string, width: number, options?: TimeColumnOptions, headerColumnDef?: any): any {
        return this.options.addColumnTime(field, displayName, width, options, headerColumnDef);
    }

    // TimeSpan
    addColumnTimeSpan(field: string, displayName: string, width: number, options?: TimeColumnOptions, headerColumnDef?: any): any {
        return this.options.addColumnTimeSpan(field, displayName, width, options, headerColumnDef);
    }

    // Shape
    addColumnShape(field: string, displayName?: string, width?: number, options?: ShapeColumnOptions, headerColumnDef?: any): any {
        return this.options.addColumnShape(field, displayName, width, options, headerColumnDef);
    }

    // TypeAhead
    addColumnTypeAhead(field: string, headerName?: string, width?: number, options?: TypeAheadColumnOptions, soeData?: any, headerColumnDef?: any): any {
        return this.options.addColumnTypeAhead(field, headerName, width, options, soeData, headerColumnDef);
    }

    public addStandardMenuItems(addPdfExportOption = false) {
        const gridMenuBuilder = new GridMenuBuilder(this.options, this.translationService, this.coreService, this.notificationService);
        gridMenuBuilder.buildDefaultMenu(addPdfExportOption);
        this.restoreState();
    }

    public selectedItemChanged(colDef: any, entity: any, selectedItem: any) {
        this.preventNextEnterKey = true;
        (this as any).$scope.$broadcast('uiGridEventEndCellEdit');//this is the string value of uiGridEditConstants.events.END_CELL_EDIT. I would need to change 135 files to get it in the correct way, which seems like overkill.

        this.options.tabToNextCell();
    }

    public finalizeInitGrid(exportFileName: string, addTotals: boolean, totalsRowName?: string, setIsActiveDefaultFilter?: boolean) {
        this.addStandardMenuItems(false);
        const keys: string[] = [
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total",
            "core.aggrid.totals.selected"
        ];

        if (exportFileName)
            keys.push(exportFileName)

        this.translationService.translateMany(keys).then((terms) => {
            if (exportFileName)
                this.options.exportFilename = terms[exportFileName];

            if (addTotals) {
                if (!totalsRowName)
                    totalsRowName = 'totals-grid';

                this.options.addTotalRow("#" + totalsRowName, {
                    filtered: terms["core.aggrid.totals.filtered"],
                    total: terms["core.aggrid.totals.total"],
                    selected: terms["core.aggrid.totals.selected"]
                });
            }

            this.options.finalizeInitGrid(undefined, setIsActiveDefaultFilter);
        });
    }

    protected handleNavigateToNextCell(params): { rowIndex: number, column: any } {
        return params.nextCellPosition;
    }

    public findNextRow(row) {
        var index = this.findIndex(row);
        var data = this.options.getData();

        if (index === data.length - 1)
            return null;

        return data[index + 1];
    }

    public findNextRowInfo(row, usingDetail: boolean = false): { rowIndex: number, rowNode: any } {
        const result = this.options.getNextRow(row, usingDetail);

        return !!result.rowNode ? result : null;
    }

    public findPreviousRowInfo(row, usingDetail: boolean = false): { rowIndex: number, rowNode: any } {
        const result = this.options.getPreviousRow(row, usingDetail);

        return !!result.rowNode ? result : null;
    }

    public setupSortGroup(sortProp: string = "sort", disabled?: () => void, hidden?: () => void) {
        const group = ToolBarUtility.createSortGroup(
            () => {
                this.options.sortFirst(sortProp);
                this.messagingService.publish(Constants.EVENT_SET_DIRTY, {});
            },
            () => {
                this.options.sortUp(sortProp);
                this.messagingService.publish(Constants.EVENT_SET_DIRTY, {});
            },
            () => {
                this.options.sortDown(sortProp);
                this.messagingService.publish(Constants.EVENT_SET_DIRTY, {});
            },
            () => {
                this.options.sortLast(sortProp);
                this.messagingService.publish(Constants.EVENT_SET_DIRTY, {});
            },
            disabled,
            hidden
        );
        this.sortMenuButtons.push(group);
    }
    private findIndex(row) {
        var entity = row.entity || row;

        // find real row by comparing $$hashKey with entity in row
        var rowIndex = -1;
        var hash = entity.$$hashKey;
        var data = this.options.getData();     // original rows of data
        for (var ndx = 0; ndx < data.length; ndx++) {
            if (data[ndx].$$hashKey === hash) {
                rowIndex = ndx;
                break;
            }
        }
        return rowIndex;
    }

    options: ISoeGridOptionsAg;
    detailOptions: ISoeGridOptionsAg;
    sortMenuButtons = new Array<ToolBarButtonGroup>();
}
