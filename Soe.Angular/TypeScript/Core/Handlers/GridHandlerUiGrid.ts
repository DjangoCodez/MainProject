import { ICoreService } from "../Services/CoreService";
import { INotificationService } from "../Services/NotificationService";
import { ITranslationService } from "../Services/TranslationService";
import { CoreUtility } from "../../Util/CoreUtility";
import { UIGridMenuItem, SoeGridOptions, TypeAheadOptions, ISoeGridOptions, GridEvent } from "../../Util/SoeGridOptions";
import { SOEMessageBoxImage, SOEMessageBoxButtons, SoeGridOptionsEvent } from "../../Util/Enumerations";
import { Constants } from "../../Util/Constants";

export interface IGridHandlerUiGrid {
    //TypeAhead
    currentlyEditing: {
        entity: any;
        colDef: uiGrid.IColumnDef;
    };
    findNextRow(row);
    //TypeAhead
    addSumAggregationFooterToColumns(...args: uiGrid.IColumnDef[])

    setExporterFilenamesAndHeader(baseText: string);
    addGridMenuItems(items: UIGridMenuItem[]);
    addStandardMenuItems();

    clearData();
    clearFilters();

    setData(data: any);

    findInData(predicate: (item: any) => boolean): any[];
    
    saveState();
    deleteState();
    setDefaultState();
    saveDefaultState();
    deleteDefaultState();
    restoreState();
    refresh();
    refreshColumns();
    resize();

    subscribe(events: any[]);

    setupTypeAhead();
    handleKeyPressInEditCell(evt);

    //functions for handling "activated" column
    initUpdateStates(idField: string): any;
    selectItem(id: number);
    clearSelectedItems();
    selectedItemsExist(): boolean;
    //---------------

    addColumn(columnDef): uiGrid.IColumnDef;
    addColumnBool(field: string, displayName: string, width: string, enableCellEdit?: boolean, clickEvent?: string, clickEventField?: string, disabledField?: string, termIndeterminate?: boolean): uiGrid.IColumnDef;
    addColumnActive(field: string, displayName?: string, width?: string, changeEventHandlerName?: string): uiGrid.IColumnDef;
    addColumnIcon(field: string, icon: string, toolTip: string, clickEvent: string, showIconField?: string, showIconFunction?: string, displayName?: string, width?: string, enableHiding?: boolean, enableResizing?: boolean, ctrlName?: string, isSubgrid?: boolean, tooltipField?: string): uiGrid.IColumnDef;
    addColumnEdit(toolTip: string, clickEvent?: string, ctrlName?: string, isSubgrid?: boolean): uiGrid.IColumnDef;
    addColumnDelete(toolTip: string, onDeleteEvent?: string, ctrlName?: string, isSubgrid?: boolean, showIconField?: string, showIconFunction?: string): uiGrid.IColumnDef;
    addColumnPdf(toolTip: string): uiGrid.IColumnDef;
    addColumnIsModified(field?: string, displayName?: string, width?: string): uiGrid.IColumnDef;
    addColumnText(field: string, displayName: string, width: string, enableHiding?: boolean, toolTipField?: string, toolTip?: string, className?: string, classFunction?: string): uiGrid.IColumnDef;
    addColumnHtml(field: string, displayName: string, width: string, enableHiding?: boolean): uiGrid.IColumnDef;
    addColumnNumber(field: string, displayName: string, width: string, enableHiding?: boolean, decimals?: number, type?: string, onChangeEvent?: string): uiGrid.IColumnDef;
    addColumnSelect(field: string, displayName: string, width: string, selectOptions: any[], enableHiding?: boolean, enableCellEdit?: boolean, fieldValue?: string, dropdownIdLabel?: string, dropdownValueLabel?: string, onChangeEvent?: string, ctrlName?: string, collectionField?: string): uiGrid.IColumnDef;
    addColumnDateTime(field: string, displayName: string, width: string, enableHiding?: boolean, cellFilter?: string, toolTipField?: string, toolTip?: string, className?: string, classFunction?: string): uiGrid.IColumnDef
    addColumnDate(field: string, displayName: string, width: string, enableHiding?: boolean, cellFilter?: string, toolTipField?: string, toolTip?: string, className?: string, classFunction?: string): uiGrid.IColumnDef;
    addColumnTime(field: string, displayName: string, width: string, enableHiding?: boolean, cellFilter?: string, columnDefType?: string): uiGrid.IColumnDef
    addColumnTimeSpan(field: string, displayName: string, width: string, enableHiding?: boolean): uiGrid.IColumnDef
    addColumnShape(field: string, displayName?: string, width?: string, shapeField?: string, shape?: string, toolTipField?: string, toolTip?: string, showIconField?: string, showIconFunction?: string, ctrlName?: string, isSubgrid?: boolean): uiGrid.IColumnDef;
    addColumnTypeAhead(field: string, typeAheadOptions: TypeAheadOptions, displayName?: string, width?: string, minChars?: number, wait?: number): uiGrid.IColumnDef;
    enableExpansion(expansionTemplate?: string);
    enableDynamicHeight();

