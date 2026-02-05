import {
  Component,
  EventEmitter,
  inject,
  input,
  Input,
  OnDestroy,
  OnInit,
  Output,
} from '@angular/core';
import {
  AccountDimsForm,
  SelectedAccounts,
  SelectedAccountsChangeSet,
} from '../account-dims-form.model';
import { AccountDimSmallDTO } from '@shared/components/accounting-rows/models/accounting-rows-model';
import { AccountDTO } from '@shared/models/account.model';
import {
  SoeOriginStatus,
  SoeOriginType,
} from '@shared/models/generated-interfaces/Enumerations';
import { Guid } from '@shared/util/string-util';
import { TranslateService } from '@ngx-translate/core';
import { concatMap, Observable, Subject, take, takeUntil, tap } from 'rxjs';
import { TermCollection } from '@shared/localization/term-types';
import { CoreService } from '@shared/services/core.service';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { set } from 'lodash';
import { emit } from 'process';

@Component({
  selector: 'soe-account-dims',
  templateUrl: './account-dims.component.html',
  styleUrl: './account-dims.component.scss',
  standalone: false,
})
export class AccountDimsComponent implements OnInit, OnDestroy {
  unsubscribe = new Subject<void>();
  accountDims!: AccountDimSmallDTO[];

  standardAccounts: SmallGenericType[] = [];
  internalAccounts2: SmallGenericType[] = [];
  internalAccounts3: SmallGenericType[] = [];
  internalAccounts4: SmallGenericType[] = [];
  internalAccounts5: SmallGenericType[] = [];
  internalAccounts6: SmallGenericType[] = [];

  accountId1!: number;
  accountId2!: number;
  accountId3!: number;
  accountId4!: number;
  accountId5!: number;
  accountId6!: number;

  dimLabels = {
    dim1: '',
    dim2: '',
    dim3: '',
    dim4: '',
    dim5: '',
    dim6: '',
  };

  mandatoryDims = {
    dim1: false,
    dim2: false,
    dim3: false,
    dim4: false,
    dim5: false,
    dim6: false,
  };

  dimAccounts: {
    dim1: AccountDTO[];
    dim2: AccountDTO[];
    dim3: AccountDTO[];
    dim4: AccountDTO[];
    dim5: AccountDTO[];
    dim6: AccountDTO[];
  } = {
    dim1: [],
    dim2: [],
    dim3: [],
    dim4: [],
    dim5: [],
    dim6: [],
  };

  selectedAccount1!: AccountDTO;
  selectedAccount2!: AccountDTO;
  selectedAccount3!: AccountDTO;
  selectedAccount4!: AccountDTO;
  selectedAccount5!: AccountDTO;
  selectedAccount6!: AccountDTO;

  private skipCache = false;
  private originType?: SoeOriginType;
  private originStatus?: SoeOriginStatus;
  private doNotUseTerm!: TermCollection;

  @Input() form!: AccountDimsForm;
  @Input() parentGuid!: Guid;
  @Input() addNotUsed = false;
  @Input() hideStdDim!: boolean;
  updateCodingSetting = input<Subject<void>>();

  @Output() accountDimsChanged = new EventEmitter<SelectedAccountsChangeSet>();

  translation = inject(TranslateService);
  coreService = inject(CoreService);

  ngOnInit(): void {
    this.initLoadAccounts();

    this.updateCodingSetting()
      ?.pipe(takeUntil(this.unsubscribe))
      .subscribe(() => {
        this.initLoadAccounts();
      });
  }

