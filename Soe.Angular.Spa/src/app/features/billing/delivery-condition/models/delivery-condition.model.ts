import { IDeliveryConditionDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

export class DeliveryConditionDTO implements IDeliveryConditionDTO {
  deliveryConditionId: number;
  actorCompanyId: number;
  code: string;
  name: string;
  created?: Date;
  createdBy: string;
  modified?: Date;
  modifiedBy: string;

  constructor() {
    this.deliveryConditionId = 0;
    this.actorCompanyId = 0;
    this.code = '';
    this.name = '';
    this.createdBy = '';
    this.modifiedBy = '';
  }
}
