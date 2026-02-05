import { Component, OnInit, inject, signal } from '@angular/core';
import { StockBalanceFileImportDialogData } from '@features/billing/shared/components/stock-balance-file-import/stock-balance-file-import/models/stock-balance-file-import.model';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { IStockProductDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { IconUtil } from '@shared/util/icon-util';
import { Perform } from '@shared/util/perform.class';
import { AggregationType } from '@ui/grid/interfaces';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable, take, tap } from 'rxjs';
import { StockBalanceFileImportComponent } from '../../../shared/components/stock-balance-file-import/stock-balance-file-import/stock-balance-file-import.component';
import { StockBalanceService } from '../../services/stock-balance.service';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Component({
  selector: 'soe-stock-balance-grid',
  templateUrl: './stock-balance-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class StockBalanceGridComponent
  extends GridBaseDirective<IStockProductDTO, StockBalanceService>
  implements OnInit
{
  service = inject(StockBalanceService);
  readonly progressService = inject(ProgressService);
  readonly dialogService = inject(DialogService);
  purchasePermission = true;
  hasViewAvgPriceAndValuPermission = false;
  performLoadRecalCulatedStrockProduct = new Perform(this.progressService);
  performStockProductLoad = new Perform<IStockProductDTO[]>(
    this.progressService
  );
  showInactive = false;

  ngOnInit(): void {
    this.startFlow(Feature.Billing_Stock_Saldo, 'Billing.Stock.StockSaldo', {
      additionalModifyPermissions: [
        Feature.Billing_Purchase,
        Feature.Billing_Stock_ViewAvgPriceAndValue,
      ],
      useLegacyToolbar: true,
    });
  }

  override onPermissionsLoaded() {
    super.onPermissionsLoaded();
    this.purchasePermission = this.flowHandler.hasModifyAccess(
      Feature.Billing_Purchase
    );

    this.hasViewAvgPriceAndValuPermission = this.flowHandler.hasModifyAccess(
      Feature.Billing_Stock_ViewAvgPriceAndValue
    );
  }

  override createLegacyGridToolbar(): void {
    super.createLegacyGridToolbar({
      reloadOption: {
        onClick: () => this.refreshGrid(),
      },
    });

    this.toolbarUtils.createLegacyGroup({
      buttons: [
        this.toolbarUtils.createLegacyButton({
          icon: IconUtil.createIcon('fal', 'cog'),
          title: 'billing.stock.stocksaldo.recalculatebalance',
          label: 'billing.stock.stocksaldo.recalculatebalance',
          onClick: () => this.reCalculateBalance(),
          disabled: signal(false),
          hidden: signal(false),
        }),
      ],
    });

    this.toolbarUtils.createLegacyGroup({
      buttons: [
        this.toolbarUtils.createLegacyButton({
          icon: IconUtil.createIcon('fal', 'upload'),
          title: 'billing.stock.stocksaldo.importstockbalance',
          label: 'billing.stock.stocksaldo.importstockbalance',
          onClick: () => this.importFileDialog(),
          disabled: signal(false),
          hidden: signal(false),
        }),
      ],
    });
  }

  doFilter(showInactive: boolean) {
    this.showInactive = showInactive;
    this.refreshGrid();
  }

  importFileDialog() {
    const dialogData = new StockBalanceFileImportDialogData();
    dialogData.title = 'core.fileupload.choosefiletoimport';
    dialogData.size = 'lg';
    dialogData.stockInventoryHeadId = 0;

    this.dialogService.open(StockBalanceFileImportComponent, dialogData);
  }

  reCalculateBalance() {
    this.performLoadRecalCulatedStrockProduct.load(
      this.service
        .recalCulateStockBalance(0)
        .pipe(tap(this.updateStatesAndEmitChange))
    );
  }

  updateStatesAndEmitChange = (response: BackendResponse) => {
    if (response.success) {
      this.refreshGrid();
    }
  };

  onGridReadyToDefine(grid: GridComponent<IStockProductDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'billing.stock.stocksaldo.productnumber',
        'common.name',
        'billing.stock.stocks.stock',
        'billing.stock.stockplaces.stockplace',
        'billing.stock.stocksaldo.saldo',
        'billing.stock.stocksaldo.ordered',
        'billing.stock.stocksaldo.reserved',
        'billing.stock.stocksaldo.avgprice',
        'billing.stock.stocksaldo.value',
        'billing.stock.stocksaldo.purchasetriggerquantity',
        'billing.stock.stocksaldo.purchasequantity',
        'billing.stock.stocksaldo.purchasedquantity',
        'core.edit',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText(
          'productNumber',
          terms['billing.stock.stocksaldo.productnumber'],
          {
            flex: 1,
          }
        );
        this.grid.addColumnText('productName', terms['common.name'], {
          flex: 1,
        });
        this.grid.addColumnText(
          'stockName',
          terms['billing.stock.stocks.stock'],
          {
            flex: 1,
            showSetFilter: true,
          }
        );
        this.grid.addColumnText(
          'stockShelfName',
          terms['billing.stock.stockplaces.stockplace'],
          {
            flex: 1,
            showSetFilter: true,
          }
        );
        this.grid.addColumnNumber(
          'quantity',
          terms['billing.stock.stocksaldo.saldo'],
          {
            flex: 1,
            decimals: 2,
          }
        );
        this.grid.addColumnNumber(
          'orderedQuantity',
          terms['billing.stock.stocksaldo.ordered'],
          {
            flex: 1,
            decimals: 2,
          }
        );
        this.grid.addColumnNumber(
          'reservedQuantity',
          terms['billing.stock.stocksaldo.reserved'],
          {
            flex: 1,
            decimals: 2,
          }
        );
        if (this.hasViewAvgPriceAndValuPermission) {
          this.grid.addColumnNumber(
            'avgPrice',
            terms['billing.stock.stocksaldo.avgprice'],
            {
              flex: 1,
              decimals: 2,
            }
          );
          this.grid.addColumnNumber(
            'stockValue',
            terms['billing.stock.stocksaldo.value'],
            {
              flex: 1,
              decimals: 2,
            }
          );
        }
        if (this.purchasePermission) {
          this.grid.addColumnNumber(
            'purchaseTriggerQuantity',
            terms['billing.stock.stocksaldo.purchasetriggerquantity'],
            {
              flex: 1,
              decimals: 2,
              enableHiding: true,
            }
          );
          this.grid.addColumnNumber(
            'purchaseQuantity',
            terms['billing.stock.stocksaldo.purchasequantity'],
            {
              flex: 1,
              decimals: 2,
              enableHiding: true,
            }
          );
          this.grid.addColumnNumber(
            'purchasedQuantity',
            terms['billing.stock.stocksaldo.purchasedquantity'],
            {
              flex: 1,
              decimals: 2,
              enableHiding: true,
            }
          );
        }
        this.grid.addColumnIconEdit({
          tooltip: terms['core.edit'],
          onClick: row => {
            this.edit(row);
          },
        });
        this.grid.addAggregationsRow({
          quantity: AggregationType.Sum,
          orderedQuantity: AggregationType.Sum,
          reservedQuantity: AggregationType.Sum,
          stockValue: AggregationType.Sum,
          purchasedQuantity: AggregationType.Sum,
        });

        super.finalizeInitGrid();
      });
  }

  override loadData(
    id?: number | undefined,
    additionalProps?: {
      includeInactive: boolean;
    }
  ): Observable<IStockProductDTO[]> {
    return super.loadData(id, { includeInactive: this.showInactive });
  }
}
