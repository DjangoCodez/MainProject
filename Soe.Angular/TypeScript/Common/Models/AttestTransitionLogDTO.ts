import { IAttestTransitionLogDTO } from "../../Scripts/TypeLite.Net4";
import { CalendarUtility } from "../../Util/CalendarUtility";

export class AttestTransitionLogDTO implements IAttestTransitionLogDTO {
    attestStateFromName: string;
    attestStateToName: string;
    attestTransitionCreatedBySupport: boolean;
    attestTransitionDate: Date;
    attestTransitionLogId: number;
    attestTransitionUserId: number;
    attestTransitionUserName: string;
    timePayrollTransactionId: number;

    public fixDates() {
        this.attestTransitionDate = CalendarUtility.convertToDate(this.attestTransitionDate);
    }
}

