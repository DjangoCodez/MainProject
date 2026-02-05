import { Component } from '@angular/core';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import {
  PurchaseDateDialogData,
  ReturnSetPurchaseDateDialog,
} from '../../models/purchase.model';
import { SoeOriginStatus } from '@shared/models/generated-interfaces/Enumerations';
import { BehaviorSubject } from 'rxjs';
import { PurchaseRowDTO } from '../../models/purchase-rows.model';

@Component({
    selector: 'soe-purchase-set-purchase-date-dialog',
    templateUrl: './purchase-set-purchase-date-dialog.component.html',
    styleUrls: ['./purchase-set-purchase-date-dialog.component.scss'],
    providers: [FlowHandlerService],
    standalone: false
})
export class PurchaseSetPurchaseDateDialogComponent extends DialogComponent<PurchaseDateDialogData> {
  rows!: BehaviorSubject<PurchaseRowDTO[]>;
  newStatus: SoeOriginStatus = SoeOriginStatus.None;
  confirmedDeliveryDate: Date = new Date();
  purchaseDate: Date = new Date();
  useConfirmed = false;
  selectedDate!: Date;
  originalDate!: Date;
  originalDateSet = false;
  propName!: string;
  purchaseRowsChanges: PurchaseRowDTO[] = [];
  constructor() {
    super();
    this.setDialogParam();
  }

  setDialogParam() {
    if (this.data) {
      if (this.data.purchaseRows) {
        this.rows = this.data.purchaseRows;
      }
      if (this.data.newStatus) {
        this.newStatus = this.data.newStatus;
      }
      if (this.data.confirmedDeliveryDate) {
        this.confirmedDeliveryDate = this.data.confirmedDeliveryDate;
      }
      if (this.data.purchaseDate) {
        this.purchaseDate = this.data.purchaseDate;
      }
      if (this.data.useConfirmed) {
        this.useConfirmed = this.data.useConfirmed;
      }
    }
  }

  dateChange(dateObj: ReturnSetPurchaseDateDialog) {
    this.selectedDate = dateObj.selectedDate;
    this.originalDate = dateObj.originalDate;
    this.originalDateSet = dateObj.originalDateSet;
    this.propName = dateObj.propName;
    this.purchaseRowsChanges = dateObj.purchaseRowsChanges;
  }

  selectPurchaseDate() {
    const obj = new ReturnSetPurchaseDateDialog(
      this.selectedDate,
      this.originalDate,
      this.originalDateSet,
      this.propName,
      this.purchaseRowsChanges
    );
    this.close(obj);
  }

  close(data?: ReturnSetPurchaseDateDialog) {
    this.dialogRef.close(data);
  }

  closeDialog() {
    this.close();
  }
}
