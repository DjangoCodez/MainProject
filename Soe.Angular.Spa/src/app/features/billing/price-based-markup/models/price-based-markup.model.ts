import { SoeEntityState } from '@shared/models/generated-interfaces/Enumerations';
import { IPriceBasedMarkupDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

export class PriceBasedMarkupDTO implements IPriceBasedMarkupDTO {
  priceBasedMarkupId!: number;
  priceListTypeId?: number;
  priceListName!: string;
  minPrice?: number;
  maxPrice?: number;
  markupPercent!: number;
  created?: Date;
  createdBy!: string;
  modified?: Date;
  modifiedBy!: string;
  state!: SoeEntityState;

  isModified?: boolean;
}
