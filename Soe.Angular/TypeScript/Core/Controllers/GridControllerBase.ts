import { ICoreService } from "../Services/CoreService";
import { ITranslationService } from "../Services/TranslationService";
import { IUrlHelperService } from "../Services/UrlHelperService";
import { IMessagingService } from "../Services/MessagingService";
import { INotificationService } from "../Services/NotificationService";
import { CoreUtility } from "../../Util/CoreUtility";
import { Guid } from "../../Util/StringUtility";
import { CalendarUtility } from "../../Util/CalendarUtility";
import { ToolBarButton, ToolBarButtonGroup, ToolBarUtility } from "../../Util/ToolBarUtility";
import { ISoeGridOptions, SoeGridOptions, UIGridMenuItem, TypeAheadOptions, GridEvent } from "../../Util/SoeGridOptions";
import { ProgressController } from "../Dialogs/Progress/ProgressController";
import { SOEMessageBoxImage, SOEMessageBoxButtons, SoeGridOptionsEvent } from "../../Util/Enumerations";
import { Constants } from "../../Util/Constants";
import { Feature } from "../../Util/CommonEnumerations";

export interface IGridControllerBase {
    isHomeTab: boolean;
    loadGridData(): void;
    reloadData(): void;
    guid: Guid;
}

export class GridControllerBase {
    protected doubleClickToEdit: boolean = true;
    public isHomeTab: boolean = false;
    public guid: Guid;
    public isDirty: boolean = false;

    protected currentlyEditing: {
        entity: any;
        colDef: uiGrid.IColumnDef;
    };

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
    protected progressModalBusy: boolean = false;

    // Grid
    protected soeGridOptions: ISoeGridOptions;
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

