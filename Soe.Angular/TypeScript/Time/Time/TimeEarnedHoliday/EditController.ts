import { IMessagingHandler } from "../../../Core/Handlers/MessagingHandler";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { EditControllerBase } from "../../../Core/Controllers/EditControllerBase";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../Core/Services/UrlHelperService";
import { IEditControllerFlowHandler } from "../../../Core/Handlers/ControllerFlowHandler";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandler } from "../../../Core/Handlers/ProgressHandler";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IToolbar } from "../../../Core/Handlers/Toolbar";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IValidationSummaryHandler } from "../../../Core/Handlers/ValidationSummaryHandler";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { ISoeGridOptions, SoeGridOptions } from "../../../Util/SoeGridOptions";
import { EmployeeEarnedHolidayDTO } from "../../../Common/Models/EmployeeEarnedHolidayDTO";
import { SmallGenericType } from "../../../Common/Models/smallgenerictype";
import { HolidayDTO } from "../../../Common/Models/HolidayDTO";
import { ToolBarButtonGroup, ToolBarUtility, ToolBarButton } from "../../../Util/ToolBarUtility";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ITimeService } from "../TimeService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { IconLibrary, SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../Util/Enumerations";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { Feature } from "../../../Util/CommonEnumerations";

export class EditController extends EditControllerBase {

    //Data
    private yearId: number;
    private holidayId: number;
    private employeeEarnedHolidays: EmployeeEarnedHolidayDTO[];
    private loadSuggestions: boolean;

    //Lookups
    private terms: any;
    private gridTerms: any;
    private years: SmallGenericType[];
    private holidays: HolidayDTO[];
    private filteredHolidays: HolidayDTO[];

    // Grid
    protected gridOptions: ISoeGridOptions;

    // ToolBar
    protected gridButtonGroups = new Array<ToolBarButtonGroup>();

    //@ngInject
    constructor(
        private timePeriodAccountValueId: number,
        private $timeout: ng.ITimeoutService,
        private $window: ng.IWindowService,
        $uibModal,
        coreService: ICoreService,
        private timeService: ITimeService,
        translationService: ITranslationService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        private uiGridConstants: uiGrid.IUiGridConstants) {

        super("Time.Time.TimeEarnedHoliday.Edit", Feature.Time_Time_EarnedHoliday, $uibModal, translationService, messagingService, coreService, notificationService, urlHelperService);
        this.initGrid();
    }

    private initGrid() {
        this.gridOptions = new SoeGridOptions("Time.Time.TimeEarnedHoliday", this.$timeout, this.uiGridConstants);
        this.gridOptions.enableGridMenu = false;
        this.gridOptions.showGridFooter = false;
        this.gridOptions.enableRowSelection = true;
        this.gridOptions.expandableRowScope = {};
        this.gridOptions.setData([]);
    }

