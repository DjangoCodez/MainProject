import { ICoreService } from "../../../../Core/Services/CoreService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { EditAssignmentHelper } from "../../../../Shared/Time/Schedule/Planning/Dialogs/EditAssignment/EditAssignmentHelper";
import { EditShiftHelper } from "../../../../Shared/Time/Schedule/Planning/Dialogs/EditShift/EditShiftHelper";
import { IScheduleService as ISharedScheduleService } from "../../../../Shared/Time/Schedule/ScheduleService";
import { SoeModule } from "../../../../Util/CommonEnumerations";
import { ISoeGridOptions } from "../../../../Util/SoeGridOptions";
import { WantedShiftsGaugeDTO } from "../../../Models/DashboardDTOs";
import { WidgetControllerBase } from "../../Base/WidgetBase";

export class WantedShiftsGaugeDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {
        return WidgetControllerBase.getWidgetDirective(urlHelperService.getWidgetUrl('WantedShifts', 'WantedShiftsGauge.html'), WantedShiftsGaugeController);
    }
}

class WantedShiftsGaugeController extends WidgetControllerBase {

    private editShiftHelper: EditShiftHelper;
    private editAssignmentHelper: EditAssignmentHelper;

    private soeGridOptions: ISoeGridOptions;
    private wantedShifts: WantedShiftsGaugeDTO[] = [];

    private employeeId: number = 0;
    private employeeGroupId: number = 0;

    // Terms
    private terms: { [index: string]: string; };
    private shiftTypeLabel: string;

    // Flags
    private isSchedulePlanningMode: boolean = false;
    private isOrderPlanningMode: boolean = false;

    //@ngInject
    constructor(
        $timeout: ng.ITimeoutService,
        $q: ng.IQService,
        private $uibModal,
        private coreService: ICoreService,
        private sharedScheduleService: ISharedScheduleService,
        private urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        uiGridConstants: uiGrid.IUiGridConstants) {
        super($timeout, $q, uiGridConstants);

        this.soeGridOptions = this.createGrid();
    }

