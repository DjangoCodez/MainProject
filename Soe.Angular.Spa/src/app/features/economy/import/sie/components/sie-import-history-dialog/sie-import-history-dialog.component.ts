import { Component } from '@angular/core';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component'
import { DialogData } from '@ui/dialog/models/dialog';

@Component({
  selector: 'soe-sie-import-history-dialog',
  templateUrl: './sie-import-history-dialog.component.html',
  standalone: false,
})
export class SieImportHistoryDialogComponent extends DialogComponent<DialogData> {}
