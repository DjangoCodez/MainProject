import { Component, inject, OnInit, signal, ViewChild } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { BrowserUtil } from '@shared/util/browser-util';
import { Perform } from '@shared/util/perform.class';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import { AccountingLiquidityPlanningService } from '../../services/accounting-liquidity-planning.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import {
  Feature,
  LiquidityPlanningTransactionType,
} from '@shared/models/generated-interfaces/Enumerations';
import { Observable, of, take, tap } from 'rxjs';
import { ILiquidityPlanningDTO } from '@shared/models/generated-interfaces/LiquidityPlanningDTO';
import {
  GetLiquidityPlanningModel,
  ILiquidityPlanningChartModel,
  LiquidityPlanningDialogData,
} from '../../models/liquidity-planning.model';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { GridComponent } from '@ui/grid/grid.component';
import { GridResizeType } from '@ui/grid/enums/resize-type.enum';
import { GroupDisplayType } from '@ui/grid/enums/grid-options.enum';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { ManualTransactionComponent } from '../manual-transaction/manual-transaction.component';
import { CrudActionTypeEnum } from '@shared/enums';
import { AccountingLiquidityPlanningGridFilterComponent } from '../accounting-liquidity-planning-grid-filter/accounting-liquidity-planning-grid-filter.component';
import { FormControl } from '@angular/forms';
import { ChartConfig } from '@ui/chart/interfaces/chart-type.interface';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Component({
  selector: 'soe-accounting-liquidity-planning-grid',
  templateUrl: './accounting-liquidity-planning-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class AccountingLiquidityPlanningGridComponent
  extends GridBaseDirective<
    ILiquidityPlanningDTO,
    AccountingLiquidityPlanningService
  >
  implements OnInit
{
  @ViewChild(AccountingLiquidityPlanningGridFilterComponent)
  filterRef!: AccountingLiquidityPlanningGridFilterComponent;
  liquidityPlanningChartConfig!: ChartConfig<ILiquidityPlanningChartModel>;
  service = inject(AccountingLiquidityPlanningService);
  private readonly dialogService = inject(DialogService);
  private readonly messageBoxService = inject(MessageboxService);
  private readonly performAction = new Perform<BackendResponse>(
    this.progressService
  );
  private readonly performLoad = new Perform<ILiquidityPlanningDTO[]>(
    this.progressService
  );

  enableGraph: boolean = false;
  valueIn: FormControl = new FormControl({ value: 0, disabled: true });
  valueOut: FormControl = new FormControl({ value: 0, disabled: true });
  totalIn: FormControl = new FormControl({ value: 0, disabled: true });
  totalOut: FormControl = new FormControl({ value: 0, disabled: true });

  ngOnInit(): void {
    this.startFlow(
      Feature.Economy_Accounting_LiquidityPlanning,
      'Economy.Accounting.LiquidityPlanning',
      {
        skipInitialLoad: true,
      }
    );
  }

  override createGridToolbar(): void {
    super.createGridToolbar();

    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton('manualtransaction', {
          iconName: signal('plus'),
          caption: signal(
            'economy.accounting.liquidityplanning.manualtransaction'
          ),
          tooltip: signal(
            'economy.accounting.liquidityplanning.manualtransactiontooltip'
          ),
          onAction: () => this.createEditTransaction(null),
        }),
      ],
    });
  }

  override edit(row: ILiquidityPlanningDTO): void {
    if (!row) return;
    if (
      row.transactionType === LiquidityPlanningTransactionType.CustomerInvoice
    ) {
      const uri =
        '/soe/economy/customer/invoice/status' +
        '?invoiceId=' +
        row.invoiceId +
        '&invoiceNr=' +
        row.invoiceNr +
        '&c=' +
        SoeConfigUtil.actorCompanyId;
      BrowserUtil.openInNewTab(window, uri);
    } else if (
      row.transactionType === LiquidityPlanningTransactionType.SupplierInvoice
    ) {
      const uri =
        '/soe/economy/supplier/invoice/status' +
        '?invoiceId=' +
        row.invoiceId +
        '&invoiceNr=' +
        row.invoiceNr +
        '&c=' +
        SoeConfigUtil.actorCompanyId;
      BrowserUtil.openInNewTab(window, uri);
    } else if (
      row.transactionType === LiquidityPlanningTransactionType.Manual
    ) {
      const data = new LiquidityPlanningDialogData(row);
      this.createEditTransaction(data);
    }
  }

  onGridReadyToDefine(grid: GridComponent<ILiquidityPlanningDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.date',
        'economy.accounting.liquidityplanning.valuein',
        'economy.accounting.liquidityplanning.valueout',
        'common.balance',
        'economy.accounting.liquidityplanning.transactiontype',
        'economy.accounting.liquidityplanning.specification',
        'economy.accounting.liquidityplanning.liquidity',
        'economy.supplier.suppliercentral.unpaiedinvoices',
        'economy.accounting.liquidityplanning.paidunchecked',
        'economy.accounting.liquidityplanning.paidchecked',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.setGroupDisplayType(GroupDisplayType.Custom);
        this.grid.addColumnDate('date', terms['common.date'], {
          enableGrouping: true,
        });
        this.grid.addColumnText(
          'transactionTypeName',
          terms['economy.accounting.liquidityplanning.transactiontype'],
          {
            enableGrouping: true,
            enableHiding: true,
          }
        );
        this.grid.addColumnText(
          'specification',
          terms['economy.accounting.liquidityplanning.specification'],
          {
            enableGrouping: true,
            enableHiding: true,
          }
        );
        this.grid.addColumnNumber(
          'valueIn',
          terms['economy.accounting.liquidityplanning.valuein'],
          {
            enableGrouping: true,
            enableHiding: false,
            decimals: 2,
            aggFuncOnGrouping: 'sum',
          }
        );
        this.grid.addColumnNumber(
          'valueOut',
          terms['economy.accounting.liquidityplanning.valueout'],
          {
            enableGrouping: true,
            enableHiding: false,
            decimals: 2,
            aggFuncOnGrouping: 'sum',
          }
        );
        this.grid.addColumnNumber('total', terms['common.balance'], {
          enableGrouping: true,
          enableHiding: false,
          decimals: 2,
          aggFuncOnGrouping: 'sum',
        });
        this.grid.addColumnIconEdit({
          tooltip: terms['core.edit'],
          onClick: this.edit.bind(this),
          showIcon: this.showEditButton.bind(this),
        });

        this.grid.useGrouping({
          stickyGroupTotalRow: 'bottom',
          stickyGrandTotalRow: 'bottom',
        });

        this.grid.groupRowsByColumn('date', false);

        super.finalizeInitGrid();
      });
  }

  override onGridIsDefined() {
    this.grid.resizeColumns(GridResizeType.ToFit);
  }

  createEditTransaction(row: LiquidityPlanningDialogData | null) {
    if (!row) {
      row = new LiquidityPlanningDialogData();
      row.transactionType = LiquidityPlanningTransactionType.Manual;
    }

    row.title = 'economy.accounting.liquidityplanning.manualtransaction';
    row.size = 'lg';

    const dialog = this.dialogService.open(ManualTransactionComponent, row);
    dialog
      .afterClosed()
      .pipe(
        take(1),
        tap(result => {
          if (result.delete) {
            this.messageBoxService
              .warning('core.warning', 'core.deletewarning')
              .afterClosed()
              .subscribe(res => {
                if (!res.result) {
                  return;
                }
                this.performAction.crud(
                  CrudActionTypeEnum.Delete,
                  this.service.deleteLiquidityPlanningTransaction(
                    result.item.liquidityPlanningTransactionId
                  ),
                  () => this.refreshGrid()
                );
              });
          } else if (result.item) {
            this.performAction.crud(
              CrudActionTypeEnum.Save,
              this.service.saveLiquidityPlanningTransaction(result.item),
              () => this.refreshGrid()
            );
          }
        })
      )
      .subscribe();
  }

  override loadData(
    id?: number | undefined
  ): Observable<ILiquidityPlanningDTO[]> {
    const filter = this.filterRef?.getFilter();
    if (!filter) return of([]);

    return this.performLoad.load$(
      this.service.getGrid(undefined, {
        from: filter.from,
        to: filter.to,
        exclusion: filter.exclusion,
        balance: filter.balance,
        unpaid: filter.unpaid,
        unchecked: filter.paidUnchecked,
        checked: filter.paidChecked,
      })
    );
  }

  override onAfterLoadData(data: ILiquidityPlanningDTO[]) {
    this.grid.resizeColumns(GridResizeType.ToFit);
    this.enableGraph = data == undefined ? false : true;
    this.summarize(data);
  }

  searchNew(filter: GetLiquidityPlanningModel) {
    this.performLoad.load(
      this.service
        .getLiquidityPlanningNew(
          filter.from,
          filter.to,
          filter.exclusion,
          filter.balance,
          filter.unpaid,
          filter.paidUnchecked,
          filter.paidChecked
        )
        .pipe(
          tap(data => {
            this.enableGraph = data == undefined ? false : true;
            this.grid.setData(data);
            this.summarize(data);
            this.grid.resizeColumns(GridResizeType.ToFit);
          })
        )
    );
  }

  private summarize(data: ILiquidityPlanningDTO[]) {
    let valueIn = 0;
    let valueOut = 0;
    let totalIn = 0;
    let totalOut = 0;

    let first: boolean = true;
    data.forEach((y: ILiquidityPlanningDTO) => {
      if (first) {
        totalIn = y.total;
        first = false;
      }

      if (
        y.transactionType ===
          LiquidityPlanningTransactionType.CustomerInvoice ||
        y.transactionType ===
          LiquidityPlanningTransactionType.SupplierInvoice ||
        y.transactionType === LiquidityPlanningTransactionType.Manual
      ) {
        valueIn += y.valueIn;
        valueOut -= y.valueOut;
        totalOut += y.total;
      } else {
        totalOut = y.total;
      }
    });
    this.valueIn.setValue(valueIn);
    this.valueOut.setValue(valueOut);
    this.totalIn.setValue(totalIn);
    this.totalOut.setValue(totalOut);
  }

  private showEditButton(row: ILiquidityPlanningDTO) {
    return (
      row &&
      (row.transactionType ===
        LiquidityPlanningTransactionType.CustomerInvoice ||
        row.transactionType ===
          LiquidityPlanningTransactionType.SupplierInvoice ||
        row.transactionType === LiquidityPlanningTransactionType.Manual)
    );
  }

  open($event: boolean) {
    if ($event) {
      console.log('Open Chart : ' + $event);
      this.liquidityPlanningChartConfig = {
        type: 'line',
        title: 'Monthly Sales',
        data: [
          { date: new Date(2023, 0, 1), outgoingLiquidity: 120 },
          { date: new Date(2023, 1, 1), outgoingLiquidity: 180 },
          { date: new Date(2023, 2, 1), outgoingLiquidity: 150 },
        ],
        xKey: 'date',
        yKey: 'outgoingLiquidity',
      };
    }
  }
}
