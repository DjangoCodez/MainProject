import { SmallGenericType } from "../../../Common/Models/SmallGenericType";
import { IProgressHandler } from "../../../Core/Handlers/ProgressHandler";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { ITimeHibernatingAbsenceHeadDTO } from "../../../Scripts/TypeLite.Net4";
import { EmployeeService } from "../../../Time/Employee/EmployeeService";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { SoeModule } from "../../../Util/CommonEnumerations";
import { ITimeService } from "../../Time/timeservice";

export class TimeHibernatingAbsenceController {
    private timeHibarnatingAbsence: ITimeHibernatingAbsenceHeadDTO;
    private hibernatingTimeDeviationCauses: SmallGenericType[];
    public progress: IProgressHandler;
    private dateFrom: Date;
    private dateTo: Date;

    private _selectedTimeDeviationCause: number;
    get selectedTimeDeviationCause(): number {
        return this._selectedTimeDeviationCause;
    }
    set selectedTimeDeviationCause(value: number) {
        this._selectedTimeDeviationCause = value;
        this.timeHibarnatingAbsence.timeDeviationCauseId = value;
    }

    //@ngInject
    constructor(
        private $timeout: ng.ITimeoutService,
        private $q: ng.IQService,
        private $uibModalInstance,
        private timeService: ITimeService,
        private progressHandlerFactory: IProgressHandlerFactory,
        private employeeService: EmployeeService,
        private module: SoeModule,
        private employeeId: number,
        private employmentId: number) {
        this.progress = progressHandlerFactory.create()
    }

    public $onInit() {
        this.$q.all([
            this.loadHibernatingTimeDeviationCauses()]).then(() => {
                this.loadData();
            });
    }

    private loadData(): ng.IPromise<any> {
        return this.timeService.getTimeHibernatingAbsence(this.employeeId, this.employmentId).then(x => {
            this.timeHibarnatingAbsence = x;
            this.selectedTimeDeviationCause = this.timeHibarnatingAbsence.timeDeviationCauseId;
            this.dateFrom = CalendarUtility.convertToDate(this.timeHibarnatingAbsence.employment.dateFrom);
            this.dateTo = CalendarUtility.convertToDate(this.timeHibarnatingAbsence.employment.dateTo);
            _.forEach(this.timeHibarnatingAbsence.rows, (r) => {
                r.absenceTimeMinutes = CalendarUtility.minutesToTimeSpan(r.absenceTimeMinutes);
                r.scheduleTimeMinutes = CalendarUtility.minutesToTimeSpan(r.scheduleTimeMinutes); 
            });
            
        });
    }
    private loadHibernatingTimeDeviationCauses(): ng.IPromise<any> {
        this.hibernatingTimeDeviationCauses = [];
        return this.employeeService.getHibernatingTimeDeviationCauses().then((x) => {
            if (x.length > 0)
                this.hibernatingTimeDeviationCauses.push({ id: null, name:'' });
            _.forEach(x, y => {
                this.hibernatingTimeDeviationCauses.push({ id: y.timeDeviationCauseId, name: y.name });
            })
        });
    }
 
    private save() {
        this.progress.startSaveProgress((completion) => {
            _.forEach(this.timeHibarnatingAbsence.rows, (r) => {
                r.absenceTimeMinutes = r.absenceTimeMinutes.length >= 4 ? CalendarUtility.timeSpanToMinutes(r.absenceTimeMinutes) : CalendarUtility.timeSpanToMinutes(CalendarUtility.toFormattedTime(r.absenceTimeMinutes));
                r.scheduleTimeMinutes = r.scheduleTimeMinutes.length >= 4 ? CalendarUtility.timeSpanToMinutes(r.scheduleTimeMinutes) : CalendarUtility.timeSpanToMinutes(CalendarUtility.toFormattedTime(r.scheduleTimeMinutes));
                if (r.scheduleTimeMinutes < r.absenceTimeMinutes)
                    r.absenceTimeMinutes = r.scheduleTimeMinutes;
            });

            this.timeService.saveTimeHibernatingAbsence(this.timeHibarnatingAbsence).then(result => {
                if (result.success) {
                    completion.completed(null, true);
                    this.$uibModalInstance.close({ success: true });
                } else {
                    completion.failed(result.errorMessage);
                    
                }
            }, error => {
                completion.failed(error.message);
                
            }); 
        }, null).then(data => {


        }, error => {
           
        });
       
    }
    private checkValues(i) {
        this.$timeout(() => {
            let absence = this.timeHibarnatingAbsence.rows[i].absenceTimeMinutes;
            let schedule = this.timeHibarnatingAbsence.rows[i].scheduleTimeMinutes;
            absence = absence.length >= 4 ? CalendarUtility.timeSpanToMinutes(absence) : CalendarUtility.timeSpanToMinutes(CalendarUtility.toFormattedTime(absence));
            schedule = schedule.length >= 4 ? CalendarUtility.timeSpanToMinutes(schedule) : CalendarUtility.timeSpanToMinutes(CalendarUtility.toFormattedTime(schedule));

            if (this.timeHibarnatingAbsence.rows[i].absenceTimeMinutes && this.timeHibarnatingAbsence.rows[i].scheduleTimeMinutes && schedule < absence)
                this.timeHibarnatingAbsence.rows[i].absenceTimeMinutes = CalendarUtility.minutesToTimeSpan(schedule);
            
        },200);
    }

    private cancel() {
        this.$uibModalInstance.dismiss('cancel');
    }
}