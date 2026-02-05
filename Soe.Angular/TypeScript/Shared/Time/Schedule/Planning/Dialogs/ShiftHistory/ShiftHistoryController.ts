import { EditControllerBase } from "../../../../../../Core/Controllers/EditControllerBase";
import { ISoeGridOptions, SoeGridOptions } from "../../../../../../Util/SoeGridOptions";
import { ICoreService } from "../../../../../../Core/Services/CoreService";
import { IScheduleService as ISharedScheduleService } from "../../../ScheduleService";
import { ITranslationService } from "../../../../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../../../Core/Services/NotificationService";
import { IUrlHelperService } from "../../../../../../Core/Services/UrlHelperService";
import { ShiftHistoryDTO } from "../../../../../../Common/Models/TimeSchedulePlanningDTOs";
import { Feature, TermGroup_TimeScheduleTemplateBlockType } from "../../../../../../Util/CommonEnumerations";

export class ShiftHistoryController extends EditControllerBase {

    private history: ShiftHistoryDTO[] = [];
    private soeGridOptions: ISoeGridOptions;
    private $timeout: ng.ITimeoutService;

    //Terms
    private terms: { [index: string]: string; };

    //@ngInject
    constructor(private $uibModalInstance,
        $uibModal,
        coreService: ICoreService,
        $timeout: ng.ITimeoutService,
        $q: ng.IQService,
        private sharedScheduleService: ISharedScheduleService,
        translationService: ITranslationService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        private uiGridConstants: uiGrid.IUiGridConstants,
        urlHelperService: IUrlHelperService,
        private shiftType: TermGroup_TimeScheduleTemplateBlockType,
        private timeScheduleTemplateBlockId: number) {

        super("", Feature.Time_Schedule_SchedulePlanning, $uibModal, translationService, messagingService, coreService, notificationService, urlHelperService);

        this.soeGridOptions = new SoeGridOptions("", this.$timeout, this.uiGridConstants);
        this.soeGridOptions.enableGridMenu = false;
        this.soeGridOptions.enableColumnMenus = false;
        this.soeGridOptions.enableFiltering = false;
        this.soeGridOptions.showGridFooter = true;
        this.soeGridOptions.setMinRowsToShow(12);
        this.setupGrid();

        this.loadHistory();
    }

