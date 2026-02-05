import { IDeliveryTypeDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

export class DeliveryTypeDTO implements IDeliveryTypeDTO {
  deliveryTypeId!: number;
  actorCompanyId!: number;
  code!: string;
  name!: string;
  created?: Date;
  createdBy!: string;
  modified?: Date;
  modifiedBy!: string;
}
