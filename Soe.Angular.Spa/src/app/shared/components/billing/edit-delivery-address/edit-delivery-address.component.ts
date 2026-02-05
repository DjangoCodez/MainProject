import { Component, inject, signal, Input } from '@angular/core';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component'
import { DialogData } from '@ui/dialog/models/dialog';
import { ValidationHandler } from '@shared/handlers';
import { EditDeliveryAddressForm } from './models/edit-delivery-address-form.model';
import { EditDeliveryAddressDTO } from './models/edit-delivery-address.model';
import { FlowHandlerService } from '@shared/services/flow-handler.service';

export interface IEditDeliveryAddressDialogData extends DialogData {
  form: EditDeliveryAddressForm;
  addressString: string | undefined;
}

@Component({
    selector: 'soe-edit-delivery-address',
    templateUrl: './edit-delivery-address.component.html',
    providers: [FlowHandlerService],
    standalone: false
})
export class EditDeliveryAddressComponent extends DialogComponent<IEditDeliveryAddressDialogData> {
  @Input() addressString: string | undefined;

  validationHandler = inject(ValidationHandler);
  showAddress = signal(false);

  form: EditDeliveryAddressForm = new EditDeliveryAddressForm({
    validationHandler: this.validationHandler,
    element: new EditDeliveryAddressDTO(),
  });

  constructor() {
    super();
    this.form.patchValue({ deliveryAddress: this.data.addressString });
  }

  private textToAddress() {
    if (this.form?.deliveryAddress.value) {
      const address = this.form?.deliveryAddress.value.split(/\r\n|\r|\n/g);
      for (let i = 0; i < address.length; i++) {
        switch (i) {
          case 0:
            this.form.patchValue({ name: address[0] });
            break;
          case 1:
            this.form.patchValue({ address: address[1] });
            break;
          case 2:
            this.form.patchValue({ postalCode: address[2] });
            break;
          case 3:
            this.form.patchValue({ postalAddress: address[3] });
            break;
          case 4:
            this.form.patchValue({ country: address[4] });
            break;
        }
      }
    }
  }

  private addressToText() {
    let text = '';
    text += this.form.name.value;
    text += '\n' + this.form.address.value;
    text += '\n' + this.form.postalCode.value;
    text += '\n' + this.form.postalAddress.value;
    if (this.form.country.value !== '') {
      text += '\n' + this.form.country.value;
    }

    this.form.patchValue({
      deliveryAddress: text,
      name: '',
      address: '',
      postalCode: '',
      postalAddress: '',
      country: '',
    });
  }

  showAddressChanged(event: boolean) {
    this.showAddress.set(event);
    if (event) this.textToAddress();
    else this.addressToText();
  }

  buttonOkClick() {
    if (this.showAddress() == true) this.addressToText();   
    this.dialogRef.close(this.form.deliveryAddress.value);
  }

  closeDialog(): void {
    if (this.form.deliveryAddress.value)
      this.dialogRef.close(this.form.deliveryAddress.value);
    else this.dialogRef.close('');
  }
}
