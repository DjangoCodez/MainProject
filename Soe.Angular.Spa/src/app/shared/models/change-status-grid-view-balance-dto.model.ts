import { IChangeStatusGridViewBalanceDTO } from './generated-interfaces/ChangeStatusGridViewDTO';
import { SoeOriginStatusClassification } from './generated-interfaces/Enumerations';

export class ChangeViewStatusGridViewBalanceDTO
  implements IChangeStatusGridViewBalanceDTO
{
  classification!: SoeOriginStatusClassification;
  count!: number;
  balanceTotal!: number;
  balanceExVat!: number;
}