    protected setup(): ng.IPromise<any> {
        var deferral = this.$q.defer();

        if (this.widgetUserGauge.module === SoeModule.Time)
            this.isSchedulePlanningMode = true;
        else if (this.widgetUserGauge.module === SoeModule.Billing)
            this.isOrderPlanningMode = true;

        this.$q.all([
            this.loadTerms(),
            this.loadEmployee()
        ]).then(() => {
            this.setupGrid();
            deferral.resolve();
        });
        return deferral.promise;
    }

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "core.edit",
            "common.day",
            "common.date",
            "common.time",
            "common.type",
            "common.dashboard.wantedshifts.employee",
            "common.dashboard.wantedshifts.employeesinqueue",
            "common.dashboard.openshifts.linked",
            "common.dashboard.openshifts.open",
            "common.dashboard.openshifts.unwanted"
        ];

        if (this.isSchedulePlanningMode) {
            keys.push("common.dashboard.wantedshifts.title");
            keys.push("common.shifttype");
        } else if (this.isOrderPlanningMode) {
            keys.push("common.dashboard.wantedassignments.title");
            keys.push("common.ordershifttype");
        }

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;

            if (this.isSchedulePlanningMode) {
                this.widgetTitle = this.terms["common.dashboard.wantedshifts.title"];
                this.shiftTypeLabel = this.terms["common.shifttype"];
            } else if (this.isOrderPlanningMode) {
                this.widgetTitle = this.terms["common.dashboard.wantedassignments.title"];
                this.shiftTypeLabel = this.terms["common.ordershifttype"];
            }
        });
    }

    private loadEmployee(): ng.IPromise<any> {
        return this.coreService.getIdsForEmployeeAndGroup().then((x) => {
            this.employeeId = x.item1;
            this.employeeGroupId = x.item2;
        });
    }

    private setupGrid() {
        this.soeGridOptions.addColumnIcon("", "fal fa-link", this.terms["common.dashboard.openshifts.linked"], "selectShift", null, "showLinkedIcon");
        this.soeGridOptions.addColumnText("dayName", this.terms["common.day"], "10%", false, null, null, "linked-shifts", "isLinked");
        this.soeGridOptions.addColumnText("dateString", this.terms["common.date"], "15%", false, null, null, "linked-shifts", "isLinked");  // class on date column doesn't work
        this.soeGridOptions.addColumnText("time", this.terms["common.time"], "15%", false, null, null, "linked-shifts", "isLinked");
        this.soeGridOptions.addColumnText("shiftTypeName", this.shiftTypeLabel, null, false, null, null, "linked-shifts", "isLinked");
        this.soeGridOptions.addColumnText("employee", this.terms["common.dashboard.wantedshifts.employee"], null, false, null, null, "linked-shifts", "isLinked");
        this.soeGridOptions.addColumnText("openTypeName", this.terms["common.type"], "10%", false, null, null, "linked-shifts", "isLinked");
        this.soeGridOptions.addColumnText("employeesInQueue", this.terms["common.dashboard.wantedshifts.employeesinqueue"], null, false, null, null, "linked-shifts", "isLinked");
        this.soeGridOptions.addColumnEdit(this.terms["core.edit"], "edit");
    }

    protected load() {
        super.load();
        this.coreService.getWantedShiftsWidgetData().then(x => {
            this.wantedShifts = x;
            _.forEach(this.wantedShifts, (row: WantedShiftsGaugeDTO) => {
                switch (row.openType) {
                    case 1:
                        row.openTypeName = this.terms["common.dashboard.openshifts.open"];
                        break;
                    case 2:
                        row.openTypeName = this.terms["common.dashboard.openshifts.unwanted"];
                        break;
                }
            });
            this.soeGridOptions.setData(this.wantedShifts);
            super.loadComplete(this.wantedShifts.length);
        });
    }

    private edit(row: WantedShiftsGaugeDTO) {
        if (!row)
            return;

        if (this.isSchedulePlanningMode) {
            if (!this.editShiftHelper)
                this.editShiftHelper = new EditShiftHelper(this.$uibModal, this.$q, this.coreService, this.sharedScheduleService, this.urlHelperService, this.translationService, true, row.date, row.date, row.employeeId, true, () => this.openEditShiftDialog(row.timeScheduleTemplateBlockId));
            else
                this.openEditShiftDialog(row.timeScheduleTemplateBlockId);
        } else if (this.isOrderPlanningMode) {
            if (!this.editAssignmentHelper)
                this.editAssignmentHelper = new EditAssignmentHelper(this.$uibModal, this.$q, this.coreService, this.sharedScheduleService, this.urlHelperService, this.translationService, true, row.employeeId, true, () => this.openEditAssignmentDialog(row.timeScheduleTemplateBlockId));
            else
                this.openEditAssignmentDialog(row.timeScheduleTemplateBlockId);
        }
    }

    private openEditShiftDialog(timeScheduleTemplateBlockId: number) {
        this.editShiftHelper.loadShift(timeScheduleTemplateBlockId).then(shift => {
            this.editShiftHelper.openEditShiftDialog(shift, null, shift.employeeId, true, false, (result) => {
                if (result.reload && result.reload === true) {
                    this.reload();
                }
            });
        });
    }

    private openEditAssignmentDialog(timeScheduleTemplateBlockId: number) {
        this.editAssignmentHelper.loadShift(timeScheduleTemplateBlockId).then(shift => {
            this.editAssignmentHelper.openEditAssignmentDialog(shift, (result) => {
                if (result.reload && result.reload === true) {
                    this.reload();
                }
            });
        });
    }

    private showLinkedIcon(row: WantedShiftsGaugeDTO): boolean {
        if (row.link) {
            return _.filter(this.wantedShifts, s => s.link === row.link).length > 1;
        }

        return false;
    }

    private selectShift(row: WantedShiftsGaugeDTO) {
        _.forEach(this.wantedShifts, s => {
            s['linked'] = s.link === row.link;
        });
    }

    private isLinked(row: WantedShiftsGaugeDTO): boolean {
        return row['linked'];
    }
}