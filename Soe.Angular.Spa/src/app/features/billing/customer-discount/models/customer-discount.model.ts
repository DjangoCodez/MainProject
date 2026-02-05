import { SoeEntityState } from '@shared/models/generated-interfaces/Enumerations';
import { IMarkupDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

export class CustomerDiscountMarkupDTO implements IMarkupDTO {
  markupId: number;
  actorCompanyId: number;
  sysWholesellerId: number;
  actorCustomerId?: number;
  code: string;
  productIdFilter: string;
  markupPercent: number;
  discountPercent?: number;
  created?: Date;
  createdBy!: string;
  modified?: Date;
  modifiedBy!: string;
  state: SoeEntityState;
  wholesellerName: string;
  wholesellerDiscountPercent: number;
  categoryId?: number;
  categoryName: string;
  customerName: string;

  isModified?: boolean;

  constructor() {
    this.markupId = 0;
    this.actorCompanyId = 0;
    this.sysWholesellerId = 0;
    this.actorCustomerId = 0;
    this.code = '';
    this.productIdFilter = '';
    this.markupPercent = 0;
    this.discountPercent = undefined;
    this.state = SoeEntityState.Active;
    this.wholesellerName = '';
    this.wholesellerDiscountPercent = 0;
    this.categoryId = 0;
    this.categoryName = '';
    this.customerName = '';
  }
}
