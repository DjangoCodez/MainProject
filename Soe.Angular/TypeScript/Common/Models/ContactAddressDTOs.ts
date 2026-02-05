import { IContactAddressDTO, IContactAddressRowDTO, IContactAddressItem } from "../../Scripts/TypeLite.Net4";
import { StringUtility } from "../../Util/StringUtility";
import { TermGroup_SysContactAddressType, TermGroup_SysContactAddressRowType, ContactAddressItemType } from "../../Util/CommonEnumerations";

export class ContactAddressDTO implements IContactAddressDTO {
    contactAddressId: number;
    contactId: number;
    sysContactAddressTypeId: TermGroup_SysContactAddressType;
    name: string;
    isSecret: boolean;
    created: Date;
    createdBy: string;
    modified: Date;
    modifiedBy: string;
    contactAddressRows: ContactAddressRowDTO[];
    address: string;
}

export class ContactAddressRowDTO implements IContactAddressRowDTO {
    rowNr: number;
    contactAddressId: number;
    sysContactAddressRowTypeId: TermGroup_SysContactAddressRowType;
    text: string;
    created: Date;
    createdBy: string;
    modified: Date;
    modifiedBy: string;
}

export class ContactAddressItemDTO implements IContactAddressItem {
    address: string;
    addressCO: string;
    addressCOIsSecret: boolean;
    addressIsSecret: boolean;
    addressName: string;
    contactAddressId: number;
    contactAddressItemType: ContactAddressItemType;
    contactEComId: number;
    contactId: number;
    country: string;
    countryIsSecret: boolean;
    displayAddress: string;
    eComDescription: string;
    eComIsSecret: boolean;
    eComText: string;
    entranceCode: string;
    entranceCodeIsSecret: boolean;
    icon: string;
    isAddress: boolean;
    isSecret: boolean;
    name: string;
    postalAddress: string;
    postalCode: string;
    postalIsSecret: boolean;
    streetAddress: string;
    streetAddressIsSecret: boolean;
    sysContactAddressTypeId: number;
    sysContactEComTypeId: number;
    typeName: string;

    // Extensions
    closestRelativeName: string;
    closestRelativeRelation: string;
    longitude: string;
    latitude: string;

    public get isAddressDelivery(): boolean {
        return this.contactAddressItemType === ContactAddressItemType.AddressDelivery;
    }

    public get isAddressVisiting(): boolean {
        return this.contactAddressItemType === ContactAddressItemType.AddressVisiting;
    }

    public get isAddressBoardHQ(): boolean {
        return this.contactAddressItemType === ContactAddressItemType.AddressBoardHQ;
    }

    public get isECom(): boolean {
        return (this.contactAddressItemType === ContactAddressItemType.EComEmail ||
            this.contactAddressItemType === ContactAddressItemType.EComPhoneHome ||
            this.contactAddressItemType === ContactAddressItemType.EComPhoneJob ||
            this.contactAddressItemType === ContactAddressItemType.EComPhoneMobile ||
            this.contactAddressItemType === ContactAddressItemType.EComFax ||
            this.contactAddressItemType === ContactAddressItemType.EComWeb);
    }

    public get isClosestRelative(): boolean {
        return this.contactAddressItemType === ContactAddressItemType.ClosestRelative;
    }

    public get isCoordinates(): boolean {
        return this.contactAddressItemType === ContactAddressItemType.Coordinates;
    }

