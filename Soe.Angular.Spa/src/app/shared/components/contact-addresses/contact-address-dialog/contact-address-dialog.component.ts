import { Component, inject } from '@angular/core';
import { ValidationHandler } from '@shared/handlers';
import {
  ContactAddressItemType,
  TermGroup_SysContactAddressRowType,
  TermGroup_SysContactEComType,
} from '@shared/models/generated-interfaces/Enumerations';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component'
import { DialogData, DialogSize } from '@ui/dialog/models/dialog';
import { ContactAddressForm } from '../contact-addresses-form.model';
import { ContactAddressItem } from '../contact-addresses.model';

export class ContactAddressDialogData implements DialogData {
  title!: string;
  size?: DialogSize;
  rowToUpdate?: ContactAddressItem;
  newRowAddressItemType?: number;
  addressRowTypes!: { field1: number; field2: number }[];
  addressTypes!: { id: number; name: string }[];
  eComTypes!: { id: number; name: string }[];
  allowShowSecret!: boolean;
  readOnly!: boolean;
}

@Component({
  selector: 'soe-contact-address-dialog',
  templateUrl: './contact-address-dialog.component.html',
  styleUrls: ['./contact-address-dialog.component.scss'],
  standalone: false,
})
export class ContactAddressDialogComponent extends DialogComponent<ContactAddressDialogData> {
  validationHandler = inject(ValidationHandler);

  form!: ContactAddressForm;

  constructor() {
    super();

    this.form = new ContactAddressForm({
      validationHandler: this.validationHandler,
      element: this.data.rowToUpdate,
      allowShowSecret: this.data.allowShowSecret,
      readOnly: this.data.readOnly,
    });

    if (this.data.newRowAddressItemType) {
      this.form.customContactAddressItemTypePatchValue(
        this.data.newRowAddressItemType,
        this.data.addressTypes,
        this.data.eComTypes
      );
    }
  }

  isAddress(): boolean {
    return this.form.isAddress.value === true;
  }

  isCoordinates(): boolean {
    return this.form.isCoordinates;
  }

  showAddress(): boolean {
    return this.data.addressRowTypes.some(
      x =>
        x.field1 === this.form.sysContactAddressTypeId.value &&
        x.field2 === TermGroup_SysContactAddressRowType.Address
    );
  }

  showAddressCO(): boolean {
    return this.data.addressRowTypes.some(
      x =>
        x.field1 === this.form.sysContactAddressTypeId.value &&
        x.field2 === TermGroup_SysContactAddressRowType.AddressCO
    );
  }

  showStreetAddress(): boolean {
    return this.data.addressRowTypes.some(
      x =>
        x.field1 === this.form.sysContactAddressTypeId.value &&
        x.field2 === TermGroup_SysContactAddressRowType.StreetAddress
    );
  }

  showEntranceCode(): boolean {
    return this.data.addressRowTypes.some(
      x =>
        x.field1 === this.form.sysContactAddressTypeId.value &&
        x.field2 === TermGroup_SysContactAddressRowType.EntranceCode
    );
  }

  showPostalCode(): boolean {
    return this.data.addressRowTypes.some(
      x =>
        x.field1 === this.form.sysContactAddressTypeId.value &&
        x.field2 === TermGroup_SysContactAddressRowType.PostalCode
    );
  }

  showPostalAddress(): boolean {
    return this.data.addressRowTypes.some(
      x =>
        x.field1 === this.form.sysContactAddressTypeId.value &&
        x.field2 === TermGroup_SysContactAddressRowType.PostalAddress
    );
  }

  showCountry(): boolean {
    return this.data.addressRowTypes.some(
      x =>
        x.field1 === this.form.sysContactAddressTypeId.value &&
        x.field2 === TermGroup_SysContactAddressRowType.Country
    );
  }

  showEmail(): boolean {
    return (
      this.form.sysContactEComTypeId.value ==
        TermGroup_SysContactEComType.Email ||
      this.form.sysContactEComTypeId.value ==
        TermGroup_SysContactEComType.CompanyAdminEmail
    );
  }

  showClosestRelative(): boolean {
    return (
      this.form.sysContactEComTypeId.value ==
        TermGroup_SysContactEComType.ClosestRelative ||
      this.form.contactAddressItemType.value ==
        ContactAddressItemType.ClosestRelative
    );
  }

  cancel() {
    this.dialogRef.close();
  }

  protected ok(): void {
    this.dialogRef.close(this.form.value);
  }
}
