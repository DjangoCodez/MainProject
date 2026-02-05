import { SoeEntityState } from '@shared/models/generated-interfaces/Enumerations';
import {
  IAccountDTO,
  IAccountDimDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';

export class AccountDimDTO implements IAccountDimDTO {
  allowAccountsWithoutParent: boolean;
  accountDimId: number;
  actorCompanyId: number;
  sysAccountStdTypeParentId?: number;
  parentAccountDimId?: number;
  sysSieDimNr?: number;
  accountDimNr: number;
  name: string;
  shortName: string;
  minChar?: number;
  maxChar?: number;
  linkedToProject: boolean;
  linkedToShiftType: boolean;
  parentAccountDimName: string;
  created?: Date;
  createdBy: string;
  modified?: Date;
  modifiedBy: string;
  onlyAllowAccountsWithParent: boolean;
  state: SoeEntityState;
  useInSchedulePlanning: boolean;
  excludeinAccountingExport: boolean;
  excludeinSalaryReport: boolean;
  useVatDeduction: boolean;
  mandatoryInOrder: boolean;
  mandatoryInCustomerInvoice: boolean;
  accounts: IAccountDTO[];
  isStandard: boolean;
  isInternal: boolean;
  level: number;
  isSelected: boolean;

  get isActive(): boolean {
    return this.state === SoeEntityState.Active;
  }

  set isActive(active: boolean) {
    this.state = active ? SoeEntityState.Active : SoeEntityState.Inactive;
  }

  constructor() {
    this.accountDimId = 0;
    this.actorCompanyId = 0;
    this.accountDimNr = 0;
    this.name = '';
    this.shortName = '';
    this.linkedToProject = false;
    this.linkedToShiftType = false;
    this.parentAccountDimName = '';
    this.createdBy = '';
    this.modifiedBy = '';
    this.state = SoeEntityState.Active;
    this.useInSchedulePlanning = false;
    this.excludeinAccountingExport = false;
    this.excludeinSalaryReport = false;
    this.useVatDeduction = false;
    this.mandatoryInOrder = false;
    this.mandatoryInCustomerInvoice = false;
    this.accounts = [];
    this.isStandard = false;
    this.isInternal = false;
    this.level = 0;
    this.isSelected = false;
    this.isActive = this.state === SoeEntityState.Active;
    this.onlyAllowAccountsWithParent = false;
    this.allowAccountsWithoutParent = false;
  }
}
