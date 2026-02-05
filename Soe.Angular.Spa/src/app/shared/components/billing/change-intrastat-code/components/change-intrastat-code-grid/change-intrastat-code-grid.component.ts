import { Component, Input, OnInit, inject } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  Feature,
  SoeEntityState,
  SoeOriginType,
  TermGroup,
} from '@shared/models/generated-interfaces/Enumerations';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { CommodityCodesService } from '@src/app/features/manage/commodity-codes/services/commodity-codes.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { BehaviorSubject, of, take, tap } from 'rxjs';
import { IntrastatTransactionDTO } from '../../models/change-intrastat-code.model';

@Component({
  selector: 'soe-change-intrastat-code-grid',
  templateUrl: './change-intrastat-code-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class ChangeIntrastatCodeGridComponent
  extends GridBaseDirective<IntrastatTransactionDTO>
  implements OnInit
{
  @Input() rows = new BehaviorSubject<IntrastatTransactionDTO[]>([]);
  @Input() originType = 0;

  commodityCodesService = inject(CommodityCodesService);
  coreService = inject(CoreService);

  intrastatCodes: ISmallGenericType[] = [];
  private transactionDict: ISmallGenericType[] = [];
  private countryDict: ISmallGenericType[] = [];
  transactions: any;
  usedAmount: any;

  override ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(Feature.Economy_Intrastat, '', {
      skipInitialLoad: true,
      lookups: [
        this.loadIntrastatCodes(),
        this.loadTransactionTypes(),
        this.loadCountries(),
      ],
    });
  }

  private loadIntrastatCodes() {
    return this.commodityCodesService.getCustomerCommodyCodesDict(false).pipe(
      tap(x => {
        this.intrastatCodes = x;
      })
    );
  }

  loadCountries() {
    return this.coreService
      .getCountries(true, true)
      .pipe(tap(x => (this.countryDict = x)));
  }

  loadTransactionTypes() {
    return this.coreService
      .getTermGroupContent(TermGroup.IntrastatTransactionType, false, false)
      .pipe(
        tap(res => {
          this.transactionDict = res;
        })
      );
  }

  override onGridReadyToDefine(grid: GridComponent<IntrastatTransactionDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.customer.invoices.row',
        'common.customer.invoices.productnr',
        'common.customer.invoices.productname',
        'common.customer.invoices.quantity',
        'common.customer.invoices.unit',
        'common.commoditycodes.code',
        'economy.accounting.liquidityplanning.transactiontype',
        'common.commoditycodes.netweight',
        'common.commoditycodes.otherquantity',
        'common.countryoforigin',
        'common.commoditycodes.notintrastat',
        'billing.productrows.productunit',
        'billing.purchaserows.sumamount',
        'core.deleterow',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        if (this.originType === SoeOriginType.SupplierInvoice) {
          this.grid.addColumnNumber(
            'amount',
            terms['billing.purchaserows.sumamount'],
            {
              flex: 1,
              enableHiding: false,
              editable: row => !row.data?.notIntrastat,
              decimals: 2,
            }
          );
          this.grid.addColumnNumber(
            'quantity',
            terms['common.customer.invoices.quantity'],
            {
              flex: 1,
              enableHiding: false,
            }
          );
        } else {
          this.grid.addColumnNumber(
            'rowNr',
            terms['common.customer.invoices.row'],
            {
              flex: 1,
              enableHiding: false,
              pinned: 'left',
            }
          );
          this.grid.addColumnText(
            'productNr',
            terms['common.customer.invoices.productnr'],
            {
              flex: 1,
              enableHiding: false,
            }
          );
          this.grid.addColumnText(
            'productName',
            terms['common.customer.invoices.productname'],
            {
              flex: 1,
              enableHiding: false,
            }
          );
          this.grid.addColumnNumber(
            'quantity',
            terms['common.customer.invoices.quantity'],
            {
              flex: 1,
              enableHiding: false,
            }
          );
          this.grid.addColumnText(
            'productUnitCode',
            terms['common.customer.invoices.unit'],
            {
              flex: 1,
              enableHiding: false,
            }
          );
        }

        this.grid.addColumnAutocomplete<SmallGenericType>(
          'intrastatCodeId',
          terms['common.commoditycodes.code'],
          {
            flex: 1,
            editable: row => !row.data?.notIntrastat,
            source: () => this.intrastatCodes,
            optionIdField: 'id',
            optionNameField: 'name',
          }
        );
        this.grid.addColumnAutocomplete<SmallGenericType>(
          'intrastatTransactionType',
          terms['economy.accounting.liquidityplanning.transactiontype'],
          {
            flex: 1,
            editable: row => !row.data?.notIntrastat,
            source: () => this.transactionDict,
            optionIdField: 'id',
            optionNameField: 'name',
          }
        );

        this.grid.addColumnNumber(
          'netWeight',
          terms['common.commoditycodes.netweight'],
          {
            flex: 1,
            enableHiding: false,
            decimals: 3,
            editable: row => !row.data?.notIntrastat,
          }
        );
        this.grid.addColumnText(
          'otherQuantity',
          terms['common.commoditycodes.otherquantity'],
          {
            flex: 1,
            enableHiding: false,
            editable: row => !row.data?.notIntrastat,
          }
        );

        this.grid.addColumnAutocomplete<SmallGenericType>(
          'sysCountryId',
          terms['common.countryoforigin'],
          {
            flex: 1,
            editable: row => !row.data?.notIntrastat,
            source: () => this.countryDict,
            optionIdField: 'id',
            optionNameField: 'name',
          }
        );

        this.grid.addColumnBool(
          'notIntrastat',
          terms['common.commoditycodes.notintrastat'],
          {
            flex: 1,
            editable: true,
          }
        );
        this.grid.addColumnIconDelete({ onClick: r => this.deleteRow(r) });

        this.grid.columns.forEach(col => {
          const cellcls: string = col.cellClass ? col.cellClass.toString() : '';

          col.cellClass = (grid: any) => {
            if (grid.data['notIntrastat']) return cellcls + ' closedRow';
            else return cellcls;
          };
        });

        super.finalizeInitGrid();
      });
  }

  //Actions
  private deleteRow(row: IntrastatTransactionDTO) {
    if (row.intrastatTransactionId && row.intrastatTransactionId > 0) {
      row.state = SoeEntityState.Deleted;
      row.isModified = true;
    } else {
      const index = this.transactions.indexOf(row);
      this.transactions.splice(index, 1);
    }

    const filteredTransactions = this.transactions.filter(
      (t: { state: SoeEntityState }) => t.state !== SoeEntityState.Deleted
    );
    this.usedAmount = filteredTransactions.reduce(
      (total: any, t: { amount: any }) => total + t.amount,
      0
    );
  }
}
