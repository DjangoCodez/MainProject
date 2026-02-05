import { ITimePeriodDTO } from "../../Scripts/TypeLite.Net4";
import { CalendarUtility } from "../../Util/CalendarUtility";
import { TimePeriodHeadDTO } from "./TimePeriodHeadDTO";

export class TimePeriodDTO implements ITimePeriodDTO {
    comment: string;
    created: Date;
    createdBy: string;
    extraPeriod: boolean;
    hasRequiredPayrollProperties: boolean;
    isModified: boolean;
    modified: Date;
    modifiedBy: string;
    name: string;
    nameAndPaymentDate: string;
    paymentDate: Date;
    paymentDateString: string;
    payrollStartDate: Date;
    payrollStopDate: Date;
    rowNr: number;
    showAsDefault: boolean;
    sortDate: Date;
    startDate: Date;
    stopDate: Date;
    timePeriodHead: TimePeriodHeadDTO;
    timePeriodHeadId: number;
    timePeriodId: number;

    // Extensions
    public get hasComment(): boolean {
        return !!this.comment;
    }
    public get icon(): string {
        return "fal {0} iconEdit".format(this.hasComment ? "fa-comment-dots" : "fa-comment");
    }

    public fixDates() {
        this.startDate = CalendarUtility.convertToDate(this.startDate);
        this.stopDate = CalendarUtility.convertToDate(this.stopDate);
        this.payrollStartDate = CalendarUtility.convertToDate(this.payrollStartDate);
        this.payrollStopDate = CalendarUtility.convertToDate(this.payrollStopDate);
        this.paymentDate = CalendarUtility.convertToDate(this.paymentDate);
        this.sortDate = CalendarUtility.convertToDate(this.sortDate);
    }

    public setTypes() {
        if (this.timePeriodHead) {
            let obj = new TimePeriodHeadDTO();
            angular.extend(obj, this.timePeriodHead);
            this.timePeriodHead = obj;
        }
    }
}
