import { IShiftRequestStatusDTO, IShiftRequestStatusRecipientDTO } from "../../../../../Scripts/TypeLite.Net4";
import { IScheduleService as ISharedScheduleService } from "../../../../../Shared/Time/Schedule/ScheduleService";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { XEMailAnswerType } from "../../../../../Util/CommonEnumerations";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../../../Util/Enumerations";
import { ModalUtility } from "../../../../../Util/ModalUtility";
import { CalendarUtility } from "../../../../../Util/CalendarUtility";

export class ShiftRequestStatusController {

    private terms: any;
    private status: IShiftRequestStatusDTO;
    private shiftIds: number[];

    //@ngInject
    constructor(private $uibModalInstance,
        $uibModal,
        private $q: ng.IQService,
        private notificationService: INotificationService,
        private coreService: ICoreService,
        private sharedScheduleService: ISharedScheduleService,
        private translationService: ITranslationService,
        private shiftId: number,
        private modifyPermission: boolean) {

        this.shiftIds = [this.shiftId];

        this.loadTerms().then(() => {
            this.load(true);
        });
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "core.yes",
            "core.no",
            "common.modified",
            "common.by",
            "time.schedule.planning.shiftrequeststatus.removerecipient",
            "time.schedule.planning.shiftrequeststatus.removerecipient.message",
            "time.schedule.planning.shiftrequeststatus.undorequest",
            "time.schedule.planning.shiftrequeststatus.undorequest.message",
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private load(loadLinked: boolean) {
        if (loadLinked) {
            this.loadLinkedShiftIds().then(shiftIds => {
                this.shiftIds = shiftIds;
                this.loadStatus();
            });
        } else {
            this.loadStatus();
        }
    }

    private loadLinkedShiftIds(): ng.IPromise<number[]> {
        var deferral = this.$q.defer<number[]>();

        this.coreService.getLinkedShifts(this.shiftId).then(x => {
            deferral.resolve(x.map(s => s.timeScheduleTemplateBlockId));
        });

        return deferral.promise;
    }

    private loadStatus() {
        if (this.shiftIds.length === 0)
            return;

        this.shiftId = this.shiftIds.shift();

        this.sharedScheduleService.getShiftRequestStatus(this.shiftId).then(x => {
            this.status = x;
            if (this.status && this.status.recipients) {
                this.setStatusInfo();
            } else {
                this.load(false);
            }
        });
    }

    private setStatusInfo() {
        _.forEach(this.status.recipients, recipient => {
            if (recipient.answerType === XEMailAnswerType.Yes)
                recipient['answerTypeText'] = this.terms["core.yes"];
            else if (recipient.answerType === XEMailAnswerType.No)
                recipient['answerTypeText'] = this.terms["core.no"];

            if (recipient.modified)
                recipient['modifiedInfo'] = '{0} {1} {2} {3}'.format(this.terms["common.modified"], CalendarUtility.convertToDate(recipient.modified).toFormattedDateTime(), this.terms["common.by"], recipient.modifiedBy);
        });
    }

    // ACTIONS

    private removeRecipientFromShiftRequest(recipient: IShiftRequestStatusRecipientDTO) {
        var modal = this.notificationService.showDialogEx(this.terms["time.schedule.planning.shiftrequeststatus.removerecipient"], this.terms["time.schedule.planning.shiftrequeststatus.removerecipient.message"].format(recipient.employeeName), SOEMessageBoxImage.Question, SOEMessageBoxButtons.OKCancel);
        modal.result.then(val => {
            if (val) {
                this.sharedScheduleService.removeRecipientFromShiftRequest(this.shiftId, recipient.userId).then(result => {
                    this.$uibModalInstance.close({ reload: true });
                });
            };
        });
    }

    private undoRequest() {
        var modal = this.notificationService.showDialogEx(this.terms["time.schedule.planning.shiftrequeststatus.undorequest"], this.terms["time.schedule.planning.shiftrequeststatus.undorequest.message"], SOEMessageBoxImage.Question, SOEMessageBoxButtons.OKCancel);
        modal.result.then(val => {
            if (val) {
                this.sharedScheduleService.undoShiftRequest(this.shiftId).then(result => {
                    this.$uibModalInstance.close({ reload: true });
                });
            };
        });
    }

    private cancel() {
        this.$uibModalInstance.dismiss(ModalUtility.MODAL_CANCEL);
    }

    // HELP METHODS

    private get showUndoRequest(): boolean {
        let hasPositiveAnswer: boolean = this.status && _.filter(this.status.recipients, r => r.answerType === XEMailAnswerType.Yes).length > 0;
        return this.modifyPermission && this.status && !hasPositiveAnswer;
    }
}
