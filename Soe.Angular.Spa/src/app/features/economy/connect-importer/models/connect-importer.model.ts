import { IImportIOModel } from '@shared/models/generated-interfaces/CoreModels';
import {
  TermGroup_IOImportHeadType,
  TermGroup_IOSource,
  TermGroup_IOStatus,
  TermGroup_IOType,
} from '@shared/models/generated-interfaces/Enumerations';
import { IImportBatchDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

export class ConnectImporterGridFilterDTO {
  iOImportHeadType!: number;
  dateSelectionId!: number;
}

export class ImportBatchDTO implements IImportBatchDTO {
  recordId!: number;
  type!: TermGroup_IOType;
  typeName!: string;
  source!: TermGroup_IOSource;
  sourceName!: string;
  importHeadType!: TermGroup_IOImportHeadType;
  importHeadTypeName!: string;
  status!: TermGroup_IOStatus[];
  statusName!: string[];
  statusNameStr!: string;
  statusNameId!: number;
  batchId!: string;
  created?: Date;
}

export class ImportIOModel implements IImportIOModel {
  importHeadType!: TermGroup_IOImportHeadType;
  ioIds!: number[];
  useAccountDistribution!: boolean;
  useAccoungDims!: boolean;
  defaultDim1AccountId!: number;
  defaultDim2AccountId!: number;
  defaultDim3AccountId!: number;
  defaultDim4AccountId!: number;
  defaultDim5AccountId!: number;
  defaultDim6AccountId!: number;

  constructor(
    importHeadType: TermGroup_IOImportHeadType,
    ioIds: number[],
    useAccountDistribution: boolean,
    useAccoungDims: boolean,
    defaultDim2AccountId: number,
    defaultDim3AccountId: number,
    defaultDim4AccountId: number,
    defaultDim5AccountId: number,
    defaultDim6AccountId: number
  ) {
    this.importHeadType = importHeadType;
    this.ioIds = ioIds;
    this.useAccountDistribution = useAccountDistribution;
    this.useAccoungDims = useAccoungDims;
    this.defaultDim2AccountId = defaultDim2AccountId;
    this.defaultDim3AccountId = defaultDim3AccountId;
    this.defaultDim4AccountId = defaultDim4AccountId;
    this.defaultDim5AccountId = defaultDim5AccountId;
    this.defaultDim6AccountId = defaultDim6AccountId;
  }
}
