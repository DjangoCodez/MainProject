import {
  SoeEntityType,
  TermGroup_DataStorageRecordAttestStatus,
  SoeDataStorageRecordType,
} from './generated-interfaces/Enumerations';
import { IDataStorageRecordDTO } from './generated-interfaces/SOECompModelDTOs';

export class DataStorageRecordDTO implements IDataStorageRecordDTO {
  dataStorageRecordId: number;
  recordId: number;
  entity: SoeEntityType;
  attestStateId?: number;
  currentAttestUsers: string;
  attestStatus: TermGroup_DataStorageRecordAttestStatus;
  data: number[];
  type: SoeDataStorageRecordType;
  attestStateName: string;
  attestStateColor: string;
  roleIds: number[];

  constructor() {
    this.dataStorageRecordId = 0;
    this.recordId = 0;
    this.entity = SoeEntityType.None;
    this.currentAttestUsers = '';
    this.attestStatus = TermGroup_DataStorageRecordAttestStatus.None;
    this.data = [];
    this.type = SoeDataStorageRecordType.Unknown;
    this.attestStateName = '';
    this.attestStateColor = '';
    this.roleIds = [];
  }
}
