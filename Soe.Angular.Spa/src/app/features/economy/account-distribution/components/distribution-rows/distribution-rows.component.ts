import { Component, inject, input, OnInit, signal } from '@angular/core';
import { AccountDistributionForm } from '@features/economy/account-distribution/models/account-distribution-form.model';
import { AccountDimSmallDTO } from '@shared/components/accounting-rows/models/accounting-rows-model';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { ValidationHandler } from '@shared/handlers';
import { AccountDTO } from '@shared/models/account.model';
import {
  Feature,
  SoeEntityState,
} from '@shared/models/generated-interfaces/Enumerations';
import { IAccountDimSmallDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { StringKeyOfNumberProperty } from '@shared/types';
import { AccountDistributionRowDTO } from '@src/app/features/economy/account-distribution/models/account-distribution.model';
import { EconomyService } from '@src/app/features/economy/services/economy.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { CellValueChangedEvent } from 'ag-grid-community';
import { orderBy } from 'lodash';
import { BehaviorSubject, Observable, of, take, tap } from 'rxjs';

@Component({
  selector: 'soe-distribution-rows',
  templateUrl: './distribution-rows.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
}) //, OnChanges
export class DistributionRowsComponent
  extends GridBaseDirective<AccountDistributionRowDTO>
  implements OnInit
{
  rows = input.required<BehaviorSubject<AccountDistributionRowDTO[]>>();
  activeRows = input<BehaviorSubject<AccountDistributionRowDTO[]>>(
    new BehaviorSubject<AccountDistributionRowDTO[]>([])
  );
  form = input.required<AccountDistributionForm>();

  validationHandler = inject(ValidationHandler);
  economyService = inject(EconomyService);
  accountDims: AccountDimSmallDTO[] = [];

  sameSum = 0;
  oppositeSum = 0;
  difference = 0;

  ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(
      Feature.Economy_Preferences_VoucherSettings_AccountDistributionPeriod,
      'Common.Directives.DistributionRows',
      {
        skipInitialLoad: true,
        lookups: this.loadAccounts(),
      }
    );
    this.rows().subscribe(rows => {
      this.setActiveRows(rows);
    });
  }

  setActiveRows(rows: AccountDistributionRowDTO[]) {
    const updatedRows: AccountDistributionRowDTO[] = [];
    rows.forEach(f => {
      const row = new AccountDistributionRowDTO();
      Object.assign(row, f);
      if (row.dim1Id === 0 && row.dim1Nr === '*') {
        row.dim1Id = -1;
      }
      if (row.dim2Id === 0 && row.dim2Nr === '*') {
        row.dim2Id = -1;
      }
      if (row.dim3Id === 0 && row.dim3Nr === '*') {
        row.dim3Id = -1;
      }
      if (row.dim4Id === 0 && row.dim4Nr === '*') {
        row.dim4Id = -1;
      }
      if (row.dim5Id === 0 && row.dim5Nr === '*') {
        row.dim5Id = -1;
      }
      if (row.dim6Id === 0 && row.dim6Nr === '*') {
        row.dim6Id = -1;
      }
      updatedRows.push(row);
      this.validateAccountingRow(row);
    });

    this.activeRows().next(updatedRows.sort((a, b) => a.rowNbr - b.rowNbr));
  }

  override onFinished(): void {
    this.setActiveRows(this.rows().getValue());
    this.summeryCalculation();
  }

  override createGridToolbar(): void {
    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton('common.newrow', {
          iconName: signal('plus'),
          caption: signal('common.newrow'),
          tooltip: signal('common.newrow'),
          onAction: () => this.addRow(),
        }),
        this.toolbarService.createToolbarButton(
          'common.accountingrows.reloadaccounts',
          {
            iconName: signal('sync'),
            caption: signal('common.accountingrows.reloadaccounts'),
            tooltip: signal('common.accountingrows.reloadaccounts'),
            onAction: () => {
              this.loadAccounts().subscribe();
            },
          }
        ),
      ],
    });
  }

  private loadAccounts(): Observable<IAccountDimSmallDTO[]> {
    return this.economyService
      .getAccountDimsSmall(
        false,
        false,
        true,
        false,
        false,
        false,
        false,
        false
      )
      .pipe(
        tap(dimDicts => {
          this.accountDims = dimDicts;
        })
      );
  }

  addRow() {
    let row = new AccountDistributionRowDTO();
    const distributionRow: AccountDistributionRowDTO[] =
      this.activeRows().getValue();

    if (distributionRow.length > 0) {
      row.rowNbr = distributionRow[distributionRow.length - 1]?.rowNbr + 1;
    } else row.rowNbr = 1;
    row.dim1Id = 0;
    row.dim2Id = 0;
    row.dim3Id = 0;
    row.dim4Id = 0;
    row.dim5Id = 0;
    row.dim6Id = 0;

    row = <AccountDistributionRowDTO>(
      this.form().addDistributionAccountingRow(row)
    );
    distributionRow.push(row);
    this.activeRows().next(distributionRow.sort((a, b) => a.rowNbr - b.rowNbr));

    this.form().markAsDirty();
  }

  public filterAccounts(accountDims: AccountDTO[]) {
    return orderBy(accountDims, 'accountNr');
  }

  private validateAccountingRow(row: AccountDistributionRowDTO) {
    row.selectOptions = this.giveMeTheSameListEveryTimeBasedOnInput(row.rowNbr);
  }

  private giveMeTheSameListEveryTimeBasedOnInput(rowNr: number) {
    const arr = [{ id: 0, value: ' ' }];

    for (let i = 1; i < rowNr; i++) {
      arr.push({ id: i, value: i.toString() });
    }

    return arr;
  }

  private setAccountingRowsBaseAccountName() {
    this.accountDims.forEach((ad, i) => {
      this.activeRows()
        .getValue()
        .forEach((ar: any) => {
          if (ar[`dim${i + 1}Nr` as keyof typeof ar] === '*')
            ar[`dim${i + 1}Name`] =
              '* ' +
              this.terms[
                'economy.accounting.accountdistributionauto.keepaccount'
              ];
        });
    });
  }

  summeryCalculation() {
    this.sameSum = 0;
    this.oppositeSum = 0;
    this.difference = 0;

    this.activeRows().subscribe(x => {
      x.forEach(e => {
        this.sameSum += e.sameBalance;
        this.oppositeSum += e.oppositeBalance;
      });
    });

    this.difference = +(this.sameSum - this.oppositeSum).toFixed(2);
  }

  onCellChanged(event: CellValueChangedEvent) {
    if (
      event.colDef.field == 'sameBalance' ||
      event.colDef.field == 'oppositeBalance' ||
      event.colDef.field == 'rowNbr'
    ) {
      this.summeryCalculation();
    }
    const row = event.data as AccountDistributionRowDTO;
    this.form().updateDistributionAccountingRow(row);
    this.form().markAsDirty();
  }

  onComponentStateChanged() {
    this.summeryCalculation();
  }

  onGridReadyToDefine(grid: GridComponent<AccountDistributionRowDTO>): void {
    super.onGridReadyToDefine(grid);

    this.grid.api.updateGridOptions({
      onCellValueChanged: this.onCellChanged.bind(this),
      onComponentStateChanged: this.onComponentStateChanged.bind(this),
    });

    this.translate
      .get([
        'common.number',
        'common.date',
        'common.text',
        'common.debit',
        'common.credit',
        'common.balance',
        'common.rownr',
        'economy.accounting.voucher.voucherseries',
        'economy.accounting.voucher.vatvoucher',
        'core.deleterow',
        'common.accountingrows.missingaccount',
        'common.accountingrows.invalidaccount',
        'economy.accounting.accountdistributionauto.keepaccount',
        'economy.accounting.accountdistributionauto.calculaterownbr',
        'economy.accounting.accountdistributionauto.samesign',
        'economy.accounting.accountdistributionauto.oppositesign',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.terms = terms;

        this.grid.addColumnNumber('rowNbr', terms['common.rownr'], {
          enableHiding: false,
          suppressFilter: true,
          width: 75,
        });

        this.accountDims.forEach((dim, i) => {
          const index = i + 1;
          const field = `dim${index}Id`;
          const nameField: any = `dim${index}Name`;
          let activeAccounts: AccountDTO[] = [];
          const filterActiveAct = this.filterAccounts(dim.accounts);
          const firstAct = new AccountDTO();
          firstAct.accountId = 0;
          firstAct.accountNr = '';
          firstAct.name = '';
          activeAccounts.push(firstAct);
          const act = filterActiveAct.find(f => f.accountNr == '*');

          if (!act) {
            const keepAct = new AccountDTO();
            keepAct.accountId = -1;
            keepAct.accountNr = '*';
            keepAct.name =
              this.terms[
                'economy.accounting.accountdistributionauto.keepaccount'
              ];
            keepAct.numberName =
              '* ' +
              this.terms[
                'economy.accounting.accountdistributionauto.keepaccount'
              ];
            activeAccounts.push(keepAct);
          }
          activeAccounts = activeAccounts.concat(filterActiveAct);
          this.grid.addColumnAutocomplete<AccountDTO>(
            field as StringKeyOfNumberProperty<AccountDistributionRowDTO>,
            dim.name,
            {
              optionIdField: 'accountId',
              optionNameField: 'numberName',
              optionDisplayNameField: `dim${index}Nr` as any,
              limit: 7,
              source: _ => activeAccounts,
              flex: 1,
              editable: true,
              suppressFilter: true,

              updater: (row, account): void => {
                const editedRow = row as any;
                editedRow[`dim${index}Nr`] = account ? account.accountNr : '';
                editedRow[`dim${index}Name`] = account ? account.name : '';
                editedRow[`dim${index}Id`] = account ? account.accountId : 0;
                editedRow[`dim${index}KeepSourceRowAccount`] = false;

                if (account?.accountNr === '*')
                  editedRow[`dim${index}KeepSourceRowAccount`] = true;
                this.grid.refreshCells();
                this.form().markAsDirty();
              },
            }
          );
        });

        this.grid.addColumnSelect(
          'calculateRowNbr',
          terms['economy.accounting.accountdistributionauto.calculaterownbr'],
          [],
          undefined,
          {
            dropDownIdLabel: 'id',
            dropDownValueLabel: 'value',
            dynamicSelectOptions: row => this.selectDropdownOptions(row),
            enableHiding: false,
            editable: true,
            suppressFilter: true,
            flex: 1,
          }
        );
        this.grid.addColumnNumber(
          'sameBalance',
          terms['economy.accounting.accountdistributionauto.samesign'],
          {
            decimals: 2,
            suppressFilter: true,
            enableHiding: false,
            editable: true,
            flex: 1,
          }
        );
        this.grid.addColumnNumber(
          'oppositeBalance',
          terms['economy.accounting.accountdistributionauto.oppositesign'],
          {
            decimals: 2,
            suppressFilter: true,
            enableHiding: false,
            editable: true,
            flex: 1,
          }
        );
        this.grid.addColumnText('description', terms['common.text'], {
          flex: 1,
          suppressFilter: true,
        });
        this.grid.addColumnIconDelete({
          tooltip: terms['core.delete'],
          onClick: row => {
            this.delete(row);
          },
        });

        this.grid.context.suppressGridMenu = true;
        super.finalizeInitGrid({ hidden: true });
        this.setAccountingRowsBaseAccountName();
      });
  }

  selectDropdownOptions(row: any) {
    return row.data.selectOptions;
  }

  delete(row: AccountDistributionRowDTO): void {
    if (row) {
      row.state = SoeEntityState.Deleted;
      this.grid.deleteRow(row, 'rowNbr');
      const rows = this.grid.getAllRows();
      this.setActiveRows(rows);
      this.form().resetDistributionAccountingRow(rows);
      this.form().markAsDirty();
    }
  }
}
