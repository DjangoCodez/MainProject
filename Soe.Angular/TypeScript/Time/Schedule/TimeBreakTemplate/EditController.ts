import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IFocusService } from "../../../Core/Services/FocusService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ISoeGridOptions, SoeGridOptions, GridEvent } from "../../../Util/SoeGridOptions";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { SelectShiftTypesController } from "../../Dialogs/SelectShiftTypes/SelectShiftTypesController";
import { SelectDayOfWeeksController } from "../../Dialogs/SelectDayOfWeeks/SelectDayOfWeeksController";
import { SelectDayTypesController } from "../../Dialogs/SelectDayTypes/SelectDayTypesController";
import { TimeBreakTemplateGridDTO } from "../../../Common/Models/TimeBreakTemplate";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { SoeGridOptionsEvent, SOEMessageBoxImage, SOEMessageBoxButtons, IconLibrary } from "../../../Util/Enumerations";
import { ITimeCodeBreakGroupGridDTO } from "../../../Scripts/TypeLite.Net4";
import { DayTypeDTO } from "../../../Common/Models/DayTypeDTO";
import { ToolBarUtility, ToolBarButton } from "../../../Util/ToolBarUtility";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IScheduleService } from "../ScheduleService";
import { IScheduleService as ISharedScheduleService } from "../../../Shared/Time/Schedule/ScheduleService";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { ShiftTypeDTO } from "../../../Common/Models/ShiftTypeDTO";
import { SmallGenericType } from "../../../Common/Models/smallgenerictype";
import { TimeCodeBreakGroupGridDTO } from "../../../Common/Models/TimeCode";
import { CoreUtility } from "../../../Util/CoreUtility";
import { Feature, SoeEntityState } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Data
    private breakTemplates: TimeBreakTemplateGridDTO[];

    // Lookups 
    private shiftTypes: ShiftTypeDTO[];
    private dayTypes: DayTypeDTO[];
    private dayOfWeeks: SmallGenericType[];
    private timeCodeBreakGroups: TimeCodeBreakGroupGridDTO[];

    // Grid
    protected gridOptions: ISoeGridOptions;
    protected currentlyEditing: {
        entity: any;
        colDef: uiGrid.IColumnDef;
    };

    private modalInstance: any;

    // CompanySettings

    //@ngInject
    constructor(
        $uibModal,
        private $q: ng.IQService,
        private $timeout: ng.ITimeoutService,
        private uiGridConstants: uiGrid.IUiGridConstants,
        private coreService: ICoreService,
        private scheduleService: IScheduleService,
        private sharedScheduleService: ISharedScheduleService,
        private urlHelperService: IUrlHelperService,
        private notificationService: INotificationService,
        private translationService: ITranslationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory
    ) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.modalInstance = $uibModal;

        this.initGrid();
        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.onDoLookups())
            .onLoadData(() => this.onLoadData())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    onInit(parameters: any) {
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([
            { feature: Feature.Time_Schedule_TimeBreakTemplate, loadModifyPermissions: true },
        ])
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.modifyPermission = response[Feature.Time_Schedule_TimeBreakTemplate].modifyPermission;
    }

    private onDoLookups() {
        return this.$q.all([
            this.loadShiftTypes(),
            this.loadDayTypes(),
            this.loadBreakTimeCodeGroups()])
            .then(() => {
                return this.$q.all([
                    this.setupGrid()
                ]);
            });
    }

    private onLoadData(): ng.IPromise<any> {
        var deferral = this.$q.defer();

        this.scheduleService.getTimeBreakTemplates().then((x: TimeBreakTemplateGridDTO[]) => {
            this.breakTemplates = x;

            if (this.breakTemplates.length === 0)
                this.addBreakTemplate(true);
            this.resetBreakTemplates(null);
            deferral.resolve();
        });

        return deferral.promise;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createEmpty();
        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("time.schedule.timebreaktemplate.loadbreaktemplates", "time.schedule.timebreaktemplate.loadbreaktemplates", IconLibrary.FontAwesome, "fa-sync", () => { this.onLoadData(); })));
        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("time.schedule.timebreaktemplate.newbreaktemplate", "time.schedule.timebreaktemplate.newbreaktemplate", IconLibrary.FontAwesome, "fa-plus", () => { this.addBreakTemplate(); }, () => { return !this.isValidForLoad(); })));
    }

    // LOOKUPS

    private loadShiftTypes(): ng.IPromise<any> {
        return this.sharedScheduleService.getShiftTypes(false, false, false, false, false, false).then(x => {
            this.shiftTypes = x;
            // Insert empty
            this.translationService.translate("core.notspecified").then((term) => {
                var shiftType: ShiftTypeDTO = new ShiftTypeDTO();
                shiftType.shiftTypeId = 0;
                shiftType.name = term;
                shiftType.color = Constants.SHIFT_TYPE_UNSPECIFIED_COLOR;
                shiftType["value"] = 0;
                shiftType["label"] = term;
                this.shiftTypes.splice(0, 0, shiftType);
            });
        });
    }

    private loadDayTypes(): ng.IPromise<any> {
        return this.scheduleService.getDayTypes().then((x) => {
            this.dayTypes = x;

            // Insert empty
            this.translationService.translate("core.notselected").then((term) => {
                var dayType: DayTypeDTO = new DayTypeDTO();
                dayType.dayTypeId = 0;
                dayType.name = term;
                dayType["value"] = 0;
                dayType["label"] = term;
                this.dayTypes.splice(0, 0, dayType);
            });
        });
    }

    private setupDayOfWeeks() {
        this.dayOfWeeks = []
        _.forEach(CalendarUtility.getDayOfWeekNames(true), dayOfWeek => {
            this.dayOfWeeks.push({ id: dayOfWeek.id, name: dayOfWeek.name });
        });

        // Insert empty
        this.translationService.translate("core.notselected").then((term) => {
            var dayType: DayTypeDTO = new DayTypeDTO();
            dayType.dayTypeId = 0;
            dayType.name = term;
            dayType["value"] = 0;
            dayType["label"] = term;
            this.dayTypes.splice(0, 0, dayType);
        });
    }

    private loadBreakTimeCodeGroups(): ng.IPromise<any> {
        return this.scheduleService.getTimeCodeBreakGroups().then((x: TimeCodeBreakGroupGridDTO[]) => {
            this.timeCodeBreakGroups = x;
            // Insert empty
            this.translationService.translate("core.notselected").then((term) => {
                var timeCodeBreakGroup: TimeCodeBreakGroupGridDTO = new TimeCodeBreakGroupGridDTO();
                timeCodeBreakGroup.timeCodeBreakGroupId = 0;
                timeCodeBreakGroup.name = term;
                timeCodeBreakGroup.description = "";
                timeCodeBreakGroup["value"] = 0;
                timeCodeBreakGroup["label"] = term;
                this.timeCodeBreakGroups.splice(0, 0, timeCodeBreakGroup);
            });

            _.forEach(this.timeCodeBreakGroups, (timeCodeBreakGroup: ITimeCodeBreakGroupGridDTO) => {
                timeCodeBreakGroup["value"] = timeCodeBreakGroup.timeCodeBreakGroupId;
                timeCodeBreakGroup["label"] = timeCodeBreakGroup.name;
            })
        });
    }

    // ACTIONS

    public addBreakTemplate(empty: boolean = false) {
        this.translationService.translate("core.notselected").then((term) => {
            var breakTemplate = new TimeBreakTemplateGridDTO();
            breakTemplate.actorCompanyId = CoreUtility.actorCompanyId;
            breakTemplate.shiftTypes = null;
            breakTemplate.dayTypes = null;
            breakTemplate.dayOfWeeks = null;
            breakTemplate.timeBreakTemplateId = 0;
            breakTemplate.shiftLength = empty ? 0 : 480;
            breakTemplate.shiftStartFromTimeMinutes = empty ? 0 : 480;
            breakTemplate.useMaxWorkTimeBetweenBreaks = false;
            breakTemplate.majorMinTimeAfterStart = 0;
            breakTemplate.majorMinTimeBeforeEnd = 0;
            breakTemplate.majorNbrOfBreaks = empty ? 0 : 1;
            breakTemplate.majorTimeCodeBreakGroupId = 0;
            breakTemplate.majorTimeCodeBreakGroupName = term;
            breakTemplate.minorMinTimeAfterStart = 0;
            breakTemplate.minorMinTimeBeforeEnd = 0;
            breakTemplate.minorNbrOfBreaks = 0;
            breakTemplate.minorTimeCodeBreakGroupId = 0;
            breakTemplate.minorTimeCodeBreakGroupName = term;
            breakTemplate.minTimeBetweenBreaks = 0;
            breakTemplate.startDate = null;
            breakTemplate.stopDate = null;
            breakTemplate.state = SoeEntityState.Active;

            if (this.breakTemplates && this.breakTemplates.length > 1) {
                var rowNrs = _.sortBy(this.breakTemplates.map(s => s.rowNr), 'rowNr');
                breakTemplate.rowNr = rowNrs[rowNrs.length - 1] + 1;
            }
            else
                breakTemplate.rowNr = 1;

            if (!this.breakTemplates)
                this.breakTemplates = [];
            this.breakTemplates.push(breakTemplate);
            this.gridOptions.focusRowByRow(breakTemplate, 0);
        });
    }

    public deleteBreakTemplate(breakTemplate: TimeBreakTemplateGridDTO) {
        this.gridOptions.deleteRow(breakTemplate);
        breakTemplate.state = SoeEntityState.Deleted;
        this.resetBreakTemplates(null);
    }

    protected showBreakTemplateValidationIcon(row: TimeBreakTemplateGridDTO): boolean {
        return row && row.isValid === false;
    }

    protected showBreakTemplateValidation(row: TimeBreakTemplateGridDTO) {
        if (!row || row.isValid === true)
            return;

        return this.translationService.translate("core.error").then((term) => {
            this.notificationService.showDialog(term, row.validationResult.errorMessage, SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
        });
    }

    public saveBreakTemplates() {
        this.progress.startSaveProgress((completion) => {
            this.scheduleService.validateTimeBreakTemplates(this.breakTemplates).then(x => {
                this.breakTemplates = x;

                var hasInvalid = (_.filter(this.breakTemplates, { isValid: false })).length > 0;
                if (!hasInvalid) {
                    this.scheduleService.saveTimeBreakTemplates(this.breakTemplates).then(result => {
                        if (result.success) {
                            completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.breakTemplates);
                            this.dirtyHandler.clean();
                            this.onLoadData();
                        } else {
                            if (result.integerValue && result.integerValue > 0) {
                                var breakTemplate = _.filter(this.breakTemplates, { rowNr: result.integerValue })[0];
                                if (breakTemplate) {
                                    breakTemplate.validationResult = result;
                                    this.resetBreakTemplates(null);
                                }
                                this.translationService.translate("time.schedule.timebreaktemplate.validationerrorrownr").then((term) => {
                                    completion.failed(result.errorMessage + ". " + term.format(result.integerValue.toString()));
                                });
                            }
                            else {
                                completion.failed(result.errorMessage);
                            }
                        }
                    })
                } else {
                    this.resetBreakTemplates(null);
                    var keys: string[] = [
                        "time.schedule.timebreaktemplate.validationerror",
                        "time.schedule.timebreaktemplate.validationerrorrownr",
                    ];
                    var invalidRows = _.sortBy(_.filter(this.breakTemplates, s => s.isValid === false), 'rowNr');
                    var invalidRowNrs: string = _.map(invalidRows, s => s.rowNr).join(', ');
                    this.translationService.translateMany(keys).then((terms) => {
                        if (invalidRows.length === 1)
                            completion.failed(invalidRows[0].validationResult.errorMessage + ". " + terms["time.schedule.timebreaktemplate.validationerrorrownr"].format(invalidRowNrs));
                        else
                            completion.failed(terms["time.schedule.timebreaktemplate.validationerror"].format(invalidRowNrs));
                    });
                }
            })
        }, this.guid);
    }

    // EVENTS

    protected shiftType_Changed(timeBreakTemplate: TimeBreakTemplateGridDTO) {

    }

    protected dayType_Changed(timeBreakTemplate: TimeBreakTemplateGridDTO) {

    }

    protected majorTimeCodeBreakGroupId_Changed(timeBreakTemplate: TimeBreakTemplateGridDTO) {
        var timeCodeBreakGroup = (_.filter(this.timeCodeBreakGroups, { timeCodeBreakGroupId: timeBreakTemplate.majorTimeCodeBreakGroupId }))[0];
        if (timeCodeBreakGroup) {
            timeBreakTemplate.majorTimeCodeBreakGroupName = timeCodeBreakGroup.name;
        }
    }

    protected minorTimeCodeBreakGroupId_Changed(timeBreakTemplate: TimeBreakTemplateGridDTO) {
        var timeCodeBreakGroup = (_.filter(this.timeCodeBreakGroups, { timeCodeBreakGroupId: timeBreakTemplate.minorTimeCodeBreakGroupId }))[0];
        if (timeCodeBreakGroup) {
            timeBreakTemplate.minorTimeCodeBreakGroupName = timeCodeBreakGroup.name;
        }
    }

    // HELP-METHODS

    private initGrid() {
        this.gridOptions = new SoeGridOptions("Time.Schedule.TimeBreakTemplate", this.$timeout, this.uiGridConstants);
        this.gridOptions.enableGridMenu = false;
        this.gridOptions.showGridFooter = false;
        this.gridOptions.enableRowSelection = true;
        this.gridOptions.expandableRowScope = {};
        this.gridOptions.enableDoubleClick = false;
        this.gridOptions.setMinRowsToShow(20);
        this.gridOptions.setData([]);

        var events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.BeginCellEdit, (entity, colDef, newValue, oldValue) => {
            this.currentlyEditing = { entity, colDef };
            if (colDef.field === 'shiftTypes') {
                if (Array.isArray(entity.shiftTypes)) {
                    if (entity.shiftTypes.length <= 1) {
                        entity.shiftTypes = entity.shiftTypes[0];
                    } else {
                        this.selectShiftTypes(entity);
                    }
                }
            } else if (colDef.field === 'dayTypes') {
                if (Array.isArray(entity.dayTypes)) {
                    if (entity.dayTypes.length <= 1) {
                        entity.dayTypes = entity.dayTypes[0];
                    } else {
                        this.selectDayTypes(entity);
                    }
                }
            } else if (colDef.field === 'dayOfWeeks') {
                if (Array.isArray(entity.dayOfWeeks)) {
                    if (entity.dayOfWeeks.length <= 1) {
                        entity.dayOfWeeks = entity.dayOfWeeks[0];
                    } else {
                        this.selectDayOfWeeks(entity);
                    }
                }
            } else if (colDef.field === 'shiftLength') {
                entity['shiftLength'] = CalendarUtility.minutesToTimeSpan(entity['shiftLength']);
            } else if (colDef.field === 'shiftStartFromTimeMinutes') {
                entity['shiftStartFromTimeMinutes'] = CalendarUtility.minutesToTimeSpan(entity['shiftStartFromTimeMinutes']);
            }
        }));
        events.push(new GridEvent(SoeGridOptionsEvent.AfterCellEdit, (entity, colDef, newValue, oldValue) => {
            if (colDef['soeType'] === Constants.GRID_COLUMN_TYPE_TYPEAHEAD && colDef.soeData.onBlur) {
                colDef.soeData.onBlur(entity, colDef);
            }

            if (colDef.field === 'shiftLength') {
                let span = CalendarUtility.parseTimeSpan(newValue);
                entity['shiftLength'] = CalendarUtility.timeSpanToMinutes(span);
            } else if (colDef.field === 'shiftStartFromTimeMinutes') {
                let span = CalendarUtility.parseTimeSpan(newValue);
                entity['shiftStartFromTimeMinutes'] = CalendarUtility.timeSpanToMinutes(span);
            }
        }));
        this.gridOptions.subscribe(events);
    }

    private setupGrid(): ng.IPromise<any> {

        this.setupDayOfWeeks();

        var keys: string[] = [
            "core.edit",
            "core.delete",
            "core.info",
            "core.select",
            "common.daytype",
            "common.rownr",
            "common.shifttype",
            "common.startdate",
            "common.stopdate",
            "common.weekday",
            "time.schedule.timebreaktemplate.lengthincludingbreak",
            "time.schedule.timebreaktemplate.shiftstartfromtime",
            "time.schedule.timebreaktemplate.majornbrofbreaks",
            "time.schedule.timebreaktemplate.nbrofbreaks",
            "time.schedule.timebreaktemplate.majorbreaktype",
            "time.schedule.timebreaktemplate.breaktype",
            "time.schedule.timebreaktemplate.mintimeafterstart",
            "time.schedule.timebreaktemplate.mintimebeforeend",
            "time.schedule.timebreaktemplate.max2hours",
            "time.schedule.timebreaktemplate.mintimebetweenbreaks",
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.gridOptions.addColumnText("rowNr", terms["common.rownr"], "50");
            this.gridOptions.addColumnText("shiftTypeNames", terms["common.shifttype"], null, true, null, terms["core.edit"], null, null, null, null, "fal fa-pencil iconEdit", "selectShiftTypes");
            this.gridOptions.addColumnText("dayOfWeekNames", terms["common.weekday"], null, true, null, terms["core.edit"], null, null, null, null, "fal fa-pencil iconEdit", "selectDayOfWeeks");
            this.gridOptions.addColumnTimeSpan("shiftLength", terms["time.schedule.timebreaktemplate.lengthincludingbreak"], "100");
            this.gridOptions.addColumnTimeSpan("shiftStartFromTimeMinutes", terms["time.schedule.timebreaktemplate.shiftstartfromtime"], "100");
            this.gridOptions.addColumnText("majorNbrOfBreaks", terms["time.schedule.timebreaktemplate.majornbrofbreaks"], "100");
            this.gridOptions.addColumnSelect("majorTimeCodeBreakGroupId", terms["time.schedule.timebreaktemplate.majorbreaktype"], "80", this.timeCodeBreakGroups, false, true, "majorTimeCodeBreakGroupName", "timeCodeBreakGroupId", "name", "majorTimeCodeBreakGroupId_Changed");
            this.gridOptions.addColumnText("majorMinTimeAfterStart", terms["time.schedule.timebreaktemplate.mintimeafterstart"], "100");
            this.gridOptions.addColumnText("majorMinTimeBeforeEnd", terms["time.schedule.timebreaktemplate.mintimebeforeend"], "100");
            this.gridOptions.addColumnText("minorNbrOfBreaks", terms["time.schedule.timebreaktemplate.nbrofbreaks"], "100");
            this.gridOptions.addColumnSelect("minorTimeCodeBreakGroupId", terms["time.schedule.timebreaktemplate.breaktype"], "80", this.timeCodeBreakGroups, false, true, "minorTimeCodeBreakGroupName", "timeCodeBreakGroupId", "name", "minorTimeCodeBreakGroupId_Changed");
            this.gridOptions.addColumnText("minorMinTimeAfterStart", terms["time.schedule.timebreaktemplate.mintimeafterstart"], "100");
            this.gridOptions.addColumnText("minorMinTimeBeforeEnd", terms["time.schedule.timebreaktemplate.mintimebeforeend"], "100");
            this.gridOptions.addColumnText("minTimeBetweenBreaks", terms["time.schedule.timebreaktemplate.mintimebetweenbreaks"], "100");
            this.gridOptions.addColumnDate("startDate", terms["common.startdate"], "90");
            this.gridOptions.addColumnDate("stopDate", terms["common.startdate"], "90");
            this.gridOptions.addColumnIcon(null, "fal fa-info-circle iconEdit", terms["core.info"], "showBreakTemplateValidation", null, "showBreakTemplateValidationIcon", null, null, null, false);
            if (this.modifyPermission)
                this.gridOptions.addColumnDelete(terms["core.delete"], "deleteBreakTemplate");

            var sumColumnDefs: uiGrid.IColumnDef[] = [];
            _.forEach(this.gridOptions.getColumnDefs(), (colDef: uiGrid.IColumnDef) => {
                colDef.enableColumnMenu = false;
                colDef.enableCellEdit = (colDef['soeType'] !== Constants.GRID_COLUMN_TYPE_ICON && colDef.field !== "rowNr" && colDef.field !== "shiftTypeNames" && colDef.field !== "dayOfWeekNames");

                if (colDef.field === "shiftTypeNames" || colDef.field == "dayOfWeekNames")
                    colDef.minWidth = 100;

                if (colDef.field === "majorNbrOfBreaks" || colDef.field === "majorTimeCodeBreakGroupId" || colDef.field === "majorMinTimeAfterStart" || colDef.field === "majorMinTimeBeforeEnd") {
                    var cellcls1: string = colDef.cellClass ? colDef.cellClass.toString() : "";
                    colDef.cellClass = (grid: any, row, col, rowRenderIndex, colRenderIndex) => {
                        var c = cellcls1 + " successRow";
                        return c;
                    };
                    sumColumnDefs.push(colDef);
                } else if (colDef.field === "minorNbrOfBreaks" || colDef.field === "minorTimeCodeBreakGroupId" || colDef.field === "minorMinTimeAfterStart" || colDef.field === "minorMinTimeBeforeEnd") {
                    var cellcls2: string = colDef.cellClass ? colDef.cellClass.toString() : "";
                    colDef.cellClass = (grid: any, row, col, rowRenderIndex, colRenderIndex) => {
                        var c = cellcls2 + " infoRow";
                        return c;
                    };
                    sumColumnDefs.push(colDef);
                } else if (colDef.field === "useMaxWorkTimeBetweenBreaks" || colDef.field === "minTimeBetweenBreaks") {
                    var cellcls3: string = colDef.cellClass ? colDef.cellClass.toString() : "";
                    colDef.cellClass = (grid: any, row, col, rowRenderIndex, colRenderIndex) => {
                        var c = cellcls3 + " errorRow";
                        return c;
                    };
                    sumColumnDefs.push(colDef);
                }
            });
            sumColumnDefs.forEach(col => {
                col.aggregationType = this.uiGridConstants.aggregationTypes.sum;
                col.aggregationHideLabel = true;
                col.footerCellFilter = 'number:2';
                col.footerCellTemplate =
                    '<div class="ui-grid-cell-contents" col-index="renderIndex">' +
                    '<div class="pull-right">{{col.getAggregationText() + (col.getAggregationValue() CUSTOM_FILTERS )}}</div>' +
                    '</div>';
            });
        });
    }

    protected onBlur(entity, colDef) {
        if (colDef.field === 'shiftTypes') {
            // If only one selected (from type ahead) insert it into an array
            if (!Array.isArray(entity.shiftTypes))
                entity.shiftTypes = [entity.shiftTypes];
        }
        if (colDef.field === 'dayTypes') {
            // If only one selected (from type ahead) insert it into an array
            if (!Array.isArray(entity.dayTypes))
                entity.dayTypes = [entity.dayTypes];
        }
        if (colDef.field === 'dayOfWeeks') {
            // If only one selected (from type ahead) insert it into an array
            if (!Array.isArray(entity.dayOfWeeks))
                entity.dayOfWeeks = [entity.dayOfWeeks];
        }
    }

    protected allowNavigationFromTypeAhead(entity, colDef) {
        return true;
    }

    public handleKeyPressInEditCell(evt) {
        if (!this.currentlyEditing) {
            return undefined;
        }

        if ((evt.keyCode === 13 || evt.keyCode === 38 || evt.keyCode === 40) && this.currentlyEditing.colDef['soeType'] === Constants.GRID_COLUMN_TYPE_TYPEAHEAD) {
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
        var row = this.gridOptions.getCurrentRow();

        var colDefs = this.gridOptions.getColumnDefs();

        //this is a naive implementation that assumes that all columns are editable. 
        for (var i = 0; i < colDefs.length; i++) {
            if (colDefs[i] === coldef) {
                if (i !== colDefs.length - 1) {
                    this.gridOptions.scrollToFocus(row, i + 1);
                } else {
                    var nextRow = this.findNextRow(row);
                    if (nextRow)
                        this.gridOptions.scrollToFocus(nextRow, 0);
                }
            }
        }
    }

    protected findNextRow(row) {
        var index = this.findIndex(row);
        var data = this.gridOptions.getData();

        if (index === data.length - 1)
            return null;

        return data[index + 1];
    }

    protected findIndex(row) {
        var entity = row.entity || row;

        // find real row by comparing $$hashKey with entity in row
        var rowIndex = -1;
        var hash = entity.$$hashKey;
        var data = this.gridOptions.getData();     // original rows of data
        for (var ndx = 0; ndx < data.length; ndx++) {
            if (data[ndx].$$hashKey === hash) {
                rowIndex = ndx;
                break;
            }
        }
        return rowIndex;
    }

    public filterShiftTypes(filter) {
        return this.shiftTypes.filter(s => {
            return s.name.contains(filter);
        });
    }

    public filterDayTypes(filter) {
        return this.dayTypes.filter(s => {
            return s.name.contains(filter);
        });
    }

    public filterDayOfWeeks(filter) {
        return this.dayOfWeeks.filter(s => {
            return s.name.contains(filter);
        });
    }

    protected selectShiftTypes(row) {
        if (!Array.isArray(row.shiftTypes))
            row.shiftTypes = [row.shiftTypes];

        var modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Dialogs/SelectShiftTypes/selectShiftTypes.html"),
            controller: SelectShiftTypesController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'lg',
            resolve: {
                shiftTypes: () => { return this.shiftTypes },
                selectedShiftTypes: () => { return _.map(_.filter(row.shiftTypes, s => s != null), s => s['shiftTypeId']) }
            }
        });

        modal.result.then(result => {
            if (result && result.success) {
                row.shiftTypes = _.filter(this.shiftTypes, s => _.includes(result.selectedShiftTypes, s.shiftTypeId));
            }
        }, function () {
            // Cancelled
        });

        return modal;
    }

    protected selectDayTypes(row) {
        if (!Array.isArray(row.dayTypes))
            row.dayTypes = [row.dayTypes];

        var modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Dialogs/SelectDayTypes/selectDayTypes.html"),
            controller: SelectDayTypesController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'lg',
            resolve: {
                dayTypes: () => { return this.dayTypes },
                selectedDayTypes: () => { return _.map(_.filter(row.dayTypes, s => s != null), s => s['dayTypeId']) }
            }
        });

        modal.result.then(result => {
            if (result && result.success) {
                row.dayTypes = _.filter(this.dayTypes, s => _.includes(result.selectedDayTypes, s.dayTypeId));
            }
        }, function () {
            // Cancelled
        });

        return modal;
    }

    protected selectDayOfWeeks(row) {
        if (!Array.isArray(row.dayOfWeeks))
            row.dayOfWeeks = [row.dayOfWeeks];

        var modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Dialogs/SelectDayOfWeeks/selectDayOfWeeks.html"),
            controller: SelectDayOfWeeksController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'lg',
            resolve: {
                dayOfWeeks: () => { return this.dayOfWeeks },
                selectedDayOfWeeks: () => { return _.map(_.filter(row.dayOfWeeks, s => s != null), s => s['id']) }
            }
        });

        modal.result.then(result => {
            if (result && result.success) {
                row.dayOfWeeks = _.filter(this.dayOfWeeks, s => _.includes(result.selectedDayOfWeeks, s.id));
            }
        }, function () {
            // Cancelled
        });

        return modal;
    }

    protected resetBreakTemplates(breakTemplate: any) {
        if (this.breakTemplates) {
            var rowNr: number = 1;
            _.forEach(this.breakTemplates, bt => {
                bt.rowNr = rowNr;
                rowNr++;
            });
            this.gridOptions.setData(this.breakTemplates);
        }
        if (breakTemplate) {
            this.gridOptions.scrollToFocus(breakTemplate, 0);
        }
    }

    protected isValidForLoad() {
        return this.timeCodeBreakGroups && this.shiftTypes && this.dayTypes;
    }
}
