import { SoeEntityState } from '@shared/models/generated-interfaces/Enumerations';
import { IPriceListTypeDTO } from '@shared/models/generated-interfaces/PriceListTypeDTOs';
import { DialogData, DialogSize } from '@ui/dialog/models/dialog';
import { Observable } from 'rxjs';

export class PriceListTypeDialogData implements DialogData {
  size?: DialogSize | undefined;
  title: string = '';
  content?: string | undefined;
  primaryText?: string | undefined;
  secondaryText?: string | undefined;
  disableClose?: boolean | undefined;
  disableContentScroll?: boolean | undefined;
  noToolbar?: boolean | undefined;
  hideFooter?: boolean | undefined;
  priceListTypeId!: number;
  callbackAction?:
    | (() => Observable<unknown> | unknown | Promise<unknown>)
    | undefined;
}

export class PricelistTypeDTO implements IPriceListTypeDTO {
  priceListTypeId!: number;
  currencyId!: number;
  name!: string;
  description!: string;
  discountPercent!: number;
  inclusiveVat!: boolean;
  isProjectPriceList!: boolean;
  created?: Date | undefined;
  createdBy!: string;
  modified?: Date | undefined;
  modifiedBy!: string;
  state!: SoeEntityState;
}
