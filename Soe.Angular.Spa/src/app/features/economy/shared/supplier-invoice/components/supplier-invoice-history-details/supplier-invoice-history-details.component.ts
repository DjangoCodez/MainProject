import {
  Component,
  computed,
  DestroyRef,
  inject,
  OnInit,
  signal,
} from '@angular/core';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { combineLatest, debounceTime, Observable, tap } from 'rxjs';
import { AutoHeightService } from '@shared/directives/auto-height/auto-height.service';
import { SupplierInvoiceForm } from '../../models/supplier-invoice-form.model';
import { SupplierInvoiceService } from '../../services/supplier-invoice.service';
import { SupplierInvoiceDTO } from '../../models/supplier-invoice.model';
import { InvoiceIds } from '../../models/utility-models';
import { CurrencyService } from '@shared/services/currency.service';
import { InvoicePaymentConditionService } from '../../domain-services/payment-condition.service';
import { InvoiceVatService } from '../../domain-services/vat.service';
import { IAccountingRowDTO } from '@shared/models/generated-interfaces/AccountingRowDTO';
import { AccountingRowDTO } from '@shared/components/accounting-rows/models/accounting-rows-model';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { EditComponentDialogData } from '@ui/dialog/edit-component-dialog/edit-component-dialog.component';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

@Component({
  selector: 'soe-supplier-invoice-history-details',
  templateUrl: './supplier-invoice-history-details.component.html',
  providers: [
    FlowHandlerService,
    ToolbarService,
    AutoHeightService,
    CurrencyService,
    InvoicePaymentConditionService,
    InvoiceVatService,
  ],
  standalone: false,
})
export class SupplierInvoiceHistoryDetailsComponent
  extends EditBaseDirective<
    SupplierInvoiceDTO,
    SupplierInvoiceService,
    SupplierInvoiceForm
  >
  implements OnInit
{
  service = inject(SupplierInvoiceService);
  data: EditComponentDialogData<
    SupplierInvoiceDTO,
    SupplierInvoiceService,
    SupplierInvoiceForm,
    number[]
  > = inject(MAT_DIALOG_DATA);

  private readonly destroyRef = inject(DestroyRef);

  ids = signal<InvoiceIds>({});
  otherIds = signal<number[]>([]);
  hasOtherIds = computed(() => this.otherIds().length > 1);

  ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(Feature.Economy_Supplier_Invoice, {
      lookups: [this.loadTerms([])],
    });
    this.idSubscription();
    this.otherIds.set(this.data?.parameters ?? []);
  }

  // Subscriptions
  private idSubscription() {
    if (!this.form) return;
    combineLatest({
      invoiceId: this.form.invoiceId.valueChanges,
      supplierId: this.form.actorId.valueChanges,
      ediEntryId: this.form.ediEntryId.valueChanges,
      scanningEntryId: this.form.scanningEntryId.valueChanges,
    })
      .pipe(debounceTime(0), takeUntilDestroyed(this.destroyRef))
      .subscribe(ids => {
        this.ids.set(ids);
      });
  }

  goToNextInvoice() {
    const currentInvoiceId = this.form?.invoiceId.value;
    const currentIdx = this.otherIds().indexOf(currentInvoiceId) ?? -1;
    const next =
      currentIdx < 0 || currentIdx + 1 >= this.otherIds().length
        ? this.otherIds()[0]
        : this.otherIds()[currentIdx + 1];
    this.form?.patchValue({ invoiceId: next });
    this.loadData().subscribe();
  }

  goToPreviousInvoice() {
    const currentInvoiceId = this.form?.invoiceId.value;
    const currentIdx = this.otherIds().indexOf(currentInvoiceId) ?? -1;
    const previous =
      currentIdx < 1
        ? this.otherIds()[this.otherIds().length - 1]
        : this.otherIds()[currentIdx - 1];
    this.form?.patchValue({ invoiceId: previous });
    this.loadData().subscribe();
  }

  copyAccountingRows() {
    const accountingRowsToCopy: AccountingRowDTO[] = [];
    this.form?.accountingRows.value.forEach((row: AccountingRowDTO, i) => {
      if (i === 0) return; // Skip first row
      if (row.isVatRow || row.isContractorVatRow) return; // Skip VAT rows
      accountingRowsToCopy.push(row);
    });
    this.triggerCloseDialog(accountingRowsToCopy as any);
  }

  override loadData(): Observable<void> {
    const invoiceId = this.form?.invoiceId.value;
    return this.performLoadData.load$(
      this.service.get(invoiceId).pipe(
        tap(invoice => {
          const accountingRows: IAccountingRowDTO[] = [];
          invoice.supplierInvoiceRows.forEach(row =>
            accountingRows.push(...row.accountingRows)
          );
          this.form?.reset(invoice);
          this.form?.patchAccountingRows(accountingRows as AccountingRowDTO[]);
          this.form?.markAsPristine();
        })
      )
    );
  }
}
