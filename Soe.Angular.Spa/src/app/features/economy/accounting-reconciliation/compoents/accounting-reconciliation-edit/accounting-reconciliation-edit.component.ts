import { Component, inject, OnInit } from '@angular/core';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import {
  Feature,
  ReconciliationRowType,
  SoeOriginStatusClassificationGroup,
} from '@shared/models/generated-interfaces/Enumerations';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { AccountingReconciliationForm } from '../../models/accounting-reconciliation-form.model';
import { ReconciliationRowDTO } from '../../models/accounting-reconciliation.model';
import { AccountingReconciliationService } from '../../services/accounting-reconciliation.service';
import { BehaviorSubject, Observable, take, tap } from 'rxjs';
import { GridComponent } from '@ui/grid/grid.component';
import { GridResizeType } from '@ui/grid/enums/resize-type.enum';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { VoucherService } from '@features/economy/voucher/services/voucher.service';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { VoucherSeriesDTO } from '@features/economy/account-years-and-periods/models/account-years-and-periods.model';
import { TermCollection } from '@shared/localization/term-types';
import { VoucherEditComponent } from '@features/economy/voucher/components/voucher-edit/voucher-edit.component';
import { VoucherForm } from '@features/economy/voucher/models/voucher-form.model';
import { BrowserUtil } from '@shared/util/browser-util';

@Component({
  selector: 'soe-accounting-reconciliation-edit',
  templateUrl: './accounting-reconciliation-edit.component.html',
  styleUrl: './accounting-reconciliation-edit.component.scss',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class AccountingReconciliationEditComponent
  extends EditBaseDirective<
    ReconciliationRowDTO,
    AccountingReconciliationService,
    AccountingReconciliationForm
  >
  implements OnInit
{
  service = inject(AccountingReconciliationService);
  voucherService = inject(VoucherService);
  toolbarService = inject(ToolbarService);

  subGrid!: GridComponent<ReconciliationRowDTO>;
  gridRows = new BehaviorSubject<ReconciliationRowDTO[]>([]);
  voucherSeries: SmallGenericType[] = [];
  terms!: TermCollection;

  ngOnInit(): void {
    super.ngOnInit();

    this.flowHandler.execute({
      parentGuid: this.ref(),
      lookups: [this.loadVoucherSeries()],
      onFinished: () => this.loadData().subscribe(),
      permission: Feature.Economy_Accounting_Reconciliation,
      setupDefaultToolbar: this.createToolBar.bind(this),
      setupGrid: this.setupGrid.bind(this),
    });
  }

  override loadData() {
    return this.performLoadData.load$(
      this.service
        .getReconciliationPerAccount(
          this.form?.accountId.value,
          this.form?.fromDate.value,
          this.form?.toDate.value,
          this.form?.accountYearId.value
        )
        .pipe(
          tap(rows => {
            this.gridRows.next(rows);
            this.subGrid.resizeColumns(GridResizeType.ToFit);
          })
        )
    );
  }

  createToolBar() {
    const config = {
      clearFiltersOption: this.getDefaultClearFiltersOption(),
      reloadOption: this.getDefaultReloadOption(),
    };
    this.toolbarService.createDefaultGridToolbar(config);
  }

  setupGrid(grid: GridComponent<ReconciliationRowDTO>): void {
    this.subGrid = grid;
    this.translate
      .get([
        'common.type',
        'common.date',
        'economy.accounting.vatverification.vouchernumber',
        'economy.accounting.voucherseriestype',
        'common.name',
        'common.amount',
        'common.state',
        'core.edit',
        'economy.accounting.voucher.voucher',
        'economy.supplier.invoice.invoice',
        'economy.accounting.reconciliation.paymentamount',
        'economy.accounting.accountdistribution.customerinvoice',
        'economy.accounting.reconciliation.paymentinvoice',
        'common.green',
        'common.yellow',
        'common.red',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.terms = terms;

        this.subGrid.addColumnText('typeName', terms['common.type']);
        this.subGrid.addColumnText(
          'number',
          terms['economy.accounting.vatverification.vouchernumber']
        );
        this.subGrid.addColumnText(
          'voucherSeriesTypeName',
          terms['economy.accounting.voucherseriestype']
        );
        this.subGrid.addColumnText('name', terms['common.name']);
        this.subGrid.addColumnNumber('customerAmount', terms['common.amount'], {
          decimals: 2,
        }); //customerAmount using as amount in details grid (and therefor diffAmount, supplierAmount, paymentAmount is not used)
        this.subGrid.addColumnDate('date', terms['common.date']);
        this.subGrid.addColumnShape('attestStateColor', '', {
          shape: 'circle',
          colorField: 'attestStateColor',
        });
        this.subGrid.addColumnIconEdit({
          onClick: (row: ReconciliationRowDTO) => this.edit(row),
        });

        this.subGrid.finalizeInitGrid();
      });
  }

  private loadVoucherSeries(): Observable<VoucherSeriesDTO[]> {
    return this.voucherService
      .getVoucherSeriesByYear(this.form?.accountYearId.value, false)
      .pipe(
        tap(x => {
          x.forEach((y: VoucherSeriesDTO) => {
            this.voucherSeries.push({
              id: y.voucherSeriesId,
              name: y.voucherSeriesTypeName,
            });
          });
        })
      );
  }

  edit(row: ReconciliationRowDTO) {
    if (row.type == ReconciliationRowType.Voucher) {
      this.openEditInNewTab({
        id: row.associatedId,
        additionalProps: {
          editComponent: VoucherEditComponent,
          FormClass: VoucherForm,
          editTabLabel: `${row.typeName}`,
        },
      });
    } else if (row.type == ReconciliationRowType.CustomerInvoice) {
      BrowserUtil.openInNewTab(
        window,
        `/soe/economy/customer/invoice/status/default.aspx?invoiceId=${row.associatedId}`
      );
    } else if (row.type == ReconciliationRowType.SupplierInvoice) {
      BrowserUtil.openInNewTab(
        window,
        `/soe/economy/supplier/invoice/status/default.aspx?invoiceId=${row.associatedId}`
      );
    } else if (row.type == ReconciliationRowType.Payment) {
      BrowserUtil.openInNewTab(
        window,
        `/soe/economy/supplier/invoice/status/default.aspx?paymentId=${row.associatedId}&classificationgroup=${SoeOriginStatusClassificationGroup.HandleSupplierPayments}`
      );
    }
  }

  getDefaultReloadOption() {
    return {
      onAction: () => {
        this.loadData().subscribe(), this.subGrid.clearSelectedItems();
      },
    };
  }

  getDefaultClearFiltersOption() {
    return {
      onAction: () => this.subGrid?.clearFilters(),
    };
  }
}
