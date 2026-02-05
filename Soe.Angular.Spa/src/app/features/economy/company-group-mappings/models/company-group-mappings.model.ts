import { SoeEntityState } from '@shared/models/generated-interfaces/Enumerations';
import {
  ICompanyGroupMappingHeadDTO,
  ICompanyGroupMappingRowDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';

export class CompanyGroupMappings {}

export class CompanyGroupMappingHeadDTO implements ICompanyGroupMappingHeadDTO {
  companyGroupMappingHeadId: number;
  actorCompanyId: number;
  number: number;
  name: string;
  description: string;
  type: number;
  created?: Date;
  createdBy: string;
  modified?: Date;
  modifiedBy: string;
  state: SoeEntityState;

  //Extension
  rows: ICompanyGroupMappingRowDTO[];

  constructor() {
    this.companyGroupMappingHeadId = 0;
    this.actorCompanyId = 0;
    this.number = 0;
    this.name = '';
    this.description = '';
    this.type = 0;
    this.createdBy = '';
    this.modifiedBy = '';
    this.state = SoeEntityState.Active;

    this.rows = [];
  }
}

export class CompanyGroupMappingRowDTO implements ICompanyGroupMappingRowDTO {
  companyGroupMappingRowId: number;
  companyGroupMappingHeadId: number;
  childAccountFrom: number;
  childAccountTo?: number;
  groupCompanyAccount: number;
  created?: Date;
  createdBy!: string;
  modified?: Date;
  modifiedBy!: string;
  state: SoeEntityState;
  rowNr!: number;
  isModified: boolean;
  isDeleted: boolean;
  isProcessed!: boolean;
  childAccountFromName: string;
  childAccountToName: string;
  groupCompanyAccountName: string;

  constructor(
    companyGroupMappingRowId: number,
    companyGroupMappingHeadId: number,
    childAccountFrom: number,
    childAccountTo: number,
    groupCompanyAccount: number,
    childAccountToName: string,
    childAccountFromName: string,
    groupCompanyAccountName: string
  ) {
    this.companyGroupMappingRowId = companyGroupMappingRowId;
    this.companyGroupMappingHeadId = companyGroupMappingHeadId;
    this.childAccountFrom = childAccountFrom;
    (this.childAccountTo = childAccountTo),
      (this.groupCompanyAccount = groupCompanyAccount);
    this.state = SoeEntityState.Active;
    this.isModified = false;
    this.isDeleted = false;
    this.childAccountFromName = childAccountFromName;
    this.childAccountToName = childAccountToName;
    this.groupCompanyAccountName = groupCompanyAccountName;
  }
}
