import {
  Component,
  EventEmitter,
  inject,
  Input,
  OnDestroy,
  OnInit,
  Output,
} from '@angular/core';
import { ExtSupplierDTO } from '../../models/extended-supplier-dto.model';
import { ChangeViewStatusGridViewBalanceDTO } from '@shared/models/change-status-grid-view-balance-dto.model';
import { Observable, Subject, takeUntil, tap } from 'rxjs';
import { TranslateService } from '@ngx-translate/core';

@Component({
    selector: 'soe-supplier-central-header',
    templateUrl: './supplier-central-header.component.html',
    styleUrl: './supplier-central-header.component.scss',
    standalone: false
})
export class SupplierCentralHeaderComponent implements OnInit, OnDestroy {
  translate = inject(TranslateService);

  unsubscribe = new Subject<void>();
  supplier: ExtSupplierDTO = new ExtSupplierDTO();
  supplierInvoiceStatusForeignPermission = false;
  // Data
  supplierNumber!: string;
  supplierName: string = '';

  supplierPaymentsSupplierCentralUnpayed: number = 0;
  supplierPaymentsSupplierCentralUnpayedExVat: number = 0;
  supplierInvoicesOverdue: number = 0;
  supplierInvoicesOverdueExVat: number = 0;
  supplierPaymentsSupplierCentralPayed: number = 0;
  supplierPaymentsSupplierCentralPayedExVat: number = 0;
  supplierInvoicesAmountTotal: number = 0;
  supplierInvoicesAmountTotalExVat: number = 0;

  supplierPaymentsSupplierCentralUnpayedForeign: number = 0;
  supplierPaymentsSupplierCentralUnpayedForeignExVat: number = 0;
  supplierInvoicesOverdueForeign: number = 0;
  supplierInvoicesOverdueForeignExVat: number = 0;
  supplierPaymentsSupplierCentralPayedForeign: number = 0;
  supplierPaymentsSupplierCentralPayedForeignExVat: number = 0;
  supplierInvoicesForeignAmountTotal: number = 0;
  supplierInvoicesForeignAmountTotalExVat: number = 0;

  @Input() supplierCentralCountersAndBalances!: Observable<
    ChangeViewStatusGridViewBalanceDTO[]
  >;
  @Input() supplierInput!: Observable<ExtSupplierDTO>;
  @Input() supplierInvoiceStatusForeignPermissionInput!: Observable<boolean>;

  @Output() openSupplierEdit = new EventEmitter<ExtSupplierDTO>();

  ngOnInit(): void {
    this.supplierCentralCountersAndBalances
      .pipe(
        takeUntil(this.unsubscribe),
        tap(data => {
          if (!data || data.length < 1) return;
          this.supplierPaymentsSupplierCentralUnpayed = data[0].balanceTotal;
          this.supplierPaymentsSupplierCentralUnpayedExVat =
            data[0].balanceExVat;
          this.supplierInvoicesOverdue = data[2].balanceTotal;
          this.supplierInvoicesOverdueExVat = data[2].balanceExVat;
          this.supplierPaymentsSupplierCentralPayed = data[4].balanceTotal;
          this.supplierPaymentsSupplierCentralPayedExVat = data[4].balanceExVat;
          this.supplierInvoicesAmountTotal =
            data[0].balanceTotal + data[4].balanceTotal;
          this.supplierInvoicesAmountTotalExVat =
            data[0].balanceExVat + data[4].balanceExVat;

          this.supplierPaymentsSupplierCentralUnpayedForeign =
            data[1].balanceTotal;
          this.supplierPaymentsSupplierCentralUnpayedForeignExVat =
            data[1].balanceExVat;
          this.supplierInvoicesOverdueForeign = data[3].balanceTotal;
          this.supplierInvoicesOverdueForeignExVat = data[3].balanceExVat;
          this.supplierPaymentsSupplierCentralPayedForeign =
            data[5].balanceTotal;
          this.supplierPaymentsSupplierCentralPayedForeignExVat =
            data[5].balanceExVat;
          this.supplierInvoicesForeignAmountTotal =
            data[1].balanceTotal + data[5].balanceTotal;
          this.supplierInvoicesForeignAmountTotalExVat =
            data[1].balanceExVat + data[5].balanceExVat;
        })
      )
      .subscribe();

    this.supplierInput
      .pipe(
        takeUntil(this.unsubscribe),
        tap(supplier => {
          this.supplierNumber = supplier.supplierNr;
          this.supplierName = supplier.name;
          this.supplier = supplier;
        })
      )
      .subscribe();

    this.supplierInvoiceStatusForeignPermissionInput
      .pipe(
        takeUntil(this.unsubscribe),
        tap(permission => {
          this.supplierInvoiceStatusForeignPermission = permission;
        })
      )
      .subscribe();
  }

  openSupplier() {
    this.openSupplierEdit.emit(this.supplier);
  }

  ngOnDestroy(): void {
    this.unsubscribe.next();
    this.unsubscribe.complete();
  }
}
