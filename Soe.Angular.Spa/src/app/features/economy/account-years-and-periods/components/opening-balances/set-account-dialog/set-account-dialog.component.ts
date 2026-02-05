import { Component, inject, OnInit } from '@angular/core';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { SetAccountDialogData } from '../../../models/account-years-and-periods.model';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ValidationHandler } from '@shared/handlers';
import { SetAccountDialogForm } from '@features/economy/account-years-and-periods/models/set-account-dialog-form.model';

@Component({
  selector: 'soe-set-account-dialog',
  templateUrl: './set-account-dialog.component.html',
  providers: [FlowHandlerService, ValidationHandler],
  standalone: false,
})
export class SetAccountDialogComponent
  extends DialogComponent<SetAccountDialogData>
  implements OnInit
{
  validationHandler = inject(ValidationHandler);

  form: SetAccountDialogForm = new SetAccountDialogForm({
    validationHandler: this.validationHandler,
    element: new SetAccountDialogData(),
  });

  constructor(public handler: FlowHandlerService) {
    super();
  }

  ngOnInit(): void {
    this.form.amount.patchValue(this.data.amount);
    this.form.amount.disable();
  }

  cancel() {
    this.dialogRef.close(false);
  }

  save() {
    this.dialogRef.close(
      this.data.accounts.find(x => x.accountId === this.form.accountId.value)
    );
  }
}
