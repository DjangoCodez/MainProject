import {
  SoeEntityState,
  TermGroup_IOImportHeadType,
  TermGroup_SysImportDefinitionType,
} from '@shared/models/generated-interfaces/Enumerations';
import { IImportSelectionGridRowDTO } from '@shared/models/generated-interfaces/ImportSelectionGridRowDTO';
import { IImportDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

export class SimpleFile {
  fileName!: string;
  fileType?: string;
  fileSize?: number;
  dataStorageId!: number;
  isImported!: boolean;
  isDoingWork!: boolean;
  isDuplicate!: boolean;

  static fromImportSelectionGridRowDTO(
    row: IImportSelectionGridRowDTO
  ): SimpleFile {
    return {
      fileName: row.fileName,
      fileType: row.fileType,
      fileSize: undefined,
      dataStorageId: row.dataStorageId,
      isImported: false,
      isDoingWork: false,
      isDuplicate: false,
    };
  }
}

export class ImportDTO implements IImportDTO {
  importId!: number;
  actorCompanyId!: number;
  importDefinitionId!: number;
  accountYearId?: number;
  voucherSeriesId?: number;
  module!: number;
  name!: string;
  headName!: string;
  state!: SoeEntityState;
  importHeadType!: TermGroup_IOImportHeadType;
  type!: TermGroup_SysImportDefinitionType;
  typeText!: string;
  useAccountDistribution!: boolean;
  useAccountDimensions!: boolean;
  updateExistingInvoice!: boolean;
  dim1AccountId!: number;
  dim2AccountId!: number;
  dim3AccountId!: number;
  dim4AccountId!: number;
  dim5AccountId!: number;
  dim6AccountId!: number;
  guid!: string;
  specialFunctionality!: string;
  isStandard!: boolean;
  isStandardText!: string;
  created?: Date;
  createdBy!: string;
  modified?: Date;
  modifiedBy!: string;

  constructor(init?: Partial<ImportDTO>) {
    Object.assign(this, init);
  }
}