    public static setDisplayAddress(item: ContactAddressItemDTO) {
        switch (item.contactAddressItemType) {
            case ContactAddressItemType.AddressDistribution:
                item.displayAddress = "{0}{1} {2}{3} {4}{5} {6}{7} {8}".format(item.address, (!item.postalCode) ? '' : ',', item.postalCode, (!item.postalAddress) ? '' : ',', item.postalAddress, (!item.addressCO) ? '' : ',', item.addressCO, (!item.country) ? '' : ',', item.country);
                break;
            case ContactAddressItemType.AddressVisiting:
                item.displayAddress = "{0}{1} {2}{3} {4}".format(item.streetAddress, (!item.postalCode) ? '' : ',', item.postalCode, (!item.postalAddress) ? '' : ',', item.postalAddress);
                break;
            case ContactAddressItemType.AddressBilling:
                item.displayAddress = "{0}{1} {2}{3} {4}{5} {6}{7} {8}".format(item.address, (!item.postalCode) ? '' : ',', item.postalCode, (!item.postalAddress) ? '' : ',', item.postalAddress, (!item.addressCO) ? '' : ',', item.addressCO, (!item.country) ? '' : ',', item.country);
                break;
            case ContactAddressItemType.AddressBoardHQ:
                item.displayAddress = "{0}{1} {2}".format(item.postalAddress, (!item.postalAddress) ? '' : ',', item.country);
                break;
            case ContactAddressItemType.AddressDelivery:
                item.displayAddress = "{0}{1} {2}{3} {4}{5} {6}{7} {8}{9} {10}".format(item.addressName, (!item.address) ? '' : ',', item.address, (!item.postalCode) ? '' : ',', item.postalCode, (!item.postalAddress) ? '' : ',', item.postalAddress, (!item.addressCO) ? '' : ',', item.addressCO, (!item.country) ? '' : ',', item.country);
                break;
            case ContactAddressItemType.EComEmail:
            case ContactAddressItemType.EComPhoneHome:
            case ContactAddressItemType.EComPhoneJob:
            case ContactAddressItemType.EComPhoneMobile:
            case ContactAddressItemType.EComFax:
            case ContactAddressItemType.EComWeb:
            case ContactAddressItemType.EcomCompanyAdminEmail:
            case ContactAddressItemType.IndividualTaxNumber:
            case ContactAddressItemType.GlnNumber:
                item.displayAddress = item.eComText;
                break;
            case ContactAddressItemType.ClosestRelative:
                item.displayAddress = item.eComText;
                if (item.eComDescription)
                    item.displayAddress += ' ' + item.eComDescription.replace(';', ', ');
                break;
            case ContactAddressItemType.Coordinates:
                if (item.eComDescription)
                    item.displayAddress = item.eComDescription.replace(';', ', ');
                break;
        }

        // Remove leading or trailing commas
        if (item.displayAddress)
            item.displayAddress = item.displayAddress.substring(item.displayAddress.startsWith(',') ? 1 : undefined, item.displayAddress.endsWith(',') ? item.displayAddress.length - 1 : undefined);
    }

    public static getIcon(type: ContactAddressItemType) {

        switch (type) {
            case ContactAddressItemType.AddressDistribution:
                return "fal fa-fw fa-mailbox";
            case ContactAddressItemType.AddressVisiting:
                return "fal fa-fw fa-home";
            case ContactAddressItemType.AddressBilling:
                return "fal fa-fw fa-file-alt";
            case ContactAddressItemType.AddressBoardHQ:
                return "fal fa-fw fa-building";
            case ContactAddressItemType.AddressDelivery:
                return "fal fa-fw fa-truck";
            case ContactAddressItemType.EComEmail:
                return "fal fa-fw fa-envelope";
            case ContactAddressItemType.EComPhoneHome:
                return "fal fa-fw fa-phone";
            case ContactAddressItemType.EComPhoneJob:
                return "fal fa-fw fa-phone-office";
            case ContactAddressItemType.EComPhoneMobile:
                return "fal fa-fw fa-mobile-android-alt";
            case ContactAddressItemType.EComFax:
                return "fal fa-fw fa-fax";
            case ContactAddressItemType.EComWeb:
                return "fal fa-fw fa-globe-europe";
            case ContactAddressItemType.ClosestRelative:
                return "fal fa-fw fa-user-friends";
            case ContactAddressItemType.EcomCompanyAdminEmail:
                return "fal fa-fw fa-envelope";
            case ContactAddressItemType.Coordinates:
                return "fal fa-fw fa-map-marked";
            case ContactAddressItemType.IndividualTaxNumber:
                return "fal fa-fw fa-money-bill-alt";
            case ContactAddressItemType.GlnNumber:
                return "fal fa-fw fa-paper-plane";
        }
    }

    public static setIcon(item: ContactAddressItemDTO) {
        item.icon = this.getIcon(item.contactAddressItemType);
    }
}