    protected setupGrid() {
        var keys: string[] = [
            "time.schedule.planning.shifthistory.type",
            "time.schedule.planning.shifthistory.statusfrom",
            "time.schedule.planning.shifthistory.statusto",
            "time.schedule.planning.shifthistory.planningfrom",
            "time.schedule.planning.shifthistory.planningto",
            "time.schedule.planning.shifthistory.employeefrom",
            "time.schedule.planning.shifthistory.employeeto",
            "time.schedule.planning.shifthistory.timefrom",
            "time.schedule.planning.shifthistory.timeto",
            "time.schedule.planning.shifthistory.shifttypefrom",
            "time.schedule.planning.shifthistory.shifttypeto",
            "time.schedule.planning.shifthistory.absensefrom",
            "time.schedule.planning.shifthistory.absenseto",
            "time.schedule.planning.shifthistory.modifiedby",
            "time.schedule.planning.shifthistory.modified",
            "time.schedule.planning.shifthistory.worktypefrom",
            "time.schedule.planning.shifthistory.worktypeto",
            "time.schedule.planning.shifthistory.extrashiftbefore",
            "time.schedule.planning.shifthistory.extrashiftafter"
        ];

        this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
            this.soeGridOptions.addColumnText("typeName", terms["time.schedule.planning.shifthistory.type"], "*");
            this.soeGridOptions.addColumnDate("fromShiftStatus", terms["time.schedule.planning.shifthistory.statusfrom"], "*");
            this.soeGridOptions.addColumnText("toShiftStatus", terms["time.schedule.planning.shifthistory.statusto"], "*");
            this.soeGridOptions.addColumnText("fromShiftUserStatus", terms["time.schedule.planning.shifthistory.planningfrom"], "*");
            this.soeGridOptions.addColumnText("toShiftUserStatus", terms["time.schedule.planning.shifthistory.planningto"], "*");
            this.soeGridOptions.addColumnText("fromEmployeeNrAndName", terms["time.schedule.planning.shifthistory.employeefrom"], "*");
            this.soeGridOptions.addColumnText("toEmployeeNrAndName", terms["time.schedule.planning.shifthistory.employeeto"], "*");
            this.soeGridOptions.addColumnText("fromDateAndTime", terms["time.schedule.planning.shifthistory.timefrom"], "190");
            this.soeGridOptions.addColumnText("toDateAndTime", terms["time.schedule.planning.shifthistory.timeto"], "190");
            if (this.shiftType === TermGroup_TimeScheduleTemplateBlockType.Order) {
                this.soeGridOptions.addColumnText("fromShiftType", terms["time.schedule.planning.shifthistory.worktypefrom"], "*");
                this.soeGridOptions.addColumnText("toShiftType", terms["time.schedule.planning.shifthistory.worktypeto"], "*");
            } else {
                this.soeGridOptions.addColumnText("fromShiftType", terms["time.schedule.planning.shifthistory.shifttypefrom"], "*");
                this.soeGridOptions.addColumnText("toShiftType", terms["time.schedule.planning.shifthistory.shifttypeto"], "*");
            }
            this.soeGridOptions.addColumnText("fromExtraShift", terms["time.schedule.planning.shifthistory.extrashiftbefore"], "*");
            this.soeGridOptions.addColumnText("toExtraShift", terms["time.schedule.planning.shifthistory.extrashiftafter"], "*");
            this.soeGridOptions.addColumnText("fromTimeDeviationCause", terms["time.schedule.planning.shifthistory.absensefrom"], "*");
            this.soeGridOptions.addColumnText("toTimeDeviationCause", terms["time.schedule.planning.shifthistory.absenseto"], "*");
            this.soeGridOptions.addColumnText("createdBy", terms["time.schedule.planning.shifthistory.modifiedby"], "*");
            this.soeGridOptions.addColumnDateTime("created", terms["time.schedule.planning.shifthistory.modified"], "*");

            // Set min width on columns
            var colDefs = this.soeGridOptions.getColumnDefs();
            _.forEach(colDefs, colDef => { 
                if (colDef.field === "typeName" || colDef.field === "created")
                    colDef.minWidth = 140;
                else
                    colDef.minWidth = 100;
            });

            // Append red to cellClass for columns, if StatusChanged
            var colDef1 = colDefs[this.soeGridOptions.getColumnIndex('fromShiftStatus')];
            var cellClass1: string = colDef1.cellClass ? colDef1.cellClass.toString() : "";
            colDef1.cellClass = (grid: any, row, col, rowRenderIndex, colRenderIndex) => {
                return cellClass1 + (row.entity.shiftStatusChanged ? " errorRow" : "");
            };
            var colDef2 = colDefs[this.soeGridOptions.getColumnIndex('toShiftStatus')];
            var cellClass2: string = colDef2.cellClass ? colDef2.cellClass.toString() : "";
            colDef2.cellClass = (grid: any, row, col, rowRenderIndex, colRenderIndex) => {
                return cellClass2 + (row.entity.shiftStatusChanged ? " errorRow" : "");
            };
            var colDef3 = colDefs[this.soeGridOptions.getColumnIndex('fromShiftUserStatus')];
            var cellClass3: string = colDef3.cellClass ? colDef3.cellClass.toString() : "";
            colDef3.cellClass = (grid: any, row, col, rowRenderIndex, colRenderIndex) => {
                return cellClass3 + (row.entity.shiftUserStatusChanged ? " errorRow" : "");
            };
            var colDef4 = colDefs[this.soeGridOptions.getColumnIndex('toShiftUserStatus')];
            var cellClass4: string = colDef4.cellClass ? colDef4.cellClass.toString() : "";
            colDef4.cellClass = (grid: any, row, col, rowRenderIndex, colRenderIndex) => {
                return cellClass4 + (row.entity.shiftUserStatusChanged ? " errorRow" : "");
            };
            var colDef5 = colDefs[this.soeGridOptions.getColumnIndex('fromEmployeeNrAndName')];
            var cellClass5: string = colDef5.cellClass ? colDef5.cellClass.toString() : "";
            colDef5.cellClass = (grid: any, row, col, rowRenderIndex, colRenderIndex) => {
                return cellClass5 + (row.entity.employeeChanged ? " errorRow" : "");
            };
            var colDef6 = colDefs[this.soeGridOptions.getColumnIndex('toEmployeeNrAndName')];
            var cellClass6: string = colDef6.cellClass ? colDef6.cellClass.toString() : "";
            colDef6.cellClass = (grid: any, row, col, rowRenderIndex, colRenderIndex) => {
                return cellClass6 + (row.entity.employeeChanged ? " errorRow" : "");
            };
            var colDef7 = colDefs[this.soeGridOptions.getColumnIndex('fromDateAndTime')];
            var cellClass7: string = colDef7.cellClass ? colDef7.cellClass.toString() : "";
            colDef7.cellClass = (grid: any, row, col, rowRenderIndex, colRenderIndex) => {
                return cellClass7 + (row.entity.dateAndTimeChanged ? " errorRow" : "");
            };
            var colDef8 = colDefs[this.soeGridOptions.getColumnIndex('toDateAndTime')];
            var cellClass8: string = colDef8.cellClass ? colDef8.cellClass.toString() : "";
            colDef8.cellClass = (grid: any, row, col, rowRenderIndex, colRenderIndex) => {
                return cellClass8 + (row.entity.dateAndTimeChanged ? " errorRow" : "");
            };
            var colDef9 = colDefs[this.soeGridOptions.getColumnIndex('fromShiftType')];
            var cellClass9: string = colDef9.cellClass ? colDef9.cellClass.toString() : "";
            colDef9.cellClass = (grid: any, row, col, rowRenderIndex, colRenderIndex) => {
                return cellClass9 + (row.entity.shiftTypeChanged ? " errorRow" : "");
            };
            var colDef10 = colDefs[this.soeGridOptions.getColumnIndex('toShiftType')];
            var cellClass10: string = colDef10.cellClass ? colDef10.cellClass.toString() : "";
            colDef10.cellClass = (grid: any, row, col, rowRenderIndex, colRenderIndex) => {
                return cellClass10 + (row.entity.shiftTypeChanged ? " errorRow" : "");
            };
            var colDef11 = colDefs[this.soeGridOptions.getColumnIndex('fromTimeDeviationCause')];
            var cellClass11: string = colDef11.cellClass ? colDef11.cellClass.toString() : "";
            colDef11.cellClass = (grid: any, row, col, rowRenderIndex, colRenderIndex) => {
                return cellClass11 + (row.entity.timeDeviationCauseChanged ? " errorRow" : "");
            };
            var colDef12 = colDefs[this.soeGridOptions.getColumnIndex('toTimeDeviationCause')];
            var cellClass12: string = colDef12.cellClass ? colDef12.cellClass.toString() : "";
            colDef12.cellClass = (grid: any, row, col, rowRenderIndex, colRenderIndex) => {
                return cellClass12 + (row.entity.timeDeviationCauseChanged ? " errorRow" : "");
            };
            var colDef13 = colDefs[this.soeGridOptions.getColumnIndex('fromExtraShift')];
            var cellClass13: string = colDef13.cellClass ? colDef13.cellClass.toString() : "";
            colDef13.cellClass = (grid: any, row, col, rowRenderIndex, colRenderIndex) => {
                return cellClass13 + (row.entity.extraShiftChanged ? " errorRow" : "");
            };
            var colDef14 = colDefs[this.soeGridOptions.getColumnIndex('toExtraShift')];
            var cellClass14: string = colDef14.cellClass ? colDef14.cellClass.toString() : "";
            colDef14.cellClass = (grid: any, row, col, rowRenderIndex, colRenderIndex) => {
                return cellClass14 + (row.entity.extraShiftChanged ? " errorRow" : "");
            };
        });
    }

    private loadHistory() {
        this.startLoad();
        this.sharedScheduleService.getTimeScheduleTemplateBlockHistory(this.timeScheduleTemplateBlockId).then(x => {
            this.history = x;
            this.soeGridOptions.setData(this.history);
            this.stopProgress();
        });
    }

    private cancel() {
        this.$uibModalInstance.dismiss('cancel');
    }
}
