import { ICardNumberGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

export class CardNumberGridDTO implements ICardNumberGridDTO {
  employeeId!: number;
  cardNumber!: string;
  employeeNumber!: string;
  employeeName!: string;
  employeeNrSort!: string;
}
