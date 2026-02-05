import { ICommodityCodeDTO } from '@shared/models/generated-interfaces/CommodityCodeDTO';
import { ICommodityCodeUploadDTO } from '@shared/models/generated-interfaces/CoreModels';

export class CommodityCodeDTO implements ICommodityCodeDTO {
  sysIntrastatCodeId!: number;
  intrastatCodeId?: number;
  code!: string;
  text!: string;
  useOtherQuantity!: boolean;
  startDate?: Date;
  endDate?: Date;
  isActive!: boolean;
}

export class CommodityCodeUploadDTO implements ICommodityCodeUploadDTO {
  year!: number;
  fileString!: string;
  selectedDate!: Date;
  fileName!: string;
}
