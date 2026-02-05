import { AbstractControl, FormControl, Validators } from '@angular/forms';
import {
  SoeCheckboxFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import {
  ContactAddressItemType,
  TermGroup_SysContactEComType,
} from '@shared/models/generated-interfaces/Enumerations';
import { EmailValidator } from '@shared/validators/email.validator';
import { merge } from 'rxjs';
import { ContactAddressItem } from './contact-addresses.model';

interface IContactAddressForm {
  validationHandler: ValidationHandler;
  element: ContactAddressItem | undefined;
  allowShowSecret: boolean;
  readOnly: boolean;
}

export class ContactAddressForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler;
  thisAllowShowSecret: boolean;

  get isAddressDelivery(): boolean {
    return (
      this.controls.contactAddressItemType.value ===
      ContactAddressItemType.AddressDelivery
    );
  }

  get isAddressVisiting(): boolean {
    return (
      this.controls.contactAddressItemType.value ===
      ContactAddressItemType.AddressVisiting
    );
  }

  get isAddressBoardHQ(): boolean {
    return (
      this.controls.contactAddressItemType.value ===
      ContactAddressItemType.AddressBoardHQ
    );
  }

  get isECom(): boolean {
    return (
      this.controls.contactAddressItemType.value ===
        ContactAddressItemType.EComEmail ||
      this.controls.contactAddressItemType.value ===
        ContactAddressItemType.EComPhoneHome ||
      this.controls.contactAddressItemType.value ===
        ContactAddressItemType.EComPhoneJob ||
      this.controls.contactAddressItemType.value ===
        ContactAddressItemType.EComPhoneMobile ||
      this.controls.contactAddressItemType.value ===
        ContactAddressItemType.EComFax ||
      this.controls.contactAddressItemType.value ===
        ContactAddressItemType.EComWeb
    );
  }

  get isEComEmail(): boolean {
    return (
      this.controls.contactAddressItemType.value ===
      ContactAddressItemType.EComEmail
    );
  }

  get isClosestRelative(): boolean {
    return (
      this.controls.contactAddressItemType.value ===
      ContactAddressItemType.ClosestRelative
    );
  }

  get isCoordinates(): boolean {
    return (
      this.controls.contactAddressItemType.value ===
      ContactAddressItemType.Coordinates
    );
  }

  get contactAddressItemType(): FormControl {
    return <FormControl>this.controls.contactAddressItemType;
  }

  get isAddress(): FormControl {
    return <FormControl>this.controls.isAddress;
  }

  get sysContactAddressTypeId(): FormControl {
    return <FormControl>this.controls.sysContactAddressTypeId;
  }

  get sysContactEComTypeId(): FormControl {
    return <FormControl>this.controls.sysContactEComTypeId;
  }

  get closestRelativeName(): FormControl {
    return <FormControl>this.controls.closestRelativeName;
  }

  get closestRelativeRelation(): FormControl {
    return <FormControl>this.controls.closestRelativeRelation;
  }

  get longitude(): FormControl {
    return <FormControl>this.controls.longitude;
  }

  get latitude(): FormControl {
    return <FormControl>this.controls.latitude;
  }

  constructor({
    validationHandler,
    element,
    allowShowSecret,
    readOnly,
  }: IContactAddressForm) {
    super(validationHandler, {
      contactId: new SoeSelectFormControl(element?.contactId || 0),
      contactAddressItemType: new SoeSelectFormControl(
        element?.contactAddressItemType || 0
      ),
      isSecret: new SoeCheckboxFormControl(element?.isSecret || false),
      isAddress: new SoeCheckboxFormControl(element?.isAddress || false),
      typeName: new SoeTextFormControl(element?.typeName || ''),
      icon: new SoeTextFormControl(element?.icon || ''),
      name: new SoeTextFormControl(
        element?.name || '',
        {},
        'common.contactaddresses.name'
      ),
      displayAddress: new SoeTextFormControl(element?.displayAddress || ''),
      contactAddressId: new SoeNumberFormControl(
        element?.contactAddressId || 0
      ),
      sysContactAddressTypeId: new SoeNumberFormControl(
        element?.sysContactAddressTypeId || 0
      ),
      addressName: new SoeTextFormControl(
        element?.addressName || '',
        {},
        'common.contactaddresses.addressrow.name'
      ),
      address: new SoeTextFormControl(
        element?.address || '',
        {},
        'common.contactaddresses.addressrow.address'
      ),
      addressIsSecret: new SoeCheckboxFormControl(
        element?.addressIsSecret || false
      ),
      addressCO: new SoeTextFormControl(
        element?.addressCO || '',
        {},
        'common.contactaddresses.addressrow.co'
      ),
      addressCOIsSecret: new SoeCheckboxFormControl(
        element?.addressCOIsSecret || false
      ),
      postalCode: new SoeTextFormControl(
        element?.postalCode || '',
        {},
        'common.contactaddresses.addressrow.postalcode'
      ),
      postalAddress: new SoeTextFormControl(
        element?.postalAddress || '',
        {},
        'common.contactaddresses.addressrow.postaladdress'
      ),
      postalIsSecret: new SoeCheckboxFormControl(
        element?.postalIsSecret || false
      ),
      country: new SoeTextFormControl(
        element?.country || '',
        {},
        'common.contactaddresses.addressrow.country'
      ),
      countryIsSecret: new SoeCheckboxFormControl(
        element?.countryIsSecret || false
      ),
      streetAddress: new SoeTextFormControl(
        element?.streetAddress || '',
        {},
        'common.contactaddresses.addressrow.street'
      ),
      streetAddressIsSecret: new SoeCheckboxFormControl(
        element?.streetAddressIsSecret || false
      ),
      entranceCode: new SoeTextFormControl(
        element?.entranceCode || '',
        {},
        'common.contactaddresses.addressrow.entrancecode'
      ),
      entranceCodeIsSecret: new SoeCheckboxFormControl(
        element?.entranceCodeIsSecret || false
      ),
      contactEComId: new SoeNumberFormControl(element?.contactEComId || 0),
      sysContactEComTypeId: new SoeNumberFormControl(
        element?.sysContactEComTypeId || 0
      ),
      eComText: new SoeTextFormControl(
        element?.eComText || '',
        {},
        'common.contactaddresses.value'
      ),
      eComDescription: new SoeTextFormControl(element?.eComDescription || ''),
      eComIsSecret: new SoeCheckboxFormControl(element?.eComIsSecret || false),
      //Extensions - stored in eComDescription
      closestRelativeName: new SoeTextFormControl(
        '',
        {},
        'common.contactaddresses.ecom.closestrelative.name'
      ),
      closestRelativeRelation: new SoeTextFormControl(
        '',
        {},
        'common.contactaddresses.ecom.closestrelative.relation'
      ),
      longitude: new SoeTextFormControl(
        element?.longitude || '',
        {},
        'common.contactaddresses.ecom.longitude'
      ),
      latitude: new SoeTextFormControl(
        element?.latitude || '',
        {},
        'common.contactaddresses.ecom.latitude'
      ),
    });

    if (readOnly) {
      this.disable();
    }

    if (
      this.controls.contactAddressItemType.value ==
      ContactAddressItemType.ClosestRelative
    )
      this.splitEComDescription(
        this.controls.closestRelativeName,
        this.controls.closestRelativeRelation
      );
    if (
      this.controls.contactAddressItemType.value ==
      ContactAddressItemType.Coordinates
    )
      this.splitEComDescription(
        this.controls.longitude,
        this.controls.latitude
      );

    this.thisValidationHandler = validationHandler;
    this.thisAllowShowSecret = allowShowSecret;

    this.setFormValidators(
      element?.sysContactEComTypeId,
      element?.contactAddressItemType,
      element?.isAddress
    );
    this.setChangeHandlers();
  }

  splitEComDescription(target1: AbstractControl, target2: AbstractControl) {
    const input = this.controls.eComDescription.value;
    if (input.length > 0) {
      const parts = input.split(';');
      if (parts.length > 0) target1.patchValue(parts[0]);
      if (parts.length > 1) {
        target2.patchValue(parts[0]);
      }
    }
  }

  joinEComDescription(value1: string, value2: string) {
    if (value1 || value2) {
      this.controls.eComDescription.patchValue(
        `${value1 || ''};${value2 || ''}`
      );
    } else {
      this.controls.eComDescription.patchValue(null);
    }
  }

  setFormValidators(
    sysContactEComTypeId?: number,
    contactAddressItemType?: ContactAddressItemType,
    isAddress?: boolean
  ) {
    this.controls.eComText.clearValidators();
    this.controls.longitude.clearValidators();
    this.controls.latitude.clearValidators();
    this.controls.closestRelativeName.clearValidators();
    this.controls.closestRelativeRelation.clearValidators();

    if (isAddress === undefined || isAddress === true) {
      return;
    }

    if (
      sysContactEComTypeId === TermGroup_SysContactEComType.Email ||
      sysContactEComTypeId === TermGroup_SysContactEComType.CompanyAdminEmail
    ) {
      this.controls.eComText.addAsyncValidators(
        EmailValidator.validateEmailFormat()
      );
      this.controls.eComText.addValidators(Validators.required);
      (this.controls.eComText as SoeTextFormControl).setValidatorTermKey(
        'common.contactaddresses.ecommenu.email'
      );
    }

    if (contactAddressItemType === ContactAddressItemType.Coordinates) {
      this.controls.longitude.addValidators(Validators.required);
      this.controls.latitude.addValidators(Validators.required);
    }

    if (
      sysContactEComTypeId === TermGroup_SysContactEComType.ClosestRelative ||
      contactAddressItemType === ContactAddressItemType.ClosestRelative
    ) {
      this.controls.closestRelativeName.addValidators(Validators.required);
      this.controls.closestRelativeRelation.addValidators(Validators.required);
    }

    if (
      sysContactEComTypeId !== TermGroup_SysContactEComType.Email &&
      sysContactEComTypeId !== TermGroup_SysContactEComType.CompanyAdminEmail &&
      contactAddressItemType !== ContactAddressItemType.Coordinates
    ) {
      this.controls.eComText.addValidators(Validators.required);
    }
  }

  setChangeHandlers() {
    merge([
      this.controls.name.valueChanges,
      this.controls.addressName.valueChanges,
      this.controls.addressCO.valueChanges,
      this.controls.address.valueChanges,
      this.controls.streetAddress.valueChanges,
      this.controls.entranceCode.valueChanges,
      this.controls.postalCode.valueChanges,
      this.controls.postalAddress.valueChanges,
      this.controls.eComText.valueChanges,
      this.controls.closestRelativeName.valueChanges,
      this.controls.closestRelativeRelation.valueChanges,
      this.controls.longitude.valueChanges,
      this.controls.latitude.valueChanges,
    ]).subscribe(() => {
      // EComDescription
      if (
        this.controls.contactAddressItemType.value ===
        ContactAddressItemType.ClosestRelative
      ) {
        this.joinEComDescription(
          this.controls.closestRelativeName.value,
          this.controls.closestRelativeRelation.value
        );
      }
      if (
        this.controls.contactAddressItemType.value ===
        ContactAddressItemType.Coordinates
      ) {
        this.joinEComDescription(
          this.controls.longitude.value,
          this.controls.latitude.value
        );
        this.controls.eComText.patchValue(this.controls.eComDescription.value);
      }
      // DisplayAddress
      if (this.controls.isSecret.value === true && !this.thisAllowShowSecret) {
        this.controls.displayAddress.patchValue('');
      } else {
        const address = this.controls.address.value;
        const postalCode = this.controls.postalCode.value;
        const postalAddress = this.controls.postalAddress.value;
        const addressCO = this.controls.addressCO.value;
        const streetAddress = this.controls.streetAddress.value;
        const addressName = this.controls.addressName.value;
        const eComText = this.controls.eComText.value;
        const eComDescription = this.controls.eComDescription.value;
        const country = this.controls.country.value;

        switch (this.controls.contactAddressItemType.value) {
          case ContactAddressItemType.AddressDistribution:
            this.controls.displayAddress.patchValue(
              '{0}{1} {2}{3} {4}{5} {6}{7} {8}'.format(
                address,
                !address ? '' : ',',
                postalCode,
                !postalCode ? '' : ',',
                postalAddress,
                !postalAddress ? '' : ',',
                addressCO,
                !addressCO ? '' : ',',
                country
              )
            );
            break;
          case ContactAddressItemType.AddressVisiting:
            this.controls.displayAddress.patchValue(
              '{0}{1} {2}{3} {4}'.format(
                streetAddress,
                !streetAddress ? '' : ',',
                postalCode,
                !postalCode ? '' : ',',
                postalAddress
              )
            );
            break;
          case ContactAddressItemType.AddressBilling:
            this.controls.displayAddress.patchValue(
              '{0}{1} {2}{3} {4}{5} {6}{7} {8}'.format(
                address,
                !address ? '' : ',',
                postalCode,
                !postalCode ? '' : ',',
                postalAddress,
                !postalAddress ? '' : ',',
                addressCO,
                !addressCO ? '' : ',',
                country
              )
            );
            break;
          case ContactAddressItemType.AddressBoardHQ:
            this.controls.displayAddress.patchValue(
              '{0}{1} {2}'.format(
                postalAddress,
                !postalAddress ? '' : ',',
                country
              )
            );
            break;
          case ContactAddressItemType.AddressDelivery:
            this.controls.displayAddress.patchValue(
              '{0}{1} {2}{3} {4}{5} {6}{7} {8}{9} {10}'.format(
                addressName,
                !addressName ? '' : ',',
                address,
                !address ? '' : ',',
                postalCode,
                !postalCode ? '' : ',',
                postalAddress,
                !postalAddress ? '' : ',',
                addressCO,
                !addressCO ? '' : ',',
                country
              )
            );
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
            this.controls.displayAddress.patchValue(eComText);
            break;
          case ContactAddressItemType.ClosestRelative:
            this.controls.displayAddress.patchValue(
              eComText +
                (eComDescription
                  ? ' ' + eComDescription.replace(';', ', ')
                  : '')
            );
            break;
          case ContactAddressItemType.Coordinates:
            if (eComDescription)
              this.controls.displayAddress.patchValue(
                eComDescription.replace(';', ', ')
              );
            break;
        }
        const fullAddress = this.controls.displayAddress.value.trim();
        this.controls.displayAddress.patchValue(
          fullAddress.substring(
            fullAddress.charAt(0) == ',' ? 1 : undefined,
            fullAddress.charAt(fullAddress.length - 1) == ','
              ? fullAddress.length - 1
              : undefined
          )
        );
      }
    });
  }

  customContactAddressItemTypePatchValue(
    value: number,
    addressTypes: { id: number; name: string }[],
    eComTypes: { id: number; name: string }[]
  ) {
    this.controls.contactAddressItemType.patchValue(value);

    switch (value) {
      case ContactAddressItemType.AddressDistribution:
      case ContactAddressItemType.AddressVisiting:
      case ContactAddressItemType.AddressBilling:
      case ContactAddressItemType.AddressDelivery:
      case ContactAddressItemType.AddressBoardHQ: {
        this.controls.isAddress.patchValue(true);
        this.controls.sysContactAddressTypeId.patchValue(value);
        this.setFormValidators(value, value, true);
        break;
      }
      default: {
        this.controls.isAddress.patchValue(false);
        this.controls.sysContactEComTypeId.patchValue(value - 10);
        this.setFormValidators(value - 10, value, false);
        break;
      }
    }

    // Set default name
    if (this.controls.isAddress.value === true) {
      const addressType = addressTypes.find(x => x.id === value);
      if (addressType) this.controls.name.patchValue(addressType.name);
    } else {
      const eComType = eComTypes.find(x => x.id === value);
      if (eComType) {
        this.controls.name.patchValue(eComType.name);
      }
    }
  }
}
