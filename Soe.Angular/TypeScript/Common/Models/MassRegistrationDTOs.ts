import { IMassRegistrationGridDTO, IMassRegistrationTemplateHeadDTO, IMassRegistrationTemplateRowDTO } from "../../Scripts/TypeLite.Net4";
import { CalendarUtility } from "../../Util/CalendarUtility";
import { SoeEntityState } from "../../Util/CommonEnumerations";
import { AccountInternalDTO } from "./AccountInternalDTO";

export class MassRegistrationGridDTO implements IMassRegistrationGridDTO {
    isRecurring: boolean;
    massRegistrationTemplateHeadId: number;
    name: string;
    recurringDateTo: Date;
    state: SoeEntityState;

    // Extensions
    public get isActive(): boolean {
        return this.state === SoeEntityState.Active;
    }
    public set isActive(value: boolean) {
        this.state = value ? SoeEntityState.Active : SoeEntityState.Inactive;
    }

    public fixDates() {
        this.recurringDateTo = CalendarUtility.convertToDate(this.recurringDateTo);
    }
}

export class MassRegistrationTemplateHeadDTO implements IMassRegistrationTemplateHeadDTO {
    actorCompanyId: number;
    comment: string;
    created: Date;
    createdBy: string;
    dateFrom: Date;
    dateTo: Date;
    dim1Id: number;
    dim2Id: number;
    dim3Id: number;
    dim4Id: number;
    dim5Id: number;
    dim6Id: number;
    hasCreatedTransactions: boolean;
    inputType: any;
    isRecurring: boolean;
    isSpecifiedUnitPrice: boolean;
    massRegistrationTemplateHeadId: number;
    modified: Date;
    modifiedBy: string;
    name: string;
    payrollProductId: number;
    quantity: number;
    recurringDateTo: Date;
    rows: MassRegistrationTemplateRowDTO[];
    state: SoeEntityState;
    stopOnDateFrom: boolean;
    stopOnDateTo: boolean;
    stopOnIsSpecifiedUnitPrice: boolean;
    stopOnProduct: boolean;
    stopOnQuantity: boolean;
    stopOnUnitPrice: boolean;
    stopOnPaymentDate: boolean;
    paymentDate: Date;
    unitPrice: number;

    // Extensions
    public get isActive(): boolean {
        return this.state === SoeEntityState.Active;
    }
    public set isActive(value: boolean) {
        this.state = value ? SoeEntityState.Active : SoeEntityState.Inactive;
    }

    public fixDates() {
        this.dateFrom = CalendarUtility.convertToDate(this.dateFrom);
        this.dateTo = CalendarUtility.convertToDate(this.dateTo);
        this.recurringDateTo = CalendarUtility.convertToDate(this.recurringDateTo);
        this.paymentDate = CalendarUtility.convertToDate(this.paymentDate);
    }

    public setTypes() {
        if (this.rows) {
            this.rows = this.rows.map(x => {
                let obj = new MassRegistrationTemplateRowDTO();
                angular.extend(obj, x);
                obj.fixDates();
                return obj;
            });
        } else {
            this.rows = [];
        }
    }
}

export class MassRegistrationTemplateRowDTO implements IMassRegistrationTemplateRowDTO {
    created: Date;
    createdBy: string;
    dateFrom: Date;
    dateTo: Date;
    dim1Id: number;
    dim1Name: string;
    dim1Nr: string;
    dim2DimNr: number;
    dim2Id: number;
    dim2Name: string;
    dim2Nr: string;
    dim3DimNr: number;
    dim3Id: number;
    dim3Name: string;
    dim3Nr: string;
    dim4DimNr: number;
    dim4Id: number;
    dim4Name: string;
    dim4Nr: string;
    dim5DimNr: number;
    dim5Id: number;
    dim5Name: string;
    dim5Nr: string;
    dim6DimNr: number;
    dim6Id: number;
    dim6Name: string;
    dim6Nr: string;
    employeeId: number;
    employeeName: string;
    employeeNr: string;
    employeeNrSort: string;
    errorMessage: string;
    isSpecifiedUnitPrice: boolean;
    massRegistrationTemplateHeadId: number;
    massRegistrationTemplateRowId: number;
    modified: Date;
    modifiedBy: string;
    paymentDate: Date;
    productId: number;
    productName: string;
    productNr: string;
    quantity: number;
    state: SoeEntityState;
    unitPrice: number;
    warnings: string;

    // Extensions
    public get employeeNrAndName(): string {
        return "({0}) {1}".format(this.employeeNr, this.employeeName);
    }

    public get paymentDateFormatted(): string {
        return this.paymentDate ? this.paymentDate.toFormattedDate() : '';
    }
    public set paymentDateFormatted(value: string) {
        this.paymentDate = CalendarUtility.convertToDate(value);
    }

    public fixDates() {
        this.dateFrom = CalendarUtility.convertToDate(this.dateFrom);
        this.dateTo = CalendarUtility.convertToDate(this.dateTo);
        this.paymentDate = CalendarUtility.convertToDate(this.paymentDate);
    }

    public setTypes() {
        this.employeeNr = this.employeeNr || '';
        this.employeeName = this.employeeName || '';
        this.productNr = this.productNr || '';
        this.productName = this.productName || '';
        this.dim1Name = this.dim1Name || '';
        this.dim2Name = this.dim2Name || '';
        this.dim3Name = this.dim3Name || '';
        this.dim4Name = this.dim4Name || '';
        this.dim5Name = this.dim5Name || '';
        this.dim6Name = this.dim6Name || '';
    }
}
