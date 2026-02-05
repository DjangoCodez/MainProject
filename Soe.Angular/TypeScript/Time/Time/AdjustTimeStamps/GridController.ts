import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { ITimeService } from "../../../Time/Time/TimeService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { SoeGridOptionsEvent } from "../../../Util/Enumerations";
import { IEmployeeSmallDTO } from "../../../Scripts/TypeLite.Net4";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { EmployeeService as SharedEmployeeService } from "../../../Shared/Time/Employee/EmployeeService";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { TimeStampEntryDTO } from "../../../Common/Models/TimeStampDTOs";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { Feature } from "../../../Util/CommonEnumerations";
import { TimeStampDetailsController } from "../../Directives/AttestEmployee/Dialogs/TimeStampDetails/TimeStampDetailsController";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    // Lookups
    private terms: { [index: string]: string; };
    private employees: IEmployeeSmallDTO[];

    // Data
    timeStampEntries: TimeStampEntryDTO[] = [];

    // GUI
    toolbarInclude: any;
    gridHeaderComponentUrl: any;
    gridFooterComponentUrl: any;

    private selectedEmployees: any[] = [];
    private selectedDateFrom: Date;
    private selectedDateTo: Date;

    // Row
    currentTimeStampId: number;

    // Properties
    get saveEnabled() {
        return _.filter(this.timeStampEntries, (e) => e.isModified).length > 0;
    }

    //@ngInject
    constructor(
        private $timeout: ng.ITimeoutService,
        private timeService: ITimeService,
        private sharedEmployeeService: SharedEmployeeService,
        private translationService: ITranslationService,
        private urlHelperService: IUrlHelperService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        private $scope: ng.IScope,
        private $uibModal,
        private $q: ng.IQService) {
        super(gridHandlerFactory, "Time.Time.AdjustTimeStamps", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onDoLookUp(() => this.loadLookups())
            .onBeforeSetUpGrid(() => this.loadTerms())
            .onSetUpGrid(() => this.setUpGrid());

        this.toolbarInclude = urlHelperService.getViewUrl("toolbarInclude.html");
        this.gridHeaderComponentUrl = urlHelperService.getViewUrl("gridHeader.html");
        this.gridFooterComponentUrl = urlHelperService.getViewUrl("gridFooter.html");
    }

    public onInit(parameters: any) {
        this.flowHandler.start([{ feature: Feature.Time_Time_Attest_AdjustTimeStamps, loadReadPermissions: true, loadModifyPermissions: true }]);

        //Set dates
        this.selectedDateFrom = this.selectedDateTo = CalendarUtility.getDateToday();
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Time_Time_Attest_AdjustTimeStamps].readPermission;
        this.modifyPermission = response[Feature.Time_Time_Attest_AdjustTimeStamps].modifyPermission;
    }

    public edit(row: any) {
    };

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.search());
        this.toolbar.addInclude(this.toolbarInclude);
    }

    private loadLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.loadTerms(),
            this.loadEmployees()
        ]);
    }

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "core.yes",
            "core.no",
            "core.comment",
            "common.name",
            "common.type",
            "common.date",
            "common.time",
            "common.accounting",
            "common.entitylogviewer.changelog",
            "common.time.timedeviationcause",
            "time.employee.employee.employeenr",
            "time.time.timeterminal.timeterminal",
            "time.time.adjusttimestamps.adjusted",
            "time.time.adjusttimestamps.belongstodate",
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private loadEmployees(): ng.IPromise<any> {
        return this.sharedEmployeeService.getEmployeesForGridSmall(true).then(x => {
            this.employees = x;
        });
    }

    public search() {
        this.progress.startLoadingProgress([() => {
            var employeeIds = _.map(this.selectedEmployees, e => e.employeeId);
            return this.timeService.searchTimeStampEntries(employeeIds, this.selectedDateFrom, this.selectedDateTo).then((x) => {
                this.timeStampEntries = x;
                _.forEach(this.timeStampEntries, (y) => {
                    y.date = CalendarUtility.convertToDate(y.date);
                    y.time = y.adjustedTime = CalendarUtility.convertToDate(y.time);
                    y.adjustedTimeBlockDateDate = CalendarUtility.convertToDate(y.adjustedTimeBlockDateDate);
                    y['accountNrName'] = y.accountNr ? y.accountNr + " - " + y.accountName : "";
                    y['manuallyAdjustedString'] = y.manuallyAdjusted ? this.terms["core.yes"] : this.terms["core.no"];
                });
                this.setData(this.timeStampEntries);
            });
        }]);
    }

    public save() {
        this.progress.startSaveProgress((completion) => {
            var adjustedTimeStamps = _.filter(this.timeStampEntries, (e) => e.isModified);
            this.timeService.saveAdjustedTimeStampEntries(adjustedTimeStamps).then((result) => {
                if (result.success) {
                    completion.completed();
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid).then(data => {
            this.search();
        });
    }

    private setUpGrid() {
        this.gridAg.options.enableRowSelection = false;
        this.gridAg.addColumnIsModified("isModified", "", 20);
        this.gridAg.addColumnText("employeeNr", this.terms["time.employee.employee.employeenr"], null);
        this.gridAg.addColumnText("employeeName", this.terms["common.name"], null, false);
        this.gridAg.addColumnText("timeTerminalId", this.terms["time.time.timeterminal.timeterminal"], null, false);
        this.gridAg.addColumnText("typeName", this.terms["common.type"], null);
        this.gridAg.addColumnText("manuallyAdjustedString", this.terms["time.time.adjusttimestamps.adjusted"], null);
        this.gridAg.addColumnDate("date", this.terms["common.date"], null);
        this.gridAg.addColumnTime("adjustedTime", this.terms["common.time"], null, { editable: true, cellFilter: 'minutesToTimeSpan', handleAsTimeSpan: true });
        this.gridAg.addColumnDate("adjustedTimeBlockDateDate", this.terms["time.time.adjusttimestamps.belongstodate"], null, false, null, { editable: true });
        this.gridAg.addColumnText("timeDeviationCauseName", this.terms["common.time.timedeviationcause"], null);
        this.gridAg.addColumnText("accountNrName", this.terms["common.accounting"], null);
        this.gridAg.addColumnText("note", this.terms["core.comment"], null);
        this.gridAg.addColumnIcon(null, " ", null, { icon: "fal fa-history", onClick: this.showTimeStampDetails.bind(this), toolTip: this.terms["common.entitylogviewer.changelog"] });

        this.$timeout(() => {
        this.gridAg.options.getColumnDefs()
            .forEach(f => {
                var cellCls: string = f.cellClass ? f.cellClass.toString() : "";
                f.cellClass = (grid: any) => {
                    // Append modifiedCell to cellClass on editable columns
                    if (f.field === 'adjustedTime') {
                        var currentRow = this.gridAg.options.getCurrentRow();
                        if (!currentRow)
                            return;

                        cellCls = ((grid.data['timeModified'] === true && currentRow.timeStampEntryId === grid.data.timeStampEntryId) ? "time text-right modifiedCell" : "time text-right");
                    } else if (f.field === 'adjustedTimeBlockDateDate') {
                        cellCls = (grid.data['dateModified'] === true ? "date modifiedCell" : "date");
                    }

                    return cellCls;
                };
            })
        });

        // Events
        var events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.AfterCellEdit, (entity, colDef, newValue, oldValue) => {
            this.afterCellEdit(entity, colDef, newValue, oldValue);
        }));
        this.gridAg.options.subscribe(events);

        this.gridAg.finalizeInitGrid("time.time.adjusttimestamps.adjusttimestamps", true);
    }

    private afterCellEdit(row: TimeStampEntryDTO, colDef: uiGrid.IColumnDef, newValue, oldValue) {
        if (newValue === oldValue)
            return;

        switch (colDef.field) {
            case 'adjustedTime':
                if (!row['timeOriginal'])
                    row['timeOriginal'] = oldValue;
                row['timeModified'] = row['timeOriginal'] && CalendarUtility.convertToDate(row['timeOriginal']).isSameMinuteAs(CalendarUtility.convertToDate(newValue)) ? false : true;
                break;
            case 'adjustedTimeBlockDateDate':
                if (!row['dateOriginal'])
                    row['dateOriginal'] = oldValue;
                row['dateModified'] = row['dateOriginal'] && CalendarUtility.convertToDate(row['dateOriginal']).isSameDayAs(CalendarUtility.convertToDate(newValue)) ? false : true;
                break;
        }

        this.$timeout(() => {
            row.isModified = row['timeModified'] || row['dateModified'];
            this.gridAg.options.refreshRows(row);
        });
    }

    private showTimeStampDetails(timeStamp: TimeStampEntryDTO) {
        if (!timeStamp?.timeStampEntryId)
            return;

        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Directives/AttestEmployee/Dialogs/TimeStampDetails/TimeStampDetails.html"),
            controller: TimeStampDetailsController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'xl',
            windowClass: 'fullsize-modal',
            resolve: {
                timeStampEntryId: () => { return timeStamp.timeStampEntryId }
            }
        }
        this.$uibModal.open(options);
    }
}