    options: ISoeGridOptions;
}

export class GridHandlerUiGrid implements IGridHandlerUiGrid {
    //TypeAhead
    public currentlyEditing: {
        entity: any;
        colDef: uiGrid.IColumnDef;
    };
    private preventNextEnterKey = false;
    //TypeAhead

    constructor(name: string, private $timeout: ng.ITimeoutService, private uiGridConstants: uiGrid.IUiGridConstants, private coreService: ICoreService,
        private translationService: ITranslationService, private notificationService: INotificationService) {
        this.options = SoeGridOptions.create(name, $timeout, uiGridConstants);
    }

    setExporterFilenamesAndHeader(translationKey: string) {
        this.translationService.translate(translationKey).then(term => {
            this.options.exporterCsvFilename = term + ".csv";
            this.options.exporterPdfFilename = term + ".pdf";
            this.options.exporterPdfHeader = { text: term, style: 'headerStyle' };
        });
    }

    addGridMenuItems(items: UIGridMenuItem[]) {
        if (this.options.enableGridMenu) {
            this.$timeout(() => {
                _.each(items, x => { this.options.addGridMenuItem(x); })
            }, 200);
        }
    }

    clearData() {
        this.clearSelectedItems();
        this.options.clearData();
    }
    clearFilters() {
        this.options.clearFilters();
    }

    setData(data: any) {
        this.clearSelectedItems();
        this.options.setData(data);
    }

    findInData(predicate: (item: any) => boolean): any[] {
        return this.options.findAllInData(predicate);
    }

