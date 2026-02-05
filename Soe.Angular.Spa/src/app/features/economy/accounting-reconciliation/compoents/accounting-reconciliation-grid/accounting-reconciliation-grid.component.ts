import { Component, inject, OnInit } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { AccountingReconciliationService } from '../../services/accounting-reconciliation.service';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import {
  AccountingReconciliationFilterDTO,
  ReconciliationRowDTO,
} from '../../models/accounting-reconciliation.model';
import { take, tap } from 'rxjs';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Perform } from '@shared/util/perform.class';

@Component({
  selector: 'soe-accounting-reconciliation-grid',
  templateUrl: './accounting-reconciliation-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class AccountingReconciliationGridComponent
  extends GridBaseDirective<
    ReconciliationRowDTO,
    AccountingReconciliationService
  >
  implements OnInit
{
  service = inject(AccountingReconciliationService);

  private readonly performLoad = new Perform<ReconciliationRowDTO[]>(
    this.progressService
  );

  filter!: AccountingReconciliationFilterDTO;

  ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(
      Feature.Economy_Accounting_Reconciliation,
      'economy.accounting.reconciliation.reconciliation',
      {
        useLegacyToolbar: true,
      }
    );
  }

  searchRows(filter: AccountingReconciliationFilterDTO | undefined): void {
    if (
      filter &&
      filter.currentAccountDimId &&
      filter.fromAccount &&
      filter.toAccount &&
      filter.fromDate &&
      filter.toDate
    ) {
      this.filter = filter;
      this.performLoad.load(
        this.service
          .getRows(
            filter.currentAccountDimId,
            filter.fromAccount,
            filter.toAccount,
            filter.fromDate,
            filter.toDate
          )
          .pipe(
            take(1),
            tap(x => {
              const rows = x.map(r => {
                r.number = r.account.split(' ')[0];
                return r;
              });
              this.rowData.next(rows);
            })
          )
      );
    }
  }

  onGridReadyToDefine(grid: GridComponent<ReconciliationRowDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'economy.accounting.reconciliation.account',
        'economy.accounting.reconciliation.customeramount',
        'economy.accounting.reconciliation.supplieramount',
        'economy.accounting.reconciliation.paymentamount',
        'economy.accounting.reconciliation.ledgeramount',
        'economy.accounting.reconciliation.diffamount',
        'core.aggrid.totals.total',
        'core.aggrid.totals.filtered',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText(
          'account',
          terms['economy.accounting.reconciliation.account'],
          {
            enableHiding: true,
            flex: 25,
          }
        );
        this.grid.addColumnNumber(
          'customerAmount',
          terms['economy.accounting.reconciliation.customeramount'],
          {
            enableHiding: true,
            decimals: 2,
            flex: 15,
          }
        );
        this.grid.addColumnNumber(
          'supplierAmount',
          terms['economy.accounting.reconciliation.supplieramount'],
          {
            enableHiding: true,
            decimals: 2,
            flex: 15,
          }
        );
        this.grid.addColumnNumber(
          'paymentAmount',
          terms['economy.accounting.reconciliation.paymentamount'],
          {
            enableHiding: true,
            decimals: 2,
            flex: 15,
          }
        );
        this.grid.addColumnNumber(
          'ledgerAmount',
          terms['economy.accounting.reconciliation.ledgeramount'],
          {
            enableHiding: true,
            decimals: 2,
            flex: 15,
          }
        );
        this.grid.addColumnNumber(
          'diffAmount',
          terms['economy.accounting.reconciliation.diffamount'],
          {
            enableHiding: true,
            decimals: 2,
            flex: 15,
          }
        );
        this.grid.addColumnIcon(null, '', {
          flex: 30,
          iconName: 'info-circle',
          onClick: row => {
            row.accountYearId = this.filter.currentAccountYearId ?? 0;
            this.edit(row);
          },
        });

        super.finalizeInitGrid();
      });
  }
}
