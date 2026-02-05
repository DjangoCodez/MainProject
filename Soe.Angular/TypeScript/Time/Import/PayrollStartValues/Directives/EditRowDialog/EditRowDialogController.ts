import { PayrollStartValueRowDTO } from "../../../../../Common/Models/PayrollImport";
import { ProductSmallDTO } from "../../../../../Common/Models/ProductDTOs";
import { ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";
import { CalendarUtility } from "../../../../../Util/CalendarUtility";
import { SoeEntityState } from "../../../../../Util/CommonEnumerations";

export class EditRowDialogController {

    private row: PayrollStartValueRowDTO;
    private selectedEmployee: ISmallGenericType;
    private selectedProduct: ProductSmallDTO;

    //@ngInject
    constructor(
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private minutesLabel: string,
        private employees: ISmallGenericType[],
        private payrollProducts: ProductSmallDTO[],
        private isNew: boolean,
        row: PayrollStartValueRowDTO) {

        this.row = new PayrollStartValueRowDTO();
        this.row['tmpId'] = row['tmpId'];

        if (this.isNew)
            this.new();
        else {
            this.row.absenceTimeMinutes = row.absenceTimeMinutes;
            this.row.amount = row.amount;
            this.row.date = row.date;
            this.row.doCreateTransaction = false;
            this.row.employeeId = row.employeeId;
            this.row.payrollStartValueRowId = row.payrollStartValueRowId;
            this.row.productId = row.productId;
            this.row.quantity = row.quantity;
            this.row.scheduleTimeMinutes = row.scheduleTimeMinutes;
            this.row.state = row.state;
            this.row.sysPayrollStartValueId = row.sysPayrollStartValueId;

            this.selectedEmployee = this.employees.find(e => e.id === this.row.employeeId);
            this.selectedProduct = this.payrollProducts.find(p => p.productId === this.row.productId);
        }
    }

    private new() {
        // Default values
        this.row.absenceTimeMinutes = 0;
        this.row.amount = 0;
        this.row.date = CalendarUtility.getDateToday();
        this.row.doCreateTransaction = true;
        this.row.quantity = 0;
        this.row.scheduleTimeMinutes = 0;
        this.row.state = SoeEntityState.Active;
    }

    private cancel() {
        this.$uibModalInstance.close();
    }

    private ok() {
        this.row.employeeId = this.selectedEmployee?.id;
        this.row.productId = this.selectedProduct?.productId;

        this.$uibModalInstance.close({ row: this.row });
    }
}
