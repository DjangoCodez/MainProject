import { IContractGroupExtendedGridDTO, IContractGroupGridDTO, IContractGroupDTO } from "../../Scripts/TypeLite.Net4";
import { AccountDTO } from "./AccountDTO";
import { SoeEntityState, TermGroup_ContractGroupPeriod, TermGroup_ContractGroupPriceManagement } from "../../Util/CommonEnumerations";

export class ContractGroupDTO implements IContractGroupDTO  {
    actorCompanyId: number;
    contractGroupId: number;
    created: Date;
    createdBy: string;
    dayInMonth: number;
    description: string;
    interval: number;
    invoiceTemplate: number;
    invoiceText: string;
    invoiceTextRow: string;
    modified: Date;
    modifiedBy: string;
    name: string;
    orderTemplate: number;
    period: TermGroup_ContractGroupPeriod;
    priceManagement: TermGroup_ContractGroupPriceManagement;
    state: SoeEntityState;
}

export class ContractGroupGridDTO implements IContractGroupGridDTO {
    contractGroupId: number;
    description: string;
    name: string;
}

export class ContractGroupExtendedGridDTO extends ContractGroupGridDTO implements IContractGroupExtendedGridDTO {
    dayInMonth: number;
    interval: number;
    periodId: number;
    periodText: string;
    priceManagementText: string;
}
