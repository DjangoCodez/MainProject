import { ICoreService } from "../../../Core/Services/CoreService";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { TypeAheadOptionsAg } from "../../../Util/SoeGridOptionsAg";

export class EditTimeHelper {
    protected employeeDaysWithSchedule: any[] = [];
    //@ngInject
    constructor(private coreService: ICoreService, private $q: ng.IQService, private getEmployee: ((employeeId: number) => any)) {

    }

    public loadEmployeeTimesAndSchedule(employeeId: number, date: Date ): ng.IPromise<number> {
        const deferral = this.$q.defer<number>();

        //Load previous times and schedule
        const dayObject = this.employeeDaysWithSchedule ? this.employeeDaysWithSchedule.filter(e => e.employeeId === employeeId && e.date.toDateString() === date.toDateString()) : null;
        if (dayObject && dayObject.length > 0) {
            this.employeeScheduleInfoChanged(dayObject[0]);
            //this.filterTimeDeviationCauses(dayObject[0].employeeGroupId);
            deferral.resolve(dayObject[0].employeeGroupId);
        }
        else {
            this.getEmployeeScheduleAndTransactionInfo(employeeId, date).then((employeeGroupId:number) => {
                deferral.resolve(employeeGroupId);
            });
        }

        return deferral.promise;
    }

    private getEmployeeScheduleAndTransactionInfo(employeeId: number, date: Date, openDialog: boolean = false): ng.IPromise<number> {
        const deferral = this.$q.defer<number>();

        if (!this.employeeDaysWithSchedule)
            this.employeeDaysWithSchedule = [];

        this.coreService.getEmployeeScheduleAndTransactionInfo(employeeId, date).then(result => {
            //this.setRowErrorMessage("");
            if (result) {
                deferral.resolve(result.employeeGroupId);
                this.employeeScheduleInfoChanged(result);
                //this.filterTimeDeviationCauses(result.employeeGroupId);
                if (_.filter(this.employeeDaysWithSchedule, e => e.employeeId === employeeId && e.date === date).length === 0) {
                    result.date = CalendarUtility.convertToDate(result.date);
                    this.employeeDaysWithSchedule.push(result);

                    //if (openDialog)
                    //((    this.showInfoDialog(result);
                }
            }
            else {
                deferral.resolve(0);
            }
        });

        return deferral.promise;
    }

    private employeeScheduleInfoChanged(data: any) {
        //this.selectedRow.autoGenTimeAndBreakForProject = data.autoGenTimeAndBreakForProject;
        if (data.employeeGroupId) {
            const employee = this.getEmployee(data.employeeId);
            if (employee && !employee.timeDeviationCauseId && data.timeDeviationCauseId) {
                employee.timeDeviationCauseId = data.timeDeviationCauseId;
            }

            //this.setShowStartStop();
        }
        else {
            //this.setRowErrorMessage("billing.project.timesheets.employeegroupmissing");
            //this.selectedRow.timeDeviationCauseId = 0;
        }
    }


    public createTypeAheadOptions(fieldName = "label"): TypeAheadOptionsAg {
        const typeAheadOptions = new TypeAheadOptionsAg;
        typeAheadOptions.displayField = fieldName;
        typeAheadOptions.dataField = fieldName;
        typeAheadOptions.minLength = 0;
        typeAheadOptions.delay = 0;
        typeAheadOptions.useScroll = true;
        return typeAheadOptions;
    }
}