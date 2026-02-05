import { Component, inject, signal } from '@angular/core';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { PurchaseDeliveryAddressesDialogData } from '../../models/purchase.model';
import { FlowHandlerService } from '@shared/services/flow-handler.service'
import { ProgressService } from '@shared/services/progress/progress.service';
import { PurchaseService } from '../../services/purchase.service';
import { Perform } from '@shared/util/perform.class';
import { tap } from 'rxjs';

@Component({
    selector: 'soe-purchase-delivery-addresses-dialog',
    templateUrl: './purchase-delivery-addresses-dialog.component.html',
    styleUrls: ['./purchase-delivery-addresses-dialog.component.scss'],
    providers: [FlowHandlerService],
    standalone: false
})
export class PurchaseDeliveryAddressesDialogComponent extends DialogComponent<PurchaseDeliveryAddressesDialogData> {
  customerOrderId = 0;
  addresses: any[] = [];
  selectedAddress = '';
  noAddresses = signal(true);
  isAddressSelected = signal(false);

  purchaseService = inject(PurchaseService);
  progressService = inject(ProgressService);
  performDeliveryAddressLoad = new Perform<string[]>(this.progressService);

  constructor() {
    super();
    this.setDialogParam();
    this.getDeliveryAddresses(this.customerOrderId);
  }

  setDialogParam() {
    if (this.data) {
      if (this.data.customerOrderId) {
        this.customerOrderId = this.data.customerOrderId;
      }
    }
  }

  getDeliveryAddresses(customerOrderId: number) {
    return this.performDeliveryAddressLoad.load(
      this.purchaseService.getDeliveryAddresses(customerOrderId).pipe(
        tap(data => {
          this.addresses = [];
          data.forEach((address, index) => {
            this.addresses.push({
              index: index,
              checked: false,
              address: address,
            });
          });
          if (this.addresses.length > 0) {
            this.noAddresses.set(false);
          }
        })
      )
    );
  }
  selectAddresses(address: any) {
    this.isAddressSelected.set(false);
    this.addresses.forEach((a, index) => {
      if (address.index == a.index) {
        if (!address.checked) {
          a.checked = true;
          this.selectedAddress = address.address;
          this.isAddressSelected.set(true);
        } else {
          a.checked = false;
        }
      } else {
        a.checked = false;
      }
    });
  }
  cancel(): void {
    this.dialogRef.close(null);
  }
  ok(): void {
    if (this.selectedAddress) this.dialogRef.close(this.selectedAddress);
  }
}
