import {
  IBatchUpdateDTO,
  INameAndIdDTO,
} from '@shared/models/generated-interfaces/BatchUpdateDTO';
import {
  IPerformBatchUpdateModel,
  IRefreshBatchUpdateOptionsModel,
} from '@shared/models/generated-interfaces/CoreModels';
import {
  BatchUpdateFieldType,
  SoeEntityType,
} from '@shared/models/generated-interfaces/Enumerations';

export class BatchUpdateDTO implements IBatchUpdateDTO {
  field: number;
  label: string;
  dataType: BatchUpdateFieldType;
  doShowFilter: boolean;
  doShowFromDate: boolean;
  doShowToDate: boolean;
  stringValue: string;
  boolValue: boolean;
  intValue: number;
  decimalValue: number;
  dateValue?: Date;
  fromDate?: Date;
  toDate?: Date;
  options: NameAndIdDTO[];
  children: BatchUpdateDTO[];

  //Extensions
  timeValue!: string;
  added!: boolean;

  constructor() {
    this.field = 0;
    this.label = '';
    this.dataType = BatchUpdateFieldType.Unknown;
    this.doShowFilter = false;
    this.doShowFromDate = false;
    this.doShowToDate = false;
    this.stringValue = '';
    this.boolValue = false;
    this.intValue = 0;
    this.decimalValue = 0;
    this.options = [];
    this.children = [];
  }
}

export class NameAndIdDTO implements INameAndIdDTO {
  name!: string;
  id!: number;
}

export class RefreshBatchUpdateOptionsModel
  implements IRefreshBatchUpdateOptionsModel
{
  entityType: SoeEntityType;
  batchUpdate: BatchUpdateDTO;

  constructor(entityType: SoeEntityType, batchUpdate: BatchUpdateDTO) {
    this.entityType = entityType;
    this.batchUpdate = batchUpdate;
  }
}

export class PerformBatchUpdateModel implements IPerformBatchUpdateModel {
  entityType!: SoeEntityType;
  batchUpdates: BatchUpdateDTO[];
  ids: number[];
  filterIds: number[];

  //Extension
  selectedFieldId!: number;

  constructor() {
    this.batchUpdates = [];
    this.ids = [];
    this.filterIds = [];
  }
}
