import { SoeEntityState } from '@shared/models/generated-interfaces/Enumerations';
import { IHouseholdTaxDeductionApplicantDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { DialogData, DialogSize } from '@ui/dialog/models/dialog';
import { Observable } from 'rxjs';

export class HouseholdTaxDeductionApplicantDialogData implements DialogData {
  size?: DialogSize | undefined;
  title: string = '';
  content?: string | undefined;
  rowToUpdate?: HouseholdTaxDeductionApplicantDTO;
  primaryText?: string | undefined;
  secondaryText?: string | undefined;
  disableClose?: boolean | undefined;
  disableContentScroll?: boolean | undefined;
  noToolbar?: boolean | undefined;
  hideFooter?: boolean | undefined;
  callbackAction?:
    | (() => Observable<unknown> | unknown | Promise<unknown>)
    | undefined;
}

// export class HouseholdTaxDeductionDialogData implements DialogData {
//     title!: string;
//     size?: DialogSize;
//     rowToUpdate?: HouseholdTaxDeductionApplicantDTO;
//     newRowAddressItemType?: number;
//     addressRowTypes!: { field1: number, field2: number }[];
//     // addressTypes!: { id: number, name: string }[];
//     // eComTypes!: { id: number, name: string }[];
//     // allowShowSecret!: boolean;
//     // readOnly!: boolean;
//   }

export class HouseholdTaxDeductionApplicantDTO
  implements IHouseholdTaxDeductionApplicantDTO
{
  hidden!: boolean;
  showButton!: boolean;
  householdTaxDeductionApplicantId!: number;
  socialSecNr!: string;
  apartmentNr!: string;
  name!: string;
  property!: string;
  cooperativeOrgNr!: string;
  identifierString!: string;
  share!: number;
  state: SoeEntityState = SoeEntityState.Active;
  customerInvoiceRowId?: number;
  comment!: string;
}
