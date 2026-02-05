import { EmploymentPriceTypePeriodDTO } from "../../../../../Common/Models/EmployeeUserDTO";
import { PayrollGroupPriceTypeDTO } from "../../../../../Common/Models/PayrollGroupDTOs";
import { PayrollLevelDTO } from "../../../../../Common/Models/PayrollLevelDTO";
import { CalendarUtility } from "../../../../../Util/CalendarUtility";

export class EmploymentPriceTypePeriodDialogController {

    private period: EmploymentPriceTypePeriodDTO;
    private payrollGroupAmount: number
    private isNew: boolean;

    //@ngInject
    constructor(private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        period: EmploymentPriceTypePeriodDTO,
        private payrollLevels: PayrollLevelDTO[],
        private payrollPriceTypeId: number,
        private payrollGroupPriceTypes: PayrollGroupPriceTypeDTO[]) {
        this.isNew = !period;

        this.period = new EmploymentPriceTypePeriodDTO();        
        angular.extend(this.period, period);
        if (this.isNew) {
            this.period.fromDate = CalendarUtility.getDateToday();
            this.period.amount = 0;
        } else {
            this.payrollLevelChanged(this.period.payrollLevelId);
        }
    }

    public payrollLevelChanged(payrollLevelId: number) {
        let payrollLevel = _.find(this.payrollLevels, p => p.payrollLevelId === payrollLevelId);
        if (!payrollLevel)
            return;

        this.payrollGroupAmount = 0;

        let pgpt: PayrollGroupPriceTypeDTO; 
        if (payrollLevel.payrollLevelId !== 0) {
            pgpt = _.find(this.payrollGroupPriceTypes, p => p.payrollPriceTypeId === this.payrollPriceTypeId && p.payrollLevelId === payrollLevel.payrollLevelId);
        }
        else {
            pgpt = _.find(this.payrollGroupPriceTypes, p => p.payrollPriceTypeId === this.payrollPriceTypeId && (!p.payrollLevelId || p.payrollLevelId === 0));            
        }

        if (pgpt.periods && pgpt.periods.length > 0) {
            let period = _.orderBy(_.filter(pgpt.periods, p => !p.fromDate || p.fromDate.isSameOrBeforeOnDay(this.period.fromDate)), p => p.fromDate, 'desc')[0];
            if (period)
                this.payrollGroupAmount = period.amount;            
        }

        if (this.isNew)
            this.period.amount = this.payrollGroupAmount;
    }

    public cancel() {
        this.$uibModalInstance.close();
    }

    public ok() {
        this.$uibModalInstance.close({ period: this.period });
    }
}
