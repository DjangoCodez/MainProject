import {
  Component,
  OnInit,
  WritableSignal,
  inject,
  signal,
} from '@angular/core';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';

import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { ValidationHandler } from '@shared/handlers';
import { forkJoin, tap } from 'rxjs';
import { AddAccountForm } from '../../models/add-account-form.model';
import {
  AccountEditDTO,
  AddAccountDialogData,
  AddAccountDialogResultData,
  AddAccountDialogResultType,
} from '../../models/add-account.model';
import { AccountingService } from '@features/economy/services/accounting.service';
import { ISysAccountStdDTO } from '@shared/models/generated-interfaces/SOESysModelDTOs';
import { TermGroup } from '@shared/models/generated-interfaces/Enumerations';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'soe-add-account-dialog',
  templateUrl: './add-account-dialog.component.html',
  providers: [FlowHandlerService],
  standalone: false,
})
export class AddAccountDialogComponent
  extends DialogComponent<AddAccountDialogData>
  implements OnInit
{
  handler = inject(FlowHandlerService);
  progressService = inject(ProgressService);
  accountingService = inject(AccountingService);
  coreService = inject(CoreService);
  translateService = inject(TranslateService);

  validationHandler = inject(ValidationHandler);
  form: AddAccountForm = new AddAccountForm({
    validationHandler: this.validationHandler,
    element: new AccountEditDTO(),
  });

  sysAccountFound: WritableSignal<boolean> = signal(false);
  searching: WritableSignal<boolean> = signal(true);
  addingAccount: WritableSignal<boolean> = signal(false);

  accountNr: WritableSignal<string> = signal('');
  sysAccount!: ISysAccountStdDTO;
  accountTypes: WritableSignal<SmallGenericType[]> = signal([]);
  sysVatAccounts: WritableSignal<SmallGenericType[]> = signal([]);
  sysAccountSruCodes: WritableSignal<SmallGenericType[]> = signal([]);

  ngOnInit() {
    this.setDialogParam();
  }

  //#region Helper Methods

  setDialogParam() {
    if (this.data) {
      this.accountNr.set(this.data.accountNr ?? '');
      this.form.patchValue({
        accountNr: this.data.accountNr ?? '',
      });

      if (this.accountNr.length === 4) {
        this.loadSysAccount();
      } else {
        this.searching.set(false);
      }
    }
  }
  //#endregion

  //#region UI Actions

  cancel(data: AddAccountDialogResultData | null = null) {
    this.dialogRef.close(data);
  }
  save(): void {
    if (!(!this.sysAccountFound() && !this.addingAccount())) {
      if (this.sysAccountFound()) {
        this.cancel(
          new AddAccountDialogResultData(
            AddAccountDialogResultType.Copy,
            this.sysAccount
          )
        );
      } else {
        this.cancel(
          new AddAccountDialogResultData(
            AddAccountDialogResultType.New,
            this.form.value
          )
        );
      }
    }
  }
  ok(): void {
    if (!this.sysAccountFound() && !this.addingAccount()) {
      this.addingAccount.set(true);
      forkJoin([
        this.loadAccountTypes(),
        this.loadSysVatAccounts(),
        this.loadSysAccountSruCodes(),
      ]).subscribe();

      return;
    }
  }
  //#endregion

  //#region Data Loading

  private loadSysAccount() {
    this.accountingService
      .getSysAccountStd(0, this.accountNr())
      .pipe(
        tap(data => {
          this.sysAccount = data;
          if (this.sysAccount) this.sysAccountFound.set(true);
          this.searching.set(false);
        })
      )
      .subscribe();
  }

  private loadAccountTypes() {
    return this.coreService
      .getTermGroupContent(TermGroup.AccountType, false, true)
      .pipe(
        tap(data => {
          this.accountTypes.set(data);
        })
      );
  }

  private loadSysVatAccounts() {
    return this.accountingService
      .getSysVatAccounts(SoeConfigUtil.sysCountryId, true)
      .pipe(
        tap(data => {
          this.sysVatAccounts.set(data);
        })
      );
  }

  private loadSysAccountSruCodes() {
    return this.accountingService.getSysAccountSruCodes(true).pipe(
      tap(x => {
        this.sysAccountSruCodes.set(x);
      })
    );
  }

  //#endregion
}
