import {
  ISalesEUDetailDTO,
  ISalesEUDTO,
} from '../../../../shared/models/generated-interfaces/SOECompModelDTOs';
export interface SalesEUGridDTO extends ISalesEUDTO {
  detailsLoaded: boolean;
  detailsRows: ISalesEUDetailDTO[];
}

export class DistributionSalesEuFilterDTO {
  reportPeriod: number = 3; //Month
  startDate?: Date;
  stopDate?: Date;

  accountYear?: number;
  fromInterval?: number;
  toInterval?: number;
}
