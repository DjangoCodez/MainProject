import {
  Component,
  computed,
  effect,
  inject,
  input,
  OnDestroy,
  OnInit,
  output,
  signal,
} from '@angular/core';
import { EconomyService } from '@features/economy/services/economy.service';
import { VoucherSeriesTypeService } from '@features/economy/services/voucher-series-type.service';
import { SearchVoucherFilterForm } from '@features/economy/voucher-search/models/voucher-search-form.model';
import { SearchVoucherFilterDTO } from '@features/economy/voucher-search/models/voucher-search.model';
import { ValidationHandler } from '@shared/handlers';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  IAccountDimSmallDTO,
  IAccountDTO,
  IUserWithNameAndLoginDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { addEmptyOption } from '@shared/util/array-util';
import { Subject, takeUntil, tap } from 'rxjs';

type UserLoginDTO = IUserWithNameAndLoginDTO & { id: number };

@Component({
    selector: 'soe-voucher-search-filter',
    templateUrl: './voucher-search-filter.component.html',
    styleUrls: ['./voucher-search-filter.component.scss'],
    standalone: false
})
export class VoucherSearchFilterComponent implements OnInit, OnDestroy {
  accountDims = input.required<IAccountDimSmallDTO[]>();
  economyService = input.required<EconomyService>();
  searchClicked = output<SearchVoucherFilterDTO>();

  private readonly voucherSeriesTypeService = inject(VoucherSeriesTypeService);
  private validationHandler = inject(ValidationHandler);
  private _destroy$ = new Subject<void>();
  protected form = new SearchVoucherFilterForm({
    validationHandler: this.validationHandler,
    element: new SearchVoucherFilterDTO(),
  });

  protected moreFilterOpened = signal(false);
  protected moreFilterLabel = computed(() => {
    return this.moreFilterOpened()
      ? 'economy.accounting.vouchersearch.showless'
      : 'economy.accounting.vouchersearch.showmore';
  });

  protected accountDim1Accounts: IAccountDTO[] = [];
  protected voucherSeriesTypes: SmallGenericType[] = [];
  protected users: UserLoginDTO[] = [];
  constructor() {
    effect(() => {
      const accDims = this.accountDims() ?? [];
      this.updateFormControls(accDims);
    });

    this.form?.voucherDateFrom.valueChanges
      .pipe(takeUntil(this._destroy$))
      .subscribe(date => {
        this.form?.voucherDateTo.setValue(date);
      });

    this.form?.userId.valueChanges
      .pipe(takeUntil(this._destroy$))
      .subscribe(v => {
        this.form?.patchValue({
          createdBy: this.users.find(u => u.id === v)?.loginName ?? '',
        });
      });

    this.form?.dim1AccountFr.valueChanges
      .pipe(takeUntil(this._destroy$))
      .subscribe(v => {
        this.form?.dim1AccountTo.setValue(v);
      });
  }

  ngOnInit() {
    this.loadVoucherSeries();
    this.loadUsers();
  }

  private loadVoucherSeries(): void {
    this.voucherSeriesTypeService
      .getGrid()
      .pipe(
        tap(vsTypes => {
          this.voucherSeriesTypes = vsTypes
            .map(
              type =>
                new SmallGenericType(
                  type.voucherSeriesTypeId,
                  `${type.name} (${type.voucherSeriesTypeNr})`
                )
            )
            .sort((x, y): number => {
              if (x.name.toLowerCase() < y.name.toLowerCase()) return -1;
              if (x.name.toLowerCase() > y.name.toLowerCase()) return 1;
              return 0;
            });
        })
      )
      .subscribe();
  }

  private loadUsers(): void {
    this.economyService()
      .getUserNamesWithLogin()
      .pipe(
        tap(_users => {
          let id = 0;
          this.users = _users
            .map(u => {
              return <UserLoginDTO>{
                ...u,
                id: ++id,
              };
            })
            .sort((x, y): number => {
              if (
                x.userNameAndLogin.toLowerCase() <
                y.userNameAndLogin.toLowerCase()
              )
                return -1;
              if (
                x.userNameAndLogin.toLowerCase() >
                y.userNameAndLogin.toLowerCase()
              )
                return 1;
              return 0;
            });
          addEmptyOption(this.users);
        })
      )
      .subscribe();
  }

  private updateFormControls(accDims: IAccountDimSmallDTO[]) {
    accDims.forEach(accountDim => {
      if (accountDim.accounts === undefined) {
        accountDim.accounts = [];
      }

      addEmptyOption(accountDim.accounts);
      if (accountDim.accountDimNr == 1) {
        this.accountDim1Accounts = accountDim.accounts;
      }
      this.form.patchValue({
        [`dim${accountDim.accountDimNr}AccountId`]: accountDim.accountDimId,
      });
    });
  }

  protected moreFilterExpanded(isOpened: boolean): void {
    this.moreFilterOpened.set(isOpened);
  }

  protected getAccountFromControlName(dimNr: number): string {
    return `dim${dimNr}AccountFr`;
  }

  protected triggerSearch(): void {
    this.searchClicked.emit(<SearchVoucherFilterDTO>this.form.getRawValue());
  }

  protected amountFromChanged(value: number): void {
    this.form?.amountTo.setValue(value);
  }

  ngOnDestroy(): void {
    this._destroy$.next();
    this._destroy$.complete();
  }
}
