import { ISysBankDTO } from '@shared/models/generated-interfaces/SOESysModelDTOs';

// #region ENUMs
export enum SysCompDBType {
  Unknown = 0,
  Production = 1,
  Demo = 2,
  Test = 10,
}

export enum SoeSysEntityState {
  Active = 0,
  Inactive = 1,
  Deleted = 2,
  Temporary = 3,
}

export enum SysCompanySettingType {
  WholesellerCustomerNumber = 1,
  OrganisationNumber = 2,
  SysEdiMessageTypeAndNumber = 3,
  ExternalFtp = 4,
  UserName = 10,
  Password = 11,
  BillingFinvoiceAddress = 12,
}

// #endregion

// #region Interfaces

export interface ISysCompanyDTO {
  sysCompanyId: number;
  sysCompDbId: number;
  companyApiKey: string;
  actorCompanyId?: number;
  name: string;
  number: string;
  dbName: string;
  serverName: string;
  licenseId?: number;
  licenseNumber: string;
  licenseName: string;
  verifiedOrgNr?: string;
  isSOP: boolean;
  usesBankIntegration: boolean;
  modified?: Date;
  modifiedBy?: string;
  sysCompDBDTO: ISysCompDBDTO;
  sysCompanySettingDTOs: ISysCompanySettingDTO[];
  sysCompanyBankAccountDTOs: ISysCompanyBankAccountDTO[];
  state: SoeSysEntityState;
}

interface ISysCompanyBankAccountDTO {
  sysCompanyBankAccountId: number;
  sysBankId: number;
  sysCompanyId: number;
  accountType: number;
  paymentNr: string;
  created?: Date;
  createdBy?: string;
  modified?: Date;
  modifiedBy?: string;
  state: SoeSysEntityState;
}

export interface ISysCompDBDTO {
  sysCompDbId: number;
  sysCompServerId: number;
  name: string;
  description: string;
  apiUrl: string;
  type: SysCompDBType;
  sysCompServerDTO: ISysCompServerDTO;
}

export interface ISysCompServerDTO {
  sysCompServerId: number;
  name: string;
  sysServiceUrl: string;
}

export interface ISysCompanySettingDTO {
  sysCompanySettingId: number;
  sysCompanyId: number;
  settingType: SysCompanySettingType;
  stringValue: string;
  intValue?: number;
  boolValue?: boolean;
  decimalValue?: number;
  childSysCompanySettingDTOs: ISysCompanySettingDTO[];
}

export class SysCompanyUniqueValueDTO {
  public sysCompanyUniqueValueId!: number;
  public sysCompanyId!: number;
  public uniqueValueType!: number;
  public value!: string;
  public created!: Date;
  public createdBy!: string;
  public modified?: Date;
  public modifiedBy?: string;
  public state: SoeSysEntityState = SoeSysEntityState.Active;
}
// #endregion

// #region Classes

export class SysCompanyDTO implements ISysCompanyDTO {
  sysCompanyId: number;
  sysCompDbId: number;
  companyApiKey: string;
  companyGuid?: string;
  actorCompanyId?: number;
  name: string;
  number: string;
  dbName: string;
  serverName: string;
  licenseId?: number;
  licenseNumber: string;
  licenseName: string;
  isSOP: boolean;
  verifiedOrgNr: string;
  usesBankIntegration: boolean;
  modified?: Date;
  modifiedBy?: string;
  sysCompDBDTO!: SysCompDBDTO;
  sysCompanyBankAccountDTOs: SysCompanyBankAccountDTO[] = [];
  sysCompanySettingDTOs: SysCompanySettingDTO[] = [];
  sysCompanyUniqueValueDTOs: SysCompanyUniqueValueDTO[] = [];
  state: SoeSysEntityState = SoeSysEntityState.Active;

  constructor() {
    this.sysCompanyId = 0;
    this.sysCompDbId = 0;
    this.companyApiKey = '';
    this.name = '';
    this.number = '';
    this.dbName = '';
    this.serverName = '';
    this.licenseName = '';
    this.licenseNumber = '';
    this.isSOP = false;
    this.verifiedOrgNr = '';
    this.usesBankIntegration = false;
  }
}

export class SysCompanyBankAccountDTO implements ISysCompanyBankAccountDTO {
  sysCompanyBankAccountId: number;
  sysBankId: number;
  sysCompanyId: number;
  accountType: number;
  paymentNr: string;
  created?: Date;
  createdBy?: string;
  modified?: Date;
  modifiedBy?: string;
  state: SoeSysEntityState;

  constructor() {
    this.sysCompanyBankAccountId = 0;
    this.sysBankId = 0;
    this.sysCompanyId = 0;
    this.accountType = 0;
    this.paymentNr = '';
    this.state = SoeSysEntityState.Active;
  }
}

export class SysCompanySettingDTO implements ISysCompanySettingDTO {
  sysCompanySettingId: number;
  sysCompanyId: number;
  settingType!: SysCompanySettingType;
  stringValue: string;
  intValue?: number;
  boolValue?: boolean;
  decimalValue?: number;
  childSysCompanySettingDTOs: SysCompanySettingDTO[] = [];

  constructor() {
    this.sysCompanySettingId = 0;
    this.sysCompanyId = 0;
    this.stringValue = '';
  }
}

export class SysCompDBDTO implements ISysCompDBDTO {
  sysCompDbId: number;
  sysCompServerId: number;
  name: string;
  description!: string;
  apiUrl!: string;
  type: SysCompDBType = SysCompDBType.Unknown;
  sysCompServerDTO!: SysCompServerDTO;

  constructor() {
    this.sysCompDbId = 0;
    this.sysCompServerId = 0;
    this.name = '';
  }
}

export class SysBankDTO implements ISysBankDTO {
  sysBankId!: number;
  sysCountryId!: number;
  name!: string;
  bic!: string;
  hasIntegration!: boolean;

  //ext
  nameWithBic!: string;
}

export class SysCompServerDTO implements ISysCompServerDTO {
  sysCompServerId: number;
  name: string;
  sysServiceUrl!: string;

  constructor() {
    this.sysCompServerId = 0;
    this.name = '';
  }
}

// #endregion
