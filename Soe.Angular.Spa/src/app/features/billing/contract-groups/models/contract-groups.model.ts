import {
  TermGroup_ContractGroupPeriod,
  TermGroup_ContractGroupPriceManagement,
  SoeEntityState,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  IContractGroupDTO,
  IContractGroupExtendedGridDTO,
  IContractGroupGridDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';

export class ContractGroupDTO implements IContractGroupDTO {
  contractGroupId!: number;
  actorCompanyId!: number;
  name!: string;
  description!: string;
  period!: TermGroup_ContractGroupPeriod;
  interval!: number;
  dayInMonth!: number;
  priceManagement!: TermGroup_ContractGroupPriceManagement;
  invoiceText!: string;
  invoiceTextRow!: string;
  orderTemplate?: number;
  invoiceTemplate?: number;
  created?: Date;
  createdBy!: string;
  modified?: Date;
  modifiedBy!: string;
  state: SoeEntityState = SoeEntityState.Active;
}

export class ContractGroupExtendedGridDTO
  implements IContractGroupGridDTO, IContractGroupExtendedGridDTO
{
  periodId!: number;
  periodText!: string;
  interval!: number;
  priceManagementText!: string;
  dayInMonth!: number;
  contractGroupId!: number;
  name!: string;
  description!: string;
}
