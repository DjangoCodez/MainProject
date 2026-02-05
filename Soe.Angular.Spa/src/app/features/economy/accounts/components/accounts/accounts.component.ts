import { Component, inject } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { AccountsGridComponent } from '../accounts-grid/accounts-grid.component';
import { AccountsEditComponent } from '../accounts-edit/accounts-edit.component';
import { tap } from 'rxjs';
import { AccountForm } from '../../models/accounts-form.model';
import { TranslateService } from '@ngx-translate/core';
import { AccountingCodingLevelsService } from '@features/economy/accounting-coding-levels/services/accounting-coding-levels.service';
import { AccountUrlParamsService } from '../../services/account-params.service';

@Component({
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
  providers: [AccountUrlParamsService],
})
export class AccountsComponent {
  accountDimService = inject(AccountingCodingLevelsService);
  translationService = inject(TranslateService);
  paramService = inject(AccountUrlParamsService);

  config: MultiTabConfig[] = [
    {
      gridComponent: AccountsGridComponent,
      editComponent: AccountsEditComponent,
      FormClass: AccountForm,
      gridTabLabel: 'economy.accounting.accounts',
      editTabLabel: 'economy.accounting.account',
      createTabLabel: 'economy.accounting.newaccount',
      exportFilenameKey: 'economy.accounting.accounts',
    },
  ];

  constructor() {
    if (!this.paramService.isAccountStd && this.paramService.accountDimId > 0) {
      this.loadAccountDim();
    }
  }

  private loadAccountDim() {
    this.accountDimService
      .get(this.paramService.accountDimId)
      .pipe(
        tap(dim => {
          this.config[0].gridTabLabel = dim.name;
          this.config[0].editTabLabel = dim.name;
          this.config[0].createTabLabel =
            this.translationService.instant('common.new') + ' ' + dim.name;
        })
      )
      .subscribe();
  }
}
