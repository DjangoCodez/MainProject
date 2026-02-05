import { IFileDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { ISysPriceListImportDTO } from '@shared/models/generated-interfaces/SOESysModelDTOs';
import { AttachedFile } from '@ui/forms/file-upload/file-upload.component';

export class ImportPriceListDTO {
  supplierId!: number;
}

export class ImportPriceListUploadDTO {
  file!: AttachedFile;
  fileName!: string;
  providerId!: number;
}

export class SysPriceListImportDTO implements ISysPriceListImportDTO {
  provider!: number;
  file!: IFileDTO;
}