  private setupWatchers() {
    this.form.account1.valueChanges
      .pipe(
        takeUntil(this.unsubscribe),
        tap(x => {
          const account = this.getAccount(1, x);
          if (account) this.selectionChanged(1, account, false);
          else if (x > 0) this.accountId1 = x;
        })
      )
      .subscribe();

    this.form.account2.valueChanges
      .pipe(
        takeUntil(this.unsubscribe),
        tap(x => {
          const account = this.getAccount(2, x);
          if (account) this.selectionChanged(2, account, false);
          else if (x > 0) this.accountId2 = x;
        })
      )
      .subscribe();

    this.form.account3.valueChanges
      .pipe(
        takeUntil(this.unsubscribe),
        tap(x => {
          const account = this.getAccount(3, x);
          if (account) this.selectionChanged(3, account, false);
          else if (x > 0) this.accountId3 = x;
        })
      )
      .subscribe();

    this.form.account4.valueChanges
      .pipe(
        takeUntil(this.unsubscribe),
        tap(x => {
          const account = this.getAccount(4, x);
          if (account) this.selectionChanged(4, account, false);
          else if (x > 0) this.accountId4 = x;
        })
      )
      .subscribe();

    this.form.account5.valueChanges
      .pipe(
        takeUntil(this.unsubscribe),
        tap(x => {
          const account = this.getAccount(5, x);
          if (account) this.selectionChanged(5, account, false);
          else if (x > 0) this.accountId5 = x;
        })
      )
      .subscribe();

    this.form.account6.valueChanges
      .pipe(
        takeUntil(this.unsubscribe),
        tap(x => {
          const account = this.getAccount(6, x);
          if (account) this.selectionChanged(6, account, false);
          else if (x > 0) this.accountId6 = x;
        })
      )
      .subscribe();
  }

  selectionChanged(
    dim: number,
    account: any,
    manuallyChanged = true,
    emitEvent = true
  ) {
    if (account && manuallyChanged)
      account = {
        accountId: account.id,
        name: account.name,
        numberName: account.accountNr + ' ' + account.name,
      } as AccountDTO;
    switch (dim) {
      case 1:
        this.selectedAccount1 = account;
        break;
      case 2:
        this.selectedAccount2 = account;
        break;
      case 3:
        this.selectedAccount3 = account;
        break;
      case 4:
        this.selectedAccount4 = account;
        break;
      case 5:
        this.selectedAccount5 = account;
        break;
      case 6:
        this.selectedAccount6 = account;
        break;
    }

    const selectedValues: SelectedAccounts = {
      account1: this.selectedAccount1,
      account2: this.selectedAccount2,
      account3: this.selectedAccount3,
      account4: this.selectedAccount4,
      account5: this.selectedAccount5,
      account6: this.selectedAccount6,
    };

    if (emitEvent) {
      this.accountDimsChanged.emit({
        selectedAccounts: selectedValues,
        manuallyChanged: manuallyChanged,
        dimNr: dim,
      });
    }
  }

  private initLoadAccounts() {
    if (this.addNotUsed) {
      this.translation
        .get(['common.donotuse'])
        .pipe(
          take(1),
          tap(term => {
            this.doNotUseTerm = term;
          }),
          concatMap(() => this.loadAccounts())
        )
        .subscribe();
    } else {
      this.loadAccounts().subscribe();
    }
  }

  private getAccount(
    dimIdx: number,
    accountId: number
  ): AccountDTO | undefined {
    return dimIdx <= this.accountDims.length
      ? this.accountDims[dimIdx - 1].accounts.find(
          acc => acc.accountId === accountId
        )
      : undefined;
  }

