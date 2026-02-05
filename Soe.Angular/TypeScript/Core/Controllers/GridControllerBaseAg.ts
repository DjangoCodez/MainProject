import { Guid } from "../../Util/StringUtility";
import { ICoreService } from "../Services/CoreService";
import { ITranslationService } from "../Services/TranslationService";
import { IUrlHelperService } from "../Services/UrlHelperService";
import { IMessagingService } from "../Services/MessagingService";
import { INotificationService } from "../Services/NotificationService";
import { ToolBarButton, ToolBarButtonGroup, ToolBarUtility } from "../../Util/ToolBarUtility";
import { ISoeGridOptionsAg, SoeGridOptionsAg, CellChangedCallback, IconColumnOptions, DataCallback, FieldOrPredicate, TextColumnOptions, NumberColumnOptions, SelectColumnOptions, TypeAheadColumnOptions, TimeColumnOptions, BoolColumnOptions, DateColumnOptions, ShapeColumnOptions } from "../../Util/SoeGridOptionsAg";
import { ProgressController } from "../Dialogs/Progress/ProgressController";
import { GridMenuBuilder } from "../../Util/ag-grid/GridMenuBuilder";
import { SOEMessageBoxImage, SOEMessageBoxButtons } from "../../Util/Enumerations";
import { Constants } from "../../Util/Constants";
import { Feature } from "../../Util/CommonEnumerations";

export interface IGridControllerBase {
    isHomeTab: boolean;
    loadGridData(): void;
    reloadData(): void;
    guid: Guid;
}

export class GridControllerBaseAg {
    protected doubleClickToEdit: boolean = true;
    public isHomeTab: boolean = false;
    public guid: Guid;
    public isDirty: boolean = false;

    // Progress bar
    protected progressMessage: string;
    protected progressBusy: boolean = true;

    // ToolBar
    protected buttonGroups = new Array<ToolBarButtonGroup>();
    protected buttonSave: ToolBarButton;

    //Modal
    public isModal: boolean = false;
    protected progressModalMetaData;
    protected progressModal;

    // Grid
    protected soeGridOptions: ISoeGridOptionsAg;
    protected canEditCell: boolean;

    // Permissions
    protected readPermission: boolean = false;
    protected _modifyPermission: boolean = false;
    get modifyPermission(): boolean {
        return this._modifyPermission;
    }
    set modifyPermission(value: boolean) {
        this._modifyPermission = value;

        // Send messages to TabsController
        if (this.currentFeature !== Feature.None)
            this.messagingService.publish(value ? Constants.EVENT_ACTIVATE_ADD_TAB : Constants.EVENT_DISABLE_ADD_TAB, null);
    }

    // Collections
    protected selectedItems = [];

    protected sortMenuButtons = new Array<ToolBarButtonGroup>();
    protected parameters: any;

    private preventNextEnterKey = false;

    public totalFilteredText: string = "";
    public totalSelectedText: string = "";

    private loadUserStatePromise: ng.IPromise<string>;

    constructor(
        private gridName: string,
        protected exportFileNameTranslationKey: string,
        private currentFeature: Feature,
        $http,
        $templateCache,
        protected $timeout: ng.ITimeoutService,
        protected $uibModal,
        protected coreService: ICoreService,
        protected translationService: ITranslationService,
        protected urlHelperService: IUrlHelperService,
        protected messagingService: IMessagingService,
        protected notificationService: INotificationService,
        protected uiGridConstants: uiGrid.IUiGridConstants,
        enableExpansion?: boolean,
        expansionTemplate?: string,
        gridControllerName?: string,
        lazyPermissionsLoad: boolean = false,
        private skipAutoReload: boolean = false,
        private useProgressModal: boolean = false,
        skipPreloadUserState: boolean = false) {

        this.soeGridOptions = new SoeGridOptionsAg(gridName, $timeout);
        this.initGridText();

        if (!lazyPermissionsLoad) {
            if (currentFeature !== undefined && currentFeature !== null)
                this.loadPermissions();
            else
                this.permissionsLoaded();
        }

        if (!skipPreloadUserState) {
            this.loadState();
        }

        this.soeGridOptions.customTabToCellHandler = (params) => this.handleNavigateToNextCell(params);
    }

