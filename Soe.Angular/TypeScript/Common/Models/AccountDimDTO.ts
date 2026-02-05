import { IAccountDimDTO, IAccountDimSmallDTO } from "../../Scripts/TypeLite.Net4";
import { AccountDTO } from "./AccountDTO";
import { SoeEntityState } from "../../Util/CommonEnumerations";

export class AccountDimDTO implements IAccountDimDTO {
    accountDimId: number;
    accountDimNr: number;
    accounts: AccountDTO[];
    actorCompanyId: number;
    created: Date;
    createdBy: string;
    excludeinAccountingExport: boolean;
    excludeinSalaryReport: boolean;
    isInternal: boolean;
    isStandard: boolean;
    level: number;
    linkedToProject: boolean;
    linkedToShiftType: boolean;
    mandatoryInCustomerInvoice: boolean;
    mandatoryInOrder: boolean;
    maxChar: number;
    minChar: number;
    modified: Date;
    modifiedBy: string;
    name: string;
    parentAccountDimId: number;
    parentAccountDimName: string;
    shortName: string;
    state: SoeEntityState;
    sysAccountStdTypeParentId: number;
    sysSieDimNr: number;
    useInSchedulePlanning: boolean;
    useVatDeduction: boolean;

    get isActive(): boolean {
        return (this.state === SoeEntityState.Active);
    }
    set isActive(active: boolean) {
        this.state = (active ? SoeEntityState.Active : SoeEntityState.Inactive);
    }
}

export class AccountDimSmallDTO implements IAccountDimSmallDTO {
    accountDimId: number;
    accountDimNr: number;
    accounts: AccountDTO[];
    linkedToShiftType: boolean;
    name: string;
    parentAccountDimId: number;
    level: number;
    mandatoryInCustomerInvoice: boolean;
    mandatoryInOrder: boolean;
    isAboveCompanyStdSetting: boolean;

    // Extensions
    filteredAccounts: AccountDTO[];
    selectedAccounts: any[];
    groupByIndex: number;
}
