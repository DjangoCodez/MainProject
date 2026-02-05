import {
  Component,
  computed,
  DestroyRef,
  inject,
  input,
  OnInit,
  signal,
} from '@angular/core';
import { SupplierInvoiceForm } from '../../models/supplier-invoice-form.model';
import { AccountingRowDTO } from '@shared/components/accounting-rows/models/accounting-rows-model';
import { SupplierInvoiceSettingsService } from '../../services/supplier-invoice-settings.service';
import { CurrencyService } from '@shared/services/currency.service';
import { ToasterService } from '@ui/toaster/services/toaster.service';
import { debounceTime } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Guid } from '@shared/util/string-util';

@Component({
  selector: 'soe-supplier-invoice-accounting-rows',
  templateUrl: './supplier-invoice-accounting-rows.component.html',
  styleUrls: ['./supplier-invoice-accounting-rows.component.scss'],
  standalone: false,
})
export class SupplierInvoiceAccountingRowsComponent implements OnInit {
  currencyService = inject(CurrencyService);
  settingService = inject(SupplierInvoiceSettingsService);
  toasterService = inject(ToasterService);
  private readonly destroyRef = inject(DestroyRef);

  form = input.required<SupplierInvoiceForm>();
  isLocked = input(false);
  useSimplifiedView = input(false);

  accountingRows = signal<AccountingRowDTO[]>([]);
  accountingRowsHasBeenSet = computed(() => this.accountingRows().length > 0);

  ngOnInit() {
    this.setupRowSubscription();
  }

  setupRowSubscription() {
    const form = this.form();
    form.accountingRows.valueChanges
      .pipe(debounceTime(0), takeUntilDestroyed(this.destroyRef))
      .subscribe(rows => {
        if (!this.form().pristine) {
          // Only notify if there are existing rows.
          this.toasterService.info('Accounting rows regenerated', 'Info', {
            timeOut: 1000,
          });
        }
        this.accountingRows.set(rows);
      });
  }

  accountingRowsReady(guid: Guid) {
    // Handle the event when accounting rows are ready
  }

  accountingRowsChanged(rows: AccountingRowDTO[]) {
    this.form().patchAccountingRows(rows, false);
    // this.accountingRows.set(rows);
  }
}
