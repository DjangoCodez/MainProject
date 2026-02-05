import { Component, OnInit, inject } from '@angular/core';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { MatchCodeDTO } from '../../models/match-codes.model';
import { MatchCodeService } from '../../services/match-codes.service';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import {
  Feature,
  TermGroup,
} from '@shared/models/generated-interfaces/Enumerations';
import { Observable } from 'rxjs';
import { EconomyService } from '../../../services/economy.service';
import { tap } from 'rxjs/operators';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { MatchCodesForm } from '../../models/match-codes-form.model';
import { AccountStdNumberNameDTO } from '../../../models/account-std.model';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';

@Component({
  selector: 'soe-match-settings-edit',
  templateUrl: './match-codes-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class MatchCodeEditComponent
  extends EditBaseDirective<MatchCodeDTO, MatchCodeService, MatchCodesForm>
  implements OnInit
{
  // Properties
  service = inject(MatchCodeService);
  private economyService = inject(EconomyService);
  private coreService = inject(CoreService);
  matchCodeTypes: SmallGenericType[] = [];
  accountsDict: AccountStdNumberNameDTO[] = [];

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(Feature.Economy_Preferences_VoucherSettings_MatchCodes, {
      skipDefaultToolbar: true,
      lookups: [this.loadAccounts(), this.loadTypes()],
    });
  }

  loadAccounts(): Observable<AccountStdNumberNameDTO[]> {
    return this.performLoadData.load$(
      this.economyService.getAccountStdsNameNumber(true).pipe(
        tap(x => {
          this.accountsDict = x;
        })
      )
    );
  }

  loadTypes(): Observable<SmallGenericType[]> {
    return this.performLoadData.load$(
      this.coreService
        .getTermGroupContent(TermGroup.MatchCodeType, false, false)
        .pipe(
          tap(x => {
            this.matchCodeTypes = x;
          })
        )
    );
  }

  getSelectedAccountCode(account: AccountStdNumberNameDTO) {
    return (account as AccountStdNumberNameDTO).name.split('. ')[0] ?? '';
  }

  // EVENTS

  accountChanged(selectedAccount: AccountStdNumberNameDTO) {
    this.form?.patchValue({
      accountNr: this.getSelectedAccountCode(selectedAccount),
    });
  }

  vatAccountchanged(selectedAccount: AccountStdNumberNameDTO) {
    this.form?.patchValue({
      vatAccountNr: this.getSelectedAccountCode(selectedAccount),
    });
  }
}
