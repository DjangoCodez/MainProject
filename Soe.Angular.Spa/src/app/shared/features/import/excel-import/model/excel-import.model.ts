import { IExcelImportDTO } from '@shared/models/generated-interfaces/Excel';

export class ExcelImportDTO implements IExcelImportDTO {
  filename: string;
  doNotUpdateWithEmptyValues: boolean = false;
  bytes: number[] = [];

  constructor(
    filename: string,
    doNotUpdateWithEmptyValues: boolean,
    bytes: number[]
  ) {
    this.filename = filename;
    this.doNotUpdateWithEmptyValues = doNotUpdateWithEmptyValues;
    this.bytes = bytes;
  }
}