    saveState() {
        this.options.saveState((name, data) => this.coreService.saveUserGridState(name, data));
    }
    setDefaultState() {
        this.options.restoreDefaultState((name) => this.coreService.getSysGridState(name));
    }
    deleteState() {
        this.options.deleteState((name) => this.coreService.deleteUserGridState(name), (name) => this.coreService.getSysGridState(name));
    }
    deleteDefaultState() {
        var keys: string[] = [
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
        var keys: string[] = [
            "core.warning",
            "core.uigrid.savedefaultstatewarning",
            "core.enterpassword",
            "core.wrongpassword"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            var modal = this.notificationService.showDialogEx(terms["core.warning"], terms["core.uigrid.savedefaultstatewarning"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel, { showTextBox: true, textBoxLabel: terms["core.enterpassword"] });
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
    }
    restoreState() {
        this.options.restoreState((name) => this.coreService.getUserGridState(name), true);
    }
    refresh() {
        this.options.refreshGrid();
    }
    refreshColumns() {
        this.options.refreshColumns();
    }
    resize() {
        this.$timeout(() =>
            this.options.resize(),10
        )
    }
    subscribe(events: any[]) {
        this.options.subscribe(events);
    }

    // Common
    addColumn(columnDef): uiGrid.IColumnDef {
        return this.options.addColumn(columnDef);
    }

    // Boolean (checkbox)      
    addColumnBool(field: string, displayName: string, width: string, enableCellEdit?: boolean, clickEvent?: string, clickEventField?: string, disabledField?: string, termIndeterminate?: boolean): uiGrid.IColumnDef {
        return this.options.addColumnBool(field, displayName, width, enableCellEdit, clickEvent, clickEventField, disabledField, termIndeterminate);
    }

    // Active
    addColumnActive(field: string, displayName?: string, width?: string, changeEventHandlerName?: string): uiGrid.IColumnDef {
        return this.options.addColumnActive(field, displayName, width, changeEventHandlerName);
    }

    // Icon
    addColumnIcon(field: string, icon: string, toolTip: string, clickEvent: string, showIconField?: string, showIconFunction?: string, displayName?: string, width?: string, enableHiding?: boolean, enableResizing?: boolean, ctrlName?: string, isSubgrid?: boolean, tooltipField?: string): uiGrid.IColumnDef {
        return this.options.addColumnIcon(field, icon, toolTip, clickEvent, showIconField, showIconFunction, displayName, width, enableHiding, enableResizing, ctrlName, isSubgrid, tooltipField);
    }

    // Edit
    addColumnEdit(toolTip: string, clickEvent?: string, ctrlName?: string, isSubgrid?: boolean): uiGrid.IColumnDef {
        return this.options.addColumnEdit(toolTip, clickEvent, ctrlName, isSubgrid);
    }

    // Delete
    addColumnDelete(toolTip: string, onDeleteEvent?: string, ctrlName?: string, isSubgrid?: boolean, showIconField?: string, showIconFunction?: string): uiGrid.IColumnDef {
        return this.options.addColumnDelete(toolTip, onDeleteEvent, ctrlName, isSubgrid, showIconField, showIconFunction);
    }

    // PDF
    addColumnPdf(toolTip: string): uiGrid.IColumnDef {
        return this.options.addColumnPdf(toolTip);
    }

    // IsModified
    addColumnIsModified(field?: string, displayName?: string, width?: string, clickEvent?: string, ctrlName?: string): uiGrid.IColumnDef {
        return this.options.addColumnIsModified(field, displayName, width, clickEvent, ctrlName);
    }

    // Text
    addColumnText(field: string, displayName: string, width: string, enableHiding: boolean = false, toolTipField?: string, toolTip?: string, className?: string, classFunction?: string): uiGrid.IColumnDef {
        return this.options.addColumnText(field, displayName, width, enableHiding, toolTipField, toolTip, className, classFunction);
    }

    // HTML
    addColumnHtml(field: string, displayName: string, width: string, enableHiding: boolean = false): uiGrid.IColumnDef {
        return this.options.addColumnHtml(field, displayName, width, enableHiding);
    }

    // Number
    addColumnNumber(field: string, displayName: string, width: string, enableHiding: boolean = false, decimals: number = null, type?: string, onChangeEvent?: string, alignLeft?: boolean, toolTipField?: string, toolTip?: string): uiGrid.IColumnDef {
        return this.options.addColumnNumber(field, displayName, width, enableHiding, decimals, type, null, null, onChangeEvent, alignLeft, toolTipField, toolTip);
    }

    // Select
    addColumnSelect(field: string, displayName: string, width: string, selectOptions: any[], enableHiding: boolean = true, enableCellEdit: boolean = false, fieldValue: string = "", dropdownIdLabel: string = "id", dropdownValueLabel: string = "value", onChangeEvent: string = "", ctrlName?: string, collectionField?: string): uiGrid.IColumnDef {
        return this.options.addColumnSelect(field, displayName, width, selectOptions, enableHiding, enableCellEdit, fieldValue, dropdownIdLabel, dropdownValueLabel, onChangeEvent, ctrlName, collectionField);
    }

    // MultiSelect
    addColumnMultiSelect(field: string, displayName: string, width: string, selectOptions: any[], enableHiding?: boolean, enableCellEdit?: boolean, fieldValue?: string, dropdownIdLabel?: string, dropdownValueLabel?: string, placeholder?: string): uiGrid.IColumnDef {
        return this.options.addColumnMultiSelect(field, displayName, width, selectOptions, enableHiding, enableCellEdit, fieldValue, dropdownIdLabel, dropdownValueLabel, placeholder);
    }

    // DateTime
    addColumnDateTime(field: string, displayName: string, width: string, enableHiding: boolean = false, cellFilter: string = null, toolTipField?: string, toolTip?: string, className?: string, classFunction?: string): uiGrid.IColumnDef {
        return this.options.addColumnDateTime(field, displayName, width, enableHiding, cellFilter, toolTipField, toolTip, className, classFunction);
    }

    // Date
    addColumnDate(field: string, displayName: string, width: string, enableHiding: boolean = false, cellFilter: string = null, toolTipField?: string, toolTip?: string, className?: string, classFunction?: string): uiGrid.IColumnDef {
        return this.options.addColumnDate(field, displayName, width, enableHiding, cellFilter, toolTipField, toolTip, className, classFunction);
    }

    // Time
    addColumnTime(field: string, displayName: string, width: string, enableHiding: boolean = false, cellFilter: string = null, columnDefType: string = null): uiGrid.IColumnDef {
        return this.options.addColumnTime(field, displayName, width, enableHiding, cellFilter, columnDefType);
    }

    // TimeSpan
    addColumnTimeSpan(field: string, displayName: string, width: string, enableHiding: boolean = false): uiGrid.IColumnDef {
        return this.options.addColumnTimeSpan(field, displayName, width, enableHiding);
    }

    // Shape
    addColumnShape(field: string, displayName?: string, width?: string, shapeField?: string, shape?: string, toolTipField?: string, toolTip?: string, showIconField?: string, showIconFunction?: string, ctrlName?: string, isSubgrid?: boolean): uiGrid.IColumnDef {
        return this.options.addColumnShape(field, displayName, width, shapeField, shape, toolTipField, toolTip, showIconField, showIconFunction, ctrlName, isSubgrid);
    }

    // TypeAhead
    addColumnTypeAhead(field: string, typeAheadOptions: TypeAheadOptions, displayName?: string, width?: string, minChars?: number, wait?: number): uiGrid.IColumnDef {
        return this.options.addColumnTypeAhead(field, typeAheadOptions, displayName, width, minChars, wait);
    }

    enableExpansion(expansionTemplate?: string) {
        this.options.enableExpandable = true;
        this.options.enableExpandableRowHeader = true;
        this.options.expandableRowTemplate = expansionTemplate;
        this.options.expandableRowHeight = 150;
        this.options.expandableRowScope = {};
    }

    enableDynamicHeight() {
        this.options.enableDynamicHeight();
    }

    public addSumAggregationFooterToColumns(...args: uiGrid.IColumnDef[]) {
        args.forEach(col => {
            col.aggregationType = this.uiGridConstants.aggregationTypes.sum;
            col.aggregationHideLabel = true;
            col.footerCellFilter = 'number:2';
            col.footerCellTemplate = '<div class="ui-grid-cell-contents" col-index="renderIndex">' +
                '<div class="pull-right">{{col.getAggregationText() + (col.getAggregationValue() CUSTOM_FILTERS )}}</div>' +
                '</div>';
        });
    }

    public addStandardMenuItems() {
        var keys: string[] = [
            "core.uigrid.export",
            "core.uigrid.gridstate",
            "core.uigrid.savedefaultstate",
            "core.uigrid.deletedefaultstate",
            "core.uigrid.savestate",
            "core.uigrid.defaultstate",
            "core.uigrid.deletestate"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            var gridMenuItems = [
                new UIGridMenuItem(terms["core.uigrid.export"] + ":", 149),
                new UIGridMenuItem(terms["core.uigrid.gridstate"] + ":", 250),
                new UIGridMenuItem(terms["core.uigrid.savestate"], 253, "fal fa-save", this, ($event) => { this.saveState() }),
                new UIGridMenuItem(terms["core.uigrid.defaultstate"], 254, "fal fa-columns", this, ($event) => { this.setDefaultState() }),
                new UIGridMenuItem(terms["core.uigrid.deletestate"], 255, "fal fa-undo", this, ($event) => { this.deleteState() })
            ];

            if (CoreUtility.isSupportAdmin) {
                gridMenuItems.splice(2, 0,
                    new UIGridMenuItem(terms["core.uigrid.savedefaultstate"], 251, "fal fa-save", this, ($event) => { this.saveDefaultState() }),
                    new UIGridMenuItem(terms["core.uigrid.deletedefaultstate"], 252, "fal fa-times", this, ($event) => { this.deleteDefaultState() })
                );
            }

            this.addGridMenuItems(gridMenuItems);
        });

        this.restoreState();
    }

    //TypeAhead functions
    public setupTypeAhead() {

        var events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.BeginCellEdit, (entity, colDef) => {
            this.currentlyEditing = { entity, colDef };
            //this.beginCellEditInTypeahead(entity, colDef);
        }));
        events.push(new GridEvent(SoeGridOptionsEvent.AfterCellEdit, (entity, colDef) => {
            this.currentlyEditing = null;

            if (this.getSoeType(colDef) === Constants.GRID_COLUMN_TYPE_TYPEAHEAD && colDef.soeData.secondRowBinding) {
                entity[colDef.soeData.secondRowBinding] = colDef.soeData.getSecondRowBindingValue(entity, colDef);
            }

            if (this.getSoeType(colDef) === Constants.GRID_COLUMN_TYPE_TYPEAHEAD && colDef.soeData.onBlur) {
                colDef.soeData.onBlur(entity, colDef);
            }
        }));
        this.options.subscribe(events);
    }