    private setupGrid() {
        var keys: string[] = [
            "common.name",
            "time.employee.employee.employeenr",
            "time.employee.employee.percent",
            "time.employee.work5daysperweek",
            "time.employee.hasearnedholiday",
            "time.employee.suggestionearnedholiday",
            "common.note",
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.gridTerms = terms;
            this.gridOptions.addColumnText("employeeNr", terms["time.employee.employee.employeenr"], "100");
            this.gridOptions.addColumnText("employeeName", terms["common.name"], "200");
            this.gridOptions.addColumnText("employeePercent", terms["time.employee.employee.percent"], "200");
            this.gridOptions.addColumnText("work5DaysPerWeekString", terms["time.employee.work5daysperweek"], "200");
            this.gridOptions.addColumnText("hasTransactionString", terms["time.employee.hasearnedholiday"], "200");
            this.gridOptions.addColumnText("suggestionString", terms["time.employee.suggestionearnedholiday"], "200");
            this.gridOptions.addColumnText("suggestionNote", terms["common.note"], null);

            _.forEach(this.gridOptions.getColumnDefs(), (colDef: uiGrid.IColumnDef) => {
                colDef.enableColumnResizing = true;
                colDef.enableSorting = true;
                colDef.enableColumnMenu = false;
                colDef.enableCellEdit = true;
                colDef.enableFiltering = true;
            });
        });
    }

    private setupToolBar() {
        if (this.gridButtonGroups) {
            this.gridButtonGroups.length = 0;
        }

        //Reload
        this.gridButtonGroups.push(ToolBarUtility.createGroup(new ToolBarButton("time.employee.loademployees", "time.employee.loademployees", IconLibrary.FontAwesome, "fa-sync",
            () => {
                this.loadEarnedHolidaysContent(false);
            },
            () => {
                return !this.isValidForLoad();
            }
        )));
    }

    protected setupLookups() {
        this.setupGrid(); //must be called after permissions in base class is done            
        this.lookups = 3;
        this.startLoad();
        this.loadTerms();
        this.loadYears();
        this.loadHolidays();

    }

    // LOOKUPS

    private loadTerms(): ng.IPromise<any> {

        var keys: string[] = [
            "time.time.timeearnedholiday.savetransactions",
            "time.time.timeearnedholiday.savetransactionsmessage",
            "time.time.timeearnedholiday.deletetransactions",
            "time.time.timeearnedholiday.deletetransactionsmessage",
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
            this.lookupLoaded();
        });
    }

    private loadYears() {
        this.startLoad();
        this.timeService.getYears(1).then((x: SmallGenericType[]) => {
            this.years = x;
            this.yearId = CalendarUtility.getDateNow().getFullYear();
            this.isDirty = false;
            this.lookupLoaded();
        });
    }

    private loadHolidays() {
        this.startLoad();
        this.timeService.getHolidays(0, true, true, true).then((x: HolidayDTO[]) => {
            _.forEach(x, (y: HolidayDTO) => {
                y['year'] = CalendarUtility.convertToDate(y.date).getFullYear();
            });
            this.holidays = x;
            this.filterHolidays();
            this.holidayId = 0;
            this.isDirty = false;
            this.lookupLoaded();
        });
    }

    private loadEarnedHolidaysContent(clearContent: boolean) {
        if (!this.isValidForLoad())
            return;

        if (clearContent)
            this.employeeEarnedHolidays = null;

        this.startLoad();
        this.timeService.loadEarnedHolidaysContent(this.holidayId, this.yearId, this.loadSuggestions, this.employeeEarnedHolidays).then((x: EmployeeEarnedHolidayDTO[]) => {
            this.employeeEarnedHolidays = x;
            this.gridOptions.setData(this.employeeEarnedHolidays);
            this.isDirty = false;
            this.stopProgress();
        });
    }

    // EVENTS

    protected lookupLoaded() {
        super.lookupLoaded();
        if (this.lookups <= 0) {
            this.setupToolBar();
        }
    }

    private filterHolidays() {

        this.$timeout(() => {
            var filtered = _.filter(this.holidays, h => CalendarUtility.convertToDate(h.date).getFullYear() == this.yearId);
            this.filteredHolidays = filtered;

        });
    }

    protected initSaveTransactions() {
        if (!this.isValidForAction())
            return;

        // Show verification dialog
        var keys: string[] = [
            "time.time.timeearnedholiday.savetransactions",
            "time.time.timeearnedholiday.savetransactionsmessage",
        ];
        this.translationService.translateMany(keys).then((terms) => {
            var modal = this.notificationService.showDialog(terms["time.time.timeearnedholiday.savetransactions"], terms["time.time.timeearnedholiday.savetransactionsmessage"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
            modal.result.then(val => {
                if (val) {
                    this.saveTransactions();
                }
            });
        });
    }

    protected initDeleteTransactions() {
        if (!this.isValidForAction())
            return;

        // Show verification dialog
        var keys: string[] = [
            "time.time.timeearnedholiday.deletetransactions",
            "time.time.timeearnedholiday.deletetransactionsmessage",
        ];
        this.translationService.translateMany(keys).then((terms) => {
            var modal = this.notificationService.showDialog(terms["time.time.timeearnedholiday.deletetransactions"], terms["time.time.timeearnedholiday.deletetransactionsmessage"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
            modal.result.then(val => {
                if (val) {
                    this.deleteTransactions();
                }
            });
        });
    }

    private saveTransactions() {
        this.startSave();
        this.timeService.saveEarnedHolidayTransactions(this.holidayId, this.getSelectedEmployeeIds(), this.yearId).then((result) => {
            if (result.success) {
                this.completedSave(null);
                this.loadEarnedHolidaysContent(true);
            } else {
                this.failedSave(result.errorMessage);
            }
        }, error => {
            this.failedSave(error.message);
        });
    }

    private deleteTransactions() {
        this.startDelete();
        this.timeService.deleteEarnedHolidayTransactions(this.holidayId, this.getSelectedEmployeeIds(), this.yearId).then((result) => {
            if (result.success) {
                this.completedDelete(null);
                this.loadEarnedHolidaysContent(true);
            } else {
                this.failedDelete(result.errorMessage);
            }
        }, error => {
            this.failedDelete(error.message);
        });
    }

    // HELP-METHODS

    protected getSelectedEmployeeIds() {
        var employeeIds: number[] = [];
        var selectedRows = this.gridOptions.getSelectedRows();
        _.forEach(selectedRows, (row: any) => {
            employeeIds.push(row.employeeId);
        });
        return employeeIds;
    }

    protected isValidForLoad() {
        return this.yearId && this.yearId > 0 && this.holidayId && this.holidayId > 0;
    }

    protected isValidForAction() {
        return this.isValidForLoad() && this.employeeEarnedHolidays && this.employeeEarnedHolidays.length > 0 && this.getSelectedEmployeeIds().length > 0;
    }
}
