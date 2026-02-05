import { Component, OnInit, inject, signal } from '@angular/core';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import {
  IAccountNumberNameDTO,
  IPaymentMethodDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { CustomerPaymentMethodsService } from '../../../services/customer-payment-methods.service';
import {
  Feature,
  TermGroup_AccountType,
} from '@shared/models/generated-interfaces/Enumerations';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { EconomyService } from '@src/app/features/economy/services/economy.service';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { CustomerPaymentMethodsForm } from '../../../models/customer-payment-methods-form.model';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';

@Component({
  selector: 'soe-customer-payment-methods-edit',
  templateUrl: './customer-payment-methods-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class CustomerPaymentMethodsEditComponent
  extends EditBaseDirective<
    IPaymentMethodDTO,
    CustomerPaymentMethodsService,
    CustomerPaymentMethodsForm
  >
  implements OnInit
{
  service = inject(CustomerPaymentMethodsService);
  accountService = inject(EconomyService);
  accountStds: IAccountNumberNameDTO[] = [];
  sysPaymentMethods: SmallGenericType[] = [];
  paymentInformation: SmallGenericType[] = [];
  canVisibleTransactionCode = signal(false);

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(
      Feature.Economy_Preferences_CustInvoiceSettings_PaymentMethods_Edit,
      {
        lookups: [
          this.loadAccountStdsDict(),
          this.loadSysPaymentTypes(),
          this.loadPaymentInformation(),
        ],
      }
    );
    this.setControllerVisibility();
  }

  private loadAccountStdsDict(): Observable<IAccountNumberNameDTO[]> {
    return this.accountService
      .getAccountStdsNameNumber(false, +TermGroup_AccountType.Asset)
      .pipe(tap(x => (this.accountStds = x)));
  }

  private loadSysPaymentTypes(): Observable<SmallGenericType[]> {
    return this.service
      .getSysPaymentMethodsDict(true)
      .pipe(tap(x => (this.sysPaymentMethods = x)));
  }

  private loadPaymentInformation(): Observable<SmallGenericType[]> {
    return this.service
      .getPaymentInformationViewsDict(true)
      .pipe(tap(x => (this.paymentInformation = x)));
  }

  override onFinished(): void {
    this.setControllerVisibility();
  }

  sysPaymentMethodChanged(value: number) {
    this.setControllerVisibility();
  }

  setControllerVisibility() {
    this.canVisibleTransactionCode.set(
      this.form?.value.sysPaymentMethodId == 13
    );
  }
}
