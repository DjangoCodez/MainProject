import {
  Component,
  inject,
  Injector,
  input,
  OnInit,
  output,
  signal,
} from '@angular/core';
import { EconomyService } from '@features/economy/services/economy.service';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { ValidationHandler } from '@shared/handlers';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { ISearchVoucherRowDTO } from '@shared/models/generated-interfaces/SearchVoucherRowDTO';
import { IAccountDimSmallDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { BrowserUtil } from '@shared/util/browser-util';
import { Perform } from '@shared/util/perform.class';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable, of, take, tap } from 'rxjs';
import { VoucherSearchSummaryForm } from '../../models/voucher-search-total-form.model';
import {
  SearchVoucherFilterDTO,
  VoucherSearchSummaryDTO,
} from '../../models/voucher-search.model';
import { VoucherSearchService } from '../../services/voucher-search.service';
import { TwoValueCellRenderer } from '@ui/grid/cell-renderers/two-value-cell-renderer/two-value-cell-renderer.component';

@Component({
  selector: 'soe-voucher-search-grid',
  templateUrl: './voucher-search-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class VoucherSearchGridComponent
  extends GridBaseDirective<ISearchVoucherRowDTO>
  implements OnInit
{
  isDialog = input(false);
  closeDialog = output<ISearchVoucherRowDTO>();

  protected readonly injector = Injector.create({
    providers: [{ provide: ToolbarService, useClass: ToolbarService }],
  });
  protected readonly _toolbarService = this.injector.get(ToolbarService);
  private readonly progress = inject(ProgressService);
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  private readonly performLoad = new Perform<any>(this.progress);
  private validationHandler = inject(ValidationHandler);
  protected readonly economyService = inject(EconomyService);
  voucherService = inject(VoucherSearchService);
  private filter?: SearchVoucherFilterDTO;

  protected accountDimsFrom = signal<IAccountDimSmallDTO[]>([]);

  protected summaryForm = new VoucherSearchSummaryForm({
    validationHandler: this.validationHandler,
    element: new VoucherSearchSummaryDTO(),
  });

  override ngOnInit(): void {
    super.ngOnInit();
    this.summaryForm.disable();

    this.startFlow(
      Feature.Economy_Accounting_Vouchers,
      'Economy.Accounting.VoucherSearch',
      {
        skipInitialLoad: true,
        lookups: [this.loadAccountDims()],
      }
    );
  }

  override onFinished(): void {
    this.createPageToolbar();
  }

  private createPageToolbar(): void {
    this._toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton('newVoucher', {
          caption: signal('economy.accounting.voucher.new'),
          tooltip: signal('economy.accounting.voucher.new'),
          iconName: signal('plus'),
          onAction: () => {
            BrowserUtil.openInNewTab(
              window,
              `/soe/economy/accounting/vouchers/default.aspx?new=true`
            );
          },
        }),
      ],
    });
  }

  override onGridReadyToDefine(
    grid: GridComponent<ISearchVoucherRowDTO>
  ): void {
    super.onGridReadyToDefine(grid);
    this.translate
      .get([
        'economy.accounting.vouchersearch.vouchernumber',
        'common.date',
        'common.text',
        'economy.accounting.vouchersearch.debet',
        'economy.accounting.vouchersearch.credit',
        'economy.accounting.vouchersearch.createddate',
        'economy.accounting.vouchersearch.selectuser',
        'core.edit',
        'economy.accounting.voucher.voucher',
        'economy.accounting.voucherseriestype',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        const defaultOptions = {
          flex: 1,
          enableHiding: true,
          enableGrouping: true,
        };
        this.grid.enableRowSelection(() => true, false);
        this.grid.addColumnNumber(
          'voucherNr',
          terms['economy.accounting.vouchersearch.vouchernumber'],
          {
            ...defaultOptions,
            sort: 'desc',
          }
        );
        this.grid.addColumnDate(
          'voucherDate',
          terms['common.date'],
          defaultOptions
        );
        this.grid.addColumnText(
          'voucherText',
          terms['common.text'],
          defaultOptions
        );
        this.grid.addColumnText(
          'voucherSeriesName',
          terms['economy.accounting.voucherseriestype'],
          defaultOptions
        );

        this.accountDimsFrom().forEach(ad => {
          this.grid.addColumnText(
            'dim' + ad.accountDimNr + 'AccountName',
            ad.name,
            {
              ...defaultOptions,
              cellRenderer: TwoValueCellRenderer,
              cellRendererParams: {
                primaryValueKey: `dim${ad.accountDimNr}AccountNr`,
                secondaryValueKey: `dim${ad.accountDimNr}AccountName`,
              },
            }
          );
        });

        this.grid.addColumnNumber(
          'debit',
          terms['economy.accounting.vouchersearch.debet'],
          {
            ...defaultOptions,
            decimals: 2,
            aggFuncOnGrouping: 'sum',
          }
        );
        this.grid.addColumnNumber(
          'credit',
          terms['economy.accounting.vouchersearch.credit'],
          {
            ...defaultOptions,
            decimals: 2,
            aggFuncOnGrouping: 'sum',
          }
        );
        this.grid.addColumnDate(
          'created',
          terms['economy.accounting.vouchersearch.createddate'],
          defaultOptions
        );
        this.grid.addColumnText(
          'createdBy',
          terms['economy.accounting.vouchersearch.selectuser'],
          defaultOptions
        );
        this.grid.addColumnIconEdit({
          tooltip: terms['core.edit'],
          onClick: row => this.edit(row),
        });
        this.grid.addGroupTimeSpanSumAggFunction(true);
        this.grid.useGrouping({
          includeFooter: true,
          includeTotalFooter: true,
        });
        super.finalizeInitGrid();
      });
  }

  override edit(row: ISearchVoucherRowDTO): void {
    if (this.isDialog()) {
      this.closeDialog.emit(row);
    } else {
      this.openVoucher(row);
    }
  }

  private openVoucher(row: ISearchVoucherRowDTO) {
    if (row && row.voucherHeadId && row.voucherHeadId != null)
      BrowserUtil.openInNewTab(
        window,
        `/soe/economy/accounting/vouchers/default.aspx?voucherHeadId=${row.voucherHeadId}&voucherNr=${row.voucherNr}`
      );
  }

  private loadAccountDims(): Observable<void> {
    return this.performLoad.load$(
      this.economyService
        .getAccountDimsSmall(
          false,
          false,
          true,
          true,
          false,
          false,
          false,
          false,
          false
        )
        .pipe(
          tap(dims => {
            this.accountDimsFrom.set(dims ?? []);
          })
        )
    );
  }

  protected searchVouchers(filter: SearchVoucherFilterDTO): void {
    this.filter = filter;
    this.loadData().subscribe();
  }

  override loadData(id?: number): Observable<ISearchVoucherRowDTO[]> {
    if (!this.filter) return of([]);

    if (this.filter.isCredit && this.filter.isDebit) {
      this.filter.creditFrom = this.filter.debitFrom = this.filter.amountFrom;
      this.filter.creditTo = this.filter.debitTo = this.filter.amountTo;

      this.filter.amountFrom = 0;
      this.filter.amountTo = 0;
    } else if (this.filter.isCredit) {
      this.filter.creditFrom = this.filter.amountFrom;
      this.filter.creditTo = this.filter.amountTo;

      this.filter.amountFrom = 0;
      this.filter.amountTo = 0;
    } else if (this.filter.isDebit) {
      this.filter.debitFrom = this.filter.amountFrom;
      this.filter.debitTo = this.filter.amountTo;

      this.filter.amountFrom = 0;
      this.filter.amountTo = 0;
    } else {
      if (this.filter.amountFrom < 0) {
        this.filter.creditFrom = this.filter.amountFrom;
        this.filter.amountFrom = 0;
      }

      if (this.filter.amountTo < 0) {
        this.filter.creditTo = this.filter.amountTo;
        this.filter.amountTo = 0;
      }
    }

    return this.performLoad.load$(
      this.voucherService.getSearchedVoucherRows(this.filter).pipe(
        tap(data => {
          this.notifyGridDataLoaded(data);
          this.summarize(data);
          if (this.isDialog()) this.rowData.next(data);
        })
      )
    );
  }

  override selectionChanged(rows: ISearchVoucherRowDTO[]) {
    this.summarize(rows, true);
  }

  private summarize(rows: ISearchVoucherRowDTO[], isSelected = false): void {
    let totalCredit = 0;
    let totalDebit = 0;
    rows.forEach((y: ISearchVoucherRowDTO) => {
      totalCredit += y.credit;
      totalDebit += y.debit;
    });
    totalCredit = Math.abs(totalCredit);
    totalDebit = Math.abs(totalDebit);
    const totalBalance = totalDebit - totalCredit;
    this.summaryForm.enable();
    if (isSelected) {
      this.summaryForm.patchValue({
        creditTotalSelected: totalCredit,
        debitTotalSelected: totalDebit,
        balanceSelected: totalBalance,
      });
    } else {
      this.summaryForm.patchValue({
        debitTotal: totalDebit,
        creditTotal: totalCredit,
        balance: totalBalance,
      });
    }
    this.summaryForm.disable();
  }
}
