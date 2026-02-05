import { SoeOriginType } from '@shared/models/generated-interfaces/Enumerations';
import { ISearchInvoicesPaymentsAndMatchesDTO } from '@shared/models/generated-interfaces/SearchInvoicesPaymentsAndMatchesDTO';

export class ActorInvoiceMatchesFilterDTO
  implements ISearchInvoicesPaymentsAndMatchesDTO
{
  actorId!: number;
  type!: number;
  amountFrom!: number;
  amountTo!: number;
  dateFrom?: Date;
  dateTo?: Date;
  originType!: SoeOriginType;
}