    constructor(private gridName: string,
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
        private featureIds: any[] = []
    ) {

        this.soeGridOptions = new SoeGridOptions(gridName, $timeout, uiGridConstants, gridControllerName ? gridControllerName : undefined);
        if (enableExpansion) {
            this.soeGridOptions.enableExpandable = true;
            this.soeGridOptions.enableExpandableRowHeader = true;
            this.soeGridOptions.expandableRowTemplate = expansionTemplate;
            this.soeGridOptions.expandableRowHeight = 150;
            this.soeGridOptions.expandableRowScope = {};
        }

        if (!lazyPermissionsLoad) {
            if (currentFeature !== undefined && currentFeature !== null)
                this.loadPermissions();
            else
                this.permissionsLoaded();
        }
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

        if (this.featureIds.length === 0) {
            this.featureIds.push(feature);
        }

        this.coreService.hasReadOnlyPermissions(this.featureIds).then((x) => {
            _.forEach(this.featureIds, (id) => {
                if (x[id]) {
                    this.readPermission = true;
                }
            });

            if (this.readPermission) {

                this.coreService.hasModifyPermissions(this.featureIds).then((y) => {
                    _.forEach(this.featureIds, (id) => {
                        if (y[id]) {
                            this.modifyPermission = true;
                        }
                    });

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
        var group = ToolBarUtility.createSortGroup(
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

    protected addSaveButton(isDisabled: () => boolean = null) {
        if (!this.modifyPermission)
            return;

        this.buttonSave = ToolBarUtility.createSaveButton(() => {
            this.save();
        }, () => {
            return isDisabled ? isDisabled() : this.selectedItems.length == 0;
        });
        this.buttonGroups.push(ToolBarUtility.createGroup(this.buttonSave));
    }

    // GRID

    protected initSetupGrid() {

        // Set export names
        if (this.exportFileNameTranslationKey) {
            this.translationService.translate(this.exportFileNameTranslationKey).then((term) => {
                this.soeGridOptions.exporterCsvFilename = term + ".csv";
                this.soeGridOptions.exporterPdfFilename = term + ".pdf";
                this.soeGridOptions.exporterPdfHeader = { text: term, style: 'headerStyle' };
            });
        }

        // Dynamic grid menu items
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
            this.addGridMenuItems(terms);
        });
        this.restoreState();

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

    private addGridMenuItems(terms) {
        if (this.soeGridOptions.enableGridMenu) {
            this.$timeout(() => {
                this.soeGridOptions.addGridMenuItem(new UIGridMenuItem(terms["core.uigrid.export"] + ":", 149));
                this.soeGridOptions.addGridMenuItem(new UIGridMenuItem(terms["core.uigrid.gridstate"] + ":", 250));
                if (CoreUtility.isSupportAdmin) {
                    this.soeGridOptions.addGridMenuItem(new UIGridMenuItem(terms["core.uigrid.savedefaultstate"], 251, "fal fa-save", this, ($event, context) => { context.saveDefaultState() }));
                    this.soeGridOptions.addGridMenuItem(new UIGridMenuItem(terms["core.uigrid.deletedefaultstate"], 252, "fal fa-times", this, ($event, context) => { context.deleteDefaultState() }));
                }
                this.soeGridOptions.addGridMenuItem(new UIGridMenuItem(terms["core.uigrid.savestate"], 253, "fal fa-save", this, ($event, context) => { context.saveState() }));
                this.soeGridOptions.addGridMenuItem(new UIGridMenuItem(terms["core.uigrid.defaultstate"], 254, "fal fa-columns", this, ($event, context) => { context.defaultState() }));
                this.soeGridOptions.addGridMenuItem(new UIGridMenuItem(terms["core.uigrid.deletestate"], 255, "fal fa-undo", this, ($event, context) => { context.deleteState() }));
            }, 800);
        }
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
        if (this.progressModalBusy)
            return;

        this.progressModalBusy = true;
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

    protected startSaveModal(message?: string) {
        if (!message) {
            this.translationService.translate('core.saving').then(s => {
                this.openProgressModal(s);
            });
        }
        else {
            this.openProgressModal(message);
        }
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

    protected stopProgress(closeAnyModal: boolean = false) {
        this.progressBusy = false;
        this.progressModalBusy = false;
        if (closeAnyModal) {
            if (this.progressModal) {
                this.progressModal.close();
            }
        }
    }

    protected completedSave(data: any, skipDialog: boolean = true, message?: string) {
        if (skipDialog) {
            this.stopProgress(true);
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
            this.stopProgress(true);

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
            this.stopProgress(true);
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
            this.stopProgress(true);
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

    protected rowClicked(row) {
        // Override in child class;
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
        this.clearGridData(true);
        this.loadGridData(false);
    }

    protected gridDataLoaded(data) {
        this.soeGridOptions.setData(data);
        this.stopProgress(true);
        this.isDirty = false;
    }

    protected gridDataLoadedEx(data) {
        this.gridDataLoaded(data);
        this.soeGridOptions.resize();
        this.$timeout(() => this.messagingService.publish(Constants.EVENT_RESIZE_WINDOW, null), 100);
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

    protected restoreState = function () {
        this.soeGridOptions.restoreState((name) => this.coreService.getUserGridState(name), true);
    };

    protected deleteState = function () {
        this.soeGridOptions.deleteState((name) => this.coreService.deleteUserGridState(name), (name) => this.coreService.getSysGridState(name));
    };

    // GRID FUNCTIONS

    // Common
    protected addColumn(columnDef): uiGrid.IColumnDef {
        return this.soeGridOptions.addColumn(columnDef);
    }

    // Boolean (checkbox)      
    protected addColumnBool(field: string, displayName: string, width: string, enableCellEdit?: boolean, clickEvent?: string, clickEventField?: string, disabledField?: string, termIndeterminate?: boolean): uiGrid.IColumnDef {
        return this.soeGridOptions.addColumnBool(field, displayName, width, enableCellEdit, clickEvent, clickEventField, disabledField, termIndeterminate);
    }

    // Active
    protected addColumnActive(field: string, displayName?: string, width?: string): uiGrid.IColumnDef {
        return this.soeGridOptions.addColumnActive(field, displayName, width);
    }

    // Icon
    protected addColumnIcon(field: string, icon: string, toolTip: string, clickEvent: string, showIconField?: string, showIconFunction?: string, displayName?: string, width?: string, enableHiding?: boolean, enableResizing?: boolean, ctrlName?: string, isSubgrid?: boolean, tooltipField?: string): uiGrid.IColumnDef {
        return this.soeGridOptions.addColumnIcon(field, icon, toolTip, clickEvent, showIconField, showIconFunction, displayName, width, enableHiding, enableResizing, ctrlName, isSubgrid, tooltipField);
    }

    // Edit
    protected addColumnEdit(toolTip: string, clickEvent?: string, ctrlName?: string, isSubgrid?: boolean): uiGrid.IColumnDef {
        return this.soeGridOptions.addColumnEdit(toolTip, clickEvent, ctrlName, isSubgrid);
    }

    // Delete
    protected addColumnDelete(toolTip: string, onDeleteEvent?: string, ctrlName?: string, isSubgrid?: boolean, showIconField?: string, showIconFunction?: string): uiGrid.IColumnDef {
        return this.soeGridOptions.addColumnDelete(toolTip, onDeleteEvent, ctrlName, isSubgrid, showIconField, showIconFunction);
    }

    // PDF
    protected addColumnPdf(toolTip: string): uiGrid.IColumnDef {
        return this.soeGridOptions.addColumnPdf(toolTip);
    }

    // IsModified
    protected addColumnIsModified(field?: string, displayName?: string, width?: string, clickEvent?: string, ctrlName?: string): uiGrid.IColumnDef {
        return this.soeGridOptions.addColumnIsModified(field, displayName, width, clickEvent, ctrlName);
    }

    // Text
    protected addColumnText(field: string, displayName: string, width: string, enableHiding: boolean = false, toolTipField?: string, toolTip?: string, className?: string, classFunction?: string, shapeValueField?: string, shape?: string, buttonIcon?: string, buttonFunction?: string, ctrlName?: string, showButtonField?: string): uiGrid.IColumnDef {
        return this.soeGridOptions.addColumnText(field, displayName, width, enableHiding, toolTipField, toolTip, className, classFunction, shapeValueField, shape, buttonIcon, buttonFunction, ctrlName, showButtonField);
    }

    // HTML
    protected addColumnHtml(field: string, displayName: string, width: string, enableHiding: boolean = false): uiGrid.IColumnDef {
        return this.soeGridOptions.addColumnHtml(field, displayName, width, enableHiding);
    }

    // Number
    protected addColumnNumber(field: string, displayName: string, width: string, enableHiding: boolean = false, decimals: number = null, type?: string, onChangeEvent?: string, alignLeft?: boolean, toolTipField?: string, toolTip?: string): uiGrid.IColumnDef {
        return this.soeGridOptions.addColumnNumber(field, displayName, width, enableHiding, decimals, type, null, null, onChangeEvent, alignLeft, toolTipField, toolTip);
    }

    // Select
    protected addColumnSelect(field: string, displayName: string, width: string, selectOptions: any[], enableHiding: boolean = true, enableCellEdit: boolean = false, fieldValue: string = "", dropdownIdLabel: string = "id", dropdownValueLabel: string = "value", onChangeEvent: string = "", ctrlName?: string, collectionField?: string, shapeValueField?: string, shape?: string): uiGrid.IColumnDef {
        return this.soeGridOptions.addColumnSelect(field, displayName, width, selectOptions, enableHiding, enableCellEdit, fieldValue, dropdownIdLabel, dropdownValueLabel, onChangeEvent, ctrlName, collectionField, shapeValueField, shape);
    }

    // MultiSelect
    protected addColumnMultiSelect(field: string, displayName: string, width: string, selectOptions: any[], enableHiding?: boolean, enableCellEdit?: boolean, fieldValue?: string, dropdownIdLabel?: string, dropdownValueLabel?: string, placeholder?: string): uiGrid.IColumnDef {
        return this.soeGridOptions.addColumnMultiSelect(field, displayName, width, selectOptions, enableHiding, enableCellEdit, fieldValue, dropdownIdLabel, dropdownValueLabel, placeholder);
    }

    // DateTime
    protected addColumnDateTime(field: string, displayName: string, width: string, enableHiding: boolean = false, cellFilter: string = null): uiGrid.IColumnDef {
        return this.soeGridOptions.addColumnDateTime(field, displayName, width, enableHiding, cellFilter);
    }

    // Date
    protected addColumnDate(field: string, displayName: string, width: string, enableHiding: boolean = false, cellFilter: string = null): uiGrid.IColumnDef {
        return this.soeGridOptions.addColumnDate(field, displayName, width, enableHiding, cellFilter);
    }

    // Time
    protected addColumnTime(field: string, displayName: string, width: string, enableHiding: boolean = false, cellFilter: string = null, columnDefType: string = null): uiGrid.IColumnDef {
        return this.soeGridOptions.addColumnTime(field, displayName, width, enableHiding, cellFilter, columnDefType);
    }

    // TimeSpan
    protected addColumnTimeSpan(field: string, displayName: string, width: string, enableHiding: boolean = false): uiGrid.IColumnDef {
        return this.soeGridOptions.addColumnTimeSpan(field, displayName, width, enableHiding);
    }

    // Shape
    protected addColumnShape(field: string, displayName?: string, width?: string, shapeField?: string, shape?: string, toolTipField?: string, toolTip?: string, showIconField?: string, showIconFunction?: string, ctrlName?: string, isSubgrid?: boolean, attestGradient?: boolean): uiGrid.IColumnDef {
        return this.soeGridOptions.addColumnShape(field, displayName, width, shapeField, shape, toolTipField, toolTip, showIconField, showIconFunction, ctrlName, isSubgrid, attestGradient);
    }

    // TypeAhead
    protected addColumnTypeAhead(field: string, typeAheadOptions: TypeAheadOptions, displayName?: string, width?: string, minChars?: number, wait?: number): uiGrid.IColumnDef {
        return this.soeGridOptions.addColumnTypeAhead(field, typeAheadOptions, displayName, width, minChars, wait);
    }

    protected setupTypeAhead() {
        var events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.BeginCellEdit, (entity, colDef) => {
            this.currentlyEditing = { entity, colDef };
            this.beginCellEditInTypeahead(entity, colDef);
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
        this.soeGridOptions.subscribe(events);
    }

    public beginCellEditInTypeahead(entity, colDef) {
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

    protected navigateToNextCell(coldef: uiGrid.IColumnDef) {
        var row = this.soeGridOptions.getCurrentRow();

        var colDefs = this.soeGridOptions.getColumnDefs();

        //this is a naive implementation that assumes that all columns are editable. 
        for (var i = 0; i < colDefs.length; i++) {
            if (colDefs[i] === coldef) {
                if (i !== colDefs.length - 1) {
                    this.soeGridOptions.scrollToFocus(row, i + 1);
                } else {
                    var nextRow = this.findNextRow(row);
                    if (nextRow)
                        this.soeGridOptions.scrollToFocus(nextRow, 0);
                }
            }
        }
    }

    protected findNextRow(row) {
        var index = this.findIndex(row);
        var data = this.soeGridOptions.getData();

        if (index === data.length - 1)
            return null;

        return data[index + 1];
    }

    protected findIndex(row) {
        var entity = row.entity || row;

        // find real row by comparing $$hashKey with entity in row
        var rowIndex = -1;
        var hash = entity.$$hashKey;
        var data = this.soeGridOptions.getData();     // original rows of data
        for (var ndx = 0; ndx < data.length; ndx++) {
            if (data[ndx].$$hashKey === hash) {
                rowIndex = ndx;
                break;
            }
        }
        return rowIndex;
    }

    protected getSoeType(colDef: any) {
        return colDef.soeType;
    }

    protected getSecondRowValue(entity, colDef) {
        return null;
    }

    protected allowNavigationFromTypeAhead(entity, colDef) {
        return false;
    }

    protected addFilteredAndSelectedTotalsToFirstColumn(totalFilteredText: string, totalSelectedText: string) {
        this.totalFilteredText = totalFilteredText;
        this.totalSelectedText = totalSelectedText;

        var colDefs = this.soeGridOptions.getColumnDefs();
        var col = colDefs[0];
        col.aggregationType = this.uiGridConstants.aggregationTypes.sum;
        col.aggregationHideLabel = true;
        col.cellClass = "gridCellAlignleft";
        col.footerCellTemplate = '<div class="ui-grid-cell-contents" col-index=renderindex>' +
            '<div>{{ grid.appScope.ctrl.totalFilteredText + " " + (grid.appScope.ctrl.getFilteredRowsCount() )}}</div>' +
            '<div>{{ grid.appScope.ctrl.totalSelectedText + " " + (grid.appScope.ctrl.getSelectedRowsCount() )}}</div>' +
            '</div>';
    }

    protected addSumAggregationFooterToColumns(...args: uiGrid.IColumnDef[]) {
        args.forEach(col => {
            col.aggregationType = this.uiGridConstants.aggregationTypes.sum;
            col.aggregationHideLabel = true;
            col.footerCellFilter = 'number:2';
            col.footerCellTemplate = '<div class="ui-grid-cell-contents" col-index="renderIndex">' +
                '<div class="pull-right">{{col.getAggregationText() + (col.getAggregationValue() CUSTOM_FILTERS )}}</div>' +
                '</div>';
        });
    }

    protected addSumAggregationFooterToColumnsGrouping(...args: uiGrid.IColumnDef[]) {
        args.forEach(col => {
            col.aggregationType = this.uiGridConstants.aggregationTypes.sum;
            col.aggregationHideLabel = true;
            col.footerCellFilter = 'number:2';
            col.footerCellTemplate = '<div class="ui-grid-cell-contents" col-index="renderIndex">' +
                '<div class="pull-right">{{(grid.appScope.' + this.soeGridOptions.getDirectiveName() + '.getAggregationRowFilteredValue(col, rows) CUSTOM_FILTERS )}}</div>' +
                '</div>';
        });
    }

    protected addSumAggregationFooterToColumnsGroupingTime(...args: uiGrid.IColumnDef[]) {
        args.forEach(col => {
            col.aggregationType = this.uiGridConstants.aggregationTypes.sum;
            col.aggregationHideLabel = true;
            col.footerCellFilter = 'number:2';
            col.footerCellTemplate = '<div class="ui-grid-cell-contents" col-index="renderIndex">' +
                '<div class="pull-right">{{(grid.appScope.' + this.soeGridOptions.getDirectiveName() + '.getAggregationRowFilteredValueToTime(col, rows))}}</div>' +
                '</div>';
        });
    }

    protected addSumAggregationFooterToColumnsGroupingDirective(...args: uiGrid.IColumnDef[]) {
        args.forEach(col => {
            col.aggregationType = this.uiGridConstants.aggregationTypes.sum;
            col.aggregationHideLabel = true;
            col.footerCellTemplate = '<div class="ui-grid-cell-contents" col-index="renderIndex">' +
                '<div class="pull-right">{{(grid.appScope.directiveCtrl.getAggregationRowFilteredValue(col, rows) CUSTOM_FILTERS )}}</div>' +
                '</div>';
        });
    }

    protected addSumFilteredAndSelectedFooterToColumns(...cols: uiGrid.IColumnDef[]) {
        cols.forEach(col => {
            col.aggregationHideLabel = true;
            col.footerCellFilter = 'number:2';
            col.footerCellTemplate = '<div class="ui-grid-cell-contents" col-index="renderIndex">' +
                '<div class="pull-right" style="vertical-align: top;">{{(grid.appScope.ctrl.getAggregationRowFilteredValue(col, rows) CUSTOM_FILTERS )}}</div><br/>' +
                '<div class="pull-right style="vertical-align: top;">{{(grid.appScope.ctrl.getAggregationRowSelectedValue(col) CUSTOM_FILTERS )}}</div>' +
                '</div>';
        });
    }

    protected getFilteredRowsCount() {
        return this.soeGridOptions.getFilteredRows().length;
    }

    protected getSelectedRowsCount() {
        return this.soeGridOptions.getSelectedRows().length;
    }

    protected getAggregationRowFilteredValue(col, rows) {
        var val: number = 0;
        var filteredRows = this.soeGridOptions.getFilteredRows();
        if (_.filter(filteredRows, r => r.groupHeader).length > 0) {
            _.forEach(_.filter(this.soeGridOptions.getFilteredRows(), row => row.groupHeader), row => {
                val += row.entity['$$' + col.uid].value;
            });
        }
        else {
            _.forEach(this.soeGridOptions.getFilteredRows(), row => {
                val += row.entity[col.field];
            });
        }
        return val;
    }

    protected getAggregationRowFilteredValueToTime(col, rows) {
        var val: number = 0;
        var filteredRows = this.soeGridOptions.getFilteredRows();
        if (_.filter(filteredRows, r => r.groupHeader).length > 0) {
            _.forEach(_.filter(this.soeGridOptions.getFilteredRows(), row => row.groupHeader), row => {
                val += row.entity['$$' + col.uid].value;
            });
        }
        else {
            _.forEach(this.soeGridOptions.getFilteredRows(), row => {
                val += row.entity[col.field];
            });
        }
        return CalendarUtility.minutesToTimeSpan(val);
    }

    protected getAggregationRowSelectedValue(col) {
        var val: number = 0;
        _.forEach(_.filter(this.soeGridOptions.getSelectedRows(), row => !row.isDeleted), row => {
            val += row[col.field];
        });

        return val;
    }

    protected addCountAggregationFooterToColumns(...args: uiGrid.IColumnDef[]) {
        args.forEach(col => {
            col.aggregationType = this.uiGridConstants.aggregationTypes.count;
            col.aggregationHideLabel = true;
            col.footerCellFilter = 'number:0';
            col.footerCellTemplate = '<div class="ui-grid-cell-contents" col-index="renderIndex">' +
                '<div class="pull-right">{{col.getAggregationText() + (col.getAggregationValue() CUSTOM_FILTERS )}}</div>' +
                '</div>';
        });
    }
}