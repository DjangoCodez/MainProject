import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { IScheduleService } from "../../../ScheduleService";
import { IProgressHandler } from "../../../../../Core/Handlers/ProgressHandler";
import { IProgressHandlerFactory } from "../../../../../Core/Handlers/ProgressHandlerFactory";
import { EmployeeListDTO } from "../../../../../Common/Models/EmployeeListDTO";
import { ShiftDTO } from "../../../../../Common/Models/TimeSchedulePlanningDTOs";

export class CreateTemplateBreaksController {

    private progress: IProgressHandler;

    // Terms
    private terms: { [index: string]: string; };
    private info: string;

    // Flags
    private executing: boolean = false;

    // Properties

    private get allItemsSelected(): boolean {
        var selected = true;
        _.forEach(this.employees, employee => {
            if (!employee['selected']) {
                selected = false;
                return false;
            }
        });

        return selected;
    }

    //@ngInject
    constructor(private $uibModalInstance,
        $uibModal,
        private scheduleService: IScheduleService,
        private translationService: ITranslationService,
        progressHandlerFactory: IProgressHandlerFactory,
        private date: Date,
        private employees: EmployeeListDTO[],
        private timeScheduleScenarioHeadId: number) {

        this.progress = progressHandlerFactory.create();
        this.loadTerms();
    }

    private loadTerms() {
        var keys: string[] = [
            "time.schedule.planning.createtemplatebreaks.info"
        ];

        this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
            this.info = this.terms["time.schedule.planning.createtemplatebreaks.info"];
        });
    }

    // EVENTS

    private clearAllItems() {
        _.forEach(this.employees, employee => {
            employee['selected'] = false;
        });
    }

    private selectAllItems() {
        var selected: boolean = this.allItemsSelected;
        _.forEach(this.employees, employee => {
            employee['selected'] = !selected;
        });
    }

    private save() {
        this.executing = true;
        this.progress.startWorkProgress((completion) => {
            this.scheduleService.createBreaksFromTemplatesForEmployees(this.date.beginningOfDay(), this.getSelectedEmployeeIds(), this.timeScheduleScenarioHeadId).then(x => {
                this.executing = false;
                completion.completed(x, true);
                this.close(x);
            });
        });
    }

    private cancel() {
        this.clearAllItems();
        this.$uibModalInstance.dismiss('cancel');
    }

    private close(shifts: ShiftDTO[]) {
        var employeeIds: number[] = this.getSelectedEmployeeIds();
        this.clearAllItems();
        this.$uibModalInstance.close({ success: true, employeeIds: employeeIds, shifts: shifts });
    }

    // HELP-METHODS

    private getSelectedEmployeeIds(): number[] {
        return _.map(_.filter(this.employees, e => e['selected']) || [], e => e.employeeId);
    }
}
