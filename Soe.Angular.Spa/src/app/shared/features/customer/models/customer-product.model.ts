import { ICustomerProductPriceSmallDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

export class CustomerProductPriceSmallDTO {
  productRowId: number;
  customerProductId!: number;
  productId!: number;
  number!: string;
  name!: string;
  price!: number;
  isDelete: boolean;
  numberName: string = '';

  constructor(productRowId: number = 0) {
    this.productRowId = productRowId;
    this.isDelete = false;
  }

  static ToCustomerProductPriceSmallDTO(
    customerProductSmallDTOs: ICustomerProductPriceSmallDTO[]
  ): CustomerProductPriceSmallDTO[] {
    return customerProductSmallDTOs.map(customerProductSmallDTO => {
      const customerProductPriceSmallDTO = new CustomerProductPriceSmallDTO();
      customerProductPriceSmallDTO.customerProductId =
        customerProductSmallDTO.customerProductId;
      customerProductPriceSmallDTO.productId =
        customerProductSmallDTO.productId;
      customerProductPriceSmallDTO.number = customerProductSmallDTO.number;
      customerProductPriceSmallDTO.name = customerProductSmallDTO.name;
      customerProductPriceSmallDTO.price = customerProductSmallDTO.price;
      return customerProductPriceSmallDTO;
    });
  }
}
