import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { Observable, Subject } from 'rxjs';
import { take, takeUntil, tap } from 'rxjs/operators';
import { VatCodeService } from '../../services/vat-codes.service';
import { EconomyService } from '../../../services/economy.service';
import { VatCodeDTO } from '../../../models/vat-code.model';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { VatCodeForm } from '../../models/vat-codes-form.model';
import { TermCollection } from '@shared/localization/term-types';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';

@Component({
  selector: 'soe-vat-codes-edit',
  templateUrl: './vat-codes-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class VatCodesEditComponent
  extends EditBaseDirective<VatCodeDTO, VatCodeService, VatCodeForm>
  implements OnInit, OnDestroy
{
  service = inject(VatCodeService);
  economyService = inject(EconomyService);

  private _destroy$ = new Subject<void>();

  accounts: SmallGenericType[] = [];

  ngOnInit() {
    super.ngOnInit();

    this.loadAccountSysVatRate();
    this.loadPurchaseVATAccountSysVatRate();

    this.form?.accountId.valueChanges
      .pipe(takeUntil(this._destroy$))
      .subscribe(() => {
        this.loadAccountSysVatRate();
      });
    this.form?.purchaseVATAccountId.valueChanges
      .pipe(takeUntil(this._destroy$))
      .subscribe(() => {
        this.loadPurchaseVATAccountSysVatRate();
      });

    this.startFlow(Feature.Economy_Preferences_VoucherSettings_VatCodes_Edit, {
      lookups: [this.loadAccounts()],
    });
  }

  // SERVICE CALLS

  override onPermissionsLoaded() {
    super.onPermissionsLoaded();
    this.form?.controls.accountSysVatRate.disable();
    this.form?.controls.purchaseVATAccountSysVatRate.disable();
  }

  override loadTerms(): Observable<TermCollection> {
    return super.loadTerms(['core.deletewarning', 'core.delete']);
  }

  private loadAccounts() {
    return this.economyService
      .getAccountStdsDict(true)
      .pipe(tap(accounts => (this.accounts = accounts)));
  }

  private loadAccountSysVatRate() {
    if (!this.form?.accountId.value) {
      this.form?.patchValue({ accountSysVatRate: null });
      return;
    }
    this.economyService
      .getAccountSysVatRate(this.form?.accountId.value)
      .pipe(take(1))
      .subscribe(x => {
        this.form?.patchValue({ accountSysVatRate: `${x} %` });
      });
  }

  private loadPurchaseVATAccountSysVatRate() {
    if (!this.form?.purchaseVATAccountId.value) {
      this.form?.patchValue({ purchaseVATAccountSysVatRate: null });
      return;
    }
    this.economyService
      .getAccountSysVatRate(this.form?.purchaseVATAccountId.value)
      .pipe(take(1))
      .subscribe(x => {
        this.form?.patchValue({ purchaseVATAccountSysVatRate: `${x} %` });
      });
  }

  ngOnDestroy(): void {
    this._destroy$.next();
    this._destroy$.complete();
  }
}
