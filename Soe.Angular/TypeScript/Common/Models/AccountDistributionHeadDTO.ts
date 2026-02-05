import { IAccountDistributionRowDTO, IAccountDistributionHeadDTO } from "../../Scripts/TypeLite.Net4";
import { TermGroup_AccountDistributionTriggerType, TermGroup_AccountDistributionCalculationType, TermGroup_AccountDistributionPeriodType, SoeEntityState } from "../../Util/CommonEnumerations";

    export class AccountDistributionHeadDTO implements IAccountDistributionHeadDTO {
        private static internalIdCounter = 1;

        constructor() {
        }
        useInPayrollVacationVoucher: boolean;
        useInPayrollVoucher: boolean;
        accountDistributionHeadId: number;
        actorCompanyId: number;
        voucherSeriesTypeId: number;
        type: number;
        name: string;
        description: string;
        triggerType: TermGroup_AccountDistributionTriggerType;
        calculationType: TermGroup_AccountDistributionCalculationType;
        calculate: number;
        periodType: TermGroup_AccountDistributionPeriodType;
        periodValue: number;
        sort: number;
        startDate: Date;
        endDate: Date;
        dayNumber: number;
        amount: number;
        amountOperator: number;
        keepRow: boolean;
        useInVoucher: boolean;
        useInSupplierInvoice: boolean;
        useInCustomerInvoice: boolean;
        useInImport: boolean;
        created: Date;
        createdBy: string;
        modified: Date;
        modifiedBy: string;
        state: SoeEntityState;
        dim1Id: number;
        dim1Expression: string;
        dim2Id: number;
        dim2Expression: string;
        dim3Id: number;
        dim3Expression: string;
        dim4Id: number;
        dim4Expression: string;
        dim5Id: number;
        dim5Expression: string;
        dim6Id: number;
        dim6Expression: string;
        rows: IAccountDistributionRowDTO[];
    }
