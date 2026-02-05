import { SoeEntityState } from './generated-interfaces/Enumerations';
import {
  IAccountDimDTO,
  IAccountDTO,
  IAccountInternalDTO,
} from './generated-interfaces/SOECompModelDTOs';

export class AccountDTO implements IAccountDTO {
  accountId: number;
  accountDimId: number;
  accountNr: string;
  parentAccountId?: number;
  name: string;
  description: string;
  externalCode: string;
  hierarchyOnly!: boolean;
  accountTypeSysTermId: number;
  accountDim!: IAccountDimDTO;
  accountDimNr!: number;
  numberName: string;
  dimNameNumberAndName: string;
  amountStop!: number;
  unitStop!: boolean;
  unit: string;
  rowTextStop!: boolean;
  grossProfitCode!: number[];
  attestWorkFlowHeadId?: number;
  state!: SoeEntityState;
  isAccrualAccount!: boolean;
  accountInternals!: IAccountInternalDTO[];
  accountIdWithDelimeter: string;
  isAbstract!: boolean;
  hasVirtualParent!: boolean;
  virtualParentAccountId?: number;
  parentAccounts!: IAccountDTO[];
  parentHierachy!: { [key: number]: string };
  noOParentHierachys!: number;
  hierachyId: string;
  hierachyName: string;
  accountHierarchyUniqueId: string;
  hierarchyNotOnSchedule: boolean;

  constructor() {
    this.accountId = 0;
    this.accountDimId = 0;
    this.accountNr = '';
    this.name = '';
    this.description = '';
    this.externalCode = '';
    this.accountTypeSysTermId = 0;
    this.numberName = '';
    this.dimNameNumberAndName = '';
    this.unit = '';
    this.accountIdWithDelimeter = '';
    this.hierachyId = '';
    this.hierachyName = '';
    this.accountHierarchyUniqueId = '';
    this.hierarchyNotOnSchedule = false;
  }
}
