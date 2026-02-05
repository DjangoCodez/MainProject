import { ExternalCompanySearchProvider } from '@shared/models/generated-interfaces/Enumerations';
import { IExternalCompanyAddressDTO } from '@shared/models/generated-interfaces/ExternalCompanyResultDTO';
import { IExternalCompanyFilterDTO } from '@shared/models/generated-interfaces/ExternalCompanyFilterDTO';
import { IExternalCompanyResultDTO } from '@shared/models/generated-interfaces/ExternalCompanyResultDTO';
import { DialogData } from '@ui/dialog/models/dialog';

export class ExternalCompanySearchFilter implements IExternalCompanyFilterDTO {
  registrationNr!: string;
  name!: string;

  constructor(data?: IExternalCompanyFilterDTO) {
    if (data) {
      this.registrationNr = data.registrationNr;
      this.name = data.name;
    }
  }

  isEmpty(): boolean {
    return !this.registrationNr && !this.name;
  }
}
export interface ExternalCompanySearchDialogData extends DialogData {
  searchProvider: ExternalCompanySearchProvider;
  searchFilter?: ExternalCompanySearchFilter;
  source: string;
  result?: ExternalCompanyGridRow;
}

export class ExternalCompanyGridRow implements IExternalCompanyResultDTO {
  registrationNr: string;
  name: string;
  streetAddress: IExternalCompanyAddressDTO;
  postalAddress: IExternalCompanyAddressDTO;
  webUrl: string = '';
  addressStr: string = '';

  constructor(row: IExternalCompanyResultDTO) {
    this.registrationNr = row.registrationNr;
    this.name = row.name;
    this.streetAddress = row.streetAddress;
    this.postalAddress = row.postalAddress;
    this.webUrl = row.webUrl;

    this.createAddressString();
  }

  private createAddressString(): void {
    const addressParts = this.getAddressParts(
      this.streetAddress ?? this.postalAddress,
      ['addressLine1', 'zipCode', 'city', 'co']
    );
    this.addressStr = addressParts.filter(x => !!x).join(', ');
  }

  private getAddressParts<T extends object>(
    address: T,
    order: (keyof T)[]
  ): string[] {
    if (!address) return [];
    return order.map(key => String(address[key]));
  }
}