    public selectedItemChanged(colDef: uiGrid.IColumnDef, entity: any, selectedItem: any) {
        this.preventNextEnterKey = true;
        (this as any).$scope.$broadcast('uiGridEventEndCellEdit');//this is the string value of uiGridEditConstants.events.END_CELL_EDIT. I would need to change 135 files to get it in the correct way, which seems like overkill.
        this.navigateToNextCell(colDef);
    }

    public handleKeyPressInEditCell(evt) {
        if (this.preventNextEnterKey && evt.keyCode === 13) {
            this.preventNextEnterKey = false;
            return null;
        }

        if (!this.currentlyEditing) {
            return undefined;
        }

        if ((evt.keyCode === 38 || evt.keyCode === 40) && this.getSoeType(this.currentlyEditing.colDef) === Constants.GRID_COLUMN_TYPE_SELECT) {
            return null;
        }

        if ((evt.keyCode === 13 || evt.keyCode === 38 || evt.keyCode === 40) && this.getSoeType(this.currentlyEditing.colDef) === Constants.GRID_COLUMN_TYPE_TYPEAHEAD) {

            if (evt.keyCode === 13) {

                var val = (<any>this.currentlyEditing.colDef).soeData.allowNavigationFromTypeAhead(this.currentlyEditing.entity, this.currentlyEditing.colDef);

                if (val) { //if there is a value and it is valid, allow it.
                    this.navigateToNextCell(this.currentlyEditing.colDef);
                    return 'stopEdit';
                }
                return null;//prevent navigation in all other cases.
            }

            return null;
        }

        if (evt.keyCode === 13) { //enter
            this.navigateToNextCell(this.currentlyEditing.colDef);
            return 'stopEdit';
        }

        if (evt.keyCode === 9) {//tab
            this.navigateToNextCell(this.currentlyEditing.colDef);
            return 'stopEdit';
        }

        //return null stops navigtion
        //return 'stopEdit' stops editing and stops navigation, allowing navigateToNextCell to run uninterrupted. Mainly needed for IE.
        //return undefined lets the original keypress of ui grid run. 
        return undefined;
    }

