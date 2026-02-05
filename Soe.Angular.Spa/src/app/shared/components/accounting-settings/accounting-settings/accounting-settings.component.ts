import {
  Component,
  EventEmitter,
  Input,
  OnChanges,
  OnDestroy,
  OnInit,
  Output,
  SimpleChanges,
  inject,
  input,
} from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { SoeFormGroup } from '@shared/extensions';
import { TermCollection } from '@shared/localization/term-types';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import {
  IAccountDTO,
  IAccountDimSmallDTO,
  IAccountingSettingsRowDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { StringKeyOfNumberProperty } from '@shared/types';
import { Perform } from '@shared/util/perform.class';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { CellValueChangedEvent } from 'ag-grid-community';
import { BehaviorSubject, Observable, Subject, takeUntil, tap } from 'rxjs';
import { AccountingSettingsRowDTO } from './accounting-settings.models';
import {
  SelectedAccounts,
  SelectedAccountsChangeSet,
} from '@shared/components/account-dims/account-dims-form.model';
import { AccountDTO } from '@shared/models/account.model';

@Component({
  selector: 'soe-accounting-settings',
  templateUrl: './accounting-settings.component.html',
  styleUrls: ['./accounting-settings.component.scss'],
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class AccountingSettingsComponent
  extends GridBaseDirective<IAccountingSettingsRowDTO>
  implements OnInit, OnChanges, OnDestroy
{
  @Input() hideAccordion = false;
  @Input() form: SoeFormGroup | undefined;
  @Input() minRows = 10;
  @Input() maxRows = 10;
  @Input() settingTypes!: SmallGenericType[];
  @Input() settings: IAccountingSettingsRowDTO[] = [];
  @Input() hideStdDim = false;
  @Input() includeOrphanAccounts = false;
  @Input() useNoAccount!: boolean;
  @Input() baseAccounts!: SmallGenericType[];
  @Input() showBaseAccount!: boolean;
  @Input() isModified!: boolean;
  @Input() isValid!: boolean;
  @Input() readOnly!: boolean;
  @Input() mustHaveStandardIfInternal!: boolean;

  reloadAccounts = input<Subject<void>>();
  updateInternalAccounts = input<Subject<SelectedAccountsChangeSet>>();

  @Output() settingsChange = new EventEmitter<IAccountingSettingsRowDTO[]>();

  protected readonly labelKey = 'common.accountingsettings.accountingsettings';
  readonly coreService = inject(CoreService);
  readonly flowHandler = inject(FlowHandlerService);
  readonly progress = inject(ProgressService);
  readonly performLoadAccount = new Perform<IAccountDimSmallDTO[]>(
    this.progress
  );
  settingsRows = new BehaviorSubject<IAccountingSettingsRowDTO[]>([]);
  accountDims: IAccountDimSmallDTO[] = [];
  validationErrors = '';
  private editingRowType: number | null = null;

  accountSubjects: IAccountDTO[][] = [];
  private _destroy$ = new Subject<void>();

  ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(Feature.None, 'Common.Directives.AccountingSettings', {
      skipInitialLoad: true,
      lookups: [this.loadAccounts(false)],
    });

    this.reloadAccounts()
      ?.pipe(takeUntil(this._destroy$))
      .subscribe(() => {
        this.loadAccounts(false).subscribe(_ => {
          this.accountDims.forEach((dim, i) => {
            this.accountSubjects[i] = [...dim.accounts];
          });
          this.setAccountSettings();
        });
      });

    this.updateInternalAccounts()
      ?.pipe(takeUntil(this._destroy$))
      .subscribe((values: SelectedAccountsChangeSet) => {
        let modified = false;
        // Dim2
        if (
          values.selectedAccounts.account2 &&
          values.selectedAccounts.account2.accountId > 0 &&
          values.dimNr === 2
        ) {
          this.settings.forEach(settingRow => {
            const account = this.accountDims[1]?.accounts.find(
              a => a.accountId === values.selectedAccounts.account2!.accountId
            );

            if (account) {
              settingRow.account2Id = account.accountId;
              settingRow.account2Name = account.name;
              settingRow.account2Nr = account.accountNr;

              modified = true;
            }
          });
        }

        // Dim3
        if (
          values.selectedAccounts.account3 &&
          values.selectedAccounts.account3.accountId > 0 &&
          values.dimNr === 3
        ) {
          this.settings.forEach(settingRow => {
            const account = this.accountDims[2]?.accounts.find(
              a => a.accountId === values.selectedAccounts.account3!.accountId
            );

            if (account) {
              settingRow.account3Id = account.accountId;
              settingRow.account3Name = account.name;
              settingRow.account3Nr = account.accountNr;

              modified = true;
            }
          });
        }

        // Dim4
        if (
          values.selectedAccounts.account4 &&
          values.selectedAccounts.account4.accountId > 0 &&
          values.dimNr === 4
        ) {
          this.settings.forEach(settingRow => {
            const account = this.accountDims[3]?.accounts.find(
              a => a.accountId === values.selectedAccounts.account4!.accountId
            );

            if (account) {
              settingRow.account4Id = account.accountId;
              settingRow.account4Name = account.name;
              settingRow.account4Nr = account.accountNr;

              modified = true;
            }
          });
        }

        // Dim5
        if (
          values.selectedAccounts.account5 &&
          values.selectedAccounts.account5.accountId > 0 &&
          values.dimNr === 5
        ) {
          this.settings.forEach(settingRow => {
            const account = this.accountDims[4]?.accounts.find(
              a => a.accountId === values.selectedAccounts.account5!.accountId
            );

            if (account) {
              settingRow.account5Id = account.accountId;
              settingRow.account5Name = account.name;
              settingRow.account5Nr = account.accountNr;

              modified = true;
            }
          });
        }

        // Dim6
        if (
          values.selectedAccounts.account6 &&
          values.selectedAccounts.account6.accountId > 0 &&
          values.dimNr === 6
        ) {
          this.settings.forEach(settingRow => {
            const account = this.accountDims[5]?.accounts.find(
              a => a.accountId === values.selectedAccounts.account6!.accountId
            );

            if (account) {
              settingRow.account6Id = account.accountId;
              settingRow.account6Name = account.name;
              settingRow.account6Nr = account.accountNr;

              modified = true;
            }
          });
        }

        if (modified) {
          this.isModified = true;
          this.grid.resetRows();
          this.settingsChange.emit(this.settingsRows.value);
        }
      });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.settingTypes && changes.settingTypes.currentValue) {
      this.setAccountSettings();
    }
    if (changes.settings && changes.settings.currentValue) {
      this.setAccountSettings();
    }
  }

  onGridReadyToDefine(grid: GridComponent<IAccountingSettingsRowDTO>): void {
    super.onGridReadyToDefine(grid);

    this.setUpGridColumns();

    this.grid?.setNbrOfRowsToShow(this.minRows, this.maxRows);
    this.grid.context.suppressGridMenu = true;
    super.finalizeInitGrid({
      hidden: true,
    });
    this.grid.updateGridHeightBasedOnNbrOfRows();
    this.setAccountSettings();
  }

  private setUpGridColumns(): void {
    this.grid.columns = [];

    this.grid.api.updateGridOptions({
      onCellValueChanged: this.onCellValueChanged.bind(this),
      onCellEditingStarted: this.onCellEditingStarted.bind(this),
      onCellEditingStopped: this.onCellEditingStopped.bind(this),
    });

    this.grid.addColumnText('typeName', this.terms['common.type'], {
      suppressFilter: true,
      sortable: false,
      editable: false,
      flex: 1,
    });

    this.accountDims.forEach((dim, i) => {
      this.accountSubjects[i] = [...dim.accounts];
      if (this.hideStdDim && i === 0) return;

      const errorField = i === 0 ? 'account1Error' : null,
        dimNr = String(i + 1),
        field = `account${dimNr}Id`,
        header =
          dim.accountDimNr === 1
            ? this.terms['common.accountingsettings.account']
            : dim.name;

      this.grid.addColumnAutocomplete<IAccountDTO>(
        field as StringKeyOfNumberProperty<IAccountingSettingsRowDTO>,
        header,
        {
          editable: !this.readOnly,
          optionIdField: 'accountId',
          optionNameField: 'numberName',
          scrollable: true,
          source: _ => this.accountSubjects[i],
          updater: _ => {
            this.form?.markAsDirty();
            this.form?.markAsTouched();
          },
          suppressFilter: true,
          sortable: false,
          flex: 1,
        }
      );
    });

    if (this.showBaseAccount) {
      this.grid.addColumnText(
        'baseAccount',
        this.terms['common.accountingsettings.baseaccount'],
        {
          suppressFilter: true,
          sortable: false,
          editable: false,
          flex: 1,
        }
      );
    }
  }

  onCellValueChanged(event: CellValueChangedEvent) {
    this.settingsChange.emit(this.settingsRows.value);
  }

  onCellEditingStarted(event: any): void {
    this.editingRowType = event.data.type;
  }

  onCellEditingStopped(event: any): void {
    this.editingRowType = null;
  }

  override loadTerms(): Observable<TermCollection> {
    return super.loadTerms([
      'core.delete',
      'common.type',
      'common.accountingsettings.account',
      'common.accountingsettings.baseaccount',
      'common.accountingsettings.fixed',
      'common.accountingsettings.noaccount',
      'common.accountingsettings.error.musthavestandardifinternal',
      'common.accountingsettings.error.missingaccount',
      'common.accountingsettings.error.invalidpercent',
      'common.percent',
      'common.accountingrows.missingaccount',
    ]);
  }

  private loadAccounts(useCache: boolean): Observable<IAccountDimSmallDTO[]> {
    return this.performLoadAccount.load$(
      this.coreService
        .getAccountDimsSmall(
          false,
          this.hideStdDim,
          true,
          false,
          true,
          false,
          false,
          true,
          useCache,
          false,
          0,
          this.includeOrphanAccounts
        )
        .pipe(
          tap(x => {
            this.accountDims = x;

            if (this.hideStdDim) {
              // Creating empty standard dim, to make indexing easier
              this.accountDims.splice(0, 0, <IAccountDimSmallDTO>{
                accountDimId: 0,
                accountDimNr: 1,
                accounts: <IAccountDTO[]>[],
              });
            }

            this.accountDims.forEach((dim, idx) => {
              if (!dim.accounts) dim.accounts = [];

              if (this.useNoAccount) {
                //Adding 'No Account'
                dim.accounts.splice(
                  0,
                  0,
                  this.getAccount(
                    -1,
                    dim.accountDimId,
                    '-',
                    this.terms['common.accountingsettings.noaccount'],
                    this.terms['common.accountingsettings.noaccount']
                  )
                );
              }

              //Adding Empty Account
              dim.accounts.splice(
                0,
                0,
                this.getAccount(0, dim.accountDimId, '', '', '')
              );
            });
          })
        )
    );
  }

  private getAccount(
    accountId: number,
    accountDimId: number,
    accountNr: string,
    name: string,
    numberName: string
  ): IAccountDTO {
    return <IAccountDTO>{
      accountId,
      accountDimId,
      accountNr,
      name,
      numberName,
    };
  }

  private validate(): void {
    this.isValid = true;
    this.validationErrors = '';

    if (this.mustHaveStandardIfInternal) {
      this.settings.forEach(settingRow => {
        if (
          !settingRow.account1Id &&
          (settingRow.account2Id ||
            settingRow.account3Id ||
            settingRow.account4Id ||
            settingRow.account5Id ||
            settingRow.account6Id)
        ) {
          this.validationErrors +=
            this.terms[
              'common.accountingsettings.error.musthavestandardifinternal'
            ] + '\n';
          this.isValid = false;
          return;
        }
      });
    }

    //Since there will be a seperate accounting settings compoenent for HRM module
    //,fixed account validation (Check that percent total is 100%) did't implemented.
    // For now we are going to use the same.
    // TODO: Implement fixed account validation
  }

  private filterAccounts(dimIndex: number, filter: unknown): IAccountDTO[] {
    return this.accountDims[dimIndex].accounts.filter(acc => {
      if (Number(filter)) {
        return acc.accountNr.startsWithCaseInsensitive(String(filter));
      }

      return (
        acc.accountNr.startsWithCaseInsensitive(String(filter)) ||
        acc.name.includes(String(filter))
      );
    });
  }

  private setAccountSettings(): void {
    this.initSettings();

    this.settingsRows.next(
      this.settings.filter(x =>
        this.settingTypes.map(st => st.id).includes(x.type)
      )
    );
  }

  private initSettings(): void {
    if (!this.settings || this.settings.every(x => x.type === null))
      this.settings = [];

    this.settingTypes?.forEach(settingType => {
      let setting = <AccountingSettingsRowDTO>(
        this.settings.find(x => x.type === settingType.id)
      );
      if (!setting) {
        setting = <AccountingSettingsRowDTO>{
          type: settingType.id,
        };
        this.settings.push(setting);
      }
      setting.typeName = settingType.name;

      //Set base account
      let account;
      if (this.accountDims && this.accountDims.length > 0) {
        const baseAcc = this.baseAccounts.find(x => x.id === settingType.id);
        if (baseAcc) {
          account = this.accountDims[0].accounts.find(
            x => x.accountId === Number(baseAcc.name)
          );
        }
      }
      setting.baseAccount = account ? account.numberName : '';
    });

    this.isModified = true;
    this.validate();
  }

  // HELPER METHODS
  public reloadGridWithNewAccounts(): void {
    this.loadAccounts(false)
      .pipe(
        tap(() => {
          // Re-initialize grid to update with new accounts
          if (this.grid) {
            this.grid.refreshCells();
            this.setUpGridColumns();
          }
        })
      )
      .subscribe();
  }

  ngOnDestroy(): void {
    this._destroy$.next();
    this._destroy$.complete();
  }
}
