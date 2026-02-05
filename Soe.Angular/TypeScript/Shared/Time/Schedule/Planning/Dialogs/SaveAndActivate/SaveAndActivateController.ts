import { CalendarUtility } from "../../../../../../Util/CalendarUtility";
import { ShiftDTO } from "../../../../../../Common/Models/TimeSchedulePlanningDTOs";
import { IScheduleService as SharedScheduleService } from "../../../ScheduleService";
import { TimeScheduleTemplateChangeDTO, TimeScheduleTemplateHeadDTO } from "../../../../../../Common/Models/TimeScheduleTemplateDTOs";
import { SOEMessageBoxImage } from "../../../../../../Util/Enumerations";
import { EmployeeScheduleDTO } from "../../../../../../Common/Models/EmployeeScheduleDTOs";
import { INotificationService } from "../../../../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../../../../Core/Services/TranslationService";

export class SaveAndActivateController {

    // Terms
    private terms: { [index: string]: string; };

    // Properties
    private template: TimeScheduleTemplateHeadDTO;
    private placement: EmployeeScheduleDTO;
    private dateFrom: Date;
    private dateTo: Date;
    private activateDayNumber: number;

    private shiftsAfterUpdate: string;
    private rows: TimeScheduleTemplateChangeDTO[] = [];
    private selectedRow: TimeScheduleTemplateChangeDTO;
    private allRowsSelected: boolean = false;

    private get nbrOfShiftsToSave(): number {
        return this.shiftsToSave.length;
    }

    // Flags
    private executing: boolean = false;

    //@ngInject
    constructor(
        private $uibModalInstance,
        private $q: ng.IQService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private sharedScheduleService: SharedScheduleService,
        private employeeId: number,
        private date: Date,
        private timeScheduleTemplateHeadId: number,
        private shiftsToSave: ShiftDTO[]) {

        this.dateFrom = date;
        this.activateDayNumber = shiftsToSave.length > 0 ? shiftsToSave[0].dayNumber : 0;

        this.$q.all([
            this.loadTerms(),
            this.checkExistingPlacement()
        ]).then(() => {
            this.loadTemplateChanges();
        });
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        let keys: string[] = [
            "time.schedule.planning.saveandactivate.hasinvaliddaytype",
            "time.schedule.planning.evaluateworkrules.warning",
            "time.schedule.planning.saveandactivate.haswarnings"];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private checkExistingPlacement(): ng.IPromise<any> {
        return this.sharedScheduleService.getTimeScheduleTemplate(this.timeScheduleTemplateHeadId, true, false).then(x => {
            this.template = x;

            if (this.template) {
                if (this.template.employeeSchedules && this.template.employeeSchedules.length > 0) {
                    // Get current placement
                    this.placement = _.find(this.template.employeeSchedules, e => e.startDate.isSameOrBeforeOnDay(this.date) && e.stopDate.isSameOrAfterOnDay(this.date));
                    if (!this.placement) {
                        // No placement for current date, check for future placements
                        this.placement = _.find(this.template.employeeSchedules, e => e.stopDate.isSameOrAfterOnDay(this.date));
                    }
                }

                if (this.placement) {
                    this.dateTo = this.placement.stopDate;
                    this.createStringFromShifts();
                }
            }
        });
    }

    private createStringFromShifts(): ng.IPromise<any> {
        return this.sharedScheduleService.createStringFromShifts(this.shiftsToSave).then(x => {
            this.shiftsAfterUpdate = x;
        });
    }

    private loadTemplateChanges(): ng.IPromise<any> {
        this.executing = true;

        return this.sharedScheduleService.getTimeScheduleTemplateChanges(this.employeeId, this.timeScheduleTemplateHeadId, this.date, this.dateFrom, this.dateTo, this.shiftsToSave).then(x => {
            this.rows = x;

            _.filter(this.rows, r => r.hasInvalidDayType).forEach(r => r.dayTypeToolTip = this.terms["time.schedule.planning.saveandactivate.hasinvaliddaytype"]);
            _.filter(this.rows, r => r.shiftsBeforeUpdate === this.shiftsAfterUpdate).forEach(r => r.hasSameChanges = true);
            _.filter(this.rows, r => r.hasAbsence || r.hasInvalidDayType || r.hasWorkRuleErrors).forEach(r => r.notSelectable = true);
            _.filter(this.rows, r => !r.notSelectable && !r.hasManualChanges && !r.hasWarnings).forEach(r => this.rowSelected(r));

            this.executing = false;
        });
    }

    // EVENTS

    private selectAllRows() {
        this.allRowsSelected = !this.allRowsSelected;

        _.forEach(this.rows, (row: TimeScheduleTemplateChangeDTO) => {
            if (!row.notSelectable)
                row.selected = this.allRowsSelected;
        });
    }

    private rowSelected(row: TimeScheduleTemplateChangeDTO) {
        if (!row.notSelectable)
            row.selected = !row.selected;

        this.allRowsSelected = !_.some(this.rows, r => !r.selected && !r.notSelectable);
    }

    private showWarnings(row: TimeScheduleTemplateChangeDTO) {
        this.notificationService.showDialogEx(this.terms["time.schedule.planning.saveandactivate.haswarnings"], row.warnings, SOEMessageBoxImage.Warning);
    }

    private showWorkRulesResult(row: TimeScheduleTemplateChangeDTO) {
        let msg: string = '';
        _.forEach(row.workRulesResults, result => {
            if (msg.length > 0)
                msg += '\n\n';
            msg += result.errorMessage;
        });

        this.notificationService.showDialogEx(this.terms["time.schedule.planning.evaluateworkrules.warning"], msg, row.hasWorkRuleErrors ? SOEMessageBoxImage.Error : SOEMessageBoxImage.Warning);
    }

    private cancel() {
        this.$uibModalInstance.dismiss('cancel');
    }

    private save() {
        this.$uibModalInstance.close({ noChanges: this.template && !this.placement, activateDayNumber: this.activateDayNumber, activateDates: _.filter(this.rows, r => r.selected).map(r => r.date) });
    }

    // HELP-METHODS

    private get disableLoadTemplateChanges(): boolean {
        if (this.executing || !this.dateFrom || !this.dateTo)
            return true;

        if (this.dateFrom.isAfterOnDay(this.dateTo))
            return true;

        return false;
    }

    private get okToSave(): boolean {
        if (this.nbrOfShiftsToSave === 0)
            return false;

        let noChanges: boolean = !!(this.template && !this.placement);

        if (noChanges || _.some(this.rows, r => r.selected))
            return true;

        return false;
    }
}
