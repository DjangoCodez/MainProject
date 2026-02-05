import { IProductGroupDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

export class ProductGroupDTO implements IProductGroupDTO {
  productGroupId!: number;
  actorCompanyId!: number;
  code!: string;
  name!: string;
}
