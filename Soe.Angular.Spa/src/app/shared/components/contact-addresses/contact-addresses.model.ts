import { IconName } from '@fortawesome/fontawesome-svg-core';
import { IContactAddressItem } from '@shared/models/generated-interfaces/ContactAddressItem';
import { ContactAddressItemType } from '@shared/models/generated-interfaces/Enumerations';

export class ContactAddressItem implements IContactAddressItem {
  contactId!: number;
  contactAddressItemType!: ContactAddressItemType;
  isSecret!: boolean;
  isAddress!: boolean;
  typeName!: string;
  icon!: string;
  name!: string;
  displayAddress!: string;
  contactAddressId!: number;
  sysContactAddressTypeId!: number;
  addressName!: string;
  address!: string;
  addressIsSecret!: boolean;
  addressCO!: string;
  addressCOIsSecret!: boolean;
  postalCode!: string;
  postalAddress!: string;
  postalIsSecret!: boolean;
  country!: string;
  countryIsSecret!: boolean;
  streetAddress!: string;
  streetAddressIsSecret!: boolean;
  entranceCode!: string;
  entranceCodeIsSecret!: boolean;
  contactEComId!: number;
  sysContactEComTypeId!: number;
  eComText!: string;
  eComDescription!: string;
  eComIsSecret!: boolean;

  //Extensions
  typeIcon!: IconName;
  longitude!: string;
  latitude!: string;

  static fromServerModel(model: ContactAddressItem): ContactAddressItem {
    return { ...model, typeIcon: getIcon(model.contactAddressItemType) };
  }
}

export function getIcon(type: ContactAddressItemType): IconName {
  switch (type) {
    case ContactAddressItemType.AddressDistribution:
      return 'mailbox';
    case ContactAddressItemType.AddressVisiting:
      return 'home';
    case ContactAddressItemType.AddressBilling:
      return 'file-alt';
    case ContactAddressItemType.AddressBoardHQ:
      return 'building';
    case ContactAddressItemType.AddressDelivery:
      return 'truck';
    case ContactAddressItemType.EComEmail:
      return 'envelope';
    case ContactAddressItemType.EComPhoneHome:
      return 'phone';
    case ContactAddressItemType.EComPhoneJob:
      return 'phone-office';
    case ContactAddressItemType.EComPhoneMobile:
      return 'mobile-android-alt';
    case ContactAddressItemType.EComFax:
      return 'fax';
    case ContactAddressItemType.EComWeb:
      return 'globe-europe';
    case ContactAddressItemType.ClosestRelative:
      return 'user-friends';
    case ContactAddressItemType.EcomCompanyAdminEmail:
      return 'envelope';
    case ContactAddressItemType.Coordinates:
      return 'map-marked';
    case ContactAddressItemType.IndividualTaxNumber:
      return 'money-bill-alt';
    case ContactAddressItemType.GlnNumber:
      return 'paper-plane';
  }
}
