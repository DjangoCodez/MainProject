import { AttachedFile } from '@ui/forms/file-upload/file-upload.component';
import {
  SoeDataStorageRecordType,
  SoeEntityType,
} from './generated-interfaces/Enumerations';
import { IFilesLookupDTO } from './generated-interfaces/FilesLookupDTO';
import { IImportFileDTO } from './generated-interfaces/ImportFileDTO';
import { IFileRecordDTO } from './generated-interfaces/SOECompModelDTOs';

export class FileRecordDTO implements IFileRecordDTO {
  fileRecordId: number;
  recordId: number;
  entity: SoeEntityType;
  created?: Date;
  createdBy: string;
  modified?: Date;
  modifiedBy: string;
  fileId: number;
  fileName: string;
  description: string;
  extension: string;
  entityTypeName: string;
  fileSize?: number;
  actorCompanyId: number;
  type: SoeDataStorageRecordType;
  identifierId: number;
  identifierNumber: string;
  identifierName: string;
  data: number[];

  // Extensions
  isModified: boolean | undefined;

  constructor() {
    this.fileRecordId = 0;
    this.recordId = 0;
    this.entity = SoeEntityType.None;
    this.createdBy = '';
    this.modifiedBy = '';
    this.fileId = 0;
    this.fileName = '';
    this.description = '';
    this.extension = '';
    this.entityTypeName = '';
    this.actorCompanyId = 0;
    this.type = SoeDataStorageRecordType.Unknown;
    this.identifierId = 0;
    this.identifierNumber = '';
    this.identifierName = '';
    this.data = [];
  }
}

export class FilesLookupDTO implements IFilesLookupDTO {
  entity: SoeEntityType;
  files: ImportFileDTO[];

  constructor(entity: SoeEntityType, files: ImportFileDTO[]) {
    this.entity = entity;
    this.files = files;
  }
}

export class ImportFileDTO implements IImportFileDTO {
  dataStorageId: number;
  fileName: string;
  file: AttachedFile | undefined;

  constructor(
    dataStorageId: number,
    fileName: string,
    attachedFile: AttachedFile
  ) {
    this.dataStorageId = dataStorageId;
    this.fileName = fileName;
    this.file = attachedFile;
  }
}
