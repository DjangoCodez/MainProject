import { IImportFileDTO } from "../../Scripts/TypeLite.Net4";

export class ImportFileDTO implements IImportFileDTO {
  constructor(
    _dataStorageId: number,
    _fileName: string
  ) { 
    this.dataStorageId = _dataStorageId;
    this.fileName = _fileName;

  }
  dataStorageId: number;
  fileName: string;
}