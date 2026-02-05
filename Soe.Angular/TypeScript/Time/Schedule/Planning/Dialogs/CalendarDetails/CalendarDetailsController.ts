import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { IScheduleService } from "../../../ScheduleService";
import { TimeSchedulePlanningMonthDetailDTO } from "../../../../../Common/Models/TimeSchedulePlanningDTOs";
import { TermGroup_TimeScheduleTemplateBlockType } from "../../../../../Util/CommonEnumerations";

export class CalendarDetailsController {

    private detail: TimeSchedulePlanningMonthDetailDTO;

    private nbrOfOpen: number = 0;
    private nbrOfAssigned: number = 0;
    private nbrOfWanted: number = 0;
    private nbrOfUnwanted: number = 0;
    private nbrOfAbsenceRequested: number = 0;
    private nbrOfAbsenceApproved: number = 0;
    private nbrOfPreliminary: number = 0;

    //@ngInject
    constructor(private $uibModalInstance,
        $uibModal,
        private scheduleService: IScheduleService,
        translationService: ITranslationService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        private date: Date,
        private dayDescription: string,
        private employeeId: number,
        private employeeIds: number[],
        private shiftTypeIds: number[],
        private deviationCauseIds: number[],
        private preliminaryPermission: boolean,
        private calendarViewCountByEmployee: boolean,
        private isOrderPlanningMode: boolean) {

        this.loadDetails();
    }

    private loadDetails() {
        // TODO: Check preliminary parameter
        this.scheduleService.getShiftPeriodDetails(this.date, this.employeeId, [this.isOrderPlanningMode ? TermGroup_TimeScheduleTemplateBlockType.Order : TermGroup_TimeScheduleTemplateBlockType.Schedule], this.employeeIds, this.shiftTypeIds ? this.shiftTypeIds : null, this.deviationCauseIds ? this.deviationCauseIds : null, this.preliminaryPermission, null).then(x => {
            this.detail = x;

            this.nbrOfOpen = this.detail.open ? (this.calendarViewCountByEmployee ? _.uniqBy(this.detail.open, 'employeeId').length : this.detail.open.length) : 0;
            this.nbrOfAssigned = this.detail.assigned ? (this.calendarViewCountByEmployee ? _.uniqBy(this.detail.assigned, 'employeeId').length : this.detail.assigned.length) : 0;
            this.nbrOfWanted = this.detail.wanted ? (this.calendarViewCountByEmployee ? _.uniqBy(this.detail.wanted, 'employeeId').length : this.detail.wanted.length) : 0;
            this.nbrOfUnwanted = this.detail.unwanted ? (this.calendarViewCountByEmployee ? _.uniqBy(this.detail.unwanted, 'employeeId').length : this.detail.unwanted.length) : 0;
            this.nbrOfAbsenceRequested = this.detail.absenceRequested ? (this.calendarViewCountByEmployee ? _.uniqBy(this.detail.absenceRequested, 'employeeId').length : this.detail.absenceRequested.length) : 0;
            this.nbrOfAbsenceApproved = this.detail.absenceApproved ? (this.calendarViewCountByEmployee ? _.uniqBy(this.detail.absenceApproved, 'employeeId').length : this.detail.absenceApproved.length) : 0;
            this.nbrOfPreliminary = this.detail.preliminary ? (this.calendarViewCountByEmployee ? _.uniqBy(this.detail.preliminary, 'employeeId').length : this.detail.preliminary.length) : 0;
        });
    }

    private cancel() {
        this.$uibModalInstance.dismiss('cancel');
    }
}