    // SETUP
    protected onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = parameters.isHomeTab;
        this.guid = this.parameters.guid;
        this.init();
    }

    protected init() { }

    protected loadPermissions() {
        var feature = this.currentFeature;

        if (feature === Feature.None) {
            this.readPermission = true;
            this.modifyPermission = true;
            this.$timeout(() => {
                this.permissionsLoaded();
            });
            return;
        }

        var featureIds: number[] = [];
        featureIds.push(feature);

        this.coreService.hasReadOnlyPermissions(featureIds).then((x) => {
            if (x[feature]) {
                this.readPermission = true;

                this.coreService.hasModifyPermissions(featureIds).then((y) => {
                    if (y[feature]) {
                        this.modifyPermission = true;
                    }
                    this.permissionsLoaded();
                });
            } else {
                this.permissionsLoaded();
            }
        });
    }

    protected permissionsLoaded() {
        this.setupCustomToolBar();

        // Setup grid
        this.initSetupGrid();

        // Load grid data
        this.initLoadGridData();

        this.loadLookups();

        // Override in child class to call other setup methods below after permissions are loaded
        // Make sure to call super.permissionsLoaded(); in child class if overridden.
    }

    protected setName(name: string) {
        this.soeGridOptions.setName(name);
    }

    protected getName() {
        return this.soeGridOptions.getName();
    }

    // TOOLBAR

    protected setupDefaultToolBar(useSave: boolean = false, hideReload: boolean = false) {
        if (this.buttonGroups && this.buttonGroups.length > 0)
            return false;

        // Setup ToolBar

        var group = ToolBarUtility.createGroup();
        // Clear filters
        group.buttons.push(ToolBarUtility.createClearFiltersButton(() => {
            this.clearFilters();
        }));
        // Reload data
        if (!hideReload) {
            group.buttons.push(ToolBarUtility.createReloadDataButton(() => {
                this.reloadData();
            }));
        }
        this.buttonGroups.push(group);

        // Save
        if (useSave) {
            this.addSaveButton();
        }

        return true;
    }

    protected setupSortGroup(sortProp: string = "sort", disabled = () => { }, hidden = () => { }) {
        const group = ToolBarUtility.createSortGroup(
            () => {
                this.soeGridOptions.sortFirst(sortProp);
                this.messagingService.publish(Constants.EVENT_SET_DIRTY, {});
            },
            () => {
                this.soeGridOptions.sortUp(sortProp);
                this.messagingService.publish(Constants.EVENT_SET_DIRTY, {});
            },
            () => {
                this.soeGridOptions.sortDown(sortProp);
                this.messagingService.publish(Constants.EVENT_SET_DIRTY, {});
            },
            () => {
                this.soeGridOptions.sortLast(sortProp);
                this.messagingService.publish(Constants.EVENT_SET_DIRTY, {});
            },
            disabled,
            hidden
        );
        this.sortMenuButtons.push(group);
    }

    protected callout() {
    }

    protected setupCustomToolBar() {
        this.setupDefaultToolBar();

        // Override in child class to add more buttons
    }

    protected addSaveButton() {
        if (!this.modifyPermission)
            return;

        this.buttonSave = ToolBarUtility.createSaveButton(() => {
            this.save();
        }, () => {
            return this.selectedItems.length === 0;
        });
        this.buttonGroups.push(ToolBarUtility.createGroup(this.buttonSave));
    }

    // GRID

    private initGridText() {
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
            this.soeGridOptions.translateText = (key: string, defaultValue: string) => {
                return terms["core.aggrid." + key] || defaultValue
            };
        });
    }

    protected initSetupGrid() {
        if (this.exportFileNameTranslationKey) {
            const keys: string[] = [
                this.exportFileNameTranslationKey
            ];
            this.translationService.translateMany(keys).then((terms) => {
                this.soeGridOptions.exportFilename = terms[this.exportFileNameTranslationKey];
            });
        }

        const gridMenuBuilder = new GridMenuBuilder(this.soeGridOptions, this.translationService, this.coreService, this.notificationService);
        gridMenuBuilder.buildDefaultMenu();

        //this.restoreState();

        if (this.isHomeTab) {
            // Subscribe to events
            this.messagingService.subscribe(Constants.EVENT_EDIT_WORKED, (x) => {

            });
            this.messagingService.subscribe(Constants.EVENT_EDIT_SAVED, (x) => {
                this.isDirty = true;
                if (!this.skipAutoReload) { this.reloadData(); }
            });
            this.messagingService.subscribe(Constants.EVENT_EDIT_ADDED, (x) => {
                this.isDirty = true;
                if (!this.skipAutoReload) { this.reloadData(); }
            });
            this.messagingService.subscribe(Constants.EVENT_EDIT_DELETED, (x) => {
                this.isDirty = true;
                if (!this.skipAutoReload) { this.reloadData(); }
            });
            this.messagingService.subscribe(Constants.EVENT_RELOAD_GRID, (x) => {
                this.reloadData();
            });
        }

        this.setupGrid();
    }

    protected setupGrid() {
        // Override in child class
    }

    protected loadLookups() {
        // Override in child class to load additional data for filters etc.
    }

    // PROGRESS

    protected startProgress(messageKey: string = undefined) {
        this.progressBusy = true;
        if (messageKey) {
            this.translationService.translate(messageKey).then((term) => {
                this.progressMessage = term;
            });
        }
    }

    protected startLoadModal() {
        this.translationService.translate('core.loading').then(s => {
            this.openProgressModal(s);
        });
    }

    protected startLoad(force: boolean = false) {
        this.startProgress("core.loading");
    }

    private openProgressModal(message: string, icon: string = 'far fa-spinner fa-pulse') {
        this.progressModalMetaData = { icon: icon, text: message };
        this.progressModal = this.$uibModal.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Core/Dialogs/Progress/Views/Progress.html"),
            controller: ProgressController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'md',
            resolve: {
                metadata: () => this.progressModalMetaData,
                progressParent: () => this
            }
        });
    }

    protected startSave() {
        this.startProgress("core.saving");

        this.translationService.translate('core.saving').then(s => {
            this.openProgressModal(s);
        });
    }

    protected startDelete() {
        this.startProgress("core.deleting");

        this.translationService.translate('core.deleting').then(s => {
            this.openProgressModal(s);
        });
    }

    protected startWork() {
        this.startProgress("core.working");

        this.translationService.translate('core.working').then(s => {
            this.openProgressModal(s);
        });
    }

    protected startWorkModal(message?: string) {
        if (!message) {
            this.translationService.translate('core.working').then(s => {
                this.openProgressModal(s);
            });
        }
        else {
            this.translationService.translate('core.working').then(s => {
                this.openProgressModal(message);
            });
        }
    }

    protected stopProgress() {
        this.progressBusy = false;
    }

    protected completedSave(data: any, skipDialog: boolean = true, message?: string) {
        if (skipDialog) {
            this.stopProgress();

            if (this.progressModal)
                this.progressModal.close();
        } else {
            this.progressModalMetaData.icon = 'fa-check-circle okColor';
            this.progressModalMetaData.showclose = true;
            if (message) {
                this.progressModalMetaData.text = message;
            } else {
                this.translationService.translate('core.saved').then(s => {
                    this.progressModalMetaData.text = s;
                });
            }
            this.stopProgress();
            this.reloadData();
        }
    }

    protected completedDelete(data, skipDialog?: boolean, message?: string) {
        if (skipDialog) {
            this.stopProgress();

            if (this.progressModal)
                this.progressModal.close();
        } else {
            this.progressModalMetaData.icon = 'fa-check-circle okColor';
            this.progressModalMetaData.showclose = true;
            if (message) {
                this.progressModalMetaData.text = message;
            } else {
                this.translationService.translate('core.deleted').then(s => {
                    this.progressModalMetaData.text = s;
                });
            }
            this.stopProgress();
            this.reloadData();
        }
    }

    protected completedWork(data, skipDialog?: boolean, message?: string) {
        if (skipDialog) {
            this.stopProgress();

            if (this.progressModal)
                this.progressModal.close();
        } else {
            this.progressModalMetaData.icon = 'fa-check-circle okColor';
            this.progressModalMetaData.showclose = true;
            if (message) {
                this.progressModalMetaData.text = message;
            } else {
                this.translationService.translate('core.worked').then(s => {
                    this.progressModalMetaData.text = s;
                });
            }
            this.stopProgress();
            this.reloadData();
        }
    }

    protected completedWorkModal(data, skipDialog?: boolean, message?: string, reloadData: boolean = false) {
        if (skipDialog) {
            this.stopProgress();

            if (this.progressModal)
                this.progressModal.close();
        } else {
            this.progressModalMetaData.icon = 'fa-check-circle okColor';
            this.progressModalMetaData.showclose = true;
            if (message) {
                this.progressModalMetaData.text = message;
            } else {
                this.translationService.translate('core.worked').then(s => {
                    this.progressModalMetaData.text = s;
                });
            }

            if (reloadData)
                this.reloadData();
        }
    }

    protected failedSave(message = "") {
        if (!this.progressModalMetaData)
            this.openProgressModal('');
        this.progressModalMetaData.icon = 'fa-exclamation-triangle errorColor';
        this.progressModalMetaData.showclose = true;

        if (message === "") {
            this.translationService.translate('core.savefailed').then(s => this.progressModalMetaData.text = s);
        } else {
            this.progressModalMetaData.text = message;
        }
        this.stopProgress();
    }

    protected failedDelete(message = "") {
        if (!this.progressModalMetaData)
            this.openProgressModal('');
        this.progressModalMetaData.icon = 'fa-exclamation-triangle errorColor';
        this.progressModalMetaData.showclose = true;

        if (message === "") {
            this.translationService.translate('core.deletefailed').then(s => this.progressModalMetaData.text = s);
        } else {
            this.progressModalMetaData.text = message;
        }
        this.stopProgress();
    }

    protected failedWork(message = "") {
        if (!this.progressModalMetaData)
            this.openProgressModal('');
        this.progressModalMetaData.icon = 'fa-exclamation-triangle errorColor';
        this.progressModalMetaData.showclose = true;

        if (message === "") {
            this.translationService.translate('core.workfailed').then(s => this.progressModalMetaData.text = s);
        } else {
            this.progressModalMetaData.text = message;
        }
        this.stopProgress();
    }

    protected showErrorDialog(message) {
        this.notificationService.showDialog("", message, SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
        this.stopProgress();
    }

    protected edit(row: any, data: any = null) {
        // Send message to TabsController
        if (this.doubleClickToEdit && (this.readPermission || this.modifyPermission))
            this.messagingService.publish(Constants.EVENT_EDIT, { row: row, data: data });
    }

    protected initDeleteRow(row) {
        // Override in child class
    }

    // LOAD

    protected initLoadGridData(useCache: boolean = true) {
        this.clearGridData(this.useProgressModal);
        this.loadGridData(useCache);
    }

    protected clearGridData(useModalProgress: boolean = false) {
        if (useModalProgress) {
            this.startLoadModal();
        }
        else {
            this.startLoad();
        }
        this.soeGridOptions.clearData();
        this.selectedItems = [];
    }

    public loadGridData(useCache: boolean = true) {
        // Override in child class
    }

    public reloadData() {
        this.initLoadGridData(false);
    }

    protected gridDataLoaded(data,resize=false) {
        this.soeGridOptions.setData(data);
        this.stopProgress();
        this.isDirty = false;
        if (resize) {
            this.messagingService.publish(Constants.EVENT_RESIZE_WINDOW, null);
        }
    }

    // SAVE

    protected initSave() {
        this.startSave();
    }

    protected save() {
        // Override in child class
    }

    protected selectItem(id: number) {
        // If item exists, remove it (it has been clicked twice and returned to original state),
        // otherwise add it.
        if (_.includes(this.selectedItems, id)) {
            this.selectedItems.splice(this.selectedItems.indexOf(id), 1);
        } else {
            this.selectedItems.push(id);
        }
    }

    protected initUpdateStates(idField: string): any {
        if (this.selectedItems.length > 0) {
            this.startProgress("core.saving");
            var dict: any = {};
            _.forEach(this.selectedItems, (id: number) => {
                // Find entity
                var entity: any = this.soeGridOptions.findInData((ent: any) => ent[idField] === id);

                // Push id and active flag to array
                if (entity !== undefined) {
                    dict[id] = entity.isActive;
                }
            });
            this.stopProgress();
            return dict;
        }
    }

    // TOOLBAR

    protected clearFilters() {
        this.soeGridOptions.clearFilters();
    }

    // SAVE STATE

    protected saveDefaultState = function () {
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
                        this.soeGridOptions.saveDefaultState((name, data) => this.coreService.saveSysGridState(name, data));
                    } else {
                        this.notificationService.showDialogEx(terms["core.warning"], terms["core.wrongpassword"], SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK);
                    }
                }
            });
        });
    };

    protected defaultState = function () {
        this.soeGridOptions.restoreDefaultState((name) => this.coreService.getSysGridState(name));
    };

    protected deleteDefaultState = function () {
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
                            this.soeGridOptions.deleteDefaultState((name) => this.coreService.deleteSysGridState(name));
                        } else {
                            this.notificationService.showDialogEx(terms["core.warning"], terms["core.wrongpassword"], SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK);
                        }
                    }
                });
            });
        });
    };

    protected saveState = function () {
        this.soeGridOptions.saveState((name, data) => this.coreService.saveUserGridState(name, data));
    };

    protected restoreState = function (force: boolean = false) {  
        this.soeGridOptions.restoreState((name) => this.getUserStatePromise(force), true);
    };

    protected deleteState = function () {
        this.soeGridOptions.deleteState((name) => this.coreService.deleteUserGridState(name), (name) => this.coreService.getSysGridState(name));
    };

    protected loadState(): ng.IPromise<string> {
        return this.loadUserStatePromise = this.coreService.getUserGridState(this.soeGridOptions.getNormalizedName());
    }

    private getUserStatePromise(force: boolean): ng.IPromise<string> {
        if (force || !this.loadUserStatePromise) {
            this.loadState();
        }

        return this.loadUserStatePromise;
    }

    // GRID FUNCTIONS

    protected reNumberGridRows(list: any[], sortProp:string) {
        _.forEach(list, (row: any, index: number) => {
            row[sortProp] = index + 1;
        });
    }

    protected addColumns(columnDefs: any[]) {
        this.soeGridOptions.addColumns(columnDefs);
    }

    protected addColumn(columnDef: any) {
        this.soeGridOptions.addColumn(columnDef);
    }

    protected addColumnHeader(field: string, headerName: string, options?: TextColumnOptions): any {
        return this.soeGridOptions.addColumnHeader(field, headerName, options);
    }

    // Boolean (checkbox)      
    protected addColumnBool(field: string, headerName: string, width: number, enableCellEdit?: boolean, onChanged?: CellChangedCallback, disabledField?: string, termIndeterminate?: boolean, useSetFilter?: boolean, filterOptions?: any[], filterLabel?: string, setChecked?: boolean, headerColumnDef: any = null): any {
        return this.soeGridOptions.addColumnBool(field, headerName, width, { enableEdit: enableCellEdit, onChanged: onChanged, disabledField: disabledField, termIndeterminate: termIndeterminate, useSetFilter: useSetFilter, filterOptions: filterOptions, filterLabel: filterLabel, setChecked: setChecked }, headerColumnDef);
    }

    // Active
    protected addColumnActive(field: string, headerName?: string, width?: number, onChanged?: CellChangedCallback): any {
        return this.soeGridOptions.addColumnActive(field, headerName, width, onChanged);
    }

    // Icon
    protected addColumnIcon(field: string, headerName?: string, width?: number, options?: IconColumnOptions, headerColumnDef: any = null) {
        return this.soeGridOptions.addColumnIcon(field, headerName, width, options, headerColumnDef);
    }

    // Edit
    protected addColumnEdit(toolTip: string, onClickEvent?: DataCallback, isSubgrid?: boolean): any {
        return this.soeGridOptions.addColumnEdit(toolTip, onClickEvent, isSubgrid);
    }

    // Delete
    protected addColumnDelete(toolTip: string, onClick?: DataCallback, isSubgrid?: boolean, showIcon?: FieldOrPredicate, icon?: string) {
        return this.soeGridOptions.addColumnDelete(toolTip, onClick, isSubgrid, showIcon, icon);
    }

    // PDF
    protected addColumnPdf(toolTip: string, onClickEvent?: DataCallback): any {
        return this.soeGridOptions.addColumnPdf(toolTip, onClickEvent);
    }

    // IsModified
    protected addColumnIsModified(field?: string, headerName?: string, width?: number, clickCallback?: (params: any) => void): any {
        return this.soeGridOptions.addColumnIsModified(field, headerName, width, clickCallback);
    }

    // HTML
    protected addColumnHtml(field: string, headerName: string, width: number, enableHiding: boolean = false): any {
        return this.soeGridOptions.addColumnHtml(field, headerName, width, enableHiding);
    }

    // Text
    protected addColumnText(field: string, headerName: string, width: number, options?: TextColumnOptions, headerColumnDef: any = null): any {
        return this.soeGridOptions.addColumnText(field, headerName, width, options, headerColumnDef);
    }

    // Number
    protected addColumnNumber(field: string, headerName: string, width: number, options?: NumberColumnOptions, headerColumnDef: any = null): any {
        return this.soeGridOptions.addColumnNumber(field, headerName, width, options, headerColumnDef);
    }

    // Select
    protected addColumnSelect(field: string, headerName: string, width: number, options?: SelectColumnOptions, headerColumnDef: any = null): any {
        return this.soeGridOptions.addColumnSelect(field, headerName, width, options, headerColumnDef);
    }

    // DateTime
    protected addColumnDateTime(field: string, headerName: string, width: number, enableHiding: boolean = false, headerColumnDef: any = null, cellFilter: string = null): any {
        return this.soeGridOptions.addColumnDateTime(field, headerName, width, enableHiding, headerColumnDef, cellFilter);
    }

    // Date
    protected addColumnDate(field: string, headerName: string, width: number, enableHiding: boolean = false, headerColumnDef: any = null, cellFilter: string = null, options?: DateColumnOptions): any {
        return this.soeGridOptions.addColumnDate(field, headerName, width, enableHiding, headerColumnDef, cellFilter, options);
    }

    // Time
    protected addColumnTime(field: string, headerName: string, width: number, options?: TimeColumnOptions, headerColumnDef: any = null): any {
        return this.soeGridOptions.addColumnTime(field, headerName, width, options, headerColumnDef);
    }

    // TimeSpan
    protected addColumnTimeSpan(field: string, headerName: string, width: number, options?: TimeColumnOptions, headerColumnDef: any = null): any {
        return this.soeGridOptions.addColumnTimeSpan(field, headerName, width, options, headerColumnDef);
    }

    // Shape
    protected addColumnShape(field: string, headerName?: string, width?: number, options?: ShapeColumnOptions): any {
        return this.soeGridOptions.addColumnShape(field, headerName, width, options);
    }

    // TypeAhead
    protected addColumnTypeAhead(field: string, headerName?: string, width?: number, options?: TypeAheadColumnOptions, soeData?: any) {
        return this.soeGridOptions.addColumnTypeAhead(field, headerName, width, options, soeData);
    }

    protected handleNavigateToNextCell(params): { rowIndex: number, column: any } {
        return params.nextCellPosition;
    }

    public selectedItemChanged(colDef: any, entity: any, selectedItem: any) {
        this.preventNextEnterKey = true;
        (this as any).$scope.$broadcast('uiGridEventEndCellEdit');//this is the string value of uiGridEditConstants.events.END_CELL_EDIT. I would need to change 135 files to get it in the correct way, which seems like overkill.
        this.soeGridOptions.tabToNextCell();
    }

    protected findNextRow(row): { rowIndex: number, rowNode: any } {
        const result = this.soeGridOptions.getNextRow(row);

        return !!result.rowNode ? result : null;
    }

    protected findPreviousRow(row): { rowIndex: number, rowNode: any } {
        const result = this.soeGridOptions.getPreviousRow(row);

        return !!result.rowNode ? result : null;
    }

    protected getSoeType(colDef: any) {
        return colDef.soeType;
    }

    protected getSecondRowValue(entity, colDef) {
        return null;
    }

    protected allowNavigationFromTypeAhead(value, entity, colDef) {
        return false;
    }

    protected getFilteredRowsCount() {
        return this.soeGridOptions.getFilteredRows().length;
    }

    protected getSelectedRowsCount() {
        return this.soeGridOptions.getSelectedRows().length;
    }

    protected setModified(value: boolean, parentGuid: Guid = null) {
        this.messagingService.publish(Constants.EVENT_SET_DIRTY, {
            guid: parentGuid ? parentGuid : this.guid,
            dirty: value
        });
    }
}