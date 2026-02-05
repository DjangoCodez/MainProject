import { Component, inject, OnInit } from '@angular/core';
import {
  UpdateAccountDimStdDialogDTO,
  UpdateAccountDimStdDialogForm,
} from './update-account-dim-std-dialog.model';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component'
import { DialogData } from '@ui/dialog/models/dialog';
import { ValidationHandler } from '@shared/handlers';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { EconomyService } from '@features/economy/services/economy.service';
import { take, tap } from 'rxjs';

@Component({
  selector: 'soe-update-account-dim-std',
  templateUrl: './update-account-dim-std.component.html',
  standalone: false,
})
export class UpdateAccountDimStdComponent
  extends DialogComponent<DialogData>
  implements OnInit
{
  validationHandler = inject(ValidationHandler);
  economyService = inject(EconomyService);

  accountStdTypes: SmallGenericType[] = [];

  ngOnInit(): void {
    this.economyService
      .getSysAccountStdTypes()
      .pipe(
        take(1),
        tap(types => {
          this.accountStdTypes = types;
        })
      )
      .subscribe();
  }

  form: UpdateAccountDimStdDialogForm = new UpdateAccountDimStdDialogForm({
    validationHandler: this.validationHandler,
    element: new UpdateAccountDimStdDialogDTO(),
  });

  cancel(): void {
    this.dialogRef.close(false);
  }

  import(): void {
    this.economyService
      .importSysAccountStdType(this.form.accountStdTypeId.getRawValue())
      .pipe(
        take(1),
        tap(res => {
          if (res.success) this.dialogRef.close(true);
          else this.dialogRef.close(false);
        })
      )
      .subscribe();
  }
}
