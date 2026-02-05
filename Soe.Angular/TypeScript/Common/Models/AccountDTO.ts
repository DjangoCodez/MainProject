import { IAccountEditDTO, IAccountInternalDTO, IAccountMappingDTO, IAccountDTO, IAccountVatRateViewSmallDTO, IAccountSmallDTO, IAccountDimDTO } from "../../Scripts/TypeLite.Net4";
import { AccountInternalDTO } from "./AccountInternalDTO";
import { SoeEntityState } from "../../Util/CommonEnumerations";

export class AccountDTO implements IAccountDTO {
    accountDim: IAccountDimDTO;
    accountDimId: number;
    accountDimNr: number;
    accountId: number;
    accountInternals: AccountInternalDTO[];
    accountNr: string;
    accountTypeSysTermId: number;
    amountStop: number;
    attestWorkFlowHeadId: number;
    description: string;
    externalCode: string;
    grossProfitCode: number[];
    hasVirtualParent: boolean;
    hierachyId: string;
    hierachyName: string;
    hierarchyOnly: boolean;
    isAbstract: boolean;
    isAccrualAccount: boolean;
    name: string;
    noOParentHierachys: number;
    numberName: string;
    parentAccountId: number;
    rowTextStop: boolean;
    state: SoeEntityState;
    unit: string;
    unitStop: boolean;
    virtualParentAccountId: number;

    public get accountNrSort(): string {
        return this.accountNr.padLeft(50, '0');
    }
}

export class AccountSmallDTO implements IAccountSmallDTO {
    accountDimId: number;
    accountId: number;
    description: string;
    name: string;
    number: string;
    parentAccountId: number;
    percent: number;

    public get numberAndName(): string {
        return '{0} {1}'.format(this.number, this.name);
    }
}

export class AccountEditDTO implements IAccountEditDTO {
    accountDimId: number;
    accountHierachyPayrollExportExternalCode: string;
    accountHierachyPayrollExportUnitExternalCode: string;
    accountId: number;
    accountInternals: IAccountInternalDTO[];
    accountMappings: IAccountMappingDTO[];
    accountNr: string;
    accountTypeSysTermId: number;
    active: boolean;
    amountStop: number;
    attestWorkFlowHeadId: number;
    created: Date;
    createdBy: string;
    description: string;
    excludeVatVerification: boolean;
    externalCode: string;
    hierarchyOnly: boolean;
    isAccrualAccount: boolean;
    isStdAccount: boolean;
    modified: Date;
    modifiedBy: string;
    name: string;
    parentAccountId: number;
    rowTextStop: boolean;
    sieKpTyp: string;
    state: SoeEntityState;
    sysAccountSruCode1Id: number;
    sysAccountSruCode2Id: number;
    sysVatAccountId: number;
    unit: string;
    unitStop: boolean;
    useVatDeduction: boolean;
    useVatDeductionDim: boolean;
    vatDeduction: number;
}

export class AccountVatRateViewSmallDTO implements IAccountVatRateViewSmallDTO {
    accountId: number;
    accountNr: string;
    name: string;
    vatRate: number;

    // Extensions
    get numberName() {
        return this.accountNr + ' ' + this.name;
    }
}
