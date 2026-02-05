import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { EmployeeListDTO } from "../../../../../Common/Models/EmployeeListDTO";
import { StringUtility } from "../../../../../Util/StringUtility";
import { CalendarUtility } from "../../../../../Util/CalendarUtility";

export class AssignEmployeePostController {

    // Terms
    private terms: any = [];
    private headingRow: string;
    private infoStr: string;
    private warningStr: string;

    private get hasDifferentWorkTime(): boolean {
        return (this.employee.workTimeMinutes !== this.employeePost.workTimeMinutes);
    }

    //@ngInject
    constructor(
        private $uibModalInstance,
        private translationService: ITranslationService,
        private employee: EmployeeListDTO,
        private employeePost: EmployeeListDTO) {

        var keys: string[] = [
            "time.schedule.planning.assignemployeepost.headingrow",
            "time.schedule.planning.assignemployeepost.info",
            "time.schedule.planning.assignemployeepost.differentworktime"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;

            this.headingRow = this.terms["time.schedule.planning.assignemployeepost.headingrow"].format(this.employee.employeeNr, this.employee.name.trim(), this.employeePost.name.trim());
            this.infoStr = StringUtility.ToBr(this.terms["time.schedule.planning.assignemployeepost.info"]);

            if (this.hasDifferentWorkTime)
                this.warningStr = StringUtility.ToBr(this.terms["time.schedule.planning.assignemployeepost.differentworktime"].format(this.employeePost.name, CalendarUtility.minutesToTimeSpan(this.employeePost.workTimeMinutes), this.employee.name, CalendarUtility.minutesToTimeSpan(this.employee.workTimeMinutes)));
        });
    }

    // EVENTS

    private cancel() {
        this.$uibModalInstance.dismiss('cancel');
    }

    private save() {
        this.$uibModalInstance.close({ success: true });
    }
}