    public findNextRow(row) {
        var index = this.findIndex(row);
        var data = this.options.getData();

        if (index === data.length - 1)
            return null;

        return data[index + 1];
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

    protected navigateToNextCell(coldef: uiGrid.IColumnDef) {
        var row = this.options.getCurrentRow();

        var colDefs = this.options.getColumnDefs();

        //this is a naive implementation that assumes that all columns are editable. 
        for (var i = 0; i < colDefs.length; i++) {
            if (colDefs[i] === coldef) {
                if (i !== colDefs.length - 1) {
                    this.options.scrollToFocus(row, i + 1);
                } else {
                    var nextRow = this.findNextRow(row);
                    if (nextRow)
                        this.options.scrollToFocus(nextRow, 0);
                }
            }
        }
    }

    //functions for handling "activated" column
    private selectedItems = [];

    public clearSelectedItems() {
        this.selectedItems = [];
    }

    public selectedItemsExist() {
        return this.selectedItems.length > 0;
    }

    public selectItem(id: number) {
        // If item exists, remove it (it has been clicked twice and returned to original state),
        // otherwise add it.
        if (_.includes(this.selectedItems, id)) {
            this.selectedItems.splice(this.selectedItems.indexOf(id), 1);
        } else {
            this.selectedItems.push(id);
        }
    }

    public initUpdateStates(idField: string): any {
        if (this.selectedItems.length > 0) {
    
            var dict: any = {};
            _.forEach(this.selectedItems, (id: number) => {
                // Find entity
                var entity: any = this.options.findInData((ent: any) => ent[idField] === id);

                // Push id and active flag to array
                if (entity !== undefined) {
                    dict[id] = entity.isActive;
                }
            });
            return dict;
        }
    }
    //----------------

    private getSoeType(colDef: any) {
        return colDef.soeType;
    }

    options: ISoeGridOptions;
}