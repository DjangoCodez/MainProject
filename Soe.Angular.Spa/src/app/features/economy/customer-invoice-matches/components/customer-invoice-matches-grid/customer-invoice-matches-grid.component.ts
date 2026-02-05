import { Component, inject, OnInit } from '@angular/core';
import { BrowserUtil } from '@shared/util/browser-util';
import { IInvoiceMatchingDTO } from '@shared/models/generated-interfaces/InvoiceMatchingDTO';
import {
  Feature,
  OrderInvoiceRegistrationType,
  SoeOriginStatusClassificationGroup,
  SoeOriginType,
  TermGroup_BillingType,
} from '@shared/models/generated-interfaces/Enumerations';
import { Observable,tap } from 'rxjs';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { TermCollection } from '@shared/localization/term-types';
import { ActorInvoiceMatchesGridBaseDirective } from '@features/economy/shared/actor-invoice-matches/directives/actor-invoice-matches-grid-base.directive';
import { CommonCustomerService } from '@billing/shared/services/common-customer.service';


@Component({
  selector: 'soe-customer-invoice-matches-grid',
  standalone: false,
  templateUrl: './customer-invoice-matches-grid.component.html'
})

export class CustomerInvoiceMatchesGridComponent
  extends ActorInvoiceMatchesGridBaseDirective
  implements OnInit
{
  private readonly commonCustomerService = inject(CommonCustomerService);

  constructor() {
    const gridColumnLabels: Record<
      'actorName' | 'invoiceNr' | 'paymentNr' | 'amount' | 'date',
      string
    > = {
      actorName: 'common.customer',
      invoiceNr: 'economy.customer.invoice.matches.invoicenr',
      paymentNr: 'economy.customer.invoice.matches.paymentnr',
      amount: 'economy.customer.invoice.matches.amount',
      date: 'economy.customer.invoice.matches.date',
    };

    super(
      Feature.Economy_Customer_Invoice_Matches,
      'economy.customer.invoice.matches.matches',
      SoeOriginType.CustomerInvoice,
      'common.customer',
      'economy.customer.invoice.matches.matches',
      gridColumnLabels
    );
  }

  override loadTerms(): Observable<TermCollection> {
    const translationsKeys = [
      "common.customer",
      "common.customer.invoices.invoicedate",
      "common.customer.invoices.paydate",
      "economy.customer.invoice.matches.date",
      "common.customer.invoices.customerinvoice",
      "economy.customer.invoice.matches.invoicenr",
      "economy.customer.invoice.matches.paymentnr",
      "economy.customer.invoice.matches.amount",
      "economy.customer.invoice.matches.showpayment",
      "economy.customer.invoice.matches.showinvoice",
      "economy.customer.invoice.matches.debitinvoice",
      "economy.customer.invoice.matches.creditinvoice",
      "economy.customer.invoice.matches.interestinvoice",
      "economy.customer.invoice.matches.demandinvoice",
      "economy.customer.invoice.matches.payment",
      "economy.customer.invoice.matches.paymentsuggestion"
    ];

    return super.loadTerms(translationsKeys);
  }

  protected override loadActors(): Observable<SmallGenericType[]> {
    return this.commonCustomerService.getCustomersDict(false, false, true).pipe(
      tap(x => {
        if (x) {
          this.actors = x;
        }
      })
    );
  }

  protected override setTypes(): void {
    this.types = [
      { id: 0, name: '' },
      {
        id: 1,
        name: this.terms['economy.customer.invoice.matches.debitinvoice'],
      },
      {
        id: 2,
        name: this.terms['economy.customer.invoice.matches.creditinvoice'],
      },
      {
        id: 3,
        name: this.terms['economy.customer.invoice.matches.interestinvoice'],
      },
      {
        id: 4,
        name: this.terms['economy.customer.invoice.matches.demandinvoice'],
      },
      { id: 5, name: this.terms['economy.customer.invoice.matches.payment'] },
    ];

    const suggestionText = ` (${this.terms['economy.customer.invoice.matches.paymentsuggestion']})`;
    const typesGridFilterItems = [
      {
        id: 1,
        name: this.terms['economy.customer.invoice.matches.debitinvoice'],
      },
      {
        id: 2,
        name:
          this.terms['economy.customer.invoice.matches.debitinvoice'] +
          suggestionText,
      },
      {
        id: 3,
        name: this.terms['economy.customer.invoice.matches.creditinvoice'],
      },
      {
        id: 4,
        name:
          this.terms['economy.customer.invoice.matches.creditinvoice'] +
          suggestionText,
      },
      {
        id: 5,
        name: this.terms['economy.customer.invoice.matches.interestinvoice'],
      },
      {
        id: 6,
        name:
          this.terms['economy.customer.invoice.matches.interestinvoice'] +
          suggestionText,
      },
      {
        id: 7,
        name: this.terms['economy.customer.invoice.matches.demandinvoice'],
      },
      {
        id: 8,
        name:
          this.terms['economy.customer.invoice.matches.demandinvoice'] +
          suggestionText,
      },
      { id: 9, name: this.terms['economy.customer.invoice.matches.payment'] },
    ];

    this.setTypesGridFilterItems(typesGridFilterItems);
  }


  protected override setTypeName(row: IInvoiceMatchingDTO) {
    if (
      row.type === SoeOriginType.CustomerInvoice
    ) {
      if (row.billingType === TermGroup_BillingType.Credit) {
        row.typeName =
          this.terms['economy.customer.invoice.matches.creditinvoice'];
      } else if (row.billingType === TermGroup_BillingType.Debit) {
        row.typeName =
          this.terms['economy.customer.invoice.matches.debitinvoice'];
      } else if (row.billingType === TermGroup_BillingType.Interest) {
        row.typeName =
          this.terms['economy.customer.invoice.matches.interestinvoice'];
      } else if (row.billingType === TermGroup_BillingType.Reminder) {
        row.typeName =
          this.terms['economy.customer.invoice.matches.demandinvoice'];
      }

      if (row.typeName && !row.isEditable) {
        row.typeName += ` (${this.terms['economy.customer.invoice.matches.paymentsuggestion']})`;
      }
    } else {
      row.typeName = this.terms['economy.customer.invoice.matches.payment'];
    }

  }

  protected override setAmount(row: IInvoiceMatchingDTO): void {
    if (row.type === SoeOriginType.CustomerPayment) {
      row.amount = row.invoicePayedAmount;
    } else {
      row.amount = row.invoiceTotalAmount;
    }
  }

  protected override openInvoice(row: IInvoiceMatchingDTO): void {

    let url: string;
    if (row.type === SoeOriginType.CustomerInvoice) {

      if (row.registrationType === OrderInvoiceRegistrationType.Ledger) {

        url =
        `/soe/economy/customer/invoice/status/default.aspx?` +
        `classificationgroup=${SoeOriginStatusClassificationGroup.HandleCustomerInvoices}` +
        `&invoiceId=${row.invoiceId}&invoiceNr=${row.invoiceNr}`;
      } else {

        url =
        `/soe/billing/invoice/status/default.aspx?` +
        `classificationgroup=${SoeOriginStatusClassificationGroup.HandleCustomerInvoices}` +
        `&invoiceId=${row.invoiceId}&invoiceNr=${row.invoiceNr}`;
      }

    } else {

      url =
        `/soe/economy/customer/invoice/status/default.aspx?` +
        `classificationgroup=${SoeOriginStatusClassificationGroup.HandleCustomerPayments}` +
        `&paymentId=${row.paymentRowId}&seqNr=${row.paymentNr}`;
 
    }

    BrowserUtil.openInNewTab(window, url);
  }

}
