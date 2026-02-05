import { ITranslationService } from "../../../Core/Services/TranslationService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IScheduleService } from "../../Schedule/ScheduleService";
import { SoeRecalculateTimeHeadAction, TermGroup_RecalculateTimeHeadStatus, TermGroup_RecalculateTimeRecordStatus, TermGroup_TemplateScheduleActivateFunctions } from "../../../Util/CommonEnumerations";
import { SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../Util/Enumerations";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { CoreUtility } from "../../../Util/CoreUtility";
import { RecalculateTimeHeadDTO, RecalculateTimeRecordDTO } from "../../../Common/Models/RecalculateTimeDTOs";
import { ActivateScheduleControlDTO, ActivateScheduleGridDTO } from "../../../Common/Models/EmployeeScheduleDTOs";
import { StringUtility, Guid } from "../../../Util/StringUtility";
import { ActiveScheduleControlDialogController } from "../ActiveScheduleControl/ActiveScheduleControlDialogController";

export class RecalculateTimeStatusDialogController {

    private modalInstance: any;

    // Terms
    private terms: { [index: string]: string; };
    private progressMessage: string;
    private errorMessage: string;

    // Data
    private heads: RecalculateTimeHeadDTO[];
    private currentHead: RecalculateTimeHeadDTO;
    private headCreated: Date;
    private recalculateTimeHeadId: number;

    // Properties
    private showAll: boolean = false;
    private showHistory: boolean = false;
    private dateFrom: Date;
    private dateTo: Date;

    private allHeadsExpanded: boolean = false;
    private expandedHeadIds: number[] = [];

    // Flags
    private loading: boolean = false;
    private validating: boolean = false;
    private activating: boolean = false;
    private overrideLimit: boolean = false;
    private showOverrideLimit: boolean = false;
    private cancelPoll: boolean = false;

    //@ngInject
    constructor(
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private $q: ng.IQService,
        private $timeout: ng.ITimeoutService,
        private $uibModal,
        private $scope,
        private urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private coreService: ICoreService,
        private scheduleService: IScheduleService,
        private activateMode: boolean,
        private items: ActivateScheduleGridDTO[],
        private selectedFunction: TermGroup_TemplateScheduleActivateFunctions,
        private selectedHeadId: number,
        private selectedPeriodId: number,
        private startDate: Date,
        private stopDate: Date,
        private preliminary: boolean) {

        this.modalInstance = $uibModal;

        if (CoreUtility.isSupportAdmin)
            this.showOverrideLimit = true;

        this.$q.all([
            this.loadTerms()
        ]).then(() => {
            if (this.activateMode)
                this.initActivate();
            else
                this.loadData()
        });
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "time.recalculatetimestatus.warnings",
            "time.recalculatetimestatus.errors",
            "time.recalculatetimestatus.cancel.error",
            "time.recalculatetimestatus.activateschedulecontrol",
            "time.recalculatetimestatus.validate.gethead",
            "time.recalculatetimestatus.activating",
            "time.recalculatetimestatus.activated",
            "time.recalculatetimestatus.showhistory.limitreached"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private loadData(keepExpanded: boolean = false): ng.IPromise<any> {
        if (this.showHistory && (!this.dateFrom || !this.dateTo))
            return;

        this.loading = true;

        if (keepExpanded)
            this.expandedHeadIds = _.filter(this.heads, h => h['expanded']).map(h => h.recalculateTimeHeadId);

        // Set limit (number of heads returned)
        let limit: number = 0;
        if (this.showHistory && !this.overrideLimit)
            limit = 20;

        return this.scheduleService.getRecalculateTimeHeads(SoeRecalculateTimeHeadAction.Placement, true, this.showHistory, true, this.dateFrom, this.dateTo, limit).then(x => {
            this.heads = x;

            if (keepExpanded && this.expandedHeadIds.length > 0)
                _.filter(this.heads, h => _.includes(this.expandedHeadIds, h.recalculateTimeHeadId)).forEach(h => h['expanded'] = true);

            this.setAllHeadsExpanded();

            this.loading = false;
        });
    }

    private initActivate() {
        this.validating = true;
        this.progressMessage = this.terms["time.recalculatetimestatus.activateschedulecontrol"];

        this.scheduleService.controlActivations(this.items, this.startDate, this.stopDate).then(control => {
            if (!control.hasWarnings) {
                this.activate(control);
            }
            else {
                var modal = this.modalInstance.open({
                    templateUrl: this.urlHelperService.getGlobalUrl("Time/Dialogs/ActiveScheduleControl/Views/ActiveScheduleControlDialog.html"),
                    controller: ActiveScheduleControlDialogController,
                    controllerAs: 'ctrl',
                    bindToController: true,
                    backdrop: 'static',
                    keyboard: true,
                    size: 'xl',
                    windowClass: 'fullsize-modal',
                    scope: this.$scope,
                    resolve: {
                        control: () => { return control; },
                        activateDate: () => { return this.stopDate; }
                    }
                });

                modal.result.then(val => {
                    control.createResult();
                    this.activate(control);
                }, (reason) => {
                    // Cancelled
                    this.validating = false;
                    this.close();
                });
            }
        });
    }

    private activate(control: ActivateScheduleControlDTO) {
        this.validating = false;
        this.activating = true;
        this.progressMessage = this.terms["time.recalculatetimestatus.validate.gethead"];

        this.coreService.getServerTime().then(x => {
            this.headCreated = x;
            this.getRecalculateTimeHeadId(control.key);

            this.scheduleService.activateSchedule(control, this.items, this.selectedFunction, this.selectedHeadId || 0, this.selectedPeriodId || 0, this.startDate, this.stopDate, this.preliminary).then(result => {
                this.activating = false;
                if (result.success) {
                    if (!this.showAll)
                        this.close(true);
                    else {
                        this.progressMessage = this.terms["time.recalculatetimestatus.activated"];
                        this.getRecalculateTimeHead();
                        this.loadData();
                    }
                } else {
                    this.progressMessage = '';
                    this.errorMessage = result.errorMessage;
                }
            });
        });
    }

    private getRecalculateTimeHeadId(key: Guid) {
        // Poll until id is found, then get whole head
        this.scheduleService.getRecalculateTimeHeadId(key).then(id => {
            if (id) {
                this.recalculateTimeHeadId = id;
                this.getRecalculateTimeHead();
            } else if (!this.cancelPoll) {
                this.$timeout(() => {
                    this.getRecalculateTimeHeadId(key);
                }, 1000);
            }
        });
    }

    private getRecalculateTimeHead() {
        this.scheduleService.getRecalculateTimeHead(this.recalculateTimeHeadId, true, true).then(x => {
            if (!this.currentHead)
                this.progressMessage = StringUtility.ToBr(this.terms["time.recalculatetimestatus.activating"]);

            this.currentHead = x;

            // Poll status changes until activation is done
            if (this.activating && !this.cancelPoll) {
                this.$timeout(() => {
                    this.getRecalculateTimeHead();
                }, 15000);
            }
        });
    }

    private cancelHead(head: RecalculateTimeHeadDTO) {
        const keys: string[] = [
            "time.recalculatetimestatus.cancelhead.ask.title",
            "time.recalculatetimestatus.cancelhead.ask.message"
        ];

        this.translationService.translateMany(keys).then(terms => {
            var modal = this.notificationService.showDialogEx(terms["time.recalculatetimestatus.cancelhead.ask.title"], terms["time.recalculatetimestatus.cancelhead.ask.message"], SOEMessageBoxImage.Question, SOEMessageBoxButtons.OKCancel);
            modal.result.then(val => {
                if (val) {
                    this.scheduleService.cancelRecalculateTimeHead(head.recalculateTimeHeadId).then(result => {
                        if (!result.success)
                            this.notificationService.showDialogEx(this.terms["time.recalculatetimestatus.cancel.error"], result.errorMessage, SOEMessageBoxImage.Error);

                        this.loadData(true);
                    });
                }
            });
        });
    }

    private setHeadToProcessed(head: RecalculateTimeHeadDTO) {
        const keys: string[] = [
            "time.recalculatetimestatus.setheadtoprocessed.ask.title",
            "time.recalculatetimestatus.setheadtoprocessed.ask.message"
        ];

        this.translationService.translateMany(keys).then(terms => {
            var modal = this.notificationService.showDialogEx(terms["time.recalculatetimestatus.setheadtoprocessed.ask.title"], terms["time.recalculatetimestatus.setheadtoprocessed.ask.message"], SOEMessageBoxImage.Question, SOEMessageBoxButtons.OKCancel);
            modal.result.then(val => {
                if (val) {
                    this.scheduleService.setRecalculateTimeHeadToProcessed(head.recalculateTimeHeadId).then(result => {
                        if (!result.success)
                            this.notificationService.showDialogEx(this.terms["time.recalculatetimestatus.cancel.error"], result.errorMessage, SOEMessageBoxImage.Error);

                        this.loadData(true);
                    });
                }
            });
        });
    }

    private cancelRecord(record: RecalculateTimeRecordDTO) {
        const keys: string[] = [
            "time.recalculatetimestatus.cancel",
            "time.recalculatetimestatus.cancel.ask"
        ];

        this.translationService.translateMany(keys).then(terms => {
            var modal = this.notificationService.showDialogEx(terms["time.recalculatetimestatus.cancel"], terms["time.recalculatetimestatus.cancel.ask"], SOEMessageBoxImage.Question, SOEMessageBoxButtons.OKCancel);
            modal.result.then(val => {
                if (val) {
                    this.scheduleService.cancelRecalculateTimeRecord(record.recalculateTimeRecordId).then(result => {
                        if (!result.success)
                            this.notificationService.showDialogEx(this.terms["time.recalculatetimestatus.cancel.error"], result.errorMessage, SOEMessageBoxImage.Error);

                        this.loadData(true);
                    });
                }
            });
        });
    }

    // EVENTS

    private showHistoryChanged() {
        this.$timeout(() => {
            if (this.showHistory) {
                this.dateFrom = CalendarUtility.getDateToday().addDays(-7);
                this.dateTo = CalendarUtility.getDateToday();
            } else {
                this.dateFrom = this.dateTo = null;
            }
        });
    }

    private overrideLimitChanged() {
        this.$timeout(() => {
            this.loadData();
        });
    }

    private expandAllHeads() {
        let expand: boolean = !this.allHeadsExpanded;

        _.filter(this.heads, h => h.records.length > 0).forEach(h => h['expanded'] = expand);
        this.setAllHeadsExpanded();
    }

    private setAllHeadsExpanded() {
        this.allHeadsExpanded = !_.some(this.heads, h => !h['expanded']);
    }

    private headExpanded(head: RecalculateTimeHeadDTO) {
        if (head.records.length > 0) {
            head['expanded'] = !head['expanded'];
            this.setAllHeadsExpanded();
        }
    }

    private showWarningMessage(record: RecalculateTimeRecordDTO) {
        this.notificationService.showDialogEx(this.terms["time.recalculatetimestatus.warnings"], record.warningMsg, SOEMessageBoxImage.Warning);
    }

    private showErrorMessage(record: RecalculateTimeRecordDTO) {
        this.notificationService.showDialogEx(this.terms["time.recalculatetimestatus.errors"], record.errorMsg, SOEMessageBoxImage.Error);
    }

    private toggleShowAll() {
        this.showAll = true;
        this.loadData();
    }

    private close(reload: boolean = false) {
        this.cancelPoll = true;
        this.$uibModalInstance.close({ reload: reload });
    }

    // HELP-METHODS

    private canCancel(head: RecalculateTimeHeadDTO, record: RecalculateTimeRecordDTO): boolean {
        return head && record &&
            record.recalculateTimeRecordStatus == TermGroup_RecalculateTimeRecordStatus.Waiting &&
            (head.status === TermGroup_RecalculateTimeHeadStatus.Processed || head.status === TermGroup_RecalculateTimeHeadStatus.Error);
    }

    private canCancelHead(head: RecalculateTimeHeadDTO): boolean {
        return head && head.status === TermGroup_RecalculateTimeHeadStatus.Started &&
            head.created.addDays(1).isBeforeOnMinute(CalendarUtility.getDateNow()) &&
            head.records.filter(r => r.recalculateTimeRecordStatus === TermGroup_RecalculateTimeRecordStatus.Waiting).length > 0;
    }

    private canSetHeadToProcessed(head: RecalculateTimeHeadDTO): boolean {
        return head && head.status !== TermGroup_RecalculateTimeHeadStatus.Processed &&
            head.records.filter(r => r.recalculateTimeRecordStatus !== TermGroup_RecalculateTimeRecordStatus.Processed && r.recalculateTimeRecordStatus !== TermGroup_RecalculateTimeRecordStatus.Error && r.recalculateTimeRecordStatus !== TermGroup_RecalculateTimeRecordStatus.Cancelled).length === 0;
    }
}
