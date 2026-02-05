import { ISoeBankerRequestFilterDTO } from '@shared/models/generated-interfaces/BankIntegrationDTOs';

export class SoeBankerRequestFilterDTO implements ISoeBankerRequestFilterDTO {
  fromDate?: Date;
  toDate?: Date;
  onlyError?: boolean;
  materialType?: number;
  statusCodes: number[];

  constructor() {
    this.fromDate = new Date().addDays(-30);
    this.toDate = new Date();
    this.onlyError = false;
    this.statusCodes = [11]; //Downloaded;
  }
}
