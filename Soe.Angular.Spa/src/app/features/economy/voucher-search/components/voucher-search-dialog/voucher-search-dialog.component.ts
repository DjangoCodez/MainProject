import { Component } from '@angular/core';
import { ISearchVoucherRowDTO } from '@shared/models/generated-interfaces/SearchVoucherRowDTO';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component'
import { DialogData } from '@ui/dialog/models/dialog';

export interface VoucherSearchDialogData extends DialogData {
  dim1Id: number;
  dim1Name: string;
  dim1Nr: string;
}

@Component({
  selector: 'soe-voucher-search-dialog',
  templateUrl: './voucher-search-dialog.component.html',
  standalone: false,
})
export class VoucherSearchDialogComponent extends DialogComponent<VoucherSearchDialogData> {
  close() {
    this.dialogRef.close();
  }

  closeVoucherSearchDialog(row: ISearchVoucherRowDTO) {
    this.dialogRef.close(row);
  }
}
