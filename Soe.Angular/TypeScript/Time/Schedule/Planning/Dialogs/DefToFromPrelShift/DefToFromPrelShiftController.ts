import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { IScheduleService } from "../../../ScheduleService";
import { EditControllerBase } from "../../../../../Core/Controllers/EditControllerBase";
import { EmployeeListDTO } from "../../../../../Common/Models/EmployeeListDTO";
import { ISoeGridOptions, SoeGridOptions } from "../../../../../Util/SoeGridOptions";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { SOEMessageBoxImage, SOEMessageBoxButtons, SOEMessageBoxSize } from "../../../../../Util/Enumerations";
import { Feature } from "../../../../../Util/CommonEnumerations";

export class DefToFromPrelShiftController extends EditControllerBase {

    private employees: EmployeeListDTO[] = [];
    private soeGridOptions: ISoeGridOptions;

    // Terms
    private terms: { [index: string]: string; };
    private title: any;
    private infoText: any;

    // Flags
    private includeScheduleShifts: boolean = true;
    private includeStandbyShifts: boolean = false;
    private standbyModifyPermission: boolean = false;
    private executing: boolean = false;

    //@ngInject
    constructor(private $uibModalInstance,
        $uibModal,
        coreService: ICoreService,
        private $timeout: ng.ITimeoutService,
        private scheduleService: IScheduleService,
        translationService: ITranslationService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        private uiGridConstants: uiGrid.IUiGridConstants,
        urlHelperService: IUrlHelperService,
        private prelToDef: boolean,
        private dateFrom: Date,
        private dateTo: Date,
        private employeeId: number,
        private filteredEmployeeIds: number[]) {

        super("", Feature.Time_Schedule_SchedulePlanning, $uibModal, translationService, messagingService, coreService, notificationService, urlHelperService);

        this.soeGridOptions = new SoeGridOptions("", this.$timeout, this.uiGridConstants);
        this.soeGridOptions.enableGridMenu = false;
        this.soeGridOptions.enableColumnMenus = false;
        this.soeGridOptions.enableFiltering = false;
        this.soeGridOptions.showGridFooter = true;
        this.soeGridOptions.setMinRowsToShow(12);
        this.soeGridOptions.enableRowSelection = true;
        this.setupGrid();
        this.loadModifyPermissions();
        this.loadEmployees();
    }

    protected setupGrid() {
        var keys: string[] = [
            "time.schedule.planning.deftofromprel.employeename",
            "time.schedule.planning.deftofromprel.employeenr",
            "time.schedule.planning.deftofromprel.titlepreltodef",
            "time.schedule.planning.deftofromprel.titledeftoprel",
            "time.schedule.planning.deftofromprel.infodeftoprel",
            "time.schedule.planning.deftofromprel.infopreltodef",
            "time.schedule.planning.deftofromprel.dayswarning",
            "common.obs"
        ];

        this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;

            if (this.prelToDef) {
                this.title = terms["time.schedule.planning.deftofromprel.titlepreltodef"];
                this.infoText = terms["time.schedule.planning.deftofromprel.infopreltodef"];
            } else {
                this.title = terms["time.schedule.planning.deftofromprel.titledeftoprel"];
                this.infoText = terms["time.schedule.planning.deftofromprel.infodeftoprel"];
            }

            this.soeGridOptions.addColumnText("employeeNr", terms["time.schedule.planning.deftofromprel.employeenr"], "70");
            this.soeGridOptions.addColumnText("name", terms["time.schedule.planning.deftofromprel.employeename"], null);
        });
    }

    private loadModifyPermissions(): ng.IPromise<any> {
        var features: number[] = [];

        features.push(Feature.Time_Schedule_SchedulePlanning_StandbyShifts);

        return this.coreService.hasReadOnlyPermissions(features).then((x) => {
            this.standbyModifyPermission = x[Feature.Time_Schedule_SchedulePlanning_StandbyShifts];
            if (this.standbyModifyPermission)
                this.includeStandbyShifts = true;
        });
    }

    private loadEmployees() {
        this.startLoad();
        this.scheduleService.getEmployeesForDefToFromPrelShift(this.prelToDef, this.dateFrom, this.dateTo, this.employeeId, this.filteredEmployeeIds).then(x => {
            this.employees = x;
            this.soeGridOptions.setData(this.employees);
            this.stopProgress();
        });
    }

    private save() {
        this.executing = true;

        if (this.validateDates()) {
            this.saveShifts();
        } else {
            this.notificationService.showDialog(this.terms["common.obs"], this.terms["time.schedule.planning.deftofromprel.dayswarning"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK, SOEMessageBoxSize.Medium);
            this.executing = false;
        }
    }

    private validateDates(): boolean {
        var validDates: boolean = true;
        var diffDays: number = this.dateTo.diffDays(this.dateFrom);
        if (diffDays < 0)
            validDates = false;

        return validDates;
    }

    private saveShifts() {
        var employeeIds: number[] = this.soeGridOptions.getSelectedIds("employeeId");
        this.scheduleService.saveDefToFromPrelShift(this.prelToDef, this.dateFrom, this.dateTo, employeeIds, this.includeScheduleShifts, this.includeStandbyShifts).then(result => {
            if (result.success) {
                this.close(employeeIds);
            } else {
                this.failedSave(result.errorMessage);
                this.executing = false;
            }
        }, error => {
            this.failedSave(error.message);
            this.executing = false;
        });
    }

    private cancel() {
        this.$uibModalInstance.dismiss('cancel');
    }

    private close(employeeIds: number[]) {
        this.$uibModalInstance.close({ success: true, employeeIds: employeeIds });
    }
}
