import { Component, OnInit, inject, signal } from '@angular/core';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { SupplierPaymentMethodsService } from '../../services/supplier-payment-methods.service';
import {
  IPaymentInformationViewDTOSmall,
  IPaymentMethodDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import {
  Feature,
  SoeOriginType,
  TermGroup_Languages,
  TermGroup_SysPaymentMethod,
} from '@shared/models/generated-interfaces/Enumerations';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { EconomyService } from '@src/app/features/economy/services/economy.service';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import { SupplierPaymentMethodsForm } from '../../models/supplier-payment-methods-form.model';
import { ProgressOptions } from '@shared/services/progress';
import { CrudActionTypeEnum } from '@shared/enums';
import { PaymentInformationRowDTO } from '../../models/supplier-payment-methods.model';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';

@Component({
  selector: 'soe-supplier-payment-methods-edit',
  templateUrl: './supplier-payment-methods-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class SupplierPaymentMethodsEditComponent
  extends EditBaseDirective<
    IPaymentMethodDTO,
    SupplierPaymentMethodsService,
    SupplierPaymentMethodsForm
  >
  implements OnInit
{
  service = inject(SupplierPaymentMethodsService);
  accountService = inject(EconomyService);
  accountStdsDict: SmallGenericType[] = [];
  sysPaymentMethods: SmallGenericType[] = [];
  paymentInformation: IPaymentInformationViewDTOSmall[] = [];

  showBankId = signal(false);
  showPaymentInformationParantheses = signal(false);
  paymentInformationCurrency = signal('');

  ngOnInit() {
    super.ngOnInit();
    this.startFlow(
      Feature.Economy_Preferences_SuppInvoiceSettings_PaymentMethods_Edit,
      {
        skipDefaultToolbar: true,
        lookups: [
          this.loadAccountStdsDict(),
          this.loadSysPaymentTypes(),
          this.loadPaymentInformation(),
        ],
      }
    );

    this.setDefaultValues();
    this.onCopy();
    this.onNew();
  }

  private setDefaultValues() {
    this.form?.patchValue({ paymentType: SoeOriginType.SupplierPayment });
  }
  private onNew() {
    if (this.form?.isNew) {
      this.form?.patchValue({ PaymentInformationRowId: 0 });
    }
  }

  private onCopy() {
    if (this.form?.isCopy) {
      this.form?.patchValue({ PaymentInformationRowId: 0 });
      this.updateShowFields(this.form?.value.sysPaymentMethodId);
    }
  }

  private loadAccountStdsDict(): Observable<void> {
    return this.performLoadData.load$(
      this.accountService
        .getAccountStdsDict(false)
        .pipe(tap(x => (this.accountStdsDict = x)))
    );
  }

  private loadSysPaymentTypes(): Observable<void> {
    return this.performLoadData.load$(
      this.service
        .getSysPaymentMethodsDict(false)
        .pipe(tap(x => (this.sysPaymentMethods = x)))
    );
  }

  private loadPaymentInformation(): Observable<void> {
    this.paymentInformation = [];
    return this.performLoadData.load$(
      this.service.getPaymentInformationViewsSmall(false).pipe(
        tap(x => {
          this.paymentInformation = x;
          this.setCurrencyCodeParantheses(
            this.form?.value.paymentInformationRowId
          );
        })
      )
    );
  }

  onPaymentInformationChange(value: number) {
    this.setCurrencyCodeParantheses(value);
  }

  sysPaymentMethodChanged(value: any) {
    this.updateShowFields(value);
  }

  private setCurrencyCodeParantheses(value: any) {
    this.showPaymentInformationParantheses.set(false);
    this.paymentInformationCurrency.set('');
    const payInfo = this.paymentInformation?.find(pi => pi.id == value);
    if (payInfo) {
      if (payInfo.currencyCode) {
        this.showPaymentInformationParantheses.set(true);
        this.paymentInformationCurrency.set(payInfo.currencyCode);
      }
    }
  }

  private updateShowFields(sysPaymentMethod: any) {
    this.showBankId.set(
      sysPaymentMethod == TermGroup_SysPaymentMethod.NordeaCA ||
        SoeConfigUtil.sysCountryId == TermGroup_Languages.Finnish ||
        sysPaymentMethod == TermGroup_SysPaymentMethod.ISO20022
    );
  }

  override loadData(): Observable<void> {
    return this.performLoadData.load$(
      this.service.get(this.form?.getIdControl()?.value).pipe(
        tap(value => {
          this.form?.reset(value);
          this.setCurrencyCodeParantheses(value.paymentInformationRowId);
          this.updateShowFields(value.sysPaymentMethodId);
        })
      )
    );
  }

  override performSave(options?: ProgressOptions): void {
    if (!this.form || this.form.invalid || !this.service) return;
    const model = this.form?.getRawValue();
    model.paymentInformationRow = new PaymentInformationRowDTO();
    model.paymentInformationRow.paymentInformationRowId =
      model.paymentInformationRowId;
    model.paymentInformationRow.payerBankId = model.payerBankId;
    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.service.save(model).pipe(
        tap(res => {
          this.updateFormValueAndEmitChange(res);
        })
      ),
      undefined,
      undefined,
      options
    );
  }
}
