import { Component, inject, OnInit } from '@angular/core';
import { BrowserUtil } from '@shared/util/browser-util';
import { IInvoiceMatchingDTO } from '@shared/models/generated-interfaces/InvoiceMatchingDTO';
import {
  Feature,
  SoeOriginStatusClassificationGroup,
  SoeOriginType,
  TermGroup_BillingType,
} from '@shared/models/generated-interfaces/Enumerations';
import { Observable,tap } from 'rxjs';
import { SupplierService } from '@features/economy/services/supplier.service';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { TermCollection } from '@shared/localization/term-types';
import { ActorInvoiceMatchesGridBaseDirective } from '@features/economy/shared/actor-invoice-matches/directives/actor-invoice-matches-grid-base.directive';

@Component({
  selector: 'soe-supplier-invoice-matches-grid',
  standalone: false,
  templateUrl:
    './supplier-invoice-matches-grid.component.html',
})
export class SupplierInvoiceMatchesGridComponent
  extends ActorInvoiceMatchesGridBaseDirective
  implements OnInit
{
  private readonly supplierService = inject(SupplierService);

  constructor() {
    const gridColumnLabels: Record<
      'actorName' | 'invoiceNr' | 'paymentNr' | 'amount' | 'date',
      string
    > = {
      actorName: 'economy.supplier.supplier.supplier',
      invoiceNr: 'economy.supplier.invoice.matches.invoicenr',
      paymentNr: 'economy.supplier.invoice.matches.paymentnr',
      amount: 'economy.supplier.invoice.matches.amount',
      date: 'economy.supplier.invoice.invoicedate',
    };

    super(
      Feature.Economy_Supplier_Invoice_Matches,
      'economy.supplier.invoice.matches.matches',
      SoeOriginType.SupplierInvoice,
      'economy.supplier.supplier.supplier',
      'economy.supplier.invoice.matches.matches',
      gridColumnLabels
    );
  }

  override loadTerms(): Observable<TermCollection> {
    const translationsKeys = [
      'economy.supplier.supplier.supplier',
      'economy.supplier.invoice.invoice',
      'economy.supplier.invoice.invoicedate',
      'economy.supplier.invoice.matches.invoicenr',
      'economy.supplier.invoice.matches.paymentnr',
      'economy.supplier.invoice.matches.amount',
      'economy.supplier.invoice.matches.showpayment',
      'economy.supplier.invoice.matches.showinvoice',
      'economy.supplier.invoice.matches.debitinvoice',
      'economy.supplier.invoice.matches.creditinvoice',
      'economy.supplier.invoice.matches.interestinvoice',
      'economy.supplier.invoice.matches.demandinvoice',
      'economy.supplier.invoice.matches.payment',
      'economy.supplier.invoice.matches.paymentsuggestion',
    ];

    return super.loadTerms(translationsKeys);
  }

  protected override loadActors(): Observable<SmallGenericType[]> {
    return this.supplierService.getSupplierDict(false, false, true).pipe(
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
        name: this.terms['economy.supplier.invoice.matches.debitinvoice'],
      },
      {
        id: 2,
        name: this.terms['economy.supplier.invoice.matches.creditinvoice'],
      },
      {
        id: 3,
        name: this.terms['economy.supplier.invoice.matches.interestinvoice'],
      },
      {
        id: 4,
        name: this.terms['economy.supplier.invoice.matches.demandinvoice'],
      },
      { id: 5, name: this.terms['economy.supplier.invoice.matches.payment'] },
    ];

    const suggestionText = ` (${this.terms['economy.supplier.invoice.matches.paymentsuggestion']})`;
    const typesGridFilterItems = [
      {
        id: 1,
        name: this.terms['economy.supplier.invoice.matches.debitinvoice'],
      },
      {
        id: 2,
        name:
          this.terms['economy.supplier.invoice.matches.debitinvoice'] +
          suggestionText,
      },
      {
        id: 3,
        name: this.terms['economy.supplier.invoice.matches.creditinvoice'],
      },
      {
        id: 4,
        name:
          this.terms['economy.supplier.invoice.matches.creditinvoice'] +
          suggestionText,
      },
      {
        id: 5,
        name: this.terms['economy.supplier.invoice.matches.interestinvoice'],
      },
      {
        id: 6,
        name:
          this.terms['economy.supplier.invoice.matches.interestinvoice'] +
          suggestionText,
      },
      {
        id: 7,
        name: this.terms['economy.supplier.invoice.matches.demandinvoice'],
      },
      {
        id: 8,
        name:
          this.terms['economy.supplier.invoice.matches.demandinvoice'] +
          suggestionText,
      },
      { id: 9, name: this.terms['economy.supplier.invoice.matches.payment'] },
    ];

    this.setTypesGridFilterItems(typesGridFilterItems);
  }

  protected override setTypeName(row: IInvoiceMatchingDTO) {
    if (
      row.type === SoeOriginType.CustomerInvoice ||
      row.type === SoeOriginType.SupplierInvoice
    ) {
      if (row.billingType === TermGroup_BillingType.Credit) {
        row.typeName =
          this.terms['economy.supplier.invoice.matches.creditinvoice'];
      } else if (row.billingType === TermGroup_BillingType.Debit) {
        row.typeName =
          this.terms['economy.supplier.invoice.matches.debitinvoice'];
      } else if (row.billingType === TermGroup_BillingType.Interest) {
        row.typeName =
          this.terms['economy.supplier.invoice.matches.interestinvoice'];
      } else if (row.billingType === TermGroup_BillingType.Reminder) {
        row.typeName =
          this.terms['economy.supplier.invoice.matches.demandinvoice'];
      }

      if (row.typeName && !row.isEditable) {
        row.typeName += ` (${this.terms['economy.supplier.invoice.matches.paymentsuggestion']})`;
      }
    } else {
      row.typeName = this.terms['economy.supplier.invoice.matches.payment'];
    }

  }

  protected override setAmount(row: IInvoiceMatchingDTO): void {
    row.amount = row.invoiceTotalAmount;
  }

  protected override openInvoice(row: IInvoiceMatchingDTO): void {
    let url: string;
    if (row.type === SoeOriginType.SupplierInvoice) {
      url =
        `/soe/economy/supplier/invoice/status/default.aspx?` +
        `classificationgroup=${SoeOriginStatusClassificationGroup.HandleSupplierInvoices}` +
        `&invoiceId=${row.invoiceId}&invoiceNr=${row.invoiceNr}`;
    } else {
      url =
        `/soe/economy/supplier/invoice/status/default.aspx?` +
        `classificationgroup=${SoeOriginStatusClassificationGroup.HandleSupplierPayments}` +
        `&paymentId=${row.paymentRowId}`;
    }

    BrowserUtil.openInNewTab(window, url);
  }
}