  private loadAccounts(useCache = false): Observable<AccountDimSmallDTO[]> {
    return this.coreService
      .getAccountDimsSmall(
        false,
        this.hideStdDim,
        true,
        false,
        false,
        false,
        false,
        false,
        useCache
      )
      .pipe(
        take(1),
        tap((x: AccountDimSmallDTO[]) => {
          this.accountDims = x;
          // Add empty standard dim for placeholder
          if (
            this.hideStdDim &&
            !this.accountDims.find(ad => ad.accountDimNr === 1)
          ) {
            const newDim = new AccountDimSmallDTO();
            newDim.accountDimId = 0;
            newDim.accountDimNr = 1;
            newDim.name = '';
            newDim.accounts = [];
            this.accountDims.unshift(newDim);
          }

          let i = 0;
          this.standardAccounts = [];
          this.internalAccounts2 = [];
          this.internalAccounts3 = [];
          this.internalAccounts4 = [];
          this.internalAccounts5 = [];
          this.internalAccounts6 = [];
          this.accountDims.forEach((ad: AccountDimSmallDTO) => {
            i++;
            const _name = `dim${i}`;
            if (this.isObjKey(_name, this.dimLabels)) {
              this.dimLabels[_name] = ad.name;
            }

            if (this.originType) this.setMandatory(ad, i);

            // Add empty row
            if (!ad.accounts) ad.accounts = [];

            if (ad.accounts.length === 0 || ad.accounts[0].accountId !== 0) {
              if (this.addNotUsed) {
                const acc = new AccountDTO();
                acc.accountId = -1;
                acc.accountNr = ' ';
                acc.name = this.doNotUseTerm['common.donotuse'];
                acc.numberName = this.doNotUseTerm['common.donotuse'];
                (<AccountDTO[]>ad.accounts).unshift(acc);
              }

              let accounts = undefined;
              switch (i) {
                case 1:
                  accounts = this.standardAccounts;
                  break;
                case 2:
                  accounts = this.internalAccounts2;
                  break;
                case 3:
                  accounts = this.internalAccounts3;
                  break;
                case 4:
                  accounts = this.internalAccounts4;
                  break;
                case 5:
                  accounts = this.internalAccounts5;
                  break;
                case 6:
                  accounts = this.internalAccounts6;
                  break;
              }

              if (accounts) {
                ad.accounts.forEach((a: AccountDTO) => {
                  accounts.push({ id: a.accountId, name: a.numberName });
                });

                (<SmallGenericType[]>accounts).unshift({ id: 0, name: '' });
                x;
              }
            }

            if (this.isObjKey(_name, this.dimAccounts)) {
              this.dimAccounts[_name] = ad.accounts as AccountDTO[];
            }
          });

          this.setupWatchers();

          if (this.accountId1) {
            const account = this.getAccount(1, this.accountId1);
            if (account) this.selectionChanged(1, account, false, false);
            this.accountId1 = 0;
          }
          if (this.accountId2) {
            const account = this.getAccount(2, this.accountId2);
            if (account) this.selectionChanged(2, account, false, false);
            this.accountId2 = 0;
          }
          if (this.accountId3) {
            const account = this.getAccount(3, this.accountId3);
            if (account) this.selectionChanged(3, account, false, false);
            this.accountId3 = 0;
          }
          if (this.accountId4) {
            const account = this.getAccount(4, this.accountId4);
            if (account) this.selectionChanged(4, account, false, false);
            this.accountId4 = 0;
          }
          if (this.accountId5) {
            const account = this.getAccount(5, this.accountId5);
            if (account) this.selectionChanged(5, account, false, false);
            this.accountId5 = 0;
          }
          if (this.accountId6) {
            const account = this.getAccount(6, this.accountId6);
            if (account) this.selectionChanged(6, account, false, false);
            this.accountId6 = 0;
          }
        })
      );
  }

  private setMandatory(accountDim: AccountDimSmallDTO, i: number) {
    const _name = `dim${i}`;
    if (!this.isObjKey(_name, this.mandatoryDims)) return;
    switch (this.originType) {
      case SoeOriginType.Order:
        this.mandatoryDims[_name] = accountDim.mandatoryInOrder;
        break;
      case SoeOriginType.CustomerInvoice:
        this.mandatoryDims[_name] =
          this.originStatus === SoeOriginStatus.Draft ||
          this.originStatus === SoeOriginStatus.Origin
            ? accountDim.mandatoryInCustomerInvoice
            : false;
        break;
    }
  }

  ngOnDestroy(): void {
    this.unsubscribe.next();
    this.unsubscribe.complete();
  }

  private isObjKey<T extends object>(key: PropertyKey, obj: T): key is keyof T {
    return key in obj;
  }
}
