import { Component, inject, OnInit, output } from '@angular/core';
import { AccountingReconciliationFilterForm } from '@features/economy/accounting-reconciliation/models/accounting-reconciliation-filter-form.model';
import { AccountingReconciliationFilterDTO } from '@features/economy/accounting-reconciliation/models/accounting-reconciliation.model';
import { EconomyService } from '@features/economy/services/economy.service';
import { ValidationHandler } from '@shared/handlers';
import {
  IAccountDimDTO,
  IAccountSmallDTO,
  IAccountYearDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CoreService } from '@shared/services/core.service'
import { ProgressService } from '@shared/services/progress/progress.service';
import { Perform } from '@shared/util/perform.class';
import { sortBy } from 'lodash';
import { forkJoin, mergeMap, Observable, of, tap } from 'rxjs';

@Component({
    selector: 'soe-accounting-reconciliation-grid-filter',
    templateUrl: './accounting-reconciliation-grid-filter.component.html',
    standalone: false
})
export class AccountingReconciliationGridFilterComponent implements OnInit {
  searchRows = output<AccountingReconciliationFilterDTO | undefined>();

  private readonly validationHandler = inject(ValidationHandler);
  private readonly progressService = inject(ProgressService);
  private readonly performLoad = new Perform(this.progressService);

  protected readonly form = new AccountingReconciliationFilterForm({
    validationHandler: this.validationHandler,
    element: new AccountingReconciliationFilterDTO(),
  });

  economyService = inject(EconomyService);
  coreService = inject(CoreService);

  accounts: IAccountSmallDTO[] = [];

  ngOnInit(): void {
    this.performLoad.load(this.doLookUps());
  }

  private doLookUps() {
    return forkJoin([
      this.loadAccountDimStd(),
      this.loadCurrentAccountYear(),
    ]).pipe(mergeMap(() => this.loadAccounts()));
  }

  private loadAccounts() {
    if (
      this.form.currentAccountDimId.value > 0 &&
      this.form.currentAccountYearId.value > 0
    ) {
      return this.economyService
        .getAccountsSmall(
          this.form.currentAccountDimId.value,
          this.form.currentAccountYearId.value
        )
        .pipe(
          tap(x => {
            this.accounts = sortBy(x, 'number');

            this.form.fromAccount.patchValue(this.accounts[0]?.number);
            this.form.toAccount.patchValue(
              this.accounts[this.accounts.length - 1]?.number
            );
          })
        );
    }

    return of(undefined);
  }

  private loadCurrentAccountYear(): Observable<IAccountYearDTO> {
    return this.coreService.getCurrentAccountYear().pipe(
      tap(x => {
        this.form.currentAccountYearId.patchValue(x.accountYearId);
        this.form.fromDate.patchValue(new Date(x.from));
        this.form.toDate.patchValue(new Date(x.to));
      })
    );
  }

  private loadAccountDimStd(): Observable<IAccountDimDTO> {
    return this.economyService.getAccountDimStd().pipe(
      tap(x => {
        this.form.currentAccountDimId.patchValue(x.accountDimId);
      })
    );
  }

  search(): void {
    if (this.form.valid) {
      this.searchRows.emit(this.form.getRawValue());
    }
  }
}
