import { Component, OnInit, inject } from '@angular/core';
import {
  ActionTaken,
  EditBaseDirective,
} from '@shared/directives/edit-base/edit-base.directive';
import { CrudActionTypeEnum } from '@shared/enums';
import { TermCollection } from '@shared/localization/term-types';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  Feature,
  TermGroup,
  TermGroup_BillingType,
} from '@shared/models/generated-interfaces/Enumerations';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { BrowserUtil } from '@shared/util/browser-util';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { DirectDebitForm } from '../../models/direct-debit-form.model';
import {
  InvoiceExportDTO,
  InvoiceExportIODTO,
} from '../../models/direct-debit.model';
import { DirectDebitService } from '../../services/direct-debit.service';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';
import { ResponseUtil } from '@shared/util/response-util';

@Component({
  selector: 'soe-direct-debit-edit',
  templateUrl: './direct-debit-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class DirectDebitEditComponent
  extends EditBaseDirective<
    InvoiceExportDTO,
    DirectDebitService,
    DirectDebitForm
  >
  implements OnInit
{
  rows = new BehaviorSubject<InvoiceExportIODTO[]>([]);
  selectedInvoices: InvoiceExportIODTO[] = [];

  service = inject(DirectDebitService);
  messageboxService = inject(MessageboxService);
  coreService = inject(CoreService);
  paymentServices: SmallGenericType[] = [];
  terms: TermCollection = {};
  showCreateTemplate: boolean = false;

  ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(Feature.Economy_Export_Payments, {
      lookups: [this.loadPaymentService()],
    });
    this.recordConfig.hideRecordNavigator = true;
  }

  getSelectedInvoices(invoices: InvoiceExportIODTO[]) {
    this.selectedInvoices = invoices;
  }

  export(): void {
    if (this.selectedInvoices.filter(i => !i.bankAccount).length > 0) {
      const message = this.terms['economy.export.payments.bankaccountmissing'];
      this.messageboxService.error(message, message);
      return;
    }
    return this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.service.saveCustomerPaymentService(
        this.selectedInvoices,
        this.form?.paymentServiceId.value
      ),
      (res: BackendResponse) => {
        const url: string =
          '?c=' +
          SoeConfigUtil.actorCompanyId +
          '&r=' +
          SoeConfigUtil.roleId +
          '&filename=' +
          ResponseUtil.getStringValue(res);
        BrowserUtil.openInNewTab(window, url);

        this.refreshGrid(ResponseUtil.getNumberValue(res));
      }
    );
  }

  private refreshGrid(invoiceExportId: number): void {
    const action: ActionTaken = {
      rowItemId: invoiceExportId,
      ref: this.ref(),
      type: CrudActionTypeEnum.Save,
      form: this.form,
      additionalProps: this.additionalSaveProps,
    };

    if (this.service.getGrid) {
      action.updateGrid = () => {
        return this.service.getGrid(invoiceExportId);
      };
    }

    this.actionTakenSignal().set(action);
  }

  private loadPaymentService(): Observable<SmallGenericType[]> {
    return this.performLoadData.load$(
      this.coreService
        .getTermGroupContent(TermGroup.InvoicePaymentService, false, true)
        .pipe(
          tap(x => {
            this.paymentServices = x;
            this.form?.patchValue({ paymentServiceId: x[0].id });
            this.selectPaymentService(x[0].id);
          })
        )
    );
  }

  override loadData(): Observable<void> {
    return this.performLoadData.load$(
      this.service.get(this.form?.getIdControl()?.value).pipe(
        tap(value => {
          this.form?.patchValue(value[0]);
          this.rows.next(this.getInvoicesType(value));
        })
      )
    );
  }

  private getInvoicesType(invoices: InvoiceExportIODTO[]) {
    invoices.forEach(invoice => {
      switch (invoice.invoiceType) {
        case TermGroup_BillingType.Debit:
          invoice.invoiceTypeName = this.terms['common.debit'];
          break;
        case TermGroup_BillingType.Credit:
          invoice.invoiceTypeName = this.terms['common.credit'];
          break;
        case TermGroup_BillingType.Interest:
          invoice.invoiceTypeName =
            this.terms['economy.customer.invoice.matches.interestinvoice'];
          break;
        case TermGroup_BillingType.Reminder:
          invoice.invoiceTypeName =
            this.terms['economy.customer.invoice.matches.demandinvoice'];
          break;
      }
    });

    return invoices;
  }

  override loadTerms(): Observable<TermCollection> {
    return super.loadTerms([
      'common.debit',
      'common.credit',
      'economy.customer.invoice.matches.interestinvoice',
      'economy.customer.invoice.matches.demandinvoice',
      'economy.export.payments.bankaccountmissing',
    ]);
  }

  selectPaymentService(id: number) {
    this.showCreateTemplate = false;
    this.form?.markAsPristine();
    if (id > 0) this.showCreateTemplate = true;
  }

  createTemplate() {
    return this.performLoadData.load(
      this.service
        .getInvoicesForPaymentService(this.form?.paymentServiceId.value)
        .pipe(tap(x => this.rows.next(this.getInvoicesType(x))))
    );
  }
}
